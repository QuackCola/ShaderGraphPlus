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
		var dev_name = "ShaderGraphPlus";
		var user_staging = "quack.shadergraphplus_staging";
		var user_name = "quack.shadergraphplus";
		var dev_path = $"{Project.Current.GetRootPath().Replace('\\', '/')}/Libraries/{dev_name}";
		var user_staging_path = $"{Project.Current.GetRootPath().Replace( '\\', '/' )}/Libraries/{user_staging}";
		var user_path = $"{Project.Current.GetRootPath().Replace('\\', '/')}/Libraries/{user_name}";

		var targetPath = "";

		if ( Directory.Exists( dev_path ) )
		{
			targetPath = dev_name;
		}
		else if ( Directory.Exists( user_staging_path ) )
		{
			if ( !string.IsNullOrWhiteSpace( targetPath ) )
			{
				Utilities.EdtiorSound.OhFiddleSticks();

				Log.Error( $"user_staging_path \"{user_staging_path}\" was found but we already selected a path!. Bailing!!!!" );

				return null;
			}

			targetPath = user_staging_path;
		}
		else if ( Directory.Exists( user_path ))
		{
			if ( !string.IsNullOrWhiteSpace( targetPath ) )
			{
				Utilities.EdtiorSound.OhFiddleSticks();
				
				Log.Error( $"user_path \"{user_path}\" was found but we already selected a path!. Bailing!!!!" );

				return null;
			}

			targetPath = user_name;
		}

		if ( !string.IsNullOrWhiteSpace( targetPath ) )
		{
			return targetPath;
		}

		// No valid path exists.
		return null;
	}
}
