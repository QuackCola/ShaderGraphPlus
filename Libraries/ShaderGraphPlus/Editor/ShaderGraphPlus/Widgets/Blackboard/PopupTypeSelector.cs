using Editor;

namespace ShaderGraphPlus;

internal class PopupTypeSelector : PopupWidget
{
	public Action<TypeDescription> OnSelect
	{
		get => Widget.OnSelect;
		set => Widget.OnSelect = value;
	}

	TypeSelectorWidget Widget { get; set; }

	public PopupTypeSelector( Widget parent ) : base( parent )
	{
		Widget = new TypeSelectorWidget( this )
		{
			OnDestroy = Destroy
		};

		Layout = Layout.Column();
		Layout.Add( Widget );

		DeleteOnClose = true;
	}
}

internal partial class TypeSelectorWidget : Widget
{
	public Action<TypeDescription> OnSelect { get; set; }
	public Action OnDestroy { get; set; }
	List<TypeSelection> Panels { get; set; } = new();
	int CurrentPanelId { get; set; } = 0;
	Widget Main { get; set; }

	string searchString;
	internal LineEdit Search { get; init; }

	public TypeSelectorWidget( Widget parent ) : base( parent )
	{
		Layout = Layout.Column();

		var head = Layout.Row();
		head.Margin = 6;

		Layout.Add( head );

		Main = new Widget( this );
		Main.Layout = Layout.Row();
		Main.Layout.Enabled = false;
		Main.FixedSize = new( 300, 400 );
		Layout.Add( Main, 1 );

		DeleteOnClose = true;

		Search = new LineEdit( this );
		Search.Layout = Layout.Row();
		Search.Layout.AddStretchCell( 1 );
		Search.MinimumHeight = 22;
		Search.PlaceholderText = "Search...";
		Search.TextEdited += ( t ) =>
		{
			searchString = t;
			ResetSelection();
		};


		var clearButton = Search.Layout.Add( new ToolButton( string.Empty, "clear", this ) );
		clearButton.MouseLeftPress = () =>
		{
			Search.Text = searchString = string.Empty;
			ResetSelection();
		};

		head.Add( Search );

		var filterButton = new TypeFilterControlWidget( this, true );
		head.Add( filterButton );

		ResetSelection();

		Search.Focus();


	}

	void OnTypeSelected( TypeDescription type )
	{
		OnSelect( type );
		OnDestroy?.Invoke();
	}

	protected override void OnKeyRelease( KeyEvent e )
	{
		if ( e.Key == KeyCode.Down )
		{
			var selection = Panels[CurrentPanelId];
			if ( selection.ItemList.FirstOrDefault().IsValid() )
			{
				selection.Focus();
				selection.PostKeyEvent( KeyCode.Down );
				e.Accepted = true;
			}
		}
	}

	/// <summary>
	/// Resets the current selection, useful when setting up / searching
	/// </summary>
	protected void ResetSelection()
	{
		Main.Layout.Clear( true );
		Panels.Clear();

		var selection = new TypeSelection( Main, this );

		UpdatedSelection( selection );

		Panels.Add( selection );
		Main.Layout.Add( selection );
	}

	int SearchScore( TypeDescription type, string[] parts )
	{
		var score = 0;

		var t = type.Title.Replace( " ", "" );
		var c = type.ClassName.Replace( " ", "" );

		foreach ( var w in parts )
		{
			if ( t.Contains( w, StringComparison.OrdinalIgnoreCase ) ) score += 10;
			if ( c.Contains( w, StringComparison.OrdinalIgnoreCase ) ) score += 5;
		}

		return score;
	}

	void UpdatedSelection( TypeSelection selection )
	{
		selection.Clear();

		var types = EditorTypeLibrary.GetTypes<BaseBlackboardParameter>().Where( x => !x.IsAbstract 
		&& !x.HasAttribute<HideAttribute>() ).OrderBy( x => x.Order );

		if ( !string.IsNullOrWhiteSpace( searchString ) )
		{
			var searchWords = searchString.Split( ' ', StringSplitOptions.RemoveEmptyEntries );
			var query = types.Select( x => new { x, score = SearchScore( x, searchWords ) } )
								.ToArray()
								.Where( x => x.score > 0 );

			foreach ( var type in query.OrderByDescending( x => x.score ).Select( x => x.x ) )
			{
				selection.AddEntry( new TypeEntry( selection, type ) { MouseClick = () => OnTypeSelected( type ) } );
			}

			selection.AddStretchCell();

			return;
		}

		foreach ( var type in types )
		{
			selection.AddEntry( new TypeEntry( selection, type ) { MouseClick = () => OnTypeSelected( type ) } );
		}

		selection.AddStretchCell();
	}

	protected override void OnPaint()
	{
		Paint.Antialiasing = true;
		Paint.SetPen( Theme.WidgetBackground.Darken( 0.4f ), 1 );
		Paint.SetBrush( Theme.WidgetBackground );
		Paint.DrawRect( LocalRect.Shrink( 1 ), 3 );
	}
}

internal partial class TypeSelection : Widget
{
	internal string VariableTypeName { get; init; }
	ScrollArea Scroller { get; init; }

	internal List<Widget> ItemList { get; private set; } = new();

	internal Widget CurrentItem { get; private set; }

	internal TypeSelection( Widget parent, TypeSelectorWidget selector, string variableTypeName = null ) : base( parent )
	{
		VariableTypeName = variableTypeName;
		FixedSize = parent.ContentRect.Size;
		Layout = Layout.Column();

		Scroller = new ScrollArea( this );
		Scroller.Layout = Layout.Column();
		Scroller.FocusMode = FocusMode.None;
		Layout.Add( Scroller, 1 );

		Scroller.Canvas = new Widget( Scroller );
		Scroller.Canvas.Layout = Layout.Column();
	}

	/// <summary>
	/// Adds a new entry to the current selection.
	/// </summary>
	/// <param name="entry"></param>
	internal Widget AddEntry( Widget entry )
	{
		var layoutWidget = Scroller.Canvas.Layout.Add( entry );
		ItemList.Add( entry );

		if ( entry is TypeEntry e ) e.Selector = this;

		return layoutWidget;
	}

	/// <summary>
	/// Adds a stretch cell to the bottom of the selection - good to call this when you know you're done adding entries.
	/// </summary>
	internal void AddStretchCell()
	{
		Scroller.Canvas.Layout.AddStretchCell( 1 );
		Update();
	}

	/// <summary>
	/// Adds a separator cell.
	/// </summary>
	internal void AddSeparator()
	{
		Scroller.Canvas.Layout.AddSeparator( true );
		Update();
	}

	/// <summary>
	/// Clears the current selection
	/// </summary>
	internal void Clear()
	{
		Scroller.Canvas.Layout.Clear( true );
		ItemList.Clear();
	}

	protected override void OnPaint()
	{
		Paint.Antialiasing = true;
		Paint.SetPen( Theme.WidgetBackground.Darken( 0.8f ), 1 );
		Paint.SetBrush( Theme.WidgetBackground.Darken( 0.2f ) );
		Paint.DrawRect( LocalRect.Shrink( 0 ), 3 );
	}

}

internal class TypeEntry : Widget
{
	public string Text { get; set; } = "Test";
	public string Icon { get; set; } = "note_add";

	internal TypeSelection Selector { get; set; }

	public TypeDescription Type { get; init; }

	internal TypeEntry( Widget parent, TypeDescription type = null ) : base( parent )
	{
		FixedHeight = 24;
		Type = type;

		if ( type is not null )
		{
			Text = type.Title;
			Icon = type.Icon;
			//ToolTip = $"<b>{type.FullName}</b><br/>{type.Description}";
		}

	}

	protected override void OnPaint()
	{
		var r = LocalRect.Shrink( 12, 2 );
		var selected = IsUnderMouse || Selector.CurrentItem == this;
		var opacity = selected ? 1.0f : 0.7f;

		if ( selected )
		{
			Paint.ClearPen();
			Paint.SetBrush( Theme.ControlBackground );
			Paint.DrawRect( LocalRect );
		}

		r.Left += r.Height + 6;

		Paint.SetDefaultFont( 8 );
		Paint.SetPen( Theme.TextControl.WithAlpha( selected ? 1.0f : 0.5f ) );
		Paint.DrawText( r, Text, TextFlag.LeftCenter ); ;
	}
}

file class TypeFilterControlWidget : Widget
{

	TypeSelectorWidget Target;

	Widget additionalOptions;

	public bool disabled { get; set; } = false;

	public TypeFilterControlWidget( TypeSelectorWidget targetObject, bool disable = false )
	{
		Target = targetObject;
		Cursor = CursorShape.Finger;
		MinimumWidth = Theme.RowHeight;
		HorizontalSizeMode = SizeMode.CanShrink;

		additionalOptions = null;

		disabled = disable;

		ToolTip = "Filter Settings";
	}

	protected override Vector2 SizeHint()
	{
		return new( Theme.RowHeight, Theme.RowHeight );
	}

	protected override Vector2 MinimumSizeHint()
	{
		return new( Theme.RowHeight, Theme.RowHeight );
	}

	protected override void OnDoubleClick( MouseEvent e ) { }

	protected override void OnMousePress( MouseEvent e )
	{
		if ( ReadOnly ) return;
		//OpenSettings();
		e.Accepted = true;
	}

	protected override void OnPaint()
	{
		Paint.Antialiasing = true;
		Paint.TextAntialiasing = true;

		var rect = LocalRect.Shrink( 2 );
		var icon = "sort";

		if ( additionalOptions?.IsValid ?? false )
		{
			Paint.SetPen( Theme.Blue.WithAlpha( 0.3f ), 1 );
			Paint.SetBrush( Theme.Blue.WithAlpha( 0.2f ) );
			Paint.DrawRect( rect, 2 );

			Paint.SetPen( Theme.Blue );
			Paint.DrawIcon( rect, icon, 13 );
		}
		else
		{
			Paint.SetPen( Theme.Blue.WithAlpha( 0.3f ) );
			Paint.DrawIcon( rect, icon, 13 );
		}

		if ( IsUnderMouse )
		{
			if ( !disabled )
			{
				Paint.SetPen( Theme.Blue.WithAlpha( 0.5f ), 1 );
				Paint.ClearBrush();
				Paint.DrawRect( rect, 1 );
			}
		}
	}

	bool PaintMenuBackground()
	{
		Paint.SetBrushAndPen( Theme.ControlBackground );
		Paint.DrawRect( Paint.LocalRect, 0 );
		return true;
	}
}
