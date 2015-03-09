////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#ifndef _SPOT_INTEROPSAMPLE_NATIVE_MICROSOFT_SPOT_INTEROP_TESTBASICTYPES_H_
#define _SPOT_INTEROPSAMPLE_NATIVE_MICROSOFT_SPOT_INTEROP_TESTBASICTYPES_H_

namespace Microsoft
{
    namespace SPOT
    {
        namespace Interop
        {
            struct TestBasicTypes
            {
                // Helper Functions to access fields of managed object
                // Declaration of stubs. These functions are implemented by Interop code developers
                static INT64 VerifyEnumType( INT32 param0, UINT16 param1, UINT8 param2, HRESULT &hr );
                static INT64 Add_SignedIntegralValues( CHAR param0, INT8 param1, INT16 param2, INT32 param3, INT64 param4, HRESULT &hr );
                static INT64 Add_SignedIntegralValuesByRef( CHAR * param0, INT8 * param1, INT16 * param2, INT32 * param3, INT64 * param4, HRESULT &hr );
                static void Circle_SignedIntegralValues( CHAR * param0, INT8 * param1, INT16 * param2, INT32 * param3, INT64 * param4, HRESULT &hr );
                static UINT64 Add_UnsignedIntegralValues( INT8 param0, UINT8 param1, UINT16 param2, UINT32 param3, UINT64 param4, HRESULT &hr );
                static INT64 Add_UnsignedIntegralValuesByRef( INT8 * param0, UINT8 * param1, UINT16 * param2, UINT32 * param3, UINT64 * param4, HRESULT &hr );
                static void Circle_UnsignedIntegralValues( INT8 * param0, UINT8 * param1, UINT16 * param2, UINT32 * param3, UINT64 * param4, HRESULT &hr );
                static double Add_FloatPointValues( float param0, float * param1, double param2, double * param3, HRESULT &hr );
                static void Circle_FloatPointValues( float * param0, double * param1, HRESULT &hr );
                static INT32 String_GetLength( LPCSTR param0, HRESULT &hr );
                static LPCSTR GetVersion( HRESULT &hr );
                static INT32 Add_Values_Array_Bool( CLR_RT_TypedArray_INT8 param0, HRESULT &hr );
                static INT32 Add_Values_Array_Byte( CLR_RT_TypedArray_UINT8 param0, HRESULT &hr );
                static INT32 Add_Values_Array_Int64( CLR_RT_TypedArray_INT64 param0, HRESULT &hr );
                static float Add_Values_Array_float( CLR_RT_TypedArray_float param0, HRESULT &hr );
                static double Add_Values_Array_double( CLR_RT_TypedArray_double param0, HRESULT &hr );
                static UINT32 Get_Buffer_Address_Array( CLR_RT_TypedArray_INT32 param0, HRESULT &hr );
                static INT64 VerifyStructType( UNSUPPORTED_TYPE param0, HRESULT &hr );
                static INT64 VerifyClassType( UNSUPPORTED_TYPE param0, HRESULT &hr );
                static INT64 VerifyStructType( UNSUPPORTED_TYPE * param0, HRESULT &hr );
                static INT64 VerifyClassType( UNSUPPORTED_TYPE * param0, HRESULT &hr );
            };
        }
    }
}
#endif  //_SPOT_INTEROPSAMPLE_NATIVE_MICROSOFT_SPOT_INTEROP_TESTBASICTYPES_H_
