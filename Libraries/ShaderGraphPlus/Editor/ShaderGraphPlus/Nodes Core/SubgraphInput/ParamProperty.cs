namespace ShaderGraphPlus;

internal abstract class ParamProperty : SerializedProperty
{
	public static ParamProperty Create( VariantValueSerializedObject parent )
	{
		var variantProperty = parent.Node.DefaultValue;
		//var type = parent.Node.PortType;

		SGPLog.Info( $"Creating ParamProperty of \"{variantProperty.GetType()}\"" );

		return variantProperty switch
		{
			VariantValueBool => new VariantValueBool.ParamProperty( parent, "Default Bool" ),
			VariantValueInt => new VariantValueInt.ParamProperty( parent, "Default Int" ),
			VariantValueFloat => new VariantValueFloat.ParamProperty( parent, "Default Float" ),
			VariantValueVector2 => new VariantValueVector2.ParamProperty( parent, "Default Vector2" ),
			VariantValueVector3 => new VariantValueVector3.ParamProperty( parent, "Default Vector3" ),
			VariantValueVector4 => new VariantValueVector4.ParamProperty( parent, "Default Vector4" ),
			VariantValueColor => new VariantValueColor.ParamProperty( parent, "Default Color" ),
			VariantValueSampler => new VariantValueSampler.ParamProperty( parent, "Default Sampler" ),
			VariantValueTexture2D => new VariantValueTexture2D.ParamProperty( parent, "Default Texture2D" ),
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
	public override string Name { get; }

	public override string Description => PropertyDescription;
	public string PropertyDescription { get; }

	protected ParamProperty( VariantValueSerializedObject parent, string name )
	: base( parent, name, typeof( TValue ) )
	{
		Parameter = parent.Node.GetValueAsVariantParam<TValue>();
		Name = Parameter.Name;
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
