////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_HeapBlock_ArrayList::GetItem( CLR_INT32 index, CLR_RT_HeapBlock*& value )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(index < 0 || index >= GetSize()) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    value = ((CLR_RT_HeapBlock*)GetItems()->GetElement( index ))->Dereference();

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_ArrayList::SetItem( CLR_INT32 index, CLR_RT_HeapBlock* value )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(index < 0 || index >= GetSize()) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    ((CLR_RT_HeapBlock*)GetItems()->GetElement( index ))->SetObjectReference( value );

    TINYCLR_NOCLEANUP();
}

// May Trigger GC, but parameter value will be protected
HRESULT CLR_RT_HeapBlock_ArrayList::Add( CLR_RT_HeapBlock* value, CLR_INT32& index )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* items    = GetItems();
    CLR_INT32               size     = GetSize();
    CLR_INT32               capacity = items->m_numOfElements;

    if(size == capacity)
    {
        // Protect value from GC, in case EnsureCapacity triggers one
        CLR_RT_HeapBlock valueHB; valueHB.SetObjectReference( value );
        CLR_RT_ProtectFromGC gc( valueHB );
        
        TINYCLR_CHECK_HRESULT(EnsureCapacity( size + 1, capacity ));

        // needs to update the reference to the new array
        items = GetItems();
    }

    SetSize( size + 1 );

    ((CLR_RT_HeapBlock*)items->GetElement( size ))->SetObjectReference( value );

    index = size;
    
    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_ArrayList::Clear()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_CHECK_HRESULT(GetItems()->ClearElements( 0, GetSize() ));

    SetSize( 0 );
    
    TINYCLR_NOCLEANUP();
}

// May Trigger GC, but parameter value will be protected
HRESULT CLR_RT_HeapBlock_ArrayList::Insert( CLR_INT32 index, CLR_RT_HeapBlock* value )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* items    = GetItems();
    CLR_INT32               size     = GetSize();
    CLR_INT32               capacity = items->m_numOfElements;

    if(index < 0 || index > size) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    if(size == capacity)
    {
        // Protect value from GC, in case EnsureCapacity triggers one
        CLR_RT_HeapBlock valueHB; valueHB.SetObjectReference( value );
        CLR_RT_ProtectFromGC gc( valueHB );
        
        TINYCLR_CHECK_HRESULT(EnsureCapacity( size + 1, capacity ));

        // needs to update the reference to the new array
        items = GetItems();
    }

    if(index < size)
    {
        // Move everything up one slot.
        CLR_RT_HeapBlock* current = (CLR_RT_HeapBlock*)items->GetElement( size  );
        CLR_RT_HeapBlock* end     = (CLR_RT_HeapBlock*)items->GetElement( index );

        do
        {
            current->Assign( *(current - 1) );
        }
        while(--current != end);
    }

    ((CLR_RT_HeapBlock*)items->GetElement( index ))->SetObjectReference( value );

    SetSize( size + 1 );
    
    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_ArrayList::RemoveAt( CLR_INT32 index )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* items = GetItems();
    CLR_INT32               size  = GetSize();

    if(index < 0 || index >= size) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    // Need to shift everything, if it's not the last item
    if(index < size - 1)
    {
        // Move everything down one slot.
        CLR_RT_HeapBlock* current = (CLR_RT_HeapBlock*)items->GetElement( index    );
        CLR_RT_HeapBlock* end     = (CLR_RT_HeapBlock*)items->GetElement( size - 1 );

        do
        {
            current->Assign( *(current + 1) );
        }
        while(++current != end);
    }

    size--;

    ((CLR_RT_HeapBlock*)items->GetElement( size ))->SetObjectReference( NULL );

    SetSize( size );
    
    TINYCLR_NOCLEANUP();
}

// May Trigger GC
HRESULT CLR_RT_HeapBlock_ArrayList::SetCapacity( CLR_INT32 newCapacity )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* items = GetItems();
    CLR_INT32               size  = GetSize();

    if(newCapacity != items->m_numOfElements) // if capacity is changing
    {
        CLR_RT_HeapBlock        newItemsHB;
        CLR_RT_HeapBlock_Array* newItems;
        
        if(newCapacity < size) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

        if(newCapacity < c_DefaultCapacity)
        {
            newCapacity = c_DefaultCapacity;
        }

        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( newItemsHB, newCapacity, g_CLR_RT_WellKnownTypes.m_Object ));

        newItems = newItemsHB.DereferenceArray();

        if(size > 0)
        {
            memcpy( newItems->GetFirstElement(), items->GetFirstElement(), size * sizeof(CLR_RT_HeapBlock) );
        }

        SetItems( newItems );
    }
    
    TINYCLR_NOCLEANUP();
}

// May Trigger GC
HRESULT CLR_RT_HeapBlock_ArrayList::EnsureCapacity( CLR_INT32 min, CLR_INT32 currentCapacity )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(currentCapacity < min)
    {
        CLR_INT32 newCapacity = currentCapacity * 3 / 2;

        if(newCapacity < min) newCapacity = min;

        TINYCLR_SET_AND_LEAVE(SetCapacity( newCapacity ));
    }
    
    TINYCLR_NOCLEANUP();
}

//--//

CT_ASSERT(Library_corlib_native_System_Collections_ArrayList__FIELD___items == Library_corlib_native_System_Collections_ArrayList::FIELD___items);
CT_ASSERT(Library_corlib_native_System_Collections_ArrayList__FIELD___size  == Library_corlib_native_System_Collections_ArrayList::FIELD___size );

