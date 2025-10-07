using NodeEditorPlus;
using ShaderGraphPlus.Nodes;
using System.Linq;

namespace ShaderGraphPlus;

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
	//[Icon( "build" )] // TODO
	//Custom,
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

[AssetType( Name = "Shader Graph Plus", Extension = "sgrph", Flags = AssetTypeFlags.NoEmbedding ), Icon( "account_tree" )]
public partial class ShaderGraphPlus : INodeGraph, ISGPJsonUpgradeable
{
	[Hide]
	public int Version => 3;

	[Hide, JsonIgnore]
	public IEnumerable<BaseNodePlus> Nodes => _nodes.Values;

	[Hide, JsonIgnore]
	private readonly Dictionary<string, BaseNodePlus> _nodes = new();

	[Hide, JsonIgnore]
	IEnumerable<INodePlus> INodeGraph.Nodes => Nodes;

	[Hide, JsonIgnore]
	public IEnumerable<BaseBlackboardParameter> Parameters => _parameters.Values;

	[Hide, JsonIgnore]
	public readonly Dictionary<Guid, BaseBlackboardParameter> _parameters = new();

	// TODO : Remove this once i finish changing how you define shader features for a graph.
	[Hide, JsonIgnore]
	public Dictionary<string,ShaderFeatureInfo> Features { get; set; }

	[Hide, JsonIgnore]
	public IEnumerable<ShaderFeatureBase> FeaturesNew => _features;

	[Hide, JsonIgnore]
	private readonly List<ShaderFeatureBase> _features = new();

	[Hide]
	public bool IsSubgraph { get; set; }

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

	/// <summary>
	///   Custom key-value storage for this project.
	/// </summary>
	[Hide]
	public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

	[Hide]
	public PreviewSettings PreviewSettings { get; set; } = new();

	public ShaderGraphPlus()
	{
		UpdateUpgraders();
	}

	[Event( "hotloaded" )]
	static void UpdateUpgraders()
	{
		SGPJsonUpgrader.UpdateUpgraders( EditorTypeLibrary );
	}

	public void AssignSwitchInfo( string name, GraphCompiler.ComboSwitchInfo info )
	{
		_nodes[name].ComboSwitchInfo = info;
	}

	public void ClearSwitchInfo( string name )
	{
		if ( _nodes.ContainsKey( name ) )
			_nodes[name].ComboSwitchInfo = default;
	}

	public bool ContainsNode( string id )
	{
		if ( _nodes.ContainsKey( id ) ) return true;
		return false;
	}

	public bool ContainsParameter( Guid identifier )
	{
		if ( _parameters.ContainsKey( identifier ) ) return true;
		return false;
	}

	internal bool ContainsParameterWithName( string nameToMatch )
	{
		foreach ( var parameter in _parameters )
		{
			if ( nameToMatch == parameter.Value.Name )
				return true;
		}

		return false;
	}

	internal void UpdateParameterNode( BaseBlackboardParameter parameter )
	{
		foreach ( var iBlackboardSyncable in Nodes.OfType<IBlackboardSyncable>().Where( x => x.BlackboardParameterIdentifier == parameter.Identifier ) )
		{
			iBlackboardSyncable.UpdateFromBlackboard( parameter );
		}
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

		//SGPLog.Info( $"Removing node with id : {node.Identifier}");

		_nodes.Remove( node.Identifier );
	}

	public BaseNodePlus FindNode( string name )
	{
		_nodes.TryGetValue( name, out var node );
		return node;
	}

	internal void AddParameter( BaseBlackboardParameter parameter )
	{
		parameter.Graph = this;
		_parameters.Add( parameter.Identifier, parameter );
	}

	internal void RemoveParameter( BaseBlackboardParameter parameter )
	{
		RemoveParameter( parameter.Identifier );
	}


	internal void RemoveParameter( Guid identifier )
	{
		if ( _parameters.ContainsKey( identifier ) )
		{
			_parameters.Remove( identifier );
		}
	}

	internal BaseBlackboardParameter FindParameterByGuid( Guid guid )
	{
		if ( _parameters.TryGetValue( guid, out var parameter ) )
		{
			return parameter;
		}

		return null;
	}

	internal NamedRerouteDeclarationNode FindNamedRerouteDeclarationNode( string name )
	{
		var node = Nodes.OfType<NamedRerouteDeclarationNode>().Where( x => x.Name == name ).FirstOrDefault();

		if ( node != null )
		{
			return node;
		}

		SGPLog.Error( $"Could not find NamedReroute \"{name}\"" );

		return null;
	}

	public void ClearNodes()
	{
		_nodes.Clear();
	}

	public void ClearParameters()
	{
		_parameters.Clear();
	}

	string INodeGraph.SerializeNodes( IEnumerable<INodePlus> nodes )
	{
		return SerializeNodes( nodes.Cast<BaseNodePlus>() );
	}

	IEnumerable<INodePlus> INodeGraph.DeserializeNodes( string serialized )
	{
		return DeserializeNodes( serialized );
	}

	void INodeGraph.AddNode( INodePlus node )
	{
		AddNode( (BaseNodePlus)node );
	}

	void INodeGraph.RemoveNode( INodePlus node )
	{
		RemoveNode( (BaseNodePlus)node );
	}

	//public void AddFeature( ShaderFeatureInfo feature )
	//{
	//	if ( !Features.ContainsKey( feature.FeatureName ) )
	//	{
	//		Features.Add( feature.FeatureName, feature.FeatureString );
	//	}
	//	else
	//	{
	//		//SGPLog.Error( "Feature is already known to the graph!" );
	//	}
	//}

	/// <summary>
	/// Try to get a value at given key in <see cref="ShaderGraphPlus.Metadata"/>.
	/// </summary>
	/// <typeparam name="T">Type of the value.</typeparam>
	/// <param name="keyname">The key to retrieve the value of.</param>
	/// <param name="outvalue"> The value, if it was present in the metadata storage.</param>
	/// <returns>Whether the value was successfully retrieved.</returns>
	public bool TryGetMeta<T>( string keyname, out T outvalue )
	{
		outvalue = default( T );
		if ( Metadata == null )
		{
			return false;
		}
	
		if ( !Metadata.TryGetValue( keyname, out var value ) )
		{
			return false;
		}
	
		if ( value is T val )
		{
			outvalue = val;
			return true;
		}
	
		if (value is JsonElement element)
		{
			try
			{
				T val2 = element.Deserialize<T>( new JsonSerializerOptions() );
				outvalue = ( ( val2 != null ) ? val2 : default( T ) );
			}
			catch ( Exception )
			{
				return false;
			}
		}
	
		return true;
	}

	/// <summary>
	/// Store custom data at given key in <see cref="ShaderGraphPlus.Metadata"/>.
	/// </summary>
	/// <param name="keyname">The key for the data.</param>
	/// <param name="outvalue">The data itself to store.</param>
	/// <returns>Always true.</returns>
	public bool SetMeta( string keyname, object outvalue )
	{
		if ( Metadata == null )
		{
			Dictionary<string, object> dictionary2 = ( Metadata = new Dictionary<string, object>() );
		}
	
		if ( outvalue == null )
		{
			return Metadata.Remove( keyname );
		}

		Metadata[keyname] = outvalue;
		return true;
	}

}

[AssetType( Name = "Shader Graph Plus Function", Extension = "sgpfunc", Flags = AssetTypeFlags.NoEmbedding )]
public sealed partial class ShaderGraphPlusSubgraph : ShaderGraphPlus
{


}
