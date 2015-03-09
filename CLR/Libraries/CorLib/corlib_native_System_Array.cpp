////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"


HRESULT Library_corlib_native_System_Array::System_Collections_IList_get_Item___OBJECT__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock& thisRef = stack.ThisRef();
    int               index   = stack.Arg1().NumericByRef().s4;

    CLR_RT_HeapBlock  ref; TINYCLR_CHECK_HRESULT(ref.InitializeArrayReference( thisRef, index ));

    {
        CLR_RT_HeapBlock& top = stack.PushValueAndClear();

        TINYCLR_CHECK_HRESULT(top.LoadFromReference    ( ref ));
        TINYCLR_CHECK_HRESULT(top.PerformBoxingIfNeeded(     ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Array::System_Collections_IList_set_Item___VOID__I4__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock& thisRef = stack.ThisRef();
    int               index   = stack.Arg1().NumericByRef().s4;

    CLR_RT_HeapBlock  ref; TINYCLR_CHECK_HRESULT(ref.InitializeArrayReference( thisRef, index ));

    TINYCLR_SET_AND_LEAVE(stack.Arg2().StoreToReference( ref, 0 ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Array::get_Length___I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_CHECK_HRESULT(CLR_Checks::VerifyArrayReference( stack.Arg0() ));

    stack.SetResult_I4( stack.Arg0().DereferenceArray()->m_numOfElements );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Array::CreateInstance___STATIC__SystemArray__SystemType__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDef_Instance td;
    CLR_RT_HeapBlock* hbType = stack.Arg0().Dereference();

    TINYCLR_CHECK_HRESULT(Library_corlib_native_System_RuntimeType::GetTypeDescriptor( *hbType, td ));

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( stack.PushValue(), stack.Arg1().NumericByRefConst().s4, td ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Array::Copy___STATIC__VOID__SystemArray__I4__SystemArray__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(Copy( stack, stack.Arg0(), stack.Arg1().NumericByRef().s4, stack.Arg2(), stack.Arg3().NumericByRef().s4, stack.Arg4().NumericByRef().s4 ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Array::Clear___STATIC__VOID__SystemArray__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(Clear( stack, stack.Arg0(), stack.Arg1().NumericByRef().s4, stack.Arg2().NumericByRef().s4 ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Array::TrySZIndexOf___STATIC__BOOLEAN__SystemArray__I4__I4__OBJECT__BYREF_I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_INT32 start = stack.Arg1().NumericByRefConst().s4;
    CLR_INT32 count = stack.Arg2().NumericByRefConst().s4;    
    CLR_INT32 stop  = start + count;

    TINYCLR_SET_AND_LEAVE(TrySZIndexOf( stack, stack.Arg0(), stack.Arg3(), start, stop, true, stack.Arg4() ));

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT Library_corlib_native_System_Array::Clear( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock& arg, int index, int length )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* array;
    int                     numElem;

    TINYCLR_CHECK_HRESULT(CLR_Checks::VerifyArrayReference( arg ));

    array    = arg.DereferenceArray();
    numElem  = array->m_numOfElements;

    if(length == -1) length = numElem;

    if(length         < 0       ||
       index          < 0       ||
       length + index > numElem  )
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
    }

    TINYCLR_SET_AND_LEAVE(array->ClearElements( index, length ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Array::Copy( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock& argSrc, int indexSrc, CLR_RT_HeapBlock& argDst, int indexDst, int length )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(length)
    {
        TINYCLR_CHECK_HRESULT(CLR_Checks::VerifyArrayReference( argSrc ));
        TINYCLR_CHECK_HRESULT(CLR_Checks::VerifyArrayReference( argDst ));

        TINYCLR_SET_AND_LEAVE(CLR_RT_HeapBlock_Array::Copy( argSrc.DereferenceArray(), indexSrc, argDst.DereferenceArray(), indexDst, length ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_Array::TrySZIndexOf( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock& argSrc, CLR_RT_HeapBlock& match, int start, int stop, bool fForward, CLR_RT_HeapBlock& retValue )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    int index;
    bool fRet;
    CLR_RT_HeapBlock_Array* array = argSrc.DereferenceArray();

    TINYCLR_CHECK_HRESULT(CLR_Checks::VerifyArrayReference( argSrc ));

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::IndexOf( array, match, start, stop, fForward, index ));

    _ASSERTE(retValue.DataType() == DATATYPE_BYREF);

    retValue.Dereference()->SetInteger( index );

    fRet = (index >= 0);

    if(array->m_typeOfElement <= DATATYPE_LAST_PRIMITIVE)
    {
        //Primitives do not override Equals, so this check is good enough.  
        //If match is not found, index == -1, so the managed Array.IndexOf will
        //skip the more expensive Object.Equals call.

        _ASSERTE(FIMPLIES(!fRet, index == -1));
        fRet = true;
    }

    stack.SetResult_Boolean( fRet );

    TINYCLR_NOCLEANUP();
}

