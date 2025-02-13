namespace Editor.ShaderGraphPlus.Nodes;

/// <summary>
/// Color of the scene
/// </summary>
[Title( "Scene Color" ), Category( "PostProcessing/Variables" )]
public sealed class SceneColorNode : ShaderNodePlus
{
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput ScreenUVs { get; set; }

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func SceneColor => ( GraphCompiler compiler ) =>
	{
		var coords = compiler.Result( ScreenUVs );
		return new NodeResult( ResultType.Vector3, $"g_tColorBuffer.Sample( g_sBilinearMirror ,{(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vPositionSs.xy / g_vRenderTargetSize")})" );
	};
}
