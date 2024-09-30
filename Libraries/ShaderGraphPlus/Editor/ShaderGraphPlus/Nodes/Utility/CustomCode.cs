
using Sandbox;
using System.Numerics;

namespace Editor.ShaderGraphPlus.Nodes;

/// <summary>
/// Simple container for custom expressions. Pretty basic for now.
/// </summary>
[Title( "Custom Expression" ), Category( "Utility" )]
public class CustomCodeNode : ShaderNodePlus
{

	[Hide]
	public override string Title => string.IsNullOrEmpty( Name ) ?
	$"{DisplayInfo.For( this ).Name}" :
	$"{DisplayInfo.For( this ).Name} ({Name})";

	public string Name { get; set; }

	public string Code { get; set; }

	public ResultType ResultType { get; set; }

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{

		if ( !string.IsNullOrWhiteSpace( Code ) )
		{
			return new( ResultType, $"{Code}" );

		}
		else
		{
			return new( ResultType, $"0" );
		}
		
	};
	
}
