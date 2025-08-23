namespace ShaderGraphPlus.Nodes;

[Title( "Int Cast Test" ), Category( "Dev" ), Icon( "arrow" ), Hide]
public sealed class CastTest : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Input( typeof( int ) ) , Hide]
	public NodeInput Input { get; set; }

	[Sandbox.Range( 1 , 4 )]
	public int CastType { get; set; } = 1;

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = compiler.ResultOrDefault( Input, 1 );
	


		var castTest = input.Cast( CastType );

		SGPLog.Info( $"Casted int to \"{castTest}\"" );

		return new NodeResult( ResultType.Float, "0.0f" );
	};

}
