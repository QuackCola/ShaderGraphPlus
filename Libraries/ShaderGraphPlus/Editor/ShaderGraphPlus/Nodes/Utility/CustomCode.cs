
using Sandbox;
using System.Numerics;
using System.Text;

namespace Editor.ShaderGraphPlus.Nodes;

/// <summary>
/// Simple container for custom expressions. Pretty basic for now.
/// </summary>
[Title( "Custom Expression" ), Category( "Utility" )]
public class CustomCodeNode : ShaderNodePlus//, IErroringNode
{

	[Hide]
	public override string Title => string.IsNullOrEmpty( Name ) ?
	$"{DisplayInfo.For( this ).Name}" :
	$"{DisplayInfo.For( this ).Name} ({Name})";

	public string Name { get; set; }

	//public string FunctionHeader { get; set; }

	[TextArea]
	public string Body { get; set; }
    //[Hide]
    public ResultType ResultType { get; set; } = ResultType.Float;


	[Title( "Inputs" )]
	public List<ExpressionInputs> ExpressionInputs { get; set; }

	[Hide]
	private List<IPlugIn> InternalInputs = new();

	[Hide]
	public override IEnumerable<IPlugIn> Inputs => InternalInputs;

	[Hide, JsonIgnore]
	int _lastHashCode = 0;

    [Hide]
    private List<NodeResult> _inputResults = new();

    [Hide, JsonIgnore]
    private string FunctionTest { get; set; } = string.Empty;

	[Output]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{

		//foreach (var input in Inputs)
		//{
		//	//Log.Info(input.ConnectedOutput.Node.Identifier);
		//	//
		//	//var result = compiler.GetByIdentifier( input.ConnectedOutput.Node.Identifier );
		//	//
		//	//if (!_inputResults.Contains(result))
		//	//{
		//	//	_inputResults.Add( result );
		//	//}
		//
		//	NodeInput nodeInput = new NodeInput { Identifier = input.ConnectedOutput.Node.Identifier, Output = input.ConnectedOutput.Identifier };
		//
		//	//if (nodeInput.IsValid)
		//	//{
		//	//
		//	//	var result = compiler.Result( nodeInput );
		//	//
		//	//	if (result.IsValid)
		//	//	{
        //    //        Log.Info($"Input NodeResult is valid `{result}`");
        //    //    }
		//	//
		//	//	
		//	//}
		//
		//}

		if ( !string.IsNullOrWhiteSpace( Name ) )
		{
            var sb = new StringBuilder();
			sb.AppendLine();
				sb.AppendLine($"{GetFuncType()} {Name}({GetFunctionInputs()})");
				sb.AppendLine("{" );
				sb.AppendLine(GraphCompiler.IndentString( Body, 1));
				sb.AppendLine("}");
            sb.AppendLine();

            FunctionTest = sb.ToString();


            return new(ResultType, compiler.ResultFunctionCustomExpression(FunctionTest, Name, args: $"1,1"));
        }
		else
		{
            return new(ResultType, $"1" );
        }
	};
	

	private string GetFunctionInputs()
	{
		var sb = new StringBuilder();

		int inputIndex = 0;

		foreach ( var input in ExpressionInputs)
		{
			if (inputIndex == 0)
			{
				sb.Append( $" {input.TypeNameTest} {input.Name}," );
				inputIndex++;
			}
			else if ( inputIndex != (ExpressionInputs.Count - 1) )
			{
                sb.Append( $" {input.TypeNameTest} {input.Name}," );
                inputIndex++;
            }
            else
            {
                sb.Append( $" {input.TypeNameTest} {input.Name} " );
            }
        }

        return sb.ToString();
	}

	private string GetFuncType()
	{


        if (ResultType is ResultType.Int)
        {
            return $"float"; // Just identify as a float.
        }
        else if (ResultType is ResultType.Float)
        {
            return $"float";
        }
        else if (ResultType is ResultType.Vector2 or ResultType.Vector3 or ResultType.Color)
        {
            return $"float{Components()}";
        }
        else if (ResultType is ResultType.Float2x2)
        {
            return "float2x2";
        }
        else if (ResultType is ResultType.Float3x3)
        {
            return "float3x3";
        }
        else if (ResultType is ResultType.Float4x4)
        {
            return "float4x4";
        }
        else if (ResultType is ResultType.Bool)
        {
            return "bool";
        }
        else if (ResultType is ResultType.String)
        {
            return "";
        }
        else if (ResultType is ResultType.Gradient)
        {
            return "Gradient";
        }

        return "float";
    }

    private int Components()
    {
        int components = 0;

        switch (ResultType)
        {
            case ResultType.Int:
                components = 1;
                break;
            case ResultType.Float:
                components = 1;
                break;
            case ResultType.Vector2:
                components = 2;
                break;
            case ResultType.Vector3:
                components = 3;
                break;
            case ResultType.Color:
                components = 4;
                break;
            case ResultType.Float2x2:
                components = 4;
                break;
            case ResultType.Float3x3:
                components = 9;
                break;
            case ResultType.Float4x4:
                components = 16;
                break;
            default:
                Log.Warning($"Result type: '{ResultType}' has no components.");
                break;
        }

        return components;
    }

    public override void OnFrame()
	{
		var hashCode = 0;
		foreach ( var output in ExpressionInputs )
		{
			hashCode += output.GetHashCode();
		}
		if ( hashCode != _lastHashCode )
		{
			_lastHashCode = hashCode;

			CreateInputs();
			Update();
		}
	}

	public void CreateInputs()
	{
		var plugs = new List<IPlugIn>();
		if ( ExpressionInputs == null )
		{
			InternalInputs = new();
		}
		else
		{
			foreach ( var output in ExpressionInputs.OrderBy( x => x.Priority ) )
			{
				if ( output.Type is null ) continue;
				var info = new PlugInfo()
				{
					Id = output.Id,
					Name = output.Name,
					Type = output.Type,
					DisplayInfo = new()
					{
						Name = output.Name,
						Fullname = output.Type.FullName
					}
				};
				var plug = new BasePlugIn( this, info, info.Type );
				var oldPlug = InternalInputs.FirstOrDefault( x => x is BasePlugIn plugIn && plugIn.Info.Id == info.Id ) as BasePlugIn;
				if ( oldPlug is not null )
				{
					oldPlug.Info.Name = info.Name;
					oldPlug.Info.Type = info.Type;
					oldPlug.Info.DisplayInfo = info.DisplayInfo;
					plugs.Add( oldPlug );
				}
				else
				{
					plugs.Add( plug );
				}
			};
			InternalInputs = plugs;
		}
	}
	
	
}
public class ExpressionInputs
{
	[Hide]
	public Guid Id { get; } = Guid.NewGuid();

	[KeyProperty]
	public string Name { get; set; }

	[Hide, JsonIgnore]
	public Type Type
	{
		get
		{
			if ( string.IsNullOrEmpty( TypeName ) ) return null;
			var typeName = TypeName;
			if ( typeName == "float" ) typeName = typeof( float ).FullName;
			if ( typeName == "int" ) typeName = typeof( int ).FullName;
			if ( typeName == "bool" ) typeName = typeof( bool ).FullName;
			var type = TypeLibrary.GetType( typeName ).TargetType;
			return type;
		}
	}

	[KeyProperty, Editor( "shadertype" ), JsonPropertyName( "Type" )]
	public string TypeName { get; set; }

    public int Priority { get; set; }

    public string TypeNameTest
    {
        get
        {
            if ( TypeName is "int" )
            {
                return $"float"; // Just identify as a float.
            }
            else if (TypeName is "float")
            {
                return $"float";
            }
            else if (TypeName is "Vector2" )
            {
                return $"float2";
            }
            else if (TypeName is "Vector3")
            {
                return $"float3";
            }
            else if (TypeName is "Vector4")
            {
                return $"float4";
            }
            else if (TypeName is "Color")
            {
                return $"float4";
            }
            else if (TypeName is "bool")
            {
                return "bool";
            }

            return "float";
        }
    }

    public override int GetHashCode()
	{
		return System.HashCode.Combine( Id, Name, TypeName, Priority );
	}
}