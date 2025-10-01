using Editor;

namespace ShaderGraphPlus.AssetBrowser;

internal static class CreateShaderGraphPlusAsset
{
	internal static void AddShaderGraphPlusOption( Menu parent, DirectoryInfo folder )
	{
		parent.AddOption( $"New Shader Graph Plus", "account_tree", () =>
		{
			var ProjectCreator = new ProjectCreator();
			ProjectCreator.DeleteOnClose = true;
			ProjectCreator.FolderEditPath = folder.FullName;
			ProjectCreator.Show();
		});
	}
	
	[Event("folder.contextmenu", Priority = 101)]
	internal static void OnShaderGraphPlusAssetFolderContext( FolderContextMenu e )
	{
		if ( e.Target != null )
		{
			var menu = e.Menu.FindOrCreateMenu( "New" ).FindOrCreateMenu( "Shader" );
			AddShaderGraphPlusOption( menu, e.Target );
		}
	}
}

internal static class CreateShaderGraphPlusSubgraphAsset
{
	internal static void Create( string targetPath )
	{
		var extension = System.IO.Path.GetExtension( "$name.sgpfunc" );
		var template_path = ShaderGraphPlusFileSystem.Root.GetFullPath( "templates" );

		var sourceFile = template_path + "/$name.sgpfunc";
		
		if ( !System.IO.File.Exists( sourceFile ) )
			return;

		// assure extension
		targetPath = System.IO.Path.ChangeExtension( targetPath, extension );

		System.IO.File.Copy( sourceFile, targetPath );
		var asset = AssetSystem.RegisterFile( targetPath );

		MainAssetBrowser.Instance?.Local.UpdateAssetList();
	}

	internal static void AddShaderGraphPlusOption( Menu parent, DirectoryInfo folder )
	{
		parent.AddOption( $"New Shader Graph Plus Function", "account_tree", () =>
		{
			var extension = System.IO.Path.GetExtension( "$name.sgpfunc" ).Trim( '.' );

			var fd = new FileDialog( null );
			fd.Title = $"Create Shader Graph Plus Subgraph";
			fd.Directory = folder.FullName;
			fd.DefaultSuffix = $".{extension}";
			fd.SelectFile( $"untitled.{extension}" );
			fd.SetFindFile();
			fd.SetModeSave();
			fd.SetNameFilter( $"Shader Graph Plus Subgraph (*.{extension})" );

			if ( !fd.Execute() )
				return;

			Create( fd.SelectedFile );
		} );
	}

	[Event( "folder.contextmenu", Priority = 101 )]
	internal static void OnShaderGraphPlusAssetFolderContext( FolderContextMenu e )
	{
		if ( e.Target != null )
		{
			var menu = e.Menu.FindOrCreateMenu( "New" ).FindOrCreateMenu( "Shader" );
			AddShaderGraphPlusOption( menu, e.Target );
		}
	}
}
