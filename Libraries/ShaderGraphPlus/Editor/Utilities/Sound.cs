namespace Editor.ShaderGraphPlus.Utilities;

public static class EdtiorSound
{
    public static void OhFiddleSticks()
    {
        EditorUtility.PlayRawSound(FileSystem.Content.GetFullPath("sounds/editor/kl_fiddlesticks.wav"));
    }

    public static void Success()
    {
        EditorUtility.PlayRawSound(FileSystem.Content.GetFullPath("sounds/editor/success.wav"));
    }
}