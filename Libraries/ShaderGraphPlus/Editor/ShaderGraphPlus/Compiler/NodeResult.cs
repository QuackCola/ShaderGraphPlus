namespace Editor.ShaderGraphPlus;

public enum ResultType
{ 
	Bool,
	Float,
	Vector2,
	Vector3,
	Color,
	Float2x2,
	Float3x3,
	Float4x4,
	Sampler,
	TextureObject,
	String,
	Gradient,
}

public struct NodeResult : IValid
{
	public delegate NodeResult Func( GraphCompiler compiler );
	public string Code { get; private set; }

	/// <summary>
	/// Holds other shit. mainly #if blocks and whatnot for static and dynamic combos if need be.
	/// </summary>
	//public string CodeTwo { get; private set; }
	public ResultType ResultType { get; private set; }
	public bool Constant { get; set; }
	public string[] Errors { get; private init; }
	public string[] Warnings { get; private init; }
	public bool IsComponentLess { get; set; }

	public bool IsDepreciated { get; set; }
	public readonly bool IsValid => ResultType > (ResultType)(-1) && !string.IsNullOrWhiteSpace( Code );

	// Old
	//public readonly string TypeName => ResultType > ResultType.Float ? $"float{(int)ResultType}" : ResultType == ResultType.Bool ? "bool" : "float";

	// Handling more than just floats now so this should do...
	public readonly string TypeName
	{
		get
		{
			if ( ResultType is ResultType.Float )
			{
				return $"float";
			}
			else if ( ResultType is ResultType.Vector2 or ResultType.Vector3 or ResultType.Color )
			{
				return $"float{(int)ResultType}";
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
			return ResultType switch
			{
				ResultType.Bool => typeof( bool ),
				ResultType.Float => typeof( float ),
				ResultType.Vector2 => typeof( Vector2 ),
				ResultType.Vector3 => typeof( Vector3 ),
				ResultType.Color => typeof( Color ),
				ResultType.Float2x2 => typeof( Float2x2 ),
				ResultType.Float3x3 => typeof( Float3x3 ),
				ResultType.Float4x4 => typeof( Float4x4 ),
				ResultType.Sampler => typeof( Sampler ),
				ResultType.TextureObject => typeof( TextureObject ),
				ResultType.String => typeof( string ), // Generic Result
				ResultType.Gradient => typeof( Gradient ),
				_ => throw new System.NotImplementedException(),
			};
		}
	}

	public NodeResult( ResultType resulttype, string code, bool constant = false, bool iscomponentless = false)
	{
		ResultType = resulttype;
		Code = code;
		//CodeTwo = codetwo;
		Constant = constant;
		IsComponentLess = iscomponentless;
	}

	public static NodeResult Error( params string[] errors ) => new() { Errors = errors };
	public static NodeResult Warning( params string[] warnings ) => new() { Warnings = warnings };
	public static NodeResult MissingInput( string name ) => Error( $"Missing required input '{name}'." );
	public static NodeResult Depreciated( (string,string) name ) => Error( $"'{name.Item1}' is depreciated please use '{name.Item2} instead'." );

	/// <summary>
	/// "Cast" this result to different float types
	/// </summary>
	public string Cast( int components, float defaultValue = 0.0f )
	{
		if ( ResultType == (ResultType)components )
			return Code;

		if ( ResultType > (ResultType)components )
		{
			return $"{Code}.{"xyzw"[..components]}";
		}
		else if ( ResultType == (ResultType)1 )
		{
			return $"float{components}( {string.Join( ", ", Enumerable.Repeat( Code, components ) )} )";
		}
		else
		{
			return $"float{components}( {Code}, {string.Join( ", ", Enumerable.Repeat( $"{defaultValue}", (ResultType)components - ResultType ) )} )";
		}
	}

	public readonly int Components()
	{
		return (int)ResultType;
	}

	public override readonly string ToString()
	{
		return Code;
	}
}
