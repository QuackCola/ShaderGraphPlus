namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Sample depth texture
/// </summary>
[Title( "Depth" ), Category( "Camera" )]
public sealed class Depth : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color PrimaryHeaderColor => ShaderGraphPlusTheme.PrimaryNodeHeaderColors.GlobalVariableNode;

	public enum DepthSamplingMode
	{
		///<summary>The raw value of the depth buffer.</summary>
		Raw,
		///<summary>Depth from 0..1 based on the z-near and z-far of the current viewport.</summary>
		[Title("Normalized ( Projected Space )")]
		Normalized,
		///<summary>Depth in world units from the camera.</summary>
		[Title("Linear ( View Space )")]
		Linear
	}

    [Hide]
    public override string Title => $"{DisplayInfo.For(this).Name} ({Mode})";

    [Input( typeof( Vector2 ) ), Hide]
    public NodeInput UV { get; set; }


	/// <summary>
	/// How to sample the depth buffer.
	/// </summary>
	public DepthSamplingMode SamplingMode { get; set; } = DepthSamplingMode.Linear;

	[Output( typeof( float ) ), Hide]
	public NodeResult.Func Out => ( GraphCompiler compiler ) =>
	{
		var result = UV.IsValid() ? compiler.Result( UV ).Cast( 2 ) :
			compiler.IsVs ? "i.vPositionPs.xy" : "i.vPositionSs.xy";

		string funcCall = "";
		switch ( SamplingMode )
		{
			case DepthSamplingMode.Raw: funcCall = $"Depth::Get( {result} )"; break;
			case DepthSamplingMode.Normalized: funcCall = $"Depth::GetNormalized( {result} )"; break;
			case DepthSamplingMode.Linear: funcCall = $"Depth::GetLinear( {result} )"; break;
			default: SGPLog.Error( $"Unknown Mode : \"{SamplingMode}\"" ); break;
		}

		return new NodeResult( ResultType.Float, funcCall );
	};
}
