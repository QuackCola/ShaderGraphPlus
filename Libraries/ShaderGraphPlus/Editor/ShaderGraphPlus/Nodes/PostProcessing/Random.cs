namespace Editor.ShaderGraphPlus.Nodes;

/// <summary>
///
/// </summary>
[Title( "Random" ), Category( "PostProcessing/Utility" )]
public sealed class RandomNode : ShaderNodePlus
{

[Hide]
public static string Random => @"
float2 Random(float2 vUv)
{
    vUv = float2( dot(vUv, float2(127.1,311.7) ), dot(vUv, float2(269.5,183.3) ) );
    return -1.0 + 2.0 * frac(sin(vUv) * 43758.5453123);
}
";

	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput ScreenUVs { get; set; }

	[Output( typeof( Vector2 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var coords = compiler.Result( ScreenUVs );
		
		string funcCall = $"{compiler.RegisterFunction( Random )}( {(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vPositionSs.xy / g_vRenderTargetSize")} );";
		
		return new NodeResult( ResultType.Vector2, funcCall );
	};
}
