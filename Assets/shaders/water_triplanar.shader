
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
	#define S_TRANSLUCENT 1
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
	
	BoolAttribute( bWantsFBCopyTexture, true );
	
	Texture2D g_tFrameBufferCopyTexture < Attribute( "FrameBufferCopyTexture" ); SrgbRead( false ); >;
	
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	SamplerState g_sSampler1 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( deep, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( near, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tdeep < Channel( RGBA, Box( deep ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tnear < Channel( RGBA, Box( near ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	TextureAttribute( LightSim_DiffuseAlbedoTexture, g_tnear )
	TextureAttribute( RepresentativeTexture, g_tnear )
	float g_flscale < UiGroup( "Parameters,0/Textures,0/0" ); Default1( 512 ); Range1( 0, 1024 ); >;
	float2 g_vspeed < UiGroup( "Parameters,0/Movement,0/0" ); Default2( 8,0 ); Range2( 0,0, 512,512 ); >;
	float g_fldepth < UiGroup( "Parameters,0/Textures,0/0" ); Default1( 128 ); Range1( 0, 1024 ); >;
	float4 g_vFoamColor < UiType( Color ); UiGroup( "Parameters,0/Foam,0/0" ); Default4( 0.96, 0.95, 0.95, 1.00 ); >;
	float g_flfoam < UiGroup( "Parameters,0/Foam,0/0" ); Default1( 8 ); Range1( 0, 1024 ); >;
	float2 g_vwaterflow < UiGroup( "Parameters,0/Movement,0/0" ); Default2( 4,0 ); Range2( 0,0, 512,512 ); >;
	float g_flrefraction < UiGroup( "Parameters,0/,0/0" ); Default1( 0.01 ); Range1( 0, 256 ); >;
	float g_flblending < UiGroup( "Parameters,0/Textures,0/0" ); Default1( 0.5 ); Range1( 0, 1 ); >;
		
	float DepthIntersect( float3 vWorldPos, float2 vUv, float flDepthOffset )
	{
		float3 l_1 = vWorldPos - g_vCameraPositionWs;
		float l_2 = dot( l_1, g_vCameraDirWs );
	
	    flDepthOffset += l_2;
		float Depth = Depth::GetLinear( vUv );
	
		float l_3 = smoothstep( l_2, flDepthOffset, Depth );
	
	    // One Minus the result before return
		return 1 - l_3;
	}
	
	float3 Height2Normal( float flHeight , float flStrength, float3 vPosition, float3 vNormal )
	{
	    float3 worldDerivativeX = ddx(vPosition);
	    float3 worldDerivativeY = ddy(vPosition);
	
	    float3 crossX = cross(vNormal, worldDerivativeX);
	    float3 crossY = cross(worldDerivativeY, vNormal);
	
	    float d = dot(worldDerivativeX, crossY);
	
	    float sgn = d < 0.0 ? (-1.f) : 1.f;
	    float surface = sgn / max(0.00000000000001192093f, abs(d));
	
	    float dHdx = ddx(flHeight);
	    float dHdy = ddy(flHeight);
	
	    float3 surfGrad = surface * (dHdx*crossY + dHdy*crossX);
	
	    return normalize(vNormal - (flStrength * surfGrad));
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
		
		float3 l_0 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float l_1 = g_flscale;
		float l_2 = 1 / l_1;
		float2 l_3 = g_vspeed;
		float2 l_4 = l_3 / float2( l_1, l_1 );
		float2 l_5 = float2( g_flTime, g_flTime ) * l_4;
		float2 l_6 = TileAndOffsetUv( l_0.xy, float2( l_2, l_2 ), l_5 );
		float4 l_7 = g_tdeep.Sample( g_sSampler0,l_6 );
		float3 l_8 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float l_9 = g_flscale;
		float l_10 = 1 / l_9;
		float2 l_11 = TileAndOffsetUv( l_8.xy, float2( l_10, l_10 ), float2( 0, 0 ) );
		float4 l_12 = g_tnear.Sample( g_sSampler1,l_11 );
		float l_13 = g_fldepth;
		float l_14 = DepthIntersect(i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz, i.vPositionSs.xy, l_13);
		float4 l_15 = saturate( lerp( l_7, l_12, l_14 ) );
		float4 l_16 = g_vFoamColor;
		float l_17 = cos( g_flTime );
		float l_18 = 8 + l_17;
		float l_19 = VoronoiNoise( l_6, l_18, 10 );
		float l_20 = saturate( l_19 );
		float l_21 = g_flfoam;
		float l_22 = DepthIntersect(i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz, i.vPositionSs.xy, l_21);
		float l_23 = step( l_20, l_22 );
		float4 l_24 = saturate( lerp( l_15, l_16, l_23 ) );
		float2 l_25 = CalculateViewportUv( i.vPositionSs.xy );
		float2 l_26 = g_vFrameBufferCopyInvSizeAndUvScale.zw;
		float2 l_27 = l_25 * l_26;
		float3 l_28 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float2 l_29 = g_vwaterflow;
		float2 l_30 = float2( g_flTime, g_flTime ) * l_29;
		float2 l_31 = TileAndOffsetUv( l_28.xy, float2( 0.05, 0.05 ), l_30 );
		float l_32 = Simplex2D(l_31);
		float l_33 = g_flrefraction;
		float3 l_34 = Height2Normal(l_32, l_33, i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz, i.vNormalWs);
		float3 l_35 = float3( l_27, 0 ) + l_34;
		float3 l_36 = g_tFrameBufferCopyTexture.Sample( g_sAniso ,l_35.xy);
		float l_37 = g_flblending;
		float4 l_38 = saturate( lerp( l_24, float4( l_36, 0 ), l_37 ) );
		
		m.Albedo = l_38.xyz;
		m.Opacity = 1;
		m.Roughness = 0;
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
