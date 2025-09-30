using Editor;

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

	private Layout _rowLayout;
	private Layout _leftLayout;
	private Layout _rightLayout;
	private Layout _bodylayout;
	private ControlSheet _sheet;
	private ListView _parameterListView;
	private bool GraphInit;

	public Blackboard( Widget parent ) : base( parent )
	{
		Name = "Blackboard";
		WindowTitle = "Blackboard";
		SetWindowIcon( "edit" );

		Layout = Layout.Column();
		_bodylayout = Layout.Add( Layout.Column(), 1 );

		_rowLayout = Layout.Row();
		_rowLayout.Spacing = 8;
		_bodylayout.Add( _rowLayout );

		_leftLayout = _rowLayout.AddColumn();
		_leftLayout.Add( new Label( "Parameters" ) );
		_leftLayout.Alignment = TextFlag.Center;
		_leftLayout.Spacing = 8;
		_leftLayout.Margin = 4;
		{
			_parameterListView = new( this );
			_parameterListView.ItemSize = new Vector2( 0, 32 );
			_parameterListView.ItemSpacing = 4;
			_parameterListView.ItemAlign = Sandbox.UI.Align.Center;
			_parameterListView.OnPaintOverride = () => 
			{ 
				Paint.ClearPen(); 
				Paint.SetBrush( Theme.ControlBackground ); 
				Paint.DrawRect( _parameterListView.LocalRect, Theme.ControlRadius );

				return false; 
			};
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

			_leftLayout.Add( _parameterListView, 1 );
		}

		_rightLayout = _rowLayout.AddColumn( 1 );
		_rightLayout.Add( new Label( "Parameter Properties" ) );
		_rightLayout.Spacing = 8;
		_rightLayout.Alignment = TextFlag.Center;
		_rightLayout.Margin = 4;
		{
			_sheet = new ControlSheet();
			_rightLayout.Add( _sheet, 1 );
			_rightLayout.AddStretchCell( 16 );
		}

		_bodylayout.AddSpacingCell( 16 );

		var bottom = Layout.Column();
		bottom.Spacing = 8;
		bottom.Margin = 4;
		{
			var addButton = AddPrimaryButton( bottom, "Add", "new_label" );
			addButton.Clicked += () =>
			{
				var popup = new PopupTypeSelector( this );
				popup.OnSelect += ( t ) =>
				{
					AddBlackboardParameter( t );
				};
				popup.OpenAtCursor();
			};
		}

		_bodylayout.Add( bottom );

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

		var textColor = Theme.TextControl;
		var itemColor = Theme.ControlBackground;

		if ( item.Hovered )
		{
			textColor = Color.White;
		}
		if ( item.Selected )
		{
			textColor = Theme.TextControl;
			itemColor = Theme.Primary;
		}

		Paint.SetPen( itemColor );
		Paint.SetBrush( itemColor );
		Paint.DrawRect( rect, Theme.ControlRadius );

		Paint.SetPen( textColor );
		Paint.SetBrush( textColor );
		Paint.DrawText( rect.Shrink( 4, 0, 0, 0 ), $"{variable.Name}", TextFlag.Left | TextFlag.CenterVertically | TextFlag.SingleLine );

		Paint.DrawText( rect.Shrink( 0 , 0, 4, 0 ), $"{DisplayInfo.ForType( variable.GetType() ).Name}", TextFlag.Right | TextFlag.CenterVertically | TextFlag.SingleLine );
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
				GraphInit = false;
			}
	
			_parameterListView.AddItem( parameter );
		}

		if ( baseBlackboardParameter != null && !GraphInit )
		{
			if ( Graph.Parameters.Any() )
			{
				_sheet.AddObject( baseBlackboardParameter.GetSerialized() );
				_parameterListView.SelectItem( baseBlackboardParameter );
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
