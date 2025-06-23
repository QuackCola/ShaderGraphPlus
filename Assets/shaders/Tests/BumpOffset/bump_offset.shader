
HEADER
{
    Description = "";
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
	CreateInputTexture2D( Normal, Srgb, 8, "None", "_normal", "Normal,0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( HeightMap, Linear, 8, "None", "_height", "Height,0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( ColorMap, Srgb, 8, "None", "_color", "Color,0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tNormal < Channel( RGBA, Box( Normal ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tHeightMap < Channel( RGBA, Box( HeightMap ), Linear ); OutputFormat( DXT1 ); SrgbRead( False ); >;
	Texture2D g_tColorMap < Channel( RGBA, Box( ColorMap ), Srgb ); OutputFormat( BC7 ); SrgbRead( True ); >;
	float g_flReferencePlane < UiGroup( "Height,0/,0/2" ); Default1( 0.42 ); Range1( 0, 1 ); >;
	float g_flDepthScale < UiGroup( "Height,0/,0/1" ); Default1( 0.125 ); Range1( 0, 1 ); >;
	bool g_bEnableBumpOffset < UiGroup( "Height,0/,0/3" ); Default( 1 ); >;
		
	float3 GetTangentViewVector( float3 vPosition, float3 vNormalWs, float3 vTangentUWs, float3 vTangentVWs )
	{
	    float3 vCameraToPositionDirWs = CalculateCameraToPositionDirWs( vPosition.xyz );
	    vNormalWs = normalize( vNormalWs.xyz );
	   	float3 vTangentViewVector = Vec3WsToTs( vCameraToPositionDirWs.xyz, vNormalWs.xyz, vTangentUWs.xyz, vTangentVWs.xyz );
		
		// Result
		return vTangentViewVector.xyz;
	}
	
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
		
		
		float2 l_0 = i.vTextureCoords.xy * float2( 1, 1 );
		float3 l_1 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float3 l_2 = GetTangentViewVector( l_1, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		float l_3 = l_2.x;
		float l_4 = l_2.y;
		float2 l_5 = float2( l_3, l_4);
		float l_6 = g_flReferencePlane;
		float2 l_7 = i.vTextureCoords.xy * float2( 1, 1 );
		float4 l_8 = g_tHeightMap.Sample( g_sSampler1,l_7 );
		float l_9 = l_6 - l_8.r;
		float2 l_10 = l_5 * float2( l_9, l_9 );
		float l_11 = g_flDepthScale;
		float2 l_12 = l_10 * float2( l_11, l_11 );
		float2 l_13 = l_12 * float2( 0.1, 0.1 );
		float2 l_14 = l_0 + l_13;
		float l_15 = l_14.x;
		float l_16 = l_14.y;
		float2 l_17 = float2( l_15, l_16);
		float2 l_18 = g_bEnableBumpOffset ? l_17 : l_7;
		float4 l_19 = g_tNormal.Sample( g_sSampler0,l_18 );
		float3 l_20 = l_19.xyz;
		float4 l_21 = g_tColorMap.Sample( g_sSampler2,l_18 );
		float l_22 = l_21.r * 1.8999925;
		
		m.Opacity = 1;
		m.Normal = l_20;
		m.Roughness = l_22;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		
		
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
