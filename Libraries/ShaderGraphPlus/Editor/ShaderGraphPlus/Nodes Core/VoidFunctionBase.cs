namespace ShaderGraphPlus.Nodes;

public interface IVoidFunctionNode
{
	public void RegisterIncludes( GraphCompiler compiler );
	public void RegisterVoidFunction( GraphCompiler compiler );
}

public abstract class VoidFunctionBase : ShaderNodePlus, IVoidFunctionNode
{
	public virtual string FunctionName { get; }
	public virtual string FunctionArgs { get; }

	public virtual void RegisterIncludes( GraphCompiler compiler )
	{
	}

	public void RegisterVoidFunction( GraphCompiler compiler )
	{
		if ( compiler.CheckIfVoidFunctionIsRegisterd( Identifier ) )
		{
			SGPLog.Info( $"Node with ID `{Identifier}` has already been registerd!", compiler.IsPreview );
			return;
		}

		var outputProperties = GetNodeVoidFunctionOutputProperties( this.GetType() );
		Dictionary<string, string> outputs = new();

		var functionName = FunctionName;
		var functionArgs = FunctionArgs;

		if ( !string.IsNullOrWhiteSpace( functionName ) )
		{
			foreach ( var property in outputProperties.Index() )
			{
				var attribute = property.Item.GetCustomAttribute<VoidFunctionOutputArgumentAttribute>();

				if ( attribute is null )
					continue;

				functionArgs = functionArgs.Replace( attribute.VarName, property.Item.Name );
	
				outputs.Add( $"{property.Item.Name}", attribute.HLSLDataType );
			}
			
			compiler.RegisterVoidFunction( functionName, functionArgs, outputs, Identifier, out var functionOutputs );

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

	internal static IEnumerable<PropertyInfo> GetNodeInputProperties( Type type )
	{
		return type.GetProperties( BindingFlags.Instance | BindingFlags.Public )
			.Where( property => property.GetSetMethod() != null &&
			property.PropertyType == typeof( NodeInput ) &&
			property.IsDefined( typeof( BaseNodePlus.InputAttribute ), false ) &&
			property.IsDefined( typeof( VoidFunctionInputArgumentAttribute ), false )
			);
	}

	internal static IEnumerable<PropertyInfo> GetNodeVoidFunctionOutputProperties( Type type )
	{
		return type.GetProperties( BindingFlags.Instance | BindingFlags.Public )
			.Where( property => property.GetSetMethod() != null &&
			property.PropertyType == typeof( string ) &&
			property.IsDefined( typeof( VoidFunctionOutputArgumentAttribute ), false ) );
	}

	[System.AttributeUsage( AttributeTargets.Property )]
	public sealed class VoidFunctionInputArgumentAttribute : Attribute
	{
		public string HLSLDataType;
		public string VarName;
		public string VarDefault;

		public VoidFunctionInputArgumentAttribute( ResultType resultType, string varName, string varDefault ) : this( resultType, varName )
		{
			VarDefault = varDefault;
		}

		public VoidFunctionInputArgumentAttribute( ResultType resultType, string varName )
		{
			HLSLDataType = resultType.GetHLSLDataType();
			VarName = varName;
		}

	}

	[System.AttributeUsage( AttributeTargets.Property )]
	public sealed class VoidFunctionOutputArgumentAttribute : Attribute
	{
		public string HLSLDataType;
		public string VarName;

		public VoidFunctionOutputArgumentAttribute( ResultType resultType, string varName )
		{
			HLSLDataType = resultType.GetHLSLDataType();
			VarName = varName;
		}
	}
}
