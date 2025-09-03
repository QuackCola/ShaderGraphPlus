using Editor;

using NodeEditorPlus;
using GraphView = NodeEditorPlus.GraphView;
using NodeUI = NodeEditorPlus.NodeUI;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;
using ICommentNode = NodeEditorPlus.ICommentNode;
using CommentColor = NodeEditorPlus.CommentColor;
using CommentUI = NodeEditorPlus.CommentUI;

namespace ShaderGraphPlus;

[Icon( "notes" ), Hide]
public class CommentNode : BaseNodePlus, ICommentNode
{
	[Hide]
	public override int Version => 1;

	[Hide, Browsable( false )]
	public Vector2 Size { get; set; }

	public CommentColor Color { get; set; } = CommentColor.Green;
	public string Title { get; set; } = "Untitled";

	[TextArea]
	public string Description { get; set; } = "";

	[Hide, Browsable( false )]
	public int Layer { get; set; } = 5;

	[Hide, JsonIgnore]
	public override DisplayInfo DisplayInfo
	{
		get
		{
			var info = DisplayInfo.For( this );

			info.Name = Title;
			info.Description = Description;

			return info;
		}
	}

	public override NodeUI CreateUI( GraphView view )
	{
		return new CommentUI( view, this );
	}
}
