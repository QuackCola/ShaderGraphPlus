namespace ShaderGraphPlus.Nodes;

public abstract class MatrixParameterNode<T> : ShaderNodePlus
{
	public string Name { get; set; } = "";

	[Hide]
	public override string Title => string.IsNullOrWhiteSpace(Name) ?
		$"{DisplayInfo.For(this).Name}" :
		$"{DisplayInfo.For(this).Name} ( {Name} )";

	[InlineEditor]
	public T Value { get; set; }

	/// <summary>
	/// Enable this if you want to be able to set this via an Attribute via code. 
	/// False means it wont be generated as a global in the generated shader and thus will be local to the code.
	/// </summary>
	[Hide]
	public bool IsAttribute { get; set; } = false;
}
