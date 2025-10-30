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

		if ( isSubgraph )
		{
			BlackboardParameter = CreateBlackboardSubgraphInputeParameterFromConstant( Name, blackboardParameterIdentifier, IConstantNode );
		}
		else
		{
			BlackboardParameter = CreateBlackboardMaterialParameterFromConstant( iParameterNodeType, Name, blackboardParameterIdentifier, IConstantNode );
		}

		var parameterNode = BlackboardParameter.InitializeNode();
		parameterNode.Identifier = IConstantNode.Identifier;

		return parameterNode;
	}

	internal static BaseBlackboardParameter CreateBlackboardMaterialParameterFromConstant( Type parameterNodeType, string name, Guid guid, IConstantNode iConstantNode )
	{
		if ( iConstantNode is IRangedConstantNode iRangedConstant )
		{
			var stepValue = (float)iRangedConstant.GetStepValue();

			return parameterNodeType switch
			{
				Type t when t == typeof( IntParameterNode ) => new IntParameter( (int)iConstantNode.GetValue(), false )
				{
					Name = name,
					Identifier = guid,
					Min = (int)iRangedConstant.GetMinValue(),
					Max = (int)iRangedConstant.GetMaxValue(),
				},
				Type t when t == typeof( FloatParameterNode ) => new FloatParameter( (float)iConstantNode.GetValue(), false )
				{
					Name = name,
					Identifier = guid,
					Min = (float)iRangedConstant.GetMinValue(),
					Max = (float)iRangedConstant.GetMaxValue(),
					UI = new() { Step = stepValue, ShowStepProperty = true },
				},
				Type t when t == typeof( Float2ParameterNode ) => new Float2Parameter( (Vector2)iConstantNode.GetValue(), false )
				{
					Name = name,
					Identifier = guid,
					Min = (Vector2)iRangedConstant.GetMinValue(),
					Max = (Vector2)iRangedConstant.GetMaxValue(),
					UI = new() { Step = stepValue, ShowStepProperty = true },
				},
				Type t when t == typeof( Float3ParameterNode ) => new Float3Parameter( (Vector3)iConstantNode.GetValue(), false )
				{
					Name = name,
					Identifier = guid,
					Min = (Vector3)iRangedConstant.GetMinValue(),
					Max = (Vector3)iRangedConstant.GetMaxValue(),
					UI = new() { Step = stepValue, ShowStepProperty = true },
				},
				Type t when t == typeof( Float4ParameterNode ) => new Float4Parameter( (Vector4)iConstantNode.GetValue(), false )
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
			return parameterNodeType switch
			{
				Type t when t == typeof( BoolParameterNode ) => new BoolParameter( (bool)iConstantNode.GetValue(), false )
				{
					Name = name,
					Identifier = guid
				},
				Type t when t == typeof( ColorParameterNode ) => new ColorParameter( (Color)iConstantNode.GetValue(), false )
				{
					Name = name,
					Identifier = guid
				},
				_ => throw new NotImplementedException( $"Unknown type \"{parameterNodeType}\"" ),
			};
		}

	}

	internal static BaseBlackboardParameter CreateBlackboardSubgraphInputeParameterFromConstant( string name, Guid guid, IConstantNode iConstantNode )
	{
		var iConstantNodeType = iConstantNode.GetType();

		if ( iConstantNode is IRangedConstantNode iRangedConstant )
		{
			//var stepValue = (float)iRangedConstant.GetStepValue();

			return iConstantNodeType switch
			{
				Type t when t == typeof( IntConstantNode ) => new IntSubgraphInputParameter( (int)iConstantNode.GetValue() )
				{
					Name = name,
					Identifier = guid,
					Min = (int)iRangedConstant.GetMinValue(),
					Max = (int)iRangedConstant.GetMaxValue(),
					IsRequired = false,
				},
				Type t when t == typeof( FloatConstantNode ) => new FloatSubgraphInputParameter( (float)iConstantNode.GetValue() )
				{
					Name = name,
					Identifier = guid,
					Min = (float)iRangedConstant.GetMinValue(),
					Max = (float)iRangedConstant.GetMaxValue(),
					IsRequired = false,
				},
				Type t when t == typeof( Float2ConstantNode ) => new Float2SubgraphInputParameter( (Vector2)iConstantNode.GetValue() )
				{
					Name = name,
					Identifier = guid,
					Min = (Vector2)iRangedConstant.GetMinValue(),
					Max = (Vector2)iRangedConstant.GetMaxValue(),
					IsRequired = false,
				},
				Type t when t == typeof( Float3ConstantNode ) => new Float3SubgraphInputParameter( (Vector3)iConstantNode.GetValue() )
				{
					Name = name,
					Identifier = guid,
					Min = (Vector3)iRangedConstant.GetMinValue(),
					Max = (Vector3)iRangedConstant.GetMaxValue(),
					IsRequired = false,
				},
				Type t when t == typeof( Float4ConstantNode ) => new Float4SubgraphInputParameter( (Vector4)iConstantNode.GetValue() )
				{
					Name = name,
					Identifier = guid,
					Min = (Vector4)iRangedConstant.GetMinValue(),
					Max = (Vector4)iRangedConstant.GetMaxValue(),
					IsRequired = false,
				},
				_ => throw new NotImplementedException( $"Unknown type \"{iConstantNodeType}\"" ),
			};
		}
		else
		{
			return iConstantNodeType switch
			{
				Type t when t == typeof( BoolConstantNode ) => new BoolSubgraphInputParameter( (bool)iConstantNode.GetValue() )
				{
					Name = name,
					Identifier = guid,
					IsRequired = false,
				},
				Type t when t == typeof( ColorConstantNode ) => new ColorSubgraphInputParameter( (Color)iConstantNode.GetValue() )
				{
					Name = name,
					Identifier = guid,
					IsRequired = false,
				},
				_ => throw new NotImplementedException( $"Unknown type \"{iConstantNodeType}\"" ),
			};
		}

	}
}
