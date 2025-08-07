using ShaderGraphPlus.Nodes;
using System.Reflection;
using System.Text.Json.Nodes;

namespace ShaderGraphPlus;

public interface ISGPJsonUpgradeable
{
	[JsonPropertyName( "__version" ), Hide]
	public int Version { get; }
}

partial class ShaderGraphPlus
{
	/// <summary>
	/// Key is the old node type name and value is the new node type name.
	/// </summary>
	public static Dictionary<string, string> NodeTypeNameMapping => new()
	{
		{ "TextureObjectNode", "Texture2DObjectNode" },
		{ "NormapMapTriplanar", "NormalMapTriplanar" },
	};
}

partial class ShaderGraphPlus
{
	[JsonIgnore, Hide]
	internal bool Upgrade { get; set; } = false;

	internal static JsonSerializerOptions SerializerOptions( bool indented = false )
	{
		var options = new JsonSerializerOptions
		{
			WriteIndented = indented,
			PropertyNameCaseInsensitive = true,
			NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
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

	private static int GetProjectVersion( JsonElement root )
	{
		if ( root.TryGetProperty( "__version", out var versionElement ) && versionElement.TryGetInt32( out var versionNum ) )
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
				if ( jsonProperty.Value.TryGetProperty( "__version", out var versionElement ) )
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

		var arrayProperty = doc.GetProperty("nodes");
		foreach (var element in arrayProperty.EnumerateArray())
		{
			var typeName = element.GetProperty( "_class" ).GetString();

			// Use the new typename if applicable.
			if ( NodeTypeNameMapping.TryGetValue( typeName, out string newTypeName ) )
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
				DeserializeObject( node, element, options );

				if ( identifiers != null && _nodes.ContainsKey( node.Identifier ) )
				{
					identifiers.Add( node.Identifier, node.NewIdentifier() );
				}

				// Replace named IParameter nodes with a SubgraphInput node.
				if ( node is IParameterNode parameterNode && !string.IsNullOrWhiteSpace( parameterNode.Name ) && IsSubgraph )
				{
					node.UpgradedToNewNode = true;

					var subgraphInputNode = parameterNode.UpgradeToSubgraphInput();
					subgraphInputNode.Position = parameterNode.ParameterNodePosition;
					
					// Take the Identifier of the node that we are replacing.
					subgraphInputNode.Identifier = node.Identifier;

					nodes.Add( subgraphInputNode.Identifier, subgraphInputNode );
					AddNode( subgraphInputNode );
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

				if ( node is CustomFunctionNode customCode )
				{
					customCode.OnNodeCreated();
				}

				// TODO : Remove FunctionResult node later once time has passed. Will mean that old project thats havent been converted
				// will just have missing node in place.
				if ( node is FunctionResult funcResult )
				{
					funcResult.CreateInputs();
#region FunctionResult To SubgraphOutput Region
					node.UpgradedToNewNode = true;

					Vector2 lastOffset = Vector2.Zero;
					foreach ( var funcResultInput in funcResult.Inputs )
					{
						var subgraphOutputNode = new SubgraphOutput();

						lastOffset.y += 64;
						subgraphOutputNode.Position = funcResult.Position + new Vector2( 0, lastOffset.y );

						subgraphOutputNode.SubgraphFunctionOutput = new ShaderFunctionOutput( ((BasePlugIn)funcResultInput).Info.Id )
						{
							OutputName = funcResultInput.Identifier,
							Preview = funcResult.FunctionOutputs.Where( x => x.Name == funcResultInput.Identifier ).FirstOrDefault().Preview,
						};

						subgraphOutputNode.SubgraphFunctionOutput.SetOutputTypeFromType( funcResultInput.Type );
						
						// Chnage some stuff with a new PlugInfo & BasePlugIn.
						var oldPlug = (BasePlugIn)funcResultInput;
						var plugInfoNew = new PlugInfo()
						{
							Id = oldPlug.Info.Id,
							Name = oldPlug.Info.Name,
							Type = funcResultInput.Type,
							DisplayInfo = new()
							{
								Name = oldPlug.Info.Name,
								Fullname = funcResultInput.Type.FullName,
							}
						};
						var plugInNew = new BasePlugIn( subgraphOutputNode, plugInfoNew, plugInfoNew.Type );
						subgraphOutputNode.InternalInput = plugInNew;

						if ( !element.TryGetProperty( subgraphOutputNode.InternalInput.Identifier, out var connectedElem ) )
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

							connections.Add( (subgraphOutputNode.InternalInput, connection) );
					
						}

						nodes.Add( subgraphOutputNode.Identifier, subgraphOutputNode );
						AddNode( subgraphOutputNode );

					}
#endregion FunctionResult To SubgraphOutput Region
				}

				if ( node is SubgraphOutput subgraphOutput )
				{
					subgraphOutput.CreateInput();
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
	
				if ( node.UpgradedToNewNode == false )
				{
					nodes.Add( node.Identifier, node );

					AddNode( node );
				}

				//if ( node.CanAddToGraph )
				//{
				//	AddNode( node );
				//}
			}
		}

		foreach ( var (input, value) in connections )
		{
			var outputIdent = identifiers?.TryGetValue( value.Identifier, out var newIdent ) ?? false
				? newIdent : value.Identifier;

			if ( nodes.TryGetValue( outputIdent, out var node ) )
			{
				var output = node.Outputs.FirstOrDefault( x => x.Identifier == value.Output );
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

		if ( obj is ISGPJsonUpgradeable upgradeable )
		{
			doc.Add( "__version", JsonSerializer.SerializeToNode( upgradeable.Version, options ) );
		}

		foreach ( var property in properties )
		{
			if ( !property.CanRead)
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
			var nodeObject = new JsonObject { { "_class", type.Name } };
		
			SerializeObject(node, nodeObject, options, identifiers);

			nodeArray.Add(nodeObject);
		}

		doc.Add("nodes", nodeArray);
	}
}
