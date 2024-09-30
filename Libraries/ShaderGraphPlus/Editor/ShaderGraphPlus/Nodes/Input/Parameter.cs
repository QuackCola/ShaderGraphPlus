﻿
using System;

namespace Editor.ShaderGraphPlus.Nodes;

public abstract class ParameterNode<T> : ShaderNodePlus
{
	[Hide]
	public override string Title => string.IsNullOrWhiteSpace( Name ) ?
		$"{DisplayInfo.For( this ).Name}" :
		$"{DisplayInfo.For( this ).Name} {Name}";

	public T Value { get; set; }

	public string Name { get; set; } = "";

	/// <summary>
	/// If true, this parameter can be modified with <see cref="RenderAttributes"/>.
	/// </summary>
	public bool IsAttribute { get; set; }

	[InlineEditor]
	//[Group( "UI" )]
	//[Title("")]
	public ParameterUI UI { get; set; }

	protected NodeResult Component( string component, float value, GraphCompiler compiler )
	{
		if ( compiler.IsPreview )
			return compiler.ResultValue( value );

		var result = compiler.Result( new NodeInput { Identifier = Identifier, Output = nameof( Result ) } );
		return new( ResultType.Float, $"{result}.{component}", true );
	}
}

public abstract class MatrixParameterNode<T> : ShaderNodePlus
{

    public string Name { get; set; } = "";

    [Hide]
    public override string Title => string.IsNullOrWhiteSpace(Name) ?
        $"{DisplayInfo.For(this).Name}" :
        $"{DisplayInfo.For(this).Name} ({Name})";

    [InlineEditor]

    public T Value { get; set; }

    /// <summary>
    /// If true, this parameter can be modified with <see cref="RenderAttributes"/>.
    /// </summary>
    //[Hide]
    //public bool IsAttribute { get; set; } = false;

    // Material Editor dosent have a Matrix Property Type.
    //[Hide]
    //public ParameterUI UI { get; set; }

}

[Title("Float 2x2"), Category("Constants")]
public sealed class Float2x2Node : MatrixParameterNode<Float2x2>
{
    [Output(typeof(Float2x2)), Title("Value")]
    [Hide]
    [Editor(nameof(Value))]
    public NodeResult.Func Result => (GraphCompiler compiler) =>
    {
        return compiler.ResultParameter(Name, Value, default, default, false, false, default);
    };
}

[Title("Float 3x3"), Category("Constants")]
public sealed class Float3x3Node : MatrixParameterNode<Float3x3>
{
    [Output(typeof(Float3x3)), Title("Value")]
    [Hide]
    [Editor(nameof(Value))]
    public NodeResult.Func Result => (GraphCompiler compiler) =>
    {
        return compiler.ResultParameter(Name, Value, default, default, false, false, default);
    };
}

[Title("Float 4x4"), Category("Constants")]
public sealed class Float4x4Node : MatrixParameterNode<Float4x4>
{
    [Output(typeof(Float4x4)), Title("Value")]
    [Hide]
    [Editor(nameof(Value))]
    public NodeResult.Func Result => (GraphCompiler compiler) =>
    {
        return compiler.ResultParameter(Name, Value, default, default, false, false, default);
    };
}

/// <summary>
/// Bool value for use in the material editor.
/// </summary>
///*
[Title( "Bool" ), Category( "Constants" )]
public sealed class Bool : ParameterNode<bool>
{
	[Output( typeof( bool ) ), Title( "Value" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Bool, $"{compiler.ResultParameter( Name, Value, default, default, false, isAttribute: true, UI )}", true , true);
	};
}
//*/

/// <summary>
/// Single float value
/// </summary>
[Title( "Float" ), Category( "Constants" )]
public sealed class Float : ParameterNode<float>
{
	[Output( typeof( float ) ), Title( "Value" )]
	[Hide, Editor( nameof( Value ) ), Range( nameof( Min ), nameof( Max ) )]
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
}

/// <summary>
/// 2 float values
/// </summary>
[Title( "Float2" ), Category( "Constants" )]
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

	/// <summary>
	/// X component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueX ) ), Title( "X" )]
	[Range( nameof( MinX ), nameof( MaxX ) )]
	public NodeResult.Func X => ( GraphCompiler compiler ) => Component( "x", ValueX, compiler );

	/// <summary>
	/// Y component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueY ) ), Title( "Y" )]
	[Range( nameof( MinY ), nameof( MaxY ) )]
	public NodeResult.Func Y => ( GraphCompiler compiler ) => Component( "y", ValueY, compiler );
}

/// <summary>
/// 3 float values
/// </summary>
[Title( "Float3" ), Category( "Constants" )]
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

	/// <summary>
	/// X component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueX ) ), Title( "X" )]
	[Range( nameof( MinX ), nameof( MaxX ) )]
	public NodeResult.Func X => ( GraphCompiler compiler ) => Component( "x", ValueX, compiler );

	/// <summary>
	/// Y component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueY ) ), Title( "Y" )]
	[Range( nameof( MinY ), nameof( MaxY ) )]
	public NodeResult.Func Y => ( GraphCompiler compiler ) => Component( "y", ValueY, compiler );

	/// <summary>
	/// Z component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueZ ) ), Title( "Z" )]
	[Range( nameof( MinZ ), nameof( MaxZ ) )]
	public NodeResult.Func Z => ( GraphCompiler compiler ) => Component( "z", ValueZ, compiler );
}

/// <summary>
/// 4 float values, normally used as a color
/// </summary>
[Title( "Color" ), Category( "Constants" )]
public sealed class Float4 : ParameterNode<Color>
{
	[Output( typeof( Color ) ), Title( "RGBA" )]
	[Hide, Editor( nameof( Value ) )]
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

	public Float4()
	{
		Value = Color.White;
		UI = new ParameterUI { Type = UIType.Color };
	}
}
