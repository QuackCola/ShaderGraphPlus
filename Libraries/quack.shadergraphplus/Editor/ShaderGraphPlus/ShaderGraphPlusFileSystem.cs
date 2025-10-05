namespace ShaderGraphPlus;

internal static class ShaderGraphPlusFileSystem
{
	public static BaseFileSystem Root => Editor.FileSystem.Libraries.CreateSubSystem($"/{GetLibraryFolderPath()}");
	public static BaseFileSystem Content => Editor.FileSystem.Libraries.CreateSubSystem($"/{GetLibraryFolderPath()}/Assets");

	/// <summary>
	/// Get's the folder name of the Shader Graph Plus library.
	/// </summary>
	/// <returns></returns>
	private static string GetLibraryFolderPath()
	{
		var stagingName = "quack.shadergraphplus_staging";
		var releaseName = "quack.shadergraphplus";
		var staging_path = $"{Project.Current.GetRootPath().Replace( '\\', '/' )}/Libraries/{stagingName}";
		var release_path = $"{Project.Current.GetRootPath().Replace('\\', '/')}/Libraries/{releaseName}";

		var targetPath = "";

		if ( Directory.Exists( staging_path ) )
		{
			targetPath = staging_path;
		}
		else if ( Directory.Exists( release_path ))
		{
			if ( !string.IsNullOrWhiteSpace( targetPath ) )
			{
				Utilities.EdtiorSound.OhFiddleSticks();
				
				SGPLog.Error( $"release_path \"{release_path}\" was found but we already found staging_path path!. Bailing!!!!" );

				return null;
			}

			targetPath = release_path;
		}

		if ( !string.IsNullOrWhiteSpace( targetPath ) )
		{
			return targetPath;
		}

		// No valid path exists.
		return null;
	}
}
