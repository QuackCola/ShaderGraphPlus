namespace ShaderGraphPlus;

/// <summary>
/// Bool value subgraph input
/// </summary>
[Title( "Bool" ), Icon( "check_box" ), Order( 0 )]
[SubgraphOnly]
public sealed class BoolSubgraphInputBlackboardParameter : BlackboardSubgraphInputParameter<bool>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 0;

	public BoolSubgraphInputBlackboardParameter() : base()
	{
		Value = false;
	}

	public BoolSubgraphInputBlackboardParameter( bool value ) : base( value )
	{

	}

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputData = new VariantValueBool( Value, SubgraphPortType.Bool )
		};
	}
}

/// <summary>
/// Int value subgraph input
/// </summary>
[Title( "Int" ), Icon( "looks_one" ), Order( 1 )]
[SubgraphOnly]
public sealed class IntSubgraphInputBlackboardParameter : BlackboardSubgraphInputParameter<int>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 1;

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

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputData = new VariantValueInt( Value, SubgraphPortType.Int )
		};
	}
}

/// <summary>
/// Float value subgraph input
/// </summary>
[Title( "Float" ), Icon( "looks_one" ), Order( 2 )]
[SubgraphOnly]
public sealed class FloatSubgraphInputBlackboardParameter : BlackboardSubgraphInputParameter<float>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 2;

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

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputData = new VariantValueFloat( Value, SubgraphPortType.Float )
		};
	}
}

/// <summary>
/// Float2 value subgraph input
/// </summary>
[Title( "Float2" ), Icon( "looks_two" ), Order( 3 )]
[SubgraphOnly]
public sealed class Float2SubgraphInputBlackboardParameter : BlackboardSubgraphInputParameter<Vector2>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 3;

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

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputData = new VariantValueVector2( Value, SubgraphPortType.Vector2 )
		};
	}
}

/// <summary>
/// Float3 value subgraph input
/// </summary>
[Title( "Float3" ), Icon( "looks_3" ), Order( 4 )]
[SubgraphOnly]
public sealed class Float3SubgraphInputBlackboardParameter : BlackboardSubgraphInputParameter<Vector3>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 4;

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

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputData = new VariantValueVector3( Value, SubgraphPortType.Vector3 )
		};
	}
}

/// <summary>
/// Float4 value subgraph input
/// </summary>
[Title( "Float4" ), Icon( "looks_4" ), Order( 5 )]
[SubgraphOnly]
public sealed class Float4SubgraphInputBlackboardParameter : BlackboardSubgraphInputParameter<Vector4>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 5;

	[Group( "Range" )] public Vector4 Min { get; set; }
	[Group( "Range" )] public Vector4 Max { get; set; }

	public Float4SubgraphInputBlackboardParameter()
	{
		Value = Vector4.One;
		Min = Vector4.Zero;
		Max = Vector4.One;
	}

	public Float4SubgraphInputBlackboardParameter( Vector4 value ) : base( value )
	{
		Min = Vector4.Zero;
		Max = Vector4.One;
	}

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputData = new VariantValueVector4( Value, SubgraphPortType.Vector4 )
		};
	}
}

/// <summary>
/// Color value subgraph input
/// </summary>
[Title( "Color" ), Icon( "palette" ), Order( 6 )]
[SubgraphOnly]
public sealed class ColorSubgraphInputBlackboardParameter : BlackboardSubgraphInputParameter<Color>
{
	[Hide, JsonIgnore, Browsable( false )]
	public override int MenuOrder => 6;

	public ColorSubgraphInputBlackboardParameter()
	{
		Value = Color.White;
	}

	public ColorSubgraphInputBlackboardParameter( Color value ) : base( value )
	{
	}

	public override BaseNodePlus InitializeNode()
	{
		return new SubgraphInput()
		{
			BlackboardParameterIdentifier = Identifier,
			InputName = Name,
			InputDescription = Description,
			InputData = new VariantValueColor( Value, SubgraphPortType.Color )
		};
	}
}
