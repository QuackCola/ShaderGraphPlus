using System.Text;

namespace Editor.ShaderGraphPlus.Nodes;

/// <summary>
/// Container for HLSL code.
/// </summary>
[Title( "Custom Function" ), Category( "Utility" ), Icon( "code" )]
public class CustomFunctionNode : ShaderNodePlus, IErroringNode
{
    public enum CustomCodeNodeMode
    {
        /// <summary>
        /// Inlines the code within body, into the generated shader.
        /// </summary>
        Inline,
        /// <summary>
        /// Retrive the function from an external hlsl include file.
        /// </summary>
        File,
    }

    [Hide]
    public override string Title => string.IsNullOrEmpty( Name ) ?
    $"{DisplayInfo.For( this ).Name}" :
    $"{DisplayInfo.For( this ).Name} ({Name})";

	[Hide, JsonIgnore]
    public override bool CanPreview => false;

    public string Name { get; set; }
    
    public CustomCodeNodeMode Type { get; set; } = CustomCodeNodeMode.Inline;
    
    
    [TextArea]
    [HideIf( nameof( Type ), CustomCodeNodeMode.File )]
    public string Body { get; set; }
    
    [HideIf( nameof( Type ), CustomCodeNodeMode.Inline )]
    [HLSLAssetPath]
    public string Source { get; set; }
    
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
    public List<CustomCodeOutputData> OutputData { get; set; } = new();
    
    public void OnNodeCreated()
    {
        CreateInputs();
        CreateOutputs();
        
        Update();
    }

    [Hide, JsonIgnore]
    public string id2;


    public NodeResult GetResult( GraphCompiler compiler )
    {
        if ( Type is CustomCodeNodeMode.File )
        {
            compiler.RegisterVoidFunctionResults(this, GetFunctionVoidLocals(), out List<CustomCodeOutputData> outputData, out List<string> functionOutputs);

            StringBuilder sb = new StringBuilder();

            // Construct function outputs, for example : out float output01, out float2 output02
            foreach ( var voidLocal in functionOutputs)
            {
                sb.Append( voidLocal == functionOutputs.Max() ? $"{voidLocal} " : $" {voidLocal}, ");
            }

            if ( !OutputData.Any() )
            {
                OutputData = outputData;
            }
            else
            {
                //SGPLog.Warning( "Output Data was already generated!" );
            }

            var functionInputs = GetInputResults(compiler);
            string funcCall = compiler.ResultFunctionCustomExpression( this, $"{Source}", Name, args: $" {functionInputs}{(ExpressionInputs.Any() ? "," : "")}{sb.ToString()}", true ) + ";";

            return new NodeResult( ResultType.Void, funcCall, voidComponents: 0 );
        }
        else if ( Type is CustomCodeNodeMode.Inline )
        {
            compiler.RegisterVoidFunctionResults( this, GetFunctionVoidLocals(), out List<CustomCodeOutputData> outputData, out List<string> functionOutputs, true );
            
            if ( !OutputData.Any() )
            {
                OutputData = outputData;
            }
            else
            {
                //SGPLog.Warning( "Output Data was already generated!" );
            }
            
            StringBuilder sb = new StringBuilder();
            var inputs = GetInputResultsInline( compiler );
            
            sb.AppendLine( "{" );
            sb.AppendLine( Body );
            sb.AppendLine( "};" );
            
            // Relpace the user defined input names with the compiler assigned names.
            foreach ( var input in inputs )
            {
                sb.Replace( input.Item1, input.Item2 );
            }
            
            // Relpace the user defined output names with the compiler assigned names.
            foreach ( var data in OutputData )
            {
                sb.Replace( data.FriendlyName, data.CompilerName );
            }
            
            return new( ResultType.Void, sb.ToString(), voidComponents: 0 );
        }


        return NodeResult.Error( $"{DisplayInfo.Name}( {Name} ) Something is fucked!" );
    }

    /// <summary>
    /// Fetches the results from the user defined node inputs.
    /// </summary>
    private string GetInputResults( GraphCompiler compiler )
    {
        StringBuilder sb = new StringBuilder();
        int index = 0;
        
        foreach ( IPlugIn input in Inputs )
        {
            if ( compiler.IsNotPreview )
            {
                if ( compiler.Debug )
                {
                    Log.Info( $"Evaluating Input `{input.DisplayInfo.Name}` from `{input.ConnectedOutput}`" );
                }
            }
            
            NodeResult result = new NodeResult();
            
            if ( input.ConnectedOutput is null ) // TODO : Should the user be able to define a default or should it just be 0.0f?
            {
                result = new NodeResult( ResultType.Float, $"0", constant: true );
            }
            else
            {
                NodeInput nodeInput = new NodeInput { Identifier = input.ConnectedOutput.Node.Identifier, Output = input.ConnectedOutput.Identifier };
            
                result = compiler.Result( nodeInput );
            }
            
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

    private List<(string, string)> GetInputResultsInline( GraphCompiler compiler )
    {
        StringBuilder sb = new StringBuilder();
        int index = 0;
        List<(string, string)> inputResults = new List<(string,string)>();
        
        foreach ( IPlugIn input in Inputs )
        {
            if ( compiler.IsNotPreview )
            {
                if ( compiler.Debug )
                {
                    Log.Info( $"Evaluating Input `{input.DisplayInfo.Name}` from `{input.ConnectedOutput}`" );
                }
            }
            
            NodeResult result = new NodeResult();
            
            if ( input.ConnectedOutput is null ) // TODO : Should the user be able to define a default or should it just be 0.0f?
            {
                result = new NodeResult( ResultType.Float, $"0", constant: true );
            }
            else
            {
                NodeInput nodeInput = new NodeInput { Identifier = input.ConnectedOutput.Node.Identifier, Output = input.ConnectedOutput.Identifier };
            
                result = compiler.Result( nodeInput );
            }
            
            //Log.Info($" Result : {result.Code}");
            
            inputResults.Add( ( input.DisplayInfo.Name, result.Code )  );
            
            index++;
        }
        
        return inputResults;
    }

    internal string ConstructFunctionInputs()
    {
        var sb = new StringBuilder();
        
        for ( int index = 0; index < ExpressionInputs.Count; index++ )
        {
            var input = ExpressionInputs[index];
            
            sb.Append(index == ExpressionInputs.Count - 1 ? $" {input.HLSLDataType} {input.Name}" : $" {input.HLSLDataType} {input.Name},");
        }
        
        return sb.ToString();
    }

    internal string ConstructFunctionOutputs()
    {
        var sb = new StringBuilder();
        
        for ( int index = 0; index < ExpressionOutputs.Count; index++ )
        {
            var input = ExpressionOutputs[index];
            
            sb.Append( index == ExpressionOutputs.Count - 1 ? $" out {input.HLSLDataType} {input.Name} " : $" out {input.HLSLDataType} {input.Name}," );
        }
        
        return sb.ToString();
    }

    private Dictionary<string, string> GetFunctionVoidLocals()
    {
        Dictionary<string, string> result = new();

        foreach ( CustomCodeNodePorts output in ExpressionOutputs )
        {
            result.Add( output.Name, output.HLSLDataType );
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

            //Log.Info($"Updating Outputs!");

            CreateOutputs();
            Update();
        }
    }


    //[Hide, JsonIgnore]
    //internal Dictionary<IPlugIn, (IParameterNode, Type)> InputReferences = new();

    public void CreateInputs()
    {
        var plugs = new List<IPlugIn>();

        //var defaults = new Dictionary<Type, int>();
        //InputReferences.Clear();


        if ( ExpressionInputs == null )
        {
        	InternalInputs = new();
        }
        else
        {
        	foreach ( var input in ExpressionInputs.OrderBy( x => x.Priority ) )
        	{
        		if ( input.Type is null ) continue;
        		var info = new PlugInfo()
        		{
        			Id = input.Id,
        			Name = input.Name,
        			Type = input.Type,
        			DisplayInfo = new()
        			{
        				Name = input.Name,
        				Fullname = input.Type.FullName
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
        if ( ExpressionOutputs == null )
        {
            InternalOutputs = new();
        }
        else
        {
            foreach ( var output in ExpressionOutputs.OrderBy( x => x.Priority ) )
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
                var plug = new BasePlugOut( this, info, info.Type );
                var oldPlug = InternalOutputs.FirstOrDefault( x => x is BasePlugOut plugOut && plugOut.Info.Id == info.Id ) as BasePlugOut;
                if (oldPlug is not null)
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
            InternalOutputs = plugs;
        }
    }

    public List<string> GetErrors()
    {
        OnNodeCreated();
        var errors = new List<string>();
        
        if ( !ExpressionOutputs.Any() )
        {
            errors.Add( $"`{DisplayInfo.Name}` has no outputs." );
        }
        
        if ( string.IsNullOrWhiteSpace( Name ) )
        {
            if ( Type is CustomCodeNodeMode.File )
            {
                return new List<string> { $"`{DisplayInfo.Name}` Cannot call function with no name!" };
            }
            else
            {
                return new List<string> { $"`{DisplayInfo.Name}` Cannot generate a function with no name!" };
            }
        }
        
        if ( Type is CustomCodeNodeMode.File )
        {
            if ( string.IsNullOrWhiteSpace( Source ) )
            {
                errors.Add( $"`{DisplayInfo.Name}` Source path is empty!" );
            }
            
            if ( !Editor.FileSystem.Content.FileExists( $"shaders/{Source}" ) )
            {
                errors.Add( $"Include file `shaders/{Source}` does not exist." );
            }
        }
        
        return errors;
    }
}

public struct CustomCodeOutputData : IValid
{
    public string FriendlyName { get; set; }
    public string CompilerName { get; set; }
    public string DataType { get; set; }
    public int ComponentCount { get; set; }
    public ResultType ResultType { get; set; }
    public string NodeId { get; set; }

    public readonly bool IsValid => !string.IsNullOrWhiteSpace( FriendlyName );

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
            if ( typeName == "Texture2D" ) typeName = typeof( Texture2DObject ).FullName;
            var type = TypeLibrary.GetType( typeName ).TargetType;
            return type;
        }
    }
    
    [KeyProperty, Editor( "portType" ), JsonPropertyName( "Type" )]
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
            else if (TypeName is "Texture2D" )
            {
                return "Texture2D";
            }

            throw new ArgumentException("Unsupported value type", nameof( TypeName ) );
        }
    }
    
    public override int GetHashCode()
    {
        return System.HashCode.Combine( Id, Name, TypeName, Priority );
    }
}
