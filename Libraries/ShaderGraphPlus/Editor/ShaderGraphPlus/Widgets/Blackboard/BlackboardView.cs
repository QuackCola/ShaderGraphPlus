using Editor;

namespace ShaderGraphPlus;

internal class BlackboardView : Widget
{
	private ControlSheet _controlSheet;
	private Button.Primary _addButton;
	private Button.Danger _deleteButton;
	private ListView _parameterListView;
	private bool _graphInit;
	private object _selectedItem;

	private ShaderGraphPlus _graph;
	public ShaderGraphPlus Graph
	{
		get => _graph;
		set
		{
			if ( value == null )
				return;

			_graph = value;
		}
	}

	public Action OnDirty { get; set; }

	/// <summary>
	/// Invoked when a blackboard parameter changes.
	/// </summary>
	public Action<BaseBlackboardParameter> OnParameterChanged { get; set; }

	public BlackboardView( Widget parent ) : base( parent )
	{
		Layout = Layout.Row();

		FocusMode = FocusMode.TabOrClickOrWheel;

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
		_deleteButton.ToolTip = $"Delete selected item";
		_deleteButton.Clicked += () =>
		{
			if ( _selectedItem != null )
			{
				SGPLog.Info( $"Delete selected item {_selectedItem}" );
			}
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

		_parameterListView = leftColumn.Add( new ListView(), 1 );//new ListView();
		_parameterListView.Margin = 4;
		_parameterListView.ItemSize = new Vector2( 0, 24 );
		_parameterListView.ItemSpacing = 4;
		_parameterListView.OnPaintOverride = () =>
		{
			Paint.ClearPen();
			Paint.SetBrush( Theme.ControlBackground );
			Paint.DrawRect( _parameterListView.LocalRect, Theme.ControlRadius );

			return false;
		};
		//_parameterListView.ItemSelected = ( item ) =>
		//{
		//
		//};
		_parameterListView.ItemPaint = PaintItem;
		_parameterListView.ItemClicked = ClickItem;
		_parameterListView.ItemDrag = ( a ) =>
		{
			var parameter = a as BaseBlackboardParameter;

			var drag = new Drag( this );
			drag.Data.Object = parameter;
			drag.Execute();

			return true;
		};
		//leftColumn.AddStretchCell();

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

		_graphInit = true;
	}

	private void ClickItem( object item )
	{
		var variable = item as BaseBlackboardParameter;

		SetBlackboardParameterTarget( variable );
	}

	private void PaintItem( VirtualWidget item )
	{
		var variable = item.Object as BaseBlackboardParameter;
		var rect = item.Rect;
		var rowRect = rect.Grow( 0, 0, 0, 0 );

		var textColor = Theme.TextControl;
		var itemColor = Theme.ControlBackground;

		if ( item.Hovered )
		{
			textColor = Color.White;
			itemColor = Theme.Primary.Lighten( 0.1f ).Desaturate( 0.3f ).WithAlpha( 0.4f * 0.6f );
		}
		if ( item.Selected )
		{
			textColor = Theme.TextControl;
			itemColor = Theme.Primary;
		}

		Paint.ClearPen();
		Paint.SetBrush( itemColor );
		Paint.DrawRect( rect, 3f );

		var typeColor = ShaderGraphPlusTheme.GetBlackboardParameterTypeColor( variable );

		Paint.SetPen( typeColor.WithAlpha( 0.7f ) );
		Paint.DrawIcon( rect.Shrink( 4f ), "circle", 12f, TextFlag.LeftCenter );
		rect.Left += 24f;

		var variableName = variable.Name;

		Paint.SetPen( textColor.WithAlpha( 0.7f ) );
		Paint.SetBrush( textColor.WithAlpha( 0.7f ) );

		Paint.DrawText( rect.Shrink( 4, 0, 0, 0 ), $"{variableName}", TextFlag.Left | TextFlag.CenterVertically | TextFlag.SingleLine );
		Paint.DrawText( rect.Shrink( 0, 0, 4, 0 ), $"{DisplayInfo.ForType( variable.GetType() ).Name}", TextFlag.Right | TextFlag.CenterVertically | TextFlag.SingleLine );

		//Paint.SetPen( Color.Gray.WithAlpha( 0.77f ) );
		//Paint.SetBrush( Color.Gray.WithAlpha( 0.77f ) );
		//Paint.DrawRect( rowRect  );
	}

	public void UpdateParameterList()
	{
		UpdateParameterList( null );
	}

	private void UpdateParameterList( BaseBlackboardParameter baseBlackboardParameter )
	{
		_parameterListView.Clear();
		_controlSheet.Clear( true );

		foreach ( var parameter in Graph.Parameters )
		{
			if ( _graphInit )
			{
				_controlSheet.AddObject( parameter.GetSerialized() );
				_parameterListView.SelectItem( parameter );
				_selectedItem = parameter;
				_deleteButton.Enabled = true;
				_graphInit = false;
			}

			_parameterListView.AddItem( parameter );
		}

		if ( !Graph.Parameters.Any() )
		{
			_deleteButton.Enabled = false;
		}

		if ( baseBlackboardParameter != null && !_graphInit )
		{
			if ( Graph.Parameters.Any() )
			{
				_controlSheet.AddObject( baseBlackboardParameter.GetSerialized() );
				_parameterListView.SelectItem( baseBlackboardParameter );

				_selectedItem = baseBlackboardParameter;
				_deleteButton.Enabled = true;
			}
			else
			{
				_deleteButton.Enabled = false;
			}
		}
	}

	private void SetBlackboardParameterTarget( BaseBlackboardParameter blackboardParameter )
	{
		_controlSheet.Clear( true );

		var so = blackboardParameter.GetSerialized();
		so.OnPropertyChanged += ( prop ) =>
		{
			OnParameterChanged?.Invoke( blackboardParameter );
		};

		_selectedItem = blackboardParameter;
		_deleteButton.Enabled = true;

		_controlSheet.AddObject( so );
	}

	private void AddBlackboardParameter( TypeDescription typeDescription )
	{
		int id = _graph._parameters.Count;
		string name = $"Parameter{id}";

		var parameterInstance = BlackboardUtils.CreateBaseBlackboardParameterInstance( typeDescription.TargetType, id, name );
		_graph.AddBlackboardParameter( parameterInstance );

		OnDirty?.Invoke();

		UpdateParameterList( parameterInstance );
	}
}
