using System.Linq;

namespace ShaderGraphPlus.Nodes;

public enum StaticSwitchMode
{
	/// <summary>
	/// Create a new Shader Feature from this node.
	/// </summary>
	Create,
	/// <summary>
	/// Reference an existing Shader Feature.
	/// </summary>
	Reference
}

[System.AttributeUsage( AttributeTargets.Property )]
internal sealed class ShaderFeatureInfoReferenceAttribute : Attribute
{ 
}

[Title( "Static Combo Switch" ), Category( "Utility" ), Icon( "alt_route" )]
public sealed class StaticSwitchNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Hide]
	public override string Title
	{
		get
		{
			if ( Mode == StaticSwitchMode.Create )
			{
				return $"{DisplayInfo.For( this ).Name} ( {Feature.FeatureName} )";
			}
			else
			{
				return $"{DisplayInfo.For( this ).Name} Ref ( {FeatureReference} )";
			}
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

	[ShowIf( nameof( Mode ), StaticSwitchMode.Create )]
	public bool PreviewToggle { get; set; } = false;

	//[Sandbox.ReadOnly]
	public StaticSwitchMode Mode { get; set; } = StaticSwitchMode.Create;

	[InlineEditor( Label = false ), ShowIf( nameof( Mode ), StaticSwitchMode.Create ), Group( "Feature" )]
	public ShaderFeature Feature { get; set; } = new();

	//[JsonIgnore]
	[Title( "Feature" ), ShowIf( nameof( Mode ), StaticSwitchMode.Reference )]
	//[Editor( "FeatureReference" )]
	[ShaderFeatureInfoReference]
	public string FeatureReference { get; set; } = "None";

	public StaticSwitchNode() : base()
	{
		ExpandSize = new Vector2( 8 + Inputs.Count() * 8, 0 );
	}

	public void OnNodeCreated()
	{
		Update();
	}

	[Output, Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		if ( Mode == StaticSwitchMode.Create && string.IsNullOrWhiteSpace( Feature.FeatureName ) )
			return NodeResult.Error( "Feature must have a valid name!" );

		//if ( Mode == StaticSwitchMode.Create && compiler.ShaderFeatures.ContainsKey( Feature.FeatureName ) )
		//	return NodeResult.Error( $"Feature name `{Feature.FeatureName}` is already registered!" );

		if ( Mode is StaticSwitchMode.Create )
		{
			if ( compiler.ShaderFeatures.TryGetValue( Feature.FeatureName, out var shaderFeature ) )
			{
				if ( compiler.GenerateComboSwitch( shaderFeature, InputTrue, InputFalse, PreviewToggle, false, out var switchResultVariableName, out var switchBody, out var switchResultType ) )
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
				return NodeResult.Error( "Feature Is Invalid!" );
			}
		}
		else
		{
			if ( compiler.ShaderFeatures.ContainsKey( FeatureReference ) && FeatureReference != "None" )
			{
				//SGPLog.Info( $"GraphFeatures contains feature {FeatureReference} ? : {compiler.Graph.Features.ContainsKey( FeatureReference )}", compiler.IsNotPreview );

				if ( compiler.GenerateComboSwitch( compiler.ShaderFeatures[FeatureReference], InputTrue, InputFalse, PreviewToggle, true, out var switchResultVariableName, out var switchBody, out var switchResultType ) )
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
				return NodeResult.Error( "You must select an available registered feature from the dropdown!" );
			}

		}
	};
}
