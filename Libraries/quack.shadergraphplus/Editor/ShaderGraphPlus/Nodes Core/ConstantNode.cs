namespace ShaderGraphPlus;

public interface IConstantNode
{
	public string Identifier { get; set; }
	public object GetValue();
}

public interface IRangedConstant
{
	public object GetStepValue();
	public object GetMinValue();
	public object GetMaxValue();
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

	public object GetValue() 
	{ 
		return Value; 
	}
}

