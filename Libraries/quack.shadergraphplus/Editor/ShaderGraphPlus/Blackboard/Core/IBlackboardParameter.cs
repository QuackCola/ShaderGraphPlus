
namespace ShaderGraphPlus;

internal interface IBlackboardParameter
{
	Guid Identifier { get; }

	string Name { get; set; }
}
