namespace Editor.ShaderGraphPlus.Nodes;


/// <summary>
/// Test sheid effect with depth intersection
/// </summary>
[Title("Sheild"), Category("Dev")]
public sealed class IntersectionNode : ShaderNodePlus
{

    [Hide]
    public string Intersection => @"
float3 Intersection( float3 WorldPos, float3 WorldNormal, float2 TexCoord , float2 screencoords , float flIntersectionSharpness, float flBubbleAlphaMul, float flMasterAlphaMul, Texture2D tTintMask, float3 vShieldColor)
{
		float2 f2FinalTexCoord = TexCoord;

		float3 pos = WorldPos;
		float depth = Depth::GetNormalized(screencoords);
		float notSure = -abs(dot(pos, -g_vCameraDirWs)) + (1 / depth);

		float clampedDepth = saturate(notSure * flIntersectionSharpness * 0.1);
		
		float something = pow(dot(pos * rsqrt(dot(pos, pos)), WorldNormal), 2);
		float clampedSomething = saturate(something);

		float varDiff = something - clampedSomething;
		float sampleArg = abs((varDiff * clampedDepth + clampedSomething) * clampedDepth);
		float sample2 = abs((varDiff * clampedDepth + clampedSomething) * clampedDepth - 0.02);

		float alphaMul = flBubbleAlphaMul;
		float alpha = alphaMul / sampleArg - alphaMul;
		alpha = alpha + (alphaMul / sample2 - alphaMul);

		alpha = abs(alpha);

		float time = g_flTime;

		float2 offset1 = float2(time * 0.0725, time * 0.04);
		float f1TintMask = tTintMask.Sample( TextureFiltering, f2FinalTexCoord + offset1 ).x;

		float2 offset2 = float2(time * -0.0725, time * -0.04);
		float f2TintMask = tTintMask.Sample( TextureFiltering, f2FinalTexCoord + offset2 ).x;

		alpha = alpha * (f1TintMask + f2TintMask);

		float3 outVar = vShieldColor * alpha;

		outVar = outVar * flMasterAlphaMul;

		return outVar;
}
";

    [Input(typeof(Vector3))]
    [Hide]
    public NodeInput WorldPos { get; set; }

    [Input(typeof(Vector3))]
	[Hide]
	public NodeInput WorldNormal { get; set; }

    [Input(typeof(Vector2))]
    [Hide]
    public NodeInput Coordinates { get; set; }

    [Input(typeof(Vector2))]
    [Hide]
    public NodeInput ScreenUV { get; set; }

    [Input(typeof(float))]
    [Hide]
    public NodeInput IntersectSharpness { get; set; }

    [Input(typeof(float))]
    [Hide]
    public NodeInput BubbleAlphaMul { get; set; }

    [Input(typeof(float))]
    [Hide]
    public NodeInput MasterAlphaMul { get; set; }

    [Input(typeof(TextureObject))]
    [Hide]
    public NodeInput TintMask { get; set; }

    [Input(typeof(Color))]
    [Hide]
    public NodeInput ShieldColor { get; set; }


    public float DefaultIntersectSharpness { get; set; } = 0.2f;
    public float DefaultBubbleAlphaMul { get; set; } = 0.1f;
    public float DefaultMasterAlphaMul { get; set; } = 1.0f;
    public Color DefaultShieldColor { get; set; } = Color.White;

    [Output(typeof(float))]
    [Hide]
    public NodeResult.Func Result => (GraphCompiler compiler) =>
    {
        // float3 WorldPos, float3 WorldNormal, float2 TexCoord , float2 screencoords , float flIntersectionSharpness, float flBubbleAlphaMul, float flMasterAlphaMul, Texture2D tTintMask, float3 vShieldColor
        var worldpos = compiler.Result(WorldPos);
        var worldnormal = compiler.Result(WorldNormal);

        if (!worldpos.IsValid)
        {
            return NodeResult.MissingInput(nameof(WorldPos));
        }
        if (!worldnormal.IsValid)
        {
            return NodeResult.MissingInput(nameof(WorldNormal));
        }

        var texcoords = compiler.Result(Coordinates);
        var screencoords = compiler.Result(ScreenUV);

        var intersectsharpness = compiler.ResultOrDefault(IntersectSharpness, DefaultIntersectSharpness);
        var bubblealphamul = compiler.ResultOrDefault(BubbleAlphaMul, DefaultBubbleAlphaMul);
        var masteralphamul = compiler.ResultOrDefault(MasterAlphaMul, DefaultMasterAlphaMul);
        
        var tintmask = compiler.Result(TintMask);
        if (!tintmask.IsValid)
        {
            return NodeResult.MissingInput(nameof(TintMask));
        }

        var shieldcolor = compiler.ResultOrDefault(ShieldColor, DefaultShieldColor);

        return new NodeResult(ResultType.Float, compiler.ResultFunction(Intersection, 
            args: 
            $"{worldpos}," +
            $"{worldnormal}," +
            $"{(texcoords.IsValid ? $"{texcoords.Cast(2)}" : "i.vTextureCoords.xy")}," +
            $"{(screencoords.IsValid ? $"{screencoords.Cast(2)}" : "i.vPositionSs.xy")}," +
            $"{intersectsharpness}," +
            $"{bubblealphamul}," +
            $"{masteralphamul}," +
            $"{tintmask}," +
            $"{shieldcolor}"
        ));
    };
}
