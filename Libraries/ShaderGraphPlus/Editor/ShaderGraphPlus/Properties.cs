
using Facepunch.ActionGraphs;
using static Sandbox.Gizmo;

namespace Editor.ShaderGraphPlus;

public class Properties : Widget
{

	private readonly List<ExpandGroup> expandGroups = new List<ExpandGroup>();

	private object _target;
	public object Target
	{
		get => _target;
		set
		{
			if ( value == _target )
				return;

			_target = value;

			Editor.Clear( true );

			if ( value is null )
				return;

			var so = value.GetSerialized();
			so.OnPropertyChanged += x =>
			{
				PropertyUpdated?.Invoke();
			};

			var sheet = new ControlSheet();
	
			sheet.AddObject( so );

			var scroller = new ScrollArea( this );
			scroller.Canvas = new Widget();
			scroller.Canvas.Layout = Layout.Column();
			scroller.Canvas.VerticalSizeMode = SizeMode.CanGrow;
			scroller.Canvas.HorizontalSizeMode = SizeMode.Flexible;
			scroller.Canvas.Layout.Add( sheet );
			scroller.Canvas.Layout.AddStretchCell();

			Editor.Add( scroller );
		}
	}

	private readonly Layout Editor;

	public Action PropertyUpdated { get; set; }

	public Properties( Widget parent ) : base( parent )
	{
		Name = "Properties";
		WindowTitle = "Properties";
		SetWindowIcon( "edit" );

		Layout = Layout.Column();

		var toolbar = new ToolBar( this );
		var filter = new LineEdit( toolbar ) { PlaceholderText = "Filter Properties.." };
		filter.TextEdited += OnFilterEdited;
		toolbar.AddWidget( filter );
		Layout.Add( toolbar );
		Layout.AddSeparator();

		Editor = Layout.AddRow( 1 );
		Layout.AddStretchCell();
	}

	private void OnFilterEdited( string filter )
	{
	}
}
