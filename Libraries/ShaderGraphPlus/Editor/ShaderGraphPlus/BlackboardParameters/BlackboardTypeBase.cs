namespace ShaderGraphPlus;

public abstract class BaseBlackboardParameter
{
	[Sandbox.ReadOnly, Browsable( false )]
	public Guid Identifier { get; set; } = Guid.NewGuid();

	public virtual string Name { get; set; } = "";

	[Hide, JsonIgnore, Browsable( false )]
	public ShaderGraphPlus Graph { get; set; }

	[Hide, JsonIgnore, Browsable( false )]
	public bool IsSubgraph => Graph.IsSubgraph;

	public BaseBlackboardParameter()
	{

	}

	public virtual object GetValue()
	{
		throw new NotImplementedException();
	}

	internal static BaseBlackboardParameter CreateTypeInstance( Type targetType, string name = "", bool isSubgraph = false )
	{
		return CreateTypeInstance( targetType, name, Guid.NewGuid(), isSubgraph );
	}

	internal static BaseBlackboardParameter CreateTypeInstance( Type targetType, string name = "", Guid guid = default, bool isSubgraph = false )
	{
		var typeInstance = EditorTypeLibrary.Create( targetType.Name, targetType );

		if ( guid == default )
			guid = Guid.NewGuid();

		if ( isSubgraph )
		{
			return typeInstance switch
			{
				BoolSubgraphInputBlackboardParameter => new BoolSubgraphInputBlackboardParameter( false )
				{ Name = name, Identifier = guid },
				IntSubgraphInputBlackboardParameter => new IntSubgraphInputBlackboardParameter( 1 )
				{ Name = name, Identifier = guid },
				FloatSubgraphInputBlackboardParameter => new FloatSubgraphInputBlackboardParameter( 1.0f )
				{ Name = name, Identifier = guid },
				Float2SubgraphInputBlackboardParameter => new Float2SubgraphInputBlackboardParameter( Vector2.One )
				{ Name = name, Identifier = guid },
				Float3SubgraphInputBlackboardParameter => new Float3SubgraphInputBlackboardParameter( Vector3.One )
				{ Name = name, Identifier = guid },
				Float4SubgraphInputBlackboardParameter => new Float4SubgraphInputBlackboardParameter( Vector4.One )
				{ Name = name, Identifier = guid },
				ColorSubgraphInputBlackboardParameter => new ColorSubgraphInputBlackboardParameter( Color.White )
				{ Name = name, Identifier = guid },
				_ => throw new NotImplementedException( $"unknown `{typeInstance}`" ),
			};
		}
		else
		{
			return typeInstance switch
			{
				BoolBlackboardParameter => new BoolBlackboardParameter( false )
				{ Name = name, Identifier = guid },
				IntBlackboardParameter => new IntBlackboardParameter( 1 )
				{ Name = name, Identifier = guid },
				FloatBlackboardParameter => new FloatBlackboardParameter( 1.0f )
				{ Name = name, Identifier = guid },
				Float2BlackboardParameter => new Float2BlackboardParameter( Vector2.One )
				{ Name = name, Identifier = guid },
				Float3BlackboardParameter => new Float3BlackboardParameter( Vector3.One )
				{ Name = name, Identifier = guid },
				Float4BlackboardParameter => new Float4BlackboardParameter( Vector4.One )
				{ Name = name, Identifier = guid },
				ColorBlackboardParameter => new ColorBlackboardParameter( Color.White )
				{ Name = name, Identifier = guid },
				ShaderFeatureBooleanBlackboardParameter => new ShaderFeatureBooleanBlackboardParameter( new()
				{ FeatureName = name } )
				{ Name = name, Identifier = guid },
				ShaderFeatureEnumBlackboardParameter => new ShaderFeatureEnumBlackboardParameter( new()
				{ FeatureName = name } )
				{ Name = name, Identifier = guid },
				_ => throw new NotImplementedException( $"{typeInstance}" ),
			};
		}
	}
}

public abstract class BlackboardParameterGeneric<T> : BaseBlackboardParameter
{
	[InlineEditor( Label = false )]
	public T Value { get; set; }

	public override object GetValue()
	{
		return Value;
	}

	public BlackboardParameterGeneric() : base() 
	{ 
	}

	public BlackboardParameterGeneric( T value ) : base() 
	{ 
		Value = value;
	}
}

public abstract class BlackboardMaterialParameter<T> : BlackboardParameterGeneric<T>
{
	[InlineEditor( Label = false ), Group( "UI" )]
	[HideIf( nameof( IsSubgraph ), true )]
	public ParameterUI UI { get; set; }

	public BlackboardMaterialParameter() : base() 
	{ 
	}

	public BlackboardMaterialParameter( T value ) : base( value )
	{
	}
}

public abstract class BlackboardSubgraphInputParameter<T> : BlackboardParameterGeneric<T>
{
	[InlineEditor( Label = false ), Group( "UI" )]
	[HideIf( nameof( IsSubgraph ), true )]
	public ParameterUI UI { get; set; }

	[Title( "Input Name" )]
	public override string Name { get; set; }

	[TextArea]
	public string InputDescription { get; set; }

	public override object GetValue()
	{
		return Value;
	}

	/// <summary>
	/// Is this input required to have a valid connection?
	/// </summary>
	public bool IsRequired { get; set; } = false;

	public int PortOrder { get; set; }

	public BlackboardSubgraphInputParameter() : base()
	{
	}

	public BlackboardSubgraphInputParameter( T value ) : this()
	{
		Value = value;
	}
}
