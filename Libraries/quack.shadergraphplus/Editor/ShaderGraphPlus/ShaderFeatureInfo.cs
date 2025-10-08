using static Sandbox.Internal.IControlSheet;

namespace ShaderGraphPlus;

/// <summary>
/// Container for the generated shader feature info.
/// </summary>
public struct ShaderFeatureInfo : IValid
{
	public string UserDefinedName { get; private set; }
	public string FeatureDescription { get; private set; }
	public string FeatureHeader { get; private set; }
	public int OptionsCount { get; private set; }
	public bool IsDynamicCombo { get; private set; }
	public bool IsEnumFeature { get; private set; }
	public List <string> Options { get; private set; }

	[Hide, JsonIgnore]
	public bool IsValid => !string.IsNullOrWhiteSpace( PlaceHolder ) ? true : !string.IsNullOrWhiteSpace( UserDefinedName );

	// Internal use only!
	internal string PlaceHolder { get; set; }

	/// <summary>
	/// How many times this feature has been referenced.
	/// </summary>
	[Hide, JsonIgnore]
	internal int ReferenceCount { get; set; } = 0;

	public ShaderFeatureInfo( string userDefinedName, string featureDescription, string featureHeader, int optionsCount, bool isDynamicCombo, bool isEnumFeature, List<string> options )
	{
		UserDefinedName = userDefinedName;
		FeatureDescription = featureDescription;
		FeatureHeader = featureHeader;
		OptionsCount = optionsCount;
		IsDynamicCombo = isDynamicCombo;
		IsEnumFeature = isEnumFeature;
		Options = options;
	}

	public ShaderFeatureInfo()
	{
		UserDefinedName = "";
		FeatureDescription = "";
		FeatureHeader = "";
		OptionsCount = 0;
		IsDynamicCombo = false;
		IsEnumFeature = false;
		PlaceHolder = "";
		Options = new List<string>();
	}

	[Hide, JsonIgnore]
	public readonly string CreateFeature => $"F_{UserDefinedName.Replace( " ", "_" ).ToUpper()}";


	public readonly string CreateCombo( string name,  bool previewOverride = false )
	{
		if ( IsDynamicCombo || previewOverride ) return $"D_{name.Replace( " ", "_" ).ToUpper()}";
		//if ( previewOverride ) return $"D_{name.Replace( " ", "_" ).ToUpper()}";
		if ( !IsDynamicCombo ) return $"S_{name.Replace( " ", "_" ).ToUpper()}";
		return default;
	}

	public override string ToString()
	{
		return !string.IsNullOrWhiteSpace( PlaceHolder ) ? PlaceHolder : UserDefinedName;
	}
}
