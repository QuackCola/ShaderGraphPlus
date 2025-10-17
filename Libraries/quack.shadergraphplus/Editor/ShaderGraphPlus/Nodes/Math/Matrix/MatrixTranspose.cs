using Editor;
using NodeEditorPlus;
using GraphView = NodeEditorPlus.GraphView;
using NodeUI = NodeEditorPlus.NodeUI;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;

namespace ShaderGraphPlus.Nodes;

[Title( "Matrix Transpose" ), Category( "Math/Matrix" ), Icon( "table_convert" )]
public sealed class MatrixTranspose : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Input]
	[Hide]
	public NodeInput Input { get; set; }

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = compiler.Result( Input );
		var resultType = ResultType.Invalid;
		
		if ( !input.IsValid )
		{
			return new NodeResult( ResultType.Float2x2, $"transpose( float2x2( 0, 0, 0, 0 ) )" );
		}

		switch ( input.ResultType )
		{
			case ResultType.Float2x2:
				resultType = ResultType.Float2x2;
				break;
			case ResultType.Float3x3:
				resultType = ResultType.Float3x3;
				break;
			case ResultType.Float4x4:
				resultType = ResultType.Float4x4;
				break;
			default:
				return NodeResult.Error( $"ResultType `{input.ResultType}` is not one of the avalible matrix types." );
		}


		return new NodeResult( resultType, $"transpose( {input.Code} )" );
	};
}
