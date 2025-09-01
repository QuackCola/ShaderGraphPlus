namespace ShaderGraphPlus.Nodes;

/// <summary>
/// Get the dimensions of a Texture2D Object in the width and height.
/// </summary>
[Title( "Get Dimensions" ), Category( "Textures" ), Icon( "straighten" )]
public sealed class GetDimensionsNode : VoidFunctionBase
{
	[Hide]
	public override int Version => 1;

	[Title( "Tex 2D" )]
	[Input( typeof( Texture2DObject ) )]
	[Hide]
	public NodeInput TextureObject { get; set; }

	[JsonIgnore, Hide]
	public override bool CanPreview => false;

	[JsonIgnore, Hide]
	public string TextureObjectSize { get; set; } = "";

	public override void BuildFunctionCall( ref List<VoidFunctionArgument> args, ref string functionName, ref string functionCall )
	{
		args.Add( new VoidFunctionArgument( nameof( TextureObject ), "$in0", VoidFunctionArgumentType.Input, ResultType.TextureCubeObject ) );
		args.Add( new VoidFunctionArgument( nameof( TextureObjectSize ), "$out0", VoidFunctionArgumentType.Output, ResultType.Vector2 ) );
		
		functionName = $"{args[0].VarName}.GetDimensions";
		functionCall = $"{functionName}( {args[1].VarName}.x, {args[1].VarName}.y )";
	}

	[Output( typeof( Vector2 ) )]
	[Title( "Tex Size" )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) => new NodeResult( ResultType.Vector2, TextureObjectSize, constant: false );
}
