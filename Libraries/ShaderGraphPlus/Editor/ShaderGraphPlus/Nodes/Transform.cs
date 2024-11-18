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
[Title( "Normalize" ), Category( "Transform" )]
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

/// <summary>
/// Transforms a normal from tangent or object space into world space
/// </summary>
[Title( "Transform Normal" ), Category( "Transform" )]
public sealed class TransformNormal : ShaderNodePlus
{
	[Hide]
	public static string Vec3OsToTs => @"
float3 Vec3OsToTs( float3 vVectorOs, float3 vNormalOs, float3 vTangentUOs, float3 vTangentVOs )
{
	float3 vVectorTs;
	vVectorTs.x = dot( vVectorOs.xyz, vTangentUOs.xyz );
	vVectorTs.y = dot( vVectorOs.xyz, vTangentVOs.xyz );
	vVectorTs.z = dot( vVectorOs.xyz, vNormalOs.xyz );
	return vVectorTs.xyz;
}
";


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
			inputNormal = compiler.ResultFunction( Vec3OsToTs,
				args:
				$"{inputNormal}" +
                ",i.vNormalOs.xyz" +
                ",i.vTangentUOs_flTangentVSign.xyz" +
                ",cross( i.vNormalOs.xyz, i.vTangentUOs_flTangentVSign.xyz ) * i.vTangentUOs_flTangentVSign.w" 
			);
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
[Title( "Apply TRS" ), Category( "Transform" )]
public sealed class ApplyTrs : ShaderNodePlus
{

	public static string Quaternion_FromAngles => @"
float4 Quaternion_FromAngles( float3 vAngles )
{
	float4 rot = { 0.0, 0.0, 0.0, 1.0 };

	const float ANGLE_CONVERSION = 3.14159265 / 360.0;

	float pitch = vAngles.x * ANGLE_CONVERSION;
	float yaw = vAngles.y * ANGLE_CONVERSION;
	float roll = vAngles.z * ANGLE_CONVERSION;

	float sp = sin( pitch );
	float cp = cos( pitch );

	float sy = sin( yaw );
	float cy = cos( yaw );

	float sr = sin( roll );
	float cr = cos( roll );

	float srXcp = sr * cp;
	float crXsp = cr * sp;

	rot.x = srXcp * cy - crXsp * sy; // X
	rot.y = crXsp * cy + srXcp * sy; // Y

	float crXcp = cr * cp;
	float srXsp = sr * sp;

	rot.z = crXcp * sy - srXsp * cy; // Z
	rot.w = crXcp * cy + srXsp * sy; // W (real component)

	return rot;
}
";

	public static string Matrix_Identity => @"
float4x4 Matrix_Identity()
{
	return
	{
		1.0, 0.0, 0.0, 0.0,
		0.0, 1.0, 0.0, 0.0,
		0.0, 0.0, 1.0, 0.0,
		0.0, 0.0, 0.0, 1.0
	};
}
";

	public static string Matrix_FromQuaternion => @"
float4x4 Matrix_FromQuaternion( float4 qRotation )
{
	float xx = qRotation.x * qRotation.x;
	float yy = qRotation.y * qRotation.y;
	float zz = qRotation.z * qRotation.z;

	float xy = qRotation.x * qRotation.y;
	float wz = qRotation.z * qRotation.w;
	float xz = qRotation.z * qRotation.x;
	float wy = qRotation.y * qRotation.w;
	float yz = qRotation.y * qRotation.z;
	float wx = qRotation.x * qRotation.w;

	float4x4 result =
	{
		1.0, 0.0, 0.0, 0.0,
		0.0, 1.0, 0.0, 0.0,
		0.0, 0.0, 1.0, 0.0,
		0.0, 0.0, 0.0, 1.0
	};

	result._11 = 1.0 - 2.0 * (yy + zz);
	result._21 = 2.0 * (xy + wz);
	result._31 = 2.0 * (xz - wy);

	result._12 = 2.0 * (xy - wz);
	result._22 = 1.0 - 2.0 * (zz + xx);
	result._32 = 2.0 * (yz + wx);

	result._13 = 2.0 * (xz + wy);
	result._23 = 2.0 * (yz - wx);
	result._33 = 1.0 - 2.0 * (yy + xx);

	return result;
}
";

	public static string Matrix_FromScale => @"
float4x4 Matrix_FromScale( float3 vScale )
{
	float4x4 result =
	{
		1.0, 0.0, 0.0, 0.0,
		0.0, 1.0, 0.0, 0.0,
		0.0, 0.0, 1.0, 0.0,
		0.0, 0.0, 0.0, 1.0
	};

	result._11 = vScale.x;
	result._22 = vScale.y;
	result._33 = vScale.z;

	return result;
}
";

	public static string Matrix_FromTranslation => @"
float4x4 Matrix_FromTranslation( float3 vTranslation )
{
	float4x4 result =
	{
		1.0, 0.0, 0.0, 0.0,
		0.0, 1.0, 0.0, 0.0,
		0.0, 0.0, 1.0, 0.0,
		0.0, 0.0, 0.0, 1.0
	};

	result._14 = vTranslation.x;
	result._24 = vTranslation.y;
	result._34 = vTranslation.z;

	return result;
}
";

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
			rotation = new NodeResult( ResultType.Color, compiler.ResultFunction( Quaternion_FromAngles , args: rotationResult.Code ) );
		}
		else
		{
			rotation = compiler.ResultValue( new Vector4( DefaultRotation.x, DefaultRotation.y, DefaultRotation.z, DefaultRotation.w ) );
		}

		string matrix = null;

		if ( scale.IsValid ) ApplyMatrix( ref matrix, compiler.ResultFunction( Matrix_FromScale, args: scale.Code ) );
		if ( rotation.IsValid ) ApplyMatrix( ref matrix, compiler.ResultFunction(  Matrix_FromQuaternion, args: rotation.Code ) );
		if ( translation.IsValid ) ApplyMatrix( ref matrix, compiler.ResultFunction( Matrix_FromTranslation, args: translation.Code ) );

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
[Title( "Polar Coordinates" ), Category( "Transform" )]
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

		if ( compiler.Graph.MaterialDomain is MaterialDomain.PostProcess )
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

	public static string ColorBurn_blend => @"
float ColorBurn_blend( float a, float b )
{
    if ( a >= 1.0f ) return 1.0f;
    if ( b <= 0.0f ) return 0.0f;
    return 1.0f - saturate( ( 1.0f - a ) / b );
}

float3 ColorBurn_blend( float3 a, float3 b )
{
    return float3(
        ColorBurn_blend( a.r, b.r ),
        ColorBurn_blend( a.g, b.g ),
        ColorBurn_blend( a.b, b.b )
	);
}

float4 ColorBurn_blend( float4 a, float4 b, bool blendAlpha = false )
{
    return float4(
        ColorBurn_blend( a.rgb, b.rgb ).rgb,
        blendAlpha ? ColorBurn_blend( a.a, b.a ) : max( a.a, b.a )
    );
}
";

	public static string LinearBurn_blend => @"
float LinearBurn_blend( float a, float b )
{
    return max( 0.0f, a + b - 1.0f );
}

float3 LinearBurn_blend( float3 a, float3 b )
{
    return float3(
        LinearBurn_blend( a.r, b.r ),
        LinearBurn_blend( a.g, b.g ),
        LinearBurn_blend( a.b, b.b )
	);
}

float4 LinearBurn_blend( float4 a, float4 b, bool blendAlpha = false )
{
    return float4(
        LinearBurn_blend( a.rgb, b.rgb ).rgb,
        blendAlpha ? LinearBurn_blend( a.a, b.a ) : max( a.a, b.a )
    );
}
";

	public static string ColorDodge_blend => @"
float ColorDodge_blend( float a, float b )
{
    if ( a <= 0.0f ) return 0.0f;
    if ( b >= 1.0f ) return 1.0f;
    return saturate( a / ( 1.0f - b ) );
}

float3 ColorDodge_blend( float3 a, float3 b )
{
    return float3(
        ColorDodge_blend( a.r, b.r ),
        ColorDodge_blend( a.g, b.g ),
        ColorDodge_blend( a.b, b.b )
	);
}

float4 ColorDodge_blend( float4 a, float4 b, bool blendAlpha = false )
{
    return float4(
        ColorDodge_blend( a.rgb, b.rgb ).rgb,
        blendAlpha ? ColorDodge_blend( a.a, b.a ) : max( a.a, b.a )
    );
}
";

	public static string LinearDodge_blend => @"
float LinearDodge_blend( float a, float b )
{
    return min( 1.0f, a + b );
}

float3 LinearDodge_blend( float3 a, float3 b )
{
    return float3(
        LinearDodge_blend( a.r, b.r ),
        LinearDodge_blend( a.g, b.g ),
        LinearDodge_blend( a.b, b.b )
	);
}

float4 LinearDodge_blend( float4 a, float4 b, bool blendAlpha = false )
{
    return float4(
        LinearDodge_blend( a.rgb, b.rgb ).rgb,
        blendAlpha ? LinearDodge_blend( a.a, b.a ) : max( a.a, b.a )
    );
}
";

	public static string Overlay_blend => @"
float Overlay_blend( float a, float b )
{
    if ( a <= 0.5f )
        return 2.0f * a * b;
    else
        return 1.0f - 2.0f * ( 1.0f - a ) * ( 1.0f - b );
}

float3 Overlay_blend( float3 a, float3 b )
{
    return float3(
        Overlay_blend( a.r, b.r ),
        Overlay_blend( a.g, b.g ),
        Overlay_blend( a.b, b.b )
	);
}

float4 Overlay_blend( float4 a, float4 b, bool blendAlpha = false )
{
    return float4(
        Overlay_blend( a.rgb, b.rgb ).rgb,
        blendAlpha ? Overlay_blend( a.a, b.a ) : max( a.a, b.a )
    );
}
";

	public static string SoftLight_blend => @"
float SoftLight_blend( float a, float b )
{
    if ( b <= 0.5f )
        return 2.0f * a * b + a * a * ( 1.0f * 2.0f * b );
    else 
        return sqrt( a ) * ( 2.0f * b - 1.0f ) + 2.0f * a * (1.0f - b);
}

float3 SoftLight_blend( float3 a, float3 b )
{
    return float3(
        SoftLight_blend( a.r, b.r ),
        SoftLight_blend( a.g, b.g ),
        SoftLight_blend( a.b, b.b )
	);
}

float4 SoftLight_blend( float4 a, float4 b, bool blendAlpha = false )
{
    return float4(
        SoftLight_blend( a.rgb, b.rgb ).rgb,
        blendAlpha ? SoftLight_blend( a.a, b.a ) : max( a.a, b.a )
    );
}
";

	public static string HardLight_blend => @"
float HardLight_blend( float a, float b )
{
    if(b <= 0.5f)
        return 2.0f * a * b;
    else
        return 1.0f - 2.0f * (1.0f - a) * (1.0f - b);
}

float3 HardLight_blend( float3 a, float3 b )
{
    return float3(
        HardLight_blend( a.r, b.r ),
        HardLight_blend( a.g, b.g ),
        HardLight_blend( a.b, b.b )
	);
}

float4 HardLight_blend( float4 a, float4 b, bool blendAlpha = false )
{
    return float4(
        HardLight_blend( a.rgb, b.rgb ).rgb,
        blendAlpha ? HardLight_blend( a.a, b.a ) : max( a.a, b.a )
    );
}
";

	public static string VividLight_blend => @"
float VividLight_blend( float a, float b )
{
    if ( b <= 0.5f )
	{
		b *= 2.0f;
		if ( a >= 1.0f ) return 1.0f;
		if ( b <= 0.0f ) return 0.0f;
		return 1.0f - saturate( ( 1.0f - a ) / b );
	}
    else
	{
		b = 2.0f * ( b - 0.5f );
		if ( a <= 0.0f ) return 0.0f;
		if ( b >= 1.0f ) return 1.0f;
		return saturate( a / ( 1.0f - b ) );
	}
}

float3 VividLight_blend( float3 a, float3 b )
{
    return float3(
        VividLight_blend( a.r, b.r ),
        VividLight_blend( a.g, b.g ),
        VividLight_blend( a.b, b.b )
	);
}

float4 VividLight_blend( float4 a, float4 b, bool blendAlpha = false )
{
    return float4(
        VividLight_blend( a.rgb, b.rgb ).rgb,
        blendAlpha ? VividLight_blend( a.a, b.a ) : max( a.a, b.a )
    );
}
";

	public static string LinearLight_blend => @"
float LinearLight_blend( float a, float b )
{
    if ( b <= 0.5f )
	{
		b *= 2.0f;
		return max( 0.0f, a + b - 1.0f );
	}
    else
	{
		b = 2.0f * ( b - 0.5f );
		return min( 1.0f, a + b );
	}
}

float3 LinearLight_blend( float3 a, float3 b )
{
    return float3(
        LinearLight_blend( a.r, b.r ),
        LinearLight_blend( a.g, b.g ),
        LinearLight_blend( a.b, b.b )
	);
}

float4 LinearLight_blend( float4 a, float4 b, bool blendAlpha = false )
{
    return float4(
        LinearLight_blend( a.rgb, b.rgb ).rgb,
        blendAlpha ? LinearLight_blend( a.a, b.a ) : max( a.a, b.a )
    );
}
";

	public static string HardMix_blend => @"
float HardMix_blend( float a, float b )
{
    if(a + b >= 1.0f) return 1.0f;
    else return 0.0f;
}

float3 HardMix_blend( float3 a, float3 b )
{
    return float3(
        HardMix_blend( a.r, b.r ),
        HardMix_blend( a.g, b.g ),
        HardMix_blend( a.b, b.b )
	);
}

float4 HardMix_blend( float4 a, float4 b, bool blendAlpha = false )
{
    return float4(
        HardMix_blend( a.rgb, b.rgb ).rgb,
        blendAlpha ? HardMix_blend( a.a, b.a ) : max( a.a, b.a )
    );
}
";

	public static string Divide_blend => @"
float Divide_blend( float a, float b )
{
    if( b > 0.0f )
        return saturate( a / b );
    else
        return 0.0f;
}

float3 Divide_blend( float3 a, float3 b )
{
    return float3(
        Divide_blend( a.r, b.r ),
        Divide_blend( a.g, b.g ),
        Divide_blend( a.b, b.b )
	);
}

float4 Divide_blend( float4 a, float4 b, bool blendAlpha = false )
{
    return float4(
        Divide_blend( a.rgb, b.rgb ).rgb,
        blendAlpha ? Divide_blend( a.a, b.a ) : max( a.a, b.a )
    );
}
";

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
				returnCall = compiler.ResultFunction( ColorBurn_blend, args: $"{aStr}, {bStr}" );
				break;
			case BlendNodeMode.LinearBurn:
				returnCall = compiler.ResultFunction( LinearBurn_blend, args: $"{aStr}, {bStr}" );
				break;
			case BlendNodeMode.ColorDodge:
				returnCall = compiler.ResultFunction( ColorDodge_blend, args: $"{aStr}, {bStr}" );
				break;
			case BlendNodeMode.LinearDodge:
				returnCall = compiler.ResultFunction( LinearDodge_blend, args: $"{aStr}, {bStr}" );
				break;
			case BlendNodeMode.Overlay:
				returnCall = compiler.ResultFunction( Overlay_blend, args: $"{aStr}, {bStr}" );
				break;
			case BlendNodeMode.SoftLight:
				returnCall = compiler.ResultFunction( SoftLight_blend, args: $"{aStr}, {bStr}" );
				break;
			case BlendNodeMode.HardLight:
				returnCall = compiler.ResultFunction( HardLight_blend, args: $"{aStr}, {bStr}" );
				break;
			case BlendNodeMode.VividLight:
				returnCall = compiler.ResultFunction( VividLight_blend, args: $"{aStr}, {bStr}" );
				break;
			case BlendNodeMode.LinearLight:
				returnCall = compiler.ResultFunction( LinearLight_blend, args: $"{aStr}, {bStr}" );
				break;
			case BlendNodeMode.HardMix:
				returnCall = compiler.ResultFunction( HardMix_blend, args: $"{aStr}, {bStr}" );
				break;
			case BlendNodeMode.Divide:
				returnCall = compiler.ResultFunction( Divide_blend, args: $"{aStr}, {bStr}" );
				break;
		}

		if ( BlendMode != BlendNodeMode.Mix )
			returnCall = $"lerp( {aStr}, {returnCall}, {fractionStr} )";

		if ( Clamp )
			returnCall = $"saturate( {returnCall} )";

		return new NodeResult( results.Item1.ResultType, returnCall );
	};
}

[Title( "RGB to HSV" ), Category( "Transform" )]
public sealed class RGBtoHSV : ShaderNodePlus
{

	public static string RGB2HSV => @"
float3 RGB2HSV( float3 c )
{
    float4 K = float4( 0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0 );
    float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
    float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );

    float d = q.x - min( q.w, q.y );
    float e = 1.0e-10;
    return float3( abs( q.z + ( q.w - q.y ) / ( 6.0 * d + e ) ), d / ( q.x + e ), q.x );
}
";

	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput In { get; set; }

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func Out => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Vector3, compiler.ResultFunction( RGB2HSV, args: $"{compiler.ResultOrDefault( In, Vector3.One )}" ) );
	};
}

[Title( "HSV to RGB" ), Category( "Transform" )]
public sealed class HSVtoRGB : ShaderNodePlus
{
	public static string HSV2RGB => @"
float3 HSV2RGB( float3 c )
{
    float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
    float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
    return c.z * lerp( K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y );
}
";

	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput In { get; set; }

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func Out => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Vector3, compiler.ResultFunction( HSV2RGB, args: $"{compiler.ResultOrDefault( In, Vector3.One )}" ) );
	};
}

[Title("RGB to Linear"), Category("Transform")]
public sealed class RGBtoLinear : ShaderNodePlus
{

public static string RGB2Linear => @"
float3 RGB2Linear( float3 c )
{
    float3 vlinearRGBLo = c / 12.92;;
    float3 vlinearRGBHi = pow( max( abs( ( c + 0.055 ) / 1.055 ), 1.192092896e-07 ), float3( 2.4, 2.4, 2.4 ) );

    return float3( c <= 0.04045 ) ? vlinearRGBLo : vlinearRGBHi;
}
";

    [Input(typeof(Vector3))]
    [Hide]
    public NodeInput In { get; set; }

    [Output(typeof(Vector3))]
    [Hide]
    public NodeResult.Func Out => (GraphCompiler compiler) =>
    {
        return new NodeResult(ResultType.Vector3, compiler.ResultFunction(RGB2Linear, args: $"{compiler.ResultOrDefault(In, Vector3.One)}"));
    };
}

[Title("Linear to RGB"), Category("Transform")]
public sealed class LineartoRGB : ShaderNodePlus
{

public static string Linear2RGB => @"
float3 Linear2RGB( float3 c )
{
    float3 vSRGBLo = c * 12.92;
    float3 vSRGBHi = ( pow( max( abs(c), 1.192092896e-07 ), float3( 1.0 / 2.4, 1.0 / 2.4, 1.0 / 2.4 ) ) * 1.055 ) - 0.055;
    
	return float3( c <= 0.0031308 ) ? vSRGBLo : vSRGBHi;
}
";

    [Input(typeof(Vector3))]
    [Hide]
    public NodeInput In { get; set; }

    [Output(typeof(Vector3))]
    [Hide]
    public NodeResult.Func Out => (GraphCompiler compiler) =>
    {
        return new NodeResult(ResultType.Vector3, compiler.ResultFunction(Linear2RGB, args: $"{compiler.ResultOrDefault(In, Vector3.One)}"));
    };
}

[Title("Linear to HSV"), Category("Transform")]
public sealed class LineartoHSV : ShaderNodePlus
{

public static string Linear2HSV => @"
float3 Linear2HSV( float3 c )
{
    float3 vSRGBLo = c * 12.92;
    float3 vSRGBHi = (pow(max(abs(c), 1.192092896e-07), float3(1.0 / 2.4, 1.0 / 2.4, 1.0 / 2.4)) * 1.055) - 0.055;
    
	float3 Linear = float3(c <= 0.0031308) ? vSRGBLo : vSRGBHi;
    
	float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 P = lerp(float4(Linear.bg, K.wz), float4(Linear.gb, K.xy), step(Linear.b, Linear.g));
    float4 Q = lerp(float4(P.xyw, Linear.r), float4(Linear.r, P.yzx), step(P.x, Linear.r));
    float D = Q.x - min(Q.w, Q.y);
    float  E = 1e-10;

    return float3(abs(Q.z + (Q.w - Q.y)/(6.0 * D + E)), D / (Q.x + E), Q.x);
}
";

    [Input(typeof(Vector3))]
    [Hide]
    public NodeInput In { get; set; }

    [Output(typeof(Vector3))]
    [Hide]
    public NodeResult.Func Out => (GraphCompiler compiler) =>
    {
        return new NodeResult(ResultType.Vector3, compiler.ResultFunction(Linear2HSV, args: $"{compiler.ResultOrDefault(In, Vector3.One)}"));
    };
}

[Title("HSV to Linear"), Category("Transform")]
public sealed class HSVtoLinear : ShaderNodePlus
{

public static string HSV2Linear => @"
float3 HSV2Linear( float3 c )
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 P = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    float3 RGB = c.z * lerp(K.xxx, saturate(P - K.xxx), c.y);

    float3 vlinearRGBLo = RGB / 12.92;
    float3 vlinearRGBHi = pow(max(abs((RGB + 0.055) / 1.055), 1.192092896e-07), float3(2.4, 2.4, 2.4));

    return float3(RGB <= 0.04045) ? vlinearRGBLo : vlinearRGBHi;
}
";

    [Input(typeof(Vector3))]
    [Hide]
    public NodeInput In { get; set; }

    [Output(typeof(Vector3))]
    [Hide]
    public NodeResult.Func Out => (GraphCompiler compiler) =>
    {
        return new NodeResult(ResultType.Vector3, compiler.ResultFunction(HSV2Linear, args: $"{compiler.ResultOrDefault(In, Vector3.One)}"));
    };
}

[Title( "Make Greyscale" ), Category( "Transform" )]
public class MakeGreyscaleNode : ShaderNodePlus
{

[Hide]
public static string MakeGreyscale => @"
float MakeGreyscale(float3 vInput )
{
	return dot(vInput,float3(.299,.587,.114));
}
";

	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput ColorInput { get; set; }

	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		return new NodeResult( ResultType.Float, compiler.ResultFunction( MakeGreyscale, args: $"{compiler.ResultOrDefault( ColorInput, Vector3.One )}" ) );
	};
}
