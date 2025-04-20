
namespace Editor.ShaderGraphPlus;

/// <summary>
/// Final result of the lighting calcuations.
/// </summary>
[Title( "Light Result" ), Icon( "output" )]
public sealed class LightingResult : BaseResult
{
	[Hide]
	[Input( typeof( Vector3 ) )]
	public NodeInput Albedo { get; set; }

	public override NodeInput GetAlbedo()
	{
		if ( Albedo.IsValid )
			return Albedo;
		
		return default;
	}
}
