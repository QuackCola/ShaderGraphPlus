namespace Editor.ShaderGraphPlus.Nodes;

/// <summary>
/// WIP Depth Intersection Node Test.
/// </summary>
[Title("Depth Intersection"), Category( "Utility" )]
public sealed class DepthIntersectionNode : ShaderNodePlus
{

[Hide]
public string DepthIntersect => @"
float DepthIntersect( float3 vWorldPos, float2 vUv, float flDepthOffset )
{
	float3 l_1 = vWorldPos - g_vCameraPositionWs;
	float l_2 = dot( l_1, g_vCameraDirWs );
	float l_3 = l_2 + flDepthOffset;
	float l_4 = Depth::GetLinear( vUv );
	float l_5 = smoothstep( l_2, l_3, l_4 );
	float result = 1 - l_5;

	return result;
}
";

    [Input(typeof(float))]
    [Hide]
    public NodeInput DepthOffset { get; set; }

    public float DefaultDepthOffset { get; set; } = 1.0f;


    [Output(typeof(float))]
    [Hide]
    public NodeResult.Func Result => (GraphCompiler compiler) =>
    {
        var coords = $"i.vPositionSs.xy";
        var worldpos = $"i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz";

        var depthoffset = compiler.ResultOrDefault(DepthOffset, DefaultDepthOffset);

        var result = compiler.ResultFunction(DepthIntersect, args: $"{worldpos}, {coords}, {depthoffset}");
        return new NodeResult(ResultType.Float, result);
    };

}