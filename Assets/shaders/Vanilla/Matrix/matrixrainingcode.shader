
HEADER
{
	Description = "Red pill or the Blue pill?";
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
	#include "procedural.hlsl"

	#define S_UV2 1
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
	#if ( PROGRAM == VFX_PROGRAM_PS )
		bool vFrontFacing : SV_IsFrontFace;
	#endif
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		i.vColor = v.vColor;
		
		ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData( v.nInstanceTransformID );
		i.vTintColor = extraShaderData.vTint;
		
		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );
		return FinalizeVertex( i );
		
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( MovingTextures, Srgb, 8, "None", "_color", "Textures,0/,0/0", DefaultFile( "textures/matrixrain/1k/moving_textures.png" ) );
	CreateInputTexture2D( NotMovingMask, Srgb, 8, "None", "_color", "Textures,0/,0/0", DefaultFile( "textures/matrixrain/1k/letters_lanes_pixel_variations.png" ) );
	CreateInputTexture2D( MovingTextures_0, Srgb, 8, "None", "_color", "Textures,0/,0/0", DefaultFile( "textures/matrixrain/1k/moving_textures.png" ) );
	CreateInputTexture2D( MovingTextures_1, Srgb, 8, "None", "_color", "Textures,0/,0/0", DefaultFile( "textures/matrixrain/1k/moving_textures.png" ) );
	CreateInputTexture2D( MovingTextures_2, Srgb, 8, "None", "_color", "Textures,0/,0/0", DefaultFile( "textures/matrixrain/1k/moving_textures.png" ) );
	Texture2D g_tMovingTextures < Channel( RGBA, Box( MovingTextures ), Srgb ); OutputFormat( BC7 ); SrgbRead( True ); >;
	Texture2D g_tNotMovingMask < Channel( RGBA, Box( NotMovingMask ), Srgb ); OutputFormat( BC7 ); SrgbRead( True ); >;
	Texture2D g_tMovingTextures_0 < Channel( RGBA, Box( MovingTextures_0 ), Srgb ); OutputFormat( BC7 ); SrgbRead( True ); >;
	Texture2D g_tMovingTextures_1 < Channel( RGBA, Box( MovingTextures_1 ), Srgb ); OutputFormat( BC7 ); SrgbRead( True ); >;
	Texture2D g_tMovingTextures_2 < Channel( RGBA, Box( MovingTextures_2 ), Srgb ); OutputFormat( BC7 ); SrgbRead( True ); >;
	TextureAttribute( LightSim_DiffuseAlbedoTexture, g_tMovingTextures_2 )
	TextureAttribute( RepresentativeTexture, g_tMovingTextures_2 )
	bool g_bUseScreenCoordinates < Attribute( "Use Screen Coordinates" ); Default( 0 ); >;
	float2 g_vTileXYAmount < UiGroup( "Parameters,0/,0/3" ); Default2( 1,1 ); Range2( 0,0, 1,1 ); >;
	float g_flRainSpeed < UiType( Slider ); UiGroup( "Parameters,0/,0/4" ); Default1( 8 ); Range1( 0, 64 ); >;
	float g_flStepInCharacters < UiGroup( "Parameters,0/,0/6" ); Default1( 0.015625 ); Range1( 0, 1 ); >;
	float g_flDoubleSpeedValue < UiGroup( "Parameters,0/,0/6" ); Default1( 0.2 ); Range1( 0, 1 ); >;
	float g_flRainBrightness < UiType( Slider ); UiGroup( "Parameters,0/,0/4" ); Default1( 1 ); Range1( 0, 8 ); >;
	float4 g_vLettersColor < UiType( Color ); UiGroup( "Parameters,0/,0/0" ); Default4( 0.40, 1.00, 0.22, 1.00 ); >;
	float4 g_vLettersColorEmission < UiType( Color ); UiGroup( "Parameters,0/,0/1" ); Default4( 0.83, 1.00, 0.79, 1.00 ); >;
	bool g_bEnableTransparency < Attribute( "Enable Transparency" ); Default( 0 ); >;
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		
		Material m = Material::Init( i );
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = float3( 0, 0, 1 );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;
		
		float2 l_0 = CalculateViewportUv( i.vPositionSs.xy );
		float2 l_1 = i.vTextureCoords.xy * float2( 1, 1 );
		float2 l_2 = g_bUseScreenCoordinates ? l_0 : l_1;
		float2 l_3 = g_vTileXYAmount;
		float2 l_4 = TileAndOffsetUv( l_2, l_3, float2( 0, 0 ) );
		float l_5 = l_4.x;
		float l_6 = l_4.y;
		float l_7 = g_flRainSpeed;
		float l_8 = g_flTime * l_7;
		float l_9 = g_flStepInCharacters;
		float l_10 = l_8 * l_9;
		float l_11 = l_6 - l_10;
		float2 l_12 = float2( l_5, l_11);
		float4 l_13 = Tex2DS( g_tMovingTextures, g_sSampler0, l_12 );
		float4 l_14 = Tex2DS( g_tNotMovingMask, g_sSampler0, l_4 );
		float l_15 = 1 - l_14.r;
		float4 l_16 = l_13 * float4( l_15, l_15, l_15, l_15 );
		float l_17 = g_flDoubleSpeedValue;
		float l_18 = l_8 * l_17;
		float l_19 = l_18 * l_9;
		float l_20 = l_6 - l_19;
		float2 l_21 = float2( l_5, l_20);
		float4 l_22 = Tex2DS( g_tMovingTextures_0, g_sSampler0, l_21 );
		float4 l_23 = float4( l_14.r, l_14.r, l_14.r, l_14.r ) * l_22;
		float4 l_24 = l_16 + l_23;
		float l_25 = l_24.x;
		float l_26 = ceil( l_18 );
		float l_27 = l_26 * l_9;
		float l_28 = l_6 - l_27;
		float2 l_29 = float2( l_5, l_28);
		float4 l_30 = Tex2DS( g_tMovingTextures_1, g_sSampler0, l_29 );
		float4 l_31 = float4( l_14.r, l_14.r, l_14.r, l_14.r ) * l_30;
		float l_32 = ceil( l_8 );
		float l_33 = l_32 * l_9;
		float l_34 = l_6 - l_33;
		float2 l_35 = float2( l_5, l_34);
		float4 l_36 = Tex2DS( g_tMovingTextures_2, g_sSampler0, l_35 );
		float4 l_37 = float4( l_15, l_15, l_15, l_15 ) * l_36;
		float4 l_38 = l_31 + l_37;
		float l_39 = l_38.y;
		float l_40 = l_25 + l_39;
		float l_41 = g_flRainBrightness;
		float l_42 = l_14.g * l_41;
		float l_43 = l_40 * l_42;
		float4 l_44 = g_vLettersColor;
		float4 l_45 = float4( l_43, l_43, l_43, l_43 ) * l_44;
		float4 l_46 = g_vLettersColorEmission;
		float l_47 = l_39 * l_42;
		float4 l_48 = l_46 * float4( l_47, l_47, l_47, l_47 );
		float4 l_49 = l_45 + l_48;
		float4 l_50 = float4( 1, 1, 1, 1 );
		float l_51 = g_bEnableTransparency ? l_14.g : l_50.r;
		
		m.Albedo = l_49.xyz;
		m.Emission = l_49.xyz;
		m.Opacity = l_51;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 0;
		
		
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
				
		return ShadingModelStandard::Shade( m );
	}
}
