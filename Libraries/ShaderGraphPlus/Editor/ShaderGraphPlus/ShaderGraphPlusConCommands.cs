namespace Editor.ShaderGraphPlus;

internal static class ConCommands
{
	public static bool VerboseDebgging { get; internal set; } = false;

	internal static IEnumerable<MainWindow> GetAllShaderGraphPlusWindows()
	{
		return Editor.Window.All.OfType<MainWindow>();
	}

	[ConCmd( "shadergraphplus_verbosedebug" )]
	public static void CC_VerboseDebugging( bool verboseDebgging )
	{
		VerboseDebgging = verboseDebgging;
	}
}
