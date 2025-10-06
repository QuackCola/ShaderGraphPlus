
using NodeEditorPlus;
using ShaderGraphPlus.Nodes;

namespace ShaderGraphPlus;

public sealed class ParameterNodeType : ClassNodeType
{
	public BaseBlackboardParameter BlackboardParameter { get; private set; }

	private readonly string Name;
	private readonly Type BlackboardParameterType;

	public ParameterNodeType( TypeDescription type, Type targetBlackboardType,string name ) : base( type )
	{
		Name = name;
		BlackboardParameterType = targetBlackboardType;
	}

	public override INodePlus CreateNode( INodeGraph graph )
	{
		var node = base.CreateNode( graph );



		//var nodeType = new NamedRerouteDeclarationNodeType( EditorTypeLibrary.GetType<NamedRerouteDeclarationNode>(), name );

		//BlackboardParameter = BaseBlackboardParameter.CreateTypeInstance( BlackboardParameterType, Name, Guid.NewGuid() );

		node = BlackboardParameter.InitializeNode();

		return node;
	}
}
