using ShaderGraphPlus.Nodes;
using System.Text.Json.Nodes;

namespace ShaderGraphPlus;

public interface ISGPJsonUpgradeable
{
	[Hide]
	public int Version { get; }
}

partial class ShaderGraphPlus
{
	[JsonIgnore, Hide]
	internal bool Upgrade { get; set; } = false;

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

		doc.Add( VersioningInfo.VersionJsonPropertyName, Version );

		SerializeObject( this, doc, options );
		SerializeNodes( Nodes, doc, options );

		return doc.ToJsonString( options );
	}

	public void Deserialize( string json, string subgraphPath = null )
	{
		using var doc = JsonDocument.Parse( json );
		var root = doc.RootElement;
		var options = SerializerOptions();

		var projectFileVersion = GetProjectVersion( root );
		if ( projectFileVersion < Version )
		{
			//SGPLog.Info( $"Version of loading Project is \"{projectFileVersion}\" which is less than the defined version \"{Version}\"." );
		}
		
		DeserializeObject( this, root, options );
		DeserializeNodes( root, options, subgraphPath );
	}

	public static int GetProjectVersion( JsonElement root )
	{
		if ( root.TryGetProperty( VersioningInfo.VersionJsonPropertyName, out var versionElement ) && versionElement.TryGetInt32( out var versionNum ) )
		{
			return versionNum;
		}
		else // Older Projects that dont contain a listed __version property
		{
			return 0;
		}
	}

	public IEnumerable<BaseNodePlus> DeserializeNodes( string json )
	{
		using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
		return DeserializeNodes(doc.RootElement, SerializerOptions());
	}

	public static JsonElement UpgradeJsonUpgradeable( int versionNumber, ISGPJsonUpgradeable jsonUpgradeable, Type type, JsonProperty jsonProperty, JsonSerializerOptions serializerOptions )
	{
		var jsonObject = JsonNode.Parse( jsonProperty.Value.GetRawText() ) as JsonObject;

		SGPJsonUpgrader.Upgrade( versionNumber, jsonObject, type );

		return JsonSerializer.Deserialize<JsonElement>( jsonObject.ToJsonString(), serializerOptions );
	}

	private static void DeserializeObject( object obj, JsonElement doc, JsonSerializerOptions options )
	{
		var type = obj.GetType();
		var properties = type.GetProperties( BindingFlags.Instance | BindingFlags.Public )
			.Where( x => x.GetSetMethod() != null );

		//if ( obj is BaseNodePlus baseNode )
		//{
		//	SGPLog.Info( $"Deserializing BaseNodePlus object \"{type}\"" );
		//}
		//else
		//{
		//	SGPLog.Info( $"Deserializing object \"{type}\"" );
		//}

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
				}

				//SGPLog.Info( $"Got \"{propertyInfo.PropertyType}\" upgradeable version \"{oldVersionNumber}\"" );

				// Dont even bother upgrading if we dont need to.
				if ( propertyTypeInstance is ISGPJsonUpgradeable upgradeable && oldVersionNumber < upgradeable.Version )
				{
					var upgradedElement = UpgradeJsonUpgradeable( oldVersionNumber, upgradeable, propertyInfo.PropertyType, jsonProperty, options );

					propertyInfo.SetValue( obj, JsonSerializer.Deserialize( upgradedElement.GetRawText(), propertyInfo.PropertyType, options ) );

					// Continue to the next jsonProperty :)
					continue;
				}

				//SGPLog.Info( $"\"{propertyInfo.PropertyType}\" is already at the latest version :)" );
			}

			propertyInfo.SetValue( obj, JsonSerializer.Deserialize( jsonProperty.Value.GetRawText(), propertyInfo.PropertyType, options ) );
		}
	}

	private IEnumerable<BaseNodePlus> DeserializeNodes( JsonElement doc, JsonSerializerOptions options, string subgraphPath = null )
	{
		var nodes = new Dictionary<string, BaseNodePlus>();
		var identifiers = _nodes.Count > 0 ? new Dictionary<string, string>() : null;
		var connections = new List<(IPlugIn Plug, NodeInput Value)>();
		var replacedNodes = new Dictionary<string, BaseNodePlus>();

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
				node = EditorTypeLibrary.Create<BaseNodePlus>( typeName );
				var replaceAttribute = node.GetType().GetCustomAttribute<NodeReplaceAttribute>();
				DeserializeObject( node, element, options );

				if ( identifiers != null && _nodes.ContainsKey( node.Identifier ) )
				{
					identifiers.Add( node.Identifier, node.NewIdentifier() );
				}

				// Replace named IParameter nodes with a SubgraphInput node.
				//if ( node is IParameterNode parameterNode && !string.IsNullOrWhiteSpace( parameterNode.Name ) && IsSubgraph )
				//{
				//	node.UpgradedToNewNode = true;
				//
				//	var subgraphInputNode = parameterNode.UpgradeToSubgraphInput();
				//	subgraphInputNode.Position = parameterNode.ParameterNodePosition;
				//	
				//	// Take the Identifier of the node that we are replacing.
				//	subgraphInputNode.Identifier = node.Identifier;
				//
				//	nodes.Add( subgraphInputNode.Identifier, subgraphInputNode );
				//	AddNode( subgraphInputNode );
				//}

				if ( replaceAttribute != null && node is IReplaceNode iReplaceNode && iReplaceNode.ReplacementCondition )
				{
					if ( replaceAttribute.Mode == ReplacementMode.SubgraphOnly && IsSubgraph )
					{
						node.UpgradedToNewNode = true;
						var newNode = iReplaceNode.GetReplacementNode();
						newNode.Position = node.Position;

						// Take the Identifier of the node that we are replacing.
						newNode.Identifier = node.Identifier;

						//SGPLog.Info( $"Upgraded subgraph node \"{node}\" to \"{newNode}\"" );

						//foreach ( var plugOut in node.Outputs )
						//{
						//	SGPLog.Info( $"node output  \"{plugOut.Node}\"" );
						//}

						SGPLog.Info( $"Upgraded node \"{node}\" in subgraph only to \"{newNode}\"" );

						replacedNodes.Add( node.Identifier, node );
						nodes.Add( newNode.Identifier, newNode );
						AddNode( newNode );
					}
					else if ( replaceAttribute.Mode == ReplacementMode.Both  )
					{
						node.UpgradedToNewNode = true;
						var newNode = iReplaceNode.GetReplacementNode();
						newNode.Position = node.Position;

						// Take the Identifier of the node that we are replacing.
						newNode.Identifier = node.Identifier;

						SGPLog.Info( $"Upgraded node \"{node}\" to \"{newNode}\"" );

						nodes.Add( newNode.Identifier, newNode );
						AddNode( newNode );
					}
				}

				if ( node is not IReplaceNode && node is IInitializeNode initializeableNode )
				{
					initializeableNode.InitializeNode();
				}

				// TODO : Remove FunctionResult node later once time has passed. Will mean that old project thats havent been converted
				// will just have missing node in place.
				if ( node is FunctionResult funcResult )
				{
					node.UpgradedToNewNode = true;

					var subgraphOutputs = ProjectUpgrading.ReplaceFunctionResult( funcResult, element, subgraphPath, ref connections );

					foreach ( var subgraphOutput in subgraphOutputs )
					{
						nodes.Add( subgraphOutput.Identifier, subgraphOutput );
						AddNode( subgraphOutput );
					}
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

				if ( node is not FunctionResult )
				{
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
				if ( replacedNodes.TryGetValue( node.Identifier, out var oldNode ) && output is null )
				{
					ProjectUpgrading.ReplaceOutputReference( node, oldNode, value.Output, ref output );
				}

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

		return nodes.Values;
	}

	public string SerializeNodes()
	{
		return SerializeNodes(Nodes);
	}

	public string SerializeNodes(IEnumerable<BaseNodePlus> nodes)
	{
		var doc = new JsonObject();
		var options = SerializerOptions();

		SerializeNodes( nodes, doc, options );

		return doc.ToJsonString(options);
	}

	private static void SerializeObject( object obj, JsonObject doc, JsonSerializerOptions options, Dictionary<string, string> identifiers = null )
	{
		var type = obj.GetType();
		var properties = type.GetProperties( BindingFlags.Instance | BindingFlags.Public )
			.Where( x => x.GetSetMethod() != null );

		foreach ( var property in properties )
		{
			if ( !property.CanRead)
				continue;

			if ( property.Name == VersioningInfo.VersionClassPropertyName )
				continue;

			if ( property.PropertyType == typeof( NodeInput ) )
				continue;
	
			if ( property.IsDefined( typeof( JsonIgnoreAttribute ) ) )
				continue;
	
			var propertyName = property.Name;
			if ( property.GetCustomAttribute<JsonPropertyNameAttribute>() is { } jpna )
				propertyName = jpna.Name;
	
			var propertyValue = property.GetValue(obj);
			if ( propertyName == "Identifier" && propertyValue is string identifier )
			{
				if ( identifiers.TryGetValue(identifier, out var newIdentifier ) )
				{
					propertyValue = newIdentifier;
				}
			}


			doc.Add( propertyName, JsonSerializer.SerializeToNode( propertyValue, options ) );
		}

		if ( obj is INode node )
		{
			string subgraphPath = null;
			if ( obj is SubgraphNode subgraphNode )
			{
				subgraphPath = subgraphNode.SubgraphPath;
			}
			foreach ( var input in node.Inputs )
			{
				if ( input.ConnectedOutput is not { } output )
					continue;

				doc.Add( input.Identifier, JsonSerializer.SerializeToNode( new NodeInput
				{
					Identifier = identifiers?.TryGetValue( output.Node.Identifier, out var newIdent ) ?? false ? newIdent : output.Node.Identifier,
					Output = output.Identifier,
					Subgraph = subgraphPath
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

			SerializeObject( node, nodeObject, options, identifiers);

			nodeArray.Add(nodeObject);
		}

		doc.Add("nodes", nodeArray);
	}
}
