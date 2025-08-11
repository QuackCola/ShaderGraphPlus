using Editor;

namespace ShaderGraphPlus;

public static class ShaderGraphPlusEditorMenus
{
	[Menu( "Editor", "Shader Graph Plus/Upgrade Subgraph Projects" )]
	public static void UpgradeSubgraphProjects()
	{
		var projectPaths = Directory.GetFiles( $"{Project.Current.GetAssetsPath()}/shaders", "*.sgpfunc", SearchOption.AllDirectories );
		int projectsUpgraded = 0;

		if ( projectPaths.Any() )
		{
			SGPLog.Info( $"Found \"{projectPaths.Count()}\" subgraphs" );

			foreach ( var projectPath in projectPaths )
			{
				var graph = new ShaderGraphPlus();
				var file = System.IO.File.ReadAllText( projectPath );

				if ( ShaderGraphPlus.GetProjectVersion( JsonDocument.Parse( file ).RootElement ) == 0 )
				{
					graph.Deserialize( file );

					var asset = AssetSystem.FindByPath( projectPath );

					graph.Path = asset.RelativePath;
					graph.IsSubgraph = true;

					SGPLog.Info( $"Upgraded project at path \"{projectPath}\"" );

					System.IO.File.WriteAllText( asset.AbsolutePath, graph.Serialize() );
					asset ??= AssetSystem.RegisterFile( asset.AbsolutePath );
					projectsUpgraded++;
				}
				else
				{
					SGPLog.Info( $"Project at path \"{projectPath}\" has already been upgraded." );
				}
			}

		}

		EditorUtility.DisplayDialog( "", $"Upgraded \"{projectsUpgraded}\" subgraphs." );
	}

	/*
	[Menu( "Editor", "Shader Graph Plus/Update Subgraphs internal Path String" )]
	public static void UpdateSubgraphsInternalPath()
	{
		var projectPaths = Directory.GetFiles( $"{Project.Current.GetAssetsPath()}/shaders", "*.sgpfunc", SearchOption.AllDirectories );

		if ( projectPaths.Any() )
		{
			SGPLog.Info( $"Found \"{projectPaths.Count()}\" subgraphs" );

			foreach ( var projectPath in projectPaths )
			{
				var graph = new ShaderGraphPlus();
				var file = System.IO.File.ReadAllText( projectPath );

				graph.Deserialize( file );

				var asset = AssetSystem.FindByPath( projectPath );

				var oldPath = graph.Path;
				graph.Path = asset.RelativePath;
				graph.IsSubgraph = true;

				SGPLog.Info( $"Upgraded project subgraphPath from \"{oldPath}\" to \"{graph.Path}\"" );

				System.IO.File.WriteAllText( asset.AbsolutePath, graph.Serialize() );
				asset ??= AssetSystem.RegisterFile( asset.AbsolutePath );
			}
		}

		EditorUtility.DisplayDialog( "", $"Updated \"{projectPaths.Count()}\" subgraphs internal path property." );
	}
	*/
}

