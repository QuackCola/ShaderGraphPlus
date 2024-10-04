namespace Editor.ShaderGraphPlus;

public static class Utils
{
	
	public static class ProjectHelpers
	{
		/// <summary>
		/// Uses Regex pattern matching to fetch the Ident from the project's executing assembly name.
		/// </summary>
		public static string GetIdentFromExecutingAssemblyName()
		{
			string executingAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

			string pattern = @"^[^.]+\.[^.]+\.([^.]+)\.";

			Match match = Regex.Match( executingAssemblyName, pattern );

			var result = "";

			if ( match.Success )
			{
				result = match.Groups[1].Value;
			}

			return result;
		}


		/// <summary>
		/// Gets the org ident of the matching library package ident.
		/// </summary>
		public static string GetLibraryOrgIdent( string ident )
		{
			var libraryOrg = "";

			foreach ( var library in LibrarySystem.All )
			{
				if ( library.Project.Package.Ident == ident )
				{
					libraryOrg = library.Project.Package.Org.Ident;
				}
			}

			return libraryOrg;
		}

		/// <summary>
		/// Gets the matching library package ident.
		/// </summary>
		public static string GetLibraryPackageIdent( string ident )
		{
			var libraryIdent = "";

			foreach ( var library in LibrarySystem.All )
			{
				if ( library.Project.Package.Ident == ident )
				{
					libraryIdent = library.Project.Package.Ident;
				}
			}
			return libraryIdent;
		}
	}
	
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
		else if ( Directory.Exists( path1 ) || Directory.Exists( path2 ) )
		{
			Log.Error( $"Both path 1 & path 2 exist!" );
			return null;
		}
		else
		{
			return null; // Neither path exists
		}
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
        return GetProjectAbsolutePath();
    }

	/// <summary>
	/// Absolute path to the location of the .sbproj file of the project.
	/// </summary>
	public static string GetProjectAbsolutePath()
	{
		return Project.Current.GetRootPath().Replace( '\\', '/' );
	}

	/// <summary>
	/// Absolute path to a file or directory thats within a mounted library project.
	/// </summary>
	public static string GetLibaryAbsolutePath( string path )
	{
		return FileSystem.Libraries.GetFullPath( path ).Replace( '\\', '/' );
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
