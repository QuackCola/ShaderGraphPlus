namespace Editor.ShaderGraphPlus.Nodes;

/// <summary>
/// Vertex normal in world space
/// </summary>
[Title( "World Normal" ), Category( "Variables" )]
public sealed class WorldNormal : ShaderNodePlus
{
	[Output( typeof( Vector3 ) )]
	[Hide]
	public static NodeResult.Func Result => ( GraphCompiler compiler ) => new( ResultType.Vector3, "i.vNormalWs", compiler.IsNotPreview );
}

/// <summary>
/// Vertex tangents in world space
/// </summary>
[Title( "World Tangent" ), Category( "Variables" )]
public sealed class WorldTangent : ShaderNodePlus
{
	[Output( typeof( Vector3 ) )]
	[Hide]
	public static NodeResult.Func U => ( GraphCompiler compiler ) => new( ResultType.Vector3, "i.vTangentUWs", compiler.IsNotPreview );

	[Output( typeof( Vector3 ) )]
	[Hide]
	public static NodeResult.Func V => ( GraphCompiler compiler ) => new( ResultType.Vector3, "i.vTangentVWs", compiler.IsNotPreview );
}

/// <summary>
/// Vertex normal in object space
/// </summary>
[Title( "Object Space Normal" ), Category( "Variables" )]
public sealed class ObjectSpaceNormal : ShaderNodePlus
{
	[Output( typeof( Vector3 ) )]
	[Hide]
	public static NodeResult.Func Result => ( GraphCompiler compiler ) => new( ResultType.Vector3, "i.vNormalOs", compiler.IsNotPreview );
}

/// <summary>
/// Return the current screen position of the object
/// </summary>
[Title( "Screen Position" ), Category( "Variables" )]
public sealed class ScreenPosition : ShaderNodePlus
{
	// Note: We could make all of these constants but I don't like the situation where it can generated something like
	// "i.vPositionSs.xy.xy" when casting.. even though that should be valid.

	[Output( typeof( Vector3 ) )]
	[Hide]
	public static NodeResult.Func XYZ => ( GraphCompiler compiler ) => compiler.IsVs ? new( ResultType.Vector3, "i.vPositionPs.xyz" ) : new( ResultType.Vector3, "i.vPositionSs.xyz" );

	[Output( typeof( Vector2 ) )]
	[Hide]
	public static NodeResult.Func XY => ( GraphCompiler compiler ) => compiler.IsVs ? new( ResultType.Vector2, "i.vPositionPs.xy" ) : new( ResultType.Vector2, "i.vPositionSs.xy" );

	[Output( typeof( float ) )]
	[Hide]
	public static NodeResult.Func Z => ( GraphCompiler compiler ) => compiler.IsVs ? new( ResultType.Float, "i.vPositionPs.z" ) : new( ResultType.Float, "i.vPositionSs.z" );

	[Output( typeof( float ) )]
	[Hide]
	public static NodeResult.Func W => ( GraphCompiler compiler ) => compiler.IsVs ? new( ResultType.Float, "i.vPositionPs.w" ) : new( ResultType.Float, "i.vPositionSs.w" );
}

/// <summary>
/// Return the current screen uvs of the object
/// </summary>
[Title( "Screen Coordinate" ), Category( "Variables" )]
public sealed class ScreenCoordinate : ShaderNodePlus
{
	[Output( typeof( Vector2 ) )]
	[Hide]
	public static NodeResult.Func Result => ( GraphCompiler compiler ) =>
		compiler.IsVs ?
		new( ResultType.Vector2, "CalculateViewportUv( i.vPositionPs.xy )" ) :
		new( ResultType.Vector2, "CalculateViewportUv( i.vPositionSs.xy )" );
}

/// <summary>
/// Return the current world space position
/// </summary>
[Title( "World Space Position" ), Category( "Variables" )]
public sealed class WorldPosition : ShaderNodePlus
{
	[Output( typeof( Vector3 ) )]
	[Hide]
	public static NodeResult.Func Result => ( GraphCompiler compiler ) =>
		compiler.IsVs ?
		new( ResultType.Vector3, "i.vPositionWs" ) :
		new( ResultType.Vector3, "i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz" );
}

/// <summary>
/// Return the current object space position of the pixel
/// </summary>
[Title( "Object Space Position" ), Category( "Variables" )]
public sealed class ObjectPosition : ShaderNodePlus
{
	[Output( typeof( Vector3 ) )]
	[Hide]
	public static NodeResult.Func Result => ( GraphCompiler compiler ) => new( ResultType.Vector3, "i.vPositionOs" );
}

/// <summary>
/// Return the current view direction of the pixel
/// </summary>
[Title( "View Direction" ), Category( "Variables" )]
public sealed class ViewDirection : ShaderNodePlus
{
	[Output( typeof( Vector3 ) )]
	[Hide]
	public static NodeResult.Func Result => ( GraphCompiler compiler ) =>
		compiler.IsVs ?
		new( ResultType.Vector3, "CalculatePositionToCameraDirWs( i.vPositionWs )" ) :
		new( ResultType.Vector3, "CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz )" );
}

/// <summary>
/// Color of the vertex
/// </summary>
[Title( "Vertex Color" ), Category( "Variables" )]
public sealed class VertexColor : ShaderNodePlus
{
	[Output( typeof( Vector3 ) )]
	[Hide]
	public static NodeResult.Func RGB => ( GraphCompiler compiler ) => new( ResultType.Vector3, "i.vColor.rgb" );

	[Output( typeof( float ) )]
	[Hide]
	public static NodeResult.Func Alpha => ( GraphCompiler compiler ) => new( ResultType.Float, "i.vColor.a" );
}

/// <summary>
/// Tint of the scene object
/// </summary>
[Title( "Tint" ), Category( "Variables" )]
public sealed class Tint : ShaderNodePlus
{
	[Hide, Output( typeof( Color ) )]
	public static NodeResult.Func RGBA => ( GraphCompiler compiler ) => new( ResultType.Color, "i.vTintColor" );
}

/// <summary>
/// 
/// </summary>
[Title( "TangentUWs" ), Category( "Variables" )]
public sealed class TangentUWsNode : ShaderNodePlus
{
	[Hide, Output( typeof( Vector3 ) )]
	public static NodeResult.Func Result => ( GraphCompiler compiler ) => new( ResultType.Vector3, "i.vTangentUWs.xyz" , compiler.IsNotPreview );
}

/// <summary>
/// 
/// </summary>
[Title( "TangentVWs" ), Category( "Variables" )]
public sealed class TangentVWsNode : ShaderNodePlus
{
	[Hide, Output( typeof( Vector3 ) )]
	public static NodeResult.Func Result => ( GraphCompiler compiler ) => new( ResultType.Vector3, "i.vTangentVWs.xyz" , compiler.IsNotPreview );
}