
namespace Editor.ShaderGraphPlus;

/// <summary>
/// Final result of the lighting calcuations.
/// </summary>
[Title( "Light Result" ), Icon( "output" ), Category( "Shading" )]
public sealed class LightingResult : BaseResult
{
	[Hide]
	[Input( typeof( Vector3 ) )]
	public NodeInput Albedo { get; set; }

	[Hide]
	[Input( typeof( Vector3 ) )]
	public NodeInput Indirect { get; set; }

	/// <summary>
	/// If you want fog to be applied to your fragment, set this to true.
	/// </summary>
	public bool ApplyFog { get; set; } = false;

	public override NodeInput GetAlbedo()
	{
		if ( Albedo.IsValid )
			return Albedo;
		
		return default;
	}
}
