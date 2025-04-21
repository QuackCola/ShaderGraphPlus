namespace Editor.ShaderGraphPlus.Nodes;

/// <summary>
/// Vertex normal in world space
/// </summary>
[Title( "World Normal" ), Category( "Variables" ), Icon( "public" )]
public sealed class WorldNormal : ShaderNodePlus
{
	[Output( typeof( Vector3 ) )]
	[Hide]
	public static NodeResult.Func Result => ( GraphCompiler compiler ) => new( ResultType.Vector3, "i.vNormalWs", compiler.IsNotPreview );
}

/// <summary>
/// Vertex tangents in world space
/// </summary>
[Title( "World Tangent" ), Category( "Variables" ), Icon( "public" )]
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
[Title( "Object Space Normal" ), Category( "Variables" ), Icon( "view_in_ar" )]
public sealed class ObjectSpaceNormal : ShaderNodePlus
{
	[Output( typeof( Vector3 ) )]
	[Hide]
	public static NodeResult.Func Result => ( GraphCompiler compiler ) => new( ResultType.Vector3, "i.vNormalOs", compiler.IsNotPreview );
}



/// <summary>
/// Return the current screen position of the object
/// </summary>
[Title( "Screen Position" ), Category( "Variables" ), Icon( "install_desktop" )]
public sealed class ScreenPosition : ShaderNodePlus
{
	// Note: We could make all of these constants but I don't like the situation where it can generated something like
	// "i.vPositionSs.xy.xy" when casting.. even though that should be valid.

	public enum ScreenPositionMode
	{
	Raw,
	Center,
	//Tiled,
	//Pixel
	}

	[Hide]
	public ScreenPositionMode Mode { get; set; } = ScreenPositionMode.Raw;

	private string GetMode( string components, GraphCompiler compiler)
	{
		string returnCall = string.Empty;

		switch ( Mode )
		{
			case ScreenPositionMode.Raw:
				returnCall = $"{(compiler.IsVs ? $"i.vPositionPs.{components}" : $"i.vPositionSs.{components}")}";
				break;
			case ScreenPositionMode.Center:
				returnCall = $"{(compiler.IsVs ? $"i.vPositionPs.{components} * 2 - 1" : $"i.vPositionSs.{components} * 2 - 1")}";
				break;
		}

		return returnCall;
	}

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func XYZ => (GraphCompiler compiler) => new (ResultType.Vector3, GetMode("xyz", compiler));
 
	[Output( typeof( Vector2 ) )]
	[Hide]
	public NodeResult.Func XY => ( GraphCompiler compiler ) => new(ResultType.Vector2, GetMode("xy", compiler));

	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Z => ( GraphCompiler compiler ) => new(ResultType.Vector3, GetMode("z", compiler));

	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func W => (GraphCompiler compiler) => new(ResultType.Float, GetMode("w", compiler));
}

/// <summary>
/// Return the current screen uvs of the object
/// </summary>
[Title( "Screen Coordinate" ), Category( "Variables" ), Icon( "tv" )]
public sealed class ScreenCoordinate : ShaderNodePlus
{
	[Output( typeof( Vector2 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
		compiler.IsVs ?
		new( ResultType.Vector2, $"CalculateViewportUv( i.vPositionPs.xy )" ) :
		new( ResultType.Vector2, $"CalculateViewportUv( i.vPositionSs.xy )" );
}

/// <summary>
/// Return the projected screen space as texture coordinates
/// </summary>
[Title( "Projected Screen Space" ), Category( "Variables" )]
public sealed class ProjectedScreenCoordinate : ShaderNodePlus
{
	[Output( typeof( Vector2 ) ), Title( "UV" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
		compiler.IsVs ?
		new( ResultType.Vector2, $"CalculateViewportUv( i.vPositionPs.xy ) * g_vFrameBufferCopyInvSizeAndUvScale.zw" ) :
		new( ResultType.Vector2, $"CalculateViewportUv( i.vPositionSs.xy ) * g_vFrameBufferCopyInvSizeAndUvScale.zw" );
}

/// <summary>
/// Return the current world space position
/// </summary>
[Title( "World Space Position" ), Category( "Variables" ), Icon( "public" )]
public sealed class WorldPosition : ShaderNodePlus
{
	public bool NoHighPrecisionLightingOffsets { get; set; } = false;

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
		compiler.IsVs ?
		new( ResultType.Vector3, "i.vPositionWs" ) :
		new( ResultType.Vector3, $"i.vPositionWithOffsetWs.xyz {(NoHighPrecisionLightingOffsets ? "" : "+ g_vHighPrecisionLightingOffsetWs.xyz")}" );
}

/// <summary>
/// Return the current object space position of the pixel
/// </summary>
[Title( "Object Space Position" ), Category( "Variables" ), Icon( "view_in_ar" )]
public sealed class ObjectPosition : ShaderNodePlus
{
	[Output( typeof( Vector3 ) )]
	[Hide]
	public static NodeResult.Func Result => ( GraphCompiler compiler ) => new( ResultType.Vector3, "i.vPositionOs" );
}

/// <summary>
/// Return the current view direction of the pixel
/// </summary>
[Title( "View Direction" ), Category( "Variables" ), Icon( "cameraswitch" )]
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
[Title( "Vertex Color" ), Category( "Variables" ), Icon( "format_color_fill" )]
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
/// Blend of the vertex
/// </summary>
[Title( "Vertex Blend" ), Category( "Variables" ), Icon( "blender" )]
public sealed class VertexBlend : ShaderNodePlus
{
	[Output( typeof( float ) )]
	[Hide]
	public static NodeResult.Func R => ( GraphCompiler compiler ) => new( ResultType.Float, "i.vBlendValues.r" );

	[Output( typeof( float ) )]
	[Hide]
	public static NodeResult.Func G => ( GraphCompiler compiler ) => new( ResultType.Float, "i.vBlendValues.g" );

	[Output( typeof( float ) )]
	[Hide]
	public static NodeResult.Func B => ( GraphCompiler compiler ) => new( ResultType.Float, "i.vBlendValues.b" );

	[Output( typeof( float ) )]
	[Hide]
	public static NodeResult.Func A => ( GraphCompiler compiler ) => new( ResultType.Float, "i.vBlendValues.a" );
}

/// <summary>
/// Paint of the vertex
/// </summary>
[Title( "Vertex Paint" ), Category( "Variables" ), Icon( "brush" )]
public sealed class VertexPaint : ShaderNodePlus
{
	[Output( typeof( Vector3 ) )]
	[Hide]
	public static NodeResult.Func RGB => ( GraphCompiler compiler ) => new( ResultType.Vector3, "i.vPaintValues.rgb" );

	[Output( typeof( float ) )]
	[Hide]
	public static NodeResult.Func Alpha => ( GraphCompiler compiler ) => new( ResultType.Float, "i.vPaintValues.a" );
}

/// <summary>
/// Tint of the scene object
/// </summary>
[Title( "Tint" ), Category( "Variables" ), Icon( "palette" )]
public sealed class Tint : ShaderNodePlus
{
	[Hide, Output( typeof( Color ) )]
	public static NodeResult.Func RGBA => ( GraphCompiler compiler ) => new( ResultType.Color, "i.vTintColor" );
}
