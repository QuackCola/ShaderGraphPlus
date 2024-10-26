
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

    private string TemplatePath { get; init; }

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
}