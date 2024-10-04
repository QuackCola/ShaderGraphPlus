
HEADER
{
	Description = "";
}

FEATURES
{
	#include "common/features.hlsl"
	Feature(F_NOISE, 0..1(0="Value",1="Voronoi"), "Noise");
	Feature(F_COLOR, 0..1(0="Red",1="Blue"), "Color");
	Feature(F_SHAPE, 0..1(0="Box",1="NoBox"), "Shape");
	
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
	
	StaticCombo( S_NOISE, F_NOISE, Sys( ALL ) );
	StaticCombo( S_COLOR, F_COLOR, Sys( ALL ) );
	StaticCombo( S_SHAPE, F_SHAPE, Sys( ALL ) );
	float4 g_vBlue < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 0.00, 0.30, 1.00, 1.00 ); >;
	float4 g_vRed < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 1.00, 0.00, 0.00, 1.00 ); >;
		
	float BoxShape( float2 UV, float Width, float Height )
	{
		float2 d = abs(UV * 2 - 1) - float2(Width, Height);
	    d = 1 - d / fwidth(d);
		return saturate(min(d.x, d.y));
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
		
		float l_0 = VoronoiNoise( i.vTextureCoords.xy, 3.1415925, 10 );
		float l_1 = ValueNoise(i.vTextureCoords.xy);
		
		#if S_NOISE == 0 
		l_0 = l_1;
		#endif
		
		float l_2 = l_0;
		float4 l_3 = g_vBlue;
		float4 l_4 = g_vRed;
		
		#if S_COLOR == 0 
		l_3 = l_4;
		#endif
		
		float4 l_5 = l_3;
		float4 l_6 = lerp( float4( l_2, l_2, l_2, l_2 ), l_5, 0.5 );
		float l_7 = BoxShape( i.vTextureCoords.xy,0.5,0.5 );
		float l_8 = 1 - l_7;
		float4 l_9 = lerp( l_6, max( 0.0f, (l_6) - (float4( 1, 1, 1, 1 )) ), l_8 );
		
		#if S_SHAPE == 0 
		l_6 = l_9;
		#endif
		
		float4 l_10 = l_6;
		
		m.Albedo = l_10.xyz;
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
