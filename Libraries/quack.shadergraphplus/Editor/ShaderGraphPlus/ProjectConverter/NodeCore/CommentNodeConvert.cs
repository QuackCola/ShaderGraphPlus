using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

namespace ShaderGraphPlus.Internal;

internal class CommentNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaGraph.CommentNode );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();

		var oldCommentNode = oldNode as VanillaGraph.CommentNode;

		//SGPLog.Info( "Convert comment node" );

		var newNode = new CommentNode();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.Size = oldCommentNode.Size;
		newNode.Color = oldCommentNode.Color switch
		{
			Editor.NodeEditor.CommentColor.White => Color.Parse( "#c2b5b5" )!.Value,
			Editor.NodeEditor.CommentColor.Red => Color.Parse( "#d60000" )!.Value,
			Editor.NodeEditor.CommentColor.Green => Color.Parse( "#33b679" )!.Value,
			Editor.NodeEditor.CommentColor.Blue => Color.Parse( "#039be5" )!.Value,
			Editor.NodeEditor.CommentColor.Yellow => Color.Parse( "#f6c026" )!.Value,
			Editor.NodeEditor.CommentColor.Purple => Color.Parse( "#8e24aa" )!.Value,
			Editor.NodeEditor.CommentColor.Orange => Color.Parse( "#f5511d" )!.Value,
			_ => Color.Parse( "#c2b5b5" )!.Value,
		}; //(NodeEditorPlus.CommentColor)oldCommentNode.Color;
		newNode.Title = oldCommentNode.Title;
		newNode.Description = oldCommentNode.Description;
		newNode.Layer = oldCommentNode.Layer;

		newNodes.Add( newNode );

		return newNodes;
	}
}
