

namespace Editor.ShaderGraphPlus.Nodes;

[Title( "Invert Colors" ), Category( "PostProcessing/Transform" )]
public class InvertColorsNode : ShaderNodePlus
{

[Hide]
public static string InvertColors => @"
float3 InvertColors(float3 vInput )
{
return float3(1.0 - vInput.r,1.0 - vInput.g,1.0 - vInput.b);
}
";

	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput SceneColor { get; set; }

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Vector3, compiler.ResultFunction( InvertColors, 
			args:
			$"{compiler.ResultOrDefault( SceneColor, Vector3.One )}"
		));
	};
}
