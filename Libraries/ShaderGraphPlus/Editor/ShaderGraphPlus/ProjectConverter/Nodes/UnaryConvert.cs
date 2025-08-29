using Editor;
using ShaderGraphPlus.Nodes;
using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

namespace ShaderGraphPlus.Internal;

internal class CosineNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Cosine );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldCosineNode = oldNode as VanillaNodes.Cosine;

		//SGPLog.Info( "Convert cosine node" );

		var newNode = new Cosine();
		newNode.Position = oldCosineNode.Position;
		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class AbsNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Abs );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldAbsNode = oldNode as VanillaNodes.Abs;

		//SGPLog.Info( "Convert abs node" );

		var newNode = new Abs();
		newNode.Position = oldAbsNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class DotProductNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.DotProduct );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldDotProductNode = oldNode as VanillaNodes.DotProduct;

		//SGPLog.Info( "Convert dotProduct node" );

		var newNode = new DotProduct();
		newNode.Position = oldDotProductNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class DDXNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.DDX );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldDDXNode = oldNode as VanillaNodes.DDX;

		//SGPLog.Info( "Convert dDX node" );

		var newNode = new DDX();
		newNode.Precision = oldDDXNode.Precision switch
		{
			VanillaNodes.DerivativePrecision.Standard => DerivativePrecision.Standard,
			VanillaNodes.DerivativePrecision.Course => DerivativePrecision.Course,
			VanillaNodes.DerivativePrecision.Fine => DerivativePrecision.Fine,
			_ => throw new NotImplementedException(),
		};
		newNode.Position = oldDDXNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class DDYNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.DDY );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldDDYNode = oldNode as VanillaNodes.DDY;

		//SGPLog.Info( "Convert dDY node" );

		var newNode = new DDY();
		newNode.Precision = oldDDYNode.Precision switch
		{
			VanillaNodes.DerivativePrecision.Standard => DerivativePrecision.Standard,
			VanillaNodes.DerivativePrecision.Course => DerivativePrecision.Course,
			VanillaNodes.DerivativePrecision.Fine => DerivativePrecision.Fine,
			_ => throw new NotImplementedException(),
		};
		newNode.Position = oldDDYNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class DDXYNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.DDXY );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldDDXYNode = oldNode as VanillaNodes.DDXY;

		//SGPLog.Info( "Convert dDXY node" );

		var newNode = new DDXY();
		newNode.Position = oldDDXYNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class ExponentialNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Exponential );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldExponentialNode = oldNode as VanillaNodes.Exponential;

		//SGPLog.Info( "Convert exponential node" );

		var newNode = new Exponential();
		newNode.Base = oldExponentialNode.Base switch
		{
			VanillaNodes.ExponentBase.BaseE => ExponentBase.BaseE,
			VanillaNodes.ExponentBase.Base2 => ExponentBase.Base2,
			_ => throw new NotImplementedException(),
		};
		newNode.Position = oldExponentialNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class FracNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Frac );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldFracNode = oldNode as VanillaNodes.Frac;

		//SGPLog.Info( "Convert frac node" );

		var newNode = new Frac();
		newNode.Position = oldFracNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class FloorNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Floor );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldFloorNode = oldNode as VanillaNodes.Floor;

		//SGPLog.Info( "Convert floor node" );

		var newNode = new Floor();
		newNode.Position = oldFloorNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class LengthNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Length );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldLengthNode = oldNode as VanillaNodes.Length;

		//SGPLog.Info( "Convert length node" );

		var newNode = new Length();
		newNode.Position = oldLengthNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class BaseLogNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.BaseLog );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldBaseLogNode = oldNode as VanillaNodes.BaseLog;

		//SGPLog.Info( "Convert baseLog node" );

		var newNode = new BaseLog();
		newNode.Base = oldBaseLogNode.Base switch
		{
			VanillaNodes.LogBase.BaseE => LogBase.BaseE,
			VanillaNodes.LogBase.Base2 => LogBase.Base2,
			VanillaNodes.LogBase.Base10 => LogBase.Base10,
			_ => throw new NotImplementedException(),
		};
		newNode.Position = oldBaseLogNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class MinNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Min );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldMinNode = oldNode as VanillaNodes.Min;

		//SGPLog.Info( "Convert min node" );

		var newNode = new Min();
		newNode.DefaultA = oldMinNode.DefaultA;
		newNode.DefaultB = oldMinNode.DefaultB;
		newNode.Position = oldMinNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class MaxNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Max );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldMaxNode = oldNode as VanillaNodes.Max;

		//SGPLog.Info( "Convert max node" );

		var newNode = new Max();
		newNode.DefaultA = oldMaxNode.DefaultA;
		newNode.DefaultB = oldMaxNode.DefaultB;
		newNode.Position = oldMaxNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class SaturateNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Saturate );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldSaturateNode = oldNode as VanillaNodes.Saturate;

		//SGPLog.Info( "Convert saturate node" );

		var newNode = new Saturate();
		newNode.Position = oldSaturateNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class SineNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Sine );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldSineNode = oldNode as VanillaNodes.Sine;

		//SGPLog.Info( "Convert sine node" );

		var newNode = new Sine();
		newNode.Position = oldSineNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class StepNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Step );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldStepNode = oldNode as VanillaNodes.Step;

		//SGPLog.Info( "Convert step node" );

		var newNode = new Step();
		newNode.DefaultInput = oldStepNode.DefaultInput;
		newNode.DefaultEdge = oldStepNode.DefaultEdge;
		newNode.Position = oldStepNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class SmoothStepNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.SmoothStep );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldSmoothStepNode = oldNode as VanillaNodes.SmoothStep;

		//SGPLog.Info( "Convert smoothStep node" );

		var newNode = new SmoothStep();
		newNode.DefaultInput = oldSmoothStepNode.DefaultInput;
		newNode.DefaultEdge1 = oldSmoothStepNode.DefaultEdge1;
		newNode.DefaultEdge2 = oldSmoothStepNode.DefaultEdge2;
		newNode.Position = oldSmoothStepNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class TanNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Tan );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldTanNode = oldNode as VanillaNodes.Tan;

		//SGPLog.Info( "Convert tan node" );

		var newNode = new Tan();
		newNode.Position = oldTanNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class ArcsinNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Arcsin );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldArcsinNode = oldNode as VanillaNodes.Arcsin;

		//SGPLog.Info( "Convert arcsin node" );

		var newNode = new Arcsin();
		newNode.Position = oldArcsinNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class ArccosNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Arccos );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldArccosNode = oldNode as VanillaNodes.Arccos;

		//SGPLog.Info( "Convert arccos node" );

		var newNode = new Arccos();
		newNode.Position = oldArccosNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class RoundNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Round );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldRoundNode = oldNode as VanillaNodes.Round;

		//SGPLog.Info( "Convert round node" );

		var newNode = new Round();
		newNode.Position = oldRoundNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class CeilNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Ceil );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldCeilNode = oldNode as VanillaNodes.Ceil;

		//SGPLog.Info( "Convert ceil node" );

		var newNode = new Ceil();
		newNode.Position = oldCeilNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class OneMinusNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.OneMinus );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldOneMinusNode = oldNode as VanillaNodes.OneMinus;

		//SGPLog.Info( "Convert oneMinus node" );

		var newNode = new OneMinus();
		newNode.Position = oldOneMinusNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class SqrtNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Sqrt );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldSqrtNode = oldNode as VanillaNodes.Sqrt;

		//SGPLog.Info( "Convert sqrt node" );

		var newNode = new Sqrt();
		newNode.Position = oldSqrtNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class DistanceNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Distance );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldDistanceNode = oldNode as VanillaNodes.Distance;

		//SGPLog.Info( "Convert distance node" );

		var newNode = new Distance();
		newNode.Position = oldDistanceNode.Position;

		newNodes.Add( newNode );

		return newNodes;
	}
}
