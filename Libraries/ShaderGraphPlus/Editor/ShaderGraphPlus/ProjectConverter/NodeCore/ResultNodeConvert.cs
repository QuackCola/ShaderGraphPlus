using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

namespace ShaderGraphPlus.Internal;

internal class ResultNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaGraph.Result );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldResultNode = oldNode as VanillaGraph.Result;

		//SGPLog.Info( "Convert result node" );

		var newNode = new Result();
		newNode.DefaultAmbientOcclusion = oldResultNode.DefaultAmbientOcclusion;
		newNode.DefaultMetalness = oldResultNode.DefaultMetalness;
		newNode.DefaultOpacity = oldResultNode.DefaultOpacity;
		newNode.DefaultRoughness = oldResultNode.DefaultRoughness;
		newNode.Position = oldResultNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}
