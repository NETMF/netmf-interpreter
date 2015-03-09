////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tsc2046_driver.h>

//--//

BOOL HAL_TOUCH_PANEL_Enable( GPIO_INTERRUPT_SERVICE_ROUTINE touchIsrProc )
{ 
    return TSC2046_Driver::Enable(touchIsrProc);
}

BOOL HAL_TOUCH_PANEL_Disable()
{
    return TSC2046_Driver::Disable();
}

void HAL_TOUCH_PANEL_GetPoint( TOUCH_PANEL_SAMPLE_FLAGS* pTipState, int* pSource, int* pUnCalX, int* pUnCalY )
{
    TSC2046_Driver::GetPoint(pTipState, pSource, pUnCalX, pUnCalY);
}

HRESULT HAL_TOUCH_PANEL_GetDeviceCaps(unsigned int iIndex, void* lpOutput)
{
    return TSC2046_Driver::GetDeviceCaps(iIndex, lpOutput);
}

