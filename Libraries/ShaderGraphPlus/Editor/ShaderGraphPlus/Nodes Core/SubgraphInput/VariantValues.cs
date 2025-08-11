namespace ShaderGraphPlus;

/// <summary>
/// Base class of VariantProperty T
/// </summary>
[JsonConverter( typeof( VariantValueConverter ) )]
[JsonDerivedType( typeof( VariantValueBool ) ) ]
[JsonDerivedType( typeof( VariantValueFloat ) )]
[JsonDerivedType( typeof( VariantValueVector2 ) )]
[JsonDerivedType( typeof( VariantValueVector3 ) )]
[JsonDerivedType( typeof( VariantValueVector4 ) )]
[JsonDerivedType( typeof( VariantValueColor ) )]
[JsonDerivedType( typeof( VariantValueSampler ) )]
[JsonDerivedType( typeof( VariantValueTexture2D ) )]
public abstract class VariantValueBase
{
	public virtual SubgraphInputType InputType { get; set; }

	[Hide, JsonIgnore]
	public virtual bool HasRange => false;

	public VariantValueBase()
	{
	}

	public VariantValueBase( SubgraphInputType inputType )
	{
		InputType = inputType;
	}

	public static VariantValueBase CreateNew( object typeInstance, object minValueTypeInstance, object maxValueTypeInstance, SubgraphInputType inputType )
	{

		if ( minValueTypeInstance == null )
		{
			switch ( inputType )
			{
				case SubgraphInputType.Float:
					minValueTypeInstance = 0.0f;
					break;
				case SubgraphInputType.Vector2:
					minValueTypeInstance = Vector2.Zero;
					break;
				case SubgraphInputType.Vector3:
					minValueTypeInstance = Vector3.Zero;
					break;
				case SubgraphInputType.Color:
					minValueTypeInstance = Color.Black;
					break;
				default:
					minValueTypeInstance = null;
					break;
			}
		}

		if ( maxValueTypeInstance == null )
		{
			switch ( inputType )
			{
				case SubgraphInputType.Float:
					maxValueTypeInstance = 0.0f;
					break;
				case SubgraphInputType.Vector2:
					maxValueTypeInstance = Vector2.One;
					break;
				case SubgraphInputType.Vector3:
					maxValueTypeInstance = Vector3.One;
					break;
				case SubgraphInputType.Color:
					maxValueTypeInstance = Color.Black;
					break;
				default:
					maxValueTypeInstance = null;
					break;
			}
		}

		return typeInstance switch
		{
			bool => new VariantValueBool( (bool)typeInstance, SubgraphInputType.Bool),
			float => new VariantValueFloat( (float)typeInstance, (float)minValueTypeInstance, (float)maxValueTypeInstance, SubgraphInputType.Float ),
			Vector2 => new VariantValueVector2( (Vector2)typeInstance, (Vector2)minValueTypeInstance, (Vector2)maxValueTypeInstance, SubgraphInputType.Vector2 ),
			Vector3 => new VariantValueVector3( (Vector3)typeInstance, (Vector3)minValueTypeInstance, (Vector3)maxValueTypeInstance, SubgraphInputType.Vector3 ),
			Vector4 => new VariantValueVector4( (Vector4)typeInstance, (Vector4)minValueTypeInstance, (Vector4)maxValueTypeInstance, SubgraphInputType.Color ),
			Color => new VariantValueColor( (Color)typeInstance, (Color)minValueTypeInstance, (Color)maxValueTypeInstance, SubgraphInputType.Color ),
			Sampler => new VariantValueSampler( (Sampler)typeInstance, SubgraphInputType.Sampler ),
			TextureInput => new VariantValueTexture2D( (TextureInput)typeInstance, SubgraphInputType.Texture2DObject ),
			_ => throw new NotImplementedException( $"Unknown object of type \"{typeInstance}\"" ),
		};
	}
}

public static class VariantValueBaseExtentions
{
	/// <summary>
	/// Get the VariantValue<typeparamref name="T"/> as a VariantParam<typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="variantValueBase"></param>
	/// <param name="propertyDescription"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	public static VariantParam<T> GetAsVariantParam<T>( this VariantValueBase variantValueBase, string propertyDescription )
	{
		if ( variantValueBase is VariantValue<T> variantValue )
		{
			VariantParam<T> variantParam = new VariantParam<T>();
			variantParam.Name = "Default Value";
			variantParam.Description = propertyDescription;
			variantParam.Value = variantValue.Value;
			variantParam.DefaultValue = variantValue.Value;
			variantParam.MinValue = variantValue.MinValue;
			variantParam.MaxValue = variantValue.MaxValue;

			return variantParam;
		}
		else
		{
			throw new Exception( $"Unable to get \"{variantValueBase}\" as \"{ typeof( VariantParam<T> )}\"" );
		}
	}

	/// <summary>
	/// Fetch the value contained within a VariantValue<typeparamref name="T"/>. Throws an exeption when variaintValueBase is not VariantValue<typeparamref name="T"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="variantValueBase"></param>
	/// <returns>Object value of <typeparamref name="T"/></returns>
	/// <exception cref="Exception"></exception>
	public static T GetValue<T>( this VariantValueBase variantValueBase )
	{
		if ( variantValueBase is VariantValue<T> variantValue )
		{
			return variantValue.Value;
		}
		else
		{
			throw new Exception( $"Unable to get Value of type \"{typeof( T )}\"" );
		}
	}

	public static object GetValueAsObject( this VariantValueBase variantValueBase )
	{
		return variantValueBase switch
		{
			VariantValueBool v => v.Value,
			VariantValueInt v => v.Value,
			VariantValueFloat v => v.Value,
			VariantValueVector2 v => v.Value,
			VariantValueVector3 v => v.Value,
			VariantValueVector4 v => v.Value,
			VariantValueColor v => v.Value,
			VariantValueTexture2D v => v.Value,
			VariantValueSampler v => v.Value,
			_ => throw new NotImplementedException(),
		};
	}

	public static T GetValueRangeMin<T>( this VariantValueBase variantValueBase )
	{
		if ( variantValueBase is VariantValue<T> variantValue )
		{
			return variantValue.MinValue;
		}
		else
		{
			throw new Exception( $"Unable to get MinValue of type \"{typeof( T )}\"" );
		}
	}

	public static T GetValueRangeMax<T>( this VariantValueBase variantValueBase )
	{
		if ( variantValueBase is VariantValue<T> variantValue )
		{
			return variantValue.MaxValue;
		}
		else
		{
			throw new Exception( $"Unable to get MaxValue of type \"{typeof( T )}\"" );
		}
	}

	/// <summary>
	/// Set the value contained within a VariantValue<typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="variantValueBase"></param>
	/// <param name="value"></param>
	/// <exception cref="Exception"></exception>
	public static void SetValue<T>( this VariantValueBase variantValueBase, T value )
	{
		if ( variantValueBase is VariantValue<T> variantValue )
		{
			variantValue.Value = value;
		}
		else
		{
			throw new Exception( $"Unable to set Value of type \"{typeof( T )}\"" );
		}
	}

	public static void SetValueRangeMin<T>( this VariantValueBase variantValueBase, T value )
	{
		if ( variantValueBase is VariantValue<T> variantValue )
		{
			variantValue.MinValue = value;
		}
		else
		{
			throw new Exception( $"Unable to set MinValue of type \"{typeof( T )}\"" );
		}
	}

	public static void SetValueRangeMax<T>( this VariantValueBase variantValueBase, T value )
	{
		if ( variantValueBase is VariantValue<T> variantValue )
		{
			variantValue.MaxValue = value;
		}
		else
		{
			throw new Exception( $"Unable to set MaxValue of type \"{typeof( T )}\"" );
		}
	}
}

/// <summary>
/// Defines the actual value that is contained and the Accompanying ParamProperty
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class VariantValue<T> : VariantValueBase
{
	public T Value { get; set; }

	public virtual T MinValue { get; set; }

	public virtual T MaxValue { get; set; }

	public VariantValue( T value ) : base( )
	{
		Value = value;
	}

	public VariantValue( T value, SubgraphInputType inputType ) : this( value )
	{
		InputType = inputType;
	}

	public VariantValue() : base()
	{
		Value = default( T );
	}

	public VariantValue( SubgraphInputType inputType ) : base( inputType )
	{
	}
}

//[Title( "Bool" )]
public class VariantValueBool : VariantValue<bool>
{
	[Hide,JsonIgnore]
	public override bool MaxValue { get; set; }

	[Hide, JsonIgnore]
	public override bool MinValue { get; set; }

	public VariantValueBool( bool value, SubgraphInputType inputType ) : base( value, inputType )
	{
	}

	public VariantValueBool() : base()
	{
	}

	internal class ParamPropertyBool : ParamProperty<bool>
	{
		public ParamPropertyBool( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
			SetAttributes(
				new TitleAttribute( "" ),
				new InlineEditorAttribute() { Label = false },
				new DefaultValueAttribute( Parameter.DefaultValue )
			);
		}

		public override bool Value
		{
			get => Accessor.GetValue<bool>();
			set => Accessor.SetValue<bool>( value );
		}
	}
}

//[Title( "Int" )]
public class VariantValueInt : VariantValue<int>
{
	[Hide, JsonIgnore]
	public override bool HasRange => true;

	public VariantValueInt( int value, SubgraphInputType inputType ) : base( value, inputType )
	{
	}

	public VariantValueInt() : base()
	{
	}

	internal class ParamPropertyInt : ParamProperty<int>
	{
		public Vector2 Range { get; private init; }

		public ParamPropertyInt( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
			Range = new Vector2( Parameter.MinValue, Parameter.MaxValue );

			SetAttributes(
				new TitleAttribute( "" ),
				new InlineEditorAttribute() { Label = false },
				//new RangeAttribute( Range.x, Range.y ),
				new DefaultValueAttribute( Parameter.DefaultValue )
			);
		}

		public override int Value
		{
			get => Accessor.GetValue<int>();
			set => Accessor.SetValue<int>( value );
		}

	}

	internal class ParamPropertyIntRangeMin : ParamProperty<int>
	{
		public override string Name => "Range Min";
		public override string Description => PropertyDescription + "TODO : Remove this?";

		public ParamPropertyIntRangeMin( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
		}

		public override int Value
		{
			get => Accessor.GetValueRangeMin<int>();
			set => Accessor.SetValueRangeMin<int>( value );
		}
	}

	internal class ParamPropertyIntRangeMax : ParamProperty<int>
	{
		public override string Name => "Range Max";
		public override string Description => PropertyDescription + "TODO : Remove this?";
		public ParamPropertyIntRangeMax( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
		}

		public override int Value
		{
			get => Accessor.GetValueRangeMax<int>();
			set => Accessor.SetValueRangeMax<int>( value );
		}
	}
}

//[Title( "Float" )]
public class VariantValueFloat : VariantValue<float>
{
	[Hide, JsonIgnore]
	public override bool HasRange => true;

	public override float MinValue { get; set; } = 0.0f;
	public override float MaxValue { get; set; } = 1.0f;

	public VariantValueFloat( float value, float minValue, float maxValue, SubgraphInputType inputType ) : base( value , inputType )
	{
		MinValue = minValue;
		MaxValue = maxValue;
	}

	public VariantValueFloat() : base()
	{
	}

	internal class ParamPropertyFloat : ParamProperty<float>
	{
		public Vector2 Range { get; private init; }

		public ParamPropertyFloat( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
			Range = new Vector2( Parameter.MinValue, Parameter.MaxValue );

			SetAttributes(
				new TitleAttribute( "" ),
				new InlineEditorAttribute() { Label = false },
				//new RangeAttribute( Range.x, Range.y ),
				new DefaultValueAttribute( Parameter.DefaultValue )
			);
		}

		public override float Value
		{
			get => Accessor.GetValue<float>();
			set => Accessor.SetValue<float>( value );
		}
	}

	internal class ParamPropertyFloatRangeMin : ParamProperty<float>
	{
		public override string Name => "Range Min";
		public override string Description => PropertyDescription + "TODO : Remove this?";

		public ParamPropertyFloatRangeMin( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
		}
	
		public override float Value
		{
			get => Accessor.GetValueRangeMin<float>();
			set => Accessor.SetValueRangeMin<float>( value );
		}
	}

	internal class ParamPropertyFloatRangeMax : ParamProperty<float>
	{
		public override string Name => "Range Max";
		public override string Description => PropertyDescription + "TODO : Remove this?";

		public ParamPropertyFloatRangeMax( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
		}

		public override float Value
		{
			get => Accessor.GetValueRangeMax<float>();
			set => Accessor.SetValueRangeMax<float>( value );
		}
	}
}

//[Title( "Vector 2" )]
public class VariantValueVector2 : VariantValue<Vector2>
{
	[Hide, JsonIgnore]
	public override bool HasRange => true;

	public override Vector2 MinValue { get; set; } = Vector2.Zero;
	public override Vector2 MaxValue { get; set; } = Vector2.One;

	public VariantValueVector2( Vector2 value, Vector2 minValue, Vector2 maxValue, SubgraphInputType inputType ) : base( value, inputType)
	{
		MinValue = minValue;
		MaxValue = maxValue;
	}

	public VariantValueVector2() : base()
	{
	}

	internal class ParamPropertyVector2 : ParamProperty<Vector2>
	{
		public ParamPropertyVector2( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
			SetAttributes(
				new TitleAttribute( "" ),
				new InlineEditorAttribute() { Label = false },
				new DefaultValueAttribute( Parameter.DefaultValue )
			);
		}

		public override Vector2 Value
		{
			get => Accessor.GetValue<Vector2>();
			set => Accessor.SetValue<Vector2>( value );
		}
	}

	internal class ParamPropertyVector2RangeMin : ParamProperty<Vector2>
	{
		public override string Name => "Range Min";
		public override string Description => PropertyDescription + "TODO : Remove this?";

		public ParamPropertyVector2RangeMin( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
		}

		public override Vector2 Value
		{
			get => Accessor.GetValueRangeMin<Vector2>();
			set => Accessor.SetValueRangeMin<Vector2>( value );
		}
	}

	internal class ParamPropertyVector2RangeMax : ParamProperty<Vector2>
	{
		public override string Name => "Range Max";
		public override string Description => PropertyDescription + "TODO : Remove this?";

		public ParamPropertyVector2RangeMax( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
		}

		public override Vector2 Value
		{
			get => Accessor.GetValueRangeMax<Vector2>();
			set => Accessor.SetValueRangeMax<Vector2>( value );
		}
	}
}

//[Title( "Vector 3" )]
public class VariantValueVector3 : VariantValue<Vector3>
{
	[Hide, JsonIgnore]
	public override bool HasRange => true;

	public override Vector3 MinValue { get; set; } = Vector3.Zero;
	public override Vector3 MaxValue { get; set; } = Vector3.One;

	public VariantValueVector3( Vector3 value, Vector3 minValue, Vector3 maxValue, SubgraphInputType inputType ) : base( value, inputType )
	{
		MinValue = minValue;
		MaxValue = maxValue;
	}

	public VariantValueVector3() : base()
	{
	}

	internal class ParamPropertyVector3 : ParamProperty<Vector3>
	{
		public ParamPropertyVector3( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
			SetAttributes(
				new TitleAttribute( "" ),
				new InlineEditorAttribute() { Label = false },
				new DefaultValueAttribute( Parameter.DefaultValue )
			);
		}

		public override Vector3 Value
		{
			get => Accessor.GetValue<Vector3>();
			set => Accessor.SetValue<Vector3>( value );
		}
	}

	internal class ParamPropertyVector3RangeMin : ParamProperty<Vector3>
	{
		public override string Name => "Range Min";
		public override string Description => PropertyDescription + "TODO : Remove this?";

		public ParamPropertyVector3RangeMin( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
		}

		public override Vector3 Value
		{
			get => Accessor.GetValueRangeMin<Vector3>();
			set => Accessor.SetValueRangeMin<Vector3>( value );
		}
	}

	internal class ParamPropertyVector3RangeMax : ParamProperty<Vector3>
	{
		public override string Name => "Range Max";
		public override string Description => PropertyDescription + "TODO : Remove this?";

		public ParamPropertyVector3RangeMax( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
		}

		public override Vector3 Value
		{
			get => Accessor.GetValueRangeMax<Vector3>();
			set => Accessor.SetValueRangeMax<Vector3>( value );
		}
	}

}


//[Title( "Vector 4" )]
public class VariantValueVector4 : VariantValue<Vector4>
{
	[Hide, JsonIgnore]
	public override bool HasRange => true;

	public override Vector4 MinValue { get; set; } = Vector4.Zero;
	public override Vector4 MaxValue { get; set; } = Vector4.One;

	public VariantValueVector4( Vector4 value, Vector4 minValue, Vector4 maxValue, SubgraphInputType inputType ) : base( value, inputType )
	{
		MinValue = minValue;
		MaxValue = maxValue;
	}

	public VariantValueVector4() : base()
	{
	}

	internal class ParamPropertyVector4 : ParamProperty<Vector4>
	{
		public ParamPropertyVector4( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
			SetAttributes(
				new TitleAttribute( "" ),
				new InlineEditorAttribute() { Label = false },
				new DefaultValueAttribute( Parameter.DefaultValue )
			);
		}

		public override Vector4 Value
		{
			get => Accessor.GetValue<Vector4>();
			set => Accessor.SetValue<Vector4>( value );
		}
	}

	internal class ParamPropertyVector4RangeMin : ParamProperty<Vector4>
	{
		public override string Name => "Range Min";
		public override string Description => PropertyDescription + "TODO : Remove this?";

		public ParamPropertyVector4RangeMin( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
		}

		public override Vector4 Value
		{
			get => Accessor.GetValueRangeMin<Vector4>();
			set => Accessor.SetValueRangeMin<Vector4>( value );
		}
	}

	internal class ParamPropertyVector4RangeMax : ParamProperty<Vector4>
	{
		public override string Name => "Range Max";
		public override string Description => PropertyDescription + "TODO : Remove this?";

		public ParamPropertyVector4RangeMax( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
		}

		public override Vector4 Value
		{
			get => Accessor.GetValueRangeMax<Vector4>();
			set => Accessor.SetValueRangeMax<Vector4>( value );
		}
	}
}

//[Title( "Color" )]
public class VariantValueColor : VariantValue<Color>
{
	[Hide, JsonIgnore]
	public override bool HasRange => false;

	public override Color MinValue { get; set; } = Color.Black;
	public override Color MaxValue { get; set; } = Color.White;

	public VariantValueColor( Color value, Color minValue, Color maxValue, SubgraphInputType inputType ) : base( value, inputType )
	{
		MinValue = minValue; 
		MaxValue = maxValue;
	}

	public VariantValueColor( Color value, SubgraphInputType inputType ) : base( value, inputType )
	{

	}

	public VariantValueColor() : base()
	{
	}

	internal class ParamPropertyColor : ParamProperty<Color>
	{
		public ParamPropertyColor( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
			SetAttributes(
				new TitleAttribute( "" ),
				new InlineEditorAttribute() { Label = false },
				new DefaultValueAttribute( Parameter.DefaultValue )
			);
		}

		public override Color Value
		{
			get => Accessor.GetValue<Color>();
			set => Accessor.SetValue<Color>( value );
		}
	}

	internal class ParamPropertyColorRangeMin : ParamProperty<Color>
	{
		public override string Name => "Range Min";

		public ParamPropertyColorRangeMin( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
		}

		public override Color Value
		{
			get => Accessor.GetValueRangeMin<Color>();
			set => Accessor.SetValueRangeMin<Color>( value );
		}
	}

	internal class ParamPropertyColorRangeMax : ParamProperty<Color>
	{
		public override string Name => "Range Max";

		public ParamPropertyColorRangeMax( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
		}

		public override Color Value
		{
			get => Accessor.GetValueRangeMax<Color>();
			set => Accessor.SetValueRangeMax<Color>( value );
		}
	}
}

public class VariantValueSampler : VariantValue<Sampler>
{
	[Hide, JsonIgnore]
	public override Sampler MaxValue { get; set; }

	[Hide, JsonIgnore]
	public override Sampler MinValue { get; set; }

	public VariantValueSampler( Sampler value, SubgraphInputType inputType ) : base( value, inputType )
	{
	}

	public VariantValueSampler() : base()
	{

	}
	public VariantValueSampler( SubgraphInputType inputType ) : base( inputType )
	{
	}

	internal class ParamPropertySampler : ParamProperty<Sampler>
	{
		public ParamPropertySampler( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
			SetAttributes(
				new TitleAttribute( "" ),
				new InlineEditorAttribute() { Label = false },
				new DefaultValueAttribute( Parameter.DefaultValue )
			);
		}

		public override Sampler Value
		{
			get => Accessor.GetValue<Sampler>();
			set => Accessor.SetValue<Sampler>( value );
		}
	}
}

public class VariantValueTexture2D : VariantValue<TextureInput>
{
	[Hide, JsonIgnore]
	public override TextureInput MaxValue { get; set; }

	[Hide, JsonIgnore]
	public override TextureInput MinValue { get; set; }

	public VariantValueTexture2D( TextureInput value, SubgraphInputType inputType ) : base( value, inputType )
	{
	}

	public VariantValueTexture2D() : base()
	{
	}

	internal class ParamPropertyTexture2D : ParamProperty<TextureInput>
	{
		public ParamPropertyTexture2D( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
			SetAttributes(
				new TitleAttribute( "" ),
				new InlineEditorAttribute() { Label = false },
				new DefaultValueAttribute( Parameter.DefaultValue )
			);
		}

		public override TextureInput Value
		{
			get => Accessor.GetValue<TextureInput>();
			set => Accessor.SetValue<TextureInput>( value );
		}
	}
}
