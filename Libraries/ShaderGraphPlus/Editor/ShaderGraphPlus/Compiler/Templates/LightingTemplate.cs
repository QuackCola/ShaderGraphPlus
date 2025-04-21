namespace Editor.ShaderGraphPlus;

public static class LightingTemplate
{
	public static string Contents => @"
static float4 Shade( Material m  )
{{
{0}
	for ( int index = 0; index < Light::Count( m.ScreenPosition.xy ); index++ )
	{{
		Light light = Light::From( m.ScreenPosition.xy, m.WorldPosition, index);
{1}
{2}
	}}

	return float4(Albedo.xyz, 0);
}}
";
}
