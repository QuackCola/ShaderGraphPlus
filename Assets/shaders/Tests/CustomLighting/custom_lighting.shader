
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
	
	float g_flTest < UiGroup( ",0/,0/0" ); Default1( 0.25806516 ); Range1( 0, 1 ); >;
	
    PixelInput MainVs( VertexInput v )
    {
		
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		i.vColor = v.vColor;
		
		ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData( v );
		i.vTintColor = extraShaderData.vTint;
		
		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );
		
		float l_0 = g_flTest;
		i.vPositionWs.xyz += float3( l_0, l_0, l_0 );
		i.vPositionPs.xyzw = Position3WsToPs( i.vPositionWs.xyz );
		return FinalizeVertex( i );
		
    }
}

PS
{
    #include "common/pixel.hlsl"
	
	DynamicCombo( D_RENDER_BACKFACES, 0..1, Sys( ALL ) );
	RenderState( CullMode, D_RENDER_BACKFACES ? NONE : BACK );
		
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	
	float4 g_vLitColor < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 0.81, 0.81, 0.81, 1.00 ); >;
	float4 g_vShadeColor < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 0.41, 0.41, 0.41, 1.00 ); >;
	float g_flLightThreshold < UiType( Slider ); UiGroup( ",0/,0/0" ); Default1( 0.5 ); Range1( 0, 1 ); >;
	CreateInputTexture2D( Color, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tColor < Channel( RGBA, Box( Color ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
		
	static float4 Shade( PixelInput i, Material m  )
	{
		float3 Albedo = float3( 0, 0, 0 );
		
		
		// Loop through all lights 
		for ( int index = 0; index < Light::Count( m.ScreenPosition.xy ); index++ )
		{
			Light light = Light::From( m.ScreenPosition.xy, m.WorldPosition, index );
			
			
			float4 l_0 = g_vLitColor;
			float4 l_1 = g_vShadeColor;
			float l_2 = light.Visibility * light.Attenuation;
			float l_3 = dot( i.vNormalWs, light.Direction );
			float l_4 = l_3 * 0.5;
			float l_5 = l_4 + 0.5;
			float l_6 = l_2 * l_5;
			float l_7 = g_flLightThreshold;
			float l_8 = step( l_6, l_7 );
			float4 l_9 = lerp( l_0, l_1, l_8 );
			float4 l_10 = l_9 * float4( light.Color, 0 );
			float4 l_11 = l_10 + float4( m.Albedo, 0 );
			
			Albedo += l_11.xyz;
			
		}
	
	
		// Calcuate Indirect Lighting
		{
	
	
		}
	
		if( DepthNormals::WantsDepthNormals() )
			return DepthNormals::Output( m.Normal, m.Roughness );
		
		// TODO
		//if( ToolsVis::WantsToolsVis() )
		//	return DoToolsVis( Albedo, m, lightingTerms );
	
		// Composite atmospherics after lighting
			//Albedo.xyz = Fog::Apply( m.WorldPosition, m.ScreenPosition.xy, float4( Albedo.xyz, 0 ) );
			
	
		return float4(Albedo.xyz, m.Opacity);
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
		
		
		float4 l_0 = g_tColor.Sample( g_sSampler0,i.vTextureCoords.xy );
		

		m.Albedo = l_0.xyz;
		m.Opacity = 1;
		
		return Shade( i, m );
		//return float4( l_0.xyz, 1 );
    }
}
