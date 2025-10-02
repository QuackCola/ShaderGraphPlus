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

		string name = BaseBlackboardValue switch
		{
			BoolBlackboardParameter v => v.Name,
			IntBlackboardParameter v => v.Name,
			FloatBlackboardParameter v => v.Name,
			Float2BlackboardParameter v => v.Name,
			Float3BlackboardParameter v => v.Name,
			Float4BlackboardParameter v => v.Name,
			ShaderFeatureBooleanBlackboardParameter v => v.Value.FeatureName,
			ShaderFeatureEnumBlackboardParameter v => v.Value.FeatureName,
			_ => throw new NotImplementedException(),
		};

		Guid identifier = BaseBlackboardValue.Identifier;

		object value = BaseBlackboardValue switch
		{
			BoolBlackboardParameter v => v.Value,
			IntBlackboardParameter v => v.Value,
			FloatBlackboardParameter v => v.Value,
			Float2BlackboardParameter v => v.Value,
			Float3BlackboardParameter v => v.Value,
			Float4BlackboardParameter v => v.Value,
			ShaderFeatureBooleanBlackboardParameter v => v.Value,
			ShaderFeatureEnumBlackboardParameter v => v.Value,
			_ => throw new NotImplementedException(),
		};

		BaseNodePlus parameterNode = node switch
		{
			Bool => new Bool() { Name = name, Value = (bool)value, BlackboardParameterIdentifier = identifier, ParameterNodeType = ParameterNodeModeType.Property },
			Int  => new Int() { Name = name, Value = (int)value, BlackboardParameterIdentifier = identifier, ParameterNodeType = ParameterNodeModeType.Property },
			Float  => new Float() { Name = name, Value = (float)value, BlackboardParameterIdentifier = identifier, ParameterNodeType = ParameterNodeModeType.Property },
			Float2  => new Float2() { Name = name, Value = (Vector2)value, BlackboardParameterIdentifier = identifier, ParameterNodeType = ParameterNodeModeType.Property },
			Float3  => new Float3() { Name = name, Value = (Vector3)value, BlackboardParameterIdentifier = identifier, ParameterNodeType = ParameterNodeModeType.Property },
			Float4  => new Float4() { Name = name, Value = (Color)value, BlackboardParameterIdentifier = identifier, ParameterNodeType = ParameterNodeModeType.Property },
			StaticSwitchNode => new StaticSwitchNode { Feature = (ShaderFeatureBoolean)value, BlackboardParameterIdentifier = identifier },
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

		BaseNodePlus parameterNode = node switch
		{
			Bool => new Bool() { Name = name, Value = (bool)value, BlackboardParameterIdentifier = identifier, ParameterNodeType = ParameterNodeModeType.Property },
			Int => new Int() { Name = name, Value = (int)value, BlackboardParameterIdentifier = identifier, ParameterNodeType = ParameterNodeModeType.Property },
			Float => new Float() { Name = name, Value = (float)value, BlackboardParameterIdentifier = identifier, ParameterNodeType = ParameterNodeModeType.Property },
			Float2 => new Float2() { Name = name, Value = (Vector2)value, BlackboardParameterIdentifier = identifier, ParameterNodeType = ParameterNodeModeType.Property },
			Float3 => new Float3() { Name = name, Value = (Vector3)value, BlackboardParameterIdentifier = identifier, ParameterNodeType = ParameterNodeModeType.Property },
			Float4 => new Float4() { Name = name, Value = (Color)value, BlackboardParameterIdentifier = identifier, ParameterNodeType = ParameterNodeModeType.Property },
			_ => throw new NotImplementedException(),
		};

		BlackboardParameter = node switch
		{
			Bool => new BoolBlackboardParameter( (bool)value ) { Name = name, Identifier = identifier },
			Int => new IntBlackboardParameter( (int)value ) { Name = name, Identifier = identifier },
			Float => new FloatBlackboardParameter( (float)value ) { Name = name, Identifier = identifier },
			Float2 => new Float2BlackboardParameter( (Vector2)value ) { Name = name, Identifier = identifier },
			Float3 => new Float3BlackboardParameter( (Vector3)value ) { Name = name, Identifier = identifier },
			Float4 => new Float4BlackboardParameter( (Color)value ) { Name = name, Identifier = identifier },
			_ => throw new NotImplementedException(),
		};

		return parameterNode;
	}
}
