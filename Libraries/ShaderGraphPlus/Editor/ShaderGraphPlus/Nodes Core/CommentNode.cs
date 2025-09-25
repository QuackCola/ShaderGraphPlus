using System.Text.Json.Nodes;
using CommentUI = NodeEditorPlus.CommentUI;
using GraphView = NodeEditorPlus.GraphView;
using ICommentNode = NodeEditorPlus.ICommentNode;
using IPlugIn = NodeEditorPlus.IPlugIn;
using IPlugOut = NodeEditorPlus.IPlugOut;
using NodeUI = NodeEditorPlus.NodeUI;

namespace ShaderGraphPlus;

[Icon( "notes" ), Hide]
public class CommentNode : BaseNodePlus, ICommentNode
{
	[Hide]
	public override int Version => 2;

	[Hide, Browsable( false )]
	public Vector2 Size { get; set; }

	public Color Color { get; set; } = Color.Parse( $"#33b679" )!.Value;

	public string Title { get; set; } = "Untitled";

	[TextArea]
	public string Description { get; set; } = "";

	[Hide, Browsable( false )]
	public int Layer { get; set; } = 5;

	[Hide, JsonIgnore]
	public override DisplayInfo DisplayInfo
	{
		get
		{
			var info = DisplayInfo.For( this );

			info.Name = Title;
			info.Description = Description;

			return info;
		}
	}

	public override NodeUI CreateUI( GraphView view )
	{
		return new CommentUI( view, this );
	}

	[SGPJsonUpgrader( typeof( CommentNode ), 2 )]
	public static void Upgrader_v2( JsonObject json )
	{
		if ( !json.ContainsKey( "Color" ) )
		{
			return;
		}
	
		try
		{
			switch ( json["Color"].ToString() )
			{
				case "White":
					json["Color"] = JsonSerializer.SerializeToNode( Color.Parse( $"#c2b5b5" )!.Value, ShaderGraphPlus.SerializerOptions() );
				break;
				case "Red":
					json["Color"] = JsonSerializer.SerializeToNode( Color.Parse( $"#d60000" )!.Value, ShaderGraphPlus.SerializerOptions() );
				break;
				case "Green":
					json["Color"] = JsonSerializer.SerializeToNode( Color.Parse( $"#33b679" )!.Value, ShaderGraphPlus.SerializerOptions() );
				break;
				case "Blue":
					json["Color"] = JsonSerializer.SerializeToNode( Color.Parse( $"#039be5" )!.Value, ShaderGraphPlus.SerializerOptions() );
				break;
				case "Yellow":
					json["Color"] = JsonSerializer.SerializeToNode( Color.Parse( $"#f6c026" )!.Value, ShaderGraphPlus.SerializerOptions() );
				break;
				case "Purple":
					json["Color"] = JsonSerializer.SerializeToNode( Color.Parse( $"#8e24aa" )!.Value, ShaderGraphPlus.SerializerOptions() );
				break;
				case "Orange":
					json["Color"] = JsonSerializer.SerializeToNode( Color.Parse( $"#f5511d" )!.Value, ShaderGraphPlus.SerializerOptions() );
				break;
				default:
					json["Color"] = JsonSerializer.SerializeToNode( Color.Parse( $"#c2b5b5" )!.Value, ShaderGraphPlus.SerializerOptions() );
				break;
			}
		}
		catch
		{
		}
	}
}

