////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_HeapBlock_Queue::Dequeue( CLR_RT_HeapBlock*& value )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* array   = GetArray();
    CLR_INT32               size    = GetSize();
    CLR_INT32               head    = Head();
    CLR_RT_HeapBlock*       removed;

    if(size == 0) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);

    removed = (CLR_RT_HeapBlock*)array->GetElement( head );

    value = removed->Dereference();

    removed->SetObjectReference( NULL );

    SetHead( (head + 1) % array->m_numOfElements );
    
    SetSize( size - 1 );

    TINYCLR_NOCLEANUP();
}

// May Trigger GC, but parameter value will be protected
HRESULT CLR_RT_HeapBlock_Queue::Enqueue( CLR_RT_HeapBlock* value )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* array    = GetArray();
    CLR_INT32               size     = GetSize();
    CLR_INT32               tail     = GetTail();
    CLR_INT32               capacity = array->m_numOfElements;

    if(size == capacity)
    {
        // Set new capacity
        CLR_RT_HeapBlock newArrayHB;
        
        // Protect value from GC, in case CreateInstance triggers one
        CLR_RT_HeapBlock valueHB; valueHB.SetObjectReference( value );
        CLR_RT_ProtectFromGC gc( valueHB );

        capacity *= 2;

        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( newArrayHB, capacity, g_CLR_RT_WellKnownTypes.m_Object ));

        array = newArrayHB.DereferenceArray();

        CopyTo( array, 0 );

        tail = size;

        SetArray( array );
        SetHead ( 0     );
        SetTail ( tail  );
    }

    ((CLR_RT_HeapBlock*)array->GetElement( tail ))->SetObjectReference( value );

    SetTail( (tail + 1) % capacity );

    SetSize( size + 1 );

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_Queue::Peek( CLR_RT_HeapBlock*& value )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(GetSize() == 0) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);

    value = ((CLR_RT_HeapBlock*)GetArray()->GetElement( Head() ))->Dereference();

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_Queue::Clear()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* array = GetArray();
    CLR_INT32               size  = GetSize();
    CLR_INT32               head  =    Head();
    CLR_INT32               tail  = GetTail();

    if(size > 0)
    {
        if(head < tail)
        {
            TINYCLR_CHECK_HRESULT(array->ClearElements( head, size ));
        }
        else
        {
            TINYCLR_CHECK_HRESULT(array->ClearElements( head, array->m_numOfElements - head ));
            TINYCLR_CHECK_HRESULT(array->ClearElements( 0   , tail                          ));
        }
        
        SetSize( 0 );
    }

    SetHead( 0 );
    SetTail( 0 );

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_Queue::ObjArrayMemcpy( CLR_RT_HeapBlock_Array* arraySrc, int indexSrc, CLR_RT_HeapBlock_Array* arrayDst, int indexDst, int length )
{
    TINYCLR_HEADER();

    memcpy( arraySrc->GetElement( indexSrc ), arrayDst->GetElement( indexDst ), length * sizeof(CLR_RT_HeapBlock) );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT CLR_RT_HeapBlock_Queue::CopyTo( CLR_RT_HeapBlock_Array* toArray, CLR_INT32 index )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    
    CLR_RT_HeapBlock_Array* array = GetArray();
    CLR_INT32               size  = GetSize();
    CLR_INT32               head  =    Head();
    CLR_INT32               tail  = GetTail();

    // if the target array is of type Object, we don't need to call the complex Array::Copy() since there will be no casting involved
    HRESULT (*arrayCopy)( CLR_RT_HeapBlock_Array*, int, CLR_RT_HeapBlock_Array*, int, int ) = 
        (toArray->m_typeOfElement == DATATYPE_OBJECT) ? ObjArrayMemcpy : CLR_RT_HeapBlock_Array::Copy;
    
    if(((CLR_INT32)toArray->m_numOfElements) - index < size) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

    if(size > 0)
    {
        if(head < tail)
        {
            TINYCLR_SET_AND_LEAVE(arrayCopy( array, head, toArray, index, size ));
        }
        else
        {
            CLR_INT32 firstPart = array->m_numOfElements - head;

            TINYCLR_CHECK_HRESULT(arrayCopy( array, head, toArray, index            , firstPart ));
            TINYCLR_SET_AND_LEAVE(arrayCopy( array, 0   , toArray, index + firstPart, tail      ));
        }
    }

    TINYCLR_NOCLEANUP();
}

//--//

CT_ASSERT(Library_corlib_native_System_Collections_Queue__FIELD___array == Library_corlib_native_System_Collections_Queue::FIELD___array);
CT_ASSERT(Library_corlib_native_System_Collections_Queue__FIELD___head  == Library_corlib_native_System_Collections_Queue::FIELD___head );
CT_ASSERT(Library_corlib_native_System_Collections_Queue__FIELD___tail  == Library_corlib_native_System_Collections_Queue::FIELD___tail );
CT_ASSERT(Library_corlib_native_System_Collections_Queue__FIELD___size  == Library_corlib_native_System_Collections_Queue::FIELD___size );

