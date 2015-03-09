////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#include "CorLib.h"
#include "Double_decl.h"

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
HRESULT Library_corlib_native_System_Double::CompareTo___STATIC__I4__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    double val = stack.Arg0().NumericByRefConst().r8;
    CLR_UINT32 res = System::Double::CompareTo( d, val );

    stack.PushValue().SetInteger( res );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Double::IsInfinity___STATIC__BOOLEAN__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    bool res = System::Double::IsInfinity( d );

    stack.SetResult_Boolean( res );    

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Double::IsNaN___STATIC__BOOLEAN__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    bool res = System::Double::IsNaN( d );

    stack.SetResult_Boolean( res );    

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Double::IsNegativeInfinity___STATIC__BOOLEAN__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    bool res = System::Double::IsNegativeInfinity( d );

    stack.SetResult_Boolean( res );    

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Double::IsPositiveInfinity___STATIC__BOOLEAN__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    double d = stack.Arg0().NumericByRefConst().r8;
    bool res = System::Double::IsPositiveInfinity( d );

    stack.SetResult_Boolean( res );    

    TINYCLR_NOCLEANUP_NOLABEL();
}
#else
HRESULT Library_corlib_native_System_Double::CompareTo___STATIC__I4__R8__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_INT64 left  = stack.Arg0().NumericByRef().s8;
    CLR_INT64 right = stack.Arg1().NumericByRef().s8;

    stack.SetResult_I4((left < right) ? -1 : (left > right) ? 1 : 0);

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Double::IsInfinity___STATIC__BOOLEAN__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    stack.SetResult_Boolean(false);

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Double::IsNaN___STATIC__BOOLEAN__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    stack.SetResult_Boolean(false);

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Double::IsNegativeInfinity___STATIC__BOOLEAN__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    stack.SetResult_Boolean(false);

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Double::IsPositiveInfinity___STATIC__BOOLEAN__R8( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    stack.SetResult_Boolean(false);

    TINYCLR_NOCLEANUP_NOLABEL();
}
#endif
