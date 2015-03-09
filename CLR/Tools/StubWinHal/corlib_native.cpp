////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <stdafx.h>
#include "corlib_native.h"

__int64*
Library_corlib_native_System_TimeSpan::GetValuePtr( struct CLR_RT_HeapBlock & )
{
    return NULL;
}

__int64*
Library_corlib_native_System_DateTime::GetValuePtr( struct CLR_RT_HeapBlock & )
{
    return NULL;
}

HRESULT
Library_corlib_native_System_Exception::CreateInstance(
    struct CLR_RT_HeapBlock &,
    struct CLR_RT_TypeDef_Index const &,
    long,
    struct CLR_RT_StackFrame *
    )
{
    return S_OK;
}

HRESULT
Library_corlib_native_System_Exception::CreateInstance(
    struct CLR_RT_HeapBlock &,
    long,
    struct CLR_RT_StackFrame *
    )
{
    return S_OK;
}

HRESULT
Library_corlib_native_System_Exception::SetStackTrace(
    struct CLR_RT_HeapBlock &,
    struct CLR_RT_StackFrame *
    )
{
    return S_OK;
}

struct Library_corlib_native_System_Exception::StackTrace *
Library_corlib_native_System_Exception::GetStackTrace( struct CLR_RT_HeapBlock *,unsigned int & )
{
    return NULL;
}

struct CLR_RT_HeapBlock *
Library_corlib_native_System_Exception::GetTarget( struct CLR_RT_HeapBlock & )
{
    return NULL;
}

