////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include <TinyCLR_Interop.h>
#include "Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestBasicTypes.h"

using namespace Microsoft::SPOT::Interop;

INT64 TestBasicTypes::VerifyEnumType( INT32 param0, UINT16 param1, UINT8 param2, HRESULT &hr )
{
    INT64 retVal = param0 + param1 + param2; 
    return retVal;
}
INT64 TestBasicTypes::Add_SignedIntegralValues( CHAR param0, INT8 param1, INT16 param2, INT32 param3, INT64 param4, HRESULT &hr )
{
	INT64 retVal = param0 + param1 + param2 + param3 + param4; 
	return retVal;
}

INT64 TestBasicTypes::Add_SignedIntegralValuesByRef( CHAR * param0, INT8 * param1, INT16 * param2, INT32 * param3, INT64 * param4, HRESULT &hr )
{
	INT64 retVal = *param0 + *param1 + *param2 + *param3 + *param4; 
	return retVal;
}

void TestBasicTypes::Circle_SignedIntegralValues( CHAR * param0, INT8 * param1, INT16 * param2, INT32 * param3, INT64 * param4, HRESULT &hr )
{
    INT64 tempVal = *param0;
    *param0 = *param1;
    *param1 = (INT8 )*param2;
    *param2 = *param3;
    *param3 = (INT32)*param4;
    *param4 = tempVal;
}

UINT64 TestBasicTypes::Add_UnsignedIntegralValues( INT8 param0, UINT8 param1, UINT16 param2, UINT32 param3, UINT64 param4, HRESULT &hr )
{
	UINT64 retVal = param0 + param1 + param2 + param3 + param4; 
	return retVal;
}

INT64 TestBasicTypes::Add_UnsignedIntegralValuesByRef( INT8 * param0, UINT8 * param1, UINT16 * param2, UINT32 * param3, UINT64 * param4, HRESULT &hr )
{
	INT64 retVal = *param0 + *param1 + *param2 + *param3 + *param4; 
	return retVal;
}

void TestBasicTypes::Circle_UnsignedIntegralValues( INT8 * param0, UINT8 * param1, UINT16 * param2, UINT32 * param3, UINT64 * param4, HRESULT &hr )
{
    UINT64 tempVal = *param0;
    *param0 = *param1;
    *param1 = (UINT8)*param2;
    *param2 = *param3;
    *param3 = (UINT32)*param4;
    *param4 = tempVal;
}

double TestBasicTypes::Add_FloatPointValues( float param0, float * param1, double param2, double * param3, HRESULT &hr )
{
	double retVal = param0 + *param1 + param2 + *param3; 
	return retVal;
}

void TestBasicTypes::Circle_FloatPointValues( float * param0, double * param1, HRESULT &hr )
{
    double tempVal = *param1;
    *param1 = *param0;
    *param0 = (float)tempVal;
}

INT32 TestBasicTypes::String_GetLength( LPCSTR param0, HRESULT &hr )
{
	INT32 retVal = (INT32)hal_strlen_s( param0 );

	return retVal;
}

LPCSTR TestBasicTypes::GetVersion( HRESULT &hr )
{
	LPCSTR retVal = "Interop Version 1.0"; 
	return retVal;
}

template <class T, class R>
static R TestBasicTypes_ProcessArray( T elemArray )
{
	R retVal = 0; 
    for ( UINT32 i = 0; i < elemArray.GetSize(); i++ )
    {
        retVal += (R)elemArray.GetValue( i );
    }
    return retVal;
}

INT32 TestBasicTypes::Add_Values_Array_Bool( CLR_RT_TypedArray_INT8 param0, HRESULT &hr )
{
	return TestBasicTypes_ProcessArray<CLR_RT_TypedArray_INT8, UINT32>( param0 );
}

INT32 TestBasicTypes::Add_Values_Array_Byte( CLR_RT_TypedArray_UINT8 param0, HRESULT &hr )
{
	return TestBasicTypes_ProcessArray<CLR_RT_TypedArray_UINT8, UINT32>( param0 );
}

INT32 TestBasicTypes::Add_Values_Array_Int64( CLR_RT_TypedArray_INT64 param0, HRESULT &hr )
{
	return TestBasicTypes_ProcessArray<CLR_RT_TypedArray_INT64, UINT32>( param0 );
}

float TestBasicTypes::Add_Values_Array_float( CLR_RT_TypedArray_float param0, HRESULT &hr )
{
	return TestBasicTypes_ProcessArray<CLR_RT_TypedArray_float, float>( param0 );
}

double TestBasicTypes::Add_Values_Array_double( CLR_RT_TypedArray_double param0, HRESULT &hr )
{
	return TestBasicTypes_ProcessArray<CLR_RT_TypedArray_double, double>( param0 );
}

UINT32 TestBasicTypes::Get_Buffer_Address_Array( CLR_RT_TypedArray_INT32 param0, HRESULT &hr )
{
    UINT32 retVal = (UINT32)param0.GetBuffer(); 
    return retVal;
}

INT64 TestBasicTypes::VerifyStructType( UNSUPPORTED_TYPE param0, HRESULT &hr )
{
    INT64 retVal = 0; 
    return retVal;
}

INT64 TestBasicTypes::VerifyClassType( UNSUPPORTED_TYPE param0, HRESULT &hr )
{
    INT64 retVal = 0; 
    return retVal;
}

INT64 TestBasicTypes::VerifyStructType( UNSUPPORTED_TYPE * param0, HRESULT &hr )
{
    INT64 retVal = 0; 
    return retVal;
}

INT64 TestBasicTypes::VerifyClassType( UNSUPPORTED_TYPE * param0, HRESULT &hr )
{
    INT64 retVal = 0; 
    return retVal;
}

