namespace ShaderGraphPlus.Nodes;

[Title( "Float 4x4" ), Category( "Constants/Matrix" ), Icon( "dataset" )]
public sealed class Float4x4Node : MatrixParameterNode<Float4x4>
{
	[Hide] public override int Version => 1;

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Output( typeof( Float4x4 ) ), Title( "Value" )]
	[Hide]
	[Editor( nameof( Value ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( Name, Value, default, default, false, IsAttribute, default );
	};
}
