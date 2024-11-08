namespace Editor.ShaderGraphPlus.Nodes;

[Title("Intersection"), Category("Utility")]
public sealed class IntersectionNode : ShaderNodePlus
{

    [Hide]
    public string Intersection => @"
float3 Intersection( float3 WorldPos, float3 WorldNormal, float2 TexCoord , float2 screencoords)
{
		float2 f2FinalTexCoord = TexCoord;//i.vTextureCoords.xy;

		float3 pos = WorldPos;
		float depth = Depth::GetNormalized(screencoords);
		float notSure = -abs(dot(pos, -g_vCameraDirWs)) + (1 / depth);

		float clampedDepth = saturate(notSure * 0.2 * 0.1);
		
		float something = pow(dot(pos * rsqrt(dot(pos, pos)), WorldNormal), 2);
		float clampedSomething = saturate(something);

		float varDiff = something - clampedSomething;
		float sampleArg = abs((varDiff * clampedDepth + clampedSomething) * clampedDepth);
		float sample2 = abs((varDiff * clampedDepth + clampedSomething) * clampedDepth - 0.02);

		float alphaMul = 0.05;
		float alpha = alphaMul / sampleArg - alphaMul;
		alpha = alpha + (alphaMul / sample2 - alphaMul);

		alpha = abs(alpha);

		float time = g_flTime;

		float2 offset1 = float2(time * 0.0725, time * 0.04);
		//float f1TintMask = g_tTintMask.Sample( TextureFiltering, f2FinalTexCoord + offset1 ).x;

		float2 offset2 = float2(time * -0.0725, time * -0.04);
		//float f2TintMask = g_tTintMask.Sample( TextureFiltering, f2FinalTexCoord + offset2 ).x;

		alpha = alpha; //* (f1TintMask + f2TintMask);

		float3 outVar = float3(22,44,88) * alpha;

		outVar = outVar * 1;//g_flMasterAlphaMul;

		return outVar;
}
";

   //[Input(typeof(float))]
   //[Hide]
   //public NodeInput Time { get; set; }
   //
   //[Input(typeof(float))]
   //[Hide]
   //public NodeInput Frequency { get; set; }
   //
   //[Input(typeof(float))]
   //[Hide]
   //public NodeInput Phase { get; set; }
   //
   //[Input(typeof(float))]
   //[Hide]
   //public NodeInput Strength { get; set; }

    public float DefaultFrequency { get; set; } = 1.0f;
    public float DefaultPhase { get; set; } = 0.0f;
    public float DefaultStrength { get; set; } = 10.0f;

    [Output(typeof(float))]
    [Hide]
    public NodeResult.Func Result => (GraphCompiler compiler) =>
    {

        return new NodeResult(ResultType.Float, compiler.ResultFunction(Intersection, args: $"i.vPositionWithOffsetWs.xyz,i.vNormalWs,i.vTextureCoords.xy, i.vPositionSs" ));
    };
}
