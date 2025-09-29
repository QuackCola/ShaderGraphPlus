namespace ShaderGraphPlus;


public class ShaderFeatureBase : ISGPJsonUpgradeable
{
	[Hide, JsonPropertyName( "__version" )]
	public int Version { get; set; } = 1;

	/// <summary>
	/// Name of this feature.
	/// </summary>
	public string FeatureName { get; set; } = "";

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

	}
}

public class ShaderFeatureBoolean : ShaderFeatureBase
{
	public ShaderFeatureBoolean() : base() { }
}

public class ShaderFeatureEnum : ShaderFeatureBase
{
	/// <summary>
	/// Name of your options. No special characters. Lowercase letters 
	/// will be made uppercase.
	/// </summary>
	public List<string> Options { get; set; } = new List<string>();

	public ShaderFeatureEnum() : base() { }

}
