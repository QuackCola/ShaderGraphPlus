namespace Editor.ShaderGraphPlus;


/// <summary>
/// Container for the generated shader feature info.
/// </summary>
public struct ShaderFeatureInfo : IValid
{
	public string FeatureName;
	public string FeatureDeclaration;
	public string FeatureBody;
	public int OptionsCount;
	public bool IsDynamicCombo;

	public bool IsValid => string.IsNullOrWhiteSpace( FeatureName );

	/// <summary>
	/// Name of the result varible in the MainVs or MainPs functions.
	/// </summary>
	public readonly string FeatureResultString
	{
		get
		{
			return $"{FeatureName}_result";
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
}

/// <summary>
///	
/// </summary>
public struct ShaderFeature : IValid
{
	public string FeatureName { get; set; }

	/// <summary>
	/// Header Name of this Feature that shows up in the Material Editor.
	/// </summary>
	public string HeaderName { get; set; }

	/// <summary>
	/// String list of feature options.
	/// </summary>
	[Hide]
	public List<string> Options { get; set; }

	[Hide]
	public bool IsDynamicCombo { get; set; }

    [Hide]
    public bool Preview { get; set; }

    [Hide]
	public readonly bool IsValid
	{
		get
		{
			if ( string.IsNullOrWhiteSpace( FeatureName ) ) //|| !Options.Any() )
			{
				return false;
			}
			else
			{
				return true;
			}
		}
	}


	/// <summary>
	/// Returns true if all options have valid names. False if one option has an invalid name.
	/// </summary>
	//[Hide]
	//public readonly bool IsOptionsValid
	//{
	//	get
	//	{
	//		var optionsCount = Options.Count;
	//		var validOptionsCount = 0;
	//
	//		foreach ( var option in Options )
	//		{
	//			if ( !string.IsNullOrWhiteSpace( option ) )
	//			{
	//				validOptionsCount++;
	//			}
	//		}
	//
	//		if ( validOptionsCount == optionsCount )
	//		{
	//			return true;
	//		}
	//		else
	//		{
	//			return false;
	//		}
	//	}
	//}
}
