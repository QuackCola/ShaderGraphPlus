namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Interface for nodes that want to setup anyting post node object deserializeation.
/// </summary>
public interface IInitializeNode
{
	public void InitializeNode();
}

/// <summary>
/// For nodes that are being replaced with something else.
/// </summary>
public interface IReplaceNode
{
	public bool ReplacementCondition { get; }
	public BaseNodePlus GetReplacementNode();
}
