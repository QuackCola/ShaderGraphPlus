#ifndef COMMON_TEST_PIXEL_H
#define COMMON_TEST_PIXEL_H

//-----------------------------------------------------------------------------
//
// Combos
//
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
//
// Includes
//
//-----------------------------------------------------------------------------
#include "sbox_pixel.fxc"

#include "common/material.hlsl"
#include "common/light.hlsl"

#ifdef UNLIT
#include "common/unlit_shadingmodel.hlsl"
#endif

#ifndef UNLIT
#include "common/shadingmodel.hlsl"
#endif 


#endif // COMMON_TEST_PIXEL_H