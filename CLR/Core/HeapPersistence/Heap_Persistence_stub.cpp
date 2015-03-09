////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////


bool CLR_RT_Persistence_Manager::ObjectHeader::Initialize( CLR_RT_HeapBlock_WeakReference* wr )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return true;
}

bool CLR_RT_Persistence_Manager::ObjectHeader::IsGood( bool fIncludeData ) const
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return true;
}

void CLR_RT_Persistence_Manager::ObjectHeader::Delete()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
}

CLR_UINT32 CLR_RT_Persistence_Manager::ObjectHeader::Length() const
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return 0;
}

CLR_UINT32 CLR_RT_Persistence_Manager::ObjectHeader::Length( const CLR_RT_HeapBlock_WeakReference* ref )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return 0;
}

CLR_UINT32 CLR_RT_Persistence_Manager::ObjectHeader::Length( CLR_UINT32 data )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return 0;
}

CLR_RT_Persistence_Manager::ObjectHeader* CLR_RT_Persistence_Manager::ObjectHeader::Next() const
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return NULL;
}

CLR_UINT32 CLR_RT_Persistence_Manager::ObjectHeader::ComputeCRC() const
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return 0;
}

CLR_RT_Persistence_Manager::ObjectHeader* CLR_RT_Persistence_Manager::ObjectHeader::Find( FLASH_WORD* start, FLASH_WORD* end )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return NULL;
}

//--//--//--//--//--//

void CLR_RT_Persistence_Manager::BankHeader::Initialize()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
}

bool CLR_RT_Persistence_Manager::BankHeader::IsGood() const
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return true;
}

void CLR_RT_Persistence_Manager::BankHeader::Delete()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
}

CLR_RT_Persistence_Manager::BankHeader* CLR_RT_Persistence_Manager::BankHeader::Find( FLASH_WORD* start, FLASH_WORD* end )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return NULL;
}

//--//--//--//--//--//

bool CLR_RT_Persistence_Manager::Bank::Initialize( UINT32 kind )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();

    return false;
}

bool CLR_RT_Persistence_Manager::Bank::IsGood() const
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return true;
}

bool CLR_RT_Persistence_Manager::Bank::Erase( int& sectorIndex )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return true;
}

void CLR_RT_Persistence_Manager::Bank::EraseAll()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
}

bool CLR_RT_Persistence_Manager::Bank::Format()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return true;
}

void CLR_RT_Persistence_Manager::Bank::Switch( Bank& other )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
}

//--//

CLR_RT_Persistence_Manager::ObjectHeader* CLR_RT_Persistence_Manager::Bank::RecoverHeader( CLR_RT_HeapBlock_WeakReference* ref )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return NULL;
}

bool CLR_RT_Persistence_Manager::Bank::WriteHeader( CLR_RT_HeapBlock_WeakReference* ref, ObjectHeader*& pOH, FLASH_WORD*& pData )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return true;
}

//--//

bool CLR_RT_Persistence_Manager::Bank::CanWrite( FLASH_WORD* dst, CLR_UINT32 length )
{    
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return true;
}

bool CLR_RT_Persistence_Manager::Bank::Write( FLASH_WORD* dst, const FLASH_WORD* src, CLR_UINT32 length )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return true;
}

void CLR_RT_Persistence_Manager::Bank::Invalidate( FLASH_WORD* dst, FLASH_WORD match, CLR_UINT32 length )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
}

//--//

void CLR_RT_Persistence_Manager::Uninitialize()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
}


void CLR_RT_Persistence_Manager::Initialize()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
}

void CLR_RT_Persistence_Manager::EraseAll()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
}

//--//

CLR_RT_Persistence_Manager::ObjectHeader* CLR_RT_Persistence_Manager::RecoverHeader( CLR_RT_HeapBlock_WeakReference* weak )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return NULL;
}

void CLR_RT_Persistence_Manager::InvalidateEntry( CLR_RT_HeapBlock_WeakReference* weak )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
}

//--//

void CLR_RT_Persistence_Manager::Callback( void* arg )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
}

void CLR_RT_Persistence_Manager::EnqueueNextCallback()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
}

void CLR_RT_Persistence_Manager::Relocate()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
}

bool CLR_RT_Persistence_Manager::AdvanceState( bool force )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return false;
}

void CLR_RT_Persistence_Manager::Flush()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
}

//--//

#if !defined(BUILD_RTM)
void CLR_RT_Persistence_Manager::GenerateStatistics( CLR_UINT32& totalSize, CLR_UINT32& inUse )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
}
#endif
////////////////////////////////////////////////////////////////////////////////////////////////////

CLR_UINT32 CLR_RT_HeapBlock_WeakReference_Identity::ComputeCRC( const CLR_UINT8* ptr, CLR_UINT32 len ) const
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    CLR_UINT32 hash;

    hash = SUPPORT_ComputeCRC(  ptr           ,        len            , g_buildCRC );
    hash = SUPPORT_ComputeCRC( &m_selectorHash, sizeof(m_selectorHash), hash       );
    hash = SUPPORT_ComputeCRC( &m_id          , sizeof(m_id          ), hash       );

    return hash;
}

//--//

HRESULT CLR_RT_HeapBlock_WeakReference::CreateInstance( CLR_RT_HeapBlock_WeakReference*& weakref )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    TINYCLR_HEADER();

    weakref = (CLR_RT_HeapBlock_WeakReference*)g_CLR_RT_ExecutionEngine.ExtractHeapBytesForObjects( DATATYPE_WEAKCLASS, CLR_RT_HeapBlock::HB_InitializeToZero, sizeof(*weakref) );

    CHECK_ALLOCATION(weakref);

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_WeakReference::SetTarget( CLR_RT_HeapBlock& targetReference )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    TINYCLR_HEADER();

    //
    // Only classes and value types can be associated with a weak reference!
    //
    if(targetReference.DataType() != DATATYPE_OBJECT) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

    m_targetDirect = targetReference.Dereference();
    
    InsertInPriorityOrder();
    
    TINYCLR_NOCLEANUP();        
}

HRESULT CLR_RT_HeapBlock_WeakReference::GetTarget( CLR_RT_HeapBlock& targetReference )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    TINYCLR_HEADER();

    targetReference.SetObjectReference( m_targetDirect );

    TINYCLR_NOCLEANUP_NOLABEL();
}

void CLR_RT_HeapBlock_WeakReference::InsertInPriorityOrder()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();

    this->Unlink(); // Remove from the list before looking for a spot, to avoid comparing against ourselves.

    g_CLR_RT_ExecutionEngine.m_weakReferences.LinkAtBack( this );
}


void CLR_RT_HeapBlock_WeakReference::Relocate()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    CLR_RT_HeapBlock_Node::Relocate();

    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_targetDirect     );
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_targetSerialized );
}

////////////////////////////////////////////////////////////////////////////////////////////////////

bool CLR_RT_HeapBlock_WeakReference::PrepareForRecovery( CLR_RT_HeapBlock_Node* ptr, CLR_RT_HeapBlock_Node* end, CLR_UINT32 blockSize )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    
    while(ptr < end)
    {
        if(ptr->DataType() == DATATYPE_WEAKCLASS)
        {
            CLR_RT_HeapBlock_WeakReference* weak = (CLR_RT_HeapBlock_WeakReference*)ptr;

            if(weak->DataSize() == CONVERTFROMSIZETOHEAPBLOCKS(sizeof(*weak)) && weak->m_targetSerialized != NULL && (weak->m_identity.m_flags & CLR_RT_HeapBlock_WeakReference::WR_SurviveBoot))
            {
                weak->SetNext( NULL );
                weak->SetPrev( NULL );

                weak->m_identity.m_flags &= ~CLR_RT_HeapBlock_WeakReference::WR_Persisted;

                weak->m_targetDirect = NULL;

                weak->MarkAlive();

                ptr += ptr->DataSize();
                continue;
            }
        }
        else if(ptr->DataType() == DATATYPE_SZARRAY)
        {
            CLR_RT_HeapBlock_Array* array = (CLR_RT_HeapBlock_Array*)ptr;

            if(array->m_typeOfElement == DATATYPE_U1 && array->m_fReference == 0)
            {
                CLR_UINT32 tot = sizeof(*array) + array->m_sizeOfElement * array->m_numOfElements;

                if(array->DataSize() == CONVERTFROMSIZETOHEAPBLOCKS(tot) && (ptr + ptr->DataSize()) <= end)
                {
                    array->MarkAlive();

                    ptr += ptr->DataSize();
                    continue;
                }
            }
        }

        if((UINT32)(ptr + blockSize) > (UINT32)end)
        {
            blockSize = (CLR_UINT32)(end - ptr);
        }

        ptr->SetDataId( CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_FREEBLOCK,0,blockSize) );
        ptr += blockSize;
    }

    return true;
}

void CLR_RT_HeapBlock_WeakReference::RecoverObjects( CLR_RT_DblLinkedList& lstHeap )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    TINYCLR_FOREACH_NODE(CLR_RT_HeapCluster,hc,lstHeap)
    {
        CLR_RT_HeapBlock_Node* ptr = hc->m_payloadStart;
        CLR_RT_HeapBlock_Node* end = hc->m_payloadEnd;

        while(ptr < end)
        {
            if(ptr->DataType() == DATATYPE_WEAKCLASS)
            {
                CLR_RT_HeapBlock_WeakReference* weak   = (CLR_RT_HeapBlock_WeakReference*)ptr;
                CLR_RT_HeapBlock*               target = weak->m_targetSerialized;

                TINYCLR_FOREACH_NODE(CLR_RT_HeapCluster,hc2,lstHeap)
                {
                    CLR_RT_HeapBlock_Node* ptr2 = hc2->m_payloadStart;
                    CLR_RT_HeapBlock_Node* end2 = hc2->m_payloadEnd;

                    if(ptr2 <= target && target < end2)
                    {
                        while(ptr2 < end2)
                        {
                            if(ptr2 == target)
                            {
                                CLR_RT_HeapBlock_Array* array = (CLR_RT_HeapBlock_Array*)ptr2;

                                if(array->m_numOfElements == weak->m_identity.m_length)
                                {
                                    if(weak->m_identity.m_crc == weak->m_identity.ComputeCRC( array->GetFirstElement(), weak->m_identity.m_length ))
                                    {
                                        weak->m_identity.m_flags |= CLR_RT_HeapBlock_WeakReference::WR_Restored;

                                        weak->InsertInPriorityOrder();

                                        weak = NULL;
                                    }
                                }

                                break;
                            }

                            ptr2 += ptr2->DataSize();
                        }

                        if(ptr2 < end2) break;
                    }
                }
                TINYCLR_FOREACH_NODE_END();

                if(weak)
                {
                    weak->ChangeDataType( DATATYPE_FREEBLOCK );
                }
            }

            ptr += ptr->DataSize();
        }
    }
    TINYCLR_FOREACH_NODE_END();
}
