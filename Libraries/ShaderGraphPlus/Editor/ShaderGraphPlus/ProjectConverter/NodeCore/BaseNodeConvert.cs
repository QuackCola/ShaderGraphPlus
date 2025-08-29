namespace ShaderGraphPlus.Internal;
using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

internal abstract class BaseNodeConvert
{
	public virtual Type NodeTypeToConvert { get; }

	public BaseNodeConvert()
	{

	}

	public virtual IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode vanillaNode )
	{
		throw new NotImplementedException();
	}
}
