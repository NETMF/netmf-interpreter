////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "spot_touch_native.h"


static const CLR_RT_MethodHandler method_lookup[] =
{
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    Library_spot_touch_native_Microsoft_SPOT_Touch_Ink::SetInkRegion___STATIC__VOID__U4__I4__I4__I4__I4__I4__I4__I4__MicrosoftSPOTGraphicsMicrosoftSPOTBitmap,
    Library_spot_touch_native_Microsoft_SPOT_Touch_Ink::ResetInkRegion___STATIC__VOID,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    Library_spot_touch_native_Microsoft_SPOT_Touch_TouchCollectorConfiguration::EnableTouchCollection___STATIC__VOID__I4__I4__I4__I4__I4__MicrosoftSPOTGraphicsMicrosoftSPOTBitmap,
    Library_spot_touch_native_Microsoft_SPOT_Touch_TouchCollectorConfiguration::GetTouchPoints___STATIC__VOID__BYREF_I4__SZARRAY_I2__SZARRAY_I2,
    Library_spot_touch_native_Microsoft_SPOT_Touch_TouchCollectorConfiguration::GetTouchInput___STATIC__VOID__MicrosoftSPOTTouchTouchCollectorConfigurationTouchInput__BYREF_I4__BYREF_I4__BYREF_I4,
    Library_spot_touch_native_Microsoft_SPOT_Touch_TouchCollectorConfiguration::SetTouchInput___STATIC__VOID__MicrosoftSPOTTouchTouchCollectorConfigurationTouchInput__I4__I4__I4,
    NULL,
    Library_spot_touch_native_Microsoft_SPOT_Touch_TouchEventProcessor::ProcessEvent___MicrosoftSPOTNativeMicrosoftSPOTBaseEvent__U4__U4__mscorlibSystemDateTime,
    NULL,
    NULL,
    NULL,
    Library_spot_touch_native_Microsoft_SPOT_Touch_TouchPanel::SetCalibration___VOID__I4__SZARRAY_I2__SZARRAY_I2__SZARRAY_I2__SZARRAY_I2,
    Library_spot_touch_native_Microsoft_SPOT_Touch_TouchPanel::GetCalibrationPointCount___VOID__BYREF_I4,
    Library_spot_touch_native_Microsoft_SPOT_Touch_TouchPanel::StartCalibration___VOID,
    Library_spot_touch_native_Microsoft_SPOT_Touch_TouchPanel::GetCalibrationPoint___VOID__I4__BYREF_I4__BYREF_I4,
    Library_spot_touch_native_Microsoft_SPOT_Touch_TouchPanel::EnableInternal___VOID__BOOLEAN,
    NULL, 
};

const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Touch =
{
    "Microsoft.SPOT.Touch", 
    0x2D4E2DCC,
    method_lookup
};

