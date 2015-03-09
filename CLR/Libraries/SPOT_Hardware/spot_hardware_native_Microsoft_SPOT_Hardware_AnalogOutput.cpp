////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  D/A Implementation: Copyright (c) Oberon microsystems, Inc.
//
//  *** Analog Output Interop ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware.h"


HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_AnalogOutput::WriteRaw___VOID__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = stack.This();
    DA_CHANNEL channel = (DA_CHANNEL)pThis[FIELD__m_channel].NumericByRef().s4;
    
    INT32 level = stack.Arg1().NumericByRef().s4;

    ::DA_Write(channel, level);

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_AnalogOutput::Initialize___STATIC__VOID__MicrosoftSPOTHardwareCpuAnalogOutputChannel__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    
    DA_CHANNEL channel = (DA_CHANNEL)stack.Arg0().NumericByRef().s4;
    INT32 precisionInBits = stack.Arg1().NumericByRef().s4;
    
    bool fRes = ::DA_Initialize(channel, precisionInBits) != 0;
    
    TINYCLR_SET_AND_LEAVE(fRes ? S_OK : CLR_E_FAIL);

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_AnalogOutput::Uninitialize___STATIC__VOID__MicrosoftSPOTHardwareCpuAnalogOutputChannel( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    
    ::DA_Uninitialize((DA_CHANNEL)stack.Arg0().NumericByRef().s4);
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

