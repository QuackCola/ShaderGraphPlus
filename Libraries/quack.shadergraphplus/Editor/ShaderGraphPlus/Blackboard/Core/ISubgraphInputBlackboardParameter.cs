namespace ShaderGraphPlus;

internal interface ISubgraphInputBlackboardParameter
{
	public string Name { get; set; }
	public string Description { get; set; }
	public bool IsRequired { get; set; }
	public int PortOrder { get; set; }

	public object GetValue();
}
