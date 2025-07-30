
using Facepunch.ActionGraphs;

namespace ShaderGraphPlus.Nodes;
public interface IVoidFunctionNode
{
	public void RegisterIncludes( GraphCompiler compiler );
	public void RegisterVoidFunction( GraphCompiler compiler, string functionName, string functionArgs );
}

public abstract class VoidFunctionBase : ShaderNodePlus, IVoidFunctionNode
{
	public virtual void RegisterIncludes( GraphCompiler compiler )
	{
	}

	public void RegisterVoidFunction( GraphCompiler compiler, string functionName ,string functionArgs )
	{
		if ( compiler.CheckIfVoidFunctionIsRegisterd( Identifier ) )
		{
			SGPLog.Info( $"Node with ID `{Identifier}` has already been registerd!", compiler.IsPreview );
			return;
		}

		//var inputProperties = GetProperties( this.GetType(), false );
		var outputProperties = GetProperties( this.GetType(), true );
		Dictionary<string, string> inputs = new();
		Dictionary<string, string> outputs = new();

		if ( !string.IsNullOrWhiteSpace( functionName ) )
		{
			foreach ( var property in outputProperties.Index() )
			{
				var attribute = property.Item.GetCustomAttribute<VoidFunctionResultAttribute>();

				if ( attribute is null )
					continue;

				outputs.Add( $"{property.Item.Name}", attribute.HLSLType );
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

	public NodeResult GetResult( GraphCompiler compiler, string targetPropertyName )
	{
		var property = this.GetType().GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static )
			.FirstOrDefault( x => x.GetGetMethod() != null && x.PropertyType == typeof( NodeResult.Func ) && x.Name == targetPropertyName );


		//foreach ( var propertyInfo in property )
		{
			SGPLog.Info( $"Found NodeResult.Func Property `{property.Name}`", compiler.IsPreview );
		}

		//RegisterVoidFunction( compiler, "TestFunc", $"{inputA.Code}, {nameof( OutA )}, {nameof( OutB )}" );

		//var result = compiler.Result( new NodeInput { Identifier = Identifier, Output = nameof( Result ) } );


		return default( NodeResult );
	}

	public static IEnumerable<PropertyInfo> GetProperties( Type type , bool output )
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
				property.IsDefined( typeof( BaseNodePlus.VoidFunctionResultAttribute ), false ) );
		}
	}
}
