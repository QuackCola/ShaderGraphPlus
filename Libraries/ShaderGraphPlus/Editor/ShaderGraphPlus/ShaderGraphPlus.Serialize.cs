using System.Text.Json.Nodes;
using static Sandbox.Spline;

namespace Editor.ShaderGraphPlus;

internal static class ObsoleteMapping
{
    public static Dictionary<string, string> Mapping => new()
    {
        { "MaterialDomain", "Domain" }
    };

}

partial class ShaderGraphPlus
{
    private static JsonSerializerOptions SerializerOptions( bool indented = false )
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

		Dictionary<(string, string), (string, string)> mapping = new();

		foreach ( var entry in ObsoleteMapping.Mapping )
		{
			FetchObsolete( this, root, options, entry, out var newPropData, out var oldPropData );

			if ( newPropData.Item2 != oldPropData.Item2 )
			{
                mapping.Add(newPropData, oldPropData);
            }
		}

		DeserializeObject( this, root, options, mapping);
		DeserializeNodes( root, options, subgraphPath );
	}

    private void FetchObsolete( object obj, JsonElement doc, JsonSerializerOptions options, KeyValuePair<string,string> mappingEntry, out (string, string) newPropData, out (string,string) oldPropData )
    {
		var type = obj.GetType();
		var properties = type.GetProperties( BindingFlags.Instance | BindingFlags.Public )
			.Where( x => x.GetSetMethod() != null );

        oldPropData = new();
		newPropData = new();

        foreach ( var nodeProperty in doc.EnumerateObject() )
		{
			var prop = properties.FirstOrDefault( x =>
			{
				var propName = x.Name;
			
			
			    if ( x.GetCustomAttribute<JsonPropertyNameAttribute>() is JsonPropertyNameAttribute jpna )
					propName = jpna.Name;
			
				return string.Equals( propName, nodeProperty.Name, StringComparison.OrdinalIgnoreCase );
			} );
			
			
			if ( prop == null )
				continue;

			if ( prop.CanWrite == false )
				continue;
			
			if ( prop.IsDefined( typeof( JsonIgnoreAttribute ) ) )
				continue;
            
            if ( prop.IsDefined( typeof( ObsoleteAttribute ) ) )
			{
                if ( prop.Name == mappingEntry.Key )
                {
                    oldPropData.Item1 = prop.Name;
                    oldPropData.Item2 = nodeProperty.Value.GetRawText();
                }
				else
				{
					throw new Exception( $"Cannot find property with name `{mappingEntry.Key}`" );
				}
            }
            else if (prop.Name == mappingEntry.Value)
            {
                newPropData.Item1 = prop.Name;
                newPropData.Item2 = nodeProperty.Value.GetRawText();
            }
        }
    }

    public IEnumerable<BaseNodePlus> DeserializeNodes(string json)
    {
        using var doc = JsonDocument.Parse( json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip } );
        return DeserializeNodes( doc.RootElement, SerializerOptions() );
    }

 	private static void DeserializeObject( object obj, JsonElement doc, JsonSerializerOptions options, Dictionary<(string, string), (string, string)> oldToNewMapping = null)
	{
		var type = obj.GetType();
		var properties = type.GetProperties( BindingFlags.Instance | BindingFlags.Public )
			.Where( x => x.GetSetMethod() != null );
	
		foreach ( var nodeProperty in doc.EnumerateObject() )
		{
            (string, string) newData = new();
  
            var prop = properties.FirstOrDefault( x =>
			{
				var propName = x.Name;
			
			
			    if ( x.GetCustomAttribute<JsonPropertyNameAttribute>() is JsonPropertyNameAttribute jpna )
					propName = jpna.Name;
			
				return string.Equals( propName, nodeProperty.Name, StringComparison.OrdinalIgnoreCase );
			} );
			
			
			if ( prop == null )
				continue;

			if ( prop.CanWrite == false )
				continue;
			
			if ( prop.IsDefined( typeof( JsonIgnoreAttribute ) ) )
				continue;

			if ( oldToNewMapping != null && oldToNewMapping.Any())
			{
				foreach (var entry in oldToNewMapping)
				{
				    if ( prop.Name == entry.Key.Item1 )
				    {
				        SGPLog.Info( $"Cloning data from obsolete property `{entry.Value.Item1}` to `{entry.Key.Item1}`" );
				        newData.Item1 = entry.Value.Item1;
						newData.Item2 = entry.Value.Item2;
				    }
				}
            }

			if ( newData.Item1 != null )
			{
                prop.SetValue( obj, JsonSerializer.Deserialize( newData.Item2, prop.PropertyType, options ) );
            }
			else
			{
                prop.SetValue( obj, JsonSerializer.Deserialize( nodeProperty.Value.GetRawText(), prop.PropertyType, options ) );
            }
        }
	}

    private IEnumerable<BaseNodePlus> DeserializeNodes(JsonElement doc, JsonSerializerOptions options, string subgraphPath = null )
    {
        var nodes = new Dictionary<string, BaseNodePlus>();
        var identifiers = _nodes.Count > 0 ? new Dictionary<string, string>() : null;
        var connections = new List<(IPlugIn Plug, NodeInput Value)>();

        var arrayProperty = doc.GetProperty("nodes");
        foreach (var element in arrayProperty.EnumerateArray())
        {
  			var typeName = element.GetProperty( "_class" ).GetString();
			var typeDesc = EditorTypeLibrary.GetType( typeName );
			var type = new ClassNodeType( typeDesc );

			BaseNodePlus node;
			if ( typeDesc is null )
			{
				var missingNode = new MissingNode( typeName, element );
				node = missingNode;
				DeserializeObject( node, element, options, null );
			}
			else
			{
				node = EditorTypeLibrary.Create<BaseNodePlus>( typeName );
				DeserializeObject( node, element, options, null );
				
				if ( identifiers != null && _nodes.ContainsKey( node.Identifier ) )
				{
					identifiers.Add( node.Identifier, node.NewIdentifier() );
				}

				if ( node is CustomFunctionNode customCode )
				{
					customCode.OnNodeCreated();
				}
				
				if ( node is FunctionResult funcResult )
				{
				    funcResult.CreateInputs();
				}
				
				if ( node is SubgraphNode subgraphNode )
				{
					subgraphNode.OnNodeCreated();
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
			}

            nodes.Add( node.Identifier, node );

            AddNode( node );
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
        return SerializeNodes( Nodes );
    }

    public string SerializeNodes(IEnumerable<BaseNodePlus> nodes)
    {
        var doc = new JsonObject();
        var options = SerializerOptions();

        SerializeNodes(nodes, doc, options);

        return doc.ToJsonString(options);
    }

	private static void SerializeObject( object obj, JsonObject doc, JsonSerializerOptions options, Dictionary<string, string> identifiers = null )
	{
		var type = obj.GetType();
		var properties = type.GetProperties( BindingFlags.Instance | BindingFlags.Public )
			.Where( x => x.GetSetMethod() != null );

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
				if ( identifiers.TryGetValue( identifier, out var newIdentifier ) )
				{
					propertyValue = newIdentifier;
				}
			}

			doc.Add( propertyName, JsonSerializer.SerializeToNode( propertyValue, options ) );
		}

		//if ( obj is INode node )
		//{
		//	string subgraphPath = null;
		//	if ( obj is SubgraphNode subgraphNode )
		//	{
		//		subgraphPath = subgraphNode.SubgraphPath;
		//	}
		//	foreach ( var input in node.Inputs )
		//	{
		//		if ( input.ConnectedOutput is not { } output )
		//			continue;
		//
		//		doc.Add( input.Identifier, JsonSerializer.SerializeToNode( new NodeInput
		//		{
		//			Identifier = identifiers?.TryGetValue( output.Node.Identifier, out var newIdent ) ?? false ? newIdent : output.Node.Identifier,
		//			Output = output.Identifier,
		//			Subgraph = subgraphPath
		//		} ) );
		//	}
		//}

		if ( obj is INode node )
		{
			foreach ( var input in node.Inputs )
			{
				if ( input.ConnectedOutput is not { } output )
					continue;

				doc.Add( input.Identifier, JsonSerializer.SerializeToNode( new NodeInput
				{
					Identifier = identifiers?.TryGetValue( output.Node.Identifier, out var newIdent ) ?? false ? newIdent : output.Node.Identifier,
					Output = output.Identifier
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
            var type = node.GetType();
            var nodeObject = new JsonObject { { "_class", type.Name } };

            SerializeObject( node, nodeObject, options, identifiers );

            nodeArray.Add( nodeObject );
        }

        doc.Add( "nodes", nodeArray );
    }
}