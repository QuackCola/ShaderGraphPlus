
HEADER
{
    Description = "";
}

FEATURES
{
    #include "common/features.hlsl"
	Feature( F_NOISETYPE, 0..1, "Noise" );
	
}

MODES
{
    Forward();
    Depth();
    ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#ifndef SWITCH_TRUE
	#define SWITCH_TRUE 1
	#endif
	#ifndef SWITCH_FALSE
	#define SWITCH_FALSE 0
	#endif
	
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
	
	StaticCombo( S_NOISETYPE, F_NOISETYPE, Sys( ALL ) );
	
	
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
		
	StaticCombo( S_NOISETYPE, F_NOISETYPE, Sys( ALL ) );
	
	
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
		
		
		
		float4 NoiseTypeSwitchResult_Ref0;
		#if ( S_NOISETYPE == SWITCH_TRUE )
		{
			float4 l_0 = float4( 1, 0, 1, 1 );
			NoiseTypeSwitchResult_Ref0 = l_0; // result
		
		}
		#else
		{
			float4 l_0 = float4( 1, 0, 0, 1 );
			NoiseTypeSwitchResult_Ref0 = l_0; // result
		
		}
		#endif
		
		float4 l_0 = NoiseTypeSwitchResult_Ref0;
		
		float NoiseTypeSwitchResult;
		#if ( S_NOISETYPE == SWITCH_TRUE )
		{
			float l_0 = FuzzyNoise(i.vTextureCoords.xy);
			NoiseTypeSwitchResult = l_0; // result
		
		}
		#else
		{
			float l_0 = VoronoiNoise( i.vTextureCoords.xy, 3.1415925, 10 );
			NoiseTypeSwitchResult = l_0; // result
		
		}
		#endif
		
		float l_1 = NoiseTypeSwitchResult;
		float4 l_2 = l_0 * float4( l_1, l_1, l_1, l_1 );
		
		m.Albedo = l_2.xyz;
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
