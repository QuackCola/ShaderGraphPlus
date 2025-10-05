namespace ShaderGraphPlus;

public struct PostProcessingComponentInfo
{
    public bool GenerateClass { get; set; }

    [HideIf(nameof(GenerateClass), false)]
    public string ComponentTitle { get; set; }

    [HideIf(nameof(GenerateClass), false)]
    public string ComponentCategory { get; set; }

    [HideIf(nameof(GenerateClass), false)]
    public string Icon { get; set; }

    [HideIf(nameof(GenerateClass), false)]
    public int Order { get; set; }

    public PostProcessingComponentInfo(int order)
    { 
        //ClassTitle = title;
        //ClassCategory = category;
        //Icon = icon; 
        Order = order;
    }


    public bool IsValid()
    {
        if (!string.IsNullOrWhiteSpace(ComponentTitle))
        {
            return true;
        }
        else
        { 
            return false;
        }
    
    
    }

}