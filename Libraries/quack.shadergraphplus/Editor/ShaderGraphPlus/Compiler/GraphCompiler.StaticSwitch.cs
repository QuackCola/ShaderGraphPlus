// Static Switch related code
//

using Editor.ShaderGraph;
using NodeEditorPlus;
using System;
using System.Text;
using GraphView = NodeEditorPlus.GraphView;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;
using NodeUI = NodeEditorPlus.NodeUI;


namespace ShaderGraphPlus;

public enum StaticSwitchBlock
{
	None,
	True,
	False,
}

public struct FeatureRule : IValid
{
	/// <summary>
	/// Features bound to this rule.
	/// </summary>
	[InlineEditor( Label = false )]
	public List<string> Features { get; set; }

	/// <summary>
	/// Text hint when hovering over features
	/// </summary>
	public string HoverHint { get;  set; }

	[Hide, JsonIgnore]
	public bool IsValid
	{
		get
		{
			if ( Features.Any() )
			{
				foreach ( var feature in Features )
				{
					if ( string.IsNullOrWhiteSpace( feature ) )
					{
						return false;
					}
					else
					{
						continue;
					}
				}

				return true;
			}
			else
			{
				return false;
			}
		}
	}

	public FeatureRule()
	{
		Features = new();
		HoverHint = string.Empty;
	}
}

public sealed partial class GraphCompiler
{
	/// <summary>
	/// Registerd ShaderFeatures of the current project.
	/// </summary>
	public Dictionary<string, ShaderFeatureBase> ShaderFeatures = new();

	struct SwitchBlockResultHolder
	{
		public string BlockLocals { get; set; }
		public NodeResult Result { get; set; }

		public SwitchBlockResultHolder( string locals, NodeResult nodeResult )
		{
			BlockLocals = locals;
			Result = nodeResult;
		}
	}

	public void ResultComboPreview( string comboName, int preview )
	{
		if ( comboName.StartsWith( $"D_" ) )
		{
			OnAttribute?.Invoke( comboName, preview, true );
		}
		else
		{
			SGPLog.Error( $"Cannot set a static combo \"{comboName}\"" );
		}
	}

	public NodeResult ResultFeatureSwitch( IEnumerable<NodeInput> inputs, ShaderFeatureBase shaderFeature, int previewInt )
	{
		var sb = new StringBuilder();
		var results = new List<SwitchBlockResultHolder>();

		foreach ( var input in inputs )
		{
			if ( !input.IsValid )
			{
				results.Add( new SwitchBlockResultHolder( $"float l_0 = 1.0f;", new NodeResult( ResultType.Float, "l_0" ) ) );
			}
			else
			{
				results.Add( SubEvaluate( input, input.Identifier, shaderFeature ) );
			}
		}

		var resultType = results.Select( x => x.Result.ResultType ).Where( x => !((int)x > 6) ).Max();
		var resultLocal = $"{shaderFeature.Name}_result";
		var resultDataType = resultType.GetHLSLDataType();

		foreach ( var (index, result) in results.Index() )
		{
			var finaleResult = new NodeResult( resultType, result.Result.Cast( resultType.GetComponentCount() ) );

			if ( index == 0 )
			{
				sb.AppendLine( ResultInit( resultType, resultLocal ) ) ; //$"{resultDataType} {resultLocal};" );
				
				if ( shaderFeature is ShaderFeatureBoolean boolFeature )
				{
					sb.AppendLine( $"#if ( {(IsPreview ? "D" : "S")}_{shaderFeature.Name.ToUpper()} == SWITCH_TRUE )" );
				}
				else
				{
					sb.AppendLine( $"#if ( {(IsPreview ? "D" : "S")}_{shaderFeature.Name.ToUpper()} == {index} )" );
				}

				ConstructSwitchStart( sb, shaderFeature.Name, index, result.BlockLocals, resultLocal, finaleResult );
			}

			if ( shaderFeature is ShaderFeatureBoolean )
			{
				if ( index == results.Count - 1 )
				{
					ConstructSwitchEnd( sb, result.BlockLocals, resultLocal, finaleResult );
				}
			}
			else
			{
				if ( index != 0 && index != results.Count - 1 )
				{
					ConstructSwitchMid( sb, shaderFeature.Name, index, result.BlockLocals, resultLocal, finaleResult );
				}
				else if ( index == results.Count - 1 )
				{
					sb.AppendLine( $"#elif ( {(IsPreview ? "D" : "S")}_{shaderFeature.Name.ToUpper()} == {index} )" );
					sb.AppendLine( $"{{" );
					sb.AppendLine( $"{IndentString( result.BlockLocals, 1 )}" );
					sb.AppendLine( $"{IndentString( $"{resultLocal} = {finaleResult}", 1 )};" );
					sb.AppendLine( $"}}" );
					sb.AppendLine( $"#endif" );
				}
			}
		}

		if ( IsPreview )
		{
			SGPLog.Info( $"Generated Switch D_{shaderFeature.Name.ToUpper()}: \n {sb.ToString()}" );
			var comboName = shaderFeature.GetDynamicComboString();

			if ( comboName.StartsWith( "D_" ) )
			{
				OnAttribute?.Invoke( comboName, previewInt, true );
			}
		}

		var finalResult = new NodeResult( resultType, resultLocal );
		finalResult.SetMetadata( nameof( MetadataType.ComboSwitchBody ), sb.ToString() );

		return finalResult;
	}

	private SwitchBlockResultHolder SubEvaluate( NodeInput input, string block, ShaderFeatureBase shaderFeature )
	{
		var outerPixelResult = PixelResult;
		var outerVertexResult = VertexResult;
		var outerInputStack = InputStack;
		var sb = new StringBuilder();

		VertexResult = new();
		PixelResult = new();

		VertexResult.SetAttributes( outerVertexResult.Attributes );
		PixelResult.SetAttributes( outerPixelResult.Attributes );
		InputStack = new();

		var result = Result( input );
		var blockCode = GenerateLocals( true );

		//SGPLog.Info( $"GeneratedBlock {block} : \n {{ {IndentString( locals, 1)} \n }}" );

		foreach ( var attribute in ShaderResult.Attributes )
		{
			outerPixelResult.Attributes[attribute.Key] = attribute.Value;
			outerVertexResult.Attributes[attribute.Key] = attribute.Value;
			//if ( IsPs )
			//{
			//	outerPixelResult.Attributes[attribute.Key] = attribute.Value;
			//}
			//else if ( IsVs )
			//{
			//	outerVertexResult.Attributes[attribute.Key] = attribute.Value;
			//}
		}

		VertexResult = outerPixelResult;
		PixelResult = outerVertexResult;
		InputStack = outerInputStack;

		return new( blockCode, result );
	}

	private StringBuilder ConstructSwitchStart( StringBuilder sb, string featureName, int index, string locals, string resultLocal, NodeResult nodeResult )
	{
		sb.AppendLine( $"{{" );
		sb.AppendLine( $"{IndentString( locals, 1 )}" );
		sb.AppendLine( $"{IndentString( $"{resultLocal} = {nodeResult}", 1 )};" );
		sb.AppendLine( $"}}" );

		return sb;
	}

	private StringBuilder ConstructSwitchMid( StringBuilder sb, string featureName, int index, string locals, string resultLocal, NodeResult nodeResult )
	{
		sb.AppendLine( $"#elif ( {(IsPreview ? "D" : "S")}_{featureName.ToUpper()} == {index} )" );
		sb.AppendLine( $"{{" );
		sb.AppendLine( $"{IndentString( locals, 1 )}" );
		sb.AppendLine( $"{IndentString( $"{resultLocal} = {nodeResult}", 1 )};" );
		sb.AppendLine( $"}}" );

		return sb;
	}

	private static StringBuilder ConstructSwitchEnd( StringBuilder sb, string locals, string resultLocal, NodeResult nodeResult )
	{
		sb.AppendLine( $"#else" );
		sb.AppendLine( $"{{" );
		sb.AppendLine( $"{IndentString( locals, 1 )}" );
		sb.AppendLine( $"{IndentString( $"{resultLocal} = {nodeResult}", 1 )};" );
		sb.AppendLine( $"}}" );
		sb.AppendLine( $"#endif" );

		return sb;
	}

	internal string BuildFeatureOptionsBody( List<string> options )
	{
		var options_body = "";
		int count = 0;

		foreach ( var option in options )
		{
			if ( count == 0 ) // first option starts at 0 :)
			{
				options_body += $"0=\"{option}\", ";
				count++;
			}
			else if ( count != (options.Count - 1) )  // These options dont get the privilege of being the first >:)
			{
				options_body += $"{count}=\"{option}\", ";
				count++;
			}
			else // Last option in the list oh well...:(
			{
				options_body += $"{count}=\"{option}\"";
			}
		}

		return options_body;
	}

	internal string ResultInit( ResultType resultType, string name )
	{
		switch ( resultType )
		{
			case ResultType.Bool:
				return $"bool {name} = false;";
			case ResultType.Int:
				return $"int {name} = 0;";
			case ResultType.Float:
				return $"float {name} = 0.0f;";
			case ResultType.Vector2:
				return $"float2 {name} = float2( 0.0f, 0.0f );";
			case ResultType.Vector3:
				return $"float3 {name} = float3( 0.0f, 0.0f, 0.0f );";
			case ResultType.Vector4:
				return $"float4 {name} = float4( 0.0f, 0.0f, 0.0f, 0.0f );";
			case ResultType.Color:
				return $"float4 {name} = float4( 0.0f, 0.0f, 0.0f, 0.0f );";
			case ResultType.Float2x2:
				return $"float2x2 {name} = float2x2( 0.0f, 0.0f, 0.0f, 0.0f );";
			case ResultType.Float3x3:
				return $"float3x3 {name} = float3x3( 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f );";
			case ResultType.Float4x4:
				return $"float4x4 {name} = float4x4( 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f );";
			default:
				throw new NotImplementedException( $"ResultType `{resultType}` not implemented!" );
		}
	}
}
