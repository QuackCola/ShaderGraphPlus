namespace Editor.ShaderGraphPlus;

public enum BlendMode
{
	Opaque,
	Masked,
	Translucent,
}

public enum ShadingModel
{
	Lit,
	Unlit,
}
public enum MaterialDomain
{
	Surface,
	PostProcess,
}


[GameResource( "Shader Graph Plus", "sgrph", "Editor Resource", Icon = "account_tree" )]
public sealed partial class ShaderGraphPlus : IGraph
{
	[Hide, JsonIgnore]
	public IEnumerable<BaseNodePlus> Nodes => _nodes.Values;

	[Hide, JsonIgnore]
	private readonly Dictionary<string, BaseNodePlus> _nodes = new();

	[Hide, JsonIgnore]
	IEnumerable<INode> IGraph.Nodes => Nodes;

	[Hide]
	public string Model { get; set; }

	public string Description { get; set; }

	[HideIf( nameof( MaterialDomain ), MaterialDomain.PostProcess )]
	public BlendMode BlendMode { get; set; }

	[HideIf( nameof ( MaterialDomain ), MaterialDomain.PostProcess )]
	public ShadingModel ShadingModel { get; set; }

	public MaterialDomain MaterialDomain { get; set; }

    [HideIf( nameof(MaterialDomain), MaterialDomain.Surface  )]
    [InlineEditor]
    [Group("Post Processing")]
    public PostProcessingComponentInfo postProcessComponentInfo { get; set; } = new PostProcessingComponentInfo(500);

	[Hide, JsonIgnore]
	public List<string> MissingNodes { get; set; } = new();

    public ShaderGraphPlus()
	{
	}

	public void AddNode( BaseNodePlus node )
	{
		node.Graph = this;
		_nodes.Add( node.Identifier, node );
	}

	public void RemoveNode( BaseNodePlus node )
	{
		if ( node.Graph != this )
			return;

		_nodes.Remove( node.Identifier );
	}

	public BaseNodePlus FindNode( string name )
	{
		_nodes.TryGetValue( name, out var node );
		return node;
	}

	public void ClearNodes()
	{
		_nodes.Clear();
	}

	

	string IGraph.SerializeNodes( IEnumerable<INode> nodes )
	{
		return SerializeNodes( nodes.Cast<BaseNodePlus>() );
	}

	IEnumerable<INode> IGraph.DeserializeNodes( string serialized )
	{
		return DeserializeNodes( serialized );
	}

	void IGraph.AddNode( INode node )
	{
		AddNode( (BaseNodePlus)node );
	}

	void IGraph.RemoveNode( INode node )
	{
		RemoveNode( (BaseNodePlus)node );
	}
}
