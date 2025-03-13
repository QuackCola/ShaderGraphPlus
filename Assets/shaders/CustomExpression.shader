
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
	
	float g_fltestfloat < Attribute( "testfloat" ); Default1( 0 ); >;
		
	float4 Func( float2 uv, float speed )
	{
		float3 col1 = float3(0,0,0);
		
		
		
		for(float i=0.; i<.5; i+=.01)
		{
		uv.x+= clamp(sin(2.*g_flTime*speed)*.1,-.5,.5)*.15;
		uv.y+= clamp(cos(g_flTime+i*5.)*.1,-.5,.5)*.15;
		float d = length(uv);
		float s = step(d,i)*.01;
		col1+=s;
		
		
		
		}
		
		return float4((col1),1);
	}
	
	
    float4 MainPs( PixelInput i ) : SV_Target0
    {

		
		float2 l_0 = i.vTextureCoords.xy * float2( 1, 1 );
		float2 l_1 = TileAndOffsetUv( l_0, float2( 8, 8 ), float2( -3.199998, -4.1999993 ) );
		float l_2 = g_fltestfloat;
		float4 l_3 = Func(l_1, l_2);
		

		return float4( l_3.xyz, 1 );
    }
}
