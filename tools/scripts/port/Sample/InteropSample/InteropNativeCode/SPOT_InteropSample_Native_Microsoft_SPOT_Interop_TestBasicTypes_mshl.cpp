////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "Spot_InteropSample_Native.h"
#include "Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes.h"

using namespace Microsoft::SPOT::Interop;


HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::VerifyEnumType___STATIC__I8__MicrosoftSPOTInteropTestEnumDefaultType__MicrosoftSPOTInteropTestEnumUshortType__MicrosoftSPOTInteropTestEnumByteType( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        INT32 param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT32( stack, 0, param0 ) );

        UINT16 param1;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT16( stack, 1, param1 ) );

        UINT8 param2;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT8( stack, 2, param2 ) );

        INT64 retVal = TestBasicTypes::VerifyEnumType( param0, param1, param2, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT64( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_SignedIntegralValues___STATIC__I8__CHAR__I1__I2__I4__I8( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CHAR param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_CHAR( stack, 0, param0 ) );

        INT8 param1;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT8( stack, 1, param1 ) );

        INT16 param2;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT16( stack, 2, param2 ) );

        INT32 param3;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT32( stack, 3, param3 ) );

        INT64 param4;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT64( stack, 4, param4 ) );

        INT64 retVal = TestBasicTypes::Add_SignedIntegralValues( param0, param1, param2, param3, param4, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT64( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_SignedIntegralValuesByRef___STATIC__I8__BYREF_CHAR__BYREF_I1__BYREF_I2__BYREF_I4__BYREF_I8( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CHAR * param0;
        UINT8 heapblock0[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_CHAR_ByRef( stack, heapblock0, 0, param0 ) );

        INT8 * param1;
        UINT8 heapblock1[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT8_ByRef( stack, heapblock1, 1, param1 ) );

        INT16 * param2;
        UINT8 heapblock2[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT16_ByRef( stack, heapblock2, 2, param2 ) );

        INT32 * param3;
        UINT8 heapblock3[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT32_ByRef( stack, heapblock3, 3, param3 ) );

        INT64 * param4;
        UINT8 heapblock4[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT64_ByRef( stack, heapblock4, 4, param4 ) );

        INT64 retVal = TestBasicTypes::Add_SignedIntegralValuesByRef( param0, param1, param2, param3, param4, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT64( stack, retVal );

        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock0, 0 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock1, 1 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock2, 2 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock3, 3 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock4, 4 ) );
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Circle_SignedIntegralValues___STATIC__VOID__BYREF_CHAR__BYREF_I1__BYREF_I2__BYREF_I4__BYREF_I8( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CHAR * param0;
        UINT8 heapblock0[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_CHAR_ByRef( stack, heapblock0, 0, param0 ) );

        INT8 * param1;
        UINT8 heapblock1[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT8_ByRef( stack, heapblock1, 1, param1 ) );

        INT16 * param2;
        UINT8 heapblock2[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT16_ByRef( stack, heapblock2, 2, param2 ) );

        INT32 * param3;
        UINT8 heapblock3[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT32_ByRef( stack, heapblock3, 3, param3 ) );

        INT64 * param4;
        UINT8 heapblock4[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT64_ByRef( stack, heapblock4, 4, param4 ) );

        TestBasicTypes::Circle_SignedIntegralValues( param0, param1, param2, param3, param4, hr );
        TINYCLR_CHECK_HRESULT( hr );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock0, 0 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock1, 1 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock2, 2 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock3, 3 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock4, 4 ) );
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_UnsignedIntegralValues___STATIC__U8__BOOLEAN__U1__U2__U4__U8( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        INT8 param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT8( stack, 0, param0 ) );

        UINT8 param1;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT8( stack, 1, param1 ) );

        UINT16 param2;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT16( stack, 2, param2 ) );

        UINT32 param3;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT32( stack, 3, param3 ) );

        UINT64 param4;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT64( stack, 4, param4 ) );

        UINT64 retVal = TestBasicTypes::Add_UnsignedIntegralValues( param0, param1, param2, param3, param4, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_UINT64( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_UnsignedIntegralValuesByRef___STATIC__I8__BYREF_BOOLEAN__BYREF_U1__BYREF_U2__BYREF_U4__BYREF_U8( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        INT8 * param0;
        UINT8 heapblock0[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT8_ByRef( stack, heapblock0, 0, param0 ) );

        UINT8 * param1;
        UINT8 heapblock1[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT8_ByRef( stack, heapblock1, 1, param1 ) );

        UINT16 * param2;
        UINT8 heapblock2[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT16_ByRef( stack, heapblock2, 2, param2 ) );

        UINT32 * param3;
        UINT8 heapblock3[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT32_ByRef( stack, heapblock3, 3, param3 ) );

        UINT64 * param4;
        UINT8 heapblock4[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT64_ByRef( stack, heapblock4, 4, param4 ) );

        INT64 retVal = TestBasicTypes::Add_UnsignedIntegralValuesByRef( param0, param1, param2, param3, param4, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT64( stack, retVal );

        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock0, 0 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock1, 1 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock2, 2 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock3, 3 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock4, 4 ) );
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Circle_UnsignedIntegralValues___STATIC__VOID__BYREF_BOOLEAN__BYREF_U1__BYREF_U2__BYREF_U4__BYREF_U8( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        INT8 * param0;
        UINT8 heapblock0[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT8_ByRef( stack, heapblock0, 0, param0 ) );

        UINT8 * param1;
        UINT8 heapblock1[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT8_ByRef( stack, heapblock1, 1, param1 ) );

        UINT16 * param2;
        UINT8 heapblock2[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT16_ByRef( stack, heapblock2, 2, param2 ) );

        UINT32 * param3;
        UINT8 heapblock3[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT32_ByRef( stack, heapblock3, 3, param3 ) );

        UINT64 * param4;
        UINT8 heapblock4[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT64_ByRef( stack, heapblock4, 4, param4 ) );

        TestBasicTypes::Circle_UnsignedIntegralValues( param0, param1, param2, param3, param4, hr );
        TINYCLR_CHECK_HRESULT( hr );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock0, 0 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock1, 1 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock2, 2 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock3, 3 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock4, 4 ) );
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_FloatPointValues___STATIC__R8__R4__BYREF_R4__R8__BYREF_R8( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        float param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_float( stack, 0, param0 ) );

        float * param1;
        UINT8 heapblock1[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_float_ByRef( stack, heapblock1, 1, param1 ) );

        double param2;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_double( stack, 2, param2 ) );

        double * param3;
        UINT8 heapblock3[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_double_ByRef( stack, heapblock3, 3, param3 ) );

        double retVal = TestBasicTypes::Add_FloatPointValues( param0, param1, param2, param3, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_double( stack, retVal );

        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock1, 1 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock3, 3 ) );
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Circle_FloatPointValues___STATIC__VOID__BYREF_R4__BYREF_R8( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        float * param0;
        UINT8 heapblock0[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_float_ByRef( stack, heapblock0, 0, param0 ) );

        double * param1;
        UINT8 heapblock1[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_double_ByRef( stack, heapblock1, 1, param1 ) );

        TestBasicTypes::Circle_FloatPointValues( param0, param1, hr );
        TINYCLR_CHECK_HRESULT( hr );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock0, 0 ) );
        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock1, 1 ) );
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::String_GetLength___STATIC__I4__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        LPCSTR param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_LPCSTR( stack, 0, param0 ) );

        INT32 retVal = TestBasicTypes::String_GetLength( param0, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::GetVersion___STATIC__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        LPCSTR retVal = TestBasicTypes::GetVersion( hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_LPCSTR( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_Values_Array_Bool___STATIC__I4__SZARRAY_BOOLEAN( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_TypedArray_INT8 param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT8_ARRAY( stack, 0, param0 ) );

        INT32 retVal = TestBasicTypes::Add_Values_Array_Bool( param0, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_Values_Array_Byte___STATIC__I4__SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_TypedArray_UINT8 param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT8_ARRAY( stack, 0, param0 ) );

        INT32 retVal = TestBasicTypes::Add_Values_Array_Byte( param0, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_Values_Array_Int64___STATIC__I4__SZARRAY_I8( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_TypedArray_INT64 param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT64_ARRAY( stack, 0, param0 ) );

        INT32 retVal = TestBasicTypes::Add_Values_Array_Int64( param0, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT32( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_Values_Array_float___STATIC__R4__SZARRAY_R4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_TypedArray_float param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_float_ARRAY( stack, 0, param0 ) );

        float retVal = TestBasicTypes::Add_Values_Array_float( param0, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_float( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_Values_Array_double___STATIC__R8__SZARRAY_R8( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_TypedArray_double param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_double_ARRAY( stack, 0, param0 ) );

        double retVal = TestBasicTypes::Add_Values_Array_double( param0, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_double( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Get_Buffer_Address_Array___STATIC__U4__SZARRAY_I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_TypedArray_INT32 param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_INT32_ARRAY( stack, 0, param0 ) );

        UINT32 retVal = TestBasicTypes::Get_Buffer_Address_Array( param0, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_UINT32( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::VerifyStructType___STATIC__I8__MicrosoftSPOTInteropTestClassType( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        UNSUPPORTED_TYPE param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UNSUPPORTED_TYPE( stack, 0, param0 ) );

        INT64 retVal = TestBasicTypes::VerifyStructType( param0, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT64( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::VerifyClassType___STATIC__I8__MicrosoftSPOTInteropTestClassType( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        UNSUPPORTED_TYPE param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UNSUPPORTED_TYPE( stack, 0, param0 ) );

        INT64 retVal = TestBasicTypes::VerifyClassType( param0, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT64( stack, retVal );

    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::VerifyStructType___STATIC__I8__BYREF_MicrosoftSPOTInteropTestClassType( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        UNSUPPORTED_TYPE * param0;
        UINT8 heapblock0[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UNSUPPORTED_TYPE_ByRef( stack, heapblock0, 0, param0 ) );

        INT64 retVal = TestBasicTypes::VerifyStructType( param0, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT64( stack, retVal );

        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock0, 0 ) );
    }
    TINYCLR_NOCLEANUP();
}

HRESULT Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::VerifyClassType___STATIC__I8__BYREF_MicrosoftSPOTInteropTestClassType( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;
    {
        UNSUPPORTED_TYPE * param0;
        UINT8 heapblock0[CLR_RT_HEAP_BLOCK_SIZE];
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UNSUPPORTED_TYPE_ByRef( stack, heapblock0, 0, param0 ) );

        INT64 retVal = TestBasicTypes::VerifyClassType( param0, hr );
        TINYCLR_CHECK_HRESULT( hr );
        SetResult_INT64( stack, retVal );

        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock0, 0 ) );
    }
    TINYCLR_NOCLEANUP();
}
