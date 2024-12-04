namespace Editor.ShaderGraphPlus.Diagnostics;

public static class Assert
{
    internal static string m_AssertSound => "sounds/editor/kl_fiddlesticks.wav";

    public static void CheckNull<T>(T obj, string message)
    {
        EditorUtility.PlayRawSound(FileSystem.Content.GetFullPath(m_AssertSound));
        Sandbox.Diagnostics.Assert.IsNull(obj, message);
    }

    public static void CheckNotNull<T>(T obj, string message)
    {
        EditorUtility.PlayRawSound(FileSystem.Content.GetFullPath(m_AssertSound));
        Sandbox.Diagnostics.Assert.NotNull(obj, message);
    }

    public static void CheckAreEqual<T>(T a, T b, string message = null)
    {
        EditorUtility.PlayRawSound(FileSystem.Content.GetFullPath(m_AssertSound));
        Sandbox.Diagnostics.Assert.AreEqual(a, b, message);
    }

    public static void CheckAreNotEqual<T>(T a, T b, string message = null)
    {
        EditorUtility.PlayRawSound(FileSystem.Content.GetFullPath(m_AssertSound));
        Sandbox.Diagnostics.Assert.AreNotEqual(a, b, message);
    }

    /// <summary>
    /// Returns true if input a equals input b. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool Check<T>(T a, T b)
    {
        if (object.Equals(a, b))
        {
            EditorUtility.PlayRawSound(FileSystem.Content.GetFullPath(m_AssertSound));
            return true;
        }
        else
        {
            return false;
        }
    }

}