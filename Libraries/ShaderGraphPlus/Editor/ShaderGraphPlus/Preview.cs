
using Sandbox;
using Sandbox.UI;

namespace Editor.ShaderGraphPlus;

public class PostProcessingPreview : SceneCustomObject
{

	private Material _material;

	public Material Material
	{
		set
		{
			_material = value;
		}
	}

	// On the attribute train!
	private Dictionary<string, Texture> _textureAttributes = new();
	public Dictionary<string, Texture> textureAttributes
	{
		get => _textureAttributes;
		set
		{
			_textureAttributes = value;
		}
	}

	private Dictionary<string, Color> _float4Attributes = new();
	public Dictionary<string, Color> float4Attributes
	{
		get => _float4Attributes;
		set
		{
			_float4Attributes = value;
		}
	}

	private Dictionary<string, Vector3> _float3Attributes = new();
	public Dictionary<string, Vector3> float3Attributes
	{
		get => _float3Attributes;
		set
		{
			_float3Attributes = value;
		}
	}

	private Dictionary<string, Vector2> _float2Attributes = new();
	public Dictionary<string, Vector2> float2Attributes
	{
		get => _float2Attributes;
		set
		{
			_float2Attributes = value;
		}
	}

	private Dictionary<string, float> _floatAttributes = new();
	public Dictionary<string, float> floatAttributes
	{
		get => _floatAttributes;
		set
		{
			_floatAttributes = value;
		}
	}

	private Dictionary<string, bool> _boolAttributes = new();
	public Dictionary<string, bool> boolAttributes
	{
		get => _boolAttributes;
		set
		{
			_boolAttributes = value;
		}
	}

	private bool _enabled;
	public bool Enabled
	{
		get => _enabled;
		set
		{
			_enabled = value;
		}
	}

	private Preview _preview;

	public PostProcessingPreview( SceneWorld sceneWorld, Preview preview ) : base( sceneWorld )
	{
		_preview = preview;
	}

	// hmmmm
	private void SetAttributes()
	{
		foreach ( var attrib in _textureAttributes )
		{
			Graphics.Attributes.Set( attrib.Key, attrib.Value );
		}

		foreach ( var attrib in _float4Attributes )
		{
			Graphics.Attributes.Set( attrib.Key, attrib.Value );
		}

		foreach ( var attrib in _float3Attributes )
		{
			Graphics.Attributes.Set( attrib.Key, attrib.Value );
		}

		foreach ( var attrib in _float2Attributes )
		{
			Graphics.Attributes.Set( attrib.Key, attrib.Value );
		}

		foreach ( var attrib in _floatAttributes )
		{
			Graphics.Attributes.Set( attrib.Key, attrib.Value );
		}

		foreach ( var attrib in _boolAttributes )
		{
			Graphics.Attributes.Set( attrib.Key, attrib.Value );
		}

		Graphics.Attributes.Set( "g_flPreviewTime", RealTime.Now );

	}

	public override void RenderSceneObject()
	{
		base.RenderSceneObject();

		if ( !_enabled )
			return;

		if ( _material is null )
		{
			Log.Error( "_material is NULL!!!" );
			return;
		}

		SetAttributes();

		Graphics.GrabFrameTexture( "ColorBuffer", Graphics.Attributes );
		Graphics.GrabDepthTexture( "DepthBuffer", Graphics.Attributes );
		Graphics.Blit( _material, Graphics.Attributes );
	}
}

public class Throbber : SceneCustomObject
{
	private readonly Texture _texture;

	private bool _enabled;
	private RealTimeSince _timeSinceDisabled;
	public bool Enabled
	{
		set
		{
			_enabled = value;

			if ( !value )
			{
				_timeSinceDisabled = 0;
			}
		}
	}

	private Preview _preview;

	public Throbber( SceneWorld sceneWorld, Preview preview ) : base( sceneWorld )
	{
		_preview = preview;
		_texture = Texture.Load( FileSystem.Content, "tools/images/common/busy.png", true );
		Bounds = BBox.FromPositionAndSize( Vector3.Zero, float.MaxValue );
	}

	public override void RenderSceneObject()
	{
		base.RenderSceneObject();

		if ( !_enabled && _timeSinceDisabled > 0.5f )
			return;

		var speed = 300;
		var delta = _enabled ? 0.0f : _timeSinceDisabled * 2.0f;
		var angle = RealTime.Now % (MathF.PI * (2.0f * speed));
		var dpiScale = _preview.DpiScale;

		var pos = new Vector2( _preview.Width - 39, 41 ) * dpiScale;
		Matrix mat = Matrix.CreateRotation( Rotation.From( 0, angle * speed, 0 ) );
		mat *= Matrix.CreateTranslation( pos );
		Graphics.Attributes.Set( "LayerMat", mat );

		Graphics.Attributes.Set( "Texture", _texture );
		Graphics.Attributes.SetComboEnum( "D_BLENDMODE", Sandbox.BlendMode.Normal );
		Graphics.DrawQuad( new Rect( -50, 100 ) * dpiScale, Material.UI.Basic, Color.Black.WithAlpha( 0.5f.LerpTo( 0.0f, delta ) ) );
		Graphics.Attributes.SetComboEnum( "D_BLENDMODE", Sandbox.BlendMode.Lighten );

		pos = new Vector2( _preview.Width - 40, 40 ) * dpiScale;
		mat = Matrix.CreateRotation( Rotation.From( 0, angle * speed, 0 ) );
		mat *= Matrix.CreateTranslation( pos );
		Graphics.Attributes.Set( "LayerMat", mat );
		Graphics.DrawQuad( new Rect( -50, 100 ) * dpiScale, Material.UI.Basic, _enabled ? Theme.White : Theme.White.WithAlpha( 1.0f.LerpTo( 0.0f, delta ) ) );
	}
}

public class PreviewPanel : Widget
{
	private readonly Preview _preview;
	public Preview Preview => _preview;
	private readonly ComboBox _animationCombo;

	public Model Model
	{
		get => _preview.Model;
		set
		{
			if ( Model == value )
				return;

			_preview.Model = value ?? _preview.SphereModel;

			UpdateAnimationCombo();

			OnModelChanged?.Invoke( value );
		}
	}

	public Material Material
	{
		set => _preview.Material = value;
	}

	public Material PostProcessingMaterial
	{
		set => _preview.PostProcessingMaterial = value;
	}

	public Color Tint
	{
		set => _preview.Tint = value;
	}

	private void UpdateAnimationCombo()
	{
		_animationCombo.Clear();

		var model = Model;
		var animationCount = Model.AnimationCount;

		if ( animationCount > 0 )
		{
			_animationCombo.Visible = true;
			_animationCombo.AddItem( "None", "animgraph_editor/single_frame_icon.png" );

			for ( int i = 0; i < model.AnimationCount; ++i )
			{
				_animationCombo.AddItem( model.GetAnimationName( i ), "animgraph_editor/single_frame_icon.png" );
			}
		}
		else
		{
			_animationCombo.Visible = false;
		}
	}

	public bool IsCompiling
	{
		set
		{
			_preview.IsCompiling = value;
		}
	}

	public bool IsPostProcessShader
	{
		set
		{
			_preview.IsPostProcessShader = value;
		}
	}

	public Action<Model> OnModelChanged { get; set; }

	public void SetAttribute( string id, in Float2x2 value ) // Stub - Quack
	{
		_preview.SetAttribute( id, value );
	}

	public void SetAttribute( string id, in Float3x3 value ) // Stub - Quack
	{
		_preview.SetAttribute( id, value );
	}
	public void SetAttribute( string id, in Float4x4 value ) // Stub - Quack
	{
		_preview.SetAttribute( id, value );
	}

	public void SetAttribute( string id, in Texture value )
	{
		_preview.SetAttribute( id, value );
	}

	public void SetAttribute( string id, in Color value )
	{
		_preview.SetAttribute( id, value );
	}

	public void SetAttribute( string id, in Vector3 value )
	{
		_preview.SetAttribute( id, value );
	}

	public void SetAttribute( string id, in Vector2 value )
	{
		_preview.SetAttribute( id, value );
	}

	public void SetAttribute( string id, in float value )
	{
		_preview.SetAttribute( id, value );
	}

	public void SetAttribute( string id, in bool value )
	{
		_preview.SetAttribute( id, value );
	}

	public void SetStage( int value )
	{
		_preview.SetStage( value );
	}

	public void ClearAttributes()
	{
		_preview.ClearAttributes();
	}

	public PreviewPanel( Widget parent, string model ) : base( parent )
	{
		Name = "Preview";
		WindowTitle = "Preview";
		SetWindowIcon( "photo" );

		_preview = new Preview( this, model );

		Layout = Layout.Column();

		var toolBar = new ToolBar( this, "PreviewToolBar" );
		toolBar.SetIconSize( 16 );
		toolBar.AddOption( null, "view_in_ar", () => Model = Model.Load( "models/dev/box.vmdl" ) ).ToolTip = "Box";
		toolBar.AddOption( null, "circle", () => Model = null ).ToolTip = "Sphere";
		toolBar.AddOption( null, "square", () => Model = Model.Load( "models/dev/plane.vmdl" ) ).ToolTip = "Plane";
		toolBar.AddOption( null, "accessibility", () =>
		{
			var picker = new AssetPicker( this, AssetType.Model );
			picker.Window.StateCookie = "PreviewPanel";
			picker.Window.RestoreFromStateCookie();
			picker.Window.Title = $"Select {AssetType.Model.FriendlyName}";
			picker.OnAssetHighlighted = x => Model = x.First().LoadResource<Model>();
			picker.OnAssetPicked = x => Model = x.First().LoadResource<Model>();
			picker.Window.Show();
		} ).ToolTip = "Model";

		toolBar.AddSeparator();

		var combo = new Widget( toolBar );
		combo.Layout = Layout.Row();
		_animationCombo = new ComboBox( combo );
		combo.Layout.Add( _animationCombo, 1 );
		toolBar.AddWidget( combo );

		UpdateAnimationCombo();

		_animationCombo.ItemChanged += () =>
		{
			if ( _animationCombo.CurrentIndex == 0 )
			{
				_preview.UseAnimGraph = true;
			}
			else
			{
				_preview.UseAnimGraph = false;
				_preview.CurrentSequence = _animationCombo.CurrentText;
			}
		};

		var stretcher = new Widget( toolBar );
		stretcher.Layout = Layout.Row();
		stretcher.Layout.AddStretchCell( 1 );
		toolBar.AddWidget( stretcher );

		var option = toolBar.AddOption( null, "preview" );
		option.Checkable = true;
		option.Toggled = ( e ) => _preview.EnableNodePreview = e;
		option.ToolTip = "Toggle Node Preview";
		option.StatusTip = "Toggle Node Preview";

		option = toolBar.AddOption( null, "flare" );
		option.Checkable = true;
		option.Toggled = ( e ) => _preview.EnablePostProcessing = e;
		option.ToolTip = "Toggle Post Processing";
		option.StatusTip = "Toggle Post Processing";

		toolBar.AddSeparator();

		option = toolBar.AddOption( null, "lightbulb" );
		option.Enabled = false;
		option.ToolTip = "Coming Soon";

		toolBar.AddSeparator();

		option = toolBar.AddOption( null, "settings", OpenSettings );

		Layout.Add( toolBar );
		Layout.Add( _preview );
	}

	public void OpenSettings()
	{
		var popup = new PopupWidget( this );
		popup.IsPopup = true;
		popup.Layout = Layout.Column();
		popup.Layout.Margin = 16;

		var ps = new PropertySheet( popup );

		ps.AddSectionHeader( "Render Options" );
		{
			var w = ps.AddRow( "Render Backfaces", new Checkbox( "", this ) );
			w.Value = _preview.RenderBackfaces;
			w.Bind( "Value" ).From( () => _preview.RenderBackfaces, x => _preview.RenderBackfaces = x );
		}
		{
			var w = ps.AddRow( "Enable Shadows", new Checkbox( "", this ) );
			w.Value = _preview.EnableShadows;
			w.Bind( "Value" ).From( () => _preview.EnableShadows, x => _preview.EnableShadows = x );
		}
		{
			var w = ps.AddRow( "Show Ground", new Checkbox( "", this ) );
			w.Value = _preview.ShowGround;
			w.Bind( "Value" ).From( () => _preview.ShowGround, x => _preview.ShowGround = x );
		}
		{
			var showSkybox = ps.AddRow( "Show Skybox", new Checkbox( "", this ) );
			var backgroundColor = ps.AddRow( "Background Color", new ColorProperty( this ) );
			showSkybox.Value = _preview.ShowSkybox;
			backgroundColor.Value = _preview.Camera.BackgroundColor;
			backgroundColor.Enabled = !_preview.ShowSkybox;
			showSkybox.Bind( "Value" ).From( () => _preview.ShowSkybox, x =>
			{
				_preview.ShowSkybox = x;
				backgroundColor.Enabled = !x;
			} );

			backgroundColor.Bind( "Value" ).From( () => _preview.Camera.BackgroundColor, x => _preview.Camera.BackgroundColor = x );
		}
		{
			var tintColor = ps.AddRow( "Tint Color", new ColorProperty( this ) );
			tintColor.Value = _preview.Tint;
			tintColor.Bind( "Value" ).From( () => _preview.Tint, x => _preview.Tint = x );
		}

		popup.Layout.Add( ps );
		popup.MinimumWidth = 300;
		popup.OpenAtCursor();
	}
}

public class Preview : NativeRenderingWidget
{
	private SceneWorld _world;
	private SceneCamera _camera;

	private Vector2 _lastCursorPos;
	private Vector2 _cursorDelta;
	private Vector2 _angles;
	private Vector3 _origin;
	private float _distance;
	private float _modelRotation;
	private bool _orbitControl;
	private bool _orbitLights;
	private bool _zoomControl;
	private bool _panControl;

	private SceneModel _sceneObject;
	private Throbber _thobber;
	private PostProcessingPreview _postprocessingpreview;

	private Dictionary<string, Texture> _textureAttributes = new();
	private Dictionary<string, Float2x2> _float2x2Attributes = new();
	private Dictionary<string, Float3x3> _float3x3Attributes = new();
	private Dictionary<string, Float4x4> _float4x4Attributes = new();
	private Dictionary<string, Color> _float4Attributes = new();
	private Dictionary<string, Vector3> _float3Attributes = new();
	private Dictionary<string, Vector2> _float2Attributes = new();
	private Dictionary<string, float> _floatAttributes = new();
	private Dictionary<string, bool> _boolAttributes = new();
	private Dictionary<string, object> _postprocessingAttributes = new();
	private int _stageId;

	public bool EnablePostProcessing
	{
		get => _camera.EnablePostProcessing;
		set => _camera.EnablePostProcessing = value;
	}

	private bool _enableNodePreview;
	public bool EnableNodePreview
	{
		get => _enableNodePreview;
		set
		{
			_enableNodePreview = value;

			if ( _sceneObject.IsValid() )
			{
				_sceneObject.Attributes.Set( "g_iStageId", _enableNodePreview ? _stageId : 0 );
			}
		}
	}

	private bool _enableShadows = true;
	public bool EnableShadows
	{
		get => _enableShadows;
		set
		{
			_enableShadows = value;
			_camera.Attributes.Set( "enableShadows", _enableShadows );
		}
	}

	public bool ShowSkybox
	{
		get => _skybox.RenderingEnabled;
		set => _skybox.RenderingEnabled = value;
	}

	public bool ShowGround
	{
		get => _ground.RenderingEnabled;
		set => _ground.RenderingEnabled = value;
	}

	private bool _renderBackfaces;
	public bool RenderBackfaces
	{
		get => _renderBackfaces;
		set
		{
			_renderBackfaces = value;

			if ( _sceneObject.IsValid() )
			{
				_sceneObject.Attributes.SetCombo( "D_RENDER_BACKFACES", _renderBackfaces );
			}
		}
	}

	private float _tonemapScalar = 1.0f;
	public float TonemapScalar
	{
		get => _tonemapScalar;
		set
		{
			_tonemapScalar = value;
			_camera.Attributes.Set( "tonemapScalar", _tonemapScalar );
		}
	}

	public void UpdateMaterial()
	{
		_sceneObject.GetType().GetMethod( "SetMaterialOverrideForMeshInstances",
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )?.Invoke( _sceneObject, new[] { _material } );

		//_postprocessingpreview.GetType().GetMethod( "SetMaterialOverrideForMeshInstances",
		//BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )?.Invoke( _postprocessingpreview, new[] { _PostProcessingMaterial } );
	}

	private Material _material;
	public Material Material
	{
		get => _material;
		set
		{
			_material = value;
			UpdateMaterial();
		}
	}

	private Material _PostProcessingMaterial;
	public Material PostProcessingMaterial
	{
		get => _PostProcessingMaterial;
		set
		{
			_PostProcessingMaterial = value;
		}
	}

	private Color _tint = Color.White;
	public Color Tint
	{
		get => _tint;
		set
		{
			_tint = value;
			if ( _sceneObject.IsValid() )
				_sceneObject.ColorTint = _tint;
		}
	}

	private Model _model;
	public Model Model
	{
		get => _model;
		set
		{
			_model = value;

			if ( _sceneObject.IsValid() )
			{
				_sceneObject.RenderingEnabled = false;
				_sceneObject.Delete();
			}

			_sceneObject = new SceneModel( _world, value, Transform.Zero )
			{
				ColorTint = Tint,
				Batchable = false
			};

			_sceneObject.Update( 1 );

			UpdateMaterial();

			foreach ( var texture in _textureAttributes )
			{
				_sceneObject.Attributes.Set( texture.Key, texture.Value );
			}

			foreach ( var v in _float4Attributes )
			{
				_sceneObject.Attributes.Set( v.Key, v.Value );
			}

			foreach ( var v in _float3Attributes )
			{
				_sceneObject.Attributes.Set( v.Key, v.Value );
			}

			foreach ( var v in _float2Attributes )
			{
				_sceneObject.Attributes.Set( v.Key, v.Value );
			}

			foreach ( var v in _floatAttributes )
			{
				_sceneObject.Attributes.Set( v.Key, v.Value );
			}

			foreach ( var v in _boolAttributes )
			{
				_sceneObject.Attributes.Set( v.Key, v.Value );
			}

			if ( _enableNodePreview )
			{
				_sceneObject.Attributes.Set( "g_iStageId", _stageId );
			}

			_sceneObject.Attributes.SetCombo( "D_RENDER_BACKFACES", _renderBackfaces );
		}
	}

	public string CurrentSequence
	{
		set => _sceneObject.CurrentSequence.Name = value;
	}

	public bool UseAnimGraph
	{
		set => _sceneObject.UseAnimGraph = value;
	}

	//private readonly Dictionary<string, Float2x2> _float2x2Attributes = new();
	//private readonly Dictionary<string, Float3x3> _float3x3Attributes = new();
	//private readonly Dictionary<string, Float4x4> _float4x4Attributes = new();

	public void SetAttribute( string id, Float2x2 value ) // Stub - Quack
	{
		_float2x2Attributes.Add( id, value );
		if ( _postprocessingpreview.Enabled )
		{
			//_postprocessingpreview.textureAttributes.Add( id, value );
		}
		_sceneObject.Attributes.SetData( id, value );
	}

	public void SetAttribute( string id, Float3x3 value ) // Stub - Quack
	{
		_float3x3Attributes.Add( id, value );
		if ( _postprocessingpreview.Enabled )
		{
			//_postprocessingpreview.textureAttributes.Add( id, value );
		}
		_sceneObject.Attributes.SetData( id, value );
	}

	public void SetAttribute( string id, Float4x4 value ) // Stub - Quack
	{
		_float4x4Attributes.Add( id, value );
		if ( _postprocessingpreview.Enabled )
		{
			//_postprocessingpreview.textureAttributes.Add( id, value );
		}
		_sceneObject.Attributes.SetData( id, value );
	}

	public void SetAttribute( string id, Texture value )
	{
		_textureAttributes.Add( id, value );
		if ( _postprocessingpreview.Enabled )
		{
			_postprocessingpreview.textureAttributes.Add( id, value );
		}
		_sceneObject.Attributes.Set( id, value );
	}

	public void SetAttribute( string id, Color value )
	{
		_float4Attributes.Add( id, value );
		if ( _postprocessingpreview.Enabled )
		{
			_postprocessingpreview.float4Attributes.Add( id, value );
		}
		_sceneObject.Attributes.Set( id, value );
	}

	public void SetAttribute( string id, Vector3 value )
	{
		_float3Attributes.Add( id, value );
		if ( _postprocessingpreview.Enabled )
		{
			_postprocessingpreview.float3Attributes.Add( id, value );
		}
		_sceneObject.Attributes.Set( id, value );
	}

	public void SetAttribute( string id, Vector2 value )
	{
		_float2Attributes.Add( id, value );
		if ( _postprocessingpreview.Enabled )
		{
			_postprocessingpreview.float2Attributes.Add( id, value );
		}
		_sceneObject.Attributes.Set( id, value );
	}

	public void SetAttribute( string id, float value )
	{
		_floatAttributes.Add( id, value );
		if ( _postprocessingpreview.Enabled )
		{
			_postprocessingpreview.floatAttributes.Add( id, value );
		}
		_sceneObject.Attributes.Set( id, value );
	}

	public void SetAttribute( string id, in bool value )
	{
		_boolAttributes.Add( id, value );
		if ( _postprocessingpreview.Enabled )
		{
			_postprocessingpreview.boolAttributes.Add( id, value );
		}
		_sceneObject.Attributes.Set( id, value );
	}

	public void SetStage( int value )
	{
		_stageId = value;

		if ( _sceneObject.IsValid() )
		{
			_sceneObject.Attributes.Set( "g_iStageId", _enableNodePreview ? _stageId : 0 );
		}
	}

	public void ClearAttributes()
	{
		_textureAttributes.Clear();
		_float2x2Attributes.Clear();
		_float3x3Attributes.Clear();
		_float4x4Attributes.Clear();
		_float4Attributes.Clear();
		_float3Attributes.Clear();
		_float2Attributes.Clear();
		_floatAttributes.Clear();
		_boolAttributes.Clear();


		if ( _postprocessingpreview.Enabled )
		{
			_postprocessingpreview.textureAttributes.Clear();
			_postprocessingpreview.float4Attributes.Clear();
			_postprocessingpreview.float3Attributes.Clear();
			_postprocessingpreview.float2Attributes.Clear();
			_postprocessingpreview.floatAttributes.Clear();
			_postprocessingpreview.boolAttributes.Clear();
		}

		if ( _sceneObject.IsValid() )
		{
			_sceneObject.Attributes.Clear();
		}

		if ( _postprocessingpreview.IsValid() )
		{
		
		}
	}

	public bool IsCompiling
	{
		set
		{
			_thobber.Enabled = value;
		}
	}

	public bool IsPostProcessShader
	{
		set
		{
			_postprocessingpreview.Enabled = value;
			_postprocessingpreview.Material = _PostProcessingMaterial;
		}
	}

	public Model SphereModel { get; set; }
	public Model GroundModel { get; set; }

	private SceneSkyBox _skybox;
	private SceneObject _ground;

	public Preview( Widget parent, string model ) : base( parent )
	{
		MouseTracking = true;
		FocusMode = FocusMode.Click;

		_world = new SceneWorld();
		_camera = new SceneCamera
		{
			World = _world,
			AmbientLightColor = Color.Gray,
			ZNear = 0.1f,
			ZFar = 4000,
			EnablePostProcessing = true,
		};

		Camera = _camera;

		_distance = 150.0f;
		_angles = new Vector2( 45 * 3, 30 );
		_camera.Angles = new Angles( _angles.y, -_angles.x, 0 );
		_camera.Position = _camera.Rotation.Backward * _distance;
		_camera.FieldOfView = 45;
		_camera.AntiAliasing = true;
		_camera.BackgroundColor = Color.White;

		SphereModel = Model.Builder
			.AddMesh( CreateTessellatedSphere( 64, 64, 4.0f, 4.0f, 32.0f ) )
			.Create();

		GroundModel = Model.Builder
			.AddMesh( CreatePlane() )
			.Create();

		_skybox = new SceneSkyBox( _world, Material.Load( "materials/skybox/skybox_studio_01.vmat" ) );
		new SceneCubemap( _world, Texture.Load( "materials/skybox/skybox_studio_01.vtex" ), BBox.FromPositionAndSize( Vector3.Zero, 1000 ) );
		new SceneDirectionalLight( _world, Rotation.FromPitch( 50 ), Color.White * 2.5f + Color.Cyan * 0.05f );

		new SceneLight( _world, new Vector3( 100, 100, 100 ), 500, Color.Orange * 3 )
		{
			ShadowsEnabled = true
		};

		new SceneLight( _world, new Vector3( -100, -100, 100 ), 500, Color.Cyan * 3 )
		{
			ShadowsEnabled = true
		};

		_thobber = new Throbber( _world, this );
		_postprocessingpreview = new PostProcessingPreview( _world, this );

		_PostProcessingMaterial = Material.Load( "materials/core/ShaderGraphPlus/shader_editor_postprocess.vmat" );
		_material = Material.Load( "materials/core/shader_editor.vmat" );

		Model = string.IsNullOrWhiteSpace( model ) ? SphereModel : Model.Load( model );

		_ground = new SceneObject( _world, GroundModel );
		_ground.RenderingEnabled = false;
	}

	// Application.CursorPosition is fucked for different DPI
	private static Vector2 CursorPosition => Application.UnscaledCursorPosition;

	public override void PreFrame()
	{
		var cursorPos = CursorPosition;
		_cursorDelta = cursorPos - _lastCursorPos;

		if ( _orbitControl )
		{
			if ( _cursorDelta.Length > 0.0f )
			{
				_angles.x += _cursorDelta.x * 0.2f;

				if ( !_orbitLights )
					_angles.y += _cursorDelta.y * 0.2f;

				_angles.y = _angles.y.Clamp( -90, 90 );
				_angles.x = _angles.x.NormalizeDegrees();

				if ( _orbitLights )
				{
					_modelRotation -= _cursorDelta.x * 0.2f;
				}
			}

			Application.UnscaledCursorPosition = _lastCursorPos;
			Cursor = CursorShape.Blank;
		}
		else if ( _zoomControl )
		{
			if ( Math.Abs( _cursorDelta.y ) > 0.0f )
			{
				Zoom( _cursorDelta.y );
			}

			Application.UnscaledCursorPosition = _lastCursorPos;
			Cursor = CursorShape.Blank;
		}
		else if ( _panControl )
		{
			if ( _cursorDelta.Length > 0.0f )
			{
				var right = _camera.Rotation.Right * _cursorDelta.x * 0.2f;
				var down = _camera.Rotation.Down * _cursorDelta.y * 0.2f;
				var invRot = Rotation.FromYaw( _modelRotation ).Inverse;
				_origin += right * invRot;
				_origin += down * invRot;
			}

			Application.UnscaledCursorPosition = _lastCursorPos;
			Cursor = CursorShape.Blank;
		}
		else
		{
			_lastCursorPos = cursorPos;
			Cursor = CursorShape.None;
		}

		_sceneObject.ColorTint = Tint;
		_sceneObject.Rotation = Rotation.FromYaw( _modelRotation );
		_sceneObject.Update( RealTime.Delta );
		_sceneObject.Attributes.Set( "g_flPreviewTime", RealTime.Now );

		_ground.Position = Vector3.Up * (_model.RenderBounds.Mins.z - 0.1f);

		_camera.Angles = new Angles( _angles.y, -_angles.x, 0 );
		_camera.Position = (_origin + _model.RenderBounds.Center) * _sceneObject.Rotation + _camera.Rotation.Backward * _distance;
	}

	protected override void OnKeyPress( KeyEvent e )
	{
		base.OnKeyPress( e );

		if ( e.Key == KeyCode.Control )
		{
			_orbitLights = true;
		}
	}

	protected override void OnKeyRelease( KeyEvent e )
	{
		base.OnKeyRelease( e );

		if ( e.Key == KeyCode.Control )
		{
			_orbitLights = false;
		}
	}

	protected override void OnMousePress( MouseEvent e )
	{
		base.OnMousePress( e );

		_orbitLights = e.HasCtrl;

		if ( e.LeftMouseButton )
		{
			_orbitControl = true;
			_lastCursorPos = CursorPosition;
			_modelRotation = _sceneObject.Rotation.Yaw();
		}
		else if ( e.RightMouseButton )
		{
			_zoomControl = true;
			_lastCursorPos = CursorPosition;
		}
		else if ( e.MiddleMouseButton )
		{
			_panControl = true;
			_lastCursorPos = CursorPosition;
		}
	}

	protected override void OnMouseReleased( MouseEvent e )
	{
		base.OnMouseReleased( e );

		if ( e.LeftMouseButton )
		{
			_orbitControl = false;
			_orbitLights = false;
		}
		else if ( e.RightMouseButton )
		{
			_zoomControl = false;
		}
		else if ( e.MiddleMouseButton )
		{
			_panControl = false;
		}
	}

	protected override void OnWheel( WheelEvent e )
	{
		base.OnWheel( e );

		Zoom( e.Delta * -0.1f );
	}

	private void Zoom( float delta )
	{
		_distance += delta;
		_distance = _distance.Clamp( 0, 2000 );
	}

	public override void OnDestroyed()
	{
		base.OnDestroyed();

		_world?.Delete();
		_world = null;
	}

	static Mesh CreatePlane()
	{
		var material = Material.Load( "materials/dev/gray_grid_8.vmat" );
		var mesh = new Mesh( material );
		mesh.CreateVertexBuffer<Vertex>( 4, Vertex.Layout, new[]
		{
			new Vertex( new Vector3( -200, -200, 0 ), Vector3.Up, Vector3.Forward, new Vector4( 0, 0, 0, 0 ) ),
			new Vertex( new Vector3( 200, -200, 0 ), Vector3.Up, Vector3.Forward, new Vector4( 2, 0, 0, 0 ) ),
			new Vertex( new Vector3( 200, 200, 0 ), Vector3.Up, Vector3.Forward, new Vector4( 2, 2, 0, 0 ) ),
			new Vertex( new Vector3( -200, 200, 0 ), Vector3.Up, Vector3.Forward, new Vector4( 0, 2, 0, 0 ) ),
		} );
		mesh.CreateIndexBuffer( 6, new[] { 0, 1, 2, 2, 3, 0 } );
		mesh.Bounds = BBox.FromPositionAndSize( 0, 100 );

		return mesh;
	}

	// We could do with a nice geometry API but this is tools code so fuck it!
	static Mesh CreateTessellatedSphere( int uFacets, int vFacets, float maxU, float maxV, float radius )
	{
		float dU = 1.0f / uFacets;
		float dV = 1.0f / vFacets;

		var material = Material.Load( "materials/core/shader_editor.vmat" );
		var mesh = new Mesh( material );
		mesh.CreateVertexBuffer<Vertex>( (uFacets + 1) * (vFacets + 1), Vertex.Layout );
		mesh.CreateIndexBuffer( 2 * 3 * uFacets * vFacets );
		mesh.Bounds = BBox.FromPositionAndSize( 0, radius * 2 );

		mesh.LockVertexBuffer<Vertex>( ( vertices ) =>
		{
			float v = 0.5f;
			int i = 0;

			for ( int nV = 0; nV < (vFacets + 1); nV++ )
			{
				float u = 0.0f;

				for ( int nU = 0; nU < (uFacets + 1); nU++ )
				{
					float sinTheta = MathF.Sin( u * MathF.PI );
					float cosTheta = MathF.Cos( u * MathF.PI );
					float sinPhi = MathF.Sin( v * 2.0f * MathF.PI );
					float cosPhi = MathF.Cos( v * 2.0f * MathF.PI );

					var vertex = new Vertex();
					vertex.Position = radius * new Vector3( sinTheta * cosPhi, sinTheta * sinPhi, cosTheta );
					vertex.Normal = new Vector3( sinTheta * cosPhi, sinTheta * sinPhi, cosTheta ).Normal;
					vertex.Tangent = new Vector4( new Vector3( -sinPhi, cosPhi, 0.0f ).Normal, -1.0f );
					vertex.TexCoord0 = new Vector2( (v - 0.5f) * maxV, u * maxU );
					vertex.TexCoord1 = vertex.TexCoord0 * -1.0f;
					vertex.Color = Color.Lerp( Color.Red, Color.Green, (vertex.Position.z + radius) / (2 * radius) );

					vertices[i++] = vertex;

					u += dU;
				}

				v += dV;
			}
		} );

		mesh.LockIndexBuffer( ( indices ) =>
		{
			int i = 0;

			for ( int v = 0; v < vFacets; v++ )
			{
				for ( int u = 0; u < uFacets; u++ )
				{
					indices[i++] = v * (uFacets + 1) + u;
					indices[i++] = v * (uFacets + 1) + (u + 1);
					indices[i++] = (v + 1) * (uFacets + 1) + u;
					indices[i++] = v * (uFacets + 1) + (u + 1);
					indices[i++] = (v + 1) * (uFacets + 1) + (u + 1);
					indices[i++] = (v + 1) * (uFacets + 1) + u;
				}
			}
		} );

		return mesh;
	}
}
