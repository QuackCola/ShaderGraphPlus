namespace ShaderGraphPlus;

public struct VoidData : IValid
{
	public string TargetResult;
	public ResultType ResultType;
	public string FunctionCall;
	public bool AlreadyDefined;
	public string BoundNodeId;

	public Dictionary<(string freindlyName,string compilerName), ResultType> Results;

	public bool IsValid
	{
		get
		{
			if ( Results != null && Results.Any() )
				return true;

			return false;
		}
	}

	public string ResultInit
	{
		get
		{
			switch ( ResultType )
			{
				case ResultType.Bool:
					return $"bool {TargetResult} = false;";
				case ResultType.Int:
					return $"int {TargetResult} = 0;";
				case ResultType.Float:
					return $"float {TargetResult} = 0.0f;";
				case ResultType.Vector2:
					return $"float2 {TargetResult} = float2( 0.0f, 0.0f );";
				case ResultType.Vector3:
					return $"float3 {TargetResult} = float3( 0.0f, 0.0f, 0.0f );";
				case ResultType.Color:
					return $"float4 {TargetResult} = float4( 0.0f, 0.0f, 0.0f, 0.0f );";
				case ResultType.Float2x2:
					return $"float2x2 {TargetResult} = float2x2( 0.0f, 0.0f, 0.0f, 0.0f );";
				case ResultType.Float3x3:
					return $"float3x3 {TargetResult} = float3x3( 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f );";
				case ResultType.Float4x4:
					return $"float4x4 {TargetResult} = float4x4( 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f );";
				default:
					throw new NotImplementedException( $"ResultType `{ResultType}` not implemented!" );
			}
		}
	}

	public string ResultInitNew( string name,ResultType resultType )
	{
		switch ( resultType )
		{
			case ResultType.Bool:
				return $"bool {name} = false;";
			case ResultType.Int:
				return $"int {name} = 0;";
			case ResultType.Float:
				return $"float {name} = 0.0f;";
			case ResultType.Vector2:
				return $"float2 {name} = float2( 0.0f, 0.0f );";
			case ResultType.Vector3:
				return $"float3 {name} = float3( 0.0f, 0.0f, 0.0f );";
			case ResultType.Color:
				return $"float4 {name} = float4( 0.0f, 0.0f, 0.0f, 0.0f );";
			case ResultType.Float2x2:
				return $"float2x2 {name} = float2x2( 0.0f, 0.0f, 0.0f, 0.0f );";
			case ResultType.Float3x3:
				return $"float3x3 {name} = float3x3( 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f );";
			case ResultType.Float4x4:
				return $"float4x4 {name} = float4x4( 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f );";
			default:
				throw new NotImplementedException( $"ResultType `{resultType}` not implemented!" );
		}
	}

	public ResultType GetResultResultType( string resultKey )
	{

		foreach ( var result in Results )
		{
			if ( result.Key.compilerName == resultKey )
			{
				return result.Value;
			}
		}
		
		{
			throw new Exception( $"Key `{resultKey}` does not exist within `{nameof( VoidData.Results )}`" );
		}
		//if ( Results.ContainsKey( resultKey ) )
		//{
		//	return Results[resultKey];
		//}
		//else
		//{
		//	throw new Exception( $"Key `{resultKey}` does not exist within `{nameof( VoidData.Results )}`" );
		//}
	}

	internal string GetCompilerName( string functionOutputName )
	{
		foreach ( var result in Results )
		{
			if ( result.Key.freindlyName == functionOutputName )
			{
				return result.Key.compilerName;
			}
		}

		throw new Exception( "Shits fucked..." );
	}
}
