namespace Editor.ShaderGraphPlus.Nodes;

/// <summary>
/// Get the dimentions of a Texture2D Object in the width and height.
/// </summary>
[Title( "Get Dimentions" ), Category( "Textures" ), Icon( "straighten" )]
public sealed class GetDimentionsNode : ShaderNodePlus//, IErroringNode
{
	[Title( "Tex Object" )]
	[Input( typeof( TextureObject ) )]
	[Hide]
	public NodeInput TextureObject { get; set; }

	[Output( typeof( Vector2 ) )]
	[Hide]
	public NodeResult.Func TextureSize => ( GraphCompiler compiler ) =>
	{
		var textureObject = compiler.Result( TextureObject );

		if ( textureObject.IsValid )
		{

			compiler.RegisterVoid(
				$"float2 {textureObject.Code}_wh = float2( 0, 0 );",
				$"{textureObject.Code}_wh",
				$"{textureObject.Code}.GetDimensions( {textureObject.Code}_wh.x, {textureObject.Code}_wh.y );"
			);


			return new NodeResult( ResultType.Vector2, $"{textureObject.Code}_wh", constant: false );
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
