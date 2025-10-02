using Editor;
using System.Collections.Immutable;
using static Sandbox.Services.Inventory;

namespace ShaderGraphPlus;

internal class BlackboardView : Widget
{
	private ControlSheet _controlSheet;
	private Button.Primary _addButton;
	private Button.Danger _deleteButton;
	private BlackboardParameterList _parameterListView;
	private Blackboard _parentBlackboard;
	private object _selectedItem;
	private Guid _selectedItemGuid;

	private ShaderGraphPlus _graph;
	public ShaderGraphPlus Graph
	{
		get => _graph;
		set
		{
			if ( value == null ) return;
			if ( _graph == value ) return;
			
			_graph = value;

			RebuildBuildFromParameters();
		}
	}

	public Action OnDirty { get; set; }

	/// <summary>
	/// Invoked when a blackboard parameter changes.
	/// </summary>
	public Action<BaseBlackboardParameter> OnParameterChanged { get; set; }

	/// <summary>
	/// Invoked when a blackboard parameter is deleated.
	/// </summary>
	public Action<BaseBlackboardParameter> OnParameterDeleated { get; set; }

	public BlackboardView( Blackboard parent ) : base( parent )
	{
		Layout = Layout.Row();
		FocusMode = FocusMode.TabOrClickOrWheel;

		_parentBlackboard = parent;

		var canvas = new Widget( null );
		canvas.Layout = Layout.Row();
		canvas.Layout.Spacing = 8;
		canvas.Layout.Spacing = 4;

		var leftColumn = canvas.Layout.AddColumn( 1, false );
		leftColumn.Spacing = 8;
		leftColumn.Spacing = 4;

		var leftColumnTopLayout = leftColumn.AddRow( 1, false );
		leftColumnTopLayout.Spacing = 8;
		leftColumnTopLayout.Spacing = 4;

		leftColumnTopLayout.AddStretchCell();

		_deleteButton = new Button.Danger( "Delete", "delete" );
		_deleteButton.Enabled = false;
		_deleteButton.ToolTip = $"Delete selected blackboard parameter";
		_deleteButton.Clicked += () =>
		{
			DeleteSelectedBlackboardParameter();
		};

		leftColumnTopLayout.Add( _deleteButton );

		_addButton = new Button.Primary( "Add", "new_label" );
		_addButton.Enabled = true;
		_addButton.ToolTip = $"Add new blackboard parameter";
		_addButton.Clicked += () =>
		{
			var popup = new PopupTypeSelector( this );
			popup.OnSelect += ( t ) =>
			{
				AddBlackboardParameter( t );
			};
			popup.OpenAtCursor();
		};

		leftColumnTopLayout.Add( _addButton );

		_parameterListView = leftColumn.Add( new BlackboardParameterList( null ), 1 );//new ListView();
		_parameterListView.ItemClicked = OnItemClicked;
		_parameterListView.ItemSelected = OnItemSelected;
		_parameterListView.ItemDrag = ( a ) =>
		{
			var parameter = a as BaseBlackboardParameter;

			var drag = new Drag( this );
			drag.Data.Object = parameter;
			drag.Execute();

			return true;
		};

		var rightColumn = canvas.Layout.AddColumn( 1, false );
		rightColumn.Spacing = 8;
		rightColumn.Spacing = 4;
		rightColumn.SizeConstraint = SizeConstraint.SetMaximumSize;

		_controlSheet = new ControlSheet();
		_controlSheet.SizeConstraint = SizeConstraint.SetMaximumSize;
		_controlSheet.SetColumnStretch( 2, 0 );
		_controlSheet.SetMinimumColumnWidth( 0, 400 );
		rightColumn.Add( _controlSheet );
		rightColumn.AddStretchCell();

		Layout.Add( canvas );
	}

	private void OnItemSelected( object item )
	{
		var parameter = item as BaseBlackboardParameter;
		
		//SGPLog.Info( $"Selected item : {variable}" );
		
		SetControlSheetTarget( parameter );
	}

	private void OnItemClicked( object item )
	{
		var parameter = item as BaseBlackboardParameter;

		//SGPLog.Info( $"Clicked item : {variable}" );

		SetSelectedItem( parameter );
	}

	public void RebuildBuildFromParameters( bool preserveCurrentSelection = false )
	{
		BuildFromParameters( _graph.Parameters, preserveCurrentSelection );
	}

	public void BuildFromParameters( IEnumerable<BaseBlackboardParameter> parameters, bool preserveCurrentSelection = false )
	{
		_parameterListView.SetItems( parameters.Cast<object>() );

		if ( !preserveCurrentSelection )
		{
			ClearSeletedItem();
		}

		// If we have nothing selected then set an initital selection.
		if ( _selectedItem == null )
		{
			var firstParameter = parameters.FirstOrDefault();
		
			if ( firstParameter != null )
			{
				SetSelectedItem( firstParameter );
		
				_deleteButton.Enabled = true;
			}
		}

		if ( preserveCurrentSelection )
		{
			var selection = _graph.GetBlackboardParameterByGuid( _selectedItemGuid );
		
			//SGPLog.Info( $"Preserving selected item : {selection}" );
		
			_parameterListView.SelectItem( selection );
		}
	}

	private void AddBlackboardParameter( TypeDescription typeDescription )
	{
		int id = _parentBlackboard.Graph._parameters.Count;
		string name = $"Parameter{id}";

		var parameterInstance = BlackboardUtils.CreateBaseBlackboardParameterInstance( typeDescription.TargetType, id, name );
		_parentBlackboard.Graph.AddBlackboardParameter( parameterInstance );

		OnDirty?.Invoke();

		RebuildBuildFromParameters();

		SetSelectedItem( parameterInstance );
	}

	private void DeleteSelectedBlackboardParameter()
	{
		var parameter = _selectedItem as BaseBlackboardParameter;

		if ( _selectedItem != null )
		{
			_selectedItem = null;

			OnParameterDeleated?.Invoke( parameter );

			//SGPLog.Info( $"Deleted selected parameter : {parameter}" );
		}

		if ( !_graph.Parameters.Any() )
		{
			_controlSheet.Clear( true );
			_deleteButton.Enabled = false;
		}

	}

	private void SetSelectedItem( BaseBlackboardParameter blackboardParameter )
	{
		_selectedItem = blackboardParameter;
		_selectedItemGuid = blackboardParameter.Identifier;

		_parameterListView.SelectItem( blackboardParameter );
		SetControlSheetTarget( blackboardParameter );

		_deleteButton.Enabled = true;
	}

	private void SetControlSheetTarget( BaseBlackboardParameter newTarget )
	{
		_controlSheet.Clear( true );

		var so = newTarget.GetSerialized();
		so.OnPropertyChanged += ( prop ) =>
		{
			OnParameterChanged?.Invoke( newTarget );
		};

		_controlSheet.AddObject( so );
	}

	private void ClearSeletedItem()
	{
		_selectedItem = null;
		_controlSheet.Clear( true );
		
		_deleteButton.Enabled = false;
	}
}
