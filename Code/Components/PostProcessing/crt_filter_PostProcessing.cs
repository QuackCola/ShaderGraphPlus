

using Sandbox;
using System;

//[Title( "CRT Filter" )]
//[Category( "Post Processing" )]
//[Icon( "Camera" )]
//public sealed class crt_filter_PostProcess : Component, Component.ExecuteInEditor
//{
//	
//	[Property]
//	[Title("Resolution")]
//	public Vector2 vResolution { get; set; } = new Vector2(640f,480f);
//	
//	[Property]
//	[Title("Pixelate")]
//	public bool bPixelate { get; set; } = true;
//	
//
//    IDisposable renderHook;
//
//    protected override void OnEnabled()
//    {
//        renderHook?.Dispose();
//        var cc = Components.Get<CameraComponent>( true );
//        renderHook = cc.AddHookBeforeOverlay( "crt_filter_PostProcess", 500, RenderEffect );
//    }
//	
//    protected override void OnDisabled()
//    {
//        renderHook?.Dispose();
//        renderHook = null;
//    }
//
//    RenderAttributes attributes = new RenderAttributes();
//
//    public void RenderEffect( SceneCamera camera )
//    {
//        if ( !camera.EnablePostProcessing )
//            return;
//
//		// Set Shader attributes.
//		attributes.Set( "Resolution", vResolution );
//		attributes.Set( "Pixelate", bPixelate );
//		
//
//		// Set Shader Combos.
//		//attributes.SetCombo( "D_DIRECTIONAL", Directional );
//
//		Graphics.GrabFrameTexture( "ColorBuffer", attributes );
//        Graphics.GrabDepthTexture( "DepthBuffer", attributes );
//        Graphics.Blit( Material.FromShader("shaders/shadergraphplus/postprocessing/crt_filter.shader"), attributes );
//            
//    }
//}
