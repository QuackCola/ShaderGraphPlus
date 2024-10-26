using Sandbox.DataModel;

namespace Editor.ShaderGraphPlus;

internal class ProjectTemplatesListView : ListView
{
    private const string TemplatesDirectory = "/templates";

    private List<ProjectTemplate> Templates = new List<ProjectTemplate>();

    public ProjectTemplate Template { get; set; }


    public ProjectTemplate ChosenTemplate { get; set; }

    public ProjectTemplatesListView(Widget parent) : base(parent)
    {
        ItemSelected = OnItemClicked;
        ItemSize = new Vector2(0f, 48f);
        ItemSpacing = new Vector2( 4.0f, 4.0f);

        FindLocalTemplates();

        List<ProjectTemplate> orderedTemplates = Templates.OrderBy(x => x.Order).ToList();

        foreach (ProjectTemplate x in orderedTemplates)
        {
            Log.Info($"Project Template : {x.Title}");
        }
        
        SetItems(orderedTemplates);
        ChosenTemplate = orderedTemplates.FirstOrDefault();
        if (ChosenTemplate != null)
        {
            Log.Info($"Selected template : {ChosenTemplate}");
            SelectItem(ChosenTemplate, false, false);
        }
        else
        {
            Log.Info($"Didn't select a template");
        }
    }

    public void OnItemClicked(object value)
    {
        if (value is ProjectTemplate pt)
        {
            ChosenTemplate = pt;
        }
    }

    protected void FindLocalTemplates()
    {

        var dev_path = $"{Project.Current.GetRootPath().Replace('\\', '/')}/Libraries/ShaderGraphPlus/templates";
        var user_path = $"{Project.Current.GetRootPath().Replace('\\', '/')}/Libraries/quack.shadergraphplus/templates";
        var template_path = Utilities.Path.ChooseExistingPath(dev_path, user_path); // Choose the correct path for user or dev.


        if (!Directory.Exists(template_path))
        {
            return;
        }

        foreach (string directory in FileSystem.Libraries.FindDirectory("/", "*", false))
        {
            if (directory == "ShaderGraphPlus")
            {
                var v = FileSystem.Libraries.CreateSubSystem("/ShaderGraphPlus");
                //Log.Info(v.DirectoryExists("Assets"));
                foreach (string directory_inner in v.FindDirectory("/templates", "*", false))
                {
                    Log.Info(" Found : " + directory_inner);

                    string templateRoot = "/templates/" + directory_inner;
                    string addonPath = templateRoot + "/$name.sgrph";

                    Log.Info($"template root is : {templateRoot}");
                    Log.Info($"Addon path is :{addonPath}");
                    if (v.FileExists(addonPath))
                    {
                        Log.Info($"Addon path exists!");
                        ShaderGraphPlus addon = Json.Deserialize<ShaderGraphPlus>(v.ReadAllText(addonPath));
                        //if (addon != null && !(addon.Type == "library"))
                        //{
                        Log.Info($"TEST template root is : {v.GetFullPath(templateRoot)}");
                        //var ta = new ProjectTemplate(v.GetFullPath(templateRoot));
                        //Log.Info($" template : {ta.Description}");
                        Templates.Add(new ProjectTemplate(addon,templateRoot));

                        //}
                    }

                }
            }
        }

    }

    //protected override void OnPaint()
    //{
    //    base.OnPaint();
    //    Paint.ClearPen();
    //    Paint.SetBrush(Theme.WidgetBackground);
    //    Rect localRect = LocalRect;
    //    Paint.DrawRect(localRect);
    //}

    protected override void PaintItem(VirtualWidget v)
    {
        //IL_0007: Unknown result type (might be due to invalid IL or missing references)
        //IL_000c: Unknown result type (might be due to invalid IL or missing references)
        //IL_0017: Unknown result type (might be due to invalid IL or missing references)
        //IL_0018: Unknown result type (might be due to invalid IL or missing references)
        //IL_0019: Unknown result type (might be due to invalid IL or missing references)
        //IL_001e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0041: Unknown result type (might be due to invalid IL or missing references)
        //IL_0046: Unknown result type (might be due to invalid IL or missing references)
        //IL_0026: Unknown result type (might be due to invalid IL or missing references)
        //IL_002b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0060: Unknown result type (might be due to invalid IL or missing references)
        //IL_0065: Unknown result type (might be due to invalid IL or missing references)
        //IL_00bb: Unknown result type (might be due to invalid IL or missing references)
        //IL_00c0: Unknown result type (might be due to invalid IL or missing references)
        //IL_00c8: Unknown result type (might be due to invalid IL or missing references)
        //IL_00cd: Unknown result type (might be due to invalid IL or missing references)
        //IL_00db: Unknown result type (might be due to invalid IL or missing references)
        //IL_00f4: Unknown result type (might be due to invalid IL or missing references)
        //IL_00f9: Unknown result type (might be due to invalid IL or missing references)
        //IL_0102: Unknown result type (might be due to invalid IL or missing references)
        //IL_0117: Unknown result type (might be due to invalid IL or missing references)
        //IL_013a: Unknown result type (might be due to invalid IL or missing references)
        //IL_015f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0164: Unknown result type (might be due to invalid IL or missing references)
        //IL_016f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0174: Unknown result type (might be due to invalid IL or missing references)
        //IL_0090: Unknown result type (might be due to invalid IL or missing references)
        //IL_0095: Unknown result type (might be due to invalid IL or missing references)
        //IL_01ca: Unknown result type (might be due to invalid IL or missing references)
        //IL_01cf: Unknown result type (might be due to invalid IL or missing references)
        //IL_01dd: Unknown result type (might be due to invalid IL or missing references)
        //IL_01a2: Unknown result type (might be due to invalid IL or missing references)
        //IL_01a7: Unknown result type (might be due to invalid IL or missing references)
        //IL_01b5: Unknown result type (might be due to invalid IL or missing references)
        //IL_01f4: Unknown result type (might be due to invalid IL or missing references)
        //IL_0201: Unknown result type (might be due to invalid IL or missing references)
        //IL_0206: Unknown result type (might be due to invalid IL or missing references)
        //IL_0226: Unknown result type (might be due to invalid IL or missing references)
        //IL_022b: Unknown result type (might be due to invalid IL or missing references)
        Rect rect = v.Rect;
        if (v.Object is ProjectTemplate template)
        {
            Rect r = rect;
            Color fg = Theme.White;
            if (Paint.HasSelected)
            {
                fg = Theme.Blue;
            }
            Paint.Antialiasing = true;
            Paint.ClearPen();
            Color val = Theme.ButtonDefault.WithAlpha(0.1f);
            Paint.SetBrush(val);
            if (Paint.HasSelected)
            {
                val = Theme.Blue.WithAlpha(0.1f);
                Paint.SetBrush(val);
            }
            Paint.DrawRect(r, 4f);
            if (Paint.HasMouseOver)
            {
                Paint.ClearPen();
                val = Theme.White.WithAlpha(0.05f);
                Paint.SetBrush(val);
                Paint.DrawRect(r, 4f);
            }
            Paint.Antialiasing = false;
            float num = 8f;
            r = r.Shrink(num);
            val = fg.WithAlpha(0.7f);
            num = 0f;
            PenStyle val2 = (PenStyle)1;
            Paint.SetPen(val, num, val2);
            Vector2 val3 = rect.Height - 16f;
            Paint.DrawIcon(r.Align(val3, (TextFlag)129), template.Icon, 24f, (TextFlag)132);
            Paint.SetDefaultFont(8f, 400, false, false);
            num = 0f;
            val2 = (PenStyle)1;
            Paint.SetPen(fg,num,val2);
            num = rect.Height - 8f;
            float num2 = 0f;
            r = r.Shrink(num,num2);
            Rect x = Paint.DrawText(r, template.Title, (TextFlag)33);
            r.Top = r.Top + x.Height + 4f;
            if (Paint.HasSelected)
            {
                val = Theme.Blue.WithAlpha(1f);
                num = 0f;
                val2 = (PenStyle)1;
                Paint.SetPen(val,num,val2);
            }
            else
            {
                val = Theme.ControlText.WithAlpha(0.5f);
                num = 0f;
                val2 = (PenStyle)1;
                Paint.SetPen(val,num,val2);
            }
            r.Right = rect.Width;
            x = Paint.DrawIcon(r, "info", 12f, (TextFlag)33);
            r.Left = x.Right + 4f;
            x = Paint.DrawText(r, template.Description, (TextFlag)33);
            r.Left = x.Right + 4f;
        }
    }
}