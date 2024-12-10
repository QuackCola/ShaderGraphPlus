 
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
    /// What Mode to get 
    /// </summary>
	public DepthMode Mode { get; set; }


	[Output( typeof( float ) ), Hide]
	public NodeResult.Func Out => ( GraphCompiler compiler ) =>
	{
		var result = UV.IsValid() ? compiler.Result( UV ).Cast( 2 ) :
			compiler.IsVs ? "i.vPositionPs.xy" : "i.vPositionSs.xy";

        string returnCall = string.Empty;

		switch (Mode)
		{
            case DepthMode.Raw: returnCall = $"Depth::Get( {result} )"; break;
            case DepthMode.Linear: returnCall = $"Depth::GetLinear( {result} )"; break;
            case DepthMode.Normalized: returnCall = $"Depth::GetNormalized( {result} )"; break;
        }

		return new NodeResult( ResultType.Float, returnCall );
	};
}