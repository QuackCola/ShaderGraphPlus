
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
	
	float g_fltestfloat < Attribute( "testfloat" ); Default1( 1 ); >;
		
	void Func( float2 uv, float speed ,  out float2 resulta, out float resultb, out float4 resultc )
	{
		float3 col1 = float3(1,0,1);
		
		resulta = 1.0f;
		resultb = 0.0f;
		resultc = float4(col1,1);
		
		for(float i=0.; i<.5; i+=.01)
		{
		uv.x+= clamp(sin(2.*g_flTime*speed)*.1,-.5,.5)*.15;
		uv.y+= clamp(cos(g_flTime+i*5.)*.1,-.5,.5)*.15;
		float d = length(uv);
		float s = step(d,i)*.01;
		col1+=s;
		
		
		resultb = col1.y ;
		}
		 
		resulta = uv;
	}
	
	
    float4 MainPs( PixelInput i ) : SV_Target0
    {

		
		Gradient Gradient0 = Gradient::Init();
		
		Gradient0.colorsLength = 7;
		Gradient0.alphasLength = 2;
		Gradient0.colors[0] = float4( 1, 0, 0, 0 );
		Gradient0.colors[1] = float4( 1, 0.49804, 0, 0.17 );
		Gradient0.colors[2] = float4( 1, 1, 0, 0.33 );
		Gradient0.colors[3] = float4( 0, 1, 0, 0.5 );
		Gradient0.colors[4] = float4( 0, 0, 1, 0.67 );
		Gradient0.colors[5] = float4( 0.29412, 0, 0.5098, 0.83 );
		Gradient0.colors[6] = float4( 0.5451, 0, 1, 1 );
		Gradient0.alphas[0] = float( 1 );
		Gradient0.alphas[1] = float( 1 );
		
		float2 vl_0 = float2(0.0f,0.0f);
		float vl_1 = 0.0f;
		float4 vl_2 = float4(0.0f,0.0f,0.0f,0.0f);
		
		float2 l_0 = i.vTextureCoords.xy * float2( 1, 1 );
		float2 l_1 = TileAndOffsetUv( l_0, float2( 4, 4 ), float2( -2, -2 ) );
		float l_2 = g_fltestfloat;
		Func( l_1, l_2, vl_0, vl_1, vl_2 );
		float2 l_4 = (i.vTextureCoords.xy + g_flTime * float2( 6.599998, -9.8 ));
		float l_5 = l_4.x;
		float l_6 = VoronoiNoise( float2( vl_1, vl_1 ), 3.1415925, l_5 );
		float l_7 = clamp( l_6, 0, 1 );
		float4 l_8 = Gradient::SampleGradient( Gradient0, l_7 );
		float l_9 = saturate( l_6 );
		float4 l_10 = saturate( lerp( l_8, max( 0.0f, (l_8) - (float4( 1, 1, 1, 1 )) ), l_9 ) );
		

		return float4( l_10.xyz, 1 );
    }
}
