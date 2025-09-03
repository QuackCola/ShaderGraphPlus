
using NodeEditorPlus;
using GraphView = NodeEditorPlus.GraphView;
using NodeUI = NodeEditorPlus.NodeUI;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;

namespace ShaderGraphPlus.Nodes;

/// <summary>
///
/// </summary>
[Title( "Render Target Size" ), Category( "PostProcessing/Variables" )]
public sealed class RenderTargetSizeNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Output( typeof( Vector2 ) )]
	[Hide]
	public static NodeResult.Func RenderTargetSize => ( GraphCompiler compiler ) => new( ResultType.Vector2, "g_vRenderTargetSize" );
}
