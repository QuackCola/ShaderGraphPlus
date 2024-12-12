namespace Editor.ShaderGraphPlus;

internal static class ShaderGraphPlusFileSystem
{
    public static BaseFileSystem Root => Editor.FileSystem.Libraries.CreateSubSystem($"/{GetLibraryFolderName()}");
    public static BaseFileSystem Content => Editor.FileSystem.Libraries.CreateSubSystem($"/{GetLibraryFolderName()}/Assets");

    /// <summary>
    /// Get's the folder name of the Shader Graph Plus library.
    /// </summary>
    /// <returns></returns>
    private static string GetLibraryFolderName()
    {
        var dev_name = "ShaderGraphPlus";
        var user_name = "quack.shadergraphplus";
        var dev_path = $"{Project.Current.GetRootPath().Replace('\\', '/')}/Libraries/{dev_name}";
        var user_path = $"{Project.Current.GetRootPath().Replace('\\', '/')}/Libraries/{user_name}";

        if (Directory.Exists(dev_path))
        {
            return dev_name;
        }
        else if (Directory.Exists(user_path))
        {
            return user_name;
        }
        else if (Directory.Exists(dev_path) || Directory.Exists(user_path))
        {
            Utilities.EdtiorSound.OhFiddleSticks();
            Log.Error($"Both dev_path & user_path exist!");
            return null;
        }
        else
        {
            return null; // Neither path exists
        }
    }
}
