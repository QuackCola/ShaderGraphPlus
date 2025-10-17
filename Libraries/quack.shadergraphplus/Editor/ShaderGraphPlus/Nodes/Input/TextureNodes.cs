using Editor;
using Editor.ShaderGraph;
using NodeEditorPlus;
using Sandbox;
using GraphView = NodeEditorPlus.GraphView;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;
using NodeUI = NodeEditorPlus.NodeUI;

namespace ShaderGraphPlus.Nodes;

public interface ISyncableTextureNode
{
	string SyncID { get; }
	string SourceParameterName { get; }
	TextureInput UI { get; set; }
	string Image { get; set; }

	// Update function in BaseNode class.
	void Update();

	void Sync( ISyncableTextureNode targetNode );
}


public abstract class Texture2DSamplerBase : ShaderNodePlus, IErroringNode, ITextureParameterNodeNew
{
	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.FunctionNode;

	[JsonIgnore, Hide, Browsable( false )]
	public bool IsSubgraph => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph);

	[JsonIgnore, Hide, Browsable( false )]
	public bool ShowUIProperty
	{
		get
		{
			if ( IsSubgraph )
				return false;

			if ( IsTextureInputConnected )
				return false;

			return true;
		}
	}

	[JsonIgnore, Hide, Browsable( false )]
	public string Image
	{
		get => _image;
		set
		{
			_image = value;
			_asset = AssetSystem.FindByPath( _image );

			if ( _asset == null )
				return;

			CompileTexture();
		}
	}

	[JsonIgnore, Hide] private Asset _asset;
	[JsonIgnore, Hide] private string _texture;
	[JsonIgnore, Hide] private string _image;
	[JsonIgnore, Hide] private string _resourceText;

	[JsonIgnore, Hide] private Asset Asset => _asset;
	[JsonIgnore, Hide] protected string TexturePath => _texture;
	[JsonIgnore, Hide] protected bool AlreadyRegisterd { get; set; } = false;
	[JsonIgnore, Hide] protected bool IsTextureInputConnected { get; set; } = false;

	[JsonIgnore, Hide] public string Name => UI.Name;

	/// <summary>
	/// Texture2D Object input
	/// </summary>
	[Title( "Texture" )]
	[Input( typeof( Texture2DObject ), Order = 0 )]
	[Hide]
	public NodeInput Texture2DInput { get; set; }

	[InlineEditor( Label = false ), Group( "UI" ), Order( 1 )]
	[ShowIf( nameof( ShowUIProperty ), true)]
	[InputDefault( nameof( Texture2DInput ) )]
	public TextureInput UI { get; set; } = new TextureInput
	{
		ImageFormat = TextureFormat.DXT5,
		SrgbRead = true,
		DefaultColor = Color.White,
	};

	protected void CompileTexture()
	{
		if ( _asset == null )
			return;

		if ( string.IsNullOrWhiteSpace( _image ) )
			return;

		var ui = UI;
		ui.DefaultTexture = _image;
		UI = ui;

		var resourceText = string.Format( ShaderTemplate.TextureDefinition,
			_image,
			UI.ColorSpace,
			UI.ImageFormat,
			UI.Processor );

		if ( _resourceText == resourceText )
			return;

		_resourceText = resourceText;

		var assetPath = $"shadergraphplus/{_image.Replace( ".", "_" )}_shadergraphplus.generated.vtex";
		var resourcePath = Editor.FileSystem.Root.GetFullPath( "/.source2/temp" );
		resourcePath = System.IO.Path.Combine( resourcePath, assetPath );

		if ( AssetSystem.CompileResource( resourcePath, resourceText ) )
		{
			_texture = assetPath;
		}
		else
		{
			Log.Warning( $"Failed to compile {_image}" );
		}
	}

	public override void OnPaint( Rect rect )
	{
		rect = rect.Align( 130, TextFlag.LeftBottom ).Shrink( 3 );

		Paint.SetBrush( "/image/transparent-small.png" );
		Paint.DrawRect( rect.Shrink( 2 ), 2 );

		Paint.SetBrush( Theme.ControlBackground.WithAlpha( 0.7f ) );
		Paint.DrawRect( rect, 2 );

		if ( Asset != null )
		{
			Paint.Draw( rect.Shrink( 2 ), Asset.GetAssetThumb( true ) );
		}
	}

	protected Texture2DSamplerBase() : base()
	{
		Image = "materials/default/default.tga";
		ExpandSize = new Vector2( 0, 8 + Inputs.Count() * 24 );
	}

	protected bool CheckIfRegisterd( GraphCompiler compiler, TextureInput input, out KeyValuePair<string, TextureInput> existingEntry )
	{
		existingEntry = new();

		if ( AlreadyRegisterd && compiler.IsPreview )
		{
			existingEntry = compiler.GetExistingTextureInputEntry( input.Name );
			return true;
		}

		return false;
	}

	protected NodeResult Component( string component, GraphCompiler compiler )
	{
		var result = compiler.Result( new NodeInput { Identifier = Identifier, Output = nameof( Result ) } );
		return result.IsValid ? new( ResultType.Float, $"{result}.{component}", true ) : new( ResultType.Float, "0.0f", true );
	}

	protected string ProcessTexture2DInputResult( GraphCompiler compiler, NodeResult texture2DInputResult )
	{
		if ( texture2DInputResult.IsValid && texture2DInputResult.IsMetaDataResult )
		{
			UI = texture2DInputResult.GetMetadata<TextureInput>( "TextureInput" );
			Image = UI.DefaultTexture;
			IsTextureInputConnected = true;
		}
		else
		{
			UI = UI with { DefaultTexture = "materials/default/default.tga", ShowNameProperty = true }; //new() { DefaultTexture = "materials/default/default.tga", ShowNameProperty = true };
			Image = UI.DefaultTexture;
			IsTextureInputConnected = false;
		}

		var input = UI;
		input.Type = TextureType.Tex2D;

		UI = input;

		CompileTexture();

		var texture = string.IsNullOrWhiteSpace( TexturePath ) ? null : Texture.Load( TexturePath );
		texture ??= Texture.White;

		var resultTextureGlobal = "";
		if ( texture2DInputResult.IsValid && texture2DInputResult.IsMetaDataResult )
		{
			resultTextureGlobal = texture2DInputResult.GetMetadata<string>( "TextureGlobal" );

			compiler.SetShaderAttribute( resultTextureGlobal.Remove( 0, 3 ), texture );
		}
		else
		{
			resultTextureGlobal = compiler.ResultTexture( input, texture, texture2DInputResult.IsValid );
		}

		return resultTextureGlobal;
	}

	public List<string> GetErrors()
	{
		var errors = new List<string>();
		var graph = Graph as ShaderGraphPlus;

		if ( !IsTextureInputConnected )
		{
			//foreach ( var parameter in graph.Parameters )
			//{
			//	if ( parameter.Name == Name )
			//	{
			//		errors.Add( $"Blackboard parameter with identifier\"{parameter.Identifier}\" has already registerd the name \"{Name}\"" );
			//		break;
			//	}
			//}

			if ( !string.IsNullOrWhiteSpace( Name ) )
			{
				foreach ( var node in graph.Nodes.OfType<Texture2DSamplerBase>() )
				{
					if ( node == this )
						continue;

					if ( node.IsTextureInputConnected )
						continue;

					if ( node.UI.Name == UI.Name )
					{
						errors.Add( $"Other TextureSampler2D node \"{node}\" has already registerd a texture the name \"{UI.Name}\"" );
						break;
					}
				}
			}
		}

		return errors;
	}
}

/// <summary>
/// Sample a 2D Texture
/// </summary>
[Title( "Sample Texture 2D" ), Category( "Textures" ), Icon( "colorize" )]
public sealed class SampleTexture2DNode : Texture2DSamplerBase
{
	[Hide]
	public override int Version => 1;

	/// <summary>
	/// Coordinates to sample this texture (Defaults to vertex coordinates)
	/// </summary>
	[Title( "Coordinates" )]
	[Input( typeof( Vector2 ), Order = 1 )]
	[Hide]
	public NodeInput CoordsInput { get; set; }

	/// <summary>
	/// How the texture is filtered and wrapped when sampled
	/// </summary>
	[Title( "Sampler" )]
	[Input( typeof( Sampler ), Order = 2 )]
	[Hide]
	public NodeInput SamplerInput { get; set; }

	[InlineEditor( Label = false ), Group( "Sampler" ), Order( 2 )]
	public Sampler SamplerState { get; set; } = new Sampler();

	public SampleTexture2DNode() : base()
	{
		ExpandSize = new Vector2( 0, 8 + Inputs.Count() * 24 );
	}

	/// <summary>
	/// RGBA color result
	/// </summary>
	[Hide]
	[Output( typeof( Color ) ), Title( "RGBA" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var texture2DInput = compiler.Result( Texture2DInput );
		var coords = compiler.Result( CoordsInput );
		var samplerGlobal = compiler.ResultSamplerOrDefault( SamplerInput, SamplerState );

		var resultTextureGlobal = ProcessTexture2DInputResult( compiler, texture2DInput );

		if ( compiler.Stage == GraphCompiler.ShaderStage.Vertex )
		{
			return new NodeResult( ResultType.Color, $"{resultTextureGlobal}.SampleLevel(" +
				$" {samplerGlobal}," +
				$" {(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vTextureCoords.xy")}, 0 )" );
		}
		else
		{
			return new NodeResult( ResultType.Color, $"{resultTextureGlobal}.Sample( {samplerGlobal}," +
				$"{(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vTextureCoords.xy")} )" );
		}
	};

	/// <summary>
	/// Red component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "R" )]
	public NodeResult.Func R => ( GraphCompiler compiler ) => Component( "r", compiler );

	/// <summary>
	/// Green component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "G" )]
	public NodeResult.Func G => ( GraphCompiler compiler ) => Component( "g", compiler );

	/// <summary>
	/// Blue component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "B" )]
	public NodeResult.Func B => ( GraphCompiler compiler ) => Component( "b", compiler );

	/// <summary>
	/// Alpha (Opacity) component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "A" )]
	public NodeResult.Func A => ( GraphCompiler compiler ) => Component( "a", compiler );
}

/// <summary>
/// Sample a 2D texture from 3 directions, then blend based on a normal vector.
/// </summary>
[Title( "Sample Texture 2D Triplanar" ), Category( "Textures" ), Icon( "colorize" )]
public sealed class SampleTexture2DTriplanarNode : Texture2DSamplerBase
{
	[Hide]
	public override int Version => 1;

	/// <summary>
	/// Coordinates to sample this texture (Defaults to vertex coordinates)
	/// </summary>
	[Title( "Coordinates" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput CoordsInput { get; set; }

	/// <summary>
	/// How the texture is filtered and wrapped when sampled
	/// </summary>
	[Title( "Sampler" )]
	[Input( typeof( Sampler ) )]
	[Hide]
	public NodeInput SamplerInput { get; set; }

	/// <summary>
	/// Normal to use when blending between each sampled direction (Defaults to vertex normal)
	/// </summary>
	[Title( "Normal" )]
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput NormalInput { get; set; }

	/// <summary>
	/// How many times to file the coordinates.
	/// </summary>
	[Title( "Tile" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput TileInput { get; set; }

	/// <summary>
	/// Blend factor between different samples.
	/// </summary>
	[Title( "Blend Factor" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput BlendFactorInput { get; set; }

	[InlineEditor( Label = false ), Group( "Sampler" ), Order( 2 )]
	public Sampler SamplerState { get; set; } = new Sampler();

	public float DefaultTile { get; set; } = 1.0f;
	public float DefaultBlendFactor { get; set; } = 4.0f;

	public SampleTexture2DTriplanarNode() : base()
	{
		Image = "materials/default/default.tga";
		ExpandSize = new Vector2( 0, 8 + Inputs.Count() * 24 );
	}

	/// <summary>
	/// RGBA color result
	/// </summary>
	[Hide]
	[Output( typeof( Color ) ), Title( "RGBA" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var texture2DInput = compiler.Result( Texture2DInput );
		var coords = compiler.Result( CoordsInput );
		var samplerGlobal = compiler.ResultSamplerOrDefault( SamplerInput, SamplerState );
		var tile = compiler.ResultOrDefault( TileInput, DefaultTile );
		var normal = compiler.Result( NormalInput );
		var blendfactor = compiler.ResultOrDefault( BlendFactorInput, DefaultBlendFactor );

		var resultTextureGlobal = ProcessTexture2DInputResult( compiler, texture2DInput );

		var result = compiler.ResultHLSLFunction( "TexTriplanar_Color",
		resultTextureGlobal,
		samplerGlobal,
		coords.IsValid ? coords.Cast( 3 ) : "(i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz) / 39.3701",
		normal.IsValid ? normal.Cast( 3 ) : "normalize( i.vNormalWs.xyz )",
		$"{blendfactor}"
		);

		return new NodeResult( ResultType.Color, result );
	};

	/// <summary>
	/// Red component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "R" )]
	public NodeResult.Func R => ( GraphCompiler compiler ) => Component( "r", compiler );

	/// <summary>
	/// Green component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "G" )]
	public NodeResult.Func G => ( GraphCompiler compiler ) => Component( "g", compiler );

	/// <summary>
	/// Blue component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "B" )]
	public NodeResult.Func B => ( GraphCompiler compiler ) => Component( "b", compiler );

	/// <summary>
	/// Alpha (Opacity) component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "A" )]
	public NodeResult.Func A => ( GraphCompiler compiler ) => Component( "a", compiler );
}

/// <summary>
/// Sample a 2D texture from 3 directions, then blend based on a normal vector.
/// </summary>
[Title( "Sample Texture 2D Normal Map Triplanar" ), Category( "Textures" ), Icon( "colorize" )]
public sealed class SampleTexture2DNormalMapTriplanarNode : Texture2DSamplerBase
{
	[Hide]
	public override int Version => 1;

	/// <summary>
	/// Coordinates to sample this texture (Defaults to vertex coordinates)
	/// </summary>
	[Title( "Coordinates" )]
	[Input( typeof( Vector2 ), Order = 1 )]
	[Hide]
	public NodeInput CoordsInput { get; set; }

	/// <summary>
	/// How the texture is filtered and wrapped when sampled
	/// </summary>
	[Title( "Sampler" )]
	[Input( typeof( Sampler ), Order = 2 )]
	[Hide]
	public NodeInput SamplerInput { get; set; }

	/// <summary>
	/// Normal to use when blending between each sampled direction (Defaults to vertex normal)
	/// </summary>
	[Title( "Normal" )]
	[Input( typeof( Vector3 ), Order = 3 )]
	[Hide]
	public NodeInput NormalInput { get; set; }

	/// <summary>
	/// How many times to file the coordinates.
	/// </summary>
	[Title( "Tile" )]
	[Input( typeof( float ), Order = 4 )]
	[Hide]
	public NodeInput TileInput { get; set; }

	/// <summary>
	/// Blend factor between different samples.
	/// </summary>
	[Title( "Blend Factor" )]
	[Input( typeof( float ), Order = 5 )]
	[Hide]
	public NodeInput BlendFactorInput { get; set; }

	[InlineEditor( Label = false ), Group( "Sampler" ), Order( 2 )]
	public Sampler SamplerState { get; set; } = new Sampler();

	public float DefaultTile { get; set; } = 1.0f;
	public float DefaultBlendFactor { get; set; } = 4.0f;

	public SampleTexture2DNormalMapTriplanarNode() : base()
	{
		Image = "materials/default/default.tga";
		ExpandSize = new Vector2( 0, 8 + Inputs.Count() * 24 );
		UI = new TextureInput
		{
			ImageFormat = TextureFormat.DXT5,
			SrgbRead = false,
			ColorSpace = TextureColorSpace.Linear,
			Extension = TextureExtension.Normal,
			Processor = TextureProcessor.NormalizeNormals,
			DefaultColor = new Color( 0.5f, 0.5f, 1f, 1f )
		};
	}

	/// <summary>
	/// RGBA color result
	/// </summary>
	[Hide]
	[Output( typeof( Color ) ), Title( "RGBA" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var texture2DInput = compiler.Result( Texture2DInput );
		var coords = compiler.Result( CoordsInput );
		var samplerGlobal = compiler.ResultSamplerOrDefault( SamplerInput, SamplerState );
		var tile = compiler.ResultOrDefault( TileInput, DefaultTile );
		var normal = compiler.Result( NormalInput );
		var blendfactor = compiler.ResultOrDefault( BlendFactorInput, DefaultBlendFactor );

		var resultTextureGlobal = ProcessTexture2DInputResult( compiler, texture2DInput );

		var result = compiler.ResultHLSLFunction( "TexTriplanar_Normal",
		resultTextureGlobal,
		samplerGlobal,
		coords.IsValid ? coords.Cast( 3 ) : "(i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz) / 39.3701",
		normal.IsValid ? normal.Cast( 3 ) : "normalize( i.vNormalWs.xyz )",
		$"{blendfactor}"
		);

		return new NodeResult( ResultType.Vector3, result );
	};

	/// <summary>
	/// Red component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "R" )]
	public NodeResult.Func R => ( GraphCompiler compiler ) => Component( "r", compiler );

	/// <summary>
	/// Green component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "G" )]
	public NodeResult.Func G => ( GraphCompiler compiler ) => Component( "g", compiler );

	/// <summary>
	/// Blue component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "B" )]
	public NodeResult.Func B => ( GraphCompiler compiler ) => Component( "b", compiler );

	/// <summary>
	/// Alpha (Opacity) component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "A" )]
	public NodeResult.Func A => ( GraphCompiler compiler ) => Component( "a", compiler );
}

/// <summary>
/// Sample a Cube Texture
/// </summary>
[Title( "Sample Texture Cube" ), Category( "Textures" ), Icon( "colorize" )]
public sealed class SampleTextureCubeNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.FunctionNode;

	[JsonIgnore, Hide, Browsable( false )]
	public bool IsSubgraph => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph);

	[JsonIgnore, Hide, Browsable( false )]
	private bool IsTextureInputConnected { get; set; } = false;

	[JsonIgnore, Hide, Browsable( false )]
	public bool ShowUIProperty
	{
		get
		{
			if ( IsSubgraph )
				return false;

			if ( IsTextureInputConnected )
				return false;

			return true;
		}
	}

	/// <summary>
	/// Optional TextureCube Object input when outside of subgraphs.
	/// </summary>
	[Title( "Texture Cube" )]
	[Input( typeof( TextureCubeObject ) )]
	[Hide]
	public NodeInput TextureInput { get; set; }
	/// <summary>
	/// Coordinates to sample this cubemap
	/// </summary>
	[Title( "Coordinates" )]
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput CoordsInput { get; set; }

	/// <summary>
	/// How the texture is filtered and wrapped when sampled
	/// </summary>
	[Title( "Sampler" )]
	[Input( typeof( Sampler ) )]
	[Hide]
	public NodeInput SamplerInput { get; set; }

	/// <summary>
	/// Texture to sample in preview
	/// </summary>
	[ResourceType( "vtex" )]
	[ShowIf( nameof( ShowUIProperty ), true )]
	public string Texture { get; set; }

	[InlineEditor( Label = false ), Group( "Sampler" )]
	[HideIf( nameof( IsSubgraph ), true )]
	public Sampler SamplerState { get; set; } = new Sampler();

	/// <summary>
	/// Settings for how this texture shows up in material editor
	/// </summary>
	[InlineEditor( Label = false ), Group( "UI" )]
	[ShowIf( nameof( ShowUIProperty ), true )]
	public TextureInput UI { get; set; } = new TextureInput
	{
		ImageFormat = TextureFormat.DXT5,
		SrgbRead = true,
		DefaultColor = Color.White,
		Type = TextureType.TexCube,
	};

	public SampleTextureCubeNode() : base()
	{
		Texture = "materials/skybox/skybox_workshop.vtex";
		ExpandSize = new Vector2( 0, 8 + Inputs.Count() * 24 );
		UI = UI with { Type = TextureType.TexCube };
	}

	public override void OnPaint( Rect rect )
	{
		rect = rect.Align( 130, TextFlag.LeftBottom ).Shrink( 3 );

		Paint.SetBrush( "/image/transparent-small.png" );
		Paint.DrawRect( rect.Shrink( 2 ), 2 );

		Paint.SetBrush( Theme.ControlBackground.WithAlpha( 0.7f ) );
		Paint.DrawRect( rect, 2 );

		if ( !string.IsNullOrEmpty( Texture ) )
		{
			var tex = Sandbox.Texture.Find( Texture );
			if ( tex is null ) return;
			var pixmap = Pixmap.FromTexture( tex );
			Paint.Draw( rect.Shrink( 2 ), pixmap );
		}
	}

	/// <summary>
	/// RGBA color result
	/// </summary>
	[Hide]
	[Output( typeof( Color ) ), Title( "RGBA" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = UI = UI with { Type = TextureType.TexCube };

		var textureCubeObject = compiler.Result( TextureInput );
		var sampler = compiler.ResultSamplerOrDefault( SamplerInput, SamplerState );
		var coords = compiler.Result( CoordsInput );

		if ( textureCubeObject.IsValid )
		{
			UI = textureCubeObject.GetMetadata<TextureInput>( "TextureInput" );
			Texture = UI.DefaultTexture;
			IsTextureInputConnected = true;
		}
		else
		{
			//Texture = "";
			IsTextureInputConnected = false;
		}

		// If TextureCubeObject input is not valid and we are not in a SubGraph then register the texture here instead.
		if ( !textureCubeObject.IsValid && !IsSubgraph )
		{
			var resultTextureGlobal = compiler.ResultTexture( input, Sandbox.Texture.Load( Texture ) );

			return new NodeResult( ResultType.Color, $"TexCubeS( {resultTextureGlobal}," +
				$" {sampler}," +
				$" {(coords.IsValid ? $"{coords.Cast( 3 )}" : ViewDirection.Result.Invoke( compiler ))} )" );
		}

		// Make sure to let the user know that the requied input is missing if we are in in a SubGraph.
		if ( !textureCubeObject.IsValid && IsSubgraph )
		{
			return NodeResult.MissingInput( $"Tex Object" );
		}

		// If TextureCubeObject input is valid then use the registerd Texture Object from the connected Texture Cube Object node.
		// Either if the textureObject input is valid or we are in a Subgraph.
		if ( textureCubeObject.IsValid || (IsSubgraph && textureCubeObject.IsValid) )
		{
			return new NodeResult( ResultType.Color, $"TexCubeS( {textureCubeObject.Code}," +
				$" {sampler}," +
				$" {(coords.IsValid ? $"{coords.Cast( 3 )}" : ViewDirection.Result.Invoke( compiler ))} )" );
		}

		return NodeResult.Error( "Failed to evaluate!" );
	};

	private NodeResult Component( string component, GraphCompiler compiler )
	{
		var result = compiler.Result( new NodeInput { Identifier = Identifier, Output = nameof( Result ) } );
		return result.IsValid ? new( ResultType.Float, $"{result}.{component}", true ) : new( ResultType.Float, "0.0f", true );
	}

	/// <summary>
	/// Red component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "R" )]
	public NodeResult.Func R => ( GraphCompiler compiler ) => Component( "r", compiler );

	/// <summary>
	/// Green component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "G" )]
	public NodeResult.Func G => ( GraphCompiler compiler ) => Component( "g", compiler );

	/// <summary>
	/// Blue component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "B" )]
	public NodeResult.Func B => ( GraphCompiler compiler ) => Component( "b", compiler );

	/// <summary>
	/// Alpha (Opacity) component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "A" )]
	public NodeResult.Func A => ( GraphCompiler compiler ) => Component( "a", compiler );
}

/// <summary>
/// Texture Coordinate from vertex data.
/// </summary>
[Title( "Texture Coordinate" ), Category( "Variables" ), Icon( "texture" )]
public sealed class TextureCoord : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.StageInputNode;

	/// <summary>
	/// Use the secondary vertex coordinate
	/// </summary>
	public bool UseSecondaryCoord { get; set; } = false;

	/// <summary>
	/// How many times this coordinate repeats itself to give a tiled effect
	/// </summary>
	public Vector2 Tiling { get; set; } = 1;

	[Hide]
	public override string Title => $"{DisplayInfo.For( this ).Name}{(UseSecondaryCoord ? " 2" : "")}";

	/// <summary>
	/// Coordinate result
	/// </summary>
	[Output( typeof( Vector2 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		if ( compiler.IsPreview )
		{
			var result = $"{compiler.ResultValue( UseSecondaryCoord )} ? i.vTextureCoords.zw : i.vTextureCoords.xy";
			return new( ResultType.Vector2, $"{compiler.ResultValue( Tiling.IsNearZeroLength )} ? {result} : ({result}) * {compiler.ResultValue( Tiling )}" );
		}
		else
		{
			var result = UseSecondaryCoord ? "i.vTextureCoords.zw" : "i.vTextureCoords.xy";
			return Tiling.IsNearZeroLength ? new( ResultType.Vector2, result ) : new( ResultType.Vector2, $"{result} * {compiler.ResultValue( Tiling )}" );
		}
	};
}

/// <summary>
/// How a texture is filtered and wrapped when sampled.
/// </summary>
[Title( "Sampler State" ), Category( "Textures" ), Icon( "colorize" )]
public sealed class SamplerNode : ShaderNodePlus, IParameterNode
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.ParameterNode;

	[JsonIgnore, Hide, Browsable( false )]
	public override bool CanPreview => false;

	public SamplerNode() : base()
	{
		ExpandSize = new Vector2( 0, 8 );
	}

	[InlineEditor( Label = false ), Group( "Sampler" )]
	[HideIf( nameof( IsSubgraph ), true )]
	public Sampler SamplerState { get; set; } = new Sampler();

	// TODO : Remove in future update.
	[Hide]
	public bool IsAttribute { get; set; } = false;

	[Hide]
	public override string Title
	{
		get
		{
			string name = $"{DisplayInfo.For( this ).Name}";

			if ( !IsSubgraph && !string.IsNullOrWhiteSpace( SamplerState.Name ) )
			{
				return $"{name} ( {SamplerState.Name} )";
			}
			else if ( !IsSubgraph )
			{
				return name;
			}
			else if ( IsSubgraph && !string.IsNullOrWhiteSpace( Name ) )
			{
				return $"{name} ( {Name} )";
			}
			else
			{
				return name;
			}
		}
	}

	[Hide]
	private bool IsSubgraph => ( Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph );

	[Hide]
	public string Name { get; set; }

	[Hide, JsonIgnore]
	public ParameterUI UI { get; set; }

	[Output( typeof( Sampler ) ), Hide]
	public NodeResult.Func Sampler => ( GraphCompiler compiler ) =>
	{
		var samplerResult = compiler.ResultSampler( SamplerState, Processed );

		return new NodeResult( ResultType.Sampler, samplerResult, true );
	};
}
