namespace ShaderGraphPlus;

public sealed partial class GraphCompiler
{
	/// <summary>
	/// Avalible data value types that are passed between <seealso cref="ShaderNodePlus"/> nodes.
	/// Value represents if the key type is defined in editor code or not.
	/// </summary>
	internal static Dictionary<Type, bool> ValueTypes => new()
	{
		{ typeof( bool ), false },
		{ typeof( int ), false },
		{ typeof( float ), false },
		{ typeof( Vector2 ),false },
		{ typeof( Vector3 ), false },
		{ typeof( Vector4 ), false },
		{ typeof( Color ), false },
		{ typeof( Float2x2 ), true },
		{ typeof( Float3x3 ), true },
		{ typeof( Float4x4 ), true },
		{ typeof( Texture2DObject ), true },
		{ typeof( TextureCubeObject ), true },
		{ typeof( Sampler ), true },
	};

	internal static Dictionary<Type, string> HlslTypes => new()
	{
		{ typeof( bool ), "bool" },
		{ typeof( int ), "int" },
		{ typeof( float ), "float" },
		{ typeof( Vector2 ), "float2" },
		{ typeof( Vector3 ), "float3" },
		{ typeof( Vector4 ), "float4" },
		{ typeof( Color ), "float4" },
		{ typeof( Float2x2 ), "float2x2" },
		{ typeof( Float3x3 ), "float3x3" },
		{ typeof( Float4x4 ), "float4x4"},
		{ typeof( Texture2DObject ), "Texture2D" },
		{ typeof( TextureCubeObject ), "TextureCube" },
		{ typeof( Sampler ), "SamplerState" },
	};

	internal static Dictionary<Type, string> ValueTypeGlobalPrefixes => new()
	{
		{ typeof( bool ), "g_b" },
		{ typeof( int ), "g_n" },
		{ typeof( float ), "g_fl"},
		{ typeof( Vector2 ), "g_v" },
		{ typeof( Vector3 ), "g_v" },
		{ typeof( Vector4 ), "g_v" },
		{ typeof( Color ), "g_v" },
		{ typeof( Float2x2 ), "g_m" },
		{ typeof( Float3x3 ), "g_m" },
		{ typeof( Float4x4 ), "g_m" },
		{ typeof( Texture2DObject ), "g_t" },
		{ typeof( TextureCubeObject ), "g_t" },
		{ typeof( Sampler  ), "g_s" }
	};

	/// <summary>
	/// Data types that are exposed to the material editor.
	/// </summary>
	internal static List<Type> MaterialParameterTypes => new()
	{
		{ typeof( bool ) },
		{ typeof( int ) },
		{ typeof( float ) },
		{ typeof( Vector2 ) },
		{ typeof( Vector3 ) },
		{ typeof( Vector4 ) },
		{ typeof( Color ) },
		{ typeof( Texture ) },
		//{ typeof( Sampler ) },
	};

	/// <summary>
	/// Data types that can be set via <seealso cref="RenderAttributes"/>
	/// </summary>
	internal static List<Type> ShaderAttributeTypes => new()
	{
		{ typeof( bool ) },
		{ typeof( int ) },
		{ typeof( Vector2Int ) },
		{ typeof( Vector3Int ) },
		{ typeof( float ) },
		{ typeof( Double ) },
		{ typeof( Angles ) },
		{ typeof( Vector2 ) },
		{ typeof( Vector3 ) },
		{ typeof( Vector4 ) },
		{ typeof( Color ) },
		{ typeof( Texture ) },
		{ typeof( Sampler ) },
	};

	internal static HashSet<Type> ValueTypesNoDefault => new()
	{
		{ typeof( Sampler ) },
		{ typeof( Texture2DObject ) },
		{ typeof( TextureCubeObject ) },
	};
}
