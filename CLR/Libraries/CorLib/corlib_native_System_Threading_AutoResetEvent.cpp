////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"


HRESULT Library_corlib_native_System_Threading_AutoResetEvent::_ctor___VOID__BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = stack.This();

    pThis->SetFlags( CLR_RT_HeapBlock::HB_SignalAutoReset );

    if(stack.Arg1().NumericByRef().s4)
    {
        pThis->SetFlags( CLR_RT_HeapBlock::HB_Signaled );
    }

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Threading_AutoResetEvent::Reset___BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    Library_corlib_native_System_Threading_WaitHandle::Reset( stack );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Threading_AutoResetEvent::Set___BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    Library_corlib_native_System_Threading_WaitHandle::Set( stack );

    TINYCLR_NOCLEANUP_NOLABEL();    
}
