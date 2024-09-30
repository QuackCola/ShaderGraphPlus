
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
	#define S_TRANSLUCENT 0
	#endif
	
	#include "common/shared.hlsl"
	#include "procedural.hlsl"
	
	#define UNLIT
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
	#include "common/test_pixel.hlsl"
	
	SamplerState g_sTestSampler < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( Height, Linear, 8, "None", "_height", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tHeight < Channel( RGBA, Box( Height ), Srgb ); OutputFormat( DXT1 ); SrgbRead( True ); >;
	float1 g_flSliceCount < UiGroup( ",0/,0/0" ); Default1( 24 ); Range1( 0, 25 ); >;
	float1 g_flSliceDistance < UiGroup( ",0/,0/0" ); Default1( 0.15 ); Range1( 0.001, 4 ); >;
	float2 g_vTexCoordScale < UiStep( 1 ); UiGroup( ",0/,0/0" ); Default2( 1,1 ); Range2( 1,1, 8,8 ); >;
		
	float3 GetTangentViewVector( float3 vPosition, float3 vNormalWs, float3 vTangentUWs, float3 vTangentVWs)
	{
	    float3 vCameraToPositionDirWs = CalculateCameraToPositionDirWs( vPosition.xyz );
	    vNormalWs = normalize( vNormalWs.xyz );
	   	float3 vTangentViewVector = Vec3WsToTs( vCameraToPositionDirWs.xyz, vNormalWs.xyz, vTangentUWs.xyz, vTangentVWs.xyz );
		
		// Result
		return vTangentViewVector.xyz;
	}
	
	float3 SimpleParallax(float flSlices, float flSliceDistance, float2 vUV, float3 vTangentViewDir, Texture2D vHeight, SamplerState vSampler)
	{
		// flSlices Default is 25.0 
		// flSliceDistance Default is 0.15 
	
		float flHeightTex = vHeight.Sample( vSampler, vUV).x; 
							
		vTangentViewDir = normalize( vTangentViewDir.xyz );
	
		float3 vResult;
	
		[loop]
		for(int i = 0; i < flSlices; i++)
		{
			if(flHeightTex > 0.1)
			{
				vResult = float3(i,0,0);
				return vResult;
			}
	
			vUV.xy += (vTangentViewDir * flSliceDistance);
			flHeightTex = vHeight.Sample( vSampler, vUV.xy ).x;
		}
	
		// Raymarch Result
		return vResult;
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
		
		float1 l_0 = g_flSliceCount;
		float1 l_1 = g_flSliceDistance;
		float2 l_2 = i.vTextureCoords.xy * float2( 1, 1 );
		float2 l_3 = g_vTexCoordScale;
		float2 l_4 = (l_2 * l_3);
		float3 l_5 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float3 l_6 = GetTangentViewVector( l_5, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		float3 l_7 = SimpleParallax( l_0, l_1, l_4, l_6, g_tHeight, g_sTestSampler );
		
		m.Albedo = l_7;
		m.Emission = l_7;
		m.Opacity = 1;
		m.Roughness = 0;
		m.Metalness = 1;
		m.AmbientOcclusion = 0;
		
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
		
		return ShadingModelStandardUnlit::Shade( i, m );
		//return float4( m.Albedo, 1 );
	}
}
