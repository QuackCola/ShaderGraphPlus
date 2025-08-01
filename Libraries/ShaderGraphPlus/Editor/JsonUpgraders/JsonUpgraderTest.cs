using ShaderGraphPlus;
using ShaderGraphPlus.Nodes;
using System.Text.Json.Nodes;





internal static class JsonUpgraders
{
	[JsonUpgrader( typeof( SamplerNode ), 1 )]
	private static void SamplerNodeUpgrader( JsonObject json )
	{
		SGPLog.Info( $"Upgrading SamplerNode" );
	}

}
