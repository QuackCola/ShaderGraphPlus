using static Sandbox.Gizmo;

namespace ShaderGraphPlus.Nodes;

public enum MatrixNodeMode
{
	Row,
	Column
}

/// <summary>
/// Constructs square matrices from the four input vectors.
/// </summary>
[Title( "Matrix Construction" ), Category( "Math/Matrix" ), Icon( "construction" )]
public sealed class MatrixConstructionode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;


	[Title( "M0" )]
	[Input( typeof( Color ) )]
	[Hide]
	public NodeInput M0 { get; set; }

	[Title( "M1" )]
	[Input( typeof( Color ) )]
	[Hide]
	public NodeInput M1 { get; set; }

	[Title( "M2" )]
	[Input( typeof( Color ) )]
	[Hide]
	public NodeInput M2 { get; set; }

	[Title( "M3" )]
	[Input( typeof( Color ) )]
	[Hide]
	public NodeInput M3 { get; set; }

	[Hide, JsonIgnore]
	public override bool CanPreview => false;

	public MatrixNodeMode Mode { get; set; } = MatrixNodeMode.Row;

	public Vector4 DefaultM0 { get; set; } = Vector4.Zero;
	public Vector4 DefaultM1 { get; set; } = Vector4.Zero;
	public Vector4 DefaultM2 { get; set; } = Vector4.Zero;
	public Vector4 DefaultM3 { get; set; } = Vector4.Zero;

	[Output( typeof( Float4x4 ) ), Title( "4x4" )]
	[Hide]
	public NodeResult.Func ResultA => ( GraphCompiler compiler ) =>
	{
		var inM0 = compiler.ResultOrDefault( M0, DefaultM0 );
		var inM1 = compiler.ResultOrDefault( M1, DefaultM1 );
		var inM2 = compiler.ResultOrDefault( M2, DefaultM2 );
		var inM3 = compiler.ResultOrDefault( M3, DefaultM3 );
		
		var result = $"";
		
		if ( Mode == MatrixNodeMode.Row )
		{
			result = $"{inM0}.x, {inM0}.y, {inM0}.z, {inM0}.w, {inM1}.x, {inM1}.y, {inM1}.z, {inM1}.w, {inM2}.x, {inM2}.y, {inM2}.z, {inM2}.w, {inM3}.x, {inM3}.y, {inM3}.z, {inM3}.w";
		}
		else
		{
			result = $"{inM0}.x, {inM1}.x, {inM2}.x, {inM3}.x, {inM0}.y, {inM1}.y, {inM2}.y, {inM3}.y, {inM0}.z, {inM1}.z, {inM2}.z, {inM3}.z, {inM0}.w, {inM1}.w, {inM2}.w, {inM3}.w";
		}

		return new NodeResult( ResultType.Float4x4, result );
	};


	[Output( typeof( Float3x3 ) ), Title( "3x3" )]
	[Hide]
	public NodeResult.Func ResultB => ( GraphCompiler compiler ) =>
	{
		var inM0 = compiler.ResultOrDefault( M0, DefaultM0 );
		var inM1 = compiler.ResultOrDefault( M1, DefaultM1 );
		var inM2 = compiler.ResultOrDefault( M2, DefaultM2 );

		var result = $"";

		if ( Mode == MatrixNodeMode.Row )
		{
			result = $"{inM0}.x, {inM0}.y, {inM0}.z, {inM1}.x, {inM1}.y, {inM1}.z, {inM2}.x, {inM2}.y, {inM2}.z";
		}
		else
		{
			result = $"{inM0}.x, {inM1}.x, {inM2}.x, {inM0}.y, {inM1}.y, {inM2}.y, {inM0}.z, {inM1}.z, {inM2}.z";
		}

		return new NodeResult( ResultType.Float3x3, result );
	};

	[Output( typeof( Float2x2 ) ), Title( "2x2" )]
	[Hide]
	public NodeResult.Func ResultC => ( GraphCompiler compiler ) =>
	{
		var inM0 = compiler.ResultOrDefault( M0, DefaultM0 );
		var inM1 = compiler.ResultOrDefault( M1, DefaultM1 );

		var result = $"";

		if ( Mode == MatrixNodeMode.Row )
		{
			result = $"{inM0}.x, {inM0}.y, {inM1}.x, {inM1}.y";
		}
		else
		{
			result = $"{inM0}.x, {inM1}.x, {inM0}.y, {inM1}.y";
		}

		return new NodeResult( ResultType.Float2x2, result );
	};

}
