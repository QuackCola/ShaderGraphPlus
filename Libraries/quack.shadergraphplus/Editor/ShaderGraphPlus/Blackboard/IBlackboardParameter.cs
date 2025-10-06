
namespace ShaderGraphPlus;

internal interface IBlackboardParameter
{
	public Guid Identifier { get; }
	string Name { get; set; }

	Color GetTypeColor( BlackboardView view );
}
