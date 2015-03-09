////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"


HRESULT Library_corlib_native_System_Threading_Timer::Dispose___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    (void)SetValues( stack, CLR_RT_HeapBlock_Timer::c_ACTION_Destroy );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Threading_Timer::_ctor___VOID__SystemThreadingTimerCallback__OBJECT__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(SetValues( stack, CLR_RT_HeapBlock_Timer::c_ACTION_Create | CLR_RT_HeapBlock_Timer::c_INPUT_Int32 ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Threading_Timer::_ctor___VOID__SystemThreadingTimerCallback__OBJECT__SystemTimeSpan__SystemTimeSpan( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(SetValues( stack, CLR_RT_HeapBlock_Timer::c_ACTION_Create | CLR_RT_HeapBlock_Timer::c_INPUT_TimeSpan ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Threading_Timer::Change___BOOLEAN__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(CheckDisposed( stack ))
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }
    
    stack.SetResult_Boolean( true );

    TINYCLR_SET_AND_LEAVE(SetValues( stack, CLR_RT_HeapBlock_Timer::c_ACTION_Change | CLR_RT_HeapBlock_Timer::c_INPUT_Int32 ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Threading_Timer::Change___BOOLEAN__SystemTimeSpan__SystemTimeSpan( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(CheckDisposed( stack ))
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    stack.SetResult_Boolean( true );

    TINYCLR_SET_AND_LEAVE(SetValues( stack, CLR_RT_HeapBlock_Timer::c_ACTION_Change | CLR_RT_HeapBlock_Timer::c_INPUT_TimeSpan ));

    TINYCLR_NOCLEANUP();
}

//--//

bool Library_corlib_native_System_Threading_Timer::CheckDisposed( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    
    return CLR_RT_HeapBlock_Timer::CheckDisposed( stack );
}


HRESULT Library_corlib_native_System_Threading_Timer::SetValues( CLR_RT_StackFrame& stack, CLR_UINT32 flags )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(CLR_RT_HeapBlock_Timer::ConfigureObject( stack, flags ));

    TINYCLR_NOCLEANUP();
}

