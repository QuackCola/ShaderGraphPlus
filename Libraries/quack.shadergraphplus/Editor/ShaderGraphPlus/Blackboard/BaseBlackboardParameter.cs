namespace ShaderGraphPlus;

internal interface ISubgraphBlackboardParameter
{
	public string Name { get; set; }
	public string Description { get; set; }
	public bool IsRequired { get; set; }
	public int PortOrder { get; set; }

	public object GetValue();
}

internal interface IShaderFeatureBlackboardParameter
{
}

public abstract class BaseBlackboardParameter : IBlackboardParameter
{
	[Sandbox.ReadOnly, Browsable( false )]
	public Guid Identifier { get; set; } = Guid.NewGuid();

	public virtual string Name { get; set; } = "";

	[Hide, JsonIgnore, Browsable( false )]
	public ShaderGraphPlus Graph { get; set; }

	[Hide, JsonIgnore, Browsable( false )]
	public bool IsSubgraph => Graph.IsSubgraph;

	[Hide, JsonIgnore, Browsable( false )]
	public virtual int MenuOrder => 0;

	public BaseBlackboardParameter()
	{
	}

	public virtual object GetValue()
	{
		throw new NotImplementedException();
	}

	Color IBlackboardParameter.GetTypeColor( BlackboardView view )
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Called when a blackboard parameter needs to 
	/// get converted into an accompanying node.
	/// </summary>
	public abstract BaseNodePlus InitializeNode();
}

public abstract class BlackboardGenericParameter<T> : BaseBlackboardParameter
{
	[InlineEditor( Label = false )]
	public T Value { get; set; }

	public override object GetValue()
	{
		return Value;
	}

	public BlackboardGenericParameter() : base() 
	{ 
	}

	public BlackboardGenericParameter( T value ) : this() 
	{ 
		Value = value;
	}
}

public abstract class BlackboardMaterialParameter<T> : BlackboardGenericParameter<T>
{
	[InlineEditor( Label = false ), Group( "UI" )]
	public ParameterUI UI { get; set; }

	public bool IsAttribute { get; set; }

	public BlackboardMaterialParameter() : base() 
	{ 
	}

	public BlackboardMaterialParameter( T value ) : base( value )
	{
	}
}

public abstract class BlackboardSubgraphInputParameter<T> : BlackboardGenericParameter<T>, ISubgraphBlackboardParameter
{
	[Title( "Input Name" )]
	public override string Name { get; set; }

	[Title( "Input Description" )]
	[TextArea]
	public string Description { get; set; }

	/// <summary>
	/// Is this input required to have a valid connection?
	/// </summary>
	public bool IsRequired { get; set; } = false;

	public int PortOrder { get; set; }

	public BlackboardSubgraphInputParameter() : base()
	{
	}

	public BlackboardSubgraphInputParameter( T value ) : this()
	{
		Value = value;
	}

	public override object GetValue()
	{
		return Value;
	}
}
