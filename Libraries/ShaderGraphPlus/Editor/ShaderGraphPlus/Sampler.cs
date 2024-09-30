namespace Editor.ShaderGraphPlus;

public enum SamplerFilter
{
	Aniso,
	Bilinear,
	Trilinear,
	Point,
}

public enum SamplerAddress
{
	Wrap,
	Mirror,
	Clamp,
	Border,
	Mirror_Once,
}

public struct Sampler
{
	/// <summary>
	/// Name of this Sampler
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Smooth or Pixelated filtering
	/// </summary>
	public SamplerFilter Filter { get; set; }

	/// <summary>
	/// Horizontal wrapping, repeating or stretched
	/// </summary>
	public SamplerAddress AddressU { get; set; }

	/// <summary>
	/// Vertical wrapping, repeating or stretched
	/// </summary>
	public SamplerAddress AddressV { get; set; }

	public readonly string CreateSampler( string name )
	{
		return $"SamplerState g_s{name}";
	}
}
