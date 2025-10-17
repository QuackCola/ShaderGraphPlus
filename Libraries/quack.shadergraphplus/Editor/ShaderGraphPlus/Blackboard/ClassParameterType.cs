namespace ShaderGraphPlus;

internal class ClassParameterType : IBlackboardParameterType
{
	public virtual string Identifier => Type.FullName;
	public TypeDescription Type { get; }

	public ClassParameterType( TypeDescription type )
	{
		Type = type;
	}

	public virtual IBlackboardParameter CreateParameter( ShaderGraphPlus graph, string name = "" )
	{
		if ( EditorTypeLibrary.Create( Type.Name, Type.TargetType ) is BaseBlackboardParameter blackboardParameter )
		{
			blackboardParameter.Name = name;

			return blackboardParameter;
		}
		else
		{
			throw new Exception( $"Failed to create blackboard parameter instance of type \"{Type.Name}\"" );
		}
	}
}
