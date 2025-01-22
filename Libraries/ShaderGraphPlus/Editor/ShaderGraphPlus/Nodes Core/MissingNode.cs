namespace Editor.ShaderGraphPlus;

public class MissingNode : BaseNodePlus
{
	[Hide]
	public string Title { get; set; }

	[Hide]
	public string Content { get; set; }

	[Hide]
	Vector2 ContentSize = new();

	public override Color PrimaryColor => Theme.MultipleValues;

	public MissingNode( string title, JsonElement json ) : base()
	{
		Title = title;
		Content = json.ToString();

		Paint.SetDefaultFont();
		ContentSize = Paint.MeasureText( Content );
		ExpandSize = new Vector3( 30, ContentSize.y + 16 );

		InitPlugs( json );
	}

	private void InitPlugs( JsonElement json )
	{

	}

	public override void OnPaint( Rect rect )
	{
		Paint.SetDefaultFont();
		Paint.DrawText( rect.Shrink( 8, 22, 8, 8 ), Content, TextFlag.LeftTop );
	}

	[JsonIgnore, Hide, Browsable( false )]
	public override DisplayInfo DisplayInfo
	{
		get
		{
			var info = base.DisplayInfo;
			info.Name = "Missing " + Title ?? info.Name;
			info.Icon = "error";
			return info;
		}
	}
}
