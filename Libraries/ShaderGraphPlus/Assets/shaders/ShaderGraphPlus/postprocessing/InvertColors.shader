
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
	
		
	float3 InvertColors(float3 vInput )
	{
		return float3( 1.0 - vInput.r,1.0 - vInput.g,1.0 - vInput.b );
	}
	
    float4 MainPs( PixelInput i ) : SV_Target0
    {

		
		float3 l_0 = g_tColorBuffer.Sample( g_sAniso ,CalculateViewportUv( i.vPositionSs.xy ) );
		float3 l_1 = InvertColors(l_0);
		

		return float4( l_1, 1 );
    }
}
