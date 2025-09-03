
using Editor;
using Sandbox;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

public abstract class ShaderNodePlus : BaseNodePlus
{
	[Hide]
	public virtual string Title => null;

	[Hide]
	public bool IsDirty = false;

	[JsonIgnore, Hide, Browsable( false )]
	public override DisplayInfo DisplayInfo
	{
		get
		{
			var info = base.DisplayInfo;
			info.Name = Title ?? info.Name;
			return info;
		}
	}

	[Hide,JsonIgnore]
	public virtual Color HeaderColor1 { get; } = Color.Gray;
	
	[Hide, JsonIgnore]
	public virtual Color HeaderColor2 { get; } = Color.Black;

	public override NodeUI CreateUI( GraphView view )
	{
		return new NodeUIPlus( view, this );//base.CreateUI( view );
	}
}

internal sealed class NodeUIPlus : NodeUI
{
	ShaderNodePlus ShaderNodePlus;

	public NodeUIPlus( GraphView graph, ShaderNodePlus node ) : base( graph, node )
	{
		ShaderNodePlus = node;
	}

	protected override void OnPaint()
	{
		//
		base.OnPaint();
		var rect = new Rect( 0f, Size );
		var radius = 4;
		var display = DisplayInfo;
		var headerColor1 = ShaderNodePlus.HeaderColor1;
		var headerColor2 = ShaderNodePlus.HeaderColor2;

		if ( Node.HasTitleBar )
		{
			var titleRect = new Rect( rect.Position, new Vector2( rect.Width, TitleHeight ) ).Shrink( 4f, 0f, 4f, 0f );
			float borderSize = 3;
			Paint.ClearPen();
			Paint.SetBrushLinear( rect.Left, rect.Right, headerColor1, headerColor2 );
			Paint.DrawRect( titleRect.Shrink( borderSize ), radius );
		
		}
		//else
		//{
		//	Paint.ClearPen();
		//	Paint.SetBrush( PrimaryColor.Darken( 0.6f ) );
		//	Paint.DrawRect( rect, radius );
		//}
		
	}
}
