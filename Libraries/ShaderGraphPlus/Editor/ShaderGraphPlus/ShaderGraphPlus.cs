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

    //
    // Summary:
    //     Custom key-value storage for this project.
    [Hide]
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();


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

    //
    // Summary:
    //     Try to get a value at given key in Editor.ShaderGraphPlus.Metadata.
    //
    //
    // Parameters:
    //   keyname:
    //     The key to retrieve the value of.
    //
    //   outvalue:
    //     The value, if it was present in the metadata storage.
    //
    // Type parameters:
    //   T:
    //     Type of the value.
    //
    // Returns:
    //     Whether the value was successfully retrieved.
    public bool TryGetMeta<T>(string keyname, out T outvalue)
    {
        outvalue = default(T);
        if (Metadata == null)
        {
            return false;
        }

        if (!Metadata.TryGetValue(keyname, out var value))
        {
            return false;
        }

        if (value is T val)
        {
            outvalue = val;
            return true;
        }

        if (value is JsonElement element)
        {
            try
            {
                T val2 = element.Deserialize<T>(new JsonSerializerOptions());
                outvalue = ((val2 != null) ? val2 : default(T));
            }
            catch (Exception)
            {
                return false;
            }
        }

        return true;
    }

    //
    // Summary:
    //     Store custom data at given key in the Editor.ShaderGraphPlus.Metadata.
    //
    //
    // Parameters:
    //   keyname:
    //     The key for the data.
    //
    //   outvalue:
    //     The data itself to store.
    //
    // Returns:
    //     Always true.
    public bool SetMeta(string keyname, object outvalue)
    {
        if (Metadata == null)
        {
            Dictionary<string, object> dictionary2 = (Metadata = new Dictionary<string, object>());
        }

        if (outvalue == null)
        {
            return Metadata.Remove(keyname);
        }

        Metadata[keyname] = outvalue;
        return true;
    }

}
