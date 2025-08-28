using Editor.ShaderGraph;
using Sandbox.Resources;
using ShaderGraphPlus.Nodes;
using System.Text;


namespace ShaderGraphPlus;

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

	public string OutputName { get; set; } = "Ouat0";

	/// <summary>
	/// Description of this output.
	/// </summary>
	[TextArea]
	public string OutputDescription { get; set; } = "";

	public SubgraphPortType OutputType { get; set; } = SubgraphPortType.Vector3;

	public SubgraphOutputPreviewType Preview { get; set; } = SubgraphOutputPreviewType.None;

	public int PortOrder { get; set; } = 0;

	[Hide, JsonIgnore]
	public Type Type
	{
		get
		{
			return OutputType switch
			{
				SubgraphPortType.Bool => typeof( bool ),
				SubgraphPortType.Int => typeof( int ),
				SubgraphPortType.Float => typeof( float ),
				SubgraphPortType.Vector2 => typeof( Vector2 ),
				SubgraphPortType.Vector3 => typeof( Vector3 ),
				SubgraphPortType.Color => typeof( Color ),
				SubgraphPortType.Sampler => typeof( Sampler ),
				SubgraphPortType.Texture2DObject => typeof( Texture2DObject ),
				_ => throw new Exception( $"Unknown PortType \"{OutputType}\"" )
			};
		}
	}

	public void SetSubgraphPortTypeFromType( Type type )
	{
		switch ( type )
		{
			case Type t when t == typeof( bool ):
				OutputType = SubgraphPortType.Bool;
				break;
			case Type t when t == typeof( int ):
				OutputType = SubgraphPortType.Int;
				break;
			case Type t when t == typeof( float ):
				OutputType = SubgraphPortType.Float;
				break;
			case Type t when t == typeof( Vector2 ):
				OutputType = SubgraphPortType.Vector2;
				break;
			case Type t when t == typeof( Vector3 ):
				OutputType = SubgraphPortType.Vector3;
				break;
			case Type t when t == typeof( Vector4 ):
				OutputType = SubgraphPortType.Color;
				break;
			case Type t when t == typeof( Color ):
				OutputType = SubgraphPortType.Color;
				break;
			case Type t when t == typeof( ColorTextureGenerator ):
				OutputType = SubgraphPortType.Color;
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
				OutputType = SubgraphPortType.Sampler;
				break;
			case Type t when t == typeof( Texture2DObject ):
				OutputType = SubgraphPortType.Texture2DObject;
				break;
			//case Type t when t == typeof( TextureCubeObject ):
			//
			//	break;
			default:
				throw new Exception( $"Unknown type \"{type}\"" );
		}
	}

	//public override int GetHashCode()
	//{
	//	return System.HashCode.Combine( Id, OutputName, OutputDescription, OutputType.GetHlslType(), PortOrder );
	//}
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

	[Hide]
	public Guid Id { get; internal set; } = Guid.NewGuid();

	public string OutputName { get; set; } = "Ouat0";

	[TextArea]
	public string OutputDescription { get; set; } = "";

	public SubgraphPortType OutputType { get; set; } = SubgraphPortType.Vector3;

	public SubgraphOutputPreviewType Preview { get; set; } = SubgraphOutputPreviewType.None;

	public int PortOrder { get; set; } = 0;

	public override void OnFrame()
	{
		var hashCode = new HashCode();
		hashCode.Add( Id );
		hashCode.Add( OutputName );
		hashCode.Add( OutputDescription );
		hashCode.Add( OutputType.GetHlslType() );
		hashCode.Add( PortOrder );

		var hc = hashCode.ToHashCode();

		if ( hc != _lastHashCode )
		{
			//var oldhashCode = _lastHashCode;
			_lastHashCode = hc;
			
			//SGPLog.Info( $"SubgraphFunctionOutput hashcode changed from \"{oldhashCode}\" to \"{_lastHashCode}\"" );
			
			CreateInput();
			IsDirty = true;
			Update();
		}
	}

	public void SetSubgraphPortTypeFromType( Type type )
	{
		switch ( type )
		{
			case Type t when t == typeof( bool ):
				OutputType = SubgraphPortType.Bool;
				break;
			case Type t when t == typeof( int ):
				OutputType = SubgraphPortType.Int;
				break;
			case Type t when t == typeof( float ):
				OutputType = SubgraphPortType.Float;
				break;
			case Type t when t == typeof( Vector2 ):
				OutputType = SubgraphPortType.Vector2;
				break;
			case Type t when t == typeof( Vector3 ):
				OutputType = SubgraphPortType.Vector3;
				break;
			case Type t when t == typeof( Vector4 ):
				OutputType = SubgraphPortType.Color;
				break;
			case Type t when t == typeof( Color ):
				OutputType = SubgraphPortType.Color;
				break;
			case Type t when t == typeof( ColorTextureGenerator ):
				OutputType = SubgraphPortType.Color;
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
				OutputType = SubgraphPortType.Sampler;
				break;
			case Type t when t == typeof( Texture2DObject ):
				OutputType = SubgraphPortType.Texture2DObject;
				break;
			//case Type t when t == typeof( TextureCubeObject ):
			//
			//	break;
			default:
				throw new Exception( $"Unknown type \"{type}\"" );
		}
	}

	public void InitializeNode()
	{
		CreateInput();
		IsDirty = true;
		Update();
	}

	public List<string> GetErrors()
	{
		var errors = new List<string>();

		if ( Graph is ShaderGraphPlus shaderGraphPlus && shaderGraphPlus.IsSubgraph )
		{
			foreach ( var node in Graph.Nodes )
			{
				if ( node == this ) continue;

				if ( node is SubgraphOutput otherOutput && otherOutput.OutputName == OutputName )
				{
					errors.Add( $"Duplicate output name \"{OutputName}\"" );
					break;
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
		var outputType = OutputType switch
		{
			SubgraphPortType.Bool => typeof( bool ),
			SubgraphPortType.Int => typeof( int ),
			SubgraphPortType.Float => typeof( float ),
			SubgraphPortType.Vector2 => typeof( Vector2 ),
			SubgraphPortType.Vector3 => typeof( Vector3 ),
			SubgraphPortType.Color => typeof( Color ),
			SubgraphPortType.Sampler => typeof( Sampler ),
			SubgraphPortType.Texture2DObject => typeof( Texture2DObject ),
			_ => throw new Exception( $"Unknown PortType \"{OutputType}\"" )
		};

		var plugInfo = new PlugInfo()
		{
			Id = Id,
			Name = OutputName,
			Type = outputType,
			DisplayInfo = new()
			{
				Name = OutputName,
				Fullname = outputType.FullName,
				Description = OutputDescription,
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
		if ( Preview == previewType )
		{
			var input = Inputs.FirstOrDefault( x => x is BasePlugIn plugIn && plugIn.Info.Id == Id );
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
