
using NodeEditorPlus;
using GraphView = NodeEditorPlus.GraphView;
using NodeUI = NodeEditorPlus.NodeUI;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;

namespace ShaderGraphPlus.Nodes;

// TODO : Remove this?

/// <summary>
///
/// </summary>
[Title( "Random" ), Category( "PostProcessing/Utility" )]
public sealed class RandomNode : ShaderNodePlus, IWarningNode
{
	[Hide]
	public override int Version => 1;

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
		
		string func = compiler.RegisterFunction( Random );
		string funcCall = compiler.ResultFunction( func, $"{(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vPositionSs.xy / g_vRenderTargetSize")}" );
		
		return new NodeResult( ResultType.Vector2, funcCall );
	};

	public List<string> GetWarnings()
	{
		var warnings = new List<string>();

		warnings.Add( $"RandomNode is depreciated and will be removed in a future update." );

		return warnings;
	}
}
