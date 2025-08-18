using Editor;

namespace ShaderGraphPlus.AssetBrowser;

/// <summary>
/// For adding an option to create a default .sgrph in the Asset Browser.
/// </summary>
public static class CreateShaderGraphPlusAsset
{
	internal static void AddShaderGraphPlusOption( Menu parent, DirectoryInfo folder )
	{
		parent.AddOption( $"New Shader Graph Plus Asset...", "account_tree", () =>
		{
			var ProjectCreator = new ProjectCreator();
			ProjectCreator.DeleteOnClose = true;
			ProjectCreator.FolderEditPath = folder.FullName;
			ProjectCreator.Show();
		});
	}
	
	[Event("folder.contextmenu", Priority = 101)]
	internal static void OnFolderContextMenu_BottomSection( FolderContextMenu e )
	{
		if ( e.Target != null )
		{
			e.Menu.AddSeparator();
			AddShaderGraphPlusOption( e.Menu, e.Target );
		}
	}
}

public static class CreateShaderGraphPlusSubgraphAsset
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
		parent.AddOption( $"New Shader Graph Plus SubGraph...", "account_tree", () =>
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
	internal static void OnFolderContextMenu_BottomSection( FolderContextMenu e )
	{
		if ( e.Target != null )
		{
			e.Menu.AddSeparator();
			AddShaderGraphPlusOption( e.Menu, e.Target );
		}
	}
}
