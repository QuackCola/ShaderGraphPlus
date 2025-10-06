
using NodeEditorPlus;

namespace ShaderGraphPlus;

internal interface IBlackboardParameterType
{
	public TypeDescription Type { get; }
	IBlackboardParameter CreateParameter( ShaderGraphPlus graph );
}
