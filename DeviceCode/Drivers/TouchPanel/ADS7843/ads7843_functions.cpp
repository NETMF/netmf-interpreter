////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <ADS7843_driver.h>

//--//

BOOL HAL_TOUCH_PANEL_Enable( GPIO_INTERRUPT_SERVICE_ROUTINE touchIsrProc )
{ 
    return ADS7843_Driver::Enable(touchIsrProc);
}

BOOL HAL_TOUCH_PANEL_Disable()
{
    return ADS7843_Driver::Disable();
}

void HAL_TOUCH_PANEL_GetPoint( TOUCH_PANEL_SAMPLE_FLAGS* pTipState, int* pSource, int* pUnCalX, int* pUnCalY )
{
    ADS7843_Driver::GetPoint(pTipState, pSource, pUnCalX, pUnCalY);
}

HRESULT HAL_TOUCH_PANEL_GetDeviceCaps(unsigned int iIndex, void* lpOutput)
{
    return ADS7843_Driver::GetDeviceCaps(iIndex, lpOutput);
}

