////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "CorLib.h"


HRESULT Library_corlib_native_System_Collections_ArrayList::get_Item___OBJECT__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_ArrayList* pThis = (CLR_RT_HeapBlock_ArrayList*)stack.This(); FAULT_ON_NULL(pThis);
    CLR_RT_HeapBlock*           value;

    TINYCLR_CHECK_HRESULT(pThis->GetItem( stack.Arg1().NumericByRef().s4, value ));

    stack.SetResult_Object( value );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Collections_ArrayList::set_Item___VOID__I4__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_ArrayList* pThis = (CLR_RT_HeapBlock_ArrayList*)stack.This(); FAULT_ON_NULL(pThis);

    TINYCLR_SET_AND_LEAVE(pThis->SetItem( stack.Arg1().NumericByRef().s4, stack.Arg2().Dereference() ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Collections_ArrayList::Add___I4__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_ArrayList* pThis = (CLR_RT_HeapBlock_ArrayList*)stack.This(); FAULT_ON_NULL(pThis);
    CLR_INT32                   index;

    TINYCLR_CHECK_HRESULT(pThis->Add( stack.Arg1().Dereference(), index ));

    stack.SetResult_I4( index );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Collections_ArrayList::Clear___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_ArrayList* pThis = (CLR_RT_HeapBlock_ArrayList*)stack.This(); FAULT_ON_NULL(pThis);

    TINYCLR_SET_AND_LEAVE(pThis->Clear());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Collections_ArrayList::Insert___VOID__I4__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_ArrayList* pThis = (CLR_RT_HeapBlock_ArrayList*)stack.This(); FAULT_ON_NULL(pThis);

    TINYCLR_SET_AND_LEAVE(pThis->Insert( stack.Arg1().NumericByRef().s4, stack.Arg2().Dereference() ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Collections_ArrayList::RemoveAt___VOID__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_ArrayList* pThis = (CLR_RT_HeapBlock_ArrayList*)stack.This(); FAULT_ON_NULL(pThis);

    TINYCLR_SET_AND_LEAVE(pThis->RemoveAt( stack.Arg1().NumericByRef().s4 ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Collections_ArrayList::SetCapacity___VOID__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_ArrayList* pThis = (CLR_RT_HeapBlock_ArrayList*)stack.This(); FAULT_ON_NULL(pThis);

    TINYCLR_SET_AND_LEAVE(pThis->SetCapacity( stack.Arg1().NumericByRef().s4 ));

    TINYCLR_NOCLEANUP();
}

