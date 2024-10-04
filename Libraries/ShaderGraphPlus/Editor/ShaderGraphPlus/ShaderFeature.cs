namespace Editor.ShaderGraphPlus;

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
			if ( string.IsNullOrWhiteSpace( FeatureName ) || !Options.Any() )
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
	[Hide]
	public readonly bool IsOptionsValid
	{
		get
		{
			var optionsCount = Options.Count;
			var validOptionsCount = 0;

			foreach ( var option in Options )
			{
				if ( !string.IsNullOrWhiteSpace( option ) )
				{
					validOptionsCount++;
				}
			}

			if ( validOptionsCount == optionsCount )
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}


	public void SetPreview( bool val)
	{ Preview = val; }

	public readonly string ToFeatureName()
	{
		return $"F_{FeatureName.ToUpper()}";
	}

	public readonly string ToStaticComboString()
	{
		return $"S_{FeatureName.ToUpper()}";
	}

	public readonly string ToDynamicComboString()
	{
		return $"D_{FeatureName.ToUpper()}";
	}
}
