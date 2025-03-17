namespace Editor.ShaderGraphPlus;

internal class HLSLAssetPathAttribute : Attribute
{
    internal HLSLAssetPathAttribute()
    {

    }
}

[CustomEditor( typeof(string), WithAllAttributes = new[] { typeof( HLSLAssetPathAttribute ) } )]
internal class HLSLIncludeStringControlWidget : ControlWidget
{
    public override bool IsControlButton => true;
    public override bool SupportsMultiEdit => true;
    
    string FilePath;
    
    IconButton PreviewButton;
    
    public HLSLIncludeStringControlWidget( SerializedProperty property ) : base( property )
    {
        FilePath = property.GetValue<string>();
        
        HorizontalSizeMode = SizeMode.CanGrow | SizeMode.Expand;
        Cursor = CursorShape.Finger;
        MouseTracking = true;
        AcceptDrops = true;
        IsDraggable = true;
    }
    
    protected override void DoLayout()
    {
        base.DoLayout();
        
        if (PreviewButton.IsValid())
        {
            PreviewButton.FixedSize = Height - 2;
            PreviewButton.Position = new Vector2(Width - Height + 1, 1);
        }
    }
    
    private void DrawContent( Rect rect, string title, string path )
    {
        bool multiline = Height > 32;
        Rect textRect = rect.Shrink( 0, 6 );
        var alpha = IsControlDisabled ? 0.6f : 1f;
        
        if ( multiline )
        {
        	textRect = new Rect( textRect.TopLeft, new Vector2( textRect.Width, textRect.Height / 2 ) );
        }
        
        Paint.SetPen( Color.White.WithAlpha( 0.9f * alpha ) );
        Paint.SetFont( "Poppins", 8, 450 );
        var t = Paint.DrawText( textRect, title, multiline ? TextFlag.LeftCenter : TextFlag.LeftCenter );
        
        if ( multiline )
        {
        	textRect.Position += new Vector2( 0, textRect.Height );
        }
        else
        {
        	textRect.Left = t.Right + 6;
        }
        
        Paint.SetDefaultFont( 7 );
        Theme.DrawFilename( textRect, path, multiline ? TextFlag.LeftCenter : TextFlag.LeftBottom, Color.White.WithAlpha( 0.5f * alpha ) );
    }
    
    protected override void PaintControl()
    {
        var rect = new Rect(0, Size);
        
        var iconRect = rect.Shrink(2);
        iconRect.Width = iconRect.Height;
        
        var alpha = IsControlDisabled ? 0.6f : 1f;
        var textRect = rect.Shrink(0, 3);
        var pickerName = DisplayInfo.ForType(SerializedProperty.PropertyType).Name;
        
        //Paint.SetBrush(Theme.Red.Darken(0.8f).WithAlpha(alpha));
        //Paint.DrawRect(iconRect, 2);
        
        //Paint.SetPen(Theme.Red.WithAlpha(alpha));
        //Paint.DrawIcon(iconRect, "error", Math.Max(16, iconRect.Height / 2));
        
        DrawContent(rect, $"", FilePath);
    }
    
    protected override void OnMouseClick( MouseEvent e )
    {
        base.OnMouseClick( e );
        
        var fd = new FileDialog(null)
        {
            Title = $"Select HLSL File",
            DefaultSuffix = $".hlsl"
            
        };
        
        if ( !Directory.Exists( $"{Project.Current.GetAssetsPath()}/shaders" ) )
        {
            Directory.CreateDirectory( $"{Project.Current.GetAssetsPath()}/shaders" );
        }
        
        fd.Directory = $"{Project.Current.GetAssetsPath()}/shaders";
        
        fd.SelectFile($"");
        fd.SetFindFile();
        fd.SetModeOpen();
        fd.SetNameFilter($"Shader Include (*.hlsl)");
        
        
        if (!fd.Execute())
            return;
        
        FilePath = Path.GetRelativePath(Project.Current.GetAssetsPath(), fd.SelectedFile).Replace('\\', '/').Remove(0,8);
        
        UpdateProperty();
    }
    
    
    private void UpdateProperty()
    {
        SerializedProperty.SetValue( FilePath );
    }
}