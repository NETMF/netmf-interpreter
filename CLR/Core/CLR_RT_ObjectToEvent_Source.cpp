////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_ObjectToEvent_Source::CreateInstance( CLR_RT_ObjectToEvent_Destination* event, CLR_RT_HeapBlock& object, CLR_RT_HeapBlock& reference )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_ObjectToEvent_Source* oe = EVENTCACHE_EXTRACT_NODE(g_CLR_RT_EventCache,CLR_RT_ObjectToEvent_Source,DATATYPE_OBJECT_TO_EVENT); CHECK_ALLOCATION(oe);

    oe->m_eventPtr     = event;      // Pointer to the event referenced.

    oe->m_objectPtr    = &object;    // Pointer to the object to keep alive.
    oe->m_referencePtr = &reference; // Pointer to the HeapBlock referencing this structure.

    reference.SetObjectReference( oe );

    event->m_references.LinkAtBack( oe );

    TINYCLR_NOCLEANUP();
}

CLR_RT_ObjectToEvent_Source* CLR_RT_ObjectToEvent_Source::ExtractInstance( CLR_RT_HeapBlock& reference )
{
    NATIVE_PROFILE_CLR_CORE();
    if(reference.IsAReferenceOfThisType( DATATYPE_OBJECT_TO_EVENT ) == false) return NULL;

    return (CLR_RT_ObjectToEvent_Source*)reference.Dereference();
}


void CLR_RT_ObjectToEvent_Source::EnsureObjectIsAlive()
{
    NATIVE_PROFILE_CLR_CORE();
    if(m_objectPtr && m_objectPtr->IsAlive()) return;

    Detach();
}

void CLR_RT_ObjectToEvent_Source::Detach()
{
    NATIVE_PROFILE_CLR_CORE();
    if(m_referencePtr)
    {
        m_referencePtr->SetObjectReference( NULL );
    }

    g_CLR_RT_EventCache.Append_Node( this );
}

void CLR_RT_ObjectToEvent_Source::Relocate()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_objectPtr    );
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_referencePtr );
}
