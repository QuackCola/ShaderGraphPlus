namespace ShaderGraphPlus;


public class ShaderFeatureBase : ISGPJsonUpgradeable
{
	[Hide, JsonPropertyName( "__version" )]
	public int Version { get; set; } = 1;

	public string Name { get; set; } = "";

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
