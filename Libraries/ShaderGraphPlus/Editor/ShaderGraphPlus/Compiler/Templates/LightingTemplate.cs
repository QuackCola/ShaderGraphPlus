namespace Editor.ShaderGraphPlus;

public static class LightingTemplate
{
	public static string Contents => @"
static float4 Shade( PixelInput i, Material m  )
{{
{0}
	// Loop through all lights 
	for ( int index = 0; index < Light::Count( m.ScreenPosition.xy ); index++ )
	{{
		Light light = Light::From( m.ScreenPosition.xy, m.WorldPosition, index );
{1}
{2}
	}}


	// Calcuate Indirect Lighting
	float3 indirectColor = 0;
	{{
{3}
	}}

	if( DepthNormals::WantsDepthNormals() )
		return DepthNormals::Output( m.Normal, m.Roughness );
	
	// TODO
	//if( ToolsVis::WantsToolsVis() )
	//	return DoToolsVis( Albedo, m, lightingTerms );

	// Composite atmospherics after lighting
{4}

	return float4(Albedo.xyz, m.Opacity);
}}
";
}
