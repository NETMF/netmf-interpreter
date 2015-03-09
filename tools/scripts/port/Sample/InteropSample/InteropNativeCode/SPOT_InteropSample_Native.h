////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#ifndef _SPOT_INTEROPSAMPLE_NATIVE_H_
#define _SPOT_INTEROPSAMPLE_NATIVE_H_

#include <TinyCLR_Interop.h>
struct Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes
{
    TINYCLR_NATIVE_DECLARE(VerifyEnumType___STATIC__I8__MicrosoftSPOTInteropTestEnumDefaultType__MicrosoftSPOTInteropTestEnumUshortType__MicrosoftSPOTInteropTestEnumByteType);
    TINYCLR_NATIVE_DECLARE(Add_SignedIntegralValues___STATIC__I8__CHAR__I1__I2__I4__I8);
    TINYCLR_NATIVE_DECLARE(Add_SignedIntegralValuesByRef___STATIC__I8__BYREF_CHAR__BYREF_I1__BYREF_I2__BYREF_I4__BYREF_I8);
    TINYCLR_NATIVE_DECLARE(Circle_SignedIntegralValues___STATIC__VOID__BYREF_CHAR__BYREF_I1__BYREF_I2__BYREF_I4__BYREF_I8);
    TINYCLR_NATIVE_DECLARE(Add_UnsignedIntegralValues___STATIC__U8__BOOLEAN__U1__U2__U4__U8);
    TINYCLR_NATIVE_DECLARE(Add_UnsignedIntegralValuesByRef___STATIC__I8__BYREF_BOOLEAN__BYREF_U1__BYREF_U2__BYREF_U4__BYREF_U8);
    TINYCLR_NATIVE_DECLARE(Circle_UnsignedIntegralValues___STATIC__VOID__BYREF_BOOLEAN__BYREF_U1__BYREF_U2__BYREF_U4__BYREF_U8);
    TINYCLR_NATIVE_DECLARE(Add_FloatPointValues___STATIC__R8__R4__BYREF_R4__R8__BYREF_R8);
    TINYCLR_NATIVE_DECLARE(Circle_FloatPointValues___STATIC__VOID__BYREF_R4__BYREF_R8);
    TINYCLR_NATIVE_DECLARE(String_GetLength___STATIC__I4__STRING);
    TINYCLR_NATIVE_DECLARE(GetVersion___STATIC__STRING);
    TINYCLR_NATIVE_DECLARE(Add_Values_Array_Bool___STATIC__I4__SZARRAY_BOOLEAN);
    TINYCLR_NATIVE_DECLARE(Add_Values_Array_Byte___STATIC__I4__SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(Add_Values_Array_Int64___STATIC__I4__SZARRAY_I8);
    TINYCLR_NATIVE_DECLARE(Add_Values_Array_float___STATIC__R4__SZARRAY_R4);
    TINYCLR_NATIVE_DECLARE(Add_Values_Array_double___STATIC__R8__SZARRAY_R8);
    TINYCLR_NATIVE_DECLARE(Get_Buffer_Address_Array___STATIC__U4__SZARRAY_I4);
    TINYCLR_NATIVE_DECLARE(VerifyStructType___STATIC__I8__MicrosoftSPOTInteropTestClassType);
    TINYCLR_NATIVE_DECLARE(VerifyClassType___STATIC__I8__MicrosoftSPOTInteropTestClassType);
    TINYCLR_NATIVE_DECLARE(VerifyStructType___STATIC__I8__BYREF_MicrosoftSPOTInteropTestClassType);
    TINYCLR_NATIVE_DECLARE(VerifyClassType___STATIC__I8__BYREF_MicrosoftSPOTInteropTestClassType);

    //--//

};

struct Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestCallback
{
    static const int FIELD__m_CallbackReceived = 5;
    static const int FIELD__m_CallbackRequired = 6;

    TINYCLR_NATIVE_DECLARE(GenerateInterrupt___STATIC__VOID);

    //--//

};

struct Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestClassType
{
    static const int FIELD__FirstClassValue = 1;
    static const int FIELD__SecondClassValue = 2;


    //--//

};

struct Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions
{
    static const int FIELD__enum_default_type_data = 1;
    static const int FIELD__enum_byte_type_data = 2;
    static const int FIELD__enum_ushort_type_data = 3;
    static const int FIELD__struct_type_data = 4;
    static const int FIELD__class_type_data = 5;
    static const int FIELD__char_data = 6;
    static const int FIELD__sbyte_data = 7;
    static const int FIELD__short_data = 8;
    static const int FIELD__int_data = 9;
    static const int FIELD__long_data = 10;
    static const int FIELD__bool_data = 11;
    static const int FIELD__byte_data = 12;
    static const int FIELD__ushort_data = 13;
    static const int FIELD__uint_data = 14;
    static const int FIELD__ulong_data = 15;
    static const int FIELD__float_data = 16;
    static const int FIELD__double_data = 17;

    TINYCLR_NATIVE_DECLARE(_ctor___VOID__I1);
    TINYCLR_NATIVE_DECLARE(IncreaseAllMembers___I4__I4);
    TINYCLR_NATIVE_DECLARE(RetrieveMemberVariables___VOID__BYREF_CHAR__BYREF_I1__BYREF_I2__BYREF_I4__BYREF_I8__BYREF_BOOLEAN__BYREF_U1__BYREF_U2__BYREF_U4__BYREF_U8__BYREF_R4__BYREF_R8);
    TINYCLR_NATIVE_DECLARE(TestTwoParams___VOID__I4__R8);
    TINYCLR_NATIVE_DECLARE(set_prop_double_data___VOID__R8);
    TINYCLR_NATIVE_DECLARE(get_prop_double_data___R8);

    //--//

};

struct Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestStructType
{
    static const int FIELD__FirstStructValue = 1;
    static const int FIELD__SecondStructValue = 2;


    //--//

};

struct Library_Spot_InteropSample_Native_Microsoft_SPOT_Sample_HelloWorldSample
{
    static const int FIELD_STATIC__staticArrayTest = 0;


    //--//

};

struct Library_Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions__TestInternalType
{
    static const int FIELD__m_value = 1;

    TINYCLR_NATIVE_DECLARE(GetValue___I8);

    //--//

};



extern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_InteropSample;

#endif  //_SPOT_INTEROPSAMPLE_NATIVE_H_
