using Editor.ShaderGraphPlus;

/// <summary>
///
/// </summary>
[Title( "Render Target Size" ), Category( "PostProcessing/Variables" )]
public sealed class RenderTargetSizeNode : ShaderNodePlus
{
	[Output( typeof( Vector2 ) )]
	[Hide]
	public static NodeResult.Func RenderTargetSize => ( GraphCompiler compiler ) => new( ResultType.Vector2, "g_vRenderTargetSize" );
}
