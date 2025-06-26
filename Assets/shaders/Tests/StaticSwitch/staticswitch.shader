
HEADER
{
    Description = "";
}

FEATURES
{
    #include "common/features.hlsl"
	Feature(F_FRESNEL, 0..1, "Effects");
	
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
		
	StaticCombo( S_FRESNEL, F_FRESNEL, Sys( ALL ) );
	float4 g_vColorOne < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 1.00, 0.00, 1.00, 1.00 ); >;
	float4 g_vColorTwo < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 0.00, 0.00, 0.00, 1.00 ); >;
		
	float Oscillator( float flTime, float flFrequency, float flPhase, float flStrength )
	{
		float period, amplitude, currentPhase;
	
		if(flFrequency > 0.0001f)
		{
			period = 1.0f/flFrequency;
			currentPhase = (fmod(flTime, period)*flFrequency) + flPhase/255.0f;
			amplitude = flStrength * sin(currentPhase * 3.1415926535897932f * 2.0f);
		}
		else
		{
			amplitude = flStrength;
		}
	
		return amplitude;
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
		
		
		float4 staticSwitch_0_result;
		#if ( S_FRESNEL == 1 )
		{
			float4 l_0 = g_vColorOne;
			float3 l_1 = pow( 1.0 - dot( normalize( i.vNormalWs ), normalize( CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz ) ) ), 6.300008 );
			float l_2 = Oscillator( g_flTime, 2.3, l_0.r, l_1 );
			float4 l_3 = l_0 * float4( l_2, l_2, l_2, l_2 );
			staticSwitch_0_result = l_0 * float4( l_2, l_2, l_2, l_2 );
		
		}
		#else
		{
			float4 l_4 = g_vColorTwo;
			float4 l_5 = l_4 * float4( 3, 3, 3, 3 );
			staticSwitch_0_result = l_4 * float4( 3, 3, 3, 3 );
		}
		#endif
		
		float4 l_6 = staticSwitch_0_result; 
		float4 l_7 = float4( 0, 0, 0, 1 ); 
		float4 l_8 = lerp( l_6, min( 1.0f, (l_6) + (l_7) ), 1 ); 
		
		m.Albedo = l_8.xyz;
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
