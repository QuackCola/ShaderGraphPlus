

using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace Editor.ShaderGraphPlus;

public static class CustomNodeLoader
{
    // Hacky fix so that users can make custom nodes until https://github.com/Facepunch/sbox-issues/issues/6284 is resloved.
    [Event( "editor.created" )]//[Event("tools.gamedata.refresh")]
    public static void OnGameDataRefresh()
    {
        var dev_path = $"{Project.Current.GetRootPath().Replace('\\', '/')}/Libraries/ShaderGraphPlus";
        var user_path = $"{Project.Current.GetRootPath().Replace('\\', '/')}/Libraries/quack.shadergraphplus";
        var lib_path = Utilities.Path.ChooseExistingPath(dev_path, user_path); // Choose the correct path for user or dev.
        string projectroot = Editor.ShaderGraphPlus.Utilities.Path.GetProjectRootPath();

        var files = System.IO.Directory.GetFiles($"{projectroot}/sgpnodes", "*.sgpnode");
        
        if (files != null)
        {
            foreach (string file in files)
            {
                var path = file.Replace('\\', '/');
                var filename = Path.GetFileNameWithoutExtension(path);

                //Log.Info($"Found file: '{filename}' at path: '{path}'");

                var destination = $"{lib_path}/Editor/Generated/ShaderGraphPlus/Nodes";

                System.IO.Directory.CreateDirectory(destination);

                System.IO.File.Copy(path, $"{destination}/{filename}.cs");
            }
        }
    }
}