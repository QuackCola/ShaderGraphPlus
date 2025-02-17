namespace Editor.ShaderGraphPlus;

public static class ShaderTemplateBlending
{
    public static string Code => @"
HEADER
{{
	Description = ""{0}"";
}}

FEATURES
{{
	#include ""common/features.hlsl""
{1}
}}

MODES
{{
	Forward();
	Depth(); 
	ToolsShadingComplexity( ""tools_shading_complexity.shader"" );
}}

COMMON
{{
{2}
	#include ""common/shared.hlsl""
	#include ""common/gradient.hlsl""
	#include ""procedural.hlsl""
	
	#define S_UV2 1
	#define CUSTOM_MATERIAL_INPUTS
}}

struct VertexInput
{{
	float4 vColorBlendValues : TEXCOORD4 < Semantic( VertexPaintBlendParams ); >;
	float4 vColorPaintValues : TEXCOORD5 < Semantic( VertexPaintTintColor ); >;
	float4 vColor : COLOR0 < Semantic( Color ); >;
	#include ""common/vertexinput.hlsl""
}};

struct PixelInput
{{
	float4 vColor : COLOR0;
	float4 vBlendValues		 : TEXCOORD14;
	float4 vPaintValues		 : TEXCOORD15;
	#include ""common/pixelinput.hlsl""
}};

VS
{{
	StaticCombo( S_MULTIBLEND, F_MULTIBLEND, Sys( PC ) );

	#include ""common/vertex.hlsl""

	BoolAttribute( VertexPaintUI2Layer, F_MULTIBLEND == 1 );
	BoolAttribute( VertexPaintUI3Layer, F_MULTIBLEND == 2 );
	BoolAttribute( VertexPaintUI4Layer, F_MULTIBLEND == 3 );
	BoolAttribute( VertexPaintUI5Layer, F_MULTIBLEND == 4 );
	BoolAttribute( VertexPaintUIPickColor, true );

{8}{7}{11}
	PixelInput MainVs( VertexInput v )
	{{
{6}
	}}
}}

PS
{{
	StaticCombo( S_MULTIBLEND, F_MULTIBLEND, Sys( PC ) );

	#include ""common/pixel.hlsl""
{9}{3}{10}
	float4 MainPs( PixelInput i ) : SV_Target0
	{{
{12}
{4}
{5}
{13}
	}}
}}
";

public static string Material_init => @"
Material m = Material::Init();
m.Albedo = float3( 1, 1, 1 );
m.Normal = float3( 0, 0, 1 );
m.Roughness = 1;
m.Metalness = 0;
m.AmbientOcclusion = 1;
m.TintMask = 1;
m.Opacity = 1;
m.Emission = float3( 0, 0, 0 );
m.Transmission = 0;";

public static string Material_output => @"
m.AmbientOcclusion = saturate( m.AmbientOcclusion );
m.Roughness = saturate( m.Roughness );
m.Metalness = saturate( m.Metalness );
m.Opacity = saturate( m.Opacity );

// Result node takes normal as tangent space, convert it to world space now
m.Normal = TransformNormal( m.Normal, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );

// for some toolvis shit
m.WorldTangentU = i.vTangentUWs;
m.WorldTangentV = i.vTangentVWs;
m.TextureCoords = i.vTextureCoords.xy;
		
return ShadingModelStandard::Shade( i, m );";



}
