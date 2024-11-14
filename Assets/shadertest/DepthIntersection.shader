
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
	VrForward();
	Depth(); 
	ToolsVis( S_MODE_TOOLS_VIS );
	ToolsWireframe( "vr_tools_wireframe.shader" );
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
	#include "common/unlit_pixel.hlsl"
	
	float g_flDepthOffset < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 1, 64 ); >;
		
	float DepthIntersect( float3 vWorldPos, float2 vUv, float flDepthOffset )
	{
		float3 l_1 = vWorldPos - g_vCameraPositionWs;
		float l_2 = dot( l_1, g_vCameraDirWs );
		float l_3 = l_2 + flDepthOffset;
		float l_4 = Depth::GetLinear( vUv );
		float l_5 = smoothstep( l_2, l_3, l_4 );
		float result = 1 - l_5;
	
		return result;
	}
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::Init();
		m.Albedo = float3( 1, 1, 1 );
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		
		float l_0 = g_flDepthOffset;
		float l_1 = DepthIntersect(i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz, i.vPositionSs.xy, l_0);
		
		m.Opacity = l_1;
		
		m.Opacity = saturate( m.Opacity );

		return ShadingModelUnlit::Shade( i, m );
	}
}
