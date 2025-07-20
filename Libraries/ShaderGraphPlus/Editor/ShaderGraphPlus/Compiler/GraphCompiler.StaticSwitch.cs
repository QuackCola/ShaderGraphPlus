// Static Switch related code
//

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
			return $"BoundSwitch:`{BoundSwitch}`,BoundBlock`{BoundSwitchBlock}`";
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
	public ComboSwitchInfo CurrentComboSwitchInfo { get; set; } = default;
	private bool IsInComboSwitch => CurrentComboSwitchInfo.IsValid;
	//public IEnumerable<string> RegisterdFeatureNames => ShaderFeatures.Keys;

	/// <summary>
	/// Registerd ShaderFeatures of the current project.
	/// </summary>
	public Dictionary<string, ShaderFeatureInfo> ShaderFeatures = new();

	private partial class CompileResult
	{

		public List<(NodeResult, NodeResult)> SwitchBlockResults = new();
		public Dictionary<NodeInput, NodeResult> SwitchBlockInputResults = new();
		public Dictionary<string, TextureInput> SwitchTextureInputs = new();
		public Dictionary<string, string> StaticComboSwitches { get; private set; } = new();
	}

	private ( NodeResult TrueResult, NodeResult FalseResult ) BinaryComboSwitchResult( NodeInput a, NodeInput b, float defaultA = 0.0f, float defaultB = 1.0f )
	{
		var resultA = ResultOrDefault( a, defaultA );

		ShaderResult.SwitchBlockInputResults.Clear();
		ShaderResult.SwitchBlockResults.Clear();

		var resultB = ResultOrDefault( b, defaultB );

		//if ( resultA.Constant )
		//{
		//	SGPLog.Info( $"True Result is constant of value {resultA.Code}", IsNotPreview );
		//}
		//if ( resultB.Constant )
		//{
		//	SGPLog.Info( $"False Result is constant of value {resultB.Code}", IsNotPreview );
		//}

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

	private void ConstructSwitchBlock(
		out StringBuilder sb,
		IEnumerable<(NodeResult, NodeResult)> shaderResults,
		NodeResult constantResult,
		string blockResultName,
		int blockResultComponentCount,
		bool debug = false,
		string blockName = ""
	)
	{
		var indentLevel = 1;
		sb = new StringBuilder();

		GenerateLocalResults( ref sb, shaderResults, out var lastResult, false, true, indentLevel );

		//SGPLog.Info( $"test : {lastResult.Item1}" , IsNotPreview);

		if ( !lastResult.Item1.IsValid )
		{
			// Handle the situation where the result of switch block is constant.
			if ( constantResult.IsValid )
			{
				if ( constantResult.Components() == blockResultComponentCount )
				{
					sb.AppendLine( IndentString( $"{blockResultName} = {constantResult}; {(debug ? $"// result" : "")}", indentLevel ) );
				}
				else
				{
					sb.AppendLine( IndentString( $"{blockResultName} = {constantResult.Cast( blockResultComponentCount )}; {(debug ? $"// result" : "")}", indentLevel ) );
				}
			}
			else
			{
				sb.AppendLine( IndentString( $"// No valid results", indentLevel ) );
			}
			//SGPLog.Info( $"lastResult of `{blockName}` is not valid!", IsNotPreview );
		}
		else
		{
			if ( lastResult.Item1.Components() == blockResultComponentCount )
			{
				sb.AppendLine( IndentString( $"{blockResultName} = {lastResult.Item1}; {(debug ? $"// result" : "")}", indentLevel ) );
			}
			else
			{
				//SGPLog.Info( $"lastResult type : {lastResult.Item1.ResultType}", IsNotPreview );

				sb.AppendLine( IndentString( $"{blockResultName} = {lastResult.Item1.Cast( blockResultComponentCount )}; {(debug ? $"// result" : "")}", indentLevel ) );
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

	// Gets results from the two provided NodeInputs.
	internal bool GenerateComboSwitch
	(
		ShaderFeatureInfo feature,
		NodeInput inputTrue,
		NodeInput inputFalse,
		bool previewToggle,
		bool isReference,
		out string switchResultVariableNameOut,
		out string switchBodyOut,
		out ResultType switchResultTypeOut
	)
	{
		var resultNameInternal = isReference ? $"{feature.UserDefinedName.Replace( " ", "_" )}SwitchResult_Ref{feature.ReferenceCount}" : $"{feature.UserDefinedName.Replace( " ", "_" )}SwitchResult";

		// If we are a reference. Make sure to increment the reference count then update the pre-existing dictionary key.
		if ( isReference )
		{
			feature.ReferenceCount++;
			ShaderFeatures[feature.UserDefinedName] = feature;
		}

		switchResultVariableNameOut = resultNameInternal;
		switchBodyOut = "";

		inputTrue.ComboSwitchInfo = new ComboSwitchInfo() { BoundSwitch = resultNameInternal, BoundSwitchBlock = StaticSwitchBlock.True };
		inputFalse.ComboSwitchInfo = new ComboSwitchInfo() { BoundSwitch = resultNameInternal, BoundSwitchBlock = StaticSwitchBlock.False };

		var results = BinaryComboSwitchResult( inputTrue, inputFalse, 0.0f, 0.0f );
		switchResultTypeOut = results.Item1.ResultType;

		//foreach ( var result in ShaderResult.TestResults )
		//{
		//	SGPLog.Info( $"result entry : `{result.Item2.Code}`", IsNotPreview );
		//}

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

		ConstructSwitchBlock( out sbTrueBody, shaderResultsTrue, results.TrueResult, resultNameInternal, nodeResultComponentCount, true, "True Block" );

		SGPLog.Info( $"There is a total of `{shaderResultsFalse.Count()}` false block shader results", IsNotPreview && ConCommands.VerboseDebgging );

		ConstructSwitchBlock( out sbFalseBody, shaderResultsFalse, results.FalseResult, resultNameInternal, nodeResultComponentCount, true, "False Block" );

		sbSwitchBody.AppendLine();
		sbSwitchBody.AppendLine( $"{nodeResultTypeName} {resultNameInternal};" );

		// TODO : Work out combos with more than just true or false.
		if ( IsPreview )
		{
			var comboString = feature.CreateCombo( feature.UserDefinedName, true );

			ResultComboPreview( comboString, previewToggle );

			sbSwitchBody.AppendLine( $"#if ( {comboString} )" );
		}
		else
		{
			sbSwitchBody.AppendLine( $"#if ( {feature.CreateCombo( feature.UserDefinedName )} == SWITCH_TRUE )" );
		}

		sbSwitchBody = AppendSwitchBlock( sbSwitchBody, sbTrueBody.ToString() );
		sbSwitchBody.AppendLine( "#else" );
		sbSwitchBody = AppendSwitchBlock( sbSwitchBody, sbFalseBody.ToString() );
		sbSwitchBody.AppendLine( "#endif" );

		//SGPLog.Info( $"StaticSwitch `{resultNameInternal}` generated body : \n{sbSwitchBody.ToString()}", IsNotPreview  );
		var featureString = $"{feature.CreateFeature}";
		
		if ( isReference )
		{
			switchBodyOut = sbSwitchBody.ToString();

			return true;
		}

		if ( !ShaderResult.StaticComboSwitches.ContainsKey( featureString ) )
		{
			ShaderResult.StaticComboSwitches.Add( featureString, sbSwitchBody.ToString() );
			switchBodyOut = sbSwitchBody.ToString();

			return true;
		}

		//SGPLog.Info( $"ShaderResult.StaticSwitches already contains key : `{resultName}`", IsPreview );

		return false;
	}

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
