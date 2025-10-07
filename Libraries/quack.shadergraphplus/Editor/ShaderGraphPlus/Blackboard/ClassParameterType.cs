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
		var parameterInstance = EditorTypeLibrary.Create( Type.Name, Type.TargetType );

		if ( parameterInstance is BaseBlackboardParameter blackboardParameter )
		{
			blackboardParameter.Name = name;

			return blackboardParameter;
		}
		else
		{
			throw new Exception( "Failed to create a blackboard parameter instance!" );
		}
	}
}
