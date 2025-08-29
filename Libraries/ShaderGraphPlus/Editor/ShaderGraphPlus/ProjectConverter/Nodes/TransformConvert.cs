
using ShaderGraphPlus.Nodes;
using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

namespace ShaderGraphPlus.Internal;

internal class TileAndOffsetNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.TileAndOffset );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldTileAndOffsetNode = oldNode as VanillaNodes.TileAndOffset;

		//SGPLog.Info( "Convert tileAndOffset node" );

		var newNode = new TileAndOffset();

		newNode.DefaultTile = oldTileAndOffsetNode.DefaultTile;
		newNode.DefaultOffset = oldTileAndOffsetNode.DefaultOffset;
		newNode.WrapTo01 = oldTileAndOffsetNode.WrapTo01;
		newNode.Position = oldTileAndOffsetNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class BlendNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Blend );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldBlendNode = oldNode as VanillaNodes.Blend;

		//SGPLog.Info( "Convert blend node" );

		var newNode = new Blend();
		newNode.DefaultA = oldBlendNode.DefaultA;
		newNode.DefaultB = oldBlendNode.DefaultB;
		newNode.Fraction = oldBlendNode.Fraction;
		newNode.BlendMode = oldBlendNode.BlendMode switch
		{
			VanillaNodes.BlendNodeMode.Mix => BlendNodeMode.Mix,
			VanillaNodes.BlendNodeMode.Darken => BlendNodeMode.Darken,
			VanillaNodes.BlendNodeMode.Multiply => BlendNodeMode.Multiply,
			VanillaNodes.BlendNodeMode.ColorBurn => BlendNodeMode.ColorBurn,
			VanillaNodes.BlendNodeMode.LinearBurn => BlendNodeMode.LinearBurn,
			VanillaNodes.BlendNodeMode.Lighten => BlendNodeMode.Lighten,
			VanillaNodes.BlendNodeMode.Screen => BlendNodeMode.Screen,
			VanillaNodes.BlendNodeMode.ColorDodge => BlendNodeMode.ColorDodge,
			VanillaNodes.BlendNodeMode.LinearDodge => BlendNodeMode.LinearDodge,
			VanillaNodes.BlendNodeMode.Overlay => BlendNodeMode.Overlay,
			VanillaNodes.BlendNodeMode.SoftLight => BlendNodeMode.SoftLight,
			VanillaNodes.BlendNodeMode.HardLight => BlendNodeMode.HardLight,
			VanillaNodes.BlendNodeMode.VividLight => BlendNodeMode.VividLight,
			VanillaNodes.BlendNodeMode.LinearLight => BlendNodeMode.LinearLight,
			VanillaNodes.BlendNodeMode.HardMix => BlendNodeMode.HardMix,
			VanillaNodes.BlendNodeMode.Difference => BlendNodeMode.Difference,
			VanillaNodes.BlendNodeMode.Exclusion => BlendNodeMode.Exclusion,
			VanillaNodes.BlendNodeMode.Subtract => BlendNodeMode.Subtract,
			VanillaNodes.BlendNodeMode.Divide => BlendNodeMode.Divide,
			VanillaNodes.BlendNodeMode.Add => BlendNodeMode.Add,
			_ => throw new NotImplementedException(),
		};
		newNode.Clamp = oldBlendNode.Clamp;
		newNode.Position = oldBlendNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}
