using Sandbox.DataModel;

namespace Editor.ShaderGraphPlus;

internal class ProjectTemplate
{
    private class DisplayData
    {
        public string Icon { get; set; }

        public int? Order { get; set; }

        public string Description { get; set; }
    }

    private ShaderGraphPlus Config { get; init; }

    public string TemplatePath { get; init; }

    public string Title => Config.Description;

    public string Icon { get; set; } = "question_mark";
    public int Order { get; set; }
    public string Description { get; set; } = "No description provided.";

    public ProjectTemplate(ShaderGraphPlus templateConfig, string path)
    {
        Config = templateConfig;
        TemplatePath = path;
        DisplayData display = default(DisplayData);

        if (Config.TryGetMeta("ProjectTemplate", out display))
        {
            Icon = display.Icon;
            Order = display.Order.GetValueOrDefault();
            Description = display.Description ?? "No description provided.";
        }
    }

    public void Apply(string targetDir, ShaderGraphPlus config, string title_test)
    {
        string sourceDir = ShaderGraphPlusFileSystem.FileSystem.GetFullPath(TemplatePath);
        //Log.Info($"sourceDir is : {sourceDir}");
        if (Directory.Exists(sourceDir))
        {
            CopyFolder(sourceDir, targetDir, title_test);
            //string ident = title_test;
            //string title = title_test;
            string fullPath = Path.Combine(sourceDir, "$name.sgrph");
            //var test = ProjectConfig
            if (Directory.Exists(fullPath))
            {
                config = JsonSerializer.Deserialize<ShaderGraphPlus>(File.ReadAllText(fullPath));
   
                config.SetMeta("ProjectTemplate", null);
            }
        }
    }

    private void CopyFolder(string sourceDir, string targetDir, string addonIdent)
    {
        if (!sourceDir.Contains("\\.", StringComparison.OrdinalIgnoreCase))
        {
            Directory.CreateDirectory(targetDir);
            string[] files = Directory.GetFiles(sourceDir);
            foreach (string file in files)
            {
                CopyAndProcessFile(file, targetDir, addonIdent);
            }
            files = Directory.GetDirectories(sourceDir);
            foreach (string directory in files)
            {
                CopyFolder(directory, Path.Combine(targetDir, Path.GetFileName(directory)), addonIdent);
            }
        }
    }

    private void CopyAndProcessFile(string file, string targetDir, string addonIdent)
    {
        string targetname = Path.Combine(targetDir, Path.GetFileName(file));
        targetname = targetname.Replace("$name", addonIdent);
        File.Copy(file, targetname, overwrite: true);
    }

}