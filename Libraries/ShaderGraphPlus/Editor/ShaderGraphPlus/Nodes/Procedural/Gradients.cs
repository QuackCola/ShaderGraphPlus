using Facepunch.ActionGraphs;
using Sandbox;
using System.Xml.Linq;
using static Sandbox.Material;

namespace Editor.ShaderGraphPlus.Nodes;

[Title( "Round Gradient" ), Category( "Procedural/Gradients" )]
public sealed class RoundGradientNode : ShaderNodePlus
{

[Hide]
public string RoundGradient => @"
float RoundGradient( float2 vUV, float2 flCenter, float flRadius, float flDensity, bool bInvert )
{
	float distance = length(vUV - flCenter);
	float Result = pow(saturate( distance / flRadius), flDensity);

	if( bInvert )
	{
		return 1 - Result;
	}

	return Result;
}
";

	[Title( "UV" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	[Title( "Center" )]
	[Description( "Position of the gradients center" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput CenterPos { get; set; }


	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Radius { get; set; }


	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Density { get; set; }

	[Input( typeof( bool ) )]
	[Hide]
	public NodeInput Invert { get; set; }


	public Vector2 DefaultCenterPos { get; set; } = new Vector2( 0.5f, 0.5f );
	public float DefaultRadius { get; set; } = 0.25f;
	public float DefaultDensity { get; set; } = 2.33f;
	public bool DefaultInvert { get; set; } = false;

	//[Hide]
	//public ParameterUI InvertUIOption { get; set; }

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

		return new NodeResult( ResultType.Float, compiler.ResultFunction( RoundGradient, 
			args:
			$"{coords}" +
            $",{center}" +
            $",{radius}" +
            $",{density}" + 
			$",{invert}" 
		));
	};

}
