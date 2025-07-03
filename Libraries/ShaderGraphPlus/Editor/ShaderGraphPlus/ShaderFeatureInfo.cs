namespace Editor.ShaderGraphPlus;

/// <summary>
/// Container for the generated shader feature info.
/// </summary>
public struct ShaderFeatureInfo : IValid
{
	public string UserDefinedName { get; private set; }
	public string FeatureName { get; private set; }
	public string FeatureDeclaration { get; private set; }
	public int OptionsCount { get; private set; }
	public bool IsDynamicCombo { get; private set; }

	public bool IsValid => string.IsNullOrWhiteSpace( FeatureName );

	public ShaderFeatureInfo( string userDefinedName, string featureName, string featureDeclaration, int optionsCount, bool isDynamicCombo )
	{
		UserDefinedName = userDefinedName;
		FeatureName = featureName;
		FeatureDeclaration = featureDeclaration;
		OptionsCount = optionsCount;
		IsDynamicCombo = isDynamicCombo;
	}

	public ShaderFeatureInfo()
	{
		UserDefinedName = "";
		FeatureName = "";
		FeatureDeclaration = "";
		OptionsCount = 0;
		IsDynamicCombo = false;
	}
}
