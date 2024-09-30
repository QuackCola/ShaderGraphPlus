using System.IO;

namespace Editor.ShaderGraphPlus.AssetBrowser;

/// <summary>
/// For adding an option to create a default .sgrph in the Asset Browser.
/// </summary>
public static class CreateShaderGraphPlusAsset
{

	internal static void Create( string targetPath )
	{
		var extension = System.IO.Path.GetExtension( "default.sgrph" );

		var dev_path = $"{Project.Current.GetRootPath().Replace( '\\', '/' )}/Libraries/ShaderGraphPlus/templates";
		var user_path = $"{Project.Current.GetRootPath().Replace( '\\', '/' )}/Libraries/quack.shadergraphplus/templates";
		var template_path = Utils.ChooseExistingPath( dev_path, user_path ); // Choose the correct path for user or dev.

		var sourceFile = template_path + "/default.sgrph";

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
			var extension = System.IO.Path.GetExtension( "default.sgrph" ).Trim( '.' );

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
		var extension = System.IO.Path.GetExtension( "default_postprocessing.sgrph" );

		var dev_path = $"{Project.Current.GetRootPath().Replace( '\\', '/' )}/Libraries/ShaderGraphPlus/templates";
		var user_path = $"{Project.Current.GetRootPath().Replace( '\\', '/' )}/Libraries/quack.shadergraphplus/templates";
		var template_path = Utils.ChooseExistingPath( dev_path, user_path ); // Choose the correct path for user or dev.

		var sourceFile = template_path + "/default_postprocessing.sgrph";

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
			var extension = System.IO.Path.GetExtension( "default_postprocessing.sgrph" ).Trim( '.' );

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
