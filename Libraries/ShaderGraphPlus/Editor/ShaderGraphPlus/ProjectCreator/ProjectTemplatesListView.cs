﻿namespace Editor.ShaderGraphPlus;

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
            //Log.Info($"Selected ShadergraphPlus Template : {ChosenTemplate.TemplatePath}");
            //Utilities.EdtiorSound.Success();
        }
    }

    protected void FindLocalTemplates()
    {
        var template_path = ShaderGraphPlusFileSystem.Root.GetFullPath("/templates");


        if (!Directory.Exists(template_path))
        {
            return;
        }

        foreach (string template_folder in ShaderGraphPlusFileSystem.Root.FindDirectory("/templates", "*", false))
        {
            string templateRoot = "/templates/" + template_folder;
            string addonPath = templateRoot + "/$name.sgrph";

            if (ShaderGraphPlusFileSystem.Root.FileExists(addonPath))
            {
                ShaderGraphPlus shadergraphplusproject = Json.Deserialize<ShaderGraphPlus>(ShaderGraphPlusFileSystem.Root.ReadAllText(addonPath));
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
			Color fg = Color.White;

			if (Paint.HasSelected)
			{
				fg = Theme.Blue;
			}

			Paint.Antialiasing = true;
			Paint.ClearPen();
			Paint.SetBrush(Theme.TextButton.WithAlpha(0.1f));

			if (Paint.HasSelected)
			{
				Paint.SetBrush(Theme.Blue.WithAlpha(0.1f));
			}

			Paint.DrawRect(in r, 4f);

			if (Paint.HasMouseOver)
			{
				Paint.ClearPen();
				Paint.SetBrush( Color.White.WithAlpha( 0.05f ) );
				Paint.DrawRect(in r, 4f);
			}

			Paint.Antialiasing = false;

			r = r.Shrink(8f);
	
			Paint.SetPen(fg.WithAlpha(0.7f), 0f, PenStyle.Solid);
			Paint.DrawIcon(r.Align(rect.Height - 16f, TextFlag.LeftCenter), template.Icon, 24f);
			Paint.SetDefaultFont();

			Paint.SetPen(in fg, 0f, PenStyle.Solid);

			r = r.Shrink(rect.Height - 8f, 0f);

			Rect x = Paint.DrawText(in r, template.Title, TextFlag.LeftTop);

			r.Top += x.Height + 4f;

			if (Paint.HasSelected)
			{
				Paint.SetPen(Theme.Blue.WithAlpha(1f), 0f, PenStyle.Solid);
			}
			else
			{
				Paint.SetPen(Theme.TextControl.WithAlpha(0.5f), 0f, PenStyle.Solid);
			}

			r.Right = rect.Width;
			r.Left = Paint.DrawIcon(r, "info", 12f, TextFlag.LeftTop).Right + 4f;
			r.Left = Paint.DrawText(in r, template.Description, TextFlag.LeftTop).Right + 4f;
		}
	}
}
