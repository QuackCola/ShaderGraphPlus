
namespace Editor.ShaderGraphPlus;

/// <summary>
/// Final result
/// </summary>
[Title( "Material" )]
public class Result : ShaderNodePlus
{
	[Hide]
	[Input( typeof( Vector3 ) )]
	public NodeInput Albedo { get; set; }

	[Hide]
	[Input( typeof( Vector3 ) )]
	public NodeInput Emission { get; set; }

	[Hide, Editor( nameof( DefaultOpacity ) )]
	[Input( typeof( float ) )]
	public NodeInput Opacity { get; set; }

	// This does fuck all?
	//[Hide, Editor( nameof( DefaultTintMask ) )]
	//[Input( typeof( float ) )]
	//public NodeInput TintMask { get; set; }

	[Hide]
	[Input( typeof( Vector3 ) )]
	public NodeInput Normal { get; set; }

	[Hide, Editor( nameof( DefaultRoughness ) )]
	[Input( typeof( float ) )]
	public NodeInput Roughness { get; set; }

	[Hide, Editor( nameof( DefaultMetalness ) )]
	[Input( typeof( float ) )]
	public NodeInput Metalness { get; set; }

	[Hide, Editor( nameof( DefaultAmbientOcclusion ) )]
	[Input( typeof( float ) )]
	public NodeInput AmbientOcclusion { get; set; }

	public float DefaultOpacity { get; set; } = 1.0f;
	//public float DefaultTintMask { get; set; } = 1.0f;
	public float DefaultRoughness { get; set; } = 1.0f;
	public float DefaultMetalness { get; set; } = 0.0f;
	public float DefaultAmbientOcclusion { get; set; } = 1.0f;

	[Hide]
	[Input( typeof( Vector3 ) )]
	public NodeInput PositionOffset { get; set; }

	//public string[] Defines { get; set; }


	///public List<ShaderFeature> ShaderFeatures { get; set; }

	//public Dictionary<string, string> Defines { get; set; } = new();

	// All of these do fuck all?

	//[Hide]
	//[Input( typeof( Vector3 ) )] 
	//public NodeInput Sheen { get; set; }

	//[Hide]
	//[Input( typeof( float ) )]
	//public NodeInput SheenRoughness { get; set; }

	//[Hide]
	//[Input( typeof( float ) )] 
	//public NodeInput Clearcoat { get; set; }

	//[Hide]
	//[Input( typeof( float ) )] 
	//public NodeInput ClearcoatRoughness { get; set; }

	//[Hide]
	//[Input( typeof( Vector3 ) )] 
	//public NodeInput ClearcoatNormal { get; set; }

	//[Hide]
	//[Input( typeof( float ) )]
	//public NodeInput Anisotropy { get; set; }

	//[Hide]
	//[Input( typeof( Vector3 ) )] 
	//public NodeInput AnisotropyRotation { get; set; }

	//[Hide]
	//[Input( typeof( float ) )]
	//public NodeInput Thickness { get; set; }

	//[Hide]
	//[Input( typeof( float ) )] 
	//public NodeInput SubsurfacePower { get; set; }

	//[Hide]
	//[Input( typeof( float ) )] 
	//public NodeInput SheenColor { get; set; }

	//[Hide]
	//[Input( typeof( Vector3 ) )]
	//public NodeInput SubsurfaceColor { get; set; }

	//[Hide]
	//[Input( typeof( float ) )] 
	//public NodeInput Transmission { get; set; }

	//[Hide]
	//[Input( typeof( Vector3 ) )]
	//public NodeInput Absorption { get; set; }

	//[Hide]
	//[Input( typeof( float ) )] 
	//public NodeInput IndexOfRefraction { get; set; }

	//[Hide]
	//[Input( typeof( float ) )] 
	//public NodeInput MicroThickness { get; set; }
}

/// <summary>
/// Final Postprocessing result
/// </summary>
[Title( "Post Processing" ), Category( "" )]
public sealed class PostProcessingResult : ShaderNodePlus
{
	[Hide]
	[Input( typeof( Vector3 ) )]
	public NodeInput SceneColor { get; set; }
}
