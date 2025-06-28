// Static Switch related code
//

using System.Text;

namespace Editor.ShaderGraphPlus;

public struct StaticSwitchData
{
	public StaticSwitchState State;
}

public enum StaticSwitchBlock
{
	None,
	True,
	False,
}

public struct FeatureRule : IValid
{
	/// <summary>
	/// Features bound to this rule.
	/// </summary>
	[InlineEditor( Label = false )]
	public List<string> Features { get; set; }

	/// <summary>
	/// Text hint when hovering over features
	/// </summary>
	public string HoverHint { get;  set; }

	[Hide, JsonIgnore]
	public bool IsValid
	{
		get
		{
			if ( Features.Any() )
			{
				foreach ( var feature in Features )
				{
					if ( string.IsNullOrWhiteSpace( feature ) )
					{
						return false;
					}
					else
					{
						continue;
					}
				}

				return true;
			}
			else
			{
				return false;
			}
		}
	}

	public FeatureRule()
	{
		Features = new();
		HoverHint = string.Empty;
	}
}

public sealed partial class GraphCompiler
{
	/// <summary>
	/// Container for every result to let things further down the line what belongs to what.
	/// </summary>
	public struct ComboSwitchInfo : IValid
	{
		/// <summary>
		/// Name of the Switch that this code belongs to.
		/// </summary>
		public string BoundSwitch;

		/// <summary>
		/// What block of the switch this code belongs to. example : true or false blocks.
		/// </summary>
		public StaticSwitchBlock BoundSwitchBlock;

		public bool IsValid => !string.IsNullOrWhiteSpace( BoundSwitch );

		public override string ToString()
		{
			return $"Bound Switch : `{BoundSwitch}` Bound Switch Block `{BoundSwitchBlock}`";
		}

		public static bool operator ==( ComboSwitchInfo a, ComboSwitchInfo b ) => a.BoundSwitch == b.BoundSwitch && a.BoundSwitchBlock == b.BoundSwitchBlock;
		public static bool operator !=( ComboSwitchInfo a, ComboSwitchInfo b ) => a.BoundSwitch != b.BoundSwitch && a.BoundSwitchBlock != b.BoundSwitchBlock;
		public override bool Equals( object obj ) => obj is ComboSwitchInfo switchInfo && this == switchInfo;
		public override int GetHashCode() => System.HashCode.Combine( BoundSwitch, BoundSwitchBlock );
	}

	//private StaticSwitchNode StaticSwitchNode = null;
	//private List<StaticSwitchNode> StaticSwitchStack = new();
	/// <summary>
	/// 
	/// </summary>
	private List<ComboSwitchInfo> ComboSwitchInfoStack = new();

	/// <summary>
	/// Currently active SwitchInfo data.
	/// </summary>
	private ComboSwitchInfo CurrentComboSwitchInfo { get; set; } = default;

	public IEnumerable<string> RegisterdFeatureNames => ShaderResult.StaticComboSwitches.Keys;

	private partial class CompileResult
	{
		public Dictionary<string, string> StaticComboSwitches { get; private set; } = new();
	}

	internal (NodeResult, NodeResult) BinaryComboSwitchResult( NodeInput a, NodeInput b, float defaultA = 0.0f, float defaultB = 1.0f )
	{
		var resultA = ResultOrDefault( a, defaultA );
		var resultB = ResultOrDefault( b, defaultB );

		if ( resultA.Components() == resultB.Components() )
			return (resultA, resultB);

		if ( resultA.Components() < resultB.Components() )
			return ( new NodeResult( resultB.ResultType, resultA.Cast( resultB.Components() ) ), resultB );

		return ( resultA, new NodeResult( resultA.ResultType, resultB.Cast( resultA.Components() ) ) );
	}

	// TODO : 
	internal void ResetCurrentComboSwitchInfo()
	{
		CurrentComboSwitchInfo = default;
	}

	/// <summary>
	/// Registers a true or false Shader Feature.
	/// </summary>
	internal bool RegisterShaderFeatureBinary( ShaderFeature feature , out ShaderFeatureInfo shaderFeatureInfo )
	{
		var result = ShaderResult;
		shaderFeatureInfo = new ShaderFeatureInfo();

		if ( feature.IsValid )
		{
			var featureOptionAmount = 2;

			shaderFeatureInfo = new ShaderFeatureInfo
			(
				feature.FeatureName.Replace( " ", "_" ),
				$"Feature(F_{feature.FeatureName.ToUpper()}, 0..1, \"{feature.HeaderName}\");",
				featureOptionAmount,
				feature.IsDynamicCombo
			);

			var id = shaderFeatureInfo.FeatureName;

			if ( !result.ShaderFeatures.ContainsKey( id ) )
			{
				result.ShaderFeatures.Add( id, shaderFeatureInfo );

				return true;
			}

			return false;
		}
		else
		{
			return false;
		}
	}

	internal bool GenerateComboSwitch
	(
		ShaderFeatureInfo feature,
		NodeInput inputTrue, 
		NodeInput inputFalse, 
		bool previewToggle, 
		out string switchResultVariableNameOut, 
		out string switchBodyOut, 
		out ResultType switchResultTypeOut 
	)
	{
		var resultNameInternal = feature.FeatureResultString;
		switchResultVariableNameOut = resultNameInternal;
		switchBodyOut = "";

		inputTrue.ComboSwitchInfo = new ComboSwitchInfo() { BoundSwitch = resultNameInternal, BoundSwitchBlock = StaticSwitchBlock.True };
		inputFalse.ComboSwitchInfo = new ComboSwitchInfo() { BoundSwitch = resultNameInternal, BoundSwitchBlock = StaticSwitchBlock.False };

		var results = BinaryComboSwitchResult( inputTrue, inputFalse, 0.0f, 0.0f );
		switchResultTypeOut = results.Item1.ResultType;

		ResetCurrentComboSwitchInfo();

		string nodeResultTypeName = results.Item1.TypeName;
		int nodeResultComponentCount = results.Item1.Components();

		var sbTrueBody = new StringBuilder();
		var sbFalseBody = new StringBuilder();
		var sbSwitchBody = new StringBuilder();

		// make sure our results go into the correct switch and the correct block. TODO : Support more than just true or false switches.
		var shaderResultsTrue = ShaderResult.Results.Where( 
			x => x.Item2.SwitchInfo.BoundSwitch == resultNameInternal
			&& x.Item2.SwitchInfo.BoundSwitchBlock == StaticSwitchBlock.True
		);

		var shaderResultsFalse = ShaderResult.Results.Where( 
			x => x.Item2.SwitchInfo.BoundSwitch == resultNameInternal
			&& x.Item2.SwitchInfo.BoundSwitchBlock == StaticSwitchBlock.False
		);

		var index = 1;
		var indentLevel = 1;
		foreach ( var resultTrue in shaderResultsTrue )
		{
			if ( !string.IsNullOrWhiteSpace( resultTrue.Item1.ComboSwitchBody ) )
			{
				sbTrueBody.AppendLine( IndentString( $"{resultTrue.Item1.ComboSwitchBody}", indentLevel ) );
			}

			//sbTrueBody.AppendLine( IndentString( $"{resultTrue.Item2.TypeName} {resultTrue.Item1} = {resultTrue.Item2.Code};", indentLevel ) );

			if ( index == shaderResultsTrue.Count() )
			{
				if ( resultTrue.Item2.Components() == nodeResultComponentCount )
				{
					sbTrueBody.AppendLine( IndentString( $"{resultNameInternal} = {resultTrue.Item2.Code};", indentLevel ) );
				}
				else
				{
					sbTrueBody.Append( IndentString( $"{resultNameInternal} = {resultTrue.Item1.Cast( nodeResultComponentCount )};", indentLevel ) );
				}
			}
			else
			{
				sbTrueBody.AppendLine( IndentString( $"{resultTrue.Item2.TypeName} {resultTrue.Item1} = {resultTrue.Item2.Code};", indentLevel ) );
			}

			index++;
		}

		index = 1;
		var lastResultCode = "";
		var lastResultLocal = "";
		foreach ( var resultFalse in shaderResultsFalse )
		{
			if ( !string.IsNullOrWhiteSpace( resultFalse.Item1.ComboSwitchBody ) )
			{
				sbFalseBody.AppendLine( IndentString( $"{resultFalse.Item1.ComboSwitchBody}", indentLevel ) );
			}

			sbFalseBody.AppendLine( IndentString( $"{resultFalse.Item2.TypeName} {resultFalse.Item1} = {resultFalse.Item2.Code};", indentLevel ) );

			// Little hack to avoid duplicate code.
			lastResultCode = resultFalse.Item2.Code;
			lastResultLocal = resultFalse.Item1.Code;

			if ( index == shaderResultsFalse.Count() )
			{
				if ( resultFalse.Item2.Components() == nodeResultComponentCount )
				{
					//sbFalseBody.AppendLine( IndentString( $"{resultNameInternal} = {resultFalse.Item2.Code};", indentLevel ) );
					sbFalseBody.AppendLine( IndentString( $"{resultNameInternal} = {lastResultLocal};", indentLevel ) );
				}
				else
				{
					sbFalseBody.Append( IndentString( $"{resultNameInternal} = {resultFalse.Item1.Cast( nodeResultComponentCount )};", indentLevel ) );
				}
			}

			index++;
		}

		sbSwitchBody.AppendLine();
		sbSwitchBody.AppendLine( $"{nodeResultTypeName} {resultNameInternal};" );

		if ( IsPreview )
		{
			sbSwitchBody.AppendLine( $"#if ( {feature.ComboString} == {(previewToggle ? "0" : "1")} )" );
		}
		else
		{
			sbSwitchBody.AppendLine( $"#if ( {feature.ComboString} == 1 )" );
		}

		sbSwitchBody.AppendLine( "{" );
		sbSwitchBody.Append( sbTrueBody.ToString() );
		sbSwitchBody.AppendLine( "}" );
		sbSwitchBody.AppendLine( "#else" );
		sbSwitchBody.AppendLine( "{" );
		sbSwitchBody.Append( sbFalseBody.ToString() );
		sbSwitchBody.AppendLine( "}" );
		sbSwitchBody.AppendLine( "#endif" );

		if ( !ShaderResult.StaticComboSwitches.ContainsKey( feature.FeatureString ) )
		{
			ShaderResult.StaticComboSwitches.Add( feature.FeatureString, sbSwitchBody.ToString() );
			switchBodyOut = sbSwitchBody.ToString();

			//Graph.AddFeature( feature );

			SGPLog.Info( $"StaticSwitch `{resultNameInternal}` generated body : \n{switchBodyOut}", ConCommands.VerboseDebgging );

			return true;
		}

		//SGPLog.Info( $"ShaderResult.StaticSwitches already contains key : `{resultName}`", IsPreview );

		return false;
	}

	internal StringBuilder AppendIf( StringBuilder sb, string comboName, string previewToggle )
	{
		if ( IsPreview )
		{
			sb.AppendLine( $"#if ( {comboName} == {previewToggle} )" );
		}
		else
		{
			sb.AppendLine( $"#if ( {comboName} == 1 )" );
		}

		return sb;
	}

	// TODO : Once i decide to support more than a single bool option in a feature. give this a lookover.
	/*
	/// <summary>
	/// Register and Build a Shader Feature.
	/// </summary>
	/// 
	internal void RegisterShaderFeature(ShaderFeature feature, string falseResult, string trueResult, bool previewToggle)
	{
		var result = ShaderResult;
		var sfinfo = new ShaderFeatureInfo();

		var feature_body = new StringBuilder();

		//Log.Info($"Shader feature : {feature.ToFeatureName()} result true : {trueResult}");
		//Log.Info($"Shader feature : {feature.ToFeatureName()} defualt result : {falseResult}");

		if ( feature.IsValid )
		{
			var featureDeclaration = "";
			var featureDeclarationOptionAmount = 0;

			feature_body.AppendLine();
			feature_body.AppendLine($"#if S_{feature.FeatureName.ToUpper()}" + " == {0} ");
			feature_body.AppendLine($"{falseResult} = {trueResult};");
			feature_body.AppendLine("#endif");

			if (feature.IsDynamicCombo is not true)
			{
				var featureDeclarationName = $"F_{feature.FeatureName.ToUpper()}";
				var featureDeclarationHeaderName = feature.HeaderName;
				featureDeclarationOptionAmount = feature.Options.Count - 1;
				var featureDeclarationOptions = BuildFeatureOptions(feature.Options);

				if (!string.IsNullOrWhiteSpace(featureDeclarationOptions))
				{
					featureDeclaration = $"Feature({featureDeclarationName}, 0..{featureDeclarationOptionAmount}({featureDeclarationOptions}), \"{featureDeclarationHeaderName}\");";
					//if ( DebugSpew )
					{
						// Log.Info($"Generated Static Combo Feature : {_feature}.");
					}
				
				}
			}
			else
			{
				Log.Info($"Generated Dynamic Combo Feature.");
			}

			sfinfo.FeatureName = feature.FeatureName.Replace(" ", "_");
			sfinfo.FeatureDeclaration = featureDeclaration;
			sfinfo.FeatureBody = feature_body.ToString();
			sfinfo.OptionsCount = featureDeclarationOptionAmount;
			sfinfo.TrueResult = trueResult;
			sfinfo.FalseResult = falseResult;
			sfinfo.IsDynamicCombo = feature.IsDynamicCombo;

			var id = sfinfo.FeatureName;

			if ( !result.ShaderFeatures.ContainsKey( id ) )
			{
				result.ShaderFeatures.Add( id, (sfinfo, previewToggle));
			}

		}
		else
		{
			Log.Warning("invalid feature!");
		}

	}
	*/
	internal string BuildFeatureOptionsBody( List<string> options )
	{
		var options_body = "";
		int count = 0;

		foreach ( var option in options )
		{
			if ( count == 0 ) // first option starts at 0 :)
			{
				options_body += $"0=\"{option}\",";
				count++;
			}
			else if ( count != (options.Count - 1) )  // These options dont get the privilege of being the first >:)
			{
				options_body += $"{count}=\"{option}\",";
				count++;
			}
			else // Last option in the list oh well...:(
			{
				options_body += $"{count}=\"{option}\"";
			}
		}

		//Log.Info($"Feature Options Count is : {options.Count}");
		//Log.Info($"Feature Option Option String : {options_body}");

		return options_body;
	}
}
