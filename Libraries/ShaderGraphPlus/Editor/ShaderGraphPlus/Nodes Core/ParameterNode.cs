using Editor.ShaderGraph;
using ShaderGraphPlus.Nodes;
using System.Text.Json.Serialization;

namespace ShaderGraphPlus;

public interface IParameterNode
{
	string Name { get; set; }
	
	bool IsAttribute { get; set; }
	
	ParameterUI UI { get; set; }
	
	public int PortOrder { get; set; }
	
	NodeInput PreviewInput { get; set; }

	public Vector2 ParameterNodePosition { get; }

	Type GetPortType();

	object GetValue();
	void SetValue( object val );

	Vector4 GetRangeMin();
	Vector4 GetRangeMax();

	//public SubgraphInput UpgradeToSubgraphInput();
}

public interface ITextureParameterNode
{
	string Image { get; set; }
	TextureInput UI { get; set; }

	/// <summary>
	/// Only used by Preview.
	/// </summary>
	bool AlreadyRegisterd { get; set; }
}

//[NodeReplace( ReplacementMode.SubgraphOnly )]
public abstract class ParameterNode<T> : ShaderNodePlus, IParameterNode, IErroringNode//, IReplaceNode
{
	[Hide]
	protected bool IsSubgraph => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph);

	//[Hide, JsonIgnore]
	//public bool ReplacementCondition => !string.IsNullOrWhiteSpace( Name );
	//
	//public BaseNodePlus GetReplacementNode()
	//{
	//	return UpgradeToSubgraphInput();
	//}

	[Hide]
	public override string Title => string.IsNullOrWhiteSpace( Name ) ?
		$"{DisplayInfo.For( this ).Name}" :
		$"{DisplayInfo.For( this ).Name} ( {Name} )";

	//[Input, ShowIf( nameof( IsSubgraph ), true ), Title( "Preview" ), Hide]
	[Hide]
	public NodeInput PreviewInput { get; set; }

	//[ShowIf( nameof( IsSubgraph ), true )]
	[Hide]
	public int PortOrder { get; set; }

	public T Value { get; set; }

	[HideIf( nameof( IsSubgraph ), true )]
	public string Name { get; set; } = "";

	[Hide, JsonIgnore]
	public Vector2 ParameterNodePosition => Position;

	/// <summary>
	/// If true, this parameter can be modified with <see cref="RenderAttributes"/>.
	/// </summary>
	[HideIf( nameof( IsSubgraph ), true )]
	public bool IsAttribute { get; set; }

	/// <summary>
	/// If true, this parameter can be modified directly on the subgraph node.
	/// </summary>
	//[JsonIgnore, ShowIf( nameof( IsSubgraph ), true )]
	[Hide]
	protected bool IsRequiredInput
	{
		get => IsAttribute;
		set => IsAttribute = value;
	}

	[InlineEditor( Label = false ), Group( "UI" )]
	[HideIf( nameof( IsSubgraph ), true )]
	public ParameterUI UI { get; set; }


	protected NodeResult Component( string component, float value, GraphCompiler compiler )
	{
		if ( compiler.IsPreview )
			return compiler.ResultValue( value );

		var result = compiler.Result( new NodeInput { Identifier = Identifier, Output = nameof( Result ) } );
		return new( ResultType.Float, $"{result}.{component}", true );
	}

	public Type GetPortType()
	{
		return typeof( T );
	}

	public virtual object GetDefaultValue()
	{
		return default( T );
	}

	public object GetValue()
	{
		return Value;
	}

	public virtual Vector4 GetRangeMin()
	{
		return Vector4.Zero;
	}

	public virtual Vector4 GetRangeMax()
	{
		return Vector4.Zero;
	}

	public void SetValue( object val )
	{
		Value = (T)val;
	}

	//public virtual SubgraphInput UpgradeToSubgraphInput()
	//
	//	return default ( SubgraphInput );
	//

	public List<string> GetErrors()
	{
		var errors = new List<string>();

		if ( Name.Contains( ' ' ) )
		{
			//errors.Add( $"Parameter name \"{Name}\" cannot contain spaces" );
		}

		foreach ( var parameterNode in Graph.Nodes )
		{
			if ( parameterNode == this )
				continue;

			if ( !string.IsNullOrWhiteSpace( Name ) && parameterNode is IParameterNode pn && pn.Name == Name )
			{
				errors.Add( $"Duplicate name \"{Name}\" on {this.DisplayInfo.Name}" );
				break;
			}
		}

		return errors;
	}
}
