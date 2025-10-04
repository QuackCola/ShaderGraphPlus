using Facepunch.ActionGraphs;
using NodeEditorPlus;
using ShaderGraphPlus.Nodes;
using static Sandbox.Material;

namespace ShaderGraphPlus;

public sealed class ParameterNodeType : ClassNodeType
{
	public BaseBlackboardParameter BlackboardParameter { get; private set; }

	string Name;
	Type BlackboardParameterType;

	public ParameterNodeType( TypeDescription type, Type targetBlackboardType,string name ) : base( type )
	{
		Name = name;
		BlackboardParameterType = targetBlackboardType;
	}

	internal static BaseNodePlus InitParameterNode( BaseNodePlus parameterNode, string name ,Guid blackboardParameterIdentifier,
		object value = null,
		object minValue = null,
		object maxValue = null,
		float stepValue = 0.0f
	)
	{
		value ??= parameterNode switch
		{
			BoolParameterNode => false,
			IntParameterNode => 1,
			FloatParameterNode => 1.0f,
			Float2ParameterNode => Vector2.One,
			Float3ParameterNode => Vector3.One,
			Float4ParameterNode => Vector4.One,
			ColorParameterNode => Color.White,
			_ => throw new NotImplementedException(),
		};

		if ( parameterNode is not BoolParameterNode || parameterNode is not ColorParameterNode )
		{
			minValue ??= parameterNode switch
			{
				IntParameterNode => 0,
				FloatParameterNode => 0.0f,
				Float2ParameterNode => Vector2.Zero,
				Float3ParameterNode => Vector3.Zero,
				Float4ParameterNode => Vector4.Zero,
				_ => throw new NotImplementedException(),
			};

			maxValue ??= parameterNode switch
			{
				IntParameterNode => 1,
				FloatParameterNode => 1.0f,
				Float2ParameterNode => Vector2.One,
				Float3ParameterNode => Vector3.One,
				Float4ParameterNode => Vector4.One,
				_ => throw new NotImplementedException(),
			};
		}

		SGPLog.Info($"Step Value is : {stepValue}");

		return parameterNode switch
		{
			BoolParameterNode => new BoolParameterNode()
			{
				Value = (bool)value,
				Name = name,
				BlackboardParameterIdentifier = blackboardParameterIdentifier
			},
			IntParameterNode => new IntParameterNode()
			{
				Value = (int)value,
				Min = (int)minValue,
				Max = (int)maxValue,
				Name = name,
				BlackboardParameterIdentifier = blackboardParameterIdentifier
			},
			FloatParameterNode => new FloatParameterNode()
			{
				Value = (float)value,
				Min = (float)minValue,
				Max = (float)maxValue,
				UI = new() { Step = stepValue, ShowStepProperty = true },
				Name = name,
				BlackboardParameterIdentifier = blackboardParameterIdentifier
			},
			Float2ParameterNode => new Float2ParameterNode()
			{
				Value = (Vector2)value,
				Min = (Vector2)minValue,
				Max = (Vector2)maxValue,
				UI = new() { Step = stepValue, ShowStepProperty = true },
				Name = name,
				BlackboardParameterIdentifier = blackboardParameterIdentifier
			},
			Float3ParameterNode => new Float3ParameterNode()
			{
				Value = (Vector3)value,
				Min = (Vector3)minValue,
				Max = (Vector3)maxValue,
				UI = new() { Step = stepValue, ShowStepProperty = true },
				Name = name,
				BlackboardParameterIdentifier = blackboardParameterIdentifier
			},
			Float4ParameterNode => new Float4ParameterNode()
			{
				Value = (Vector4)value,
				Min = (Vector4)minValue,
				Max = (Vector4)maxValue,
				UI = new() { Step = stepValue, ShowStepProperty = true },
				Name = name,
				BlackboardParameterIdentifier = blackboardParameterIdentifier
			},
			ColorParameterNode => new ColorParameterNode()
			{
				Value = (Color)value,
				Name = name,
				BlackboardParameterIdentifier = blackboardParameterIdentifier
			},
			_ => throw new NotImplementedException(),
		};
	}

	public override INodePlus CreateNode( INodeGraph graph )
	{
		var node = base.CreateNode( graph );

		var blackboardParameterIdentifier = Guid.NewGuid();
		var iParameterNodeType = node.GetType();

		BlackboardParameter = BaseBlackboardParameter.CreateTypeInstance( BlackboardParameterType, Name, blackboardParameterIdentifier );

		return InitParameterNode( (BaseNodePlus)node, Name, blackboardParameterIdentifier );
	}
}
