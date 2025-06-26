namespace Editor.ShaderGraphPlus;

public static class SGPLog
{
    private static Logger _log => new Logger( "ShaderGraphPlus" );

    /// <summary>
    /// Name of this logger.
    /// </summary>
    public static string Name => "ShaderGraphPlus";

	internal static IEnumerable<MainWindow> GetAllShadergraphPlusWindows()
	{
		return Editor.Window.All.OfType<MainWindow>();
	}

	public static void Info( string message, bool isNotPreview )
	{
		if ( isNotPreview )
			_log.Info( message );
	}

	public static void Info( string message )
	{
		_log.Info( message );
	}

	public static void Trace( string message )
    {
        _log.Trace( message );
    }

    public static void Warning( string message )
    {
        _log.Warning( message );
    }

    public static void Error( string message )
    {
        _log.Error( message );
    }

    public static void Error( Exception exception, string message )
    {
        _log.Error( exception, message );
    }
}
