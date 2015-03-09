////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#ifndef _DRIVERS_INK_DECL_H_
#define _DRIVERS_INK_DECL_H_ 1

#include <graphics_decl.h>

//--//

struct InkRegionInfo
{
    UINT16          X1, X2, Y1, Y2; /// Inking region in screen co-ordinates.
    UINT16          BorderWidth; /// border width for inking region
    PAL_GFX_Bitmap* Bmp; /// This field may be NULL, if not NULL it must be valid pinned memory.
                         /// Other criterion is this bitmap must have size (X2-X1) x (Y2-Y1).
    GFX_Pen         Pen;       
};

HRESULT Ink_Initialize  ();
HRESULT Ink_Uninitialize();
HRESULT Ink_SetRegion   ( InkRegionInfo* inkRegionInfo );
HRESULT Ink_ResetRegion ();


#endif // _DRIVERS_INK_DECL_H_

