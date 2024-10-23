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
	public IEnumerable<IPlug> Inputs { get; }

	[JsonIgnore, Hide, Browsable( false )]
	public IEnumerable<IPlug> Outputs { get; }

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




	public static (IEnumerable<IPlug> Inputs, IEnumerable<IPlug> Outputs) GetPlugs( BaseNodePlus node )
	{
		var type = node.GetType();

		var inputs = new List<BasePlug>();
		var outputs = new List<BasePlug>();

		//inputs.Exists( < BasePlug >, "");

		foreach ( var propertyInfo in type.GetProperties() )
		{
			if ( propertyInfo.GetCustomAttribute<InputAttribute>() is { } inputAttrib )
			{
				inputs.Add( new BasePlug( node, propertyInfo, inputAttrib.Type ?? typeof( object ) ) );
			}

			if ( propertyInfo.GetCustomAttribute<OutputAttribute>() is { } outputAttrib )
			{
				outputs.Add( new BasePlug( node, propertyInfo, outputAttrib.Type ?? typeof( object ) ) );
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

	IPlug IPlug.ConnectedOutput
	{
		get
		{
			if ( Property.PropertyType != typeof( NodeInput ) )
			{
				return null;
			}

			var value = (NodeInput)Property.GetValue( Node )!;

			if ( !value.IsValid )
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
			if ( Property.PropertyType != typeof( NodeInput ) )
			{
				return;
			}

			if ( value is null )
			{
				Property.SetValue( Node, default( NodeInput ) );
				return;
			}

			if ( value is not BasePlug fromPlug )
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

    public float? GetHandleOffset(string name)
    {
        return null;
    }

    public void SetHandleOffset(string name, float? value)
    {
        throw new NotImplementedException();
    }

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
