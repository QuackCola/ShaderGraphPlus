using Editor;

namespace ShaderGraphPlus;

internal static class ShaderGraphPlusFileSystem
{
	public static BaseFileSystem Root => Editor.FileSystem.Libraries.CreateSubSystem( GetLibraryName() );
	public static BaseFileSystem Content => Editor.FileSystem.Libraries.CreateSubSystem( GetLibraryName() );





	/// <summary>
	/// Get the name of the Shader Graph Plus library.
	/// </summary>
	/// <returns></returns>
	private static string GetLibraryName()
	{
		var stagingName = "quack.shadergraphplus_staging";
		var releaseName = "quack.shadergraphplus";
		var staging_path = $"{Project.Current.GetRootPath().Replace( '\\', '/' )}/Libraries/{stagingName}";
		var release_path = $"{Project.Current.GetRootPath().Replace('\\', '/')}/Libraries/{releaseName}";

		if ( Directory.Exists( staging_path ) )
		{
			return stagingName;
		}
		else if ( Directory.Exists( release_path ))
		{
			return releaseName;
		}

		// No valid path exists.
		return null;
	}
}
