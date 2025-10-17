namespace ShaderGraphPlus;

/// <summary>
/// Base class of VariantProperty T
/// </summary>
[JsonConverter( typeof( VariantValueConverter ) )]
[JsonDerivedType( typeof( VariantValueBool ) ) ]
[JsonDerivedType( typeof( VariantValueInt ) )]
[JsonDerivedType( typeof( VariantValueFloat ) )]
[JsonDerivedType( typeof( VariantValueVector2 ) )]
[JsonDerivedType( typeof( VariantValueVector3 ) )]
[JsonDerivedType( typeof( VariantValueVector4 ) )]
[JsonDerivedType( typeof( VariantValueColor ) )]
[JsonDerivedType( typeof( VariantValueTexture2D ) )]
[JsonDerivedType( typeof( VariantValueTextureCube ) )]
[JsonDerivedType( typeof( VariantValueSamplerState ) )]
public abstract class VariantValueBase
{
	public virtual SubgraphPortType InputType { get; set; }

	public VariantValueBase()
	{
	}

	public VariantValueBase( SubgraphPortType inputType )
	{
		InputType = inputType;
	}

	public static VariantValueBase InitilizeInstance( object typeInstance, SubgraphPortType inputType, bool istextureCubeType = false )
	{
		return typeInstance switch
		{
			bool => new VariantValueBool( (bool)typeInstance, SubgraphPortType.Bool),
			int => new VariantValueInt( (int)typeInstance, SubgraphPortType.Int ),
			float => new VariantValueFloat( (float)typeInstance, SubgraphPortType.Float ),
			Vector2 => new VariantValueVector2( (Vector2)typeInstance, SubgraphPortType.Vector2 ),
			Vector3 => new VariantValueVector3( (Vector3)typeInstance, SubgraphPortType.Vector3 ),
			Vector4 => new VariantValueVector4( (Vector4)typeInstance, SubgraphPortType.Vector4 ),
			Color => new VariantValueColor( (Color)typeInstance, SubgraphPortType.Color ),
			TextureInput => !istextureCubeType ? new VariantValueTexture2D( ((TextureInput)typeInstance) with { Type = TextureType.Tex2D }, SubgraphPortType.Texture2DObject ) : new VariantValueTextureCube( ((TextureInput)typeInstance) with { Type = TextureType.TexCube }, SubgraphPortType.TextureCubeObject ),
			Sampler => new VariantValueSamplerState( (Sampler)typeInstance, SubgraphPortType.SamplerState ),
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
			VariantValueTextureCube v => v.Value,
			VariantValueSamplerState v => v.Value,
			_ => throw new NotImplementedException(),
		};
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
}

/// <summary>
/// Defines the actual value that is contained and the Accompanying ParamProperty
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class VariantValue<T> : VariantValueBase
{
	public T Value { get; set; }

	public VariantValue( T value ) : base( )
	{
		Value = value;
	}

	public VariantValue( T value, SubgraphPortType inputType ) : this( value )
	{
		InputType = inputType;
	}

	public VariantValue() : base()
	{
		Value = default;
	}

	public VariantValue( SubgraphPortType inputType ) : base( inputType )
	{
	}
}

//[Title( "Bool" )]
public class VariantValueBool : VariantValue<bool>
{
	public VariantValueBool( bool value, SubgraphPortType inputType ) : base( value, inputType )
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
			set => Accessor.SetValue( value );
		}
	}
}

//[Title( "Int" )]
public class VariantValueInt : VariantValue<int>
{
	public VariantValueInt( int value, SubgraphPortType inputType ) : base( value, inputType )
	{
	}

	public VariantValueInt() : base()
	{
	}

	internal class ParamPropertyInt : ParamProperty<int>
	{
		public ParamPropertyInt( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
			SetAttributes(
				new TitleAttribute( "" ),
				new InlineEditorAttribute() { Label = false },
				new DefaultValueAttribute( Parameter.DefaultValue )
			);
		}

		public override int Value
		{
			get => Accessor.GetValue<int>();
			set => Accessor.SetValue( value );
		}

	}
}

//[Title( "Float" )]
public class VariantValueFloat : VariantValue<float>
{
	public VariantValueFloat( float value, SubgraphPortType inputType ) : base( value, inputType )
	{
	}

	public VariantValueFloat() : base()
	{
	}

	internal class ParamPropertyFloat : ParamProperty<float>
	{
		public Vector2 Range { get; private init; }

		public ParamPropertyFloat( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
			SetAttributes(
				new TitleAttribute( "" ),
				new InlineEditorAttribute() { Label = false },
				new DefaultValueAttribute( Parameter.DefaultValue )
			);
		}

		public override float Value
		{
			get => Accessor.GetValue<float>();
			set => Accessor.SetValue( value );
		}
	}
}

//[Title( "Vector 2" )]
public class VariantValueVector2 : VariantValue<Vector2>
{
	public VariantValueVector2( Vector2 value, SubgraphPortType inputType ) : base( value, inputType )
	{
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
			set => Accessor.SetValue( value );
		}
	}
}

//[Title( "Vector 3" )]
public class VariantValueVector3 : VariantValue<Vector3>
{
	public VariantValueVector3( Vector3 value, SubgraphPortType inputType ) : base( value, inputType )
	{
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
			set => Accessor.SetValue( value );
		}
	}
}

//[Title( "Vector 4" )]
public class VariantValueVector4 : VariantValue<Vector4>
{
	public VariantValueVector4( Vector4 value, SubgraphPortType inputType ) : base( value, inputType )
	{
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
			set => Accessor.SetValue( value );
		}
	}
}

//[Title( "Color" )]
public class VariantValueColor : VariantValue<Color>
{
	public VariantValueColor( Color value, SubgraphPortType inputType ) : base( value, inputType )
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
			set => Accessor.SetValue( value );
		}
	}
}

public class VariantValueSamplerState : VariantValue<Sampler>
{
	public VariantValueSamplerState( Sampler value, SubgraphPortType inputType ) : base( value, inputType )
	{
	}

	public VariantValueSamplerState() : base()
	{

	}
	public VariantValueSamplerState( SubgraphPortType inputType ) : base( inputType )
	{
	}

	internal class ParamPropertySamplerState : ParamProperty<Sampler>
	{
		public ParamPropertySamplerState( VariantValueSerializedObject parent, string name ) : base( parent, name )
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
			set => Accessor.SetValue( value );
		}
	}
}

public class VariantValueTexture2D : VariantValue<TextureInput>
{
	public VariantValueTexture2D( TextureInput value, SubgraphPortType inputType ) : base( value, inputType )
	{
	}

	public VariantValueTexture2D() : base()
	{
		Value = Value with { Type = TextureType.Tex2D };
	}

	internal class ParamPropertyTexture2D : ParamProperty<TextureInput>
	{
		public ParamPropertyTexture2D( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
			SetAttributes(
				new TitleAttribute( "" ),
				new InlineEditorAttribute() { Label = false },
				new DefaultValueAttribute( Parameter.DefaultValue with { Type = TextureType.Tex2D } )
			);
		}

		public override TextureInput Value
		{
			get => Accessor.GetValue<TextureInput>();
			set => Accessor.SetValue( value );
		}
	}
}

public class VariantValueTextureCube : VariantValue<TextureInput>
{
	public VariantValueTextureCube( TextureInput value, SubgraphPortType inputType ) : base( value, inputType )
	{
	}

	public VariantValueTextureCube() : base()
	{
		Value = Value with { Type = TextureType.TexCube };
	}

	internal class ParamPropertyTextureCube : ParamProperty<TextureInput>
	{
		public ParamPropertyTextureCube( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
			SetAttributes(
				new TitleAttribute( "" ),
				new InlineEditorAttribute() { Label = false },
				new DefaultValueAttribute( Parameter.DefaultValue with { Type = TextureType.TexCube } )
			);
		}

		public override TextureInput Value
		{
			get => Accessor.GetValue<TextureInput>();
			set => Accessor.SetValue( value );
		}
	}
}
