namespace Editor.ShaderGraphPlus.Nodes;


/// <summary>
/// Gets the tangent view vector from the 4 inputs.
/// </summary>
[Title( "Get Tangent View Vector" ), Category( "Utility" )]
public sealed class GetTangentViewVectorNode : ShaderNodePlus
{
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

		var result = compiler.ResultFunction( "GetTangentViewVector", $"{worldspaceposition}",
				$"{worldnormal}",
				$"{tangentuws}",
				$"{tangentvws}" 
		);

		return new NodeResult( ResultType.Vector3, $"{result}" );
	};
}
