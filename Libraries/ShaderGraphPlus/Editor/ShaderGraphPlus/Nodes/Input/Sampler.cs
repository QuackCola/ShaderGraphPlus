using Editor.ShaderGraphPlus;

[Title( "Sampler" ), Category( "Variables" )]
[Description( "How a texture is filtered and wrapped when sampled." )]
public sealed class SamplerNode : ShaderNodePlus
{
	[InlineEditor]
	public Sampler SamplerState { get; set; } = new Sampler();

	[Hide]
	public override string Title => string.IsNullOrWhiteSpace( SamplerState.Name ) ? null : $"{DisplayInfo.For( this ).Name} ({SamplerState.Name})";

	public SamplerNode() : base()
	{
		ExpandSize = new Vector2( 0, 8 );
	}

	[Output( typeof( Sampler ) ), Hide]
	public NodeResult.Func Sampler => ( GraphCompiler compiler ) =>
	{
		// Register sampler	with the compiler.
		var result = compiler.ResultSampler( SamplerState );

		if ( !string.IsNullOrWhiteSpace( result ) )
		{
			return new NodeResult( ResultType.Sampler, result, true, true );
		}
		else
		{
			return NodeResult.Error( "null was somehow returned from ResultSampler();!!!" );
		}
	};
}
