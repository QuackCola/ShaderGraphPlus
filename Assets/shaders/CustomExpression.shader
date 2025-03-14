
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
		
		float2 vl_0 = float2(0.0f,0.0f);
		float vl_1 = 0.0f;
		float4 vl_2 = float4(0.0f,0.0f,0.0f,0.0f);
		
		// IsVertexStage? : False
		float2 l_0 = i.vTextureCoords.xy * float2( 1, 1 );
		// IsVertexStage? : False
		float2 l_1 = TileAndOffsetUv( l_0, float2( 4, 4 ), float2( -2, -2 ) );
		// IsVertexStage? : False
		float l_2 = g_fltestfloat;
		// IsVertexStage? : False
		Func( l_1, l_2, vl_0, vl_1, vl_2 );
		// IsVertexStage? : False
		float l_4 = VoronoiNoise( vl_0, 3.1415925, 10 );
		
		m.Albedo = float3( l_4, l_4, l_4 );
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
