namespace Editor.ShaderGraphPlus;

public abstract class ShaderNodePlus : BaseNodePlus
{
	[Hide]
	public virtual string Title => null;

	[JsonIgnore, Hide, Browsable( false )]
	public override DisplayInfo DisplayInfo
	{
		get
		{
			var info = base.DisplayInfo;
			info.Name = Title ?? info.Name;
			return info;
		}
	}
}
