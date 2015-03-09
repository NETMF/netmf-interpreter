////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#ifndef _SPOT_INTEROPSAMPLE_NATIVE_MICROSOFT_SPOT_INTEROP_TESTNONSTATICFUNCTIONS_H_
#define _SPOT_INTEROPSAMPLE_NATIVE_MICROSOFT_SPOT_INTEROP_TESTNONSTATICFUNCTIONS_H_

namespace Microsoft
{
    namespace SPOT
    {
        namespace Interop
        {
            struct TestNonStaticFunctions
            {
                // Helper Functions to access fields of managed object
                static INT32& Get_enum_default_type_data( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_INT32( pMngObj, Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::FIELD__enum_default_type_data ); }

                static UINT8& Get_enum_byte_type_data( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_UINT8( pMngObj, Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::FIELD__enum_byte_type_data ); }

                static UINT16& Get_enum_ushort_type_data( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_UINT16( pMngObj, Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::FIELD__enum_ushort_type_data ); }

                static UNSUPPORTED_TYPE& Get_struct_type_data( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_UNSUPPORTED_TYPE( pMngObj, Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::FIELD__struct_type_data ); }

                static UNSUPPORTED_TYPE& Get_class_type_data( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_UNSUPPORTED_TYPE( pMngObj, Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::FIELD__class_type_data ); }

                static CHAR& Get_char_data( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_CHAR( pMngObj, Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::FIELD__char_data ); }

                static INT8& Get_sbyte_data( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_INT8( pMngObj, Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::FIELD__sbyte_data ); }

                static INT16& Get_short_data( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_INT16( pMngObj, Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::FIELD__short_data ); }

                static INT32& Get_int_data( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_INT32( pMngObj, Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::FIELD__int_data ); }

                static INT64& Get_long_data( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_INT64( pMngObj, Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::FIELD__long_data ); }

                static INT8& Get_bool_data( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_INT8( pMngObj, Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::FIELD__bool_data ); }

                static UINT8& Get_byte_data( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_UINT8( pMngObj, Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::FIELD__byte_data ); }

                static UINT16& Get_ushort_data( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_UINT16( pMngObj, Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::FIELD__ushort_data ); }

                static UINT32& Get_uint_data( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_UINT32( pMngObj, Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::FIELD__uint_data ); }

                static UINT64& Get_ulong_data( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_UINT64( pMngObj, Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::FIELD__ulong_data ); }

                static float& Get_float_data( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_float( pMngObj, Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::FIELD__float_data ); }

                static double& Get_double_data( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_double( pMngObj, Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::FIELD__double_data ); }

                // Declaration of stubs. These functions are implemented by Interop code developers
                static void _ctor( CLR_RT_HeapBlock* pMngObj, INT8 param0, HRESULT &hr );
                static INT32 IncreaseAllMembers( CLR_RT_HeapBlock* pMngObj, INT32 param0, HRESULT &hr );
                static void RetrieveMemberVariables( CLR_RT_HeapBlock* pMngObj, CHAR * param0, INT8 * param1, INT16 * param2, INT32 * param3, INT64 * param4, INT8 * param5, UINT8 * param6, UINT16 * param7, UINT32 * param8, UINT64 * param9, float * param10, double * param11, HRESULT &hr );
                static void TestTwoParams( CLR_RT_HeapBlock* pMngObj, INT32 param0, double param1, HRESULT &hr );
                static void set_prop_double_data( CLR_RT_HeapBlock* pMngObj, double param0, HRESULT &hr );
                static double get_prop_double_data( CLR_RT_HeapBlock* pMngObj, HRESULT &hr );
            };
        }
    }
}
#endif  //_SPOT_INTEROPSAMPLE_NATIVE_MICROSOFT_SPOT_INTEROP_TESTNONSTATICFUNCTIONS_H_
