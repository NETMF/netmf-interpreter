////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware.h"

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_I2CDevice__Configuration::GetInitialConfig(CLR_RT_HeapBlock& ref, I2C_USER_CONFIGURATION& nativeConfig)
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* config = ref.Dereference(); FAULT_ON_NULL(config);

    nativeConfig.Address   = config[ FIELD__Address      ].NumericByRef().u2;
    nativeConfig.ClockRate = config[ FIELD__ClockRateKhz ].NumericByRef().s4;

    if(nativeConfig.Address > c_MaxI2cAddress)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    if(nativeConfig.ClockRate < c_MimimumClockRateKhz || c_MaximumClockRateKhz < nativeConfig.ClockRate)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER)
    }

    TINYCLR_NOCLEANUP();
}
