using System.Numerics;

namespace Editor.ShaderGraphPlus.Nodes;


/// <summary>
/// Gerstner Waves
/// </summary>
[Title( "Gerstner Waves" ), Category( "Effects" )]
public sealed class GerstnerWavesNode : ShaderNodePlus
{
[Hide]
public static string GerstnerWaves => @"
float3 GerstnerWaves(float3 vWorldSpacePosition, float2 vDirection, float flWaveLength, float flSpeed, float flAmplitude, float flSteepness, float flNumWaves, float flGravityConstant )
{
		//static const float flGravityConstant = 385.827;
		static const float TwoPI = 6.28318530718;

		float l_3 = TwoPI / flWaveLength;
		float l_7 = (flSteepness / (l_3 * flAmplitude)) * flAmplitude;
		float3 l_10 = abs( vWorldSpacePosition );
		float l_16 = dot( float2( l_10.x, l_10.y ), normalize( vDirection ) * float2( l_3, l_3 ) );
		float l_24 = (l_16 * l_3) - ((sqrt( l_3 * flGravityConstant ) * flSpeed) * g_flTime);
		float l_26 = TwoPI * cos( l_24 );
		float l_32 = TwoPI * sin( l_24 );
		float3 result = float3( l_7 * (vDirection.y * l_26), l_7 * (l_26 * vDirection.x), l_32 * flAmplitude );
		return result;
}
";

	[Title( "World Pos" )]
	[Description("")]
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput WorldSpacePosition { get; set; }

	[Title( "Direction" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Direction { get; set; }

	[Title( "Wave Length" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput WaveLength { get; set; }

	[Title( "Speed" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Speed { get; set; }

	[Title( "Amplitude" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Amplitude { get; set; }


	[Title( "Steepness" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Steepness { get; set; }

	[Title( "Num Waves" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput NumWaves { get; set; }

	[Title( "Gravity Constant" )]
	[Description( "Gravitational constant to be used. Default is 385.827" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput GravityConstant { get; set; }

	public GerstnerWavesNode()
	{
		ExpandSize = new Vector2( 32, 0 );
	}

	public Vector2 DefaultDirection { get; set; } = new Vector2( 0.0f, 1.0f );
	public float DefaultWaveLength { get; set; } = 0.4f;
	public float DefaultSpeed { get; set; } = 0.05f;
	public float DefaultAmplitude { get; set; } = 0.5f;
	public float DefaultSteepness { get; set; } = 0.420f;
	public float DefaultNumWaves { get; set; } = 1.0f;

	[Description( "Gravitational constant to be used. Default is 385.827" )]
	public float DefaultGravityConstant { get; set; } = 385.827f;

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		if ( compiler.Graph.MaterialDomain is MaterialDomain.PostProcess )
		{
			return NodeResult.Error( $"{DisplayInfo.Name} Is not ment for postprocessing shaders!" );
		}

		var worldspaceposition = compiler.Result( WorldSpacePosition );

		if ( !worldspaceposition.IsValid() )
		{
			return NodeResult.MissingInput( nameof( WorldSpacePosition ) );
		}

		var	direction = compiler.ResultOrDefault( Direction, DefaultDirection );
		var wavelength = compiler.ResultOrDefault( WaveLength, DefaultWaveLength );
		var speed = compiler.ResultOrDefault( Speed, DefaultSpeed );
		var amplitude = compiler.ResultOrDefault( Amplitude, DefaultAmplitude );
		var steepness = compiler.ResultOrDefault( Steepness, DefaultSteepness );
		var numwaves = compiler.ResultOrDefault( NumWaves, DefaultNumWaves );
		var gravityconstant = compiler.ResultOrDefault( GravityConstant, DefaultGravityConstant );

		// Old
		//return new NodeResult( ResultType.Vector3, compiler.ResultFunction( compiler.GetFunction( GerstnerWaves ),
		//$"{worldspaceposition}",
		//$"{direction}",
		//$"{wavelength}",
		//$"{speed}",
		//$"{amplitude}",
		//$"{steepness}",
		//$"{numwaves}" ,
		//$"{gravityconstant}" ) );

		// New 
		return new NodeResult( ResultType.Vector3, compiler.ResultFunction( GerstnerWaves, 
		args:
        $"{worldspaceposition}" + 
        $",{direction}" +
        $",{wavelength}" +
        $",{speed}" +
        $",{amplitude}" +
        $",{steepness}" +
        $",{numwaves}" +
        $",{gravityconstant}" ) );
    };
}
