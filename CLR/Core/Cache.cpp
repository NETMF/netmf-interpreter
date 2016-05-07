////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_USE_AVLTREE_FOR_METHODLOOKUP)

int CLR_RT_EventCache::LookupEntry::Callback_Compare( void* state, CLR_RT_AVLTree::Entry* left, CLR_RT_AVLTree::Entry* right )
{
    NATIVE_PROFILE_CLR_CORE();

    LookupEntry* leftDirect  = (LookupEntry*)left;
    LookupEntry* rightDirect = (LookupEntry*)right;

    return leftDirect->m_payload.Compare( rightDirect->m_payload );
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_RT_EventCache::VirtualMethodTable::Initialize()
{
    NATIVE_PROFILE_CLR_CORE();

    m_entries = (Payload*)&g_scratchVirtualMethodPayload[ 0 ];
    
    LookupEntry* node;
    size_t       i;

    m_tree.Initialize();                         
                                                 
                                                 
    m_list_freeItems.DblLinkedList_Initialize(); 
    m_list_inUse    .DblLinkedList_Initialize(); 

    m_tree.m_owner.m_ftn_compare      = LookupEntry       ::Callback_Compare;
    m_tree.m_owner.m_ftn_newNode      = VirtualMethodTable::Callback_NewNode;
    m_tree.m_owner.m_ftn_freeNode     = VirtualMethodTable::Callback_FreeNode;
    m_tree.m_owner.m_ftn_reassignNode = VirtualMethodTable::Callback_Reassign;
    m_tree.m_owner.m_state            = this;


    for(i=0, node=m_entries; i<PayloadArraySize(); i++, node++)
    {
        node->GenericNode_Initialize();

        m_list_freeItems.LinkAtBack( node );
    }
}

bool CLR_RT_EventCache::VirtualMethodTable::FindVirtualMethod( const CLR_RT_TypeDef_Index& cls, const CLR_RT_MethodDef_Index& mdVirtual, CLR_RT_MethodDef_Index& md )
{
    NATIVE_PROFILE_CLR_CORE();
    LookupEntry*              en;
    LookupEntry               key;
    CLR_RT_MethodDef_Instance instMD;
    CLR_RT_TypeDef_Instance   instCLS;

    instMD .InitializeFromIndex ( mdVirtual );
    instCLS.InitializeFromMethod( instMD    );

    //
    // Shortcut for terminal virtual methods.
    //
    if(cls.m_data == instCLS.m_data)
    {
        if((instMD.m_target->flags & CLR_RECORD_METHODDEF::MD_Abstract) == 0)
        {
            md = mdVirtual;

            return true;
        }
    }

    key.m_payload.m_mdVirtual = mdVirtual;
    key.m_payload.m_cls       = cls;
    key.m_payload.m_md.Clear();

    en = (LookupEntry*)m_tree.Find( &key );
    if(en)
    {
        md = en->m_payload.m_md;

        //
        // Move the node to the top of the MRU list.
        //
        m_list_inUse.LinkAtFront( en );

        return true;
    }

    {
        if(g_CLR_RT_TypeSystem.FindVirtualMethodDef( cls, mdVirtual, md ) == false)
        {
            return false;
        }
    }

    {
        if(m_list_freeItems.IsEmpty())
        {
            en = (LookupEntry*)m_list_inUse.LastNode();
            if(en->Prev() == NULL)
            {
                //
                // No node to steal, return.
                //
                return true;
            }

            m_tree.Remove( en );

            CLR_PROF_Handler::SuspendTime();
            if(!ConsistencyCheck())
            {
                DumpTree();
            }
            CLR_PROF_Handler::ResumeTime();
        }

        key.m_payload.m_md = md;
        m_tree.Insert( &key );

        CLR_PROF_Handler::SuspendTime();
        if(!ConsistencyCheck())
        {
            DumpTree();
        }
        CLR_PROF_Handler::ResumeTime();
    }

    return true;
}

//--//

CLR_RT_AVLTree::Entry* CLR_RT_EventCache::VirtualMethodTable::Callback_NewNode( void* state, CLR_RT_AVLTree::Entry* payload )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_EventCache::VirtualMethodTable* pThis         = (CLR_RT_EventCache::VirtualMethodTable*)state;
    LookupEntry*                           payloadDirect = (LookupEntry*)payload;
    LookupEntry*                           en;

    en = (LookupEntry*)pThis->m_list_freeItems.ExtractFirstNode();
    if(en)
    {
        en->m_payload = payloadDirect->m_payload;

        pThis->m_list_inUse.LinkAtFront( en );
    }

    return en;
}

void CLR_RT_EventCache::VirtualMethodTable::Callback_FreeNode( void* state, CLR_RT_AVLTree::Entry* node )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_EventCache::VirtualMethodTable* pThis      = (CLR_RT_EventCache::VirtualMethodTable*)state;
    LookupEntry*                           nodeDirect = (LookupEntry*)node;

    pThis->m_list_freeItems.LinkAtBack( nodeDirect );
}

void CLR_RT_EventCache::VirtualMethodTable::Callback_Reassign( void* state, CLR_RT_AVLTree::Entry* from, CLR_RT_AVLTree::Entry* to )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_EventCache::VirtualMethodTable* pThis      = (CLR_RT_EventCache::VirtualMethodTable*)state;
    LookupEntry*                           fromDirect = (LookupEntry*)from;
    LookupEntry*                           toDirect   = (LookupEntry*)to;

    pThis->m_list_inUse.InsertAfterNode( fromDirect, toDirect );

    toDirect->m_payload = fromDirect->m_payload;
}

#else

void CLR_RT_EventCache::VirtualMethodTable::Initialize()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_UINT32 idx;

    m_entries    = (Link*)   &g_scratchVirtualMethodTableLink   [ 0 ];
    m_entriesMRU = (Link*)   &g_scratchVirtualMethodTableLinkMRU[ 0 ];
    m_payloads   = (Payload*)&g_scratchVirtualMethodPayload     [ 0 ];

    //
    // Link all the entries to themselves => no elements in the lists.
    //
    for(idx = 0; idx < LinkArraySize(); idx++)
    {
        Link& lnk = m_entries[ idx ];

        lnk.m_next = idx;
        lnk.m_prev = idx;
    }

    //
    // Link all the entries to the following one => all the elements are in the MRU list.
    //
    _ASSERTE(LinkMRUArraySize() < 0xFFFF);
    for(idx = 0; idx < LinkMRUArraySize(); idx++)
    {
        Link& lnk = m_entriesMRU[ idx ];

        lnk.m_next = idx == LinkMRUArraySize() - 1 ? 0                                       : idx + 1;
        lnk.m_prev = idx == 0                           ? (CLR_UINT16)LinkMRUArraySize() - 1 : idx - 1;
    }
}

bool CLR_RT_EventCache::VirtualMethodTable::FindVirtualMethod( const CLR_RT_TypeDef_Index& cls, const CLR_RT_MethodDef_Index& mdVirtual, CLR_RT_MethodDef_Index& md )
{
    NATIVE_PROFILE_CLR_CORE();
    Payload::Key key;
    CLR_UINT32   idx;
    CLR_UINT32   idxHead;
    CLR_UINT32   clsData       = cls      .m_data;
    CLR_UINT32   mdVirtualData = mdVirtual.m_data;

#if defined(_WIN32)
    bool fVerify = false;

    {
        CLR_RT_MethodDef_Instance instMD;
        CLR_RT_TypeDef_Instance   instCLS;

        instMD .InitializeFromIndex ( mdVirtual );
        instCLS.InitializeFromMethod( instMD    );

        //
        // Shortcut for terminal virtual methods.
        //
        if(clsData == instCLS.m_data)
        {
            if((instMD.m_target->flags & CLR_RECORD_METHODDEF::MD_Abstract) == 0)
            {
                md = mdVirtual;

                fVerify = true;
            }
        }
    }
#endif

    if(cls.Assembly() == mdVirtual.Assembly())
    {
        CLR_RT_Assembly* assm  = g_CLR_RT_TypeSystem.m_assemblies [ mdVirtual.Assembly()-1 ];
        CLR_IDX          owner = assm->m_pCrossReference_MethodDef[ mdVirtual.Method()     ].GetOwner();

        if(cls.Type() == owner)
        {
#if defined(_WIN32)
            if(fVerify != true)
            {
                CLR_Debug::Printf( "INTERNAL ERROR: Shortcut for terminal virtual methods failed: CLS:%08x:%08x => %08x\r\n", cls.m_data, mdVirtual.m_data, md.m_data );
                ::DebugBreak();
            }
#endif

            md = mdVirtual;
                
            return true;
        }
    }

#if defined(_WIN32)
    if(fVerify != false)
    {
        CLR_Debug::Printf( "INTERNAL ERROR: Shortcut for terminal virtual methods failed: CLS:%08x:%08x\r\n", cls.m_data, mdVirtual.m_data );
        ::DebugBreak();
    }
#endif


    key.m_mdVirtual.m_data = mdVirtualData;
    key.m_cls      .m_data = clsData;

    idxHead = (SUPPORT_ComputeCRC( &key, sizeof(key), 0 ) % (LinkArraySize() - PayloadArraySize())) + PayloadArraySize();

    for(idx = m_entries[ idxHead ].m_next; ; idx = m_entries[ idx ].m_next)
    {
        if(idx != idxHead)
        {
            Payload& res = m_payloads[ idx ];

            if(res.m_key.m_mdVirtual.m_data != mdVirtualData) continue;
            if(res.m_key.m_cls      .m_data != clsData      ) continue;

            md = res.m_md;

            break;
        }
        else
        {
            if(g_CLR_RT_TypeSystem.FindVirtualMethodDef( cls, mdVirtual, md ) == false) return false;

            idx = GetNewEntry();

            Payload& res = m_payloads[ idx ];

            res.m_md  = md;
            res.m_key = key;

            break;
        }
    }

    MoveEntryToTop( m_entries   , idxHead               , idx );
    MoveEntryToTop( m_entriesMRU, LinkMRUArraySize() - 1, idx );

    return true;
}

void CLR_RT_EventCache::VirtualMethodTable::MoveEntryToTop( Link* entries, CLR_UINT32 slot, CLR_UINT32 idx )
{
    NATIVE_PROFILE_CLR_CORE();
    Link& list = entries[ slot ];

    if(list.m_next != idx)
    {
        Link&      node = entries[ idx ];
        CLR_UINT32 next;
        CLR_UINT32 prev;

        //
        // Unlink.
        //
        next = node.m_next;
        prev = node.m_prev;

        entries[ next ].m_prev = prev;
        entries[ prev ].m_next = next;

        //
        // Insert.
        //
        next = list.m_next;

        node.m_next = next;
        node.m_prev = slot;

        list           .m_next = idx;
        entries[ next ].m_prev = idx;
    }
}

#endif // #if defined(TINYCLR_USE_AVLTREE_FOR_METHODLOOKUP)

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_RT_EventCache::EventCache_Initialize()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_CLEAR(*this);

    m_events = (BoundedList*)&m_scratch[ 0 ];

    BoundedList* lst = m_events;
    size_t       num = c_maxFastLists;

    while(num--)
    {
        lst->m_blocks.DblLinkedList_Initialize();

        lst++;
    }

    m_lookup_VirtualMethod.Initialize();

#ifndef TINYCLR_NO_IL_INLINE
    m_inlineBufferStart = (CLR_RT_InlineBuffer*)g_scratchInlineBuffer;

    num = InlineBufferCount()-1;

    for(int i=0; i<(int)num; i++)
    {
        m_inlineBufferStart[i].m_pNext = &m_inlineBufferStart[i+1];
    }
    m_inlineBufferStart[num].m_pNext = NULL;
#endif    
}

CLR_UINT32 CLR_RT_EventCache::EventCache_Cleanup()
{
    NATIVE_PROFILE_CLR_CORE();
    BoundedList* lst = m_events;
    size_t       num = c_maxFastLists;
    CLR_UINT32   tot = 0;

    while(num--)
    {
        TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Node,ptr,lst->m_blocks)
        {
            ptr->SetDataId( CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_FREEBLOCK, CLR_RT_HeapBlock::HB_Pinned, ptr->DataSize()) );
            ptr->ClearData();

            tot += ptr->DataSize();
        }
        TINYCLR_FOREACH_NODE_END();

        lst->m_blocks.DblLinkedList_Initialize();

        lst++;
    }

    return tot;
}

//--//

void CLR_RT_EventCache::Append_Node( CLR_RT_HeapBlock* node )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock_Node* ptr    = (CLR_RT_HeapBlock_Node*)node;
    CLR_UINT32             blocks = ptr->DataSize();
    BoundedList&           lst    = m_events[blocks < c_maxFastLists ? blocks : 0];

    ptr->ChangeDataType ( DATATYPE_CACHEDBLOCK                                    );
    ptr->ChangeDataFlags( CLR_RT_HeapBlock::HB_Alive | CLR_RT_HeapBlock::HB_Event );

    TINYCLR_CHECK_EARLY_COLLECTION(ptr);

    ptr->Debug_ClearBlock( 0xAB );

    lst.m_blocks.LinkAtBack( ptr );

#if defined(TINYCLR_PROFILE_NEW_ALLOCATIONS)
    g_CLR_PRF_Profiler.TrackObjectDeletion(node);
#endif
}

CLR_RT_HeapBlock* CLR_RT_EventCache::Extract_Node_Slow( CLR_UINT32 dataType, CLR_UINT32 flags, CLR_UINT32 blocks )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock_Node* ptr;
    CLR_RT_HeapBlock_Node* best     = NULL;
    CLR_UINT32             bestSize = 0;

    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_Node,ptr,m_events[ 0 ].m_blocks)
    {
        CLR_UINT32 size = ptr->DataSize();

        if(size == blocks)
        {
            best     = ptr;
            bestSize = blocks;
            break;
        }

        if(size >= blocks)
        {
            if(( best && (size <   bestSize         )) ||
                (!best && (size <= (blocks * 20) / 16))  ) // Accept a maximum overhead of 25%.
            {
                best     = ptr;
                bestSize = size;
            }
        }
    }
    TINYCLR_FOREACH_NODE_END();

    ptr = best;

    if(ptr)
    {
        //
        // Did we select a block bigger than requested? Requeue the tail.
        //
        if(bestSize > blocks)
        {
            CLR_RT_HeapBlock_Node* next = &ptr[ blocks ];

            ptr->SetDataId ( CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_CACHEDBLOCK,CLR_RT_HeapBlock::HB_Alive | CLR_RT_HeapBlock::HB_Event,           blocks) );
            next->SetDataId( CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_CACHEDBLOCK,CLR_RT_HeapBlock::HB_Alive | CLR_RT_HeapBlock::HB_Event,bestSize - blocks) );
            next->ClearData();

            Append_Node( next );
        }

        ptr->Unlink();

        ptr->ChangeDataType ( dataType                                                );
        ptr->ChangeDataFlags( CLR_RT_HeapBlock::HB_Alive | CLR_RT_HeapBlock::HB_Event );

        if(flags & CLR_RT_HeapBlock::HB_InitializeToZero)
        {
            ptr->InitializeToZero();
        }
        else
        {
            ptr->Debug_ClearBlock( 0xAD );
        }

#if defined(TINYCLR_PROFILE_NEW_ALLOCATIONS)
        g_CLR_PRF_Profiler.TrackObjectCreation( ptr );
#endif

        return ptr;
    }

    return g_CLR_RT_ExecutionEngine.ExtractHeapBlocksForEvents( dataType, flags, blocks );
}

CLR_RT_HeapBlock* CLR_RT_EventCache::Extract_Node_Fast( CLR_UINT32 dataType, CLR_UINT32 flags, CLR_UINT32 blocks )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock_Node* ptr = m_events[ blocks ].m_blocks.FirstNode();

    if(ptr->Next())
    {
        ptr->Unlink();

        ptr->ChangeDataType ( dataType                                                );
        ptr->ChangeDataFlags( CLR_RT_HeapBlock::HB_Alive | CLR_RT_HeapBlock::HB_Event );

        if(flags & CLR_RT_HeapBlock::HB_InitializeToZero)
        {
            ptr->InitializeToZero();
        }
        else
        {
            ptr->Debug_ClearBlock( 0xAD );
        }

#if defined(TINYCLR_PROFILE_NEW_ALLOCATIONS)
        g_CLR_PRF_Profiler.TrackObjectCreation( ptr );
#endif

        return ptr;
    }

    return g_CLR_RT_ExecutionEngine.ExtractHeapBlocksForEvents( dataType, flags, blocks );
}

CLR_RT_HeapBlock* CLR_RT_EventCache::Extract_Node_Bytes( CLR_UINT32 dataType, CLR_UINT32 flags, CLR_UINT32 bytes )
{
    NATIVE_PROFILE_CLR_CORE();
    return Extract_Node( dataType, flags, CONVERTFROMSIZETOHEAPBLOCKS(bytes) );
}

CLR_RT_HeapBlock* CLR_RT_EventCache::Extract_Node( CLR_UINT32 dataType, CLR_UINT32 flags, CLR_UINT32 blocks )
{
    NATIVE_PROFILE_CLR_CORE();
#if defined(TINYCLR_FORCE_GC_BEFORE_EVERY_ALLOCATION)        
    return g_CLR_RT_ExecutionEngine.ExtractHeapBlocksForEvents( dataType, flags, blocks );
#else
    if(blocks > 0 && blocks < c_maxFastLists) return Extract_Node_Fast( dataType, flags, blocks );
    else                                      return Extract_Node_Slow( dataType, flags, blocks );
#endif
}

//--//

bool CLR_RT_EventCache::FindVirtualMethod( const CLR_RT_TypeDef_Index& cls, const CLR_RT_MethodDef_Index& mdVirtual, CLR_RT_MethodDef_Index& md )
{
    NATIVE_PROFILE_CLR_CORE();
    return m_lookup_VirtualMethod.FindVirtualMethod( cls, mdVirtual, md );
}

// -- //

#ifndef TINYCLR_NO_IL_INLINE
bool CLR_RT_EventCache::GetInlineFrameBuffer(CLR_RT_InlineBuffer** ppBuffer)
{
    if(m_inlineBufferStart != NULL)
    {
        *ppBuffer = m_inlineBufferStart;

        m_inlineBufferStart = m_inlineBufferStart->m_pNext;

        return true;
    }

    *ppBuffer = NULL;

    return false;
}

bool CLR_RT_EventCache::FreeInlineBuffer(CLR_RT_InlineBuffer* pBuffer)
{
    pBuffer->m_pNext = m_inlineBufferStart;
    m_inlineBufferStart = pBuffer;
    return true;
}
#endif

////////////////////////////////////////////////////////////////////////////////

