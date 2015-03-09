////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_RT_HeapCluster::HeapCluster_Initialize( CLR_UINT32 size, CLR_UINT32 blockSize )
{
    NATIVE_PROFILE_CLR_CORE();
    GenericNode_Initialize();

    size = (size - sizeof(*this)) / sizeof(CLR_RT_HeapBlock);

    m_freeList.DblLinkedList_Initialize();                            // CLR_RT_DblLinkedList    m_freeList;
    m_payloadStart = (CLR_RT_HeapBlock_Node*)&this[ 1 ];              // CLR_RT_HeapBlock_Node*  m_payloadStart;
    m_payloadEnd   =                         &m_payloadStart[ size ]; // CLR_RT_HeapBlock_Node*  m_payloadEnd;

    //
    // Scan memory looking for possible objects to salvage.  This method returns false if HeapPersistence is stubbed
    // which requires us to set up the free list.
    //
    if(!CLR_RT_HeapBlock_WeakReference::PrepareForRecovery( m_payloadStart, m_payloadEnd, blockSize ))
    {
        CLR_RT_HeapBlock_Node *ptr = m_payloadStart;
        
        while(ptr < m_payloadEnd)
        {
            if((UINT32)(ptr + blockSize) > (UINT32)m_payloadEnd)
            {
                blockSize = (CLR_UINT32)(m_payloadEnd - ptr);
            }
            
            ptr->SetDataId( CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_FREEBLOCK,0,blockSize) );
            ptr += blockSize;
        }
    }

    RecoverFromGC();

    m_freeList.ValidateList();
}

CLR_RT_HeapBlock* CLR_RT_HeapCluster::ExtractBlocks( CLR_UINT32 dataType, CLR_UINT32 flags, CLR_UINT32 length )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock_Node* res = NULL;
    CLR_UINT32             available = 0;

    m_freeList.ValidateList();

    if(flags & CLR_RT_HeapBlock::HB_Event)
    {
        TINYCLR_FOREACH_NODE_BACKWARD(CLR_RT_HeapBlock_Node,ptr,m_freeList)
        {
            available = ptr->DataSize();

            if(available >= length)
            {
                res = ptr;
                break;
            }
        }
        TINYCLR_FOREACH_NODE_BACKWARD_END();
    }
    else
    {
        TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Node,ptr,m_freeList)
        {
            available = ptr->DataSize();

            if(available >= length)
            {
                res = ptr;
                break;
            }
        }
        TINYCLR_FOREACH_NODE_END();
    }

    if(res)
    {
        CLR_RT_HeapBlock_Node* next = res->Next();
        CLR_RT_HeapBlock_Node* prev = res->Prev();

        available -= length;

        if(available != 0)
        {
            if(flags & CLR_RT_HeapBlock::HB_Event)
            {
                res->SetDataId( CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_FREEBLOCK,CLR_RT_HeapBlock::HB_Pinned,available) );

                res += available;
            }
            else
            {
                CLR_RT_HeapBlock_Node* ptr = &res[ length ];

                //
                // Relink to the new free block.
                //
                ptr->SetDataId( CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_FREEBLOCK,CLR_RT_HeapBlock::HB_Pinned,available) );

                ptr->SetNext( next );
                ptr->SetPrev( prev );

                prev->SetNext( ptr );
                next->SetPrev( ptr );
            }
        }
        else
        {
            //
            // Unlink the whole block.
            //
            prev->SetNext( next );
            next->SetPrev( prev );
        }

        res->SetDataId( CLR_RT_HEAPBLOCK_RAW_ID(dataType,flags,length) );

        if(flags & CLR_RT_HeapBlock::HB_InitializeToZero)
        {
            res->InitializeToZero();
        }
        else
        {
            res->Debug_ClearBlock( 0xCB );
        }
    }

    m_freeList.ValidateList();

    return res;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_RT_HeapCluster::RecoverFromGC()
{
    NATIVE_PROFILE_CLR_CORE();

    CLR_RT_HeapBlock_Node* ptr = m_payloadStart;
    CLR_RT_HeapBlock_Node* end = m_payloadEnd;

    //
    // Open the free list.
    //
    CLR_RT_HeapBlock_Node* last = m_freeList.Head(); last->SetPrev( NULL );

    while(ptr < end)
    {

        ValidateBlock( ptr );

        if(ptr->IsAlive() == false)
        {
            CLR_RT_HeapBlock_Node* next   = ptr;
            CLR_UINT32             lenTot = 0;

            do
            {
                ValidateBlock( next );

#if defined(TINYCLR_PROFILE_NEW_ALLOCATIONS)
                g_CLR_PRF_Profiler.TrackObjectDeletion( next );
#endif

                TINYCLR_CHECK_EARLY_COLLECTION(next);

                int len = next->DataSize();

                next   += len;
                lenTot += len;

            } while(next < end && next->IsAlive() == false);

            ptr->SetDataId( CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_FREEBLOCK,CLR_RT_HeapBlock::HB_Pinned,lenTot) );

            //
            // Link to the free list.
            //
            last->SetNext( ptr  );
            ptr ->SetPrev( last );
            last = ptr;

            ptr->Debug_ClearBlock( 0xDF );

            ptr = next;
        }
        else
        {
            if(ptr->IsEvent() == false) ptr->MarkDead();

            ptr += ptr->DataSize();
        }
    }

    //
    // Close the free list.
    //
    last             ->SetNext( m_freeList.Tail() );
    m_freeList.Tail()->SetPrev( last              );
    m_freeList.Tail()->SetNext( NULL              );
}

CLR_RT_HeapBlock_Node* CLR_RT_HeapCluster::InsertInOrder( CLR_RT_HeapBlock_Node* node, CLR_UINT32 size )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock_Node* ptr;

    TINYCLR_FOREACH_NODE__NODECL(CLR_RT_HeapBlock_Node,ptr,m_freeList)
    {
        if(ptr > node) break;
    }
    TINYCLR_FOREACH_NODE_END();

    node->ClearData(); m_freeList.InsertBeforeNode( ptr, node );

    //
    // Try to coalesce with the following free block.
    //
    ptr = node->Next();
    if(ptr->Next() && (node + size) == ptr)
    {
        size += ptr->DataSize(); ptr->Unlink();
    }

    //
    // Try to coalesce with the preceding free block.
    //
    ptr = node->Prev();
    if(ptr->Prev() && (ptr + ptr->DataSize()) == node)
    {
        size += ptr->DataSize(); node->Unlink(); node = ptr;
    }

    node->SetDataId( CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_FREEBLOCK,CLR_RT_HeapBlock::HB_Pinned,size ) );
    node->Debug_ClearBlock( 0xCF );

    return node;
}

//--//

#if TINYCLR_VALIDATE_HEAP >= TINYCLR_VALIDATE_HEAP_1_HeapBlocksAndUnlink

void CLR_RT_HeapCluster::ValidateBlock( CLR_RT_HeapBlock* ptr )
{
    NATIVE_PROFILE_CLR_CORE();

    while(true)
    {
        if(ptr < m_payloadStart || ptr >= m_payloadEnd)
        {
            CLR_Debug::Printf( "Block beyond cluster limits: %08x [%08x : %08x-%08x]\r\n", (size_t)ptr, (size_t)this, (size_t)m_payloadStart, (size_t)m_payloadEnd );

            break;
        }

        if(ptr->DataType() >= DATATYPE_FIRST_INVALID)
        {
            CLR_Debug::Printf( "Bad Block Type: %08x %02x [%08x : %08x-%08x]\r\n", (size_t)ptr, ptr->DataType(), (size_t)this, (size_t)m_payloadStart, (size_t)m_payloadEnd );

            break;
        }

        if(ptr->DataSize() == 0)
        {
            CLR_Debug::Printf( "Bad Block null-size: %08x [%08x : %08x-%08x]\r\n", (size_t)ptr, (size_t)this, (size_t)m_payloadStart, (size_t)m_payloadEnd );

            break;
        }

        if(ptr + ptr->DataSize() > m_payloadEnd)
        {
            CLR_Debug::Printf( "Bad Block size: %d %08x [%08x : %08x-%08x]\r\n", ptr->DataSize(), (size_t)ptr, (size_t)this, (size_t)m_payloadStart, (size_t)m_payloadEnd );

            break;
        }

        return;
    }

    TINYCLR_DEBUG_STOP();
}

#endif
