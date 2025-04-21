namespace Editor.ShaderGraphPlus;

public static class LightingTemplate
{
	public static string Contents => @"
{0}
	for ( int index = 0; index < Light::Count( ScreenPos ); index++ )
	{{
{1}
{2}
	}}

	return Albedo;
";
}
