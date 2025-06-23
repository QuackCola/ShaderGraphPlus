
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
	CreateInputTexture2D( ColorMap, Srgb, 8, "None", "_color", "Color,0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( HeightMap, Linear, 8, "None", "_height", "Height,0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Normal, Srgb, 8, "None", "_normal", "Normal,0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tColorMap < Channel( RGBA, Box( ColorMap ), Srgb ); OutputFormat( BC7 ); SrgbRead( True ); >;
	Texture2D g_tHeightMap < Channel( RGBA, Box( HeightMap ), Linear ); OutputFormat( DXT1 ); SrgbRead( False ); >;
	Texture2D g_tNormal < Channel( RGBA, Box( Normal ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	TextureAttribute( LightSim_DiffuseAlbedoTexture, g_tHeightMap )
	TextureAttribute( RepresentativeTexture, g_tHeightMap )
	float g_flDepthScale < UiGroup( "Height,0/,0/1" ); Default1( 0.125 ); Range1( 0, 1 ); >;
	float g_flReferencePlane < UiGroup( "Height,0/,0/2" ); Default1( 0.42 ); Range1( 0, 1 ); >;
	bool g_bEnableBumpOffset < UiGroup( "Height,0/,0/3" ); Default( 1 ); >;
	float g_flRoughness < UiGroup( "Roughness,0/,0/3" ); Default1( 2.5 ); Range1( 0, 8 ); >;
		
	float3 GetTangentViewVector( float3 vPosition, float3 vNormalWs, float3 vTangentUWs, float3 vTangentVWs )
	{
	    float3 vCameraToPositionDirWs = CalculateCameraToPositionDirWs( vPosition.xyz );
	    vNormalWs = normalize( vNormalWs.xyz );
	   	float3 vTangentViewVector = Vec3WsToTs( vCameraToPositionDirWs.xyz, vNormalWs.xyz, vTangentUWs.xyz, vTangentVWs.xyz );
		
		// Result
		return vTangentViewVector.xyz;
	}
	
	float2 BumpOffset( float flHeightMap, float flDepthScale, float flReferencePlane, float2 vTextureCoords, float3 vTangentViewVector )
	{
			float l_10 = flReferencePlane - flHeightMap;
			float2 l_11 = vTangentViewVector.xy * float2( l_10, l_10 );
		
			float2 l_13 = l_11 * float2( flDepthScale, flDepthScale );
			float2 l_14 = l_13 * float2( 0.1f, 0.1f );
			float2 l_15 = vTextureCoords.xy + l_14;
	
			return l_15;
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
		
		
		float4 l_0 = float4( 0.39535, 0.39535, 0.39535, 1 );
		float2 l_1 = i.vTextureCoords.xy * float2( 1, 1 );
		float4 l_2 = g_tHeightMap.Sample( g_sSampler1,l_1 );
		float l_3 = g_flDepthScale;
		float l_4 = g_flReferencePlane;
		float2 l_5 = BumpOffset( l_2.r, l_3, l_4, l_1, GetTangentViewVector( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz, i.vNormalWs, i.vTangentUWs, i.vTangentVWs ) );
		float2 l_6 = g_bEnableBumpOffset ? l_5 : l_1;
		float4 l_7 = g_tColorMap.Sample( g_sSampler0,l_6 );
		float l_8 = 1 - l_7.r;
		float4 l_9 = l_0 * float4( l_8, l_8, l_8, l_8 );
		float4 l_10 = g_tNormal.Sample( g_sSampler2,l_6 );
		float3 l_11 = l_10.xyz;
		float l_12 = g_flRoughness;
		float l_13 = l_7.r * l_12;
		
		m.Albedo = l_9.xyz;
		m.Opacity = 1;
		m.Normal = l_11;
		m.Roughness = l_13;
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
