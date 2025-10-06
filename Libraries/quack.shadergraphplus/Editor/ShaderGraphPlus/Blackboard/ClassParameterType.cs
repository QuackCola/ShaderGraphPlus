
using NodeEditorPlus;

namespace ShaderGraphPlus;

internal class ClassParameterType : IBlackboardParameterType
{
	public virtual string Identifier => Type.FullName;
	public TypeDescription Type { get; }

	public ClassParameterType( TypeDescription type )
	{
		Type = type;
	}

	public virtual IBlackboardParameter CreateParameter( ShaderGraphPlus graph )
	{
		var parameter = Type.Create<BaseBlackboardParameter>();

		parameter.Graph = graph;

		return parameter;
	}

}
