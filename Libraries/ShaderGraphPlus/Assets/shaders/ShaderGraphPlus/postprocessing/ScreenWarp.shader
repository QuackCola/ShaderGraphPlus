
HEADER
{
    Description = "PostProcessing Shadergraph Material";
}

FEATURES
{
    #include "common/features.hlsl"

}

MODES
{
    VrForward();
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
		
		PixelInput i;
		i.vPositionPs = float4(v.vPositionOs.xy, 0.0f, 1.0f );
		i.vPositionWs = float3(v.vTexCoord, 0.0f);
		
		return i;
		
    }
}

PS
{
    #include "common/pixel.hlsl"
	
	Texture2D g_tColorBuffer < Attribute( "ColorBuffer" ); SrgbRead( true ); >;
	
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	Texture2D g_tWarpTexture < Attribute( "WarpTexture" ); >;
	float2 g_vWavyTime < Attribute( "WavyTime" ); Default2( 0.85,0.03 ); >;
	
    float4 MainPs( PixelInput i ) : SV_Target0
    {

		
		float2 l_0 = CalculateViewportUv( i.vPositionSs.xy );
		float2 l_1 = g_vWavyTime;
		float2 l_2 = float2( g_flTime, g_flTime ) * l_1;
		float2 l_3 = CalculateViewportUv( i.vPositionSs.xy );
		float2 l_4 = l_2 + l_3;
		float4 l_5 = g_tWarpTexture.Sample( g_sSampler0,l_4 );
		float l_6 = l_5.r * 0.03;
		float2 l_7 = l_0 + float2( l_6, l_6 );
		float3 l_8 = g_tColorBuffer.Sample( g_sAniso ,l_7 );
		

		return float4( l_8, 1 );
    }
}
