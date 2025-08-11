
namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Sample a provided gradient.
/// </summary>
[Title( "Sample Gradient" ), Category( "Gradient" ), Icon( "gradient" )]
public sealed class SampleGradientNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;


	[Title( "Gradient" )]
	[Input( typeof( Gradient ) )]
	[Hide]
	public NodeInput Gradient { get; set; }

	[Title( "Time" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Time { get; set; }

	[Output( typeof( Color ) )]
	[Hide]
	public NodeResult.Func Result => (GraphCompiler compiler) =>
	{
		var gradient = compiler.Result(Gradient);
	
		if ( !gradient.IsValid() )
		{
			return NodeResult.MissingInput( nameof( Gradient ) );
		}
	
		if ( gradient.ResultType != ResultType.Gradient )
		{
			return NodeResult.Error($"Gradient input is not a gradient!");
		}
	
		var time = compiler.ResultOrDefault(Time, 0.0f);
	
		return new NodeResult( ResultType.Color, $"Gradient::SampleGradient( {gradient.Code}, {time.Code} )", constant: false );
	
	};
}
