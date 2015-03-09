////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Spot_InteropSample_Native.h"
#include "Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestNonStaticFunctions.h"

using namespace Microsoft::SPOT::Interop;

void TestNonStaticFunctions::_ctor( CLR_RT_HeapBlock* pMngObj, INT8 param0, HRESULT &hr )
{
    Get_char_data( pMngObj )              = param0;
    Get_sbyte_data( pMngObj )             = param0;
    Get_short_data( pMngObj )             = param0;
    Get_int_data( pMngObj )               = param0;
    Get_long_data( pMngObj )              = param0;
    Get_bool_data( pMngObj )              = param0;
    Get_byte_data( pMngObj )              = param0;
    Get_ushort_data( pMngObj )            = param0;
    Get_uint_data( pMngObj )              = param0;
    Get_ulong_data( pMngObj )             = param0;
    Get_float_data( pMngObj )             = param0;
    Get_double_data( pMngObj )            = param0;
    Get_enum_default_type_data( pMngObj ) = param0;
    Get_enum_byte_type_data( pMngObj )    = param0;
    Get_enum_ushort_type_data( pMngObj )  = param0;
}

INT32 TestNonStaticFunctions::IncreaseAllMembers( CLR_RT_HeapBlock* pMngObj, INT32 param0, HRESULT &hr )
{
    INT32 retVal = 0; 
    CHAR &char_data = Get_char_data( pMngObj );
    char_data    += param0; retVal++;
    
    INT8 &sbyte_data = Get_sbyte_data( pMngObj );
    sbyte_data   += param0; retVal++;
    
    INT16 &short_data = Get_short_data( pMngObj );
    short_data   += param0; retVal++;
    
    INT32 &int_data = Get_int_data( pMngObj );
    int_data     += param0; retVal++;
    
    INT64 &long_data = Get_long_data( pMngObj );
    long_data    += param0; retVal++;
    
    INT8 &bool_data = Get_bool_data( pMngObj );
    bool_data    += param0; retVal++;
    
    UINT8 &byte_data = Get_byte_data( pMngObj );
    byte_data    += param0; retVal++;
    
    UINT16 &ushort_data = Get_ushort_data( pMngObj );
    ushort_data  += param0; retVal++;
    
    UINT32 &uint_data = Get_uint_data( pMngObj );
    uint_data    += param0; retVal++;
    
    UINT64 &ulong_data = Get_ulong_data( pMngObj );
    ulong_data   += param0; retVal++;
    
    float &float_data = Get_float_data( pMngObj );
    float_data   += param0; retVal++;
    
    double &double_data = Get_double_data( pMngObj );
    double_data  += param0; retVal++;
    
    INT32 &enum_default_type_data = Get_enum_default_type_data( pMngObj ); 
    enum_default_type_data += param0; retVal++;
    
    UINT8 &enum_byte_type_data = Get_enum_byte_type_data( pMngObj );    
    enum_byte_type_data += param0; retVal++;
    
    UINT16 &enum_ushort_type_data = Get_enum_ushort_type_data( pMngObj );  
    enum_ushort_type_data += param0; retVal++;
    
    return retVal;
}

void TestNonStaticFunctions::RetrieveMemberVariables( CLR_RT_HeapBlock* pMngObj, CHAR * param0, INT8 * param1, INT16 * param2, INT32 * param3, INT64 * param4, INT8 * param5, UINT8 * param6, UINT16 * param7, UINT32 * param8, UINT64 * param9, float * param10, double * param11, HRESULT &hr )
{
    *param0 = Get_char_data( pMngObj );
    *param1 = Get_sbyte_data( pMngObj );
    *param2 = Get_short_data( pMngObj );
    *param3 = Get_int_data( pMngObj );
    *param4 = Get_long_data( pMngObj );
    *param5 = Get_bool_data( pMngObj );
    *param6 = Get_byte_data( pMngObj );
    *param7 = Get_ushort_data( pMngObj );
    *param8 = Get_uint_data( pMngObj );
    *param9 = Get_ulong_data( pMngObj );
    *param10 = Get_float_data( pMngObj );
    *param11 = Get_double_data( pMngObj );
}

void TestNonStaticFunctions::TestTwoParams( CLR_RT_HeapBlock* pMngObj, INT32 param0, double param1, HRESULT &hr )
{
}

void TestNonStaticFunctions::set_prop_double_data( CLR_RT_HeapBlock* pMngObj, double param0, HRESULT &hr )
{
    Get_double_data( pMngObj ) = param0;
}

double TestNonStaticFunctions::get_prop_double_data( CLR_RT_HeapBlock* pMngObj, HRESULT &hr )
{
    double retVal = Get_double_data( pMngObj ); 
    return retVal;
}




