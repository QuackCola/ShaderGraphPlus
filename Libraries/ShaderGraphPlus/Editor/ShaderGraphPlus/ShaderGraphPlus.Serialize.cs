using System.Text.Json.Nodes;

namespace Editor.ShaderGraphPlus;

partial class ShaderGraphPlus
{
    private static JsonSerializerOptions SerializerOptions(bool indented = false)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = indented,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };

        options.Converters.Add(new JsonStringEnumConverter(null, true));

        return options;
    }

    public string Serialize()
    {
        var doc = new JsonObject();
        var options = SerializerOptions(true);

        SerializeObject(this, doc, options);
        SerializeNodes(Nodes, doc, options);

        return doc.ToJsonString(options);
    }

    public void Deserialize(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var options = SerializerOptions();

        DeserializeObject(this, root, options);
        DeserializeNodes(root, options);
    }

    public IEnumerable<BaseNodePlus> DeserializeNodes(string json)
    {
        using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
        return DeserializeNodes(doc.RootElement, SerializerOptions());
    }

    private static void DeserializeObject(object obj, JsonElement doc, JsonSerializerOptions options)
    {
        var type = obj.GetType();
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.GetSetMethod() != null);

        foreach (var nodeProperty in doc.EnumerateObject())
        {
            var prop = properties.FirstOrDefault(x =>
            {
                var propName = x.Name;
                if (x.GetCustomAttribute<JsonPropertyNameAttribute>() is JsonPropertyNameAttribute jpna)
                    propName = jpna.Name;

                return string.Equals(propName, nodeProperty.Name, StringComparison.OrdinalIgnoreCase);
            });

            if (prop == null)
                continue;

            if (prop.CanWrite == false)
                continue;

            if (prop.IsDefined(typeof(JsonIgnoreAttribute)))
                continue;

            prop.SetValue(obj, JsonSerializer.Deserialize(nodeProperty.Value.GetRawText(), prop.PropertyType, options));
        }
    }

    private IEnumerable<BaseNodePlus> DeserializeNodes(JsonElement doc, JsonSerializerOptions options)
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

				foreach ( var input in node.Inputs )
				{
					if ( !element.TryGetProperty( input.Identifier, out var connectedElem ) )
						continue;

					var connected = connectedElem
						.Deserialize<NodeInput?>();

					if ( connected is { IsValid: true } )
					{
						connections.Add( (input, connected.Value) );
					}
				}
			}

            nodes.Add(node.Identifier, node);

            AddNode(node);
        }

        foreach (var (input, value) in connections)
        {
            var outputIdent = identifiers?.TryGetValue(value.Identifier, out var newIdent) ?? false
                ? newIdent : value.Identifier;

            input.ConnectedOutput = nodes.TryGetValue(outputIdent, out var node)
                ? node.Outputs.FirstOrDefault(x => x.Identifier == value.Output)
                : null;
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

        SerializeNodes(nodes, doc, options);

        return doc.ToJsonString(options);
    }

    private static void SerializeObject(object obj, JsonObject doc, JsonSerializerOptions options, Dictionary<string, string> identifiers = null)
    {
        var type = obj.GetType();
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.GetSetMethod() != null);

        foreach (var property in properties)
        {
            if (!property.CanRead)
                continue;

            if (property.PropertyType == typeof(NodeInput))
                continue;

            if (property.IsDefined(typeof(JsonIgnoreAttribute)))
                continue;

            var propertyName = property.Name;
            if (property.GetCustomAttribute<JsonPropertyNameAttribute>() is { } jpna)
                propertyName = jpna.Name;

            var propertyValue = property.GetValue(obj);
            if (propertyName == "Identifier" && propertyValue is string identifier)
            {
                if (identifiers.TryGetValue(identifier, out var newIdentifier))
                {
                    propertyValue = newIdentifier;
                }
            }

            doc.Add(propertyName, JsonSerializer.SerializeToNode(propertyValue, options));
        }

        if (obj is INode node)
        {
            foreach (var input in node.Inputs)
            {
                if (input.ConnectedOutput is not { } output)
                    continue;

                doc.Add(input.Identifier, JsonSerializer.SerializeToNode(new NodeInput
                {
                    Identifier = identifiers?.TryGetValue(output.Node.Identifier, out var newIdent) ?? false ? newIdent : output.Node.Identifier,
                    Output = output.Identifier
                }));
            }
        }
    }

    private static void SerializeNodes(IEnumerable<BaseNodePlus> nodes, JsonObject doc, JsonSerializerOptions options)
    {
        var identifiers = new Dictionary<string, string>();
        foreach (var node in nodes)
        {
            identifiers.Add(node.Identifier, $"{identifiers.Count}");
        }

        var nodeArray = new JsonArray();

        foreach (var node in nodes)
        {
            var type = node.GetType();
            var nodeObject = new JsonObject { { "_class", type.Name } };

            SerializeObject(node, nodeObject, options, identifiers);

            nodeArray.Add(nodeObject);
        }

        doc.Add("nodes", nodeArray);
    }
}
