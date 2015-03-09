////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"


HRESULT Library_corlib_native_System_Threading_Monitor::Enter___STATIC__VOID__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_StackFrame* caller = NULL;
        
    if(stack.Arg0().Dereference() == NULL)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_ARGUMENT_NULL);
    }
    
    switch(stack.m_customState)
    {
    case 0:
        {
            caller = stack.Caller(); FAULT_ON_NULL(caller); // We need to set the constraint on the caller, not on us...

            hr = g_CLR_RT_ExecutionEngine.LockObject( stack.Arg0(), caller->m_owningSubThread, TIMEOUT_INFINITE, false );
            if(hr == CLR_E_THREAD_WAITING)
            {
                stack.m_customState = 1;
            }

            TINYCLR_LEAVE();
        }
        break;

    case 1:
        TINYCLR_SET_AND_LEAVE(S_OK);
        break;

    default:
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Threading_Monitor::Exit___STATIC__VOID__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    
    CLR_RT_StackFrame* caller = NULL;

    if(stack.Arg0().Dereference() == NULL)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_ARGUMENT_NULL);
    }
    
    caller = stack.Caller(); FAULT_ON_NULL(caller); // We need to set the constraint on the caller, not on us...

    TINYCLR_SET_AND_LEAVE(g_CLR_RT_ExecutionEngine.UnlockObject( stack.Arg0(), caller->m_owningSubThread ));

    TINYCLR_NOCLEANUP();
}
