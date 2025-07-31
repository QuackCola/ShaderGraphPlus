namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Get the dimensions of a Texture2D Object in the width and height.
/// </summary>
[Title( "Get Dimensions" ), Category( "Textures" ), Icon( "straighten" )]
public sealed class GetDimensionsNode : VoidFunctionBase
{
	[Title( "Tex 2D" )]
	[Input( typeof( Texture2DObject ) )]
	[Hide]
	[VoidFunctionInputArgument( ResultType.Texture2DObject, "$in0" )]
	public NodeInput TextureObject { get; set; }

	[JsonIgnore, Hide]
	public override bool CanPreview => false;

	[JsonIgnore, Hide]
	public override string FunctionName => "$in0.GetDimensions";

	[JsonIgnore, Hide]
	public override string FunctionArgs => $"$out0.x, $out0.y";

	[JsonIgnore, Hide, VoidFunctionOutputArgument( ResultType.Vector2, "$out0" )]
	public string TextureObjectSize { get; set; } = "";

	[Output( typeof( Vector2 ) )]
	[Title( "Tex Size" )]
	[Hide]
	public NodeResult.Func TextureSize => ( GraphCompiler compiler ) =>
	{
		//var textureObject = compiler.Result( TextureObject );

		//if ( textureObject.IsValid )
		{
			compiler.PreProcessVoidResult( this, Identifier );
			return new NodeResult( ResultType.Vector2, TextureObjectSize, constant: false );
		}
		//else
		//{
		//	return NodeResult.MissingInput( $"Tex Object" );
		//}
	};

}

[Title( "TestFunc" ), Category( "Textures" ), Icon( "straighten" )]
public sealed class TestFuncNode : VoidFunctionBase
{
	[JsonIgnore, Hide]
	public override bool CanPreview => false;

	[JsonIgnore, Hide]
	public override string FunctionName => "TestFunc";

	[JsonIgnore, Hide]
	public override string FunctionArgs => $"$in0, $out0, $out1";

	[Title( "InA" )]
	[Input( typeof( float ) )]
	[Hide]
	[VoidFunctionInputArgument( ResultType.Float, "$in0", nameof( DefaultInputA ) )]
	public NodeInput InputA { get; set; }

	[JsonIgnore, Hide, VoidFunctionOutputArgument( ResultType.Vector2, "$out0" )]
	public string OutA { get; set;} = "";
	
	[JsonIgnore, Hide, VoidFunctionOutputArgument( ResultType.Color, "$out1" )]
	public string OutB { get; set; } = "";

	public float DefaultInputA { get; set; } = 0.0f;

	public override void RegisterIncludes( GraphCompiler compiler )
	{
		compiler.RegisterInclude( $"TestFuncs.hlsl" );
	}

	[Output( typeof( Vector2 ) )]
	[Title( "OutA" )]
	[Hide]
	public NodeResult.Func ResultA => ( GraphCompiler compiler ) =>
	{
		compiler.PreProcessVoidResult( this, Identifier );

		if ( string.IsNullOrWhiteSpace( OutA ) )
			return NodeResult.Error( $"`{nameof( OutA )}` property is empty!" );

		return new NodeResult( ResultType.Vector2, OutA, constant: false );
	};

	[Output( typeof( Color ) )]
	[Title( "OutB" )]
	[Hide]
	public NodeResult.Func ResultB => ( GraphCompiler compiler ) =>
	{
		compiler.PreProcessVoidResult( this, Identifier );

		if ( string.IsNullOrWhiteSpace( OutB ) )
			return NodeResult.Error( $"`{nameof( OutB )}` property is empty!" );

		return new NodeResult( ResultType.Color, OutB, constant: false );
	};
	
}
