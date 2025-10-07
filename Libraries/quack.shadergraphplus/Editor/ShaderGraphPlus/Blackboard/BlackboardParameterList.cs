using Editor;

namespace ShaderGraphPlus;

internal class BlackboardParameterList : ListView
{
	public BlackboardParameterList( Widget widget) : base( widget )
	{
		Margin = 8;
		ItemSpacing = 4;
		ItemSize = new Vector2( 0, 24 );
		AcceptDrops = false;
	}

	protected override void PaintItem( VirtualWidget item )
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

		var typeColor = Color.White;
		if ( ShaderGraphPlusTheme.BlackboardConfigs.TryGetValue( variable.GetType(), out var blackboardConfig ) )
		{
			typeColor = blackboardConfig.Color;
		}

		Paint.SetPen( typeColor );
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

	protected override void OnPaint()
	{
		Paint.ClearPen();
		Paint.SetBrush( Theme.ControlBackground );
		Paint.DrawRect( LocalRect, 4 );

		base.OnPaint();
	}
}
