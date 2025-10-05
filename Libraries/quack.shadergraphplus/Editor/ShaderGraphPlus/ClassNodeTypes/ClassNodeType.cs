using Editor;
using NodeEditorPlus;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

public class ClassNodeType : INodeTypePlus
{
	public virtual string Identifier => Type.FullName;

	public TypeDescription Type { get; }
	public DisplayInfo DisplayInfo { get; protected set; }

	public Menu.PathElement[] Path => Menu.GetSplitPath( DisplayInfo );
	public bool LowPriority => false;

	public ClassNodeType( TypeDescription type )
	{
		Type = type;
		if ( Type is not null )
			DisplayInfo = DisplayInfo.ForType( Type.TargetType );
		else
			DisplayInfo = new DisplayInfo();
	}

	public bool TryGetInput( Type valueType, out string name )
	{
		var property = Type.Properties
			.Select( x => (Property: x, Attrib: x.GetCustomAttribute<BaseNodePlus.InputAttribute>()) )
			.Where( x => x.Attrib != null )
			.FirstOrDefault( x => x.Attrib.Type?.IsAssignableFrom( valueType ) ?? true ).Property;

		name = property?.Name;
		return name is not null;
	}

	public bool TryGetOutput(Type valueType, out string name)
	{
		var property = Type.Properties
			.Select( x => ( Property: x, Attrib: x.GetCustomAttribute<BaseNodePlus.OutputAttribute>() ) )
			.Where( x => x.Attrib != null )
			.FirstOrDefault( x => x.Attrib.Type?.IsAssignableTo( valueType ) ?? true )
			.Property;

		name = property?.Name;
		return name is not null;
	}

	public virtual INodePlus CreateNode(INodeGraph graph)
	{
		var node = Type.Create<BaseNodePlus>();

		node.Graph = graph;

		return node;
	}
}
