using Editor;
using NodeEditorPlus;
using ShaderGraphPlus.Nodes;
using static Sandbox.Material;
using GraphView = NodeEditorPlus.GraphView;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;
using NodeUI = NodeEditorPlus.NodeUI;

namespace ShaderGraphPlus;

public enum SubgraphPortType
{
	[Icon( "check_box" )]
	Bool,
	[Icon( "looks_one" )]
	Int,
	[Icon( "looks_one" )]
	Float,
	[Title( "Float2" ), Icon( "looks_two" )]
	Vector2,
	[Title( "Float3" ), Icon( "looks_3" )]
	Vector3,
	[Title( "Float4" ), Icon( "looks_4" )]
	Vector4,
	[Title( "Color" ), Icon( "palette" )]
	Color,
	[Title( "Sampler State" ), Icon( "colorize" )]
	SamplerState,
	[Title( "Texture2D" ), Icon( "texture" )]
	Texture2DObject,
	[Title( "TextureCube" ), Icon( "view_in_ar" )]
	TextureCubeObject,
	[Hide]
	Invalid
}

/// <summary>
/// Input of a Subgraph.
/// </summary>
[Title( "Subgraph Input" ), Icon( "input" ), SubgraphOnly]
[InternalNode]
public sealed class SubgraphInput : ShaderNodePlus, IErroringNode, IWarningNode, IBlackboardSyncableNode, IMetaDataNode
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.SubgraphNode;

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	//[Hide, JsonIgnore]
	//private bool IsSubgraph => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph);

	[Hide]
	public Guid BlackboardParameterIdentifier { get; set; }

	public void UpdateFromBlackboard( BaseBlackboardParameter blackboardParameter )
	{
		if ( blackboardParameter is BoolSubgraphInputParameter boolParameter )
		{
			SetDefaultValue( boolParameter.Value );
		}
		else if ( blackboardParameter is IntSubgraphInputParameter intParameter )
		{
			SetDefaultValue( intParameter.Value );
		}
		else if ( blackboardParameter is FloatSubgraphInputParameter floatParameter )
		{
			SetDefaultValue( floatParameter.Value );
		}
		else if ( blackboardParameter is Float2SubgraphInputParameter float2Parameter )
		{
			SetDefaultValue( float2Parameter.Value );
		}
		else if ( blackboardParameter is Float3SubgraphInputParameter float3Parameter )
		{
			SetDefaultValue( float3Parameter.Value );
		}
		else if ( blackboardParameter is Float4SubgraphInputParameter float4Parameter )
		{
			SetDefaultValue( float4Parameter.Value );
		}
		else if ( blackboardParameter is ColorSubgraphInputParameter colorParameter )
		{
			SetDefaultValue( colorParameter.Value );
		}
		else if ( blackboardParameter is Texture2DSubgraphInputParameter texture2DParameter )
		{
			SetDefaultValue( texture2DParameter.Value );
		}
		else if ( blackboardParameter is TextureCubeSubgraphInputParameter textureCubeParameter )
		{
			SetDefaultValue( textureCubeParameter.Value );
		}
		else if ( blackboardParameter is SamplerStateSubgraphInputParameter samplerStateParameter )
		{
			SetDefaultValue( samplerStateParameter.Value );
		}
	}

	[Hide]
	public override string Title => string.IsNullOrWhiteSpace( InputName ) ?
	$"Subgraph Input" :
	$"{InputName} ({InputData.InputType})";

	/// <summary>
	/// Name of this input.
	/// </summary>
	public string InputName { get; set; } = "In0";

	/// <summary>
	/// Description of this input.
	/// </summary>
	[TextArea]
	public string InputDescription { get; set; } = "";

	//[global::Editor( "SGP.VariantValue" ), InlineEditor( Label = false )]
	[Hide]
	public VariantValueBase InputData { get; set; } = new VariantValueVector3( Vector3.Zero, SubgraphPortType.Vector3 );

	/// <summary>
	/// Is this input required to have a valid connection?
	/// </summary>
	public bool IsRequired { get; set; } = false;

	public int PortOrder { get; set; }

	[Input, Title( "Preview" ), Hide]
	public NodeInput PreviewInput { get; set; }

	public SubgraphInput()
	{
	}

	internal VariantParam<T> GetValueAsVariantParam<T>()
	{
		return InputData.GetAsVariantParam<T>( InputDescription );
	}

	internal T GetDefaultValue<T>()
	{
		return InputData.GetValue<T>();
	}

	internal void SetDefaultValue<T>( T value )
	{
		InputData.SetValue<T>( value );
	}

	[Output, Title( "Value" ), Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		if ( InputData.InputType == SubgraphPortType.Texture2DObject || InputData.InputType == SubgraphPortType.TextureCubeObject )
		{
			var resultType = ResultType.Invalid;
			var textureInput = InputData.GetValue<TextureInput>();
			var textureGlobal = "";
			if ( InputData.InputType == SubgraphPortType.Texture2DObject )
			{
				textureInput = textureInput with { Name = InputName, Type = TextureType.Tex2D };
				textureGlobal = compiler.ResultTexture( textureInput, true );
				resultType =  ResultType.Texture2DObject;
			}
			else
			{
				textureInput = textureInput with { Name = InputName, Type = TextureType.TexCube };
				textureGlobal = compiler.ResultTexture( textureInput, true );
				resultType = ResultType.TextureCubeObject;
			}

			var result = new NodeResult( resultType, "TextureInput", textureInput );

			SGPLog.Info( $"SubgraphInput.Result // textureGlobal is : {textureGlobal}" );

			result.AddMetadataEntry( "TextureGlobal", textureGlobal );

			return result;
		}
		else
		{
			(ResultType resultType, string defaultCode) defaultResult = InputData.InputType switch
			{
				SubgraphPortType.Bool => (ResultType.Bool, $"{compiler.ResultValue( InputData.GetValue<bool>() )}"),
				SubgraphPortType.Int => (ResultType.Int, $"{compiler.ResultValue( InputData.GetValue<int>() )}"),
				SubgraphPortType.Float => (ResultType.Float, $"{compiler.ResultValue( InputData.GetValue<float>() )}"),
				SubgraphPortType.Vector2 => (ResultType.Vector2, $"float2( {compiler.ResultValue( InputData.GetValue<Vector2>() )} )"),
				SubgraphPortType.Vector3 => (ResultType.Vector3, $"float3( {compiler.ResultValue( InputData.GetValue<Vector3>() )} )"),
				SubgraphPortType.Vector4 => (ResultType.Color, $"float4( {compiler.ResultValue( InputData.GetValue<Vector4>() )} )"),
				SubgraphPortType.Color => (ResultType.Color, $"float4( {compiler.ResultValue( InputData.GetValue<Color>() )} )"),
				SubgraphPortType.SamplerState => (ResultType.Sampler, $"{compiler.ResultSampler( InputData.GetValue<Sampler>() )}"),
				_ => throw new Exception( $"Unknown PortType \"{InputData.InputType}\"" )
			};

			//SGPLog.Info( $"defaultResult is : {defaultResult.defaultCode}" );

			return new NodeResult( defaultResult.resultType, defaultResult.defaultCode, constant: true );
		}
	};

	public NodeResult GetResult( GraphCompiler compiler )
	{
		return Result.Invoke( compiler );
	}

	public List<string> GetWarnings()
	{
		var warnings = new List<string>();

		return warnings;
	}

	public List<string> GetErrors()
	{
		List<string> errors = new List<string>();

		if ( string.IsNullOrWhiteSpace( InputName ) )
		{
			errors.Add( $"SubgraphInput of InputType \"{InputData.InputType}\" must have a name!" );
		}

		//if ( InputName.Contains( ' ' ) )
		//{
		//	errors.Add( $"Parameter name \"{InputName}\" cannot contain spaces" );
		//}

		if ( InputData.InputType == SubgraphPortType.Invalid )
		{
			errors.Add( $"SubgraphInput InputType is invalid!" );
		}

		return errors;
	}

}
