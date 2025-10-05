using NodeEditorPlus;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

public sealed class ConstantToParameterNodeType : ClassNodeType
{
	private readonly IConstantNode IConstantNode;
	private readonly string Name;

	public BaseBlackboardParameter BlackboardParameter { get; private set; }

	public ConstantToParameterNodeType( TypeDescription type, IConstantNode value, string name ) : base( type )
	{
		IConstantNode = value;
		Name = name;
	}

	public override INodePlus CreateNode( INodeGraph graph )
	{
		var node = base.CreateNode( graph );
		var isSubgraph = ((ShaderGraphPlus)graph).IsSubgraph;

		var blackboardParameterIdentifier = Guid.NewGuid();
		var iParameterNodeType = node.GetType();

		BlackboardParameter = CreateBlackboardParameterFromConstant( iParameterNodeType, Name, blackboardParameterIdentifier, IConstantNode );

		var parameterNode = BlackboardParameter.InitializeNode();
		parameterNode.Identifier = IConstantNode.Identifier;

		return parameterNode;
	}

	internal static BaseBlackboardParameter CreateBlackboardParameterFromConstant( Type parameterNodeType, string name, Guid guid, IConstantNode iConstantNode )
	{
		var stepValue = (float)iConstantNode.GetStepValue();

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
				UI = new() { Step = stepValue, ShowStepProperty = true },
			},
			Type t when t == typeof( Float2ParameterNode ) => new Float2BlackboardParameter( (Vector2)iConstantNode.GetValue() )
			{
				Name = name,
				Identifier = guid,
				Min = (Vector2)iConstantNode.GetMinValue(),
				Max = (Vector2)iConstantNode.GetMaxValue(),
				UI = new() { Step = stepValue, ShowStepProperty = true },
			},
			Type t when t == typeof( Float3ParameterNode ) => new Float3BlackboardParameter( (Vector3)iConstantNode.GetValue() )
			{
				Name = name,
				Identifier = guid,
				Min = (Vector3)iConstantNode.GetMinValue(),
				Max = (Vector3)iConstantNode.GetMaxValue(),
				UI = new() { Step = stepValue, ShowStepProperty = true },
			},
			Type t when t == typeof( Float4ParameterNode ) => new Float4BlackboardParameter( (Vector4)iConstantNode.GetValue() )
			{
				Name = name,
				Identifier = guid,
				Min = (Vector4)iConstantNode.GetMinValue(),
				Max = (Vector4)iConstantNode.GetMaxValue(),
				UI = new() { Step = stepValue, ShowStepProperty = true },
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
