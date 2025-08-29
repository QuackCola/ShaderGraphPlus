using ShaderGraphPlus.Nodes;
using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

namespace ShaderGraphPlus.Internal;

internal class BranchNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Branch );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldBranchNode = oldNode as VanillaNodes.Branch;

		//SGPLog.Info( "Convert branch node" );

		var newNode = new Branch();

		// TODO : Replace with switch node or compare node depending on if the name is set.

		newNode.Name = oldBranchNode.Name;
		newNode.IsAttribute = oldBranchNode.IsAttribute;
		newNode.Position = oldBranchNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}
