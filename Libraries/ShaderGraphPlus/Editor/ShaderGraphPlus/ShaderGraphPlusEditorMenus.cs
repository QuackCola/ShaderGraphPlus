using Editor;
using MaterialDesign;
using System.Text.Json.Nodes;

namespace ShaderGraphPlus;

internal static class ShaderGraphPlusEditorMenus
{
	[Menu( "Editor", "Shader Graph Plus/Upgrade Subgraph Projects" )]
	public static void UpgradeSubgraphProjects()
	{
		var projectPaths = Directory.GetFiles( $"{Project.Current.GetAssetsPath()}/shaders", "*.sgpfunc", SearchOption.AllDirectories );
		int projectsUpgraded = 0;

		if ( projectPaths.Any() )
		{
			SGPLog.Info( $"Found \"{projectPaths.Count()}\" subgraphs" );

			foreach ( var projectPath in projectPaths )
			{
				var graph = new ShaderGraphPlus();
				var file = System.IO.File.ReadAllText( projectPath );

				if ( ShaderGraphPlus.GetProjectVersion( JsonDocument.Parse( file ).RootElement ) == 0 )
				{
					graph.Deserialize( file );

					var asset = AssetSystem.FindByPath( projectPath );

					graph.Path = asset.RelativePath;
					graph.IsSubgraph = true;

					SGPLog.Info( $"Upgraded project at path \"{projectPath}\"" );

					System.IO.File.WriteAllText( asset.AbsolutePath, graph.Serialize() );
					asset ??= AssetSystem.RegisterFile( asset.AbsolutePath );
					projectsUpgraded++;
				}
				else
				{
					SGPLog.Info( $"Project at path \"{projectPath}\" has already been upgraded." );
				}
			}

		}

		EditorUtility.DisplayDialog( "", $"Upgraded \"{projectsUpgraded}\" subgraphs." );
	}

	[Menu( "Editor", "Shader Graph Plus/Convert ShaderGraph projects to ShaderGraphPlus projects ( Experimental )" )]
	public static void ConvertShaderGraphToShaderGraphPlus()
	{
		var projectPaths = Directory.GetFiles( $"{Project.Current.GetAssetsPath()}/shaders", "*.shdrgrph", SearchOption.AllDirectories );

		var projectItems = new List<ProjectItem>();
		foreach ( var path in projectPaths )
		{
			projectItems.Add( new ProjectItem( path ) );
		}

		ProjectConverterDialog.DisplayDialog( projectItems );
	}


	/*
	[Menu( "Editor", "Shader Graph Plus/Update Subgraphs internal Path String" )]
	public static void UpdateSubgraphsInternalPath()
	{
		var projectPaths = Directory.GetFiles( $"{Project.Current.GetAssetsPath()}/shaders", "*.sgpfunc", SearchOption.AllDirectories );

		if ( projectPaths.Any() )
		{
			SGPLog.Info( $"Found \"{projectPaths.Count()}\" subgraphs" );

			foreach ( var projectPath in projectPaths )
			{
				var graph = new ShaderGraphPlus();
				var file = System.IO.File.ReadAllText( projectPath );

				graph.Deserialize( file );

				var asset = AssetSystem.FindByPath( projectPath );

				var oldPath = graph.Path;
				graph.Path = asset.RelativePath;
				graph.IsSubgraph = true;

				SGPLog.Info( $"Upgraded project subgraphPath from \"{oldPath}\" to \"{graph.Path}\"" );

				System.IO.File.WriteAllText( asset.AbsolutePath, graph.Serialize() );
				asset ??= AssetSystem.RegisterFile( asset.AbsolutePath );
			}
		}

		EditorUtility.DisplayDialog( "", $"Updated \"{projectPaths.Count()}\" subgraphs internal path property." );
	}
	*/
}

file class ProjectConverterDialog : Dialog
{
	private ProjectList _projectList; 
	
	public Layout ListLayout { get; private set; }
	public Layout ButtonLayout { get; private set; }
	
	public ProjectConverterDialog( List<ProjectItem> projectItems ) : base( null, true )
	{
		Window.FixedWidth = 650f;
		Window.MaximumSize = Window.Size;
		Window.MinimumSize = Window.Size;
		Window.Title = "Convert ShaderGraph To ShaderGraphPlus";
		Window.SetWindowIcon( MaterialIcons.Gradient );
		Window.SetModal( true, true );

		CreateUI();

		SetItems( projectItems );
	}

	private void CreateUI()
	{
		Layout = Layout.Column();
		Layout.Spacing = 3;

		Layout.AddSpacingCell( 8 );

		var label = Layout.Add( new Label() );
		label.Text = "Projects to convert";
		label.Alignment = TextFlag.Center;

		ListLayout = Layout.AddColumn();
		ListLayout.Add( _projectList = new ProjectList( this ) );
		ListLayout.Spacing = 8f;
		ListLayout.Margin = 16f;

		ButtonLayout = Layout.AddColumn();
		ButtonLayout.Spacing = 8f;
		ButtonLayout.Margin = 16f;
	}

	private void SetItems( List<ProjectItem> projectItems )
	{
		_projectList.Projects = projectItems;
	}

	protected override bool OnClose()
	{
		return true;
	}

	public static void DisplayDialog( List<ProjectItem> projectItems )
	{
		var dialog = new ProjectConverterDialog( projectItems );

		dialog.ButtonLayout.Add( new Button.Primary( "Convert Projects" )
		{
			Clicked = delegate
			{
				ConvertProjects( dialog );

				//dialog.Destroy();
				//dialog.Close();
			}
		} );

		dialog.SetModal( on: true, application: true );
		dialog.Hide();
		dialog.Show();
	}

	private static void ConvertProjects( ProjectConverterDialog dialog )
	{
		var projects = dialog._projectList.Projects;

		foreach ( var project in projects )
		{
			SGPLog.Info( $"Converting project at path \"{project}\"" );

			var graph = new Editor.ShaderGraph.ShaderGraph();
			var graphText = System.IO.File.ReadAllText( project.Path );
			graph.Deserialize( graphText );

			var newGraph = new ShaderGraphPlus();
			newGraph.IsSubgraph = graph.IsSubgraph;
			newGraph.Path = graph.Path.Replace( ".shdrgrph", ".sgrph" );
			newGraph.Model = graph.Model;
			newGraph.Description = graph.Description;

			switch ( graph.BlendMode )
			{
				case Editor.ShaderGraph.BlendMode.Opaque:
					newGraph.BlendMode = BlendMode.Opaque;
					break;
				case Editor.ShaderGraph.BlendMode.Masked:
					newGraph.BlendMode = BlendMode.Masked;
					break;
				case Editor.ShaderGraph.BlendMode.Translucent:
					newGraph.BlendMode = BlendMode.Translucent;
					break;
			}

			switch ( graph.ShadingModel )
			{
				case Editor.ShaderGraph.ShadingModel.Lit:
					newGraph.ShadingModel = ShadingModel.Lit;
					break;
				case Editor.ShaderGraph.ShadingModel.Unlit:
					newGraph.ShadingModel = ShadingModel.Unlit;
					break;
			}

			switch ( graph.Domain )
			{
				case Editor.ShaderGraph.ShaderDomain.Surface:
					newGraph.MaterialDomain = MaterialDomain.Surface;
					break;
				case Editor.ShaderGraph.ShaderDomain.PostProcess:
					newGraph.MaterialDomain = MaterialDomain.PostProcess;
					break;
			}

			newGraph.PreviewSettings.ShowGround = graph.PreviewSettings.ShowGround;
			newGraph.PreviewSettings.ShowSkybox = graph.PreviewSettings.ShowSkybox;
			newGraph.PreviewSettings.EnableShadows = graph.PreviewSettings.EnableShadows;
			newGraph.PreviewSettings.BackgroundColor = graph.PreviewSettings.BackgroundColor;
			newGraph.PreviewSettings.Tint = graph.PreviewSettings.Tint;

			// Convert the nodes.
			using var doc = JsonDocument.Parse( graphText );
			var nodesElement = doc.RootElement.GetProperty( "nodes" );
			var nodeArray = new JsonArray();

			foreach ( var node in nodesElement.EnumerateArray() )
			{
				var nodeObject = JsonNode.Parse( node.GetRawText() ) as JsonObject;

				if ( nodeObject != null )
				{
					SGPLog.Info( $"Found jsonObject : \n {nodeObject}" );

					if ( nodeObject["_class"]?.ToString() == "SubgraphNode" )
					{
						var subgraphpath = nodeObject["SubgraphPath"]?.ToString();
						var subgraphFullPath = $"{Project.Current.GetAssetsPath()}/{subgraphpath}".Replace("/","\\");

						SGPLog.Info( $"Found subgraph with graph at path : {subgraphFullPath}" );

						continue;
					}

					if ( nodeObject["_class"]?.ToString() == "Reroute" )
					{
						nodeObject["_class"] = "ReroutePlus";
					}

					nodeArray.Add( nodeObject );
				}
			}

			System.IO.File.WriteAllText( project.Path.Replace( ".shdrgrph", ".sgrph" ), newGraph.Serialize( nodeArray ) );
			
			AssetSystem.RegisterFile( newGraph.Path );
			Utilities.EdtiorSound.Success();

			projects[projects.IndexOf( project )].SetConverted();
		}

		dialog.SetItems( projects );
	}
}

internal class ProjectItem
{
	public bool Converted { get; private set; } = false;
	public string Path { get; private set; }

	public ProjectItem( string path )
	{
		Path = path;
	}

	public void SetConverted()
	{
		Converted = true;
	}
}

internal class ProjectList : Widget
{
	private List<ProjectItem> _projects;
	public List<ProjectItem> Projects
	{
		get
		{
			return _projects;
		}
		set
		{
			_projectListView.Clear();

			_projects = value;
			_projectListView.SetItems( _projects.Cast<object>() );
		}
	}

	private ProjectListView _projectListView;

	public void UpdateList( List<ProjectItem> items )
	{
		Projects = items;
	}

	public ProjectList( Widget parent ) : base( parent )
	{
		Name = "ShaderGraph Projects";
		WindowTitle = "ShaderGraph Projects";

		SetWindowIcon( "notes" );

		Layout = Layout.Column();

		_projectListView = new ProjectListView( this );
		Layout.Add( _projectListView );
	}

	class ProjectListView : ListView
	{
		private ProjectList _projectList;

		public ProjectListView( ProjectList parent ) : base( parent )
		{
			_projectList = parent;

			ItemClicked = ( item ) =>
			{
				SGPLog.Info( $"Clicked Item" );
			};

			ItemContextMenu = OpenItemContextMenu;
			ItemSize = new Vector2( 0, 48 );
			ItemSpacing = 0;
			Margin = 0;
		}

		private void OpenItemContextMenu( object item )
		{
		
		}

		protected override void OnPaint()
		{
			Paint.ClearPen();
			Paint.SetBrush( Theme.WindowBackground );
			Paint.DrawRect( LocalRect );
		
			base.OnPaint();
		}

		protected override void PaintItem( VirtualWidget item )
		{
			if ( item.Object is ProjectItem projectItem )
			{
				var color = Theme.Text;
				
				if ( projectItem.Converted )
				{
					color = Theme.Green;
				}
	
				Paint.SetBrush( color.WithAlpha( Paint.HasMouseOver ? 0.1f : 0.03f ) );
				Paint.ClearPen();
				Paint.DrawRect( item.Rect.Shrink( 0, -1 ) );

				Paint.Antialiasing = true;
				Paint.SetPen( color.WithAlpha( Paint.HasMouseOver ? 1 : 0.7f ), 3.0f );
				Paint.ClearBrush();

				var iconRect = item.Rect.Shrink( 12, 0 );
				iconRect.Width = 24;

				Paint.DrawIcon( iconRect, "article", 24 );

				var rect = item.Rect.Shrink( 48, 8, 0, 8 );
		
				Paint.SetPen( Color.White.WithAlpha( Paint.HasMouseOver ? 1 : 0.8f ), 3.0f );
				Paint.DrawText( rect, projectItem.Path, TextFlag.LeftCenter | TextFlag.SingleLine );
			}
		}
	}
}
