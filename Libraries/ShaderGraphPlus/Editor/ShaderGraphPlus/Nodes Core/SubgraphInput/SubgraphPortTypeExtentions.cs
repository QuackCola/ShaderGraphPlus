
namespace ShaderGraphPlus.Nodes;

public static class SubgraphPortTypeExtentions
{
	public static string GetHlslType( this SubgraphPortType subgraphPortType )
	{
		return subgraphPortType switch
		{

			SubgraphPortType.Bool => "bool",
			SubgraphPortType.Float => "float",
			SubgraphPortType.Vector2 => "float2",
			SubgraphPortType.Vector3 => "float3",
			SubgraphPortType.Color => "float4",
			SubgraphPortType.Sampler => "SamplerState",
			SubgraphPortType.Texture2DObject => "Texture2D",
			_ => throw new NotImplementedException( $"Unknown PortType \"{subgraphPortType}\"" )
		};
	}
}
