namespace Editor.ShaderGraphPlus;

public static class Template
{

    public static string ReadTemplate( string filePath )
    {
        string template = "";

        if (!File.Exists(filePath))
        {
            Log.Warning($"Template Dosent exist!");
        }
        else
        {
            Log.Warning($"Template exists!");

            template = File.ReadAllText(filePath);
        }


        return template;
    }
}