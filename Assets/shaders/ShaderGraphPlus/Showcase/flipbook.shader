
HEADER
{
	Description = "Flipbook Demo";
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
	#define S_ALPHA_TEST 1
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 0
	#endif
	
	#include "common/shared.hlsl"
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
	
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( TexSheet, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tTexSheet < Channel( RGBA, Box( TexSheet ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	float1 g_flRowAmount < UiGroup( ",0/,0/0" ); Default1( 4 ); Range1( 1, 64 ); >;
	float1 g_flColumnAmount < UiGroup( ",0/,0/0" ); Default1( 4 ); Range1( 1, 64 ); >;
	float1 g_flSpeed < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 16 ); >;
		
	float2 FlipBook(float2 vUV, float flWidth, float flHeight, float flTile, float2 Invert)
	{
	    flTile = fmod(flTile, flWidth * flHeight);
	    float2 vtileCount = float2(1.0, 1.0) / float2(flWidth, flHeight);
	    float tileY = abs(Invert.y * flHeight - (floor(flTile * vtileCount.x) + Invert.y * 1));
	    float tileX = abs(Invert.x * flWidth - ((flTile - flWidth * floor(flTile * vtileCount.x)) + Invert.x * 1));
	    return (vUV + float2(tileX, tileY)) * vtileCount;
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
		
		float2 l_0 = i.vTextureCoords.xy * float2( 1, 1 );
		float1 l_1 = g_flRowAmount;
		float1 l_2 = g_flColumnAmount;
		float1 l_3 = g_flSpeed;
		float1 l_4 = g_flTime * l_3;
		float1 l_5 = floor( l_4 );
		float2 l_6 = FlipBook( l_0, l_1, l_2, l_5, float2(0,0) );
		float4 l_7 = g_tTexSheet.Sample( g_sSampler0,l_6 );
		
		m.Albedo = l_7.xyz;
		m.Opacity = 1;
		m.Roughness = 1;
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
