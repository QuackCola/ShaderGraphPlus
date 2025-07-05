using Editor.ShaderGraphPlus;

[Title( "Sampler" ), Category( "Textures" ), Icon( "colorize" )]
[Description( "How a texture is filtered and wrapped when sampled." )]
public sealed class SamplerNode : ShaderNodePlus, IParameterNode
{
	[InlineEditor( Label = false ), Group( "Sampler" )]
	[HideIf( nameof( IsSubgraph ), true )]
	public Sampler SamplerState { get; set; } = new Sampler();

	[Hide]
	public override string Title
	{
		get
		{
			if ( !string.IsNullOrWhiteSpace( SamplerState.Name ) && !IsSubgraph )
			{
				return $"{DisplayInfo.For( this ).Name} ({SamplerState.Name})";
			}
			else if ( !IsSubgraph )
			{
				return $"{DisplayInfo.For( this ).Name}";
			}

			if ( IsSubgraph )
			{
				return $"{DisplayInfo.For( this ).Name} ({Name})";
			}


			return $"{DisplayInfo.For( this ).Name}";
		}
	}

	public SamplerNode() : base()
	{
		ExpandSize = new Vector2( 0, 8 );
	}

	[Hide]
	private bool IsSubgraph => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.IsSubgraph);

	[ShowIf( nameof( IsSubgraph ), true )]
	[Title( "Input Name" )]
	public string Name { get; set; }

	[Hide, JsonIgnore]
	public bool IsAttribute { get; set; }


	[Hide, JsonIgnore]
	ParameterUI IParameterNode.UI { get; set; }

	[Input, Title( "Preview" ), Hide]
	[HideIf( nameof( IsSubgraph ), false )]
	public NodeInput PreviewInput { get; set; }

	public Type GetPortType()
	{
		return typeof( Sampler );
	}

	public object GetValue()
	{
		return null;
	}

	public void SetValue( object val )
	{
		throw new NotImplementedException( $"{DisplayInfo.ClassName}.SetValue" );
	}

	public Vector4 GetRangeMin()
	{
		return Vector4.Zero;
	}

	public Vector4 GetRangeMax()
	{
		return Vector4.One;
	}

	[Output( typeof( Sampler ) ), Hide]
	public NodeResult.Func Sampler => ( GraphCompiler compiler ) =>
	{
        // Register sampler	with the compiler.
        var result = compiler.ResultSampler( SamplerState );

		if ( !string.IsNullOrWhiteSpace( result ) )
		{
			return new NodeResult( ResultType.Sampler, result, true, true );
		}
		else
		{
			return NodeResult.Error( "Shits fucked..." );
		}
	};
}
