using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

namespace ShaderGraphPlus.Internal;

internal class SubgraphInputNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaGraph.SubgraphInput );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldSubgraphInputNode = oldNode as VanillaGraph.SubgraphInput;

		var newNode = new SubgraphInput();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.InputName = oldSubgraphInputNode.InputName;
		newNode.InputDescription = oldSubgraphInputNode.InputDescription;
		newNode.InputData = oldSubgraphInputNode.InputType switch
		{
			VanillaGraph.InputType.Float => new VariantValueFloat( oldSubgraphInputNode.DefaultFloat, SubgraphPortType.Float ),
			VanillaGraph.InputType.Float2 => new VariantValueVector2( oldSubgraphInputNode.DefaultFloat2, SubgraphPortType.Vector2 ),
			VanillaGraph.InputType.Float3 => new VariantValueVector3( oldSubgraphInputNode.DefaultFloat3, SubgraphPortType.Vector3 ),
			VanillaGraph.InputType.Color => new VariantValueColor( oldSubgraphInputNode.DefaultColor, SubgraphPortType.Color ),
			_ => throw new NotImplementedException( $"Unknown InputType \"{oldSubgraphInputNode.InputType}\"" ),
		};
		newNode.IsRequired = oldSubgraphInputNode.IsRequired;
		newNode.PortOrder = oldSubgraphInputNode.PortOrder;

		newNodes.Add( newNode );

		return newNodes;
	}
}
