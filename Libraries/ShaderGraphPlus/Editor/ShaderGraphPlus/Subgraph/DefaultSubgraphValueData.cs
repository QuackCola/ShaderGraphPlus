namespace Editor.ShaderGraphPlus;

[JsonConverter( typeof( DefaultSubgraphValueDataConverter ) )]
public class DefaultSubgraphValueData
{
	//public T DefaultValue { get; set; }
	public object DefaultValue { get; set; }

	[Hide]
	public Sampler TestSampler { get; set; }

	[Sandbox.ReadOnly]
	public string Type { get; }

	public DefaultSubgraphValueData()
	{
		DefaultValue = null;
	}

	public DefaultSubgraphValueData( object value )
	{
		DefaultValue = value;
		Type = value.GetType().Name;
	}

}
