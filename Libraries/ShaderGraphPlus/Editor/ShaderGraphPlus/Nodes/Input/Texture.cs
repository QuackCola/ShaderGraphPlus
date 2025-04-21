﻿
using Sandbox;

namespace Editor.ShaderGraphPlus.Nodes;

public abstract class TextureSamplerBase : ShaderNodePlus, ITextureParameterNode, IErroringNode
{
	/// <summary>
	/// Texture to sample in preview
	/// </summary>
	[ImageAssetPath]
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

	private Asset _asset;
	private string _texture;
	private string _image;
	private string _resourceText;

	[JsonIgnore, Hide]
	protected Asset Asset => _asset;

	[JsonIgnore, Hide]
	protected string TexturePath => _texture;

	protected void CompileTexture()
	{
		if ( _asset == null )
			return;

		if ( string.IsNullOrWhiteSpace( _image ) )
			return;

		var resourceText = string.Format( ShaderTemplate.TextureDefinition,
			_image,
			UI.ColorSpace,
			UI.ImageFormat,
			UI.Processor );

		if ( _resourceText == resourceText )
			return;

		_resourceText = resourceText;

		var assetPath = $"shadergraphplus/{_image.Replace( ".", "_" )}_shadergraphplus.generated.vtex";
		var resourcePath = FileSystem.Root.GetFullPath( "/.source2/temp" );
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

	[InlineEditor(Label = false), Group("Sampler")]
	public Sampler DefaultSampler { get; set; } = new Sampler();
	
	/// <summary>
	/// Settings for how this texture shows up in material editor
	/// </summary>
	[InlineEditor(Label = false), Group("UI")]
	public TextureInput UI { get; set; } = new TextureInput
	{
		ImageFormat = TextureFormat.DXT5,
		SrgbRead = true,
		Default = Color.White,
	};

	[Hide]
	public override string Title => string.IsNullOrWhiteSpace( UI.Name ) ? null : $"{DisplayInfo.For( this ).Name} {UI.Name}";

	protected TextureSamplerBase() : base()
	{
		Image = "materials/dev/white_color.tga";
		ExpandSize = new Vector2(0, 8 + Inputs.Count() * 24);
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

	protected NodeResult Component( string component, GraphCompiler compiler )
	{
		var result = compiler.Result( new NodeInput { Identifier = Identifier, Output = nameof( Result ) } );
		return result.IsValid ? new( ResultType.Float, $"{result}.{component}", true ) : new( ResultType.Float, "0.0f", true );
	}

	public List<string> GetErrors()
	{
		var errors = new List<string>();

		if ( Graph is ShaderGraphPlus sgp && sgp.IsSubgraph )
		{
			if ( string.IsNullOrWhiteSpace( UI.Name ) )
			{
				errors.Add( $"Texture parameter \"{DisplayInfo.For( this ).Name}\" is missing a name" );
			}

			foreach ( var node in sgp.Nodes )
			{
				if ( node is ITextureParameterNode tpn && tpn != this && tpn.UI.Name == UI.Name )
				{
					errors.Add( $"Duplicate texture parameter name \"{UI.Name}\" on {DisplayInfo.For( this ).Name}" );
				}
			}
		}

		return errors;
	}
}


/// <summary>
/// Ment for use when you want to pass a Texture2D into a node which happens to have a hlsl function that makes use of .Sample().
/// </summary>
[Title( "Texture Object" ), Category( "Textures" ), Icon( "image" )]
public sealed class TextureObjectNode : ShaderNodePlus, ITextureParameterNode, IErroringNode
{
	/// <summary>
	/// Texture to sample in preview
	/// </summary>
	[ImageAssetPath]
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

	[Hide]
	private Asset _asset;
	[Hide]
	private string _texture;
	[Hide]
	private string _image;
	[Hide]
	private string _resourceText;

	[JsonIgnore, Hide]
	private Asset Asset => _asset;

	[JsonIgnore, Hide]
	private string TexturePath => _texture;

	private void CompileTexture()
	{
		if ( _asset == null )
			return;

		if ( string.IsNullOrWhiteSpace( _image ) )
			return;

		var resourceText = string.Format( ShaderTemplate.TextureDefinition,
			_image,
			UI.ColorSpace,
			UI.ImageFormat,
			UI.Processor );

		if ( _resourceText == resourceText )
			return;

		_resourceText = resourceText;

		var assetPath = $"shadergraphplus/{_image.Replace( ".", "_" )}_shadergraphplus.generated.vtex";
		var resourcePath = FileSystem.Root.GetFullPath( "/.source2/temp" );
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

	/// <summary>
	/// Settings for how this texture shows up in material editor
	/// </summary>
	[InlineEditor(Label = false), Group("UI")]
	public TextureInput UI { get; set; } = new TextureInput
	{
		ImageFormat = TextureFormat.DXT5,
		SrgbRead = true,
		Default = Color.White,
	};

	[Hide]
	public override string Title => string.IsNullOrWhiteSpace( UI.Name ) ? null : $"{DisplayInfo.For( this ).Name} {UI.Name}";

	public TextureObjectNode() : base()
	{
		Image = "materials/dev/white_color.tga";
		ExpandSize = new Vector2( 0, 128 );
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

	public List<string> GetErrors()
	{
		var errors = new List<string>();

		if ( Graph is ShaderGraphPlus sg && sg.IsSubgraph )
		{
			if ( string.IsNullOrWhiteSpace( UI.Name ) )
			{
				errors.Add( $"Texture Object parameter \"{DisplayInfo.For( this ).Name}\" is missing a name" );
			}

			foreach ( var node in sg.Nodes )
			{
				if ( node is ITextureParameterNode tpn && tpn != this && tpn.UI.Name == UI.Name )
				{
					errors.Add( $"Duplicate texture object parameter name \"{UI.Name}\" on {DisplayInfo.For( this ).Name}" );
				}
			}
		}

		return errors;
	}

	/// <summary>
	/// Texture object result.
	/// </summary>
	[Hide]
	[Output( typeof( TextureObject ) ), Title( "Tex Object" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = UI;
		input.Type = TextureType.Tex2D;

		CompileTexture();

		var texture = string.IsNullOrWhiteSpace( TexturePath ) ? null : Texture.Load( TexturePath );
		texture ??= Texture.White;
		var result = compiler.ResultTexture( null, input, texture );

		if ( compiler.Stage == GraphCompiler.ShaderStage.Vertex )
		{
			return new NodeResult( ResultType.TextureObject, result.Item1, constant: true, iscomponentless: true );
		}
		else
		{
			return new NodeResult( ResultType.TextureObject, result.Item1, constant: true, iscomponentless: true );
		}
	};
}


/// <summary>
/// Sample a 2D Texture
/// </summary>
[Title( "Texture 2D" ), Category( "Textures" ), Icon( "image" )]
public sealed class TextureSampler : TextureSamplerBase
{
	/// <summary>
	/// Coordinates to sample this texture (Defaults to vertex coordinates)
	/// </summary>
	[Title( "Coordinates" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coords { get; set; }


	/// <summary>
	/// How the texture is filtered and wrapped when sampled
	/// </summary>
	[Title( "Sampler" )]
	[Input( typeof( Sampler ) )]
	[Hide]
	public NodeInput Sampler { get; set; }

	/// <summary>
	/// RGBA color result
	/// </summary>
	[Hide]
	[Output( typeof( Color ) ), Title( "RGBA" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = UI;
		input.Type = TextureType.Tex2D;

		CompileTexture();

		var texture = string.IsNullOrWhiteSpace( TexturePath ) ? null : Texture.Load( TexturePath );
		texture ??= Texture.White;

		var result = compiler.ResultTexture( compiler.ResultSamplerOrDefault( Sampler, DefaultSampler ), input, texture );
		var coords = compiler.Result( Coords );

		if ( compiler.Stage == GraphCompiler.ShaderStage.Vertex )
		{
			return new NodeResult( ResultType.Color, $"{result.Item1}.SampleLevel(" +
				$" {result.Item2}," +
				$" {(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vTextureCoords.xy")}, 0 )" );
		}
		else
		{
			return new NodeResult( ResultType.Color, $"{result.Item1}.Sample( {result.Item2}," +
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
/// Sample a Cube Texture
/// </summary>
[Title( "Texture Cube" ), Category( "Textures" ), Icon( "view_in_ar" )]
public sealed class TextureCube : ShaderNodePlus
{
	/// <summary>
	/// Coordinates to sample this cubemap
	/// </summary>
	[Title( "Coordinates" )]
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	/// <summary>
	/// How the texture is filtered and wrapped when sampled
	/// </summary>
	[Title( "Sampler" )]
	[Input]
	[Hide]
	public NodeInput Sampler { get; set; }


	/// <summary>
	/// Texture to sample in preview
	/// </summary>
	[ResourceType( "vtex" )]
	public string Texture { get; set; }

	[InlineEditor]
	public Sampler DefaultSampler { get; set; } = new Sampler();

	/// <summary>
	/// Settings for how this texture shows up in material editor
	/// </summary>
	[InlineEditor(Label = false), Group("UI")]
	public TextureInput UI { get; set; } = new TextureInput
	{
		ImageFormat = TextureFormat.DXT5,
		SrgbRead = true,
		Default = Color.White,
	};

	public TextureCube() : base()
	{
		Texture = "materials/skybox/skybox_workshop.vtex";
		ExpandSize = new Vector2(0, 8 + Inputs.Count() * 24);
	}

	public override void OnPaint(Rect rect)
	{
		rect = rect.Align(130, TextFlag.LeftBottom).Shrink(3);
		
		Paint.SetBrush("/image/transparent-small.png");
		Paint.DrawRect(rect.Shrink(2), 2);
		
		Paint.SetBrush(Theme.ControlBackground.WithAlpha(0.7f));
		Paint.DrawRect(rect, 2);
		
		if (!string.IsNullOrEmpty(Texture))
		{
			var tex = Sandbox.Texture.Find(Texture);
			if (tex is null) return;
			var pixmap = Pixmap.FromTexture(tex);
			Paint.Draw(rect.Shrink(2), pixmap);
		}
	}

	/// <summary>
	/// RGBA color result
	/// </summary>
	[Hide]
	[Output( typeof( Color ) ), Title( "RGBA" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = UI;
		input.Type = TextureType.TexCube;

		var result = compiler.ResultTexture( compiler.ResultSamplerOrDefault( Sampler, DefaultSampler ), input, Sandbox.Texture.Load( Texture ) );
		var coords = compiler.Result( Coords );

		return new NodeResult( ResultType.Color, $"TexCubeS( {result.Item1}," +
			$" {result.Item2}," +
			$" {(coords.IsValid ? $"{coords.Cast( 3 )}" : ViewDirection.Result.Invoke( compiler ))} )" );
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
/// Sample a 2D texture from 3 directions, then blend based on a normal vector.
/// </summary>
[Title( "Texture Triplanar" ), Category( "Textures" ), Icon( "photo_library" )]
public sealed class TextureTriplanar : TextureSamplerBase
{
	/// <summary>
	/// Coordinates to sample this texture (Defaults to vertex position)
	/// </summary>
	[Title( "Coordinates" )]
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	/// <summary>
	/// Normal to use when blending between each sampled direction (Defaults to vertex normal)
	/// </summary>
	[Title( "Normal" )]
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput Normal { get; set; }

	/// <summary>
	/// How the texture is filtered and wrapped when sampled
	/// </summary>
	[Title( "Sampler" )]
	[Input]
	[Hide]
	public NodeInput Sampler { get; set; }

	/// <summary>
	/// How many times to file the coordinates.
	/// </summary>
	[Title("Tile")]
	[Input(typeof(float))]
	[Hide]
	public NodeInput Tile { get; set; }

	/// <summary>
	/// Blend factor between different samples.
	/// </summary>
	[Title("Blend Factor")]
	[Input(typeof(float))]
	[Hide]
	public NodeInput BlendFactor { get; set; }

	[Group("Default Values")]
	public float DefaultTile { get; set; } = 1.0f;

	[Group("Default Values")]
	public float DefaultBlendFactor { get; set; } = 4.0f;

	/// <summary>
	/// RGBA color result
	/// </summary>
	[Hide]
	[Output( typeof( Color ) ), Title( "RGBA" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = UI;
		input.Type = TextureType.Tex2D;

		CompileTexture();

		var texture = string.IsNullOrWhiteSpace(TexturePath) ? null : Texture.Load(TexturePath);
		texture ??= Texture.White;

		var (tex, sampler) = compiler.ResultTexture(compiler.ResultSamplerOrDefault(Sampler, DefaultSampler), input, texture);
		var coords = compiler.Result(Coords);
		var tile = compiler.ResultOrDefault(Tile, DefaultTile);
		var normal = compiler.Result(Normal);
		var blendfactor = compiler.ResultOrDefault(BlendFactor, DefaultBlendFactor);

		var result = compiler.ResultFunction( "TexTriplanar_Color",
		tex,
		sampler,
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
[Title( "Normal Map Triplanar" ), Category( "Textures" ), Icon( "texture" )]
public sealed class NormapMapTriplanar : TextureSamplerBase
{

	/// <summary>
	/// Coordinates to sample this texture (Defaults to vertex position)
	/// </summary>
	[Title( "Coordinates" )]
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	/// <summary>
	/// Normal to use when blending between each sampled direction (Defaults to vertex normal)
	/// </summary>
	[Title( "Normal" )]
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput Normal { get; set; }

	/// <summary>
	/// How the texture is filtered and wrapped when sampled
	/// </summary>
	[Title( "Sampler" )]
	[Input]
	[Hide]
	public NodeInput Sampler { get; set; }

	public NormapMapTriplanar()
	{
		ExpandSize = new Vector2( 0f, 128f );
			
		UI = new TextureInput
		{
			ImageFormat = TextureFormat.DXT5,
			SrgbRead = false,
			ColorSpace = TextureColorSpace.Linear,
			Extension = TextureExtension.Normal,
			Processor = TextureProcessor.NormalizeNormals,
			Default = new Color( 0.5f, 0.5f, 1f, 1f )
		};
	}

	/// <summary>
	/// How many times to file the coordinates.
	/// </summary>
	[Title( "Tile" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Tile { get; set; }

	/// <summary>
	/// Blend factor between different samples.
	/// </summary>
	[Title( "Blend Factor" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput BlendFactor { get; set; }

	[Group( "Default Values" )]
	public float DefaultTile { get; set; } = 1.0f;

	[Group( "Default Values" )]
	public float DefaultBlendFactor { get; set; } = 4.0f;

	/// <summary>
	/// RGBA color result
	/// </summary>
	[Hide]
	[Output( typeof( Vector3 ) ), Title( "XYZ" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = UI;
		input.Type = TextureType.Tex2D;

		CompileTexture();

		var texture = string.IsNullOrWhiteSpace( TexturePath ) ? null : Texture.Load( TexturePath );
		texture ??= Texture.White;

		var (tex, sampler) = compiler.ResultTexture( compiler.ResultSamplerOrDefault( Sampler, DefaultSampler ), input, texture );
		var coords = compiler.Result( Coords );
		var tile = compiler.ResultOrDefault(Tile, DefaultTile);
		var normal = compiler.Result( Normal );
		var blendfactor = compiler.ResultOrDefault(BlendFactor, DefaultBlendFactor);

		var result = compiler.ResultFunction( "TexTriplanar_Normal",
			tex,
			sampler,
			coords.IsValid ? coords.Cast( 3 ) : "(i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz) / 39.3701",
			normal.IsValid ? normal.Cast( 3 ) : "normalize( i.vNormalWs.xyz )", 
			$"{blendfactor}"
		);

		return new NodeResult( ResultType.Vector3, result );
	};
}

/// <summary>
/// Texture Coordinate from vertex data
/// </summary>
[Title( "Texture Coordinate" ), Category( "Variables" ), Icon( "texture" )]
public sealed class TextureCoord : ShaderNodePlus
{
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
