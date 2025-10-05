using Editor;

using NodeEditorPlus;
using GraphView = NodeEditorPlus.GraphView;
using NodeUI = NodeEditorPlus.NodeUI;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;
using IRerouteNode = NodeEditorPlus.IRerouteNode;
using RerouteUI = NodeEditorPlus.RerouteUI;

namespace ShaderGraphPlus.Nodes;

public abstract class RerouteNode : BaseNodePlus, IRerouteNode
{
	/// <summary>
	/// Comment to show above this node
	/// </summary>
	public string Comment { get; set; }

	[Input, Hide, Title( "" )]
	public NodeInput Input { get; set; }

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	public override NodeUI CreateUI( GraphView view )
	{
		return new RerouteUI( view, this );
	}
}

public sealed class ReroutePlus : RerouteNode
{
	[Hide]
	public override int Version => 1;

	[Output, Hide,Title( "" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var result = compiler.ResultOrDefault( Input, 0.0f );
		result.Constant = true;
		return result;
	};
}
