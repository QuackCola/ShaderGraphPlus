﻿using Editor;
using NodeEditorPlus;
using GraphView = NodeEditorPlus.GraphView;
using NodeUI = NodeEditorPlus.NodeUI;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;

namespace ShaderGraphPlus.Nodes;

public abstract class Binary : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Hide, JsonIgnore]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.BinaryNode;

	[Input( typeof( float ) )]
	[Hide]
	[Title( "" )]
	public NodeInput A { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	[Title( "" )]
	public NodeInput B { get; set; }

	[InputDefault( nameof( A ) )]
	public float DefaultA { get; set; } = 0.0f;

	[InputDefault( nameof( B ) )]
	public float DefaultB { get; set; } = 1.0f;

	protected virtual string Op { get; }

	public Binary() : base()
	{
		ExpandSize = new Vector3( -85, 5 );
	}

	public override void OnPaint( Rect rect )
	{
		rect = rect.Shrink( 0, 20, 0, 0 );
		Paint.SetPen( Theme.TextControl );
		Paint.SetFont( "Poppins Bold", 20 );
		Paint.DrawText( rect, Op );
	}

	[Output]
	[Hide]
	[Title( "" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var results = compiler.Result( A, B, DefaultA, DefaultB );
		return new NodeResult( results.Item1.ResultType, $"{results.Item1} {Op} {results.Item2}" );
	};

	[JsonIgnore, Hide, Browsable( false )]
	public override DisplayInfo DisplayInfo
	{
		get
		{
			var info = base.DisplayInfo;
			info.Icon = null;
			return info;
		}
	}
}

/// <summary>
/// Add two values together
/// </summary>
[Title( "Add" ), Category( "Math/Binary" ), Icon( "+" )]
public sealed class Add : Binary
{
	[Hide]
	protected override string Op => "+";
}

/// <summary>
/// Subtract two values together
/// </summary>
[Title( "Subtract" ), Category( "Math/Binary" ), Icon( "-" )]
public sealed class Subtract : Binary
{
	[Hide]
	protected override string Op => "-";
}

/// <summary>
/// Multiply two values together
/// </summary>
[Title( "Multiply" ), Category( "Math/Binary" ), Icon( "*" )]
public sealed class Multiply : Binary
{
	[Hide]
	protected override string Op => "*";

	public override void OnPaint( Rect rect )
	{
		rect = rect.Shrink( 0, 20, 0, 0 );
		Paint.SetPen( Theme.TextControl );
		Paint.SetFont( "Poppins Bold", 20 );
		Paint.DrawText( rect, "x" );
	}
}

/// <summary>
/// Divide two values together
/// </summary>
[Title( "Divide" ), Category( "Math/Binary" ), Icon( "/" )]
public sealed class Divide : Binary
{
	[Hide]
	protected override string Op => "/";
}

/// <summary>
/// Computes the remainder of the division of two values
/// </summary>
[Title( "Mod" ), Category( "Math/Binary" ), Icon( "percent" )]
public sealed class Mod : Binary
{
	[Hide]
	protected override string Op => "%";
}

/// <summary>
/// Linear interpolation between two values
/// </summary>
[Title( "Lerp" ), Category( "Math/Binary" )]
public sealed class Lerp : ShaderNodePlus
{

	[Hide]
	public override int Version => 1;

	[Hide, JsonIgnore]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.BinaryNode;

	[Input]
	[Hide]
	public NodeInput A { get; set; }

	[Input]
	[Hide]
	public NodeInput B { get; set; }

	[Input( typeof( float ) ), Title( "Fraction" )]
	[Hide, NodeValueEditor( nameof( Fraction ) )]
	public NodeInput C { get; set; }

	[InputDefault( nameof( A ) )]
	public float DefaultA { get; set; } = 0.0f;

	[InputDefault( nameof( B ) )]
	public float DefaultB { get; set; } = 1.0f;

	[MinMax( 0, 1 )]
	[InputDefault( nameof( C ) )]
	public float Fraction { get; set; } = 0.5f;

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var results = compiler.Result( A, B );
		var fraction = compiler.Result( C );
		var fractionType = fraction.IsValid && fraction.Components > 1 ? Math.Max( results.Item1.Components, results.Item2.Components ) : 1;
		return new NodeResult( results.Item1.ResultType, $"lerp( {results.Item1}, {results.Item2}," +
			$" {(fraction.IsValid ? fraction.Cast( fractionType ) : compiler.ResultValue( Fraction ))} )" );
	};
}

/// <summary>
/// Returns the cross product of two float3's
/// </summary>
[Title( "Cross Product" ), Category( "Math/Binary" )]
public sealed class CrossProduct : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.BinaryNode;

	/// <summary>
	/// The first float3
	/// </summary>
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput A { get; set; }

	/// <summary>
	/// The second float3
	/// </summary>
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput B { get; set; }

	/// <summary>
	/// Default value for when A input is missing
	/// </summary>
	[InputDefault (nameof( A ) )]
	public Vector3 DefaultA { get; set; }

	/// <summary>
	/// Default value for when B input is missing
	/// </summary>
	[InputDefault( nameof( B ) )]
	public Vector3 DefaultB { get; set; }

	/// <summary>
	/// The result of the cross product
	/// </summary>
	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var a = compiler.ResultOrDefault( A, DefaultA ).Cast( 3 );
		var b = compiler.ResultOrDefault( B, DefaultB ).Cast( 3 );
		return new NodeResult( ResultType.Vector3, $"cross( {a}, {b} )" );
	};
}

/// <summary>
/// Transform a value from range "In Min->In Max" to "Out Min->Out Max". When clamped values Less-than "In" map to "In Max", and Greater-than "In Min" maps to "Out Min".
/// </summary>
[Title( "Remap Value" ), Category( "Math/Binary" )]
public sealed class RemapValue : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Hide, JsonIgnore]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.BinaryNode;

	/// <summary>
	/// Input value to be transformed
	/// </summary>
	[Input( typeof( float ) ), Title( "In" )]
	[Hide]
	public NodeInput A { get; set; }

	/// <summary>
	/// The minimum range of the input
	/// </summary>
	[Input( typeof( float ) ), Title( "In Min" )]
	[Hide, NodeValueEditor( nameof( InMin ) )]
	public NodeInput B { get; set; }

	/// <summary>
	/// The maximum range of the input
	/// </summary>
	[Input( typeof( float ) ), Title( "In Max" )]
	[Hide, NodeValueEditor( nameof( InMax ) )]
	public NodeInput C { get; set; }

	/// <summary>
	/// The output minimum range the input should map to
	/// </summary>
	[Input( typeof( float ) ), Title( "Out Min" )]
	[Hide, NodeValueEditor( nameof( OutMin ) )]
	public NodeInput D { get; set; }

	/// <summary>
	/// The output maximum range the input should map to
	/// </summary>
	[Input( typeof( float ) ), Title( "Out Max" )]
	[Hide, NodeValueEditor( nameof( OutMax ) )]
	public NodeInput E { get; set; }

	/// <summary>
	/// Input value to be transformed
	/// </summary>
	[InputDefault( nameof( A ) )]
	public float In { get; set; } = 0.5f;

	/// <summary>
	/// The minimum range of the input
	/// </summary>
	public float InMin { get; set; } = 0.0f;

	/// <summary>
	/// The maximum range of the input
	/// </summary>
	public float InMax { get; set; } = 1.0f;

	/// <summary>
	/// The output minimum range the input should map to
	/// </summary>
	public float OutMin { get; set; } = 0.0f;

	/// <summary>
	/// The output maximum range the input should map to
	/// </summary>
	public float OutMax { get; set; } = 1.0f;

	/// <summary>
	/// Clamp the input value to the output range
	/// </summary>
	public bool Clamp { get; set; } = true;

	struct ResultHolder
	{
		public NodeInput Input;
		private NodeResult Result;
		private float Attribute;

		public int Components => Result.IsValid ? Result.Components : 1;

		public ResultHolder( GraphCompiler compiler, NodeInput input, float attribute )
		{
			Input = input;
			Result = compiler.Result( input );
			Attribute = attribute;
		}

		public string GetResult( GraphCompiler compiler, int componentCount = 1 )
		{
			if ( Result.IsValid ) return Result.Cast( componentCount );
			return compiler.ResultValue( Attribute ).Cast( componentCount );
		}
	}

	/// <summary>
	/// The remapped value
	/// </summary>
	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var results = new ResultHolder[]
		{
			new ResultHolder(compiler, A, In),
			new ResultHolder(compiler, B, InMin),
			new ResultHolder(compiler, C, InMax),
			new ResultHolder(compiler, D, OutMin),
			new ResultHolder(compiler, E, OutMax),
		};

		var maxComponents = results.Max( x => x.Components );

		var inValue = results[0].GetResult( compiler, maxComponents );
		var inMinValue = results[1].GetResult( compiler, maxComponents );
		var inMaxValue = results[2].GetResult( compiler, maxComponents );
		var outMinValue = results[3].GetResult( compiler, maxComponents );
		var outMaxValue = results[4].GetResult( compiler, maxComponents );

		// Normalize the input to a range of [0, 1]
		var normalizedInValue = $"( {inValue} - {inMinValue} ) / ( {inMaxValue} - {inMinValue} )";

		// Apply saturation function if clamp is enabled
		if ( Clamp )
		{
			normalizedInValue = $"saturate( {normalizedInValue} )";
		}

		// Remap the normalized value to the output range
		var remappedOutput = $"({normalizedInValue} * ( {outMaxValue} - {outMinValue} )) + {outMinValue}";

		ResultType resultType = maxComponents switch
		{
			1 => ResultType.Float,
			2 => ResultType.Vector2,
			3 => ResultType.Vector3,
			4 => ResultType.Color,
			_ => throw new NotImplementedException(),
		};

		return new NodeResult( resultType, remappedOutput );
	};
}

/// <summary>
/// Computes the angle (in radians) whose tangent is the quotient of two specified numbers.
/// </summary>
[Title( "Arctan2" ), Category( "Math/Binary" )]
public sealed class Arctan2 : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Hide, JsonIgnore]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.BinaryNode;

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Y { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput X { get; set; }

	[InputDefault( nameof( Y ) )]
	public float DefaultY { get; set; } = 0.0f;

	[InputDefault( nameof( X ) )]
	public float DefaultX { get; set; } = 1.0f;

	public Arctan2() : base()
	{
		ExpandSize = new Vector3( -50, 0 );
	}

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var results = compiler.Result( Y, X, DefaultY, DefaultX );
		return new NodeResult( results.Item1.ResultType, $"atan2( {results.Item1}, {results.Item2} )" );
	};
}

/// <summary>
/// Raise a value to the power of another value
/// </summary>
[Title( "Power" ), Category( "Math/Binary" ), Icon( "^" )]
public sealed class Power : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Hide, JsonIgnore]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.BinaryNode;

	[Input( typeof( float ) )]
	[Hide]
	[Title( "" )]
	public NodeInput A { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	[Title( "" )]
	public NodeInput B { get; set; }

	[InputDefault( nameof( A ) )]
	public float DefaultA { get; set; } = 1.0f;

	[InputDefault( nameof( B ) )]
	public float DefaultB { get; set; } = 1.0f;

	public Power() : base()
	{
		ExpandSize = new Vector3( -85, 5 );
	}

	[Output]
	[Hide]
	[Title( "" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var results = compiler.Result( A, B, DefaultA, DefaultB );
		return new NodeResult( results.Item1.ResultType, $"pow( {results.Item1}, {results.Item2} )" );
	};

	public override void OnPaint( Rect rect )
	{
		rect = rect.Shrink( 0, 25, 0, 0 );
		Paint.SetPen( Theme.TextControl );
		Paint.DrawIcon( rect, "^", 50 );
	}
}
