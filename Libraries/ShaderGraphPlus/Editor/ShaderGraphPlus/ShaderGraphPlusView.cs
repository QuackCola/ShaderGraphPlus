using Editor;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

public class ShaderGraphPlusView : GraphView
{
	private readonly MainWindow _window;
	private readonly UndoStack _undoStack;

	protected override string ClipboardIdent => "shadergraphplus";

	protected override string ViewCookie => _window?.AssetPath;

	private static bool? _cachedConnectionStyle;

	public static bool EnableGridAlignedWires
	{
		get => _cachedConnectionStyle ??= EditorCookie.Get("shadergraphplus.gridwires", false);
		set => EditorCookie.Set("shadergraphplus.gridwires", _cachedConnectionStyle = value);
	}

	private ConnectionStyle _oldConnectionStyle;

	public new ShaderGraphPlus Graph
	{
		get => (ShaderGraphPlus)base.Graph;
		set => base.Graph = value;
	}

	private readonly Dictionary<string, INodeType> AvailableNodes = new( StringComparer.OrdinalIgnoreCase );

	public override ConnectionStyle ConnectionStyle => EnableGridAlignedWires
	? GridConnectionStyle.Instance
	: ConnectionStyle.Default;

	public ShaderGraphPlusView( Widget parent, MainWindow window ) : base( parent )
	{
		_window = window;
		_undoStack = window.UndoStack;

		OnSelectionChanged += SelectionChanged;
	}

	/*
	protected override Pixmap CreateBackgroundPixmap()
	{
		var cs = new ComputeShader( "core/ShaderGraphPlus/graphView_grid_cs.shader" );
		var texture = CreateTexture( "graphViewTex", 2048, 2048 );

		cs.Attributes.Set( "TextureSize", new Vector2( texture.Width, texture.Height ) );
		cs.Attributes.Set( "RWOutputTexture", texture );

		cs.Dispatch( texture.Width, texture.Height, 1 );

		var bitmap = texture.GetBitmap( 0 );

		return Pixmap.FromBitmap( bitmap );
	}


	public static Texture CreateTexture( string name, int width, int height, ImageFormat imageFormat = ImageFormat.RGBA8888 )
	{
		return Texture.Create( width, height )
		.WithName( name )
		.WithUAVBinding()
		.WithFormat( imageFormat )
		.Finish();
	}
	*/

	protected override INodeType RerouteNodeType { get; } = new ClassNodeType( EditorTypeLibrary.GetType<ReroutePlus>() );
	protected override INodeType CommentNodeType { get; } = new ClassNodeType( EditorTypeLibrary.GetType<CommentNode>() );

	public void AddNodeType<T>()
		where T : BaseNodePlus
	{
		AddNodeType( EditorTypeLibrary.GetType<T>() );
	}

	public void AddNodeType( TypeDescription type )
	{
		var nodeType = new ClassNodeType( type );

		AvailableNodes.TryAdd( nodeType.Identifier, nodeType );
	}

	public void AddNodeType( string subgraphPath )
	{
		var subgraphTxt = Editor.FileSystem.Content.ReadAllText( subgraphPath );
		var subgraph = new ShaderGraphPlus();
		subgraph.Deserialize( subgraphTxt );
		if ( !subgraph.AddToNodeLibrary ) return;
		var nodeType = new SubgraphNodeType( subgraphPath, EditorTypeLibrary.GetType<SubgraphNode>() );
		nodeType.SetDisplayInfo( subgraph );
		AvailableNodes.TryAdd( nodeType.Identifier, nodeType );
	}

	public INodeType FindNodeType( Type type )
	{
		return AvailableNodes.TryGetValue( type.FullName!, out var nodeType ) ? nodeType : null;
	}

	protected override void OnOpenContextMenu( Menu menu, Plug targetPlug )
	{
		base.OnOpenContextMenu( menu, targetPlug );

		var selectedNodes = SelectedItems.OfType<NodeUI>().ToArray();
		if ( selectedNodes.Length > 1 && !selectedNodes.Any( x => x.Node is BaseResult ) )
		{
			menu.AddOption( "Create Custom Node...", "add_box", () =>
			{
				const string extension = "sgpfunc";

				var fd = new FileDialog( null );
				fd.Title = "Create Shader Graph Function";
				fd.Directory = Project.Current.RootDirectory.FullName;
				fd.DefaultSuffix = $".{extension}";
				fd.SelectFile( $"untitled.{extension}" );
				fd.SetFindFile();
				fd.SetModeSave();
				fd.SetNameFilter( $"ShaderGraph Function (*.{extension})" );
				if ( !fd.Execute() ) return;

				CreateSubgraphFromSelection( fd.SelectedFile );
			} );
		}
		
		if ( selectedNodes.Length == 1 )
		{
			var item = selectedNodes.FirstOrDefault();

			if ( item is null )
				return;
			
			if ( item.Node is BaseNodePlus node && ConCommands.NodeDebugInfo )
			{
				menu.AddSeparator();

				node.DebugInfo( menu );
			}
		}
	}

	protected override INodeType NodeTypeFromDragEvent( DragEvent ev )
	{
		if ( ev.Data.Assets.FirstOrDefault() is { } asset )
		{
			if ( asset.IsInstalled )
			{
				if ( string.Equals( Path.GetExtension( asset.AssetPath ), ".sgpfunc", StringComparison.OrdinalIgnoreCase ) )
				{
					return new SubgraphNodeType( asset.AssetPath, EditorTypeLibrary.GetType<SubgraphNode>() );
				}
				else
				{
					var realAsset = asset.GetAssetAsync().Result;
					if ( realAsset.AssetType == AssetType.ImageFile )
					{
						return new TextureNodeType( EditorTypeLibrary.GetType<TextureSampler>(), asset.AssetPath );
					}
				}
			}
		}

		return AvailableNodes.TryGetValue( ev.Data.Text, out var type )
			? type
			: null;
	}

	protected override IEnumerable<INodeType> GetRelevantNodes( NodeQuery query )
	{
		return AvailableNodes.Values.Filter( query ).Where( x =>
		{
			if ( x is ClassNodeType classNodeType )
			{
				var targetType = classNodeType.Type.TargetType;
				if ( !Graph.IsSubgraph && targetType == typeof( FunctionResult ) ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( Result ) ) return false;
				if ( targetType == typeof( SubgraphNode ) && classNodeType.DisplayInfo.Name == targetType.Name.ToTitleCase() ) return false;
			}
			return true;
		} );
	}

	private static bool TryGetHandleConfig( Type type, out Type matchingType, out HandleConfig config )
	{
		if ( ShaderGraphPlusTheme.HandleConfigs.TryGetValue( type, out config ) )
		{
			matchingType = type;
			return true;
		}

		//SGPLog.Info( $"failed to get HandleConfig type\"{type}\"" );
		matchingType = null;
		return false;
	}

	protected override HandleConfig OnGetHandleConfig( Type type )
	{
		if ( TryGetHandleConfig( type, out var matchingType, out var config ) )
		{
			//SGPLog.Info( $"Got HandleConfig type\"{type}\" matchingType\"{matchingType}\" config.Name\"{config.Name}\"" );
			return config with { Name = type == matchingType ? config.Name : null };
		}

		return base.OnGetHandleConfig( type );
	}

	public override void ChildValuesChanged( Widget source )
	{
		BindSystem.Flush();

		base.ChildValuesChanged( source );

		BindSystem.Flush();
	}

	public override void PushUndo( string name )
	{
		Log.Info( $"Push Undo ({name})" );
		_undoStack.PushUndo( name, Graph.SerializeNodes() );
		_window.OnUndoPushed();
	}

	public override void PushRedo()
	{
		Log.Info( "Push Redo" );
		_undoStack.PushRedo( Graph.SerializeNodes() );
		_window.SetDirty();
	}

	private void CreateSubgraphFromSelection( string filePath )
	{
		if ( string.IsNullOrWhiteSpace( filePath ) ) return;

		var fileName = Path.GetFileNameWithoutExtension( filePath );
		var subgraph = new ShaderGraphPlus();
		subgraph.Title = fileName.ToTitleCase();
		subgraph.IsSubgraph = true;

		// Grab all selected nodes
		Vector2 rightmostPos = new Vector2( -9999, 0 );
		var selectedNodes = SelectedItems.OfType<NodeUI>();
		Dictionary<IPlugIn, IPlugOut> oldConnections = new();
		foreach ( var node in selectedNodes )
		{
			if ( node.Node is not BaseNodePlus baseNode ) continue;

			foreach ( var input in baseNode.Inputs )
			{
				oldConnections[input] = input.ConnectedOutput;
			}
			subgraph.AddNode( baseNode );

			rightmostPos.y += baseNode.Position.y;
			if ( baseNode.Position.x > rightmostPos.x )
			{
				rightmostPos = rightmostPos.WithX( baseNode.Position.x );
			}
		}
		rightmostPos.y /= selectedNodes.Count();

		// Create Inputs/Constants
		var nodesToAdd = new List<BaseNodePlus>();
		var previousOutputs = new Dictionary<string, IPlugOut>();
		foreach ( var node in subgraph.Nodes )
		{
			foreach ( var input in node.Inputs )
			{
				var correspondingOutput = oldConnections[input];
				var correspondingNode = subgraph.Nodes.FirstOrDefault( x => x.Identifier == correspondingOutput?.Node?.Identifier );
				if ( correspondingOutput is not null && correspondingNode is null )
				{
					var inputName = $"{input.Identifier}_{correspondingOutput?.Node?.Identifier}";
					var existingParameterNode = nodesToAdd.OfType<IParameterNode>().FirstOrDefault( x => x.Name == inputName );
					if ( input.ConnectedOutput is not null )
					{
						previousOutputs[inputName] = input.ConnectedOutput;
					}
					if ( existingParameterNode is not null )
					{
						input.ConnectedOutput = (existingParameterNode as BaseNodePlus).Outputs.FirstOrDefault();
						continue;
					}

					if ( input.Type == typeof( Texture2DObject ) )
					{
						var texture2DObjectNodeInput = FindNodeType( typeof( Texture2DObjectNode ) ).CreateNode( subgraph );
						texture2DObjectNodeInput.Position = node.Position - new Vector2( 240, 0 );
						if ( texture2DObjectNodeInput is Texture2DObjectNode texture2DObjectNode )
						{
							texture2DObjectNode.Name = inputName;
							input.ConnectedOutput = texture2DObjectNode.Outputs.FirstOrDefault();
							nodesToAdd.Add( texture2DObjectNode );
						}
					}
					else if ( input.Type == typeof( TextureCubeObject ) )
					{
						var textureCubeObjectNodeInput = FindNodeType( typeof( TextureCubeObjectNode ) ).CreateNode( subgraph );
						textureCubeObjectNodeInput.Position = node.Position - new Vector2( 240, 0 );
						if ( textureCubeObjectNodeInput is TextureCubeObjectNode textureCubeObjectNode )
						{
							textureCubeObjectNode.Name = inputName;
							input.ConnectedOutput = textureCubeObjectNode.Outputs.FirstOrDefault();
							nodesToAdd.Add( textureCubeObjectNode );
						}
					}
					else if ( input.Type == typeof( Sampler ) )
					{
						var samplerNodeInput = FindNodeType( typeof( SamplerNode ) ).CreateNode( subgraph );
						samplerNodeInput.Position = node.Position - new Vector2( 240, 0 );
						if ( samplerNodeInput is SamplerNode samplerNode )
						{
							samplerNode.Name = inputName;
							input.ConnectedOutput = samplerNode.Outputs.FirstOrDefault();
							nodesToAdd.Add( samplerNode );
						}
					}
					else if ( input.Type == typeof( Float2x2 ) )
					{
						var float2x2Input = FindNodeType( typeof( Float2x2Node ) ).CreateNode( subgraph );
						float2x2Input.Position = node.Position - new Vector2( 240, 0 );
						if ( float2x2Input is Float2x2Node float2x2Node )
						{
							float2x2Node.Name = inputName;
							input.ConnectedOutput = float2x2Node.Outputs.FirstOrDefault();
							nodesToAdd.Add( float2x2Node );
						}
					}
					else if ( input.Type == typeof( Float3x3 ) )
					{
						var float3x3Input = FindNodeType( typeof( Float3x3Node ) ).CreateNode( subgraph );
						float3x3Input.Position = node.Position - new Vector2( 240, 0 );
						if ( float3x3Input is Float3x3Node float3x3Node )
						{
							float3x3Node.Name = inputName;
							input.ConnectedOutput = float3x3Node.Outputs.FirstOrDefault();
							nodesToAdd.Add( float3x3Node );
						}
					}
					else if ( input.Type == typeof( Float4x4 ) )
					{
						var float4x4Input = FindNodeType( typeof( Float4x4Node ) ).CreateNode( subgraph );
						float4x4Input.Position = node.Position - new Vector2( 240, 0 );
						if ( float4x4Input is Float4x4Node float4x4Node )
						{
							float4x4Node.Name = inputName;
							input.ConnectedOutput = float4x4Node.Outputs.FirstOrDefault();
							nodesToAdd.Add( float4x4Node );
						}
					}
					else if ( input.Type == typeof( bool ) )
					{
						var boolInput = FindNodeType( typeof( Bool ) ).CreateNode( subgraph );
						boolInput.Position = node.Position - new Vector2( 240, 0 );
						if ( boolInput is Bool boolNode )
						{
							boolNode.Name = inputName;
							input.ConnectedOutput = boolNode.Outputs.FirstOrDefault();
							nodesToAdd.Add( boolNode );
						}
					}
					else if ( input.Type == typeof( float ) )
					{
						var floatInput = FindNodeType( typeof( Float ) ).CreateNode( subgraph );
						floatInput.Position = node.Position - new Vector2( 240, 0 );
						if ( floatInput is Float floatNode )
						{
							floatNode.Name = inputName;
							input.ConnectedOutput = floatNode.Outputs.FirstOrDefault();
							nodesToAdd.Add( floatNode );
						}
					}
					else if ( input.Type == typeof( Vector2 ) )
					{
						var vector2Input = FindNodeType( typeof( Float2 ) ).CreateNode( subgraph );
						vector2Input.Position = node.Position - new Vector2( 240, 0 );
						if ( vector2Input is Float2 vector2Node )
						{
							vector2Node.Name = inputName;
							input.ConnectedOutput = vector2Node.Outputs.FirstOrDefault();
							nodesToAdd.Add( vector2Node );
						}
					}
					else if ( input.Type == typeof( Vector3 ) )
					{
						var vector3Input = FindNodeType( typeof( Float3 ) ).CreateNode( subgraph );
						vector3Input.Position = node.Position - new Vector2( 240, 0 );
						if ( vector3Input is Float3 vector3Node )
						{
							vector3Node.Name = inputName;
							input.ConnectedOutput = vector3Node.Outputs.FirstOrDefault();
							nodesToAdd.Add( vector3Node );
						}
					}
					else if ( input.Type == typeof( Float4 ) )
					{
						var vector4Input = FindNodeType( typeof( Float4 ) ).CreateNode( subgraph );
						vector4Input.Position = node.Position - new Vector2( 240, 0 );
						if ( vector4Input is Float4 vector4Node )
						{
							vector4Node.Name = inputName;
							input.ConnectedOutput = vector4Node.Outputs.FirstOrDefault();
							nodesToAdd.Add( vector4Node );
						}
					}
				}
			}
		}

		var subgraphOutputs = new List<SubgraphOutput>();
		foreach ( var node in subgraph.Nodes )
		{
			foreach ( var output in node.Outputs )
			{
				
				var subgraphOutputNode = FindNodeType( typeof( SubgraphOutput ) ).CreateNode( subgraph );

				if ( subgraphOutputNode is SubgraphOutput subgraphOutput )
				{
					subgraphOutput.Position = rightmostPos + new Vector2( 240, 0 );
					subgraphOutput.SubgraphFunctionOutput = new();

					var correspondingNode = Graph.Nodes.FirstOrDefault( x => !subgraph.Nodes.Contains( x ) && x.Inputs.Any( x => x.ConnectedOutput == output ) );
					if ( correspondingNode is null ) continue;
					var inputName = $"{output.Identifier}_{output.Node.Identifier}";

					subgraphOutput.SubgraphFunctionOutput = new ShaderFunctionOutput()
					{
						OutputName = inputName,
					};
					subgraphOutput.SubgraphFunctionOutput.SetOutputTypeFromType( output.Type );

					subgraphOutput.InitializeNode();
		
					var input = subgraphOutput.Inputs.FirstOrDefault( x => x is BasePlugIn plugIn && plugIn.Info.Name == inputName );
					input.ConnectedOutput = output;

					subgraphOutputs.Add( subgraphOutput );
					break;
				}
			}
		}

		//nodesToAdd.Add( subgraphOutput );
		nodesToAdd.AddRange( subgraphOutputs );

		// Add all the newly created nodes
		foreach ( var node in nodesToAdd )
		{
			subgraph.AddNode( node );
		}

		/*
		// Create Output/Result node
		var frNode = FindNodeType( typeof( FunctionResult ) ).CreateNode( subgraph );
		
		if ( frNode is FunctionResult resultNode )
		{
			resultNode.Position = rightmostPos + new Vector2( 240, 0 );
			resultNode.FunctionOutputs = new();
			foreach ( var node in subgraph.Nodes )
			{
				foreach ( var output in node.Outputs )
				{
					var correspondingNode = Graph.Nodes.FirstOrDefault( x => !subgraph.Nodes.Contains( x ) && x.Inputs.Any( x => x.ConnectedOutput == output ) );
					if ( correspondingNode is null ) continue;
					var inputName = $"{output.Identifier}_{output.Node.Identifier}";
					resultNode.FunctionOutputs.Add( new FunctionOutput
					{
						Name = inputName,
						TypeName = output.Type.FullName
					} );
					resultNode.CreateInputs();

					var input = resultNode.Inputs.FirstOrDefault( x => x is BasePlugIn plugIn && plugIn.Info.Name == inputName );
					input.ConnectedOutput = output;
					break;
				}
			}
			nodesToAdd.Add( resultNode );
		}

		// Add all the newly created nodes
		foreach ( var node in nodesToAdd )
		{
			subgraph.AddNode( node );
		}
		*/

		// Save the newly created sub-graph
		System.IO.File.WriteAllText( filePath, subgraph.Serialize() );
		var asset = AssetSystem.RegisterFile( filePath );
		MainAssetBrowser.Instance?.UpdateAssetList();

		PushUndo( "Create Subgraph from Selection" );

		// Create the new subgraph node centered on the selected nodes
		Vector2 centerPos = Vector2.Zero;
		foreach ( var node in selectedNodes )
		{
			centerPos += node.Position;
		}
		centerPos /= selectedNodes.Count();
		var subgraphNode = CreateNewNode( new SubgraphNodeType( asset.RelativePath, EditorTypeLibrary.GetType<SubgraphNode>() ) ).Node as SubgraphNode;
		subgraphNode.Position = centerPos;

		// Get all the collected inputs/outputs and connect them to the new subgraph node
		foreach ( var node in Graph.Nodes )
		{
			if ( node == subgraphNode ) continue;

			if ( selectedNodes.Any( x => x.Node == node ) )
			{
				foreach ( var input in node.Inputs )
				{
					var correspondingOutput = oldConnections[input];
					if ( correspondingOutput is not null && !selectedNodes.Any( x => x.Node == correspondingOutput.Node ) )
					{
						var inputName = $"{input.Identifier}_{correspondingOutput.Node.Identifier}";
						var newInput = subgraphNode.Inputs.FirstOrDefault( x => x.Identifier == inputName );
						if ( previousOutputs.TryGetValue( inputName, out var previousOutput ) )
						{
							newInput.ConnectedOutput = previousOutput;
						}
					}
				}
			}
			else
			{
				foreach ( var input in node.Inputs )
				{
					var correspondingOutput = input.ConnectedOutput;
					if ( correspondingOutput is not null && selectedNodes.Any( x => x.Node == correspondingOutput.Node ) )
					{
						var inputName = $"{correspondingOutput.Identifier}_{correspondingOutput.Node.Identifier}";
						var newOutput = subgraphNode.Outputs.FirstOrDefault( x => x.Identifier == inputName );
						if ( newOutput is not null )
						{
							input.ConnectedOutput = newOutput;
						}
					}
				}
			}
		}

		PushRedo();
		DeleteSelection();

		// Delete all previously selected nodes
		UpdateConnections( Graph.Nodes );
		
	}

	private void SelectionChanged()
	{
		var item = SelectedItems
			.OfType<NodeUI>()
			.OrderByDescending( n => n is CommentUI )
			.FirstOrDefault();

		if ( !item.IsValid() )
		{
			_window.OnNodeSelected( null );
			return;
		}

		_window.OnNodeSelected( (BaseNodePlus)item.Node );
	}

	protected override void OnNodeCreated( INode node )
	{
		if ( node is SubgraphNode subgraphNode )
		{
			subgraphNode.OnNodeCreated();
		}
	}

	[EditorEvent.Frame]
	public void Frame()
	{
		foreach ( var node in Items )
		{
			if ( node is NodeUI nodeUI && nodeUI.Node is BaseNodePlus baseNode )
			{
				baseNode.OnFrame();
			}


		}

		if ( _oldConnectionStyle != ConnectionStyle )
		{
			_oldConnectionStyle = ConnectionStyle;

			foreach ( var connection in Items.OfType<Editor.NodeEditor.Connection>() )
			{
				connection.Layout();
			}
		}
	}
}
