using Editor;
using Editor.ShaderGraph;
using ShaderGraphPlus.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

namespace ShaderGraphPlus.Internal;

/// <summary>
/// In charge of converting a ShaderGraph project into a ShaderGraphPlus project. Use Convert() to convert a project and get the result.
/// </summary>
internal class ProjectConverter
{
	private ShaderGraph ShaderGraph { get; }
	private ShaderGraphPlus ShaderGraphPlus { get; }
	private bool IsSubgraph { get; }

	private Dictionary<Type, BaseNodeConvert> RegisterdNodes { get; set; }
	private List<(IPlugIn Plug, Editor.ShaderGraph.NodeInput Value)> ShaderGraphConnections { get; set; }

	internal ProjectConverter( ShaderGraph shaderGraph, ShaderGraphPlus shaderGraphPlus, bool isSubgraph = false )
	{
		ShaderGraph = shaderGraph;
		ShaderGraphPlus = shaderGraphPlus;
		IsSubgraph = isSubgraph;
	
		ShaderGraphConnections = new();

		RegisterConverters();
	}

	internal ShaderGraphPlus Convert()
	{
		ConvertProjectRoot();
		ConvertNodes();
		CreateConnections();

		return ShaderGraphPlus;
	}

	[Event( "hotloaded" )]
	private void RegisterConverters()
	{
		RegisterdNodes = new();

		var converters = EditorTypeLibrary.GetTypes<BaseNodeConvert>().Where( x => !x.IsAbstract );
		foreach ( var convert in converters )
		{
			var instance = EditorTypeLibrary.Create( convert.Name, convert.TargetType );

			if ( instance != null && instance is BaseNodeConvert baseNodeConvert )
			{
				SGPLog.Info( $"Created Instance of target type \"{convert.TargetType}\"" );
				RegisterdNodes.Add( baseNodeConvert.NodeTypeToConvert, baseNodeConvert );
			}
		}
	}

	private void ConvertProjectRoot()
	{
		ShaderGraphPlus.IsSubgraph = ShaderGraph.IsSubgraph;
		ShaderGraphPlus.Path = ShaderGraph.Path.Replace( !IsSubgraph ? ".shdrgrph" : ".shdrfunc", !IsSubgraph ? ".sgrph" : ".sgpfunc" );
		ShaderGraphPlus.Model = ShaderGraph.Model;
		ShaderGraphPlus.Description = ShaderGraph.Description;

		// Subgraph properties
		ShaderGraphPlus.Title = ShaderGraph.Title;
		ShaderGraphPlus.Description = ShaderGraph.Description;
		ShaderGraphPlus.Category = ShaderGraph.Category;
		ShaderGraphPlus.Icon = ShaderGraph.Icon;
		ShaderGraphPlus.AddToNodeLibrary = ShaderGraph.AddToNodeLibrary;

		switch ( ShaderGraph.BlendMode )
		{
			case Editor.ShaderGraph.BlendMode.Opaque:
				ShaderGraphPlus.BlendMode = BlendMode.Opaque;
				break;
			case Editor.ShaderGraph.BlendMode.Masked:
				ShaderGraphPlus.BlendMode = BlendMode.Masked;
				break;
			case Editor.ShaderGraph.BlendMode.Translucent:
				ShaderGraphPlus.BlendMode = BlendMode.Translucent;
				break;
		}

		switch ( ShaderGraph.ShadingModel )
		{
			case Editor.ShaderGraph.ShadingModel.Lit:
				ShaderGraphPlus.ShadingModel = ShadingModel.Lit;
				break;
			case Editor.ShaderGraph.ShadingModel.Unlit:
				ShaderGraphPlus.ShadingModel = ShadingModel.Unlit;
				break;
		}

		switch ( ShaderGraph.Domain )
		{
			case Editor.ShaderGraph.ShaderDomain.Surface:
				ShaderGraphPlus.MaterialDomain = MaterialDomain.Surface;
				break;
			case Editor.ShaderGraph.ShaderDomain.PostProcess:
				ShaderGraphPlus.MaterialDomain = MaterialDomain.PostProcess;
				break;
		}

		ShaderGraphPlus.PreviewSettings.ShowGround = ShaderGraph.PreviewSettings.ShowGround;
		ShaderGraphPlus.PreviewSettings.ShowSkybox = ShaderGraph.PreviewSettings.ShowSkybox;
		ShaderGraphPlus.PreviewSettings.EnableShadows = ShaderGraph.PreviewSettings.EnableShadows;
		ShaderGraphPlus.PreviewSettings.BackgroundColor = ShaderGraph.PreviewSettings.BackgroundColor;
		ShaderGraphPlus.PreviewSettings.Tint = ShaderGraph.PreviewSettings.Tint;
	}

	private void ConvertNodes()
	{
		var convertedNodes = new Dictionary<string, BaseNodePlus>();

		foreach ( var vanillaNode in ShaderGraph.Nodes )
		{
			if ( RegisterdNodes.TryGetValue( vanillaNode.GetType(), out var nodeConvert ) )
			{
				var newConvertedNodes = nodeConvert.Convert( this, vanillaNode );
				var connections = GetConnections( vanillaNode );

				foreach ( var convertedNode in newConvertedNodes )
				{
					convertedNodes.Add( convertedNode.Identifier, convertedNode );
				}

				ShaderGraphConnections.AddRange( connections );
			}
			else
			{
				throw new Exception( $"Node type \"{vanillaNode.GetType()}\" does not have an associated NodeConvert class" );
			}
		}

		// FIXME : This isnt perfect as if i were to replace a single node with multiple other nodes. Connections would be fucked.
		int id = 0;
		var updatedConvertedNodes = new Dictionary<string, BaseNodePlus>();

		foreach ( var convertedNode in convertedNodes.Values )
		{
			string newIdentifier = $"{id++}";
			convertedNode.Identifier = newIdentifier;

			updatedConvertedNodes[newIdentifier] = convertedNode;
		}

		convertedNodes = updatedConvertedNodes;

		// Add the converted nodes to the new graph.
		foreach ( var convertedNode in convertedNodes.Values )
		{
			ShaderGraphPlus.AddNode( convertedNode );
		}
	}

	private void CreateConnections()
	{
		foreach ( var (input, value) in ShaderGraphConnections )
		{
			var nodeA = ShaderGraphPlus.Nodes.Where( x => x.Identifier == value.Identifier ).FirstOrDefault();
			var nodeB = ShaderGraphPlus.Nodes.Where( x => x.Identifier == input.Node.Identifier ).FirstOrDefault();

			if ( nodeA != null && nodeB != null )
			{
				//SGPLog.Info( $"{ShaderGraphPlus.Path} Connecting nodeA \"{nodeA}::{value.Identifier}\" output \"{value.Output}\" to nodeB \"{nodeB}::{input.Node.Identifier}\" Input : \"{input.Identifier}\"" );
				nodeA.ConnectNode( (nodeB, input.Identifier), value.Output );
			}
		}
	}

	/// <summary>
	/// Get any valid connections of the node getting converted.
	/// </summary>
	private IEnumerable<(IPlugIn Plug, Editor.ShaderGraph.NodeInput Value)> GetConnections( ShaderGraphBaseNode vanillaNode )
	{
		List<(IPlugIn Plug, Editor.ShaderGraph.NodeInput Value)> connections = new();

		foreach ( var input in vanillaNode.Inputs )
		{
			if ( input.ConnectedOutput is not { } output )
				continue;

			var nodeInput = new Editor.ShaderGraph.NodeInput()
			{
				Identifier = output.Node.Identifier,
				Output = output.Identifier,
			};

			if ( nodeInput is { IsValid: true } )
			{
				if ( vanillaNode is Editor.ShaderGraph.SubgraphNode subgraphNode && !string.IsNullOrEmpty( subgraphNode.SubgraphPath ) )
				{
					nodeInput = new Editor.ShaderGraph.NodeInput()
					{
						Identifier = output.Node.Identifier,
						Output = output.Identifier,
						Subgraph = subgraphNode.SubgraphPath
					};
				}

				connections.Add( (input, nodeInput) );
			}
		}

		return connections;
	}
}
