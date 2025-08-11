using System.Collections.Generic;
using System.Text;
using static Sandbox.Gizmo;

namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Container for HLSL code.
/// </summary>
[Title( "Custom Function" ), Category( "Utility" ), Icon( "code" )]
public class CustomFunctionNode : ShaderNodePlus, IErroringNode, IInitializeNode
{
	[Hide]
	public override int Version => 1;

	public enum CustomCodeNodeMode
	{
		/// <summary>
		/// Inlines the code within body, into the generated shader.
		/// </summary>
		Inline,
		/// <summary>
		/// Retrive the function from an external hlsl include file.
		/// </summary>
		File,
	}

	[Hide]
	public override string Title => string.IsNullOrEmpty( Name ) ?
	$"{DisplayInfo.For( this ).Name}" :
	$"{DisplayInfo.For( this ).Name} ({Name})";

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	public string Name { get; set; }

	public CustomCodeNodeMode Type { get; set; } = CustomCodeNodeMode.Inline;

	[TextArea]
	[HideIf( nameof( Type ), CustomCodeNodeMode.File )]
	public string Body { get; set; }

	[HideIf( nameof( Type ), CustomCodeNodeMode.Inline )]
	[HLSLAssetPath]
	public string Source { get; set; }

	[Hide]
	public string CodeComment { get; set; } = "";

	[Title( "Inputs" )]
	public List<CustomCodeNodePorts> ExpressionInputs { get; set; }

	[Hide]
	private List<IPlugIn> InternalInputs = new();

	[Hide]
	public override IEnumerable<IPlugIn> Inputs => InternalInputs;

	[Title( "Outputs" )]
	public List<CustomCodeNodePorts> ExpressionOutputs { get; set; }

	[Hide]
	private List<IPlugOut> InternalOutputs = new();
	
	[Hide]
	public override IEnumerable<IPlugOut> Outputs => InternalOutputs;

	[Hide, JsonIgnore]
	int _lastHashCodeInputs = 0;

	[Hide, JsonIgnore]
	int _lastHashCodeOutputs = 0;

	public void InitializeNode()
	{
		OnNodeCreated();
	}

	private void OnNodeCreated()
	{
		CreateInputs();
		CreateOutputs();

		Update();
	}

	public NodeResult GetResult( GraphCompiler compiler, string metadataKey, object metadataValue )
	{
		foreach ( var input in ExpressionInputs )
		{
			if ( string.IsNullOrWhiteSpace( input.Name ) )
			{
				return NodeResult.Error( $"\"{input.TypeName}\" Input has no name" );
			}
		}

		Dictionary<string, object> metadata = new Dictionary<string, object>();

		if ( Type is CustomCodeNodeMode.File )
		{
			var functionInputs = GetInputResults( compiler, out string inputsError );

			if ( !string.IsNullOrWhiteSpace( inputsError ) )
			{
				return NodeResult.Error( inputsError );
			}

			compiler.RegisterInclude( Source );
			var outputResults = GetFunctionVoidLocals( out string outputsError );

			if ( !string.IsNullOrWhiteSpace( outputsError ) )
			{
				return NodeResult.Error( outputsError );
			}

			string funcCall = compiler.CustomCodeRegister( Name, functionInputs, Identifier, null, outputResults, false );
	
			metadata.Add( metadataKey, metadataValue );

			return new NodeResult( ResultType.Void, $"{funcCall}", true, metadata );//, ResultType.Invalid );// 0 );
		}
		else if ( Type is CustomCodeNodeMode.Inline )
		{
			StringBuilder inlineCodeSb = new StringBuilder();
			var inlineInputs = GetInputResultsInline( compiler, out string inputsError );
			
			if ( !string.IsNullOrWhiteSpace( inputsError ) )
			{
				return NodeResult.Error( inputsError );
			}

			var outputResults = GetFunctionVoidLocals( out string outputsError );

			if ( !string.IsNullOrWhiteSpace( outputsError ) )
			{
				return NodeResult.Error( outputsError );
			}

			inlineCodeSb.AppendLine( "{" );
			inlineCodeSb.AppendLine( Body );
			inlineCodeSb.AppendLine( "}" );

			// Relpace the user defined input names with the compiler assigned names.
			foreach ( var (userDefined, compilerDefined) in inlineInputs )
			{
				inlineCodeSb.Replace( userDefined, compilerDefined );
			}

			string funcCall = compiler.CustomCodeRegister( null, null, Identifier, inlineCodeSb, outputResults, true );

			metadata.Add( metadataKey, metadataValue );

			return new NodeResult( ResultType.Void, $"{funcCall}", true, metadata );//, ResultType.Invalid ); //0 );
		}

		return NodeResult.Error( $"Failed to evaluate!" );
	}

	/// <summary>
	/// Fetches the results from the user defined node inputs.
	/// </summary>
	private string GetInputResults( GraphCompiler compiler, out string error )
	{
		StringBuilder sb = new StringBuilder();
		int index = 0;
		error = "";

		foreach ( IPlugIn input in Inputs )
		{	
			NodeResult result = new NodeResult();
			
			if ( input.ConnectedOutput is null ) // TODO : Should the user be able to define a default or should it just be 0.0f?
			{
				if ( input.Type == typeof( Texture2DObject ) || input.Type == typeof( Sampler ) )
				{
					error = $"Required Input \"{input.DisplayInfo.Name}\" is missing";
				}
				//else if ( input.Type == typeof( Sampler ) )
				//{
				//
				//}
				result = new NodeResult( ResultType.Float, $"0", constant: true );
			}
			else
			{
				NodeInput nodeInput = new NodeInput { Identifier = input.ConnectedOutput.Node.Identifier, Output = input.ConnectedOutput.Identifier };
			
				result = compiler.Result( nodeInput );
			}
			
			if ( index < Inputs.Count() - 1 )
			{
				sb.Append($"{result}, ");
			}
			else
			{
				sb.Append($"{result}");
			}
			
			index++;
		}
		
		return sb.ToString();
	}

	private List<(string userDefined, string compilerDefined)> GetInputResultsInline( GraphCompiler compiler, out string error )
	{
		StringBuilder sb = new StringBuilder();
		int index = 0;
		error = "";
		List<(string, string)> inputResults = new List<(string,string)>();
		
		foreach ( IPlugIn input in Inputs )
		{
			NodeResult result = new NodeResult();
			
			if ( input.ConnectedOutput is null ) // TODO : Should the user be able to define a default or should it just be 0.0f?
			{
				if ( input.Type == typeof( Texture2DObject ) || input.Type == typeof( Sampler ) )
				{
					error = $"Required Input \"{input.DisplayInfo.Name}\" is missing";
				}

				result = new NodeResult( ResultType.Float, $"0", constant: true );
			}
			else
			{
				NodeInput nodeInput = new NodeInput { Identifier = input.ConnectedOutput.Node.Identifier, Output = input.ConnectedOutput.Identifier };
			
				result = compiler.Result( nodeInput );
			}
			
			//Log.Info($" Result : {result.Code}");
			
			inputResults.Add( ( input.DisplayInfo.Name, result.Code )  );
			
			index++;
		}
		
		return inputResults;
	}

	internal string ConstructFunctionInputs()
	{
		var sb = new StringBuilder();

		for ( int index = 0; index < ExpressionInputs.Count; index++ )
		{
			var input = ExpressionInputs[index];

			sb.Append(index == ExpressionInputs.Count - 1 ? $" {input.HLSLDataType} {input.Name}" : $" {input.HLSLDataType} {input.Name},");
		}

		return sb.ToString();
	}

	internal string ConstructFunctionOutputs()
	{
		var sb = new StringBuilder();
		
		for ( int index = 0; index < ExpressionOutputs.Count; index++ )
		{
			var input = ExpressionOutputs[index];
			
			sb.Append( index == ExpressionOutputs.Count - 1 ? $" out {input.HLSLDataType} {input.Name} " : $" out {input.HLSLDataType} {input.Name}," );
		}
		
		return sb.ToString();
	}

	private Dictionary<string, string> GetFunctionVoidLocals( out string error )
	{
		Dictionary<string, string> result = new();
		error = "";

		foreach ( CustomCodeNodePorts output in ExpressionOutputs )
		{
			if ( string.IsNullOrWhiteSpace( output.Name ) )
			{
				error = $"\"{output.TypeName}\" Output has no name";
				return result;
			}

			result.Add( output.Name, output.HLSLDataType );
		}

		return result;
	}

	public override void OnFrame()
	{
		var hashCodeInput = 0;
		var hashCodeOutput = 0;
	
		foreach ( var input in ExpressionInputs )
		{
			hashCodeInput += input.GetHashCode();
		}
		
		foreach ( var output in ExpressionOutputs )
		{
			hashCodeOutput += output.GetHashCode();
		}
		
		if ( hashCodeInput != _lastHashCodeInputs )
		{
			_lastHashCodeInputs = hashCodeInput;
			
			CreateInputs();
			Update();
		}

		if ( hashCodeOutput != _lastHashCodeOutputs )
		{
			_lastHashCodeOutputs = hashCodeOutput;

			CreateOutputs();
			Update();
		}
	}


	//[Hide, JsonIgnore]
	//internal Dictionary<IPlugIn, (IParameterNode, Type)> InputReferences = new();

	public void CreateInputs()
	{
		var plugs = new List<IPlugIn>();
	
		//var defaults = new Dictionary<Type, int>();
		//InputReferences.Clear();


		if ( ExpressionInputs == null )
		{
			InternalInputs = new();
		}
		else
		{
			foreach ( var input in ExpressionInputs.OrderBy( x => x.Priority ) )
			{
				if ( input.Type is null ) continue;
				var info = new PlugInfo()
				{
					Id = input.Id,
					Name = input.Name,
					Type = input.Type,
					DisplayInfo = new()
					{
						Name = input.Name,
						Fullname = input.Type.FullName
					}
				};
				var plug = new BasePlugIn( this, info, info.Type );
				var oldPlug = InternalInputs.FirstOrDefault( x => x is BasePlugIn plugIn && plugIn.Info.Id == info.Id ) as BasePlugIn;
				if ( oldPlug is not null )
				{
					oldPlug.Info.Name = info.Name;
					oldPlug.Info.Type = info.Type;
					oldPlug.Info.DisplayInfo = info.DisplayInfo;
					plugs.Add( oldPlug );
				}
				else
				{
					plugs.Add( plug );
				}
			};
			InternalInputs = plugs;
		}
	}

	[Hide, JsonIgnore]
	internal Dictionary<IPlugOut, IPlugIn> OutputReferences = new();

	public void CreateOutputs()
	{
		var plugs = new List<IPlugOut>();
		if ( ExpressionOutputs == null )
		{
			InternalOutputs = new();
		}
		else
		{
			foreach ( var output in ExpressionOutputs.OrderBy( x => x.Priority ) )
			{
				if ( output.Type is null ) continue;
				var info = new PlugInfo()
				{
					Id = output.Id,
					Name = output.Name,
					Type = output.Type,
					DisplayInfo = new()
					{
						Name = output.Name,
						Fullname = output.Type.FullName
					}
				};
				var plug = new BasePlugOut( this, info, info.Type );
				var oldPlug = InternalOutputs.FirstOrDefault( x => x is BasePlugOut plugOut && plugOut.Info.Id == info.Id ) as BasePlugOut;
				if (oldPlug is not null)
				{
					oldPlug.Info.Name = info.Name;
					oldPlug.Info.Type = info.Type;
					oldPlug.Info.DisplayInfo = info.DisplayInfo;
					plugs.Add( oldPlug );
				}
				else
				{
					plugs.Add( plug );
				}
			};
			InternalOutputs = plugs;
		}
	}

	public List<string> GetErrors()
	{
		OnNodeCreated();
		var errors = new List<string>();
		
		if ( !ExpressionOutputs.Any() )
		{
			errors.Add( $"`{DisplayInfo.Name}` has no outputs." );
		}

		foreach ( var output in ExpressionOutputs )
		{
			if ( string.IsNullOrWhiteSpace( output.Name ) )
			{
				return new List<string> { $"An output is not allowed to have an empty name!" };
			}
		}

		if ( string.IsNullOrWhiteSpace( Name ) )
		{
			if ( Type is CustomCodeNodeMode.File )
			{
				return new List<string> { $"`{DisplayInfo.Name}` Cannot call function with no name!" };
			}
			else
			{
				return new List<string> { $"`{DisplayInfo.Name}` Cannot generate a function with no name!" };
			}
		}
		
		if ( Type is CustomCodeNodeMode.File )
		{
			if ( string.IsNullOrWhiteSpace( Source ) )
			{
				errors.Add( $"`{DisplayInfo.Name}` Source path is empty!" );
			}
			
			if ( !Editor.FileSystem.Content.FileExists( $"shaders/{Source}" ) )
			{
				errors.Add( $"Include file `shaders/{Source}` does not exist." );
			}
		}
		
		return errors;
	}
}

public struct CustomCodeOutputData : IValid
{
	public string FriendlyName { get; set; }
	public string CompilerName { get; set; }
	public string DataType { get; set; }
	public int ComponentCount { get; set; }
	public ResultType ResultType { get; set; }
	public string NodeId { get; set; }

	public readonly bool IsValid => !string.IsNullOrWhiteSpace( FriendlyName );

	public CustomCodeOutputData()
	{
	}

	public CustomCodeOutputData( int components )
	{
		ComponentCount = components;
	}
}

public class CustomCodeNodePorts
{
	[Hide]
	public Guid Id { get; } = Guid.NewGuid();

	[KeyProperty]
	public string Name { get; set; }

	[Hide, JsonIgnore]
	public Type Type
	{
		get
		{
			if ( string.IsNullOrEmpty( TypeName ) ) return null;
			var typeName = TypeName;

			// Try getting type from EditorTypeLibrary.
			if ( GraphCompiler.ValueTypes.ContainsValue( new ( typeName, true ) ) )
			{
				if ( typeName == "Texture2D" ) typeName = typeof( Texture2DObject ).FullName;
				if ( typeName == "Sampler" ) typeName = typeof( Sampler ).FullName;
				if ( typeName == "float2x2" ) typeName = typeof( Float2x2 ).FullName;
				if ( typeName == "float3x3" ) typeName = typeof( Float3x3 ).FullName;
				if ( typeName == "float4x4" ) typeName = typeof( Float4x4 ).FullName;

				var editorType = EditorTypeLibrary.GetType( typeName ).TargetType;

				return editorType;
			}

			if ( typeName == "float" ) typeName = typeof( float ).FullName;
			if ( typeName == "int" ) typeName = typeof( int ).FullName;
			if ( typeName == "bool" ) typeName = typeof( bool ).FullName;

			var type = TypeLibrary.GetType( typeName ).TargetType;

			return type;
		}
	}
	
	[KeyProperty, Editor( "portType" ), JsonPropertyName( "Type" )]
	public string TypeName { get; set; }

	public int Priority { get; set; }

	[Hide, JsonIgnore]
	public string HLSLDataType
	{
		get
		{
			switch ( TypeName )
			{
				case "bool":
					return "bool";
				case "int":
					return $"float"; // Just identify as a float.
				case "float":
					return $"float";
				case "Vector2":
					return $"float2";
				case "Vector3":
					return $"float3";
				case "Vector4":
					return $"float4";
				case "Color":
					return $"float4";
				case "float2x2":
					return "float2x2";
				case "float3x3":
					return "float3x3";
				case "float4x4":
					return "float4x4";
				case "Texture2D":
					return "Texture2D";
				case "Sampler":
					return "Sampler";
			}

			throw new ArgumentException("Unsupported value type", nameof( TypeName ) );
		}
	}

	public override int GetHashCode()
	{
		return System.HashCode.Combine( Id, Name, TypeName, Priority );
	}
}
