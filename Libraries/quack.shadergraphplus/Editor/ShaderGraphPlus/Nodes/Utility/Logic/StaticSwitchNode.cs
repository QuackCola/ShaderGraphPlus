using NodeEditorPlus;
using GraphView = NodeEditorPlus.GraphView;
using IPlugOut = NodeEditorPlus.IPlugOut;
using NodeUI = NodeEditorPlus.NodeUI;

namespace ShaderGraphPlus.Nodes;

// TOODO : Remove this.
/*
[Title( "Static Combo Switch" ), Category( "Utility/Logic" ), Icon( "alt_route" )]
[InternalNode]
public sealed class StaticSwitchNode : ShaderNodePlus, IBlackboardSyncable
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleTintColor => PrimaryNodeHeaderColors.LogicNode;

	[Hide, Browsable( false )]
	public Guid BlackboardParameterIdentifier { get; set; }

	[Hide]
	public override string Title
	{
		get
		{

			return $"{DisplayInfo.For( this ).Name} ( F_{Feature.Name.ToUpper().Replace( " ", "_" )} )";
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
		if ( parameter is ShaderFeatureBooleanParameter sfboolParameter )
		{
			Feature = new ShaderFeatureBoolean 
			{ 
				Name = sfboolParameter.Name,
				Description = sfboolParameter.Description,
				HeaderName = sfboolParameter.HeaderName,
			};
		}
	}

	[Output, Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		if ( string.IsNullOrWhiteSpace( Feature.Name ) )
			return NodeResult.Error( "Feature name cannot be blank." );

		return new NodeResult( ResultType.Float, $"{1.0f}" );

		//if ( compiler.ShaderFeatures.ContainsKey( Feature.Name ) )//&& FeatureReference != "None" )
		//{
		//	//SGPLog.Info( $"GraphFeatures contains feature {FeatureReference} ? : {compiler.Graph.Features.ContainsKey( FeatureReference )}", compiler.IsNotPreview );
		//
		//	if ( compiler.GenerateComboSwitch( compiler.ShaderFeatures[Feature.Name], InputTrue, InputFalse, PreviewToggle, true, out var switchResultVariableName, out var switchBody, out var switchResultType ) )
		//	{
		//		var result = new NodeResult( switchResultType, switchResultVariableName, constant: false );
		//
		//		result.SetMetadata( nameof( MetadataType.ComboSwitchBody ), switchBody );
		//
		//		return result;
		//	}
		//	else
		//	{
		//		return new NodeResult( ResultType.Float, $"{1.0f}" );
		//		//return NodeResult.Error( "Switch body could not be generated!" );
		//	}
		//}
		//else
		//{
		//	return NodeResult.Error( $"Shader Featue \"{Feature.Name}\" is not a valid registerd feature." );
		//}
	};
}
*/
