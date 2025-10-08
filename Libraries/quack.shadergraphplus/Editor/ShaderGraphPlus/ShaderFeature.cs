using static Sandbox.Resources.ResourceGenerator;

namespace ShaderGraphPlus;

public class ShaderFeatureBase : IValid
{
	/// <summary>
	/// Name of this feature.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// What this feature does.
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// Header Name of this Feature that shows up in the Material Editor.
	/// </summary>
	public string HeaderName { get; set; }

	[Hide, JsonIgnore, Browsable( false )]
	public virtual bool IsValid => throw new NotImplementedException();

	public ShaderFeatureBase()
	{
		Name = "";
		Description = "";
		HeaderName = "";
	}
}

public class ShaderFeatureBoolean : ShaderFeatureBase
{
	[Hide, JsonIgnore, Browsable( false )]
	public override bool IsValid => !string.IsNullOrWhiteSpace( Name );

	public ShaderFeatureBoolean() : base()
	{ 

	}

	public override int GetHashCode()
	{
		return System.HashCode.Combine( Name, Description, HeaderName );
	}
}

public class ShaderFeatureEnum : ShaderFeatureBase
{
	[Hide, JsonIgnore, Browsable( false )]
	public override bool IsValid => !string.IsNullOrWhiteSpace( Name ) && Options.All( x => !string.IsNullOrWhiteSpace( x ) );

	/// <summary>
	/// Options of your feature. Must have no special characters. Note : all lowercase letters will be converted to uppercase.
	/// </summary>
	public List<string> Options { get; set; }

	public ShaderFeatureEnum() : base()
	{
		Options = new List<string>();
	}

	public override int GetHashCode()
	{
		var hashcode = System.HashCode.Combine( Name, Description, HeaderName );

		foreach ( var option in Options )
		{
			hashcode += option.GetHashCode();
		}

		return hashcode;
	}
}
