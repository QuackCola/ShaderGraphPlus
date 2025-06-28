namespace Editor.ShaderGraphPlus;

/// <summary>
/// Container for the generated shader feature info.
/// </summary>
public struct ShaderFeatureInfo : IValid
{
	public string FeatureName { get; private set; }
	public string FeatureDeclaration { get; private set; }
	public int OptionsCount { get; private set; }
	public bool IsDynamicCombo { get; private set; }

	public bool IsValid => string.IsNullOrWhiteSpace( FeatureName );

	/// <summary>
	/// Name of the result varible in the MainVs or MainPs functions.
	/// </summary>
	public readonly string FeatureResultString
	{
		get
		{
			return $"{FeatureName}SwitchResult";
		}
	}

	/// <summary>
	/// feature string which is F_FeatureName. With `FeatureName` all uppercase.
	/// </summary>
	public readonly string FeatureString
	{
		get
		{
			return $"F_{FeatureName.ToUpper()}";
		}
	}

	/// <summary>
	/// Combo string. Either S_FeatureName or D_FeatureName.
	/// </summary>
	public readonly string ComboString
	{
		get
		{
			if ( !IsDynamicCombo )
			{
				return $"S_{FeatureName.ToUpper()}";
			}
			else
			{
				return $"D_{FeatureName.ToUpper()}";
			}
		}
	}

	/// <summary>
	/// Type of combo when declared in either the Vertex or Pixel shader stages.
	/// </summary>
	public readonly string ComboTypeString
	{
		get
		{
			if ( !IsDynamicCombo )
			{
				return $"StaticCombo";
			}
			else
			{
				return $"DynamicCombo";
			}
		}
	}

	public ShaderFeatureInfo( string featureName, string featureDeclaration, int optionsCount, bool isDynamicCombo )
	{
		FeatureName = featureName;
		FeatureDeclaration = featureDeclaration;
		OptionsCount = optionsCount;
		IsDynamicCombo = isDynamicCombo;
	}

	public ShaderFeatureInfo()
	{
		FeatureName = "";
		FeatureDeclaration = "";
		OptionsCount = 0;
		IsDynamicCombo = false;
	}
}
