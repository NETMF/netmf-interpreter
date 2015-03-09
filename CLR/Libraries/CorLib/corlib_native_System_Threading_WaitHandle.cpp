////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"


HRESULT Library_corlib_native_System_Threading_WaitHandle::WaitOne___BOOLEAN__I4__BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_CHECK_HRESULT(Wait( stack, stack.Arg1(), stack.Arg2(), &stack.ThisRef(), 1, false ));

    stack.SetResult_Boolean( stack.m_owningThread->m_waitForObject_Result != CLR_RT_Thread::TH_WAIT_RESULT_TIMEOUT );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Threading_WaitHandle::WaitMultiple___STATIC__I4__SZARRAY_SystemThreadingWaitHandle__I4__BOOLEAN__BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* arr = stack.Arg0().DereferenceArray();  FAULT_ON_NULL(arr);

    TINYCLR_CHECK_HRESULT(Wait( stack, stack.Arg1(), stack.Arg2(), (CLR_RT_HeapBlock*)arr->GetFirstElement(), arr->m_numOfElements, stack.Arg3().NumericByRef().s1 == 1 ));
    
    stack.SetResult_I4( stack.m_owningThread->m_waitForObject_Result );

    TINYCLR_NOCLEANUP();
}

//--//
    
void Library_corlib_native_System_Threading_WaitHandle::Set( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock* pThis = stack.This();

    CLR_RT_HeapBlock_WaitForObject::SignalObject( *pThis );

    stack.SetResult_Boolean( true );
}

void Library_corlib_native_System_Threading_WaitHandle::Reset( CLR_RT_StackFrame& stack )
{        
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock* pThis = stack.This();

    pThis->ResetFlags( CLR_RT_HeapBlock::HB_Signaled );

    stack.SetResult_Boolean( true );
}
    
HRESULT Library_corlib_native_System_Threading_WaitHandle::Wait( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock& blkTimeout, CLR_RT_HeapBlock& blkExitContext, CLR_RT_HeapBlock* objects, int cObjects, bool fWaitAll )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_INT64 timeExpire;

    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.InitTimeout( timeExpire, blkTimeout.NumericByRef().s4 ));

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_WaitForObject::WaitForSignal( stack, timeExpire, objects, cObjects, fWaitAll ));

    TINYCLR_NOCLEANUP();
}
