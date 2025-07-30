
namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Get the dimensions of a Texture2D Object in the width and height.
/// </summary>
[Title( "Get Dimensions" ), Category( "Textures" ), Icon( "straighten" )]
public sealed class GetDimensionsNode : VoidFunctionBase
{
	[Title( "Tex 2D" )]
	[Input( typeof( Texture2DObject ) )]
	public NodeInput TextureObject { get; set; }

	[JsonIgnore, Hide]
	public override bool CanPreview => false;

	[JsonIgnore, Hide, VoidFunctionResult( "float2", true, "float" ) ]
	public string TextureObjectSize { get; set; } = "";

	[Output( typeof( Vector2 ) )]
	[Title( "Tex Size" )]
	[Hide]
	public NodeResult.Func TextureSize => ( GraphCompiler compiler ) =>
	{
		var textureObject = compiler.Result( TextureObject );

		if ( textureObject.IsValid )
		{
			RegisterVoidFunction( compiler, $"{textureObject.Code}.GetDimensions", $"{nameof( TextureObjectSize )}.x, {nameof( TextureObjectSize )}.y" );
			
			return new NodeResult( ResultType.Vector2, TextureObjectSize, constant: false );
		}
		else
		{
			return NodeResult.MissingInput( $"Tex Object" );
		}
	};

}

[Title( "TestFunc" ), Category( "Textures" ), Icon( "straighten" )]
public sealed class TestFuncNode : VoidFunctionBase
{
	[JsonIgnore, Hide]
	public override bool CanPreview => false;

	public override void RegisterIncludes( GraphCompiler compiler )
	{
		compiler.RegisterInclude( $"TestFuncs.hlsl" );
	}

	[Title( "InA" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput InputA { get; set; }

	[JsonIgnore, Hide, VoidFunctionResult( "float2", false )]
	public string OutA { get; set ;} = "";
	
	[JsonIgnore, Hide, VoidFunctionResult( "float4", false )]
	public string OutB { get; set; } = "";

	[Output( typeof( Vector2 ) )]
	[Title( "OutA" )]
	[Hide]
	public NodeResult.Func ResultA => ( GraphCompiler compiler ) =>
	{
		var inputA = compiler.ResultOrDefault( InputA, 2.0f );

		RegisterVoidFunction( compiler, "TestFunc", $"{inputA.Code}, {nameof( OutA )}, {nameof( OutB )}" );

		//GetResult( compiler, nameof( ResultA ) );

		if ( string.IsNullOrWhiteSpace( OutA ) )
			return NodeResult.Error( $"`{nameof( OutA )}` property is empty!" );

		return new NodeResult( ResultType.Vector2, OutA, constant: false );
	};

	[Output( typeof( Color ) )]
	[Title( "OutB" )]
	[Hide]
	public NodeResult.Func ResultB => ( GraphCompiler compiler ) =>
	{
		var inputA = compiler.ResultOrDefault( InputA, 2.0f );

		RegisterVoidFunction( compiler, "TestFunc", $"{inputA.Code}, {nameof( OutA )}, {nameof( OutB )}" );

		//GetResult( compiler, nameof( ResultB ) );

		if ( string.IsNullOrWhiteSpace( OutB ) )
			return NodeResult.Error( $"`{nameof( OutB )}` property is empty!" );

		return new NodeResult( ResultType.Color, OutB, constant: false );
	};

}
