using Editor;

namespace ShaderGraphPlus;

[System.AttributeUsage( AttributeTargets.Class )]
internal class SubgraphOnlyAttribute : Attribute
{
	public SubgraphOnlyAttribute()
	{
	}
}

public abstract class BaseNodePlus : INode, ISGPJsonUpgradeable
{
	public event Action Changed;

	/// <summary>
	/// Current version of this node. Used by ISGPJsonUpgradeable
	/// </summary>
	[Hide]
	public abstract int Version { get; }

	[Hide, Browsable( false )]
	public string Identifier { get; set; }

	[JsonIgnore, Hide, Browsable( false )]
	public virtual DisplayInfo DisplayInfo { get; }

	[JsonIgnore, Hide, Browsable( false )]
	public bool CanClone => true;

	[JsonIgnore, Hide, Browsable( false )]
	public virtual bool CanRemove => true;

	[JsonIgnore, Hide, Browsable( false )]
	public virtual bool CanPreview => true;

	[JsonIgnore, Hide, Browsable( false )]
	public virtual bool CanAddToGraph => true;

	[Hide, Browsable( false )]
	public Vector2 Position { get; set; }

	[JsonIgnore, Hide]
	public IGraph _graph;

	[JsonIgnore, Hide, Browsable( false )]
	public int PreviewID { get; set; }

	[JsonIgnore, Hide, Browsable( false )]
	public bool Processed { get; set; } = false;

	[JsonIgnore, Hide, Browsable( false )]
	public bool UpgradedToNewNode { get; set; } = false;

	[Browsable( false )]
	[JsonIgnore, Hide]
	public IGraph Graph
	{
		get => _graph;
		set
		{
			_graph = value;
			FilterInputsAndOutputs();
		}
	}

	[JsonIgnore, Hide, Browsable( false )]
	public Vector2 ExpandSize { get; set; }

	[JsonIgnore, Hide, Browsable( false )]
	public bool AutoSize => false;

	[JsonIgnore, Hide, Browsable( false )]
	public virtual IEnumerable<IPlugIn> Inputs { get; protected set; }

	[JsonIgnore, Hide, Browsable( false )]
	public virtual IEnumerable<IPlugOut> Outputs { get; protected set; }

	[JsonIgnore, Hide, Browsable( false )]
	public string ErrorMessage => null;

	[JsonIgnore, Hide, Browsable( false )]
	public bool IsReachable => true;

	[Hide, Browsable( false )]
	public Dictionary<string, float> HandleOffsets { get; set; } = new();
	
	// Here so I can debug Switch Info without having to sort through the console.
	[JsonIgnore, Hide, Browsable( false )]
	public GraphCompiler.ComboSwitchInfo ComboSwitchInfo { get; set; } = new();

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

	public virtual void OnFrame()
	{

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

	public Color GetPrimaryColor( GraphView view )
	{
		return PrimaryColor;
	}

	public virtual Menu CreateContextMenu( NodeUI node )
	{
		return null;
	}

	public virtual void DebugInfo( Menu menu )
	{
		var debugInfoHeading = menu.AddHeading( "Node Debug Info" );

		var nodeIDLabel = menu.AddWidget( new Label( $"Node ID : {this.Identifier}" ) );
		var previewIDLabel = menu.AddWidget( new Label( $"Preview ID : {this.PreviewID}" ) );
		var isReachableLabel = menu.AddWidget( new Label( $"IsReachable? : {this.IsReachable}" ) );
		var canPreviewLabel = menu.AddWidget( new Label( $"CanPreview? : {this.CanPreview}" ) );

		if ( this.ComboSwitchInfo.IsValid )
		{
			var comboSwitchDataHeading = menu.AddHeading( "Combo Switch Data" );

			var boundSwitchLabel = menu.AddWidget( new Label( $" BoundSwitch : {this.ComboSwitchInfo.BoundSwitch}" ) );
			var boundSwitchLabelBlock = menu.AddWidget( new Label( $" BoundSwitchBlock : {this.ComboSwitchInfo.BoundSwitchBlock}" ) );
		}
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

	[System.AttributeUsage(AttributeTargets.Property)]
	public class InputDefaultAttribute : Attribute
	{
		public string Input;

		public InputDefaultAttribute(string input)
		{
			Input = input;
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

	[System.AttributeUsage(AttributeTargets.Property)]
	public class RangeAttribute : Attribute
	{
		public string Min;
		public string Max;
		public string Step;

		public RangeAttribute(string min, string max, string step)
		{
			Min = min;
			Max = max;
			Step = step;
		}
	}

	public static (IEnumerable<IPlugIn> Inputs, IEnumerable<IPlugOut> Outputs) GetPlugs( BaseNodePlus node )
	{
		var type = node.GetType();

		var inputs = new List<BasePlugIn>();
		var outputs = new List<BasePlugOut>();

		foreach ( var propertyInfo in type.GetProperties() )
		{
			if ( propertyInfo.GetCustomAttribute<InputAttribute>() is { } inputAttrib )
			{
				inputs.Add( new BasePlugIn( node, new( propertyInfo ), inputAttrib.Type ?? typeof( object ) ) );
			}

			if ( propertyInfo.GetCustomAttribute<OutputAttribute>() is { } outputAttrib )
			{
				outputs.Add( new BasePlugOut( node, new( propertyInfo ), outputAttrib.Type ?? typeof( object ) ) );
			}
		}

		return (inputs, outputs);
	}

	private void FilterInputsAndOutputs()
	{
		if ( _graph is not null )
		{
			if ( Graph is ShaderGraphPlus sg && !sg.IsSubgraph && this is IParameterNode pn )
			{
				Inputs = new List<IPlugIn>();
			}
		}
	}

}

public record BasePlug( BaseNodePlus Node, PlugInfo Info, Type Type ) : IPlug
{
	INode IPlug.Node => Node;

	public string Identifier => Info.Name;
	public DisplayInfo DisplayInfo => Info.DisplayInfo;

	public ValueEditor CreateEditor(NodeUI node, Plug plug)
	{
		var editor = Info.CreateEditor( node, plug, Type );
		if ( editor is not null ) return editor;

		// Default
		{
			var defaultEditor = new DefaultEditor( plug );
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
	public bool ShowConnection => IsReachable;
	public bool InTitleBar => false;

	public bool IsReachable
	{
		get
		{
			var conditional = Info.Property?.GetCustomAttribute<ConditionalVisibilityAttribute>();
			if ( conditional is not null )
			{
				if ( conditional.TestCondition( Node.GetSerialized() ) ) return false;
			}

			return true;
		}
	}

	public string ErrorMessage => null;

	public override string ToString()
	{
		return $"{Node.Identifier}.{Identifier}";
	}

}

public record BasePlugIn( BaseNodePlus Node, PlugInfo Info, Type Type ) : BasePlug( Node, Info, Type ), IPlugIn
{
	IPlugOut IPlugIn.ConnectedOutput
	{
		get
		{
			if ( Info.Property is null )
			{
				return Info.ConnectedPlug;
			}

			if ( Info.Type != typeof( NodeInput ) )
			{
				return null;
			}

			var value = Info.GetInput( Node );

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
			var property = Info.Property;
			if ( property is null )
			{
				Info.ConnectedPlug = value;
				return;
			}

			if ( property.PropertyType != typeof( NodeInput ) )
			{
				return;
			}

			if ( value is null )
			{
				property.SetValue( Node, default( NodeInput ) );
				return;
			}

			if ( value is not BasePlug fromPlug )
			{
				return;
			}

			property.SetValue( Node, new NodeInput
			{
				Identifier = fromPlug.Node.Identifier,
				Output = fromPlug.Identifier
			} );
		}
	}

	public float? GetHandleOffset( string name )
	{
		if ( Node.HandleOffsets.TryGetValue( name, out var value ) )
		{
			return value;
		}
		return null;
	}

	public void SetHandleOffset( string name, float? value )
	{
		if ( value is null ) Node.HandleOffsets.Remove( name );
		else Node.HandleOffsets[name] = value.Value;
	}
}

public record BasePlugOut( BaseNodePlus Node, PlugInfo Info, Type Type ) : BasePlug( Node, Info, Type ), IPlugOut;

public class PlugInfo
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public Type Type { get; set; }
	public DisplayInfo DisplayInfo { get; set; }
	public PropertyInfo Property { get; set; } = null;
	public IPlugOut ConnectedPlug { get; set; } = null;

	public PlugInfo()
	{
		DisplayInfo = new();
	}
	public PlugInfo( PropertyInfo property )
	{
		Name = property.Name;
		Type = property.PropertyType;
		var info = DisplayInfo.ForMember( Type );
		info.Name = property.Name;
		var titleAttr = property.GetCustomAttribute<TitleAttribute>();
		if ( titleAttr is not null )
		{
			info.Name = titleAttr.Value;
		}
		DisplayInfo = info;
		Property = property;
	}

	public NodeInput GetInput( BaseNodePlus node )
	{
		if ( Property is not null )
		{
			return (NodeInput)Property.GetValue( node )!;
		}

		return default;
	}

	public ValueEditor CreateEditor( NodeUI node, Plug plug, Type type )
	{
		if ( Property is null )
		{
			// TODO 
			//if ( plug is PlugIn plugIn && node.Node is SubgraphNode subgraphNode )
			//{
			//	var entry = subgraphNode.InputReferences[plugIn.Inner];
			//	var parameterNode = entry.paramNode;
			//	var parameterType = entry.paramNodeValueType;
			//	if ( parameterNode.UI.Type == UIType.Default ) return null;
			//
			//	if ( parameterType == typeof( float ) )
			//	{
			//		var slider = new FloatEditor( plug ) { Title = DisplayInfo.Name, Node = node };
			//		slider.Bind( "Value" ).From( parameterNode, "Value" );
			//
			//		var rangeMin = parameterNode.GetRangeMin();
			//		var rangeMax = parameterNode.GetRangeMax();
			//		var rangeStep = parameterNode.UI.Step;
			//
			//		slider.Bind( "Min" ).FromObject( rangeMin.x );
			//		slider.Bind( "Max" ).FromObject( rangeMax.x );
			//		slider.Bind( "Step" ).FromObject( rangeStep );
			//
			//		return slider;
			//	}
			//	else if ( parameterType == typeof( Color ) )
			//	{
			//		var slider = new ColorEditorPlus( plug ) { Title = DisplayInfo.Name, Node = node };
			//		slider.BindToParameter( subgraphNode, plug.Inner.Identifier );
			//
			//		return slider;
			//	}
			//}

			return null;
		}

		var editor = Property?.GetCustomAttribute<BaseNodePlus.EditorAttribute>();

		if ( editor is not null )
		{
			if ( type == typeof( float ) )
			{
				var slider = new FloatEditor( plug ) { Title = DisplayInfo.Name, Node = node };
				slider.Bind( "Value" ).From( node.Node, editor.ValueName );

				var range = Property.GetCustomAttribute<BaseNodePlus.RangeAttribute>();
				if ( range != null )
				{
					slider.Bind( "Min" ).From( node.Node, range.Min );
					slider.Bind( "Max" ).From( node.Node, range.Max );
					slider.Bind( "Step" ).From( node.Node, range.Step );
				}
				else if ( Property.GetCustomAttribute<MinMaxAttribute>() is MinMaxAttribute minMax )
				{
					slider.Min = minMax.MinValue;
					slider.Max = minMax.MaxValue;
				}

				return slider;
			}

			if ( type == typeof( Color ) )
			{
				var slider = new ColorEditorPlus( plug ) { Title = DisplayInfo.Name, Node = node };
				slider.Bind( "Value" ).From( node.Node, editor.ValueName );

				return slider;
			}
		}
		return null;
	}
}
