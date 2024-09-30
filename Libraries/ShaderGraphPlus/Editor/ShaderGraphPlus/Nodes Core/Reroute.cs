﻿
namespace Editor.ShaderGraphPlus.Nodes;

public abstract class RerouteNode : BaseNodePlus, IRerouteNode
{
	/// <summary>
	/// Comment to show above this node
	/// </summary>
	public string Comment { get; set; }

	[Input, Hide, Title( "" )]
	public NodeInput Input { get; set; }

	public override NodeUI CreateUI( GraphView view )
	{
		return new RerouteUI( view, this );
	}
}

public sealed class ReroutePlus : RerouteNode
{
	[Output, Hide,Title( "" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var result = compiler.ResultOrDefault( Input, 0.0f );
		result.Constant = true;
		return result;
	};
}
