
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
	#include "common/pixel.hlsl"
	
	RenderState(AlphaToCoverageEnable, false)
	RenderState(IndependentBlendEnable, true)
	RenderState(BlendEnable, true)
	RenderState(SrcBlend, ONE)
	RenderState(DstBlend, INV_SRC_ALPHA)
	RenderState(BlendOp, ADD)
	RenderState(SrcBlendAlpha, ONE)
	RenderState(DstBlendAlpha, INV_SRC_ALPHA)
	RenderState(BlendOpAlpha, ADD)	

	// SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D(TextureTintMask, 				Linear, 	8, "", "_mask",	"Basic,10/11",	Default3(1.0, 1.0, 1.0));
	CreateTexture2DWithoutSampler(g_tTintMask) 			< Channel(R,	Box(TextureTintMask),	Linear); OutputFormat(ATI1N); SrgbRead(false); > ;
	// Texture2D g_tTintMask < Channel( RGBA, Box( TextureTintMask ), Srgb ); OutputFormat( ATI1N ); SrgbRead( false ); >;

	float3 g_vShieldColor < UiType( Color ); UiGroup( ",0/,0/0" ); Default3( 1.00, 1.00, 1.00 ); >;
	float g_flIntersectionSharpness < UiGroup( ",0/,0/0" ); Default1( 0.2 ); Range1( 0.01, 1 ); >;
	float g_flBorderDistanceFromSphereCenter < UiGroup( ",0/,0/0" ); Default1( 3 ); Range1( 0.01, 5 ); >;
	float g_flBubbleAlphaMul < UiGroup( ",0/,0/0" ); Default1( 0.1 ); Range1( 0, 10 ); >;
	float g_flMasterAlphaMul < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;

	float3 Intersection( float3 WorldPos, float3 WorldNormal, float2 TexCoord , float2 screencoords)
	{
			float2 f2FinalTexCoord = TexCoord;//i.vTextureCoords.xy;
	
			float3 pos = WorldPos;
			float depth = Depth::GetNormalized(screencoords);
			float notSure = -abs(dot(pos, -g_vCameraDirWs)) + (1 / depth);
	
			float clampedDepth = saturate(notSure * g_flIntersectionSharpness * 0.1);
			
			float something = pow(dot(pos * rsqrt(dot(pos, pos)), WorldNormal), 2);
			float clampedSomething = saturate(something);
	
			float varDiff = something - clampedSomething;
			float sampleArg = abs((varDiff * clampedDepth + clampedSomething) * clampedDepth);
			float sample2 = abs((varDiff * clampedDepth + clampedSomething) * clampedDepth - 0.02);
	
			float alphaMul = g_flBubbleAlphaMul;
			float alpha = alphaMul / sampleArg - alphaMul;
			alpha = alpha + (alphaMul / sample2 - alphaMul);
	
			alpha = abs(alpha);
	
			float time = g_flTime;
	
			float2 offset1 = float2(time * 0.0725, time * 0.04);
			float f1TintMask = g_tTintMask.Sample( TextureFiltering, f2FinalTexCoord + offset1 ).x;
	
			float2 offset2 = float2(time * -0.0725, time * -0.04);
			float f2TintMask = g_tTintMask.Sample( TextureFiltering, f2FinalTexCoord + offset2 ).x;
	
			alpha = alpha * (f1TintMask + f2TintMask);
	
			float3 outVar = g_vShieldColor * alpha;
	
			outVar = outVar * g_flMasterAlphaMul;//g_flMasterAlphaMul;
	
			return outVar;
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
		
		float l_0 = Intersection(i.vPositionWithOffsetWs.xyz,i.vNormalWs,i.vTextureCoords.xy, i.vPositionSs);
	
		m.Albedo = float3( l_0, l_0, l_0 );
		m.Opacity = 0;
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
