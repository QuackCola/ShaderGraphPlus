namespace Editor.ShaderGraphPlus.Nodes;


[Title("Depth Intersection"), Category( "Utility" )]
public sealed class DepthIntersectionNode : ShaderNodePlus
{

[Hide]
public string DepthIntersect => @"
float DepthIntersect( float2 vUv )
{
	float flDepth = Depth::Get(vUv);


	return flDepth;
}
";

    [Output(typeof(float))]
    [Hide]
    public NodeResult.Func Result => (GraphCompiler compiler) =>
    {
        var coords = $"i.vPositionSs.xy";

        var result = compiler.ResultFunction(DepthIntersect, args: $"{coords}");
        return new NodeResult(ResultType.Float, result);
    };

}