﻿

namespace Editor.ShaderGraphPlus.Nodes;

[Title( "Warp" ), Category( "PostProcessing/Effects" )]
[Description( "Takes in the Screen UV's and warps the edges, creating the spherized effect" )]
[PostProcessingCompatable]
public class WarpNode : ShaderNodePlus
{

[Hide]
public static string Warp => @"
float2 Warp(float2 vUv , float flWarp_amount)
{
	float2 delta = vUv - 0.5;
	float delta2 = dot(delta.xy, delta.xy);
	float delta4 = delta2 * delta2;
	float delta_offset = delta4 * flWarp_amount;
	
	return vUv + delta * delta_offset;
}
";

	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput ScreenUVs { get; set; }


	[Input( typeof( float ) )]
	[Hide]
	public NodeInput WarpAmount { get; set; }


	public float DefaultWarpAmount { get; set; } = 1.0f;

	[Output( typeof( Vector2 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var coords = compiler.Result( ScreenUVs );
		var warpamount = compiler.ResultOrDefault( WarpAmount , DefaultWarpAmount );


		return new NodeResult( ResultType.Vector2, compiler.ResultFunction( Warp , 
			args:
			$"{(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vPositionSs.xy / g_vRenderTargetSize")}" + 
			$",{warpamount}" 
		));
	};
}

[Title( "Vignette" ), Category( "PostProcessing/Effects" )]
[Description( "Adds a vignette shadow to the edges of the image" )]
[PostProcessingCompatable]
public class VignetteNode : ShaderNodePlus
{

[Hide]
public static string Vignette => @"
float Vignette(float2 vUv , float flVignette_intensity, float flVignette_opacity)
{
	vUv *= 1.0 - vUv.xy;
	float vignette = vUv.x * vUv.y * 15.0;
	return pow(vignette, flVignette_intensity * flVignette_opacity);
}
";

	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput ScreenUVs { get; set; }


	[Input( typeof( float ) )]
	[Hide]
	public NodeInput VignetteIntensity { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput VignetteOpacity { get; set; }

	public float DefaultVignetteIntensity { get; set; } = 1.0f;
	public float DefaultVignetteOpacity { get; set; } = 0.5f;

	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var coords = compiler.Result( ScreenUVs );
		var vignetteintensity = compiler.ResultOrDefault( VignetteIntensity, DefaultVignetteIntensity );
		var vignetteopacity = compiler.ResultOrDefault( VignetteOpacity, DefaultVignetteOpacity );

		return new NodeResult( ResultType.Float, compiler.ResultFunction( Vignette , 
			args:
			$"{(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vPositionSs.xy / g_vRenderTargetSize")}" +
			$",{vignetteintensity}" + 
			$",{vignetteopacity}" 
		));
	};
}

[Title( "Border" ), Category( "PostProcessing/Effects" )]
[Description( "Adds a black border surrounding the edges of the screen." )]
[PostProcessingCompatable]
public class BorderNode : ShaderNodePlus
{

	[Hide]
	public static string Border => @"
float Border(float2 vUv , float flWarp_amount)
{
	float radius = min(flWarp_amount, 0.08);
	radius = max(min(min(abs(radius * 2.0), abs(1.0)), abs(1.0)), 1e-5);
	float2 abs_uv = abs(vUv * 2.0 - 1.0) - float2(1.0, 1.0) + radius;
	float dist = length(max(float2(0.0,0.0), abs_uv)) / radius;
	float square = smoothstep(0.96, 1.0, dist);
	return clamp(1.0 - square, 0.0, 1.0);
}
";

	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput ScreenUVs { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput WarpAmount { get; set; }

	public float DefaultWarpAmount { get; set; } = 1.0f;

	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var coords = compiler.Result( ScreenUVs );
		var warpamount = compiler.ResultOrDefault( WarpAmount, DefaultWarpAmount );

		return new NodeResult( ResultType.Float, compiler.ResultFunction( Border, 
			args:
			$"{(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vPositionSs.xy / g_vRenderTargetSize")}" + 
			$",{warpamount}" 
		));
	};
}
