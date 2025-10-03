using NodeEditorPlus;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

public sealed class ParameterNodeType : ClassNodeType
{
	BaseBlackboardParameter BaseBlackboardValue;

	public ParameterNodeType( TypeDescription type, BaseBlackboardParameter value ) : base( type )
	{
		BaseBlackboardValue = value;
	}

	public override INodePlus CreateNode( INodeGraph graph )
	{
		var node = base.CreateNode( graph );

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

		BaseNodePlus parameterNode = node switch
		{
			BoolParameterNode => new BoolParameterNode() 
			{ Name = name, Value = (bool)value, BlackboardParameterIdentifier = identifier },
			IntParameterNode  => new IntParameterNode() 
			{ Name = name, Value = (int)value, BlackboardParameterIdentifier = identifier },
			FloatParameterNode  => new FloatParameterNode() 
			{ Name = name, Value = (float)value, BlackboardParameterIdentifier = identifier },
			Float2ParameterNode  => new Float2ParameterNode() 
			{ Name = name, Value = (Vector2)value, BlackboardParameterIdentifier = identifier },
			Float3ParameterNode  => new Float3ParameterNode() 
			{ Name = name, Value = (Vector3)value, BlackboardParameterIdentifier = identifier },
			Float4ParameterNode => new Float4ParameterNode()
			{ Name = name, Value = (Vector4)value, BlackboardParameterIdentifier = identifier },
			ColorParameterNode  => new ColorParameterNode() 
			{ Name = name, Value = (Color)value, BlackboardParameterIdentifier = identifier },
			StaticSwitchNode => new StaticSwitchNode 
			{ Feature = (ShaderFeatureBoolean)value, BlackboardParameterIdentifier = identifier },
			_ => throw new NotImplementedException(),
		};

		return parameterNode;
	}
}

public sealed class ConstantToParameterNodeType : ClassNodeType
{
	IConstantNode IConstantNode;
	string Name;

	public BaseBlackboardParameter BlackboardParameter { get; private set; }

	public ConstantToParameterNodeType( TypeDescription type, IConstantNode value, string name ) : base( type )
	{
		IConstantNode = value;
		Name = name;
	}

	public override INodePlus CreateNode( INodeGraph graph )
	{
		var node = base.CreateNode( graph );

		var name = Name;
		var value = IConstantNode.GetValue();
		var identifier = Guid.NewGuid();

		BlackboardParameter = node switch
		{
			BoolParameterNode => new BoolBlackboardParameter( (bool)value ) 
			{ Name = name, Identifier = identifier },
			IntParameterNode => new IntBlackboardParameter( (int)value ) 
			{ Name = name, Identifier = identifier },
			FloatParameterNode => new FloatBlackboardParameter( (float)value ) 
			{ Name = name, Identifier = identifier },
			Float2ParameterNode => new Float2BlackboardParameter( (Vector2)value ) 
			{ Name = name, Identifier = identifier },
			Float3ParameterNode => new Float3BlackboardParameter( (Vector3)value ) 
			{ Name = name, Identifier = identifier },
			Float4ParameterNode => new Float4BlackboardParameter( (Vector4)value ) 
			{ Name = name, Identifier = identifier },
			ColorParameterNode => new ColorBlackboardParameter( (Color)value ) 
			{ Name = name, Identifier = identifier },
			_ => throw new NotImplementedException(),
		};

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
			_ => throw new NotImplementedException(),
		};

		return parameterNode;
	}
}
