
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
	
	SamplerState g_sSampler0 < Filter( POINT ); AddressU( WRAP ); AddressV( WRAP ); AddressW( WRAP ); MaxAniso( 8 ); >;
	CreateInputTexture2D( IconSheet, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tIconSheet < Channel( RGBA, Box( IconSheet ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
		
	
	DynamicCombo( D_RENDER_BACKFACES, 0..1, Sys( ALL ) );
	RenderState( CullMode, D_RENDER_BACKFACES ? NONE : BACK );
		
	float2 FlipBook( float2 vUV, float flWidth, float flHeight, int nTileIndex, bool InvertX, bool InvertY )
	{
		float flTile = fmod( (float)nTileIndex, flWidth * flHeight );
		float2 InvertXY = float2( ( InvertX ? 1 : 0 ), ( InvertY ? 1 : 0 ));
	
		float2 vtileCount = float2( 1.0f, 1.0f ) / float2( flWidth, flHeight );
		float tileY = abs( InvertXY.y * flHeight - ( floor( flTile * vtileCount.x ) + InvertXY.y * 1 ) );
		float tileX = abs( InvertXY.x * flWidth - ( ( flTile - flWidth * floor( flTile * vtileCount.x) ) + InvertXY.x * 1 ) );
		return ( vUV + float2( tileX, tileY ) ) * vtileCount;
	}
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{

		
		float2 l_0 = FlipBook( i.vTextureCoords.xy, 4, 4, g_flTime, false, false );
		float4 l_1 = g_tIconSheet.Sample( g_sSampler0,l_0 );
		

		return float4( l_1.xyz, 1 );
	}
}
