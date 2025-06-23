namespace Editor.ShaderGraphPlus.Nodes;

/// <summary>
/// WIP Depth Intersection Effect. May not be "Correct".
/// </summary>
[Title("Depth Intersection"), Category( "Utility" )]
public sealed class DepthIntersectionNode : ShaderNodePlus
{

	[Hide]
	public static string DepthIntersect => @"
float DepthIntersect( float3 vWorldPos, float2 vUv, float flDepthOffset )
{
	float3 l_1 = vWorldPos - g_vCameraPositionWs;
	float l_2 = dot( l_1, g_vCameraDirWs );

	flDepthOffset += l_2;
	float Depth = Depth::GetLinear( vUv );

	float l_3 = smoothstep( l_2, flDepthOffset, Depth );

	/ One Minus the result before return
	return 1 - l_3;
}
";

	public DepthIntersectionNode() : base()
	{

	}

	/// <summary>
	/// Coordinates to use.
	/// </summary>
	//[Title( "ScreenUVs" )]
	//[Input( typeof( Vector2 ) )]
	//[Hide]
	//public NodeInput Coords { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput DepthOffset { get; set; }

	public float DefaultDepthOffset { get; set; } = 1.0f;


	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Result => (GraphCompiler compiler) =>
	{
		var coords = "i.vPositionSs.xy";
		var worldpos = $"i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz";

		var depthoffset = compiler.ResultOrDefault(DepthOffset, DefaultDepthOffset);

		string func = compiler.RegisterFunction( DepthIntersect );
		string funcCall = compiler.ResultFunction( func, $"{worldpos}, {coords}, {depthoffset}" );

		return new NodeResult( ResultType.Float, funcCall );
	};
}
