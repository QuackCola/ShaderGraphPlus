﻿using Editor.ShaderGraphPlus;

/// <summary>
/// 
/// </summary>
[Title( "Parallax Node Test" ), Category( "Dev" )]
public sealed class ParallaxNode : ShaderNodePlus
{
	[Hide]
	public static string SimpleParallax=> @"
float3 SimpleParallax(float flSlices, float flSliceDistance, float2 vUV, float3 vTangentViewDir, Texture2D vHeight, SamplerState vSampler)
{
	// flSlices Default is 25.0 
	// flSliceDistance Default is 0.15 

	float flHeightTex = vHeight.Sample( vSampler, vUV).x; 
						
	vTangentViewDir = normalize( vTangentViewDir.xyz );

	float3 vResult;

	[loop]
	for(int i = 0; i < flSlices; i++)
	{
		if(flHeightTex > 0.1)
		{
			vResult = float3(i,0,0);
			return vResult;
		}

		vUV.xy += (vTangentViewDir * flSliceDistance);
		flHeightTex = vHeight.Sample( vSampler, vUV.xy ).x;
	}

	// Raymarch Result
	return vResult;
}
";

	[Title( "Slice Count" )]
	[Description( "" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput SliceCount { get; set; }

	[Title( "Slice Distance" )]
	[Description( "" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput SliceDistance { get; set; }

	[Title( "Tangent View Direction" )]
	[Description( "" )]
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput TangentViewDir { get; set; }

	[Title( "Tex Object" )]
	[Description( "" )]
	[Input( typeof( TextureObject ) )]
	[Hide]
	public NodeInput TextureObject { get; set; }

	/// <summary>
	///
	/// </summary>
	[Title( "Sampler" )]
	[Input( typeof( Sampler ) )]
	[Hide]
	public NodeInput Sampler { get; set; }

	/// <summary>
	/// 
	/// </summary>
	[Title( "Coordinates" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	public ParallaxNode()
	{
		ExpandSize = new Vector2( 32, 0 );
	}

	public float DefaultSliceCount { get; set; } = 25.0f;
	public float DefaultSliceDistance { get; set; } = 0.15f;
	public Sampler DefaultSampler { get; set; } = new Sampler();

	[Title( "Stock Texture Filtering" )]
	[Description( "Toggle if you want to control the texture filtering in the Material Editor" )]
	public bool UseStockTextureFiltering { get; set; } = true;

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		if ( compiler.Graph.MaterialDomain is MaterialDomain.PostProcess )
		{
			return NodeResult.Error( $"{DisplayInfo.Name} Is not ment for postprocessing shaders!" );
		}

		var slicecount = compiler.ResultOrDefault( SliceCount, DefaultSliceCount );
		var slicedistance = compiler.ResultOrDefault( SliceDistance, DefaultSliceDistance );
		var coords = compiler.Result( Coords );
		var tangentviewdir = compiler.Result( TangentViewDir );
		var textureobject = compiler.Result( TextureObject );
		var sampler = compiler.ResultSamplerOrDefault( Sampler, DefaultSampler );


		if ( !textureobject.IsValid )
		{
			return NodeResult.MissingInput( nameof( TextureObject ) );
		}
		else if ( textureobject.ResultType is not ResultType.TextureObject )
		{
			return NodeResult.Error( $"Input to Tex Object is not a texture object!" );
		}

		if ( !tangentviewdir.IsValid() )
		{
			return NodeResult.MissingInput( nameof( TangentViewDir ) );
		}

		return new NodeResult( ResultType.Vector3, compiler.ResultFunction(  SimpleParallax ,
        args:
        $"{slicecount}" +
		$",{slicedistance}" +
        $",{(coords.IsValid ? $"{coords.Cast( 2 )}" : "i.vTextureCoords.xy")}" +
        $",{tangentviewdir}" +
        $",{textureobject}" +
        $",{(!UseStockTextureFiltering ? $"{sampler}" : "TextureFiltering" )}"
		) );
	};
}
