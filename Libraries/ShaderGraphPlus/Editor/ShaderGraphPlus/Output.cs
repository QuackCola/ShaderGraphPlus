
using static Editor.ShaderGraphPlus.GraphCompiler;

namespace Editor.ShaderGraphPlus;

public class Output : Widget
{
	private ErrorListView _errorListView;

	private WarningListView _warningListView;

	public IEnumerable<GraphCompiler.Error> Errors { set { _errorListView.SetItems( value.Cast<object>() ); } }

	public IEnumerable<GraphCompiler.Warning> Warnings { set { _warningListView.SetItems( value.Cast<object>() ); } }

	public Action<BaseNodePlus> OnNodeSelected { get; set; }

	public void Clear()
	{
		_errorListView.Clear();
		_warningListView.Clear();
	}

	public Output( Widget parent ) : base( parent )
	{
		Name = "Output";
		WindowTitle = "Output";
		SetWindowIcon( "notes" );
		
		Layout = Layout.Column();

		_errorListView = new( this );
		_warningListView = new( this );
		Layout.Add( _errorListView );
		Layout.Add( _warningListView );
	}
}

public class WarningListView : ListView
{
	private Output _output;

	public WarningListView( Output parent) : base( parent )
	{
		_output = parent;
		ItemActivated = ( a ) =>
		{
			if ( a is not GraphCompiler.Warning warning )
				return;

			_output.OnNodeSelected?.Invoke( warning.Node );
		};

		ItemContextMenu = OpenItemContextMenu;
		ItemSize = new Vector2( 0, 48 );
		ItemSpacing = 0;
		Margin = 0;
	}

	private void OpenItemContextMenu( object item )
	{
		if ( item is not GraphCompiler.Warning warning )
			return;

		var m = new Menu();

		if ( warning.Node != null )
		{
			var nodeName = DisplayInfo.ForType( warning.Node.GetType() ).Name;

			m.AddOption( "Go to Warning", "arrow_upward", () => _output.OnNodeSelected?.Invoke( warning.Node ) );
			m.AddOption( "Copy Warning", "content_copy", () => EditorUtility.Clipboard.Copy( $"{warning.Message}\n{nodeName} #{warning.Node.Identifier}" ) );
		}
		else
		{
			m.AddOption( "Copy Warning", "content_copy", () => EditorUtility.Clipboard.Copy( $"{warning.Message}" ) );
		}

		m.OpenAt( Application.CursorPosition );
	}

	protected override void OnPaint()
	{
		Paint.ClearPen();
		Paint.SetBrush( Theme.WindowBackground );
		Paint.DrawRect( LocalRect );

		base.OnPaint();
	}

	protected override void PaintItem( VirtualWidget item )
	{
		if ( item.Object is not GraphCompiler.Error error )
			return;

		var color = Theme.Yellow;
	
		Paint.SetBrush( color.WithAlpha( Paint.HasMouseOver ? 0.1f : 0.03f ) );
		Paint.ClearPen();
		Paint.DrawRect( item.Rect.Shrink( 0, 1 ) );

		Paint.Antialiasing = true;
		Paint.SetPen( color.WithAlpha( Paint.HasMouseOver ? 1 : 0.7f ), 3.0f );
		Paint.ClearBrush();

		var iconRect = item.Rect.Shrink( 12, 0 );
		iconRect.Width = 24;



		Paint.DrawIcon( iconRect, "error", 24 );

		var rect = item.Rect.Shrink( 48, 8, 0, 8 );

		Paint.SetPen( Theme.White.WithAlpha( Paint.HasMouseOver ? 1 : 0.8f ), 3.0f );
		Paint.DrawText( rect, error.Message, (error.Node != null ? TextFlag.LeftTop : TextFlag.LeftCenter) | TextFlag.SingleLine );

		if ( error.Node != null )
		{
			var nodeName = DisplayInfo.ForType( error.Node.GetType() ).Name;
			Paint.SetPen( Theme.White.WithAlpha( Paint.HasMouseOver ? 0.5f : 0.4f ), 3.0f );
			Paint.DrawText( rect, $"{nodeName}", TextFlag.LeftBottom | TextFlag.SingleLine );
		}
	}
}

public class ErrorListView : ListView
{
	private Output _output;

	public ErrorListView( Output parent ) : base( parent )
	{
		_output = parent;
		ItemActivated = ( a ) =>
		{
			if ( a is not GraphCompiler.Error error )
				return;

			_output.OnNodeSelected?.Invoke( error.Node );
		};

		ItemContextMenu = OpenItemContextMenu;
		ItemSize = new Vector2( 0, 48 );
		ItemSpacing = 0;
		Margin = 0;
	}

	private void OpenItemContextMenu( object item )
	{
		if ( item is not GraphCompiler.Error error )
			return;

		var m = new Menu();

		if ( error.Node != null )
		{
			var nodeName = DisplayInfo.ForType( error.Node.GetType() ).Name;

			m.AddOption( "Go to Error", "arrow_upward", () => _output.OnNodeSelected?.Invoke( error.Node ) );
			m.AddOption( "Copy Error", "content_copy", () => EditorUtility.Clipboard.Copy( $"{error.Message}\n{nodeName} #{error.Node.Identifier}" ) );
		}
		else
		{
			m.AddOption( "Copy Error", "content_copy", () => EditorUtility.Clipboard.Copy( $"{error.Message}" ) );
		}

		m.OpenAt( Application.CursorPosition );
	}

	protected override void OnPaint()
	{
		Paint.ClearPen();
		Paint.SetBrush( Theme.WindowBackground );
		Paint.DrawRect( LocalRect );

		base.OnPaint();
	}

	protected override void PaintItem( VirtualWidget item )
	{
		if ( item.Object is not GraphCompiler.Error error )
			return;

		var color = Theme.Red;

		Paint.SetBrush( color.WithAlpha( Paint.HasMouseOver ? 0.1f : 0.03f ) );
		Paint.ClearPen();
		Paint.DrawRect( item.Rect.Shrink( 0, 1 ) );

		Paint.Antialiasing = true;
		Paint.SetPen( color.WithAlpha( Paint.HasMouseOver ? 1 : 0.7f ), 3.0f );
		Paint.ClearBrush();

		var iconRect = item.Rect.Shrink( 12, 0 );
		iconRect.Width = 24;



		Paint.DrawIcon( iconRect, "error", 24 );

		var rect = item.Rect.Shrink( 48, 8, 0, 8 );

		Paint.SetPen( Theme.White.WithAlpha( Paint.HasMouseOver ? 1 : 0.8f ), 3.0f );
		Paint.DrawText( rect, error.Message, (error.Node != null ? TextFlag.LeftTop : TextFlag.LeftCenter) | TextFlag.SingleLine );

		if ( error.Node != null )
		{
			var nodeName = DisplayInfo.ForType( error.Node.GetType() ).Name;
			Paint.SetPen( Theme.White.WithAlpha( Paint.HasMouseOver ? 0.5f : 0.4f ), 3.0f );
			Paint.DrawText( rect, $"{nodeName}", TextFlag.LeftBottom | TextFlag.SingleLine );
		}
	}
}
