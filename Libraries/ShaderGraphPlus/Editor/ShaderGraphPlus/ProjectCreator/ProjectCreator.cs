using MaterialDesign;
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

public class ProjectCreator : Dialog
{
    private Button OkayButton;

    private LineEdit TitleEdit;

    private FolderProperty FolderEdit;

    private FieldSubtitle FolderFullPath;

    private ProjectTemplate ActiveTemplate;

    private ProjectTemplates Templates;

    //private ErrorBox FolderError;

    public Action<string> OnProjectCreated { get; set; }

    private bool NoTemplates { get; set; } = false;

	// TODO : Add in some extra options to the template metadata. Something like the ability to further configure the selected template such as shading model and the description.
	//

    public ProjectCreator(Widget parent = null) : base(null, true)
    {
        // Set some basic window stuff.
        Window.Size = new Vector2(800, 500);
        Window.MaximumSize = Window.Size;
        Window.MinimumSize = Window.Size;
        Window.Title = "Create New Shadergraph Plus Project";
        Window.SetWindowIcon(MaterialIcons.Gradient);
        Window.SetModal(true,true);
 
        // Start laying stuff out.
        Layout = Layout.Row();
        Layout.Spacing = 4;
        var body0 = Layout.AddColumn(3,false);
        body0.Margin = 20f;
        body0.Spacing = 8f;
        body0.AddSpacingCell(8f);
        body0.Add(new Label.Subtitle("Templates"));
        body0.AddSpacingCell(12f);

        ProjectTemplates templates = body0.Add( new ProjectTemplates(this) );
        Templates = templates;

        Layout.AddSeparator();

        Layout body1 = Layout.AddColumn(2,false);
        body1.Margin = 20f;
        body1.Spacing = 8f;
        body1.AddSpacingCell(8f);
        body1.Add(new FieldTitle("Shader Graph Plus Project Setup"));
        body1.AddSpacingCell(12f);
        body1.Add(new FieldTitle("Name"));

        // Title Edit.
        TitleEdit = body1.Add(new LineEdit("", null)
        {
            PlaceholderText = "Garry's Project"
        });
        TitleEdit.Text = DefaultProjectName();
        TitleEdit.ToolTip = "Name of your Shader Graph Plus project.";
        TitleEdit.TextEdited += delegate
        {
            Validate();
        };

        body1.AddSpacingCell(8f);

        // Folder Edit.
        body1.Add(new FieldTitle("Location"));
        FolderEdit = body1.Add(new FolderProperty(null));
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
        body1.AddStretchCell(1);

        // Create button.
        Layout footer = body1.AddRow(0, false);
        footer.Spacing = 8f;
        footer.AddStretchCell(0);
        FolderFullPath = footer.Add(new FieldSubtitle(""));
        OkayButton = footer.Add(new Button.Primary("Create", "add_box", null)
        {
            Clicked = CreateProject
        });

        // Template list view for all the projects in the templates folder.
        ProjectTemplatesListView listView = Templates.ListView;
  
        listView.ItemSelected = (Action<object>)Delegate.Combine(listView.ItemSelected, (Action<object>)delegate (object item)
        {
            ActiveTemplate = item as ProjectTemplate;
        });

        // Handle situations where there is no templates found.
        if (!Diagnostics.Assert.Check(Templates.ListView.Items.Count(), 0))
        {
            ActiveTemplate = Templates.ListView.SelectedItems.First() as ProjectTemplate;
        }
        else
        {
            NoTemplates = true;
            Layout error = body1.AddRow(0, false);
            error.Spacing = 8f;
            error.AddStretchCell(0);
            var errorlabel = new Label("No Templates found!");
            errorlabel.Color = Color.Red;
            error.Add(errorlabel);
        }

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
        // No templates? then dont run the rest of the code...
        if ( NoTemplates )
        {
            return;
        }

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