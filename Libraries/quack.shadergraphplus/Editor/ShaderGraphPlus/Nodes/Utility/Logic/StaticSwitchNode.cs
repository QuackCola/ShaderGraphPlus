using NodeEditorPlus;
using GraphView = NodeEditorPlus.GraphView;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;
using NodeUI = NodeEditorPlus.NodeUI;

namespace ShaderGraphPlus.Nodes;

[System.AttributeUsage( AttributeTargets.Property )]
internal sealed class ShaderFeatureReferenceAttribute : Attribute
{ 
}

[System.AttributeUsage( AttributeTargets.Property )]
internal sealed class ShaderFeatureEnumPreviewIndexAttribute : Attribute
{
}

[Title( "Enum Combo Switch" ), Category( "Utility/Logic" ), Icon( "alt_route" )]
[InternalNode]
public sealed class EnumComboSwitchNode : ShaderNodePlus, IInitializeNode, IBlackboardSyncable, IErroringNode
{
	[Hide]
	public override int Version => 1;

	[Hide]
	public override string Title
	{
		get
		{
			return $"F_{Feature.Name.ToUpper().Replace( " ", "_" )}";
		}
	}

	[Hide, Browsable( false )]
	public Guid BlackboardParameterIdentifier { get; set; }

	[Hide]
	public ShaderFeatureEnum Feature { get; set; } = new();

	[ShaderFeatureEnumPreviewIndex]
	[Title( "Preview" )]
	public int PreviewIndex { get; set; } = 0;

	[Hide]
	private List<IPlugIn> InternalInputs = new();

	[Hide]
	public override IEnumerable<IPlugIn> Inputs => InternalInputs;

	[Hide, JsonIgnore]
	int _lastHashCodeInputs = 0;
	
	[Hide, JsonIgnore]
	bool _hasFeatureError = false;

	public override void OnFrame()
	{
		var hashCodeInput = Feature.GetHashCode();
		if ( hashCodeInput != _lastHashCodeInputs )
		{
			//var oldHashCode = _lastHashCodeInputs;
			_lastHashCodeInputs = hashCodeInput;

			//SGPLog.Info( $"HashCode changed from : {oldHashCode} to {_lastHashCodeInputs}" );

			if ( !_hasFeatureError )
			{
				CreateInputs();
				Update();
			}
		}
	}

	[Output, Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var inputs = new List<NodeInput>();

		foreach ( var input in Inputs )
		{
			if ( input.ConnectedOutput is null )
			{
				NodeInput nodeInput = default;

				inputs.Add( nodeInput );
			}
			else
			{
				NodeInput nodeInput = new NodeInput { Identifier = input.ConnectedOutput.Node.Identifier, Output = input.ConnectedOutput.Identifier };

				inputs.Add( nodeInput );
			}
		}

		return compiler.ResultFeatureSwitch( inputs, Feature, PreviewIndex );
	};

	public void InitializeNode()
	{
		OnNodeCreated();
	}

	private void OnNodeCreated()
	{
		CreateInputs();
		Update();
	}

	public void CreateInputs()
	{
		var inPlugs = new List<IPlugIn>();

		if ( Feature.Options == null )
		{
			InternalInputs = new();
		}
		else
		{
			foreach ( var input in Feature.Options )
			{
				var inputName = input;
				// Default to float.
				var inputType = typeof( float );//typeof( object );

				if ( string.IsNullOrWhiteSpace( inputName ) ) continue;

				var info = new PlugInfo()
				{
					Name = inputName,
					Type = inputType,
					DisplayInfo = new DisplayInfo()
					{
						Name = inputName,
						Fullname = inputType.FullName
					}
				};

				var plug = new BasePlugIn( this, info, inputType );
				var oldPlug = InternalInputs.FirstOrDefault( x => x is BasePlugIn plugIn && plugIn.Info.Name == info.Name && plugIn.Info.Type == info.Type ) as BasePlugIn;
				if ( oldPlug is not null )
				{
					oldPlug.Info.Name = info.Name;
					oldPlug.Info.Type = info.Type;
					oldPlug.Info.DisplayInfo = info.DisplayInfo;
					plug = oldPlug;
				}

				inPlugs.Add( plug );
			}

			InternalInputs = inPlugs;
		}
	}

	public void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is ShaderFeatureEnumParameter enumFeatureParam )
		{
			if ( enumFeatureParam.IsValid )
			{
				Feature = new ShaderFeatureEnum
				{
					Name = enumFeatureParam.Name,
					Description = enumFeatureParam.Description,
					HeaderName = enumFeatureParam.HeaderName,
					Options = enumFeatureParam.Options,
				};

				_hasFeatureError = false;
			}
			else
			{
				_hasFeatureError = true;
			}

		}
	}

	public List<string> GetErrors()
	{
		var errors = new List<string>();

		//foreach ( var option in Feature.Options )
		//{
		//	if ( string.IsNullOrWhiteSpace( option ) )
		//	{
		//		errors.Add( $"element \"{Feature.Options.IndexOf( option )}\" of feature \"{Feature.Name}\" cannot have a blank name!" );
		//	}
		//}

		return errors;
	}
}

[Title( "Boolean Combo Switch" ), Category( "Utility/Logic" ), Icon( "alt_route" )]
[InternalNode]
public sealed class BooleanComboSwitchNode : ShaderNodePlus, IBlackboardSyncable
{
	[Hide]
	public override int Version => 1;

	[Hide]
	public override string Title
	{
		get
		{
			return $"F_{Feature.Name.ToUpper().Replace( " ", "_" )}";
		}
	}

	[Hide, Browsable( false )]
	public Guid BlackboardParameterIdentifier { get; set; }

	[Input]
	[Title( "True" )]
	[Hide]
	public NodeInput InputTrue { get; set; }

	[Input]
	[Title( "False" )]
	[Hide]
	public NodeInput InputFalse { get; set; }

	[Hide]
	public ShaderFeatureBoolean Feature { get; set; } = new();

	[Title( "Preview" )]
	public bool Preview { get; set; } = false;

	[Output, Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var inputs = new List<NodeInput>
		{
			InputTrue,
			InputFalse
		};

		return compiler.ResultFeatureSwitch( inputs, Feature, Preview ? 1 : 0 );
	};

	public void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is ShaderFeatureBooleanParameter boolFeatureParam )
		{
			if ( boolFeatureParam.IsValid )
			{
				Feature = new ShaderFeatureBoolean
				{
					Name = boolFeatureParam.Name,
					Description = boolFeatureParam.Description,
					HeaderName = boolFeatureParam.HeaderName,
				};
			}
		}
	}
}

// TOODO : Remove this.
[Title( "Static Combo Switch" ), Category( "Utility/Logic" ), Icon( "alt_route" )]
[InternalNode]
public sealed class StaticSwitchNode : ShaderNodePlus, IBlackboardSyncable
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color NodeTitleTintColor => PrimaryNodeHeaderColors.LogicNode;

	[Hide, Browsable( false )]
	public Guid BlackboardParameterIdentifier { get; set; }

	[Hide]
	public override string Title
	{
		get
		{

			return $"{DisplayInfo.For( this ).Name} ( F_{Feature.Name.ToUpper().Replace( " ", "_" )} )";
		}
	}

	[Hide]
	[JsonIgnore]
	public override bool CanPreview => false;

	[Input]
	[Title( "True" )]
	[Hide]
	public NodeInput InputTrue { get; set; }

	[Input]
	[Title( "False" )]
	[Hide]
	public NodeInput InputFalse { get; set; }

	//[ShowIf( nameof( Mode ), StaticSwitchMode.Create )]
	public bool PreviewToggle { get; set; } = false;

	[Hide]
	public ShaderFeatureBoolean Feature { get; set; } = new();

	public StaticSwitchNode() : base()
	{
		ExpandSize = new Vector2( 48 , 0 );
	}

	public void OnNodeCreated()
	{
		Update();
	}

	public void UpdateFromBlackboard( BaseBlackboardParameter parameter )
	{
		if ( parameter is ShaderFeatureBooleanParameter sfboolParameter )
		{
			Feature = new ShaderFeatureBoolean 
			{ 
				Name = sfboolParameter.Name,
				Description = sfboolParameter.Description,
				HeaderName = sfboolParameter.HeaderName,
			};
		}
	}

	[Output, Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		if ( string.IsNullOrWhiteSpace( Feature.Name ) )
			return NodeResult.Error( "Feature name cannot be blank." );

		return new NodeResult( ResultType.Float, $"{1.0f}" );

		//if ( compiler.ShaderFeatures.ContainsKey( Feature.Name ) )//&& FeatureReference != "None" )
		//{
		//	//SGPLog.Info( $"GraphFeatures contains feature {FeatureReference} ? : {compiler.Graph.Features.ContainsKey( FeatureReference )}", compiler.IsNotPreview );
		//
		//	if ( compiler.GenerateComboSwitch( compiler.ShaderFeatures[Feature.Name], InputTrue, InputFalse, PreviewToggle, true, out var switchResultVariableName, out var switchBody, out var switchResultType ) )
		//	{
		//		var result = new NodeResult( switchResultType, switchResultVariableName, constant: false );
		//
		//		result.SetMetadata( nameof( MetadataType.ComboSwitchBody ), switchBody );
		//
		//		return result;
		//	}
		//	else
		//	{
		//		return new NodeResult( ResultType.Float, $"{1.0f}" );
		//		//return NodeResult.Error( "Switch body could not be generated!" );
		//	}
		//}
		//else
		//{
		//	return NodeResult.Error( $"Shader Featue \"{Feature.Name}\" is not a valid registerd feature." );
		//}
	};
}
