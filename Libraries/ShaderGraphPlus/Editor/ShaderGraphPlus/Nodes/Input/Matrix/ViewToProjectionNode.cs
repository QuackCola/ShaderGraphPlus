namespace ShaderGraphPlus.Nodes;

[Title( "View To Projection" ), Category( "Constants/Matrix" ), Icon( "dataset" )]
public sealed class ViewToProjectionNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Output( typeof( Float4x4 ) ), Title( "Value" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Float4x4, "g_matViewToProjection", true );
	};
}
