namespace Editor.ShaderGraphPlus;

public class ShaderGraphPlusView : GraphView
{
	private readonly MainWindow _window;
	private readonly UndoStack _undoStack;

	protected override string ClipboardIdent => "shadergraphplus";

	protected override string ViewCookie => _window?.AssetPath;

	public new ShaderGraphPlus Graph
	{
		get => (ShaderGraphPlus)base.Graph;
		set => base.Graph = value;
	}

	private readonly Dictionary<string, INodeType> AvailableNodes = new( StringComparer.OrdinalIgnoreCase );

	public ShaderGraphPlusView( Widget parent, MainWindow window ) : base( parent )
	{
		_window = window;
		_undoStack = window.UndoStack;

		OnSelectionChanged += SelectionChanged;
	}

	protected override INodeType RerouteNodeType { get; } = new ClassNodeType( EditorTypeLibrary.GetType<ReroutePlus>() );
	protected override INodeType CommentNodeType { get; } = new ClassNodeType( EditorTypeLibrary.GetType<CommentNode>() );

	public void AddNodeType<T>()
		where T : BaseNodePlus
	{
		AddNodeType( EditorTypeLibrary.GetType<T>() );
	}

	public void AddNodeType( TypeDescription type )
	{
		var nodeType = new ClassNodeType( type );

		AvailableNodes.TryAdd( nodeType.Identifier, nodeType );
	}

	public INodeType FindNodeType( Type type )
	{
		return AvailableNodes.TryGetValue( type.FullName!, out var nodeType ) ? nodeType : null;
	}

	protected override INodeType NodeTypeFromDragEvent( DragEvent ev )
	{
		return AvailableNodes.TryGetValue( ev.Data.Text, out var type )
			? type
			: null;
	}

	protected override IEnumerable<INodeType> GetRelevantNodes()
	{
		return AvailableNodes.Values;
	}

	private Dictionary<Type, HandleConfig> HandleConfigs { get; } = new()
	{
		{ typeof(bool), new HandleConfig( "Bool", Color.Parse( "#ffc0cb" ).Value ) },
		{ typeof(float), new HandleConfig( "Float", Color.Parse( "#8ec07c" ).Value ) },
		{ typeof(Vector2), new HandleConfig( "Vector2", Color.Parse( "#ce67e0" ).Value ) },
		{ typeof(Vector3), new HandleConfig( "Vector3", Color.Parse( "#7177e1" ).Value ) },
		{ typeof(Vector4), new HandleConfig( "Vector4", Color.Parse( "#e0d867" ).Value ) },
		{ typeof(Float2x2), new HandleConfig( "Float2x2", Color.Parse( "#a3b3c9" ).Value ) },
		{ typeof(Float3x3), new HandleConfig( "Float3x3", Color.Parse( "#a3b3c9" ).Value ) },
		{ typeof(Float4x4), new HandleConfig( "Float4x4", Color.Parse( "#a3b3c9" ).Value ) },
		{ typeof(TextureObject), new HandleConfig( "Texture2D", Color.Parse( "#ffb3a7" ).Value ) },
        { typeof(Sampler), new HandleConfig( "Sampler", Color.Parse( "#dddddd" ).Value ) },
        { typeof(Gradient), new HandleConfig( "Gradient", Color.Parse( "#dddddd" ).Value ) },
        { typeof(Color), new HandleConfig( "Color", Color.Parse( "#c7ae32" ).Value ) },
	};

	protected override HandleConfig OnGetHandleConfig( Type type )
	{
		return HandleConfigs.TryGetValue( type, out var config ) ? config : base.OnGetHandleConfig( type );
	}

	public override void ChildValuesChanged( Widget source )
	{
		BindSystem.Flush();

		base.ChildValuesChanged( source );

		BindSystem.Flush();
	}

	public override void PushUndo( string name )
	{
		Log.Info( $"Push Undo ({name})" );
		_undoStack.PushUndo( name, Graph.SerializeNodes() );
		_window.OnUndoPushed();
	}

	public override void PushRedo()
	{
		Log.Info( "Push Redo" );
		_undoStack.PushRedo( Graph.SerializeNodes() );
		_window.SetDirty();
	}

	private void SelectionChanged()
	{
		var item = SelectedItems
			.OfType<NodeUI>()
			.OrderByDescending( n => n is CommentUI )
			.FirstOrDefault();

		if ( item is null )
		{
			_window.OnNodeSelected( null );
			return;
		}

		_window.OnNodeSelected( (BaseNodePlus) item.Node );
	}

	protected override void OnNodeCreated( INode node )
	{
		Log.Info( $"Node ({node.DisplayInfo.Name}) Created" );
	}
}
