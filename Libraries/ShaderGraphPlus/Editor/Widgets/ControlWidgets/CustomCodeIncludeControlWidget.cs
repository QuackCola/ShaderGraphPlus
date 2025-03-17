using System.Text;

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
    
    CustomFunctionNode Node;
    SerializedProperty FunctionNameProperty;
    
    public HLSLIncludeStringControlWidget( SerializedProperty property ) : base( property )
    {
        FilePath = property.GetValue<string>();
        
        Node = property.Parent.Targets.FirstOrDefault() as CustomFunctionNode;
    
        FunctionNameProperty = Node.GetSerialized().GetProperty( nameof( CustomFunctionNode.Name ) );
    
        if ( Node is null )
            return;
    
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
    
    public void GenerateHLSLIncludeBase()
    {
        if ( string.IsNullOrWhiteSpace( Node.Name ))
        {
            Dialog.AskString((string name) => {
                FunctionNameProperty.SetValue(name);
                Continue(name);
            }, "Please input Function Name.");
        }
        else
        {
            Continue( Node.Name );
        }
    }
    
    private void Continue( string functionName )
    {
        var funcHeader = $"{GetHLSLDataType(Node.ResultType)} {functionName}({Node.ConstructFunctionInputs()}, {Node.ConstructFunctionOutputs()})";
    
    
        var sb = new StringBuilder();
    
    
        var template = HLSLTemplate.Contents;
    
        var result = string.Format( HLSLTemplate.Contents,
            functionName.ToUpper(),
            funcHeader,
            ""
        );
    
        var fd = new FileDialog(null)
        {
            Title = $"Select Path To Save HLSL File",
            DefaultSuffix = $".hlsl"
        
        };
    
        fd.Directory = $"{Project.Current.GetAssetsPath()}/shaders";
        
        fd.SetNameFilter($"Shader Include (*.hlsl)");
        
        if ( !Directory.Exists( $"{Project.Current.GetAssetsPath()}/shaders" ) )
        {
            Directory.CreateDirectory( $"{Project.Current.GetAssetsPath()}/shaders" );
        }
        
        if (!fd.Execute())
            return;
    
        System.IO.File.WriteAllText( fd.SelectedFile, result);
        
        FilePath = Path.GetRelativePath( Project.Current.GetAssetsPath(), fd.SelectedFile).Replace('\\', '/').Remove(0, 8);
        
        UpdateProperty();
    
        Process.Start(new ProcessStartInfo
        {
            FileName = fd.SelectedFile,
            UseShellExecute = true
        });
    }
    
    private string GetHLSLDataType( ResultType resultType )
    {
        return resultType switch
        {
            ResultType r when r == ResultType.Bool => "bool",
            //ResultType r when r == ResultType.Int => "int",
            ResultType r when r == ResultType.Float => "float",
            ResultType r when r == ResultType.Vector2 => "float2",
            ResultType r when r == ResultType.Vector3 => "float3",
            ResultType r when r == ResultType.Color => "float4",
            ResultType r when r == ResultType.Gradient => "Gradient",
            ResultType r when r == ResultType.Float2x2 => "float2x2",
            ResultType r when r == ResultType.Float3x3 => "float3x3",
            ResultType r when r == ResultType.Float4x4 => "float4x4",
            ResultType r when r == ResultType.Void => "void",
            _ => throw new ArgumentException("Unsupported value type", nameof(resultType))
        };
    }
    
    
    
    protected override void OnMouseClick( MouseEvent e )
    {
        base.OnMouseClick( e);
        
        if ( string.IsNullOrWhiteSpace( FilePath ) )
        {
            GenerateHLSLIncludeBase();
        }
        else
        {
            OpenFileDialog();
        }
    }
    
    private void OpenFileDialog()
    {
        var fd = new FileDialog(null)
        {
            Title = $"Select HLSL File",
            DefaultSuffix = $".hlsl"
            
        };
    
        fd.Directory = $"{Project.Current.GetAssetsPath()}/shaders";
    
        fd.SetNameFilter($"Shader Include (*.hlsl)");
    
        if ( !Directory.Exists( $"{Project.Current.GetAssetsPath()}/shaders" ) )
        {
            Directory.CreateDirectory( $"{Project.Current.GetAssetsPath()}/shaders" );
        }
        
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