////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware.h"


HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Cpu::get_SystemClock___STATIC__U4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    stack.SetResult( ::CPU_SystemClock(), DATATYPE_U4 );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Cpu::get_SlowClock___STATIC__U4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    stack.SetResult( ::CPU_TicksPerSecond(), DATATYPE_U4 );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Cpu::get_GlitchFilterTime___STATIC__mscorlibSystemTimeSpan( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_INT64 val = (CLR_INT64)::CPU_GPIO_GetDebounce() * TIME_CONVERSION__TO_MILLISECONDS;

    stack.SetResult_I8( val );
    
    stack.TopValue().ChangeDataType( DATATYPE_TIMESPAN );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Cpu::set_GlitchFilterTime___STATIC__VOID__mscorlibSystemTimeSpan( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_INT64 val = (CLR_INT64_TEMP_CAST) stack.Arg0().NumericByRef().s8 / TIME_CONVERSION__TO_MILLISECONDS;

    if(!::CPU_GPIO_SetDebounce( val ))
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    TINYCLR_NOCLEANUP();
}

