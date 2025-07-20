using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

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

	public static Dictionary<Type, string> ValueTypes => new()
	{
		{ typeof( Color ), "float4" },
		{ typeof( Vector4 ), "float4" },
		{ typeof( Vector3 ), "float3" },
		{ typeof( Vector2 ), "float2" },
		{ typeof( float ), "float" },
		{ typeof( bool ), "bool" },
		//{ typeof( TextureObject ), "Texture2D" }
	};

	public bool Debug { get; private set; } = false;

	/// <summary>
	/// Current graph we're compiling
	/// </summary>
	public ShaderGraphPlus Graph { get; private set; }
	public ShaderGraphPlus LightingGraph { get; private set; }

	/// <summary>
	/// Are we on the lightingPage?
	/// </summary>
	public bool IsLightingPage { get; private set; }

	/// <summary>
	/// Current SubGraph
	/// </summary>
	private ShaderGraphPlus Subgraph = null;
	private SubgraphNode SubgraphNode = null;
	private List<(SubgraphNode, ShaderGraphPlus)> SubgraphStack = new();

	/// <summary>
	///  Contains the generated lighting function if any.
	/// </summary>
	internal string Shade { get; set; }

	/// <summary>
	/// The loaded sub-graphs
	/// </summary>
	public List<ShaderGraphPlus> Subgraphs { get; private set; }
	public HashSet<string> PixelIncludes { get; private set; } = new();
	public HashSet<string> VertexIncludes { get; private set; } = new();

	public Dictionary<string,string> SyncIDs = new();

	public Asset _Asset { get; private set; }

	/// <summary>
	/// Is this compile for just the preview or not, preview uses attributes for constant values
	/// </summary>
	public bool IsPreview { get; private set; }
	public bool IsNotPreview => !IsPreview;

	private partial class CompileResult
	{
		public List<(NodeResult, NodeResult)> Results = new();
		public Dictionary<NodeInput, NodeResult> InputResults = new();

		public Dictionary<string, Sampler> SamplerStates = new();
		public Dictionary<string, TextureInput> TextureInputs = new();
		public Dictionary<string, Gradient> Gradients = new();
		public Dictionary<string, (string Options, NodeResult Result)> Parameters = new();
		public Dictionary<string, object> Attributes { get; private set; } = new();
		public HashSet<string> Functions { get; private set; } = new();
		public Dictionary<string, string> Globals { get; private set; } = new();
		public string RepresentativeTexture { get; set; }

		/// <summary>
		/// A group of Void Locals that belongs to a Custom Function Node.
		/// </summary>
		public Dictionary<string, List<CustomCodeOutputData>> VoidLocalGroups { get; private set; } = new();

		public Dictionary<string, VoidData> VoidLocals { get; private set; } = new();
		public int VoidLocalCount { get; set; } = 0;
		
		public string LightingFunctionGlobals { get; set; }
	}

	public enum ShaderStage
	{
		Vertex,
		Pixel,
		Indirect, 
	}

	public ShaderStage Stage { get; private set; }
	public bool IsVs => Stage == ShaderStage.Vertex;
	public bool IsPs => Stage == ShaderStage.Pixel;
	public bool IsIndirect => Stage == ShaderStage.Indirect;

	private string StageName => Stage == ShaderStage.Vertex ? "vs" : "ps";

	private string CurrentResultInput;

	private readonly CompileResult VertexResult = new();
	private readonly CompileResult PixelResult = new();
	private readonly CompileResult IndirectResult = new();
	private CompileResult ShaderResult => GetCompileResult();//Stage == ShaderStage.Vertex ? VertexResult : PixelResult;

	private CompileResult GetCompileResult()
	{
		switch ( Stage )
		{
			case ShaderStage.Vertex:
				return VertexResult;
			case ShaderStage.Pixel:
				return PixelResult;
			case ShaderStage.Indirect:
				return IndirectResult;
			default:
				return null;
		}
	}

	public Action<string, object, bool> OnAttribute { get; set; }
	public int PreviewID { get; set; } = 0;

	//public List<BaseNodePlus> Nodes { get; private set; } = new();
	private List<NodeInput> InputStack = new();

	private readonly Dictionary<BaseNodePlus, List<string>> NodeErrors = new();
	private readonly Dictionary<BaseNodePlus, List<string>> NodeWarnings = new();

	/// <summary>
	/// Error list, doesn't give you much information currently
	/// </summary>
	public IEnumerable<Error> Errors => NodeErrors
		.Select( x => new Error { Node = x.Key, Message = x.Value.FirstOrDefault() } );

	public IEnumerable<Warning> Warnings => NodeWarnings
	.Select( x => new Warning { Node = x.Key, Message = x.Value.FirstOrDefault() } );

	public bool GeneratingLightingFunc { get; set; } = false;

	public GraphCompiler( Asset asset, ShaderGraphPlus graph, ShaderGraphPlus lightingPageGraph, Dictionary<string, ShaderFeatureInfo> shaderFeatures, bool preview )
	{
		Graph = graph;
		LightingGraph = lightingPageGraph;
		Graph.Features = shaderFeatures;
		ShaderFeatures = shaderFeatures;
		_Asset = asset;
		IsPreview = preview;
		Stage = ShaderStage.Pixel;
		Subgraphs = new();
	
		IsLightingPage = isLightingPage;
	
		AddSubgraphs( Graph );
	}

	internal KeyValuePair<string, TextureInput> GetExistingTextureInputEntry( string key )
	{
		return ShaderResult.TextureInputs.Where( x => x.Key == key ).FirstOrDefault();
	}

	public static string CleanName( string name )
	{
		if ( string.IsNullOrWhiteSpace( name ) )
			return "";

		name = name.Trim().Replace( " ", string.Empty );
		name = new string( name.Where( x => char.IsLetter( x ) || x == '_' ).ToArray() );

		return name;
	}

	private void AddSubgraphs( ShaderGraphPlus graph )
	{
		if ( graph != Graph )
		{
			if ( Subgraphs.Contains( graph ) )
				return;
			Subgraphs.Add( graph );
		}
		foreach ( var node in graph.Nodes )
		{
			if ( node is SubgraphNode subgraphNode )
			{
				AddSubgraphs( subgraphNode.Subgraph );
			}
		}
	}

	public void ResultComboPreview( string comboName, object value )
	{
		OnAttribute?.Invoke( comboName, value, true );
	}

	/// <summary>
	/// Register a void function. Atm only one Target result.
	/// </summary>
	public void RegisterVoid( ResultType resultType, string resultName, string funcCall )
	{
		if ( !ShaderResult.VoidLocals.ContainsKey( resultName ) )
		{
			var voidData = new VoidData()
			{
				TargetResult = resultName,
				ResultType = resultType,
				FunctionCall = funcCall,
				AlreadyDefined = false,
			};

			ShaderResult.VoidLocals.Add( voidData.TargetResult, voidData );
		}
	}

	public void RegisterInclude( string path )
	{
		var list = IsVs ? VertexIncludes : PixelIncludes;
		if ( list.Contains( path ) )
			return;
		list.Add( path );
	}

	/// <summary>
	/// Register some generic global parameter for a node to use.
	/// </summary>
	public void RegisterGlobal( string name, string global )
	{
		var result = ShaderResult;
		if ( result.Globals.ContainsKey( name ) )
			return;

		result.Globals.Add( name, global );
	}

	public string ResultFunction( string name, params string[] args )
	{
		if ( !GraphHLSLFunctions.HasFunction( name ) )
			return null;

		var result = ShaderResult;
		if ( !result.Functions.Contains( name ) )
			result.Functions.Add( name );

		return $"{name}( {string.Join( ", ", args )} )";
	}

	public string RegisterFunction( string code, [CallerArgumentExpression( "code" )] string propertyName = "", bool forceRegister = false )
	{
		if ( !GraphHLSLFunctions.HasFunction( propertyName ) )
		{
			GraphHLSLFunctions.RegisterFunction( propertyName, code );
		}
		else if ( forceRegister )
		{
			GraphHLSLFunctions.RemoveFunction( propertyName );
			GraphHLSLFunctions.RegisterFunction( propertyName, code );
		}


		return propertyName;
	}

	/// <summary>
	/// Slightly tweaked version of the original. Only used by the Custom Expression node.
	/// </summary>
	internal string ResultFunctionCustomExpression( BaseNodePlus node, string code, string functionName, string args = "", bool includeFile = false )
	{
		var result = ShaderResult;

		if ( !includeFile )
		{
			if ( !GraphHLSLFunctions.HasFunction( functionName ) )
			{
				GraphHLSLFunctions.RegisterFunction( functionName, code );
			}
			else
			{
				//SGPLog.Warning( $"Function `{functionName}` already registerd..." );
			}
		}
		else
		{
			RegisterInclude( code );
		}

		return $"{functionName}( {args} )";
	}

	internal void RegisterVoidFunctionResults( CustomFunctionNode node, Dictionary<string, string> values, out List<CustomCodeOutputData> outputDataList, out List<string> functionOutputs, bool inlineMode = false )
	{
		var result = ShaderResult;
		var sb = new StringBuilder();

		outputDataList = new List<CustomCodeOutputData>();
		functionOutputs = new List<string>();

		//SGPLog.Info( $" RV : AlreadyEvaluated? = {!result.VoidLocalGroupsTest.Any(x => x.Key == node.Identifier)}");

		if ( !result.VoidLocalGroups.Any( x => x.Key == node.Identifier ) )
		{
			foreach ( var value in values )
			{
				//SGPLog.Info($" RV : Starting to process `{value.Key}`:`{value.Value}`");

				var varName = $"vl_{result.VoidLocalCount++}";
				var dataType = value.Value;

				//SGPLog.Info( $" RV : Created compiler name `{varName}` for freindly name `{value.Key}`" );

				CustomCodeOutputData outputData = new CustomCodeOutputData();
				outputData.CompilerName = varName;
				outputData.FriendlyName = value.Key;
				outputData.DataType = dataType;
				outputData.ComponentCount = GetComponentCountFromHLSLDataType( dataType );
				outputData.ResultType = GetResultTypeFromHLSLDataType( dataType );
				outputData.NodeId = node.Identifier;


				//SGPLog.Info( $" RV : Created outputData for `{value.Key}`" );

				outputDataList.Add( outputData );
				functionOutputs.Add( outputData.CompilerName );
			}

			if ( !result.VoidLocalGroups.ContainsKey( node.Identifier ) )
			{
				//SGPLog.Info( $" RV : Adding new Entry for NodeID `{node.Identifier}` " );
				result.VoidLocalGroups.Add( node.Identifier, outputDataList );
			}
			else
			{
				//SGPLog.Warning($" RV : NodeID `{node.Identifier}` already exists in Dict... ");
			}
		}
		else
		{
			//SGPLog.Warning( $" RV : We have already registerd the outputs for this node..." );
		}
	}

	/// <summary>
	/// Loops through ShaderResult.Gradients to find the matching key then returns the corresponding Gradient.
	/// </summary>
	public Gradient GetGradient( string gradient_name )
	{
		var result = ShaderResult;

		Gradient gradient = new();

		foreach ( var g in result.Gradients )
		{
			if ( g.Key == gradient_name )
			{
				gradient = g.Value;
			}
		}

		return gradient;
	}

	/// <summary>
	/// Register a gradient and return the name of the graident. A generic name is returned instead if the gradient name is empty.
	/// </summary>
	public string RegisterGradient( Gradient gradient, string gradient_name )
	{
		var result = ShaderResult;

		var name = CleanName( gradient_name );

		name = string.IsNullOrWhiteSpace( name ) ? $"Gradient{result.Gradients.Count}" : name;

		var id = name;

		if ( !result.Gradients.ContainsKey( id ) )
		{
			result.Gradients.Add( id, gradient );
		}

		return name;
	}

	/// <summary>
	/// Register a sampler and return the name of it
	/// </summary>
	public string ResultSampler( Sampler sampler )
	{
		var name = CleanName( sampler.Name );
		name = string.IsNullOrWhiteSpace( name ) ? $"Sampler{ShaderResult.SamplerStates.Count}" : name;
		var id = name;
		var result = ShaderResult;

		if ( !result.SamplerStates.ContainsKey( id ) )
		{
			result.SamplerStates.Add( id, sampler ); // Add the Name of the sampler and its associated options.
		}

		return $"g_s{id}";
	}

	public string ResultSamplerOrDefault( NodeInput sampler, Sampler defaultsampler )
	{
		if ( sampler.IsValid )
		{
			return Result( sampler ).Code;
		}
		else
		{
			// Register the default sampler
			return ResultSampler( defaultsampler );
		}
	}

	internal void RegisterSyncID( string syncID, string name )
	{
		if ( !SyncIDs.ContainsKey( syncID ) )
		{
			SyncIDs.Add( syncID, name );
		}
	}

	internal bool CheckTextureInputRegistration( string name )
	{
		if ( !ShaderResult.TextureInputs.ContainsKey( name ) )
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Register a texture and return the name of it
	/// </summary>
	public (string TextureGlobal, string samplerGlobal) ResultTexture( string samplerinput, TextureInput input, Texture texture )
	{
		var name = CleanName( input.Name );
		name = string.IsNullOrWhiteSpace( name ) ? $"Texture_{StageName}_{ShaderResult.TextureInputs.Count}" : name;
		var id = name;
		//int count = 0;

		var result = ShaderResult;

		// Dont append _count++ to the end until the key dosent exists.
		// This can cause problems resulting in unnecessary Texture2D declarations with Combo Switches and inside subgraphs when the subgraphs are instanced.
		/*
		while ( result.TextureInputs.ContainsKey( id ) )
		{
			id = $"{name}_{count++}";	
		}
		
		OnAttribute?.Invoke( id, texture, false );
		
		result.TextureInputs.Add( id, input );
		*/

		if ( result.TextureInputs.TryGetValue( id, out var existingValue ) )
		{
			// Do not Invoke OnAttribute or add to the TextureInputs Dictionary...
		}
		else
		{
			OnAttribute?.Invoke( id, texture, false );
			result.TextureInputs.Add( id, input );
		}

		if ( CurrentResultInput == "Albedo" )
		{
			result.RepresentativeTexture = $"g_t{id}";
		}

		return new( $"g_t{id}", samplerinput );
	}

	/// <summary>
	/// Get result of an input with an optional default value if it failed to resolve
	/// </summary>
	public NodeResult ResultOrDefault<T>( NodeInput input, T defaultValue )
	{
		var result = Result( input );
		return result.IsValid ? result : ResultValue( defaultValue );
	}

	public void DepreciationWarning( BaseNodePlus node, string oldnode, string newnode )
	{
		var warnings = new List<string>();

		//NodeErrors.Add( node, warnings );
		NodeWarnings.Add(node, warnings);

		warnings.Add($"'{oldnode}' is depreciated please use '{newnode}' instead.");
	}

	private List<(NodeResult, NodeResult)> SwitchResultStack = new();

	/// <summary>
	/// Get result of an input
	/// </summary>
	public NodeResult Result( NodeInput input )
	{
		if (!input.IsValid)
			return default;

		BaseNodePlus node = null;
		if ( string.IsNullOrEmpty( input.Subgraph ) )
		{
			if ( Subgraph is not null )
			{
				var nodeId = string.Join( ',', SubgraphStack.Select( x => x.Item1.Identifier ) );
				return Result( new()
				{
					Identifier = input.Identifier,
					Output = input.Output,
					Subgraph = Subgraph.Path,
					SubgraphNode = nodeId
				} );
			}

			if ( !IsLightingPage )
			{
				node = Graph.FindNode( input.Identifier );
			}
			else
			{
				node = LightingGraph.FindNode( input.Identifier );
			}
		}
		else
		{
			var subgraph = Subgraphs.FirstOrDefault( x => x.Path == input.Subgraph );
			if ( subgraph is not null )
			{
				node = subgraph.FindNode( input.Identifier );
			}
		}

		ComboSwitchInfo? lastComboSwitchInfo = null;

		//SGPLog.Info( $"CurrentComboSwitchInfo is: {CurrentComboSwitchInfo}", IsPreview );
#region ComboSwitch Region
		// Set CurrentSwitchInfo to the current incoming SwitchInfo.
		if ( input.ComboSwitchInfo.BoundSwitchBlock is not StaticSwitchBlock.None )
		{
			SGPLog.Info( $"", IsPreview && ConCommands.VerboseDebgging );
			SGPLog.Info( $"Setting {nameof( CurrentComboSwitchInfo )} To: {input.ComboSwitchInfo}", IsPreview && ConCommands.VerboseDebgging );
			SGPLog.Info( $"", IsPreview && ConCommands.VerboseDebgging );
			lastComboSwitchInfo = CurrentComboSwitchInfo;
			CurrentComboSwitchInfo = input.ComboSwitchInfo;
			
			// Clear any existing Results & InputResults from a previous block.
			{
				ShaderResult.SwitchBlockInputResults.Clear();
				ShaderResult.SwitchBlockResults.Clear();
			}
		}

		//if ( IsPreview && CurrentComboSwitchInfo.IsValid )
		//{
		//	SGPLog.Info( $"Preview Info for node {node} with id {node.Identifier} : {CurrentComboSwitchInfo}" );
		//}
		if ( IsInComboSwitch )
		{
			Graph.AssignSwitchInfo( input.Identifier, CurrentComboSwitchInfo );

			if ( ShaderResult.SwitchBlockInputResults.TryGetValue( input, out var exisitingResultSB ) )
			{
				return exisitingResultSB;
			}
		}
#endregion ComboSwitch Region
		else
		{
			if ( ShaderResult.InputResults.TryGetValue( input, out var result ) )
			{
				return result;
			}
		}

		if ( node == null )
		{
			return default;
		}

		var nodeType = node.GetType();
		var property = nodeType.GetProperty(input.Output);

		if ( property == null )
		{
			// Search for alias
			var allProperties = nodeType.GetProperties();
			foreach (var prop in allProperties)
			{
				var alias = prop.GetCustomAttribute<AliasAttribute>();
				if (alias is null) continue;
				foreach (var al in alias.Value)
				{
					if (al == input.Output)
					{
						property = prop;
						break;
					}
				}
				if (property != null)
					break;
			}
		}

		object value = null;

		if ( node is not IRerouteNode && node is not CustomFunctionNode && InputStack.Contains( input ) )
		{
			NodeErrors[node] = new List<string> { "Circular reference detected" };
			return default;
		}

		InputStack.Add( input );

		if ( Subgraph is not null && node.Graph != Subgraph )
		{
			if ( node.Graph != Graph )
			{
				Subgraph = node.Graph as ShaderGraphPlus;
			}
			else
			{
				Subgraph = null;
			}
		}

		if ( node is CustomFunctionNode customFunctionNode )
		{
			var funcResult = customFunctionNode.GetResult( this );
			var outputData = new CustomCodeOutputData();
			
			foreach ( var entry in customFunctionNode.OutputData )
			{

				if ( entry.FriendlyName == input.Output )
				{
				    outputData = entry;
				    break;
				}
			}

			if ( !customFunctionNode.ExpressionOutputs.Any() )
			{
				//Utilities.EdtiorSound.OhFiddleSticks();
				
				NodeErrors[node] = new List<string> { $"`{customFunctionNode.DisplayInfo.Name}` has no outputs." };
				
				return default;
			}

			if ( !outputData.IsValid )
			{
				//Utilities.EdtiorSound.OhFiddleSticks();
				
				NodeErrors[node] = new List<string> { $"Unable to find valid CustomCodeOutputData entry for `{node.DisplayInfo.Name}`" };
				
				return default;
			}

			var localResult = new NodeResult(outputData.ResultType, $"{outputData.CompilerName}", voidComponents: outputData.ComponentCount );

			// return the localResult if we are getting a result from a node that we have already evaluated. 
			foreach ( var inputResult in ShaderResult.InputResults )
			{
				if ( inputResult.Key.Identifier == input.Identifier )
				{
					InputStack.Remove( input );
					return localResult;
				}
			}

			ShaderResult.InputResults.Add( input, localResult );
			ShaderResult.Results.Add( ( localResult, funcResult ) );
			
			//if ( IsPreview )
			//{
			//    Nodes.Add( node );
			//}
			
			InputStack.Remove( input );
			
			return localResult;
		}

		if ( node is SubgraphNode subgraphNode )
		{
			var newStack = (subgraphNode, Subgraph);
			var lastNode = SubgraphNode;

			SubgraphStack.Add( newStack );
			Subgraph = subgraphNode.Subgraph;
			SubgraphNode = subgraphNode;
			if ( !Subgraphs.Contains( Subgraph ) )
			{
				Subgraphs.Add( Subgraph );
			}

			var resultNode = Subgraph.Nodes.FirstOrDefault( x => x is FunctionResult ) as FunctionResult;
			var resultInput = resultNode.Inputs.FirstOrDefault( x => x.Identifier == input.Output );
			if ( resultInput?.ConnectedOutput is not null )
			{
				var nodeId = string.Join( ',', SubgraphStack.Select( x => x.Item1.Identifier ) );
				var newConnection = new NodeInput()
				{
					Identifier = resultInput.ConnectedOutput.Node.Identifier,
					Output = resultInput.ConnectedOutput.Identifier,
					Subgraph = Subgraph.Path,
					SubgraphNode = nodeId
				};
				var newResult = Result( newConnection );

				if ( NodeErrors.Any() )
				{
					InputStack.Remove( input );
					return default;
				}

				InputStack.Remove( input );
				SubgraphStack.RemoveAt( SubgraphStack.Count - 1 );
				Subgraph = newStack.Item2;
				SubgraphNode = lastNode;
				return newResult;
			}
			else
			{
				value = GetDefaultValue( subgraphNode, input.Output, resultInput.Type );
				SubgraphStack.RemoveAt( SubgraphStack.Count - 1 );
				Subgraph = newStack.Item2;
				SubgraphNode = lastNode;
			}
		}
		else
		{
			if ( Subgraph is not null )
			{
				if ( node is IParameterNode parameterNode && !string.IsNullOrWhiteSpace( parameterNode.Name ) )
				{
					var newResult = ResolveParameterNode( parameterNode, ref value, out var error );

					// TODO : This is just a shitty placeholder until you can just set a defaults of Sampler 
					// and Texture 2D Objects on the subgraph node itself when the input connectedPlug is null.
					if ( !string.IsNullOrWhiteSpace( error.Item2 ) )
					{
						NodeErrors.Add( error.Item1, new List<string> { error.Item2 } );
					
						InputStack.Remove( input );
						return default;
					}

					if ( newResult.IsValid )
					{
						InputStack.Remove( input );
						return newResult;
					}
				}
			}
			else if ( Graph.IsSubgraph )
			{
				if ( node is IParameterNode parameterNode )
				{
					if ( parameterNode.PreviewInput.IsValid )
					{
						var paramResult = Result( parameterNode.PreviewInput );
						InputStack.Remove( input );
						return paramResult;
					}
				}
			}

			if ( value is null )
			{
				if ( property == null )
				{
					InputStack.Remove( input );
					return default;
				}

				value = property.GetValue( node );
			}

			if ( value == null )
			{
				//SGPLog.Info( $"value of `{node}` is null", IsNotPreview );
				InputStack.Remove( input );
				return default;
			}
		}

		if ( value is NodeResult nodeResult )
		{
			InputStack.Remove( input );
			return nodeResult;
		}
		else if ( value is NodeInput nodeInput )
		{
			if ( nodeInput == input )
			{
				InputStack.Remove( input );
				return default;
			}

			var newResult = Result( nodeInput );

			InputStack.Remove( input );
			return newResult;
		}
		else if ( value is NodeResult.Func resultFunc )
		{
			var funcResult = resultFunc.Invoke( this );

			funcResult.SetSwitchInfo( CurrentComboSwitchInfo );
			ComboSwitchInfoStack.Add( funcResult.SwitchInfo );

			// Gets Static switches generating within a static switch.
			if ( node is StaticSwitchNode staticSwitchnode && ComboSwitchInfoStack.Contains( CurrentComboSwitchInfo ) && input.ComboSwitchInfo.IsValid )
			{
				funcResult.SetSwitchInfo( input.ComboSwitchInfo );
				funcResult.SkipLocalGeneration = true;
			}

			if ( funcResult.SwitchInfo.BoundSwitchBlock != StaticSwitchBlock.None )
			{
				funcResult.SkipLocalGeneration = true;
			}

			if ( IsPreview && !IsInComboSwitch )
			{
				funcResult.PreviewID = node.PreviewID;
			}

			if ( !funcResult.IsValid )
			{
				//SGPLog.Error( $"funcResult of `{node}` is Invalid!", IsNotPreview );

				if ( !NodeErrors.TryGetValue( node, out var errors ) )
				{
					errors = new();
					NodeErrors.Add( node, errors );
				}

				if ( funcResult.Errors is null || funcResult.Errors.Length == 0 )
				{
					errors.Add( $"Missing input" );
				}
				else
				{
					foreach ( var error in funcResult.Errors )
						errors.Add( error );
				}
		
				InputStack.Remove( input );
				return default;
			}

			// We can return this result without making it a local variable because it's constant
			if ( funcResult.Constant )
			{
				//SGPLog.Info( $"Result from node : `{node}` is constant! which is `{funcResult.Code}`", IsNotPreview );
				InputStack.Remove( input );
				
				if ( ComboSwitchInfoStack.Any() )
					ComboSwitchInfoStack.Remove( funcResult.SwitchInfo );

				return funcResult;
			}

			int id = IsInComboSwitch ? ShaderResult.SwitchBlockInputResults.Count : ShaderResult.InputResults.Count;
			var varName = $"l_{id}";
			var localResult = new NodeResult( funcResult.ResultType, varName, funcResult.ComboSwitchBody, CurrentComboSwitchInfo );
			localResult.SkipLocalGeneration = funcResult.SkipLocalGeneration;
			localResult.PreviewID = funcResult.PreviewID;

			if ( IsInComboSwitch && !ShaderResult.SwitchBlockInputResults.ContainsKey( input ) )
			{
				ShaderResult.SwitchBlockInputResults.Add( input, localResult );
			}
			else if ( !ShaderResult.InputResults.ContainsKey( input ) )
			{
				ShaderResult.InputResults.Add( input, localResult );
			}
	
			ShaderResult.Results.Add( (localResult, funcResult) );

			//if ( IsPreview )
			//{
			//	// Avoid fuckyness with node preview mode.
			//	if ( node.CanPreview && !Nodes.Contains( node ) )
			//	{
			//		Nodes.Add( node );
			//	}
			//}

			InputStack.Remove( input );

			if ( ComboSwitchInfoStack.Any() )
				ComboSwitchInfoStack.Remove( funcResult.SwitchInfo );

			return localResult;
		}

		var resultVal = ResultValue( value );
		InputStack.Remove( input );
		return resultVal;
	}

	/// <summary>
	/// Get result of two inputs and cast to the largest component of the two (a float2 and float3 will both become float3 results)
	/// </summary>
	public (NodeResult, NodeResult) Result( NodeInput a, NodeInput b, float defaultA = 0.0f, float defaultB = 1.0f )
	{
		var resultA = ResultOrDefault( a, defaultA );
		var resultB = ResultOrDefault( b, defaultB );

		if ( resultA.Components() == resultB.Components() )
			return (resultA, resultB);

		if ( resultA.Components() < resultB.Components() )
			return (new(resultB.ResultType, resultA.Cast( resultB.Components() ) ), resultB);

		return (resultA, new(resultA.ResultType, resultB.Cast( resultA.Components() ) ) );
	}


	/// <summary>
	/// Get result of a value that can be set in material editor
	/// </summary>
	public NodeResult ResultParameter<T>(string name, T value, T min = default, T max = default, bool isRange = false, bool isAttribute = false, ParameterUI ui = default)
	{
		if ( IsPreview || string.IsNullOrWhiteSpace( name ) || Subgraph is not null )
			return ResultValue( value );

		name = CleanName(name);
		var attribName = name;
		var prefix = GetLocalPrefix( value );

		if ( !name.StartsWith( prefix ) )
			name = prefix + name;

		if ( ShaderResult.Parameters.TryGetValue( name, out var parameter ) )
			return parameter.Result;

		parameter.Result = ResultValue( value, name );

		var options = new StringWriter();

		// If we're an attribute, we don't care about the UI options
		if ( isAttribute )
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
		else if ( value is not Float2x2 || value is not Float3x3 || value is not Float4x4 )
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

			if ( value is not bool && parameter.Result.Components() > 0 && isRange )
			{
				options.Write($"Range{parameter.Result.Components()}( {min}, {max} ); ");
			}
		}

		parameter.Options = options.ToString().Trim();

		// Avoid adding a matrix parameter to the graph if isAttribute is false. Which would make it code to the shader code and unable to be set externally via C#.
		if ( value is Float2x2 || value is Float3x3 || value is Float4x4 ) 
		{
			if ( isAttribute )
				ShaderResult.Parameters.Add( name, parameter );
		}
		else
		{
			ShaderResult.Parameters.Add( name, parameter );
		}

		return parameter.Result;
	}

	/// <summary>
	/// Get result of a value, in preview mode an attribute will be registered and returned
	/// Only supports float, Vector2, Vector3, Vector4, Color.
	/// </summary>
	public NodeResult ResultValue<T>( T value, string name = null, bool previewOverride = false )
	{
		if ( value is NodeInput nodeInput ) return Result( nodeInput );

		bool isConstant = IsPreview && !previewOverride;
		bool isNamed = isConstant || !string.IsNullOrWhiteSpace(name);
		name = isConstant ? $"g_{StageName}_{ShaderResult.Attributes.Count}" : name;

		if ( isConstant )
		{
			OnAttribute?.Invoke( name, value, false );
			ShaderResult.Attributes[name] = value;
		}

		return value switch
		{
			bool v => isNamed ? new NodeResult( ResultType.Bool, $"{name}") : new NodeResult( ResultType.Bool, $"{v.ToString().ToLower()}" ) { },
			int v => isNamed ? new NodeResult( ResultType.Int, $"{name}") : new NodeResult(ResultType.Int, $"{v}", true),
			float v => isNamed ? new NodeResult( ResultType.Float, $"{name}") : new NodeResult(ResultType.Float, $"{v}", true),
			Vector2 v => isNamed ? new NodeResult( ResultType.Vector2, $"{name}") : new NodeResult(ResultType.Vector2, $"float2( {v.x}, {v.y} )"),
			Vector3 v => isNamed ? new NodeResult( ResultType.Vector3, $"{name}") : new NodeResult(ResultType.Vector3, $"float3( {v.x}, {v.y}, {v.z} )"),
			Vector4 v => isNamed ? new NodeResult( ResultType.Color, $"{name}") : new NodeResult(ResultType.Color, $"float4( {v.x}, {v.y}, {v.z}, {v.w} )"),
			Color v => isNamed ? new NodeResult( ResultType.Color, $"{name}") : new NodeResult(ResultType.Color, $"float4( {v.r}, {v.g}, {v.b}, {v.a} )"),
			Float2x2 v => isNamed ? new NodeResult( ResultType.Float2x2, $"{value}") : new NodeResult(ResultType.Float2x2, $"float2x2( {v.M11}, {v.M12}, {v.M21}, {v.M22} )"),
			Float3x3 v => isNamed ? new NodeResult( ResultType.Float3x3, $"{value}") : new NodeResult(ResultType.Float3x3, $"float3x3( {v.M11}, {v.M12}, {v.M13}, {v.M21}, {v.M22}, {v.M23}, {v.M31}, {v.M32}, {v.M33} )"),
			Float4x4 v => isNamed ? new NodeResult( ResultType.Float4x4, $"{value}") : new NodeResult(ResultType.Float4x4, $"float4x4( {v.M11}, {v.M12}, {v.M13}, {v.M14}, {v.M21}, {v.M22}, {v.M23}, {v.M24}, {v.M31}, {v.M32}, {v.M33}, {v.M34}, {v.M41}, {v.M42}, {v.M43}, {v.M44} )"),
			Sampler v => new NodeResult( ResultType.Sampler, $"{v}", true ),
			_ => throw new ArgumentException( "Unsupported attribute type", nameof( value ) )
		};
	}

	private static string GetLocalPrefix<T>( T value)
	{
		var prefix = value switch
		{
			Color _ => "g_v",
			Vector4 _ => "g_v",
			Vector3 _ => "g_v",
			Vector2 _ => "g_v",
			float _ => "g_fl",
			int _ => "g_n",
			//int _ => "g_fl",
			bool _ => "g_b",
			Float2x2 _ => "g_m",
			Float3x3 _ => "g_m",
			Float4x4 _ => "g_m",
			_ => throw new ArgumentException( "Unsupported value type", nameof( value ) )
		};

		return prefix;
	}

	public string GetHLSLDataType( ResultType resultType )
	{
		return resultType switch
		{
			ResultType r when r == ResultType.Bool => "bool",
			ResultType r when r == ResultType.Int => "int",
			ResultType r when r == ResultType.Float => "float",
			ResultType r when r == ResultType.Vector2 => "float2",
			ResultType r when r == ResultType.Vector3 => "float3",
			ResultType r when r == ResultType.Color => "float4",
			ResultType r when r == ResultType.Gradient => "Gradient",
			ResultType r when r == ResultType.Float2x2 => "float2x2",
			ResultType r when r == ResultType.Float3x3 => "float3x3",
			ResultType r when r == ResultType.Float4x4 => "float4x4",
			ResultType r when r == ResultType.Void => "void",
			_ => throw new ArgumentException("Unsupported value type", nameof( resultType ) )
		};
	}

	private static object GetDefaultValue( SubgraphNode node, string name, Type type )
	{
		if ( !node.DefaultValues.TryGetValue( name, out var value ) )
		{
			switch ( type )
			{
				case Type t when t == typeof( Vector2 ):
					return Vector2.Zero;
				case Type t when t == typeof( Vector3 ):
					return Vector3.Zero;
				case Type t when t == typeof( Vector4 ):
					return Vector4.Zero;
				case Type t when t == typeof( Color ):
					return Color.White;
				case Type t when t == typeof( int ):
					return 0;
				case Type t when t == typeof( float ):
					return 0.0f;
				case Type t when t == typeof( bool ):
					return false;
				//case Type t when t == typeof( Sampler ):
				//	return new Sampler() { Name = "Test"};
				default:
					throw new Exception( $"Type `{type}` has no default!" );
			}
		}

		if ( value is JsonElement el )
		{
			if ( type == typeof( bool ) )
			{
				value = el.GetBoolean();
			}
			if ( type == typeof( int ) )
			{
				value = el.GetInt32();
			}
			else if ( type == typeof( float ) )
			{
				value = el.GetSingle();
			}
			else if ( type == typeof( Vector2 ) )
			{
				value = Vector2.Parse( el.GetString() );
			}
			else if ( type == typeof( Vector3 ) )
			{
				value = Vector3.Parse( el.GetString() );
			}
			else if ( type == typeof( Vector4 ) )
			{
				value = Vector4.Parse( el.GetString() );
			}
			else if ( type == typeof( Color ) )
			{
				value = Color.Parse( el.GetString() ) ?? Color.White;
			}
			else if ( type == typeof( Texture2DObject ) )
			{
				value = Color.Magenta;
			}
			//else if ( type == typeof( Sampler ) )
			//{
			//	value = new Sampler() { Name = "Test" };
			//}
		}

		return value;
	}

	private NodeResult ResolveParameterNode( IParameterNode node, ref object value, out (SubgraphNode,string) error )
	{
		var lastStack = SubgraphStack.LastOrDefault();
		var lastNodeEntered = lastStack.Item1;
		error = new();
		if ( lastNodeEntered is not null )
		{
			var parentInput = lastNodeEntered.InputReferences.FirstOrDefault( x => x.Key.Identifier == node.Name );
			if ( parentInput.Key is not null )
			{
				var lastSubgraph = Subgraph;
				var lastNode = SubgraphNode;
				Subgraph = lastStack.Item2;
				SubgraphNode = (Subgraph is null) ? null : lastNodeEntered;
				SubgraphStack.RemoveAt( SubgraphStack.Count - 1 );

				var connectedPlug = parentInput.Key.ConnectedOutput;
				if ( connectedPlug is not null )
				{
					var nodeId = string.Join( ',', SubgraphStack.Select( x => x.Item1.Identifier ) );
					var newResult = Result( new()
					{
						Identifier = connectedPlug.Node.Identifier,
						Output = connectedPlug.Identifier,
						Subgraph = Subgraph?.Path,
						SubgraphNode = nodeId
					} );
					SubgraphStack.Add( lastStack );
					Subgraph = lastSubgraph;
					SubgraphNode = lastNode;
					return newResult;
				}
				else
				{

					// TODO : This is just a shitty placeholder until you can just set them on the 
					// subgraph node itself when the input connectedPlug is null.
					if ( parentInput.Value.Item2 == typeof( Sampler ) )
					{
						error = new ( lastNode, "Missing Sampler Input! This sucks i know...");
						value = null;
						return new();
					}
					else if ( parentInput.Value.Item2 == typeof( Texture2DObject ) )
					{
						error = new( lastNode, "Missing Texture2DObject Input! This sucks i know.." );
						value = null;
						return new();
					}
					else if ( parentInput.Value.Item2 == typeof( TextureCubeObject ) )
					{
						error = new( lastNode, "Missing TextureCubeObject Input! This sucks i know.." );
						value = null;
						return new();
					}


					value = GetDefaultValue( lastNodeEntered, node.Name, parentInput.Value.Item2 );
					SubgraphStack.Add( lastStack );
					Subgraph = lastSubgraph;
					SubgraphNode = lastNode;
				}
			}
		}
		return new();
	}

	private static int GetComponentCount( Type inputType )
	{
		return inputType switch
		{
			Type t when t == typeof( Float4x4 ) => 16,
			Type t when t == typeof( Float3x3 ) => 6,
			Type t when t == typeof( Float2x2 ) => 4,
			Type t when t == typeof( Vector4 ) || t == typeof(Color) => 4,
			Type t when t == typeof( Vector3 ) => 3,
			Type t when t == typeof( Vector2 ) => 2,
			Type t when t == typeof( float ) => 1,
			Type t when t == typeof( int ) => 1,
			_ => 0
		};
	}

	private static ResultType GetResultTypeFromHLSLDataType( string DataType )
	{
		return DataType switch
		{
			"bool" => ResultType.Bool,
			"int" => ResultType.Int,
			"float" => ResultType.Float,
			"float2" => ResultType.Vector2,
			"float3" => ResultType.Vector3,
			"float4" => ResultType.Color,
			"float2x2" => ResultType.Float2x2,
			"float3x3" => ResultType.Float3x3,
			"float4x4" => ResultType.Float4x4,
			"Sampler" => ResultType.Sampler,
			"Texture2D" => ResultType.Texture2DObject,
			"TextureCube" => ResultType.TextureCubeObject,
			_ => throw new ArgumentException( "Unknown ResultType" )
		};
	}

	private static int GetComponentCountFromHLSLDataType( string DataType )
	{
		return DataType switch
		{
			"bool" => 0,
			"int" => 1,
			"float" => 1,
			"float2" => 2,
			"float3" => 3,
			"float4" => 4,
			"float2x2" => 4,
			"float3x3" => 9,
			"float4x4" => 16,
			_ => throw new ArgumentException( $"DataType is not valid : {DataType}" )
		};
	}

	private static IEnumerable<PropertyInfo> GetNodeInputProperties( Type type )
	{
		return type.GetProperties( BindingFlags.Instance | BindingFlags.Public )
			.Where( property => property.GetSetMethod() != null &&
			property.PropertyType == typeof( NodeInput ) &&
			property.IsDefined( typeof( BaseNodePlus.InputAttribute ), false ) );
	}

	private string GenerateFeatures()
	{
		var sb = new StringBuilder();
		var result = ShaderResult;
		
		// Register any Graph level Shader Features...
		//RegisterShaderFeatures( Graph.shaderFeatureNodeResults );
		
		if ( Graph.MaterialDomain is MaterialDomain.BlendingSurface )
		{
			sb.AppendLine("Feature( F_MULTIBLEND, 0..3 ( 0=\"1 Layers\", 1=\"2 Layers\", 2=\"3 Layers\", 3=\"4 Layers\", 4=\"5 Layers\" ), \"Number Of Blendable Layers\" );");
		}
		
		foreach (var feature in ShaderFeatures )
		{
			sb.AppendLine( feature.Value.CreateFeatureDeclaration() );
		}
		
		//if ( Graph.FeatureRules.Any() )
		//{
		//	foreach ( var rule in Graph.FeatureRules )
		//	{
		//		if ( rule.IsValid )
		//		{
		//			sb.AppendLine( $"FeatureRule(Allow1( {String.Join( ", ", rule.Features )} ), \"{rule.HoverHint}\");" );
		//		}
		//	}
		//}

		return sb.ToString();
	}

	private string GenerateCommon()
	{
		var sb = new StringBuilder();

		if ( ShaderFeatures.Any() )
		{
			sb.AppendLine( $"#ifndef SWITCH_TRUE" );
			sb.AppendLine( $"#define SWITCH_TRUE 1" );
			sb.AppendLine( $"#endif" );

			sb.AppendLine( $"#ifndef SWITCH_FALSE" );
			sb.AppendLine( $"#define SWITCH_FALSE 0" );
			sb.AppendLine( $"#endif" );

			sb.AppendLine();
		}

		var blendMode = Graph.BlendMode;
		var alphaTest = blendMode == BlendMode.Masked ? 1 : 0;
		var translucent = blendMode == BlendMode.Translucent ? 1 : 0;

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

	private bool GenerateLighting( bool isPreview, bool compositeAtmospherics, out string globals,out string result )
	{
		globals = ""; 
		result = "";

		GeneratingLightingFunc = isPreview;

		// May have already evaluated and there's errors
		if ( Errors.Any() )
			return false;

		var material = GenerateLightingResult();
		
		var locals = GenerateLocals();
		globals = GenerateGlobals();

		if ( Errors.Any() )
			return false;

		//SGPLog.Info( globals );

		var sb = new StringBuilder();

		sb.AppendLine( $"float3 Albedo = float3( 0, 0, 0 );" );
		sb.AppendLine();

		var str =  string.Format( LightingTemplate.Contents,
			IndentString( sb.ToString(), 1),
			IndentString( locals, 2 ),
			IndentString( material, 2 ),
			IndentString( GenerateIndirect(), 2 ),
			IndentString( GenerateAtmospherics(), 1 )
		);

		result = str;

		return true;
	}

	/// <summary>
	/// Generate shader code, will evaluate the graph if it hasn't already.
	/// Different code is generated for preview and not preview.
	/// </summary>
	public string Generate()
	{
		// May have already evaluated and there's errors
		if ( Errors.Any() )
			return null;

		var material = GenerateMaterial();
		var pixelOutput = GeneratePixelOutput();
		
		// If we have any errors after evaluating, no point going further
		if ( Errors.Any() )
			return null;

		string template = ShaderTemplate.Code;

		if ( Graph.MaterialDomain is MaterialDomain.BlendingSurface )
		{
			template = ShaderTemplateBlending.Code;
		}

		return string.Format( template,
			Graph.Description,
			IndentString( GenerateFeatures(), 1),
			IndentString( GenerateCommon(), 1 ),
			IndentString( GenerateGlobals(), 1 ),
			IndentString( GenerateLocals(), 2 ),
			IndentString( material, 2 ),
			IndentString( GenerateVertex(), 2 ),
			IndentString( GenerateGlobals(), 1 ),
			IndentString( GenerateVertexComboRules(), 1 ),
			IndentString( GeneratePixelComboRules(), 1 ),
			IndentString( GenerateFunctions( PixelResult ), 1 ),
			IndentString( GenerateFunctions( VertexResult ), 1 ),
			IndentString( GeneratePixelInit(), 2 ),
			IndentString( pixelOutput, 2 )
		);
	}

	//private static string GenerateFunctions( CompileResult result )
	//{
	//	var sb = new StringBuilder();
	//
	//	foreach ( var entry in result.Functions )
	//	{
	//		sb.Append( entry.Value );
	//	}
	//
	//	return sb.ToString();
	//}

	private static string GenerateFunctions( CompileResult result )
	{
		if ( !result.Functions.Any() )
			return null;

		var sb = new StringBuilder();
		foreach ( var function in result.Functions )
		{
			if ( GraphHLSLFunctions.TryGetFunction( function, out var code ) )
			{
				sb.Append( code );
			}
		}

		return sb.ToString();
	}

	public static string IndentString( string input, int tabCount )
	{
		if ( tabCount == 0 ) return input;

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
		var sb = new StringBuilder();
		
		foreach ( var include in VertexIncludes )
		{
			sb.AppendLine( $"#include \"{include}\"" );
		}
		
		if ( IsNotPreview )
			return null;
		
		sb.AppendLine();
		sb.AppendLine( "DynamicComboRule( Allow0( D_MORPH ) );" );
		return sb.ToString();
	}

	private string GeneratePixelComboRules()
	{
		var sb = new StringBuilder();
		var pixelIncludes = new HashSet<string>(PixelIncludes);
		
		if ( Graph.MaterialDomain == MaterialDomain.PostProcess )
		{
			pixelIncludes.Add( "postprocess/functions.hlsl" );
			pixelIncludes.Add( "postprocess/common.hlsl" );
		}
		
		foreach ( var include in pixelIncludes )
		{
			sb.AppendLine($"#include \"{include}\"");
		}
		
		sb.AppendLine();
		sb.AppendLine( "DynamicCombo( D_RENDER_BACKFACES, 0..1, Sys( ALL ) );" );
		sb.AppendLine( "RenderState( CullMode, D_RENDER_BACKFACES ? NONE : BACK );" );
		
		return sb.ToString();
	}

	private string GeneratePixelInit()
	{
		Stage = ShaderStage.Pixel;
		if ( Graph.ShadingModel == ShadingModel.Lit && Graph.MaterialDomain != MaterialDomain.PostProcess )
			return ShaderTemplate.Material_init;
		if ( Graph.ShadingModel == ShadingModel.Custom )
			return ShaderTemplate.Material_init;
		return "";
	}

	private bool GetUnlitResult( out string albedo, out string opacity)
	{
		var resultNode = Graph.Nodes.OfType<BaseResult>().FirstOrDefault();
		
		albedo = "";
		opacity = "";

		if ( resultNode == null )
			return false;
		
		var albedoResult = resultNode.GetAlbedoResult( this );
		albedo = albedoResult.Cast( GetComponentCount( typeof( Vector3 ) ) ) ?? "float3(1.00,1.00,1.00)";
		var opacityResult = resultNode.GetOpacityResult( this );
		opacity = opacityResult.Cast( 1 ) ?? "1.00";

		return true;
	}

	private string GeneratePixelOutput()
	{
		Stage = ShaderStage.Pixel;
		
		Subgraph = null;
		SubgraphStack.Clear();
		
		if ( Graph.ShadingModel == ShadingModel.Unlit || Graph.MaterialDomain == MaterialDomain.PostProcess )
		{

			if ( !GetUnlitResult( out string albedo, out string opacity ) )
				return null;
			return $"return float4( {albedo}, {opacity} );";
		}
		else if ( Graph.ShadingModel == ShadingModel.Lit )
		{
			return ShaderTemplate.Material_output;
		}
		else if ( Graph.ShadingModel == ShadingModel.Custom )
		{
			var resultLighting = Graph.LightingNodes.OfType<BaseResult>().FirstOrDefault();

			string pixelOutput = "";

			if ( resultLighting != null )
			{
				string funcCall = "";

				if ( resultLighting is LightingResult lightingResult )
				{
					var lightResult = lightingResult.GetAlbedoResult( this, true );
					var compiler = new GraphCompiler( _Asset, Graph, LightingGraph, false, true );
		
					if ( compiler.GenerateLighting( IsPreview, true, out string lightingGlobals, out string lightingFunctionResult ) )
					{
						Shade = lightingFunctionResult;
						
						var func = RegisterFunction( Shade, forceRegister: true );
						funcCall = ResultFunction( "Shade", "i, m");
						
						ShaderResult.LightingFunctionGlobals = lightingGlobals;
					}
				}

				if ( string.IsNullOrWhiteSpace( funcCall ) )
					return null;

				if ( !GetUnlitResult( out string albedo, out string opacity ) )
					return null;

				var sb = new StringBuilder();

				sb.AppendLine( $"m.Albedo = {albedo};" );
				sb.AppendLine( $"m.Opacity = {opacity};" );
				sb.AppendLine();
				sb.AppendLine( $"return {funcCall};" );
				sb.Append( $"//return float4( {albedo}, {opacity} );" );

				pixelOutput = sb.ToString();
			}
			else
			{
				if ( !GetUnlitResult( out string albedo, out string opacity ) )
					return null;

				pixelOutput = $"return float4( {albedo}, {opacity} );";
			}

			return pixelOutput;
		}

		return null;
	}

	private string GetPropertyValue(PropertyInfo property, Result resultNode)
	{
		NodeResult result;
		var inputAttribute = property.GetCustomAttribute<BaseNodePlus.InputAttribute>();
		var componentCount = GetComponentCount( inputAttribute.Type );
		
		if ( property.GetValue( resultNode ) is NodeInput connection && connection.IsValid() )
		{
			result = Result( connection );
		}
		else
		{
			var editorAttribute = property.GetCustomAttribute<BaseNodePlus.NodeEditorAttribute>();
			if ( editorAttribute == null )
				return null;
		
			var valueProperty = resultNode.GetType().GetProperty( editorAttribute.ValueName );
			if ( valueProperty == null )
				return null;
		
			result = ResultValue( valueProperty.GetValue( resultNode ), previewOverride: true );
		}
		
		if ( Errors.Any() )
			return null;
		
		if ( !result.IsValid() )
			return null;
		
		if ( string.IsNullOrWhiteSpace( result.Code ) )
			return null;
		
		return $"{result.Cast( componentCount )}";
	}

	private string GenerateGlobals()
	{
		var sb = new StringBuilder();

		foreach ( var feature in ShaderFeatures )
		{
			//SGPLog.Info( $"DynamicCombo( D_{feature.Value.FeatureName.ToUpper()}, 0..1, Sys( ALL ) );", IsPreview );
			
			var combo = feature.Value.CreateCombo( feature.Key, IsPreview );

			if ( feature.Value.IsDynamicCombo || IsPreview )
			{
				sb.AppendLine( $"DynamicCombo( {combo}, 0..{feature.Value.OptionsCount - 1}, Sys( ALL ) );" );
			}
			else
			{
				sb.AppendLine( $"StaticCombo( {combo}, {feature.Value.CreateFeature}, Sys( ALL ) );" );
			}

			sb.AppendLine();
		}

		foreach ( var global in ShaderResult.Globals )
		{
			sb.AppendLine( global.Value );
		}
		
		foreach ( var Sampler in ShaderResult.SamplerStates )
		{
			sb.Append( $"{Sampler.Value.CreateSampler( Sampler.Key )} <" )
			  .Append( $" Filter( {Sampler.Value.Filter.ToString().ToUpper()} );" )
			  .Append( $" AddressU( {Sampler.Value.AddressU.ToString().ToUpper()} );" )
			  .Append( $" AddressV( {Sampler.Value.AddressV.ToString().ToUpper()} ); >;" )
			  .AppendLine();
		}
		
		if ( IsPs )
		{
			if ( Graph.MaterialDomain is MaterialDomain.PostProcess )
			{
				sb.AppendLine("Texture2D g_tColorBuffer < Attribute( \"ColorBuffer\" ); SrgbRead( true ); >;");
			}
		}

		//if ( IsPs )
		{
			//if ( !string.IsNullOrWhiteSpace( ShaderResult.LightingFunctionGlobals ) )
			{
				sb.Append( ShaderResult.LightingFunctionGlobals );
			}
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
				if ( result.Value is Float2x2 || result.Value is Float3x3 || result.Value is Float4x4 )
					continue;

				var typeName = result.Value switch
				{
					Color _ => "float4",
					Vector4 _ => "float4",
					Vector3 _ => "float3",
					Vector2 _ => "float2",
					float _ => "float",
					int _ => "float", // treat int internally as a float.
					bool _ => "bool",
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

			if ( !string.IsNullOrWhiteSpace( ShaderResult.RepresentativeTexture ) )
			{
				sb.AppendLine( $"TextureAttribute( LightSim_DiffuseAlbedoTexture, {ShaderResult.RepresentativeTexture} )" );
				sb.AppendLine( $"TextureAttribute( RepresentativeTexture, {ShaderResult.RepresentativeTexture} )" );
			}

			foreach ( var parameter in ShaderResult.Parameters )
			{
				var resultType = parameter.Value.Result.ResultType;
				//if ( resultType is ResultType.Float2x2 || resultType is ResultType.Float3x3 || resultType is ResultType.Float3x3 )
				//{
				//
				//}
				sb.AppendLine( $"{parameter.Value.Result.TypeName} {parameter.Key} < {parameter.Value.Options} >;" );
			}
		}
		
		if ( sb.Length > 0 )
		{
			sb.Insert( 0, "\n" );
		}
		
		return sb.ToString();
	}

	internal void GenerateLocalResults( ref StringBuilder sb, IEnumerable<(NodeResult, NodeResult)> shaderResults, out (NodeResult, NodeResult) lastResult, bool preview, bool appendOverride = false, int indentLevel = 0 )
	{
		//int localId = 1;
		lastResult = (new NodeResult(), new NodeResult());

		foreach ( var result in shaderResults )
		{
			//if ( result.Item2.ResultType is ResultType.TextureObject )
			//{
			//	sb.AppendLine( $"Texture2D {result.Item1} = {result.Item2.Code};" );
			//	sb.AppendLine( $"if ( g_iStageId == {localId++} ) return {result.Item2.Code}.Sample( g_sAniso, i.vTextureCoords.xy );" );
			//}
			//else if ( result.Item2.ResultType is ResultType.Float2x2 )
			//{
			//	sb.AppendLine( $"float2x2 {result.Item1} = float2x2( {result.Item2.Code} );" );
			//}
			//else if ( result.Item2.ResultType is ResultType.Float3x3 )
			//{
			//	sb.AppendLine( $"float3x3 {result.Item1} = float3x3( {result.Item2.Code} );" );
			//}
			//else if ( result.Item2.ResultType is ResultType.Float4x4 )
			//{
			//	sb.AppendLine( $"float4x4 {result.Item1} = float4x4( {result.Item2.Code} );" );
			//}
			//else
			//{
			if ( result.Item2.ResultType is ResultType.Void ) // TODO : Get rid of this and just do it how the node GetDimensions works with a void function outptus and call.
			{
				sb.AppendLine();
				sb.AppendLine( IndentString( $"{result.Item2.Code}", indentLevel ) );
				sb.AppendLine();
			}
			else
			{
				lastResult = result;
				var shouldSkip = result.Item2.SkipLocalGeneration;

				if ( !shouldSkip || appendOverride )
				{
					if ( !string.IsNullOrWhiteSpace( result.Item2.ComboSwitchBody ) )
					{
						sb.AppendLine( IndentString( result.Item2.ComboSwitchBody, indentLevel ) );
					}

					if ( ShaderResult.VoidLocals.Any() && ShaderResult.VoidLocals.ContainsKey( result.Item2.Code ) )
					{
						var data = ShaderResult.VoidLocals[result.Item2.Code];

						if ( !data.AlreadyDefined )
						{
							var newData = new VoidData()
							{
								TargetResult = data.TargetResult,
								ResultType = data.ResultType,
								FunctionCall = data.FunctionCall,
								AlreadyDefined = true
							};

							ShaderResult.VoidLocals[result.Item2.Code] = newData;

							sb.AppendLine( IndentString( data.ResultInit, indentLevel ) );
							sb.AppendLine( IndentString( data.FunctionCall, indentLevel ) );
						}
					}

					sb.AppendLine( IndentString( $"{result.Item2.TypeName} {result.Item1} = {result.Item2.Code};", indentLevel ) );

					if ( preview && string.IsNullOrWhiteSpace( result.Item2.ComboSwitchBody )  )
					{
						if ( result.Item1.ResultType == ResultType.Bool )
						{
							// TODO
						}
						else if ( result.Item1.ResultType != ResultType.Float2x2 || result.Item1.ResultType != ResultType.Float3x3 || result.Item1.ResultType != ResultType.Float4x4 || result.Item1.ResultType != ResultType.Gradient )
						{
							sb.AppendLine( IndentString( $"if ( g_iStageId == {result.Item1.PreviewID} ) return {result.Item1.Cast( 4, 1.0f )};", indentLevel ) );
							//sb.AppendLine( IndentString( $"if ( g_iStageId == {localId++} ) return {result.Item1.Cast( 4, 1.0f )};", indentLevel ) );
						}
					}

				}

			}
			//}
		}

		//SGPLog.Info( $"`{nameof( GenerateLocalResults )}` Finished", preview );
	}

	private string GenerateLocals()
	{
		var sb = new StringBuilder();
		
		if ( ShaderResult.Results.Any() )
		{
			sb.AppendLine();
		}
		
		if ( Debug )
		{
			if ( IsPreview )
			{
				Log.Info($"Registerd Gradient Count for Preview Is : {ShaderResult.Gradients.Count}");
			}
			else
			{
				Log.Info($"Registerd Gradient Count for Compile Is : {ShaderResult.Gradients.Count}");
			}
		}

		foreach ( var gradient in ShaderResult.Gradients )
		{
			//Log.Info($"Found Gradient : {gradient.Key}");
			//Log.Info($" Gradient Blend Mode : {gradient.Value.Blending}");
		
			sb.AppendLine( $"Gradient {gradient.Key} = Gradient::Init();" );
			sb.AppendLine();
		
			var colorindex = 0;
			var alphaindex = 0;
		
			sb.AppendLine( $"{gradient.Key}.colorsLength = {gradient.Value.Colors.Count};" );
			sb.AppendLine( $"{gradient.Key}.alphasLength = {gradient.Value.Alphas.Count};" );
		
			foreach ( var color in gradient.Value.Colors )
			{
				if ( Debug )
				{
					Log.Info( $"{gradient.Key} Gradient Color {colorindex} : {color.Value} Time : {color.Time}" );
				}

			// All good with time as the 4th component?
			sb.AppendLine( $"{gradient.Key}.colors[{colorindex++}] = float4( {color.Value.r}, {color.Value.g}, {color.Value.b}, {color.Time} );" );
			}
		
			foreach ( var alpha in gradient.Value.Alphas )
			{
				sb.AppendLine( $"{gradient.Key}.alphas[{alphaindex++}] = float( {alpha.Value} );" );
			}

			sb.AppendLine();
		}

		// TODO : Get rid of VoidLocalGroups and just do it how the node GetDimensions works with a void function outptus and call.
		foreach ( var voidLocal in ShaderResult.VoidLocalGroups )
		{
			sb.AppendLine();

			foreach ( var data in voidLocal.Value )
			{
				var initialValue = "";
				
				if ( data.DataType == "bool" )
				{
					initialValue = "false";
				}
				else if ( data.DataType == "int" )
				{
					initialValue = "0";
				}
				else if ( data.DataType == "float" )
				{
					initialValue = "0.0f";
				}
				else if ( data.DataType == "float2" )
				{
					initialValue = "float2( 0.0f, 0.0f )";
				}
				else if ( data.DataType == "float3" )
				{
					initialValue = "float3( 0.0f, 0.0f, 0.0f )";
				}
				else if ( data.DataType == "float4" )
				{
					initialValue = "float4( 0.0f, 0.0f, 0.0f, 0.0f )";
				}

				sb.AppendLine( $"{data.DataType} {data.CompilerName} = {initialValue};");
			}
		}

		sb.AppendLine();

		GenerateLocalResults( ref sb, ShaderResult.Results, out _, IsPreview, false, 0 );

		return sb.ToString();
	}

	private string GenerateLightingResult()
	{
		Stage = ShaderStage.Pixel;
		Subgraph = null;
		SubgraphStack.Clear();

		//if (Graph.ShadingModel != ShadingModel.Lit || Graph.MaterialDomain == MaterialDomain.PostProcess) return "";

		var resultNode = Graph.LightingNodes.OfType<BaseResult>().FirstOrDefault();
		if ( resultNode == null )
			return null;
		
		var sb = new StringBuilder();
		var visited = new HashSet<string>();
		
		foreach ( var property in GetNodeInputProperties( resultNode.GetType() ) )
		{
			if ( property.Name == nameof( LightingResult.Indirect ) )
				continue;

			CurrentResultInput = property.Name;
			visited.Add( property.Name );

			NodeResult result;

			if ( property.GetValue( resultNode ) is NodeInput connection && connection.IsValid() )
			{
				result = Result( connection );
			}
			else
			{
				var editorAttribute = property.GetCustomAttribute<BaseNodePlus.EditorAttribute>();
				if ( editorAttribute == null )
					continue;

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

			sb.AppendLine( $"{property.Name} += {result.Cast( componentCount )};" );
		}

		if ( resultNode is FunctionResult functionResult )
		{
			functionResult.AddMaterialOutputs( this, sb, visited );
		}
		
		visited.Clear();
		
		CurrentResultInput = null;
		
		return sb.ToString();
	}

	private string GenerateIndirect()
	{
		Stage = ShaderStage.Indirect;

		var resultNode = Graph.LightingNodes.OfType<LightingResult>().FirstOrDefault();

		if ( resultNode == null )
			return null;

		var sb = new StringBuilder();

		var indirectInput = resultNode.Indirect;

		NodeResult result;

		if ( indirectInput is NodeInput connection && connection.IsValid() )
		{
			result = Result( connection );
			
			if ( !Errors.Any() && result.IsValid() && !string.IsNullOrWhiteSpace( result.Code ) )
			{
				var componentCount = GetComponentCount( typeof( Vector3 ) );
				
				sb.AppendLine();
				
				foreach ( var local in ShaderResult.Results)
				{
					sb.AppendLine( $"{local.Item2.TypeName} {local.Item1} = {local.Item2.Code};" );
				}
				
				sb.AppendLine( $"indirectColor = { result.Cast( componentCount ) };" );
				sb.AppendLine( $"Albedo += indirectColor;" );
			}
		}

		return sb.ToString();
	}

	private string GenerateAtmospherics()
	{
		var sb = new StringBuilder();

		var resultNode = Graph.LightingNodes.OfType<LightingResult>().FirstOrDefault();
		
		if ( resultNode == null )
			return null;

		if ( resultNode.ApplyFog )
		{
			sb.AppendLine( "//Albedo.xyz = Fog::Apply( m.WorldPosition, m.ScreenPosition.xy, float4( Albedo.xyz, 0 ) );" );
		}

		return sb.ToString();
	}

	private string GenerateMaterial()
	{
		Stage = ShaderStage.Pixel;
		Subgraph = null;
		SubgraphStack.Clear();
		
		if (Graph.ShadingModel != ShadingModel.Lit || Graph.MaterialDomain == MaterialDomain.PostProcess) return "";
		
		
		var resultNode = Graph.Nodes.OfType<BaseResult>().FirstOrDefault();
		if ( resultNode == null )
			return null;
		
		var sb = new StringBuilder();
		var visited = new HashSet<string>();
		
		foreach ( var property in GetNodeInputProperties( resultNode.GetType() ) )
		{
			if ( property.Name == "PositionOffset" )
				continue;

			CurrentResultInput = property.Name;
			visited.Add( property.Name );

			NodeResult result;

			if ( property.GetValue( resultNode ) is NodeInput connection && connection.IsValid() )
			{
				result = Result( connection );
			}
			else
			{
				var editorAttribute = property.GetCustomAttribute<BaseNodePlus.NodeEditorAttribute>();
				if ( editorAttribute == null )
					continue;

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

			sb.AppendLine( $"m.{property.Name} = {result.Cast( componentCount )};" );
		}

		if ( resultNode is FunctionResult functionResult )
		{
			functionResult.AddMaterialOutputs( this, sb, visited );
		}

		visited.Clear();
		
		CurrentResultInput = null;
		
		return sb.ToString();
	}

	private string GenerateVertex()
	{
		Stage = ShaderStage.Vertex;

		var resultNode = Graph.Nodes.OfType<BaseResult>().FirstOrDefault();
		if (resultNode == null)
			return null;

		var positionOffsetInput = resultNode.GetPositionOffset();

		var sb = new StringBuilder();

		switch (Graph.MaterialDomain)
		{
		case MaterialDomain.Surface:
				sb.AppendLine(@"
PixelInput i = ProcessVertex( v );
i.vPositionOs = v.vPositionOs.xyz;
i.vColor = v.vColor;

ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData( v );
i.vTintColor = extraShaderData.vTint;

VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );");
				break;
			case MaterialDomain.BlendingSurface:
				sb.AppendLine(@"
PixelInput i = ProcessVertex( v );
i.vBlendValues = v.vColorBlendValues;
i.vPaintValues = v.vColorPaintValues;
");
				break;
case MaterialDomain.PostProcess:
				sb.AppendLine(@"
PixelInput i;
i.vPositionPs = float4(v.vPositionOs.xy, 0.0f, 1.0f );
i.vPositionWs = float3(v.vTexCoord, 0.0f);
");
				break;
		}

		NodeResult result;
		
		if ( positionOffsetInput is NodeInput connection && connection.IsValid() )
		{
			result = Result( connection );
			
			if ( !Errors.Any() && result.IsValid() && !string.IsNullOrWhiteSpace( result.Code ) )
			{
				var componentCount = GetComponentCount( typeof( Vector3 ) );
				
				sb.AppendLine();
				
				foreach ( var group in ShaderResult.VoidLocalGroups )
				{
					sb.AppendLine();
					
					foreach ( var data in group.Value )
					{
						var initialValue = "";
						
						if ( data.DataType == "bool" )
						{
							initialValue = "false";
						}
						else if ( data.DataType == "int" )
						{
							initialValue = "0";
						}
						else if ( data.DataType == "float" )
						{
							initialValue = "0.0f";
						}
						else if ( data.DataType == "float2" )
						{
							initialValue = "float2( 0.0f, 0.0f )";
						}
						else if ( data.DataType == "float3" )
						{
							initialValue = "float3( 0.0f, 0.0f, 0.0f )";
						}
						else if ( data.DataType == "float4" )
						{
							initialValue = "float4( 0.0f, 0.0f, 0.0f, 0.0f )";
						}
						
						sb.AppendLine( $"{data.DataType} {data.CompilerName} = {initialValue};");
					}
				}
				
				GenerateLocalResults( ref sb, ShaderResult.Results, out _, false, false, 0 );

				//foreach ( var local in ShaderResult.Results)
				//{
				//	if ( local.Item2.ResultType is ResultType.Void )
				//	{
				//		sb.AppendLine( $"{local.Item2.Code};" );
				//	}
				//	else
				//	{
				//		if ( !string.IsNullOrWhiteSpace( local.Item2.ComboSwitchBody ) && !local.Item2.SkipLocalGeneration )
				//		{
				//			sb.AppendLine( local.Item2.ComboSwitchBody );
				//		}
				//		if ( !local.Item2.SkipLocalGeneration )
				//		{
				//			sb.AppendLine( $"{local.Item2.TypeName} {local.Item1} = {local.Item2.Code};" );
				//		}
				//	}
				//}
				
				sb.AppendLine( $"i.vPositionWs.xyz += { result.Cast( componentCount ) };" );
				sb.AppendLine( "i.vPositionPs.xyzw = Position3WsToPs( i.vPositionWs.xyz );" );
			}
		}

		switch ( Graph.MaterialDomain )
		{
			case MaterialDomain.Surface:
				sb.AppendLine( "return FinalizeVertex( i );" );
				break;
			case MaterialDomain.BlendingSurface:
				sb.AppendLine( "return FinalizeVertex( i );" );
				break;
			case MaterialDomain.PostProcess:
				sb.AppendLine( "return i;" );
				break;
		}
		return sb.ToString();
	}
}
