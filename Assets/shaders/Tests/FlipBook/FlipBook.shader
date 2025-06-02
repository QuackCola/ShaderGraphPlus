
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
	float g_flIconIndex < Attribute( "IconIndex" ); Default1( 0 ); >;
		
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

		
		
		float l_0 = g_flIconIndex;
		float2 l_1 = FlipBook( i.vTextureCoords.xy, 4, 4, l_0, float2( 0, 0 ) );
		float4 l_2 = g_tTexture_ps_0.Sample( g_sSampler0,l_1 );
		

		return float4( l_2.xyz, l_2.a );
    }
}
