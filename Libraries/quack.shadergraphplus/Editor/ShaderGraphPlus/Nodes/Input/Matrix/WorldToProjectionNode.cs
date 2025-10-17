﻿using NodeEditorPlus;
using GraphView = NodeEditorPlus.GraphView;
using NodeUI = NodeEditorPlus.NodeUI;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;

namespace ShaderGraphPlus.Nodes;

[Title( "World To Projection" ), Category( "Variables/Matrix" ), Icon( "dataset" )]
public sealed class WorldToProjectionNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleColor => PrimaryNodeHeaderColors.GlobalVariableNode;

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	[Output( typeof( Float4x4 ) ), Title( "Value" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Float4x4, "g_matWorldToProjection", true );
	};
}
