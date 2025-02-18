namespace Editor.NodeEditor;

public interface IExpresionNode : INode
{
	Vector2 Size { get; set; }
	string Description { get; set; }
}

public class ExpressionUI : NodeUI
{
	private IExpresionNode Expresion => Node as IExpresionNode;


	public ExpressionUI( GraphView graph, IExpresionNode node ) : base( graph, node )
	{
		HoverEvents = true;
		Selectable = true;
		Movable = true;
	}

}
