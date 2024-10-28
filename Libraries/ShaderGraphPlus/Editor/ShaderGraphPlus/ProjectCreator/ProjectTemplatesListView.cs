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

        SetItems(orderedTemplates);
        ChosenTemplate = orderedTemplates.FirstOrDefault();
        if (ChosenTemplate != null)
        {

            SelectItem(ChosenTemplate, false, false);
        }
        else
        {

        }
    }

    public void OnItemClicked(object value)
    {
        if (value is ProjectTemplate pt)
        {
            ChosenTemplate = pt;
            Log.Info($"Selected ShadergraphPlus Template : {ChosenTemplate.TemplatePath}");
            //Utilities.EdtiorSound.Success();
        }
    }

    protected void FindLocalTemplates()
    {
        //var dev_name = "ShaderGraphPlus";
        //var user_name = "quack.shadergraphplus";
        //var dev_path = $"{Project.Current.GetRootPath().Replace('\\', '/')}/Libraries/{dev_name}/templates";
        //var user_path = $"{Project.Current.GetRootPath().Replace('\\', '/')}/Libraries/{user_name}/templates";
        var template_path = ShaderGraphPlusFileSystem.FileSystem.GetFullPath("/templates");//Utilities.Path.ChooseExistingPath(dev_path, user_path); // Choose the correct path for user or dev.
        //var library_name = "";

        //if (template_path == user_path)
        //{
        //    library_name = user_name;
        //}
        //else 
        //{
        //    library_name = dev_name;
        //}


        if (!Directory.Exists(template_path))
        {
            return;
        }

        //foreach (string directory in FileSystem.Libraries.FindDirectory("/", "*", false))
        //{
            //if (directory == library_name)
            //{
                foreach (string template_folder in ShaderGraphPlusFileSystem.FileSystem.FindDirectory("/templates", "*", false))
                {
                    string templateRoot = "/templates/" + template_folder;
                    string addonPath = templateRoot + "/$name.sgrph";

                    if (ShaderGraphPlusFileSystem.FileSystem.FileExists(addonPath))
                    {
                        ShaderGraphPlus shadergraphplusproject = Json.Deserialize<ShaderGraphPlus>(ShaderGraphPlusFileSystem.FileSystem.ReadAllText(addonPath));
                        Templates.Add(new ProjectTemplate(shadergraphplusproject,templateRoot));

                    }

                }
           // }
        //}

    }

    protected override void OnPaint()
    {
        base.OnPaint();
        //Paint.ClearPen();
        //Paint.SetBrush(Theme.WidgetBackground);
        //Paint.DrawRect(LocalRect);
    }

    protected override void PaintItem(VirtualWidget v)
    {
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

            Paint.SetPen(val, num, PenStyle.Solid);
            Vector2 val3 = rect.Height - 16f;
            Paint.DrawIcon(r.Align(val3, TextFlag.CenterVertically), template.Icon, 24f, TextFlag.Center);
            Paint.SetDefaultFont(8f, 400, false, false);
            num = 0f;
            Paint.SetPen(fg,num, PenStyle.Solid);
            num = rect.Height - 8f;

            float num2 = 0f;
            r = r.Shrink(num,num2);
            Rect x = Paint.DrawText(r, template.Title, TextFlag.LeftTop);
            r.Top = r.Top + x.Height + 4f;
            if (Paint.HasSelected)
            {
                val = Theme.Blue.WithAlpha(1f);
                num = 0f;
                Paint.SetPen(val,num, PenStyle.Solid);
            }
            else
            {
                val = Theme.ControlText.WithAlpha(0.5f);
                num = 0f;
                Paint.SetPen(val,num, PenStyle.Solid);
            }

            r.Right = rect.Width;
            x = Paint.DrawIcon(r, "info", 12f, TextFlag.LeftTop);
            r.Left = x.Right + 4f;
            x = Paint.DrawText(r, template.Description, TextFlag.LeftTop);
            r.Left = x.Right + 4f;
        }
    }
}