////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////
HRESULT CLR_RT_HeapBlock_Stack::Pop( CLR_RT_HeapBlock*& value )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* array   = GetArray();
    CLR_INT32               size    = GetSize();
    CLR_RT_HeapBlock*       removed;

    if(size == 0) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);
    
    removed = (CLR_RT_HeapBlock*)array->GetElement( array->m_numOfElements - size );

    value = removed->Dereference();

    removed->SetObjectReference( NULL );

    SetSize( size - 1 );

    TINYCLR_NOCLEANUP();
}

// May Trigger GC, but parameter value will be protected
HRESULT CLR_RT_HeapBlock_Stack::Push( CLR_RT_HeapBlock* value )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* array    = GetArray();
    CLR_INT32               size     = GetSize();
    CLR_INT32               capacity = array->m_numOfElements;

    if(size == capacity)
    {
        // Set new capacity
        CLR_RT_HeapBlock        newArrayHB;
        CLR_RT_HeapBlock_Array* newArray;

        // Protect value from GC, in case CreateInstance triggers one
        CLR_RT_HeapBlock valueHB; valueHB.SetObjectReference( value );
        CLR_RT_ProtectFromGC gc( valueHB );

        capacity *= 2;

        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( newArrayHB, capacity, g_CLR_RT_WellKnownTypes.m_Object ));

        newArray = newArrayHB.DereferenceArray();

        memcpy( newArray->GetElement( size ), array->GetFirstElement(), size * sizeof(CLR_RT_HeapBlock) );

        SetArray( newArray ); 
        array = newArray;
    }

    size++;

    ((CLR_RT_HeapBlock*)array->GetElement( capacity - size ))->SetObjectReference( value );

    SetSize( size );

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_Stack::Peek( CLR_RT_HeapBlock*& value )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* array = GetArray();
    CLR_INT32               size  = GetSize();

    if(size == 0) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);

    value = ((CLR_RT_HeapBlock*)array->GetElement( array->m_numOfElements - size ))->Dereference();

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_Stack::Clear()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* array = GetArray();
    CLR_INT32               size  = GetSize();

    TINYCLR_CHECK_HRESULT(array->ClearElements( array->m_numOfElements - size, size ));

    SetSize( 0 );

    TINYCLR_NOCLEANUP();
}

//--//

CT_ASSERT(Library_corlib_native_System_Collections_Stack__FIELD___array == Library_corlib_native_System_Collections_Stack::FIELD___array);
CT_ASSERT(Library_corlib_native_System_Collections_Stack__FIELD___size  == Library_corlib_native_System_Collections_Stack::FIELD___size );

