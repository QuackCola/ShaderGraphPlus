namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Interface for nodes that want to setup anyting post node object deserializeation.
/// </summary>
public interface IInitializeNode
{
	public void InitializeNode();
}
