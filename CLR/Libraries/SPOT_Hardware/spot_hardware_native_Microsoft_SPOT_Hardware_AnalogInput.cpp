////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware.h"


HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_AnalogInput::ReadRaw___I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = stack.This();

    const ANALOG_CHANNEL channel = (ANALOG_CHANNEL)pThis[FIELD__m_channel].NumericByRef().s4;
    
    const CLR_INT32 raw = ::AD_Read(channel);

    stack.SetResult_I4(raw);

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_AnalogInput::Initialize___STATIC__VOID__MicrosoftSPOTHardwareCpuAnalogChannel__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    
    bool fRes = ::AD_Initialize((ANALOG_CHANNEL)stack.Arg0().NumericByRef().s4, stack.Arg1().NumericByRef().s4) != 0;
    
    TINYCLR_SET_AND_LEAVE(fRes ? S_OK : CLR_E_FAIL);

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_AnalogInput::Uninitialize___STATIC__VOID__MicrosoftSPOTHardwareCpuAnalogChannel( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    
    ::AD_Uninitialize((ANALOG_CHANNEL)stack.Arg0().NumericByRef().s4);
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

