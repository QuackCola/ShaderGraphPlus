namespace Editor.ShaderGraphPlus;

public static class ResultTypeExtentions
{

	public static string HLSLDataType( this ResultType resultType )
	{
		return resultType switch
		{
			ResultType r when r == ResultType.Bool => "bool",
			//ResultType r when r == ResultType.Int => "int",
			ResultType r when r == ResultType.Float => "float",
			ResultType r when r == ResultType.Vector2 => "float2",
			ResultType r when r == ResultType.Vector3 => "float3",
			ResultType r when r == ResultType.Color => "float4",
			ResultType r when r == ResultType.Gradient => "Gradient",
			ResultType r when r == ResultType.Float2x2 => "float2x2",
			ResultType r when r == ResultType.Float3x3 => "float3x3",
			ResultType r when r == ResultType.Float4x4 => "float4x4",
			ResultType r when r == ResultType.Void => "void",
			_ => throw new ArgumentException( "Unsupported value type", nameof( resultType ) )
		};
	}



}
