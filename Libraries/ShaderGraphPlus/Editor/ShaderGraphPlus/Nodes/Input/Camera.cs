
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
	public static NodeResult.Func FarPlane => ( GraphCompiler compiler ) => new( ResultType.Float, "g_flNearPlane" );
}

/// <summary>
/// Sample depth texture
/// </summary>
[Title( "Depth" ), Category( "Camera" )]
public sealed class Depth : ShaderNodePlus
{
	[Input( typeof( Vector2 ) ), Hide]
	public NodeInput UV { get; set; }

	[Output( typeof( float ) ), Hide]
	public NodeResult.Func Out => ( GraphCompiler compiler ) =>
	{
		var result = UV.IsValid() ? compiler.Result( UV ).Cast( 2 ) :
			compiler.IsVs ? "i.vPositionPs.xy" : "i.vPositionSs.xy";

		return new NodeResult( ResultType.Float, $"Depth::Get( {result} )" );
	};
}
