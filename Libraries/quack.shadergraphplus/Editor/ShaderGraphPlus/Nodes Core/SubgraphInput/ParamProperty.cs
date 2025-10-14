namespace ShaderGraphPlus;

internal abstract class ParamProperty : SerializedProperty
{
	public static ParamProperty Create( VariantValueSerializedObject parent )
	{
		var variantProperty = parent.Node.InputData;

		SGPLog.Info( $"Creating ParamProperty of \"{variantProperty.GetType()}\"" );

		return variantProperty switch
		{
			VariantValueBool => new VariantValueBool.ParamPropertyBool( parent, "Default Bool" ),
			VariantValueInt => new VariantValueInt.ParamPropertyInt( parent, "Default Int" ),
			VariantValueFloat => new VariantValueFloat.ParamPropertyFloat( parent, "Default Float" ),
			VariantValueVector2 => new VariantValueVector2.ParamPropertyVector2( parent, "Default Vector2" ),
			VariantValueVector3 => new VariantValueVector3.ParamPropertyVector3( parent, "Default Vector3" ),
			VariantValueVector4 => new VariantValueVector4.ParamPropertyVector4( parent, "Default Vector4" ),
			VariantValueColor => new VariantValueColor.ParamPropertyColor( parent, "Default Color" ),
			VariantValueTexture2D => new VariantValueTexture2D.ParamPropertyTexture2D( parent, "Default Texture2D" ),
			VariantValueTextureCube => new VariantValueTextureCube.ParamPropertyTextureCube( parent, "Default TextureCube" ),
			VariantValueSamplerState => new VariantValueSamplerState.ParamPropertySamplerState( parent, "Default Sampler State" ),
			_ => null,
		};
	}

	private VariantValueSerializedObject _parent;

	public override SerializedObject Parent => _parent;
	public override Type PropertyType { get; }

	protected VariantValueWidget.IAccessor Accessor => _parent.Accessor;

	public ParamProperty( VariantValueSerializedObject parent, string name, Type type )
	{
		_parent = parent;
		PropertyType = type;
	}

	public bool Enabled
	{
		get => true;
	}
}

internal abstract class ParamProperty<TValue> : ParamProperty
{
	private IReadOnlyList<Attribute> _attributes;

	protected VariantParam<TValue> Parameter { get; }

	public abstract TValue Value { get; set; }

	public override string DisplayName => Name;
	public override string Name => "Value";

	public virtual bool HasMinMax { get; } = false;

	public override string Description => PropertyDescription;
	public string PropertyDescription { get; }

	protected ParamProperty( VariantValueSerializedObject parent, string name )
	: base( parent, name, typeof( TValue ) )
	{
		Parameter = parent.Node.GetValueAsVariantParam<TValue>();
		PropertyDescription = Parameter.Description;
	}

	protected void SetAttributes( params Attribute[] attributes )
	{
		_attributes = attributes;
	}

	public override IEnumerable<Attribute> GetAttributes() => _attributes ?? Enumerable.Empty<Attribute>();

	public override T GetValue<T>( T defaultValue = default )
	{
		return ValueToType( Value, defaultValue );
	}

	public override void SetValue<T>( T value )
	{
		if ( value is null )
		{
			SetValue( GetDefault() );

			return;
		}

		Value = ValueToType( value, Value );
	}

	public override bool TryGetAsObject( out SerializedObject obj )
	{
		var typeDesc = EditorTypeLibrary.GetType<TValue>();

		obj = EditorTypeLibrary.GetSerializedObject( () => Value, typeDesc, this );
		return true;
	}
}
