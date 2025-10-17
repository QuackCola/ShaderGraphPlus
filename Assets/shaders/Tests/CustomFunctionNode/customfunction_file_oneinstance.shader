
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
	#define S_ALPHA_TEST 1
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 0
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
	
	float g_flTestFloat0 < UiGroup( ",0/,0/0" ); Default1( 0 ); Range1( 0, 1 ); >;
		
	#include "Tests/CustomFunctionNode/Function0.hlsl"
	
	DynamicCombo( D_RENDER_BACKFACES, 0..1, Sys( ALL ) );
	RenderState( CullMode, D_RENDER_BACKFACES ? NONE : BACK );
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{

		
		float l_0 = g_flTestFloat0;
		float2 l_1 = i.vTextureCoords.xy * float2( 1, 1 );
		float4 ol_0 = float4( 0.0f, 0.0f, 0.0f, 0.0f );
		float4 ol_1 = float4( 0.0f, 0.0f, 0.0f, 0.0f );
		Function0( l_0, l_1, ol_0, ol_1 );
		float4 l_3 = float4( 0.31667, 0, 1, 1 );
		float l_4 = VoronoiNoise( i.vTextureCoords.xy, 3.099682, 61.599304 );
		float4 l_5 = lerp( ol_0, l_3, l_4 );
		float4 l_6 = lerp( l_5, ol_1, l_4 );
		

		return float4( l_6.xyz, 1 );
	}
}
