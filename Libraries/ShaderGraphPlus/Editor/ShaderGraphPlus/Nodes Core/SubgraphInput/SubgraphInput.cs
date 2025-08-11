
using Editor;

namespace ShaderGraphPlus;

public enum SubgraphInputType
{
	[Icon( "check_box" )]
	Bool,
	[Icon( "filter_1" )]
	Float,
	[Icon( "filter_2" )]
	Vector2,
	[Icon( "filter_3" )]
	Vector3,
	[Icon( "palette" )]
	Color,
	[Icon( "colorize" )]
	Sampler,
	[Title( "Texture2D Object" ), Icon( "texture" )]
	Texture2DObject,
	[Hide]
	Invalid
}

/// <summary>
/// Input of a Subgraph.
/// </summary>
[Title( "Subgraph Input" ), Icon( "input" ), SubgraphOnly]
public sealed class SubgraphInput : ShaderNodePlus, IErroringNode, IWarningNode
{
	[Hide]
	public override int Version => 1;

	[Input, Title( "Preview" ), Hide]
	public NodeInput PreviewInput { get; set; }

	/// <summary>
	/// Name of this input.
	/// </summary>
	public string InputName { get; set; } = "In0";

	/// <summary>
	/// Description of this input.
	/// </summary>
	[TextArea]
	public string InputDescription { get; set; } = "";

	[global::Editor( "DefaultValue" ), InlineEditor( Label = false )]
	public VariantValueBase InputData { get; set; } = new VariantValueVector3( Vector3.Zero, Vector3.Zero, Vector3.One, SubgraphInputType.Vector3 );

	/// <summary>
	/// Is this input required to have a valid connection?
	/// </summary>
	public bool IsRequired { get; set; } = false;

	public int PortOrder { get; set; }

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Hide]
	public override string Title
	{
		get
		{
			string name = $"{DisplayInfo.For( this ).Name}";

			return $"{InputName} ( {InputData.InputType} )";
		}
	}

	[Hide, JsonIgnore]
	public Type PortType
	{
		get
		{
			return InputData.InputType switch
			{
				SubgraphInputType.Bool => typeof( bool ),
				SubgraphInputType.Float => typeof( float ),
				SubgraphInputType.Vector2 => typeof( Vector2 ),
				SubgraphInputType.Vector3 => typeof( Vector3 ),
				SubgraphInputType.Color => typeof( Color ),
				SubgraphInputType.Sampler => typeof( Sampler ),
				SubgraphInputType.Texture2DObject => typeof( Texture2DObject ),
				_ => throw new NotImplementedException(),
			};
		}
	}

	[Hide]
	private bool IsSubgraph => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph);

	internal VariantParam<T> GetValueAsVariantParam<T>()
	{
		return InputData.GetAsVariantParam<T>( InputDescription );
	}

	internal T GetDefaultValue<T>()
	{
		return InputData.GetValue<T>();
	}

	internal T GetDefaultValueRangeMin<T>()
	{
		return InputData.GetValueRangeMin<T>();
	}

	internal T GetDefaultValueRangeMax<T>()
	{
		return InputData.GetValueRangeMax<T>();
	}

	internal void SetDefaultValue<T>( T value )
	{
		InputData.SetValue<T>( value );
	}
	internal void SetDefaultValueRangeMin<T>( T value )
	{
		InputData.SetValueRangeMin<T>( value );
	}

	internal void SetDefaultValueRangeMax<T>( T value )
	{
		InputData.SetValueRangeMax<T>( value );
	}

	public void OnNodeCreated()
	{

	}

	public List<string> GetWarnings()
	{
		var warnings = new List<string>();

		return warnings;
	}

	public List<string> GetErrors()
	{
		List<string> errors = new List<string>();

		if ( string.IsNullOrWhiteSpace( InputName ) )
		{
			errors.Add( $"SubgraphInput of InputType \"{InputData.InputType}\" must have a name!" );
		}

		if ( InputName.Contains( ' ' ) )
		{
			// TODO : Un-Comment if issues arise.
			//errors.Add( $"Parameter name \"{InputName}\" cannot contain spaces" );
		}

		if ( InputData.InputType == SubgraphInputType.Invalid )
		{
			errors.Add( $"SubgraphInput InputType is invalid!" );
		}

		return errors;
	}

	private string CompileTexture( string imagePath, TextureInput UI )
	{
		//if ( _asset == null )
		//	return;

		if ( string.IsNullOrWhiteSpace( imagePath ) )
			return "";

		var resourceText = string.Format( ShaderTemplate.TextureDefinition,
			imagePath,
			UI.ColorSpace,
			UI.ImageFormat,
			UI.Processor );

		//if ( _resourceText == resourceText )
		//	return;
		//
		//_resourceText = resourceText;

		var assetPath = $"shadergraphplus/{imagePath.Replace( ".", "_" )}_shadergraphplus.generated.vtex";
		var resourcePath = Editor.FileSystem.Root.GetFullPath( "/.source2/temp" );
		resourcePath = System.IO.Path.Combine( resourcePath, assetPath );

		string _texture = null;

		if ( AssetSystem.CompileResource( resourcePath, resourceText ) )
		{
			return _texture = assetPath;
		}
		else
		{

			Log.Warning( $"Failed to compile {imagePath}" );
			return "";
		}
	}

	private string ResultTexture( GraphCompiler compiler )
	{
		var texpath = CompileTexture( InputData.GetValue<TextureInput>().PreviewImage, InputData.GetValue<TextureInput>() );

		return compiler.ResultTexture( "PlaceHolderTexture2D", default, Texture.Load( texpath ) ).TextureGlobal;
	}

	[Output, Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		(ResultType resultType, string defaultCode) defaultResult = InputData.InputType switch
		{
			SubgraphInputType.Bool => ( ResultType.Bool, $"{compiler.ResultValue( InputData.GetValue<bool>() )}" ),
			SubgraphInputType.Float => ( ResultType.Float, $"{compiler.ResultValue( InputData.GetValue<float>() )}" ),
			SubgraphInputType.Vector2 => ( ResultType.Vector2, $"float2( {compiler.ResultValue( InputData.GetValue<Vector2>() )} )" ),
			SubgraphInputType.Vector3 => ( ResultType.Vector3, $"float3( {compiler.ResultValue( InputData.GetValue<Vector3>() )} )" ),
			SubgraphInputType.Color => ( ResultType.Color, $"float4( {compiler.ResultValue( InputData.GetValue<Color>() )} )" ),
			SubgraphInputType.Sampler =>  (ResultType.Sampler, $"{compiler.ResultSampler( InputData.GetValue<Sampler>() )}" ),
			SubgraphInputType.Texture2DObject => (ResultType.Texture2DObject, ResultTexture( compiler )),
			_ => throw new NotImplementedException()
		};

		return new NodeResult( defaultResult.resultType, defaultResult.defaultCode, constant: true );
	};
}
