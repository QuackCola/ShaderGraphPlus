using System.Reflection.Emit;
using static Sandbox.Gizmo;

namespace Editor.ShaderGraphPlus;

public abstract class BaseNodePlus : INode
{
	public event Action Changed;

	[Hide, Browsable( false )]
	public string Identifier { get; set; }

	[JsonIgnore, Hide, Browsable( false )]
	public virtual DisplayInfo DisplayInfo { get; }

	[JsonIgnore, Hide, Browsable( false )]
	public bool CanClone => true;

	[JsonIgnore, Hide, Browsable( false )]
	public bool CanRemove => true;

	[Hide, Browsable( false )]
	public Vector2 Position { get; set; }

	[Browsable( false )]
	[JsonIgnore, Hide]
	public IGraph Graph { get; set; }

	[JsonIgnore, Hide, Browsable( false )]
	public Vector2 ExpandSize { get; set; }

	[JsonIgnore, Hide, Browsable( false )]
	public bool AutoSize => false;

	[JsonIgnore, Hide, Browsable( false )]
	public IEnumerable<IPlugIn> Inputs { get; private set; }

	[JsonIgnore, Hide, Browsable( false )]
	public IEnumerable<IPlugOut> Outputs { get; private set; }

	[JsonIgnore, Hide, Browsable( false )]
	public string ErrorMessage => null;

	[JsonIgnore, Hide, Browsable( false )]
	public bool IsReachable => true;

	public BaseNodePlus()
	{
		DisplayInfo = DisplayInfo.For( this );
		NewIdentifier();

		(Inputs, Outputs) = GetPlugs( this );
    }

	public void AddInputPortsTest(BaseNodePlus node, string inputName, ResultType intype)
	{
        var inputs = new List<BasePlugIn>();
        var type = intype switch
        {
            ResultType.Bool => typeof(Boolean),
            ResultType.Float => typeof(Float),
            ResultType.Vector2 => typeof(Vector2),
            ResultType.Vector3 => typeof(Vector3),
            ResultType.Color => typeof(Color),
            ResultType.Float2x2 => typeof(Float2x2),
            ResultType.Float3x3 => typeof(Float3x3),
            ResultType.Float4x4 => typeof(Float4x4),
            ResultType.Sampler => typeof(Sampler),
            ResultType.TextureObject => typeof(TextureObject),
            ResultType.String => typeof(string),
            ResultType.Gradient => typeof(Gradient),
            _ => throw new ArgumentException("Unsupported value type", nameof(intype))
        };

        var prop = CreatePropertyInfo(inputName, type);

        inputs.Add(new BasePlugIn(node, prop, type));

        Inputs = inputs;
    }

    public void AddInputs(BaseNodePlus node, Dictionary<string, ResultType> inputdict)
    {
        var inputs = new List<BasePlugIn>();

        // Build the inputs list
        foreach (var input in inputdict)
		{
            var type = input.Value switch
            {
                ResultType.Bool => typeof(Boolean),
                ResultType.Float => typeof(Float),
                ResultType.Vector2 => typeof(Vector2),
                ResultType.Vector3 => typeof(Vector3),
                ResultType.Color => typeof(Color),
                ResultType.Float2x2 => typeof(Float2x2),
                ResultType.Float3x3 => typeof(Float3x3),
                ResultType.Float4x4 => typeof(Float4x4),
                ResultType.Sampler => typeof(Sampler),
                ResultType.TextureObject => typeof(TextureObject),
                ResultType.String => typeof(string),
                ResultType.Gradient => typeof(Gradient),
                _ => throw new ArgumentException("Unsupported value type", nameof(input.Value))
            };

            // Generate the needed propertyInfo
            var prop = CreatePropertyInfo(input.Key, type);

            inputs.Add(new BasePlugIn(node, prop, type));
        }


        Inputs = inputs;
    }

	/// I have no fucking idea but it works? though there has to be better way. right..?
    public static PropertyInfo CreatePropertyInfo(string propertyName, Type propertyType)
    {
        // Create a dynamic assembly and module
        AssemblyName assemblyName = new AssemblyName("DynamicAssembly");
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

        // Define a new type to hold the property
        TypeBuilder typeBuilder = moduleBuilder.DefineType("DynamicType", TypeAttributes.Public);

        // Define the property
        FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
        PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

        // Define the 'get' accessor method for the property
        MethodBuilder getMethodBuilder = typeBuilder.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
        ILGenerator getIL = getMethodBuilder.GetILGenerator();
        getIL.Emit(OpCodes.Ldarg_0);
        getIL.Emit(OpCodes.Ldfld, fieldBuilder);
        getIL.Emit(OpCodes.Ret);
        propertyBuilder.SetGetMethod(getMethodBuilder);

        // Define the 'set' accessor method for the property
        MethodBuilder setMethodBuilder = typeBuilder.DefineMethod("set_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new Type[] { propertyType });
        ILGenerator setIL = setMethodBuilder.GetILGenerator();
        setIL.Emit(OpCodes.Ldarg_0);
        setIL.Emit(OpCodes.Ldarg_1);
        setIL.Emit(OpCodes.Stfld, fieldBuilder);
        setIL.Emit(OpCodes.Ret);
        propertyBuilder.SetSetMethod(setMethodBuilder);

        // Create the type and return the PropertyInfo object
        Type dynamicType = typeBuilder.CreateType();
        return dynamicType.GetProperty(propertyName);
    }

	public void Update()
    {
        Changed?.Invoke();
    }
    public string NewIdentifier()
    {
		Identifier = Guid.NewGuid().ToString();
		return Identifier;
	}

	public virtual NodeUI CreateUI( GraphView view )
	{
		return new NodeUI( view, this );
	}

    public Color GetPrimaryColor(GraphView view)
    {
        return PrimaryColor;
    }

    public virtual Menu CreateContextMenu( NodeUI node )
	{
		return null;
	}

	[JsonIgnore, Hide, Browsable( false )]
	public virtual Pixmap Thumbnail { get; }

	[JsonIgnore, Hide, Browsable( false )]
	public virtual Color PrimaryColor { get; } = Color.Lerp( new Color( 0.7f, 0.7f, 0.7f ), Theme.Blue, 0.1f );

	public virtual void OnPaint( Rect rect )
	{

	}

	public virtual void OnDoubleClick( MouseEvent e )
	{

	}

	[JsonIgnore, Hide, Browsable( false )]
	public bool HasTitleBar => true;

	[System.AttributeUsage( AttributeTargets.Property )]
	public class InputAttribute : Attribute
	{
		public System.Type Type;

		public InputAttribute( Type type = null )
		{
			Type = type;
		}
	}

	[System.AttributeUsage( AttributeTargets.Property )]
	public class OutputAttribute : Attribute
	{
		public System.Type Type;

		public OutputAttribute( Type type = null )
		{
			Type = type;
		}
	}

	[System.AttributeUsage( AttributeTargets.Property )]
	public class HideOutputAttribute : Attribute
	{
		public System.Type Type;

		public HideOutputAttribute( Type type = null )
		{
			Type = type;
		}
	}

	[System.AttributeUsage( AttributeTargets.Property )]
	public class EditorAttribute : Attribute
	{
		public string ValueName;

		public EditorAttribute( string valueName )
		{
			ValueName = valueName;
		}
	}

	[System.AttributeUsage( AttributeTargets.Property )]
	public class RangeAttribute : Attribute
	{
		public string Min;
		public string Max;

		public RangeAttribute( string min, string max )
		{
			Min = min;
			Max = max;
		}
	}




	public static (IEnumerable<IPlugIn> Inputs, IEnumerable<IPlugOut> Outputs) GetPlugs( BaseNodePlus node )
	{
		var type = node.GetType();

		var inputs = new List<BasePlugIn>();
		var outputs = new List<BasePlugOut>();

		//inputs.Exists( < BasePlug >, "");

		foreach ( var propertyInfo in type.GetProperties() )
		{
			if ( propertyInfo.GetCustomAttribute<InputAttribute>() is { } inputAttrib )
			{
				inputs.Add( new BasePlugIn( node, propertyInfo, inputAttrib.Type ?? typeof( object ) ) );
			}

			if ( propertyInfo.GetCustomAttribute<OutputAttribute>() is { } outputAttrib )
			{
				outputs.Add( new BasePlugOut( node, propertyInfo, outputAttrib.Type ?? typeof( object ) ) );
			}
		}

		return (inputs, outputs);
	}
}

public record BasePlug( BaseNodePlus Node, PropertyInfo Property, Type Type ) : IPlug
{
	INode IPlug.Node => Node;

	public string Identifier => Property.Name;
	public DisplayInfo DisplayInfo => DisplayInfo.ForMember( Property );

	public ValueEditor CreateEditor( NodeUI node, Plug plug )
	{
		var editor = Property.GetCustomAttribute<BaseNodePlus.EditorAttribute>();
		if ( editor == null )
			return null;

		if ( Type == typeof( float ) )
		{
			var slider = new FloatEditor( plug ) { Title = DisplayInfo.Name, Node = node };
			slider.Bind( "Value" ).From( node.Node, editor.ValueName );

			var range =
				Property.GetCustomAttribute<BaseNodePlus.RangeAttribute>();
			if ( range != null )
			{
				slider.Bind( "Min" ).From( node.Node, range.Min );
				slider.Bind( "Max" ).From( node.Node, range.Max );
			}
			else if ( Property.GetCustomAttribute<MinMaxAttribute>() is MinMaxAttribute minMax )
			{
				slider.Min = minMax.MinValue;
				slider.Max = minMax.MaxValue;
			}

			return slider;
		}

			if ( Type == typeof( Color ) )
		{
			var slider = new ColorEditor( plug ) { Title = DisplayInfo.Name, Node = node };
			slider.Bind( "Value" ).From( node.Node, editor.ValueName );

			return slider;
		}

		return null;
	}

	public Menu CreateContextMenu( NodeUI node, Plug plug )
	{
		return null;
	}

	public void OnDoubleClick( NodeUI node, Plug plug, MouseEvent e )
	{

	}

	public bool ShowLabel => true;
	public bool AllowStretch => true;
	public bool ShowConnection => true;
	public bool InTitleBar => false;
	public bool IsReachable => true;

	public string ErrorMessage => null;

	public override string ToString()
	{
		return $"{Node.Identifier}.{Identifier}";
	}

}


public record BasePlugIn( BaseNodePlus Node, PropertyInfo Property, Type Type )
	: BasePlug( Node, Property, Type ), IPlugIn
{
	IPlugOut IPlugIn.ConnectedOutput
	{
		get
		{
			if (Property.PropertyType != typeof( NodeInput ))
			{
				return null;
			}

			var value = (NodeInput) Property.GetValue( Node )!;

			if (!value.IsValid)
			{
				return null;
			}

			var node = ((ShaderGraphPlus) Node.Graph).FindNode( value.Identifier );
			var output = node?.Outputs
				.FirstOrDefault( x => x.Identifier == value.Output );

			return output;
		}
		set
		{
			if (Property.PropertyType != typeof( NodeInput ))
			{
				return;
			}

			if (value is null)
			{
				Property.SetValue( Node, default( NodeInput ) );
				return;
			}

			if (value is not BasePlug fromPlug)
			{
				return;
			}

			Property.SetValue( Node, new NodeInput
			{
				Identifier = fromPlug.Node.Identifier,
				Output = fromPlug.Identifier
			} );
		}
	}

	public float? GetHandleOffset( string name )
	{
		return null;
	}

	public void SetHandleOffset( string name, float? value )
	{
		throw new NotImplementedException();
	}
}

public record BasePlugOut( BaseNodePlus Node, PropertyInfo Property, Type Type )
	: BasePlug( Node, Property, Type ), IPlugOut;
