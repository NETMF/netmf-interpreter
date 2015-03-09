////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_RT_ObjectToEvent_Destination::Initialize()
{
    NATIVE_PROFILE_CLR_CORE();
    m_references.DblLinkedList_Initialize();
}

void CLR_RT_ObjectToEvent_Destination::CheckAll()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_NODE(CLR_RT_ObjectToEvent_Source,ref,m_references)
    {
        ref->EnsureObjectIsAlive();
    }
    TINYCLR_FOREACH_NODE_END();
}

void CLR_RT_ObjectToEvent_Destination::SignalAll()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_NODE(CLR_RT_ObjectToEvent_Source,ref,m_references)
    {
        if(ref->m_objectPtr)
        {
            CLR_RT_HeapBlock_WaitForObject::SignalObject( *ref->m_objectPtr );
        }
    }
    TINYCLR_FOREACH_NODE_END();
}

void CLR_RT_ObjectToEvent_Destination::DetachAll()
{
    NATIVE_PROFILE_CLR_CORE();
    while(true)
    {
        CLR_RT_ObjectToEvent_Source* ref = (CLR_RT_ObjectToEvent_Source*)m_references.ExtractFirstNode(); if(!ref) break;

        ref->Detach();
    }
}

bool CLR_RT_ObjectToEvent_Destination::IsReadyForRelease()
{
    NATIVE_PROFILE_CLR_CORE();
    if(m_references.IsEmpty())
    {
        if(IsForcedAlive() == false)
        {
            return true;
        }
    }

    return false;
}

bool CLR_RT_ObjectToEvent_Destination::ReleaseWhenDead()
{
    NATIVE_PROFILE_CLR_CORE();
    bool res = IsReadyForRelease();

    if(res) g_CLR_RT_EventCache.Append_Node( this );

    return res;
}
