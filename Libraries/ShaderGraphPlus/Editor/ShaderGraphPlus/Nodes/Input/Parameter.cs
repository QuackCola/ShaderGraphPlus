
using Editor.ShaderGraph;
using System;

namespace Editor.ShaderGraphPlus.Nodes;

public abstract class MatrixParameterNode<T> : ShaderNodePlus
{
	public string Name { get; set; } = "";

	[Hide]
	public override string Title => string.IsNullOrWhiteSpace(Name) ?
		$"{DisplayInfo.For(this).Name}" :
		$"{DisplayInfo.For(this).Name} ( {Name} )";

	[InlineEditor]
	public T Value { get; set; }

	/// <summary>
	/// Enable this if you want to be able to set this via an Attribute via code. 
	/// False means it wont be generated as a global in the generated shader and thus will be local to the code.
	/// </summary>
	public bool IsAttribute { get; set; } = false;
}

[Title( "Float 2x2" ), Category( "Constants" )]
public sealed class Float2x2Node : MatrixParameterNode<Float2x2>
{
	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Output( typeof( Float2x2 ) ), Title( "Value" )]
	[Hide]
	[NodeEditor( nameof( Value ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( Name, Value, default, default, false, IsAttribute, default );
	};
}

[Title( "Float 3x3" ), Category( "Constants" )]
public sealed class Float3x3Node : MatrixParameterNode<Float3x3>
{
	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Output( typeof( Float3x3 ) ), Title( "Value" )]
	[Hide]
	[NodeEditor( nameof( Value ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( Name, Value, default, default, false, IsAttribute, default );
	};
}

[Title( "Float 4x4" ), Category( "Constants" )]
public sealed class Float4x4Node : MatrixParameterNode<Float4x4>
{
	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Output( typeof( Float4x4 ) ), Title( "Value" )]
	[Hide]
	[NodeEditor( nameof( Value ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( Name, Value, default, default, false, IsAttribute, default );
	};
}

/// <summary>
/// Bool value for use in the material editor.
/// </summary>
[Title( "Bool" ), Category( "Constants" ), Icon( "check_box" )]
public sealed class Bool : ParameterNode<bool>
{
	public Bool() : base()
	{
	}

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Output( typeof( bool ) ), Title( "Value" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		UI = UI with { HideProps = true };
		return compiler.ResultParameter( Name, Value, default, default, false, IsAttribute, UI );
	};
}

// <summary>
// Single int32 value stored as a float internally.
// </summary>
//[Title("Int"), Category("Constants"), Icon("looks_one")]
//public sealed class Int : ParameterNode<int>
//{
//	[Output(typeof(float)), Title("Value")]
//	[Hide, Range(nameof(Min), nameof(Max), nameof(Step))]
//	public NodeResult.Func Result => (GraphCompiler compiler) =>
//	{
//		return compiler.ResultParameter(Name, Value, default, default, false, IsAttribute, UI);
//	};
//	
//	[Group("Range")] public int Min { get; set; }
//	[Group("Range")] public int Max { get; set; }
//	
//	public Int()
//	{
//		Min = 0;
//		Max = 1;
//	}
//}

/// <summary>
/// Single float value
/// </summary>
[Title( "Float" ), Category( "Constants" ), Icon( "looks_one" )]
public sealed class Float : ParameterNode<float>
{
	[Hide] public float Step => UI.Step;

	[Output( typeof( float ) ), Title( "Value" )]
	[Hide, NodeEditor( nameof( Value ) ), Range( nameof( Min ), nameof( Max ), nameof( Step ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( Name, Value, Min, Max, Min != Max, IsAttribute, UI );
	};

	[Group( "Range" )] public float Min { get; set; }
	[Group( "Range" )] public float Max { get; set; }

	public Float()
	{
		Min = 0;
		Max = 1;
	}

	public override Vector4 GetRangeMin()
	{
		return new( Min );
	}

	public override Vector4 GetRangeMax()
	{
		return new( Max );
	}
}

/// <summary>
/// 2 float values
/// </summary>
[Title( "Float2" ), Category( "Constants" ), Icon( "looks_two" )]
public sealed class Float2 : ParameterNode<Vector2>
{
	[Output( typeof( Vector2 ) ), Title( "XY" ), Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( Name, Value, Min, Max, Min != Max, IsAttribute, UI );
	};

	[Group( "Range" )] public Vector2 Min { get; set; }
	[Group( "Range" )] public Vector2 Max { get; set; }

	public Float2()
	{
		Min = 0;
		Max = 1;
	}

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

	[Hide] public float MinX => Min.x;
	[Hide] public float MinY => Min.y;
	[Hide] public float MaxX => Max.x;
	[Hide] public float MaxY => Max.y;

	[Hide] public float Step => UI.Step;

	/// <summary>
	/// X component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeEditor( nameof( ValueX ) ), Title( "X" )]
	[Range( nameof( MinX ), nameof( MaxX ), nameof( Step ) )]
	public NodeResult.Func X => ( GraphCompiler compiler ) => Component( "x", ValueX, compiler );

	/// <summary>
	/// Y component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeEditor( nameof( ValueY ) ), Title( "Y" )]
	[Range( nameof( MinY ), nameof( MaxY ), nameof( Step ) )]
	public NodeResult.Func Y => ( GraphCompiler compiler ) => Component( "y", ValueY, compiler );

	public override Vector4 GetRangeMin()
	{
		return new( Min.x, Min.y, 0, 0 );
	}

	public override Vector4 GetRangeMax()
	{
		return new( Max.x, Max.y, 0, 0 );
	}
}

/// <summary>
/// 3 float values
/// </summary>
[Title( "Float3" ), Category( "Constants" ), Icon( "looks_3" )]
public sealed class Float3 : ParameterNode<Vector3>
{
	[Output( typeof( Vector3 ) ), Title( "XYZ" ), Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( Name, Value, Min, Max, Min != Max, IsAttribute, UI );
	};

	[Group( "Range" )] public Vector3 Min { get; set; }
	[Group( "Range" )] public Vector3 Max { get; set; }

	public Float3()
	{
		Min = 0;
		Max = 1;
	}

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

	[Hide] public float MinX => Min.x;
	[Hide] public float MinY => Min.y;
	[Hide] public float MinZ => Min.z;
	[Hide] public float MaxX => Max.x;
	[Hide] public float MaxY => Max.y;
	[Hide] public float MaxZ => Max.z;

	[Hide] public float Step => UI.Step;

	/// <summary>
	/// X component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeEditor( nameof( ValueX ) ), Title( "X" )]
	[Range( nameof( MinX ), nameof( MaxX ), nameof( Step ) )]
	public NodeResult.Func X => ( GraphCompiler compiler ) => Component( "x", ValueX, compiler );

	/// <summary>
	/// Y component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeEditor( nameof( ValueY ) ), Title( "Y" )]
	[Range( nameof( MinY ), nameof( MaxY ), nameof( Step ) )]
	public NodeResult.Func Y => ( GraphCompiler compiler ) => Component( "y", ValueY, compiler );

	/// <summary>
	/// Z component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeEditor( nameof( ValueZ ) ), Title( "Z" )]
	[Range( nameof( MinZ ), nameof( MaxZ ), nameof( Step ) )]
	public NodeResult.Func Z => ( GraphCompiler compiler ) => Component( "z", ValueZ, compiler );
}


/// <summary>
/// 4 float values, normally used as a color
/// </summary>
[Title( "Color" ), Category( "Constants" )]
public sealed class Float4 : ParameterNode<Color>
{
	[Output( typeof( Color ) ), Title( "RGBA" )]
	[Hide, NodeEditor( nameof( Value ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( Name, Value, default, default, false, IsAttribute, UI );
	};

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

	/// <summary>
	/// Red component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeEditor( nameof( ValueR ) ), Title( "Red" )]
	public NodeResult.Func R => ( GraphCompiler compiler ) => Component( "r", ValueR, compiler );

	/// <summary>
	/// Green component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeEditor( nameof( ValueG ) ), Title( "Green" )]
	public NodeResult.Func G => ( GraphCompiler compiler ) => Component( "g", ValueG, compiler );

	/// <summary>
	/// Green component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeEditor( nameof( ValueB ) ), Title( "Blue" )]
	public NodeResult.Func B => ( GraphCompiler compiler ) => Component( "b", ValueB, compiler );

	/// <summary>
	/// Alpha component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, NodeEditor( nameof( ValueA ) ), Title( "Alpha" )]
	public NodeResult.Func A => ( GraphCompiler compiler ) => Component( "a", ValueA, compiler );

	public Float4()
	{
		Value = Color.White;
		UI = new ParameterUI { Type = UIType.Color };
	}
}
