namespace ShaderGraphPlus;

public class ShaderFeatureBase
{
	/// <summary>
	/// Name of this feature.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// What this feature does.
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// Header Name of this Feature that shows up in the Material Editor.
	/// </summary>
	public string HeaderName { get; set; }

	public ShaderFeatureBase()
	{
		Name = "";
		Description = "";
		HeaderName = "";
	}
}

public class ShaderFeatureBoolean : ShaderFeatureBase
{
	public ShaderFeatureBoolean() : base()
	{ 

	}
}

public class ShaderFeatureEnum : ShaderFeatureBase
{
	/// <summary>
	/// Options of your feature. Must have no special characters. Note : all lowercase letters will be converted to uppercase.
	/// </summary>
	public List<string> Options { get; set; }

	public ShaderFeatureEnum() : base()
	{
		Options = new List<string>();
	}
}
