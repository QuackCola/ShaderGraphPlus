
HEADER
{
    Description = "";
}

FEATURES
{
	#include "common/features.hlsl"
	Feature( F_PARAMETER0, 0..3 ( 0="A", 1="B", 2="C", 3="D" ), "" );
	
}

MODES
{
	Forward();
	Depth();
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#ifndef SWITCH_TRUE
	#define SWITCH_TRUE 1
	#endif
	#ifndef SWITCH_FALSE
	#define SWITCH_FALSE 0
	#endif
	
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
	
	StaticCombo( S_PARAMETER0, F_PARAMETER0, Sys( ALL ) );
	
	
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
	
	StaticCombo( S_PARAMETER0, F_PARAMETER0, Sys( ALL ) );
	
		
	
	DynamicCombo( D_RENDER_BACKFACES, 0..1, Sys( ALL ) );
	RenderState( CullMode, D_RENDER_BACKFACES ? NONE : BACK );
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{

		
		float4 Parameter0_result = float4( 0.0f, 0.0f, 0.0f, 0.0f );
		#if ( S_PARAMETER0 == 0 )
		{
			
			float4 l_0 = float4( 1, 0, 0, 1 );
			
			Parameter0_result = l_0;
		}
		#elif ( S_PARAMETER0 == 1 )
		{
			
			float4 l_0 = float4( 0, 0.9897, 0, 1 );
			
			Parameter0_result = l_0;
		}
		#elif ( S_PARAMETER0 == 2 )
		{
			
			float4 l_0 = float4( 0, 0, 1, 1 );
			
			Parameter0_result = l_0;
		}
		#elif ( S_PARAMETER0 == 3 )
		{
			
			float4 l_0 = float4( 1, 1, 1, 1 );
			
			Parameter0_result = l_0;
		}
		#endif
		
		float4 l_0 = Parameter0_result;
		

		return float4( l_0.xyz, 1 );
	}
}
