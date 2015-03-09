////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"


HRESULT Library_corlib_native_System_GC::AnyPendingFinalizers___STATIC__BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    stack.SetResult_Boolean( g_CLR_RT_ExecutionEngine.m_finalizersPending.IsEmpty() == false 
        || (g_CLR_RT_ExecutionEngine.m_finalizerThread != NULL && !g_CLR_RT_ExecutionEngine.m_finalizerThread->CanThreadBeReused()));

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_GC::SuppressFinalize___STATIC__VOID__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pObj = stack.Arg0().Dereference(); FAULT_ON_NULL(pObj);

    CLR_RT_HeapBlock_Finalizer::SuppressFinalize( pObj );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_GC::ReRegisterForFinalize___STATIC__VOID__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDescriptor desc;
    CLR_RT_HeapBlock*     pObj = stack.Arg0().Dereference(); FAULT_ON_NULL(pObj);

    CLR_RT_HeapBlock_Finalizer::SuppressFinalize( pObj );

    TINYCLR_CHECK_HRESULT(desc.InitializeFromObject( stack.Arg0() ));

    if(desc.m_handlerCls.HasFinalizer())
    {
        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Finalizer::CreateInstance( stack.Arg0().Dereference(), desc.m_handlerCls ));
    }

    TINYCLR_NOCLEANUP();
}
