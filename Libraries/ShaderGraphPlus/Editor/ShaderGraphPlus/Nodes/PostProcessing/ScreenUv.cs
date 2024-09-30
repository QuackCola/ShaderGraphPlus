

namespace Editor.ShaderGraphPlus.Nodes;

/// <summary>
///  Return the current screen uvs.
/// </summary>
[Title( "ScreenUV" ), Category( "PostProcessing/Variables" )]
[PostProcessingCompatable]
public sealed class ScreenUVNode : ShaderNodePlus
{
	[Output( typeof( Vector2 ) )]
	[Hide]
	public static NodeResult.Func ScreenUV => ( GraphCompiler compiler ) => new( ResultType.Vector2, "i.vPositionSs.xy / g_vRenderTargetSize" );
}
