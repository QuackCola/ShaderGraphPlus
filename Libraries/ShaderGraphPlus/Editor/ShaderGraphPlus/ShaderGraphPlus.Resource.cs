namespace Editor.ShaderGraphPlus;

public enum BlendMode
{
    [Icon("circle")]
    Opaque,
    [Icon("radio_button_unchecked")]
    Masked,
    [Icon("blur_on")]
    Translucent,
}

public enum ShadingModel
{
	[Icon( "tungsten" )]
	Lit,
	[Icon( "brightness_3" )]
	Unlit,
	Custom,
}

public enum MaterialDomain
{
    [Icon("view_in_ar")]
    Surface,
	[Icon("brush")]
	BlendingSurface,
    [Icon("desktop_windows")]
    PostProcess,
}

public class PreviewSettings
{
    public bool RenderBackfaces { get; set; } = false;
    public bool EnableShadows { get; set; } = true;
    public bool ShowGround { get; set; } = false;
    public bool ShowSkybox { get; set; } = true;
    public Color BackgroundColor { get; set; } = Color.Black;
    public Color Tint { get; set; } = Color.White;
}

[GameResource( "Shader Graph Plus", "sgrph", "Editor Resource", Icon = "account_tree" )]
public partial class ShaderGraphPlus : IGraph
{
	[Hide, JsonIgnore]
	public IEnumerable<BaseNodePlus> Nodes => _nodes.Values;

	[Hide, JsonIgnore]
	private readonly Dictionary<string, BaseNodePlus> _nodes = new();

	[Hide, JsonIgnore]
	IEnumerable<INode> IGraph.Nodes => Nodes;

	[Hide, JsonIgnore]
	public IEnumerable<BaseNodePlus> LightingNodes { get; set; }

	[Hide]
	public bool IsSubgraph { get; set; }

	[Hide]
	public bool HasCustomLighting { get; set; }

	[Hide]
	public string Path { get; set; }

	[Hide]
	public string Model { get; set; }

    /// <summary>
	/// The name of the Node when used in ShaderGraph
	/// </summary>
	[ShowIf( nameof( IsSubgraph ), true )]
	public string Title { get; set; }

	public string Description { get; set; }

    /// <summary>
	/// The category of the Node when browsing the Node Library (optional)
	/// </summary>
	[ShowIf( nameof( AddToNodeLibrary ), true )]
	public string Category { get; set; }

	[IconName, ShowIf( nameof( IsSubgraph ), true )]
	public string Icon { get; set; }

    /// <summary>
	/// Whether or not this Node should appear when browsing the Node Library.
	/// Otherwise can only be referenced by dragging the Subgraph asset into the graph.
	/// </summary>
	[ShowIf( nameof( IsSubgraph ), true )]
	public bool AddToNodeLibrary { get; set; }


	public BlendMode BlendMode { get; set; }

    [ShowIf( nameof( ShowShadingModel ), true )]
    public ShadingModel ShadingModel { get; set; }

    [Hide] private bool ShowShadingModel => MaterialDomain != MaterialDomain.PostProcess;

    public MaterialDomain MaterialDomain { get; set; }

    //[ShowIf( nameof( this.MaterialDomain), MaterialDomain.PostProcess  )]
    //[InlineEditor]
    //[Group("Post Processing")]
    //public PostProcessingComponentInfo postProcessComponentInfo { get; set; } = new PostProcessingComponentInfo(500);

    //
    // Summary:
    //     Custom key-value storage for this project.
    [Hide]
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

    [Hide]
    public PreviewSettings PreviewSettings { get; set; } = new();

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
    public bool SetMeta( string keyname, object outvalue )
    {
        if ( Metadata == null )
        {
            Dictionary<string, object> dictionary2 = ( Metadata = new Dictionary<string, object>() );
        }

        if ( outvalue == null )
        {
            return Metadata.Remove(keyname);
        }

        Metadata[keyname] = outvalue;
        return true;
    }

}


[GameResource("Shader Graph Plus Function", "sgpfunc", "Editor Resource", Icon = "account_tree" )]
public sealed partial class ShaderGraphPlusSubgraph : ShaderGraphPlus
{


}

