
using NodeEditorPlus;
using GraphView = NodeEditorPlus.GraphView;
using NodeUI = NodeEditorPlus.NodeUI;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;

namespace ShaderGraphPlus.Nodes;

/// <summary>
///
/// </summary>
[Title( "Render Target Size" ), Category( "Variables/PostProcessing" )]
public sealed class RenderTargetSizeNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override (Color LeftColor, Color RightColor) PrimaryHeaderTheme => new( Color.Parse( "#803334" )!.Value, Color.Parse( "#803334" )!.Value.Darken( .5f ) );

	[Output( typeof( Vector2 ) )]
	[Hide]
	public static NodeResult.Func RenderTargetSize => ( GraphCompiler compiler ) => new( ResultType.Vector2, "g_vRenderTargetSize" );
}
