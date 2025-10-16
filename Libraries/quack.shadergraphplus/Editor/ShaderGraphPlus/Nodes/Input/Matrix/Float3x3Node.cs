using NodeEditorPlus;
using GraphView = NodeEditorPlus.GraphView;
using NodeUI = NodeEditorPlus.NodeUI;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;

namespace ShaderGraphPlus.Nodes;

[Title( "Float 3x3" ), Category( "Constants/Matrix" ), Icon( "dataset" ), Order( 10 )]
public sealed class Float3x3Node : MatrixParameterNode<Float3x3>
{
	[Hide] public override int Version => 1;

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Output( typeof( Float3x3 ) ), Title( "Value" )]
	[Hide]
	[NodeValueEditor( nameof( Value ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( Name, Value, default, default, false, IsAttribute, default );
	};
}
