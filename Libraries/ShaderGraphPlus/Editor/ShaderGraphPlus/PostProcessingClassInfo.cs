namespace Editor.ShaderGraphPlus;

public struct PostProcessingComponentInfo
{ 
    public string ComponentTitle { get; set; }

    public string ComponentCategory { get; set; }

    public string Icon { get; set; }

    public int Order { get; set; }

    public PostProcessingComponentInfo(int order)
    { 
        //ClassTitle = title;
        //ClassCategory = category;
        //Icon = icon; 
        Order = order;
    }

}