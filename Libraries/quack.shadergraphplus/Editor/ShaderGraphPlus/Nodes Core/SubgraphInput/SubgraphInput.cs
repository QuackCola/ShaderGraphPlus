
using Editor;

using NodeEditorPlus;
using GraphView = NodeEditorPlus.GraphView;
using NodeUI = NodeEditorPlus.NodeUI;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;

namespace ShaderGraphPlus;

public enum SubgraphPortType
{
	[Icon( "check_box" )]
	Bool,
	[Icon( "looks_one" )]
	Int,
	[Icon( "looks_one" )]
	Float,
	[Title( "Float2" ), Icon( "looks_two" )]
	Vector2,
	[Title( "Float3" ), Icon( "looks_3" )]
	Vector3,
	[Title( "Float4" ), Icon( "looks_4" )]
	Vector4,
	[Title( "Color" ), Icon( "palette" )]
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
[InternalNode]
public sealed class SubgraphInput : ShaderNodePlus, IErroringNode, IWarningNode, IBlackboardSyncable
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleTintColor => PrimaryNodeHeaderColors.SubgraphNode;

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	//[Hide, JsonIgnore]
	//private bool IsSubgraph => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph);

	[Hide]
	public Guid BlackboardParameterIdentifier { get; set; }

	public void UpdateFromBlackboard( BaseBlackboardParameter blackboardParameter )
	{
		if ( blackboardParameter is BoolSubgraphInputParameter boolParameter )
		{
			SetDefaultValue( boolParameter.Value );
		}
		else if ( blackboardParameter is IntSubgraphInputParameter intParameter )
		{
			SetDefaultValue( intParameter.Value );
		}
		else if ( blackboardParameter is FloatSubgraphInputParameter floatParameter )
		{
			SetDefaultValue( floatParameter.Value );
		}
		else if ( blackboardParameter is Float2SubgraphInputParameter float2Parameter )
		{
			SetDefaultValue( float2Parameter.Value );
		}
		else if ( blackboardParameter is Float3SubgraphInputParameter float3Parameter )
		{
			SetDefaultValue( float3Parameter.Value );
		}
		else if ( blackboardParameter is Float4SubgraphInputParameter float4Parameter )
		{
			SetDefaultValue( float4Parameter.Value );
		}
		else if ( blackboardParameter is ColorSubgraphInputParameter colorParameter )
		{
			SetDefaultValue( colorParameter.Value );
		}
	}

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

	//[JsonIgnore, Hide]
	//public override Color PrimaryColor => Color.Lerp( Theme.Green, Theme.Blue, 0.5f );

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

	[Output, Title( "Value" ), Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		(ResultType resultType, string defaultCode) defaultResult = InputData.InputType switch
		{
			SubgraphPortType.Bool => ( ResultType.Bool, $"{compiler.ResultValue( InputData.GetValue<bool>() )}" ),
			SubgraphPortType.Int => ( ResultType.Int, $"{compiler.ResultValue( InputData.GetValue<int>() )}" ),
			SubgraphPortType.Float => ( ResultType.Float, $"{compiler.ResultValue( InputData.GetValue<float>() )}" ),
			SubgraphPortType.Vector2 => ( ResultType.Vector2, $"float2( {compiler.ResultValue( InputData.GetValue<Vector2>() )} )" ),
			SubgraphPortType.Vector3 => ( ResultType.Vector3, $"float3( {compiler.ResultValue( InputData.GetValue<Vector3>() )} )" ),
			SubgraphPortType.Vector4 => ( ResultType.Color, $"float4( {compiler.ResultValue( InputData.GetValue<Vector4>() )} )"),
			SubgraphPortType.Color => ( ResultType.Color, $"float4( {compiler.ResultValue( InputData.GetValue<Color>() )} )" ),
			SubgraphPortType.Sampler => ( ResultType.Sampler, $"{compiler.ResultSampler( InputData.GetValue<Sampler>() )}" ),
			SubgraphPortType.Texture2DObject => (ResultType.Texture2DObject, ResultTexture( compiler )),
			_ => throw new Exception( $"Unknown PortType \"{InputData.InputType}\"" )
		};

		return new NodeResult( defaultResult.resultType, defaultResult.defaultCode, constant: true );
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

		if ( InputData.InputType == SubgraphPortType.Invalid )
		{
			errors.Add( $"SubgraphInput InputType is invalid!" );
		}

		return errors;
	}
}
