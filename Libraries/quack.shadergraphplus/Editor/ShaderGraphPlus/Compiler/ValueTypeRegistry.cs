namespace ShaderGraphPlus;

internal static class ValueTypeRegistry
{

	internal static Dictionary<Type, ValueType> ValueTypes { get; private set; }

	static ValueTypeRegistry()
	{
		Update();
	}

	[Event( "hotloaded" )]
	static void Update()
	{
		ValueTypes = new()
		{
			{ typeof( bool ), new( false, true, true, "bool", "g_b" ) },
			{ typeof( int ), new( false, true, true, "int", "g_n" ) },
			{ typeof( float ), new( false, true, true, "float", "g_fl" ) },
			{ typeof( Vector2 ), new( false, true, true, "float2", "g_v" ) },
			{ typeof( Vector3 ), new( false, true, true, "float3", "g_v" ) },
			{ typeof( Vector4 ), new( false, true, true, "float4", "g_v" ) },
			{ typeof( Color ), new( false, true, true, "float4", "g_v" ) },

			{ typeof( Float2x2 ), new( true, false, false, "float2x2", "g_m" ) },
			{ typeof( Float3x3 ), new( true, false, false, "float3x3", "g_m" ) },
			{ typeof( Float4x4 ), new( true, false, false, "float4x4", "g_m" ) },

			// Just wrappers for their actual type
			{ typeof( Texture2DObject ), new( true, false, false, "Texture2D", "g_t" ) },
			{ typeof( TextureCubeObject ), new( true, false, false, "TextureCube", "g_t" ) },
			{ typeof( Sampler ), new( true, false, true, "SamplerState", "g_s" ) },
		};
	}
}

public readonly record struct ValueType
(
	bool IsEditorType,
	bool IsExposedToMaterialEditor,
	bool IsExposedToRenderAttributes,
	string HlslTypeName,
	string GlobalPrefix
);
