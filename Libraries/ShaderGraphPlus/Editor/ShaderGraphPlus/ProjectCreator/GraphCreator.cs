using MaterialDesign;
using static Sandbox.VideoWriter;
namespace Editor.ShaderGraphPlus;

internal class FieldTitle : Label
{
    public FieldTitle(string title)
       : base(title, (Widget)null)
    {
    }

}

internal class FieldSubtitle : Label
{
    public FieldSubtitle(string title) : base(title, null)
    {
        WordWrap = true;
    }
}

public class GraphCreator : Dialog
{
    private Button OkayButton;

    private LineEdit TitleEdit;

    private FolderProperty FolderEdit;

    private FieldSubtitle FolderFullPath;

    private ProjectTemplate ActiveTemplate;

    private ProjectTemplates Templates;

    //private ErrorBox FolderError;

    public Action<string> OnProjectCreated { get; set; }

    public GraphCreator(Widget parent = null) : base(null, true)
    {
        Window.Size = new Vector2(800, 500);
        Window.MaximumSize = Window.Size;
        Window.MinimumSize = Window.Size;

        Window.Title = "Create New Shadergraph Plus Project";
        Window.SetWindowIcon(MaterialIcons.Gradient);
        Window.SetModal(true,true);
 

        Layout = Layout.Row();
        Layout.Spacing = 4;
        var obj = Layout.AddColumn(3,false);
        obj.AddSpacingCell(8f);
        obj.Add(new FieldTitle("Graph Templates"));
        obj.AddSpacingCell(18f);
        ProjectTemplates templates = obj.Add( new ProjectTemplates(this) );

        Templates = templates;

        Layout.AddSeparator();

        Layout body = Layout.AddColumn(2,false);
        body.Margin = 20f;
        body.Spacing = 8f;
        body.AddSpacingCell(8f);
        body.Add(new FieldTitle("Shader Graph Plus Project Setup"));
        body.AddSpacingCell(12f);
        body.Add(new FieldTitle("Name"));

        TitleEdit = body.Add(new LineEdit("", null)
        {
            PlaceholderText = "Garry's Project"
        });
        TitleEdit.Text = DefaultProjectName();
        TitleEdit.ToolTip = "Name of your Shader Graph Plus project.";
        TitleEdit.TextEdited += delegate
        {
            Validate();
        };

        body.AddSpacingCell(8f);

        body.Add(new FieldTitle("Location"));
        FolderEdit = body.Add(new FolderProperty(null));
        FolderEdit.PlaceholderText = "";
        FolderEdit.Text = $"{Project.Current.GetAssetsPath().Replace("\\","/")}/";
        FolderEdit.ToolTip = "Absolute path to where the Shader Graph Plus project will be saved to.";
        FolderEdit.TextEdited += delegate
        {
            Validate();
        };
        FolderProperty folderEdit = FolderEdit;
        folderEdit.FolderSelected = (Action<string>)Delegate.Combine(folderEdit.FolderSelected, (Action<string>)delegate
        {
            Validate();
        });
        body.AddStretchCell(1);

        Layout footer = body.AddRow(0, false);
        footer.Spacing = 8f;
        footer.AddStretchCell(0);
        FolderFullPath = footer.Add(new FieldSubtitle(""));
        OkayButton = footer.Add(new Button.Primary("Create", "add_box", null)
        {
            Clicked = CreateProject
        });

        ProjectTemplatesListView listView = Templates.ListView;
        listView.ItemSelected = (Action<object>)Delegate.Combine(listView.ItemSelected, (Action<object>)delegate (object item)
        {
            ActiveTemplate = item as ProjectTemplate;
        });

        ActiveTemplate = Templates.ListView.SelectedItems.First() as ProjectTemplate;

        Validate();
    }
    private static string DefaultProjectName()
    {
        string name = "My Shadergraph Plus Project";
        int i = 1;
        //while (Path.Exists(Path.Combine(EditorPreferences.AddonLocation, ConvertToIdent(name))))
        //{
            name = $"My Project {i++}";
        //}
        return name;
    }

    private void Validate()
    {
        bool enabled = true;

        if (string.IsNullOrWhiteSpace(FolderEdit.Text))
        {
            enabled = false;
        }

        if (string.IsNullOrWhiteSpace(TitleEdit.Text))
        {
            enabled = false;
        }

        OkayButton.Enabled = enabled;
    }

    private ShaderGraphPlus ReadTemplate(string templatePath)
    {
        var shaderGraphPlusTemplate = new ShaderGraphPlus();

        shaderGraphPlusTemplate.Deserialize(System.IO.File.ReadAllText(ShaderGraphPlusFileSystem.FileSystem.GetFullPath($"{templatePath}/$name.sgrph")));
        shaderGraphPlusTemplate.SetMeta("ProjectTemplate", null);
        
        return shaderGraphPlusTemplate;
    }


    private void CreateProject()
    {

        string shaderGraphProjectPath = FolderEdit.Text;//ShaderGraphPlusFileSystem.FileSystem.GetFullPath($"Assets/{FolderEdit.Text}");
        Directory.CreateDirectory(shaderGraphProjectPath);

        //Log.Info($"Chosen Template is : {Templates.ListView.ChosenTemplate.TemplatePath}");

        string OutputPath = Path.Combine(shaderGraphProjectPath, TitleEdit.Text + ".sgrph").Replace('\\', '/');
        string txt = ReadTemplate($"{Templates.ListView.ChosenTemplate.TemplatePath}").Serialize();
        File.WriteAllText(OutputPath, txt);

        // Register the generated project with the assetsystem.
        AssetSystem.RegisterFile(OutputPath); 

        Log.Info($"Creating ShaderGraphPlus project from : {Templates.ListView.ChosenTemplate.TemplatePath}");
        Utilities.EdtiorSound.Success();
        Close();

        OnProjectCreated?.Invoke(OutputPath);
    }

}