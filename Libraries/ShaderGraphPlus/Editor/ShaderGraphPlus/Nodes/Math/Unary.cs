namespace Editor.ShaderGraphPlus.Nodes;

public enum ExponentBase
{
	BaseE,
	Base2,
}

public enum LogBase
{
	BaseE,
	Base2,
	Base10,
}

public enum DerivativePrecision
{
	Standard,
	Course,
	Fine
}

public abstract class Unary : ShaderNodePlus
{
	[Input]
	[Hide]
	public NodeInput Input { get; set; }

	protected virtual string Op { get; }

    [Hide]
    protected virtual int? Components { get; set; } = null;

	public Unary() : base()
	{
		ExpandSize = new Vector3( -50, 0 );
	}

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{

		var result = compiler.ResultOrDefault( Input, 0.0f );

		Components = result.Components();

		if ( (ResultType)Components == ResultType.Float )
		{
			//Log.Info( "Output Is : Is Float" );
			return result.IsValid ? new NodeResult( ResultType.Float, $"{Op}( {result} )" ) : default;
		}
		else if ( (ResultType)Components == ResultType.Vector2 )
		{
			//Log.Info( "Output Is : Is Vector2" );
			return result.IsValid ? new NodeResult( ResultType.Vector2, $"{Op}( {result} )" ) : default;
		}
		else if ( (ResultType)Components == ResultType.Vector3 )
		{
			//Log.Info( "Output Is : Vector3" );
			return result.IsValid ? new NodeResult( ResultType.Vector3, $"{Op}( {result} )" ) : default;
		}
		else if ( (ResultType)Components == ResultType.Float2x2 )
		{

			return result.IsValid ? new NodeResult( ResultType.Float2x2, $"{Op}( {result} )" ) : default;
		}
		else if ( (ResultType)Components == ResultType.Float3x3 )
		{

			return result.IsValid ? new NodeResult( ResultType.Float3x3, $"{Op}( {result} )" ) : default;
		}
		else if ( (ResultType)Components == ResultType.Float4x4 )
		{

			return result.IsValid ? new NodeResult( ResultType.Float4x4, $"{Op}( {result} )" ) : default;
		}
		else
		{
			//Log.Info( "Output Is : Is Color" );
			return result.IsValid ? new NodeResult( ResultType.Color, $"{Op}( {result} )" ) : default;
		}

	};
}

[Title( "Transpose" ), Category( "Unary" )]
public sealed class Transpose : Unary
{
    [Hide]
    protected override string Op => "transpose";
}

[Title("Clamp"), Category("Unary")]
public sealed class Clamp : ShaderNodePlus
{
    [Input]
    [Hide]
    [Title("Value")]
    public NodeInput InputA { get; set; }

    [Input]
    [Hide]
	[Title("Min")]
    public NodeInput InputB { get; set; }

    [Input]
    [Hide]
    [Title("Max")]
    public NodeInput InputC { get; set; }


	public float DefaultMin { get; set; } = 0.0f;
	public float DefaultMax { get; set; } = 1.0f;

    [Output]
    [Hide]
    public NodeResult.Func Result => (GraphCompiler compiler) =>
    {
        var resultA = compiler.ResultOrDefault(InputA, 0.0f);
        var resultB = compiler.ResultOrDefault(InputB, DefaultMin);
        var resultC = compiler.ResultOrDefault(InputC, DefaultMax).Cast(resultB.Components());

        return new NodeResult(ResultType.Float, $"clamp( {resultA}, {resultB}, {resultC} )");
    };
}

[Title( "Cosine" ), Category( "Unary" )]
public sealed class Cosine : Unary
{
    [Hide]
    protected override string Op => "cos";
}

[Title( "Abs" ), Category( "Unary" )]
public sealed class Abs : Unary
{
    [Hide]
    protected override string Op => "abs";
}

[Title("Rsqrt"), Category("Unary")]
public sealed class Rsqrt : Unary
{
    [Hide]
    protected override string Op => "rsqrt";
}

[Title( "Sqrt" ), Category( "Unary" )]
public sealed class Sqrt : Unary
{
    [Hide]
    protected override string Op => "sqrt";
}

[Title( "Dot Product" ), Category( "Unary" )]
public sealed class DotProduct : ShaderNodePlus
{
	[Input]
	[Hide]
	public NodeInput InputA { get; set; }

	[Input]
	[Hide]
	public NodeInput InputB { get; set; }

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var resultA = compiler.ResultOrDefault( InputA, 0.0f );
		var resultB = compiler.ResultOrDefault( InputB, 0.0f ).Cast( resultA.Components() );

		return new NodeResult( ResultType.Float, $"dot( {resultA}, {resultB} )" );
	};
}

[Title( "DDX" ), Category( "Unary" )]
public sealed class DDX : Unary
{
	public DerivativePrecision Precision { get; set; }

    [Hide]
    protected override string Op
	{
		get
		{
			return Precision switch
			{
				DerivativePrecision.Course => "ddx_course",
				DerivativePrecision.Fine => "ddx_fine",
				_ => "ddx",
			};
		}
	}
}

[Title( "DDY" ), Category( "Unary" )]
public sealed class DDY : Unary
{
	public DerivativePrecision Precision { get; set; }

    [Hide]
    protected override string Op
	{
		get
		{
			return Precision switch
			{
				DerivativePrecision.Course => "ddy_course",
				DerivativePrecision.Fine => "ddy_fine",
				_ => "ddy",
			};
		}
	}
}

[Title( "DDXY" ), Category( "Unary" )]
public sealed class DDXY : Unary
{
    [Hide]
    protected override string Op => "fwidth";
}

[Title( "Exponential" ), Category( "Unary" )]
public sealed class Exponential : Unary
{
	public ExponentBase Base { get; set; }

    [Hide]
    protected override string Op => Base == ExponentBase.BaseE ? "exp" : "exp2";
}

[Title( "Frac" ), Category( "Unary" )]
public sealed class Frac : Unary
{
    [Hide]
    protected override string Op => "frac";
}

[Title( "Floor" ), Category( "Unary" )]
public sealed class Floor : Unary
{
    [Hide]
    protected override string Op => "floor";
}

[Title( "Length" ), Category( "Unary" )]
public sealed class Length : Unary
{
    [Hide]
    protected override int? Components => 1;

    [Hide]
    protected override string Op => "length";
}

[Title( "Log" ), Category( "Unary" )]
public sealed class BaseLog : Unary
{
	public LogBase Base { get; set; }

	protected override string Op
	{
		get
		{
			return Base switch
			{
				LogBase.Base2 => "log2",
				LogBase.Base10 => "log10",
				_ => "log",
			};
		}
	}
}

[Title( "Min" ), Category( "Unary" )]
public sealed class Min : ShaderNodePlus
{
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput InputA { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput InputB { get; set; }

	public float DefaultA { get; set; } = 0.0f;
	public float DefaultB { get; set; } = 0.0f;

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var a = compiler.ResultOrDefault( InputA, DefaultA );
		var b = compiler.ResultOrDefault( InputB, DefaultB );

		int maxComponents = Math.Max( a.IsValid ? a.Components() : 1, b.IsValid ? b.Components() : 1 );

		return new NodeResult( (ResultType)maxComponents, $"min( {(a.IsValid ? a : "0.0f")}, {(b.IsValid ? b : "0.0f")} )" );
	};
}

[Title( "Max" ), Category( "Unary" )]
public sealed class Max : ShaderNodePlus
{
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput InputA { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput InputB { get; set; }

	public float DefaultA { get; set; } = 0.0f;
	public float DefaultB { get; set; } = 0.0f;

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var a = compiler.ResultOrDefault( InputA, DefaultA );
		var b = compiler.ResultOrDefault( InputB, DefaultB );

		int maxComponents = Math.Max( a.IsValid ? a.Components() : 1, b.IsValid ? b.Components() : 1 );

		return new NodeResult( (ResultType)maxComponents, $"max( {(a.IsValid ? a : "0.0f")}, {(b.IsValid ? b : "0.0f")} )" );
	};
}

/// <summary>
/// Clamps the specified value within the range of 0 to 1
/// </summary>
[Title( "Saturate" ), Category( "Transform" )]
public sealed class Saturate : Unary
{
    [Hide]
    protected override string Op => "saturate";
}

[Title( "Sine" ), Category( "Unary" )]
public sealed class Sine : Unary
{
    [Hide]
    protected override string Op => "sin";
}

[Title( "Step" ), Category( "Unary" )]
public sealed class Step : ShaderNodePlus
{
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Input { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Edge { get; set; }

	public float DefaultInput { get; set; } = 0.0f;
	public float DefaultEdge { get; set; } = 0.0f;

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var edge = compiler.ResultOrDefault( Edge, DefaultEdge );
		var input = compiler.ResultOrDefault( Input, DefaultInput );

		int maxComponents = Math.Max( edge.IsValid ? edge.Components() : 1, input.IsValid ? input.Components() : 1 );

		return new NodeResult( (ResultType)maxComponents, $"step( {(edge.IsValid ? edge : "0.0f")}, {(input.IsValid ? input : "0.0f")} )" );
	};
}

[Title( "Smooth Step" ), Category( "Unary" )]
public sealed class SmoothStep : ShaderNodePlus
{
	[Input]
	[Hide]
	public NodeInput Input { get; set; }

	[Input]
	[Hide]
	public NodeInput Edge1 { get; set; }

	[Input]
	[Hide]
	public NodeInput Edge2 { get; set; }

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var edge1 = compiler.Result( Edge1 );
		var edge2 = compiler.Result( Edge2 );
		var input = compiler.Result( Input );

		int maxComponents = Math.Max( edge1.IsValid ? edge1.Components() : 1, input.IsValid ? input.Components() : 1 );
		maxComponents = Math.Max( edge2.IsValid ? edge2.Components() : 1, maxComponents );

		return new NodeResult( (ResultType)maxComponents, $"smoothstep( {(edge1.IsValid ? edge1 : "0.0f")}, {(edge2.IsValid ? edge2 : "0.0f")}, {(input.IsValid ? input : "0.0f")} )" );
	};
}

/// <summary>
/// Computes the tangent of a specified angle (in radians).
/// </summary>
[Title( "Tangent" ), Category( "Unary" )]
public sealed class Tan : Unary
{
    [Hide]
    protected override string Op => "tan";
}

/// <summary>
/// Computes the angle (in radians) whose sine is the specified number.
/// </summary>
[Title( "Arcsin" ), Category( "Unary" )]
public sealed class Arcsin : Unary
{
    [Hide]
    protected override string Op => "asin";
}

/// <summary>
/// Computes the angle (in radians) whose cosine is the specified number.
/// </summary>
[Title( "Arccos" ), Category( "Unary" )]
public sealed class Arccos : Unary
{
    [Hide]
    protected override string Op => "acos";
}

/// <summary>
/// Round to the nearest integer.
/// </summary>
[Title("Round"), Category("Unary")]
public sealed class Round : Unary
{
	[Hide]
	protected override string Op => "round";
}

/// <summary>
/// Returns the smallest integer value that is greater than or equal to the specified value.
/// </summary>
[Title( "Ceil" ), Category( "Unary" )]
public sealed class Ceil : Unary
{
    [Hide]
    protected override string Op => "ceil";
}

[Title( "One Minus" ), Category( "Unary" )]
public sealed class OneMinus : ShaderNodePlus
{
	[Input( typeof( float ) ), Hide, Title( "" )]
	public NodeInput In { get; set; }

	[Output, Hide, Title( "" )]
	public NodeResult.Func Out => ( GraphCompiler compiler ) =>
	{
		var result = compiler.ResultOrDefault( In, 0.0f );
		return new NodeResult( result.ResultType, $"1 - {result}" );
	};

	public OneMinus() : base()
	{
		ExpandSize = new Vector3( -85, 0 );
	}
}

/// <summary>
/// Returns a distance scalar between two vectors.
/// </summary>
[Title( "Distance" ), Category( "Unary" )]
public sealed class Distance : ShaderNodePlus
{
	[Input]
	[Hide]
	public NodeInput A { get; set; }

	[Input]
	[Hide]
	public NodeInput B { get; set; }

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var resultA = compiler.ResultOrDefault( A, 0.0f );
		var resultB = compiler.ResultOrDefault( B, 0.0f ).Cast( resultA.Components() );

		return new NodeResult( ResultType.Float, $"distance( {resultA}, {resultB} )" );
	};
}
