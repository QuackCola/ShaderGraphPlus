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

	public override IGraphNode CreateNode( INodeGraph graph )
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
		if ( iConstantNode is IRangedConstantNode iRangedConstant )
		{
			var stepValue = (float)iRangedConstant.GetStepValue();

			SGPLog.Info( "iConstantNode is iRangeConstant" );

			return parameterNodeType switch
			{
				Type t when t == typeof( IntParameterNode ) => new IntParameter( (int)iConstantNode.GetValue() )
				{
					Name = name,
					Identifier = guid,
					Min = (int)iRangedConstant.GetMinValue(),
					Max = (int)iRangedConstant.GetMaxValue(),
				},
				Type t when t == typeof( FloatParameterNode ) => new FloatParameter( (float)iConstantNode.GetValue() )
				{
					Name = name,
					Identifier = guid,
					Min = (float)iRangedConstant.GetMinValue(),
					Max = (float)iRangedConstant.GetMaxValue(),
					UI = new() { Step = stepValue, ShowStepProperty = true },
				},
				Type t when t == typeof( Float2ParameterNode ) => new Float2Parameter( (Vector2)iConstantNode.GetValue() )
				{
					Name = name,
					Identifier = guid,
					Min = (Vector2)iRangedConstant.GetMinValue(),
					Max = (Vector2)iRangedConstant.GetMaxValue(),
					UI = new() { Step = stepValue, ShowStepProperty = true },
				},
				Type t when t == typeof( Float3ParameterNode ) => new Float3Parameter( (Vector3)iConstantNode.GetValue() )
				{
					Name = name,
					Identifier = guid,
					Min = (Vector3)iRangedConstant.GetMinValue(),
					Max = (Vector3)iRangedConstant.GetMaxValue(),
					UI = new() { Step = stepValue, ShowStepProperty = true },
				},
				Type t when t == typeof( Float4ParameterNode ) => new Float4Parameter( (Vector4)iConstantNode.GetValue() )
				{
					Name = name,
					Identifier = guid,
					Min = (Vector4)iRangedConstant.GetMinValue(),
					Max = (Vector4)iRangedConstant.GetMaxValue(),
					UI = new() { Step = stepValue, ShowStepProperty = true },
				},
				_ => throw new NotImplementedException( $"Unknown type \"{parameterNodeType}\"" ),
			};
		}
		else
		{
			SGPLog.Info( "iConstantNode is not iRangeConstant" );

			return parameterNodeType switch
			{
				Type t when t == typeof( BoolParameterNode ) => new BoolParameter( (bool)iConstantNode.GetValue() )
				{
					Name = name,
					Identifier = guid
				},
				Type t when t == typeof( ColorParameterNode ) => new ColorParameter( (Color)iConstantNode.GetValue() )
				{
					Name = name,
					Identifier = guid
				},
				_ => throw new NotImplementedException( $"Unknown type \"{parameterNodeType}\"" ),
			};
		}

	}
}
