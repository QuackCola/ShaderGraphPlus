using Sandbox.Resources;
using System.Text;

namespace Editor.ShaderGraphPlus.Nodes;

public enum StaticSwitchMode
{
	Create,
	Reference
}

[CustomEditor( typeof( string ), NamedEditor = "FeatureReference" )]
internal sealed class FeatureReferenceControlWidget : DropdownControlWidget<string>
{ 
	ShaderGraphPlus Graph;
	StaticSwitchNode Node;


	public FeatureReferenceControlWidget( SerializedProperty property ) : base( property )
	{
		Node = property.Parent.Targets.OfType<StaticSwitchNode>().FirstOrDefault();
		Graph = (ShaderGraphPlus)Node.Graph;

		if ( Graph is null ) return;

		Log.Info("Graph!");

		if ( string.IsNullOrWhiteSpace( SerializedProperty.GetValue<string>() ) )
		{ 
			SerializedProperty.SetValue<string>( "" );
		}
	}

	protected override IEnumerable<string> GetDropdownValues()
	{
		List<string> list = new();

		foreach ( var feature in Graph.FeatureNames )
		{
			list.Add( feature );
		}

		return list;
	}
}


[Title( "Static Switch" ), Category( "Utility" )]
public sealed class StaticSwitchNode : ShaderNodePlus
{
	[Hide]
	public override string Title => $"{DisplayInfo.For( this ).Name} ({Feature.FeatureName})";

	[Input]
	[Title( "True" )]
	[Hide]
	public NodeInput InputTrue { get; set; }

	[Input]
	[Title( "False" )]
	[Hide]
	public NodeInput InputFalse { get; set; }

	public bool PreviewToggle { get; set; } = false;

	/// <summary>
	/// TODO : Implement Reference Mode.
	/// </summary>
	[Sandbox.ReadOnly]
	public StaticSwitchMode Mode { get; set; } = StaticSwitchMode.Create;

	[InlineEditor( Label = false ), ShowIf( nameof( Mode ), StaticSwitchMode.Create ), Group( "Feature" )]
	public ShaderFeature Feature { get; set; } = new();
	
	[JsonIgnore]
	[Title( "Feature" ), ShowIf( nameof( Mode ), StaticSwitchMode.Reference )]
	[Editor( "FeatureReference" )]
	public string FeatureReference { get; set; } = "";

	[Output, Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		if ( string.IsNullOrWhiteSpace( Feature.FeatureName ) )
			return NodeResult.Error( "Feature must have a valid name!" );

		if ( compiler.RegisterdFeatureNames.Contains( Feature.FeatureName ) )
			return NodeResult.Error( $"Feature name `{Feature.FeatureName}` is already registered!" );

		if ( Mode is StaticSwitchMode.Create )
		{
			if ( compiler.RegisterShaderFeatureBinary( Feature, out ShaderFeatureInfo shaderFeature ) )
			{
				if ( compiler.GenerateComboSwitch( shaderFeature, InputTrue, InputFalse, PreviewToggle, out var switchResultVariableName, out var switchBody, out var switchResultType ) )
				{
					return new NodeResult( switchResultType, switchResultVariableName, switchBody, constant: false );
				}
				else
				{
					return new NodeResult( ResultType.Float, $"{1.0f}" );
					//return NodeResult.Error( "Switch body could not be generated!" );
				}
			}
			else
			{

				return NodeResult.Error( "Feature Is Invalid!" );
			}
		}
		else
		{
			return NodeResult.Error( $"Mode `{StaticSwitchMode.Reference}` is not yet implamented" );
		}
	};
}
