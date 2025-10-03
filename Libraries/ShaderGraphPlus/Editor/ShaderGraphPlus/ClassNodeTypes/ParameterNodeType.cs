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

		// Initialize the new parameterNode
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

		var blackboardParameterIdentifier = Guid.NewGuid();
		var iParameterNodeType = node.GetType();

		BlackboardParameter = CreateBlackboardParameterFromConstant( iParameterNodeType, Name, blackboardParameterIdentifier, IConstantNode );

		// Initialize the new parameterNode
		BaseNodePlus parameterNode = node switch
		{
			BoolParameterNode => new BoolParameterNode() 
			{ 
				Name = Name, 
				Value = (bool)IConstantNode.GetValue(), 
				BlackboardParameterIdentifier = blackboardParameterIdentifier 
			},
			IntParameterNode => new IntParameterNode() 
			{ 
				Name = Name, 
				Value = (int)IConstantNode.GetValue(),
				Min = (int)IConstantNode.GetMinValue(),
				Max = (int)IConstantNode.GetMaxValue(),
				BlackboardParameterIdentifier = blackboardParameterIdentifier
			},
			FloatParameterNode => new FloatParameterNode()
			{ 
				Name = Name,
				Value = (float)IConstantNode.GetValue(),
				Min = (float)IConstantNode.GetMinValue(),
				Max = (float)IConstantNode.GetMaxValue(),
				UI = new() { Step = (float)IConstantNode.GetStepValue() },
				BlackboardParameterIdentifier = blackboardParameterIdentifier
			},
			Float2ParameterNode => new Float2ParameterNode()
			{ 
				Name = Name,
				Value = (Vector2)IConstantNode.GetValue(),
				Min = (Vector2)IConstantNode.GetMinValue(),
				Max = (Vector2)IConstantNode.GetMaxValue(),
				UI = new() { Step = (float)IConstantNode.GetStepValue() },
				BlackboardParameterIdentifier = blackboardParameterIdentifier
			},
			Float3ParameterNode => new Float3ParameterNode()
			{
				Name = Name,
				Value = (Vector3)IConstantNode.GetValue(),
				Min = (Vector3)IConstantNode.GetMinValue(),
				Max = (Vector3)IConstantNode.GetMaxValue(),
				UI = new() { Step = (float)IConstantNode.GetStepValue() },
				BlackboardParameterIdentifier = blackboardParameterIdentifier
			},
			Float4ParameterNode => new Float4ParameterNode()
			{
				Name = Name,
				Value = (Vector4)IConstantNode.GetValue(),
				Min = (Vector4)IConstantNode.GetMinValue(),
				Max = (Vector4)IConstantNode.GetMaxValue(),
				UI = new() { Step = (float)IConstantNode.GetStepValue() },
				BlackboardParameterIdentifier = blackboardParameterIdentifier
			},
			ColorParameterNode => new ColorParameterNode()
			{
				Name = Name,
				Value = (Color)IConstantNode.GetValue(),
				BlackboardParameterIdentifier = blackboardParameterIdentifier
			},
			_ => throw new NotImplementedException(),
		};

		parameterNode.Identifier = IConstantNode.Identifier;

		return parameterNode;
	}

	internal static BaseBlackboardParameter CreateBlackboardParameterFromConstant( Type parameterNodeType, string name, Guid guid, IConstantNode iConstantNode )
	{
		return parameterNodeType switch
		{
			Type t when t == typeof( BoolParameterNode ) => new BoolBlackboardParameter( (bool)iConstantNode.GetValue() )
			{
				Name = name,
				Identifier = guid
			},
			Type t when t == typeof( IntParameterNode ) => new IntBlackboardParameter( (int)iConstantNode.GetValue() )
			{
				Name = name,
				Identifier = guid,
				Min = (int)iConstantNode.GetMinValue(),
				Max = (int)iConstantNode.GetMaxValue(),
			},
			Type t when t == typeof( FloatParameterNode ) => new FloatBlackboardParameter( (float)iConstantNode.GetValue() )
			{
				Name = name,
				Identifier = guid,
				Min = (float)iConstantNode.GetMinValue(),
				Max = (float)iConstantNode.GetMaxValue(),
			},
			Type t when t == typeof( Float2ParameterNode ) => new Float2BlackboardParameter( (Vector2)iConstantNode.GetValue() )
			{
				Name = name,
				Identifier = guid,
				Min = (Vector2)iConstantNode.GetMinValue(),
				Max = (Vector2)iConstantNode.GetMaxValue(),
			},
			Type t when t == typeof( Float3ParameterNode ) => new Float3BlackboardParameter( (Vector3)iConstantNode.GetValue() )
			{
				Name = name,
				Identifier = guid,
				Min = (Vector3)iConstantNode.GetMinValue(),
				Max = (Vector3)iConstantNode.GetMaxValue(),
			},
			Type t when t == typeof( Float4ParameterNode ) => new Float4BlackboardParameter( (Vector4)iConstantNode.GetValue() )
			{
				Name = name,
				Identifier = guid,
				Min = (Vector4)iConstantNode.GetMinValue(),
				Max = (Vector4)iConstantNode.GetMaxValue(),
			},
			Type t when t == typeof( ColorParameterNode ) => new ColorBlackboardParameter( (Color)iConstantNode.GetValue() )
			{
				Name = name,
				Identifier = guid
			},
			_ => throw new NotImplementedException(),
		};
	}
}
