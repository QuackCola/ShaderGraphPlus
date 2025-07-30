using Sandbox;
using System.Text;
using static Sandbox.Gizmo;

namespace ShaderGraphPlus.Nodes;

public struct IOData
{
	public string Code;
	public string SplitCode;
	public bool IsOutput;
}

/// <summary>
/// Get the dimensions of a Texture2D Object in the width and height.
/// </summary>
[Title( "Get Dimensions" ), Category( "Textures" ), Icon( "straighten" )]
public sealed class GetDimensionsNode : VoidFunctionNodeBase//ShaderNodePlus//, IErroringNode
{
	[Title( "Tex Object" )]
	[Input( typeof( Texture2DObject ) )]
	[Ignore]
	[Hide]
	public NodeInput TextureObject { get; set; }

	[JsonIgnore, Hide]
	public override bool CanPreview => false;

	[JsonIgnore, Hide, VoidFunctionResult( "float2", true, "float" ) ]
	public string OutA { get; set; } = "";

	public override string FunctionName { get; set; } = $"##FUNCNAME##";

	public override string PreProcessFuncArgs( string funcArgs )
	{
		SGPLog.Info( $"PreProcessing FuncArgs `{funcArgs}`" );
		return funcArgs;
	}

	public override Dictionary<string, IOData> GetFunctionInputOutputSlots()
	{
		var ioDataMap = new Dictionary<string, IOData>();

		var outA = new IOData();
		outA.Code = "OutA";
		outA.SplitCode = "OutA.x, OutA.y";
		outA.IsOutput = true;

		ioDataMap.Add( "OutA", outA );

		return ioDataMap;
	}

	[Output( typeof( Vector2 ) )]
	[Title( "Tex Size" )]
	[Hide]
	public NodeResult.Func TextureSize => ( GraphCompiler compiler ) =>
	{
		var textureObject = compiler.Result( TextureObject );

		if ( textureObject.IsValid )
		{
			/*
			var texObject = $"{textureObject.Code}";
			var userDefinedName = $"{texObject}_wh";
			//compiler.RegisterVoid(
			//	ResultType.Vector2,
			//	userDefinedName,
			//	$"{textureObject.Code}.GetDimensions( {userDefinedName}.x, {userDefinedName}.y );"
			//);
			
			//Dictionary<string, object> metadata = new Dictionary<string, object>();
			List<string> inputs = new();
			Dictionary<string, string> outputs = new();
			outputs.Add( userDefinedName, "float2" );
			
			var funcArgs = $"{userDefinedName}.x, {userDefinedName}.y";
			var funcTargetOutput = "";
			var result = compiler.RegisterVoidNew( $"{textureObject.Code}.GetDimensions", funcArgs, funcTargetOutput, inputs, outputs, Identifier );

			return new NodeResult( ResultType.Vector2, result, constant: false );
			//var funcResult = customFunctionNode.GetResult( this, nameof( MetadataType.VoidResultUserDefinedName ), input.Output );
			//metadata.Add( nameof( MetadataType.VoidResultUserDefinedName ), "metadataValue" );
			*/

			var funcName = $"{textureObject.Code}.GetDimensions";

			List<string> inputs = new List<string>();

			//inputs.Add( $"{textureObject.Code}" );
			compiler.PostProcessVoidResult( inputs, Identifier, funcName );

			return new NodeResult( ResultType.Vector2, OutA, constant: false );

		}
		else
		{
			return NodeResult.MissingInput( $"Tex Object" );
		}
	};

}

public interface IVoidFunctionNode
{
	public void RegisterIncludes( GraphCompiler compiler );
	public void RegisterVoidFunction( GraphCompiler compiler );
}

public abstract class VoidFunctionNodeBase : ShaderNodePlus, IVoidFunctionNode
{

	public virtual string FunctionName { get; set; }

	public virtual Dictionary<string, IOData> GetFunctionInputOutputSlots()
	{
		return new Dictionary<string, IOData>();
	}

	public virtual void RegisterIncludes( GraphCompiler compiler )
	{
	}

	public virtual string PreProcessFuncArgs( string funcArgs )
	{
		return funcArgs;
	}

	private int GetComponents( string dataType )
	{
		switch ( dataType )
		{
			case "float":
				return 1;
			case "float2":
				return 2;
			case "float3":
				return 3;
			case "float4":
				return 4;
			default:
				return 0;
		}
	}


	public void RegisterVoidFunction( GraphCompiler compiler )
	{
		var inputProperties = GetProperties( this.GetType(), false );
		var outputProperties = GetProperties( this.GetType(), true );
		List<string> inputs = new();
		Dictionary<string, string> inputs2 = new();
		Dictionary<(string name, bool seperate), string> outputs = new();

		int index = 0 ;
		foreach ( var inputProperty in inputProperties )
		{
			var nodeInput = (NodeInput)inputProperty.GetValue( this );
			var titleAttrib = inputProperty.GetCustomAttribute<TitleAttribute>();
			var inputAttrib = inputProperty.GetCustomAttribute<InputAttribute>();
			var ignoreAttrib = inputProperty.GetCustomAttribute<IgnoreAttribute>();
			var title = inputProperty.Name;

			if ( inputAttrib is null )
				continue;

			if ( ignoreAttrib is not null )
				continue;

			if ( titleAttrib is not null )
			{
				title = titleAttrib.Value;
			}

			//SGPLog.Info( $"Got NodeInput property `{title}` of type `{inputAttrib.Type}`", compiler.IsPreview );
			//funcArgsSb.Append( (output.Index + 1) == outputs.Count ? $"{output.Item.Key}" : $" {output.Item.Key}, " );
			
			// Going to use string.Format to replace each {index}.
			inputs2.Add( title, $"{{{index++}}}" );
		}

		foreach ( var property in outputProperties )
		{
			var attrib = property.GetCustomAttribute<VoidFunctionResultAttribute>();

			if ( attrib is null )
				continue;

			//SGPLog.Info( $"Output `{property.Name}` will be split? : {attrib.SplitOutput}", compiler.IsPreview );

			//SGPLog.Info( $"Got Property `{property.Name}` of type `{attrib.HLSLType}`", compiler.IsPreview );

			var propertyName = property.Name;

			//if ( attrib.SplitOutput )
			//{
			//	var splitto = attrib.SplitToHLSLType;
			//
			//	var components = GetComponents( attrib.HLSLType );
			//	var splitComponents = GetComponents( attrib.SplitToHLSLType );
			//
			//	SGPLog.Info( $"sourceComponents is `{components}` when target is `{splitComponents}`", compiler.IsPreview );
			//
			//	if ( components > splitComponents )
			//	{
			//		
			//		var cast = $"{"xyzw"[..splitComponents]}";
			//
			//		
			//		propertyName = $"{propertyName}.{cast}, {propertyName}.{cast}";
			//
			//		SGPLog.Info( $"propertyName == {propertyName}", compiler.IsPreview );
			//	}
			//
			//	// OutA ---> OutA = "OutA.x, OutA.y"
			//}

			outputs.Add( (propertyName, attrib.SplitOutput), attrib.HLSLType );
		}

		if ( !string.IsNullOrWhiteSpace( FunctionName ) )
		{
			var funcName = FunctionName;
			StringBuilder funcArgsSb = new();
			StringBuilder funcArgsSb2 = new();
			
			// Append the inputs
			foreach ( var input in inputs2.Index() )
			{
				//SGPLog.Info( $"Input `{input.Item.Key}` with string.Format placeholder `{input.Item.Value}`" );

				//funcArgsSb2.Append( (input.Index + 1) == inputs.Count ? $"{input.Item.Value}" : $" {input.Item.Value}, " );
				funcArgsSb.Append( (input.Index + 1) == inputs.Count ? $"{input.Item.Value}" : $"{(outputs.Any() ? $" {input.Item.Value}," : $"{input.Item.Value}")} " );
			}

			// Append the outputs
			foreach ( var output in outputs.Index() )
			{
				var outputEntry = output.Item.Key.name;

				if ( output.Item.Key.seperate )
				{
					if ( GetFunctionInputOutputSlots().ContainsKey( outputEntry ) )
					{
						var data = GetFunctionInputOutputSlots()[ outputEntry ];

						outputEntry = $"{data.SplitCode}";//$"{outputEntry}.x, {outputEntry}.y";
					}
				}

				funcArgsSb.Append( (output.Index + 1) == outputs.Count ? $"{outputEntry}" : $"{outputEntry}, " );
			}

			SGPLog.Info( $"funcArgsSb == `{funcArgsSb.ToString()}`" );

			var funcArgs = PreProcessFuncArgs( funcArgsSb.ToString() );
			var funcTargetOutput = "";
	
			var result = compiler.RegisterVoidNew2( funcName, funcArgs, funcTargetOutput, inputs2, outputs, Identifier, out var funcOutputs );
			
			foreach ( var funcOutput in funcOutputs )
			{
				var prop = this.GetType().GetProperty( funcOutput.userDefined );
			
				if ( prop != null )
				{
					prop.SetValue( this, funcOutput.CompilerDefined, null );
					//SGPLog.Info( $"Set prop to `{prop.GetValue( this )}`" );
				}
			}
		}
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

//[Title( "TestFunc" ), Category( "Textures" ), Icon( "straighten" )]
//public sealed class TestFuncNode : VoidFunctionNodeBase
//{
//	public override void RegisterIncludes( GraphCompiler compiler )
//	{
//		compiler.RegisterInclude( $"TestFuncs.hlsl" );
//	}
//
//	[Title( "InA" )]
//	[Input( typeof( float ) )]
//	[Hide]
//	public NodeInput InputA { get; set; }
//
//	[JsonIgnore, Hide]
//	public override bool CanPreview => false;
//
//	[JsonIgnore, Hide]
//	public override string FunctionName { get; set; } = "TestFunc";
//
//	[JsonIgnore, Hide, VoidFunctionResult( "float2", false )]
//	public string OutA { get; set ;} = "";
//	
//	[JsonIgnore, Hide, VoidFunctionResult( "float4", false )]
//	public string OutB { get; set; } = "";
//
//	[Output( typeof( Vector2 ) )]
//	[Title( "OutA" )]
//	[Hide]
//	public NodeResult.Func ResultA => ( GraphCompiler compiler ) =>
//	{
//		if ( string.IsNullOrWhiteSpace( OutA ) )
//			return NodeResult.Error( $"Output OutA is empty!" );
//
//		SGPLog.Info( $"OutA is `{OutA}`", compiler.IsPreview );
//
//		List<string> inputs = new List<string>();
//
//		var inputA = compiler.ResultOrDefault( InputA, 2.0f );
//
//		inputs.Add( $"{inputA.Code}");
//		compiler.PostProcessVoidResult( inputs, Identifier );
//
//		return new NodeResult( ResultType.Vector2, OutA, constant: false );
//	};
//
//	[Output( typeof( Color ) )]
//	[Title( "OutB" )]
//	[Hide]
//	public NodeResult.Func ResultB => ( GraphCompiler compiler ) =>
//	{
//		if ( string.IsNullOrWhiteSpace( OutB ) )
//			return NodeResult.Error( $"Output OutB is empty!" );
//		
//		SGPLog.Info( $"OutB is `{OutB}`", compiler.IsPreview );
//
//		List<string> inputs = new List<string>();
//		
//		var inputA = compiler.ResultOrDefault( InputA, 2.0f );
//		inputs.Add( $"{inputA.Code}" );
//
//		compiler.PostProcessVoidResult( inputs, Identifier );
//
//		return new NodeResult( ResultType.Color, OutB, constant: false );
//	};
//
//}
