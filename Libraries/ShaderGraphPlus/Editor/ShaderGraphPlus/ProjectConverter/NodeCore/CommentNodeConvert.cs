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
		newNode.Color = oldCommentNode.Color;
		newNode.Title = oldCommentNode.Title;
		newNode.Description = oldCommentNode.Description;
		newNode.Layer = oldCommentNode.Layer;

		newNodes.Add( newNode );

		return newNodes;
	}
}
