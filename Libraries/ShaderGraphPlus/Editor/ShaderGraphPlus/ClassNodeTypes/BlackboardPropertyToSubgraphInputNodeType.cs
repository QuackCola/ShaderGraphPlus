using NodeEditorPlus;

namespace ShaderGraphPlus;

public sealed class BlackboardPropertyToSubgraphInputNodeType : ClassNodeType
{
	BaseBlackboardParameter BaseBlackboardValue;

	public BlackboardPropertyToSubgraphInputNodeType( TypeDescription type, BaseBlackboardParameter value ) : base( type )
	{
		BaseBlackboardValue = value;
	}

	public override INodePlus CreateNode( INodeGraph graph )
	{
		var node = base.CreateNode( graph );
		var isSubgraph = ((ShaderGraphPlus)graph).IsSubgraph;

		string name = BaseBlackboardValue.Name;

		if ( BaseBlackboardValue is ShaderFeatureBooleanBlackboardParameter sfBoolBlackboardParameter )
		{
			name = sfBoolBlackboardParameter.Value.FeatureName;
		}
		else if ( BaseBlackboardValue is ShaderFeatureEnumBlackboardParameter sfEnumBlackboardParameter )
		{
			name = sfEnumBlackboardParameter.Value.FeatureName;
		}

		return ParameterNodeType.InitSubgraphInputNode( BaseBlackboardValue );
	}
}
