
using Editor.NodeEditor;
using Sandbox.Resources;
using System.Diagnostics.CodeAnalysis;

namespace Editor.ShaderGraphPlus;

public sealed class SubgraphNode : ShaderNodePlus, IErroringNode
{
	[Hide]
	public string SubgraphPath { get; set; }

	[Hide, JsonIgnore]
	public ShaderGraphPlus Subgraph { get; set; }

	[Hide]
	private List<IPlugIn> InternalInputs = new();

	[Hide]
	public override IEnumerable<IPlugIn> Inputs => InternalInputs;

	[Hide]
	private List<IPlugOut> InternalOutputs = new();

	[Hide]
	public override IEnumerable<IPlugOut> Outputs => InternalOutputs;

	//[Editor( "subgraphplusnode" ), WideMode( HasLabel = false )]
	//public Dictionary<string, object> DefaultValues { get; set; } = new();

	[JsonIgnore, Hide]
	public override Color PrimaryColor => Color.Lerp( Theme.Blue, Theme.Green, 0.5f );

	[Editor( "subgraphplusnode" ), WideMode( HasLabel = false )]
	public Dictionary<string, DefaultSubgraphValueData> Test { get; set; } = new();

	[Hide]
	public override DisplayInfo DisplayInfo => new()
	{
		Name = Subgraph?.Title ?? (string.IsNullOrEmpty( Subgraph.Title ) ? "Untitled Subgraph" : Subgraph.Title),
		Description = Subgraph?.Description ?? "",
		Icon = Subgraph?.Icon ?? ""
	};

	public void OnNodeCreated()
	{
		//Test.Clear();
		//Test.Add( "Tint", new DefaultSubgraphValueData( new Vector3( 1.0f, 0.0f, 1.0f ) ) );
		//Test.Add( "B3", new DefaultSubgraphValueData( new Vector3( 1.0f, 0.0f, 1.0f ) ) );
		//Test.Add( "SamplerIn", new DefaultSubgraphValueData( new Sampler() { Name = "TestSamp"} ) );
		if ( Subgraph is not null ) return;

		if ( SubgraphPath != null )
		{

			Subgraph = new ShaderGraphPlus();
			var json = FileSystem.Content.ReadAllText( SubgraphPath );
			Subgraph.Deserialize( json, SubgraphPath );
			Subgraph.Path = SubgraphPath;

			CreateInputs();
			CreateOutputs();

			//foreach ( var node in Subgraph.Nodes )
			//{
			//	if ( node is ITextureParameterNode texNode && DefaultValues.TryGetValue( $"__tex_{texNode.UI.Name}", out var defaultTexVal ) )
			//	{
			//		texNode.Image = defaultTexVal.ToString();
			//	}
			//}

			Update();
		}
	}

	[Hide, JsonIgnore]
	internal Dictionary<IPlugIn, (IParameterNode paramNode, Type paramNodeValueType)> InputReferences = new();
	public void CreateInputs()
	{
		var plugs = new List<IPlugIn>();
		var defaults = new Dictionary<Type, int>();
		InputReferences.Clear();

		var parameterNodes = Subgraph.Nodes.OfType<IParameterNode>().OrderBy( x => x.UI.Priority );

		foreach ( var parameterNode in parameterNodes )
		{
			var name = parameterNode.Name;
			if ( string.IsNullOrWhiteSpace( name ) ) continue;

			var type = parameterNode.GetPortType();

			if ( string.IsNullOrEmpty( name ) )
			{
				if ( !defaults.ContainsKey( type ) )
				{
					defaults[type] = 0;
				}
				name = $"{type.Name}_{defaults[type]}";
				defaults[type]++;
			}

			var info = new PlugInfo()
			{
				Name = name,
				Type = type,
				DisplayInfo = new DisplayInfo()
				{
					Name = name,
					Fullname = type.FullName
				}
			};
			var plug = new BasePlugIn( this, info, type );
			var oldPlug = InternalInputs.FirstOrDefault( x => x is BasePlugIn plugIn && plugIn.Info.Name == info.Name && plugIn.Info.Type == info.Type ) as BasePlugIn;
			if ( oldPlug is not null )
			{
				oldPlug.Info.Name = info.Name;
				oldPlug.Info.Type = info.Type;
				oldPlug.Info.DisplayInfo = info.DisplayInfo;
				plug = oldPlug;
			}
			plugs.Add( plug );
			InputReferences[plug] = (parameterNode, type);

			if ( !Test.ContainsKey( plug.Identifier ) )
			{
				//SGPLog.Info( plug.Identifier );
				if ( parameterNode.GetValue() != null )
					Test.Add( plug.Identifier, new DefaultSubgraphValueData( parameterNode.GetValue() ) );
					//Test[plug.Identifier].DefaultValue = parameterNode.GetValue();
			}

		}

		InternalInputs = plugs;
	}

	[Hide, JsonIgnore]
	internal Dictionary<IPlugOut, IPlugIn> OutputReferences = new();
	public void CreateOutputs()
	{
		var resultNode = Subgraph.Nodes.OfType<FunctionResult>().FirstOrDefault();
		if ( resultNode is null ) return;

		var plugs = new List<IPlugOut>();
		foreach ( var output in resultNode.FunctionOutputs.OrderBy( x => x.Priority ) )
		{
			var outputType = output.Type;
			if ( outputType == typeof( ColorTextureGenerator ) )
			{
				outputType = typeof( Color );
			}
			if ( outputType is null ) continue;
			var info = new PlugInfo()
			{
				Name = output.Name,
				Type = outputType,
				DisplayInfo = new DisplayInfo()
				{
					Name = output.Name,
					Fullname = outputType.FullName
				}
			};
			var plug = new BasePlugOut( this, info, outputType );
			var oldPlug = InternalOutputs.FirstOrDefault( x => x is BasePlugOut plugOut && plugOut.Info.Name == info.Name && plugOut.Info.Type == info.Type ) as BasePlugOut;
			if ( oldPlug is not null )
			{
				oldPlug.Info.Name = info.Name;
				oldPlug.Info.Type = info.Type;
				oldPlug.Info.DisplayInfo = info.DisplayInfo;
				plugs.Add( oldPlug );
			}
			else
			{
				plugs.Add( plug );
			}
		}
		InternalOutputs = plugs;
	}

	public List<string> GetErrors()
	{
		OnNodeCreated();
		if ( Subgraph is null )
		{
			return new List<string> { $"Cannot find subgraph at \"{SubgraphPath}\"" };
		}

		var errors = new List<string>();

		foreach ( var node in Subgraph.Nodes )
		{
			if ( node is IErroringNode erroringNode )
			{
				errors.AddRange( erroringNode.GetErrors().Select( x => $"[{DisplayInfo.Name}] {x}" ) );
			}
		}

		foreach ( var input in InputReferences )
		{
			var plug = input.Key;
			var parameterNode = input.Value.paramNode;
			var inputName = parameterNode.Name;
			if ( string.IsNullOrWhiteSpace( inputName ) ) inputName = input.Key.DisplayInfo.Name;
			if ( parameterNode.IsAttribute && plug.ConnectedOutput is null )
			{
				errors.Add( $"Required Input \"{inputName}\" is missing on Node \"{Subgraph.Title}\"" );
			}
		}

		return errors;
	}

	public override void OnDoubleClick( MouseEvent e )
	{
		base.OnDoubleClick( e );

		if ( string.IsNullOrEmpty( SubgraphPath ) ) return;

		var shader = AssetSystem.FindByPath( SubgraphPath );
		if ( shader is null ) return;

		shader.OpenInEditor();
	}
}

internal static class WHT
{
	public static List<Type> WhitelistedTypes { get; private set; }
	//{
	//	typeof( bool ),
	//	typeof( float ),
	//	typeof( Vector2 ),
	//	typeof( Vector3 ),
	//	typeof( Vector4 ),
	//	typeof( Color ),
	//	typeof( Sampler ),
	//};

	static WHT()
	{
		Update();
	}

	[Event( "hotloaded" )]
	static void Update()
	{
		WhitelistedTypes = new()
		{
			typeof( bool ),
			typeof( float ),
			typeof( Vector2 ),
			typeof( Vector3 ),
			typeof( Vector4 ),
			typeof( Color ),
			typeof( Sampler ),
		};
	}
}

[CustomEditor( typeof( Dictionary<string, DefaultSubgraphValueData> ), NamedEditor = "subgraphplusnode", WithAllAttributes = [typeof( WideModeAttribute )] )]
internal class SubgraphNodeControlWidget : ControlWidget
{
	public override bool SupportsMultiEdit => false;

	SubgraphNode Node;
	ControlSheet Sheet;

	public SubgraphNodeControlWidget( SerializedProperty property ) : base( property )
	{
		Node = property.Parent.Targets.First() as SubgraphNode;

		Layout = Layout.Column();
		Layout.Spacing = 2;
		Sheet = new ControlSheet();
		Layout.Add( Sheet );

		Rebuild();
	}

	protected override void OnPaint()
	{

	}

	private void Rebuild()
	{
		Sheet.Clear( true );

		foreach ( var inputRef in Node.InputReferences )
		{
			if ( !WHT.WhitelistedTypes.Contains( inputRef.Value.paramNodeValueType ) )
			{
				SGPLog.Error( $"`{inputRef.Value.paramNodeValueType}` is not Whitelisted!!" );
				continue;
			}
	

			SGPLog.Info( $"Creating prop from inputRef {inputRef.Value.paramNodeValueType}" );

			if ( inputRef.Value.paramNode.IsAttribute ) continue;
			var name = inputRef.Key.Identifier;
			var type = inputRef.Value.paramNodeValueType;
			var getter = () =>
			{
				if ( Node.Test.ContainsKey( name ) )
				{
					//if ( Node.Test[name].DefaultValue is Sampler sampler )
					{
						//SGPLog.Info( $"Node.Test[name].DefaultValue is {Node.Test[name].DefaultValue}" );

					}

					
					return Node.Test[name].DefaultValue;
				}
				//else
				//{
				//	var val = inputRef.Value.Item1.GetValue();
				//
				//	//if ( val is null )
				//	//{
				//	//	SGPLog.Error( $"GetValue() was null!" );
				//	//}
				//	//SGPLog.Info( $"GetValue() was {val}" );
				//	return val;
				//}
				return null;
			};

			//SGPLog.Info( $"Creating prop of type `{type}` with name `{name}` with val {getter().ToString()}" );

			var displayName = $"Default {name}";
			if ( type == typeof( float ) )
			{
				Sheet.AddRow( TypeLibrary.CreateProperty<float>(
					displayName, () =>
					{
						var val = getter();
						return (float)val;
					}, x => SetDefaultValue( name, x )
				) );
			}
			else if ( type == typeof( Vector2 ) )
			{
				Sheet.AddRow( TypeLibrary.CreateProperty<Vector2>(
					displayName, () =>
					{
						var val = getter();
						return (Vector2)val;
					}, x => SetDefaultValue( name, x )
				) );
			}
			else if ( type == typeof( Vector3 ) )
			{
				Sheet.AddRow( TypeLibrary.CreateProperty<Vector3>(
					displayName, () =>
					{
						var val = getter();
						return (Vector3)val;
					}, x => SetDefaultValue( name, x )
				) );
			}
			else if ( type == typeof( Color ) )
			{
				Sheet.AddRow( TypeLibrary.CreateProperty<Color>(
					displayName, () =>
					{
						var val = getter();
						return (Color)val;
					}, x => SetDefaultValue( name, x )
				) );
			}
			else if ( type == typeof( Sampler ) )
			{
				Sheet.AddRow( TypeLibrary.CreateProperty<Sampler>(
					displayName, () =>
					{
						var val = getter();
						return (Sampler)val;
					}, x => SetDefaultValue( name, x )
				) );
			}
		}
	}

	private void SetDefaultValue( string name, object value )
	{
		Node.Test[name].DefaultValue = value;
		Node.Update();
		Node.IsDirty = true;
	}
}
