
namespace Editor.ShaderGraphPlus;

/// <summary>
/// Final result
/// </summary>
[Title( "Material" ), Icon( "tonality" )]
public sealed class Result : BaseResult
{
    [Hide]
    private bool IsLit => (Graph is ShaderGraphPlus shaderGraph && shaderGraph.ShadingModel == ShadingModel.Lit && shaderGraph.MaterialDomain != MaterialDomain.PostProcess);

    [Hide]
	[Input( typeof( Vector3 ) )]
	public NodeInput Albedo { get; set; }

	[Hide]
	[Input( typeof( Vector3 ) )]
	[ShowIf( nameof( this.IsLit ), true )]
	public NodeInput Emission { get; set; }

	[Hide, Editor( nameof( DefaultOpacity ) )]
	[Input( typeof( float ) )]
	public NodeInput Opacity { get; set; }

	// This does fuck all?
	//[Hide, Editor( nameof( DefaultTintMask ) )]
	//[Input( typeof( float ) )]
	//public NodeInput TintMask { get; set; }

	[Hide]
	[Input( typeof( Vector3 ) )]
	[ShowIf( nameof( this.IsLit ), true )]
	public NodeInput Normal { get; set; }

	[Hide, Editor( nameof( DefaultRoughness ) )]
	[Input( typeof( float ) )]
	[ShowIf( nameof( this.IsLit ), true )]
	public NodeInput Roughness { get; set; }

	[Hide, Editor( nameof( DefaultMetalness ) )]
	[Input( typeof( float ) )]
	[ShowIf( nameof( this.IsLit ), true )]
	public NodeInput Metalness { get; set; }

	[Hide, Editor( nameof( DefaultAmbientOcclusion ) )]
	[Input( typeof( float ) )]
	[ShowIf( nameof( this.IsLit ), true )]
	public NodeInput AmbientOcclusion { get; set; }

	[InputDefault( nameof( Opacity ) )]
	public float DefaultOpacity { get; set; } = 1.0f;
	//public float DefaultTintMask { get; set; } = 1.0f;
	[InputDefault( nameof( Roughness ) )]
	public float DefaultRoughness { get; set; } = 1.0f;
	[InputDefault( nameof( Metalness ) )]
	public float DefaultMetalness { get; set; } = 0.0f;
	[InputDefault( nameof( AmbientOcclusion ) )]
	public float DefaultAmbientOcclusion { get; set; } = 1.0f;

	[Hide]
	[Input( typeof( Vector3 ) )]
	public NodeInput PositionOffset { get; set; }

	[JsonIgnore, Hide]
	public override Color PrimaryColor => Color.Lerp( Theme.Blue, Theme.White, 0.25f );


    public override NodeInput GetAlbedo() => Albedo;
    public override NodeInput GetEmission() => Emission;
    public override NodeInput GetOpacity() => Opacity;
    public override NodeInput GetNormal() => Normal;
    public override NodeInput GetRoughness() => Roughness;
    public override NodeInput GetMetalness() => Metalness;
    public override NodeInput GetAmbientOcclusion() => AmbientOcclusion;
    public override NodeInput GetPositionOffset() => PositionOffset;
}


public abstract class BaseResult : ShaderNodePlus
{
	public virtual NodeInput GetAlbedo() => new();
	public virtual NodeInput GetEmission() => new();
	public virtual NodeInput GetOpacity() => new();
	public virtual NodeInput GetNormal() => new();
	public virtual NodeInput GetRoughness() => new();
	public virtual NodeInput GetMetalness() => new();
	public virtual NodeInput GetAmbientOcclusion() => new();
	public virtual NodeInput GetPositionOffset() => new();

	public virtual Color GetDefaultAlbedo() => Color.White;
	public virtual Color GetDefaultEmission() => Color.Black;
	public virtual float GetDefaultOpacity() => 1.0f;
	public virtual Vector3 GetDefaultNormal() => new( 0, 0, 0 );
	public virtual float GetDefaultRoughness() => 1.0f;
	public virtual float GetDefaultMetalness() => 0.0f;
	public virtual float GetDefaultAmbientOcclusion() => 1.0f;
	public virtual Vector3 GetDefaultPositionOffset() => new( 0, 0, 0 );

	public NodeResult GetAlbedoResult( GraphCompiler compiler )
	{
		var albedoInput = GetAlbedo();
		if ( albedoInput.IsValid )
			return compiler.ResultValue( albedoInput );
		return compiler.ResultValue( GetDefaultAlbedo() );
	}

	public NodeResult GetEmissionResult( GraphCompiler compiler )
	{
		var emissionInput = GetEmission();
		if ( emissionInput.IsValid )
			return compiler.ResultValue( emissionInput );
		return compiler.ResultValue( GetDefaultEmission() );
	}

	public NodeResult GetOpacityResult( GraphCompiler compiler )
	{
		var opacityInput = GetOpacity();
		if ( opacityInput.IsValid )
			return compiler.ResultValue( opacityInput );
		return compiler.ResultValue( GetDefaultOpacity() );
	}

	public NodeResult GetNormalResult( GraphCompiler compiler )
	{
		var normalInput = GetNormal();
		if ( normalInput.IsValid )
			return compiler.ResultValue( normalInput );
		return compiler.ResultValue( GetDefaultNormal() );
	}

	public NodeResult GetRoughnessResult( GraphCompiler compiler )
	{
		var roughnessInput = GetRoughness();
		if ( roughnessInput.IsValid )
			return compiler.ResultValue( roughnessInput );
		return compiler.ResultValue( GetDefaultRoughness() );
	}

	public NodeResult GetMetalnessResult( GraphCompiler compiler )
	{
		var metalnessInput = GetMetalness();
		if ( metalnessInput.IsValid )
			return compiler.ResultValue( metalnessInput );
		return compiler.ResultValue( GetDefaultMetalness() );
	}

	public NodeResult GetAmbientOcclusionResult( GraphCompiler compiler )
	{
		var ambientOcclusionInput = GetAmbientOcclusion();
		if ( ambientOcclusionInput.IsValid )
			return compiler.ResultValue( ambientOcclusionInput );
		return compiler.ResultValue( GetDefaultAmbientOcclusion() );
	}

	public NodeResult GetPositionOffsetResult( GraphCompiler compiler )
	{
		var positionOffsetInput = GetPositionOffset();
		if ( positionOffsetInput.IsValid )
			return compiler.ResultValue( positionOffsetInput );
		return compiler.ResultValue( GetDefaultPositionOffset() );
	}
}