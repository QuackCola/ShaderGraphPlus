using static Sandbox.Resources.ResourceGenerator;

namespace ShaderGraphPlus;

internal interface ISubgraphBlackboardParameter
{
	public string Name { get; set; }
	public string Description { get; set; }
	public bool IsRequired { get; set; }
	public int PortOrder { get; set; }

	public object GetValue();
}

internal interface IShaderFeatureParameter
{
}

public abstract class BaseBlackboardParameter : IValid, IBlackboardParameter
{
	[Sandbox.ReadOnly, Browsable( false )]
	public Guid Identifier { get; set; }

	[JsonIgnore, Hide, Browsable( false )]
	public DisplayInfo DisplayInfo { get; }

	[Hide, JsonIgnore, Browsable( false )]
	private ShaderGraphPlus _graph;
	[Hide, JsonIgnore, Browsable( false )]
	public ShaderGraphPlus Graph
	{
		get => _graph;
		set
		{
			_graph = value;
		}
	}

	[Hide, JsonIgnore, Browsable( false )]
	public bool IsSubgraph => Graph.IsSubgraph;

	[Hide, JsonIgnore, Browsable( false )]
	public virtual int MenuOrder => 0;

	[Hide, JsonIgnore, Browsable( false )]
	public virtual bool IsValid => true;

	public virtual string Name { get; set; } = "";

	public BaseBlackboardParameter()
	{
		DisplayInfo = DisplayInfo.For( this );
		NewIdentifier();
	}

	public virtual object GetValue()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Called when a blackboard parameter needs to 
	/// get converted into an accompanying node.
	/// </summary>
	public abstract BaseNodePlus InitializeNode();

	public Guid NewIdentifier()
	{
		Identifier = Guid.NewGuid();
		return Identifier;
	}

	public string GetCleanName()
	{
		if ( string.IsNullOrWhiteSpace( Name ) )
			return "";

		Name = Name.Trim();
		Name = new string( Name.Where( x => char.IsLetter( x ) || char.IsNumber( x ) || x == '_' ).ToArray() );

		return Name;
	}

	public override string ToString()
	{
		return $"{DisplayInfo.Fullname}.{Identifier}";
	}
}

public abstract class BlackboardGenericParameter<T> : BaseBlackboardParameter
{
	[InlineEditor( Label = false )]
	public T Value { get; set; }

	public BlackboardGenericParameter() : base() 
	{ 
	}

	public BlackboardGenericParameter( T value ) : this() 
	{ 
		Value = value;
	}

	public override object GetValue()
	{
		return Value;
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
