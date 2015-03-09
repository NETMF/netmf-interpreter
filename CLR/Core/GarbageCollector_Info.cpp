////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_GC_VERBOSE)

void CLR_RT_GarbageCollector::GC_Stats( int& resNumberObjects, int& resSizeObjects, int& resNumberEvents, int& resSizeEvents )
{
    NATIVE_PROFILE_CLR_CORE();
    resNumberObjects = 0;
    resSizeObjects   = 0;

    resNumberEvents  = 0;
    resSizeEvents    = 0;

    TINYCLR_FOREACH_NODE(CLR_RT_HeapCluster,hc,g_CLR_RT_ExecutionEngine.m_heap)
    {
        CLR_RT_HeapBlock_Node* ptr = hc->m_payloadStart;
        CLR_RT_HeapBlock_Node* end = hc->m_payloadEnd;

        while(ptr < end)
        {
            CLR_UINT16 size = ptr->DataSize();

            hc->ValidateBlock( ptr );

            if(ptr->DataType() != DATATYPE_FREEBLOCK)
            {
                if(ptr->IsEvent())
                {
                    resNumberEvents += 1;
                    resSizeEvents   += size * sizeof(CLR_RT_HeapBlock);
                }
                else
                {
                    resNumberObjects += 1;
                    resSizeObjects   += size * sizeof(CLR_RT_HeapBlock);
                }
            }

            ptr += size;
        }
    }
    TINYCLR_FOREACH_NODE_END();
}


static void DumpTimeout( CLR_RT_Thread* th, CLR_INT64& t )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_Debug::Printf( ": %d", th ? th->m_pid : -1 );

    if(t < TIMEOUT_INFINITE)
    {
        t -= g_CLR_RT_ExecutionEngine.m_currentMachineTime;

        CLR_Debug::Printf( " %d", (int)t );
    }
    else
    {
        CLR_Debug::Printf( " INFINITE" );
    }
}

void CLR_RT_GarbageCollector::DumpThreads()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_NODE(CLR_RT_Thread,th,g_CLR_RT_ExecutionEngine.m_threadsReady)
    {
        th->DumpStack();
    }
    TINYCLR_FOREACH_NODE_END();

    TINYCLR_FOREACH_NODE(CLR_RT_Thread,th,g_CLR_RT_ExecutionEngine.m_threadsWaiting)
    {
        th->DumpStack();
    }
    TINYCLR_FOREACH_NODE_END();

    CLR_Debug::Printf( "\r\n" );
}
#endif

////////////////////////////////////////////////////////////////////////////////////////////////////

#if TINYCLR_VALIDATE_HEAP >= TINYCLR_VALIDATE_HEAP_3_Compaction

void CLR_RT_GarbageCollector::ValidateCluster( CLR_RT_HeapCluster* hc )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock_Node* ptr = hc->m_payloadStart;
    CLR_RT_HeapBlock_Node* end = hc->m_payloadEnd;

    while(ptr < end)
    {
        hc->ValidateBlock( ptr );

        ptr += ptr->DataSize();
    }
}

void CLR_RT_GarbageCollector::ValidateHeap( CLR_RT_DblLinkedList& lst )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_NODE(CLR_RT_HeapCluster,hc,lst)
    {
        ValidateCluster( hc );
    }
    TINYCLR_FOREACH_NODE_END();
}

void CLR_RT_GarbageCollector::ValidateBlockNotInFreeList( CLR_RT_DblLinkedList& lst, CLR_RT_HeapBlock_Node* dst )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_NODE(CLR_RT_HeapCluster,hc,lst)
    {
        TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Node,ptr,hc->m_freeList)
        {
            CLR_RT_HeapBlock_Node* ptrEnd = ptr + ptr->DataSize();

            if(ptr <= dst && dst < ptrEnd)
            {
                CLR_Debug::Printf( "Pointer into free list!! %08x %08x %08x\r\n", dst, ptr, ptrEnd );

                TINYCLR_DEBUG_STOP();
            }
        }
        TINYCLR_FOREACH_NODE_END();
    }
    TINYCLR_FOREACH_NODE_END();
}

bool CLR_RT_GarbageCollector::IsBlockInFreeList( CLR_RT_DblLinkedList& lst, CLR_RT_HeapBlock_Node* dst, bool fExact )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_NODE(CLR_RT_HeapCluster,hc,lst)
    {
        TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Node,ptr,hc->m_freeList)
        {
            if(fExact)
            {
                if(ptr == dst) return true;
            }
            else
            {
                CLR_RT_HeapBlock_Node* ptrEnd = ptr + ptr->DataSize();

                if(ptr <= dst && dst < ptrEnd) return true;
            }
        }
        TINYCLR_FOREACH_NODE_END();
    }
    TINYCLR_FOREACH_NODE_END();

    return false;
}

bool CLR_RT_GarbageCollector::IsBlockInHeap( CLR_RT_DblLinkedList& lst, CLR_RT_HeapBlock_Node* dst )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_NODE(CLR_RT_HeapCluster,hc,lst)
    {
        if(hc->m_payloadStart <= dst && dst < hc->m_payloadEnd) return true;
    }
    TINYCLR_FOREACH_NODE_END();

    return false;
}

#endif

////////////////////////////////////////////////////////////////////////////////////////////////////

#if TINYCLR_VALIDATE_HEAP >= TINYCLR_VALIDATE_HEAP_4_CompactionPlus

CLR_RT_GarbageCollector::Rel_List CLR_RT_GarbageCollector::s_lstRecords;
CLR_RT_GarbageCollector::Rel_Map  CLR_RT_GarbageCollector::s_mapOldToRecord;
CLR_RT_GarbageCollector::Rel_Map  CLR_RT_GarbageCollector::s_mapNewToRecord;

//--//

bool CLR_RT_GarbageCollector::TestPointers_PopulateOld_Worker( void** ref )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_UINT32* dst = (CLR_UINT32*)*ref;

    if(dst)
    {
        RelocationRecord* ptr = new RelocationRecord();

        s_lstRecords.push_back( ptr );

        ptr->oldRef =  ref;
        ptr->oldPtr =  dst;

        ptr->newRef =  NULL;
        ptr->newPtr =  NULL;

        ptr->data   = *dst;

        if(s_mapOldToRecord.find( ref ) != s_mapOldToRecord.end())
        {
            CLR_Debug::Printf( "Duplicate base OLD: %08x\r\n", ref );
        }

        s_mapOldToRecord[ ref ] = ptr;

        if(IsBlockInFreeList( g_CLR_RT_ExecutionEngine.m_heap, (CLR_RT_HeapBlock_Node*)dst, false ))
        {
            CLR_Debug::Printf( "Some data points into a free list: %08x\r\n", dst );

            TINYCLR_DEBUG_STOP();
        }
    }

    return false;
}

void CLR_RT_GarbageCollector::TestPointers_PopulateOld()
{
    NATIVE_PROFILE_CLR_CORE();
    Rel_List_Iter itLst;

    for(itLst = s_lstRecords.begin(); itLst != s_lstRecords.end(); itLst++)
    {
        RelocationRecord* ptr = *itLst;

        delete ptr;
    }

    s_lstRecords    .clear();
    s_mapOldToRecord.clear();
    s_mapNewToRecord.clear();

    //--//

    Heap_Relocate_Pass( TestPointers_PopulateOld_Worker );
}

//--//

void CLR_RT_GarbageCollector::TestPointers_Remap()
{
    NATIVE_PROFILE_CLR_CORE();
    Rel_Map_Iter it;

    for(it = s_mapOldToRecord.begin(); it != s_mapOldToRecord.end(); it++)
    {
        RelocationRecord* ptr = it->second;
        void**            ref = it->first  ; CLR_RT_GarbageCollector::Relocation_UpdatePointer( (void**)&ref );
        CLR_UINT32*       dst = ptr->oldPtr; CLR_RT_GarbageCollector::Relocation_UpdatePointer( (void**)&dst );

        if(s_mapNewToRecord.find( ref ) != s_mapNewToRecord.end())
        {
            CLR_Debug::Printf( "Duplicate base NEW: %08x\r\n", ref );
        }

        s_mapNewToRecord[ ref ] = ptr;

        ptr->newRef = ref;
        ptr->newPtr = dst;
    }
}

//--//

bool CLR_RT_GarbageCollector::TestPointers_PopulateNew_Worker( void** ref )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_UINT32* dst = (CLR_UINT32*)*ref;

    if(dst)
    {
        Rel_Map_Iter it = s_mapNewToRecord.find( ref );

        if(it != s_mapNewToRecord.end())
        {
            RelocationRecord* ptr = it->second;

            if(ptr->newPtr != dst)
            {
                CLR_Debug::Printf( "Bad pointer: %08x %08x\r\n", ptr->newPtr, dst );
            }
            else if(ptr->data != *dst)
            {
                CLR_Debug::Printf( "Bad data: %08x %08x\r\n", ptr->data, *dst );
            }

            if(IsBlockInFreeList( g_CLR_RT_ExecutionEngine.m_heap, (CLR_RT_HeapBlock_Node*)dst, false ))
            {
                CLR_Debug::Printf( "Some data points into a free list: %08x\r\n", dst );

                TINYCLR_DEBUG_STOP();
            }
        }
        else
        {
            CLR_Debug::Printf( "Bad base: %08x\r\n", ref );
        }
    }

    return false;
}

void CLR_RT_GarbageCollector::TestPointers_PopulateNew()
{
    NATIVE_PROFILE_CLR_CORE();
    Heap_Relocate_Pass( TestPointers_PopulateNew_Worker );
}

#endif
