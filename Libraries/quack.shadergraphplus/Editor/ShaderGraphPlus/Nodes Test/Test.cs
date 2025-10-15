using NodeEditorPlus;
using GraphView = NodeEditorPlus.GraphView;
using NodeUI = NodeEditorPlus.NodeUI;
using IPlugIn = NodeEditorPlus.INodePlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;

namespace ShaderGraphPlus.Nodes;



[Title( "Test Node" ), Description( "Test node." ), Category( "Dev" )]
[Hide]
public sealed class TestNode : ShaderNodePlus
{
	public override int Version => 1;
}

