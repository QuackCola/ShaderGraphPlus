namespace Editor.ShaderGraphPlus.Nodes;

[Title("World Normals from Depth"), Category("Utility")]
public sealed class WorldSpaceNormalFromDepth : ShaderNodePlus
{
[Hide]
public string GetWorldSpaceNormal => @"
// Code by Josh Wilson.
float3 GetWorldSpaceNormal( float2 vUv )
{
    float3 pos = Depth::GetWorldPosition(vUv);
    
    float offsetAmount = 0.5;
    float2 offset1 = float2(offsetAmount, 0.0);
    float2 offset2 = float2(0.0, offsetAmount);
    
    float3 tangentX = (Depth::GetWorldPosition(vUv + offset1) - Depth::GetWorldPosition(vUv - offset1)) / 2.0;
    float3 tangentY = (Depth::GetWorldPosition(vUv + offset2) - Depth::GetWorldPosition(vUv - offset2)) / 2.0;
    
    float3 normal = cross(tangentY, tangentX);

    return lerp(float3(0.0, 0.0, 0.0), normalize(normal), step(0.01f, Depth::Get(vUv)));
}
";

    [Title("Screen Pos")]
    [Input(typeof(Vector2))]
    [Hide]
    public NodeInput Coords { get; set; }

    [Output(typeof(Vector3))]
    [Hide]
    public NodeResult.Func Result => (GraphCompiler compiler) =>
    {
        var incoords = compiler.Result(Coords);

        var coords = "";
        var defaultpos = $"{(compiler.IsVs ? $"i.vPositionPs.xy" : $"i.vPositionSs.xy")}";

        if (compiler.Graph.MaterialDomain is MaterialDomain.PostProcess)
        {

            coords = incoords.IsValid ? $"{incoords.Cast(2)}" : defaultpos;
        }
        else
        {
            coords = incoords.IsValid ? $"{incoords.Cast(2)}" : defaultpos;
        }

        return new NodeResult(ResultType.Vector3, compiler.ResultFunction(GetWorldSpaceNormal, args: $"{coords}"));
    };
}