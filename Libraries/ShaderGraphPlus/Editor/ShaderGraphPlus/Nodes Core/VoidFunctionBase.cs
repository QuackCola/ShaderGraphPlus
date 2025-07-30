namespace ShaderGraphPlus.Nodes;

public interface IVoidFunctionNode
{
	public void RegisterIncludes( GraphCompiler compiler );
	public void RegisterVoidFunction( GraphCompiler compiler, string functionName = "", string functionArgs = "" );
}

public abstract class VoidFunctionBase : ShaderNodePlus, IVoidFunctionNode
{
	public virtual string FunctionName { get; }
	public virtual string FunctionArgs { get; }

	public virtual void RegisterIncludes( GraphCompiler compiler )
	{
	}

	public void RegisterVoidFunction( GraphCompiler compiler, string functionName = "", string functionArgs = "" )
	{
		if ( compiler.CheckIfVoidFunctionIsRegisterd( Identifier ) )
		{
			SGPLog.Info( $"Node with ID `{Identifier}` has already been registerd!", compiler.IsPreview );
			return;
		}

		var inputProperties = GetNodeProperties( this.GetType(), false );
		var outputProperties = GetNodeProperties( this.GetType(), true );
		Dictionary<string, string> inputs = new();
		Dictionary<string, string> outputs = new();

		functionName = FunctionName;
		functionArgs = FunctionArgs;

		if ( !string.IsNullOrWhiteSpace( functionName ) )
		{
			foreach ( var property in outputProperties.Index() )
			{
				var attribute = property.Item.GetCustomAttribute<VoidFunctionArgumentAttribute>();

				if ( attribute is null )
					continue;

				functionArgs = functionArgs.Replace( attribute.VarName, property.Item.Name );
	
				outputs.Add( $"{property.Item.Name}", attribute.HLSLDataType );
			}

			compiler.RegisterVoidFunction( functionName, functionArgs, inputs, outputs, Identifier, out var functionOutputs );

			foreach ( var funcOutput in functionOutputs )
			{
				var propertyInfo = this.GetType().GetProperty( funcOutput.userAssigned );

				if ( propertyInfo != null )
				{
					propertyInfo.SetValue( this, funcOutput.compilerAssigned, null );
				}
			}
		}
	}

	public static IEnumerable<PropertyInfo> GetNodeProperties( Type type , bool output )
	{
		if ( !output )
		{
			return type.GetProperties( BindingFlags.Instance | BindingFlags.Public )
				.Where( property => property.GetSetMethod() != null &&
				property.PropertyType == typeof( NodeInput ) &&
				property.IsDefined( typeof( BaseNodePlus.InputAttribute ), false ) );
		}
		else
		{
			return type.GetProperties( BindingFlags.Instance | BindingFlags.Public )
				.Where( property => property.GetSetMethod() != null &&
				property.PropertyType == typeof( string ) &&
				property.IsDefined( typeof( BaseNodePlus.VoidFunctionArgumentAttribute ), false ) );
		}
	}
}
