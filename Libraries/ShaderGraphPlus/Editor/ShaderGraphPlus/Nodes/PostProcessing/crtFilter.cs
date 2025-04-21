namespace Editor.ShaderGraphPlus.Nodes;


[Title( "CRT Filter" ), Category( "PostProcessing/Crt" )]
[Description( "Crt Filter Ported from	" )]
public class CrtFilterNode : ShaderNodePlus
{

[Hide]
public static string crtFilter => @"
//
// Ported from glsl to hlsl.
// Original Shader Source by pend00 from : https://godotshaders.com/shader/vhs-and-crt-monitor-effect/
//
float3 crtFilter(
	float2 vScreenUV,
	bool overlay,
	float scanlines_opacity,
	float scanlines_width,
	float grille_opacity,
	float2 resolution,
	bool pixelate,
	bool roll,
	float roll_speed,
	float roll_size,
	float roll_variation,
	float distort_intensity,
	float noise_opacity,
	float noise_speed,
	float static_noise_intensity,
	float aberration,
	float brightness,
	bool discolor,
	float warp_amount,
	bool clip_warp,
	float vignette_intensity,
	float vignette_opacity
)
{
	float2 uv = overlay ? warp(vScreenUV, warp_amount) : warp(vScreenUV, warp_amount);//warp(UV, warp_amount); // Warp the uv. uv will be used in most cases instead of UV to keep the warping
	float2 text_uv = uv;
	float2 roll_uv = float2(0.0,0.0);
	float time = roll ? g_flTime : 0.0;
	

	// Pixelate the texture based on the given resolution.
	if (pixelate)
	{
		text_uv = ceil(uv * resolution) / resolution;
	}
	
	// Create the rolling effect. We need roll_line a bit later to make the noise effect.
	// That is why this runs if roll is true OR noise_opacity is over 0.
	float roll_line = 0.0;
	if (roll || noise_opacity > 0.0)
	{
		// Create the areas/lines where the texture will be distorted.
		roll_line = smoothstep(0.3, 0.9, sin(uv.y * roll_size - (time * roll_speed) ) );
		// Create more lines of a different size and apply to the first set of lines. This creates a bit of variation.
		roll_line *= roll_line * smoothstep(0.3, 0.9, sin(uv.y * roll_size * roll_variation - (time * roll_speed * roll_variation) ) );
		// Distort the UV where where the lines are
		roll_uv = float2(( roll_line * distort_intensity * (1.-vScreenUV.x)), 0.0);
	}
	
	float4 text;
	if (roll)
	{
		// If roll is true distort the texture with roll_uv. The texture is split up into RGB to 
		// make some chromatic aberration. We apply the aberration to the red and green channels accorging to the aberration parameter
		// and intensify it a bit in the roll distortion.
		text.r = Tex2D( g_tColorBuffer, text_uv + roll_uv * 0.8 + float2(aberration, 0.0) * .1).r;
		text.g = Tex2D( g_tColorBuffer, text_uv + roll_uv * 1.2 - float2(aberration, 0.0) * .1 ).g;
		text.b = Tex2D( g_tColorBuffer, text_uv + roll_uv).b;
		text.a = 1.0;
	}
	else
	{
		// If roll is false only apply the aberration without any distorion. The aberration values are very small so the .1 is only 
		// to make the slider in the Inspector less sensitive.
		text.r = Tex2D( g_tColorBuffer, text_uv + float2(aberration, 0.0) * .1).r;
		text.g = Tex2D( g_tColorBuffer, text_uv - float2(aberration, 0.0) * .1).g;
		text.b = Tex2D( g_tColorBuffer, text_uv).b;
		text.a = 1.0;
	}
	
	float r = text.r;
	float g = text.g;
	float b = text.b;
	
	uv = warp(vScreenUV,warp_amount);
	
	// CRT monitors don't have pixels but groups of red, green and blue dots or lines, called grille. We isolate the texture's color channels 
	// and divide it up in 3 offsetted lines to show the red, green and blue colors next to each other, with a small black gap between.
	if (grille_opacity > 0.0){
		
		float g_r = smoothstep(0.85, 0.95, abs(sin(uv.x * (resolution.x * 3.14159265))));
		r = lerp(r, r * g_r, grille_opacity);
		
		float g_g = smoothstep(0.85, 0.95, abs(sin(1.05 + uv.x * (resolution.x * 3.14159265))));
		g = lerp(g, g * g_g, grille_opacity);
		
		float b_b = smoothstep(0.85, 0.95, abs(sin(2.1 + uv.x * (resolution.x * 3.14159265))));
		b = lerp(b, b * b_b, grille_opacity);
		
	}
	
	// Apply the grille to the texture's color channels and apply Brightness. Since the grille and the scanlines (below) make the image very dark you
	// can compensate by increasing the brightness.
	text.r = saturate(r * brightness);
	text.g = saturate(g * brightness);
	text.b = saturate(b * brightness);
	
	// Scanlines are the horizontal lines that make up the image on a CRT monitor. 
	// Here we are actual setting the black gap between each line, which I guess is not the right definition of the word, but you get the idea  
	float scanlines = 0.5;
	if (scanlines_opacity > 0.0)
	{
		// Same technique as above, create lines with sine and applying it to the texture. Smoothstep to allow setting the line size.
		scanlines = smoothstep(scanlines_width, scanlines_width + 0.5, abs(sin(uv.y * (resolution.y * 3.14159265))));
		text.rgb = lerp(text.rgb, text.rgb * float3(scanlines,scanlines,scanlines), scanlines_opacity);
	}
	
	// Apply the banded noise.
	if (noise_opacity > 0.0)
	{
		// Generate a noise pattern that is very stretched horizontally, and animate it with noise_speed
		float noise = smoothstep(0.4, 0.5, TestNoise(uv * float2(2.0, 200.0) + float2(10.0, (g_flTime * (noise_speed))) ) );
		
		// We use roll_line (set above) to define how big the noise should be vertically (multiplying cuts off all black parts).
		// We also add in some basic noise with random() to break up the noise pattern above. The noise is sized according to 
		// the resolution value set in the inspector. If you don't like this look you can 
		// change ""ceil(uv * resolution) / resolution"" to only ""uv"" to make it less pixelated. Or multiply resolution with som value
		// greater than 1.0 to make them smaller.
		roll_line *= noise * scanlines * saturate(random((ceil(uv * resolution) / resolution) + float2(g_flTime * 0.8, 0.0)).x + 0.8);
		// Add it to the texture based on noise_opacity
		text.rgb = saturate(lerp(text.rgb, text.rgb + roll_line, noise_opacity));
	}
	
	// Apply static noise by generating it over the whole screen in the same way as above
	if (static_noise_intensity > 0.0)
	{
		text.rgb += saturate(random((ceil(uv * resolution) / resolution) + frac(g_flTime)).x) * static_noise_intensity;
	}
	
	// Apply a black border to hide imperfections caused by the warping.
	// Also apply the vignette
	text.rgb *= border(uv,warp_amount);
	text.rgb *= vignette(uv,vignette_intensity,vignette_opacity);
	// Hides the black border and make that area transparent. Good if you want to add the the texture on top an image of a TV or monitor.
	if (clip_warp)
	{
		text.a = border(uv,warp_amount);
	}
	
	// Apply discoloration to get a VHS look (lower saturation and higher contrast)
	// You can play with the values below or expose them in the Inspector.
	float saturation = 0.5;
	float contrast = 1.2;
	if (discolor)
	{
		// Saturation

		float tempvar0 = text.r + text.g + text.b;

		float3 greyscale = float3(tempvar0,tempvar0,tempvar0) / 3.;
		text.rgb = lerp(text.rgb, greyscale, saturation);
		
		// Contrast
		float midpoint = pow(0.5, 2.2);
		text.rgb = (text.rgb - float3(midpoint,midpoint,midpoint)) * contrast + midpoint;
	}
	
	
	return text;
}
";

	[Input( typeof( bool ) )]
	[Hide]
	public NodeInput Overlay { get; set; }

	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput ScreenUVs { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput ScanLineOpacity { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput ScanLineWidth { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput GrilleOpacity { get; set; }

	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Resolution { get; set; }

	[Input( typeof( bool ) )]
	[Hide]
	public NodeInput Pixelate { get; set; }

	[Input( typeof( bool ) )]
	[Hide]
	public NodeInput Roll { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput RollSpeed { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput RollSize { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput RollVariation { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput DistortIntensity { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput NoiseOpacity { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput NoiseSpeed { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput StaticNoiseIntensity { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Aberration { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Brigtness { get; set; }

	[Input( typeof( bool ) )]
	[Hide]
	public NodeInput Discolor { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput WarpAmount { get; set; }

	[Input( typeof( bool ) )]
	[Hide]
	public NodeInput ClipWarp { get; set; }


	[Input( typeof( float ) )]
	[Hide]
	public NodeInput VignetteIntensity { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput VignetteOpacity { get; set; }


	public bool DefaultOverlay { get; set; } = false;
	[Group( "Scanlines" )]
	public float DefaultScanLineOpacity { get; set; } = 0.4f;
	[Group( "Scanlines" )]
	public float DefaultScanLineWidth { get; set; } = 0.25f;
	public float DefaultGrilleOpacity { get; set; } = 0.3f;
	public Vector2 DefaultResolution { get; set; } = new Vector2( 640.0f, 480.0f );
	public bool DefaultPixelate { get; set; } = true;
	[Group( "Roll" )]
	public bool DefaultRoll { get; set; } = true;
	[Group( "Roll" )]
	public float DefaultRollSpeed { get; set; } = 8.0f;
	[Group( "Roll" )]
	public float DefaultRollSize { get; set; } = 15.0f;
	[Group( "Roll" )]
	public float DefaultRollVariation { get; set; } = 1.8f;
	public float DefaultDistortIntensity { get; set; } = 0.05f;
	[Group( "Noise" )]
	public float DefaultNoiseOpacity { get; set; } = 0.4f;
	[Group("Noise" )]
	public float DefaultNoiseSpeed { get; set; } = 5.0f;
	[Group("Noise" )]
	public float DefaultStaticNoiseIntensity { get; set; } = 0.06f;
	public float DefaultAberration { get; set; } = 0.03f;
	public float DefaultBrigtness { get; set; } = 1.4f;
	public bool DefaultDiscolor { get; set; } = true;
	public float DefaultWarpAmount { get; set; } = 1.0f;
	public bool DefaultClipWarp { get; set; } = false;
	[Group( "Vignette" )]
	public float DefaultVignetteIntensity { get; set; } = 0.4f;
	[Group( "Vignette" )]
	public float DefaultVignetteOpacity { get; set; } = 0.5f;

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var coords = compiler.Result( ScreenUVs );
		var overlay = compiler.ResultOrDefault( Overlay, DefaultOverlay );
		var scanlineopacity = compiler.ResultOrDefault( ScanLineOpacity, DefaultScanLineOpacity );
		var scanlinewidth = compiler.ResultOrDefault( ScanLineWidth, DefaultScanLineWidth );
		var grilleopacity = compiler.ResultOrDefault( GrilleOpacity, DefaultGrilleOpacity );
		var resolution = compiler.ResultOrDefault( Resolution, DefaultResolution );
		var pixelate = compiler.ResultOrDefault( Pixelate, DefaultPixelate );
		var roll = compiler.ResultOrDefault( Roll, DefaultRoll );
		var rollspeed = compiler.ResultOrDefault( RollSpeed, DefaultRollSpeed );
		var rollsize = compiler.ResultOrDefault( RollSize, DefaultRollSize );
		var rollvariation = compiler.ResultOrDefault( RollVariation, DefaultRollVariation );
		var distortintensity = compiler.ResultOrDefault( DistortIntensity, DefaultDistortIntensity );
		var noiseopacity = compiler.ResultOrDefault( NoiseOpacity, DefaultNoiseOpacity );
		var noisespeed = compiler.ResultOrDefault( NoiseSpeed, DefaultNoiseSpeed );
		var staticnoiseintensity = compiler.ResultOrDefault( StaticNoiseIntensity, DefaultStaticNoiseIntensity );
		var aberration = compiler.ResultOrDefault( Aberration, DefaultAberration );
		var brigtness = compiler.ResultOrDefault( Brigtness, DefaultBrigtness );
		var discolor = compiler.ResultOrDefault( Discolor, DefaultDiscolor );
		var warpamount = compiler.ResultOrDefault( WarpAmount ,DefaultWarpAmount );
		var clipwarp = compiler.ResultOrDefault( ClipWarp, DefaultClipWarp );
		var vignetteintensity = compiler.ResultOrDefault( VignetteIntensity, DefaultVignetteIntensity );
		var vignetteopacity = compiler.ResultOrDefault( VignetteOpacity, DefaultVignetteOpacity );
		
		string func = compiler.RegisterFunction(  crtFilter );
		string funcCall = compiler.ResultFunction( func, @$"
		 {(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vPositionSs.xy / g_vRenderTargetSize")},
		 {overlay},
		 {scanlineopacity},
		 {scanlinewidth},
		 {grilleopacity},
		 {resolution},
		 {pixelate},
		 {roll},
		 {rollspeed},
		 {rollsize},
		 {rollvariation},
		 {distortintensity},
		 {noiseopacity},
		 {noisespeed},
		 {staticnoiseintensity},
		 {aberration},
		 {brigtness},
		 {discolor},
		 {warpamount},
		 {clipwarp},
		 {vignetteintensity},
		 {vignetteopacity}
		");

		return new NodeResult( ResultType.Vector3, funcCall );
	};
}
