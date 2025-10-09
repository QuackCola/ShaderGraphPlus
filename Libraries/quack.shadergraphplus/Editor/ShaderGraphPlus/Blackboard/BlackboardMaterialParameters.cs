using ShaderGraphPlus.Nodes;
using static Sandbox.Resources.ResourceGenerator;

namespace ShaderGraphPlus;

/// <summary>
/// Bool value material parameter
/// </summary>
[Title( "Bool" ), Icon( "check_box" ), Order( 0 )]
public sealed class BoolParameter : BlackboardMaterialParameter<bool>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 0;

	public BoolParameter() : base() 
	{ 
		Value = false;
	}

	public BoolParameter( bool value ) : base( value )
	{

	}

	public override BaseNodePlus InitializeNode()
	{
		return new BoolParameterNode()
		{
			BlackboardParameterIdentifier = Identifier,
			Name = Name,
			Value = Value,
			UI = UI,
			IsAttribute = IsAttribute,
		};
	}
}

/// <summary>
/// Int value material parameter
/// </summary>
[Title( "Int" ), Icon( "looks_one" ), Order( 1 )]
public sealed class IntParameter : BlackboardMaterialParameter<int>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 1;

	[Group( "Range" )] public int Min { get; set; }
	[Group( "Range" )] public int Max { get; set; }

	public IntParameter() : base()
	{	
		Value = 1;
		Min = 0;
		Max = 1;
	}

	public IntParameter( int value ) : base( value )
	{
		Min = 0;
		Max = 1;
	}

	public override BaseNodePlus InitializeNode()
	{
		return new IntParameterNode()
		{
			BlackboardParameterIdentifier = Identifier,
			Name = Name,
			Value = Value,
			UI = UI,
			IsAttribute = IsAttribute,
		};
	}
}

/// <summary>
/// Float value material parameter
/// </summary>
[Title( "Float" ), Icon( "looks_one" ), Order( 2 )]
public sealed class FloatParameter : BlackboardMaterialParameter<float>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 2;

	[Group( "Range" )] public float Min { get; set; }
	[Group( "Range" )] public float Max { get; set; }

	public FloatParameter() : base()
	{
		Value = 1.0f;
		Min = 0.0f;
		Max = 1.0f;
	}

	public FloatParameter( float value ) : base( value )
	{
		Min = 0.0f;
		Max = 1.0f;
	}

	public override BaseNodePlus InitializeNode()
	{
		return new FloatParameterNode()
		{
			BlackboardParameterIdentifier = Identifier,
			Name = Name,
			Value = Value,
			UI = UI,
			IsAttribute = IsAttribute,
		};
	}
}

/// <summary>
/// Float2 value material parameter
/// </summary>
[Title( "Float2" ), Icon( "looks_two" ), Order( 3 )]
public sealed class Float2Parameter : BlackboardMaterialParameter<Vector2>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 3;

	[Group( "Range" )] public Vector2 Min { get; set; }
	[Group( "Range" )] public Vector2 Max { get; set; }

	public Float2Parameter() : base()
	{
		Value = Vector2.One;
		Min = Vector2.Zero;
		Max = Vector2.One;
	}

	public Float2Parameter( Vector2 value ) : base( value )
	{
		Min = Vector2.Zero;
		Max = Vector2.One;
	}

	public override BaseNodePlus InitializeNode()
	{
		return new Float2ParameterNode()
		{
			BlackboardParameterIdentifier = Identifier,
			Name = Name,
			Value = Value,
			UI = UI,
			IsAttribute = IsAttribute,
		};
	}
}

/// <summary>
/// Float3 value material parameter
/// </summary>
[Title( "Float3" ), Icon( "looks_3" ), Order( 4 )]
public sealed class Float3Parameter : BlackboardMaterialParameter<Vector3>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 4;

	[Group( "Range" )] public Vector3 Min { get; set; }
	[Group( "Range" )] public Vector3 Max { get; set; }

	public Float3Parameter() : base()
	{
		Value = Vector3.One;
		Min = Vector3.Zero;
		Max = Vector3.One;
	}

	public Float3Parameter( Vector3 value ) : base( value )
	{
		Min = Vector3.Zero;
		Max = Vector3.One;
	}

	public override BaseNodePlus InitializeNode()
	{
		return new Float3ParameterNode()
		{
			BlackboardParameterIdentifier = Identifier,
			Name = Name,
			Value = Value,
			UI = UI,
			IsAttribute = IsAttribute,
		};
	}
}

/// <summary>
/// Float4 value material parameter
/// </summary>
[Title( "Float4" ), Icon( "looks_4" ), Order( 5 )]
public sealed class Float4Parameter : BlackboardMaterialParameter<Vector4>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 5;

	[Group( "Range" )] public Vector4 Min { get; set; }
	[Group( "Range" )] public Vector4 Max { get; set; }

	public Float4Parameter() : base()
	{
		Value = Vector4.One;
		Min = Vector4.Zero;
		Max = Vector4.One;
		UI = new ParameterUI { Type = UIType.Default };
	}

	public Float4Parameter( Vector4 value ) : base( value )
	{
		Min = Vector4.Zero;
		Max = Vector4.One;
		UI = new ParameterUI { Type = UIType.Default };
	}

	public override BaseNodePlus InitializeNode()
	{
		return new Float4ParameterNode()
		{
			BlackboardParameterIdentifier = Identifier,
			Name = Name,
			Value = Value,
			UI = UI,
			IsAttribute = IsAttribute,
		};
	}
}

/// <summary>
/// Color value material parameter
/// </summary>
[Title( "Color" ), Icon( "palette" ), Order( 6 )]
public sealed class ColorParameter : BlackboardMaterialParameter<Color>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 6;

	public ColorParameter() : base()
	{
		Value = Color.White;
		UI = new ParameterUI { Type = UIType.Color, ShowTypeProperty = false };
	}

	public ColorParameter( Color value ) : base( value )
	{
		UI = new ParameterUI { Type = UIType.Color, ShowTypeProperty = false };
	}

	public override BaseNodePlus InitializeNode()
	{
		return new ColorParameterNode()
		{
			BlackboardParameterIdentifier = Identifier,
			Name = Name,
			Value = Value,
			UI = UI,
			IsAttribute = IsAttribute,
		};
	}
}

/// <summary>
/// Bool material feature
/// </summary>
[Title( "Shader Feature Boolean" ), Order( 7 )]
public sealed class ShaderFeatureBooleanParameter : BaseBlackboardParameter, IShaderFeatureParameter
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 7;

	[Hide, JsonIgnore, Browsable( false )]
	public override bool IsValid => !string.IsNullOrWhiteSpace( Name );

	/// <summary>
	/// Name of this feature.
	/// </summary>
	[Title( "Feature Name" )]
	public override string Name { get; set; }

	/// <summary>
	/// What this feature does.
	/// </summary>
	[Hide]
	public string Description { get; set; }

	/// <summary>
	/// Header Name of this Feature that shows up in the Material Editor.
	/// </summary>
	public string HeaderName { get; set; }

	public ShaderFeatureBooleanParameter() : base() 
	{ 
	}

	public override BaseNodePlus InitializeNode()
	{
		return new BooleanComboSwitchNode()
		{
			BlackboardParameterIdentifier = Identifier,
			Feature = new ShaderFeatureBoolean() 
			{ 
				Name = Name,
				Description = Description,
				HeaderName = HeaderName,
			}
		};
	}
}

/// <summary>
/// TODO : 
/// </summary>
[Title( "Shader Feature Enum" ), Order( 8 )]
public sealed class ShaderFeatureEnumParameter : BaseBlackboardParameter, IShaderFeatureParameter
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 8;

	[Hide, JsonIgnore, Browsable( false )]
	public override bool IsValid => !string.IsNullOrWhiteSpace( Name ) && Options.All( x => !string.IsNullOrWhiteSpace( x ) );

	/// <summary>
	/// Name of this feature.
	/// </summary>
	[Title( "Feature Name" )]
	public override string Name { get; set; }

	/// <summary>
	/// What this feature does.
	/// </summary>
	[Hide]
	public string Description { get; set; }

	/// <summary>
	/// Header Name of this Feature that shows up in the Material Editor.
	/// </summary>
	public string HeaderName { get; set; }

	/// <summary>
	/// Options of your feature. Must have no special characters. Note : all lowercase letters will be converted to uppercase.
	/// </summary>
	public List<string> Options { get; set; }

	public ShaderFeatureEnumParameter() : base()
	{
		Options = new List<string>();
	}

	public override BaseNodePlus InitializeNode()
	{
		return new EnumComboSwitchNode()
		{
			BlackboardParameterIdentifier = Identifier,
			Feature = new ShaderFeatureEnum()
			{
				Name = Name,
				Description = Description,
				HeaderName = HeaderName,
				Options = Options
			}
		};
	}
}
