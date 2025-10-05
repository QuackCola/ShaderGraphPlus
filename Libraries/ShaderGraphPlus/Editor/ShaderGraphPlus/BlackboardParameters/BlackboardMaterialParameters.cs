using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

internal interface IShaderFeatureBlackboardParameter
{
}

/// <summary>
/// Bool value material parameter
/// </summary>
[Title( "Bool" ), Icon( "check_box" ), Order( 0 )]
public sealed class BoolBlackboardParameter : BlackboardMaterialParameter<bool>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 0;

	public BoolBlackboardParameter() : base() 
	{ 
		Value = false;
	}

	public BoolBlackboardParameter( bool value ) : base( value )
	{

	}

}

/// <summary>
/// Int value material parameter
/// </summary>
[Title( "Int" ), Icon( "looks_one" ), Order( 1 )]
public sealed class IntBlackboardParameter : BlackboardMaterialParameter<int>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 1;

	[Group( "Range" )] public int Min { get; set; }
	[Group( "Range" )] public int Max { get; set; }

	public IntBlackboardParameter()
	{	
		Value = 1;
		Min = 0;
		Max = 1;
	}

	public IntBlackboardParameter( int value ) : base( value )
	{
		Min = 0;
		Max = 1;
	}
}

/// <summary>
/// Float value material parameter
/// </summary>
[Title( "Float" ), Icon( "looks_one" ), Order( 2 )]
public sealed class FloatBlackboardParameter : BlackboardMaterialParameter<float>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 2;

	[Group( "Range" )] public float Min { get; set; }
	[Group( "Range" )] public float Max { get; set; }

	public FloatBlackboardParameter()
	{
		Value = 1.0f;
		Min = 0.0f;
		Max = 1.0f;
	}

	public FloatBlackboardParameter( float value ) : base( value )
	{
		Min = 0.0f;
		Max = 1.0f;
	}
}

/// <summary>
/// Float2 value material parameter
/// </summary>
[Title( "Float2" ), Icon( "looks_two" ), Order( 3 )]
public sealed class Float2BlackboardParameter : BlackboardMaterialParameter<Vector2>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 3;

	[Group( "Range" )] public Vector2 Min { get; set; }
	[Group( "Range" )] public Vector2 Max { get; set; }

	public Float2BlackboardParameter()
	{
		Value = Vector2.One;
		Min = Vector2.Zero;
		Max = Vector2.One;
	}

	public Float2BlackboardParameter( Vector2 value ) : base( value )
	{
		Min = Vector2.Zero;
		Max = Vector2.One;
	}
}

/// <summary>
/// Float3 value material parameter
/// </summary>
[Title( "Float3" ), Icon( "looks_3" ), Order( 4 )]
public sealed class Float3BlackboardParameter : BlackboardMaterialParameter<Vector3>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 4;

	[Group( "Range" )] public Vector3 Min { get; set; }
	[Group( "Range" )] public Vector3 Max { get; set; }

	public Float3BlackboardParameter()
	{
		Value = Vector3.One;
		Min = Vector3.Zero;
		Max = Vector3.One;
	}

	public Float3BlackboardParameter( Vector3 value ) : base( value )
	{
		Min = Vector3.Zero;
		Max = Vector3.One;
	}
}

/// <summary>
/// Float4 value material parameter
/// </summary>
[Title( "Float4" ), Icon( "looks_4" ), Order( 5 )]
public sealed class Float4BlackboardParameter : BlackboardMaterialParameter<Vector4>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 5;

	[Group( "Range" )] public Vector4 Min { get; set; }
	[Group( "Range" )] public Vector4 Max { get; set; }

	public Float4BlackboardParameter()
	{
		Value = Vector4.One;
		Min = Vector4.Zero;
		Max = Vector4.One;
		UI = new ParameterUI { Type = UIType.Default };
	}

	public Float4BlackboardParameter( Vector4 value ) : base( value )
	{
		Min = Vector4.Zero;
		Max = Vector4.One;
		UI = new ParameterUI { Type = UIType.Default };
	}
}

/// <summary>
/// Color value material parameter
/// </summary>
[Title( "Color" ), Icon( "palette" ), Order( 6 )]
public sealed class ColorBlackboardParameter : BlackboardMaterialParameter<Color>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 6;

	public ColorBlackboardParameter()
	{
		Value = Color.White;
		UI = new ParameterUI { Type = UIType.Color, ShowTypeProperty = false };
	}

	public ColorBlackboardParameter( Color value ) : base( value )
	{
		UI = new ParameterUI { Type = UIType.Color, ShowTypeProperty = false };
	}
}

/// <summary>
/// Bool material feature
/// </summary>
[Title( "Shader Feature Boolean" ), Order( 7 )]
public sealed class ShaderFeatureBooleanBlackboardParameter : BlackboardGenericParameter<ShaderFeatureBoolean>, IShaderFeatureBlackboardParameter
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 7;

	[JsonIgnore, Hide]
	public override string Name { get; set; }

	public ShaderFeatureBooleanBlackboardParameter( ShaderFeatureBoolean value ) : base( value )
	{
	}

	public ShaderFeatureBooleanBlackboardParameter() : base() 
	{ 
	}

}

/// <summary>
/// TODO : Unhide when Static Combo Enum Switch or similar is implemented.
/// </summary>
[Title( "Shader Feature Enum" ), Order( 8 )]
[Hide]
public sealed class ShaderFeatureEnumBlackboardParameter : BlackboardGenericParameter<ShaderFeatureEnum>, IShaderFeatureBlackboardParameter
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 8;

	[JsonIgnore, Hide]
	public override string Name { get; set; }

	public ShaderFeatureEnumBlackboardParameter( ShaderFeatureEnum value ) : base( value )
	{
	}

	public ShaderFeatureEnumBlackboardParameter() : base()
	{
	}
}
