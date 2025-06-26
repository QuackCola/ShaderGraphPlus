namespace Editor.ShaderGraphPlus.Nodes;

[Title( "Static Switch" ), Category( "Utility" )]
public sealed class StaticSwitchNode : ShaderNodePlus
{

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
		var switchResult = "";
		var id = compiler.ShaderFeatures.Count;
		switchResult = $"staticSwitch_{id}";

		if ( Feature.IsValid )
		{
			compiler.RegisterShaderFeatureBinary( Feature, out staticComboName );
		}
		else
		{
			return NodeResult.Error( "Feature Is Invalid!" );
		}

		var results = compiler.StaticSwitchResult( InputTrue, InputFalse, 0.0f, 0.0f, StaticSwitchEntry.True, StaticSwitchEntry.False );
		results.Item1.BoundStaticSwtichBlock = StaticSwitchEntry.True;
		results.Item2.BoundStaticSwtichBlock = StaticSwitchEntry.False;

		// Reset active block after we are done. TODO : Rethink this once i can figure out a better way.
		compiler.ResetCurrentStaticSwitchCodeBlock();

		var result = compiler.GenerateShaderFeatureBody( switchResult, staticComboName, results.Item1, results.Item2, results.Item1.Components(), PreviewToggle, out var switchBody );

		if ( !string.IsNullOrWhiteSpace( switchBody ) )
		{
			return new NodeResult( results.Item1.ResultType, result, switchBody , constant: false );
		}
		else
		{
			return new NodeResult( ResultType.Float, $"{1.0f}" );
		}
	};

}
