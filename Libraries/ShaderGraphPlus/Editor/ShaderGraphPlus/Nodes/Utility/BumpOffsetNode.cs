namespace Editor.ShaderGraphPlus.Nodes;


[Title( "Bump Offset" ), Category( "Utility" ), Icon( "water" )]
public sealed class BumpOffsetNode : ShaderNodePlus
{

	[Hide]
	public static string BumpOffset => @"
float2 BumpOffset( float flHeightMap, float flDepthScale, float flReferencePlane, float2 vTextureCoords, float3 vTangentViewVector )
{
		float l_10 = flReferencePlane - flHeightMap;
		float2 l_11 = vTangentViewVector.xy * float2( l_10, l_10 );
	
		float2 l_13 = l_11 * float2( flDepthScale, flDepthScale );
		float2 l_14 = l_13 * float2( 0.1f, 0.1f );
		float2 l_15 = vTextureCoords.xy + l_14;

		return l_15;
}
";

	[Input( typeof( float ) )]
	[Title( "Height" )]
	[Hide]
	public NodeInput InputHeight { get; set; }

	[Input( typeof( float ) )]
	[Title( "Depth Scale" )]
	[Hide, Editor( nameof( DefaultDepthScale ) )]
	[MinMax( 0.0f, 1.0f )]
	public NodeInput InputDepthScale { get; set; }

	[Input( typeof( float ) )]
	[Title( "Reference Plane" )]
	[Hide, Editor( nameof( DefaultReferencePlane ) )]
	[MinMax( 0.0f, 1.0f )]
	public NodeInput InputReferencePlane { get; set; }

	[Input( typeof( Vector2 ) )]
	[Title( "Coords" )]
	[Hide]
	public NodeInput InputCoords { get; set; }

	[Title( "Height" )]
	public float DefaultHeight { get; set; } = 0.0f;

	[Title( "Depth Scale" )]
	[MinMax( 0.0f, 1.0f )]
	public float DefaultDepthScale { get; set; } = 0.125f;

	[Title( "ReferencePlane" )]
	[MinMax( 0.0f, 1.0f )]
	public float DefaultReferencePlane { get; set; } = 0.42f;

	[Output( typeof( Vector2 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var worldPos = $"i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz";
		var tangentViewVector = compiler.ResultFunction( "GetTangentViewVector", $"{worldPos}, i.vNormalWs, i.vTangentUWs, i.vTangentVWs" );

		var inputHeight = compiler.ResultOrDefault( InputHeight, DefaultHeight );
		var inputDepthScale = compiler.ResultOrDefault( InputDepthScale, DefaultDepthScale );
		var inputReferencePlane = compiler.ResultOrDefault( InputReferencePlane, DefaultReferencePlane );
		var inputCoords = compiler.Result( InputCoords );

		string func = compiler.RegisterFunction( BumpOffset );
		string funcCall = compiler.ResultFunction( func, 
			$"{inputHeight}, " +
			$"{inputDepthScale}, " +
			$"{inputReferencePlane}, " +
			$"{(inputCoords.IsValid ? $"{inputCoords.Cast( 2 )}" : "i.vTextureCoords.xy")}, " +
			$"{tangentViewVector}"
		);

		return new NodeResult( ResultType.Vector2, funcCall );
	};
}
