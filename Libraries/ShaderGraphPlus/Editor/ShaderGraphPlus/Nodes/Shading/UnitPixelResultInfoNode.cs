namespace Editor.ShaderGraphPlus.Nodes;

[Title( "Unlit Pixel Result Info" ), Description( "the result of your pixel shader before lighting." ), Category( "" )]
public sealed class UnitPixelResultInfoNode : ShaderNodePlus
{
	[Output( typeof( Vector3 ) )]
	[Hide]
	[Title( "Color" )]
	public NodeResult.Func ResultA => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Vector3, $"m.Albedo", constant: true );
	};

	[Output( typeof( float ) )]
	[Hide]
	[Title( "Opacity" )]
	public NodeResult.Func ResultB => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Float, $"m.Opacity", constant: true );
	};
}
