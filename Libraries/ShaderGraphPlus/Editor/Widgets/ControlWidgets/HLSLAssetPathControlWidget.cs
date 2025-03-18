using System.Text;

namespace Editor.ShaderGraphPlus;

internal class HLSLAssetPathAttribute : Attribute
{
    internal HLSLAssetPathAttribute()
    {

    }
}

[CustomEditor( typeof(string), WithAllAttributes = new[] { typeof( HLSLAssetPathAttribute ) } )]
internal class HLSLAssetPathControlWidget : ControlWidget
{
    public override bool IsControlButton => true;
    public override bool SupportsMultiEdit => false;
    
    string FilePath;
    string FilePathAbsolute;

    IconButton PreviewButton;
    private ContextMenu menu;

    CustomFunctionNode Node;
    SerializedProperty FunctionNameProperty;
    
    public HLSLAssetPathControlWidget( SerializedProperty property ) : base( property )
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
        
        if ( PreviewButton.IsValid() )
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
                Generate(name);
            }, "What would you like to call your function?", title: "Function Name");
        }
        else
        {
            Generate( Node.Name );
        }
    }
    
    private void Generate( string functionName )
    {
        StringBuilder sb = new StringBuilder();
        
        string functionHeader = $"void {functionName}({Node.ConstructFunctionInputs()}{( Node.ExpressionInputs.Any() ? "," : "" )}{Node.ConstructFunctionOutputs()})";
        StringBuilder functionBody = new StringBuilder();
        
        foreach ( var output in Node.ExpressionOutputs )
        {
            var initialValue = "0";
        
            if ( output.HLSLDataType == "bool" )
            {
                initialValue = "false";
            }
            else if ( output.HLSLDataType == "int" )
            {
                initialValue = "0";
            }
            else if ( output.HLSLDataType == "float" )
            {
            	initialValue = "0.0f";
            }
            else if ( output.HLSLDataType == "float2" )
            {
                initialValue = "float2( 0.0f, 0.0f )";
            }
            else if ( output.HLSLDataType == "float3" )
            {
                initialValue = "float3( 0.0f, 0.0f, 0.0f )";
            }
            else if ( output.HLSLDataType == "float4" )
            {
                initialValue = "float4( 0.0f, 0.0f, 0.0f, 0.0f )";
            }
        
            functionBody.AppendLine( $"{output.Name} = {initialValue};" );
        }

        string result = string.Format( HLSLFunctionTemplate.Contents,
            functionName.ToUpper(),
            functionHeader,
            GraphCompiler.IndentString( functionBody.ToString(), 2 )
        );
        
        string absolutePath = SaveFile( result );
        
        if ( absolutePath is null ) 
            return;
        
        FilePathAbsolute = absolutePath;
        
        OpenFile();
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

    protected override void OnMousePress( MouseEvent e )
    {
        base.OnMouseClick( e);
        
        if ( string.IsNullOrWhiteSpace( FilePath ) )
        {
            GenerateHLSLIncludeBase();
        }
        else
        {
            if ( e.RightMouseButton ) 
            {
                if ( !string.IsNullOrWhiteSpace( Node.Source ) )
                {
                    FilePathAbsolute = Editor.FileSystem.Content.GetFullPath($"shaders/{Node.Source}");

                    menu?.Close();
                    menu = new ContextMenu();

                    menu.AddOption("Open include", "file_open", action: () => OpenFile());
                    menu.AddOption("Clear...", "delete", action: () => ClearFile());

                    menu.OpenAt(ScreenRect.BottomLeft);

                    e.Accepted = true;
                }
            }
        }
    }
    
    private void OpenFile()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = FilePathAbsolute,
            UseShellExecute = true
        });
    }

    private void ClearFile()
    {
        FilePathAbsolute = "";
        FilePath = "";

        UpdateProperty();
    }

    private string SaveFile( string generatedFile )
    {
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
            return null;
        
        System.IO.File.WriteAllText(fd.SelectedFile, generatedFile);
        
        FilePath = Path.GetRelativePath(Project.Current.GetAssetsPath(), fd.SelectedFile).Replace('\\', '/').Remove(0,8);
        
        UpdateProperty();
        
        return fd.SelectedFile;
    }

    private void UpdateProperty()
    {
        SerializedProperty.SetValue( FilePath );
    }
}