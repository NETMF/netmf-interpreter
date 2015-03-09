////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_RT_DblLinkedList::DblLinkedList_Initialize()
{
    NATIVE_PROFILE_CLR_CORE();
    m_first = Tail();
    m_null  = NULL;
    m_last  = Head();
}

void CLR_RT_DblLinkedList::DblLinkedList_PushToCache()
{
    NATIVE_PROFILE_CLR_CORE();
    ValidateList();

    while(true)
    {
        CLR_RT_HeapBlock_Node* node = ExtractFirstNode(); if(!node) break;

        g_CLR_RT_EventCache.Append_Node( node );
    }
}

void CLR_RT_DblLinkedList::DblLinkedList_Release()
{
    NATIVE_PROFILE_CLR_CORE();
    ValidateList();

    while(true)
    {
        CLR_RT_HeapBlock_Node* node = ExtractFirstNode(); if(!node) break;

        CLR_RT_Memory::Release( node );
    }
}

int CLR_RT_DblLinkedList::NumOfNodes()
{
    NATIVE_PROFILE_CLR_CORE();
    ValidateList();

    int num  = 0;

    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Node,node,(*this))
    {
        num++;
    }
    TINYCLR_FOREACH_NODE_END();

    return num;
}

void CLR_RT_DblLinkedList::Relocate()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_first );
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_last  );
}

#if TINYCLR_VALIDATE_HEAP >= TINYCLR_VALIDATE_HEAP_2_DblLinkedList

void CLR_RT_DblLinkedList::ValidateList()
{
    NATIVE_PROFILE_CLR_CORE();

    if(m_null != NULL) TINYCLR_DEBUG_STOP();

    CLR_RT_HeapBlock_Node* prev = Head();
    CLR_RT_HeapBlock_Node* node = prev->Next();

    while(node)
    {
        if(node->Prev() != prev) TINYCLR_DEBUG_STOP();

        prev = node;
        node = prev->Next();
    }
}

#endif

