using Editor;
using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

namespace ShaderGraphPlus.Internal;

internal class SubgraphNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaGraph.SubgraphNode );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldSubgraphNode = oldNode as VanillaGraph.SubgraphNode;

		//SGPLog.Info( "Convert subgraph node" );

		var newNode = new SubgraphNode();
		newNode.SubgraphPath = oldSubgraphNode.SubgraphPath.Replace( ".shdrfunc", ".sgpfunc" );
		newNode.Position = oldSubgraphNode.Position;

		var fullPath = Editor.FileSystem.Content.GetFullPath( oldSubgraphNode.SubgraphPath ).Replace( ".shdrfunc", ".sgpfunc" );

		var subgraph = new VanillaGraph.ShaderGraph();
		subgraph.Deserialize( Editor.FileSystem.Content.ReadAllText( oldSubgraphNode.SubgraphPath ) );
		subgraph.Path = oldSubgraphNode.SubgraphPath;

		var subgraphPlus = new ShaderGraphPlus();
		var projectConverter = new ProjectConverter( subgraph, subgraphPlus, true );

		var conversionResult = projectConverter.Convert();

		System.IO.File.WriteAllText( fullPath, conversionResult.Serialize() );

		var asset = AssetSystem.RegisterFile( fullPath );

		if ( asset == null )
		{
			SGPLog.Error( $"Unable to register asset at path \"{fullPath}\"" );
		}
		else
		{
			SGPLog.Info( $"Registerd subgraphplus asset at path \"{fullPath}\"" );
		}

		newNodes.Add( newNode );

		return newNodes;
	}
}
