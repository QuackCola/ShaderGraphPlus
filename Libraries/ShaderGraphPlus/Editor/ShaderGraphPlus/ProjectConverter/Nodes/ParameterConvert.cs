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

		if ( string.IsNullOrWhiteSpace( oldFloatNode.Name ) )
		{
			var newConstantNode = new FloatConstantNode();
			newConstantNode.Value = oldFloatNode.Value;

			newNodes.Add( newConstantNode );
		}
		else
		{
			var newNode = new FloatParameterNode();
			newNode.Identifier = oldNode.Identifier;
			newNode.Position = oldNode.Position;
			newNode.Value = oldFloatNode.Value;
			newNode.Name = oldFloatNode.Name;
			newNode.IsAttribute = oldFloatNode.IsAttribute;
			newNode.UI = oldFloatNode.UI.ConvertVanillaUI();
			newNode.Min = oldFloatNode.Min;
			newNode.Max = oldFloatNode.Max;
			newNode.BlackboardParameterIdentifier = Guid.NewGuid();

			BaseBlackboardParameter blackboardParameter = new FloatBlackboardParameter()
			{
				Name = newNode.Name,
				Value = newNode.Value,
				UI = newNode.UI,
				Identifier = newNode.BlackboardParameterIdentifier,
			};

			converter.AddBlackboardParameter( blackboardParameter );

			newNodes.Add( newNode );
		}

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

		if ( string.IsNullOrWhiteSpace( oldFloat2Node.Name ) )
		{
			var newConstantNode = new Float2ConstantNode();
			newConstantNode.Value = oldFloat2Node.Value;

			newNodes.Add( newConstantNode );
		}
		else
		{
			var newNode = new Float2ParameterNode();
			newNode.Identifier = oldNode.Identifier;
			newNode.Position = oldNode.Position;
			newNode.Value = oldFloat2Node.Value;
			newNode.Name = oldFloat2Node.Name;
			newNode.IsAttribute = oldFloat2Node.IsAttribute;
			newNode.UI = oldFloat2Node.UI.ConvertVanillaUI();
			newNode.Min = oldFloat2Node.Min;
			newNode.Max = oldFloat2Node.Max;
			newNode.BlackboardParameterIdentifier = Guid.NewGuid();

			BaseBlackboardParameter blackboardParameter = new Float2BlackboardParameter()
			{
				Name = newNode.Name,
				Value = newNode.Value,
				UI = newNode.UI,
				Identifier = newNode.BlackboardParameterIdentifier,
			};

			converter.AddBlackboardParameter( blackboardParameter );

			newNodes.Add( newNode );
		}

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

		if ( string.IsNullOrWhiteSpace( oldFloat3Node.Name ) )
		{
			var newConstantNode = new Float3ConstantNode();
			newConstantNode.Value = oldFloat3Node.Value;

			newNodes.Add( newConstantNode );
		}
		else
		{
			var newNode = new Float3ParameterNode();
			newNode.Identifier = oldNode.Identifier;
			newNode.Position = oldNode.Position;
			newNode.Value = oldFloat3Node.Value;
			newNode.Name = oldFloat3Node.Name;
			newNode.IsAttribute = oldFloat3Node.IsAttribute;
			newNode.UI = oldFloat3Node.UI.ConvertVanillaUI();
			newNode.Min = oldFloat3Node.Min;
			newNode.Max = oldFloat3Node.Max;
			newNode.BlackboardParameterIdentifier = Guid.NewGuid();

			BaseBlackboardParameter blackboardParameter = new Float3BlackboardParameter()
			{
				Name = newNode.Name,
				Value = newNode.Value,
				UI = newNode.UI,
				Identifier = newNode.BlackboardParameterIdentifier,
			};

			converter.AddBlackboardParameter( blackboardParameter );

			newNodes.Add( newNode );
		}

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

		if ( string.IsNullOrWhiteSpace( oldFloat4Node.Name ) )
		{
			var newConstantNode = new ColorConstantNode();
			newConstantNode.Value = oldFloat4Node.Value;

			newNodes.Add( newConstantNode );
		}
		else
		{
			var newNode = new ColorParameterNode();
			newNode.Identifier = oldNode.Identifier;
			newNode.Position = oldNode.Position;
			newNode.Value = oldFloat4Node.Value;
			newNode.Name = oldFloat4Node.Name;
			newNode.IsAttribute = oldFloat4Node.IsAttribute;
			newNode.UI = oldFloat4Node.UI.ConvertVanillaUI();
			newNode.BlackboardParameterIdentifier = Guid.NewGuid();

			BaseBlackboardParameter blackboardParameter = new ColorBlackboardParameter()
			{
				Name = newNode.Name,
				Value = newNode.Value,
				UI = newNode.UI,
				Identifier = newNode.BlackboardParameterIdentifier,
			};

			converter.AddBlackboardParameter( blackboardParameter );

			newNodes.Add( newNode );
		}

		return newNodes;
	}
}
