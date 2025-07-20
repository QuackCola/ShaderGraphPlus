
HEADER
{
    Description = "Red pill or the Blue pill?";
}

FEATURES
{
    #include "common/features.hlsl"

}

MODES
{
    Forward();
    Depth();
    ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#ifndef S_ALPHA_TEST
	#define S_ALPHA_TEST 0
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 0
	#endif
	
    #include "common/shared.hlsl"
    #include "common/gradient.hlsl"
    #include "procedural.hlsl"
    
    #define S_UV2 1
    #define CUSTOM_MATERIAL_INPUTS
}

struct VertexInput
{
    #include "common/vertexinput.hlsl"
    float4 vColor : COLOR0 < Semantic( Color ); >;
};

struct PixelInput
{
    #include "common/pixelinput.hlsl"
    float3 vPositionOs : TEXCOORD14;
    float3 vNormalOs : TEXCOORD15;
    float4 vTangentUOs_flTangentVSign : TANGENT	< Semantic( TangentU_SignV ); >;
    float4 vColor : COLOR0;
    float4 vTintColor : COLOR1;
};

VS
{
    #include "common/vertex.hlsl"

    PixelInput MainVs( VertexInput v )
    {
		
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		i.vColor = v.vColor;
		
		ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData( v );
		i.vTintColor = extraShaderData.vTint;
		
		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );
		return FinalizeVertex( i );
		
    }
}

PS
{
    #include "common/pixel.hlsl"
	
	DynamicCombo( D_RENDER_BACKFACES, 0..1, Sys( ALL ) );
	RenderState( CullMode, D_RENDER_BACKFACES ? NONE : BACK );
		
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	SamplerState g_sSampler1 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	SamplerState g_sSampler2 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	SamplerState g_sSampler3 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	SamplerState g_sSampler4 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( MovingTextures, Srgb, 8, "None", "_color", "Textures,0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( NotMovingMask, Srgb, 8, "None", "_color", "Textures,0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tMovingTextures < Channel( RGBA, Box( MovingTextures ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tNotMovingMask < Channel( RGBA, Box( NotMovingMask ), Srgb ); OutputFormat( BC7 ); SrgbRead( True ); >;
	TextureAttribute( LightSim_DiffuseAlbedoTexture, g_tMovingTextures )
	TextureAttribute( RepresentativeTexture, g_tMovingTextures )
	float2 g_vTileXYAmount < UiGroup( "Parameters,0/,0/3" ); Default2( 1,1 ); Range2( 0,0, 1,1 ); >;
	float g_flRainSpeed < UiType( Slider ); UiGroup( "Parameters,0/,0/4" ); Default1( 8 ); Range1( 0, 64 ); >;
	float g_flStepInCharacters < UiGroup( "Parameters,0/,0/6" ); Default1( 0.015625 ); Range1( 0, 1 ); >;
	float g_flDoubleSpeedValue < UiGroup( "Parameters,0/,0/6" ); Default1( 0.2 ); Range1( 0, 1 ); >;
	float g_flRainBrightness < UiType( Slider ); UiGroup( "Parameters,0/,0/4" ); Default1( 1 ); Range1( 0, 8 ); >;
	float4 g_vLettersColor < UiType( Color ); UiGroup( "Parameters,0/,0/0" ); Default4( 0.40, 1.00, 0.22, 1.00 ); >;
	float4 g_vLettersColorEmission < UiType( Color ); UiGroup( "Parameters,0/,0/1" ); Default4( 0.83, 1.00, 0.79, 1.00 ); >;
	bool g_bEnableTransparency < Attribute( "EnableTransparency" ); Default( 0 ); >;
	
    float4 MainPs( PixelInput i ) : SV_Target0
    {
		
		Material m = Material::Init();
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = float3( 0, 0, 1 );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;
		
		
		float2 l_0 = g_vTileXYAmount;
		float2 l_1 = TileAndOffsetUv( i.vTextureCoords.xy, l_0, float2( 0, 0 ) );
		float l_2 = l_1.x;
		float l_3 = l_1.y;
		float l_4 = g_flRainSpeed;
		float l_5 = g_flTime * l_4;
		float l_6 = g_flStepInCharacters;
		float l_7 = l_5 * l_6;
		float l_8 = l_3 - l_7;
		float2 l_9 = float2( l_2, l_8);
		float4 l_10 = g_tMovingTextures.Sample( g_sSampler0,l_9 );
		float4 l_11 = g_tNotMovingMask.Sample( g_sSampler1,l_1 );
		float l_12 = 1 - l_11.r;
		float4 l_13 = l_10 * float4( l_12, l_12, l_12, l_12 );
		float l_14 = g_flDoubleSpeedValue;
		float l_15 = l_5 * l_14;
		float l_16 = l_15 * l_6;
		float l_17 = l_3 - l_16;
		float2 l_18 = float2( l_2, l_17);
		float4 l_19 = g_tMovingTextures.Sample( g_sSampler2,l_18 );
		float4 l_20 = float4( l_11.r, l_11.r, l_11.r, l_11.r ) * l_19;
		float4 l_21 = l_13 + l_20;
		float l_22 = l_21.x;
		float l_23 = ceil( l_15 );
		float l_24 = l_23 * l_6;
		float l_25 = l_3 - l_24;
		float2 l_26 = float2( l_2, l_25);
		float4 l_27 = g_tMovingTextures.Sample( g_sSampler3,l_26 );
		float4 l_28 = float4( l_11.r, l_11.r, l_11.r, l_11.r ) * l_27;
		float l_29 = ceil( l_5 );
		float l_30 = l_29 * l_6;
		float l_31 = l_3 - l_30;
		float2 l_32 = float2( l_2, l_31);
		float4 l_33 = g_tMovingTextures.Sample( g_sSampler4,l_32 );
		float4 l_34 = float4( l_12, l_12, l_12, l_12 ) * l_33;
		float4 l_35 = l_28 + l_34;
		float l_36 = l_35.y;
		float l_37 = l_22 + l_36;
		float l_38 = g_flRainBrightness;
		float l_39 = l_11.g * l_38;
		float l_40 = l_37 * l_39;
		float4 l_41 = g_vLettersColor;
		float4 l_42 = float4( l_40, l_40, l_40, l_40 ) * l_41;
		float4 l_43 = g_vLettersColorEmission;
		float l_44 = l_36 * l_39;
		float4 l_45 = l_43 * float4( l_44, l_44, l_44, l_44 );
		float4 l_46 = l_42 + l_45;
		float4 l_47 = float4( 1, 1, 1, 1 );
		float l_48 = g_bEnableTransparency ? l_11.g : l_47.r;
		
		m.Albedo = l_46.xyz;
		m.Emission = l_46.xyz;
		m.Opacity = l_48;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 0;
		
		
		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );
		
		// Result node takes normal as tangent space, convert it to world space now
		m.Normal = TransformNormal( m.Normal, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		
		// for some toolvis shit
		m.WorldTangentU = i.vTangentUWs;
		m.WorldTangentV = i.vTangentVWs;
		m.TextureCoords = i.vTextureCoords.xy;
				
		return ShadingModelStandard::Shade( i, m );
    }
}
