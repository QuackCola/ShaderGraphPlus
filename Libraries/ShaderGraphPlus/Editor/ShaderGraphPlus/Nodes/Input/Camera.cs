
namespace Editor.ShaderGraphPlus.Nodes;

/// <summary>
/// Camera position and shit
/// </summary>
[Title( "Camera" ), Category( "Variables" )]
public sealed class Camera : ShaderNodePlus
{
	[Output( typeof( Vector3 ) ), Title( "Position" )]
	[Hide]
	public static NodeResult.Func WorldPosition => ( GraphCompiler compiler ) => new( ResultType.Vector3, "g_vCameraPositionWs" );

	[Output( typeof( Vector3 ) )]
	[Hide]
	public static NodeResult.Func Direction => ( GraphCompiler compiler ) => new( ResultType.Vector3, "g_vCameraDirWs" );

	[Output( typeof( float ) )]
	[Hide]
	public static NodeResult.Func NearPlane => ( GraphCompiler compiler ) => new( ResultType.Float, "g_flNearPlane" );

	[Output( typeof( float ) )]
	[Hide]
	public static NodeResult.Func FarPlane => ( GraphCompiler compiler ) => new( ResultType.Float, "g_flFarPlane");
}

/// <summary>
/// Sample depth texture
/// </summary>
[Title( "Depth" ), Category( "Camera" )]
public sealed class Depth : ShaderNodePlus
{
	public enum DepthMode
	{
	    Linear,
	    Raw
	}

	[Input( typeof( Vector2 ) ), Hide]
	public NodeInput UV { get; set; }

    /// <summary>
    /// Use Linear depth
    /// </summary>
    public bool Linear { get; set; } = false;

	[Output( typeof( float ) ), Hide]
	public NodeResult.Func Out => ( GraphCompiler compiler ) =>
	{
		var result = UV.IsValid() ? compiler.Result( UV ).Cast( 2 ) :
			compiler.IsVs ? "i.vPositionPs.xy" : "i.vPositionSs.xy";

        string returnCall = string.Empty;

        if ( Linear )
		{
			returnCall = $"Depth::GetLinear( {result} )";
		}
		else
		{
			returnCall = $"Depth::Get( {result} )";
        }

		return new NodeResult( ResultType.Float, returnCall );
	};
}


/// <summary>
/// Test node for doing depth stuff..
/// </summary>
[Title("Test Depth"), Category("Camera")]
public sealed class DepthNodeThing : ShaderNodePlus
{

    public static string LinearizeDepth => @"
float LinearizeDepth(float2 vUV)
{
        float flProjectedDepth = Depth::Get(vUV);
        // Remap depth to viewport depth range
        flProjectedDepth = RemapValClamped( flProjectedDepth, g_flViewportMinZ, g_flViewportMaxZ, 0.0, 1.0 );

        float flZScale = g_vInvProjRow3.z;
        float flZTran = g_vInvProjRow3.w;

        float flDepthRelativeToRayLength = 1.0 / ( ( flProjectedDepth * flZScale + flZTran ) );

        return flDepthRelativeToRayLength;
}

";

    [Output(typeof(float)), Hide]
    public NodeResult.Func Out => (GraphCompiler compiler) =>
	{

        return new NodeResult(ResultType.Float, compiler.ResultFunction(LinearizeDepth, args: $"CalculateViewportUv(i.vPositionSs.xy)"));
    };
}