using static ShaderGraphPlus.GraphCompiler;

namespace ShaderGraphPlus;

public enum ResultType
{ 
	/// <summary>
	/// No Components, just True or False.
	/// </summary>
	Bool,
	/// <summary>
	/// 
	/// </summary>
	Int,
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
	/// <summary>
	/// 
	/// </summary>
	Sampler,
	/// <summary>
	/// 
	/// </summary>
	Texture2DObject,
	/// <summary>
	/// 
	/// </summary>
	TextureCubeObject,
	/// <summary>
	/// 
	/// </summary>
	Gradient,
	/// <summary>
	/// 
	/// </summary>
	Void,
	/// <summary>
	/// 
	/// </summary>
	Invalid,
}

public enum CastType
{
	FloatToInt,
	FloatToFloat,
	IntToFloat,
}

internal enum MetaDataType
{
	ImagePath,
	Bool,
	ComboSwitchInfo,
	ComboSwitchBody,
	VoidComponents,
	VoidResultUserDefinedName
}

public struct NodeResult : IValid
{
	public delegate NodeResult Func( GraphCompiler compiler );
	public string Code { get; private set; }
	public ResultType ResultType { get; private set; }
	public string[] Errors { get; private init; }
	public string[] Warnings { get; private init; }
	public bool IsDepreciated { get; private set; }
	public int VoidComponents { get; private set; }

	public readonly bool IsValid => ResultType != ResultType.Invalid && !string.IsNullOrWhiteSpace( Code );

	public bool SkipLocalGeneration { get; set; } = false;
	public string ImagePath { get; set; }
	public bool Constant { get; set; }
	public int PreviewID { get; set; }

	/// <summary>
	/// Generic-Ish metadata related to this NodeResult.
	/// </summary>
	internal Dictionary<string, object> Metadata { get; private set; }

	public readonly bool IsPreviewable
	{
		get
		{
			switch ( ResultType )
			{
				case ResultType.Bool:
					return false;
				case ResultType.Int:
					return true;
				case ResultType.Float:
					return true;
				case ResultType.Vector2:
					return true;
				case ResultType.Vector3:
					return true;
				case ResultType.Color:
					return true;
				case ResultType.Float2x2:
					return false;
				case ResultType.Float3x3:
					return false;
				case ResultType.Float4x4:
					return false;
				case ResultType.Sampler:
					return false;
				case ResultType.Texture2DObject:
					return false;
				case ResultType.TextureCubeObject:
					return false;
				case ResultType.Gradient:
					return false;
				case ResultType.Void:
					return false;
				case ResultType.Invalid:
					throw new Exception( "Result Type Is Invalid!" );
				default: 
					return false;
			}
		}
	}

	public readonly string TypeName
	{
		get
		{
			if ( ResultType is ResultType.Int )
			{
				return $"int"; // Just identify as a float.
			}	
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
			else if (ResultType is ResultType.Gradient )
			{
				return "Gradient";
			}
			else if ( ResultType is ResultType.Texture2DObject )
			{
				return $"Texture2D";
			}
			else if ( ResultType is ResultType.TextureCubeObject )
			{
				return $"TextureCube";
			}

			return "float";
		}
	}

	public readonly Type ComponentType
	{
		get
		{
			if ( ResultType is ResultType.Void )
			{
				return VoidComponents switch
				{
					int r when r == 0 => typeof( bool ),
					//int r when r == 1 => typeof( int ),
					int r when r == 1 => typeof( float ),
					int r when r == 2 => typeof( Vector2 ),
					int r when r == 3 => typeof( Vector3 ),
					int r when r == 4 => typeof( Color ),
					_ => throw new System.NotImplementedException(),
				};
			}
			else
			{
				return ResultType switch
				{
					ResultType.Bool => typeof( bool ),
					//ResultType.Int => typeof(int),
					ResultType.Float => typeof( float ),
					ResultType.Vector2 => typeof( Vector2 ),
					ResultType.Vector3 => typeof( Vector3 ),
					ResultType.Color => typeof( Color ),
					ResultType.Float2x2 => typeof( Float2x2 ),
					ResultType.Float3x3 => typeof( Float3x3 ),
					ResultType.Float4x4 => typeof( Float4x4 ),
					ResultType.Sampler => typeof( Sampler ),
					ResultType.Texture2DObject => typeof( Texture2DObject ),
					ResultType.TextureCubeObject => typeof( TextureCubeObject ),
					ResultType.Gradient => typeof( Gradient ),
					_ => throw new System.NotImplementedException(),
				};
			}
		}
	}

	public NodeResult( ResultType resultType, string code, bool constant = false, Dictionary<string, object> metadata = null, int voidComponents = 0 )
	{
		ResultType = resultType;
		Code = code;
		Constant = constant;
		
		if ( metadata == null )
		{
			Metadata = new();
		}
		else
		{
			Metadata = metadata;
		}

		VoidComponents = voidComponents;
	}

	public static NodeResult Error( params string[] errors ) => new() { Errors = errors };
	public static NodeResult Warning( params string[] warnings ) => new() { Warnings = warnings };
	public static NodeResult MissingInput( string name ) => Error( $"Missing required input '{name}'." );
	public static NodeResult Depreciated( (string,string) name ) => Error( $"'{name.Item1}' is depreciated please use '{name.Item2} instead'." );

	internal T GetMetadata<T>( string metaName, bool ignoreException = false )
	{
		if ( Metadata.TryGetValue( metaName, out var actualData ) )
		{
			if ( typeof( T ) == actualData.GetType() )
			{
				return (T)actualData;
			}
			else
			{
				throw new InvalidCastException( $"Generic type of `{typeof( T )}` is not of metadata actual data type `{actualData.GetType()}`" );
			}
		}

		if ( !ignoreException )
		{
			throw new Exception( $"Unable to get metadata with name `{metaName}`" );
		}

		return default( T );
	}

	internal bool TryGetMetaData<T>( string metaName, out T data )
	{
		data = default;

		if ( Metadata.TryGetValue( metaName, out var actualData ) )
		{
			if ( typeof( T ) == actualData.GetType() )
			{
				data = (T)actualData;
				return true;
			}
			else
			{
				return false;
				//throw new InvalidCastException( $"Generic type of `{typeof( T )}` is not of metadata actual data type `{actualData.GetType()}`" );
			}
		}

		return false;
	}

	internal void SetMetadata( string metaName, object actualData )
	{
		if ( !Metadata.ContainsKey( metaName ) )
		{
			Metadata.Add( metaName, actualData );
		}
		else
		{
			throw new Exception( "Metadata entry already exists!" );
		}
	}

	internal void SetMetadata( Dictionary<string, object> metadata )
	{
		Metadata = metadata;
	}

	internal void SetMetadataValue( string metaName, object actualData )
	{
		if ( Metadata.ContainsKey( metaName ) )
		{
			Metadata[metaName] = actualData;
		}
		else
		{
			//SGPLog.Warning( $"Metadata entry `{metaName}` dosent exist! Creating new Metadata entry instead." );
			SetMetadata( metaName, actualData );
		}
	}

	/// <summary>
	/// "Cast" this result to different float types
	/// </summary>
	public string Cast( int components, float defaultValue = 0.0f, CastType castType = CastType.FloatToFloat )
	{
		if ( ResultType is ResultType.Float2x2 || ResultType is ResultType.Float3x3 || ResultType is ResultType.Float4x4 )
		{
			throw new Exception( $"ResultType `{ResultType}` cannot be cast." );
		}

		if ( ResultType == ResultType.Int )
		{
			throw new NotImplementedException( $"{nameof( Cast )} : ResultType `{ResultType}` is not Implemented yet!" );
		}

		if ( ResultType is ResultType.Void )
		{
			if ( VoidComponents == components )
				return Code;

			if ( VoidComponents > components )
			{
				return $"{Code}.{"xyzw"[..components]}";
			}
			else if ( VoidComponents == 1 )
			{
				return $"float{components}( {string.Join( ", ", Enumerable.Repeat( Code, components ) )} )";
			}
			else
			{
				return $"float{components}( {Code}, {string.Join( ", ", Enumerable.Repeat( $"{defaultValue}", components - VoidComponents ) )} )";
			}
		}
		else
		{
			if ( Components() == components )
			{
				return Code;
			}
				

			if ( Components() > components )
			{
				return $"{Code}.{"xyzw"[..components]}";
			}
			else if ( Components() == 1 )
			{
				return $"float{components}( {string.Join( ", ", Enumerable.Repeat( Code, components ) )} )";
			}
			else
			{
				return $"float{components}( {Code}, {string.Join( ", ", Enumerable.Repeat( $"{defaultValue}", components - Components() ) )} )";
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
		
		switch ( ResultType )
		{
			case ResultType.Bool:
				components = 1;
				break;
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
				throw new Exception( $"ResultType `{ResultType}` has no components!" );
		}

		return components;
	}

	public override readonly string ToString()
	{
		return Code;
	}
}
