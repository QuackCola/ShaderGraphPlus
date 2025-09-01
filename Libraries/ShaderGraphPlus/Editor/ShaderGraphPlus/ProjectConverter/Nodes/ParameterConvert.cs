using Editor;
using ShaderGraphPlus.Nodes;
using VanillaGraph = Editor.ShaderGraph;
using VanillaNodes = Editor.ShaderGraph.Nodes;
using ShaderGraphBaseNode = Editor.ShaderGraph.BaseNode;

namespace ShaderGraphPlus.Internal;

file static class VanillaParameterUIExentions
{
	internal static ParameterUI ConvertVanillaUI( this VanillaGraph.ParameterUI parameterUI )
	{
		var newUi = new ParameterUI();

		newUi.Type = parameterUI.Type switch
		{
			VanillaGraph.UIType.Default => UIType.Default,
			VanillaGraph.UIType.Slider => UIType.Slider,
			VanillaGraph.UIType.Color => UIType.Color,
			_ => throw new NotImplementedException(),
		};

		newUi.Step = parameterUI.Step;
		newUi.Priority = parameterUI.Priority;
		newUi.PrimaryGroup = new() { Name = parameterUI.PrimaryGroup.Name, Priority = parameterUI.PrimaryGroup.Priority };
		newUi.SecondaryGroup = new() { Name = parameterUI.SecondaryGroup.Name, Priority = parameterUI.SecondaryGroup.Priority };

		return newUi;
	}

}

internal class FloatNodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Float );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldFloatNode = oldNode as VanillaNodes.Float;

		//SGPLog.Info( "Convert float node" );

		var newNode = new Float();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.Value = oldFloatNode.Value;
		newNode.Name = oldFloatNode.Name;
		newNode.IsAttribute = oldFloatNode.IsAttribute;
		newNode.UI = oldFloatNode.UI.ConvertVanillaUI();
		newNode.Min = oldFloatNode.Min;
		newNode.Max = oldFloatNode.Max;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class Float2NodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Float2 );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldFloat2Node = oldNode as VanillaNodes.Float2;

		//SGPLog.Info( "Convert float2 node" );

		var newNode = new Float2();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.Value = oldFloat2Node.Value;
		newNode.Name = oldFloat2Node.Name;
		newNode.IsAttribute = oldFloat2Node.IsAttribute;
		newNode.UI = oldFloat2Node.UI.ConvertVanillaUI();
		newNode.Min = oldFloat2Node.Min;
		newNode.Max = oldFloat2Node.Max;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class Float3NodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Float3 );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldFloat3Node = oldNode as VanillaNodes.Float3;

		//SGPLog.Info( "Convert float3 node" );

		var newNode = new Float3();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.Value = oldFloat3Node.Value;
		newNode.Name = oldFloat3Node.Name;
		newNode.IsAttribute = oldFloat3Node.IsAttribute;
		newNode.UI = oldFloat3Node.UI.ConvertVanillaUI();
		newNode.Min = oldFloat3Node.Min;
		newNode.Max = oldFloat3Node.Max;

		newNodes.Add( newNode );

		return newNodes;
	}
}

internal class Float4NodeConvert : BaseNodeConvert
{
	public override Type NodeTypeToConvert => typeof( VanillaNodes.Float4 );

	public override IEnumerable<BaseNodePlus> Convert( ProjectConverter converter, ShaderGraphBaseNode oldNode )
	{
		var newNodes = new List<BaseNodePlus>();
		var oldFloat4Node = oldNode as VanillaNodes.Float4;

		//SGPLog.Info( "Convert float4 node" );

		var newNode = new Float4();
		newNode.Identifier = oldNode.Identifier;
		newNode.Position = oldNode.Position;
		newNode.Value = oldFloat4Node.Value;
		newNode.Name = oldFloat4Node.Name;
		newNode.IsAttribute = oldFloat4Node.IsAttribute;
		newNode.UI = oldFloat4Node.UI.ConvertVanillaUI();

		newNodes.Add( newNode );

		return newNodes;
	}
}
