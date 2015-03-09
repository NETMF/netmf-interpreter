////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "Spot_InteropSample_Native.h"


static const CLR_RT_MethodHandler method_lookup[] =
{
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::VerifyEnumType___STATIC__I8__MicrosoftSPOTInteropTestEnumDefaultType__MicrosoftSPOTInteropTestEnumUshortType__MicrosoftSPOTInteropTestEnumByteType,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_SignedIntegralValues___STATIC__I8__CHAR__I1__I2__I4__I8,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_SignedIntegralValuesByRef___STATIC__I8__BYREF_CHAR__BYREF_I1__BYREF_I2__BYREF_I4__BYREF_I8,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Circle_SignedIntegralValues___STATIC__VOID__BYREF_CHAR__BYREF_I1__BYREF_I2__BYREF_I4__BYREF_I8,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_UnsignedIntegralValues___STATIC__U8__BOOLEAN__U1__U2__U4__U8,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_UnsignedIntegralValuesByRef___STATIC__I8__BYREF_BOOLEAN__BYREF_U1__BYREF_U2__BYREF_U4__BYREF_U8,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Circle_UnsignedIntegralValues___STATIC__VOID__BYREF_BOOLEAN__BYREF_U1__BYREF_U2__BYREF_U4__BYREF_U8,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_FloatPointValues___STATIC__R8__R4__BYREF_R4__R8__BYREF_R8,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Circle_FloatPointValues___STATIC__VOID__BYREF_R4__BYREF_R8,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::String_GetLength___STATIC__I4__STRING,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::GetVersion___STATIC__STRING,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_Values_Array_Bool___STATIC__I4__SZARRAY_BOOLEAN,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_Values_Array_Byte___STATIC__I4__SZARRAY_U1,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_Values_Array_Int64___STATIC__I4__SZARRAY_I8,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_Values_Array_float___STATIC__R4__SZARRAY_R4,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Add_Values_Array_double___STATIC__R8__SZARRAY_R8,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::Get_Buffer_Address_Array___STATIC__U4__SZARRAY_I4,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::VerifyStructType___STATIC__I8__MicrosoftSPOTInteropTestClassType,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::VerifyClassType___STATIC__I8__MicrosoftSPOTInteropTestClassType,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::VerifyStructType___STATIC__I8__BYREF_MicrosoftSPOTInteropTestClassType,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes::VerifyClassType___STATIC__I8__BYREF_MicrosoftSPOTInteropTestClassType,
    NULL,
    NULL,
    NULL,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestCallback::GenerateInterrupt___STATIC__VOID,
    NULL,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::_ctor___VOID__I1,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::IncreaseAllMembers___I4__I4,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::RetrieveMemberVariables___VOID__BYREF_CHAR__BYREF_I1__BYREF_I2__BYREF_I4__BYREF_I8__BYREF_BOOLEAN__BYREF_U1__BYREF_U2__BYREF_U4__BYREF_U8__BYREF_R4__BYREF_R8,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::TestTwoParams___VOID__I4__R8,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::set_prop_double_data___VOID__R8,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions::get_prop_double_data___R8,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions__TestInternalType::GetValue___I8,
    NULL,
};

const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_InteropSample =
{
    "Microsoft.SPOT.InteropSample", 
    0x764236BF,
    method_lookup
};

