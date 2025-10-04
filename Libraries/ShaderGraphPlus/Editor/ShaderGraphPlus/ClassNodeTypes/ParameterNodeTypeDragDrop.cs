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
		var isSubgraph = ((ShaderGraphPlus)graph).IsSubgraph;

		string name = BaseBlackboardValue.Name;//BaseBlackboardValue switch

		if ( BaseBlackboardValue is ShaderFeatureBooleanBlackboardParameter sfBoolBlackboardParameter )
		{
			 name = sfBoolBlackboardParameter.Value.FeatureName;
		}
		else if ( BaseBlackboardValue is ShaderFeatureEnumBlackboardParameter sfEnumBlackboardParameter )
		{
			name = sfEnumBlackboardParameter.Value.FeatureName;
		}

		Guid identifier = BaseBlackboardValue.Identifier;
		object value = BaseBlackboardValue.GetValue();

		// Initialize the new parameterNode
		BaseNodePlus parameterNode = node switch
		{
			BoolParameterNode => new BoolParameterNode()
			{ Name = name, Value = (bool)value, BlackboardParameterIdentifier = identifier },
			IntParameterNode => new IntParameterNode()
			{ Name = name, Value = (int)value, BlackboardParameterIdentifier = identifier },
			FloatParameterNode => new FloatParameterNode()
			{ Name = name, Value = (float)value, BlackboardParameterIdentifier = identifier },
			Float2ParameterNode => new Float2ParameterNode()
			{ Name = name, Value = (Vector2)value, BlackboardParameterIdentifier = identifier },
			Float3ParameterNode => new Float3ParameterNode()
			{ Name = name, Value = (Vector3)value, BlackboardParameterIdentifier = identifier },
			Float4ParameterNode => new Float4ParameterNode()
			{ Name = name, Value = (Vector4)value, BlackboardParameterIdentifier = identifier },
			ColorParameterNode => new ColorParameterNode()
			{ Name = name, Value = (Color)value, BlackboardParameterIdentifier = identifier },
			StaticSwitchNode => new StaticSwitchNode
			{ Feature = (ShaderFeatureBoolean)value, BlackboardParameterIdentifier = identifier },
			_ => throw new NotImplementedException(),
		};

		return parameterNode;
	}
}
