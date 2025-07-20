namespace Editor.ShaderGraphPlus;

internal static class ShaderGraphPlusTheme
{
	public static Dictionary<Type, HandleConfig> HandleConfigs { get; private set; }

	static ShaderGraphPlusTheme()
	{
		Update();
	}

	[Event( "hotloaded" )]
	static void Update()
	{
		HandleConfigs = new()
		{
		{ typeof( bool ), new HandleConfig( "bool", Theme.Blue.AdjustHue( -80 ) ) },
		{ typeof( float ), new HandleConfig( "Float", Color.Parse( "#8ec07c" ).Value ) },
		{ typeof( Vector2 ), new HandleConfig( "Vector2", Color.Parse( "#ce67e0" ).Value ) },
		{ typeof( Vector3 ), new HandleConfig( "Vector3", Color.Parse( "#7177e1" ).Value ) },
		{ typeof( Vector4 ), new HandleConfig( "Vector4", Color.Parse( "#e0d867" ).Value ) },
		{ typeof( Float2x2 ), new HandleConfig( "Float2x2", Color.Parse( "#a3b3c9" ).Value ) },
		{ typeof( Float3x3 ), new HandleConfig( "Float3x3", Color.Parse( "#a3b3c9" ).Value ) },
		{ typeof( Float4x4 ), new HandleConfig( "Float4x4", Color.Parse( "#a3b3c9" ).Value ) },
		{ typeof( Texture2DObject ), new HandleConfig( "Texture2D", Color.Parse( "#ffb3a7" ).Value ) },
		{ typeof( Sampler ), new HandleConfig( "Sampler", Color.Parse( "#dddddd" ).Value ) },
		{ typeof( Gradient ), new HandleConfig( "Gradient", Color.Parse( "#dddddd" ).Value ) },
		{ typeof( Color ), new HandleConfig( "Color", Color.Parse( "#c7ae32" ).Value ) },
		};
	}
}
