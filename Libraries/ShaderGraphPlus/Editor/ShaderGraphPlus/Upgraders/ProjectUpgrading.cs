using Facepunch.ActionGraphs;
using ShaderGraphPlus;
using ShaderGraphPlus.Nodes;

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
}
