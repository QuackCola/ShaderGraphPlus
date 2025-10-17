using NodeEditorPlus;
using ShaderGraphPlus.Nodes;
using System.Text.Json.Nodes;

using IPlugIn = NodeEditorPlus.IPlugIn;

namespace ShaderGraphPlus;

public interface ISGPJsonUpgradeable
{
	[Hide]
	public int Version { get; }
}

/// <summary>
/// Data that helps us fixup and reconnect broken node connections when updating nodes.
/// </summary>
struct NodeConnectionFixupData
{
	public Dictionary<NodeInput, string> NodeInputs;
	public BaseNodePlus NodeToConnectTo;

	public NodeConnectionFixupData( Dictionary<NodeInput, string> mapping0, BaseNodePlus nodeToConnectTo )
	{
		NodeInputs = mapping0;
		NodeToConnectTo = nodeToConnectTo;
	}

}

partial class ShaderGraphPlus
{
	internal static class VersioningInfo
	{
		/// <summary>
		/// Json Property name of the version number.
		/// </summary>
		internal const string VersionJsonPropertyName = "__version";

		internal const string VersionClassPropertyName = "Version";

		internal const string ShaderGraphReleaseVersionDate = "8/11/2025";
	}

	internal static JsonSerializerOptions SerializerOptions( bool indented = false )
	{
		var options = new JsonSerializerOptions
		{
			WriteIndented = indented,
			PropertyNameCaseInsensitive = true,
			NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
			DefaultIgnoreCondition = JsonIgnoreCondition.Never,
			ReadCommentHandling = JsonCommentHandling.Skip,
		};
	
		options.Converters.Add( new JsonStringEnumConverter( null, true ) );
	
		return options;
	}

	public string Serialize()
	{
		var doc = new JsonObject();
		var options = SerializerOptions( true );

		SerializeObject( this, doc, options );
		SerializeNodes( Nodes, doc, options );
		SerializeParameters( Parameters, doc, options );

		return doc.ToJsonString( options );
	}

	public void Deserialize( string json, string subgraphPath = null )
	{
		using var doc = JsonDocument.Parse( json );
		var root = doc.RootElement;
		var options = SerializerOptions();

		// Check for the version so we can handle upgrades
		var currentVersion = 0; // Assume 0 for files that don't have the Version property
		if ( root.TryGetProperty( VersioningInfo.VersionJsonPropertyName, out var ver ) )
		{
			currentVersion = ver.GetInt32();
		}

		DeserializeObject( this, root, options );
		DeserializeNodes( root, options, subgraphPath, currentVersion );
		DeserializeParameters( root, options );

		if ( currentVersion < 3 )
		{
			GraphV3Upgrade();
		}
	}

	public IEnumerable<BaseNodePlus> DeserializeNodes( string json )
	{
		using var doc = JsonDocument.Parse( json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip } );
		var root = doc.RootElement;
		
		// Check for version in the JSON
		var fileVersion = 1; // Default to current version
		if ( root.TryGetProperty( VersioningInfo.VersionJsonPropertyName, out var ver ) )
		{
			fileVersion = ver.GetInt32();
		}
		else
		{
			fileVersion = 0; // Old file without version
		}

		return DeserializeNodes( root, SerializerOptions(), null, fileVersion );
	}

	public IEnumerable<BaseBlackboardParameter> DeserializeParameters( string json )
	{
		using var doc = JsonDocument.Parse( json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip } );
		var root = doc.RootElement;

		return DeserializeParameters( root, SerializerOptions() );
	}

	private IEnumerable<BaseBlackboardParameter> DeserializeParameters( JsonElement doc, JsonSerializerOptions options )
	{
		var parameters = new Dictionary<string, BaseBlackboardParameter>();
	
		if ( doc.TryGetProperty( "parameters", out var arrayProperty ) )
		{
			foreach ( var element in arrayProperty.EnumerateArray() )
			{
				var typeName = element.GetProperty( "_class" ).GetString();
				var typeDesc = EditorTypeLibrary.GetType( typeName );
				var type = new ClassParameterType( typeDesc );

				BaseBlackboardParameter parameter;

				if ( typeDesc != null )
				{
					parameter = EditorTypeLibrary.Create<BaseBlackboardParameter>( typeName );
					DeserializeObject( parameter, element, options );
					
					//SGPLog.Info( $"parameter.Name == {parameter.Name}" );

					if ( string.IsNullOrWhiteSpace( parameter.Name ) )
					{
						parameter.Name = $"parameter{parameters.Count}";
					}

					if ( parameter is ColorParameter bp )
					{
						bp.UI = bp.UI with { ShowTypeProperty = false };
					}

					parameters.Add( parameter.Name, parameter );

					AddParameter( parameter );
				}
			}
		}

		return parameters.Values;
	}

	private static JsonElement UpgradeJsonUpgradeable( int versionNumber, ISGPJsonUpgradeable jsonUpgradeable, Type type, JsonProperty jsonProperty, JsonSerializerOptions serializerOptions )
	{
		ArgumentNullException.ThrowIfNull( jsonUpgradeable );

		var jsonObject = JsonNode.Parse( jsonProperty.Value.GetRawText() ) as JsonObject;

		SGPJsonUpgrader.Upgrade( versionNumber, jsonObject, type );

		return JsonSerializer.Deserialize<JsonElement>( jsonObject.ToJsonString(), serializerOptions );
	}

	private static JsonElement UpgradeJsonUpgradeable( int versionNumber, ISGPJsonUpgradeable jsonUpgradeable, Type type, JsonElement jsonElement, JsonSerializerOptions serializerOptions )
	{
		ArgumentNullException.ThrowIfNull( jsonUpgradeable );

		var jsonObject = JsonNode.Parse( jsonElement.GetRawText() ) as JsonObject;

		SGPJsonUpgrader.Upgrade( versionNumber, jsonObject, type );

		return JsonSerializer.Deserialize<JsonElement>( jsonObject.ToJsonString(), serializerOptions );
	}

	private static void DeserializeObject( object obj, JsonElement doc, JsonSerializerOptions options )
	{
		var type = obj.GetType();
		var properties = type.GetProperties( BindingFlags.Instance | BindingFlags.Public )
			.Where( x => x.GetSetMethod() != null );

		// Check if we need to upgrade the core graph.
		if ( obj is ShaderGraphPlus )
		{
			if ( typeof( ISGPJsonUpgradeable ).IsAssignableFrom( type ) )
			{
				var propertyTypeInstance = EditorTypeLibrary.Create( type.Name, type );
				int oldVersionNumber = 0;

				// if we have a valid version then set oldVersionNumber otherwise just use a version of 0.
				if ( doc.TryGetProperty( VersioningInfo.VersionJsonPropertyName, out var versionElement ) )
				{
					oldVersionNumber = versionElement.GetInt32();
					SGPLog.Info( $"Got graph \"{type}\" upgradeable version \"{oldVersionNumber}\"", ConCommands.VerboseJsonUpgrader );
				}
				else
				{
					SGPLog.Info( $"Failed to get graph \"{type}\" upgradeable version. defaulting to \"0\"", ConCommands.VerboseJsonUpgrader );
				}

				// Dont even bother upgrading if we dont need to.
				if ( propertyTypeInstance is ISGPJsonUpgradeable upgradeable && oldVersionNumber < upgradeable.Version )
				{
					SGPLog.Info( $"Upgrading grapg \"{type}\" from version \"{oldVersionNumber}\" to \"{upgradeable.Version}\"", ConCommands.VerboseJsonUpgrader );

					var upgradedElement = UpgradeJsonUpgradeable( oldVersionNumber, upgradeable, type, doc, options );

					doc = upgradedElement;
				}
				else
				{
					SGPLog.Info( $"Graph \"{type}\" is already at the latest version :)", ConCommands.VerboseJsonUpgrader );
				}
			}
		}

		// Check if we need to upgrade any nodes :).
		if ( type.IsAssignableTo( typeof( BaseNodePlus ) ) )
		{
			if ( typeof( ISGPJsonUpgradeable ).IsAssignableFrom( type ) )
			{
				var propertyTypeInstance = EditorTypeLibrary.Create( type.Name, type );
				int oldVersionNumber = 0;

				// if we have a valid version then set oldVersionNumber otherwise just use a version of 0.
				if ( doc.TryGetProperty( VersioningInfo.VersionJsonPropertyName, out var versionElement ) )
				{
					oldVersionNumber = versionElement.GetInt32();
					SGPLog.Info( $"Got node \"{type}\" upgradeable version \"{oldVersionNumber}\"", ConCommands.VerboseJsonUpgrader );
				}
				else
				{
					SGPLog.Info( $"Failed to get node \"{type}\" upgradeable version. defaulting to \"0\"", ConCommands.VerboseJsonUpgrader );
				}
				
				// Dont even bother upgrading if we dont need to.
				if ( propertyTypeInstance is ISGPJsonUpgradeable upgradeable && oldVersionNumber < upgradeable.Version )
				{
					SGPLog.Info( $"Upgrading node \"{type}\" from version \"{oldVersionNumber}\" to \"{upgradeable.Version}\"", ConCommands.VerboseJsonUpgrader );
					
					var upgradedElement = UpgradeJsonUpgradeable( oldVersionNumber, upgradeable, type, doc, options );

					doc = upgradedElement;
				}
				else
				{
					SGPLog.Info( $"Node \"{type}\" is already at the latest version :)", ConCommands.VerboseJsonUpgrader );
				}
			}
		}

		// start deserilzing each property of the current type we are deserialzing. Also handle 
		// any property that needs upgrading.
		foreach ( var jsonProperty in doc.EnumerateObject() )
		{
			var propertyInfo = properties.FirstOrDefault( x =>
			{
				var propName = x.Name;
			
			
				if ( x.GetCustomAttribute<JsonPropertyNameAttribute>() is JsonPropertyNameAttribute jpna )
					propName = jpna.Name;
			
				return string.Equals( propName, jsonProperty.Name, StringComparison.OrdinalIgnoreCase );
			} );

			if ( propertyInfo == null )
				continue;

			if ( propertyInfo.CanWrite == false )
				continue;
			
			if ( propertyInfo.IsDefined( typeof( JsonIgnoreAttribute ) ) )
				continue;

			// Handle any types that use the ISGPJsonUpgradeable interface
			if ( typeof( ISGPJsonUpgradeable ).IsAssignableFrom( propertyInfo.PropertyType ) )
			{
				var propertyTypeInstance = EditorTypeLibrary.Create( propertyInfo.PropertyType.Name, propertyInfo.PropertyType );
				int oldVersionNumber = 0;

				// if we have a valid version then set oldVersionNumber otherwise just use a version of 0.
				if ( jsonProperty.Value.TryGetProperty( VersioningInfo.VersionJsonPropertyName, out var versionElement ) )
				{
					oldVersionNumber = versionElement.GetInt32();
					SGPLog.Info( $"Got property \"{type}\" upgradeable version \"{oldVersionNumber}\"", ConCommands.VerboseJsonUpgrader );
				}
				else
				{
					SGPLog.Info( $"Failed to get property \"{type}\" upgradeable version. defaulting to \"0\"", ConCommands.VerboseJsonUpgrader );
				}

				// Dont even bother upgrading if we dont need to.
				if ( propertyTypeInstance is ISGPJsonUpgradeable upgradeable && oldVersionNumber < upgradeable.Version )
				{
					SGPLog.Info( $"Upgrading property \"{type}\" from version \"{oldVersionNumber}\" to \"{upgradeable.Version}\"", ConCommands.VerboseJsonUpgrader );

					var upgradedElement = UpgradeJsonUpgradeable( oldVersionNumber, upgradeable, propertyInfo.PropertyType, jsonProperty, options );

					propertyInfo.SetValue( obj, JsonSerializer.Deserialize( upgradedElement.GetRawText(), propertyInfo.PropertyType, options ) );

					// Continue to the next jsonProperty :)
					continue;
				}
				else
				{
					SGPLog.Info( $"property \"{propertyInfo.PropertyType}\" is already at the latest version :)", ConCommands.VerboseJsonUpgrader );
				}
			}

			propertyInfo.SetValue( obj, JsonSerializer.Deserialize( jsonProperty.Value.GetRawText(), propertyInfo.PropertyType, options ) );
		}
	}

	private IEnumerable<BaseNodePlus> DeserializeNodes( JsonElement doc, JsonSerializerOptions options, string subgraphPath = null, int fileVersion = -1 )
	{
		var nodes = new Dictionary<string, BaseNodePlus>();
		var identifiers = _nodes.Count > 0 ? new Dictionary<string, string>() : null;
		var connections = new List<(IPlugIn Plug, NodeInput Value)>();
		var connectionFixupData = new List<NodeConnectionFixupData>();
		//var replacedNodes = new Dictionary<string, BaseNodePlus>();

		var arrayProperty = doc.GetProperty("nodes");
		foreach (var element in arrayProperty.EnumerateArray())
		{
			var typeName = element.GetProperty( "_class" ).GetString();

			// Use the new typename if applicable.
			if ( ProjectUpgrading.NodeTypeNameMapping.TryGetValue( typeName, out string newTypeName ) )
			{
				typeName = newTypeName;
			}

			var typeDesc = EditorTypeLibrary.GetType( typeName );
			var type = new ClassNodeType( typeDesc );

			BaseNodePlus node;
			if ( typeDesc is null )
			{
				var missingNode = new MissingNode( typeName, element );
				node = missingNode;
				DeserializeObject( node, element, options );
			}
			else
			{
				// Check if this is a legacy parameter node that should be upgraded to SubgraphInput
				// Only upgrade for old subgraph files (files without Version property aka. 0 -> 1)
				// TODO : Get a similar system setup to how SGPJsonUpgrader works or just make it work with SGPJsonUpgrader somehow? - Quack.
				if ( IsSubgraph && fileVersion < 1 && ShouldUpgradeToSubgraphInput( typeName, element ) )
				{
					node = CreateUpgradedSubgraphInput( typeName, element, options );
					fileVersion = 1; // We've upgraded now, useful if there are future upgraders.
				}
				else if ( IsSubgraph && fileVersion < 2 && typeName == "SubgraphOutput" )
				{
					node = UpdateSubgraphOutput( element, options );
					fileVersion = 2;
				}
				else if ( fileVersion < 3 && ShouldConvertParameterNodeToConstant( typeName, element ) )
				{
					SGPLog.Info( $"Converting Unnamed Parameter node {typeName} to a constant node." );

					node = ConvertToConstantNode( typeName, element, options );
				}
				else if ( fileVersion < 3 && ShouldConvertTextureNodes( typeName, element ) )
				{
					node = ConvertToNewTextureSampleNode( typeName, element, options, out var connectionFixupDataNew );

					connectionFixupData.AddRange( connectionFixupDataNew );
				}
				else // Nothing to upgrade.
				{
					node = EditorTypeLibrary.Create<BaseNodePlus>( typeName );
					DeserializeObject( node, element, options );
				}
				
				if ( identifiers != null && _nodes.ContainsKey( node.Identifier ) )
				{
					identifiers.Add( node.Identifier, node.NewIdentifier() );
				}

				if ( node is IInitializeNode initializeableNode )
				{
					initializeableNode.InitializeNode();
				}

				if ( node is SubgraphNode subgraphNode )
				{
					if ( !Editor.FileSystem.Content.FileExists( subgraphNode.SubgraphPath ) )
					{
						var missingNode = new MissingNode( typeName, element );
						node = missingNode;
						DeserializeObject( node, element, options );
					}
					else
					{
						subgraphNode.OnNodeCreated();
					}
				}

				foreach ( var input in node.Inputs )
				{
					if ( !element.TryGetProperty( input.Identifier, out var connectedElem ) )
						continue;

					var connected = connectedElem
						.Deserialize<NodeInput?>();

					if ( connected is { IsValid: true } )
					{
						var connection = connected.Value;
						if ( !string.IsNullOrEmpty( subgraphPath ) )
						{
							connection = new()
							{
								Identifier = connection.Identifier,
								Output = connection.Output,
								Subgraph = subgraphPath
							};
						}

						connections.Add( (input, connection) );
					}
				}
	
				if ( !node.UpgradedToNewNode )
				{
					nodes.Add( node.Identifier, node );

					AddNode( node );
				}
			}
		}

		foreach ( var (input, value) in connections )
		{
			var outputIdent = identifiers?.TryGetValue( value.Identifier, out var newIdent ) ?? false
				? newIdent : value.Identifier;

			if ( nodes.TryGetValue( outputIdent, out var node ) )
			{
				var output = node.Outputs.FirstOrDefault( x => x.Identifier == value.Output );

				// Uprgraded node but the NodeResult property name on the new node output differs from the old one.
				// Bit of a hack really.
				//if ( replacedNodes.TryGetValue( node.Identifier, out var oldNode ) && output is null )
				//{
				//	ProjectUpgrading.ReplaceOutputReference( node, oldNode, value.Output, ref output );
				//}

				if ( output is null )
				{
					// Check for Aliases
					foreach ( var op in node.Outputs )
					{
						if ( op is not BasePlugOut plugOut ) continue;

						var aliasAttr = plugOut.Info.Property?.GetCustomAttribute<AliasAttribute>();
						if ( aliasAttr is not null && aliasAttr.Value.Contains( value.Output ) )
						{
							output = plugOut;
							break;
						}
					}
				}
				input.ConnectedOutput = output;
			}
		}
		
		// Fixup any broken connections for any node that was "upgraded".
		// Though in some cases it may not work when the node we are
		// connecting from has itself been "upgraded".
		foreach ( var data in connectionFixupData )
		{
			if ( data.NodeInputs == null )
				continue;

			foreach ( var mapping in data.NodeInputs )
			{
				var nodeFrom = FindNode( mapping.Key.Identifier );

				if ( nodeFrom != null )
				{
					SGPLog.Warning( $"Trying to connect \"{mapping.Key.Output}\" from node \"{nodeFrom}\" to \"{mapping.Value}\"" );

					data.NodeToConnectTo.Graph = this;
					data.NodeToConnectTo.ConnectNode( mapping.Value, mapping.Key.Output, mapping.Key.Identifier );
				}
				else
				{
					SGPLog.Error( $"Could not find node with identifier \"{mapping.Key.Identifier}\"" );
				}
			}
		}

		return nodes.Values;
	}

	public string SerializeNodes()
	{
		return SerializeNodes( Nodes );
	}

	public string UndoStackSerialize()
	{
		var doc = new JsonObject();
		var options = SerializerOptions();

		doc = SerializeNodes( Nodes, doc );

		return SerializeParameters( Parameters, doc ).ToJsonString( options );
	}

	public string SerializeNodes( IEnumerable<BaseNodePlus> nodes )
	{
		var doc = new JsonObject();
		var options = SerializerOptions();

		SerializeNodes( nodes, doc, options );

		return doc.ToJsonString( options );
	}

	public JsonObject SerializeNodes( IEnumerable<BaseNodePlus> nodes, JsonObject doc )
	{
		var options = SerializerOptions();

		SerializeNodes( nodes, doc, options );

		return doc;
	}

	private static void SerializeObject( object obj, JsonObject doc, JsonSerializerOptions options, Dictionary<string, string> identifiers = null )
	{
		var type = obj.GetType();
		var properties = type.GetProperties( BindingFlags.Instance | BindingFlags.Public )
			.Where( x => x.GetSetMethod() != null );

		if ( obj is ShaderGraphPlus sgp && sgp is ISGPJsonUpgradeable iupgradeable )
		{
			doc.Add( VersioningInfo.VersionJsonPropertyName, JsonSerializer.SerializeToNode( iupgradeable.Version, options ) );
		}

		foreach ( var property in properties )
		{
			if ( !property.CanRead )
				continue;

			if ( property.PropertyType == typeof( NodeInput ) )
				continue;
	
			if ( property.IsDefined( typeof( JsonIgnoreAttribute ) ) )
				continue;
	
			var propertyName = property.Name;
			if ( property.GetCustomAttribute<JsonPropertyNameAttribute>() is { } jpna )
				propertyName = jpna.Name;
	
			var propertyValue = property.GetValue( obj );
			if ( propertyName == "Identifier" && propertyValue is string identifier )
			{
				if ( identifiers.TryGetValue(identifier, out var newIdentifier ) )
				{
					propertyValue = newIdentifier;
				}
			}

			if ( propertyName != "Version" )
			{
				doc.Add( propertyName, JsonSerializer.SerializeToNode( propertyValue, options ) );
			}

			if ( propertyValue is ISGPJsonUpgradeable upgradeable )
			{
				//doc.Add( VersioningInfo.VersionJsonPropertyName, JsonSerializer.SerializeToNode( upgradeable.Version, options ) );
			}
		}

		if ( obj is IGraphNode node )
		{
			foreach ( var input in node.Inputs )
			{
				if ( input.ConnectedOutput is not { } output )
					continue;

				doc.Add( input.Identifier, JsonSerializer.SerializeToNode( new NodeInput
				{
					Identifier = identifiers?.TryGetValue( output.Node.Identifier, out var newIdent ) ?? false ? newIdent : output.Node.Identifier,
					Output = output.Identifier,
				} ) );
			}
		}
	}

	private static void SerializeNodes( IEnumerable<BaseNodePlus> nodes, JsonObject doc, JsonSerializerOptions options )
	{
		var identifiers = new Dictionary<string, string>();
		foreach ( var node in nodes )
		{
			identifiers.Add( node.Identifier, $"{identifiers.Count}" );
		}

		var nodeArray = new JsonArray();

		foreach ( var node in nodes )
		{
			if ( node is DummyNode )
				continue;

			var type = node.GetType();
			var nodeObject = new JsonObject { { "_class", type.Name }, { VersioningInfo.VersionJsonPropertyName, node.Version } };

			//if ( node is ISGPJsonUpgradeable upgradeable )
			//{
			//	nodeObject.Add( VersioningInfo.JsonPropertyName, upgradeable.Version );
			//}

			SerializeObject( node, nodeObject, options, identifiers );

			nodeArray.Add( nodeObject );
		}

		doc.Add( "nodes", nodeArray );
	}

	public string SerializeParameters()
	{
		return SerializeParameters( Parameters );
	}

	private string SerializeParameters( IEnumerable<BaseBlackboardParameter> parameters )
	{
		var doc = new JsonObject();
		var options = SerializerOptions();

		SerializeParameters( parameters, doc, options );

		return doc.ToJsonString( options );
	}

	private JsonObject SerializeParameters( IEnumerable<BaseBlackboardParameter> parameters, JsonObject doc )
	{
		var options = SerializerOptions();

		SerializeParameters( parameters, doc, options );

		return doc;
	}

	private static void SerializeParameters( IEnumerable<BaseBlackboardParameter> parameters, JsonObject doc, JsonSerializerOptions options )
	{
		var parameterArray = new JsonArray();

		foreach ( var parameter in parameters )
		{
			var type = parameter.GetType();
			var parameterObject = new JsonObject { { "_class", type.Name } };

			SerializeObject( parameter, parameterObject, options );

			parameterArray.Add( parameterObject );
		}

		doc.Add( "parameters", parameterArray );
	}

	private void GraphV3Upgrade()
	{
		if ( IsSubgraph )
		{
			foreach ( var subgraphInput in Nodes.OfType<SubgraphInput>() )
			{
				if ( string.IsNullOrWhiteSpace( subgraphInput.InputName ) )
					continue;

				BaseBlackboardParameter blackboardParameter = null;

				if ( subgraphInput.InputData.InputType == SubgraphPortType.Bool )
				{
					blackboardParameter = new BoolSubgraphInputParameter()
					{
						Name = subgraphInput.InputName,
						Description = subgraphInput.InputDescription,
						Value = subgraphInput.InputData.GetValue<bool>(),
						IsRequired = subgraphInput.IsRequired,
						PortOrder = subgraphInput.PortOrder,
					};
				}
				else if ( subgraphInput.InputData.InputType == SubgraphPortType.Int )
				{
					blackboardParameter = new IntSubgraphInputParameter()
					{
						Name = subgraphInput.InputName,
						Description = subgraphInput.InputDescription,
						Value = subgraphInput.InputData.GetValue<int>(),
						IsRequired = subgraphInput.IsRequired,
						PortOrder = subgraphInput.PortOrder,
					};
				}
				else if ( subgraphInput.InputData.InputType == SubgraphPortType.Float )
				{
					blackboardParameter = new FloatSubgraphInputParameter()
					{
						Name = subgraphInput.InputName,
						Description = subgraphInput.InputDescription,
						Value = subgraphInput.InputData.GetValue<float>(),
						IsRequired = subgraphInput.IsRequired,
						PortOrder = subgraphInput.PortOrder,
					};
				}
				else if ( subgraphInput.InputData.InputType == SubgraphPortType.Vector2 )
				{
					blackboardParameter = new Float2SubgraphInputParameter()
					{
						Name = subgraphInput.InputName,
						Description = subgraphInput.InputDescription,
						Value = subgraphInput.InputData.GetValue<Vector2>(),
						IsRequired = subgraphInput.IsRequired,
						PortOrder = subgraphInput.PortOrder,
					};
				}
				else if ( subgraphInput.InputData.InputType == SubgraphPortType.Vector3 )
				{
					blackboardParameter = new Float3SubgraphInputParameter()
					{
						Name = subgraphInput.InputName,
						Description = subgraphInput.InputDescription,
						Value = subgraphInput.InputData.GetValue<Vector3>(),
						IsRequired = subgraphInput.IsRequired,
						PortOrder = subgraphInput.PortOrder,
					};
				}
				else if ( subgraphInput.InputData.InputType == SubgraphPortType.Color )
				{
					blackboardParameter = new ColorSubgraphInputParameter()
					{
						Name = subgraphInput.InputName,
						Description = subgraphInput.InputDescription,
						Value = subgraphInput.InputData.GetValue<Color>(),
						IsRequired = subgraphInput.IsRequired,
						PortOrder = subgraphInput.PortOrder,
					};
				}

				subgraphInput.BlackboardParameterIdentifier = blackboardParameter.Identifier;

				AddParameter( blackboardParameter );
			}
		}
		else
		{
			foreach ( var parameterNode in Nodes.OfType<IParameterNode>() )
			{
				if ( string.IsNullOrWhiteSpace( parameterNode.Name ) )
					continue;

				BaseBlackboardParameter blackboardParameter = null;

				if ( parameterNode is IntParameterNode intNode )
				{
					blackboardParameter = new IntParameter()
					{
						Name = intNode.Name,
						Value = intNode.Value,
						UI = intNode.UI,
						IsAttribute = intNode.IsAttribute,
					};

					intNode.BlackboardParameterIdentifier = blackboardParameter.Identifier;
				}
				else if ( parameterNode is BoolParameterNode boolNode )
				{
					blackboardParameter = new BoolParameter()
					{
						Name = boolNode.Name,
						Value = boolNode.Value,
						UI = boolNode.UI,
						IsAttribute = boolNode.IsAttribute,
					};

					boolNode.BlackboardParameterIdentifier = blackboardParameter.Identifier;
				}
				else if ( parameterNode is FloatParameterNode floatNode )
				{
					blackboardParameter = new FloatParameter()
					{
						Name = floatNode.Name,
						Value = floatNode.Value,
						UI = floatNode.UI,
						IsAttribute = floatNode.IsAttribute,
					};

					floatNode.BlackboardParameterIdentifier = blackboardParameter.Identifier;
				}
				else if ( parameterNode is Float2ParameterNode float2Node )
				{
					blackboardParameter = new Float2Parameter()
					{
						Name = float2Node.Name,
						Value = float2Node.Value,
						UI = float2Node.UI,
						IsAttribute = float2Node.IsAttribute,
					};

					float2Node.BlackboardParameterIdentifier = blackboardParameter.Identifier;

				}
				else if ( parameterNode is Float3ParameterNode float3Node )
				{
					blackboardParameter = new Float3Parameter()
					{
						Name = float3Node.Name,
						Value = float3Node.Value,
						UI = float3Node.UI,
						IsAttribute = float3Node.IsAttribute,
					};

					float3Node.BlackboardParameterIdentifier = blackboardParameter.Identifier;
				}
				else if ( parameterNode is ColorParameterNode colorNode )
				{
					blackboardParameter = new ColorParameter()
					{
						Name = colorNode.Name,
						Value = colorNode.Value,
						UI = colorNode.UI,
						IsAttribute = colorNode.IsAttribute,
					};

					colorNode.BlackboardParameterIdentifier = blackboardParameter.Identifier;
				}

				AddParameter( blackboardParameter );
			}
		}
	}

	private bool ShouldConvertParameterNodeToConstant( string typeName, JsonElement element )
	{
		// Only upgrade if it's a parameter node type
		if ( !IsParameterNodeTypeToConvertToConstant( typeName ) )
			return false;

		// Only convert if it dosent have a name (indicating it's meant to be a constant value)
		if ( element.TryGetProperty( "Name", out var nameProperty ) )
		{
			var name = nameProperty.GetString();

			return string.IsNullOrWhiteSpace( name );
		}

		// No "Name" property? assume its ment to be a constant.
		return true;
	}

	private bool ShouldConvertTextureNodes( string typeName, JsonElement element )
	{
		if ( !IsOldTextureSamplerNodeType( typeName ) )
			return false;

		return true;
	}

	private static bool IsOldTextureSamplerNodeType( string typeName )
	{
		return typeName switch
		{
			"TextureCubeObjectNode" => true,
			"Texture2DObjectNode" => true,
			"TextureSampler" => true,
			"TextureTriplanar" => true,
			"NormalMapTriplanar" => true,
			"TextureCube" => true,
			_ => false
		};
	}

	private static bool IsParameterNodeTypeToConvertToConstant( string typeName )
	{
		return typeName switch
		{
			"BoolParameterNode" => true,
			"IntParameterNode" => true,
			"FloatParameterNode" => true,
			"Float2ParameterNode" => true,
			"Float3ParameterNode" => true,
			"ColorParameterNode" => true,
			_ => false
		};
	}

	private BaseNodePlus InitNewTextureSamplerNode( BaseNodePlus newSamplerNode, JsonElement element,
		out NodeConnectionFixupData connectionFixupData 
	)
	{
		connectionFixupData = new();
		if ( newSamplerNode.Graph == null )
		{
			newSamplerNode.Graph = this;
		}

		var imageProperty = element.GetProperty( "Image" );
		var textureUI = new TextureInput();
		if ( newSamplerNode is Texture2DSamplerBase samplerBase )
		{
			textureUI = samplerBase.UI with { DefaultTexture = imageProperty.GetString() };
			samplerBase.Image = imageProperty.GetString();
		}

		if ( !string.IsNullOrWhiteSpace( textureUI.Name ) )
		{
			BaseNodePlus newNode01 = default;
			if ( IsSubgraph )
			{
	
			}
			else
			{
				newNode01 = new Texture2DParameterNode()
				{
					Position = new Vector2( newSamplerNode.Position.x - 192, newSamplerNode.Position.y )
				};
			}

			BaseBlackboardParameter parameter = default;
			foreach ( var param in Parameters )
			{
				if ( param.Name == textureUI.Name )
				{
					parameter = param;
					break;
				}
			}
		
			if ( parameter == null )
			{
				if ( IsSubgraph )
				{
					parameter = new Texture2DSubgraphInputParameter()
					{
						Name = textureUI.Name,
						Value = textureUI with { DefaultTexture = imageProperty.GetString() }
					};

					AddParameter( parameter );
				}
				else
				{
					parameter = new Texture2DParameter()
					{
						Name = textureUI.Name,
						Value = textureUI with { DefaultTexture = imageProperty.GetString() }
					};

					AddParameter( parameter );
				}

			}

			if ( IsSubgraph )
			{

			}
			else
			{
				var parameterNode = newNode01 as Texture2DParameterNode;

				parameterNode.BlackboardParameterIdentifier = parameter.Identifier;
				parameterNode.Name = textureUI.Name;
				parameterNode.UI = textureUI;

				AddNode( parameterNode );

				newSamplerNode.ConnectNode(
					nameof( SampleTexture2DNode.Texture2DInput ),
					nameof( Texture2DParameterNode.Result ),
					parameterNode.Identifier
				);
			}
		}

		if ( newSamplerNode is Texture2DSamplerBase )
		{
			var connectionsToFix = new Dictionary<NodeInput, string>();
			if ( element.TryGetProperty( "Coords", out var coordsProp ) ) //&& element.TryGetProperty( "Sampler", out var samplerProp ) )
			{
				connectionsToFix.Add( JsonSerializer.Deserialize<NodeInput>( coordsProp, SerializerOptions() ), "CoordsInput" );
			}
			if ( element.TryGetProperty( "Sampler", out var samplerProp ) )
			{
				connectionsToFix.Add( JsonSerializer.Deserialize<NodeInput>( samplerProp, SerializerOptions() ), "SamplerInput" );
			}

			connectionFixupData = new NodeConnectionFixupData( connectionsToFix, newSamplerNode );
		}

		return newSamplerNode;
	}

	private BaseNodePlus ConvertToNewTextureSampleNode( string typeName, JsonElement element, JsonSerializerOptions options, out NodeConnectionFixupData connectionFixupData )
	{
		connectionFixupData = new();

		switch ( typeName )
		{
			case "Texture2DObjectNode":
				BaseNodePlus newNode0 = default;
				var uiProp = element.GetProperty( "UI" );
				var textureInput = JsonSerializer.Deserialize<TextureInput>( uiProp, SerializerOptions() );
				var newUI1 = textureInput with { Name = textureInput.Name, Type = TextureType.Tex2D };
				BaseBlackboardParameter blackboardParameter1 = default;

				if ( IsSubgraph )
				{
					blackboardParameter1 = new Texture2DSubgraphInputParameter()
					{
						Name = textureInput.Name,
						Value = newUI1
					};
				}
				else
				{
					blackboardParameter1 = new Texture2DParameter()
					{
						Name = textureInput.Name,
						Value = newUI1
					};
				}

				if ( IsSubgraph )
				{
					newNode0 = new SubgraphInput()
					{
						BlackboardParameterIdentifier = blackboardParameter1.Identifier,
						InputName = textureInput.Name,
						InputData = new VariantValueTexture2D( textureInput, SubgraphPortType.Texture2DObject )
					};
				}
				else
				{
					var tex2DParamNode = new Texture2DParameterNode();

					tex2DParamNode.BlackboardParameterIdentifier = blackboardParameter1.Identifier;
					tex2DParamNode.Name = textureInput.Name;
					AddParameter( blackboardParameter1 );

					newNode0 = tex2DParamNode;
				}

				// Copy basic node properties
				DeserializeObject( newNode0, element, options );

				return newNode0;
			case "TextureCubeObjectNode":
				var newNode1 = new TextureCubeParameterNode();

				// Copy basic node properties
				DeserializeObject( newNode1, element, options );

				var newUI2 = newNode1.UI with { Name = newNode1.UI.Name, Type = TextureType.TexCube };

				var blackboardParameter2 = new TextureCubeParameter()
				{
					Name = newNode1.UI.Name,
					Value = newUI2
				};

				newNode1.BlackboardParameterIdentifier = blackboardParameter2.Identifier;
				newNode1.Name = newNode1.UI.Name;

				AddParameter( blackboardParameter2 );

				return newNode1;
			case "TextureSampler":
				var newNode2 = new SampleTexture2DNode();

				// Copy basic node properties
				DeserializeObject( newNode2, element, options );

				return InitNewTextureSamplerNode( newNode2, element, out connectionFixupData );
			case "TextureTriplanar":
				var newNode3 = new SampleTexture2DTriplanarNode();

				// Copy basic node properties
				DeserializeObject( newNode3, element, options );

				return InitNewTextureSamplerNode( newNode3, element, out connectionFixupData );
			case "NormalMapTriplanar":
				var newNode4 = new SampleTexture2DNormalMapTriplanarNode();

				// Copy basic node properties
				DeserializeObject( newNode4, element, options );

				return InitNewTextureSamplerNode( newNode4, element, out connectionFixupData );
			case "TextureCube":
				var newNode5 = new SampleTextureCubeNode();

				// Copy basic node properties
				DeserializeObject( newNode5, element, options );

				if ( newNode5.Graph == null )
				{
					newNode5.Graph = this;
				}

				var textureProperty = element.GetProperty( "Texture" );
				var textureUI = newNode5.UI with { DefaultTexture = textureProperty.GetString(), Type = TextureType.TexCube };
				newNode5.Texture = textureProperty.GetString();

				return newNode5;
		}

		throw new Exception( $"Could not convert \"{typeName}\" to new TextureSampler node!" );
	}

	private BaseNodePlus ConvertToConstantNode( string typeName, JsonElement element, JsonSerializerOptions options )
	{
		if ( element.TryGetProperty( "Value", out var parameterValueElement ) )
		{
			// Map the parameter type to InputType and set default values
			switch ( typeName )
			{
				case "BoolParameterNode":
					var newNode1 = new BoolConstantNode() 
					{ 
						Value = parameterValueElement.GetBoolean()
					};

					// Copy basic node properties
					DeserializeObject( newNode1, element, options );
					return newNode1;
				case "IntParameterNode":
					var newNode2 = new IntConstantNode()
					{
						Value = parameterValueElement.GetInt32()
					};

					// Copy basic node properties
					DeserializeObject( newNode2, element, options );
					return newNode2;
				case "FloatParameterNode":
					var newNode3 = new FloatConstantNode()
					{
						Value = parameterValueElement.GetSingle()
					};

					// Copy basic node properties
					DeserializeObject( newNode3, element, options );
					return newNode3;
				case "Float2ParameterNode":
					var vector2 = JsonSerializer.Deserialize<Vector2>( parameterValueElement.GetRawText(), options );
					var newNode4 = new Float2ConstantNode() 
					{ 
						Value = vector2
					};

					// Copy basic node properties
					DeserializeObject( newNode4, element, options );
					return newNode4;
				case "Float3ParameterNode":
					var vector3 = JsonSerializer.Deserialize<Vector3>( parameterValueElement.GetRawText(), options );
					var newNode5 = new Float3ConstantNode()
					{
						Value = vector3
					};

					// Copy basic node properties
					DeserializeObject( newNode5, element, options );
					return newNode5;
				case "ColorParameterNode":
					var color = JsonSerializer.Deserialize<Color>( parameterValueElement.GetRawText(), options );
					var newNode6 = new ColorConstantNode()
					{
						Value = color
					};

					// Copy basic node properties
					DeserializeObject( newNode6, element, options );

					return newNode6;
			}

			throw new Exception( "Couldnt convert nameless Parameter node to Constant node" );
		}

		throw new Exception( "Couldnt convert nameless Parameter node to Constant node" );
	}

#region OldStuffToRemove
	/// <summary>
	/// Check if a legacy parameter node should be upgraded to SubgraphInput.
	/// </summary>
	private static bool ShouldUpgradeToSubgraphInput( string typeName, JsonElement element )
	{
		// Only upgrade if it's a parameter node type
		if ( !IsParameterNodeType( typeName ) )
			return false;

		// Only upgrade if it has a name (indicating it's meant to be an input)
		if ( element.TryGetProperty( "Name", out var nameProperty ) )
		{
			var name = nameProperty.GetString();
			return !string.IsNullOrWhiteSpace( name );
		}

		return false;
	}

	/// <summary>
	/// Check if the type name represents a parameter node
	/// </summary>
	private static bool IsParameterNodeType( string typeName )
	{
		return typeName switch
		{
			"BoolParameterNode" => true,
			"IntParameterNode" => true,
			"FloatParameterNode" => true,
			"Float2ParameterNode" => true,
			"Float3ParameterNode" => true,
			"ColorParameterNode" => true,
			"TextureSampler" => true,
			"Texture2DObjectNode" => true,
			"SamplerNode" => true,
			_ => false
		};
	}

	/// <summary>
	/// Create a new SubgraphInput node from a legacy parameter node
	/// </summary>
	private SubgraphInput CreateUpgradedSubgraphInput( string typeName, JsonElement element, JsonSerializerOptions options )
	{
		var subgraphInput = new SubgraphInput();

		// Copy basic node properties
		DeserializeObject( subgraphInput, element, options );

		// Set input name from the parameter's Name property
		if ( element.TryGetProperty( "Name", out var nameProperty ) )
		{
			subgraphInput.InputName = nameProperty.GetString();
		}

		// Map the parameter type to InputType and set default values
		switch ( typeName )
		{
			case "BoolParameterNode":
				subgraphInput.InputData.InputType = SubgraphPortType.Bool;
				if ( element.TryGetProperty( "Value", out var boolValue ) )
				{
					subgraphInput.InputData = new VariantValueBool( boolValue.GetBoolean(), SubgraphPortType.Bool );
				}
				break;
			case "IntParameterNode":
				subgraphInput.InputData.InputType = SubgraphPortType.Int;
				if ( element.TryGetProperty( "Value", out var intValue ) )
				{
					subgraphInput.InputData = new VariantValueInt( intValue.GetInt32(), SubgraphPortType.Int );
				}
				break;
			case "FloatParameterNode":
				subgraphInput.InputData.InputType = SubgraphPortType.Float;
				if ( element.TryGetProperty( "Value", out var floatValue ) )
				{
					subgraphInput.InputData = new VariantValueFloat( floatValue.GetSingle(), SubgraphPortType.Float );
				}
				break;
			case "Float2ParameterNode":
				subgraphInput.InputData.InputType = SubgraphPortType.Vector2;
				if ( element.TryGetProperty( "Value", out var float2Value ) )
				{
					var vector2 = JsonSerializer.Deserialize<Vector2>( float2Value.GetRawText(), options );
					subgraphInput.InputData = new VariantValueVector2( vector2, SubgraphPortType.Vector2 );
				}
				break;
			case "Float3ParameterNode":
				subgraphInput.InputData.InputType = SubgraphPortType.Vector3;
				if ( element.TryGetProperty( "Value", out var float3Value ) )
				{
					var vector3 = JsonSerializer.Deserialize<Vector3>( float3Value.GetRawText(), options );
					subgraphInput.InputData = new VariantValueVector3( vector3, SubgraphPortType.Vector3 );
				}
				break;
			case "ColorParameterNode":
				subgraphInput.InputData.InputType = SubgraphPortType.Color;
				if ( element.TryGetProperty( "Value", out var ColorValue ) )
				{
					var color = JsonSerializer.Deserialize<Color>( ColorValue.GetRawText(), options );
					subgraphInput.InputData = new VariantValueColor( color, SubgraphPortType.Color );
				}
				break;
			case "Texture2DObjectNode":
				subgraphInput.InputData.InputType = SubgraphPortType.Texture2DObject;
				if ( element.TryGetProperty( "UI", out var TextureInputValue ) )
				{
					var vector4 = JsonSerializer.Deserialize<Vector4>( TextureInputValue.GetRawText(), options );
					subgraphInput.InputData = new VariantValueColor( vector4, SubgraphPortType.Texture2DObject );
				}
				break;
			case "SamplerNode":
				subgraphInput.InputData.InputType = SubgraphPortType.SamplerState;
				if ( element.TryGetProperty( "SamplerState", out var SamplerStateValue ) )
				{
					var samplerState = JsonSerializer.Deserialize<Sampler>( SamplerStateValue.GetRawText(), options );
					subgraphInput.InputData = new VariantValueSamplerState( samplerState, SubgraphPortType.SamplerState );
				}
				break;
		}

		return subgraphInput;
	}

	private SubgraphOutput UpdateSubgraphOutput( JsonElement element, JsonSerializerOptions options )
	{
		var subgraphOutput = new SubgraphOutput();

		// Copy basic node properties
		DeserializeObject( subgraphOutput, element, options );

		if ( element.TryGetProperty( "SubgraphFunctionOutput", out var subgraphFunctionOutputProperty ) )
		{
			if ( subgraphFunctionOutputProperty.TryGetProperty( "Id", out var id ) )
			{
				subgraphOutput.OutputIdentifier = id.GetGuid();
			}
			if ( subgraphFunctionOutputProperty.TryGetProperty( "OutputName", out var outputName ) )
			{
				subgraphOutput.OutputName = outputName.GetString();
			}
			if ( subgraphFunctionOutputProperty.TryGetProperty( "OutputDescription", out var outputDescription ) )
			{
				subgraphOutput.OutputDescription = outputDescription.GetString();
			}
			if ( subgraphFunctionOutputProperty.TryGetProperty( "OutputType", out var outputType ) )
			{
				subgraphOutput.OutputType = JsonSerializer.Deserialize<SubgraphPortType>( outputType, options );
			}
			if ( subgraphFunctionOutputProperty.TryGetProperty( "Preview", out var preview ) )
			{
				subgraphOutput.Preview = JsonSerializer.Deserialize<SubgraphOutputPreviewType>( preview, options );
			}
			if ( subgraphFunctionOutputProperty.TryGetProperty( "PortOrder", out var portOrder ) )
			{
				subgraphOutput.PortOrder = portOrder.GetInt32();
			}
		}

		return subgraphOutput;
	}
#endregion OldStuffToRemove
}
