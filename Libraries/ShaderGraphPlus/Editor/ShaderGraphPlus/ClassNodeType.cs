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

public sealed class ParameterNodeType : ClassNodeType
{
	BaseBlackboardParameter BaseBlackboardValue;

	public ParameterNodeType( TypeDescription type, BaseBlackboardParameter value ) : base( type )
	{
		BaseBlackboardValue = value;
	}

	public override INodePlus CreateNode( INodeGraph graph )
	{
		var node = base.CreateNode( graph );

		string name = BaseBlackboardValue switch
		{
			BoolBlackboardParameter v => v.Name,
			IntBlackboardParameter v => v.Name,
			FloatBlackboardParameter v => v.Name,
			Float2BlackboardParameter v => v.Name,
			Float3BlackboardParameter v => v.Name,
			Float4BlackboardParameter v => v.Name,
			_ => throw new NotImplementedException(),
		};

		object value = BaseBlackboardValue switch
		{
			BoolBlackboardParameter v => v.Value,
			IntBlackboardParameter v => v.Value,
			FloatBlackboardParameter v => v.Value,
			Float2BlackboardParameter v => v.Value,
			Float3BlackboardParameter v => v.Value,
			Float4BlackboardParameter v => v.Value,
			_ => throw new NotImplementedException(),
		};
		
		int identifier = BaseBlackboardValue.Identifier;

		BaseNodePlus parameterNode = node switch
		{
			Bool => new Bool() { Name = name, Value = (bool)value, BlackboardParameterIdentifier = identifier },
			Int  => new Int() { Name = name, Value = (int)value, BlackboardParameterIdentifier = identifier },
			Float  => new Float() { Name = name, Value = (float)value, BlackboardParameterIdentifier = identifier },
			Float2  => new Float2() { Name = name, Value = (Vector2)value, BlackboardParameterIdentifier = identifier },
			Float3  => new Float3() { Name = name, Value = (Vector3)value, BlackboardParameterIdentifier = identifier },
			Float4  => new Float4() { Name = name, Value = (Color)value, BlackboardParameterIdentifier = identifier },
			_ => throw new NotImplementedException(),
		};

		return parameterNode;
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
