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

	public VariantValueBase()
	{
	}

	public VariantValueBase( SubgraphInputType inputType )
	{
		InputType = inputType;
	}

	public static VariantValueBase CreateNew( object typeInstance, SubgraphInputType inputType )
	{
		return typeInstance switch
		{
			bool => new VariantValueBool( (bool)typeInstance, SubgraphInputType.Bool),
			float => new VariantValueFloat( (float)typeInstance, SubgraphInputType.Float ),
			Vector2 => new VariantValueVector2( (Vector2)typeInstance, SubgraphInputType.Vector2 ),
			Vector3 => new VariantValueVector3( (Vector3)typeInstance, SubgraphInputType.Vector3 ),
			Vector4 => new VariantValueVector4( (Vector4)typeInstance, SubgraphInputType.Color ),
			Color => new VariantValueColor( (Color)typeInstance, SubgraphInputType.Color ),
			Sampler => new VariantValueSampler( (Sampler)typeInstance, SubgraphInputType.Sampler ),
			TextureInput => new VariantValueTexture2D( (TextureInput)typeInstance, SubgraphInputType.Texture2DObject ),
			_ => throw new NotImplementedException( $"Unknown object of type \"{typeInstance}\"" ),
		};
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

	internal class ParamProperty : ParamProperty<T>
	{
		public ParamProperty( VariantValueSerializedObject parent, string name ) : base( parent, name )
		{
			//if ( typeof( T ) == typeof( Sampler ) )
			{
				SetAttributes(
					new TitleAttribute(""),
					new InlineEditorAttribute() { Label = false }
				);
			}
		}

		public override T Value
		{
			get => Accessor.GetValue<T>();
			set => Accessor.SetValue<T>( value );
		}
	}
}

//[Title( "Bool" )]
public class VariantValueBool : VariantValue<bool>
{
	public VariantValueBool( bool value, SubgraphInputType inputType ) : base( value, inputType )
	{
	}

	public VariantValueBool() : base()
	{
	}
}

//[Title( "Int" )]
public class VariantValueInt : VariantValue<int>
{
	public VariantValueInt( int value, SubgraphInputType inputType ) : base( value, inputType )
	{
	}

	public VariantValueInt() : base()
	{
	}
}

//[Title( "Float" )]
public class VariantValueFloat : VariantValue<float>
{
	public VariantValueFloat( float value, SubgraphInputType inputType ) : base( value , inputType )
	{
	}

	public VariantValueFloat() : base()
	{
	}
}

//[Title( "Vector 2" )]
public class VariantValueVector2 : VariantValue<Vector2>
{
	public VariantValueVector2( Vector2 value, SubgraphInputType inputType ) : base( value, inputType)
	{
	}

	public VariantValueVector2() : base()
	{
	}
}

//[Title( "Vector 3" )]
public class VariantValueVector3 : VariantValue<Vector3>
{
	public VariantValueVector3( Vector3 value, SubgraphInputType inputType ) : base( value, inputType )
	{
	}

	public VariantValueVector3() : base()
	{
	}
}


//[Title( "Vector 4" )]
public class VariantValueVector4 : VariantValue<Vector4>
{
	public VariantValueVector4( Vector4 value, SubgraphInputType inputType ) : base( value, inputType )
	{
	}

	public VariantValueVector4() : base()
	{
	}
}

//[Title( "Color" )]
public class VariantValueColor : VariantValue<Color>
{
	public VariantValueColor( Color value, SubgraphInputType inputType ) : base( value, inputType )
	{
	}

	public VariantValueColor() : base()
	{
	}
}

public class VariantValueSampler : VariantValue<Sampler>
{
	public VariantValueSampler( Sampler value, SubgraphInputType inputType ) : base( value, inputType )
	{
	}

	public VariantValueSampler() : base()
	{

	}
	public VariantValueSampler( SubgraphInputType inputType ) : base( inputType )
	{
	}
}

public class VariantValueTexture2D : VariantValue<TextureInput>
{
	public VariantValueTexture2D( TextureInput value, SubgraphInputType inputType ) : base( value, inputType )
	{
	}

	public VariantValueTexture2D() : base()
	{
	}
}
