using NodeEditorPlus;
using static Sandbox.Material;
using GraphView = NodeEditorPlus.GraphView;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;
using NodeUI = NodeEditorPlus.NodeUI;

namespace ShaderGraphPlus.Nodes;

[System.AttributeUsage( AttributeTargets.Property )]
internal sealed class ShaderFeatureInfoReferenceAttribute : Attribute
{ 
}

[Title( "Static Combo Switch" ), Category( "Utility/Logic" ), Icon( "alt_route" )]
public sealed class StaticSwitchNode : ShaderNodePlus, IBlackboardSyncable
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color PrimaryHeaderColor => PrimaryNodeHeaderColors.LogicNode;

	[Hide, Browsable( false )]
	public Guid BlackboardParameterIdentifier { get; set; }

	[Hide]
	public override string Title
	{
		get
		{
			return $"{DisplayInfo.For( this ).Name} ( F_{Feature.FeatureName.ToUpper().Replace( " ", "_" )} )";
		}
	}

	[Hide]
	[JsonIgnore]
	public override bool CanPreview => false;

	[Input]
	[Title( "True" )]
	[Hide]
	public NodeInput InputTrue { get; set; }

	[Input]
	[Title( "False" )]
	[Hide]
	public NodeInput InputFalse { get; set; }

	//[ShowIf( nameof( Mode ), StaticSwitchMode.Create )]
	public bool PreviewToggle { get; set; } = false;

	[Hide]
	public ShaderFeatureBoolean Feature { get; set; } = new();

	public StaticSwitchNode() : base()
	{
		ExpandSize = new Vector2( 48 , 0 );
	}

	public void OnNodeCreated()
	{
		Update();
	}

	public void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is ShaderFeatureBooleanBlackboardParameter sfboolParameter )
		{
			Feature = sfboolParameter.Value;
		}
	}

	[Output, Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		if ( string.IsNullOrWhiteSpace( Feature.FeatureName ) )
			return NodeResult.Error( "Feature name cannot be blank." );

		if ( compiler.ShaderFeatures.ContainsKey( Feature.FeatureName ) )//&& FeatureReference != "None" )
		{
			//SGPLog.Info( $"GraphFeatures contains feature {FeatureReference} ? : {compiler.Graph.Features.ContainsKey( FeatureReference )}", compiler.IsNotPreview );

			if ( compiler.GenerateComboSwitch( compiler.ShaderFeatures[Feature.FeatureName], InputTrue, InputFalse, PreviewToggle, true, out var switchResultVariableName, out var switchBody, out var switchResultType ) )
			{
				var result = new NodeResult( switchResultType, switchResultVariableName, constant: false );

				result.SetMetadata( nameof( MetadataType.ComboSwitchBody ), switchBody );

				return result;
			}
			else
			{
				return new NodeResult( ResultType.Float, $"{1.0f}" );
				//return NodeResult.Error( "Switch body could not be generated!" );
			}
		}
		else
		{
			return NodeResult.Error( $"Shader Featue \"{Feature.FeatureName}\" is not a valid registerd feature." );
		}
	};
}
