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
            Log.Error($"ShaderGraphPlus : No templates found!!!");
            // Do Nothing...
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
        var template_path = ShaderGraphPlusFileSystem.FileSystem.GetFullPath("/templates");


        if (!Directory.Exists(template_path))
        {
            return;
        }

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
    }

    protected override void OnPaint()
    {
        base.OnPaint();
        //Paint.ClearPen();
        //Paint.SetBrush(Theme.WidgetBackground);
        //Paint.DrawRect(LocalRect);
    }

    protected override void PaintItem( VirtualWidget v )
	{
		object @object = v.Object;
		Rect rect = v.Rect;

        if (@object is ProjectTemplate template)
		{
			Rect r = rect;
			Color fg = Theme.White;

			if (Paint.HasSelected)
			{
				fg = Theme.Blue;
			}

			Paint.Antialiasing = true;
			Paint.ClearPen();
			Color color = Theme.ButtonDefault.WithAlpha(0.1f);
			Paint.SetBrush(in color);

			if (Paint.HasSelected)
			{
				color = Theme.Blue.WithAlpha(0.1f);
				Paint.SetBrush(in color);
			}

			Paint.DrawRect(in r, 4f);

			if (Paint.HasMouseOver)
			{
				Paint.ClearPen();
				color = Theme.White.WithAlpha(0.05f);
				Paint.SetBrush(in color);
				Paint.DrawRect(in r, 4f);
			}

			Paint.Antialiasing = false;

			float amt = 8f;
			r = r.Shrink(in amt);
			color = fg.WithAlpha(0.7f);
			amt = 0f;

			PenStyle style = PenStyle.Solid;
			Paint.SetPen(in color, in amt, in style);
			Vector2 size = rect.Height - 16f;
			Paint.DrawIcon(r.Align(in size, TextFlag.LeftCenter), template.Icon, 24f);
			Paint.SetDefaultFont();

			amt = 0f;
			style = PenStyle.Solid;

			Paint.SetPen(in fg, in amt, in style);

			amt = rect.Height - 8f;
			float y = 0f;
			r = r.Shrink(in amt, in y);

			Rect x = Paint.DrawText(in r, template.Title, TextFlag.LeftTop);

			r.Top += x.Height + 4f;

			if (Paint.HasSelected)
			{
				color = Theme.Blue.WithAlpha(1f);
				amt = 0f;
				style = PenStyle.Solid;
				Paint.SetPen(in color, in amt, in style);
			}
			else
			{
				color = Theme.ControlText.WithAlpha(0.5f);
				amt = 0f;
				style = PenStyle.Solid;
				Paint.SetPen(in color, in amt, in style);
			}

			r.Right = rect.Width;
			r.Left = Paint.DrawIcon(r, "info", 12f, TextFlag.LeftTop).Right + 4f;
			r.Left = Paint.DrawText(in r, template.Description, TextFlag.LeftTop).Right + 4f;
		}
	}
}