using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Editor.ShaderGraphPlus;

public struct NodeInput : IValid
{
	[Hide, Browsable( false )]
	public string Identifier { get; set; }

	[Hide, Browsable( false )]
	public string Output { get; set; }

	[Hide, Browsable( false )]
	public string Subgraph { get; set; }

	[Browsable( false )]
	[JsonIgnore, Hide]
	public readonly bool IsValid => !string.IsNullOrWhiteSpace( Identifier ) && !string.IsNullOrWhiteSpace( Output );

	public override readonly string ToString()
	{
		var subgraph = (Subgraph is not null) ? ("." + Subgraph) : "";
		return IsValid ? $"{Identifier}.{Output}{subgraph}" : "null";
	}

	public NodeInput()
	{
		Identifier = "";
		Output = "";
		Subgraph = null;
	}
}
