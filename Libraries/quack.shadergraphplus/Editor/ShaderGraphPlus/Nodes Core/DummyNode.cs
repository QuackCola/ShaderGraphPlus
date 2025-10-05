using NodeEditorPlus;
using GraphView = NodeEditorPlus.GraphView;
using NodeUI = NodeEditorPlus.NodeUI;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;

namespace ShaderGraphPlus;

/// <summary>
/// Dummy node ment for situations where an error needs to not have a node you can clock to.
/// </summary>
[Hide]
internal sealed class DummyNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;
}
