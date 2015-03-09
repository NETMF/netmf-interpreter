////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

//--//

HRESULT TOUCH_PANEL_Initialize()
{
    return CLR_E_NOTIMPL;
}

HRESULT TOUCH_PANEL_Uninitialize()
{     
    return CLR_E_NOTIMPL;
}

HRESULT TOUCH_PANEL_GetDeviceCaps(UINT32 iIndex, void* lpOutput)
{
    return CLR_E_NOTIMPL;
}

HRESULT TOUCH_PANEL_ResetCalibration()
{
    return CLR_E_NOTIMPL;
}

HRESULT TOUCH_PANEL_SetCalibration( INT32 pointCount, INT16* sx, INT16* sy, INT16* ux, INT16* uy )
{
    return CLR_E_NOTIMPL;
}

HRESULT TOUCH_PANEL_SetNativeBufferSize(INT32 transientBufferSize, INT32 strokeBufferSize)
{
    return CLR_E_NOTIMPL;
}

HRESULT TOUCH_PANEL_EnableTouchCollection(INT32 flags, INT32 x1, INT32 x2, INT32 y1, INT32 y2, PAL_GFX_Bitmap* bitmap)
{
    return CLR_E_NOTIMPL;
}

HRESULT TOUCH_PANEL_GetTouchPoints(INT32* pointCount, INT16* sx, INT16* sy)
{
    return CLR_E_NOTIMPL;
}

HRESULT TOUCH_PANEL_GetSetTouchInfo(UINT32 flags, INT32* param1, INT32* param2, INT32* param3)
{
    return CLR_E_NOTIMPL;
}

