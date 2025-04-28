namespace Editor.ShaderGraphPlus.Nodes;

[Title( "SSAO Sample" ), Description( "Samples the screen space ambient occlusion" ), Category( "Shading" )]
public sealed class SSAOSampleNode : ShaderNodePlus
{
	[Hide]
	[Input( typeof( Vector4 ) )]
	public NodeInput ScreenPosition { get; set; }


	[Output( typeof( float ) )]
	[Hide]
	[Title( "SSAO" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var screenposition = compiler.ResultOrDefault( ScreenPosition, Vector4.Zero );

		return new NodeResult( ResultType.Float, $"ScreenSpaceAmbientOcclusion::Sample( {screenposition} )", constant: false );
	};
}
