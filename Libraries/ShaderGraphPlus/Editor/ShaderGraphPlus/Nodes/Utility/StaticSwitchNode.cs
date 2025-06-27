namespace Editor.ShaderGraphPlus.Nodes;

[Title( "Static Switch" ), Category( "Utility" )]
public sealed class StaticSwitchNode : ShaderNodePlus
{

	[Hide]
	public override string Title => $"{DisplayInfo.For( this ).Name} ({Feature.FeatureName})";

	[Input]
	[Title( "True" )]
	[Hide]
	public NodeInput InputTrue { get; set; }

	[Input]
	[Title( "False" )]
	[Hide]
	public NodeInput InputFalse { get; set; }

	[Hide,JsonIgnore]
	public string InternalName { get; set; } = "";

	public bool PreviewToggle { get; set; } = false;

	[InlineEditor( Label = false ), Group( "Feature" )]
	public ShaderFeature Feature { get; set; } = new();

	[Output, Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var staticComboName = "";

		if ( string.IsNullOrWhiteSpace( Feature.FeatureName ) )
		{
			return NodeResult.Error( "Feature must have a valid name!" );
		}

		if ( compiler.RegisterdFeatureNames.Contains( Feature.FeatureName ) )
		{
			return NodeResult.Error( $"Feature name `{Feature.FeatureName}` is already registered!" );
		}

		if ( Feature.IsValid )
		{
			compiler.RegisterShaderFeatureBinary( Feature, out staticComboName );
		}
		else
		{
			return NodeResult.Error( "Feature Is Invalid!" );
		}

		if ( compiler.GenerateShaderFeatureBody( staticComboName, Feature.FeatureName, InputTrue, InputFalse, PreviewToggle, out var switchResultVariableName, out var switchBody, out var switchResultType ) )
		{
			//SGPLog.Info( switchBody, compiler.IsPreview);

			return new NodeResult( switchResultType, switchResultVariableName, switchBody, constant: false );
		}
		else
		{
			return new NodeResult( ResultType.Float, $"{4.0f}" );
		}
	};

}
