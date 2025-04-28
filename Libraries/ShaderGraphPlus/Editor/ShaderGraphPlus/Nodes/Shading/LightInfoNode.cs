namespace Editor.ShaderGraphPlus.Nodes;

[Title( "Light Info" ), Description( "" ), Category( "Shading" )]
public sealed class LightInfoNode : ShaderNodePlus
{

	[Output( typeof( Vector3 ) )]
	[Hide]
	[Title( "Color" )]
	public NodeResult.Func ResultA => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Vector3, $"light.Color", constant: true );
	};

	[Output( typeof( Vector3 ) )]
	[Hide]
	[Title( "Direction" )]
	public NodeResult.Func ResultB => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Vector3, $"light.Direction", constant: true );
	};

	[Output( typeof( Vector3 ) )]
	[Hide]
	[Title( "Position" )]
	public NodeResult.Func ResultC => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Vector3, $"light.Position", constant: true );
	};

	[Output( typeof( float ) )]
	[Hide]
	[Title( "Attenuation" )]
	public NodeResult.Func ResultD => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Float, $"light.Attenuation", constant: true );
	};

	[Output( typeof( float ) )]
	[Hide]
	[Title( "Visibility" )]
	public NodeResult.Func ResultE => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Float, $"light.Visibility", constant: true );
	};
}
