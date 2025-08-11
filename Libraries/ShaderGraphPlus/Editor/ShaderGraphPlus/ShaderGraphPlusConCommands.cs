namespace ShaderGraphPlus;

internal static class ConCommands
{
	public static bool VerboseDebgging { get; internal set; } = false;

	public static bool NodeDebugInfo { get; internal set; } = false;

	public static bool OnPropertyUpdatedDebug { get; internal set; } = false;

	public static bool TextureNodeDebug { get; internal set; } = false;

	internal static IEnumerable<MainWindow> GetAllShaderGraphPlusWindows()
	{
		return Editor.Window.All.OfType<MainWindow>();
	}

	[ConCmd( "sgp_verbosedebug" )]
	public static void CC_VerboseDebugging( bool value )
	{
		VerboseDebgging = value;
	}

	[ConCmd( "sgp_debugnodeinfo" )]
	public static void CC_DebugNodeInfo( bool value )
	{
		NodeDebugInfo = value;
	}

	[ConCmd( "sgp_onpropupdatedlogging" )]
	public static void CC_OnPropertyUpdatedDebug( bool value )
	{
		OnPropertyUpdatedDebug = value;
	}

	[ConCmd( "sgp_texnodedebug" )]
	public static void CC_TexureNodeDebug( bool value )
	{
		TextureNodeDebug = value;
	}
}
