using NodeEditorPlus;
using GraphView = NodeEditorPlus.GraphView;
using NodeUI = NodeEditorPlus.NodeUI;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;

namespace ShaderGraphPlus.Nodes;



[Title( "Test Node" ), Description( "Test node." ), Category( "Dev" )]
[InternalNode]
public sealed class TestNode : ShaderNodePlus
{
	public override int Version => 1;
}

