namespace Editor.ShaderGraphPlus.Nodes;

public enum SwizzleChannel
{
	Red = 0,
	Green = 1,
	Blue = 2,
	Alpha = 3,
}

/// <summary>
/// Split value into individual components
/// </summary>
[Title( "Split" ), Category( "Channel" )]
public sealed class SplitVector : ShaderNodePlus
{
	[Input, Hide]
	public NodeInput Input { get; set; }

	[Output( typeof( float ) ), Hide]
	public NodeResult.Func X => ( GraphCompiler compiler ) =>
	{
		var result = compiler.Result( Input );
		if ( result.IsValid && result.ResultType > 0 ) return new NodeResult( ResultType.Float, $"{result}.x" );
		return new NodeResult( ResultType.Float, "0.0f" );
	};

	[Output( typeof( float ) ), Hide]
	public NodeResult.Func Y => ( GraphCompiler compiler ) =>
	{
		var result = compiler.Result( Input );
		if ( result.IsValid && result.Components() > 1 ) return new NodeResult( ResultType.Float, $"{result}.y" );
		return new NodeResult( ResultType.Float, "0.0f" );
	};

	[Output( typeof( float ) ), Hide]
	public NodeResult.Func Z => ( GraphCompiler compiler ) =>
	{
		var result = compiler.Result( Input );
		if ( result.IsValid && result.Components() > 2 ) return new NodeResult( ResultType.Float, $"{result}.z" );
		return new NodeResult( ResultType.Float, "0.0f" );
	};

	[Output( typeof( float ) ), Hide]
	public NodeResult.Func W => ( GraphCompiler compiler ) =>
	{
		var result = compiler.Result( Input );
		if ( result.IsValid && result.Components() > 3 ) return new NodeResult( ResultType.Float, $"{result}.w" );
		return new NodeResult( ResultType.Float, "0.0f" );
	};
}

/// <summary>
/// Combine input values into 3 separate vectors
/// </summary>
[Title( "Combine" ), Category( "Channel" )]
public sealed class CombineVector : ShaderNodePlus
{
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput X { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Y { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Z { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput W { get; set; }

	public float DefaultX { get; set; } = 0.0f;
	public float DefaultY { get; set; } = 0.0f;
	public float DefaultZ { get; set; } = 0.0f;
	public float DefaultW { get; set; } = 0.0f;

	[Output( typeof( Vector4 ) )]
	[Hide]
	public NodeResult.Func XYZW => ( GraphCompiler compiler ) =>
	{
		var x = compiler.ResultOrDefault( X, DefaultX ).Cast( 1 );
		var y = compiler.ResultOrDefault( Y, DefaultY ).Cast( 1 );
		var z = compiler.ResultOrDefault( Z, DefaultZ ).Cast( 1 );
		var w = compiler.ResultOrDefault( W, DefaultW ).Cast( 1 );

		return new NodeResult( ResultType.Color, $"float4( {x}, {y}, {z}, {w} )" );
	};

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func XYZ => ( GraphCompiler compiler ) =>
	{
		var x = compiler.ResultOrDefault( X, DefaultX ).Cast( 1 );
		var y = compiler.ResultOrDefault( Y, DefaultY ).Cast( 1 );
		var z = compiler.ResultOrDefault( Z, DefaultZ ).Cast( 1 );

		return new NodeResult( ResultType.Vector3, $"float3( {x}, {y}, {z} )" );
	};

	[Output( typeof( Vector2 ) )]
	[Hide]
	public NodeResult.Func XY => ( GraphCompiler compiler ) =>
	{
		var x = compiler.ResultOrDefault( X, DefaultX ).Cast( 1 );
		var y = compiler.ResultOrDefault( Y, DefaultY ).Cast( 1 );

		return new NodeResult( ResultType.Vector2, $"float2( {x}, {y})" );
	};
}


/// <summary>
/// Create a vector 2 from two float inputs
/// </summary>
[Title( "Combine Vector 2" ), Category( "Channel" )]
public class CombineVector2 : ShaderNodePlus
{
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput X { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Y { get; set; }

	public float DefaultX { get; set; } = 0.0f;
	public float DefaultY { get; set; } = 0.0f;

	[Output( typeof( Vector2 ) ), Title( "XY" )]
	[Hide]
	public NodeResult.Func Vector => ( GraphCompiler compiler ) =>
	{
		compiler.DepreciationWarning( this, nameof( CombineVector2 ), nameof( CombineVector ) );


		var x = compiler.ResultOrDefault( X, DefaultX ).Cast( 1 );
		var y = compiler.ResultOrDefault( Y, DefaultY ).Cast( 1 );

		return new NodeResult( ResultType.Vector2, $"float2( {x}, {y} )" );
	};
}

/// <summary>
/// Create a vector 3 from three float inputs
/// </summary>
[Title( "Combine Vector 3" ), Category( "Channel" )]
public class CombineVector3 : ShaderNodePlus
{
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput X { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Y { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Z { get; set; }

	public float DefaultX { get; set; } = 0.0f;
	public float DefaultY { get; set; } = 0.0f;
	public float DefaultZ { get; set; } = 0.0f;

	[Output( typeof( Vector3 ) ), Title( "XYZ" )]
	[Hide]
	public NodeResult.Func Vector => ( GraphCompiler compiler ) =>
	{
		var x = compiler.ResultOrDefault( X, DefaultX ).Cast( 1 );
		var y = compiler.ResultOrDefault( Y, DefaultY ).Cast( 1 );
		var z = compiler.ResultOrDefault( Z, DefaultZ ).Cast( 1 );

		return new NodeResult( ResultType.Vector3, $"float3( {x}, {y}, {z} )" );
	};
}

/// <summary>
/// Create a vector 4 from four float inputs
/// </summary>
[Title( "Combine Vector 4" ), Category( "Channel" )]
public class CombineVector4 : ShaderNodePlus
{
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput X { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Y { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Z { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput W { get; set; }

	public float DefaultX { get; set; } = 0.0f;
	public float DefaultY { get; set; } = 0.0f;
	public float DefaultZ { get; set; } = 0.0f;
	public float DefaultW { get; set; } = 0.0f;

	[Output( typeof( Color ) )]
	[Hide]
	public NodeResult.Func XYZW => ( GraphCompiler compiler ) =>
	{
		var x = compiler.ResultOrDefault( X, DefaultX ).Cast( 1 );
		var y = compiler.ResultOrDefault( Y, DefaultY ).Cast( 1 );
		var z = compiler.ResultOrDefault( Z, DefaultZ ).Cast( 1 );
		var w = compiler.ResultOrDefault( W, DefaultW ).Cast( 1 );

		return new NodeResult( ResultType.Color, $"float4( {x}, {y}, {z}, {w} )" );
	};
}

/// <summary>
/// Swap components of a color around
/// </summary>
[Title( "Swizzle" ), Category( "Channel" )]
public sealed class SwizzleVector : ShaderNodePlus
{
	[Input, Hide]
	public NodeInput Input { get; set; }

	public SwizzleChannel RedOut { get; set; } = SwizzleChannel.Red;
	public SwizzleChannel GreenOut { get; set; } = SwizzleChannel.Green;
	public SwizzleChannel BlueOut { get; set; } = SwizzleChannel.Blue;
	public SwizzleChannel AlphaOut { get; set; } = SwizzleChannel.Alpha;

	private static char SwizzleToChannel( SwizzleChannel channel )
	{
		return channel switch
		{
			SwizzleChannel.Green => 'y',
			SwizzleChannel.Blue => 'z',
			SwizzleChannel.Alpha => 'w',
			_ => 'x',
		};
	}

	[Output( typeof( Vector4 ) ), Hide]
	public NodeResult.Func Output => ( GraphCompiler compiler ) =>
	{
		var input = compiler.Result( Input );
		if ( !input.IsValid )
			return default;

		var swizzle = $".";
		swizzle += SwizzleToChannel( RedOut );
		swizzle += SwizzleToChannel( GreenOut );
		swizzle += SwizzleToChannel( BlueOut );
		swizzle += SwizzleToChannel( AlphaOut );

		return new NodeResult( ResultType.Color, $"{input.Cast( 4 )}{swizzle}" );
	};
}

/// <summary>
/// Append constants to change number of channels
/// </summary>
[Title( "Append" ), Category( "Channel" )]
public sealed class AppendVector : ShaderNodePlus
{
	[Input, Hide]
	public NodeInput A { get; set; }

	[Input, Hide]
	public NodeInput B { get; set; }

	[Output, Hide]
	public NodeResult.Func Output => ( GraphCompiler compiler ) =>
	{
		var resultA = compiler.ResultOrDefault( A, 0.0f );
		var resultB = compiler.ResultOrDefault( B, 0.0f );

		var components = resultB.Components() + resultA.Components();
		if ( components < 1 || components > 4 )
			return NodeResult.Error( $"Can't append {resultB.TypeName} to {resultA.TypeName}" );

		return new NodeResult( (ResultType)components, $"float{components}( {resultA}, {resultB} )" );
	};
}
