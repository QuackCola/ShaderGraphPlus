
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
	#define S_TRANSLUCENT 1
	#endif
	
	#include "common/shared.hlsl"
	#include "common/gradient.hlsl"
	#include "procedural.hlsl"

	#define S_UV2 1
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
	#if ( PROGRAM == VFX_PROGRAM_PS )
		bool vFrontFacing : SV_IsFrontFace;
	#endif
	
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		
		i.vColor = v.vColor;
		
		ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData( v.nInstanceTransformID );
		i.vTintColor = extraShaderData.vTint;
		
		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );
				
		return FinalizeVertex( i );
		
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	float g_flDepthOffset < UiGroup( ",0/,0/0" ); Default1( 16 ); Range1( 0, 32 ); >;
	float g_flFalloff < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 12 ); >;
		
	
	DynamicCombo( D_RENDER_BACKFACES, 0..1, Sys( ALL ) );
	RenderState( CullMode, D_RENDER_BACKFACES ? NONE : BACK );
		
	float DepthFade( float3 vWorldPos, float3 vCameraPositionWs, float3 vCameraDirWs, float2 vUv, float flDepthOffset, float flFalloff )
	{
		float3 l_1 = vWorldPos - vCameraPositionWs;
		float l_2 = dot( l_1, normalize( vCameraDirWs ) );
	
		float depth = Depth::GetLinear( vUv );
		float l_3 = depth - l_2;
		
		return pow( saturate( l_3 / flDepthOffset ), flFalloff );
	}
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{

		
		float l_0 = g_flDepthOffset;
		float l_1 = g_flFalloff;
		float l_2 = DepthFade( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz, g_vCameraPositionWs, g_vCameraDirWs, i.vPositionSs.xy, l_0, l_1 );
		

		return float4( float3( l_2, l_2, l_2 ), 1 );
	}
}
