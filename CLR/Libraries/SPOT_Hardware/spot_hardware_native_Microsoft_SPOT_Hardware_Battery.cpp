////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware.h"


HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Battery::ReadVoltage___STATIC__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    INT32 val;

    if(Battery_Voltage( val ) == FALSE)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_TIMEOUT);
    }

    stack.SetResult_I4( val );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Battery::ReadTemperature___STATIC__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    INT32 val;

    if(::Battery_Temperature( val ) == FALSE)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_TIMEOUT);
    }

    stack.SetResult_I4( val );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Battery::OnCharger___STATIC__BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    UINT32 status;

    if(::Charger_Status( status ) == FALSE)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_TIMEOUT);
    }

    stack.SetResult_Boolean( (status & CHARGER_STATUS_ON_AC_POWER) ? true : false );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Battery::IsFullyCharged___STATIC__BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    UINT32 status;

    if(::Charger_Status( status ) == FALSE)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_TIMEOUT);
    }

    stack.SetResult_Boolean( (status & CHARGER_STATUS_CHARGE_COMPLETE) ? true : false );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Battery::StateOfCharge___STATIC__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    UINT8 val;

    if(::Battery_StateOfCharge( val ) == FALSE)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_TIMEOUT);
    }

    stack.SetResult_I4( val );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Battery::WaitForEvent___STATIC__BOOLEAN__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_INT64* timeout;
    bool       fRes;

    TINYCLR_CHECK_HRESULT(stack.SetupTimeout( stack.Arg0(), timeout ));

    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeout, CLR_RT_ExecutionEngine::c_Event_Battery, fRes ));

    stack.PopValue();

    stack.SetResult_Boolean( fRes );

    TINYCLR_NOCLEANUP();
}
