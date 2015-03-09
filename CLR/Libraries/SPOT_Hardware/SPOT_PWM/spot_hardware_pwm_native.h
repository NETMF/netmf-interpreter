////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#ifndef _SPOT_HARDWARE_PWM_NATIVE_H_
#define _SPOT_HARDWARE_PWM_NATIVE_H_

//--//

#include <TinyCLR_Interop.h>

//--//

struct Library_spot_hardware_pwm_native_Microsoft_SPOT_Hardware_PWM
{
    static const int c_MaxPwmChannels = 16;
    //--//
    static const int FIELD_STATIC__c_ToMicroSeconds = 0;

    static const int FIELD__m_pin = 1;
    static const int FIELD__m_channel = 2;
    static const int FIELD__m_period = 3;
    static const int FIELD__m_duration = 4;
    static const int FIELD__m_invert = 5;
    static const int FIELD__m_scale = 6;

    TINYCLR_NATIVE_DECLARE(Start___VOID);
    TINYCLR_NATIVE_DECLARE(Stop___VOID);
    TINYCLR_NATIVE_DECLARE(Commit___VOID);
    TINYCLR_NATIVE_DECLARE(Init___VOID);
    TINYCLR_NATIVE_DECLARE(Uninit___VOID);
    TINYCLR_NATIVE_DECLARE(Start___STATIC__VOID__SZARRAY_MicrosoftSPOTHardwarePWM);
    TINYCLR_NATIVE_DECLARE(Stop___STATIC__VOID__SZARRAY_MicrosoftSPOTHardwarePWM);

    //--//

    static HRESULT GetChannelsAndPins(CLR_RT_HeapBlock_Array* ports, PWM_CHANNEL* const channels, GPIO_PIN* const pins, CLR_UINT32& channelCount);

};


extern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Hardware_PWM;

//--//

#endif  //_SPOT_HARDWARE_PWM_NATIVE_H_


