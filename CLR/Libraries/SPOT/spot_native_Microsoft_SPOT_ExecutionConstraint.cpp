////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT.h"


HRESULT Library_spot_native_Microsoft_SPOT_ExecutionConstraint::Install___STATIC__VOID__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_StackFrame* caller;
    CLR_RT_SubThread*  sth;
    CLR_INT32          timeout;
    CLR_INT32          pri;

    caller  = stack.Caller(); FAULT_ON_NULL(caller); // We need to set the constraint on the caller, not on us...
    timeout = stack.Arg0().NumericByRef().s4;
    pri     = stack.Arg1().NumericByRef().s4;

    //
    // Find the owned subthread or create a new one.
    //
    sth = (CLR_RT_SubThread*)caller->m_owningSubThread->Next();
    if(sth->Next())
    {
        if(sth->m_owningStackFrame != caller)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
        }
    }
    else
    {
        CLR_RT_SubThread* sthNew;

        TINYCLR_CHECK_HRESULT(CLR_RT_SubThread::CreateInstance( stack.m_owningThread, caller, 0, sthNew ));

        sth = sthNew;
    }

    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.InitTimeout( sth->m_timeConstraint, timeout ));

    //
    // Compensate for time spent inside system calls.
    //
    if(sth->m_timeConstraint != TIMEOUT_INFINITE)
    {
        sth->m_timeConstraint -= CLR_RT_ExecutionEngine::s_compensation.Adjust( 0 );

        CLR_RT_ExecutionEngine::InvalidateTimerCache();
    }

    //
    // Only threads with priority above zero can raise their priority.
    //
    if(pri > 0 && caller->m_owningSubThread->m_priority < 0)
    {
        pri = caller->m_owningSubThread->m_priority;
    }

    sth->m_priority  = pri;
    sth->m_status   &= CLR_RT_SubThread::STATUS_Triggered;

    TINYCLR_NOCLEANUP();
}
