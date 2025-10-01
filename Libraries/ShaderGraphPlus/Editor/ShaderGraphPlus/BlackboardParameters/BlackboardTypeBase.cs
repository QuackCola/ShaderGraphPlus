namespace ShaderGraphPlus;

public abstract class BaseBlackboardParameter
{
	[Sandbox.ReadOnly, Browsable( false )]
	public Guid Identifier { get; set; } = Guid.NewGuid();

	public virtual string Name { get; set; } = "";

	public BaseBlackboardParameter()
	{

	}
}

public abstract class BlackboardParameterGeneric<T> : BaseBlackboardParameter
{
	[InlineEditor( Label = false )]
	public T Value { get; set; }

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
	public ParameterUI UI { get; set; }

	public BlackboardMaterialParameter() : base() 
	{ 
	}

	public BlackboardMaterialParameter( T value ) : base( value )
	{
	}
}
