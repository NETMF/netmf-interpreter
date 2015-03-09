////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "CorLib.h"


HRESULT Library_corlib_native_System_Collections_Stack::Clear___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Stack* pThis = (CLR_RT_HeapBlock_Stack*)stack.This(); FAULT_ON_NULL(pThis);

    TINYCLR_SET_AND_LEAVE(pThis->Clear());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Collections_Stack::Peek___OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Stack* pThis = (CLR_RT_HeapBlock_Stack*)stack.This(); FAULT_ON_NULL(pThis);
    CLR_RT_HeapBlock*       value;

    TINYCLR_CHECK_HRESULT(pThis->Peek( value ));

    stack.SetResult_Object( value );

    TINYCLR_NOCLEANUP();

}

HRESULT Library_corlib_native_System_Collections_Stack::Pop___OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Stack* pThis = (CLR_RT_HeapBlock_Stack*)stack.This(); FAULT_ON_NULL(pThis);
    CLR_RT_HeapBlock*       value;

    TINYCLR_CHECK_HRESULT(pThis->Pop( value ));

    stack.SetResult_Object( value );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Collections_Stack::Push___VOID__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Stack* pThis = (CLR_RT_HeapBlock_Stack*)stack.This(); FAULT_ON_NULL(pThis);

    TINYCLR_SET_AND_LEAVE(pThis->Push( stack.Arg1().Dereference() ));

    TINYCLR_NOCLEANUP();
}

