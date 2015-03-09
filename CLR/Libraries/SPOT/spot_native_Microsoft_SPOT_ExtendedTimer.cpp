////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT.h"


HRESULT Library_spot_native_Microsoft_SPOT_ExtendedTimer::Dispose___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    (void)SetValues( stack, CLR_RT_HeapBlock_Timer::c_ACTION_Destroy );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_native_Microsoft_SPOT_ExtendedTimer::_ctor___VOID__mscorlibSystemThreadingTimerCallback__OBJECT__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(SetValues( stack, CLR_RT_HeapBlock_Timer::c_ACTION_Create | CLR_RT_HeapBlock_Timer::c_INPUT_Int32 ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_ExtendedTimer::_ctor___VOID__mscorlibSystemThreadingTimerCallback__OBJECT__mscorlibSystemTimeSpan__mscorlibSystemTimeSpan( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(SetValues( stack, CLR_RT_HeapBlock_Timer::c_ACTION_Create | CLR_RT_HeapBlock_Timer::c_INPUT_TimeSpan ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_ExtendedTimer::_ctor___VOID__mscorlibSystemThreadingTimerCallback__OBJECT__mscorlibSystemDateTime__mscorlibSystemTimeSpan( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(SetValues( stack, CLR_RT_HeapBlock_Timer::c_ACTION_Create | CLR_RT_HeapBlock_Timer::c_INPUT_Absolute ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_ExtendedTimer::_ctor___VOID__mscorlibSystemThreadingTimerCallback__OBJECT__MicrosoftSPOTExtendedTimerTimeEvents( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_INT32  ev = stack.Arg3().NumericByRefConst().s4;
    CLR_UINT32 flags;

    if(ev < c_TimeEvents_Second || ev > c_TimeEvents_SetTime)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    flags = ev + CLR_RT_HeapBlock_Timer::c_SecondChange;

    TINYCLR_SET_AND_LEAVE(SetValues( stack, CLR_RT_HeapBlock_Timer::c_ACTION_Create | flags ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_ExtendedTimer::Change___VOID__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(SetValues( stack, CLR_RT_HeapBlock_Timer::c_ACTION_Change | CLR_RT_HeapBlock_Timer::c_INPUT_Int32 ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_ExtendedTimer::Change___VOID__mscorlibSystemTimeSpan__mscorlibSystemTimeSpan( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(SetValues( stack, CLR_RT_HeapBlock_Timer::c_ACTION_Change | CLR_RT_HeapBlock_Timer::c_INPUT_TimeSpan ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_ExtendedTimer::Change___VOID__mscorlibSystemDateTime__mscorlibSystemTimeSpan( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(SetValues( stack, CLR_RT_HeapBlock_Timer::c_ACTION_Change | CLR_RT_HeapBlock_Timer::c_INPUT_Absolute ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_ExtendedTimer::get_LastExpiration___mscorlibSystemTimeSpan( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pThis = stack.This();
    CLR_RT_HeapBlock_Timer* timer;
    CLR_INT64*              pRes;

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Timer::ExtractInstance( pThis[ FIELD__m_timer ], timer ));

    pRes = Library_corlib_native_System_TimeSpan::NewObject( stack );

    if(timer)
    {
        *pRes = timer->m_ticksLastExpiration;
    }

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT Library_spot_native_Microsoft_SPOT_ExtendedTimer::SetValues( CLR_RT_StackFrame& stack, CLR_UINT32 flags )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(CLR_RT_HeapBlock_Timer::ConfigureObject( stack, flags ));

    TINYCLR_NOCLEANUP();
}
