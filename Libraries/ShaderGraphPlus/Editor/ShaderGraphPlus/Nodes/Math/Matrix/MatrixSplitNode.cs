namespace ShaderGraphPlus.Nodes;

// TODO : Finish Implementing This!
#if false
/// <summary>
/// 
/// </summary>
[Title( "Matrix Split" ), Category( "Math/Matrix" ), Icon( "device_hub" )]
public sealed class MatrixSplitNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Input]
	[Hide]
	public NodeInput Input { get; set; }

	public MatrixNodeMode Mode { get; set; } = MatrixNodeMode.Row;

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Output( typeof( float ) ), Title( "M0" )]
	[Hide]
	public NodeResult.Func ResultA => ( GraphCompiler compiler ) =>
	{
		var input = compiler.Result( Input );
		//var resultType = ResultType.Invalid;

		if ( !input.IsValid )
		{
			return new NodeResult( ResultType.Float, "0.0f" );
		}

		switch ( input.ResultType )
		{
			case ResultType.Float2x2:
				//resultType = ResultType.Float2x2;
				break;
			case ResultType.Float3x3:
				//resultType = ResultType.Float3x3;
				break;
			case ResultType.Float4x4:
				//resultType = ResultType.Float4x4;
				break;
			default:
				return NodeResult.Error( $"ResultType `{input.ResultType}` is not one of the avalible matrix types." );
		}

		return new NodeResult( ResultType.Float, "0.0f" );
	};

	[Output( typeof( float ) ), Title( "M1" )]
	[Hide]
	public NodeResult.Func ResultB => ( GraphCompiler compiler ) =>
	{
		var input = compiler.Result( Input );
		//var resultType = ResultType.Invalid;

		if ( !input.IsValid )
		{
			return new NodeResult( ResultType.Float, "0.0f" );
		}

		switch ( input.ResultType )
		{
			case ResultType.Float2x2:
				//resultType = ResultType.Float2x2;
				break;
			case ResultType.Float3x3:
				//resultType = ResultType.Float3x3;
				break;
			case ResultType.Float4x4:
				//resultType = ResultType.Float4x4;
				break;
			default:
				return NodeResult.Error( $"ResultType `{input.ResultType}` is not one of the avalible matrix types." );
		}

		return new NodeResult( ResultType.Float, "0.0f" );
	};

	[Output( typeof( float ) ), Title( "M2" )]
	[Hide]
	public NodeResult.Func ResultC => ( GraphCompiler compiler ) =>
	{
		var input = compiler.Result( Input );
		//var resultType = ResultType.Invalid;

		if ( !input.IsValid )
		{
			return new NodeResult( ResultType.Float, "0.0f" );
		}

		switch ( input.ResultType )
		{
			case ResultType.Float2x2:
				//resultType = ResultType.Float2x2;
				break;
			case ResultType.Float3x3:
				//resultType = ResultType.Float3x3;
				break;
			case ResultType.Float4x4:
				//resultType = ResultType.Float4x4;
				break;
			default:
				return NodeResult.Error( $"ResultType `{input.ResultType}` is not one of the avalible matrix types." );
		}

		return new NodeResult( ResultType.Float, "0.0f" );
	};

	[Output( typeof( float ) ), Title( "M3" )]
	[Hide]
	public NodeResult.Func ResultD => ( GraphCompiler compiler ) =>
	{
		var input = compiler.Result( Input );
		//var resultType = ResultType.Invalid;

		if ( !input.IsValid )
		{
			return new NodeResult( ResultType.Float, "0.0f" );
		}

		switch ( input.ResultType )
		{
			case ResultType.Float2x2:
				//resultType = ResultType.Float2x2;
				break;
			case ResultType.Float3x3:
				//resultType = ResultType.Float3x3;
				break;
			case ResultType.Float4x4:
				//resultType = ResultType.Float4x4;
				break;
			default:
				return NodeResult.Error( $"ResultType `{input.ResultType}` is not one of the avalible matrix types." );
		}

		return new NodeResult( ResultType.Float, "0.0f" );
	};

}
#endif
