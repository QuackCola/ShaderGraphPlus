namespace Editor.ShaderGraphPlus;


public enum FillMode
{ 
    Wireframe,
    Solid,
}

public enum CullMode
{ 
    None,
    Back,
    Front,
}

public enum DepthFunc
{ 
    Never,
    Less,
    Equal,
    LessEqual,
    Greater,
    NotEqual,
    GreaterEqual,
    Always,
}

public enum StencilFailOp
{
    Keep, 
    Zero, 
    Replace, 
    Incr_sat, 
    Decr_sat, 
    Invert, 
    Incr, 
    Decr,
}

public enum StencilDepthFailOp
{
    Keep,
    Zero,
    Replace,
    Incr_sat,
    Decr_sat,
    Invert,
    Incr,
    Decr,
}

public enum StencilPassOp
{
    Keep,
    Zero,
    Replace,
    Incr_sat,
    Decr_sat,
    Invert,
    Incr,
    Decr,
}

public enum StencilFunc
{
    Never,
    Less,
    Equal,
    LessEqual,
    Greater,
    NotEqual,
    GreaterEqual,
    Always,
}

public enum BackStencilFailOp
{
    Keep,
    Zero,
    Replace,
    Incr_sat,
    Decr_sat,
    Invert,
    Incr,
    Decr,
}

public enum BackStencilDepthFailOp
{
    Keep,
    Zero,
    Replace,
    Incr_sat,
    Decr_sat,
    Invert,
    Incr,
    Decr,
}

public enum BackStencilPassOp
{
    Keep,
    Zero,
    Replace,
    Incr_sat,
    Decr_sat,
    Invert,
    Incr,
    Decr,
}

public enum BackStencilFunc
{
    Keep,
    Zero,
    Replace,
    Incr_sat,
    Decr_sat,
    Invert,
    Incr,
    Decr,
}

public enum SrcBlend
{
    ZERO, 
    ONE, 
    SRC_COLOR, 
    INV_SRC_COLOR, 
    SRC_ALPHA, 
    INV_SRC_ALPHA,
    DEST_ALPHA, 
    INV_DEST_ALPHA, 
    DEST_COLOR, 
    INV_DEST_COLOR, 
    SRC_ALPHA_SAT, 
    BLEND_FACTOR, 
    SRC1_COLOR, 
    INV_SRC1_COLOR, 
    SRC1_ALPHA, 
    INV_SRC1_ALPHA
}

public enum DstBlend
{
    ZERO,
    ONE,
    SRC_COLOR,
    INV_SRC_COLOR,
    SRC_ALPHA,
    INV_SRC_ALPHA,
    DEST_ALPHA,
    INV_DEST_ALPHA,
    DEST_COLOR,
    INV_DEST_COLOR,
    SRC_ALPHA_SAT,
    BLEND_FACTOR,
    SRC1_COLOR,
    INV_SRC1_COLOR,
    SRC1_ALPHA,
    INV_SRC1_ALPHA
}

public enum BlendOp
{ 
    Add,
    Subtract,
    RevSubtract,
    Min,
    Max
}

public enum BlendOpAlpha
{
    Add,
    Subtract,
    RevSubtract,
    Min,
    Max
}

public enum SrcBlendAlpha
{
    ZERO,
    ONE,
    SRC_COLOR,
    INV_SRC_COLOR,
    SRC_ALPHA,
    INV_SRC_ALPHA,
    DEST_ALPHA,
    INV_DEST_ALPHA,
    DEST_COLOR,
    INV_DEST_COLOR,
    SRC_ALPHA_SAT,
    BLEND_FACTOR,
    SRC1_COLOR,
    INV_SRC1_COLOR,
    SRC1_ALPHA,
    INV_SRC1_ALPHA
}

public enum DstBlendAlpha
{
    ZERO,
    ONE,
    SRC_COLOR,
    INV_SRC_COLOR,
    SRC_ALPHA,
    INV_SRC_ALPHA,
    DEST_ALPHA,
    INV_DEST_ALPHA,
    DEST_COLOR,
    INV_DEST_COLOR,
    SRC_ALPHA_SAT,
    BLEND_FACTOR,
    SRC1_COLOR,
    INV_SRC1_COLOR,
    SRC1_ALPHA,
    INV_SRC1_ALPHA
}

public enum AlphaTestFunc
{
    Never,
    Less,
    Equal,
    LessEqual,
    Greater,
    NotEqual,
    GreaterEqual,
    Always,
}


public class RenderState
{

    public FillMode FillMode { get; set; }
    public CullMode CullMode { get; set; }
    public DepthFunc DepthFunc { get; set; }
    public StencilFailOp StencilFailOp { get; set; }
    public StencilDepthFailOp StencilDepthFailOp { get; set; }
    public StencilPassOp StencilPassOp { get; set; }
    public StencilFunc StencilFunc { get; set; }
    public BackStencilFailOp BackStencilFailOp { get; set; }
    public BackStencilDepthFailOp BackStencilDepthFailOp { get; set; }
    public BackStencilPassOp BackStencilPassOp { get; set; }
    public BackStencilFunc BackStencilFunc { get; set; }
    public SrcBlend SrcBlend { get; set; }
    public DstBlend DstBlend { get; set; }
    public BlendOp BlendOp { get; set; }
    public BlendOpAlpha BlendOpAlpha { get; set; }
    public SrcBlendAlpha SrcBlendAlpha { get; set; }
    public DstBlendAlpha DstBlendAlpha { get; set; }
    public AlphaTestFunc AlphaTestFunc { get; set; }








    //public void Set()
    //{ 
    //    
    //}


}