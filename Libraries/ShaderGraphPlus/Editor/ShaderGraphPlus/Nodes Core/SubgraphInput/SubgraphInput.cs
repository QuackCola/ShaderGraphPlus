
using Editor;

namespace ShaderGraphPlus;

public enum SubgraphPortType
{
	[Icon( "check_box" )]
	Bool,
	[Icon( "filter_1" )]
	Int,
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

	[Hide, JsonIgnore]
	public override bool CanPreview => false;
	
	//[Hide, JsonIgnore]
	//private bool IsSubgraph => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph);

	[Hide, JsonIgnore]
	private string _textureGlobal;

	[Hide]
	public override string Title => string.IsNullOrWhiteSpace( InputName ) ?
	$"Subgraph Input" :
	$"{InputName} ({InputData.InputType})";

	/// <summary>
	/// Name of this input.
	/// </summary>
	public string InputName { get; set; } = "In0";

	/// <summary>
	/// Description of this input.
	/// </summary>
	[TextArea]
	public string InputDescription { get; set; } = "";

	[global::Editor( "SGP.VariantValue" ), InlineEditor( Label = false )]
	public VariantValueBase InputData { get; set; } = new VariantValueVector3( Vector3.Zero, SubgraphPortType.Vector3 );

	/// <summary>
	/// Is this input required to have a valid connection?
	/// </summary>
	public bool IsRequired { get; set; } = false;

	public int PortOrder { get; set; }

	[Input, Title( "Preview" ), Hide]
	public NodeInput PreviewInput { get; set; }

	[JsonIgnore, Hide]
	public override Color PrimaryColor => Color.Lerp( Theme.Green, Theme.Blue, 0.5f );

	public SubgraphInput()
	{
	}

	internal VariantParam<T> GetValueAsVariantParam<T>()
	{
		return InputData.GetAsVariantParam<T>( InputDescription );
	}

	internal T GetDefaultValue<T>()
	{
		return InputData.GetValue<T>();
	}

	internal void SetDefaultValue<T>( T value )
	{
		InputData.SetValue<T>( value );
	}

	/*
	public object GetValue()
	{
		return GetOutputValue();
	}

	private object GetOutputValue()
	{
		return InputData.InputType switch
		{
			SubgraphPortType.Bool => InputData.GetValue<bool>(),
			SubgraphPortType.Float => InputData.GetValue<float>(),
			SubgraphPortType.Vector2 => InputData.GetValue<Vector2>(),
			SubgraphPortType.Vector3 => InputData.GetValue<Vector3>(),
			SubgraphPortType.Color => InputData.GetValue<Color>(),
			SubgraphPortType.Texture2DObject => InputData.GetValue<TextureInput>(),
			SubgraphPortType.Sampler => InputData.GetValue<Sampler>(),
			_ => 1.0f,
		};
	}
	*/

	private string CompileTexture( TextureInput UI )
	{
		//if ( _asset == null )
		//	return;

		var imagePath = UI.PreviewImage;

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

		if ( AssetSystem.CompileResource( resourcePath, resourceText ) )
		{
			return assetPath;
		}
		else
		{
			SGPLog.Warning( $"Failed to compile \"{imagePath}\"" );
			return "";
		}
	}

	private string ResultTexture( GraphCompiler compiler )
	{
		var textureInput = InputData.GetValue<TextureInput>();
		var texturePath = CompileTexture( textureInput );
		bool cleanName = true;
		
		//// TODO : Stop it from registering duplicates.
		if ( string.IsNullOrWhiteSpace( textureInput.Name ) && string.IsNullOrWhiteSpace( _textureGlobal ) )
		{
			var result = compiler.ResultTexture( textureInput, Texture.Load( texturePath ), false );
			_textureGlobal = result;
			
			return result;
		}
		else if ( string.IsNullOrWhiteSpace( textureInput.Name ) && !string.IsNullOrWhiteSpace( _textureGlobal ) )
		{
			// Trim off g_t from _textureId and then make that the name of the preview Texture2D.
			textureInput = textureInput with { Name = _textureGlobal.TrimStart( ['g', '_', 't'] ) };
			cleanName = false;
		}

		return compiler.ResultTexture( textureInput, Texture.Load( texturePath ), cleanName );
	}

	[Output( typeof( float ) ), Title( "Value" ), Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		(ResultType resultType, string defaultCode) defaultResult = InputData.InputType switch
		{
			SubgraphPortType.Bool => ( ResultType.Bool, $"{compiler.ResultValue( InputData.GetValue<bool>() )}" ),
			SubgraphPortType.Float => ( ResultType.Float, $"{compiler.ResultValue( InputData.GetValue<float>() )}" ),
			SubgraphPortType.Vector2 => ( ResultType.Vector2, $"float2( {compiler.ResultValue( InputData.GetValue<Vector2>() )} )" ),
			SubgraphPortType.Vector3 => ( ResultType.Vector3, $"float3( {compiler.ResultValue( InputData.GetValue<Vector3>() )} )" ),
			SubgraphPortType.Color => ( ResultType.Color, $"float4( {compiler.ResultValue( InputData.GetValue<Color>() )} )" ),
			SubgraphPortType.Sampler =>  (ResultType.Sampler, $"{compiler.ResultSampler( InputData.GetValue<Sampler>() )}" ),
			SubgraphPortType.Texture2DObject => (ResultType.Texture2DObject, ResultTexture( compiler )),
			_ => throw new Exception( $"Unknown PortType \"{InputData.InputType}\"" )
		};

		return new NodeResult( defaultResult.resultType, defaultResult.defaultCode, constant: true );

		/*
		// In subgraphs, check if preview input is connected
		if ( compiler.Graph.IsSubgraph && PreviewInput.IsValid )
		{
			return compiler.Result( PreviewInput );
		}
		
		// Use the appropriate default value based on input type
		var outputValue = GetOutputValue();
		
		// If we're in a subgraph context, just return the value directly
		if ( compiler.Graph.IsSubgraph )
		{
			if ( InputData.InputType == SubgraphPortType.Sampler )
			{
				return new NodeResult( ResultType.Sampler, $"{compiler.ResultSampler( (Sampler)outputValue )}", constant: true );
			}
			if ( InputData.InputType == SubgraphPortType.Texture2DObject )
			{
				return new NodeResult( ResultType.Sampler, $"{compiler.ResultSampler( (Sampler)outputValue )}", constant: true );
			}
		
			return compiler.ResultValue( outputValue );
		}
		
		// For normal graphs, use ResultParameter to create a material parameter
		return compiler.ResultParameter( InputName, outputValue, default, default, false, IsRequired, new() );
		*/
	};

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

		// TODO : Un-Comment if issues arise.
		//if ( InputName.Contains( ' ' ) )
		//{
		//	errors.Add( $"Parameter name \"{InputName}\" cannot contain spaces" );
		//}

		if ( Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph )
		{
			foreach ( var node in Graph.Nodes )
			{
				if ( node == this ) continue;

				if ( node is SubgraphInput otherInput && otherInput.InputName == InputName )
				{
					errors.Add( $"Duplicate input name \"{InputName}\"" );
					break;
				}
			}
		}

		if ( InputData.InputType == SubgraphPortType.Invalid )
		{
			errors.Add( $"SubgraphInput InputType is invalid!" );
		}

		return errors;
	}
}
