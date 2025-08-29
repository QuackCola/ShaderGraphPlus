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

	/// <summary>
	/// TODO : Figure out howto solve cases where the Node Result name of an equivalent vanilla node isnt the same.
	/// </summary>
	public virtual Dictionary<string, string> GetNodeResultNameMapping()
	{
		return new Dictionary<string, string>();
	}
}
