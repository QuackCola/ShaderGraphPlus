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


public class GraphCreator : Dialog
{
    private Button OkayButton;

    private LineEdit TitleEdit;

    private LineEdit IdentEdit;

    private Checkbox CreateGitIgnore;

    private Checkbox SetDefaultProjectLocation;

    private FolderProperty FolderEdit;

    private FieldSubtitle FolderFullPath;

    private ProjectTemplate ActiveTemplate;

    private ProjectTemplates Templates;

    //private ErrorBox FolderError;

    private bool identEdited;

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
        body.Add(new FieldTitle("Shader Name"));
        TitleEdit = body.Add(new LineEdit("", null)
        {
            PlaceholderText = "Garry's Project"
        });
        TitleEdit.Text = DefaultProjectName();
        TitleEdit.TextEdited += delegate
        {
            Validate();
        };
        body.AddSpacingCell(8f);

        body.Add(new FieldTitle("Shader Location"));
        FolderEdit = body.Add(new FolderProperty(null));
        FolderEdit.PlaceholderText = "REPLACEME";
        FolderEdit.Text = "REPLACEME";
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
        OkayButton = footer.Add(new Button("Create", "add_box", (Widget)null)
        {
            //Clicked = CreateProject // TODO : Actually make it so once the button is click we use the selected template to create a project. - Quack
        });
        OkayButton.ButtonType = "primary";
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

    private void CreateProject()
    {
        //IL_0023: Unknown result type (might be due to invalid IL or missing references)
        //IL_0029: Expected O, but got Unknown
        string addonPath = Path.Combine(FolderEdit.Text, IdentEdit.Text); 
        Directory.CreateDirectory(addonPath);
        ShaderGraphPlus config = new ShaderGraphPlus();

        //config.Ident = IdentEdit.Text;
        //config.Title = TitleEdit.Text;
        //config.Org = "local";
        //config.Type = "game";
        //config.Schema = 1;
        //Templates.ListView.ChosenTemplate?.Apply(addonPath, ref config);
        //string configPath = Path.Combine(addonPath, config.Ident + ".sbproj");
        //string txt = config.ToJson();

        //File.WriteAllText(configPath, txt);

        Close();

        //OnProjectCreated?.Invoke(configPath);
    }

}