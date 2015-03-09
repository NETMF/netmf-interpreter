////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#ifndef _SPOT_TOUCH_NATIVE_H_
#define _SPOT_TOUCH_NATIVE_H_

#include <TinyCLR_Runtime.h>

//--//

struct Library_spot_touch_native_Microsoft_SPOT_Touch_Ink
{
    TINYCLR_NATIVE_DECLARE(SetInkRegion___STATIC__VOID__U4__I4__I4__I4__I4__I4__I4__I4__MicrosoftSPOTGraphicsMicrosoftSPOTBitmap);
    TINYCLR_NATIVE_DECLARE(ResetInkRegion___STATIC__VOID);

    //--//

    static CLR_RT_HeapBlock* m_InkPinnedBitmap;

};

struct Library_spot_touch_native_Microsoft_SPOT_Touch_Touch
{
    static const int FIELD_STATIC___initialized = 0;
    static const int FIELD_STATIC___activeTouchPanel = 1;

    //--//

};

struct Library_spot_touch_native_Microsoft_SPOT_Touch_TouchCollector
{
    static const int FIELD__lastTime          = 1;
    static const int FIELD___nativeBufferSize = 2;


    //--//

};

struct Library_spot_touch_native_Microsoft_SPOT_Touch_TouchCollectorConfiguration
{
    static const int FIELD_STATIC___collectionMode = 2;
    static const int FIELD_STATIC___collectionMethod = 3;
    static const int FIELD_STATIC___touchCollector = 4;
    static const int FIELD_STATIC___collectionBufferSize = 5;

    TINYCLR_NATIVE_DECLARE(EnableTouchCollection___STATIC__VOID__I4__I4__I4__I4__I4__MicrosoftSPOTGraphicsMicrosoftSPOTBitmap);
    TINYCLR_NATIVE_DECLARE(GetTouchPoints___STATIC__VOID__BYREF_I4__SZARRAY_I2__SZARRAY_I2);
    TINYCLR_NATIVE_DECLARE(GetTouchInput___STATIC__VOID__MicrosoftSPOTTouchTouchCollectorConfigurationTouchInput__BYREF_I4__BYREF_I4__BYREF_I4);
    TINYCLR_NATIVE_DECLARE(SetTouchInput___STATIC__VOID__MicrosoftSPOTTouchTouchCollectorConfigurationTouchInput__I4__I4__I4);

    //--//

};

struct Library_spot_touch_native_Microsoft_SPOT_Touch_TouchEventProcessor
{
    TINYCLR_NATIVE_DECLARE(ProcessEvent___MicrosoftSPOTNativeMicrosoftSPOTBaseEvent__U4__U4__mscorlibSystemDateTime);

    //--//

};

struct Library_spot_touch_native_Microsoft_SPOT_Touch_TouchPanel
{
    static const int FIELD___enabled = 1;

    TINYCLR_NATIVE_DECLARE(SetCalibration___VOID__I4__SZARRAY_I2__SZARRAY_I2__SZARRAY_I2__SZARRAY_I2);
    TINYCLR_NATIVE_DECLARE(GetCalibrationPointCount___VOID__BYREF_I4);
    TINYCLR_NATIVE_DECLARE(StartCalibration___VOID);
    TINYCLR_NATIVE_DECLARE(GetCalibrationPoint___VOID__I4__BYREF_I4__BYREF_I4);
    TINYCLR_NATIVE_DECLARE(EnableInternal___VOID__BOOLEAN);

    //--//

};



extern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Touch;

#endif  //_SPOT_TOUCH_NATIVE_H_
