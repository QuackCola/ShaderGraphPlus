// Static Switch related code
//

using Editor.ShaderGraph;
using Sandbox;
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

	private (NodeResult, NodeResult) CheckResults( NodeResult resultA, NodeResult resultB )
	{

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
		return (resultA, new NodeResult( resultA.ResultType, resultB.Cast( resultA.Components() ) ));
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

	private void ConstructSwitchBlock( out StringBuilder sb, IEnumerable<(NodeResult, NodeResult)> shaderResults, string blockResultName, int blockResultComponentCount, bool debug = false, string blockName = "" )
	{
		var lastResult = (new NodeResult(), new NodeResult());
		var indentLevel = 1;
		sb = new StringBuilder();

		for ( int i = 0; i < shaderResults.Count() + 1; i++ )
		{
			if ( i < shaderResults.Count() )
			{
				var debugString = "";
				if ( i + 1 == shaderResults.Count() )
				{
					debugString = "last ";
					SGPLog.Info( $"`{blockName}` At shaderResults last index `{i}`", IsNotPreview && debug );
				}
				else
				{
					if ( i == 0 )
						debugString = "start ";

					SGPLog.Info( $"`{blockName}` At shaderResults {debugString} index `{i}`", IsNotPreview && debug );
				}

				var result = shaderResults.ElementAt( i );

				if ( !string.IsNullOrWhiteSpace( result.Item1.ComboSwitchBody ) )
				{
					sb.AppendLine( IndentString( $"{result.Item1.ComboSwitchBody}", indentLevel ) );
				}

				sb.AppendLine( IndentString( $"{result.Item2.TypeName} {result.Item1} = {result.Item2.Code}; {(debug ? $"// {debugString}index `{i}`" : "")}", indentLevel ) );

				lastResult = result;
			}
			else
			{
				SGPLog.Info( $"`{blockName}` At result assingment", IsNotPreview && debug );

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

	void AddDatatoParent( GraphCompiler compiler, CompileResult shaderResult )
	{

		// Let the main compiler instance know about any added parameters
		if ( compiler.ShaderResult.Parameters.Any() )
		{
			foreach ( var parameter in compiler.ShaderResult.Parameters )
			{
				if ( ShaderResult.Parameters.TryAdd( parameter.Key, parameter.Value ) )
				{
					SGPLog.Info( $"Added `{parameter.Value.Result.Code}` to main graph.", IsNotPreview );
				}
				else
				{
					SGPLog.Info( $"Could not add `{parameter.Value.Result.Code}` to main graph...", IsNotPreview );
				}

				//SGPLog.Info( $"True Block Param : `{parameter.Key}` `{parameter.Value.Result.Code}`", IsNotPreview  );
			}
		}

		// Let the main compiler instance know about any added or registerd functions
		if ( compiler.ShaderResult.Functions.Any() )
		{
			foreach ( var function in compiler.ShaderResult.Functions )
			{
				if ( !ShaderResult.Functions.Add( function ) )
				{
					SGPLog.Info( $"Could not register `{function}` to main graph...", IsNotPreview );
				}
			}
		}
	}

	// Just spin up two GraphCompiler for True and False to avoid a bunch of issues when both inputs somewhat
	// share similar inputs upstream.
	public  void SubGraphCompilerInstance( StaticSwitchNode switchNode,
		out List<(NodeResult, NodeResult)> trueBlockResults,
		out List<(NodeResult, NodeResult)> falseBlockResults
	)
	{
		trueBlockResults = new();
		falseBlockResults = new();

		foreach ( var property in GetNodeInputProperties( switchNode.GetType() ) )
		{
			if ( property.Name == nameof( switchNode.InputTrue ) )
			{
				var compiler = new GraphCompiler( _Asset, Graph, IsPreview );


				compiler.Test( compiler, switchNode, nameof( switchNode.InputTrue ), out trueBlockResults );

				AddDatatoParent( compiler, ShaderResult );
			}

			if ( property.Name == nameof( switchNode.InputFalse ) )
			{
				var compiler = new GraphCompiler( _Asset, Graph, IsPreview );
				compiler.Test( compiler, switchNode, nameof( switchNode.InputFalse ), out falseBlockResults );

				AddDatatoParent( compiler, ShaderResult );
			}
		}
	}

	public void Test( GraphCompiler compiler ,StaticSwitchNode switchNode, string propertyName,
		out List<(NodeResult, NodeResult)> blockResults
		)
	{
		compiler.Stage = Stage;
		compiler.Subgraph = null;
		compiler.SubgraphStack.Clear();

		blockResults = new List<(NodeResult, NodeResult)>();

		if ( propertyName == "InputTrue" )
		{
			NodeResult result = switchNode.GetTrueResult( compiler );
			
			if ( compiler.ShaderResult.Results.Any() )
				blockResults = compiler.ShaderResult.Results;
		}
		if ( propertyName == "InputFalse" )
		{
			NodeResult result = switchNode.GetFalseResult( compiler );

			if ( compiler.ShaderResult.Results.Any() )
				blockResults = compiler.ShaderResult.Results;
		}
	}

	// Gets results from the two provided NodeInputs.
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

		ConstructSwitchBlock( out sbTrueBody, shaderResultsTrue, resultNameInternal, nodeResultComponentCount, true, "True Block" );

		SGPLog.Info( $"There is a total of `{shaderResultsFalse.Count()}` false block shader results", IsNotPreview && ConCommands.VerboseDebgging );

		ConstructSwitchBlock( out sbFalseBody, shaderResultsFalse, resultNameInternal, nodeResultComponentCount, true, "False Block" );

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

	void ResultsLoop( List<(NodeResult, NodeResult)> results , string switchResultString ,string debugBlockName, out string block, out (NodeResult, NodeResult) lastResult, int components = -1)
	{
		var sb = new StringBuilder();
		block = "";
		lastResult = (new NodeResult(), new NodeResult());
		for ( int i = 0; i < results.Count() + 1; i++ )
		{
			if ( i < results.Count() )
			{
				var result = results.ElementAt( i );

				if ( i + 1 == results.Count() )
				{
					lastResult = result;
				}
				
					//if ( i == 0 )
					//{
					//	SGPLog.Info( $"{debugBlockName}ShaderResults start index `{i}`", IsNotPreview );
					//}
					//else
					//{
					//	SGPLog.Info( $"{debugBlockName}ShaderResults  index `{i}`", IsNotPreview );
					//}

				sb.AppendLine( IndentString( $"{result.Item2.TypeName} {result.Item1} = {result.Item2.Code};", 1 ) );
			}
			else
			{
				//SGPLog.Info( $"`{debugBlockName}` result assingment", IsNotPreview  );

				if ( components != -1 )
				{
					if ( lastResult.Item2.Components() == components )
					{
						sb.AppendLine( IndentString( $"{switchResultString} = {lastResult.Item1}; ", 1 ) );
					}
					else
					{
						sb.AppendLine( IndentString( $"{switchResultString} = {lastResult.Item1.Cast( components )};", 1 ) );
					}
				}
			}
		}

		block = sb.ToString();
	}

	// Takes in results generated by an instance of the GraphCompiler.
	internal bool GenerateComboSwitchTest
	(
		ShaderFeatureInfo feature,
		List<(NodeResult, NodeResult)> resultsTrue,
		List<(NodeResult, NodeResult)> resultsFalse, 
		bool previewToggle, 
		out string switchResultVariableNameOut, 
		out string switchBodyOut, 
		out ResultType switchResultTypeOut 
	)
	{
		var resultNameInternal = feature.FeatureResultString;
		switchResultVariableNameOut = resultNameInternal;
		switchBodyOut = "";
		switchResultTypeOut = ResultType.Invalid; // Test

		var sbTrueBody = new StringBuilder();
		var sbFalseBody = new StringBuilder();
		var sbSwitchBody = new StringBuilder();

		//SGPLog.Info( $"There is `{resultsTrue.Count()}` trueResults", IsNotPreview );

		ResultsLoop( resultsTrue, resultNameInternal, "true" , out var trueBlock, out var lastTrueResult );
		ResultsLoop( resultsFalse, resultNameInternal, "false", out var falseBlock, out var lastFalseResult, lastTrueResult.Item1.Components() );

		switchResultTypeOut = lastTrueResult.Item1.ResultType;
		string nodeResultTypeName = lastTrueResult.Item1.TypeName;

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

		sbSwitchBody = AppendSwitchBlock( sbSwitchBody, trueBlock );
		sbSwitchBody.AppendLine( "#else" );
		sbSwitchBody = AppendSwitchBlock( sbSwitchBody, falseBlock );
		sbSwitchBody.AppendLine( "#endif" );

		SGPLog.Info( $"Swithc Result : {sbSwitchBody.ToString()}", IsNotPreview );

		if ( !ShaderResult.StaticComboSwitches.ContainsKey( feature.FeatureString ) )
		{
			ShaderResult.StaticComboSwitches.Add( feature.FeatureString, sbSwitchBody.ToString() );
			switchBodyOut = sbSwitchBody.ToString();

			//Graph.AddFeature( feature );

			SGPLog.Info( $"StaticSwitch `{resultNameInternal}` generated body : \n{switchBodyOut}", ConCommands.VerboseDebgging );

			return true;
		}

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
