using Editor;
using NodeEditorPlus;
using ShaderGraphPlus.Nodes;
using HandleConfig = NodeEditorPlus.HandleConfig;

namespace ShaderGraphPlus;

/// <summary>
/// Storing the Node Header Primary Colors here so that they are all in one place.
/// </summary>
public static class PrimaryNodeHeaderColors
{
	public static Color SubgraphNode => Color.Parse( "#e05b0a" )!.Value;
	public static Color GraphResultNode => Color.Parse( "#84705e" )!.Value;
	public static Color UnaryNode => Color.Parse( "#394d62" )!.Value;
	public static Color BinaryNode => Color.Parse( "#394d62" )!.Value;
	public static Color ConstantNode => Color.Parse( "#803334" )!.Value;
	public static Color ParameterNode => Color.Parse( "#5d9b31" )!.Value;
	public static Color StageInputNode => Color.Parse( "#803334" )!.Value;
	public static Color GlobalVariableNode => Color.Parse( "#803334" )!.Value;
	public static Color FunctionNode => Color.Parse( "#1d53ac" )!.Value;
	public static Color TransformNode => Color.Parse( "#6c3baa" )!.Value;
	public static Color LogicNode => Color.Parse( "#006b54" )!.Value;
	public static Color ChannelNode => Color.Parse( "#2e2a60" )!.Value;
}

internal static class ShaderGraphPlusTheme
{
	public record struct BlackboardConfig( string Name, Color Color );

	public static Dictionary<Type, HandleConfig> HandleConfigs { get; private set; }
	public static Dictionary<Type, BlackboardConfig> BlackboardConfigs { get; private set; }

	static ShaderGraphPlusTheme()
	{
		Update();
	}

	[Event( "hotloaded" )]
	static void Update()
	{
		HandleConfigs = new()
		{
		{ typeof( bool ), new HandleConfig( "bool", Theme.Blue.AdjustHue( -80 ) ) },
		{ typeof( int ), new HandleConfig( "int", Color.Parse( "#ce67e0" )!.Value.AdjustHue( -80 ) ) },
		{ typeof( float ), new HandleConfig( "Float", Color.Parse( "#8ec07c" )!.Value ) },
		{ typeof( Vector2 ), new HandleConfig( "Vector2", Color.Parse( "#ce67e0" )!.Value ) },
		{ typeof( Vector3 ), new HandleConfig( "Vector3", Color.Parse( "#7177e1" )!.Value ) },
		{ typeof( Vector4 ), new HandleConfig( "Vector4", Color.Parse( "#c7ae32" )!.Value ) },
		{ typeof( Color ), new HandleConfig( "Color", Color.Parse( "#c7ae32" )!.Value ) },
		{ typeof( Float2x2 ), new HandleConfig( "Float2x2", Color.Parse( "#a3b3c9" )!.Value ) },
		{ typeof( Float3x3 ), new HandleConfig( "Float3x3", Color.Parse( "#a3b3c9" )!.Value ) },
		{ typeof( Float4x4 ), new HandleConfig( "Float4x4", Color.Parse( "#a3b3c9" )!.Value ) },
		{ typeof( Texture2DObject ), new HandleConfig( "Texture2D", Color.Parse( "#ffb3a7" )!.Value ) },
		{ typeof( Sampler ), new HandleConfig( "Sampler", Color.Parse( "#dddddd" )!.Value ) },
		{ typeof( Gradient ), new HandleConfig( "Gradient", Color.Parse( "#dddddd" )!.Value ) },
		};

		BlackboardConfigs = new()
		{
			{ typeof( BoolSubgraphInputParameter ), new BlackboardConfig( "bool", HandleConfigs[typeof( bool )].Color ) },
			{ typeof( IntSubgraphInputParameter ), new BlackboardConfig( "int", HandleConfigs[typeof( int )].Color ) },
			{ typeof( FloatSubgraphInputParameter ), new BlackboardConfig( "float", HandleConfigs[typeof( float )].Color ) },
			{ typeof( Float2SubgraphInputParameter ), new BlackboardConfig( "float2", HandleConfigs[typeof( Vector2 )].Color ) },
			{ typeof( Float3SubgraphInputParameter ), new BlackboardConfig( "float3", HandleConfigs[typeof( Vector3 )].Color ) },
			{ typeof( Float4SubgraphInputParameter ), new BlackboardConfig( "float4", HandleConfigs[typeof( Vector4 )].Color ) },
			{ typeof( ColorSubgraphInputParameter ), new BlackboardConfig( "float4", HandleConfigs[typeof( Color )].Color ) },
			{ typeof( BoolParameter ), new BlackboardConfig( "bool", HandleConfigs[typeof( bool )].Color ) },
			{ typeof( IntParameter ), new BlackboardConfig( "int", HandleConfigs[typeof( int )].Color ) },
			{ typeof( FloatParameter ), new BlackboardConfig( "float", HandleConfigs[typeof( float )].Color ) },
			{ typeof( Float2Parameter ), new BlackboardConfig( "float2", HandleConfigs[typeof( Vector2 )].Color ) },
			{ typeof( Float3Parameter ), new BlackboardConfig( "float3", HandleConfigs[typeof( Vector3 )].Color ) },
			{ typeof( Float4Parameter ), new BlackboardConfig( "float4", HandleConfigs[typeof( Vector4 )].Color ) },
			{ typeof( ColorParameter ), new BlackboardConfig( "float4", HandleConfigs[typeof( Color )].Color ) },
		};
	}
}
