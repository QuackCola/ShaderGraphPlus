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

		//doc.Add( VersioningInfo.VersionJsonPropertyName, Version );

		SerializeObject( this, doc, options );
		SerializeNodes( Nodes, doc, options );

		return doc.ToJsonString( options );
	}

	public void Deserialize( string json, string subgraphPath = null )
	{
		using var doc = JsonDocument.Parse( json );
		var root = doc.RootElement;
		var options = SerializerOptions();

		// Check for the version so we can handle upgrades
		var latestVersion = Version;
		var currentVersion = 0; // Assume 0 for files that don't have the Version property
		if ( root.TryGetProperty( VersioningInfo.VersionJsonPropertyName, out var ver ) )
		{
			currentVersion = ver.GetInt32();
		}

		// Deserialize everything using the current version
		Version = currentVersion;
		DeserializeObject( this, root, options );
		DeserializeNodes( root, options, subgraphPath, currentVersion );

		// Upgrade to the latest version
		Version = latestVersion;
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

	public static JsonElement UpgradeJsonUpgradeable( int versionNumber, ISGPJsonUpgradeable jsonUpgradeable, Type type, JsonProperty jsonProperty, JsonSerializerOptions serializerOptions )
	{
		ArgumentNullException.ThrowIfNull( jsonUpgradeable );

		var jsonObject = JsonNode.Parse( jsonProperty.Value.GetRawText() ) as JsonObject;

		SGPJsonUpgrader.Upgrade( versionNumber, jsonObject, type );

		return JsonSerializer.Deserialize<JsonElement>( jsonObject.ToJsonString(), serializerOptions );
	}

	private static void DeserializeObject( object obj, JsonElement doc, JsonSerializerOptions options )
	{
		var type = obj.GetType();
		var properties = type.GetProperties( BindingFlags.Instance | BindingFlags.Public )
			.Where( x => x.GetSetMethod() != null );

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

	private IEnumerable<BaseNodePlus> DeserializeNodes( JsonElement doc, JsonSerializerOptions options, string subgraphPath = null, int fileVersion = -1 )
	{
		var nodes = new Dictionary<string, BaseNodePlus>();
		var identifiers = _nodes.Count > 0 ? new Dictionary<string, string>() : null;
		var connections = new List<(IPlugIn Plug, NodeInput Value)>();
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
				//var replaceAttribute = node.GetType().GetCustomAttribute<NodeReplaceAttribute>();

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
				else
				{
					node = EditorTypeLibrary.Create<BaseNodePlus>( typeName );
					DeserializeObject( node, element, options );
				}
				

				if ( identifiers != null && _nodes.ContainsKey( node.Identifier ) )
				{
					identifiers.Add( node.Identifier, node.NewIdentifier() );
				}

				/*
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
				*/

				//if ( node is not IReplaceNode && node is IInitializeNode initializeableNode )
				if ( node is IInitializeNode initializeableNode )
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

		return nodes.Values;
	}

	public string SerializeNodes()
	{
		return SerializeNodes( Nodes );
	}

	public string SerializeNodes( IEnumerable<BaseNodePlus> nodes )
	{
		var doc = new JsonObject();
		var options = SerializerOptions();

		SerializeNodes( nodes, doc, options );

		return doc.ToJsonString( options );
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
			"Bool" => true,
			"Int" => true,
			"Float" => true,
			"Float2" => true,
			"Float3" => true,
			"Float4" => true,
			"TextureSampler" => true,
			"Texture2DObjectNode" => true,
			"SamplerNode" => true,
			_ => false
		};
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
				subgraphOutput.Id = id.GetGuid();
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
			case "Bool":
				subgraphInput.InputData.InputType = SubgraphPortType.Bool;
				if ( element.TryGetProperty( "Value", out var boolValue ) )
				{
					subgraphInput.InputData = new VariantValueBool( boolValue.GetBoolean(), SubgraphPortType.Bool );
				}
				break;
			case "Int":
				subgraphInput.InputData.InputType = SubgraphPortType.Int;
				if ( element.TryGetProperty( "Value", out var intValue ) )
				{
					subgraphInput.InputData = new VariantValueInt( intValue.GetInt32(), SubgraphPortType.Int );
				}
				break;
			case "Float":
				subgraphInput.InputData.InputType = SubgraphPortType.Float;
				if ( element.TryGetProperty( "Value", out var floatValue ) )
				{
					subgraphInput.InputData = new VariantValueFloat( floatValue.GetSingle(), SubgraphPortType.Float );
				}
				break;
			case "Float2":
				subgraphInput.InputData.InputType = SubgraphPortType.Vector2;
				if ( element.TryGetProperty( "Value", out var float2Value ) )
				{
					var vector2 = JsonSerializer.Deserialize<Vector2>( float2Value.GetRawText(), options );
					subgraphInput.InputData = new VariantValueVector2( vector2, SubgraphPortType.Vector2 );
				}
				break;
			case "Float3":
				subgraphInput.InputData.InputType = SubgraphPortType.Vector3;
				if ( element.TryGetProperty( "Value", out var float3Value ) )
				{
					var vector3 = JsonSerializer.Deserialize<Vector3>( float3Value.GetRawText(), options );
					subgraphInput.InputData = new VariantValueVector3( vector3, SubgraphPortType.Vector3 );
				}
				break;
			case "Float4":
				subgraphInput.InputData.InputType = SubgraphPortType.Color;
				if ( element.TryGetProperty( "Value", out var ColorValue ) )
				{
					var vector4 = JsonSerializer.Deserialize<Vector4>( ColorValue.GetRawText(), options );
					subgraphInput.InputData = new VariantValueColor( vector4, SubgraphPortType.Color );
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
				subgraphInput.InputData.InputType = SubgraphPortType.Sampler;
				if ( element.TryGetProperty( "SamplerState", out var SamplerStateValue ) )
				{
					var samplerState = JsonSerializer.Deserialize<Sampler>( SamplerStateValue.GetRawText(), options );
					subgraphInput.InputData = new VariantValueSampler( samplerState, SubgraphPortType.Sampler );
				}
				break;
		}

		return subgraphInput;
	}
}
