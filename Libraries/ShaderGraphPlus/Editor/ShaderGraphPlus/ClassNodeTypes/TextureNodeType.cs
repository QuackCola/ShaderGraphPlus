using NodeEditorPlus;

namespace ShaderGraphPlus;

public sealed class TextureNodeType : ClassNodeType
{
	string ImagePath;

	public TextureNodeType( TypeDescription type, string imagePath ) : base( type )
	{
		ImagePath = imagePath;
	}
	public override INodePlus CreateNode( INodeGraph graph )
	{
		var node = base.CreateNode( graph );
		if ( node is ITextureParameterNode textureNode )
		{
			textureNode.Image = ImagePath;
		}
		return node;
	}
}
