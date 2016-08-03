////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"

CLR_INT64 s_UTCMask   = ULONGLONGCONSTANT(0x8000000000000000);
CLR_INT64 s_TickMask  = ULONGLONGCONSTANT(0x7FFFFFFFFFFFFFFF);


HRESULT Library_corlib_native_System_DateTime::_ctor___VOID__I4__I4__I4__I4__I4__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pArg = &(stack.Arg1());
    SYSTEMTIME        st;

    TINYCLR_CLEAR(st);
    st.wYear         = pArg[ 0 ].NumericByRef().s4;
    st.wMonth        = pArg[ 1 ].NumericByRef().s4;
    st.wDay          = pArg[ 2 ].NumericByRef().s4;
    st.wHour         = pArg[ 3 ].NumericByRef().s4;
    st.wMinute       = pArg[ 4 ].NumericByRef().s4;
    st.wSecond       = pArg[ 5 ].NumericByRef().s4;
    st.wMilliseconds = pArg[ 6 ].NumericByRef().s4;

    /// Our current supported range is between 1601 and 3000. Years before 1582 requires different calculation (see explanation
    /// in time_decl.h), same way years after 3000 will not hold the simple arithmatic which we are using. More complex calculations
    /// outside these range are not worth the CPU cycle and codesize.
    if ((st.wYear < 1601) || (st.wYear > 3000) ||
        (st.wMonth < 1) || (st.wMonth > 12) ||
        (st.wDay < 1) || (st.wDay > 31)) 
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
    }
    

    Compress( stack, st );

    TINYCLR_NOCLEANUP();   
}

HRESULT Library_corlib_native_System_DateTime::get_Day___I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    SYSTEMTIME st; Expand( stack, st );

    stack.SetResult_I4( st.wDay );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_DateTime::get_DayOfWeek___SystemDayOfWeek( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    SYSTEMTIME st; Expand( stack, st );

    stack.SetResult_I4( st.wDayOfWeek );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_DateTime::get_DayOfYear___I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    SYSTEMTIME st ; 
    INT32 days;
    Expand( stack, st );

    TINYCLR_CHECK_HRESULT(Time_AccDaysInMonth( st.wYear, st.wMonth, &days ));
    days += st.wDay;

    stack.SetResult_I4( days );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_DateTime::get_Hour___I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    SYSTEMTIME st; Expand( stack, st );

    stack.SetResult_I4( st.wHour );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_DateTime::get_Millisecond___I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    SYSTEMTIME st; Expand( stack, st );

    stack.SetResult_I4( st.wMilliseconds );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_DateTime::get_Minute___I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    SYSTEMTIME st; Expand( stack, st );

    stack.SetResult_I4( st.wMinute );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_DateTime::get_Month___I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    SYSTEMTIME st; Expand( stack, st );

    stack.SetResult_I4( st.wMonth );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_DateTime::get_Second___I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    SYSTEMTIME st; Expand( stack, st );

    stack.SetResult_I4( st.wSecond );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_DateTime::get_Year___I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    SYSTEMTIME st; Expand( stack, st );

    stack.SetResult_I4( st.wYear );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_DateTime::ToLocalTime___SystemDateTime( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_INT64*                pThis;
    CLR_INT64*                pRes;

    pThis = Library_corlib_native_System_DateTime::GetValuePtr( stack ); FAULT_ON_NULL(pThis);
    pRes  = Library_corlib_native_System_DateTime::NewObject  ( stack );

    if ( *pThis & s_UTCMask )
    {
        // The most significant bit of *val keeps flag if time is UTC.
        // We cannot change *val, so we create copy and clear the bit.
        CLR_INT64 ticks = *pThis & s_TickMask;

        *pRes = ticks + TIME_ZONE_OFFSET;
    }
    else
    {
        // If *pThis is already local time - we do not need conversion.
        *pRes = *pThis;
    }
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_DateTime::ToUniversalTime___SystemDateTime( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_INT64*                pThis;
    CLR_INT64*                pRes;

    pThis = Library_corlib_native_System_DateTime::GetValuePtr( stack ); FAULT_ON_NULL(pThis);
    pRes  = Library_corlib_native_System_DateTime::NewObject  ( stack );

    if ( !(*pThis & s_UTCMask) )
    {
        *pRes = *pThis - TIME_ZONE_OFFSET;
        // In converted time we need to set UTC mask
        *pRes |= s_UTCMask;
    }
    else
    {
        // If *pThis is already UTS time - we do not need conversion.
        *pRes = *pThis;
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_DateTime::DaysInMonth___STATIC__I4__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_INT32 year  = stack.Arg0().NumericByRef().s4;
    CLR_INT32 month = stack.Arg1().NumericByRef().s4;
    CLR_INT32 days  = 0;

    Time_DaysInMonth( year, month, &days );

    stack.SetResult_I4( days );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_DateTime::get_Now___STATIC__SystemDateTime( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_INT64* pRes = NewObject( stack );

    *pRes = Time_GetLocalTime();

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_DateTime::get_UtcNow___STATIC__SystemDateTime( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_INT64* pRes = NewObject( stack );

    *pRes = Time_GetUtcTime() | s_UTCMask;

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_DateTime::get_Today___STATIC__SystemDateTime( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_INT64* pRes = NewObject( stack );

    {
        SYSTEMTIME st; 
        Time_ToSystemTime( Time_GetLocalTime(), &st );

        st.wHour         = 0;
        st.wMinute       = 0;
        st.wSecond       = 0;
        st.wMilliseconds = 0;

        *pRes = Time_FromSystemTime( &st );
    }

    TINYCLR_NOCLEANUP_NOLABEL();
}

//--//

CLR_INT64* Library_corlib_native_System_DateTime::NewObject( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock& ref = stack.PushValue();

    ref.SetDataId( CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_DATETIME, 0, 1) );
    ref.ClearData(                                                  );

    return (CLR_INT64*)&ref.NumericByRef().s8;
}

CLR_INT64* Library_corlib_native_System_DateTime::GetValuePtr( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    return GetValuePtr( stack.Arg0() );
}

CLR_INT64* Library_corlib_native_System_DateTime::GetValuePtr( CLR_RT_HeapBlock& ref )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock* obj = &ref;
    CLR_DataType      dt  = obj->DataType();

    if(dt == DATATYPE_OBJECT || dt == DATATYPE_BYREF)
    {
        obj = obj->Dereference(); if(!obj) return NULL;

        dt = obj->DataType();
    }

    if(dt == DATATYPE_DATETIME)
    {
        return (CLR_INT64*)&obj->NumericByRef().s8;
    }

    if(dt == DATATYPE_I8)
    {
        return (CLR_INT64*)&obj->NumericByRef().s8;
    }

    if(dt == DATATYPE_VALUETYPE && obj->ObjectCls().m_data == g_CLR_RT_WellKnownTypes.m_DateTime.m_data)
    {
        return (CLR_INT64*)&obj[ FIELD__m_ticks ].NumericByRef().s8;
    }

    return NULL;
}

void Library_corlib_native_System_DateTime::Expand( CLR_RT_StackFrame& stack, SYSTEMTIME& st  )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_INT64* val = GetValuePtr( stack );

    if(val)
    {
    // The most significant bit of *val keeps flag if time is UTC.
    // We cannot change *val, so we create copy and clear the bit.
        CLR_INT64 ticks = *val & s_TickMask;
        Time_ToSystemTime( ticks, &st );
    }
}

// Compress function always creates local time.
void Library_corlib_native_System_DateTime::Compress( CLR_RT_StackFrame& stack, const SYSTEMTIME& st )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_INT64* val = GetValuePtr( stack );

    if(val) 
    {
        *val = Time_FromSystemTime( &st );
    }
}
