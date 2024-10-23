
namespace Editor.ShaderGraphPlus.Nodes;

[Title("Gradient"), Description(""), Category("Constants")]
public sealed class GradientNode : ShaderNodePlus
{

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

    [Output]
    [Hide]
    public NodeResult.Func Result => (GraphCompiler compiler) =>
    {
        if (Gradient.Colors.Count > 8)
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

/// <summary>
/// Sample a provided gradient.
/// </summary>
[Title("Sample Gradient"),Category("Sample")]
public sealed class SampleGradientNode : ShaderNodePlus
{
    [Title("Gradient")]
    [Input(typeof(Gradient))]
    [Hide]
    public NodeInput Gradient { get; set; }

    [Title("Time")]
    [Input(typeof(float))]
    [Hide]
    public NodeInput Time { get; set; }

    [Output(typeof(Color))]
    [Hide]
    public NodeResult.Func Result => (GraphCompiler compiler) =>
    {
        var gradient = compiler.Result(Gradient);

        if ( !gradient.IsValid() )
        {
            return NodeResult.MissingInput( nameof( Gradient ) );
        }

        if ( gradient.ResultType != ResultType.Gradient )
        {
            return NodeResult.Error($"Gradient input is not a gradient!");
        }

        var time = compiler.ResultOrDefault(Time, 0.0f);

        return new NodeResult( ResultType.Color, $"Gradient::SampleGradient( {gradient.Code}, {time.Code} )", constant: false );

    };
}