
HEADER
{
	Description = "";
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
	
	CreateTexture2D( g_tColorBuffer ) < Attribute( "ColorBuffer" ); SrgbRead( true ); Filter( MIN_MAG_LINEAR_MIP_POINT ); AddressU( MIRROR ); AddressV( MIRROR ); >;
	
		
	// Code by Josh Wilson.
	float3 GetWorldSpaceNormal( float2 vUv )
	{
	    float3 pos = Depth::GetWorldPosition(vUv);
	    
	    float offsetAmount = 0.5;
	    float2 offset1 = float2(offsetAmount, 0.0);
	    float2 offset2 = float2(0.0, offsetAmount);
	    
	    float3 tangentX = (Depth::GetWorldPosition(vUv + offset1) - Depth::GetWorldPosition(vUv - offset1)) / 2.0;
	    float3 tangentY = (Depth::GetWorldPosition(vUv + offset2) - Depth::GetWorldPosition(vUv - offset2)) / 2.0;
	    
	    float3 normal = cross(tangentY, tangentX);
	
	    return lerp(float3(0.0, 0.0, 0.0), normalize(normal), step(0.01f, Depth::Get(vUv)));
	}
	
    float4 MainPs( PixelInput i ) : SV_Target0
    {
		float3 FinalColor = float3( 1, 1, 1 );
		
		float2 l_0 = i.vPositionSs.xy;
		float3 l_1 = GetWorldSpaceNormal(l_0);
		
		FinalColor = l_1;
		
        return float4(FinalColor,1.0f);
    }
}
