using Editor.NodeEditor;
using Sandbox;
using ShaderGraphPlus;
using ShaderGraphPlus.Nodes;
using System.Text.Json;
using System.Text.Json.Nodes;

public enum ReplacementMode
{
	SubgraphOnly,
	Both
}

[AttributeUsage( AttributeTargets.Class )]
public class NodeReplaceAttribute : Attribute
{

	public ReplacementMode Mode;

	public NodeReplaceAttribute( ReplacementMode mode )
	{
		Mode = mode;
	}
}


public static class ProjectUpgrading
{
	// Key : ( Old Node type , Old output Name ) Value : New output name
	public static Dictionary<(Type OldNodeType, string OldOutputName), string> NodeOutputMapping
	{
		get
		{
			return new Dictionary<(Type, string), string>()
			{
				{ ( typeof( SamplerNode ), $"Sampler" ), $"{nameof( SubgraphInput.Result )}"},
			};
		}
	}

	public static Dictionary<string, string> NodeTypeNameMapping => new()
	{
		{ "TextureObjectNode", "Texture2DObjectNode" },
		{ "NormapMapTriplanar", "NormalMapTriplanar" },
	};

	public static void ReplaceOutputReference( BaseNodePlus newNode, BaseNodePlus oldNode, 
		string outputIdentifier, 
		ref IPlugOut outputPlug )
	{
		if ( NodeOutputMapping.TryGetValue( (oldNode.GetType(), outputIdentifier), out var newOutputIdentifier ) )
		{
			SGPLog.Info( $"Replacing Output reference \"{outputIdentifier}\" with \"{newOutputIdentifier}\"" );

			var newNodeOutputPlug = newNode.Outputs.FirstOrDefault( x => x.Identifier == newOutputIdentifier );
			if ( newNodeOutputPlug != null )
			{
				var plugOut = newNodeOutputPlug as BasePlugOut;
				var info = new PlugInfo()
				{
					Id = plugOut.Info.Id,
					Name = plugOut.DisplayInfo.Name,
					Type = plugOut.Type,
					DisplayInfo = new()
					{
						Name = plugOut.DisplayInfo.Name,
						Fullname = plugOut.Type.FullName
					}
				};

				outputPlug = new BasePlugOut( newNode, info, info.Type );
			}
			else
			{
				SGPLog.Error( $"Could not find output with name \"{newOutputIdentifier}\" on node \"{newNode}\"" );
			}
		}
		else
		{
			SGPLog.Error( $"Could not find output mapping entry with key \"{(oldNode.GetType(), outputIdentifier)}\"" );
		}

	}


	public static JsonElement ReplaceNode( JsonElement oldElement, JsonSerializerOptions serializerOptions )
	{
		var jsonObject = JsonNode.Parse( oldElement.GetRawText() ) as JsonObject;


		if ( jsonObject.ContainsKey( "FunctionOutputs" ) )
		{
			






		}






		return JsonSerializer.Deserialize<JsonElement>( jsonObject.ToJsonString(), serializerOptions );
	}


	public static List<BaseNodePlus> ReplaceFunctionResult( FunctionResult functionResult, JsonElement element, string subgraphPath, ref List<(IPlugIn Plug, NodeInput Value)> connections )
	{
		var newNodes = new List<BaseNodePlus>();
		Vector2 lastOffset = Vector2.Zero;

		foreach ( var funcResultInput in functionResult.Inputs )
		{
			var subgraphOutputNode = new SubgraphOutput();

			lastOffset.y += 64;
			subgraphOutputNode.Position = functionResult.Position + new Vector2( 0, lastOffset.y );

			subgraphOutputNode.SubgraphFunctionOutput = new ShaderFunctionOutput( ((BasePlugIn)funcResultInput).Info.Id )
			{
				OutputName = funcResultInput.Identifier,
				Preview = functionResult.FunctionOutputs.Where( x => x.Name == funcResultInput.Identifier ).FirstOrDefault().Preview,
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

			newNodes.Add( subgraphOutputNode );
		}

		return newNodes;
	}

}
