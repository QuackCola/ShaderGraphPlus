
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
	
	SamplerState g_sTestSampler < Filter( BILINEAR ); AddressU( WRAP ); AddressV( WRAP ); AddressW( WRAP ); MaxAniso( 8 ); >;
	CreateInputTexture2D( Height, Linear, 8, "None", "_height", "Height,1/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Color, Srgb, 8, "None", "_color", "Color,0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Normal, Linear, 8, "None", "_normal", "Normal,0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tHeight < Channel( RGBA, Box( Height ), Linear ); OutputFormat( ATI1N ); SrgbRead( False ); >;
	Texture2D g_tColor < Channel( RGBA, Box( Color ), Srgb ); OutputFormat( DXT1 ); SrgbRead( True ); >;
	Texture2D g_tNormal < Channel( RGBA, Box( Normal ), Linear ); OutputFormat( DXT1 ); SrgbRead( False ); >;
	TextureAttribute( LightSim_DiffuseAlbedoTexture, g_tColor )
	TextureAttribute( RepresentativeTexture, g_tColor )
	float g_flDepthScale < UiGroup( "Height,1/,0/1" ); Default1( 0.125 ); Range1( 0, 1 ); >;
	float g_flReferencePlane < UiGroup( "Height,1/,0/2" ); Default1( 0.42 ); Range1( 0, 1 ); >;
	bool g_bBumpOffset < UiGroup( ",0/,0/0" ); Default( 1 ); >;
	float g_flRoughness < UiGroup( ",0/,0/0" ); Default1( 0.19003093 ); Range1( 0, 1 ); >;
		
	
	DynamicCombo( D_RENDER_BACKFACES, 0..1, Sys( ALL ) );
	RenderState( CullMode, D_RENDER_BACKFACES ? NONE : BACK );
		
	float3 GetTangentViewVector( float3 vPosition, float3 vNormalWs, float3 vTangentUWs, float3 vTangentVWs )
	{
		float3 vCameraToPositionDirWs = CalculateCameraToPositionDirWs( vPosition.xyz );
		vNormalWs = normalize( vNormalWs.xyz );
		float3 vTangentViewVector = Vec3WsToTs( vCameraToPositionDirWs.xyz, vNormalWs.xyz, vTangentUWs.xyz, vTangentVWs.xyz );
		
		// Result
		return vTangentViewVector.xyz;
	}
	
	float2 BumpOffset( float flHeightMap, float flDepthScale, float flReferencePlane, float2 vTextureCoords, float3 vTangentViewVector )
	{
		float flHeight = flReferencePlane - flHeightMap;
	
		float2 vUVOffset = vTangentViewVector.xy * float2( flHeight, flHeight ) * float2( flDepthScale, flDepthScale ) * float2( 0.1f, 0.1f );
		float2 vDistortedUV = vTextureCoords.xy + vUVOffset;
	
		return vDistortedUV;
	}
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		
		Material m = Material::Init( i );
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
		float4 l_1 = g_tHeight.Sample( g_sTestSampler,l_0 );
		float l_2 = g_flDepthScale;
		float l_3 = g_flReferencePlane;
		float2 l_4 = BumpOffset( l_1.r, l_2, l_3, l_0, GetTangentViewVector( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz, i.vNormalWs, i.vTangentUWs, i.vTangentVWs ) );
		bool l_5 = g_bBumpOffset;
		float2 l_6 = l_5 ? l_4 : l_0;
		float4 l_7 = g_tColor.Sample( g_sTestSampler,l_6 );
		float4 l_8 = g_tNormal.Sample( g_sTestSampler,l_6 );
		float3 l_9 = l_8.xyz;
		float3 l_10 = l_9;
		float l_11 = g_flRoughness;
		
		m.Albedo = l_7.xyz;
		m.Opacity = 1;
		m.Normal = l_10;
		m.Roughness = l_11;
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
				
		return ShadingModelStandard::Shade( m );
	}
}
