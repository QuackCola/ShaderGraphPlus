namespace ShaderGraphPlus;


[Title( "Bool" ), Icon( "check_box" ), Order( 0 )]
[SubgraphOnly]
public sealed class BoolSubgraphInputBlackboardParameter : BlackboardSubgraphInputParameter<bool>
{
	public BoolSubgraphInputBlackboardParameter() : base()
	{
		Value = false;
	}

	public BoolSubgraphInputBlackboardParameter( bool value ) : base( value )
	{

	}

}

[Title( "Int" ), Icon( "looks_one" ), Order( 1 )]
[SubgraphOnly]
public sealed class IntSubgraphInputBlackboardParameter : BlackboardSubgraphInputParameter<int>
{
	[Group( "Range" )] public int Min { get; set; }
	[Group( "Range" )] public int Max { get; set; }

	public IntSubgraphInputBlackboardParameter()
	{
		Value = 1;
		Min = 0;
		Max = 1;
	}

	public IntSubgraphInputBlackboardParameter( int value ) : base( value )
	{
		Min = 0;
		Max = 1;
	}
}

[Title( "Float" ), Icon( "looks_one" ), Order( 2 )]
[SubgraphOnly]
public sealed class FloatSubgraphInputBlackboardParameter : BlackboardSubgraphInputParameter<float>
{
	[Group( "Range" )] public float Min { get; set; }
	[Group( "Range" )] public float Max { get; set; }

	public FloatSubgraphInputBlackboardParameter()
	{
		Value = 1.0f;
		Min = 0.0f;
		Max = 1.0f;
	}

	public FloatSubgraphInputBlackboardParameter( float value ) : base( value )
	{
		Min = 0.0f;
		Max = 1.0f;
	}
}

[Title( "Float2" ), Icon( "looks_two" ), Order( 3 )]
[SubgraphOnly]
public sealed class Float2SubgraphInputBlackboardParameter : BlackboardSubgraphInputParameter<Vector2>
{
	[Group( "Range" )] public Vector2 Min { get; set; }
	[Group( "Range" )] public Vector2 Max { get; set; }

	public Float2SubgraphInputBlackboardParameter()
	{
		Value = Vector2.One;
		Min = Vector2.Zero;
		Max = Vector2.One;
	}

	public Float2SubgraphInputBlackboardParameter( Vector2 value ) : base( value )
	{
		Min = Vector2.Zero;
		Max = Vector2.One;
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

}

[Title( "Float3" ), Icon( "looks_3" ), Order( 4 )]
[SubgraphOnly]
public sealed class Float3SubgraphInputBlackboardParameter : BlackboardSubgraphInputParameter<Vector3>
{
	[Group( "Range" )] public Vector3 Min { get; set; }
	[Group( "Range" )] public Vector3 Max { get; set; }

	public Float3SubgraphInputBlackboardParameter()
	{
		Value = Vector3.One;
		Min = Vector3.Zero;
		Max = Vector3.One;
	}

	public Float3SubgraphInputBlackboardParameter( Vector3 value ) : base( value )
	{
		Min = Vector3.Zero;
		Max = Vector3.One;
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
}

[Title( "Float4" ), Icon( "looks_4" ), Order( 5 )]
[SubgraphOnly]
public sealed class Float4SubgraphInputBlackboardParameter : BlackboardSubgraphInputParameter<Vector4>
{
	[Group( "Range" )] public Vector4 Min { get; set; }
	[Group( "Range" )] public Vector4 Max { get; set; }

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
	[Hide] public float MaxX => Max.x;
	[Hide] public float MaxY => Max.y;
	[Hide] public float MaxZ => Max.z;

	[Hide] public float Step => UI.Step;

	public Float4SubgraphInputBlackboardParameter()
	{
		Value = Vector4.One;
		Min = Vector4.Zero;
		Max = Vector4.One;
		UI = new ParameterUI { Type = UIType.Default };
	}

	public Float4SubgraphInputBlackboardParameter( Vector4 value ) : base( value )
	{
		Min = Vector4.Zero;
		Max = Vector4.One;
		UI = new ParameterUI { Type = UIType.Default };
	}
}

[Title( "Color" ), Icon( "palette" ), Order( 6 )]
[SubgraphOnly]
public sealed class ColorSubgraphInputBlackboardParameter : BlackboardSubgraphInputParameter<Color>
{
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

	public ColorSubgraphInputBlackboardParameter()
	{
		Value = Color.White;
		UI = new ParameterUI { Type = UIType.Color, ShowTypeProperty = false };
	}

	public ColorSubgraphInputBlackboardParameter( Color value ) : base( value )
	{
		UI = new ParameterUI { Type = UIType.Color, ShowTypeProperty = false };
	}
}
