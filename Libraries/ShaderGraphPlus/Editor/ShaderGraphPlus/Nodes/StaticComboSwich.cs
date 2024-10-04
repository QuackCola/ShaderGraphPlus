using System.Text;

namespace Editor.ShaderGraphPlus.Nodes;

[Title( "Static Combo Switch" ), Description( "for making use of Static Combos" ), Category( "Dev" )]
public sealed class StaticComboSwitchNode : ShaderNodePlus
{
	[Hide]
	public override string Title => !string.IsNullOrWhiteSpace( Feature.FeatureName ) ?
		$"{DisplayInfo.For( this ).Name} (F_{Feature.FeatureName.ToUpper()})" :
		$"{DisplayInfo.For( this ).Name}";

	[InlineEditor]
	public ShaderFeature Feature { get; set; } = new();

	[Input]
	[Hide]
	public NodeInput True { get; set; }

	[Input]
	[Hide]
	public NodeInput False { get; set; }

	//public bool CreateFeature { get; set; }

	[Hide]
	public bool IsDynamicCombo { get; set; }


	//[Hide]
	//[Title("InvertTrue")]
	public bool PreviewToggle { get; set; }

	public StaticComboSwitchNode()
	{
		ExpandSize = new Vector2( 32, 16 );
	}

	[Hide]
	[Output]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var result_default = compiler.ResultOrDefault( False, 0.0f );
		var result_true = compiler.ResultOrDefault( True, 0.0f );

		var sb = new StringBuilder();

		if ( Feature.IsValid )
		{
			if ( Feature.IsOptionsValid )
			{
				// Register the shader feature with shadergraph. 
				compiler.RegisterShaderFeatureTest( Feature , result_default.Code, result_true.Code );








				// put the if block into NodeResult CodeTwo.
				//if ( compiler.IsNotPreview )
				//{
				//	sb.AppendLine();
				//	sb.AppendLine( $"#if ( S_{Feature.FeatureName.ToUpper()} == 1 )" );
				//	sb.AppendLine( $"{result_default.Code} = {result_true.Code};" );
				//	sb.AppendLine( $"#endif" );
				//}
				//else
				//{
				//	sb.AppendLine();
				//	sb.AppendLine( $"#if ( S_{Feature.FeatureName.ToUpper()} == {(PreviewToggle ? 0 : 1)} )" );
				//	sb.AppendLine( $"{result_default.Code} = {result_true.Code};" );
				//	sb.AppendLine( $"#endif" );
				//}
			}
			else
			{
				return NodeResult.Error( "Invalid Feature Option found!" );
			}
		}
		else
		{
			return NodeResult.Error( "Feature Is Invalid!" );
		}





        // Return the default result with a variable result type.
        return new NodeResult( result_default.ResultType, $"{result_default}", constant: false);//codetwo: sb.ToString(), constant: false );
    };

}
