namespace ShaderGraphPlus;

public sealed class ParameterNodeType : ClassNodeType
{
	BaseBlackboardParameter BaseBlackboardValue;

	public ParameterNodeType( TypeDescription type, BaseBlackboardParameter value ) : base( type )
	{
		BaseBlackboardValue = value;
	}

	public override IGraphNode CreateNode( INodeGraph graph )
	{
		base.CreateNode( graph );
		return BaseBlackboardValue.InitializeNode();
	}
}
