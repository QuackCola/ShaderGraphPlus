
using System.Collections.ObjectModel;

namespace Editor.ShaderGraphPlus.Nodes;


/// <summary>
/// Custom code node TEST
/// </summary>
[Title("Custom")]
public class CustomCodeTest : ShaderNodePlus
{
    public Dictionary<string, ResultType> InputsTest { get; set; }

    public CustomCodeTest()
    {
        //InputsTestB.PropertyChanged += DictionaryUpdated();
    }

    public void DictionaryUpdated()
    {
        AddInputTest();
    }

    public void AddInputTest()
    {
        if (InputsTest.Any())
        {
    
            AddInputs(this, InputsTest);
    
            // Shit was added. Update the node now!
            Update();
        }
    }


    public override Menu CreateContextMenu(NodeUI node)
    {
        var menu = new Menu();

        // Jank way of getting the inputs to be added.
        menu.AddOption("Add Inputs Test", "add", () =>
        {
            foreach (var input in InputsTest)
            {
                //AddInput(this,input.Key, input.Value);
            }

            // Shit was added. Update now!
            Update();
        });


        return menu;
    }


}