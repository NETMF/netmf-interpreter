////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"


HRESULT Library_corlib_native_System_WeakReference::get_IsAlive___BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_WeakReference* weak;

    weak = (CLR_RT_HeapBlock_WeakReference*)stack.This(); FAULT_ON_NULL(weak);

    stack.SetResult_Boolean( weak->m_targetDirect != NULL );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_WeakReference::get_Target___OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_WeakReference* weak;

    weak = (CLR_RT_HeapBlock_WeakReference*)stack.This(); FAULT_ON_NULL(weak);

    TINYCLR_SET_AND_LEAVE(weak->GetTarget( stack.PushValue() ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_WeakReference::set_Target___VOID__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_WeakReference* weak;

    weak = (CLR_RT_HeapBlock_WeakReference*)stack.This(); FAULT_ON_NULL(weak);

    TINYCLR_SET_AND_LEAVE(weak->SetTarget( stack.Arg1() ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_WeakReference::_ctor___VOID__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_WeakReference* weak = (CLR_RT_HeapBlock_WeakReference*)stack.This(); FAULT_ON_NULL(weak);

    TINYCLR_SET_AND_LEAVE(weak->SetTarget( stack.Arg1() ));

    TINYCLR_NOCLEANUP();
}
