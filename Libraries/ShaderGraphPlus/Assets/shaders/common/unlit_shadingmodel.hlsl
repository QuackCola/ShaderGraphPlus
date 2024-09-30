#ifndef COMMON_UNLIT_PIXEL_SHADING_H
#define COMMON_UNLIT_PIXEL_SHADING_H

#include "common/material.hlsl"
#include "common/light.hlsl"

//--------------------------------------------------------------------------------------
//
// Postprocessing functions for shading models
//
//--------------------------------------------------------------------------------------
float4 DoToolVisualizations( in float4 vColor, Material m, LightingTerms_t lightingTerms )
{
    #if( S_MODE_TOOLS_VIS )
        if ( g_nToolsVisMode != TOOLS_VIS_MODE_DIFFUSE_LIGHTING && g_nToolsVisMode != TOOLS_VIS_MODE_SPECULAR_LIGHTING &&
             g_nToolsVisMode != TOOLS_VIS_MODE_DIFFUSE_AMBIENT_OCCLUSION && g_nToolsVisMode != TOOLS_VIS_MODE_CUBEMAP_REFLECTIONS &&
             g_nToolsVisMode != TOOLS_VIS_MODE_TRANSMISSIVE_LIGHTING )
            ToolsVisInitColor( vColor.rgba );

        ToolsVisHandleFlatOverlayColor( m.Albedo.rgb, vColor.rgba );
        ToolsVisHandleFullbright( vColor.rgba, m.Albedo.rgb, m.WorldPosition.xyz, m.Normal.xyz );

		ToolsVisHandleDiffuseLighting( vColor.rgba, lightingTerms );
		ToolsVisHandleSpecularLighting( vColor.rgba, lightingTerms );
		ToolsVisHandleTransmissiveLighting( vColor.rgba, lightingTerms );
		ToolsVisHandleLightingComplexity( vColor.rgba, lightingTerms );

        ToolsVisHandleAlbedo( vColor.rgba, m.Albedo.rgb );
        ToolsVisHandleReflectivity( vColor.rgba, m.Albedo.rgb );
        ToolsVisHandleRoughness( vColor.rgba, m.Roughness );
        ToolsVisHandleDiffuseAmbientOcclusion( vColor.rgba, AmbientLight::From( m.WorldPosition, m ).Visibility * m.AmbientOcclusion );
        ToolsVisHandleSpecularAmbientOcclusion( vColor.rgba, AmbientLight::From( m.WorldPosition, m ).Visibility * m.AmbientOcclusion );
        ToolsVisHandleShaderIDColor( vColor.rgba );

        ToolsVisHandleNormalTs( vColor.rgba, NormalWorldToTangent( m.Normal, m.GeometricNormal, m.WorldTangentU, m.WorldTangentV ) );
        ToolsVisHandleNormalWs( vColor.rgba, m.Normal.xyz );
        ToolsVisHandleGeometricNormalWs( vColor.rgba, m.GeometricNormal.xyz );

        #if ENABLE_NORMAL_MAPS
            ToolsVisHandleTangentUWs( vColor.rgba, m.WorldTangentU );
            ToolsVisHandleTangentVWs( vColor.rgba, m.WorldTangentV );
        #endif

        ToolsVisHandleGeometricRoughness( vColor.rgba, m.GeometricNormal.xyz );
        ToolsVisHandleBakedLight( vColor, lightingTerms );

        ToolsVisHandleTiledRenderingColors( vColor, m.Albedo.rgb, m.ScreenPosition.xy );

        #ifndef CUSTOM_MATERIAL_INPUTS
            ToolsVisShowUVs( vColor.rgba, m.Albedo.rgb, g_tColor, m.TextureCoords.xy );
            ToolsVisShowMipUtilization( vColor.rgba, m.Albedo.rgb, g_tColor, m.TextureCoords.xy );
        #endif

        //
        // Baked light visualization
        //
        {
            uint nNumLights = 0;
            
            [flatten]
            if( g_nToolsVisMode == TOOLS_VIS_MODE_LIGHTING_COMPLEXITY )
            {
                uint nNumLights = 0;
                for( uint index = 0; index < DynamicLight::Count( m.ScreenPosition ); index++ )
                {
                    Light light = DynamicLight::From( m.ScreenPosition, m.WorldPosition, index ) ;
                    if( light.Attenuation > 0.0f )
                        nNumLights++;
                }
                ToolsVisHandleLightingComplexity( vColor.rgba, nNumLights );
            }

            [flatten]
            if( g_nToolsVisMode == TOOLS_VIS_MODE_INDEXED_LIGHTING_COUNT )
            {
                for( uint index = 0; index < DynamicLight::Count( m.ScreenPosition ); index++ )
                {
                    Light light = DynamicLight::From( m.ScreenPosition, m.WorldPosition, index ) ;
                    if( light.Visibility > 0.0f && light.Attenuation > 0.0f && dot( light.Direction, m.Normal ) > 0.0f )
                        nNumLights++;
                }
                
                ToolsVisHandleLightingComplexity( vColor.rgba, nNumLights );
            }

            [flatten]
            if( g_nToolsVisMode == g_nToolsVisMode == TOOLS_VIS_MODE_BAKED_LIGHT_COUNT )
            {
                for( uint index = 0; index < StaticLight::Count(); index++ )
                {
                    Light light = StaticLight::From( m.WorldPosition, m.LightmapUV, index ) ;
                    if( light.Visibility > 0.0f && light.Attenuation > 0.0f && dot( light.Direction, m.Normal ) > 0.0f )
                        nNumLights++;
                }
                
                ToolsVisHandleLightingComplexity( vColor.rgba, nNumLights );
            }
        }
        

    #endif

    return vColor;
}

float4 DoPostProcessing( const Material material, float4 color )
{
    // Remove alpha if we are not transparent, might be shit but screenshots are being written
    // with alpha in some shaders
    #ifndef CUSTOM_MATERIAL_INPUTS
        #if ( !S_ALPHA_TEST && !S_TRANSLUCENT && !TRANSLUCENT )
        {
            color.a = 1.0f;
        }
        #endif
    #endif

    return color;
}

class ShadingModelStandardUnlit
{
    //
    // Converts our Material struct to a FinalCombinerInput_t
    // PS_InitFinalCombiner assumes that you want to control the "optional" parameters yourself.
    // These *need* to be set up by you if you want correct lighting.
    //
    // PS_FinalCombiner should be called at the end and works on the FinalCombinerInput_t data only. 
    // This does lighting, tonemapping, etc. in a standardized way.
    //
    static CombinerInput MaterialToCombinerInput( Material m )
    {
        CombinerInput o = PS_InitFinalCombiner();
      
        o.vPositionWithOffsetWs = m.WorldPositionWithOffset;
        o.vPositionWs = m.WorldPosition;
        o.vPositionSs = m.ScreenPosition;

        o.vNormalWs = m.Normal;
        o.vGeometricNormalWs = m.GeometricNormal;
        o.vNormalTs = NormalWorldToTangent( m.Normal, m.GeometricNormal, m.WorldTangentU, m.WorldTangentV );

        o.vRoughness = m.Roughness.xx;
        o.vEmissive = m.Emission;
        o.flAmbientOcclusion = m.AmbientOcclusion;
        o.vTransmissiveMask = m.Transmission;

        // Sane default
        // o.flSSSCurvature = length( fwidth( o.vNormalWs.xyz ) ) / length( fwidth( o.vPositionWithOffsetWs.xyz ) );

        return o;    
    }

    static float4 Shade( Material m )
    {
        // !!!
        // It's very odd we're not just using PS_FinalCombiner( FinalCombinerInput_t finalCombinerInput ) ?!
        // !!!

        LightingTerms_t lightingTerms = InitLightingTerms();
        CombinerInput combinerInput = MaterialToCombinerInput( m );

        // Hmm is this the right place for it
        combinerInput = CalculateDiffuseAndSpecularFromAlbedoAndMetalness( combinerInput, m.Albedo.rgb, m.Metalness );

        float3 vDiffuse = combinerInput.vDiffuseColor.rgb + combinerInput.vEmissive.rgb;

        float4 color = float4( vDiffuse , combinerInput.flOpacity );

        color = DoToolVisualizations( color, m, lightingTerms );
        color = DoPostProcessing( m, color );

        return color;
    }

#ifdef COMMON_PS_INPUT_DEFINED
    static CombinerInput MaterialToCombinerInput( PixelInput i, Material m )
    {
        CombinerInput o = PS_InitFinalCombiner();

        o = PS_CommonProcessing( i );
        
        // this should not be here
        #if ( S_ALPHA_TEST )
        {
            // Clip first to try to kill the wave if we're in an area of all zero
            o.flOpacity = m.Opacity * o.flOpacity;
            clip( o.flOpacity - .001 );

            o.flOpacity = AdjustOpacityForAlphaToCoverage( o.flOpacity, g_flAlphaTestReference, g_flAntiAliasedEdgeStrength, i.vTextureCoords.xy );
            clip( o.flOpacity - 0.001 );
        }
        #elif ( S_TRANSLUCENT )
        {
            o.flOpacity *= m.Opacity * g_flOpacityScale;
        }
        #else
            o.flOpacity *= m.Opacity;
        #endif

        o = CalculateDiffuseAndSpecularFromAlbedoAndMetalness( o, m.Albedo.rgb, m.Metalness );

        o.vNormalWs = m.Normal;
        o.vNormalTs = NormalWorldToTangent( m.Normal, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
        o.vRoughness = m.Roughness.xx;
        o.vEmissive = m.Emission;
        o.flAmbientOcclusion = m.AmbientOcclusion;
        o.vTransmissiveMask = m.Transmission;

        o.vGeometricNormalWs.xyz = i.vNormalWs;
        o.vRoughness.xy = AdjustRoughnessByGeometricNormal( o.vRoughness.xy, o.vGeometricNormalWs.xyz );

        return o;
    }

    static float4 Shade( PixelInput i, Material m )
    {
        m.WorldPosition = i.vPositionWithOffsetWs + g_vHighPrecisionLightingOffsetWs;
        m.WorldPositionWithOffset = i.vPositionWithOffsetWs;
        m.ScreenPosition = i.vPositionSs;
        m.GeometricNormal = i.vNormalWs;

        LightingTerms_t lightingTerms = InitLightingTerms();
        CombinerInput combinerInput = MaterialToCombinerInput( i, m );

        // Hmm is this the right place for it
        combinerInput = CalculateDiffuseAndSpecularFromAlbedoAndMetalness( combinerInput, m.Albedo.rgb, m.Metalness );

        float3 diffuse =  combinerInput.vDiffuseColor.rgb + combinerInput.vEmissive.rgb;
        float4 color = float4( diffuse , combinerInput.flOpacity );

        color = DoToolVisualizations( color, m, lightingTerms );
        color = DoPostProcessing( m, color );

        return color;
    }
#endif
};

#endif // COMMON_UNLIT_PIXEL_SHADING_H