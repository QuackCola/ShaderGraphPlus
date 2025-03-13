using Sandbox;
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

    [Title( "Outputs" )]
    public List<ExpressionInputs> ExpressionOutputs { get; set; }

    [Hide]
    private List<IPlugOut> InternalOutputs = new();

    [Hide]
    public override IEnumerable<IPlugOut> Outputs => InternalOutputs;


    [Hide, JsonIgnore]
	int _lastHashCodeInputs = 0;

    [Hide, JsonIgnore]
    int _lastHashCodeOutputs = 0;

    //[Output]
	//[Hide]
	//public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	//{
    //    if ( !string.IsNullOrWhiteSpace( Name ) )
	//	{
    //        var sb = new StringBuilder();
	//		sb.AppendLine();
	//			sb.AppendLine( $"{GetFuncReturnType()} {Name}({GetFunctionInputs()})" );
	//			sb.AppendLine( "{" );
	//			sb.AppendLine( GraphCompiler.IndentString( Body, 1) );
	//			sb.AppendLine( "}" );
    //        sb.AppendLine();
    //
    //        var results = GetResults( compiler );
    //        
    //        Log.Info( $"Gatherd results `{results}`" );
    //
    //        return new(ResultType, compiler.ResultFunctionCustomExpression(sb.ToString(), Name, args: results ));
    //    }
	//	else
	//	{
    //        return new(ResultType, $"1" );
    //    }
	//};
	
    public NodeResult BuildFunction( GraphCompiler compiler )
    {
        if (!string.IsNullOrWhiteSpace(Name))
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"{compiler.GetDataType( ResultType )} {Name}({GetFunctionInputs()})");
            sb.AppendLine("{");
            sb.AppendLine(GraphCompiler.IndentString(Body, 1));
            sb.AppendLine("}");
            sb.AppendLine();

            var results = GetResults(compiler);

            Log.Info($"Gatherd results `{results}`");

            return new(ResultType, compiler.ResultFunctionCustomExpression(sb.ToString(), Name, args: results));
        }
        else
        {
            return new(ResultType, $"1");
        }
    }

    public void OnNodeCreated()
    {
        CreateInputs();
        CreateOutputs();

        Update();
    }

    private string GetResults( GraphCompiler compiler )
    {
        var sb = new StringBuilder();
        int index = 0;

        foreach ( IPlugIn input in Inputs )
        {
            NodeInput nodeInput = new NodeInput { Identifier = input.ConnectedOutput.Node.Identifier, Output = input.ConnectedOutput.Identifier };
        
            var result = compiler.Result(nodeInput);
        
            if ( index == 0 )
            {
                sb.Append( $"{result}, " );
                index++;
            }
            else if ( index != (Inputs.Count() - 1) )
            {
                sb.Append( $"{result}, " );
                index++;
            }
            else
            {
                sb.Append( $"{result}" );
            }
        }
    
        return sb.ToString();
    }

    private string GetFunctionInputs()
    {
        var sb = new StringBuilder();
        int index = 0;
        
        foreach ( ExpressionInputs input in ExpressionInputs )
        {
            if ( index == 0 )
            {
            	sb.Append( $" {input.HLSLDataType} {input.Name}," );
            	index++;
            }
            else if ( index != (ExpressionInputs.Count - 1) )
            {
                sb.Append( $" {input.HLSLDataType} {input.Name}," );
                index++;
            }
            else
            {
                sb.Append( $" {input.HLSLDataType} {input.Name} " );
            }
        }
        
        return sb.ToString();
    }

    private string GetFunctionOutputs()
    {
        var sb = new StringBuilder();
        int index = 0;

        foreach ( ExpressionInputs output in ExpressionOutputs )
        {
            if (index == 0)
            {
                sb.Append($" out {output.HLSLDataType} {output.Name},");
                index++;
            }
            else if (index != (ExpressionOutputs.Count - 1))
            {
                sb.Append($" out {output.HLSLDataType} {output.Name},");
                index++;
            }
            else
            {
                sb.Append($" out {output.HLSLDataType} {output.Name} ");
            }
        }

        return sb.ToString();
    }


    public override void OnFrame()
    {
        var hashCodeInput = 0;
        var hashCodeOutput = 0;

        foreach ( var input in ExpressionInputs )
        {
        	hashCodeInput += input.GetHashCode();
        }

        foreach ( var output in ExpressionOutputs )
        {
            hashCodeOutput += output.GetHashCode();
        }


        if ( hashCodeInput != _lastHashCodeInputs )
        {
        	_lastHashCodeInputs = hashCodeInput;
        
        	CreateInputs();
        	Update();
        }

        if ( hashCodeOutput != _lastHashCodeOutputs )
        {
            _lastHashCodeOutputs = hashCodeOutput;

            CreateOutputs();
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

    [Hide, JsonIgnore]
    internal Dictionary<IPlugOut, IPlugIn> OutputReferences = new();

    public void CreateOutputs()
    {
        var plugs = new List<IPlugOut>();
        if (ExpressionOutputs == null)
        {
            InternalOutputs = new();
        }
        else
        {
            foreach (var output in ExpressionOutputs.OrderBy(x => x.Priority))
            {
                if (output.Type is null) continue;
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
                var plug = new BasePlugOut(this, info, info.Type);
                var oldPlug = InternalOutputs.FirstOrDefault(x => x is BasePlugOut plugOut && plugOut.Info.Id == info.Id) as BasePlugOut;
                if (oldPlug is not null)
                {
                    oldPlug.Info.Name = info.Name;
                    oldPlug.Info.Type = info.Type;
                    oldPlug.Info.DisplayInfo = info.DisplayInfo;
                    plugs.Add(oldPlug);
                }
                else
                {
                    plugs.Add(plug);
                }
            };
            InternalOutputs = plugs;
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
    
    [KeyProperty, Editor( "shadertypeplus" ), JsonPropertyName( "Type" )]
    public string TypeName { get; set; }
    
    public int Priority { get; set; }

    [Hide, JsonIgnore]
    public string HLSLDataType
    {
        get
        {
            if ( TypeName is "int" )
            {
                return $"float"; // Just identify as a float.
            }
            else if ( TypeName is "float" )
            {
                return $"float";
            }
            else if ( TypeName is "Vector2" )
            {
                return $"float2";
            }
            else if ( TypeName is "Vector3" )
            {
                return $"float3";
            }
            else if ( TypeName is "Vector4" )
            {
                return $"float4";
            }
            else if ( TypeName is "Color" )
            {
                return $"float4";
            }
            else if ( TypeName is "bool" )
            {
                return "bool";
            }
            
            throw new ArgumentException("Unsupported value type", nameof( TypeName ) );
        }
    }
    
    public override int GetHashCode()
    {
    	return System.HashCode.Combine( Id, Name, TypeName, Priority );
    }
}