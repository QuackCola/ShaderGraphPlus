using ShaderGraphPlus.Nodes;
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

public enum SubgraphOutputPreviewType
{
	[Icon( "clear" )]
	None,
	[Icon( "palette" )]
	Albedo,
	[Icon( "brightness_5" )]
	Emission,
	[Icon( "opacity" )]
	Opacity,
	[Icon( "texture" )]
	Normal,
	[Icon( "terrain" )]
	Roughness,
	[Icon( "auto_awesome" )]
	Metalness,
	[Icon( "tonality" )]
	AmbientOcclusion,
	[Icon( "arrow_forward" )]
	PositionOffset
}

public class ShaderFunctionOutput
{
	[Hide]
	public Guid Id { get; } = Guid.NewGuid();

	public ShaderFunctionOutput()
	{

	}

	public ShaderFunctionOutput( Guid guid )
	{
		Id = guid;
	}

	/// <summary>
	/// Name of this output.
	/// </summary>

	public string OutputName { get; set; } = "In0";

	/// <summary>
	/// Description of this output.
	/// </summary>
	[TextArea]
	public string OutputDescription { get; set; } = "";

	public SubgraphOutputType OutputType { get; set; } = SubgraphOutputType.Vector3;

	public SubgraphOutputPreviewType Preview { get; set; } = SubgraphOutputPreviewType.None;

	public int PortOrder { get; set; } = 0;

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
				//case SubgraphOutputType.Float2x2:
				//	return "float2x2";
				//case SubgraphOutputType.Float3x3:
				//	return "float3x3";
				//case SubgraphOutputType.Float4x4:
				//	return "float4x4";
				case SubgraphOutputType.Sampler:
					return "SamplerState";
				case SubgraphOutputType.Texture2DObject:
					return "Texture2D";
				//case SubgraphOutputType.TextureCubeObject:
				//	return "TextureCube";
				default:
					throw new NotImplementedException( $"Unknown OutputType \"{OutputType}\"" );
			}
		}
	}

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
public sealed class SubgraphOutput : BaseResult, IErroringNode, IInitializeNode
{
	[Hide]
	public override int Version => 1;

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

	public void InitializeNode()
	{
		CreateInput();
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
	public BasePlugIn InternalInput = null;

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

	private void CreateInput()
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

	public void AddMaterialOutputs( GraphCompiler compiler, StringBuilder sb, SubgraphOutputPreviewType previewType, out List<string> errors )
	{
		errors = new List<string>();
		if ( previewType == SubgraphOutputPreviewType.Albedo )
		{
			var albedoResult = GetAlbedoResult( compiler );

			if ( !albedoResult.IsValid )
				return;

			sb.AppendLine( $"m.Albedo = {albedoResult.Cast( 3 )};" );
		}
		if ( previewType == SubgraphOutputPreviewType.Emission )
		{
			var emissionResult = GetEmissionResult( compiler );

			if ( !emissionResult.IsValid )
				return;

			sb.AppendLine( $"m.Emission = {emissionResult.Cast( 3 )};" );
		}
		if ( previewType == SubgraphOutputPreviewType.Opacity )
		{
			var opacityResult = GetOpacityResult( compiler );

			if ( !opacityResult.IsValid )
				return;

			sb.AppendLine( $"m.Opacity = {opacityResult.Cast( 1 )};" );
		}
		if ( previewType == SubgraphOutputPreviewType.Normal )
		{
			var normalResult = GetNormalResult( compiler );

			if ( !normalResult.IsValid )
				return;

			sb.AppendLine( $"m.Normal = {normalResult.Cast( 3 )};" );
		}
		if ( previewType == SubgraphOutputPreviewType.Roughness )
		{
			var roughnessResult = GetRoughnessResult( compiler );

			if ( !roughnessResult.IsValid )
				return;

			sb.AppendLine( $"m.Roughness = {roughnessResult.Cast( 1 )};" );
		}
		if ( previewType == SubgraphOutputPreviewType.Metalness )
		{
			var metalnessResult = GetMetalnessResult( compiler );

			if ( !metalnessResult.IsValid )
				return;

			sb.AppendLine( $"m.Metalness = {metalnessResult.Cast( 1 )};" );
		}
		if ( previewType == SubgraphOutputPreviewType.AmbientOcclusion )
		{
			var ambientOcclusionResult = GetAmbientOcclusionResult( compiler );

			if ( !ambientOcclusionResult.IsValid )
				return;

			sb.AppendLine( $"m.AmbientOcclusion = {ambientOcclusionResult.Cast( 1 )};" );
		}
	}

	public NodeInput? GetInputFromPreview( SubgraphOutputPreviewType previewType )
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
		var input = GetInputFromPreview( SubgraphOutputPreviewType.Albedo );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetEmission()
	{
		var input = GetInputFromPreview( SubgraphOutputPreviewType.Emission );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetOpacity()
	{
		var input = GetInputFromPreview( SubgraphOutputPreviewType.Opacity );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetNormal()
	{
		var input = GetInputFromPreview( SubgraphOutputPreviewType.Normal );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetRoughness()
	{
		var input = GetInputFromPreview( SubgraphOutputPreviewType.Roughness );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetMetalness()
	{
		var input = GetInputFromPreview( SubgraphOutputPreviewType.Metalness );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetAmbientOcclusion()
	{
		var input = GetInputFromPreview( SubgraphOutputPreviewType.AmbientOcclusion );
		if ( input is not null )
		{
			return input.Value;
		}
		return default;
	}

	public override NodeInput GetPositionOffset()
	{
		var input = GetInputFromPreview( SubgraphOutputPreviewType.PositionOffset );
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
