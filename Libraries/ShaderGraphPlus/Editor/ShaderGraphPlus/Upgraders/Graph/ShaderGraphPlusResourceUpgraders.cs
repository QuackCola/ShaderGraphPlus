using ShaderGraphPlus.Nodes;
using System.Text.Json.Nodes;

namespace ShaderGraphPlus;

internal static class ShaderGraphPlusResourceUpgraders
{
	[SGPJsonUpgrader( typeof( ShaderGraphPlus ), 3 )]
	public static void ShaderGraphPlusUpgrader_v3( JsonObject json )
	{
		try
		{
			SGPLog.Info( "ShaderGraphPlus v3 upgrader" );
		}
		catch
		{
		}
	}
}
