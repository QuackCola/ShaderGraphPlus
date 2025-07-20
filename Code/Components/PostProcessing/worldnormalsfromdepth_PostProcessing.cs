

using Sandbox;
using System;

//[Title( "DepthWorldNormals" )]
//[Category( "" )]
//[Icon( "Camera" )]
//public sealed class worldnormalsfromdepth_PostProcess : Component, Component.ExecuteInEditor
//{
//
//
//    IDisposable renderHook;
//
//    protected override void OnEnabled()
//    {
//        renderHook?.Dispose();
//        var cc = Components.Get<CameraComponent>( true );
//        renderHook = cc.AddHookBeforeOverlay( "worldnormalsfromdepth_PostProcess", 500, RenderEffect );
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
//
//
//		// Set Shader Combos.
//		//attributes.SetCombo( "D_DIRECTIONAL", Directional );
//
//		Graphics.GrabFrameTexture( "ColorBuffer", attributes );
//        Graphics.GrabDepthTexture( "DepthBuffer", attributes );
//        Graphics.Blit( Material.FromShader("shaders/worldnormalsfromdepth.shader"), attributes );
//            
//    }
//}
