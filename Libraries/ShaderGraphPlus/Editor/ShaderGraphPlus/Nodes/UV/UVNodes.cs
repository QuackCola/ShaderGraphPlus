using System.Numerics;

namespace Editor.ShaderGraphPlus.Nodes;


/// <summary>
/// Rotate your texture coordinates.
/// </summary>
[Title( "UV Rotation" ), Category( "UV" )]
public sealed class UVRotationNode : ShaderNodePlus
{

[Hide]
public string UVRotation => @"
float2 UVRotation( float2 vUv, float2 vRotationCenter, float flRotation )
{
    vUv = vUv.xy - vRotationCenter; // Offset incoming UV's by the specified Rotation Center. For example, the incoming uv's (0.0,0.0) could become (0.5,0.5).

    // Convert degrees to radians
    flRotation = radians(flRotation);

    // U
    float x = (vUv.x * cos(flRotation)) - (vUv.y * sin(flRotation));

    // V
    float y = (vUv.x * sin(flRotation)) + (vUv.y * cos(flRotation));

    return (float2(x,y) - vRotationCenter); // Output the rotation result and then revert UV's 0,0 to its initial position.
}
";

	[Title( "UV" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	[Title( "Rotation Center" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput RotationCenter { get; set; }

	[Title( "Rotation" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Rotation { get; set; }

	[InputDefault( nameof( RotationCenter ) )]
	public Vector2 DefaultRotationCenter { get; set; } = new Vector2( 0.5f, 0.5f );
	
	[InputDefault( nameof( Rotation ) )]
	public float DefaultRotation { get; set; } = 0.0f;

	[Output( typeof( Vector2 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var incoords = compiler.Result( Coords );
		var rotationcenter = compiler.ResultOrDefault( RotationCenter, DefaultRotationCenter );
		var rotation = compiler.ResultOrDefault( Rotation, DefaultRotation );

		var coords = "";

		if ( compiler.Graph.MaterialDomain is MaterialDomain.PostProcess )
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "CalculateViewportUv( i.vPositionSs.xy )";
		}
		else
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "i.vTextureCoords.xy";
		}

		return new NodeResult( ResultType.Vector2, compiler.ResultFunction( UVRotation, args: $"{coords}, {rotationcenter}, {rotation}" ));
	};

}

/// <summary>
/// Scale your texture coordinates.
/// </summary>
[Title( "UV Scale" ), Category( "UV" )]
public sealed class UVScaleNode : ShaderNodePlus
{

	[Hide]
	public string UVScale => @"
	float2 UVScale( float2 vUv, float2 vScale )
	{
		return vUv * vScale;
	}
	";

	[Title( "UV" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	[Title( "Scale" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Scale { get; set; }

	[InputDefault( nameof( Scale ) )]
	public Vector2 DefaultScale { get; set; } = new Vector2( 1.0f, 1.0f );

	[Output( typeof( Vector2 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var incoords = compiler.Result( Coords );
		var scale = compiler.ResultOrDefault( Scale, DefaultScale );

		var coords = "";

		if ( compiler.Graph.MaterialDomain is MaterialDomain.PostProcess )
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "CalculateViewportUv( i.vPositionSs.xy )";
		}
		else
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "i.vTextureCoords.xy";
		}

		//return new NodeResult( ResultType.Vector2, compiler.ResultFunction( compiler.GetFunction( UVScale ), $"{(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vTextureCoords.xy")}", $"{scale}" ) );
		return new NodeResult( ResultType.Vector2, $"({coords} * {scale})" );
	};

}

/// <summary>
/// Scale your texture coordinates by a specified center point.
/// </summary>
[Title( "UV Scale By Point" ), Category( "UV" )]
public sealed class UVScaleByPointNode : ShaderNodePlus
{

[Hide]
public string UVScaleByPoint => @"
//  vUv - UV coordinates input.
//  flCenter - Center point to scale from. A flCenter 0f 0.5 would let you scale by the center. 
//  flScale - Amount to scale the UVs by in both the X & Y.
float2 UVScaleByPoint( float2 vUv, float flCenter, float2 flScale )
{
    vUv = vUv - flCenter; // Offset the incoming UV so that 0,0 of the UV is now at the defined center.
    float2 vScale = vUv * flScale;
    float2 vResult = vScale + flCenter; // Return Uv's 0,0 to it's initial position.
    return vResult;
}
";

	[Title( "UV" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	[Title( "Center" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Center { get; set; }

	[Title( "Scale" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Scale { get; set; }

	[InputDefault( nameof( Center ) )]
	public float DefaultCenter { get; set; } = 1.0f;

	[InputDefault( nameof( Scale ) )]
	public Vector2 DefaultScale { get; set; } = new Vector2( 1.0f, 1.0f );

	[Output( typeof( Vector2 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var incoords = compiler.Result( Coords );
		var center = compiler.ResultOrDefault( Center, DefaultCenter );
		var scale = compiler.ResultOrDefault( Scale, DefaultScale );

		var coords = "";

		if ( compiler.Graph.MaterialDomain is MaterialDomain.PostProcess )
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "CalculateViewportUv( i.vPositionSs.xy )";
		}
		else
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "i.vTextureCoords.xy";
		}

		return new NodeResult( ResultType.Vector2, compiler.ResultFunction( UVScaleByPoint, args: $"{coords}, {center}, {scale}" ));
	};

}

/// <summary>
/// Scroll your texture coordinates in a particular direction.
/// </summary>
[Title( "UV Scroll" ), Category( "UV" )]
public sealed class UVScrollNode : ShaderNodePlus
{

[Hide]
public string UVScroll => @"
float2 UVScroll( float flTime, float2 vUv, float2 vScrollSpeed )
{
    return vUv + flTime * vScrollSpeed;
}
";

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Time { get; set; }

	[Title( "UV" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	/// <summary>
	/// Direction & speed of the scrolling.
	/// </summary>
	[Title( "Scroll Speed" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput ScrollSpeed { get; set; }

	[InputDefault( nameof( ScrollSpeed ) )]
	public Vector2 DefaultScrollSpeed { get; set; } = new Vector2( 1f, 1f );


	[Output( typeof( Vector2 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var incoords = compiler.Result( Coords );
		var result_time = compiler.Result( Time );
		var time = "";
		var scrollspeed = compiler.ResultOrDefault( ScrollSpeed, DefaultScrollSpeed );


		if ( Time.IsValid() )
		{
			time = result_time.Code;
		}
		else
		{
			time = "g_flTime";
		}

		var coords = "";

		if ( compiler.Graph.MaterialDomain is MaterialDomain.PostProcess )
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "CalculateViewportUv( i.vPositionSs.xy )";
		}
		else
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "i.vTextureCoords.xy";
		}

		//return new NodeResult( ResultType.Vector2, compiler.ResultFunction( compiler.GetFunction( UVScroll ), $"{time}", $"{(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vTextureCoords.xy")}", $"{scrollspeed}" ) );
		return new NodeResult( ResultType.Vector2, $"({coords} + {time} * {scrollspeed})");
	};
}


/// <summary>
/// Tile or shift your texture coordinates. Tile works by scaling the texture up
/// and down. Offset works by adding or subtracting from the texture coordinates
/// </summary>
[Title( "UV Tile And Offset" ), Category( "UV" )]
public sealed class TileAndOffset : ShaderNodePlus
{
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput UV { get; set; }

	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Tile { get; set; }

	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Offset { get; set; }

	[InputDefault( nameof( Tile ) )]
	public Vector2 DefaultTile { get; set; } = Vector2.One;

	[InputDefault( nameof( Offset ) )]
	public Vector2 DefaultOffset { get; set; } = Vector2.Zero;

	public bool WrapTo01 { get; set; } = false;

	[Output( typeof( Vector2 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var incoords = compiler.Result( UV );
		var tile = compiler.ResultOrDefault( Tile, DefaultTile );
		var offset = compiler.ResultOrDefault( Offset, DefaultOffset );

		var coords = "";

		if ( compiler.Graph.MaterialDomain is MaterialDomain.PostProcess )
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "CalculateViewportUv( i.vPositionSs.xy )";
		}
		else
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "i.vTextureCoords.xy";
		}


		var resultCode = $"TileAndOffsetUv( {coords}," +
			$" {(tile.IsValid ? tile.Cast( 2 ) : "1.0f")}," +
			$" {(offset.IsValid ? offset.Cast( 2 ) : "0.0f")} )";

		if ( compiler.IsPreview )
		{
			resultCode = $"{compiler.ResultValue( WrapTo01 )} ? frac( {resultCode} ) : {resultCode}";
		}
		else if ( WrapTo01 )
		{
			resultCode = $"frac( {resultCode} )";
		}

		return new NodeResult( ResultType.Vector2, resultCode );
	};
}

[Title( "FlipBook" ), Category( "UV" )]
public sealed class FlipBookNode : ShaderNodePlus
{

[Hide]
public static string FlipBook => @"
float2 FlipBook(float2 vUV, float flWidth, float flHeight, float flTile, float2 Invert)
{
    flTile = fmod(flTile, flWidth * flHeight);
    float2 vtileCount = float2(1.0, 1.0) / float2(flWidth, flHeight);
    float tileY = abs(Invert.y * flHeight - (floor(flTile * vtileCount.x) + Invert.y * 1));
    float tileX = abs(Invert.x * flWidth - ((flTile - flWidth * floor(flTile * vtileCount.x)) + Invert.x * 1));
    return (vUV + float2(tileX, tileY)) * vtileCount;
}
";
	
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Width  { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Height { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput TileIndex { get; set; }

	[InputDefault( nameof( Width ) )]
	public float DefaultWidth { get; set; } = 1.0f;

	[InputDefault( nameof( Height ) )]
	public float DefaultHeight { get; set; } = 1.0f;

	[InputDefault( nameof( TileIndex ) )]
	public int DefaultTileIndex { get; set; } = 1;

	public bool Invertx { get; set; } = false;

	public bool Inverty { get; set; } = false;

	[Output( typeof( Vector2 ) ), Title( "Result" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		if ( compiler.Graph.MaterialDomain is MaterialDomain.PostProcess )
		{
			return NodeResult.Error( $"{DisplayInfo.Name} Is not ment for postprocessing shaders!" );
		}

		var coords = compiler.Result( Coords );
		var width = compiler.ResultOrDefault( Width, DefaultWidth );
		var height = compiler.ResultOrDefault(  Height, DefaultHeight );
		var tileindex = compiler.ResultOrDefault( TileIndex, DefaultTileIndex );

		return new NodeResult( ResultType.Vector2, compiler.ResultFunction( FlipBook,
			args:
			$"{(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vTextureCoords.xy")}, {width}, {height}, {tileindex}, float2({(Invertx ? 1 : 0)},{(Inverty ? 1 : 0)})" 
		) );
	};
}
