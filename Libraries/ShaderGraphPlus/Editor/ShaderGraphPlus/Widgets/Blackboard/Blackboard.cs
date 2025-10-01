using Editor;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

internal class Blackboard : Widget
{
	private ShaderGraphPlus _graph;
	public ShaderGraphPlus Graph
	{
		get => _graph;
		set
		{
			if ( value == null ) return;
			if ( _graph == value ) return;

			_graph = value;

			_blackboardView.Graph = value;
		}
	}

	public Action OnDirty { get; set; }
	public Action<BaseBlackboardParameter> OnParameterChanged { get; set; }

	private BlackboardView _blackboardView;

	public Blackboard( Widget parent ) : base( parent )
	{
		Name = "Blackboard";
		WindowTitle = "Blackboard";
		SetWindowIcon( "list" );

		Layout = Layout.Row();
		Layout.Spacing = 8;
		Layout.Margin = 4;

		_blackboardView = new BlackboardView( this );
		_blackboardView.OnDirty += () =>
		{
			OnDirty?.Invoke();
		};
		_blackboardView.OnParameterChanged += ( p ) =>
		{
			OnParameterChanged?.Invoke( p );
		};

		Layout.Add( _blackboardView );
	}

	public void UpdateBlackboard( bool preserveCurrentSelection = false )
	{
		_blackboardView?.RebuildBuildFromParameters( preserveCurrentSelection );
	}
}

internal class BlackboardUtils
{
	internal static BaseBlackboardParameter CreateBaseBlackboardParameterInstance( Type targetType, int id, string name = "" )
	{
		var typeInstance = EditorTypeLibrary.Create( targetType.Name, targetType );

		return typeInstance switch
		{
			BoolBlackboardParameter => new BoolBlackboardParameter( false ) { Name = name },
			IntBlackboardParameter => new IntBlackboardParameter( 0 ) { Name = name },
			FloatBlackboardParameter => new FloatBlackboardParameter( 0.0f ) { Name = name },
			Float2BlackboardParameter => new Float2BlackboardParameter( Vector2.Zero) { Name = name },
			Float3BlackboardParameter => new Float3BlackboardParameter( Vector3.Zero) { Name = name },
			Float4BlackboardParameter => new Float4BlackboardParameter( Color.White ) { Name = name },
			ShaderFeatureBooleanBlackboardParameter => new ShaderFeatureBooleanBlackboardParameter( new() ) { Name = name },
			ShaderFeatureEnumBlackboardParameter => new ShaderFeatureEnumBlackboardParameter( new() ) { Name = name },
			_ => throw new NotImplementedException(),
		};
	}
}
