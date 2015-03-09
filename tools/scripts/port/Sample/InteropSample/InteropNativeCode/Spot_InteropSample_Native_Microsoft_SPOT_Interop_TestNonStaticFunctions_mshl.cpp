////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "Spot_InteropSample_Native.h"
#include "Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions.h"

using namespace Microsoft::SPOT::Interop;


HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::_ctor___VOID__I1( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );

        FAULT_ON_NULL(pMngObj);

        INT8 param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT8( stack, 1, param0 ) );

        TestNonStaticFunctions::_ctor( pMngObj,  param0, hr );
        TINYCLR_CHECK_HRESULT( hr );
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::IncreaseAllMembers___I4__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );

        FAULT_ON_NULL(pMngObj);

        INT32 param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT32( stack, 1, param0 ) );

        INT32 retVal = TestNonStaticFunctions::IncreaseAllMembers( pMngObj,  param0, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::RetrieveMemberVariables___VOID__BYREF_CHAR__BYREF_I1__BYREF_I2__BYREF_I4__BYREF_I8__BYREF_BOOLEAN__BYREF_U1__BYREF_U2__BYREF_U4__BYREF_U8__BYREF_R4__BYREF_R8( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );

        FAULT_ON_NULL(pMngObj);

        CHAR * param0;
        UINT8 heapblock0[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_CHAR_ByRef( stack, heapblock0, 1, param0 ) );

        INT8 * param1;
        UINT8 heapblock1[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT8_ByRef( stack, heapblock1, 2, param1 ) );

        INT16 * param2;
        UINT8 heapblock2[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT16_ByRef( stack, heapblock2, 3, param2 ) );

        INT32 * param3;
        UINT8 heapblock3[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT32_ByRef( stack, heapblock3, 4, param3 ) );

        INT64 * param4;
        UINT8 heapblock4[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT64_ByRef( stack, heapblock4, 5, param4 ) );

        INT8 * param5;
        UINT8 heapblock5[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT8_ByRef( stack, heapblock5, 6, param5 ) );

        UINT8 * param6;
        UINT8 heapblock6[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT8_ByRef( stack, heapblock6, 7, param6 ) );

        UINT16 * param7;
        UINT8 heapblock7[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT16_ByRef( stack, heapblock7, 8, param7 ) );

        UINT32 * param8;
        UINT8 heapblock8[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT32_ByRef( stack, heapblock8, 9, param8 ) );

        UINT64 * param9;
        UINT8 heapblock9[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT64_ByRef( stack, heapblock9, 10, param9 ) );

        float * param10;
        UINT8 heapblock10[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_float_ByRef( stack, heapblock10, 11, param10 ) );

        double * param11;
        UINT8 heapblock11[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_double_ByRef( stack, heapblock11, 12, param11 ) );

        TestNonStaticFunctions::RetrieveMemberVariables( pMngObj,  param0, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, hr );
        TINYCLR_CHECK_HRESULT( hr );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock0, 1 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock1, 2 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock2, 3 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock3, 4 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock4, 5 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock5, 6 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock6, 7 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock7, 8 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock8, 9 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock9, 10 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock10, 11 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock11, 12 ) );
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::TestTwoParams___VOID__I4__R8( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );

        FAULT_ON_NULL(pMngObj);

        INT32 param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT32( stack, 1, param0 ) );

        double param1;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_double( stack, 2, param1 ) );

        TestNonStaticFunctions::TestTwoParams( pMngObj,  param0, param1, hr );
        TINYCLR_CHECK_HRESULT( hr );
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::set_prop_double_data___VOID__R8( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );

        FAULT_ON_NULL(pMngObj);

        double param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_double( stack, 1, param0 ) );

        TestNonStaticFunctions::set_prop_double_data( pMngObj,  param0, hr );
        TINYCLR_CHECK_HRESULT( hr );
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::get_prop_double_data___R8( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );

        FAULT_ON_NULL(pMngObj);

        double retVal = TestNonStaticFunctions::get_prop_double_data( pMngObj,  hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_double( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}
