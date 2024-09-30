
HEADER
{
	Description = "PostProcessing Shadergraph Material";
}

MODES
{
    Default();
    VrForward();
}

FEATURES
{
}

COMMON
{
    #include "postprocess/shared.hlsl"
    #include "common/gradient.hlsl"
	#include "procedural.hlsl"
}

struct VertexInput
{
    float3 vPositionOs : POSITION < Semantic( PosXyz ); >;
    float2 vTexCoord : TEXCOORD0 < Semantic( LowPrecisionUv ); >;
};

struct PixelInput
{
    float2 vTexCoord : TEXCOORD0;

	// VS only
	#if ( PROGRAM == VFX_PROGRAM_VS )
		float4 vPositionPs		: SV_Position;
	#endif

	// PS only
	#if ( ( PROGRAM == VFX_PROGRAM_PS ) )
		float4 vPositionSs		: SV_Position;
	#endif
};

VS
{
    PixelInput MainVs( VertexInput i )
    {
        PixelInput o;
        o.vPositionPs = float4(i.vPositionOs.xyz, 1.0f);
        o.vTexCoord = i.vTexCoord;
        return o;
    }
}

PS
{
	#include "common/classes/Depth.hlsl"
    #include "postprocess/common.hlsl"
	#include "postprocess/functions.hlsl"
	#include "postprocess/PostProcessingUtils.hlsl"
	
	RenderState( DepthWriteEnable, false );
	RenderState( DepthEnable, false );
	CreateTexture2D( g_tColorBuffer ) < Attribute( "ColorBuffer" ); SrgbRead( true ); Filter( MIN_MAG_LINEAR_MIP_POINT ); AddressU( MIRROR ); AddressV( MIRROR ); >;
	CreateTexture2D( g_tDepthBuffer ) < Attribute( "DepthBuffer" ); SrgbRead( false ); Filter( MIN_MAG_MIP_POINT ); AddressU( CLAMP ); AddressV( CLAMP ); >;
	
		
	float3 InvertColors(float3 vInput )
	{
	return float3(1.0 - vInput.r,1.0 - vInput.g,1.0 - vInput.b);
	}
	
    float4 MainPs( PixelInput i ) : SV_Target0
    {
		float3 FinalColor = float3( 1, 1, 1 );
		
		float2 l_0 = CalculateViewportUv( i.vPositionSs.xy );
		float3 l_1 = Tex2D( g_tColorBuffer, l_0);
		float3 l_2 = InvertColors( l_1 );
		
		FinalColor = l_2;
		
        return float4(FinalColor,1.0f);
    }
}
