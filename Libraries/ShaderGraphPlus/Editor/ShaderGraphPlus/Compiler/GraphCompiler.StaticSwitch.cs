
// Static Switch related code
//

using System.Text;

namespace Editor.ShaderGraphPlus;

public sealed partial class GraphCompiler
{
	/// <summary>
	/// Current StaticSwitch
	/// </summary>
	private StaticSwitchNode StaticSwitchNode = null;
	private List<StaticSwitchNode> StaticSwitchStack = new();
	
	/// <summary>
	/// Current active switch "block" that locals are being diverted to.
	/// </summary>
	private StaticSwitchEntry CurrentStaticSwitchCodeBlock { get; set; } = StaticSwitchEntry.None;

	private partial class CompileResult
	{
		public Dictionary<string, string> StaticSwitches { get; private set; } = new();
	}

	public NodeResult ResultOrDefaultTest<T>( NodeInput input, T defaultValue, StaticSwitchEntry staticSwitchEntry )
	{
		var result = Result( input, staticSwitchEntry );
		return result.IsValid ? result : ResultValue( defaultValue );
	}

	internal (NodeResult, NodeResult) StaticSwitchResult( NodeInput a, NodeInput b, float defaultA = 0.0f, float defaultB = 1.0f, StaticSwitchEntry trueBlock = StaticSwitchEntry.None, StaticSwitchEntry falseBlock = StaticSwitchEntry.None )
	{
		var resultA = ResultOrDefaultTest( a, defaultA, trueBlock );
		var resultB = ResultOrDefaultTest( b, defaultB, falseBlock );

		resultA.BoundStaticSwtichBlock = trueBlock;
		resultB.BoundStaticSwtichBlock = falseBlock;

		if ( resultA.Components() == resultB.Components() )
			return (resultA, resultB);

		if ( resultA.Components() < resultB.Components() )
		{
			
			var resa = new NodeResult( resultB.ResultType, resultA.Cast( resultB.Components() ) );
			resa.BoundStaticSwtichBlock = resultA.BoundStaticSwtichBlock;


			return ( resa, resultB);
		}
			
		var resb = new NodeResult( resultA.ResultType, resultB.Cast( resultA.Components() ));
		resb.BoundStaticSwtichBlock = resultB.BoundStaticSwtichBlock;

	

		return (resultA, resb);
	}

	public void ResetCurrentStaticSwitchCodeBlock()
	{
		CurrentStaticSwitchCodeBlock = StaticSwitchEntry.None;
	}

	/// <summary>
	/// Registers a true or false Shader Feature.
	/// </summary>
	internal void RegisterShaderFeatureBinary( ShaderFeature feature, out string staticFeatureName )
	{
		var result = ShaderResult;
		var sfinfo = new ShaderFeatureInfo();
		staticFeatureName = "";

		if ( feature.IsValid )
		{
			var featureDeclarationName = "";
			var featureOptionAmount = 2;

			if ( feature.IsDynamicCombo is not true )
			{
				staticFeatureName = $"S_{feature.FeatureName.ToUpper()}";

				featureDeclarationName = $"Feature(F_{feature.FeatureName.ToUpper()}, 0..1, \"{feature.HeaderName}\");";
			}
			else
			{
				//Log.Info( $"Generated Dynamic Combo Feature." );
			}

			sfinfo.FeatureName = feature.FeatureName.Replace( " ", "_" );
			sfinfo.FeatureDeclaration = featureDeclarationName;
			sfinfo.OptionsCount = featureOptionAmount;
			sfinfo.IsDynamicCombo = feature.IsDynamicCombo;

			var id = sfinfo.FeatureName;

			if ( !result.ShaderFeatures.ContainsKey( id ) )
			{
				result.ShaderFeatures.Add( id, sfinfo );
			}
		}
		else
		{
			SGPLog.Error( "Invalid feature!!!" );
		}
	}

	public bool GenerateShaderFeatureBody
	( 
		string staticComboName, 
		NodeInput inputTrue, 
		NodeInput inputFalse, 
		bool previewToggle, 
		out string switchResultVariableNameOut, 
		out string switchBodyOut, 
		out ResultType switchResultTypeOut 
	)
	{
		var results = StaticSwitchResult( inputTrue, inputFalse, 0.0f, 0.0f, StaticSwitchEntry.True, StaticSwitchEntry.False );

		switchResultTypeOut = results.Item1.ResultType;

		ResetCurrentStaticSwitchCodeBlock();

		string nodeResultTypeName = results.Item1.TypeName;
		int nodeResultComponentCount = results.Item1.Components();

		var sbTrueBody = new StringBuilder();
		var sbFalseBody = new StringBuilder();
		var sbSwitchBody = new StringBuilder();

		switchBodyOut = "";

		var shaderResultsTrue = ShaderResult.Results.Where( x => x.Item2.BoundStaticSwtichBlock == StaticSwitchEntry.True );
		var shaderResultsFalse = ShaderResult.Results.Where( x => x.Item2.BoundStaticSwtichBlock == StaticSwitchEntry.False );

		var resultName = $"staticSwitch_{ShaderResult.StaticSwitches.Count}";
		var resultNameInternal = $"{resultName}_result";

		switchResultVariableNameOut = resultNameInternal;

		var index = 1;
		foreach ( var resultTrue in shaderResultsTrue )
		{
			sbTrueBody.AppendLine( IndentString( $"{resultTrue.Item2.TypeName} {resultTrue.Item1} = {resultTrue.Item2.Code};", 1 ) );

			if ( index == shaderResultsTrue.Count() )
			{
				//SGPLog.Info( $"at ✅ true index {index } a {shaderResultsTrue.Count()}" );

				if ( resultTrue.Item2.Components() == nodeResultComponentCount )
				{
					sbTrueBody.AppendLine( IndentString( $"{resultNameInternal} = {resultTrue.Item2.Code};", 1 ) );
				}
				else
				{
					sbTrueBody.Append( IndentString( $"{resultNameInternal} = {resultTrue.Item1.Cast( nodeResultComponentCount )};", 1 ) );
				}
			}

			index++;
		}

		index = 1;
		foreach ( var resultFalse in shaderResultsFalse )
		{
			sbFalseBody.AppendLine( IndentString( $"{resultFalse.Item2.TypeName} {resultFalse.Item1} = {resultFalse.Item2.Code};", 1 ) );

			if ( index == shaderResultsFalse.Count() )
			{
				//SGPLog.Info( $"at ❌ false index {index } a {shaderResultsFalse.Count()}" );
				sbFalseBody.Append( IndentString( $"{resultNameInternal} = {resultFalse.Item2.Code};", 1 ) );
			}

			index++;
		}

		sbSwitchBody.AppendLine();
		sbSwitchBody.AppendLine( $"{nodeResultTypeName} {resultNameInternal};" );

		if ( IsPreview )
		{
			//sb.AppendLine( $"#if ( {(toggle ? "true" : "false")} )" );
			sbSwitchBody.AppendLine( $"#if ( {staticComboName} == {(previewToggle ? "0" : "1")} )" );
		}
		else
		{
			sbSwitchBody.AppendLine( $"#if ( {staticComboName} == 1 )" );
		}

		sbSwitchBody.AppendLine( "{" );
		sbSwitchBody.AppendLine( sbTrueBody.ToString() );
		sbSwitchBody.AppendLine( "}" );
		sbSwitchBody.AppendLine( "#else" );
		sbSwitchBody.AppendLine( "{" );
		sbSwitchBody.AppendLine( sbFalseBody.ToString() );
		sbSwitchBody.AppendLine( "}" );
		sbSwitchBody.AppendLine( "#endif" );

		if ( !ShaderResult.StaticSwitches.ContainsKey( resultName ) )
		{
			ShaderResult.StaticSwitches.Add( resultName, sbSwitchBody.ToString() );
			switchBodyOut = sbSwitchBody.ToString();

			SGPLog.Info( $"StaticSwitch `{resultNameInternal}` generated body : \n{switchBodyOut}", ConCommands.VerboseDebgging );

			return true;
		}



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

	private string BuildFeatureOptions( List<string> options )
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
