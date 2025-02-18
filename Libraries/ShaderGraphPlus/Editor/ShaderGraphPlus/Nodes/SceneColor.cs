namespace Editor.ShaderGraphPlus.Nodes;

/// <summary>
/// Color of the scene
/// </summary>
[Title( "Scene Color" ), Category( "Variables" )]
public sealed class SceneColorNode : ShaderNodePlus
{
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput UV { get; set; }

	public bool UseScreenUVs { get; set; } = true;

	//public bool MultiplyWithFrameBufferCopyInvSizeAndUvScale { get; set; } = false;

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func SceneColor => ( GraphCompiler compiler ) =>
	{
		var coords = compiler.Result( UV );
		
		var graph = compiler.Graph;

		if ( graph.MaterialDomain != MaterialDomain.PostProcess && graph.BlendMode != BlendMode.Translucent )
		{
			return NodeResult.Error($"Graph `{nameof( BlendMode )}` must be set to `{nameof( BlendMode.Translucent )}` in order to use `{DisplayInfo.Name}`");
		}

		var uv = ( UseScreenUVs ? $"CalculateViewportUv( i.vPositionSs.xy )" : $"i.vTextureCoords.xy");


		if ( graph.MaterialDomain is MaterialDomain.PostProcess )
		{
			compiler.RegisterGlobal( "Texture2D g_tColorBuffer < Attribute( \"ColorBuffer\" ); SrgbRead( true ); >;" );

		    return new NodeResult( ResultType.Vector3, $"g_tColorBuffer.Sample( g_sAniso ,{(coords.IsValid ? $"{coords.Cast(2)}" : "CalculateViewportUv( i.vPositionSs.xy )")} )");
		}
		else
		{
            compiler.RegisterGlobal( "BoolAttribute( bWantsFBCopyTexture, true );" );
            compiler.RegisterGlobal( "Texture2D g_tFrameBufferCopyTexture < Attribute( \"FrameBufferCopyTexture\" ); SrgbRead( false ); >;" );

            if ( compiler.IsPreview )
			{
			    return new NodeResult( ResultType.Vector3, $"g_tFrameBufferCopyTexture.Sample( g_sAniso ,{(coords.IsValid ? $"{coords.Cast(2)}" : $"{uv}" )} * g_vFrameBufferCopyInvSizeAndUvScale.zw )" );
			}
			else
			{
				return new NodeResult( ResultType.Vector3, $"g_tFrameBufferCopyTexture.Sample( g_sAniso ,{(coords.IsValid ? $"{coords.Cast(2)}" : $"{uv}")})" );
			}	
			
			//return new NodeResult(ResultType.Vector3, $"g_tFrameBufferCopyTexture.Sample( g_sAniso ,{(coords.IsValid ? $"{coords.Cast(2)}" : $"{uv}")} {(MultiplyWithFrameBufferCopyInvSizeAndUvScale ? "* g_vFrameBufferCopyInvSizeAndUvScale.zw" : "")} )");
		}
    };
}

[Title( "Frame Buffer Copy Inv Size And Uv Scale" ), Category( "Variables" )]
public sealed class FrameBufferCopyInvSizeAndUvScaleNode : ShaderNodePlus
{
	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func X => (GraphCompiler compiler) =>
	{
	    return new NodeResult( ResultType.Float, $"g_vFrameBufferCopyInvSizeAndUvScale.x" );
	};
	
	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Y => (GraphCompiler compiler) =>
	{
	    return new NodeResult( ResultType.Float, $"g_vFrameBufferCopyInvSizeAndUvScale.y" );
	};
	
	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Z => (GraphCompiler compiler) =>
	{
	    return new NodeResult( ResultType.Float, $"g_vFrameBufferCopyInvSizeAndUvScale.z" );
	};
	
	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func W => ( GraphCompiler compiler ) =>
	{
	    return new NodeResult( ResultType.Float, $"g_vFrameBufferCopyInvSizeAndUvScale.w" );
	};

}