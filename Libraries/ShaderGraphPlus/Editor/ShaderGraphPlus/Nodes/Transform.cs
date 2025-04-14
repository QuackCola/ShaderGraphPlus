namespace Editor.ShaderGraphPlus.Nodes;

public enum BlendNodeMode
{
	Mix,
	Darken,
	Multiply,
	ColorBurn,
	LinearBurn,
	Lighten,
	Screen,
	ColorDodge,
	LinearDodge,
	Overlay,
	SoftLight,
	HardLight,
	VividLight,
	LinearLight,
	HardMix,
	Difference,
	Exclusion,
	Subtract,
	Divide,
	Add,
}

/// <summary>
/// Normalize a vector to have a length of 1 unit
/// </summary>
[Title( "Normalize" ), Category( "Transform" ), Icon( "arrow_forward" )]
public sealed class Normalize : Unary
{
	protected override string Op => "normalize";
}

public enum NormalSpace
{
	Tangent,
	Object,
	World,
}

public enum OutputNormalSpace
{
	Tangent,
	World
}

[Title( "Invert Colors" ), Category( "Transform" ), Icon( "invert_colors" )]
public class InvertColorsNode : ShaderNodePlus
{
    [Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput Color { get; set; }
	
	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
        return new NodeResult( ResultType.Vector3, compiler.ResultFunction( "InvertColors", $"{compiler.ResultOrDefault( Color, Vector3.One )}" ) );
	};
}

/// <summary>
/// Transforms a normal from tangent or object space into world space
/// </summary>
[Title( "Transform Normal" ), Category( "Transform" ), Icon( "shortcut" )]
public sealed class TransformNormal : ShaderNodePlus
{
	/// <summary>
	/// Normal input. No input specified will output vertex normal in world space
	/// </summary>
	[Input]
	[Hide]
	public NodeInput Input { get; set; }

	/// <summary>
	/// Space of the input normal, tangent or object.
	/// </summary>
	public NormalSpace InputSpace { get; set; } = NormalSpace.Tangent;

	/// <summary>
	/// Should we output in world space or tangent space.
	/// </summary>
	public OutputNormalSpace OutputSpace { get; set; } = OutputNormalSpace.Tangent;

	/// <summary>
	/// Scale and shifts input value to ( -1, 1 ) range
	/// </summary>
	public bool DecodeNormal { get; set; } = true;

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var result = compiler.Result( Input );

		if ( !result.IsValid )
		{
			// No input, just return the vertex normal in worldspace or a default tangent space output.
			return OutputSpace == OutputNormalSpace.World ? new NodeResult( ResultType.Vector3, "i.vNormalWs.xyz" ) : new NodeResult( ResultType.Vector3, "float3( 0, 0, 1 )" );
		}

		// Cast the result to a float3
		var resultCast = result.Cast( 3 );

		string inputNormal;

		if ( compiler.IsPreview )
		{
			// Because this is in preview mode, we can afford to use a dynamic branch for the decode normal option
			inputNormal = $"{compiler.ResultValue( DecodeNormal )} ? DecodeNormal( {resultCast} ) : {resultCast}";
		}
		else
		{
			// Decode normal if it's enabled, otherwise just use it as is
			inputNormal = DecodeNormal ? $"DecodeNormal( {resultCast} )" : resultCast;
		}

		if ( InputSpace == NormalSpace.Object )
		{
			inputNormal = compiler.ResultFunction( "Vec3OsToTs", inputNormal,
				"i.vNormalOs.xyz",
				"i.vTangentUOs_flTangentVSign.xyz",
				"cross( i.vNormalOs.xyz, i.vTangentUOs_flTangentVSign.xyz ) * i.vTangentUOs_flTangentVSign.w" );
		}
		else if ( InputSpace == NormalSpace.World )
		{
			inputNormal = $"Vec3WsToTs( {inputNormal}, i.vNormalWs, i.vTangentUWs, i.vTangentVWs )";
		}

		return OutputSpace == OutputNormalSpace.World ? new NodeResult( ResultType.Vector3, $"TransformNormal( {inputNormal}, i.vNormalWs, i.vTangentUWs, i.vTangentVWs )" ) : new NodeResult( ResultType.Vector3, $"{inputNormal}" );
	};
}


/// <summary>
/// Translate, rotate and scale a <see cref="Vector3"/>.
/// </summary>
[Title( "Apply TRS" ), Category( "Transform" ), Icon( "3d_rotation" )]
public sealed class ApplyTrs : ShaderNodePlus
{
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput Vector { get; set; }

	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput Translation { get; set; }

	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput Rotation { get; set; }

	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput Scale { get; set; }

	public Vector3 DefaultTranslation { get; set; } = Vector3.Zero;
	public Rotation DefaultRotation { get; set; } = global::Rotation.Identity;
	public Vector3 DefaultScale { get; set; } = Vector3.One;

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var vector = compiler.Result( Vector );

		if ( !vector.IsValid() )
		{
			return NodeResult.MissingInput( nameof( Vector ) );
		}

		// Only use DefaultXYZ if a non-default value is specified, so we can skip some matrix multiplications

		var translation = DefaultTranslation == Vector3.Zero ? compiler.Result( Translation ) : compiler.ResultOrDefault( Translation, DefaultTranslation );
		var scale = DefaultScale == Vector3.One ? compiler.Result( Scale ) : compiler.ResultOrDefault( Scale, DefaultScale );

		NodeResult rotation;

		if ( compiler.Result( Rotation ) is { IsValid: true } rotationResult )
		{
			rotation = new NodeResult( ResultType.Color, compiler.ResultFunction( "Quaternion_FromAngles", rotationResult.Code ) );
		}
		else
		{
			rotation = compiler.ResultValue( new Vector4( DefaultRotation.x, DefaultRotation.y, DefaultRotation.z, DefaultRotation.w ) );
		}

		string matrix = null;

		if ( scale.IsValid ) ApplyMatrix( ref matrix, compiler.ResultFunction( "Matrix_FromScale", scale.Code ) );
		if ( rotation.IsValid ) ApplyMatrix( ref matrix, compiler.ResultFunction( "Matrix_FromQuaternion", rotation.Code ) );
		if ( translation.IsValid ) ApplyMatrix( ref matrix, compiler.ResultFunction( "Matrix_FromTranslation", translation.Code ) );

		return matrix is null ? vector : new NodeResult( ResultType.Vector3, $"mul( {matrix}, float4( {vector.Code}, 1.0 ) ).xyz" );
	};

	private static void ApplyMatrix( ref string lhs, string rhs )
	{
		lhs = lhs is null ? rhs : $"mul( {lhs}, {rhs} )";
	}
}

/// <summary>
/// Convert from Cartesian coordinates to polar coordinates.
/// </summary>
[Title( "Polar Coordinates" ), Category( "Transform" ), Icon( "explore" )]
public sealed class PolarCoordinates : ShaderNodePlus
{
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Center { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput RadialScale { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput LengthScale { get; set; }

	public Vector2 DefaultCenter { get; set; } = 0.5f;
	public float DefaultRadialScale { get; set; } = 1.0f;
	public float DefaultLengthScale { get; set; } = 1.0f;

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var incoords = compiler.Result( Coords );
		var center = compiler.ResultOrDefault( Center, DefaultCenter );
		var radialScale = compiler.ResultOrDefault( RadialScale, DefaultRadialScale );
		var lengthScale = compiler.ResultOrDefault( LengthScale, DefaultLengthScale );


		var coords = "";

		if ( compiler.Graph.Domain is MaterialDomain.PostProcess )
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "CalculateViewportUv( i.vPositionSs.xy )";
		}
		else
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "i.vTextureCoords.xy";
		}


		return new NodeResult( ResultType.Vector2, $"PolarCoordinates( ( {coords} ) - ( {(center.IsValid ? center : "0.0f")} ), {(radialScale.IsValid ? radialScale : "1.0f")}, {(lengthScale.IsValid ? lengthScale : "1.0f")} )" );
	};
}

/// <summary>
/// Blend two colors or textures together using various different blending modes
/// </summary>
[Title( "Blend" ), Category( "Transform" )]
public sealed class Blend : ShaderNodePlus
{
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput A { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput B { get; set; }

	[Input( typeof( float ) ), Title( "Fraction" )]
	[Hide, Editor( nameof( Fraction ) )]
	public NodeInput C { get; set; }

	[MinMax( 0, 1 )]
	public float Fraction { get; set; } = 0.5f;

	public BlendNodeMode BlendMode { get; set; } = BlendNodeMode.Mix;

	/// <summary>
	/// Clamp result between 0 and 1
	/// </summary>
	public bool Clamp { get; set; } = true;

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var results = compiler.Result( A, B );
		var fraction = compiler.Result( C );
		var fractionType = fraction.IsValid && fraction.Components() > 1 ? Math.Max( results.Item1.Components(), results.Item2.Components() ) : 1;

		string fractionStr = $"{(fraction.IsValid ? fraction.Cast( fractionType ) : compiler.ResultValue( Fraction ))}";
		string aStr = results.Item1.ToString();
		string bStr = results.Item2.ToString();

		string returnCall = string.Empty;

		switch ( BlendMode )
		{
			case BlendNodeMode.Mix:
				returnCall = $"lerp( {aStr}, {bStr}, {fractionStr} )";
				break;
			case BlendNodeMode.Darken:
				returnCall = $"min( {aStr}, {bStr} )";
				break;
			case BlendNodeMode.Multiply:
				returnCall = $"{aStr}*{bStr}";
				break;
			case BlendNodeMode.Lighten:
				returnCall = $"max( {aStr}, {bStr} )";
				break;
			case BlendNodeMode.Screen:
				returnCall = $"({aStr}) + ({bStr}) - ({aStr}) * ({bStr})";
				break;
			case BlendNodeMode.Difference:
				returnCall = $"abs( ({aStr}) - ({bStr}) )";
				break;
			case BlendNodeMode.Exclusion:
				returnCall = $"({aStr}) + ({bStr}) - 2.0f * ({aStr}) * ({bStr})";
				break;
			case BlendNodeMode.Subtract:
				returnCall = $"max( 0.0f, ({aStr}) - ({bStr}) )";
				break;
			case BlendNodeMode.Add:
				returnCall = $"min( 1.0f, ({aStr}) + ({bStr}) )";
				break;
			case BlendNodeMode.ColorBurn:
				returnCall = compiler.ResultFunction( "ColorBurn_blend", aStr, bStr );
				break;
			case BlendNodeMode.LinearBurn:
				returnCall = compiler.ResultFunction( "LinearBurn_blend", aStr, bStr );
				break;
			case BlendNodeMode.ColorDodge:
				returnCall = compiler.ResultFunction( "ColorDodge_blend", aStr, bStr );
				break;
			case BlendNodeMode.LinearDodge:
				returnCall = compiler.ResultFunction( "LinearDodge_blend", aStr, bStr );
				break;
			case BlendNodeMode.Overlay:
				returnCall = compiler.ResultFunction( "Overlay_blend", aStr, bStr );
				break;
			case BlendNodeMode.SoftLight:
				returnCall = compiler.ResultFunction( "SoftLight_blend", aStr, bStr );
				break;
			case BlendNodeMode.HardLight:
				returnCall = compiler.ResultFunction( "HardLight_blend", aStr, bStr );
				break;
			case BlendNodeMode.VividLight:
				returnCall = compiler.ResultFunction( "VividLight_blend", aStr, bStr );
				break;
			case BlendNodeMode.LinearLight:
				returnCall = compiler.ResultFunction( "LinearLight_blend", aStr, bStr );
				break;
			case BlendNodeMode.HardMix:
				returnCall = compiler.ResultFunction( "HardMix_blend", aStr, bStr );
				break;
			case BlendNodeMode.Divide:
				returnCall = compiler.ResultFunction( "Divide_blend", aStr, bStr );
				break;
		}

		if ( BlendMode != BlendNodeMode.Mix )
			returnCall = $"lerp( {aStr}, {returnCall}, {fractionStr} )";

		if ( Clamp )
			returnCall = $"saturate( {returnCall} )";

		return new NodeResult( results.Item1.ResultType, returnCall );
	};
}

[Title( "RGB to HSV" ), Category( "Transform" ), Icon( "invert_colors" )]
public sealed class RGBtoHSV : ShaderNodePlus
{

	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput In { get; set; }

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func Out => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Vector3, compiler.ResultFunction( "RGB2HSV", $"{compiler.ResultOrDefault( In, Vector3.One )}" ) );
	};
}

[Title( "HSV to RGB" ), Category( "Transform" ), Icon( "invert_colors" )]
public sealed class HSVtoRGB : ShaderNodePlus
{
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput In { get; set; }

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func Out => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Vector3, compiler.ResultFunction( "HSV2RGB", $"{compiler.ResultOrDefault( In, Vector3.One )}" ) );
	};
}

[Title("RGB to Linear"), Category("Transform"), Icon( "invert_colors" )]
public sealed class RGBtoLinear : ShaderNodePlus
{
    [Input( typeof( Vector3 ) )]
    [Hide]
    public NodeInput In { get; set; }

    [Output( typeof( Vector3 ) )]
    [Hide]
    public NodeResult.Func Out => (GraphCompiler compiler) =>
    {
        return new NodeResult(ResultType.Vector3, compiler.ResultFunction( "RGB2Linear", $"{compiler.ResultOrDefault( In, Vector3.One )}" ) );
    };
}

[Title( "Linear to RGB" ), Category( "Transform" ), Icon( "invert_colors" )]
public sealed class LineartoRGB : ShaderNodePlus
{
    [Input( typeof( Vector3 ) )]
    [Hide]
    public NodeInput In { get; set; }

    [Output( typeof( Vector3 ) )]
    [Hide]
    public NodeResult.Func Out => ( GraphCompiler compiler ) =>
    {
        return new NodeResult( ResultType.Vector3, compiler.ResultFunction( "Linear2RGB", $"{compiler.ResultOrDefault( In, Vector3.One )}" ) );
    };
}

[Title( "Linear to HSV" ), Category( "Transform" ), Icon( "invert_colors" )]
public sealed class LineartoHSV : ShaderNodePlus
{
    [Input( typeof( Vector3 ) )]
    [Hide]
    public NodeInput In { get; set; }

    [Output( typeof( Vector3 ) )]
    [Hide]
    public NodeResult.Func Out => ( GraphCompiler compiler ) =>
    {
        return new NodeResult( ResultType.Vector3, compiler.ResultFunction( "Linear2HSV", $"{compiler.ResultOrDefault( In, Vector3.One )}" ) );
    };
}

[Title( "HSV to Linear" ), Category( "Transform" ), Icon( "invert_colors" )]
public sealed class HSVtoLinear : ShaderNodePlus
{
    [Input( typeof( Vector3 ) )]
    [Hide]
    public NodeInput In { get; set; }

    [Output( typeof( Vector3 ) )]
    [Hide]
    public NodeResult.Func Out => ( GraphCompiler compiler ) =>
    {
        return new NodeResult( ResultType.Vector3, compiler.ResultFunction( "HSV2Linear", $"{compiler.ResultOrDefault( In, Vector3.One )}" ) );
    };
}

[Title( "Height to Normal" ), Category( "Transform" ), Icon( "invert_colors" )]
public sealed class HeightToNormal : ShaderNodePlus
{
    public enum OutputNormalSpace
    {
        Tangent,
        World
    }

    /// <summary>
    /// Should we output in world space or tangent space.
    /// </summary>
    public OutputNormalSpace OutputSpace { get; set; } = OutputNormalSpace.World;

    /// <summary>
    /// The height to be converted into a normal.
    /// </summary>
    [Input( typeof( float ) )]
    [Hide]
    public NodeInput Height { get; set; }

    /// <summary>
    /// How strong you want the normal map effect to be.
    /// </summary>
    [Input( typeof( float ) )]
    [Hide]
    public NodeInput Strength { get; set; }

    //[Input(typeof(Vector3))]
    //[Hide]
    //[Title("Position")]
    //public NodeInput WorldPos { get; set; }

    //[Input(typeof(Vector3))]
    //[Hide]
    //public NodeInput Normal { get; set; }

    public float DefaultStrength { get; set; } = 0.1f;

    [Output( typeof( Vector3 ) )]
    [Hide]
    public NodeResult.Func Result => ( GraphCompiler compiler ) =>
    {

        var height = compiler.Result( Height );
        var strength = compiler.ResultOrDefault( Strength, DefaultStrength );
        var worldpos = "i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz";//compiler.Result(WorldPos);
        var worldnormal = "i.vNormalWs";//compiler.Result(Normal);

        if ( !height.IsValid() )
        {
            return NodeResult.MissingInput( nameof( Height ) );
        }
        //if (!worldpos.IsValid())
        //{
        //    return NodeResult.MissingInput(nameof(WorldPos));
        //}
        //if (!worldnormal.IsValid())
        //{
        //    return NodeResult.MissingInput(nameof(Normal));
        //}

		var result = compiler.ResultFunction( "Height2Normal",
            $"{height}", 
			$"{strength}", 
			$"{worldpos}", 
			$"{worldnormal}"
        );

        if ( OutputSpace == OutputNormalSpace.Tangent )
        {
            result = $"Vec3WsToTs( {result}, i.vNormalWs, i.vTangentUWs, i.vTangentVWs )";
        }

        return new NodeResult( ResultType.Vector3, result );
    };

}

[Title( "Make Greyscale" ), Category( "Transform" ), Icon( "invert_colors" )]
public class MakeGreyscaleNode : ShaderNodePlus
{
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput ColorInput { get; set; }

	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Float, compiler.ResultFunction( "ToGreyscale", $"{compiler.ResultOrDefault( ColorInput, Vector3.One )}" ) );
	};
}
