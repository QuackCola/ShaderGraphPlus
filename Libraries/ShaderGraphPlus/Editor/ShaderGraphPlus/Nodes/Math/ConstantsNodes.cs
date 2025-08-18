namespace ShaderGraphPlus.Nodes;

/*
public abstract class ConstantsNode : ShaderNodePlus
{
    protected virtual string Constant { get; }

    [Output( typeof( float ) )]
    [Hide] 
    public NodeResult.Func Result => ( GraphCompiler compiler ) => new( ResultType.Float, $"{Constant}" );
}

[Title( "Pi" ), Category( "Constants" )]
public class PI : ConstantsNode
{
  protected override string Constant => "3.1415926";
}

[Title( "Tau" ), Category( "Constants" )]
public class TAU : ConstantsNode
{
  protected override string Constant => "6.28318530";
}

[Title( "Phi" ), Category( "Constants" )]
public class PHI : ConstantsNode
{
  protected override string Constant => "1.618034";
}

[Title( "E" ), Category( "Constants" )]
public class E : ConstantsNode
{
  protected override string Constant => "2.718282";
} 

[Title( "SQRT2" ), Category( "Constants" )]
public class SQRT2 : ConstantsNode
{
  protected override string Constant => "1.414214";
}
*/

/// <summary>
/// A container for common math constants
/// </summary>
[Title( "Math Constants" ), Category( "Constants" )]
public class MathConstantsNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Hide]
	public override string Title => $"{DisplayInfo.For( this ).Name} ({Constant})";

	public enum ConstantValues
    {
        PI,
		TWOPI,
		FOURPI,
		TAU,
        PHI,
        E,
		LOG2E,
		LOG10E,
		LN2,
		LN10,
		SQRT2,
		SQRT1_2
	}

    public ConstantValues Constant { get; set; }

    [Hide]
    private string ConstantResult
	{
		get
		{
           return Constant switch
		   {					     
				ConstantValues.PI      => "3.14159265359",
				ConstantValues.TWOPI   => "6.28318530718",
			    ConstantValues.FOURPI  => "0.78539816339",
			    ConstantValues.TAU     => "6.28318530717",
                ConstantValues.PHI     => "1.6180339887",
                ConstantValues.E       => "2.718282",
			    ConstantValues.LOG2E   => "1.44269504088",
			    ConstantValues.LOG10E  => "0.43429448190",
			    ConstantValues.LN2     => "0.69314718055",
			    ConstantValues.LN10    => "2.30258509299",
			    ConstantValues.SQRT2   => "1.41421356237",
			    ConstantValues.SQRT1_2 => "0.70710678118",
			   _ => throw new System.NotImplementedException(),
            };
        }
    }

    [Output( typeof( float ) )]
    [Hide]
    public NodeResult.Func Result => ( GraphCompiler compiler ) =>
    {
        return new NodeResult( ResultType.Float, ConstantResult );
    };
}
