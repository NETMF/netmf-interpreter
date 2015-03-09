////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware.h"


HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_SPI__Configuration::GetInitialConfig( CLR_RT_HeapBlock& ref, SPI_CONFIGURATION& nativeConfig )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* config = ref.Dereference(); FAULT_ON_NULL(config);

    nativeConfig.DeviceCS               = config[ FIELD__ChipSelect_Port        ].NumericByRef().u4             ;
    nativeConfig.CS_Active              = config[ FIELD__ChipSelect_ActiveState ].NumericByRef().u1 != 0 ? 1 : 0;
    nativeConfig.CS_Setup_uSecs         = config[ FIELD__ChipSelect_SetupTime   ].NumericByRef().u4             ;
    nativeConfig.CS_Hold_uSecs          = config[ FIELD__ChipSelect_HoldTime    ].NumericByRef().u4             ;
    nativeConfig.MSK_IDLE               = config[ FIELD__Clock_IdleState        ].NumericByRef().u1 != 0 ? 1 : 0;
    nativeConfig.MSK_SampleEdge         = config[ FIELD__Clock_Edge             ].NumericByRef().u1 != 0 ? 1 : 0;
    nativeConfig.Clock_RateKHz          = config[ FIELD__Clock_RateKHz          ].NumericByRef().u4             ;
    nativeConfig.SPI_mod                = config[ FIELD__SPI_mod                ].NumericByRef().u4             ;
    nativeConfig.BusyPin.Pin            = config[ FIELD__BusyPin                ].NumericByRef().u4             ;
    nativeConfig.BusyPin.ActiveState    = config[ FIELD__BusyPin_ActiveState    ].NumericByRef().u1 != 0 ? 1 : 0;

    TINYCLR_NOCLEANUP();
}
