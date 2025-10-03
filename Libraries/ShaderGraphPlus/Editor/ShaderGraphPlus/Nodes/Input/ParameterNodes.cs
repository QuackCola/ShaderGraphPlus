using NodeEditorPlus;
using GraphView = NodeEditorPlus.GraphView;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;
using NodeUI = NodeEditorPlus.NodeUI;

namespace ShaderGraphPlus.Nodes;


/// <summary>
/// Bool value for use in the material editor.
/// </summary>
[Title( "Bool" ), Category( "Parameters" ), Icon( "check_box" )]
public sealed class BoolParameterNode : ParameterNode<bool>
{
	[Hide]
	public override int Version => 2;

	public override void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is BoolBlackboardParameter boolBlackboardParameter )
		{
			Name = boolBlackboardParameter.Name;
			Value = boolBlackboardParameter.Value;
			UI = boolBlackboardParameter.UI;
		}
	}

	public BoolParameterNode() : base()
	{

	}

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Output( typeof( bool ) ), Title( "Value" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		UI = UI with { ShowStepProperty = false, ShowTypeProperty = false };
		return compiler.ResultParameter( Name, Value, default, default, false, IsAttribute, UI );
	};
}

///<summary>
/// Single int value.
///</summary>
[Title( "Int" ), Category( "Parameters" ), Icon( "looks_one" )]
public sealed class IntParameterNode : ParameterNode<int>
{
	[Hide]
	public override int Version => 2;

	public override void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is IntBlackboardParameter intBlackboardParameter )
		{
			Name = intBlackboardParameter.Name;
			Value = intBlackboardParameter.Value;
			UI = intBlackboardParameter.UI;
		}
	}

	[Output( typeof( int ) ), Title( "Value" )]
	[Hide, Range( nameof( Min ), nameof( Max ), nameof( Step ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		UI = UI with { ShowStepProperty = false, ShowTypeProperty = false };
		return compiler.ResultParameter( Name, Value, Min, Max, Min != Max, IsAttribute, UI );
	};
	
	[Group("Range")] public int Min { get; set; }
	[Group("Range")] public int Max { get; set; }
	
	public IntParameterNode()
	{
		Min = 0;
		Max = 1;
	}
}

/// <summary>
/// Single float value
/// </summary>
[Title( "Float" ), Category( "Parameters" ), Icon( "looks_one" )]
public sealed class FloatParameterNode : ParameterNode<float>
{
	[Hide]
	public override int Version => 2;

	public override void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is FloatBlackboardParameter floatBlackboardParameter )
		{
			Name = floatBlackboardParameter.Name;
			Value = floatBlackboardParameter.Value;
			UI = floatBlackboardParameter.UI;
		}
	}

	[Hide] public float Step => UI.Step;

	[Output( typeof( float ) ), Title( "Value" )]
	[Hide, Editor( nameof( Value ) ), Range( nameof( Min ), nameof( Max ), nameof( Step ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		UI = UI with { ShowStepProperty = true, ShowTypeProperty = true };
		return compiler.ResultParameter( Name, Value, Min, Max, Min != Max, IsAttribute, UI );
	};

	[Group( "Range" )] public float Min { get; set; }
	[Group( "Range" )] public float Max { get; set; }

	public FloatParameterNode()
	{
		Min = 0;
		Max = 1;
	}
}

/// <summary>
/// 2 float values
/// </summary>
[Title( "Float2" ), Category( "Parameters" ), Icon( "looks_two" )]
public sealed class Float2ParameterNode : ParameterNode<Vector2>
{
	[Hide]
	public override int Version => 2;

	public override void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is Float2BlackboardParameter float2BlackboardParameter )
		{
			Name = float2BlackboardParameter.Name;
			Value = float2BlackboardParameter.Value;
			UI = float2BlackboardParameter.UI;
		}
	}

	[Output( typeof( Vector2 ) ), Title( "XY" ), Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		UI = UI with { ShowStepProperty = true, ShowTypeProperty = true };
		return compiler.ResultParameter( Name, Value, Min, Max, Min != Max, IsAttribute, UI );
	};

	[Group( "Range" )] public Vector2 Min { get; set; }
	[Group( "Range" )] public Vector2 Max { get; set; }

	public Float2ParameterNode()
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
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueX ) ), Title( "X" )]
	[Range( nameof( MinX ), nameof( MaxX ), nameof( Step ) )]
	public NodeResult.Func X => ( GraphCompiler compiler ) => Component( "x", ValueX, compiler );

	/// <summary>
	/// Y component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueY ) ), Title( "Y" )]
	[Range( nameof( MinY ), nameof( MaxY ), nameof( Step ) )]
	public NodeResult.Func Y => ( GraphCompiler compiler ) => Component( "y", ValueY, compiler );

	public Vector4 GetRangeMin()
	{
		return new( Min.x, Min.y, 0, 0 );
	}

	public Vector4 GetRangeMax()
	{
		return new( Max.x, Max.y, 0, 0 );
	}
}

/// <summary>
/// 3 float values
/// </summary>
[Title( "Float3" ), Category( "Parameters" ), Icon( "looks_3" )]
public sealed class Float3ParameterNode : ParameterNode<Vector3>
{
	[Hide]
	public override int Version => 2;

	public override void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is Float3BlackboardParameter float3BlackboardParameter )
		{
			Name = float3BlackboardParameter.Name;
			Value = float3BlackboardParameter.Value;
			UI = float3BlackboardParameter.UI;
		}
	}

	[Output( typeof( Vector3 ) ), Title( "XYZ" ), Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		UI = UI with { ShowStepProperty = true, ShowTypeProperty = true };
		return compiler.ResultParameter( Name, Value, Min, Max, Min != Max, IsAttribute, UI );
	};

	[Group( "Range" )] public Vector3 Min { get; set; }
	[Group( "Range" )] public Vector3 Max { get; set; }

	public Float3ParameterNode()
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
}

/// <summary>
/// 4 float values
/// </summary>
[Title( "Float4" ), Category( "Parameters" ), Icon( "looks_4" )]
public sealed class Float4ParameterNode : ParameterNode<Vector4>
{
	[Hide]
	public override int Version => 2;

	public override void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is Float4BlackboardParameter float4BlackboardParameter )
		{
			Name = float4BlackboardParameter.Name;
			Value = float4BlackboardParameter.Value;
			UI = float4BlackboardParameter.UI;
		}
	}

	[Output( typeof( Vector3 ) ), Title( "XYZ" ), Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		UI = UI with { ShowStepProperty = true, ShowTypeProperty = true };
		return compiler.ResultParameter( Name, Value, Min, Max, Min != Max, IsAttribute, UI );
	};

	[Group( "Range" )] public Vector4 Min { get; set; }
	[Group( "Range" )] public Vector4 Max { get; set; }

	public Float4ParameterNode()
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

	[JsonIgnore, Hide]
	public float ValueW
	{
		get => Value.w;
		set => Value = Value.WithW( value );
	}

	[Hide] public float MinX => Min.x;
	[Hide] public float MinY => Min.y;
	[Hide] public float MinZ => Min.z;
	[Hide] public float MinW => Min.w;
	[Hide] public float MaxX => Max.x;
	[Hide] public float MaxY => Max.y;
	[Hide] public float MaxZ => Max.z;
	[Hide] public float MaxW => Max.w;
	[Hide] public float Step => UI.Step;

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

	/// <summary>
	/// W component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Editor( nameof( ValueW ) ), Title( "W" )]
	[Range( nameof( MinW ), nameof( MaxW ), nameof( Step ) )]
	public NodeResult.Func W => ( GraphCompiler compiler ) => Component( "w", ValueW, compiler );
}

/// <summary>
/// 4 float values, Just like <see cref="Float4ParameterNode"/> but with color controls.
/// </summary>
[Title( "Color" ), Category( "Parameters" )]
public sealed class ColorParameterNode : ParameterNode<Color>
{
	[Hide]
	public override int Version => 2;

	public override void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is ColorBlackboardParameter colorBlackboardParameter )
		{
			Name = colorBlackboardParameter.Name;
			Value = colorBlackboardParameter.Value;
			UI = colorBlackboardParameter.UI;
		}
	}

	[Output( typeof( Color ) ), Title( "RGBA" )]
	[Hide, Editor( nameof( Value ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		UI = UI with { ShowStepProperty = true, ShowTypeProperty = true };
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

	public ColorParameterNode()
	{
		Value = Color.White;
		UI = new ParameterUI { Type = UIType.Color };
	}
}
