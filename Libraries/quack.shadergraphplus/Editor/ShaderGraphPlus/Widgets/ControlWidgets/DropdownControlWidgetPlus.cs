﻿using Editor;

namespace ShaderGraphPlus;

/// <summary>
/// Abstract class to enable easily creating ControlWidgets with dropdowns.
/// </summary>
public abstract class DropdownControlWidgetPlus<T> : ControlWidget
{
	public override bool SupportsMultiEdit => true;

	public DropdownControlWidgetPlus( SerializedProperty property ) : base( property )
	{
		Layout = Layout.Row();
		Cursor = CursorShape.Finger;
	}

	public struct Entry
	{
		public T Value { get; set; }
		public string Label { get; set; }
		public string Icon { get; set; }
		public string Description { get; set; }
	}

	protected abstract IEnumerable<object> GetDropdownValues();

	protected override void PaintControl()
	{
		var value = SerializedProperty.GetValue<object>();
		var color = IsControlHovered ? Theme.Blue : Theme.TextControl;
		var rect = LocalRect;

		rect = rect.Shrink( 8, 0 );

		//var e = enumDesc.GetEntry( value );

		//if ( !string.IsNullOrEmpty( e.Icon ) )
		//{
		//	Paint.SetPen( color.WithAlpha( 0.5f ) );
		//	var i = Paint.DrawIcon( rect, e.Icon, 16, TextFlag.LeftCenter );
		//	rect.Left += i.Width + 8;
		//}

		Paint.SetPen( color );
		Paint.SetDefaultFont();

		if ( SerializedProperty.IsMultipleDifferentValues )
		{
			Paint.SetPen( Theme.MultipleValues );
			Paint.DrawText( rect, "Multiple Values", TextFlag.LeftCenter );
		}
		else
		{
			Paint.DrawText( rect, value?.ToString() ?? "None", TextFlag.LeftCenter );
		}

		Paint.SetPen( color );
		Paint.DrawIcon( rect, "Arrow_Drop_Down", 17, TextFlag.RightCenter );
	}

	PopupWidget _menu;

	public override void StartEditing()
	{
		if ( !_menu.IsValid )
		{
			OpenMenu();
		}
	}

	protected virtual void OnSelectItem( object item )
	{
		if ( item is Entry e )
		{
			SerializedProperty.SetValue( e.Value );
		}
		else
		{
			SerializedProperty.SetValue( item );
		}
	}

	protected override void OnMouseClick( MouseEvent e )
	{
		if ( e.LeftMouseButton && !_menu.IsValid() )
		{
			OpenMenu();
		}
	}

	protected override void OnDoubleClick( MouseEvent e )
	{
		// nothing
	}

	void OpenMenu()
	{
		_menu = new PopupWidget( null );

		_menu.Layout = Layout.Column();
		_menu.Width = ScreenRect.Width;

		var scroller = _menu.Layout.Add( new ScrollArea( this ), 1 );
		scroller.Canvas = new Widget( scroller )
		{
			Layout = Layout.Column(),
			VerticalSizeMode = SizeMode.CanGrow | SizeMode.Expand
		};

		object[] entries = GetDropdownValues().ToArray();

		foreach ( var o in entries )
		{
			var b = scroller.Canvas.Layout.Add( new MenuOption<T>( o, SerializedProperty ) );
			b.MouseLeftPress = () =>
			{
				OnSelectItem( o );

				_menu.Update();
				_menu.Close();
			};
		}

		_menu.Position = ScreenRect.BottomLeft;
		_menu.Visible = true;
		_menu.AdjustSize();
		_menu.ConstrainToScreen();
		_menu.OnPaintOverride = PaintMenuBackground;
	}

	bool PaintMenuBackground()
	{
		Paint.SetBrushAndPen( Theme.ControlBackground );
		Paint.DrawRect( Paint.LocalRect, 0 );
		return true;
	}

}

file class MenuOption<T> : Widget
{
	object info;
	SerializedProperty property;

	public MenuOption( object e, SerializedProperty p ) : base( null )
	{
		info = e;
		property = p;

		Layout = Layout.Row();
		Layout.Margin = 8;

		if ( e is DropdownControlWidget<T>.Entry entry )
		{
			if ( !string.IsNullOrWhiteSpace( entry.Icon ) )
			{
				Layout.Add( new IconButton( entry.Icon ) { Background = Color.Transparent, TransparentForMouseEvents = true, IconSize = 18 } );
			}

			Layout.AddSpacingCell( 8 );
			var c = Layout.AddColumn();
			var title = c.Add( new Label( entry.Label ) );
			title.SetStyles( "font-size: 12px; font-weight: bold; font-family: Poppins; color: white;" );

			if ( !string.IsNullOrWhiteSpace( entry.Description ) )
			{
				var desc = c.Add( new Label( entry.Description.Trim( '\n', '\r', '\t', ' ' ) ) );
				desc.WordWrap = true;
				desc.MinimumHeight = 1;
				desc.MinimumWidth = 400;
			}
		}
		else
		{
			Layout.AddSpacingCell( 8 );
			var c = Layout.AddColumn();

			var labelString = "";

			if ( e is DropdownControlWidgetPlus<T>.Entry entryObject )
			{
				labelString = entryObject.Label;
			}
			else
			{
				labelString = e.ToString();
			}

			var title = c.Add( new Label( labelString ) );
			title.SetStyles( "font-size: 12px; font-weight: bold; font-family: Poppins; color: white;" );
		}
	}

	bool HasValue()
	{
		if ( property.IsMultipleDifferentValues ) return false;

		var value = property.GetValue<object>( default );
		return value == info;
	}

	protected override void OnPaint()
	{
		if ( Paint.HasMouseOver || HasValue() )
		{
			Paint.SetBrushAndPen( Theme.Blue.WithAlpha( HasValue() ? 0.3f : 0.1f ) );
			Paint.DrawRect( LocalRect.Shrink( 2 ), 2 );
		}
	}
}
