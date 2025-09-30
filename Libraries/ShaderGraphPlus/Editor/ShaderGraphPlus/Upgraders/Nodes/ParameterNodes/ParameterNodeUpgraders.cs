using System.Text.Json.Nodes;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

internal static class ParameterNodeUpgraders
{
	private static void SetEnumTypeUpgrader_v2( JsonObject json )
	{
		var name = json["Name"].ToString();
		var nodeType = ParameterNodeModeType.Constant;

		if ( !string.IsNullOrWhiteSpace( name ) )
		{
			nodeType = ParameterNodeModeType.Property;
		}
		else
		{
		}

		json[nameof( IParameterNode.ParameterNodeType )] = JsonSerializer.SerializeToNode( nodeType, ShaderGraphPlus.SerializerOptions() );
	}

	[SGPJsonUpgrader( typeof( Bool ), 2 )]
	public static void BoolNodeUpgrader_v2( JsonObject json )
	{
		if ( !json.ContainsKey( "Name" ) )
		{
			return;
		}

		try
		{
			SetEnumTypeUpgrader_v2( json );
		}
		catch
		{
		}
	}

	[SGPJsonUpgrader( typeof( Int ), 2 )]
	public static void IntNodeUpgrader_v2( JsonObject json )
	{
		if ( !json.ContainsKey( "Name" ) )
		{
			return;
		}

		try
		{
			SetEnumTypeUpgrader_v2( json );
		}
		catch
		{
		}
	}

	[SGPJsonUpgrader( typeof( Float ), 2 )]
	public static void FloatNodeUpgrader_v2( JsonObject json )
	{
		if ( !json.ContainsKey( "Name" ) )
		{
			return;
		}

		try
		{
			SetEnumTypeUpgrader_v2( json );
		}
		catch
		{
		}
	}

	[SGPJsonUpgrader( typeof( Float2 ), 2 )]
	public static void Float2NodeUpgrader_v2( JsonObject json )
	{
		if ( !json.ContainsKey( "Name" ) )
		{
			return;
		}

		try
		{
			SetEnumTypeUpgrader_v2( json );
		}
		catch
		{
		}
	}

	[SGPJsonUpgrader( typeof( Float3 ), 2 )]
	public static void Float3NodeUpgrader_v2( JsonObject json )
	{
		if ( !json.ContainsKey( "Name" ) )
		{
			return;
		}

		try
		{
			SetEnumTypeUpgrader_v2( json );
		}
		catch
		{
		}
	}

	[SGPJsonUpgrader( typeof( Float4 ), 2 )]
	public static void Float4NodeUpgrader_v2( JsonObject json )
	{
		if ( !json.ContainsKey( "Name" ) )
		{
			return;
		}

		try
		{
			SetEnumTypeUpgrader_v2( json );
		}
		catch
		{
		}
	}
}
