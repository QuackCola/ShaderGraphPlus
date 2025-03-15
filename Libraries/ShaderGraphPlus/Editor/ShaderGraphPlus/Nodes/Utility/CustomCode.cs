using System.Text;

namespace Editor.ShaderGraphPlus.Nodes;

/// <summary>
/// Container for custom code.
/// </summary>
[Title( "Custom Code" ), Category( "Utility" )]
public class CustomCodeNode : ShaderNodePlus//, IErroringNode
{
    [Hide]
    public override string Title => string.IsNullOrEmpty( Name ) ?
    $"{DisplayInfo.For( this ).Name}" :
    $"{DisplayInfo.For( this ).Name} ({Name})";
    
    public string Name { get; set; }
    
    [TextArea]
    public string Body { get; set; }

    [Hide, JsonIgnore]
    public ResultType ResultType = ResultType.Void;
    
    [Title( "Inputs" )]
    public List<CustomCodeNodePorts> ExpressionInputs { get; set; }
    
    [Hide]
    private List<IPlugIn> InternalInputs = new();
    
    [Hide]
    public override IEnumerable<IPlugIn> Inputs => InternalInputs;
    
    [Title( "Outputs" )]
    public List<CustomCodeNodePorts> ExpressionOutputs { get; set; }
    
    [Hide]
    private List<IPlugOut> InternalOutputs = new();
    
    [Hide]
    public override IEnumerable<IPlugOut> Outputs => InternalOutputs;

    [Hide, JsonIgnore]
    int _lastHashCodeInputs = 0;
    
    [Hide, JsonIgnore]
    int _lastHashCodeOutputs = 0;
    
    [Hide, JsonIgnore]
    public Dictionary<string, string> _OutputMappings { get; set; } = new();
    
    [Hide, JsonIgnore]
    public List<CustomCodeOutputData> OutputData { get; set; } = new();
    
    [Hide, JsonIgnore]
    public bool AlreadyGeneratedFunc { get; set; } = false;

    public void OnNodeCreated()
    {
        CreateInputs();
        CreateOutputs();
        
        Update();
    }

    public NodeResult ConstructFunction( GraphCompiler compiler )
    {
        if ( !string.IsNullOrWhiteSpace( Name ) )
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine( $"{compiler.GetHLSLDataType( ResultType )} {Name}({ConstructFunctionInputs()}, {ConstructFunctionOutputs()})" );
            sb.AppendLine( "{" );
            sb.AppendLine( GraphCompiler.IndentString( Body, 1 ) );
            sb.AppendLine( "}" );
            sb.AppendLine();
    
            var results = GetInputResults( compiler );
            OutputData = new List<CustomCodeOutputData>();
    
            compiler.RegisterVoidFunctionResults( GetFunctionVoidLocals(), out string functionOutputs, out List<CustomCodeOutputData> outputData );
            OutputData = outputData;
    
            return new( ResultType, compiler.ResultFunctionCustomExpression( sb.ToString(), Name, args: $" {results}, {functionOutputs}" ), voidComponents: 0);
        }
        else
        {
            return new( ResultType, $"1" );
        }
    }

    /// <summary>
    /// Fetches the results from the user defined node inputs.
    /// </summary>
    /// <param name="compiler"></param>
    /// <returns></returns>
    private string GetInputResults( GraphCompiler compiler )
    {
        var sb = new StringBuilder();
        int index = 0;
        
        foreach ( IPlugIn input in Inputs )
        {
            NodeInput nodeInput = new NodeInput { Identifier = input.ConnectedOutput.Node.Identifier, Output = input.ConnectedOutput.Identifier };
            
            var result = compiler.Result(nodeInput);
            
            if ( index < Inputs.Count() - 1 )
            {
                sb.Append($"{result}, ");
            }
            else
            {
                sb.Append($"{result}");
            }
            
            index++;
        }
        
        return sb.ToString();
    }

    private string ConstructFunctionInputs()
    {
        var sb = new StringBuilder();
        
        for ( int index = 0; index < ExpressionInputs.Count; index++ )
        {
            var input = ExpressionInputs[index];
            
            sb.Append(index == ExpressionInputs.Count - 1 ? $" {input.HLSLDataType} {input.Name} " : $" {input.HLSLDataType} {input.Name},");
        }
        
        return sb.ToString();
    }

    private string ConstructFunctionOutputs()
    {
        var sb = new StringBuilder();
        
        for ( int index = 0; index < ExpressionOutputs.Count; index++ )
        {
            var input = ExpressionOutputs[index];
            
            sb.Append( index == ExpressionOutputs.Count - 1 ? $" out {input.HLSLDataType} {input.Name} " : $" out {input.HLSLDataType} {input.Name}," );
        }
        
        return sb.ToString();
    }

    private List<(string,string)> GetFunctionVoidLocals()
    {
        List<(string, string)> result = new();

        foreach ( CustomCodeNodePorts output in ExpressionOutputs )
        {
            result.Add( new ( output.HLSLDataType, output.Name ) );
        }

        return result;
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

public struct CustomCodeOutputData
{
    public string FriendlyName { get; set; }
    public string CompilerName { get; set; }
    
    public int ComponentCount { get; set; }
    
    public CustomCodeOutputData()
    {
    
    }
    
    public CustomCodeOutputData( int components )
    {
        ComponentCount = components;
    }
}

public class CustomCodeNodePorts
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