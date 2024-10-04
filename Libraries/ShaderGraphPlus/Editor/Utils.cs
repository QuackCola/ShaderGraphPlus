using System.IO;

namespace Editor.ShaderGraphPlus;

public static class Utils
{
	public static string ChooseExistingPath( string path1, string path2 )
	{
		if ( Directory.Exists( path1 ) )
		{
			return path1;
		}
		else if ( Directory.Exists( path2 ) )
		{
			return path2;
		}
		else
			return null; // Neither path exists
	}

    // Opens a specified textfile in Notepad.
    public static void OpenInNotepad(string path)
    {
        Process p = new Process();
        ProcessStartInfo psi = new ProcessStartInfo("Notepad.exe", path);
        p.StartInfo = psi;
        p.Start();
    }

    /// <summary>
    /// Returns the absolute path of the current project.
    /// </summary>
    public static string GetProjectRootPath()
    {
        return Project.Current.GetRootPath().Replace('\\', '/');
    }

    public static string GetProjectCodePath()
    {
        return Project.Current.GetCodePath().Replace('\\', '/');
    }

    public static string GetShaderPath( Asset asset)
    {
        var shaderPath = string.Empty;

        var path = System.IO.Path.ChangeExtension(asset.AbsolutePath, ".shader");
        var _asset = AssetSystem.FindByPath(path);

        Log.Info($"Shader Path : {asset.Path}");
        shaderPath = asset.Path;
        
        return shaderPath;
    }

    public static class Vectors
    {
        public static object ParseVector(string vectorString)
        {
            string[] components = vectorString.Split(',');
            switch (components.Length)
            {
                case 2:
                    return new Vector2(float.Parse(components[0]), float.Parse(components[1]));
                case 3:
                    return new Vector3(float.Parse(components[0]), float.Parse(components[1]), float.Parse(components[2]));
                case 4:
                    return new Vector4(float.Parse(components[0]), float.Parse(components[1]), float.Parse(components[2]), float.Parse(components[3]));
                default:
                    throw new ArgumentException("Invalid vector string format");
            }
        }
    }

}
