namespace Editor.ShaderGraphPlus;

public enum ResultType
{ 
	/// <summary>
	/// No Components, just True or False.
	/// </summary>
	Bool,

   // Int,
    /// <summary>
    /// 1 Component
    /// </summary>
    Float,
	/// <summary>
	/// 2 Component's
	/// </summary>
	Vector2,
	/// <summary>
	/// 3 Component's
	/// </summary>
	Vector3,
    /// <summary>
    /// 4 Component's
    /// </summary>
    Color,
    /// <summary>
    /// 4 Component's
    /// </summary>
    Float2x2,
    /// <summary>
    /// 9 Component's
    /// </summary>
    Float3x3,
    /// <summary>
    /// 16 Component's
    /// </summary>
    Float4x4,
	Sampler,
	TextureObject,
	String,
	Gradient,
	/// <summary>
	/// Dosen't return normally. uses `out` instead.
	/// </summary>
	Void,
    Inline,
}

public struct NodeResult : IValid
{
	public delegate NodeResult Func( GraphCompiler compiler );
	public string Code { get; private set; }
    public string Code2 { get; private set; }
    public ResultType ResultType { get; private set; }
	public bool Constant { get; set; }
	public string[] Errors { get; private init; }
	public string[] Warnings { get; private init; }
	public bool IsComponentLess { get; set; }
	public bool IsDepreciated { get; private set; }
    public readonly bool IsValid => ResultType > (ResultType)(-1) && !string.IsNullOrWhiteSpace( Code );

    public bool IsVoidResult { get; private set; }
	public int VoidComponents { get; private set; }

    public readonly string TypeName
    {
        get
        {
            //if ( ResultType is ResultType.Int )
            //{
            //    return $"float"; // Just identify as a float.
            //}	
            if ( ResultType is ResultType.Float )
            {
            	return $"float";
            }
            else if ( ResultType is ResultType.Vector2 or ResultType.Vector3 or ResultType.Color )
            {
            	return $"float{Components()}";
            }
            else if ( ResultType is ResultType.Float2x2 )
            {
            	return "float2x2";
            }
            else if ( ResultType is ResultType.Float3x3 )
            {
            	return "float3x3";
            }
            else if ( ResultType is ResultType.Float4x4 )
            {
            	return "float4x4";
            }
            else if ( ResultType is ResultType.Bool )
            {
            	return "bool";
            }
            else if ( ResultType is ResultType.String )
            {
            	return "";
            }
            else if (ResultType is ResultType.Gradient)
            {
                return "Gradient";
            }
            
            return "float";
        }
    }

	public readonly Type ComponentType
	{
		get
		{
			if (ResultType is ResultType.Void)
			{
                return VoidComponents switch
                {
                    int r when r == 0 => typeof(bool),
                    //int r when r == 1 => typeof(int),
                    int r when r == 1 => typeof(float),
                    int r when r == 2 => typeof(Vector2),
                    int r when r == 3 => typeof(Vector3),
                    int r when r == 4 => typeof(Color),
                    _ => throw new System.NotImplementedException(),
                };
            }
			else
			{
                return ResultType switch
                {
                    ResultType.Bool => typeof(bool),
                    //ResultType.Int => typeof(int),
                    ResultType.Float => typeof(float),
                    ResultType.Vector2 => typeof(Vector2),
                    ResultType.Vector3 => typeof(Vector3),
                    ResultType.Color => typeof(Color),
                    ResultType.Float2x2 => typeof(Float2x2),
                    ResultType.Float3x3 => typeof(Float3x3),
                    ResultType.Float4x4 => typeof(Float4x4),
                    ResultType.Sampler => typeof(Sampler),
                    ResultType.TextureObject => typeof(TextureObject),
                    ResultType.String => typeof(string), // Generic Result
                    ResultType.Gradient => typeof(Gradient),
                    _ => throw new System.NotImplementedException(),
                };
            }

		}
	}

	public NodeResult( ResultType resulttype, string code, bool constant = false, bool iscomponentless = false, int voidComponents = 0, bool isVoidResult = false, string code2 = "" )
	{
		ResultType = resulttype;
		Code = code;
		Constant = constant;
		IsComponentLess = iscomponentless;
		VoidComponents = voidComponents;
        IsVoidResult = isVoidResult;
        Code2 = code2;
    }

    public static NodeResult Error( params string[] errors ) => new() { Errors = errors };
	public static NodeResult Warning( params string[] warnings ) => new() { Warnings = warnings };
	public static NodeResult MissingInput( string name ) => Error( $"Missing required input '{name}'." );
	public static NodeResult Depreciated( (string,string) name ) => Error( $"'{name.Item1}' is depreciated please use '{name.Item2} instead'." );

	/// <summary>
	/// "Cast" this result to different float types
	/// </summary>
	public string Cast( int components, float defaultValue = 0.0f)
	{

        if ( ResultType is ResultType.Void )
		{
			//Log.Info($"Result has `{VoidComponents}` void components");
            if ( VoidComponents == components )
                return Code;

            if ( VoidComponents > components )
            {
                return $"{Code}.{"xyzw"[..components]}";
            }
            else if ( VoidComponents == 1 )
            {
                return $"float{components}( {string.Join(", ", Enumerable.Repeat(Code, components))} )";
            }
            else
            {
                return $"float{components}( {Code}, {string.Join(", ", Enumerable.Repeat($"{defaultValue}", components - VoidComponents))} )";
            }


        }
        else
		{
            if (Components() == components)
                return Code;

            if (Components() > components)
            {
                return $"{Code}.{"xyzw"[..components]}";
            }
            else if (Components() == 1)
            {
                return $"float{components}( {string.Join(", ", Enumerable.Repeat(Code, components))} )";
            }
            else
            {
                return $"float{components}( {Code}, {string.Join(", ", Enumerable.Repeat($"{defaultValue}", components - Components()))} )";
            }
        }

	}

	/// <summary>
	/// Returns the components
	/// </summary>
	/// <returns></returns>
	public readonly int Components()
	{
        int components = 0;
        //if ( ResultType is ResultType.Void )
        //{
        //	return VoidComponents;
        //}
        //else
        //{
        switch ( ResultType )
        {
            //case ResultType.Int:
            //    components = 1;
            //    break;
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
        //}
    }

	public override readonly string ToString()
	{
		return Code;
	}
}
