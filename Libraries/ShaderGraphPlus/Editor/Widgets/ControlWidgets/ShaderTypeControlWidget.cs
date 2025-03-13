namespace Editor.ShaderGraphPlus;


/// <summary>
/// 
/// </summary>
[CustomEditor(typeof(string), NamedEditor = "shadertypeplus")]
sealed class ShaderTypePlusControlWidget : DropdownControlWidget<string>
{
    public ShaderTypePlusControlWidget(SerializedProperty property) : base(property)
    {
    }

    protected override IEnumerable<object> GetDropdownValues()
    {
        List<object> list = new();
        foreach (var type in GraphCompiler.ValueTypes)
        {
            if (type.Key == typeof(float)) list.Add("float");
            else if (type.Key == typeof(int)) list.Add("int");
            else if (type.Key == typeof(bool)) list.Add("bool");
            else list.Add(type.Key);
        }
        return list;
    }
}

