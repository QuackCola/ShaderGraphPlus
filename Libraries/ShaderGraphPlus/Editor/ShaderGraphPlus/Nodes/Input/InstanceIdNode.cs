
namespace ShaderGraphPlus.Nodes;

/// <summary>
/// 
/// </summary>
[Title( "Instance Id" ), Category( "Variables" ), Icon( "123" )]
public sealed class InstanceIdNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	public void RegisterStageInputs( GraphCompiler compiler )
	{
		compiler.RegisterVertexInput( "uint", "vInstanceID", "SV_InstanceID" );
		compiler.RegisterPixelInput( "uint", "vInstanceID", "SV_InstanceID" );

	}

	[Output, Title( "Result" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		if ( compiler.IsPreview )
		{
			RegisterStageInputs( compiler );
		}

		var result = $"{(compiler.IsVs ? "i.vInstanceID" : "i.vInstanceID")}";

		return new NodeResult( ResultType.Float, result );
	};
}
