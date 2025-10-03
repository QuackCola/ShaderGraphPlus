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

	internal static BaseBlackboardParameter CreateTypeInstance( Type targetType, string name = "" )
	{
		var typeInstance = EditorTypeLibrary.Create( targetType.Name, targetType );

		return typeInstance switch
		{
			BoolBlackboardParameter => new BoolBlackboardParameter( false ) 
			{ Name = name, Identifier = Guid.NewGuid() },
			IntBlackboardParameter => new IntBlackboardParameter( 0 ) 
			{ Name = name, Identifier = Guid.NewGuid() },
			FloatBlackboardParameter => new FloatBlackboardParameter( 0.0f ) 
			{ Name = name, Identifier = Guid.NewGuid() },
			Float2BlackboardParameter => new Float2BlackboardParameter( Vector2.Zero ) 
			{ Name = name, Identifier = Guid.NewGuid() },
			Float3BlackboardParameter => new Float3BlackboardParameter( Vector3.Zero ) 
			{ Name = name, Identifier = Guid.NewGuid() },
			Float4BlackboardParameter => new Float4BlackboardParameter( Vector4.Zero ) 
			{ Name = name, Identifier = Guid.NewGuid() },
			ColorBlackboardParameter => new ColorBlackboardParameter( Color.White )
			{ Name = name, Identifier = Guid.NewGuid() },
			ShaderFeatureBooleanBlackboardParameter => new ShaderFeatureBooleanBlackboardParameter( new() 
			{ FeatureName = name } ) { Name = name, Identifier = Guid.NewGuid() },
			ShaderFeatureEnumBlackboardParameter => new ShaderFeatureEnumBlackboardParameter( new() 
			{ FeatureName = name } ) { Name = name, Identifier = Guid.NewGuid() },
			_ => throw new NotImplementedException(),
		};
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
