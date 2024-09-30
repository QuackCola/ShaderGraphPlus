using System.IO;
using System.Text;
using System.Runtime.CompilerServices;
using Editor.NodeEditor;
using Sandbox;
using System.Text.RegularExpressions;
using System.Numerics;

namespace Editor.ShaderGraphPlus;

public sealed partial class GraphCompiler
{
	public struct Error
	{
		public BaseNodePlus Node;
		public string Message;
	}

	public struct Warning
	{
		public BaseNodePlus Node;
		public string Message;
	}

	/// <summary>
	/// Current graph we're compiling
	/// </summary>
	public ShaderGraphPlus Graph { get; private set; }

	public Asset _Asset { get; private set; }

	/// <summary>
	/// Is this compile for just the preview or not, preview uses attributes for constant values
	/// </summary>
	public bool IsPreview { get; private set; }
	public bool IsNotPreview => !IsPreview;

	private class CompileResult
	{
		public List<(NodeResult, NodeResult)> Results = new();
		public Dictionary<NodeInput, NodeResult> InputResults = new();
		public Dictionary<string, Sampler> SamplerStates = new();
		public Dictionary<string, TextureInput> TextureInputs = new();
		public Dictionary<string, (string Options, NodeResult Result)> Parameters = new();
		public Dictionary<string, object> Attributes { get; private set; } = new();
		public Dictionary<string, string> Functions = new();
		public Dictionary<string, Gradient> Gradients = new();
	}

	public enum ShaderStage
	{
		Vertex,
		Pixel,
	}

	public ShaderStage Stage { get; private set; }
	public bool IsVs => Stage == ShaderStage.Vertex;
	public bool IsPs => Stage == ShaderStage.Pixel;
	private string StageName => Stage == ShaderStage.Vertex ? "vs" : "ps";

	private readonly CompileResult VertexResult = new();
	private readonly CompileResult PixelResult = new();
	private CompileResult ShaderResult => Stage == ShaderStage.Vertex ? VertexResult : PixelResult;

	public Action<string, object> OnAttribute { get; set; }

	public List<BaseNodePlus> Nodes { get; private set; } = new();

	private readonly Dictionary<BaseNodePlus, List<string>> NodeErrors = new();
	private readonly Dictionary<BaseNodePlus, List<string>> NodeWarnings = new();


	/// <summary>
	/// Error list, doesn't give you much information currently
	/// </summary>
	public IEnumerable<Error> Errors => NodeErrors
		.Select(x => new Error { Node = x.Key, Message = x.Value.FirstOrDefault() });

	public IEnumerable<Warning> Warnings => NodeWarnings
	.Select(x => new Warning { Node = x.Key, Message = x.Value.FirstOrDefault() });

	public GraphCompiler(Asset asset, ShaderGraphPlus graph, bool preview)
	{
		Graph = graph;
		_Asset = asset;
		IsPreview = preview;
		Stage = ShaderStage.Pixel;
	}

	private static string CleanName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			return "";

		name = name.Trim().Replace(" ", string.Empty);
		name = new string(name.Where(x => char.IsLetter(x) || x == '_').ToArray());

		return name;
	}

    public string ResultFunctionOld((string, string) code, params string[] args)
	{
		var result = ShaderResult;

		if (!result.Functions.ContainsKey(code.Item2))
		{
			result.Functions.Add(code.Item2, code.Item1);
		}
		else
		{
			result.Functions[code.Item2] = code.Item1;
		}

		return $"{code.Item2}( {string.Join(", ", args)} )";
	}

    public string ResultFunction( string code, [CallerArgumentExpression("code")] string propertyName = "", params string[] args )
    {
        var result = ShaderResult;

		(string, string) func = (code, propertyName);

        if (!result.Functions.ContainsKey(func.Item2))
        {
            result.Functions.Add(func.Item2, func.Item1);
        }
        else
        {
            result.Functions[func.Item2] = func.Item1;
        }
        
        return $"{func.Item2}( {string.Join(", ", args)} )";
  
    }

    /// <summary>
    /// Returns a tuple with item1 being the code itself and item2 being the name of the function that was fetched from the input property name.
    /// </summary>
    public (string, string) GetFunction(string code, [CallerArgumentExpression("code")] string propertyName = "")
	{
		return new(code, propertyName);
	}

	/// <summary>
	/// Loops through ShaderResult.Gradients to find the matching key then returns the corresponding Gradient.
	/// </summary>
	public Gradient GetGradient( string gradient_name )
	{
		var result = ShaderResult;

		Gradient gradient = new();

		foreach (var g in result.Gradients)
		{
			if (g.Key == gradient_name)
			{
				gradient = g.Value;
			}
		}

		return gradient;
	}

	/// <summary>
	/// Register a gradient and return a generic name if gradient_name is empty.
	/// </summary>
	public string RegisterGradient(Gradient gradient, string gradient_name)
	{
		var result = ShaderResult;

		var name = CleanName(gradient_name);

		name = string.IsNullOrWhiteSpace(name) ? $"Gradient{result.Gradients.Count}" : name;

		var id = name;

		if (!result.Gradients.ContainsKey(id))
		{
			result.Gradients.Add(id, gradient);
		}

		return name;
	}

	/// <summary>
	/// Register a sampler and return the name of it
	/// </summary>
	public string ResultSampler(Sampler sampler)
	{
		var name = CleanName(sampler.Name);

		var result = ShaderResult;

		name = string.IsNullOrWhiteSpace(name) ? $"Sampler{result.SamplerStates.Count}" : name;

		var id = name;

		if (!result.SamplerStates.ContainsKey(id))
		{
			result.SamplerStates.Add(id, sampler); // Add the Name of the sampler and its associated options.

		}

		var result_sampler = $"g_s{id}";

		return result_sampler;
	}

	public string ResultSamplerOrDefault(NodeInput sampler, Sampler defaultsampler)
	{
		if (sampler.IsValid)
		{
			return Result(sampler).Code;
		}
		else
		{
			// Register the default sampler
			return ResultSampler(defaultsampler);
		}
	}

	/// <summary>
	/// Register a texture and return the name of it
	/// </summary>
	public (string, string) ResultTexture(string samplerinput, TextureInput input, Texture texture)
	{
		var name = CleanName(input.Name);
		name = string.IsNullOrWhiteSpace(name) ? $"Texture_{StageName}_{ShaderResult.TextureInputs.Count}" : name;

		var id = name;
		int count = 0;

		var result = ShaderResult;

		while (result.TextureInputs.ContainsKey(id))
		{
			id = $"{name}_{count++}";
		}

		OnAttribute?.Invoke(id, texture);

		result.TextureInputs.Add(id, input);

		return new($"g_t{id}", samplerinput);
	}

	/// <summary>
	/// Get result of an input with an optional default value if it failed to resolve
	/// </summary>
	public NodeResult ResultOrDefault<T>(NodeInput input, T defaultValue)
	{
		var result = Result(input);
		return result.IsValid ? result : ResultValue(defaultValue);
	}

	public void DepreciationWarning(BaseNodePlus node, string oldnode, string newnode)
	{
		var warnings = new List<string>();


		//NodeErrors.Add( node, warnings );
		NodeWarnings.Add(node, warnings);

		warnings.Add($"'{oldnode}' is depreciated please use '{newnode}' instead.");
	}

	/// <summary>
	/// Get result of an input
	/// </summary>
	public NodeResult Result(NodeInput input)
	{
		if (!input.IsValid)
			return default;

		if (ShaderResult.InputResults.TryGetValue(input, out var result))
			return result;

		var node = Graph.FindNode(input.Identifier);
		if (node == null)
			return default;

		var nodeType = node.GetType();
		var property = nodeType.GetProperty(input.Output);
		if (property == null)
			return default;

		var value = property.GetValue(node);
		if (value == null)
			return default;

		if (value is NodeResult nodeResult)
		{
			return nodeResult;
		}
		else if (value is NodeInput nodeInput)
		{
			return Result(nodeInput);
		}
		else if (value is NodeResult.Func resultFunc)
		{
			var funcResult = resultFunc.Invoke(this);

			// only enter this if IsComponentLess is is false
			if (!funcResult.IsValid && !funcResult.IsComponentLess)
			{
				if (!NodeErrors.TryGetValue(node, out var errors))
				{
					errors = new();
					NodeErrors.Add(node, errors);
				}

				if (funcResult.Errors is null || funcResult.Errors.Length == 0)
				{
					errors.Add($"Missing input");
				}
				else
				{
					foreach (var error in funcResult.Errors)
						errors.Add(error);
				}

				return default;
			}

			if (funcResult.ResultType is ResultType.Gradient)
			{
				return funcResult;
			}

			// We can return this result without making it a local variable because it's constant & or isnt somethign that is directly put into the graph.
			if (funcResult.Constant)
			{
				return funcResult;
			}

			var id = ShaderResult.InputResults.Count;
			var varName = $"l_{id}";
			var localResult = new NodeResult(funcResult.ResultType, varName);
			//var localResult = new NodeResult( funcResult.Components, varName );

			ShaderResult.InputResults.Add(input, localResult);
			ShaderResult.Results.Add((localResult, funcResult));

			if (IsPreview)
			{
				Nodes.Add(node);
			}

			return localResult;
		}

		return ResultValue(value);
	}

	/// <summary>
	/// Get result of two inputs and cast to the largest component of the two (a float2 and float3 will both become float3 results)
	/// </summary>
	public (NodeResult, NodeResult) Result(NodeInput a, NodeInput b, float defaultA = 0.0f, float defaultB = 1.0f)
	{
		var resultA = ResultOrDefault(a, defaultA);
		var resultB = ResultOrDefault(b, defaultB);

		if (resultA.Components() == resultB.Components())
			return (resultA, resultB);

		if (resultA.Components() < resultB.Components())
			return (new(resultB.ResultType, resultA.Cast(resultB.Components())), resultB);

		return (resultA, new(resultA.ResultType, resultB.Cast(resultA.Components())));
	}

	/// <summary>
	/// Get result of a value that can be set in material editor
	/// </summary>
	public NodeResult ResultParameter<T>(string name, T value, T min = default, T max = default, bool isRange = false, bool isAttribute = false, ParameterUI ui = default)
	{
		if (IsPreview || string.IsNullOrWhiteSpace(name))
			return ResultValue(value);

		name = CleanName(name);

		var attribName = name;

		//var prefix = "";

        var prefix = value switch
        {
            Color _ => "g_v",
            Vector4 _ => "g_v",
            Vector3 _ => "g_v",
            Vector2 _ => "g_v",
            float _ => "g_fl",
            bool _ => "g_b",
            Float2x2 _ => "g_m",
            Float3x3 _ => "g_m",
            Float4x4 _ => "g_m",
            _ => throw new ArgumentException("Unsupported value type", nameof( value ) )
        };

        //if (value is float)
		//{
		//	prefix = "g_fl";
		//}
		//else if (value is bool)
		//{
		//	prefix = "g_b";
		//}
		//else if (value is Float2x2 or Float3x3 or Float4x4)
		//{
		//	prefix = "g_m";
		//}
		//else if (value is Vector2 or Vector3 or Color)
		//{
		//	prefix = "g_v";
		//}
		//else
		//{
		//	throw new ArgumentException("Unsupported value type", nameof(value));
        //}

		if (!name.StartsWith(prefix))
			name = prefix + name;

		if (ShaderResult.Parameters.TryGetValue(name, out var parameter))
			return parameter.Result;

		parameter.Result = ResultValue(value, name);

		var options = new StringWriter();

		// If we're an attribute, we don't care about the UI options
		if (isAttribute)
		{

			options.Write($"Attribute( \"{attribName}\" ); ");

			if (value is bool boolValue)
			{
				options.Write($"Default( {(boolValue ? 1 : 0)} ); ");
			}
			else
			{
				options.Write($"Default{parameter.Result.Components()}( {value} ); ");
			}


		}
		else
		{
			if (value is Float2x2 or Float3x3 or Float4x4)
			{
				//parameter.Options = options.ToString().Trim();

				if (!ShaderResult.Parameters.ContainsKey(name))
				{
					ShaderResult.Parameters.Add(name, parameter);
				}

			}
			else
			{
				if (ui.Type != UIType.Default)
				{
					options.Write($"UiType( {ui.Type} ); ");
				}

				if (ui.Step > 0.0f)
				{
					options.Write($"UiStep( {ui.Step} ); ");
				}

				options.Write($"UiGroup( \"{ui.UIGroup}\" ); ");

				if (value is bool boolValue)
				{
					options.Write($"Default( {(boolValue ? 1 : 0)} ); ");
				}
				else
				{
					options.Write($"Default{parameter.Result.Components()}( {value} ); ");
				}

				if (parameter.Result.ResultType > 0 && isRange)
				{
					options.Write($"Range{parameter.Result.Components()}( {min}, {max} ); ");
				}
			}
		}

		parameter.Options = options.ToString().Trim();

		ShaderResult.Parameters.Add(name, parameter);

		return parameter.Result;
	}

    /// <summary>
    /// Get result of a value, in preview mode an attribute will be registered and returned
    /// Only supports float, Vector2, Vector3, Vector4, Color, Float2x2, Float3x3, Float4x4, Sampler & bool.
    /// </summary>
    public NodeResult ResultValue<T>(T value, string name = null)
	{
		bool isConstant = IsPreview;
		bool isNamed = isConstant || !string.IsNullOrWhiteSpace(name);
		name = isConstant ? $"g_{StageName}_{ShaderResult.Attributes.Count}" : name;

		if (IsPreview)
		{
			OnAttribute?.Invoke(name, value);
			ShaderResult.Attributes[name] = value;
		}

		return value switch
		{
			float v => isNamed ? new NodeResult(ResultType.Float, $"{name}") : new NodeResult(ResultType.Float, $"{v}", true),
			Vector2 v => isNamed ? new NodeResult(ResultType.Vector2, $"{name}") : new NodeResult(ResultType.Vector2, $"float2( {v.x}, {v.y} )"),
			Vector3 v => isNamed ? new NodeResult(ResultType.Vector3, $"{name}") : new NodeResult(ResultType.Vector3, $"float3( {v.x}, {v.y}, {v.z} )"),
			Vector4 v => isNamed ? new NodeResult(ResultType.Color, $"{name}") : new NodeResult(ResultType.Color, $"float4( {v.x}, {v.y}, {v.z}, {v.w} )"),
			Color v => isNamed ? new NodeResult(ResultType.Color, $"{name}") : new NodeResult(ResultType.Color, $"float4( {v.r}, {v.g}, {v.b}, {v.a} )"),
			Float2x2 v => isNamed ? new NodeResult(ResultType.Float2x2, $"{value}") : new NodeResult(ResultType.Float2x2, $"float2x2({v.M11}, {v.M12}, {v.M21}, {v.M22})"),
			Float3x3 v => isNamed ? new NodeResult(ResultType.Float3x3, $"{value}") : new NodeResult(ResultType.Float3x3, $"float3x3({v.M11}, {v.M12}, {v.M13}, {v.M21}, {v.M22}, {v.M23}, {v.M31}, {v.M32}, {v.M33})"),
			Float4x4 v => isNamed ? new NodeResult(ResultType.Float4x4, $"{value}") : new NodeResult(ResultType.Float4x4, $"float4x4({v.M11}, {v.M12}, {v.M13}, {v.M14}, {v.M21}, {v.M22}, {v.M23}, {v.M24}, {v.M31}, {v.M32}, {v.M33}, {v.M34}, {v.M41}, {v.M42}, {v.M43}, {v.M44})"),
			Sampler v => new NodeResult(ResultType.Sampler, $"{v}", true, true),
			bool v => isNamed ? new NodeResult(ResultType.Bool, $"{name}") : new NodeResult(ResultType.Bool, $"{v.ToString().ToLower()}"),
			_ => throw new ArgumentException("Unsupported attribute type", nameof(value))
		};
	}

	private static int GetComponentCount(Type inputType)
	{
		return inputType switch
		{
			Type t when t == typeof(Float4x4) => 16,
			Type t when t == typeof(Float3x3) => 6,
			Type t when t == typeof(Float2x2) => 4,
			Type t when t == typeof(Vector4) || t == typeof(Color) => 4,
			Type t when t == typeof(Vector3) => 3,
			Type t when t == typeof(Vector2) => 2,
			Type t when t == typeof(float) => 1,
			_ => 0
		};
	}

	private static IEnumerable<PropertyInfo> GetNodeInputProperties(Type type)
	{
		return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			.Where(property => property.GetSetMethod() != null &&
			property.PropertyType == typeof(NodeInput) &&
			property.IsDefined(typeof(BaseNodePlus.InputAttribute), false));
	}

	private string BuildFeatureOptions(List<string> options)
	{
		var options_body = "";
		int count = 0;

		foreach (var option in options)
		{
			if (count == 0) // first option starts at 0 :)
			{
				options_body += $"0=\"{option}\",";
				count++;
			}
			else if (count != (options.Count - 1))  // These options dont get the privilege of being the first >:)
			{
				options_body += $"{count}=\"{option}\",";
				count++;
			}
			else // Last option in the list oh well...:(
			{
				options_body += $"{count}=\"{option}\"";
			}
		}

		Log.Info($"Count is : {count} , Options Count is : {options.Count}");
		Log.Info($"Option String : {options_body}");
		return options_body;
	}

	private string GenerateFeatures()
	{
		var sb = new StringBuilder();
		var result = ShaderResult;

		return sb.ToString();
	}
	private string GenerateCommon()
	{
		var sb = new StringBuilder();

		var blendMode = Graph.BlendMode;
		var alphaTest = blendMode == BlendMode.Masked ? 1 : 0;
		var translucent = blendMode == BlendMode.Translucent ? 1 : 0;

		var result = ShaderResult;

		sb.AppendLine($"#ifndef S_ALPHA_TEST");
		sb.AppendLine($"#define S_ALPHA_TEST {alphaTest}");
		sb.AppendLine($"#endif");

		sb.AppendLine($"#ifndef S_TRANSLUCENT");
		sb.AppendLine($"#define S_TRANSLUCENT {translucent}");
		sb.AppendLine($"#endif");

		return sb.ToString();
	}

    public string GeneratePostProcessingComponent( PostProcessingComponentInfo postProcessiComponentInfo, string className, string shaderPath )
	{
		var ppcb = new PostProcessingComponentBuilder( postProcessiComponentInfo );
		var type = "";

		foreach (var parameter in ShaderResult.Parameters)
		{
			type = parameter.Value.Result.ComponentType.ToSimpleString();
			
            if ( type is "System.Boolean" )
			{
                ppcb.AddBoolProperty(parameter.Key, parameter.Value.Options);
			}
            if ( type is "float" )
            {
                ppcb.AddFloatProperty(type, parameter.Key, parameter.Value.Options);
            }
            if ( type is "Vector2" )
			{
                ppcb.AddVector2Property(type, parameter.Key, parameter.Value.Options);
			}
			if ( type is "Vector3" )
			{
                ppcb.AddVector3Property(type, parameter.Key, parameter.Value.Options);
			}
			if ( type is "Color" )
			{
                ppcb.AddVector4Property(type, parameter.Key, parameter.Value.Options);
			}
		}

        return ppcb.Finish( className, shaderPath );
	}

    /// <summary>
    /// Generate shader code, will evaluate the graph if it hasn't already.
    /// Different code is generated for preview and not preview.
    /// </summary>
    public (string,string) Generate()
	{
		// May have already evaluated and there's errors
		if (Errors.Any())
			return (string.Empty, string.Empty); //null;

		// If we have any errors after evaluating, no point going further
		if ( Errors.Any() )
			return (string.Empty, string.Empty);

		var shader_tempalte = "";
		var shaderCode = "";

        // Handle Lit & Unlit shaders as well as Post Processing and regular material shaders.
        if ( Graph.MaterialDomain is MaterialDomain.Surface )
		{
			if ( Graph.ShadingModel is ShadingModel.Lit )
			{
				shader_tempalte = ShaderTemplateLit.Code;
			}
			else
			{
				shader_tempalte = ShaderTemplateUnlit.Code;
			}
		}
		else
		{
			var postprocessing_material = GeneratePostprocessingMaterial();

            shaderCode = string.Format( ShaderTemplatePostProcessing.Code,
				Graph.Description,  // {0}
				IndentString( GenerateGlobals(), 1 ), // {1}
				IndentString( GenerateLocals(), 2 ), // {2}
				IndentString( postprocessing_material, 2 ), // {3}
				IndentString( GenerateFunctions( PixelResult ), 1 ) // {4}
			);

            var postProcessClass = GeneratePostProcessingComponent(
                	Graph.postProcessComponentInfo,
                	$"{CleanName(_Asset.Name)}_PostProcess",
                	$"{System.IO.Path.ChangeExtension(_Asset.Path, ".shader")}"
            );

			return (shaderCode, postProcessClass);
        }

		var material = GenerateMaterial();

         shaderCode = string.Format( shader_tempalte,
		Graph.Description,
		IndentString( GenerateFeatures(), 1 ),
		IndentString( GenerateCommon(), 1 ),
		IndentString( GenerateGlobals(), 1 ),
		IndentString( GenerateLocals(), 2 ),
		IndentString( material, 2 ),
		IndentString( GenerateVertex(), 2 ),
		IndentString( GenerateGlobals(), 1 ),
		IndentString( GenerateVertexComboRules(), 1 ),
		IndentString( GeneratePixelComboRules(), 1 ),
		IndentString( GenerateFunctions( PixelResult ), 1 ),
		IndentString( GenerateFunctions( VertexResult ), 1 ) );

		return ( shaderCode , string.Empty );
	}

	private static string GenerateFunctions( CompileResult result )
	{
		var sb = new StringBuilder();

		foreach ( var entry in result.Functions )
		{
			sb.Append( entry.Value );
		}

		return sb.ToString();
	}

	private static string IndentString( string input, int tabCount )
	{
		if ( string.IsNullOrWhiteSpace( input ) )
			return input;

		var tabs = new string( '\t', tabCount );
		var lines = input.Split( '\n' );

		for ( int i = 0; i < lines.Length; i++ )
		{
			lines[i] = tabs + lines[i];
		}

		return string.Join( "\n", lines );
	}

	private string GenerateVertexComboRules()
	{
		if ( IsNotPreview )
			return null;

		var sb = new StringBuilder();
		sb.AppendLine();
		sb.AppendLine( "DynamicComboRule( Allow0( D_MORPH ) );" );
		sb.AppendLine( "DynamicComboRule( Allow0( D_COMPRESSED_NORMALS_AND_TANGENTS ) );" );
		return sb.ToString();
	}

	private string GeneratePixelComboRules()
	{
		if ( IsNotPreview )
			return null;

		var sb = new StringBuilder();

		sb.AppendLine();
		sb.AppendLine( "DynamicComboRule( Allow0( D_BAKED_LIGHTING_FROM_LIGHTMAP ) );" );
		sb.AppendLine( "DynamicCombo( D_RENDER_BACKFACES, 0..1, Sys( ALL ) );" );
		sb.AppendLine( "RenderState( CullMode, D_RENDER_BACKFACES ? NONE : BACK );" );

		return sb.ToString();
	}

	private string GenerateGlobals()
	{
		var sb = new StringBuilder();


		if ( Graph.MaterialDomain != MaterialDomain.Surface )
		{
			sb.AppendLine( "RenderState( DepthWriteEnable, false );" );
			sb.AppendLine( "RenderState( DepthEnable, false );" );
			sb.AppendLine( "CreateTexture2D( g_tColorBuffer ) < Attribute( \"ColorBuffer\" ); SrgbRead( true ); Filter( MIN_MAG_LINEAR_MIP_POINT ); AddressU( MIRROR ); AddressV( MIRROR ); >;" );
			sb.AppendLine( "CreateTexture2D( g_tDepthBuffer ) < Attribute( \"DepthBuffer\" ); SrgbRead( false ); Filter( MIN_MAG_MIP_POINT ); AddressU( CLAMP ); AddressV( CLAMP ); >;" );
			//sb.AppendLine( "Texture2D g_tColorBuffer < Attribute( \"ColorBuffer\" ); SrgbRead( true ); >;" );
			//sb.AppendLine( "SamplerState ColorBufferSampler < Filter( MIN_MAG_LINEAR_MIP_POINT ); AddressU( MIRROR ); AddressV( MIRROR ); >;" );
			//sb.AppendLine( "Texture2D g_tDepthBuffer < Attribute( \"DepthBuffer\" ); SrgbRead( false ); >;" );
			//sb.AppendLine( "SamplerState DepthBufferSampler < Filter( MIN_MAG_MIP_POINT ); AddressU( CLAMP ); AddressV( CLAMP ); >;" );
			sb.AppendLine();
		}

		foreach ( var Sampler in ShaderResult.SamplerStates )
		{
			sb.Append( $"{Sampler.Value.CreateSampler( Sampler.Key )} <" )
			  .Append( $" Filter( {Sampler.Value.Filter.ToString().ToUpper()} );" )
			  .Append( $" AddressU( {Sampler.Value.AddressU.ToString().ToUpper()} );" )
			  .Append( $" AddressV( {Sampler.Value.AddressV.ToString().ToUpper()} ); >;" )
			  .AppendLine();
		}

		if ( IsPreview )
		{
			foreach ( var result in ShaderResult.TextureInputs )
			{
				sb.Append( $"{result.Value.CreateTexture( result.Key )} <" )
				  .Append( $" Attribute( \"{result.Key}\" );" )
				  .Append( $" SrgbRead( {result.Value.SrgbRead} ); >;" )
				  .AppendLine();
			}

			foreach ( var result in ShaderResult.Attributes )
			{
				var typeName = result.Value switch
				{
					Color _ => "float4",
					Vector4 _ => "float4",
					Vector3 _ => "float3",
					Vector2 _ => "float2",
					float _ => "float",
					bool _ => "bool",
					Float2x2 _ => "float2x2",
					Float3x3 _ => "float3x3",
					Float4x4 _ => "float4x4",
					_ => null
				};

				sb.AppendLine( $"{typeName} {result.Key} < Attribute( \"{result.Key}\" ); >;" );
			}

			sb.AppendLine( "float g_flPreviewTime < Attribute( \"g_flPreviewTime\" ); >;" );
			sb.AppendLine( $"int g_iStageId < Attribute( \"g_iStageId\" ); >;" );
		}
		else
		{
			foreach ( var result in ShaderResult.TextureInputs )
			{
				// If we're an attribute, we don't care about texture inputs
				if ( result.Value.IsAttribute )
					continue;

				sb.Append( $"{result.Value.CreateInput}( {result.Key}, {result.Value.ColorSpace}, 8," )
				  .Append( $" \"{result.Value.Processor.ToString()}\"," )
				  .Append( $" \"_{result.Value.ExtensionString.ToLower()}\"," )
				  .Append( $" \"{result.Value.UIGroup}\"," )
				  .Append( $" Default4( {result.Value.Default} ) );" )
				  .AppendLine();
			}

			foreach ( var result in ShaderResult.TextureInputs )
			{
				// If we're an attribute, we don't care about the UI options
				if ( result.Value.IsAttribute )
				{
					sb.AppendLine( $"{result.Value.CreateTexture( result.Key )} < Attribute( \"{result.Key}\" ); >;" );
				}
				else
				{
					sb.Append( $"{result.Value.CreateTexture( result.Key )} < Channel( RGBA, Box( {result.Key} ), {(result.Value.SrgbRead ? "Srgb" : "Linear")} );" )
					  .Append( $" OutputFormat( {result.Value.ImageFormat} );" )
					  .Append( $" SrgbRead( {result.Value.SrgbRead} ); >;" )
					  .AppendLine();
				}
			}

			foreach ( var parameter in ShaderResult.Parameters )
			{
				sb.AppendLine( $"{parameter.Value.Result.TypeName} {parameter.Key} < {parameter.Value.Options} >;" );
			}
		}

		if ( sb.Length > 0 )
		{
			sb.Insert( 0, "\n" );
		}

		return sb.ToString();
	}

	private string GenerateLocals()
	{
		var sb = new StringBuilder();

		if ( ShaderResult.Results.Any() )
		{
			sb.AppendLine();
		}

		if ( IsPreview )
		{
			Log.Info($"Registerd Gradient Count for Preview Is : {ShaderResult.Gradients.Count}");
		}
		else
		{
            Log.Info($"Registerd Gradient Count for Compile Is : {ShaderResult.Gradients.Count}");
        }

        foreach (var gradient in ShaderResult.Gradients)
        {
            //Log.Info($"Found Gradient : {gradient.Key}");
            //Log.Info($" Gradient Blend Mode : {gradient.Value.Blending}");
    

            sb.AppendLine($"Gradient {gradient.Key} = Gradient::Init();");
            sb.AppendLine();
  
            var colorindex = 0;
            var alphaindex = 0;

            sb.AppendLine($"{gradient.Key}.colorsLength = {gradient.Value.Colors.Count};");
            sb.AppendLine($"{gradient.Key}.alphasLength = {gradient.Value.Alphas.Count};");

            foreach (var color in gradient.Value.Colors)
        	{
                Log.Info($"{gradient.Key} Gradient Color {colorindex} : {color.Value} Time : {color.Time}");

				// All good with time as the 4th component?
                sb.AppendLine($"{gradient.Key}.colors[{colorindex++}] = float4({color.Value.r},{color.Value.g},{color.Value.b},{color.Time});");
            }
        
            foreach (var alpha in gradient.Value.Alphas)
            {
                sb.AppendLine($"{gradient.Key}.alphas[{alphaindex++}] = float({alpha.Value});");
            }

            sb.AppendLine();
        
        }

        if ( IsPreview )
		{
			int localId = 1;

			foreach ( var result in ShaderResult.Results )
			{
                if ( result.Item2.ResultType is ResultType.TextureObject )
				{
					sb.AppendLine( $"Texture2D {result.Item1} = {result.Item2.Code};" );
					sb.AppendLine( $"if ( g_iStageId == {localId++} ) return {result.Item2.Code}.Sample( g_sAniso, i.vTextureCoords.xy );" );
				}
				else if ( result.Item2.ResultType is ResultType.Float2x2 )
				{
					sb.AppendLine( $"float2x2 {result.Item1} = float2x2({result.Item2.Code});" );
					Log.Info( $"Generated Local : float2x2({result.Item2.Code});" );
				}
				else if ( result.Item2.ResultType is ResultType.Float3x3 )
				{
					sb.AppendLine( $"float3x3 {result.Item1} = float3x3({result.Item2.Code});" );
					Log.Info( $"Generated Local : float3x3({result.Item2.Code});" );
				}
				else if ( result.Item2.ResultType is ResultType.Float4x4 )
				{
					sb.AppendLine( $"float4x4 {result.Item1} = float4x4({result.Item2.Code});" );
					Log.Info($"Generated Local : float4x4({result.Item2.Code});" );
				}
				else
				{
					if ( Graph.MaterialDomain is MaterialDomain.PostProcess )
					{
						sb.AppendLine( $"{result.Item2.TypeName} {result.Item1} = {result.Item2.Code};" );
						sb.AppendLine( $"if ( g_iStageId == {localId++} ) return {result.Item1.Cast( 4, 1.0f )};" );
					}
					else
					{
						sb.AppendLine( $"{result.Item2.TypeName} {result.Item1} = {result.Item2.Code};" );
						sb.AppendLine( $"if ( g_iStageId == {localId++} ) return {result.Item1.Cast( 4, 1.0f )};" );
					}

				}

			}
		}
		else
		{
			foreach ( var result in ShaderResult.Results )
			{
				if ( result.Item2.ResultType is ResultType.TextureObject )
				{
					sb.AppendLine( $"Texture2D {result.Item1} = {result.Item2.Code};" );
				}
				else if ( result.Item2.ResultType is ResultType.Float2x2 )
				{
					sb.AppendLine( $"float2x2 {result.Item1} = float2x2({result.Item2.Code});" );
					Log.Info( $"Generated Local : float2x2({result.Item2.Code});" );
				}
				else if ( result.Item2.ResultType is ResultType.Float3x3 )
				{
					sb.AppendLine( $"float3x3 {result.Item1} = float3x3({result.Item2.Code});" );
					Log.Info( $"Generated Local : float3x3({result.Item2.Code});" );
				}
				else if ( result.Item2.ResultType is ResultType.Float4x4 )
				{
					sb.AppendLine( $"float4x4 {result.Item1} = float4x4({result.Item2.Code});" );
					Log.Info( $"Generated Local : float4x4({result.Item2.Code});" );
				}
				else
				{

					sb.AppendLine( $"{result.Item2.TypeName} {result.Item1} = {result.Item2.Code};" );
				}

			}
		}

		return sb.ToString();
	}

	private string GeneratePostprocessingMaterial()
	{
		Stage = ShaderStage.Pixel;

		var PostProcessingResultNode = Graph.Nodes.OfType<PostProcessingResult>().FirstOrDefault();
		
		if ( PostProcessingResultNode == null )
		{
			Log.Info( "Cant find PostProcessingResultNode" );
			return null;
		}

		var sb = new StringBuilder();

		foreach ( var property in GetNodeInputProperties( PostProcessingResultNode.GetType() ) )
		{
			NodeResult result;

			if ( property.GetValue( PostProcessingResultNode ) is NodeInput connection && connection.IsValid() )
			{
				result = Result( connection );
			}
			else
			{
				var editorAttribute = property.GetCustomAttribute<BaseNodePlus.EditorAttribute>();
				if ( editorAttribute == null )
				{
					// If there is no input to PostProcessingResultNode. Then default to un-modified SceneColor.
					sb.AppendLine( $"FinalColor = Tex2D( g_tColorBuffer, i.vPositionSs.xy / g_vRenderTargetSize );" );
					continue;
				}


				var valueProperty = PostProcessingResultNode.GetType().GetProperty( editorAttribute.ValueName );
				if ( valueProperty == null )
				{
					continue;
				}

				result = ResultValue( valueProperty.GetValue( PostProcessingResultNode ) );
			}

			if ( Errors.Any() )
				return null;

			if ( !result.IsValid() )
				continue;

			if ( string.IsNullOrWhiteSpace( result.Code ) )
				continue;

			var inputAttribute = property.GetCustomAttribute<BaseNodePlus.InputAttribute>();
			var componentCount = GetComponentCount( inputAttribute.Type );

			sb.AppendLine( $"FinalColor = {result.Cast( componentCount )};" );
		}

		return sb.ToString();
	}

	private string GenerateMaterial()
	{
		Stage = ShaderStage.Pixel;

		BaseNodePlus resultNode;

		//var resultNode = Graph.Nodes.OfType<Result>().FirstOrDefault();

		if ( Graph.MaterialDomain is MaterialDomain.Surface )
		{
			resultNode = Graph.Nodes.OfType<Result>().FirstOrDefault();
		}
		else
		{
			resultNode = Graph.Nodes.OfType<PostProcessingResult>().FirstOrDefault();
		}

		if ( resultNode == null )
			return null;

		var sb = new StringBuilder();

		foreach ( var property in GetNodeInputProperties( resultNode.GetType() ) )
		{
			if ( property.Name == "PositionOffset" )
				continue;

			NodeResult result;

			if ( property.GetValue( resultNode ) is NodeInput connection && connection.IsValid() )
			{
				result = Result( connection );
			}
			else
			{
				var editorAttribute = property.GetCustomAttribute<BaseNodePlus.EditorAttribute>();
				if ( editorAttribute == null )
				{
					if ( Graph.MaterialDomain is MaterialDomain.PostProcess )
					{
						// If there is no input to PostProcessingResultNode. Then default to un-modified SceneColor.
						sb.AppendLine( $"FinalColor = Tex2D( g_tColorBuffer,i.vPositionSs.xy / g_vRenderTargetSize);" );
					}
					continue;
				}

				var valueProperty = resultNode.GetType().GetProperty( editorAttribute.ValueName );
				if ( valueProperty == null )
					continue;

				result = ResultValue( valueProperty.GetValue( resultNode ) );
			}

			if ( Errors.Any() )
				return null;

			if ( !result.IsValid() )
				continue;

			if ( string.IsNullOrWhiteSpace( result.Code ) )
				continue;

			var inputAttribute = property.GetCustomAttribute<BaseNodePlus.InputAttribute>();
			var componentCount = GetComponentCount( inputAttribute.Type );

			if ( Graph.MaterialDomain is MaterialDomain.Surface )
			{
				sb.AppendLine( $"m.{property.Name} = {result.Cast( componentCount )};" );
			}
			else
			{
				sb.AppendLine( $"FinalColor = {result.Cast( componentCount )};" );
			}

		}

		return sb.ToString();
	}

	private string GenerateVertex()
	{
		Stage = ShaderStage.Vertex;

		var resultNode = Graph.Nodes.OfType<Result>().FirstOrDefault();
		if ( resultNode == null )
			return null;

		var property = GetNodeInputProperties( resultNode.GetType() )
			.FirstOrDefault( x => x.Name == "PositionOffset" );

		var sb = new StringBuilder();

		NodeResult result;

		if ( property.GetValue( resultNode ) is NodeInput connection && connection.IsValid() )
		{
			result = Result( connection );
		}
		else
		{
			return null;
		}

		if ( Errors.Any() )
			return null;

		if ( !result.IsValid() )
			return null;

		if ( string.IsNullOrWhiteSpace( result.Code ) )
			return null;

		var inputAttribute = property.GetCustomAttribute<BaseNodePlus.InputAttribute>();
		var componentCount = GetComponentCount( inputAttribute.Type );

		sb.AppendLine();

		foreach ( var local in ShaderResult.Results )
		{
			sb.AppendLine( $"{local.Item2.TypeName} {local.Item1} = {local.Item2.Code};" );
		}

		sb.AppendLine( $"i.vPositionWs.xyz += {result.Cast( componentCount )};" );
		sb.AppendLine( "i.vPositionPs.xyzw = Position3WsToPs( i.vPositionWs.xyz );" );

		return sb.ToString();
	}
}
