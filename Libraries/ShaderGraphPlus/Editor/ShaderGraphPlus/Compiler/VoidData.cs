namespace ShaderGraphPlus;

internal struct TargetResultData : IValid
{
	public string UserAssignedName;
	public string CompilerAssignedName;
	public ResultType ResultType;

	public bool IsValid
	{
		get
		{
			if ( !string.IsNullOrWhiteSpace( UserAssignedName ) || ResultType != ResultType.Invalid )
				return true;

			return false;
		}
	}
}

internal struct VoidData : IValid
{
	internal string TargetResult;
	internal ResultType ResultType;
	internal string FunctionCall;
	internal bool AlreadyDefined;
	//internal bool AlreadyProcessed { get; set ;}
	/// <summary>
	/// Is this void data ment for a void function call or inline code?
	/// </summary>
	internal bool InlineCode;
	/// <summary>
	/// The Identifier of the node that this data is bound to.
	/// </summary>
	internal string BoundNodeIdentifier;
	internal List<TargetResultData> TargetResults;

	public bool IsValid
	{
		get
		{
			if ( TargetResults != null && TargetResults.Any() )
				return true;

			return false;
		}
	}

	internal string ResultInit
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

	internal string ResultInitNew( string name, ResultType resultType )
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

	internal ResultType GetResultResultType( string compilerAssignedName )
	{
		var result = TargetResults.Where( x => x.CompilerAssignedName == compilerAssignedName ).FirstOrDefault();

		if ( result.IsValid )
			return result.ResultType;

		throw new Exception( $"Key `{compilerAssignedName}` does not exist within `{nameof( VoidData.TargetResults )}`" );
	}

	internal string GetCompilerAssignedName( string userAssignedName )
	{
		var result = TargetResults.Where( x => x.UserAssignedName == userAssignedName ).FirstOrDefault();

		if ( result.IsValid )
			return result.CompilerAssignedName;

		throw new Exception( "Shits fucked..." );
	}
}
