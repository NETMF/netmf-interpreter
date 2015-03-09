////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"


HRESULT Library_corlib_native_System_TimeSpan::Equals___BOOLEAN__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_CHECK_HRESULT(CompareTo___I4__OBJECT( stack ));

    {
        CLR_RT_HeapBlock& res = stack.TopValue();

        res.SetBoolean( res.NumericByRef().s4 == 0 );
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_TimeSpan::ToString___STRING( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    char       rgBuffer[ 128 ];
    LPSTR      szBuffer =           rgBuffer;
    size_t     iBuffer  = ARRAYSIZE(rgBuffer);
    CLR_INT64* val      = GetValuePtr( stack ); FAULT_ON_NULL(val);

    Time_TimeSpanToStringEx( *val, szBuffer, iBuffer );

    TINYCLR_SET_AND_LEAVE(stack.SetResult_String( rgBuffer ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_TimeSpan::_ctor___VOID__I4__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pArgs = &(stack.Arg1());
    CLR_INT64*        val   = GetValuePtr( stack ); FAULT_ON_NULL(val);

    ConstructTimeSpan( val, 0, pArgs[ 0 ].NumericByRef().s4, pArgs[ 1 ].NumericByRef().s4, pArgs[ 2 ].NumericByRef().s4, 0 );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_TimeSpan::_ctor___VOID__I4__I4__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pArgs = &(stack.Arg1());
    CLR_INT64*        val   = GetValuePtr( stack ); FAULT_ON_NULL(val);

    ConstructTimeSpan( val, pArgs[ 0 ].NumericByRef().s4, pArgs[ 1 ].NumericByRef().s4, pArgs[ 2 ].NumericByRef().s4, pArgs[ 3 ].NumericByRef().s4, 0 );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_TimeSpan::_ctor___VOID__I4__I4__I4__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pArgs = &(stack.Arg1());
    CLR_INT64*        val   = GetValuePtr( stack ); FAULT_ON_NULL(val);

    ConstructTimeSpan( val, pArgs[ 0 ].NumericByRef().s4, pArgs[ 1 ].NumericByRef().s4, pArgs[ 2 ].NumericByRef().s4, pArgs[ 3 ].NumericByRef().s4, pArgs[ 4 ].NumericByRef().s4 );

    TINYCLR_NOCLEANUP();
}

void Library_corlib_native_System_TimeSpan::ConstructTimeSpan( CLR_INT64* val, CLR_INT32 days, CLR_INT32 hours, CLR_INT32 minutes, CLR_INT32 seconds, CLR_INT32 msec )

{
    *val = ((CLR_INT64)days) * TIME_CONVERSION__ONEDAY   +
           hours * TIME_CONVERSION__ONEHOUR              +
           minutes * TIME_CONVERSION__ONEMINUTE          +
           seconds * TIME_CONVERSION__ONESECOND;

    *val *= 1000;
    *val += msec;
    *val *= TIME_CONVERSION__TICKUNITS;
}

HRESULT Library_corlib_native_System_TimeSpan::CompareTo___I4__OBJECT( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pLeft  = stack.This();
    CLR_RT_HeapBlock* pRight = stack.Arg1().Dereference();

    if(pRight)
    {
        TINYCLR_SET_AND_LEAVE(Compare___STATIC__I4__SystemTimeSpan__SystemTimeSpan( stack ));
    }

    stack.SetResult_I4( 1 );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_TimeSpan::Compare___STATIC__I4__SystemTimeSpan__SystemTimeSpan( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_INT64*       pLeft;
    CLR_INT64*       pRight;
    CLR_RT_HeapBlock resLeft;
    CLR_RT_HeapBlock resRight;

    pLeft  = Library_corlib_native_System_TimeSpan::GetValuePtr( stack        ); FAULT_ON_NULL(pLeft);
    pRight = Library_corlib_native_System_TimeSpan::GetValuePtr( stack.Arg1() ); FAULT_ON_NULL(pRight);

    resLeft .SetInteger( *pLeft  );
    resRight.SetInteger( *pRight );

    stack.SetResult_I4( CLR_RT_HeapBlock::Compare_Signed_Values( resLeft, resRight ) );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_TimeSpan::Equals___STATIC__BOOLEAN__SystemTimeSpan__SystemTimeSpan( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_CHECK_HRESULT(Compare___STATIC__I4__SystemTimeSpan__SystemTimeSpan( stack ));

    stack.ConvertResultToBoolean();

    TINYCLR_NOCLEANUP();
}

//--//

CLR_INT64* Library_corlib_native_System_TimeSpan::NewObject( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock& ref = stack.PushValue();

    ref.SetDataId( CLR_RT_HEAPBLOCK_RAW_ID( DATATYPE_TIMESPAN, 0, 1 ) );
    ref.ClearData(                                                    );

    return (CLR_INT64*)&ref.NumericByRef().s8;
}

CLR_INT64* Library_corlib_native_System_TimeSpan::GetValuePtr( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    return GetValuePtr( stack.Arg0() );
}

CLR_INT64* Library_corlib_native_System_TimeSpan::GetValuePtr( CLR_RT_HeapBlock& ref )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock* obj = &ref;
    CLR_DataType      dt  = obj->DataType();

    if(dt == DATATYPE_OBJECT || dt == DATATYPE_BYREF)
    {
        obj = obj->Dereference(); if(!obj) return NULL;

        dt = obj->DataType();
    }

    if(dt == DATATYPE_TIMESPAN)
    {
        return (CLR_INT64*)&obj->NumericByRef().s8;
    }

    if(dt == DATATYPE_I8)
    {
        return (CLR_INT64*)&obj->NumericByRef().s8;
    }

    if(dt == DATATYPE_VALUETYPE && obj->ObjectCls().m_data == g_CLR_RT_WellKnownTypes.m_TimeSpan.m_data)
    {
        return (CLR_INT64*)&obj[ FIELD__m_ticks ].NumericByRef().s8;
    }

    return NULL;
}
