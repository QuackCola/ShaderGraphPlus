// Static Switch related code
//

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Editor.ShaderGraphPlus;

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

	public IEnumerable<string> RegisterdFeatureNames => ShaderResult.ShaderFeatures.Keys;

	private partial class CompileResult
	{
		public Dictionary<string, string> StaticComboSwitches { get; private set; } = new();
	}

	private (NodeResult, NodeResult) BinaryComboSwitchResult( NodeInput a, NodeInput b, float defaultA = 0.0f, float defaultB = 1.0f )
	{
		var resultA = ResultOrDefault( a, defaultA );
		var resultB = ResultOrDefault( b, defaultB );
		
		ResetCurrentComboSwitchInfo();
		
		if ( resultA.Components() == resultB.Components() )
		{
			//SGPLog.Info( "Not casing" );
			return (resultA, resultB);
		}

		if ( resultA.Components() < resultB.Components() )
		{
			//SGPLog.Info("Casting A to B compinents.");
			return (new NodeResult( resultB.ResultType, resultA.Cast( resultB.Components() ) ), resultB);
		}

		//SGPLog.Info( "Casting B to A compinents." );
		return ( resultA, new NodeResult( resultA.ResultType, resultB.Cast( resultA.Components() ) ) );
	}

	private void ResetCurrentComboSwitchInfo()
	{
		CurrentComboSwitchInfo = default;
	}

	private void AddGraphFeatureReferenceToGraph( string name )
	{
		if ( !Graph.FeatureNames.Contains( name ) )
		{
			Graph.FeatureNames.Add( name );
			
			return;
		}

		SGPLog.Info( "NameReference already known by the graph." );
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
				feature.FeatureName,
				feature.FeatureName.Replace( " ", "_" ),
				$"Feature( F_{feature.FeatureName.ToUpper()}, 0..1, \"{feature.HeaderName}\" );",
				featureOptionAmount,
				feature.IsDynamicCombo
			);

			if ( !result.ShaderFeatures.ContainsKey( feature.FeatureName ) )
			{
				result.ShaderFeatures.Add( feature.FeatureName, shaderFeatureInfo );
				AddGraphFeatureReferenceToGraph( feature.FeatureName );


				return true;
			}

			return false;
		}
		else
		{
			return false;
		}
	}

	private void ConstructSwitchBlock( out StringBuilder sb, IEnumerable<(NodeResult, NodeResult)> shaderResults, string blockResultName, int blockResultComponentCount, bool debug = false )
	{
		var lastResult = (new NodeResult(), new NodeResult());
		var indentLevel = 1;
		sb = new StringBuilder();

		for ( int i = 0; i < shaderResults.Count() + 1; i++ )
		{
			if ( i < shaderResults.Count() )
			{
				SGPLog.Info( $"At shaderResults index `{i}`", IsNotPreview && debug );
				var result = shaderResults.ElementAt( i );
				
				if ( !string.IsNullOrWhiteSpace( result.Item1.ComboSwitchBody ) )
				{
					sb.AppendLine( IndentString( $"{result.Item1.ComboSwitchBody}", indentLevel ) );
				}

				SGPLog.Info( $"`{blockResultName} = {lastResult.Item1.Cast( blockResultComponentCount )}`", IsNotPreview );
				sb.AppendLine( IndentString( $"{result.Item2.TypeName} {result.Item1} = {result.Item2.Code}; {(debug ? $"// index `{i}`" : "")}", indentLevel ) );

				lastResult = result;
			}
			else
			{
				SGPLog.Info( $"At last shaderResults index `{i}`", IsNotPreview && debug );

				if ( lastResult.Item2.Components() == blockResultComponentCount )
				{
					sb.AppendLine( IndentString( $"{blockResultName} = {lastResult.Item1}; {(debug ? $"// result" : "")}", indentLevel ) );
				}
				else
				{
					sb.AppendLine( IndentString( $"{blockResultName} = {lastResult.Item1.Cast( blockResultComponentCount )}; {(debug ? $"// result" : "")}", indentLevel ) );
				}
			}
		}
	}

	private StringBuilder AppendSwitchBlock( StringBuilder sb, string body )
	{
		sb.AppendLine( "{" );
		sb.AppendLine( body );
		sb.AppendLine( "}" );

		return sb;
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

		//ResetCurrentComboSwitchInfo();

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

		SGPLog.Info( $"There is a total of `{shaderResultsTrue.Count()}` true block shader results", IsNotPreview && ConCommands.VerboseDebgging );

		ConstructSwitchBlock( out sbTrueBody, shaderResultsTrue, resultNameInternal, nodeResultComponentCount, true );

		SGPLog.Info( $"There is a total of `{shaderResultsFalse.Count()}` false block shader results", IsNotPreview && ConCommands.VerboseDebgging );

		ConstructSwitchBlock( out sbFalseBody, shaderResultsFalse, resultNameInternal, nodeResultComponentCount, true );

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

		sbSwitchBody = AppendSwitchBlock( sbSwitchBody, sbTrueBody.ToString() );
		sbSwitchBody.AppendLine( "#else" );
		sbSwitchBody = AppendSwitchBlock( sbSwitchBody,  sbFalseBody.ToString() );
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
