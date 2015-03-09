////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware_pwm.h"
#include "spot_hardware_pwm_native.h"


static const CLR_RT_MethodHandler method_lookup[] =
{
    NULL,
    NULL,
    NULL,
    NULL,
    Library_spot_hardware_pwm_native_Microsoft_SPOT_Hardware_PWM::Start___VOID,
    Library_spot_hardware_pwm_native_Microsoft_SPOT_Hardware_PWM::Stop___VOID,
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
    Library_spot_hardware_pwm_native_Microsoft_SPOT_Hardware_PWM::Commit___VOID,
    Library_spot_hardware_pwm_native_Microsoft_SPOT_Hardware_PWM::Init___VOID,
    Library_spot_hardware_pwm_native_Microsoft_SPOT_Hardware_PWM::Uninit___VOID,
    Library_spot_hardware_pwm_native_Microsoft_SPOT_Hardware_PWM::Start___STATIC__VOID__SZARRAY_MicrosoftSPOTHardwarePWM,
    Library_spot_hardware_pwm_native_Microsoft_SPOT_Hardware_PWM::Stop___STATIC__VOID__SZARRAY_MicrosoftSPOTHardwarePWM,
    NULL,
    NULL,
    NULL,
    NULL,
};

const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Hardware_PWM =
{
    "Microsoft.SPOT.Hardware.PWM", 
    0xDD43B0B7,
    method_lookup
};
