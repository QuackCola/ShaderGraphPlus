using NodeEditorPlus;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

public sealed class ParameterNodeTypeDragDrop : ClassNodeType
{
	BaseBlackboardParameter BaseBlackboardValue;

	public ParameterNodeTypeDragDrop( TypeDescription type, BaseBlackboardParameter value ) : base( type )
	{
		BaseBlackboardValue = value;
	}

	public override INodePlus CreateNode( INodeGraph graph )
	{
		var node = base.CreateNode( graph );
		return BaseBlackboardValue.InitializeNode();
	}
}
