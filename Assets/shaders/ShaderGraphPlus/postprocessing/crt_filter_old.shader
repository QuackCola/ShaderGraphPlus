
HEADER
{
	Description = "";
}

MODES
{
    Default();
    VrForward();
}

FEATURES
{
}

COMMON
{
    #include "postprocess/shared.hlsl"
	#include "procedural.hlsl"
}

struct VertexInput
{
    float3 vPositionOs : POSITION < Semantic( PosXyz ); >;
    float2 vTexCoord : TEXCOORD0 < Semantic( LowPrecisionUv ); >;
};

struct PixelInput
{
    float2 vTexCoord : TEXCOORD0;

	// VS only
	#if ( PROGRAM == VFX_PROGRAM_VS )
		float4 vPositionPs		: SV_Position;
	#endif

	// PS only
	#if ( ( PROGRAM == VFX_PROGRAM_PS ) )
		float4 vPositionSs		: SV_Position;
	#endif
};

VS
{
    PixelInput MainVs( VertexInput i )
    {
        PixelInput o;
        o.vPositionPs = float4(i.vPositionOs.xyz, 1.0f);
        o.vTexCoord = i.vTexCoord;
        return o;
    }
}

PS
{
	#include "common/classes/Depth.hlsl"
    #include "postprocess/common.hlsl"
	#include "postprocess/functions.hlsl"
	#include "postprocess/CommonUtils.hlsl"
	
	RenderState( DepthWriteEnable, false );
	RenderState( DepthEnable, false );
	CreateTexture2D( g_tColorBuffer ) < Attribute( "ColorBuffer" ); SrgbRead( true ); Filter( MIN_MAG_LINEAR_MIP_POINT ); AddressU( MIRROR ); AddressV( MIRROR ); >;
	CreateTexture2D( g_tDepthBuffer ) < Attribute( "DepthBuffer" ); SrgbRead( false ); Filter( MIN_MAG_MIP_POINT ); AddressU( CLAMP ); AddressV( CLAMP ); >;
	
	float g_flWarpAmount < Attribute( "WarpAmount" ); Default1( 1 ); >;
	float2 g_vResolution < Attribute( "Resolution" ); Default2( 640,480 ); >;
	bool g_bPixalate < Attribute( "Pixalate" ); Default( 1 ); >;
	float g_flRollSize < Attribute( "RollSize" ); Default1( 15 ); >;
	bool g_bRoll < Attribute( "Roll" ); Default( 0 ); >;
	float g_flRollSpeed < Attribute( "RollSpeed" ); Default1( 8 ); >;
	float g_flRollVariation < Attribute( "RollVariation" ); Default1( 15 ); >;
	float g_flDistortionIntensity < Attribute( "DistortionIntensity" ); Default1( 0.05 ); >;
	float g_flAberration < Attribute( "Aberration" ); Default1( 0.03 ); >;
	float g_flGrilleOpacity < Attribute( "GrilleOpacity" ); Default1( 0.3 ); >;
	float g_flBrightness < Attribute( "Brightness" ); Default1( 0.03 ); >;
	float g_flScanlineWidth < Attribute( "ScanlineWidth" ); Default1( 0.25 ); >;
	float g_flScanlineOpacity < Attribute( "ScanlineOpacity" ); Default1( 0.4 ); >;
	float g_flNoiseSpeed < Attribute( "NoiseSpeed" ); Default1( 5 ); >;
	float g_flNoiseOpacity < Attribute( "NoiseOpacity" ); Default1( 0.4 ); >;
	float g_flStaticNoiseIntensity < Attribute( "StaticNoiseIntensity" ); Default1( 0.06 ); >;
	float g_flVignetteIntensity < Attribute( "VignetteIntensity" ); Default1( 1 ); >;
	float g_flVignetteOpacity < Attribute( "VignetteOpacity" ); Default1( 0.5 ); >;
		
	float2 Warp(float2 vUv , float flWarp_amount)
	{
		float2 delta = vUv - 0.5;
		float delta2 = dot(delta.xy, delta.xy);
		float delta4 = delta2 * delta2;
		float delta_offset = delta4 * flWarp_amount;
		
		return vUv + delta * delta_offset;
	}
	
	float2 Random(float2 vUv)
	{
	    vUv = float2( dot(vUv, float2(127.1,311.7) ), dot(vUv, float2(269.5,183.3) ) );
	    return -1.0 + 2.0 * frac(sin(vUv) * 43758.5453123);
	}
	
	float Border(float2 vUv , float flWarp_amount)
	{
		float radius = min(flWarp_amount, 0.08);
		radius = max(min(min(abs(radius * 2.0), abs(1.0)), abs(1.0)), 1e-5);
		float2 abs_uv = abs(vUv * 2.0 - 1.0) - float2(1.0, 1.0) + radius;
		float dist = length(max(float2(0.0,0.0), abs_uv)) / radius;
		float square = smoothstep(0.96, 1.0, dist);
		return clamp(1.0 - square, 0.0, 1.0);
	}
	
	float Vignette(float2 vUv , float flVignette_intensity, float flVignette_opacity)
	{
		vUv *= 1.0 - vUv.xy;
		float vignette = vUv.x * vUv.y * 15.0;
		return pow(vignette, flVignette_intensity * flVignette_opacity);
	}
	
    float4 MainPs( PixelInput i ) : SV_Target0
    {
		float3 FinalColor = float3( 1, 1, 1 );
		
		float2 l_0 = CalculateViewportUv( i.vPositionSs.xy );
		float l_1 = g_flWarpAmount;
		float2 l_2 = Warp( l_0, l_1 );
		float2 l_3 = g_vResolution;
		float2 l_4 = l_2 * l_3;
		float2 l_5 = ceil( l_4 );
		float2 l_6 = l_5 / l_3;
		float2 l_7 = g_bPixalate ? l_6 : l_2;
		float l_8 = l_2.y;
		float l_9 = g_flRollSize;
		float l_10 = l_8 * l_9;
		float l_11 = g_bRoll ? g_flTime : 0;
		float l_12 = g_flRollSpeed;
		float l_13 = l_11 * l_12;
		float l_14 = l_10 - l_13;
		float l_15 = sin( l_14 );
		float l_16 = smoothstep( 0.3, 0.9, l_15 );
		float l_17 = l_16 * l_16;
		float l_18 = g_flRollSize;
		float l_19 = l_8 * l_18;
		float l_20 = g_flRollVariation;
		float l_21 = l_19 * l_20;
		float l_22 = l_13 * l_20;
		float l_23 = l_21 - l_22;
		float l_24 = sin( l_23 );
		float l_25 = smoothstep( 0.3, 0.9, l_24 );
		float l_26 = l_17 * l_25;
		float l_27 = g_flDistortionIntensity;
		float l_28 = l_26 * l_27;
		float l_29 = l_2.x;
		float l_30 = 1 - l_29;
		float l_31 = l_28 * l_30;
		float2 l_32 = float2( l_31, 0 );
		float2 l_33 = l_7 + l_32;
		float2 l_34 = l_33 * float2( 0.8, 0.8 );
		float l_35 = g_flAberration;
		float2 l_36 = float2( l_35, 0 );
		float2 l_37 = l_36 * float2( 0.1, 0.1 );
		float2 l_38 = l_34 + l_37;
		float3 l_39 = Tex2D( g_tColorBuffer, l_38);
		float l_40 = l_39.x;
		float2 l_41 = l_33 * float2( 1.2, 1.2 );
		float2 l_42 = l_41 - l_37;
		float3 l_43 = Tex2D( g_tColorBuffer, l_42);
		float l_44 = l_43.y;
		float3 l_45 = Tex2D( g_tColorBuffer, l_33);
		float l_46 = l_45.z;
		float4 l_47 = float4( l_40, l_44, l_46, 1 );
		float2 l_48 = l_7 + l_37;
		float3 l_49 = Tex2D( g_tColorBuffer, l_48);
		float l_50 = l_49.x;
		float2 l_51 = l_7 - l_37;
		float3 l_52 = Tex2D( g_tColorBuffer, l_51);
		float l_53 = l_52.y;
		float3 l_54 = Tex2D( g_tColorBuffer, l_7);
		float l_55 = l_54.z;
		float4 l_56 = float4( l_50, l_53, l_55, 1 );
		float4 l_57 = g_bRoll ? l_47 : l_56;
		float l_58 = l_57.x;
		float2 l_59 = CalculateViewportUv( i.vPositionSs.xy );
		float2 l_60 = Warp( l_59, l_1 );
		float l_61 = l_60.x;
		float2 l_62 = g_vResolution;
		float l_63 = 3.14159265359;
		float l_64 = l_62.x * l_63;
		float l_65 = l_61 * l_64;
		float l_66 = sin( l_65 );
		float l_67 = abs( l_66 );
		float l_68 = smoothstep( 0.85, 0.95, l_67 );
		float l_69 = l_58 * l_68;
		float l_70 = g_flGrilleOpacity;
		float l_71 = lerp( l_58, l_69, l_70 );
		float l_72 = l_70 > 0 ? l_71 : l_58;
		float l_73 = g_flBrightness;
		float l_74 = l_72 * l_73;
		float l_75 = saturate( l_74 );
		float l_76 = l_57.y;
		float l_77 = 1.05 + l_61;
		float l_78 = l_62.x * l_63;
		float l_79 = l_77 * l_78;
		float l_80 = sin( l_79 );
		float l_81 = abs( l_80 );
		float l_82 = smoothstep( 0.85, 0.95, l_81 );
		float l_83 = l_76 * l_82;
		float l_84 = lerp( l_76, l_83, l_70 );
		float l_85 = l_70 > 0 ? l_84 : l_76;
		float l_86 = l_85 * l_73;
		float l_87 = saturate( l_86 );
		float l_88 = l_57.z;
		float l_89 = 2.1 + l_61;
		float l_90 = l_62.x * l_63;
		float l_91 = l_89 * l_90;
		float l_92 = sin( l_91 );
		float l_93 = abs( l_92 );
		float l_94 = smoothstep( 0.85, 0.95, l_93 );
		float l_95 = l_88 * l_94;
		float l_96 = lerp( l_88, l_95, l_70 );
		float l_97 = l_70 > 0 ? l_96 : l_88;
		float l_98 = l_97 * l_73;
		float l_99 = saturate( l_98 );
		float3 l_100 = float3( l_75, l_87, l_99 );
		float l_101 = g_flScanlineWidth;
		float l_102 = l_101 + 0.5;
		float l_103 = l_60.y;
		float2 l_104 = g_vResolution;
		float l_105 = 3.14159265359;
		float l_106 = l_104.y * l_105;
		float l_107 = l_103 * l_106;
		float l_108 = sin( l_107 );
		float l_109 = abs( l_108 );
		float l_110 = smoothstep( l_101, l_102, l_109 );
		float3 l_111 = float3( l_110, l_110, l_110 );
		float3 l_112 = l_100 * l_111;
		float l_113 = g_flScanlineOpacity;
		float3 l_114 = lerp( l_100, l_112, l_113 );
		float3 l_115 = l_113 > 0 ? l_114 : l_100;
		float2 l_116 = float2( 2, 200 );
		float2 l_117 = l_60 * l_116;
		float l_118 = g_flNoiseSpeed;
		float l_119 = g_flTime * l_118;
		float2 l_120 = float2( 10, l_119 );
		float2 l_121 = l_117 + l_120;
		float l_122 = Simplex2D(l_121);
		float l_123 = smoothstep( 0.4, 0.5, l_122 );
		float l_124 = l_26 * l_123;
		float l_125 = l_113 > 0 ? l_110 : 0.5;
		float l_126 = l_124 * l_125;
		float2 l_127 = g_vResolution;
		float2 l_128 = l_60 * l_127;
		float2 l_129 = ceil( l_128 );
		float2 l_130 = l_129 / l_127;
		float l_131 = g_flTime * 0.8;
		float2 l_132 = float2( l_131, 0 );
		float l_133 = l_132.x;
		float2 l_134 = l_130 + float2( l_133, l_133 );
		float2 l_135 = l_134 + float2( 0.8, 0.8 );
		float2 l_136 = Random( l_135 );
		float2 l_137 = saturate( l_136 );
		float l_138 = l_137.x;
		float l_139 = l_126 * l_138;
		float3 l_140 = l_115 + float3( l_139, l_139, l_139 );
		float l_141 = g_flNoiseOpacity;
		float3 l_142 = lerp( l_115, l_140, l_141 );
		float3 l_143 = saturate( l_142 );
		float3 l_144 = l_141 > 0 ? l_143 : l_115;
		float2 l_145 = g_vResolution;
		float2 l_146 = l_60 * l_145;
		float2 l_147 = ceil( l_146 );
		float2 l_148 = l_147 / l_145;
		float l_149 = frac( g_flTime );
		float2 l_150 = l_148 + float2( l_149, l_149 );
		float2 l_151 = Random( l_150 );
		float l_152 = l_151.x;
		float l_153 = saturate( l_152 );
		float l_154 = g_flStaticNoiseIntensity;
		float l_155 = l_153 * l_154;
		float3 l_156 = l_144 + float3( l_155, l_155, l_155 );
		float3 l_157 = l_154 == 0 ? l_156 : l_144;
		float l_158 = Border( l_60, l_1 );
		float3 l_159 = l_157 * float3( l_158, l_158, l_158 );
		float l_160 = g_flVignetteIntensity;
		float l_161 = g_flVignetteOpacity;
		float l_162 = Vignette( l_60, l_160, l_161 );
		float3 l_163 = l_159 * float3( l_162, l_162, l_162 );
		float l_164 = l_163.x;
		float l_165 = l_163.y;
		float l_166 = l_163.z;
		float l_167 = Border( l_60, l_1 );
		float4 l_168 = float4( l_164, l_165, l_166, l_167 );
		
		FinalColor = l_168.xyz;
		
        return float4(FinalColor,1.0f);
    }
}
