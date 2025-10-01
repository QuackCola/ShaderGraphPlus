using Editor;
using Sandbox.Internal;

namespace ShaderGraphPlus;

internal class Blackboard : Widget
{
	private ShaderGraphPlus _graph;
	public ShaderGraphPlus Graph
	{
		get => _graph;
		set
		{
			if ( value == null )
				return;

			_graph = value;

			UpdateParameterList( null );
		}
	}

	public Action OnDirty { get; set; }
	public Action<BaseBlackboardParameter> OnParameterChanged { get; set; }

	private ControlSheet _sheet;
	private Button.Primary _addButton;
	private Button.Danger _deleteButton;
	private ListView _parameterListView;
	private bool GraphInit;
	private object SelectedItem;

	public Blackboard( Widget parent ) : base( parent )
	{
		Name = "Blackboard";
		WindowTitle = "Blackboard";
		SetWindowIcon( "list" );

		Layout = Layout.Row();
		Layout.Spacing = 8;
		Layout.Margin = 4;

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
			if ( SelectedItem != null )
			{
				SGPLog.Info( $"Delete selected item {SelectedItem}" );
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

		_sheet = new ControlSheet();
		_sheet.SizeConstraint = SizeConstraint.SetMaximumSize;
		_sheet.SetColumnStretch( 2, 0 );
		_sheet.SetMinimumColumnWidth( 0, 400 );
		rightColumn.Add( _sheet );
		rightColumn.AddStretchCell();

		Layout.Add( canvas );

		GraphInit = true;
	}

	private void ClickItem( object item )
	{
		var variable = item as BaseBlackboardParameter;

		SetParameterTarget( variable );
	}

	private void PaintItem( VirtualWidget item )
	{
		var variable = item.Object as BaseBlackboardParameter;
		var rect = item.Rect;
		var rowRect = rect.Grow( 0,0,0,0 );

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

	private void UpdateParameterList( BaseBlackboardParameter baseBlackboardParameter )
	{
		_parameterListView.Clear();
		_sheet.Clear( true );

		foreach ( var parameter in Graph.Parameters )
		{
			if ( GraphInit )
			{
				_sheet.AddObject( parameter.GetSerialized() );
				_parameterListView.SelectItem( parameter );
				SelectedItem = parameter;
				_deleteButton.Enabled = true;
				GraphInit = false;
			}
	
			_parameterListView.AddItem( parameter );
		}

		if ( !Graph.Parameters.Any() )
		{
			_deleteButton.Enabled = false;
		}

		if ( baseBlackboardParameter != null && !GraphInit )
		{
			if ( Graph.Parameters.Any() )
			{
				_sheet.AddObject( baseBlackboardParameter.GetSerialized() );
				_parameterListView.SelectItem( baseBlackboardParameter );

				SelectedItem = baseBlackboardParameter;
				_deleteButton.Enabled = true;
			}
			else
			{
				_deleteButton.Enabled = false;
			}
		}
	}

	private void SetParameterTarget( BaseBlackboardParameter blackboardParameter )
	{
		_sheet.Clear( true );

		var so = blackboardParameter.GetSerialized();
		so.OnPropertyChanged += ( prop ) =>
		{
			OnParameterChanged?.Invoke( blackboardParameter );
		};

		SelectedItem = blackboardParameter;
		_deleteButton.Enabled = true;

		_sheet.AddObject( so );
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

	private Button.Primary AddPrimaryButton( Layout layout, string name = "My Button", string icon = "" )
	{
		var footer = layout.AddRow();
		footer.Margin = new( 4, 1, 4, 3 );
		footer.Spacing = 4;

		footer.AddStretchCell();

		var btn = footer.Add( new Button.Primary( name, icon ), 0 );

		footer.AddStretchCell();

		return btn;
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
