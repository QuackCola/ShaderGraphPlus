
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

[System.AttributeUsage( AttributeTargets.Class )]
internal class SubgraphOnlyAttribute : Attribute
{
	public SubgraphOnlyAttribute( )
	{
	}
}

/// <summary>
/// Input to a Subgraph.
/// </summary>
[Title( "Subgraph Input" ), Icon( "input" ), SubgraphOnly]
public sealed class SubgraphInput : ShaderNodePlus, IErroringNode, IWarningNode
{

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

	/// <summary>
	/// Is this input required to have a valid connection?
	/// </summary>
	public bool IsRequired { get; set; } = false;

	[Hide,JsonIgnore]
	private SubgraphInputType _inputType = SubgraphInputType.Float;

	[Hide]
	public SubgraphInputType InputType
	{
		get => _inputType;
		set
		{
			_inputType = value;
		}
	}

	[Hide]
	private bool IsSubgraph => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph);

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Hide]
	public override string Title
	{
		get
		{
			string name = $"{DisplayInfo.For( this ).Name}";

			return $"{InputName} ( {InputType} )";
		}
	}

	[Hide, JsonIgnore]
	public Type PortType
	{
		get
		{
			return InputType switch
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

	[global::Editor( "DefaultValue" ), InlineEditor( Label = false )]
	public VariantValueBase DefaultValue { get; set; }

	internal T GetDefaultValue<T>()
	{
		if ( DefaultValue is VariantValue<T> variantProperty )
		{
			return variantProperty.Value;
		}

		throw new Exception( $"GetDefaultValue : variantProperty is null!!!! it was actually {DefaultValue.GetType()} when {typeof( T )} was requested!" );
	}

	internal VariantParam<T> GetValueAsVariantParam<T>()
	{
		if ( DefaultValue is VariantValue<T> variantProperty )
		{
			VariantParam<T> variantParam = new VariantParam<T>();
			variantParam.Name = "Default Value Here";
			variantParam.Description = InputDescription;
			variantParam.Value = variantProperty.Value;
			variantParam.DefaultValue = variantProperty.Value;

			return variantParam;
		}

		throw new Exception( $"GetValueAsDefaultParam : variantProperty is null!!!!" );
	}

	internal void SetDefaultValue<T>( T value )
	{
		if ( DefaultValue is VariantValue<T> variantProperty )
		{
			variantProperty.Value = value;
		}
		else
		{
			throw new Exception( $"SetDefaultValue : variantProperty is null!!!!" );
		}
	}

	public void OnNodeCreated()
	{

	}

	public string CompileTexture( string imagePath, TextureInput UI )
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

	public string ResultTexture( GraphCompiler compiler )
	{
		var texpath = CompileTexture( ((VariantValueTexture2D)DefaultValue).Value.PreviewImage, ((VariantValueTexture2D)DefaultValue).Value );

		return compiler.ResultTexture( "PlaceHolderTexture2D", default, Texture.Load( texpath ) ).TextureGlobal;
	}

	public List<string> GetWarnings()
	{
		var warnings = new List<string>();


		warnings.Add( $"This is a fucking warning!" );

		return warnings;
	}

	public List<string> GetErrors()
	{
		List<string> errors = new List<string>();



		if ( string.IsNullOrWhiteSpace( InputName ) )
		{
			errors.Add( $"SubgraphInput of InputType \"{DefaultValue.InputType}\" must have a name!" );
		}

		if ( InputName.Contains( ' ' ) )
		{
			errors.Add( $"Parameter name \"{InputName}\" cannot contain spaces" );
		}

		if ( DefaultValue.InputType == SubgraphInputType.Invalid )
		{
			errors.Add( $"SubgraphInput InputType is invalid!" );
		}
		//errors.Add( $"This is a fucking error!" );
		return errors;
	}

	[Output, Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		(ResultType resultType, string defaultCode) defaultResult = InputType switch
		{
			SubgraphInputType.Bool => (ResultType.Bool, $"{compiler.ResultValue<bool>( ((VariantValueBool)DefaultValue).Value )}"),
			SubgraphInputType.Float => (ResultType.Float, $"{compiler.ResultValue<float>( ((VariantValueFloat)DefaultValue).Value )}"),
			SubgraphInputType.Vector2 => (ResultType.Vector2, $"float2( {compiler.ResultValue<Vector2>( ((VariantValueVector2)DefaultValue).Value )} )"),
			SubgraphInputType.Vector3 => (ResultType.Vector3, $"float3( {compiler.ResultValue<Vector3>( ((VariantValueVector3)DefaultValue).Value )} )"),
			SubgraphInputType.Color => (ResultType.Color, $"float4( {compiler.ResultValue<Color>( ((VariantValueColor)DefaultValue).Value)} )"),
			SubgraphInputType.Sampler => (ResultType.Sampler, $"{compiler.ResultSampler( ((VariantValueSampler)DefaultValue).Value )}"),
			SubgraphInputType.Texture2DObject => (ResultType.Texture2DObject, ResultTexture( compiler ) ),
			_ => throw new NotImplementedException()
		};


		return new NodeResult( defaultResult.resultType, defaultResult.defaultCode, constant: true );
	};
}
