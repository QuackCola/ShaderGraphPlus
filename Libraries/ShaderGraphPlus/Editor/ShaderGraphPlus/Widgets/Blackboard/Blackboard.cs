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

	/// <summary>
	/// Invoked when a blackboard parameter is selected in the BlackboardView.
	/// </summary>
	public Action<BaseBlackboardParameter> OnParameterSelected { get; set; }

	/// <summary>
	/// Invoked when a blackboard parameter changes.
	/// </summary>
	public Action<BaseBlackboardParameter> OnParameterChanged { get; set; }

	/// <summary>
	/// Invoked when a blackboard parameter is deleated.
	/// </summary>
	public Action<BaseBlackboardParameter> OnParameterDeleated { get; set; }

	private BlackboardView _blackboardView;

	public Blackboard( Widget parent ) : base( parent )
	{
		Name = "Blackboard";
		WindowTitle = "Blackboard";
		SetWindowIcon( "list" );

		Layout = Layout.Row();
		Layout.Spacing = 8;
		Layout.Margin = 4;

		BuildUI();
	}

	public void BuildUI()
	{
		_blackboardView = new BlackboardView( this );
		_blackboardView.OnDirty += () =>
		{
			OnDirty?.Invoke();
		};
		_blackboardView.OnParameterSelected += ( p ) =>
		{
			SGPLog.Info( $"Slected Parameter : {p.Name}" );
			OnParameterSelected?.Invoke( p );
		};
		_blackboardView.OnParameterChanged += ( p ) =>
		{
			SGPLog.Info( $"Parameter : {p.Name} has changed" );
			OnParameterChanged?.Invoke( p );
		};
		_blackboardView.OnParameterDeleated += ( p ) =>
		{
			OnParameterDeleated?.Invoke( p );
		};

		Layout.Add( _blackboardView );
	}

	public void UpdateBlackboard( bool preserveCurrentSelection = false )
	{
		_blackboardView?.RebuildBuildFromParameters( preserveCurrentSelection );
	}

	public void ClearSeletedItem()
	{
		_blackboardView.ClearSeletedItem();
	}

	public void SetSelectedItem( BaseBlackboardParameter parameter )
	{
		_blackboardView.SetSelectedItem( parameter );
	}
}
