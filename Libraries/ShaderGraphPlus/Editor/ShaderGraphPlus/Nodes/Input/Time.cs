
namespace Editor.ShaderGraphPlus.Nodes;

/// <summary>
/// Current time
/// </summary>
[Title( "Time" ), Category( "Variables" )]
public sealed class Time : ShaderNodePlus
{
	[JsonIgnore]
	public float Value => RealTime.Now;

	[Output( typeof( float ) ), Title( "Time" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Float, compiler.IsPreview ? "g_flPreviewTime" : "g_flTime", compiler.IsNotPreview );
	};
}
