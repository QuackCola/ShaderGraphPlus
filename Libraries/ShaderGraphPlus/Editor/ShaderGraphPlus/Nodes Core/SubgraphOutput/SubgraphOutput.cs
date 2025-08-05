using Editor.NodeEditor;
using System.Text;

namespace ShaderGraphPlus;

public enum SubgraphOutputType
{
	[Icon( "check_box" )]
	Bool,
	[Icon( "filter_1" )]
	Float,
	[Icon( "filter_2" )]
	Vector2,
	[Icon( "filter_3" )]
	Vector3,
	[Icon( "palette" )]
	Color,
	[Icon( "colorize" )]
	Sampler,
	[Title( "Texture2D Object" ), Icon( "texture" )]
	Texture2DObject,
	[Hide]
	Invalid
}

public class ShaderFunctionOutput
{
	[Hide]
	public Guid Id { get; } = Guid.NewGuid();

	/// <summary>
	/// Name of this output.
	/// </summary>

	public string OutputName { get; set; } = "In0";

	/// <summary>
	/// Description of this output.
	/// </summary>
	[TextArea]
	public string OutputDescription { get; set; }

	public SubgraphOutputType OutputType { get; set; } = SubgraphOutputType.Vector3;

	public int PortOrder { get; set; }

	[Hide, JsonIgnore]
	public Type Type
	{
		get
		{
			return OutputType switch
			{
				SubgraphOutputType.Bool => typeof( bool ),
				SubgraphOutputType.Float => typeof( float ),
				SubgraphOutputType.Vector2 => typeof( Vector2 ),
				SubgraphOutputType.Vector3 => typeof( Vector3 ),
				SubgraphOutputType.Color => typeof( Color ),
				SubgraphOutputType.Sampler => typeof( Sampler ),
				SubgraphOutputType.Texture2DObject => typeof( Texture2DObject ),
				_ => throw new NotImplementedException(),
			};
		}
	}

	[Hide, JsonIgnore]
	public string HLSLTypeName
	{
		get
		{
			switch ( OutputType )
			{
				case SubgraphOutputType.Bool:
					return "bool";
				case SubgraphOutputType.Float:
					return "float";
				case SubgraphOutputType.Vector2:
					return "float2";
				case SubgraphOutputType.Vector3:
					return "float3";
				case SubgraphOutputType.Color:
					return "float4";
				case SubgraphOutputType.Sampler:
					return "SamplerState";
				case SubgraphOutputType.Texture2DObject:
					return "Texture2D";
				default:
					return null;
			}
		}
	}

	public FunctionOutput.PreviewType Preview { get; set; }

	public void SetOutputTypeFromType( Type type )
	{
		switch ( type )
		{
			case Type t when t == typeof( bool ):
				OutputType = SubgraphOutputType.Bool;
				break;
			//case Type t when t == typeof( int ):
			//
			//	break;
			case Type t when t == typeof( float ):
				OutputType = SubgraphOutputType.Float;
				break;
			case Type t when t == typeof( Vector2 ):
				OutputType = SubgraphOutputType.Vector2;
				break;
			case Type t when t == typeof( Vector3 ):
				OutputType = SubgraphOutputType.Vector3;
				break;
			case Type t when t == typeof( Vector4 ):
				OutputType = SubgraphOutputType.Color;
				break;
			case Type t when t == typeof( Color ):
				OutputType = SubgraphOutputType.Color;
				break;
			//case Type t when t == typeof( Float2x2 ):
			//
			//	break;
			//case Type t when t == typeof( Float3x3 ):
			//
			//	break;
			//case Type t when t == typeof( Float4x4 ):
			//
			//	break;
			case Type t when t == typeof( Sampler ):
				OutputType = SubgraphOutputType.Sampler;
				break;
			case Type t when t == typeof( Texture2DObject ):
				OutputType = SubgraphOutputType.Texture2DObject;
				break;
			//case Type t when t == typeof( TextureCubeObject ):
			//
			//	break;
			default:
				throw new NotImplementedException( $"Unknown type \"{type}\"" );
		}
	}

	public override int GetHashCode()
	{
		return System.HashCode.Combine( Id, OutputName, OutputDescription, HLSLTypeName, PortOrder );
	}
}

/// <summary>
/// Output of a subgraph.
/// </summary>
[Title( "Subgraph Output" ), Icon( "output" ), SubgraphOnly]
public sealed class SubgraphOutput : BaseResult, IErroringNode
{
	[Hide, JsonIgnore]
	public override bool CanRemove => true;

	[Hide, JsonIgnore]
	int _lastHashCode = 0;

	[InlineEditor( Label = false )]
	public ShaderFunctionOutput SubgraphFunctionOutput { get; set; } = new ShaderFunctionOutput();

	public override void OnFrame()
	{
		var hashCode = 0;
		
		hashCode += SubgraphFunctionOutput.GetHashCode();

		if ( hashCode != _lastHashCode )
		{
			var oldhashCode = _lastHashCode;
			_lastHashCode = hashCode;

			//SGPLog.Info( $"SubgraphFunctionOutput hashcode changed from \"{oldhashCode}\" to \"{_lastHashCode}\"" );
			
			CreateInput();
			IsDirty = true;
			Update();
		}
	}

	public List<string> GetErrors()
	{
		var errors = new List<string>();
		if ( SubgraphFunctionOutput == null )
		{
			errors.Add( "No output defined" );
		}
		else
		{
			Dictionary<string, int> namesSoFar = new();
			var functionOutputs = new List<ShaderFunctionOutput>();
			functionOutputs.Add( SubgraphFunctionOutput );
			foreach ( var output in functionOutputs )
			{
				if ( output.OutputName is null )
				{
					errors.Add( $"{output.OutputType} Output has no name" );
					continue;
				}
				if ( !namesSoFar.ContainsKey( output.OutputName ) )
				{
					namesSoFar.Add( output.OutputName, 1 );
				}
				else
				{
					namesSoFar[output.OutputName]++;
				}
				if ( string.IsNullOrEmpty( output.OutputName ) )
				{
					errors.Add( $"{output.OutputType} Output has no name" );
				}
				if ( output.OutputType == SubgraphOutputType.Invalid )
				{
					errors.Add( $"Output '{output.OutputName}' has no type" );
				}
			}
			foreach ( var name in namesSoFar )
			{
				if ( name.Value > 1 )
				{
					errors.Add( $"Output name '{name.Key}' is used {name.Value} times" );
				}
			}
		}
		return errors;
	}

	[Hide]
	private BasePlugIn InternalInput = null;

	[Hide]
	public override IEnumerable<IPlugIn> Inputs
	{ 
		get
		{
			if ( InternalInput == null )
				return new List<BasePlugIn>();

			return new List<BasePlugIn> { InternalInput };
		}
	}

	public void CreateInput()
	{
		var plugInfo = new PlugInfo()
		{
			Id = SubgraphFunctionOutput.Id,
			Name = SubgraphFunctionOutput.OutputName,
			Type = SubgraphFunctionOutput.Type,
			DisplayInfo = new()
			{
				Name = SubgraphFunctionOutput.OutputName,
				Fullname = SubgraphFunctionOutput.Type.FullName,
				Description = SubgraphFunctionOutput.OutputDescription,
			}
		};
		
		var oldPlugIn = InternalInput;
		if ( oldPlugIn is not null )
		{
			oldPlugIn.Info.Name = plugInfo.Name;
			oldPlugIn.Info.Type = plugInfo.Type;
			oldPlugIn.Info.DisplayInfo = plugInfo.DisplayInfo;

			InternalInput = oldPlugIn;
		}
		else 
		{
			var plugIn = new BasePlugIn( this, plugInfo, plugInfo.Type );
			InternalInput = plugIn;
		}
	}

	public void AddMaterialOutputs( GraphCompiler compiler, StringBuilder sb, FunctionOutput.PreviewType previewType )
	{
		if ( previewType == FunctionOutput.PreviewType.Albedo )
		{
			var albedoResult = GetAlbedoResult( compiler );
			sb.AppendLine( $"m.Albedo = {albedoResult.Cast( 3 )};" );
		}
		if ( previewType == FunctionOutput.PreviewType.Emission )
		{
			var emissionResult = GetEmissionResult( compiler );
			sb.AppendLine( $"m.Emission = {emissionResult.Cast( 3 )};" );
		}
		if ( previewType == FunctionOutput.PreviewType.Opacity )
		{
			var opacityResult = GetOpacityResult( compiler );
			sb.AppendLine( $"m.Opacity = {opacityResult.Cast( 1 )};" );
		}
		if ( previewType == FunctionOutput.PreviewType.Normal )
		{
			var normalResult = GetNormalResult( compiler );
			sb.AppendLine( $"m.Normal = {normalResult.Cast( 3 )};" );
		}
		if ( previewType == FunctionOutput.PreviewType.Roughness )
		{
			var roughnessResult = GetRoughnessResult( compiler );
			sb.AppendLine( $"m.Roughness = {roughnessResult.Cast( 1 )};" );
		}
		if ( previewType == FunctionOutput.PreviewType.Metalness )
		{
			var metalnessResult = GetMetalnessResult( compiler );
			sb.AppendLine( $"m.Metalness = {metalnessResult.Cast( 1 )};" );
		}
		if ( previewType == FunctionOutput.PreviewType.AmbientOcclusion )
		{
			var ambientOcclusionResult = GetAmbientOcclusionResult( compiler );
			sb.AppendLine( $"m.AmbientOcclusion = {ambientOcclusionResult.Cast( 1 )};" );
		}
	}

	public NodeInput? GetInputFromPreview( FunctionOutput.PreviewType previewType )
	{
		var albedoOutput = SubgraphFunctionOutput;
		if ( albedoOutput is not null && SubgraphFunctionOutput.Preview == previewType )
		{
			var input = Inputs.FirstOrDefault( x => x is BasePlugIn plugIn && plugIn.Info.Id == albedoOutput.Id );
			if ( input is BasePlugIn plugIn )
			{
				
				if ( plugIn.Info.ConnectedPlug is not null )
				{
					return new NodeInput
					{
						Identifier = plugIn.Info.ConnectedPlug.Node.Identifier,
						Output = plugIn.Info.ConnectedPlug.Identifier
					};
				}

				return plugIn.Info.GetInput( plugIn.Node );
			}
		}
		return null;
	}

	public override NodeInput GetAlbedo()
	{
		var input = GetInputFromPreview( FunctionOutput.PreviewType.Albedo );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetEmission()
	{
		var input = GetInputFromPreview( FunctionOutput.PreviewType.Emission );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetOpacity()
	{
		var input = GetInputFromPreview( FunctionOutput.PreviewType.Opacity );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetNormal()
	{
		var input = GetInputFromPreview( FunctionOutput.PreviewType.Normal );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetRoughness()
	{
		var input = GetInputFromPreview( FunctionOutput.PreviewType.Roughness );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetMetalness()
	{
		var input = GetInputFromPreview( FunctionOutput.PreviewType.Metalness );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetAmbientOcclusion()
	{
		var input = GetInputFromPreview( FunctionOutput.PreviewType.AmbientOcclusion );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetPositionOffset()
	{
		var input = GetInputFromPreview( FunctionOutput.PreviewType.PositionOffset );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override Color GetDefaultAlbedo()
	{
		return base.GetDefaultAlbedo();
	}
}
