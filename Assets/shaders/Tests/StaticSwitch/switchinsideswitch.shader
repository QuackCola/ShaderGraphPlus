
HEADER
{
    Description = "";
}

FEATURES
{
    #include "common/features.hlsl"
	Feature( F_OUTER, 0..1, "Features" );
	Feature( F_INNER1, 0..1, "Features" );
	
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
	
	StaticCombo( S_OUTER, F_OUTER, Sys( ALL ) );
	
	StaticCombo( S_INNER1, F_INNER1, Sys( ALL ) );
	
	
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
		
	StaticCombo( S_OUTER, F_OUTER, Sys( ALL ) );
	
	StaticCombo( S_INNER1, F_INNER1, Sys( ALL ) );
	
	
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
		
		
		
		float OuterSwitchResult;
		#if ( S_OUTER == SWITCH_TRUE )
		{
			
			float Inner1SwitchResult;
			#if ( S_INNER1 == SWITCH_TRUE )
			{
				float l_0 = 0 + 1;
				Inner1SwitchResult = l_0; // result
			
			}
			#else
			{
				float l_0 = 1 + 1;
				Inner1SwitchResult = l_0; // result
			
			}
			#endif
			
			float l_0 = Inner1SwitchResult;
			OuterSwitchResult = l_0; // result
		
		}
		#else
		{
			float l_0 = 2 + 1;
			OuterSwitchResult = l_0; // result
		
		}
		#endif
		
		float l_1 = OuterSwitchResult;
		
		m.Albedo = float3( l_1, l_1, l_1 );
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
