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
		BaseNodePlus parameterNode = BaseBlackboardValue switch
		{
			BoolBlackboardParameter v => new SubgraphInput() 
			{
				InputName = v.Name,
				BlackboardParameterIdentifier = identifier,
				InputData = new VariantValueBool( v.Value, SubgraphPortType.Bool )
			},
			IntBlackboardParameter v => new SubgraphInput()
			{
				InputName = v.Name,
				BlackboardParameterIdentifier = identifier,
				InputData = new VariantValueInt( v.Value, SubgraphPortType.Int )
			},
			FloatBlackboardParameter v => new SubgraphInput()
			{
				InputName = v.Name,
				BlackboardParameterIdentifier = identifier,
				InputData = new VariantValueFloat( v.Value, SubgraphPortType.Float )
			},
			Float2BlackboardParameter v => new SubgraphInput()
			{
				InputName = v.Name,
				BlackboardParameterIdentifier = identifier,
				InputData = new VariantValueVector2( v.Value, SubgraphPortType.Vector2 )
			},
			Float3BlackboardParameter v => new SubgraphInput()
			{
				InputName = v.Name,
				BlackboardParameterIdentifier = identifier,
				InputData = new VariantValueVector3( v.Value, SubgraphPortType.Vector3 )
			},
			//Float4BlackboardParameter v => new SubgraphInput()
			//{
			//	InputData = new VariantValueVector4( v.Value, SubgraphPortType.Vector4 )
			//},
			ColorBlackboardParameter v => new SubgraphInput()
			{
				InputName = v.Name,
				BlackboardParameterIdentifier = identifier,
				InputData = new VariantValueColor( v.Value, SubgraphPortType.Color )
			},
			_ => throw new NotImplementedException(),
		};

		return parameterNode;
	}
}
