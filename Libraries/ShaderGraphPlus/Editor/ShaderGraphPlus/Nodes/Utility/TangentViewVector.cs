namespace Editor.ShaderGraphPlus.Nodes;


/// <summary>
/// Gets the tangent view vector from the 4 inputs.
/// </summary>
[Title( "Get Tangent View Vector" ), Category( "Utility" )]
public sealed class GetTangentViewVectorNode : ShaderNodePlus
{
 
[Hide]
public static string GetTangentViewVector => @"
float3 GetTangentViewVector( float3 vPosition, float3 vNormalWs, float3 vTangentUWs, float3 vTangentVWs)
{
    float3 vCameraToPositionDirWs = CalculateCameraToPositionDirWs( vPosition.xyz );
    vNormalWs = normalize( vNormalWs.xyz );
   	float3 vTangentViewVector = Vec3WsToTs( vCameraToPositionDirWs.xyz, vNormalWs.xyz, vTangentUWs.xyz, vTangentVWs.xyz );
	
	// Result
	return vTangentViewVector.xyz;
}
";

	[Title( "World Pos" )]
	[Description("")]
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput WorldSpacePosition { get; set; }

	[Title( "World Normal" )]
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput WorldNormal { get; set; }

	[Title( "TangentUWs" )]
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput TangentUWs { get; set; }

	[Title( "TangentVWs" )]
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput TangentVWs { get; set; }

	public GetTangentViewVectorNode()
	{
		ExpandSize = new Vector2( 32, 0 );
	}

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		if ( compiler.Graph.MaterialDomain is MaterialDomain.PostProcess )
		{
			return NodeResult.Error( $"{DisplayInfo.Name} Is not ment for postprocessing shaders!" );
		}

		var worldspaceposition = compiler.Result( WorldSpacePosition );
		var worldnormal = compiler.Result( WorldNormal );
		var tangentuws = compiler.Result( TangentUWs );
		var tangentvws = compiler.Result( TangentVWs );


		if ( !worldspaceposition.IsValid() )
		{
			return NodeResult.MissingInput( nameof( WorldSpacePosition ) );
		}
		
		if ( !worldnormal.IsValid() )
		{
			return NodeResult.MissingInput( nameof( WorldNormal ) );
		}
		
		if ( !tangentuws.IsValid() )
		{
			return NodeResult.MissingInput( nameof( TangentUWs ) );
		}

		if ( !tangentvws.IsValid() )
		{
			return NodeResult.MissingInput( nameof( TangentVWs ) );
		}

		return new NodeResult( ResultType.Vector3, compiler.ResultFunction( GetTangentViewVector,
			args:
			$"{worldspaceposition}" +
			$",{worldnormal}" +
            $",{tangentuws}" +
            $",{tangentvws}" 
		) );
	};
}
