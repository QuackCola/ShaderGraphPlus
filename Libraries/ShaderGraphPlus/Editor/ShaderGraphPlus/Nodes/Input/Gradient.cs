
namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Constant gradient value.
/// </summary>
[Title( "Gradient" ), Category( "Constants/Gradient" ), Icon( "gradient" )]
public sealed class GradientNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[Hide]
	public override string Title => string.IsNullOrEmpty(Name) ?
	$"{DisplayInfo.For(this).Name}" :
	$"{DisplayInfo.For(this).Name} ({Name})";

	/// <summary>
	/// Name of the gradient.
	/// </summary>
	public string Name { get; set; }

	[InlineEditor]
	public Gradient Gradient { get; set; } = new Gradient();

	//public Gradient.BlendMode blendMode { get; set; } 

	[Output( typeof( Gradient ) )]
	[Hide]
	public NodeResult.Func Result => (GraphCompiler compiler) =>
	{
		if ( Gradient.Colors.Count > 8 )
		{
			return NodeResult.Error($"{DisplayInfo.Name} has {Gradient.Colors.Count} color keys which is greater than the maximum amount of 8 allowed color keys.");
		}
		else
		{
			// Register gradient with the compiler.
			var result = compiler.RegisterGradient(Gradient, Name);
		
			// Return the gradent name that will only be used to search for it in a dictonary.
			return new NodeResult( ResultType.Gradient, result, constant: true );
		}
	};
}
