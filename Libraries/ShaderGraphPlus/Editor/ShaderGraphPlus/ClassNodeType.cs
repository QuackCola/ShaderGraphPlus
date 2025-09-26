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

public sealed class TextureNodeType : ClassNodeType
{
	string ImagePath;

	public TextureNodeType( TypeDescription type, string imagePath ) : base( type )
	{
		ImagePath = imagePath;
	}
	public override INodePlus CreateNode( INodeGraph graph )
	{
		var node = base.CreateNode( graph );
		if ( node is ITextureParameterNode textureNode )
		{
			textureNode.Image = ImagePath;
		}
		return node;
	}
}

public sealed class NamedRerouteNodeType : ClassNodeType
{
	private string Name;

	public NamedRerouteNodeType( TypeDescription type, string name = "" ) : base( type )
	{
		Name = name;
	}

	public override INodePlus CreateNode( INodeGraph graph )
	{
		var node = base.CreateNode( graph );
		if ( node is NamedRerouteNode namedRerouteNode )
		{
			namedRerouteNode.Name = Name;
		}
		return node;
	}
}

public sealed class NamedRerouteDeclarationNodeType : ClassNodeType
{
	private string Name;

	public NamedRerouteDeclarationNodeType( TypeDescription type, string name = "" ) : base( type )
	{
		Name = name;
	}

	public override INodePlus CreateNode( INodeGraph graph )
	{
		var node = base.CreateNode( graph );
		if ( node is NamedRerouteDeclarationNode namedRerouteDeclarationNode )
		{
			namedRerouteDeclarationNode.Name = Name;
		}
		return node;
	}
}

public sealed class SubgraphNodeType : ClassNodeType
{
	public override string Identifier => AssetPath;
	string AssetPath { get; }

	public SubgraphNodeType( string assetPath, TypeDescription type ) : base( type )
	{
		AssetPath = assetPath;
	}

	public void SetDisplayInfo( ShaderGraphPlus subgraph )
	{
		var info = DisplayInfo;
		if ( !string.IsNullOrEmpty( subgraph.Title ) )
			info.Name = subgraph.Title;
		else
			info.Name = System.IO.Path.GetFileNameWithoutExtension( AssetPath );
		if ( !string.IsNullOrEmpty( subgraph.Description ) )
			info.Description = subgraph.Description;
		if ( !string.IsNullOrEmpty( subgraph.Icon ) )
			info.Icon = subgraph.Icon;
		if ( !string.IsNullOrEmpty( subgraph.Category ) )
			info.Group = subgraph.Category;
		DisplayInfo = info;
	}

	public override INodePlus CreateNode( INodeGraph graph )
	{
		var node = base.CreateNode( graph );

		if ( node is SubgraphNode subgraphNode )
		{
			subgraphNode.SubgraphPath = AssetPath;
			subgraphNode.OnNodeCreated();
		}

		return node;
	}
}
