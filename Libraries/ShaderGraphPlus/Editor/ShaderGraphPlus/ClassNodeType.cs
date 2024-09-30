using System;
using Editor.NodeEditor;

namespace Editor.ShaderGraphPlus;

public class ClassNodeType : INodeType
{
	public string Identifier => Type.FullName;

	public TypeDescription Type { get; }
	public DisplayInfo DisplayInfo { get; }

	public Menu.PathElement[] Path => Menu.GetSplitPath( DisplayInfo );
	public bool LowPriority => false;

	public ClassNodeType( TypeDescription type )
	{
		Type = type;
		DisplayInfo = DisplayInfo.ForType( Type.TargetType );
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

	public INode CreateNode( IGraph graph )
	{
		var node = Type.Create<BaseNodePlus>();

		node.Graph = graph;

		return node;
	}
}
