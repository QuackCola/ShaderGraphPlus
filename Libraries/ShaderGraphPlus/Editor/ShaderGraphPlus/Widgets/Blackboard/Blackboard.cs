using Editor;

namespace ShaderGraphPlus;

public class Blackboard : Widget
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

			UpdateParameterList();
		}
	}

	public Action OnDirty { get; set; }
	public Action<BaseBlackboardParameter> OnParameterChanged { get; set; }

	private ControlSheet Sheet;
	private ListView _parameterListView;
	private Layout _rowLayout;
	private Layout _leftLayout;
	private Layout _rightLayout;
	private bool GraphInit;

	public Blackboard( Widget parent ) : base( parent )
	{
		Name = "Blackboard";
		WindowTitle = "Blackboard";
		SetWindowIcon( "edit" );

		//Graph = graph;

		Layout = Layout.Column();

		_rowLayout = Layout.Row();
		_rowLayout.Spacing = 8;
		Layout.Add( _rowLayout );

		_leftLayout = _rowLayout.AddColumn();
		_leftLayout.Add( new Label( "Parameters" ) );
		_leftLayout.Spacing = 8;

		{
			_parameterListView = new( null );
			_parameterListView.ItemSize = new Vector2( 0, 18 );
			_parameterListView.ItemSpacing = 4;
			_parameterListView.OnPaintOverride = () => { Paint.ClearPen(); Paint.SetBrush( Theme.ControlBackground ); Paint.DrawRect( _parameterListView.LocalRect, Theme.ControlRadius ); return false; };
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

		{
			Sheet = new ControlSheet();

			_rightLayout.Add( Sheet, 1 );
			_rightLayout.AddStretchCell( 16 );
		}

		Layout.AddSpacingCell( 16 );

		var bottom = Layout.Column();
		bottom.Spacing = 8;

		{
			var addButton = AddPrimaryButton( bottom, "Add", "new_label" );
			addButton.Clicked += () =>
			{
				var popup = new PopupTypeSelector( this );
				popup.OnSelect += ( t ) =>
				{
					AddBlackboardParameter( t );
					UpdateParameterList();
				};
				popup.OpenAtCursor();
			};
		}

		Layout.Add( bottom );

		GraphInit = true;

		if ( Graph != null )
		{
			UpdateParameterList();
		}
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
		Paint.DrawText( rect, $"{variable.Name}", TextFlag.CenterHorizontally | TextFlag.SingleLine );
	}

	private void UpdateParameterList()
	{
		_parameterListView.Clear();

		var parameters = Graph.Parameters;

		if ( parameters.Any() )
		{
			Sheet.AddObject( parameters.First().GetSerialized() );
		}

		foreach ( var parameter in parameters )
		{
			if ( GraphInit )
			{
				_parameterListView.SelectItem( parameter );
				GraphInit = false;
			}
	
			_parameterListView.AddItem( parameter );
		}

	}

	private void SetParameterTarget( BaseBlackboardParameter blackboardParameter )
	{
		Sheet.Clear( true );

		var so = blackboardParameter.GetSerialized();
		so.OnPropertyChanged += ( prop ) =>
		{
			var p = prop.GetValue<BaseBlackboardParameter>();

			OnParameterChanged?.Invoke( p );
		};

		Sheet.AddObject( blackboardParameter.GetSerialized() );
	}

	private void AddBlackboardParameter( TypeDescription typeDescription )
	{
		//SGPLog.Info( $"Selected type : \"{typeDescription}\"" );

		int id = _graph._parameters.Count;
		string name = $"Parameter{id}";

		var parameterInstance = BlackboardUtils.CreateBaseBlackboardParameterInstance( typeDescription.TargetType, id, name );
		_graph.AddBlackboardParameter( parameterInstance );

		OnDirty?.Invoke();
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
			BoolBlackboardParameter => new BoolBlackboardParameter( false, id ) { Name = name },
			IntBlackboardParameter => new IntBlackboardParameter( 0, id ) { Name = name },
			FloatBlackboardParameter => new FloatBlackboardParameter( 0.0f, id ) { Name = name },
			Float2BlackboardParameter => new Float2BlackboardParameter( Vector2.Zero, id ) { Name = name },
			Float3BlackboardParameter => new Float3BlackboardParameter( Vector3.Zero, id ) { Name = name },
			Float4BlackboardParameter => new Float4BlackboardParameter( Color.White, id ) { Name = name },
			_ => throw new NotImplementedException(),
		};
	}
}
