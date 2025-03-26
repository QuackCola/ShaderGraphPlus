using Facepunch.ActionGraphs;
using Sandbox;
using System.Xml.Linq;
using static Sandbox.Material;

namespace Editor.ShaderGraphPlus.Nodes;

/// <summary>
/// Basic round gradient.
/// </summary>
[Title( "Round Gradient" ), Category( "Procedural/Gradients" ), Icon( "gradient" )]
public sealed class RoundGradientNode : ShaderNodePlus
{
	[Title( "UV" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	[Title( "Center" )]
	[Description( "The center position of the round gradient." )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput CenterPos { get; set; }

	[Input( typeof( float ) )]
	[Description("The radius of the round gradient.")]
	[Hide]
	public NodeInput Radius { get; set; }

	[Input( typeof( float ) )]
	[Description("How dense you want the round gradient to be.")]
	[Hide]
	public NodeInput Density { get; set; }

	[Input( typeof( bool ) )]
	[Description("")]
	[Hide]
	public NodeInput Invert { get; set; }

	public Vector2 DefaultCenterPos { get; set; } = new Vector2( 0.5f, 0.5f );
	public float DefaultRadius { get; set; } = 0.25f;
	public float DefaultDensity { get; set; } = 2.33f;
	public bool DefaultInvert { get; set; } = false;

	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var incoords = compiler.Result( Coords );
		var center = compiler.ResultOrDefault( CenterPos, DefaultCenterPos );
		var radius = compiler.ResultOrDefault( Radius, DefaultRadius );
		var density = compiler.ResultOrDefault( Density, DefaultDensity );
		var invert = compiler.ResultOrDefault( Invert, DefaultInvert );

	
		var coords = "";

		if ( compiler.Graph.MaterialDomain is MaterialDomain.PostProcess )
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "CalculateViewportUv( i.vPositionSs.xy )";
		}
		else
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "i.vTextureCoords.xy";
		}

		return new NodeResult( ResultType.Float, compiler.ResultFunction( "RoundGradient", $"{coords}", 
			$"{center}", $"{radius}", $"{density}", $"{invert}" 
		));
	};

}
