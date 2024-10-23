﻿namespace Editor.ShaderGraphPlus;

[EditorForAssetType( "sgrph" )]
[EditorApp( "Shader Graph Plus", "gradient", "edit shaders" )]
public class MainWindow : DockWindow, IAssetEditor
{
	private ShaderGraphPlus _graph;
	private ShaderGraphPlusView _graphView;
	private Asset _asset;

	public string AssetPath => _asset?.Path;

	private Widget _graphCanvas;
	private Properties _properties;
	private PreviewPanel _preview;
	private Output _output;
	private UndoHistory _undoHistory;
	private PaletteWidget _palette;

	private readonly UndoStack _undoStack = new();

	private Option _undoOption;
	private Option _redoOption;

	private Option _undoMenuOption;
	private Option _redoMenuOption;

	public UndoStack UndoStack => _undoStack;

	private bool _dirty = false;

	private string _generatedCode;
	private readonly Dictionary<string, Texture> _textureAttributes = new();
	private readonly Dictionary<string, Float2x2> _float2x2Attributes = new();
	private readonly Dictionary<string, Float3x3> _float3x3Attributes = new();
	private readonly Dictionary<string, Float4x4> _float4x4Attributes = new();
	private readonly Dictionary<string, Color> _float4Attributes = new();
	private readonly Dictionary<string, Vector3> _float3Attributes = new();
	private readonly Dictionary<string, Vector2> _float2Attributes = new();
	private readonly Dictionary<string, float> _floatAttributes = new();
	private readonly Dictionary<string, bool> _boolAttributes = new();

	private readonly List<BaseNodePlus> _compiledNodes = new();

	private bool _isCompiling = false;
	private bool _isPendingCompile = false;
	private RealTimeSince _timeSinceCompile;

	private Menu _recentFilesMenu;
	private readonly List<string> _recentFiles = new();

	private string _defaultDockState;

	public bool CanOpenMultipleAssets => true;

	public MainWindow()
	{
		DeleteOnClose = true;

		Title = "Shader Graph Plus";
		Size = new Vector2( 1700, 1050 );

		_graph = new();

		CreateToolBar();

		_recentFiles = FileSystem.Temporary.ReadJsonOrDefault( "shadergraphplus_recentfiles.json", _recentFiles )
			.Where( x => System.IO.File.Exists( x ) ).ToList();

		CreateUI();
		Show();

		CreateNew();

	}

	public void AssetOpen( Asset asset )
	{
		if ( asset == null || string.IsNullOrWhiteSpace( asset.AbsolutePath ) )
			return;

		Open( asset.AbsolutePath );
	}

	private void RestoreShader()
	{
		if ( !_preview.IsValid() )
			return;

		_preview.Material = Material.Load( "materials/core/shader_editor.vmat" );
		_preview.PostProcessingMaterial = Material.Load( "materials/core/ShaderGraphPlus/shader_editor_postprocess.vmat" );
	}

	public void OnNodeSelected( BaseNodePlus node )
	{
		_properties.Target = node != null ? node : _graph;

		_preview.SetStage( _compiledNodes.IndexOf( node ) + 1 );
	}

	private void OpenGeneratedShader()
	{
		if ( _asset is null )
		{
			Save();
		}
		else
		{
			var path = System.IO.Path.ChangeExtension( _asset.AbsolutePath, ".shader" );
			var asset = AssetSystem.FindByPath( path );
			Log.Info( path );
			asset?.OpenInEditor();
		}
	}

	private void OpenTempGeneratedShader()
	{
		if ( !_dirty )
		{
			SetDirty();
		}

		var assetPath = $"shadergraphplus/{_asset?.Name ?? "untitled"}_shadergraphplus.generated.shader";
		var resourcePath = System.IO.Path.Combine( FileSystem.Temporary.GetFullPath( "/temp" ), assetPath );
		var asset = AssetSystem.FindByPath( resourcePath );

		//Log.Info( asset.AbsolutePath );

		asset?.OpenInEditor();
	}

    private void OpenShaderGraphProjectTxt()
    {
        if (_asset is null)
        {
            Save();
        }
        else
        {
            var path = _asset.AbsolutePath;
            Utilities.Path.OpenInNotepad(path);
        }
    }
    private void Compile()
	{

		if ( string.IsNullOrWhiteSpace( _generatedCode ) )
		{
			RestoreShader();

			return;
		}

		if ( _isCompiling )
		{
			_isPendingCompile = true;

			return;
		}

		var assetPath = $"shadergraphplus/{_asset?.Name ?? "untitled"}_shadergraphplus.generated.shader";
		var resourcePath = System.IO.Path.Combine( ".source2/temp", assetPath );

		FileSystem.Root.CreateDirectory( ".source2/temp/shadergraphplus" );
		FileSystem.Root.WriteAllText( resourcePath, _generatedCode );

		_isCompiling = true;
		_preview.IsCompiling = _isCompiling;
		
		var p = new Process();
		p.StartInfo.FileName = "bin/win64/vfxcompile.exe";
		p.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
		p.StartInfo.CreateNoWindow = true;
		p.StartInfo.RedirectStandardOutput = true;
		p.StartInfo.RedirectStandardError = true;
		p.StartInfo.UseShellExecute = false;

		p.StartInfo.ArgumentList.Add( assetPath );
		p.StartInfo.ArgumentList.Add( "-progress" );
		p.StartInfo.ArgumentList.Add( "-robot" );
		p.StartInfo.ArgumentList.Add( "-verbose" );
		p.StartInfo.ArgumentList.Add( "-searchpaths" );
		p.StartInfo.ArgumentList.Add( EditorUtility.GetSearchPaths() );

		RestoreShader();

		_timeSinceCompile = 0;

		p.OutputDataReceived += OnOutputData;
		p.ErrorDataReceived += OnErrorData;
		_shaderCompileErrors.Clear();
		_Warnings.Clear();

		p.Start();
		p.BeginOutputReadLine();
		p.BeginErrorReadLine();

		_ = Task.Run( () => MonitorProcessAsync( p ) );
	}

	private readonly List<string> _shaderCompileErrors = new();
	private readonly List<string> _Warnings = new();

	private struct StatusMessage
	{
		public string Status { get; set; }
		public string Message { get; set; }
	}

	internal void OnOutputData( object sender, DataReceivedEventArgs e )
	{
		var str = e.Data;
		if ( str == null )
			return;

		MainThread.Queue( () =>
		{
			var trimmed = str.Trim();
			if ( trimmed.StartsWith( '{' ) && trimmed.EndsWith( '}' ) )
			{
				var status = Json.Deserialize<StatusMessage>( trimmed );
				if ( status.Status == "Error" )
				{
					if ( !string.IsNullOrWhiteSpace( status.Message ) )
					{
						var lines = status.Message.Split( '\n' );
						foreach ( var line in lines.Where( x => !string.IsNullOrWhiteSpace( x ) ) )
							_shaderCompileErrors.Add( line );
					}
				}
			}
		} );
	}

	internal void OnErrorData( object sender, DataReceivedEventArgs e )
	{
		if ( e == null || e.Data == null )
			return;

		MainThread.Queue( () =>
		{
			var error = $"{e.Data}";
			if ( !string.IsNullOrWhiteSpace( error ) )
				_shaderCompileErrors.Add( error );
		} );
	}

	internal async Task MonitorProcessAsync( Process p )
	{
		await p.WaitForExitAsync();

		MainThread.Queue( () => OnCompileFinished( p.ExitCode ) );
	}

	private void OnCompileFinished( int exitCode )
	{
		_isCompiling = false;

		if ( _isPendingCompile )
		{
			_isPendingCompile = false;

			Compile();

			return;
		}

		if ( exitCode == 0 )
		{
            Log.Info( $"Compile finished in {_timeSinceCompile}" );

			var shaderPath = $"shadergraphplus/{_asset?.Name ?? "untitled"}_shadergraphplus.generated.shader";

			// Reload the shader otherwise it's gonna be the old wank
			// Alternatively Material.Create could be made to force reload the shader
			ConsoleSystem.Run( $"mat_reloadshaders {shaderPath}" );

			var created_mat = Material.Create( $"{_asset?.Name ?? "untitled"}_shadergraphplus_generated", shaderPath );


			if ( _graph.MaterialDomain is MaterialDomain.Surface )
			{
				_preview.PostProcessingMaterial = Material.Load( "materials/core/ShaderGraphPlus/shader_editor_postprocess.vmat" );
				_preview.Material = created_mat;
				_preview.IsPostProcessShader = false;
			}
			else
			{
				_preview.PostProcessingMaterial = created_mat;
				_preview.Material = Material.Load( "materials/core/shader_editor.vmat" );
				_preview.IsPostProcessShader = true;
			}

			Log.Info( $"Created Material : {created_mat.Name}" );
		}
		else
		{
			Log.Error( $"Compile failed in {_timeSinceCompile}" );

			_output.Errors = _shaderCompileErrors.Select( x => new GraphCompiler.Error { Message = x } );

			DockManager.RaiseDock( "Output" );

			if ( _graph.MaterialDomain is MaterialDomain.Surface )
			{
				_preview.IsPostProcessShader = false;
			}
			else
			{
				_preview.IsPostProcessShader = true;
			}

			RestoreShader();
			ClearAttributes();
		}

		_preview.IsCompiling = _isCompiling;

		_shaderCompileErrors.Clear();
	}

	private void OnAttribute( string name, object value )
	{
		if ( value == null )
			return;

		switch ( value )
		{
			case Color v:
				_float4Attributes.Add( name, v );
				_preview?.SetAttribute( name, v );
				break;
			case Vector4 v:
				_float4Attributes.Add( name, v );
				_preview?.SetAttribute( name, (Color)v );
				break;
			case Vector3 v:
				_float3Attributes.Add( name, v );
				_preview?.SetAttribute( name, v );
				break;
			case Vector2 v:
				_float2Attributes.Add( name, v );
				_preview?.SetAttribute( name, v );
				break;
			case float v:
				_floatAttributes.Add( name, v );
				_preview?.SetAttribute( name, v );
				break;
			case bool v:
				_boolAttributes.Add( name, v );
				_preview?.SetAttribute( name, v );
				break;
			case Texture v:
				_textureAttributes.Add( name, v );
				_preview?.SetAttribute( name, v );
				break;
			case Float2x2 v: // Stub - Quack
				_float2x2Attributes.Add( name, v );
				_preview?.SetAttribute( name, v );
				break;
			case Float3x3 v: // Stub - Quack
				_float3x3Attributes.Add( name, v );
				_preview?.SetAttribute( name, v );
				break;
			case Float4x4 v: // Stub - Quack
				_float4x4Attributes.Add( name, v );
				_preview?.SetAttribute( name, v );
				break;
			default:
				throw new InvalidOperationException( $"Unsupported attribute type: {value.GetType()}" );
		}
	}


	private string GeneratePostProcessPreviewCode()
	{
		ClearAttributes();

		var resultNode = _graph.Nodes.OfType<PostProcessingResult>().FirstOrDefault();

		var compiler = new GraphCompiler(_asset, _graph, true );
		compiler.OnAttribute = OnAttribute;

		// Evaluate all nodes
		foreach ( var node in _graph.Nodes.OfType<BaseNodePlus>() )
		{
            var property = node.GetType().GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static )
				.FirstOrDefault( x => x.GetGetMethod() != null && x.PropertyType == typeof( NodeResult.Func ) );

			if ( property == null )
				continue;

			var output = property.GetCustomAttribute<BaseNodePlus.OutputAttribute>();
			if ( output == null )
				continue;

			var result = compiler.Result( new NodeInput { Identifier = node.Identifier, Output = property.Name } );
			if ( !result.IsValid() )
				continue;

			var componentType = result.ComponentType;

			if ( componentType == null )
				continue;

			// While we're here, let's check the output plugs and update their handle configs to the result type

			var nodeUI = _graphView.FindNode( node );
			if ( !nodeUI.IsValid() )
				continue;

			var plugOut = nodeUI.Outputs.FirstOrDefault( x => ((BasePlug)x.Inner).Property == property );
			if ( !plugOut.IsValid() )
				continue;

			plugOut.PropertyType = componentType;

			// We also have to update everything so they get repainted

			nodeUI.Update();

			foreach ( var input in nodeUI.Inputs )
			{
				if ( !input.IsValid() || !input.Connection.IsValid() )
					continue;

				input.Connection.Update();
			}
		}

		_compiledNodes.Clear();
		_compiledNodes.AddRange( compiler.Nodes );

		if ( _properties.IsValid() && _properties.Target is BaseNodePlus targetNode )
		{
			_preview.SetStage( _compiledNodes.IndexOf( targetNode ) + 1 );
		}

		if ( resultNode != null )
		{
			var nodeUI = _graphView.FindNode( resultNode );
			if ( nodeUI.IsValid() )
			{
				nodeUI.Update();

				foreach ( var input in nodeUI.Inputs )
				{
					if ( !input.IsValid() || !input.Connection.IsValid() )
						continue;

					input.Connection.Update();
				}
			}
		}

		var code = compiler.Generate();


		if ( compiler.Errors.Any()  )
		{
			_output.Errors = compiler.Errors;

			DockManager.RaiseDock( "Output" );

			_generatedCode = null;

			RestoreShader();

			return null;
		}

		_output.Clear();

		if ( _generatedCode != code.Item1 )
		{
			_generatedCode = code.Item1;

			Compile();
		}
		return code.Item1;

	}


	private string GeneratePreviewCode()
	{
		ClearAttributes();

		var resultNode = _graph.Nodes.OfType<Result>().FirstOrDefault();

		var compiler = new GraphCompiler( _asset, _graph, true );

        //compiler.ClearGradientsDict();

        compiler.OnAttribute = OnAttribute;

		// Evaluate all nodes
		foreach ( var node in _graph.Nodes.OfType<BaseNodePlus>() )
		{
            var property = node.GetType().GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static )
				.FirstOrDefault( x => x.GetGetMethod() != null && x.PropertyType == typeof( NodeResult.Func ) );

			if ( property == null )
				continue;

			var output = property.GetCustomAttribute<BaseNodePlus.OutputAttribute>();
			if ( output == null )
				continue;

			var result = compiler.Result( new NodeInput { Identifier = node.Identifier, Output = property.Name } );
			if ( !result.IsValid() )
				continue;

			var componentType = result.ComponentType;
			if ( componentType == null )
				continue;

			// While we're here, let's check the output plugs and update their handle configs to the result type

			var nodeUI = _graphView.FindNode( node );
			if ( !nodeUI.IsValid() )
				continue;

			var plugOut = nodeUI.Outputs.FirstOrDefault( x => ((BasePlug)x.Inner).Property == property );
			if ( !plugOut.IsValid() )
				continue;

			plugOut.PropertyType = componentType;

			// We also have to update everything so they get repainted

			nodeUI.Update();

			foreach ( var input in nodeUI.Inputs )
			{
				if ( !input.IsValid() || !input.Connection.IsValid() )
					continue;

				input.Connection.Update();
			}
		}

		_compiledNodes.Clear();
		_compiledNodes.AddRange( compiler.Nodes );

		if ( _properties.IsValid() && _properties.Target is BaseNodePlus targetNode )
		{
			_preview.SetStage( _compiledNodes.IndexOf( targetNode ) + 1 );
		}

		if ( resultNode != null )
		{
			var nodeUI = _graphView.FindNode( resultNode );
			if ( nodeUI.IsValid() )
			{
				nodeUI.Update();

				foreach ( var input in nodeUI.Inputs )
				{
					if ( !input.IsValid() || !input.Connection.IsValid() )
						continue;

					input.Connection.Update();
				}
			}
		}

		var code = compiler.Generate();

		//if ( compiler.Warnings.Any() )
		//{
		//	_output.Warnings = compiler.Warnings;
		//	DockManager.RaiseDock( "Output" );
		//}

		if ( compiler.Errors.Any() )
		{
			_output.Errors = compiler.Errors;
			DockManager.RaiseDock( "Output" );

			_generatedCode = null;

			RestoreShader();

			return null;
		}

		_output.Clear();

		if ( _generatedCode != code.Item1 )
		{
			_generatedCode = code.Item1;

			Compile();
		}
		return code.Item1;

	}

	private (string,string) GenerateShaderCode()
	{
		var compiler = new GraphCompiler( _asset, _graph, false );

        return compiler.Generate();
	}

	public void OnUndoPushed()
	{
		_undoHistory.History = _undoStack.Names;
	}

	public void SetDirty()
	{
		_dirty = true;
		_graphCanvas.WindowTitle = $"{_asset?.Name ?? "untitled"}*";

		if ( _graph.MaterialDomain is MaterialDomain.Surface )
		{
			GeneratePreviewCode();
		}
		else
		{
			GeneratePostProcessPreviewCode();
		}
	}

	[EditorEvent.Frame]
	protected void Frame()
	{
		_undoOption.Enabled = _undoStack.CanUndo;
		_redoOption.Enabled = _undoStack.CanRedo;
		_undoMenuOption.Enabled = _undoStack.CanUndo;
		_redoMenuOption.Enabled = _undoStack.CanRedo;

		_undoOption.Text = _undoStack.UndoName;
		_redoOption.Text = _undoStack.RedoName;
		_undoMenuOption.Text = _undoStack.UndoName ?? "Undo";
		_redoMenuOption.Text = _undoStack.RedoName ?? "Redo";

		_undoOption.StatusText = _undoStack.UndoName;
		_redoOption.StatusText = _undoStack.RedoName;
		_undoMenuOption.StatusText = _undoStack.UndoName;
		_redoMenuOption.StatusText = _undoStack.RedoName;

		_undoHistory.UndoLevel = _undoStack.UndoLevel;
	}

	[Shortcut( "editor.undo", "CTRL+Z" )]
	private void Undo()
	{
		if ( _undoStack.Undo() is UndoOp op )
		{
			Log.Info( $"Undo ({op.name})" );

			_redoOption.Enabled = _undoStack.CanUndo;

			_graph.ClearNodes();
			_graph.DeserializeNodes( op.undoBuffer );
			_graphView.RebuildFromGraph();

			SetDirty();
		}
	}

	[Shortcut( "editor.redo", "CTRL+Y" )]
	private void Redo()
	{
		if ( _undoStack.Redo() is UndoOp op )
		{
			Log.Info( $"Redo ({op.name})" );

			_redoOption.Enabled = _undoStack.CanRedo;

			_graph.ClearNodes();
			_graph.DeserializeNodes( op.redoBuffer );
			_graphView.RebuildFromGraph();

			SetDirty();
		}
	}

	private void SetUndoLevel( int level )
	{
		if ( _undoStack.SetUndoLevel( level ) is UndoOp op )
		{
			Log.Info( $"SetUndoLevel ({op.name})" );

			_graph.ClearNodes();
			_graph.DeserializeNodes( op.redoBuffer );
			_graphView.RebuildFromGraph();

			SetDirty();
		}
	}

	[Shortcut( "editor.cut", "CTRL+X" )]
	private void CutSelection()
	{
		_graphView.CutSelection();
	}

	[Shortcut( "editor.copy", "CTRL+C" )]
	private void CopySelection()
	{
		_graphView.CopySelection();
	}

	[Shortcut( "editor.paste", "CTRL+V" )]
	private void PasteSelection()
	{
		_graphView.PasteSelection();
	}

	[Shortcut( "editor.select-all", "CTRL+A" )]
	private void SelectAll()
	{
		_graphView.SelectAll();
	}

	private void CreateToolBar()
	{
		var toolBar = new ToolBar( this, "ShaderGraphPlusToolbar" );
		AddToolBar( toolBar, ToolbarPosition.Top );

		toolBar.AddOption( "New", "common/new.png", New ).StatusText = "New Graph";
		toolBar.AddOption( "Open", "common/open.png", Open ).StatusText = "Open Graph";
		toolBar.AddOption( "Save", "common/save.png", () => Save() ).StatusText = "Save Graph";

		toolBar.AddSeparator();

		_undoOption = toolBar.AddOption( "Undo", "undo", Undo );
		_redoOption = toolBar.AddOption( "Redo", "redo", Redo );

		toolBar.AddSeparator();

		toolBar.AddOption( "Cut", "common/cut.png", CutSelection );
		toolBar.AddOption( "Copy", "common/copy.png", CopySelection );
		toolBar.AddOption( "Paste", "common/paste.png", PasteSelection );
		toolBar.AddOption( "Select All", "select_all", SelectAll );

		toolBar.AddSeparator();

		toolBar.AddOption( "Compile", "refresh", () => Compile() ).StatusText = "Compile Graph";

		toolBar.AddOption( "Open Generated Shader", "txtedit/appicon.png", () => OpenGeneratedShader() ).StatusText = "Open Generated Shader";

		_undoOption.Enabled = false;
		_redoOption.Enabled = false;
	}

	public void BuildMenuBar()
	{
		var file = MenuBar.AddMenu( "File" );
		file.AddOption( "New", "common/new.png", New, "editor.new" ).StatusText = "New Graph";
		file.AddOption( "Open", "common/open.png", Open, "editor.open" ).StatusText = "Open Graph";
		file.AddOption( "Save", "common/save.png", Save, "editor.save" ).StatusText = "Save Graph";
		file.AddOption( "Save As...", "common/save.png", SaveAs, "editor.save-as" ).StatusText = "Save Graph As...";

		file.AddSeparator();

		_recentFilesMenu = file.AddMenu( "Recent Files" );

		file.AddSeparator();

		file.AddOption( "Quit", null, Quit, "editor.quit" ).StatusText = "Quit";

		var edit = MenuBar.AddMenu( "Edit" );
		_undoMenuOption = edit.AddOption( "Undo", "undo", Undo, "editor.undo" );
		_redoMenuOption = edit.AddOption( "Redo", "redo", Redo, "editor.redo" );
		_undoMenuOption.Enabled = _undoStack.CanUndo;
		_redoMenuOption.Enabled = _undoStack.CanRedo;

		edit.AddSeparator();
		edit.AddOption( "Cut", "common/cut.png", CutSelection, "editor.cut" );
		edit.AddOption( "Copy", "common/copy.png", CopySelection, "editor.copy" );
		edit.AddOption( "Paste", "common/paste.png", PasteSelection, "editor.paste" );
		edit.AddOption( "Select All", "select_all", SelectAll, "editor.select-all" );

		var debug = MenuBar.AddMenu( "Debug" );
		debug.AddSeparator();
		debug.AddOption( "Open Temp Shader", "txtedit/appicon.png", OpenTempGeneratedShader );
        debug.AddOption("Open ShaderGraph Project in text editor", "txtedit/appicon.png", OpenShaderGraphProjectTxt);

        RefreshRecentFiles();

		var view = MenuBar.AddMenu( "View" );
		view.AboutToShow += () => OnViewMenu( view );
	}

	[Shortcut( "editor.quit", "CTRL+Q" )]
	void Quit()
	{
		Close();
	}
	void RefreshRecentFiles()
	{
		_recentFilesMenu.Enabled = _recentFiles.Count > 0;

		_recentFilesMenu.Clear();

		_recentFilesMenu.AddOption( "Clear recent files", null, ClearRecentFiles )
			.StatusText = "Clear recent files";

		_recentFilesMenu.AddSeparator();

		const int maxFilesToDisplay = 10;
		int fileCount = 0;

		for ( int i = _recentFiles.Count - 1; i >= 0; i-- )
		{
			if ( fileCount >= maxFilesToDisplay )
				break;

			var filePath = _recentFiles[i];

			_recentFilesMenu.AddOption( $"{++fileCount} - {filePath}", null, () => PromptSave( () => Open( filePath ) ) )
				.StatusText = $"Open {filePath}";
		}
	}

	private void OnViewMenu( Menu view )
	{
		view.Clear();
		view.AddOption( "Restore To Default", "settings_backup_restore", RestoreDefaultDockLayout );
		view.AddSeparator();

		foreach ( var dock in DockManager.DockTypes )
		{
			var o = view.AddOption( dock.Title, dock.Icon );
			o.Checkable = true;
			o.Checked = DockManager.IsDockOpen( dock.Title );
			o.Toggled += ( b ) => DockManager.SetDockState( dock.Title, b );
		}

        // Doesn't work yet.
        //var style = view.AddOption("Grid-Aligned Wires", "turn_sharp_right");
        //style.Checkable = false;//true;
        //style.Checked = ShaderGraphPlusView.EnableGridAlignedWires;
        //style.Toggled += b => ShaderGraphPlusView.EnableGridAlignedWires = b;

    }

    private void ClearRecentFiles()
	{
		if ( _recentFiles.Count == 0 )
			return;

		_recentFiles.Clear();

		RefreshRecentFiles();

		SaveRecentFiles();
	}

	private void AddToRecentFiles( string filePath )
	{
		filePath = filePath.ToLower();

		// If file is already recent, remove it so it'll become the most recent
		if ( _recentFiles.Contains( filePath ) )
		{
			_recentFiles.RemoveAll( x => x == filePath );
		}

		_recentFiles.Add( filePath );

		RefreshRecentFiles();
		SaveRecentFiles();
	}

	private void SaveRecentFiles()
	{
		FileSystem.Temporary.WriteJson( "shadergraphplus_recentfiles.json", _recentFiles );
	}

	private void PromptSave( Action action )
	{
		if ( !_dirty )
		{
			action?.Invoke();
			return;
		}

		var confirm = new PopupWindow(
			"Save Current Graph", "The open graph has unsaved changes. Would you like to save now?", "Cancel",
			new Dictionary<string, Action>()
			{
				{ "No", () => action?.Invoke() },
				{ "Yes", () => { if ( SaveInternal( false ) ) action?.Invoke(); } }
			}
		);

		confirm.Show();
	}

	[Shortcut( "editor.new", "CTRL+N" )]
	public void New()
	{
		PromptSave( CreateNew );
	}

	public void CreateNew()
	{
		_asset = null;
		_graph = new();
		_dirty = false;
		_graphView.Graph = _graph;
		_graphCanvas.WindowTitle = "untitled";
		_preview.Model = null;
		_preview.Tint = Color.White;
		_undoStack.Clear();
		_undoHistory.History = _undoStack.Names;
		_generatedCode = "";
		_properties.Target = _graph;

		_output.Clear();

		var result = _graphView.CreateNewNode( _graphView.FindNodeType( typeof( Result ) ), 0 );

		_graphView.Scale = 1;
		_graphView.CenterOn( result.Size * 0.5f );

		ClearAttributes();

		RestoreShader();
	}

	private void ClearAttributes()
	{
		_textureAttributes.Clear();
		_float2x2Attributes.Clear();
		_float3x3Attributes.Clear();
		_float4x4Attributes.Clear();
		_float4Attributes.Clear();
		_float3Attributes.Clear();
		_float2Attributes.Clear();
		_floatAttributes.Clear();
		_boolAttributes.Clear();
		_compiledNodes.Clear();

		_preview?.ClearAttributes();
	}

	public void Open()
	{
		var fd = new FileDialog( null )
		{
			Title = "Open Shader Graph Plus",
			DefaultSuffix = $".sgrph"
		};

		fd.SetNameFilter( "Shader Graph (*.sgrph)" );

		if ( !fd.Execute() )
			return;

		PromptSave( () => Open( fd.SelectedFile ) );
	}

	public void Open( string path )
	{
		var asset = AssetSystem.FindByPath( path );
		if ( asset == null )
			return;

		if ( asset == _asset )
		{
			Log.Warning( $"{asset.RelativePath} is already open" );
			return;
		}

		var graph = new ShaderGraphPlus();
		graph.Deserialize( System.IO.File.ReadAllText( path ) );

		_preview.Model = Model.Load( graph.Model );

		_asset = asset;
		_graph = graph;
		_dirty = false;
		_graphView.Graph = _graph;
		_graphCanvas.WindowTitle = _asset.Name;
		_undoStack.Clear();
		_undoHistory.History = _undoStack.Names;
		_generatedCode = "";
		_properties.Target = _graph;

		_output.Clear();

		var center = Vector2.Zero;
		var resultNode = graph.Nodes.OfType<Result>().FirstOrDefault();
		if ( resultNode != null )
		{
			var nodeUI = _graphView.FindNode( resultNode );
			if ( nodeUI.IsValid() )
			{
				center = nodeUI.Position + nodeUI.Size * 0.5f;
			}
		}

		_graphView.Scale = 1;
		_graphView.CenterOn( center );
		_graphView.RestoreViewFromCookie();

		ClearAttributes();

		AddToRecentFiles( path );

		if ( _graph.MaterialDomain is MaterialDomain.Surface )
		{
			_preview.IsPostProcessShader = false;
			GeneratePreviewCode();
		}
		else
		{
			_preview.IsPostProcessShader = true;
			GeneratePostProcessPreviewCode();
		}
	}


	[Shortcut( "editor.save-as", "CTRL+SHIFT+S" )]
	public void SaveAs()
	{
		SaveInternal( true );
	}

	[Shortcut( "editor.save", "CTRL+S" )]
	public void Save()
	{
		SaveInternal( false );
	}

	private bool SaveInternal( bool saveAs )
	{
		var savePath = _asset == null || saveAs ? GetSavePath() : _asset.AbsolutePath;
		if ( string.IsNullOrWhiteSpace( savePath ) )
			return false;

		// Write serialized graph to asset file
		System.IO.File.WriteAllText( savePath, _graph.Serialize() );

		if ( saveAs )
		{
			// If we're saving as, we want to register the new asset
			_asset = null;
		}

		// Register asset if we haven't already
		_asset ??= AssetSystem.RegisterFile( savePath );

		if ( _asset == null )
		{
			Log.Warning( $"Unable to register asset {savePath}" );

			return false;
		}

		MainAssetBrowser.Instance?.UpdateAssetList();

		_dirty = false;
		_graphCanvas.WindowTitle = _asset.Name;

		var shaderPath = System.IO.Path.ChangeExtension( savePath, ".shader" );

		var code = GenerateShaderCode();

		if ( string.IsNullOrWhiteSpace( code.Item1 ) )
			return false;


		// Write generated shader to file
		System.IO.File.WriteAllText( shaderPath, code.Item1 );

        // Write generated post processing class to file within the current projects code folder.
        if (_graph.MaterialDomain is MaterialDomain.PostProcess)
		{
			// If the post processing class code is blank, dont bother generating the class.
			if ( !string.IsNullOrWhiteSpace( code.Item2 ) )
			{
                WritePostProcessingShaderClass( code.Item2 );
            }
			
        }
       
        AddToRecentFiles( savePath );

		return true;
	}

    private void WritePostProcessingShaderClass( string classCode )
    {
		var path = System.IO.Directory.CreateDirectory($"{Utilities.Path.GetProjectCodePath()}/Components/PostProcessing");

        File.WriteAllText( Path.Combine( path.FullName , $"{_asset.Name}_PostProcessing.cs"), classCode );
    }

    private static string GetSavePath()
	{
		var fd = new FileDialog( null )
		{
			Title = $"Save Shader Graph Plus",
			DefaultSuffix = $".sgrph"
		};

		fd.SelectFile( $"untitled.sgrph" );
		fd.SetFindFile();
		fd.SetModeSave();
		fd.SetNameFilter( "Shader Graph Plus (*.sgrph)" );
		if ( !fd.Execute() )
			return null;

		return fd.SelectedFile;
	}

	public void CreateUI()
	{
		BuildMenuBar();



		DockManager.RegisterDockType( "Graph", "account_tree", null, false );
		DockManager.RegisterDockType( "Preview", "photo", null, false );
		DockManager.RegisterDockType( "Properties", "edit", null, false );
		DockManager.RegisterDockType( "Output", "notes", null, false );
		DockManager.RegisterDockType( "Console", "text_snippet", null, false );
		DockManager.RegisterDockType( "Undo History", "history", null, false );
		DockManager.RegisterDockType( "Palette", "palette", null, false );

		_graphCanvas = new Widget( this ) { WindowTitle = $"{(_asset != null ? _asset.Name : "untitled")}{(_dirty ? "*" : "")}" };
		_graphCanvas.Name = "Graph";
		_graphCanvas.SetWindowIcon( "account_tree" );
		_graphCanvas.Layout = Layout.Column();

		var graphToolBar = new ToolBar( _graphCanvas, "GraphToolBar" );
		graphToolBar.SetIconSize( 16 );
		graphToolBar.AddOption( null, "arrow_back" ).Enabled = false;
		graphToolBar.AddOption( null, "arrow_forward" ).Enabled = false;
		graphToolBar.AddSeparator();
		graphToolBar.AddOption( null, "common/home.png" ).Enabled = false;

		var stretcher = new Widget( graphToolBar );
		stretcher.Layout = Layout.Row();
		stretcher.Layout.AddStretchCell( 1 );
		graphToolBar.AddWidget( stretcher );

		graphToolBar.AddWidget( new GamePerformanceBar( () => (1000.0f / PerformanceStats.LastSecond.FrameAvg).ToString( "n0" ) + "fps" ) { ToolTip = "Frames Per Second Average" } );
		graphToolBar.AddWidget( new GamePerformanceBar( () => PerformanceStats.LastSecond.FrameAvg.ToString( "0.00" ) + "ms" ) { ToolTip = "Frame Time Average (milliseconds)" } );
		graphToolBar.AddWidget( new GamePerformanceBar( () => PerformanceStats.ApproximateProcessMemoryUsage.FormatBytes() ) { ToolTip = "Approximate Memory Usage" } );

		_graphCanvas.Layout.Add( graphToolBar );

		_graphView = new ShaderGraphPlusView( _graphCanvas, this );
		//_graphView.SetBackgroundImage( "toolimages:/grapheditor/grapheditorbackgroundpattern_shader.png" );
		_graphView.BilinearFiltering = false;

		var types = EditorTypeLibrary.GetTypes<ShaderNodePlus>()
			.Where( x => !x.IsAbstract ).OrderBy( x => x.Name );

		//if ( _graph.MaterialDomain is MaterialDomain.PostProcess )
		//{
		//	foreach ( var type in types )
		//	{
		//		if ( type.HasAttribute<PostProcessingCompatable>() )
		//		{
		//			_graphView.AddNodeType( type );
		//		}
		//	}
		//}
		//else
		//{
			foreach ( var type in types )
			{
					_graphView.AddNodeType( type );
			}

		//}
	

		_graphView.Graph = _graph;
		_graphView.OnChildValuesChanged += ( w ) => SetDirty();
		_graphCanvas.Layout.Add( _graphView, 1 );

		_output = new Output( this );
		_output.OnNodeSelected += ( node ) =>
		{
			var nodeUI = _graphView.SelectNode( node );

			_graphView.Scale = 1;
			_graphView.CenterOn( nodeUI.Center );
		};

		_preview = new PreviewPanel( this, _graph.Model )
		{
			OnModelChanged = ( model ) => _graph.Model = model?.Name
		};

		if ( _graph.MaterialDomain is MaterialDomain.PostProcess )
		{
			_preview.IsPostProcessShader = true;
		}
		else
		{
			_preview.IsPostProcessShader = false;
		}

		foreach ( var value in _textureAttributes )
		{
			_preview.SetAttribute( value.Key, value.Value );
		}

		foreach ( var value in _float4x4Attributes )
		{
			_preview.SetAttribute( value.Key, value.Value );
		}

		foreach ( var value in _float3x3Attributes )
		{
			_preview.SetAttribute( value.Key, value.Value );
		}

		foreach ( var value in _float2x2Attributes )
		{
			_preview.SetAttribute( value.Key, value.Value );
		}

		foreach ( var value in _float4Attributes )
		{
			_preview.SetAttribute( value.Key, value.Value );
		}

		foreach ( var value in _float3Attributes )
		{
			_preview.SetAttribute( value.Key, value.Value );
		}

		foreach ( var value in _float2Attributes )
		{
			_preview.SetAttribute( value.Key, value.Value );
		}

		foreach ( var value in _floatAttributes )
		{
			_preview.SetAttribute( value.Key, value.Value );
		}

		foreach ( var value in _boolAttributes )
		{
			_preview.SetAttribute( value.Key, value.Value );
		}

		_properties = new Properties( this );
		_properties.Target = _graph;
		_properties.PropertyUpdated += OnPropertyUpdated;

		_undoHistory = new UndoHistory( this, _undoStack );
		_undoHistory.OnUndo = Undo;
		_undoHistory.OnRedo = Redo;
		_undoHistory.OnHistorySelected = SetUndoLevel;

		_palette = new PaletteWidget( this );

		DockManager.AddDock( null, _preview, DockArea.Left, DockManager.DockProperty.HideOnClose );
		DockManager.AddDock( null, _graphCanvas, DockArea.Right, DockManager.DockProperty.HideCloseButton | DockManager.DockProperty.HideOnClose, 0.7f );
		DockManager.AddDock( _graphCanvas, _output, DockArea.Bottom, DockManager.DockProperty.HideOnClose, 0.25f );
		DockManager.AddDock( _preview, _properties, DockArea.Bottom, DockManager.DockProperty.HideOnClose, 0.5f );

		// Yuck, console is internal but i want it, what is the correct way?
		var console = EditorTypeLibrary.Create( "ConsoleWidget", typeof( Widget ), new[] { this } ) as Widget;
		DockManager.AddDock( _output, console, DockArea.Inside, DockManager.DockProperty.HideOnClose );

		DockManager.AddDock( _output, _undoHistory, DockArea.Inside, DockManager.DockProperty.HideOnClose );
		DockManager.AddDock( _output, _palette, DockArea.Inside, DockManager.DockProperty.HideOnClose );

		DockManager.RaiseDock( "Output" );
		DockManager.Update();

		_defaultDockState = DockManager.State;

		if ( StateCookie != "ShaderGraphPlus" )
		{
			StateCookie = "ShaderGraphPlus";
		}
		else
		{
			RestoreFromStateCookie();
		}
	}

	private void OnPropertyUpdated()
	{
        if (_properties.Target is BaseNodePlus node)
        {
            //Log.Info($"Property of {node.DisplayInfo.Name} Changed!!!");
            
            _graphView.UpdateNode(node);

        }

        SetDirty();
    }

	protected override void RestoreDefaultDockLayout()
	{
		DockManager.State = _defaultDockState;

		SaveToStateCookie();
	}

	protected override bool OnClose()
	{
		if ( !_dirty )
		{
			return true;
		}

		var confirm = new PopupWindow(
			"Save Current Graph", "The open graph has unsaved changes. Would you like to save now?", "Cancel",
			new Dictionary<string, Action>()
			{
				{ "No", () => { _dirty = false; Close(); } },
				{ "Yes", () => { if ( SaveInternal( false ) ) Close(); } }
			}
		);

		confirm.Show();

		return false;
	}

	[EditorEvent.Hotload]
	public void OnHotload()
	{
		DockManager.Clear();
		MenuBar.Clear();

		CreateUI();
        Compile();
	}

	void IAssetEditor.SelectMember( string memberName )
	{
		throw new NotImplementedException();
	}
}
