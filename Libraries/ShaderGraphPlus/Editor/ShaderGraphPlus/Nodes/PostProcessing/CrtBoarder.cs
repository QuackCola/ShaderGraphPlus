namespace Editor.ShaderGraphPlus.Nodes;


[Title( "Crt Boarder" ), Category( "PostProcessing/Crt" )]
public class CRTBoarderNode : ShaderNodePlus
{

[Hide]
public static string CRTBoarder => @"
float3 CRTBoarder(float2 vScreenUV , float3 vSceneColor, float2 vCurvature)
{
	float3 vResult;

	vScreenUV = curveRemapUV( vScreenUV, vCurvature );

	if (vScreenUV.x < 0.0 || vScreenUV.y < 0.0 || vScreenUV.x > 1.0 || vScreenUV.y > 1.0){
		vResult = float3(0.0, 0.0, 0.0);
	} else {
		vResult = vSceneColor;
	}

	return vResult;
}
";

	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput ScreenUVs { get; set; }

	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput SceneColor { get; set; }

	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Curveature { get; set; }

	public Vector2 DefaultCurvature { get; set; } = new Vector2( 3.0f, 3.0f );

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var coords = compiler.Result( ScreenUVs );
		var scenecolor = compiler.ResultOrDefault( SceneColor, Vector3.One );
		var curvature = compiler.ResultOrDefault( Curveature, DefaultCurvature );
		
		string func = compiler.RegisterFunction(  CRTBoarder );
		string funcCall = compiler.ResultFunction( func, $"{(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vPositionSs.xy / g_vRenderTargetSize")}, {scenecolor}, {curvature}" );
		
		return new NodeResult( ResultType.Vector3, funcCall );
	};
}
