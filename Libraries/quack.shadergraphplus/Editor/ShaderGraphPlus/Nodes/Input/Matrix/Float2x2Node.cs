using NodeEditorPlus;
using GraphView = NodeEditorPlus.GraphView;
using NodeUI = NodeEditorPlus.NodeUI;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;

namespace ShaderGraphPlus.Nodes;

[Title( "Float 2x2" ), Category( "Constants/Matrix" ), Icon( "dataset" ), Order( 9 )]
public sealed class Float2x2Node : MatrixParameterNode<Float2x2>
{
	[Hide] public override int Version => 1;

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Output( typeof( Float2x2 ) ), Title( "Value" )]
	[Hide]
	[NodeValueEditor( nameof( Value ) )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return compiler.ResultParameter( Name, Value, default, default, false, IsAttribute, default );
	};
}
