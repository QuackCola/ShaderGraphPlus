namespace Editor.ShaderGraphPlus.Nodes;

[Title("Test Matrix Input Node"), Description("Test for verifying a matrix can be passed into a node and not cause any issues."), Category("Dev")]
public sealed class TestMatrix4X4Node : ShaderNodePlus
{

[Hide]
public string TestMatrix => @"
float4 TestMatrix( float4x4 test4x4, float3x3 test3x3, float2x2 test2x2 )
{
	return float4(0,0,0,1);
}
";

	[Title( "Input Matrix 4x4" )]
	[Input( typeof( Float4x4 ) )]
	[Hide]
	public NodeInput Float4x4 { get; set; }

	[Title( "Input Matrix 3x3" )]
	[Input( typeof( Float3x3 ) )]
	[Hide]
	public NodeInput Float3x3 { get; set; }

	[Title( "Input Matrix 2x2" )]
	[Input( typeof( Float2x2 ) )]
	[Hide]
	public NodeInput Float2x2 { get; set; }

	[Output( typeof( Float4x4 ) ), Title( "Value" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var result4x4 = compiler.Result( Float4x4 );
		var result3x3 = compiler.Result( Float3x3 );
		var result2x2 = compiler.Result( Float2x2 );

		string func = compiler.RegisterFunction( TestMatrix );
		string funcCall = compiler.ResultFunction( func, $"{result4x4}, {result3x3}, {result2x2}" );

		return new NodeResult( ResultType.Color, funcCall );
	};

}

[Title( "Test Node" ), Description( "Test node." ), Category( "Dev" )]
public sealed class TestNode : ShaderNodePlus
{

}
