namespace ShaderGraphPlus;

public abstract class ShaderNodePlus : BaseNodePlus
{
	[Hide]
	public virtual string Title => null;

	[Hide]
	public bool IsDirty = false;

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
