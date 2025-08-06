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
			SGPLog.Info( $"Version of loading Project is \"{projectFileVersion}\" which is less than the defined version \"{Version}\"." );
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

		//if ( obj is ISGPJsonUpgradeable upgradeable )
		//{
		//
		//}

		foreach ( var jsonProperty in doc.EnumerateObject() )
		{
			var prop = properties.FirstOrDefault( x =>
			{
				var propName = x.Name;
			
			
				if ( x.GetCustomAttribute<JsonPropertyNameAttribute>() is JsonPropertyNameAttribute jpna )
					propName = jpna.Name;
			
				return string.Equals( propName, jsonProperty.Name, StringComparison.OrdinalIgnoreCase );
			} );

			if ( prop == null )
				continue;

			if ( prop.CanWrite == false )
				continue;
			
			if ( prop.IsDefined( typeof( JsonIgnoreAttribute ) ) )
				continue;

			// Handle any types that use the ISGPJsonUpgradeable interface
			if ( typeof( ISGPJsonUpgradeable ).IsAssignableFrom( prop.PropertyType ) )
			{
				var typeInstance = EditorTypeLibrary.Create( prop.PropertyType.Name, prop.PropertyType );
				int oldVersionNumber = 0;

				if ( jsonProperty.Value.TryGetProperty( "__version", out var versionElement ) )
				{
					oldVersionNumber = versionElement.GetInt32();
				}

				SGPLog.Info( $"Got \"{prop.PropertyType}\" upgradeable version \"{oldVersionNumber}\"" );

				if ( typeInstance is ISGPJsonUpgradeable upgradeable && oldVersionNumber < upgradeable.Version )
				{
					var newElement = UpgradeJsonUpgradeable( oldVersionNumber, upgradeable, prop.PropertyType, jsonProperty, options );
					prop.SetValue( obj, JsonSerializer.Deserialize( newElement.GetRawText(), prop.PropertyType, options ) );

					continue; // Continue to the next jsonProperty :)
				}

				SGPLog.Info( $"\"{prop.PropertyType}\" is already at the latest version :)" );
			}

			prop.SetValue( obj, JsonSerializer.Deserialize( jsonProperty.Value.GetRawText(), prop.PropertyType, options ) );
		}
	}

	private IEnumerable<BaseNodePlus> DeserializeNodes( JsonElement doc, JsonSerializerOptions options, string subgraphPath = null )
	{
		var nodes = new Dictionary<string, BaseNodePlus>();
		var identifiers = _nodes.Count > 0 ? new Dictionary<string, string>() : null;
		var connections = new List<(IPlugIn Plug, NodeInput Value)>();
		var SubgraphOutputNodeInputs = new List<IPlugIn>();

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

				// TODO : Think about this...
				/*
				if ( node is IParameterNode parameterNode && IsSubgraph )
				{
					if ( node is Float3 float3Node )
					{
						var subgraphInput = new SubgraphInput();
						subgraphInput.Position = parameterNode.ParameterNodePosition.WithY( parameterNode.ParameterNodePosition.y - 128 );
						subgraphInput.InputName = float3Node.Name;
						subgraphInput.DefaultValue = new VariantValueVector3( float3Node.Value, SubgraphInputType.Vector3 );

						nodes.Add( subgraphInput.Identifier, subgraphInput );
						AddNode( subgraphInput );

						//foreach ( var output in float3Node.Outputs )
						//{
						//	SGPLog.Info( $"IParameterNode \"{float3Node.Name}\" output \"{output.Identifier}\"" );
						//}
					}
				}
				*/

				if ( node is CustomFunctionNode customCode )
				{
					customCode.OnNodeCreated();
				}

				if ( node is FunctionResult funcResult )
				{
					funcResult.CreateInputs();
				}

				if ( node is SubgraphOutput subgraphOutput )
				{
					subgraphOutput.CreateInput();
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
				
				Vector2 lastPos = Vector2.Zero;
				foreach ( var input in node.Inputs )
				{
					// Replace Function result inputs with individual SubgraphOutput nodes. Remove this later along with all the FunctionResult code...
					if ( node is FunctionResult functionResultNode )
					{
						var subgraphOutputNode = new SubgraphOutput();
						subgraphOutputNode.Position = functionResultNode.Position.WithY( 64 + lastPos.y );
						lastPos = subgraphOutputNode.Position;
					
						subgraphOutputNode.SubgraphFunctionOutput = new ShaderFunctionOutput()
						{
							OutputName = input.Identifier,
							//Preview = ,
						}; 
						subgraphOutputNode.SubgraphFunctionOutput.SetOutputTypeFromType( input.Type );
						subgraphOutputNode.CreateInput();
					
						nodes.Add( subgraphOutputNode.Identifier, subgraphOutputNode );
						AddNode( subgraphOutputNode );
						
						SubgraphOutputNodeInputs.Add( subgraphOutputNode.Inputs.First() );
					}

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

						if ( node is not FunctionResult )
						{
							connections.Add( (input, connection) );
						}
						else // replace FunctionResult node
						{
							foreach ( var subgraphResultInput in SubgraphOutputNodeInputs )
							{
								connections.Add( (subgraphResultInput, connection) );
							}
						
							RemoveNode( node );
							nodes.Remove( node.Identifier );
						}
					}
				}
			}

			nodes.Add( node.Identifier, node );

			if ( node.CanAddToGraph )
			{
				AddNode( node );
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
