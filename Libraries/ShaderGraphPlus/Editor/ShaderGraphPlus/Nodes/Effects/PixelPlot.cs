namespace Editor.ShaderGraphPlus.Nodes;

[Title( "Pixel Plot" ), Category( "Effects" )]
public sealed class PixelPlotNode : TextureSamplerBase
{

	[Hide]
	public string PixelPlot => @"	
float4 PixelPlot( in Texture2D vColor, in SamplerState sSampler, float2 vUv , float2 vGridSize , float flBoarderThickness)
{

	float2 vGridBlock = 1 / vGridSize;

	float2 vUvGrid = floor(vUv * vGridSize) / vGridSize; // Divide By Gridsize so that uvspace is clamped to  0 to 1.

	float2 vGridBoarder = step(0.5 - flBoarderThickness, frac(vUv / vGridBlock)) *
						 step(frac(vUv / vGridBlock), 0.5 + flBoarderThickness);

	float4 result = vColor.Sample(sSampler,vUvGrid) * (vGridBoarder.x * vGridBoarder.y);

	return result;
}
";

	/// <summary>
	/// Coordinates to sample this texture
	/// </summary>
	[Title( "Coordinates" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	/// <summary>
	/// How the texture is filtered and wrapped when sampled
	/// </summary>
	[Title( "Sampler" )]
	[Input( typeof( Sampler ) )]
	[Hide]
	public NodeInput Sampler { get; set; }

	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput GridSize { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput BoarderThickness { get; set; }

	public Vector2 DefaultGridSize { get; set; } = new Vector2( 24.0f, 24.0f );

	public float DefaultBoarderThickness { get; set; } = 0.420f;

	public PixelPlotNode()
	{
		ExpandSize = new Vector2( 0f, 128f );

		UI = new TextureInput
		{
			ImageFormat = TextureFormat.DXT5,
			SrgbRead = true,
			Default = Color.White,
		};
	}

	/// <summary>
	/// RGBA color result
	/// </summary>
	[Hide]
	[Output( typeof( Color ) ), Title( "RGBA" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = UI;
		input.Type = TextureType.Tex2D;

		CompileTexture();

		var texture = string.IsNullOrWhiteSpace( TexturePath ) ? null : Texture.Load( TexturePath );
		texture ??= Texture.White;

		var result = compiler.ResultTexture( compiler.ResultSamplerOrDefault( Sampler, DefaultSampler ), input, texture );
		var coords = compiler.Result( Coords );
		var Grid = compiler.ResultOrDefault( GridSize, DefaultGridSize );
		var Boarder = compiler.ResultOrDefault( BoarderThickness, DefaultBoarderThickness );

        //return new NodeResult( ResultType.Color, compiler.ResultFunction( PixelPlot , 
        //args: 
        //$"{result.Item1}" + 
        //$",{result.Item2}" + 
        //$",{(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vTextureCoords.xy")}" + 
        //$",{Grid}" + 
        //$",{Boarder}" ) 
        //);

        return new NodeResult(ResultType.Color, compiler.ResultFunction(PixelPlot,
        args:
        $"{result.Item1}, {result.Item2}, {(coords.IsValid ? $"{coords.Cast(2)}" : "i.vTextureCoords.xy")}, {Grid}, {Boarder}"
		));
    };

	/// <summary>
	/// Red component of result	
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "R" )]
	public NodeResult.Func R => ( GraphCompiler compiler ) => Component( "r", compiler );

	/// <summary>
	/// Green component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "G" )]
	public NodeResult.Func G => ( GraphCompiler compiler ) => Component( "g", compiler );

	/// <summary>
	/// Blue component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "B" )]
	public NodeResult.Func B => ( GraphCompiler compiler ) => Component( "b", compiler );

	/// <summary>
	/// Alpha (Opacity) component of result
	/// </summary>
	[Output( typeof( float ) ), Hide, Title( "A" )]
	public NodeResult.Func A => ( GraphCompiler compiler ) => Component( "a", compiler );
}
