
HEADER
{
    Description = "";
}

FEATURES
{
    #include "common/features.hlsl"
	Feature( F_FRESNEL, 0..1, "Effects" );
	
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
	float g_flFresnelPower < UiGroup( ",0/,0/0" ); Default1( 14.643593 ); Range1( 0, 32 ); >;
	float4 g_vColorOne < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 1.00, 0.00, 1.00, 1.00 ); >;
		
	float3 InvertColors( float3 vColor )
	{
		return float3( 1.0 - vColor.r, 1.0 - vColor.g, 1.0 - vColor.b );
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
		
		
		
		float4 FresnelSwitchResult;
		#if ( S_FRESNEL == 1 )
		{
			float l_0 = g_flFresnelPower; // start index `0`
			float l_1 = sin( g_flTime ); // index `1`
			float l_2 = 34.099995 * l_1; // index `2`
			float l_3 = l_0 * l_2; // index `3`
			float3 l_4 = pow( 1.0 - dot( normalize( i.vNormalWs ), normalize( CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz ) ) ), l_3 ); // index `4`
			float4 l_5 = g_vColorOne; // index `5`
			float4 l_6 = float4( l_4, 0 ) * l_5; // last index `6`
			FresnelSwitchResult = l_6; // result
		
		}
		#else
		{
			float l_0 = g_flFresnelPower; // start index `0`
			float l_1 = sin( g_flTime ); // index `1`
			float l_2 = 34.099995 * l_1; // index `2`
			float l_3 = l_0 * l_2; // index `3`
			float3 l_4 = pow( 1.0 - dot( normalize( i.vNormalWs ), normalize( CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz ) ) ), l_3 ); // index `4`
			float3 l_5 = InvertColors( l_4 ); // last index `5`
			FresnelSwitchResult = float4( l_5, 0 ); // result
		
		}
		#endif
		
		float4 l_0 = FresnelSwitchResult; 
		float l_1 = sin( g_flTime ); 
		float l_2 = 34.099995 * l_1; 
		
		m.Albedo = l_0.xyz;
		m.Opacity = 1;
		m.Roughness = l_2;
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
