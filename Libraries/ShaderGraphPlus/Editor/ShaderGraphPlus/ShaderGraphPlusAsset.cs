namespace Editor.ShaderGraphPlus.AssetBrowser;

/// <summary>
/// For adding an option to create a default .sgrph in the Asset Browser.
/// </summary>
#if true
public static class CreateShaderGraphPlusAsset
{
    internal static void AddShaderGraphPlusOption(Menu parent, DirectoryInfo folder)
    {
        parent.AddOption($"New Shader Graph Plus Asset...", "account_tree", () =>
        {
            var ProjectCreator = new ProjectCreator();
            ProjectCreator.DeleteOnClose = true;
            ProjectCreator.FolderEditPath = folder.FullName;
            ProjectCreator.Show();
        });
    }

    [Event("folder.contextmenu", Priority = 101)]
    internal static void OnFolderContextMenu_BottomSection(FolderContextMenu e)
    {
        if (e.Target != null)
        {
            e.Menu.AddSeparator();
            AddShaderGraphPlusOption(e.Menu, e.Target);
        }
    }
}
#else
public static class CreateShaderGraphPlusAsset
{
	internal static void Create( string targetPath )
	{
		var extension = System.IO.Path.GetExtension( "$name.sgrph" );
		var template_path = ShaderGraphPlusFileSystem.FileSystem.GetFullPath( "templates/shadergraphplus.surface" );

		var sourceFile = template_path + "/$name.sgrph";
		
		if ( !System.IO.File.Exists( sourceFile ) )
			return;

		// assure extension
		targetPath = System.IO.Path.ChangeExtension( targetPath, extension );

		System.IO.File.Copy( sourceFile, targetPath );
		var asset = AssetSystem.RegisterFile( targetPath );

		MainAssetBrowser.Instance?.UpdateAssetList();

		MainAssetBrowser.Instance?.FocusOnAsset( asset );
		EditorUtility.InspectorObject = asset;
	}

	internal static void AddShaderGraphPlusOption( Menu parent, DirectoryInfo folder )
	{
		parent.AddOption( $"New Shader Graph Plus Surface Shader...", "account_tree", () =>
		{
			var extension = System.IO.Path.GetExtension( "$name.sgrph" ).Trim( '.' );

			var fd = new FileDialog( null );
			fd.Title = $"Create Shader Graph Plus";
			fd.Directory = folder.FullName;
			fd.DefaultSuffix = $".{extension}";
			fd.SelectFile( $"untitled.{extension}" );
			fd.SetFindFile();
			fd.SetModeSave();
			fd.SetNameFilter( $"Shader Graph Plus (*.{extension})" );

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

public static class CreateShaderGraphPlusAssetPP
{
	internal static void Create( string targetPath )
	{
		var extension = System.IO.Path.GetExtension( "$name.sgrph");
		var template_path = ShaderGraphPlusFileSystem.FileSystem.GetFullPath( "templates/shadergraphplus.postprocessing" );

		var sourceFile = template_path + "/$name.sgrph";

		if ( !System.IO.File.Exists( sourceFile ) )
			return;

		// assure extension
		targetPath = System.IO.Path.ChangeExtension( targetPath, extension );

		System.IO.File.Copy( sourceFile, targetPath );
		var asset = AssetSystem.RegisterFile( targetPath );

		MainAssetBrowser.Instance?.UpdateAssetList();

		MainAssetBrowser.Instance?.FocusOnAsset( asset );
		EditorUtility.InspectorObject = asset;
	}

	internal static void AddShaderGraphPlusOption( Menu parent, DirectoryInfo folder )
	{
		parent.AddOption( $"New Shader Graph Plus PostProcessing Shader...", "account_tree", () =>
		{
            var extension = System.IO.Path.GetExtension( "$name.sgrph" ).Trim( '.' );
            
            var fd = new FileDialog( null );
            fd.Title = $"Create Shader Graph Plus";
            fd.Directory = folder.FullName;
            fd.DefaultSuffix = $".{extension}";
            fd.SelectFile( $"untitled.{extension}" );
            fd.SetFindFile();
            fd.SetModeSave();
            fd.SetNameFilter( $"Shader Graph Plus (*.{extension})" );
            
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
#endif

