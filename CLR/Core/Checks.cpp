////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_Checks::VerifyObject( CLR_RT_HeapBlock& top )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    switch(top.DataType())
    {
    case DATATYPE_OBJECT:
    case DATATYPE_BYREF:
        if(top.Dereference() != NULL) TINYCLR_SET_AND_LEAVE(S_OK);
        break;

    case DATATYPE_ARRAY_BYREF:
        if(top.DereferenceArray() != NULL) TINYCLR_SET_AND_LEAVE(S_OK);
        break;

    default:
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    TINYCLR_SET_AND_LEAVE(CLR_E_NULL_REFERENCE);

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_Checks::VerifyArrayReference( CLR_RT_HeapBlock& ref )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* array;

    if(ref.DataType() != DATATYPE_OBJECT)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    array = ref.DereferenceArray();
    if(array == NULL)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_NULL_REFERENCE);
    }

    if(array->DataType() != DATATYPE_SZARRAY)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_Checks::VerifyUnknownInstruction( CLR_OPCODE op )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(CLR_E_UNKNOWN_INSTRUCTION);

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_Checks::VerifyUnsupportedInstruction( CLR_OPCODE op )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(CLR_E_UNSUPPORTED_INSTRUCTION);

    TINYCLR_NOCLEANUP();
}
