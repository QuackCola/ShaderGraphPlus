
namespace ShaderGraphPlus.Nodes;

[System.AttributeUsage( AttributeTargets.Property )]
internal sealed class NamedRerouteReferenceAttribute : Attribute
{
}

/// <summary>
/// Declare a named reroute.
/// </summary>
[Title( "Named Reroute Declaration" ), Category( "Utility" ), Icon( "route" )]
public sealed class NamedRerouteDeclarationNode : ShaderNodePlus, IErroringNode
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color PrimaryHeaderColor => Color.Parse( "#9d00ff" )!.Value;

	[Hide]
	public override string Title => string.IsNullOrEmpty( _name ) ?
	$"{DisplayInfo.For( this ).Name}" :
	$"{_name}"; // $"{DisplayInfo.For( this ).Name} ({Name})";

	[Hide, JsonIgnore]
	private bool HasNameConflict;

	[Hide,JsonIgnore]
	private string _name;
	public string Name 
	{
		get => _name;
		set
		{
			var lastName = _name;

			if ( !HasNameConflict )
			{
				UpdateNameReferences( lastName, value );
			}

			_name = value;
		}
	}

	private void UpdateNameReferences( string lastName, string newName )
	{
		var graph = Graph as ShaderGraphPlus;
		
		if ( !string.IsNullOrWhiteSpace( lastName ) )
		{
			foreach ( var namedReroute in graph.Nodes.OfType<NamedRerouteNode>() )
			{
				if ( namedReroute.Name == lastName )
				{
					//SGPLog.Info( $"Changing named reroute name reference from \"{namedReroute.Name}\" to \"{newName}\"" );
					namedReroute.Name = newName;
				}
			}
		}
	}

	[Title( "Input" )]
	[Input]
	[Hide]
	public NodeInput Input { get; set; }

	[Output, Hide, Title( "" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var result = compiler.ResultOrDefault( Input, 0.0f );
		result.Constant = true;
		return result;
	};

	public List<string> GetErrors()
	{
		var errors = new List<string>();

		if ( !string.IsNullOrWhiteSpace( _name ) )
		{
			var graph = Graph as ShaderGraphPlus;

			if ( _name == "None" )
				errors.Add( "\"None\" is a reserved name" );

			foreach ( var node in graph.Nodes.OfType<NamedRerouteDeclarationNode>() )
			{
				if ( node == this )
					continue;

				if ( node._name == _name )
				{
					errors.Add( $"Duplicate name \"{_name}\" on {this.DisplayInfo.Name}" );
					HasNameConflict = true;
					break;
				}
				else
				{
					HasNameConflict = false;
				}
			}
		}

		return errors;
	}
}

/// <summary>
/// Reference a declared named reroute.
/// </summary>
[Title( "Named Reroute" ), Category( "Utility" ), Icon( "route" )]
public sealed class NamedRerouteNode : ShaderNodePlus
{
	[Hide]
	public override int Version => 1;

	[JsonIgnore, Hide, Browsable( false )]
	public override Color PrimaryHeaderColor => Color.Parse( "#9d00ff" )!.Value;

	[Hide]
	public override string Title => string.IsNullOrEmpty( Name ) ?
		$"{DisplayInfo.For( this ).Name}" :
		$"{Name}"; // $"{DisplayInfo.For( this ).Name} ({Name})";

	[NamedRerouteReference]
	public string Name { get; set; } = "";

	[Output, Hide, Title( "" )]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var result = compiler.ResultNamedReroute( Name );

		if ( !result.IsValid )
		{
			return new NodeResult( ResultType.Float, "0.0f", constant : true );
		}

		return new NodeResult( result.ResultType, result.Code, constant: true );
	};
}
