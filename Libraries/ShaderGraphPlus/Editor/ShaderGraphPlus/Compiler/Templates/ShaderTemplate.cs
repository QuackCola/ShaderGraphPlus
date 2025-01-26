namespace Editor.ShaderGraphPlus;

public static class ShaderTemplate
{
public static string Code => @"
HEADER
{{
	Description = ""{0}"";
}}

FEATURES
{{
	#include ""common/features.hlsl""
}}

MODES
{{
	VrForward();
	Depth();
	ToolsShadingComplexity( ""tools_shading_complexity.shader"" );
}}

COMMON
{{
{1}
	#include ""common/shared.hlsl""
    #include ""common/gradient.hlsl""
	#include ""procedural.hlsl""

	#define S_UV2 1
	#define CUSTOM_MATERIAL_INPUTS
}}

struct VertexInput
{{
	#include ""common/vertexinput.hlsl""
	float4 vColor : COLOR0 < Semantic( Color ); >;
}};

struct PixelInput
{{
	#include ""common/pixelinput.hlsl""
	float3 vPositionOs : TEXCOORD14;
	float3 vNormalOs : TEXCOORD15;
	float4 vTangentUOs_flTangentVSign : TANGENT	< Semantic( TangentU_SignV ); >;
	float4 vColor : COLOR0;
	float4 vTintColor : COLOR1;
}};

VS
{{
	#include ""common/vertex.hlsl""
{7}{6}{10}
	PixelInput MainVs( VertexInput v )
	{{
{5}
	}}
}}

PS
{{
	#include ""common/pixel.hlsl""
{8}{2}{9}
	float4 MainPs( PixelInput i ) : SV_Target0
	{{
{11}
{3}
{4}
{12}
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


	public static string TextureDefinition => @"<!-- dmx encoding keyvalues2_noids 1 format vtex 1 -->
""CDmeVtex""
{{
    ""m_inputTextureArray"" ""element_array"" 
    [
        ""CDmeInputTexture""
        {{
            ""m_name"" ""string"" ""0""
            ""m_fileName"" ""string"" ""{0}""
            ""m_colorSpace"" ""string"" ""{1}""
            ""m_typeString"" ""string"" ""2D""
            ""m_imageProcessorArray"" ""element_array"" 
            [
                ""CDmeImageProcessor""
                {{
                    ""m_algorithm"" ""string"" ""{3}""
                    ""m_stringArg"" ""string"" """"
                    ""m_vFloat4Arg"" ""vector4"" ""0 0 0 0""
                }}
            ]
        }}
    ]
    ""m_outputTypeString"" ""string"" ""2D""
    ""m_outputFormat"" ""string"" ""{2}""
    ""m_textureOutputChannelArray"" ""element_array""
    [
        ""CDmeTextureOutputChannel""
        {{
            ""m_inputTextureArray"" ""string_array""
            [
                ""0""
            ]
            ""m_srcChannels"" ""string"" ""rgba""
            ""m_dstChannels"" ""string"" ""rgba""
            ""m_mipAlgorithm"" ""CDmeImageProcessor""
            {{
                ""m_algorithm"" ""string"" ""Box""
                ""m_stringArg"" ""string"" """"
                ""m_vFloat4Arg"" ""vector4"" ""0 0 0 0""
            }}
            ""m_outputColorSpace"" ""string"" ""{1}""
        }}
    ]
}}";
}