namespace Editor.ShaderGraphPlus.Nodes;

[Title("World Normals from Depth"), Category("Utility")]
public sealed class WorldSpaceNormalFromDepth : ShaderNodePlus
{
	[Title( "Screen Pos" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coords { get; set; }
	
	[Output( typeof( Vector3 ) )]
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
		
		return new NodeResult(ResultType.Vector3, compiler.ResultFunction( "GetWorldSpaceNormal", $"{coords}" ) );
	};
}
