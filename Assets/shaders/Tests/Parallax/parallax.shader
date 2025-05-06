
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
	
	DynamicCombo( D_RENDER_BACKFACES, 0..1, Sys( ALL ) );
	RenderState( CullMode, D_RENDER_BACKFACES ? NONE : BACK );
		
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( Texture_ps_0, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tTexture_ps_0 < Channel( RGBA, Box( Texture_ps_0 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
		
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

		
		
		float2 l_0 = i.vTextureCoords.xy * float2( 1, 1 );
		float3 l_1 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float3 l_2 = GetTangentViewVector( l_1, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		float3 l_3 = SimpleParallax( 25, 0.15, l_0, l_2, g_tTexture_ps_0, TextureFiltering );
		

		return float4( l_3, 1 );
    }
}
