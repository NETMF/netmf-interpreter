////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware.h"

//--//

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_PowerState::Reboot___STATIC__VOID__BOOLEAN( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    bool fSoft = stack.Arg0().NumericByRef().s1 != 0;
    
    g_CLR_RT_ExecutionEngine.Reboot( !fSoft );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_PowerState::WaitForIdleCPU___STATIC__BOOLEAN__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_Thread* th = stack.m_owningThread;
    CLR_INT64*     timeout;
    bool           fRes;

    if(stack.m_customState == 0)
    {
        //
        // We set this parameter on first entry. If the thread is awoken for any reason, the parameter is reset.
        //
        th->m_waitForEvents_IdleTimeWorkItem  = stack.Arg0().NumericByRef().s4;
        th->m_waitForEvents_IdleTimeWorkItem *= TIME_CONVERSION__TO_MILLISECONDS;
    }

    TINYCLR_CHECK_HRESULT(stack.SetupTimeout( stack.Arg1(), timeout ));


    if(th->m_waitForEvents_IdleTimeWorkItem == TIMEOUT_ZERO)
    {
        fRes = true;
    }
    else
    {
        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeout, CLR_RT_ExecutionEngine::c_Event_IdleCPU, fRes ));
    }

    stack.PopValue();

    stack.SetResult_Boolean( fRes );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_PowerState::get_MaximumTimeToActive___STATIC__mscorlibSystemTimeSpan( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    CLR_INT64* pRes = Library_corlib_native_System_TimeSpan::NewObject( stack );

    *pRes = g_CLR_RT_ExecutionEngine.m_maximumTimeToActive;
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_PowerState::set_MaximumTimeToActive___STATIC__VOID__mscorlibSystemTimeSpan( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    g_CLR_RT_ExecutionEngine.m_maximumTimeToActive = stack.Arg0().NumericByRef().s8;
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_PowerState::get_Uptime___STATIC__mscorlibSystemTimeSpan( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    CLR_INT64* pRes = Library_corlib_native_System_TimeSpan::NewObject( stack );

    *pRes = CLR_RT_ExecutionEngine::GetUptime();
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_PowerState::get_WakeupEvents___STATIC__MicrosoftSPOTHardwareHardwareEvent( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    stack.SetResult_U4( g_CLR_HW_Hardware.m_wakeupEvents & (~SYSTEM_EVENT_FLAG_DEBUGGER_ACTIVITY) );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_PowerState::set_WakeupEvents___STATIC__VOID__MicrosoftSPOTHardwareHardwareEvent( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    CLR_UINT32 events = stack.Arg0().NumericByRef().u4;
    
    g_CLR_HW_Hardware.m_wakeupEvents = events | g_CLR_HW_Hardware.m_DebuggerEventsMask | SYSTEM_EVENT_FLAG_DEBUGGER_ACTIVITY;
    
    TINYCLR_NOCLEANUP_NOLABEL();
}
   
HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_PowerState::InternalSleep___STATIC__VOID__MicrosoftSPOTHardwareSleepLevel__MicrosoftSPOTHardwareHardwareEvent( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    CLR_INT64 timeout_ms;
    CLR_UINT32 res;
    
    CLR_UINT32 sleepLevel = stack.Arg0().NumericByRef().s4;
    CLR_UINT32 events     = stack.Arg1().NumericByRef().s4;

    events |= g_CLR_HW_Hardware.m_DebuggerEventsMask | SYSTEM_EVENT_FLAG_DEBUGGER_ACTIVITY;

    timeout_ms = g_CLR_RT_ExecutionEngine.ProcessTimer();    
    
    // if the caller doesn't want to wakeup on system timer events then don't call process timer, just
    // use the max time to active
    if(0 == (events & SYSTEM_EVENT_FLAG_SYSTEM_TIMER))
    {
        timeout_ms = g_CLR_RT_ExecutionEngine.m_maximumTimeToActive;
    }

    CLR_RT_ExecutionEngine::ExecutionConstraint_Suspend();

    BOOL fEnabled = Watchdog_GetSetEnabled( FALSE, TRUE );

    res = g_CLR_RT_ExecutionEngine.WaitForActivity( sleepLevel, (UINT32)events, timeout_ms );

    Watchdog_GetSetEnabled( fEnabled, TRUE );

    CLR_RT_ExecutionEngine::ExecutionConstraint_Resume();

    PostManagedEvent( EVENT_SLEEPLEVEL, SLEEP_LEVEL_CATEGORY__WAKEUP, 0, res );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_PowerState::InternalChangePowerLevel___STATIC__VOID__MicrosoftSPOTHardwarePowerLevel( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    POWER_LEVEL powerLevel = (POWER_LEVEL)stack.Arg0().NumericByRef().s4;

    CPU_ChangePowerLevel( powerLevel );
    
    TINYCLR_NOCLEANUP_NOLABEL();
    
}

