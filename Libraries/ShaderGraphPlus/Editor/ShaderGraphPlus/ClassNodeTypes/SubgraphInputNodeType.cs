using NodeEditorPlus;

namespace ShaderGraphPlus;

public sealed class SubgraphInputNodeType : ClassNodeType
{
	public BaseBlackboardParameter BlackboardParameter { get; private set; }

	private readonly string Name;
	private readonly Type TargetBlackboardParameterType;
	private readonly bool InitFromBlackBoardParameter;

	public SubgraphInputNodeType( TypeDescription type, Type targetBlackboardType, string name = "" ) : base( type )
	{
		Name = name;
		TargetBlackboardParameterType = targetBlackboardType;
		InitFromBlackBoardParameter = false;
	}

	public SubgraphInputNodeType( TypeDescription type, BaseBlackboardParameter blackboardParameter, string name = "" ) : base( type )
	{
		Name = name;
		InitFromBlackBoardParameter = true;
		BlackboardParameter = blackboardParameter;
	}

	public override INodePlus CreateNode( INodeGraph graph )
	{
		var node = base.CreateNode( graph );
		if ( node is SubgraphInput subgraphInput )
		{
			if ( !InitFromBlackBoardParameter )
			{
				SubgraphPortType inputType = TargetBlackboardParameterType switch
				{
					Type t when t == typeof( BoolSubgraphInputBlackboardParameter ) => SubgraphPortType.Bool,
					Type t when t == typeof( IntSubgraphInputBlackboardParameter ) => SubgraphPortType.Int,
					Type t when t == typeof( FloatSubgraphInputBlackboardParameter ) => SubgraphPortType.Float,
					Type t when t == typeof( Float2SubgraphInputBlackboardParameter ) => SubgraphPortType.Vector2,
					Type t when t == typeof( Float3SubgraphInputBlackboardParameter ) => SubgraphPortType.Vector3,
					Type t when t == typeof( Float4SubgraphInputBlackboardParameter ) => SubgraphPortType.Vector4,
					Type t when t == typeof( ColorSubgraphInputBlackboardParameter ) => SubgraphPortType.Color,
					_ => throw new NotImplementedException(),
				};

				var blackboardParameterIdentifier = Guid.NewGuid();
				BlackboardParameter = BaseBlackboardParameter.CreateTypeInstance( TargetBlackboardParameterType, Name, blackboardParameterIdentifier );

				subgraphInput.InputName = Name;
				subgraphInput.InputData = inputType switch
				{
					SubgraphPortType.Bool => new VariantValueBool() { InputType = SubgraphPortType.Bool },
					SubgraphPortType.Int => new VariantValueInt() { InputType = SubgraphPortType.Int },
					SubgraphPortType.Float => new VariantValueFloat() { InputType = SubgraphPortType.Float },
					SubgraphPortType.Vector2 => new VariantValueVector2() { InputType = SubgraphPortType.Vector2 },
					SubgraphPortType.Vector3 => new VariantValueVector3() { InputType = SubgraphPortType.Vector3 },
					SubgraphPortType.Vector4 => new VariantValueVector4() { InputType = SubgraphPortType.Vector4 },
					SubgraphPortType.Color => new VariantValueColor() { InputType = SubgraphPortType.Color },
					SubgraphPortType.Sampler => throw new NotImplementedException(),
					SubgraphPortType.Texture2DObject => throw new NotImplementedException(),
					SubgraphPortType.Invalid => throw new NotImplementedException(),
					_ => throw new NotImplementedException(),
				};
			}
			else
			{
				SubgraphPortType inputType = BlackboardParameter switch
				{
					BoolSubgraphInputBlackboardParameter => SubgraphPortType.Bool,
					IntSubgraphInputBlackboardParameter => SubgraphPortType.Int,
					FloatSubgraphInputBlackboardParameter => SubgraphPortType.Float,
					Float2SubgraphInputBlackboardParameter => SubgraphPortType.Vector2,
					Float3SubgraphInputBlackboardParameter => SubgraphPortType.Vector3,
					Float4SubgraphInputBlackboardParameter => SubgraphPortType.Vector4,
					ColorSubgraphInputBlackboardParameter => SubgraphPortType.Color,
					_ => throw new NotImplementedException(),
				};

				var blackboardParameterIdentifier = BlackboardParameter.Identifier;
				subgraphInput.BlackboardParameterIdentifier = BlackboardParameter.Identifier;
				subgraphInput.InputName = BlackboardParameter.Name;
				subgraphInput.InputDescription = ((ISubgraphBlackboardParameter)BlackboardParameter).Description;
				subgraphInput.InputData = inputType switch
				{
					SubgraphPortType.Bool => new VariantValueBool( (bool)BlackboardParameter.GetValue(), SubgraphPortType.Bool ),
					SubgraphPortType.Int => new VariantValueInt( (int)BlackboardParameter.GetValue(), SubgraphPortType.Int ),
					SubgraphPortType.Float => new VariantValueFloat( (float)BlackboardParameter.GetValue(), SubgraphPortType.Float ),
					SubgraphPortType.Vector2 => new VariantValueVector2( (Vector2)BlackboardParameter.GetValue(), SubgraphPortType.Vector2 ),
					SubgraphPortType.Vector3 => new VariantValueVector3( (Vector3)BlackboardParameter.GetValue(), SubgraphPortType.Vector3 ),
					SubgraphPortType.Vector4 => new VariantValueVector4( (Vector4)BlackboardParameter.GetValue(), SubgraphPortType.Vector4 ),
					SubgraphPortType.Color => new VariantValueColor( (Color)BlackboardParameter.GetValue(), SubgraphPortType.Color ),
					SubgraphPortType.Sampler => throw new NotImplementedException(),
					SubgraphPortType.Texture2DObject => throw new NotImplementedException(),
					SubgraphPortType.Invalid => throw new NotImplementedException(),
					_ => throw new NotImplementedException(),
				};

			}
		}
		return node;
	}
}
