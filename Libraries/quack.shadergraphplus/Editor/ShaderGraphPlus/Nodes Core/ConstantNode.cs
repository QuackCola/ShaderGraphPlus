namespace ShaderGraphPlus;

public interface IConstantNode
{
	public object GetValue();
	public object GetStepValue();
	public object GetMinValue();
	public object GetMaxValue();

	public string Identifier { get; set; }

	public bool UseStep { get; }
	public bool UseMinMax { get; }
}

public abstract class ConstantNode<T> : ShaderNodePlus, IConstantNode
{
	public T Value { get; set; }

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.ConstantNode;

	protected NodeResult Component( string component, float value, GraphCompiler compiler )
	{
		if ( compiler.IsPreview )
			return compiler.ResultValue( value );

		var result = compiler.Result( new NodeInput { Identifier = Identifier, Output = nameof( Result ) } );
		return new( ResultType.Float, $"{result}.{component}", true );
	}

	[Hide, JsonIgnore]
	public virtual bool UseStep => true;

	[Hide, JsonIgnore]
	public virtual bool UseMinMax => true;

	public object GetValue() 
	{ 
		return Value; 
	}

	public virtual object GetStepValue()
	{
		return 0.0f;
	}

	public virtual object GetMinValue()
	{
		return 0.0f;
	}

	public virtual object GetMaxValue()
	{
		return 1.0f;
	}
}

