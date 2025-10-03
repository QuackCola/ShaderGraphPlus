using NodeEditorPlus;
using GraphView = NodeEditorPlus.GraphView;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;
using NodeUI = NodeEditorPlus.NodeUI;

namespace ShaderGraphPlus.Nodes;


/// <summary>
/// Bool value
/// </summary>
[Title( "Bool Constant" ), Category( "Constants" ), Icon( "check_box" ), Order( 0 )]
public sealed class BoolConstantNode : ConstantNode<bool>
{
	[Hide]
	public override int Version => 1;

	public BoolConstantNode() : base()
	{

	}

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Hide, JsonIgnore] 
	public override bool UseMinMax => false;

	[Hide, JsonIgnore]
	public override bool UseStep => false;

	[Output( typeof( bool ) ), Title( "Value" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( "", Value, default, default, false, false, default );
	};
}

///<summary>
/// Single int value
///</summary>
[Title( "Int Constant" ), Category( "Constants" ), Icon( "looks_one" ), Order( 1 )]
public sealed class IntConstantNode : ConstantNode<int>
{
	[Hide]
	public override int Version => 1;

	[Hide, JsonIgnore]
	public override bool UseMinMax => true;

	[Hide, JsonIgnore]
	public override bool UseStep => false;

	[Group( "Range" )] public int Min { get; set; }
	[Group( "Range" )] public int Max { get; set; }

	public IntConstantNode() : base()
	{
		Min = 0;
		Max = 1;
	}

	[Output( typeof( int ) ), Title( "Value" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( "", Value, default, default, false, false, default );
	};

	public override object GetMinValue()
	{
			return Min;
	}

	public override object GetMaxValue()
	{
		return Max;
	}
}

/// <summary>
/// Single float value
/// </summary>
[Title( "Float Constant" ), Category( "Constants" ), Icon( "looks_one" ), Order( 2 )]
public sealed class FloatConstantNode : ConstantNode<float>
{
	[Hide]
	public override int Version => 1;

	[Hide, JsonIgnore]
	public override bool UseMinMax => true;

	[Hide, JsonIgnore]
	public override bool UseStep => true;

	public FloatConstantNode() : base()
	{
		Min = 0;
		Max = 1;
	}

	[Group( "Range" )] public float Min { get; set; }
	[Group( "Range" )] public float Max { get; set; }
	public float Step { get; set; } = 0.0f;

	[Output( typeof( float ) ), Title( "Value" )]
	[Hide, Editor( nameof( Value ) ), Range( nameof( Min ), nameof( Max ), nameof( Step ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( "", Value, default, default, false, false, default );
	};

	public override object GetMinValue()
	{
		return Min;
	}

	public override object GetMaxValue()
	{
		return Max;
	}

	public override object GetStepValue()
	{
		return Step;
	}
}

/// <summary>
/// 2 float values
/// </summary>
[Title( "Float2 Constant" ), Category( "Constants" ), Icon( "looks_two" ), Order( 3 )]
public sealed class Float2ConstantNode : ConstantNode<Vector2>
{
	[Hide]
	public override int Version => 1;

	[Hide, JsonIgnore]
	public override bool UseMinMax => true;

	[Hide, JsonIgnore]
	public override bool UseStep => true;

	[JsonIgnore, Hide]
	public float ValueX
	{
		get => Value.x;
		set => Value = Value.WithX( value );
	}

	[JsonIgnore, Hide]
	public float ValueY
	{
		get => Value.y;
		set => Value = Value.WithY( value );
	}

	[Group( "Range" )] public Vector2 Min { get; set; }
	[Group( "Range" )] public Vector2 Max { get; set; }
	public float Step { get; set; } = 0.0f;

	[Hide] public float MinX => Min.x;
	[Hide] public float MinY => Min.y;
	[Hide] public float MaxX => Max.x;
	[Hide] public float MaxY => Max.y;

	public Float2ConstantNode() : base()
	{
		Min = 0;
		Max = 1;
	}

	[Output( typeof( Vector2 ) ), Title( "XY" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( "", Value, default, default, false, false, default );
	};

	/// <summary>
	/// X component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueX ) ), Title( "X" )]
	[Range( nameof( MinX ), nameof( MaxX ), nameof( Step ) )]
	public NodeResult.Func X => ( GraphCompiler compiler ) => Component( "x", ValueX, compiler );

	/// <summary>
	/// Y component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueY ) ), Title( "Y" )]
	[Range( nameof( MinY ), nameof( MaxY ), nameof( Step ) )]
	public NodeResult.Func Y => ( GraphCompiler compiler ) => Component( "y", ValueY, compiler );

	public override object GetMinValue()
	{
		return Min;
	}

	public override object GetMaxValue()
	{
		return Max;
	}

	public override object GetStepValue()
	{
		return Step;
	}
}


/// <summary>
/// 3 float values
/// </summary>
[Title( "Float3 Constant" ), Category( "Constants" ), Icon( "looks_3" ), Order( 4 )]
public sealed class Float3ConstantNode : ConstantNode<Vector3>
{
	[Hide]
	public override int Version => 1;

	[Hide, JsonIgnore]
	public override bool UseMinMax => true;

	[Hide, JsonIgnore]
	public override bool UseStep => true;

	[JsonIgnore, Hide]
	public float ValueX
	{
		get => Value.x;
		set => Value = Value.WithX( value );
	}

	[JsonIgnore, Hide]
	public float ValueY
	{
		get => Value.y;
		set => Value = Value.WithY( value );
	}

	[JsonIgnore, Hide]
	public float ValueZ
	{
		get => Value.z;
		set => Value = Value.WithZ( value );
	}

	[Group( "Range" )] public Vector3 Min { get; set; }
	[Group( "Range" )] public Vector3 Max { get; set; }
	public float Step { get; set; } = 0.0f;

	[Hide] public float MinX => Min.x;
	[Hide] public float MinY => Min.y;
	[Hide] public float MinZ => Min.z;
	[Hide] public float MaxX => Max.x;
	[Hide] public float MaxY => Max.y;
	[Hide] public float MaxZ => Max.z;

	public Float3ConstantNode() : base()
	{
		Min = 0;
		Max = 1;
	}

	[Output( typeof( Vector3 ) ), Title( "XYZ" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( "", Value, default, default, false, false, default );
	};

	/// <summary>
	/// X component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueX ) ), Title( "X" )]
	[Range( nameof( MinX ), nameof( MaxX ), nameof( Step ) )]
	public NodeResult.Func X => ( GraphCompiler compiler ) => Component( "x", ValueX, compiler );

	/// <summary>
	/// Y component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueY ) ), Title( "Y" )]
	[Range( nameof( MinY ), nameof( MaxY ), nameof( Step ) )]
	public NodeResult.Func Y => ( GraphCompiler compiler ) => Component( "y", ValueY, compiler );

	/// <summary>
	/// Z component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueZ ) ), Title( "Z" )]
	[Range( nameof( MinZ ), nameof( MaxZ ), nameof( Step ) )]
	public NodeResult.Func Z => ( GraphCompiler compiler ) => Component( "z", ValueZ, compiler );

	public override object GetMinValue()
	{
		return Min;
	}

	public override object GetMaxValue()
	{
		return Max;
	}

	public override object GetStepValue()
	{
		return Step;
	}
}

/// <summary>
/// 4 float values.
/// </summary>
[Title( "Float4 Constant" ), Category( "Constants" ), Icon( "looks_4" ), Order( 5 )]
public sealed class Float4ConstantNode : ConstantNode<Vector4>
{
	[Hide]
	public override int Version => 1;

	[Hide, JsonIgnore]
	public override bool UseMinMax => true;

	[Hide, JsonIgnore]
	public override bool UseStep => true;

	[JsonIgnore, Hide]
	public float ValueX
	{
		get => Value.x;
		set => Value = Value.WithX( value );
	}

	[JsonIgnore, Hide]
	public float ValueY
	{
		get => Value.y;
		set => Value = Value.WithY( value );
	}

	[JsonIgnore, Hide]
	public float ValueZ
	{
		get => Value.z;
		set => Value = Value.WithZ( value );
	}

	[JsonIgnore, Hide]
	public float ValueW
	{
		get => Value.w;
		set => Value = Value.WithW( value );
	}

	public Float4ConstantNode() : base()
	{
	}

	[Output( typeof( Color ) ), Title( "XYZW" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( "", Value, default, default, false, false, default );
	};

	/// <summary>
	/// X component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueX ) ), Title( "X" )]
	public NodeResult.Func R => ( GraphCompiler compiler ) => Component( "x", ValueX, compiler );

	/// <summary>
	/// Green component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueY ) ), Title( "Y" )]
	public NodeResult.Func G => ( GraphCompiler compiler ) => Component( "y", ValueY, compiler );

	/// <summary>
	/// Y component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueZ ) ), Title( "Z" )]
	public NodeResult.Func B => ( GraphCompiler compiler ) => Component( "z", ValueZ, compiler );

	/// <summary>
	/// W component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueW ) ), Title( "W" )]
	public NodeResult.Func A => ( GraphCompiler compiler ) => Component( "w", ValueW, compiler );
}

/// <summary>
/// 4 float values, Just like <see cref="Float4ConstantNode"/> but with color control ui
/// </summary>
[Title( "Color Constant" ), Category( "Constants" ), Icon( "palette" ), Order( 6 )]
public sealed class ColorConstantNode : ConstantNode<Color>
{
	[Hide]
	public override int Version => 1;

	[Hide, JsonIgnore]
	public override bool UseMinMax => true;

	[Hide, JsonIgnore]
	public override bool UseStep => true;

	[JsonIgnore, Hide]
	public float ValueR
	{
		get => Value.r;
		set => Value = Value.WithRed( value );
	}

	[JsonIgnore, Hide]
	public float ValueG
	{
		get => Value.g;
		set => Value = Value.WithGreen( value );
	}

	[JsonIgnore, Hide]
	public float ValueB
	{
		get => Value.b;
		set => Value = Value.WithBlue( value );
	}

	[JsonIgnore, Hide]
	public float ValueA
	{
		get => Value.a;
		set => Value = Value.WithAlpha( value );
	}

	public ColorConstantNode() : base()
	{
		Value = Color.White;
	}

	[Output( typeof( Color ) ), Title( "RGBA" )]
	[Hide, Editor( nameof( Value ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( "", Value, default, default, false, false, default );
	};

	/// <summary>
	/// Red component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueR ) ), Title( "Red" )]
	public NodeResult.Func R => ( GraphCompiler compiler ) => Component( "r", ValueR, compiler );

	/// <summary>
	/// Green component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueG ) ), Title( "Green" )]
	public NodeResult.Func G => ( GraphCompiler compiler ) => Component( "g", ValueG, compiler );

	/// <summary>
	/// Green component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueB ) ), Title( "Blue" )]
	public NodeResult.Func B => ( GraphCompiler compiler ) => Component( "b", ValueB, compiler );

	/// <summary>
	/// Alpha component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueA ) ), Title( "Alpha" )]
	public NodeResult.Func A => ( GraphCompiler compiler ) => Component( "a", ValueA, compiler );
}
