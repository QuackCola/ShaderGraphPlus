namespace Editor.ShaderGraphPlus;

public struct VoidData
{
	public string TargetResult;
	public ResultType ResultType;
	public string FunctionCall;
	public bool AlreadyDefined;

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
}
