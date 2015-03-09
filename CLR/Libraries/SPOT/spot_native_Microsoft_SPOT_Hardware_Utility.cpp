////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT.h"


HRESULT Library_spot_native_Microsoft_SPOT_Hardware_Utility::ComputeCRC___STATIC__U4__SZARRAY_U1__I4__I4__U4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pArgs;
    CLR_RT_HeapBlock_Array* array;
    int                     offset;
    int                     length;
    CLR_UINT32              crc;
    int                     totLength;

    pArgs      = &(stack.Arg0());
    array      = pArgs[ 0 ].DereferenceArray(); FAULT_ON_NULL(array);
    offset     = pArgs[ 1 ].NumericByRef().s4;
    length     = pArgs[ 2 ].NumericByRef().s4;
    crc        = pArgs[ 3 ].NumericByRef().u4;
    totLength  = array->m_numOfElements;

    if(offset < 0 || offset > totLength)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
    }

    if(length == -1)
    {
        length = totLength - offset;
    }
    else
    {
        if(length < 0 || (offset+length) > totLength)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
        }
    }

    crc = SUPPORT_ComputeCRC( array->GetElement( offset ), length, crc );

    stack.SetResult( crc, DATATYPE_U4 );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_Hardware_Utility::ExtractValueFromArray___STATIC__U4__SZARRAY_U1__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(InsertOrExtractValueFromArray( stack, false ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_Hardware_Utility::InsertValueIntoArray___STATIC__VOID__SZARRAY_U1__I4__I4__U4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(InsertOrExtractValueFromArray( stack, true ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_Hardware_Utility::ExtractRangeFromArray___STATIC__SZARRAY_U1__SZARRAY_U1__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       args = &stack.Arg0();
    CLR_RT_HeapBlock_Array* array;
    CLR_RT_HeapBlock&       ref = stack.PushValueAndClear();

    array = args[ 0 ].DereferenceArray(); FAULT_ON_NULL(array);

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( ref, args[ 2 ].NumericByRefConst().s4, array->ReflectionDataConst() ));

    TINYCLR_CHECK_HRESULT(Library_corlib_native_System_Array::Copy( stack, stack.Arg0(), args[ 1 ].NumericByRefConst().s4, ref, 0, args[ 2 ].NumericByRefConst().s4 ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_Hardware_Utility::CombineArrays___STATIC__SZARRAY_U1__SZARRAY_U1__SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(CombineArrays( stack, stack.Arg0(), 0, -1, stack.Arg1(), 0, -1 ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_Hardware_Utility::CombineArrays___STATIC__SZARRAY_U1__SZARRAY_U1__I4__I4__SZARRAY_U1__I4__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* args = &stack.Arg0();

    TINYCLR_SET_AND_LEAVE(CombineArrays( stack, args[ 0 ], args[ 1 ].NumericByRefConst().s4, args[ 2 ].NumericByRefConst().s4 ,
                                                args[ 3 ], args[ 4 ].NumericByRefConst().s4, args[ 5 ].NumericByRefConst().s4 ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_Hardware_Utility::SetLocalTime___STATIC__VOID__mscorlibSystemDateTime( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    CLR_INT64 utc = 0;
    INT64 utcBefore;
    INT64 utcNow;

    CLR_INT64* val = Library_corlib_native_System_DateTime::GetValuePtr( stack.Arg0() ); FAULT_ON_NULL(val);

    utc = *val - TIME_ZONE_OFFSET;
        
    utcBefore = Time_GetUtcTime();
    utcNow = Time_SetUtcTime( utc, false );

    // correct the uptime
    if(utcNow > utcBefore) 
    {
        g_CLR_RT_ExecutionEngine.m_startTime += (utcNow - utcBefore);
    }
    else
    {
        g_CLR_RT_ExecutionEngine.m_startTime -= (utcBefore - utcNow);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_Hardware_Utility::GetMachineTime___STATIC__mscorlibSystemTimeSpan( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_INT64* pRes = Library_corlib_native_System_TimeSpan::NewObject( stack );

    *pRes = Time_GetMachineTime();

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_native_Microsoft_SPOT_Hardware_Utility::Piezo___STATIC__VOID__U4__U4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pArgs = &(stack.Arg0());

    ::Piezo_Tone( pArgs[ 0 ].NumericByRef().u4, pArgs[ 1 ].NumericByRef().u4 );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_native_Microsoft_SPOT_Hardware_Utility::Backlight___STATIC__VOID__BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    ::BackLight_Set( stack.Arg0().NumericByRef().u4 != 0 );

    TINYCLR_NOCLEANUP_NOLABEL();
}

//--//

HRESULT Library_spot_native_Microsoft_SPOT_Hardware_Utility::InsertOrExtractValueFromArray( CLR_RT_StackFrame& stack, bool fInsert )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* array;
    CLR_INT32               offset;
    CLR_INT32               size;
    CLR_UINT32              res;

    array  = stack.Arg0().DereferenceArray(); FAULT_ON_NULL(array);
    offset = stack.Arg1().NumericByRefConst().s4;
    size   = stack.Arg2().NumericByRefConst().s4;
    res    = 0;

    switch(size)
    {
    case 1:
    case 2:
    case 4:
        if(offset >= 0 && (CLR_UINT32)(offset + size) <= array->m_numOfElements)
        {
            CLR_UINT8* ptr = array->GetElement( offset );

            if(fInsert)
            {
                res = stack.Arg3().NumericByRef().u4;

                memcpy( ptr, &res, size );
            }
            else
            {
                memcpy( &res, ptr, size );

                stack.SetResult( res, DATATYPE_U4 );
            }

            TINYCLR_SET_AND_LEAVE(S_OK);
        }
        break;
    }

    TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_Hardware_Utility::CombineArrays( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock& ref1, int offset1, int count1, CLR_RT_HeapBlock& ref2, int offset2, int count2 )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* array1;
    CLR_RT_HeapBlock_Array* array2;

    array1 = ref1.DereferenceArray(); FAULT_ON_NULL(array1);
    array2 = ref2.DereferenceArray(); FAULT_ON_NULL(array2);

    if(array1->SameHeader( *array2 ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    if(count1 < 0) count1 = array1->m_numOfElements;
    if(count2 < 0) count2 = array2->m_numOfElements;

    {
        CLR_RT_HeapBlock& ref = stack.PushValue();

        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( ref, count1 + count2, array1->ReflectionDataConst() ));

        TINYCLR_CHECK_HRESULT(Library_corlib_native_System_Array::Copy( stack, ref1, offset1, ref,      0, count1 ));
        TINYCLR_CHECK_HRESULT(Library_corlib_native_System_Array::Copy( stack, ref2, offset2, ref, count1, count2 ));
    }

    TINYCLR_NOCLEANUP();
}

