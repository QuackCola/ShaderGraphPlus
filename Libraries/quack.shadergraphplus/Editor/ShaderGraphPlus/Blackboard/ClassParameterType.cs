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
		var parameter = EditorTypeLibrary.Create( Type.Name, Type.TargetType ) as BaseBlackboardParameter;

		if ( parameter == null )
		{
			throw new Exception( "Failed to create a blackboard parameter instance!" );
		}
		else
		{
			parameter.Name = name;
		}

		return parameter;
	}
}
