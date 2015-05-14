////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_RT_GetVersion( UINT16* pMajor, UINT16* pMinor, UINT16* pBuild, UINT16* pRevision )
{
    NATIVE_PROFILE_CLR_CORE();
    if (pMajor) *pMajor = VERSION_MAJOR;
    if (pMinor) *pMinor = VERSION_MINOR;
    if (pBuild) *pBuild = VERSION_BUILD;
    if (pRevision) *pRevision = VERSION_REVISION;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_ArrayListHelper::PrepareArrayList( CLR_RT_HeapBlock& thisRef, int count, int capacity )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = thisRef.Dereference(); FAULT_ON_NULL(pThis);

    if(count > capacity || capacity < 1)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
    }

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( pThis[ FIELD__m_items ], capacity, g_CLR_RT_WellKnownTypes.m_Object ));

    pThis[ FIELD__m_size ].NumericByRef().s4 = count;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_ArrayListHelper::ExtractArrayFromArrayList( CLR_RT_HeapBlock & thisRef, CLR_RT_HeapBlock_Array* & array, int& count, int& capacity )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis;
    CLR_RT_HeapBlock* items;

    pThis = thisRef.Dereference(); FAULT_ON_NULL(pThis);
    items = &pThis[ FIELD__m_items ];

    if(items->DataType() != DATATYPE_OBJECT)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    array = items->DereferenceArray(); FAULT_ON_NULL(array);

    if(array->DataType() != DATATYPE_SZARRAY)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    capacity = array->m_numOfElements;
    count    = pThis[ FIELD__m_size ].NumericByRef().s4;

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT CLR_RT_ByteArrayReader::Init( const UINT8* src, UINT32 srcSize )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if (src == NULL || srcSize == 0) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

    source     = src;
    sourceSize = srcSize;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_ByteArrayReader::Read( void* dst, UINT32 size )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if (size > sourceSize) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    memcpy( dst, source, size );

    source     += size;
    sourceSize -= size;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_ByteArrayReader::Read1Byte( void* dst )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if (1 > sourceSize) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    *(UINT8*)dst = *source;

    source++;
    sourceSize--;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_ByteArrayReader::Skip( UINT32 size )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if (size > sourceSize) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    source     += size;
    sourceSize -= size;

    TINYCLR_NOCLEANUP();
}
