namespace Editor.ShaderGraphPlus.Nodes;

[Title("Height to Normal"), Category("Transform")]
public sealed class HeightToNormal : ShaderNodePlus
{

[Hide]
public string Height2Normal => @"
float3 Height2Normal( float flHeight , float flStrength, float3 vPosition, float3 vNormal )
{
    float3 worldDerivativeX = ddx(vPosition);
    float3 worldDerivativeY = ddy(vPosition);

    float3 crossX = cross(vNormal, worldDerivativeX);
    float3 crossY = cross(worldDerivativeY, vNormal);

    float d = dot(worldDerivativeX, crossY);

    float sgn = d < 0.0 ? (-1.f) : 1.f;
    float surface = sgn / max(0.00000000000001192093f, abs(d));

    float dHdx = ddx(flHeight);
    float dHdy = ddy(flHeight);

    float3 surfGrad = surface * (dHdx*crossY + dHdy*crossX);

    return normalize(vNormal - (flStrength * surfGrad));
}
";

    public enum OutputNormalSpace
    {
        Tangent,
        World
    }

    /// <summary>
    /// Should we output in world space or tangent space.
    /// </summary>
    public OutputNormalSpace OutputSpace { get; set; } = OutputNormalSpace.World;

    [Input(typeof(float))]
    [Hide]
    public NodeInput Height { get; set; }


    [Input(typeof(float))]
    [Hide]
    public NodeInput Strength { get; set; }

    //[Input(typeof(Vector3))]
    //[Hide]
    //[Title("Position")]
    //public NodeInput WorldPos { get; set; }

    //[Input(typeof(Vector3))]
    //[Hide]
    //public NodeInput Normal { get; set; }

    public float DefaultStrength { get; set; } = 0.1f;

    [Output(typeof(Vector3))]
    [Hide]
    public NodeResult.Func Result => (GraphCompiler compiler) =>
    {

        var height = compiler.Result(Height);
        var strength = compiler.ResultOrDefault(Strength, DefaultStrength);
        var worldpos = "i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz";//compiler.Result(WorldPos);
        var worldnormal = "i.vNormalWs";//compiler.Result(Normal);

        if (!height.IsValid())
        {
            return NodeResult.MissingInput(nameof(Height));
        }
        //if (!worldpos.IsValid())
        //{
        //    return NodeResult.MissingInput(nameof(WorldPos));
        //}
        //if (!worldnormal.IsValid())
        //{
        //    return NodeResult.MissingInput(nameof(Normal));
        //}

        string result = compiler.ResultFunction(Height2Normal, args: $"{height}, {strength}, {worldpos}, {worldnormal}");

        if ( OutputSpace == OutputNormalSpace.Tangent )
        {
            result = $"Vec3WsToTs( {result}, i.vNormalWs, i.vTangentUWs, i.vTangentVWs )";
        }
 
        return new NodeResult( ResultType.Vector3, result );
    };

}