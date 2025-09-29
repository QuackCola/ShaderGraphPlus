namespace ShaderGraphPlus;

public abstract class BaseBlackboardParameter
{

	public string Name { get; set; } = "";

	public BaseBlackboardParameter() { }
}

public abstract class BlackboardValue<T> : BaseBlackboardParameter
{
	public T Value { get; set; }

	[InlineEditor( Label = false ), Group( "UI" )]

	public ParameterUI UI { get; set; }

	public BlackboardValue() : base() { }

	public BlackboardValue( T value ) : base() 
	{ 
		Value = value;
	}
}

[Title( "Bool" )]
public sealed class BoolBlackboardParameter : BlackboardValue<bool>
{
	public BoolBlackboardParameter( bool value ) : base( value ) { }

	public BoolBlackboardParameter() : base() { }
}

[Title( "Int" )]
public sealed class IntBlackboardParameter : BlackboardValue<int>
{
	[Group( "Range" )] public int Min { get; set; }
	[Group( "Range" )] public int Max { get; set; }

	public IntBlackboardParameter()
	{
		Min = 0;
		Max = 0;
	}

	public IntBlackboardParameter( int value ) : base( value ) 
	{
		Min = 0;
		Max = 0;
	}
}

[Title( "Float" )]
public sealed class FloatBlackboardParameter : BlackboardValue<float>
{
	[Group( "Range" )] public float Min { get; set; }
	[Group( "Range" )] public float Max { get; set; }

	public FloatBlackboardParameter()
	{
		Min = 0;
		Max = 1;
	}

	public FloatBlackboardParameter( float value ) : base( value )
	{
		Min = 0;
		Max = 0;
	}
}

[Title( "Float2" )]
public sealed class Float2BlackboardParameter : BlackboardValue<Vector2>
{
	[Group( "Range" )] public Vector2 Min { get; set; }
	[Group( "Range" )] public Vector2 Max { get; set; }

	public Float2BlackboardParameter()
	{
		Min = 0;
		Max = 1;
	}

	public Float2BlackboardParameter( Vector2 value ) : base( value )
	{
		Min = 0;
		Max = 0;
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

[Title( "Float3" )]
public sealed class Float3BlackboardParameter : BlackboardValue<Vector3>
{
	[Group( "Range" )] public Vector3 Min { get; set; }
	[Group( "Range" )] public Vector3 Max { get; set; }

	public Float3BlackboardParameter()
	{
		Min = 0;
		Max = 1;
	}

	public Float3BlackboardParameter( Vector3 value ) : base( value )
	{
		Min = 0;
		Max = 0;
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

[Title( "Color" )]
public sealed class Float4BlackboardParameter : BlackboardValue<Color>
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

	public Float4BlackboardParameter()
	{
		Value = Color.White;
		UI = new ParameterUI { Type = UIType.Color };
	}

	public Float4BlackboardParameter( Color value ) : base( value )
	{
		UI = new ParameterUI { Type = UIType.Color };
	}
}
