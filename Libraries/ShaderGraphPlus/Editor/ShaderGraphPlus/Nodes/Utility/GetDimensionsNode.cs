namespace Editor.ShaderGraphPlus.Nodes;

/// <summary>
/// Get the dimensions of a Texture2D Object in the width and height.
/// </summary>
[Title( "Get Dimensions" ), Category( "Textures" ), Icon( "straighten" )]
public sealed class GetDimensionsNode : ShaderNodePlus//, IErroringNode
{
	[Title( "Tex Object" )]
	[Input( typeof( Texture2DObject ) )]
	[Hide]
	public NodeInput TextureObject { get; set; }

	[Output( typeof( Vector2 ) )]
	[Title( "Tex Size" )]
	[Hide]
	public NodeResult.Func TextureSize => ( GraphCompiler compiler ) =>
	{
		var textureObject = compiler.Result( TextureObject );

		if ( textureObject.IsValid )
		{
			var result = $"{textureObject.Code}_wh";

			compiler.RegisterVoid(
				ResultType.Vector2,
				result,
				$"{textureObject.Code}.GetDimensions( {result}.x, {result}.y );"
			);

			return new NodeResult( ResultType.Vector2, result, constant: false );
		}
		else
		{
			return NodeResult.MissingInput( $"Tex Object" );
		}
	};

	//public List<string> GetErrors()
	//{
	//	var errors = new List<string>();
	//
	//	//if ( !TextureObject.IsValid )
	//	//{
	//	//	errors.Add( $"input `Tex Object` is missing!" );
	//	//}
	//
	//	return errors;
	//}
}
