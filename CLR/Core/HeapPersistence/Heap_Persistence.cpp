////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Core.h"

//--//

#if defined(TINYCLR_TRACE_PERSISTENCE)

void CLR_RT_Persistence_Manager::Trace_Emit( LPSTR szText )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    char   rgBuffer[ 512 ];
    LPSTR  szBuffer = rgBuffer;
    size_t iBuffer  = MAXSTRLEN(rgBuffer);
}

void CLR_RT_Persistence_Manager::Trace_Printf( LPCSTR format, ... )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    char    rgBuffer[ 512 ];
    LPSTR   szBuffer = rgBuffer;
    size_t  iBuffer  = MAXSTRLEN(rgBuffer);
    va_list arg;

    va_start( arg, format );

    CLR_SafeSprintfV( szBuffer, iBuffer, format, arg );

    va_end( arg );

    //--//

    Trace_Emit( rgBuffer );
}

void CLR_RT_Persistence_Manager::Trace_DumpIdentity( LPSTR& szBuffer, size_t& iBuffer, CLR_RT_HeapBlock_WeakReference_Identity* identity )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    CLR_RT_ReflectionDef_Index val;
    CLR_RT_TypeDef_Instance    inst;

    val.InitializeFromHash( identity->m_selectorHash );

    if(inst.InitializeFromReflection( val, NULL ))
    {
        CLR_SafeSprintf( szBuffer, iBuffer, " PRI:%9d ", identity->m_priority );

        g_CLR_RT_TypeSystem.BuildTypeName( inst, szBuffer, iBuffer );

        CLR_SafeSprintf( szBuffer, iBuffer, ":%d", identity->m_id );
    }
}

//--//

void CLR_RT_Persistence_Manager::Trace_DumpState( LPCSTR szText, FLASH_WORD* dst, ObjectHeader* oh, CLR_RT_HeapBlock_WeakReference* wr )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    char                                     rgBuffer[ 512 ];
    LPSTR                                    szBuffer = rgBuffer;
    size_t                                   iBuffer  = MAXSTRLEN(rgBuffer);
    CLR_RT_HeapBlock_WeakReference_Identity* identity = NULL;

    if(szText)
    {
        CLR_SafeSprintf( szBuffer, iBuffer, "%-20s", szText );
    }

    if(dst)
    {
        CLR_SafeSprintf( szBuffer, iBuffer, " STG:%08X", (size_t)dst );
    }

    if(wr)
    {
        CLR_RT_HeapBlock_Array* array = wr->m_targetSerialized;

        CLR_SafeSprintf( szBuffer, iBuffer, " %08X(%5u:%5u)", (size_t)array, wr->m_identity.m_length, array ? array->m_numOfElements : (CLR_UINT32)-1 );
    }

    if(oh)
    {
        identity = &oh->m_identity;
    }
    else if(wr)
    {
        identity = &wr->m_identity;
    }

    if(identity)
    {
        Trace_DumpIdentity( szBuffer, iBuffer, identity );
    }

    //--//

    Trace_Emit( rgBuffer );
}

#endif

////////////////////////////////////////////////////////////////////////////////////////////////////

bool CLR_RT_Persistence_Manager::ObjectHeader::Initialize( CLR_RT_HeapBlock_WeakReference* wr )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    if(!wr) return false;

    CLR_RT_HeapBlock_Array* array = wr->m_targetSerialized; if(!array) return false;

    m_signature         = c_Version;
    m_status            = c_InUseBlock;

    m_identity          = wr->m_identity;
    m_identity.m_crc    = m_identity.ComputeCRC( array->GetFirstElement(), m_identity.m_length );
    m_identity.m_flags &= CLR_RT_HeapBlock_WeakReference::WR_MaskForStorage;

    memcpy( &m_object, array, sizeof(m_object) );
    m_object.SetFlags( CLR_RT_HeapBlock::HB_Pinned | CLR_RT_HeapBlock::HB_Alive );

    m_crcIdentity       = ComputeCRC();

    return true;
}

bool CLR_RT_Persistence_Manager::ObjectHeader::IsGood( bool fIncludeData ) const
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    if(HasGoodSignature() == false) return false;

    if(m_crcIdentity != ComputeCRC()) return false;

    if(fIncludeData)
    {
        if(IsInUse() == false) return false;

        if(m_identity.m_crc != m_identity.ComputeCRC( (const CLR_UINT8*)&this[ 1 ], m_identity.m_length )) return false;
    }

    return true;
}

void CLR_RT_Persistence_Manager::ObjectHeader::Delete()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    CLR_UINT32 sig = c_DeletedBlock;

    Bank::Write( (FLASH_WORD*)&m_status, (FLASH_WORD*)&sig, sizeof(sig) );
}

CLR_UINT32 CLR_RT_Persistence_Manager::ObjectHeader::Length() const
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return ObjectHeader::Length( m_identity.m_length );
}

CLR_UINT32 CLR_RT_Persistence_Manager::ObjectHeader::Length( const CLR_RT_HeapBlock_WeakReference* ref )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return ObjectHeader::Length( ref->m_identity.m_length );
}

CLR_UINT32 CLR_RT_Persistence_Manager::ObjectHeader::Length( CLR_UINT32 data )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return ROUNDTOMULTIPLE(sizeof(ObjectHeader) + data,FLASH_WORD);
}

CLR_RT_Persistence_Manager::ObjectHeader* CLR_RT_Persistence_Manager::ObjectHeader::Next() const
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    ObjectHeader *next = (ObjectHeader*)((CLR_UINT8*)this + this->Length());
    return next;
}

CLR_UINT32 CLR_RT_Persistence_Manager::ObjectHeader::ComputeCRC() const
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    CLR_UINT32 crc;

    crc = SUPPORT_ComputeCRC( &m_identity, sizeof(m_identity), 0   );
    crc = SUPPORT_ComputeCRC( &m_object  , sizeof(m_object  ), crc );

    return crc;
}

CLR_RT_Persistence_Manager::ObjectHeader* CLR_RT_Persistence_Manager::ObjectHeader::Find( FLASH_WORD* start, FLASH_WORD* end )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();

    end = Bank::DecrementPointer( end, sizeof(ObjectHeader) );  

    while(start < end)
    {
        ObjectHeader* oh = (ObjectHeader*)start;  

        if(oh->HasGoodSignature())
        {
            return oh;
        }

        start++;
    }

    return NULL;
}

//--//--//--//--//--//

void CLR_RT_Persistence_Manager::BankHeader::Initialize()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    m_signature      = c_Version;
    m_status         = c_InUseBank;

    m_sequenceNumber = 0xFFFFFFFF;
}

bool CLR_RT_Persistence_Manager::BankHeader::IsGood() const
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return (HasGoodSignature() && IsInUse() && m_sequenceNumber != 0);
}

void CLR_RT_Persistence_Manager::BankHeader::Delete()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    CLR_UINT32 sig = c_DeletedBank;

    Bank::Write( (FLASH_WORD*)&m_status, (FLASH_WORD*)&sig, sizeof(sig) );
}

CLR_RT_Persistence_Manager::BankHeader* CLR_RT_Persistence_Manager::BankHeader::Find( FLASH_WORD* start, FLASH_WORD* end )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();

    end = Bank::DecrementPointer( end, sizeof(BankHeader) );

    while(start < end)
    {
        BankHeader* bh = (BankHeader*)start; 

        if(bh->IsGood())
        {
            return bh;
        }

        start++;
    }

    return NULL;
}

//--//--//--//--//--//



bool CLR_RT_Persistence_Manager::Bank::Initialize( UINT32 kind )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();

    // Bank may reinitialize again, have to keep the m_start for the buffer address.
    BYTE * ewrBuffer = (BYTE *)m_start;    
    TINYCLR_CLEAR(*this);

    //--//

    if(!m_stream.Initialize( (kind & BlockRange::USAGE_MASK) ))
    {
        return false;
    }

    const BlockDeviceInfo* deviceInfo = m_stream.Device->GetDeviceInfo();

    if (!deviceInfo->Attribute.SupportsXIP)
    {
        
        // if already assignd, don't allocated again.
        if (ewrBuffer == NULL)
        {
            ewrBuffer = (BYTE*)CLR_RT_Memory::Allocate_And_Erase( m_stream.Length, CLR_RT_HeapBlock ::HB_Unmovable );
            // no buffer, no EWR
            if (!ewrBuffer)
            {
                m_start = NULL;
                m_end = NULL;
                return false;
            }
        }
        m_start =(FLASH_WORD*) ewrBuffer;
        m_end   =(FLASH_WORD*)(ewrBuffer + m_stream.Length);

        
        // else it is assigned already. No need to do it.
            
        m_stream.Device->Read( m_stream.BaseAddress, m_stream.Length, (BYTE *)m_start );
    }
    else
    {
        m_start = (FLASH_WORD *)m_stream.BaseAddress;

        // avoid error of getting the address at the last block end address, which will fall out of the 
        // address range, calcuate it directly from m_start
        m_end = (FLASH_WORD *)((UINT32)m_start + m_stream.Length);
    }

    m_totalBytes     = (CLR_UINT32)((CLR_UINT8*)m_end - (CLR_UINT8*)m_start) - sizeof(BankHeader);
    m_totalSafeBytes = m_totalBytes * (100 - c_SafetyMargin) / 100;
    
    m_bankHeader = BankHeader::Find( m_start, m_end );

    if(m_bankHeader)
    {
        ObjectHeader* ptr = m_bankHeader->FirstObjectHeader();

        m_current = (FLASH_WORD*)ptr;

        while((ptr = ObjectHeader::Find( (FLASH_WORD*)ptr, m_end )) != NULL)
        {
            while(ptr->IsGood( false ))
            {
                ptr = ptr->Next();

                m_current = (FLASH_WORD*)ptr;

            }

            ptr = (ObjectHeader*)Bank::IncrementPointer( (FLASH_WORD*)ptr, sizeof(FLASH_WORD) );  
        }
    }

    return true;
        
}

bool CLR_RT_Persistence_Manager::Bank::IsGood() const
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    return m_bankHeader != NULL && m_current != NULL;
}

bool CLR_RT_Persistence_Manager::Bank::Erase( int& blockIndex )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    const BlockDeviceInfo*      deviceInfo = m_stream.Device->GetDeviceInfo();
    bool                        result  = TRUE;
    const BlockRegionInfo*      pRegion = NULL;
    const BlockRange*           pRange  = NULL;

    pRegion = &deviceInfo->Regions[ m_stream.RegionIndex ];

    pRange  = &pRegion->BlockRanges[ m_stream.RangeIndex ];

    if(pRange->GetBlockCount() <= (CLR_UINT32)blockIndex)
    {
        return false;
    }
    
    result = (TRUE == m_stream.Device->EraseBlock( pRegion->BlockAddress( pRange->StartBlock + blockIndex ) ));

    blockIndex++;
        
    if(!deviceInfo->Attribute.SupportsXIP)
    {    // reload the page.
       this->ReloadNonXIPBufferData();
    }

    return result;
}

void CLR_RT_Persistence_Manager::Bank::EraseAll()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();

    int idx = 0;

    while(Erase( idx ))
    {
        Watchdog_ResetCounter();
    }
}

bool CLR_RT_Persistence_Manager::Bank::Format()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    BankHeader  bh; bh.Initialize();
    FLASH_WORD* ptr    =                         m_start;
    FLASH_WORD* ptrEnd = Bank::DecrementPointer( m_end, sizeof(bh) );

    while(ptr < ptrEnd)
    {
        if(Bank::Write( ptr, (FLASH_WORD*)&bh, sizeof(bh) ))
        {
            return this->Initialize( m_stream.Usage );
        }

        Trace_Printf( "FAULT WRITING %08X(%d)", (size_t)ptr, sizeof(bh) );

        Bank::Invalidate( ptr, BankHeader::c_Version, sizeof(bh) );

        ptr++;
    }

    return false;
}

bool CLR_RT_Persistence_Manager::Bank::SetSequence( CLR_UINT32 sequenceNumber )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    if(m_bankHeader)
    {
        for(int retries=0; retries<10; retries++)
        {
            if(Bank::Write( (FLASH_WORD*)&m_bankHeader->m_sequenceNumber, &sequenceNumber, sizeof(sequenceNumber) ))
            {
                return true;
            }
        }
    }

    return false;
}

void CLR_RT_Persistence_Manager::Bank::Switch( Bank& other )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    Bank tmp;

    tmp = *this; *this = other; other = tmp;
}

//--//

CLR_RT_Persistence_Manager::ObjectHeader* CLR_RT_Persistence_Manager::Bank::RecoverHeader( CLR_RT_HeapBlock_WeakReference* ref )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    if(ref && ref->m_targetSerialized)
    {
        ObjectHeader* ptr = (ObjectHeader*)((CLR_UINT8*)ref->m_targetSerialized - offsetof(ObjectHeader,m_object));

        if((FLASH_WORD*)ptr >= m_start && (FLASH_WORD*)ptr < m_end && ptr->IsGood( true ))
        {
            return ptr;
        }
    }

    return NULL;
}

bool CLR_RT_Persistence_Manager::Bank::WriteHeader( CLR_RT_HeapBlock_WeakReference* ref, ObjectHeader*& pOH, FLASH_WORD*& pData )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    ObjectHeader oh;

    pOH   = NULL;
    pData = NULL;

    if(m_current && oh.Initialize( ref ))
    {
        CLR_UINT32  length    = oh.Length() + sizeof(ObjectHeader) + sizeof(FLASH_WORD);
        FLASH_WORD* endMarker = Bank::DecrementPointer( m_end, length );

        while(m_current < endMarker)
        {
            if(Bank::CanWrite( m_current, length ))
            {
                Trace_DumpState( "Saving header", m_current, &oh, ref );

                if(Bank::Write( m_current, (FLASH_WORD*)&oh, sizeof(oh) ))
                {
                    pOH = (ObjectHeader*)m_current;

                    pData     = Bank::IncrementPointer( m_current, sizeof(oh)  );

                    m_current = Bank::IncrementPointer( m_current, oh.Length() );
                    return true;
                }

                Trace_Printf( "FAULT WRITING %08X(%d)", (size_t)m_current, sizeof(oh) );
            }

            Bank::Invalidate( m_current, ObjectHeader::c_Version, sizeof(oh) );

            m_current++;
        }
    }

    return false;
}

void CLR_RT_Persistence_Manager::Bank::ReloadNonXIPBufferData()
{
    UINT32 lengthInBytes = 0;
    
    // buffer is allocated at initialize, no need to allocated, it is already point to by m_start;
    
    lengthInBytes = (UINT32)m_end - (UINT32)m_start;
    m_stream.Device->Read( m_stream.BaseAddress, lengthInBytes, (BYTE*) m_start );
}


bool CLR_RT_Persistence_Manager::Bank::WriteNonXIPData(FLASH_WORD* dst, CLR_UINT32 length)
{
    const BlockDeviceInfo *deviceInfo = m_stream.Device->GetDeviceInfo();

    bool fRes = true;
 
    // if SupportsXIP then error?
    if (deviceInfo->Attribute.SupportsXIP)
    {
#if !defined(BUILD_RTM)
        CLR_Debug::Printf("Error at try to write Non-XIP but found XIP\r\n");
#endif        
        return false;
    }
    
    UINT32 offset = (UINT32)dst - (UINT32)m_start;

    if(!m_stream.Device->Write( m_stream.BaseAddress + offset, length, (BYTE *)dst, FALSE ))
    {
        fRes=false;
    }

    return fRes;
}

bool CLR_RT_Persistence_Manager::Bank::FindBankWriteNonXIPData( FLASH_WORD* dst, CLR_UINT32 length )
{
    if (((UINT32)dst >=(UINT32)g_CLR_RT_Persistence_Manager.m_bankA.m_start) && ((UINT32)dst <(UINT32)g_CLR_RT_Persistence_Manager.m_bankA.m_end))
        return g_CLR_RT_Persistence_Manager.m_bankA.WriteNonXIPData( dst, length );

    else if (((UINT32)dst >=(UINT32)g_CLR_RT_Persistence_Manager.m_bankB.m_start) && ((UINT32)dst <(UINT32)g_CLR_RT_Persistence_Manager.m_bankB.m_end))
        return g_CLR_RT_Persistence_Manager.m_bankB.WriteNonXIPData( dst, length );

    else
    {
#if !defined(BUILD_RTM)
        CLR_Debug::Printf("No right Persistence bank address found!!! (%x)\r\n",(UINT32)dst );
#endif
        return false;
    }
}


//--//

bool CLR_RT_Persistence_Manager::Bank::CanWrite( FLASH_WORD* dst, CLR_UINT32 length )
{    
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();

    for(CLR_UINT32 pos=0; pos<length; pos+=sizeof(FLASH_WORD))
    {
        if(*dst != c_Erased) return false; 

        dst++;
    }

    return true;
}

bool CLR_RT_Persistence_Manager::Bank::Write( FLASH_WORD* dst, const FLASH_WORD* src, CLR_UINT32 length )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    bool fRes = true;
    const BlockDeviceInfo *deviceInfo = g_CLR_RT_Persistence_Manager.m_bankA.m_stream.Device->GetDeviceInfo();

    if (deviceInfo->Attribute.SupportsXIP)
    {
        if(!g_CLR_RT_Persistence_Manager.m_bankA.m_stream.Device->Write( (UINT32)dst, length,(BYTE *)src, FALSE ))
        {
            fRes=false;
        }

     }
     else
     {
        //update the buffer
        memcpy( dst, src, length );
        fRes = FindBankWriteNonXIPData( dst, length );
     }

    return fRes;
}

void CLR_RT_Persistence_Manager::Bank::Invalidate( FLASH_WORD* dst, FLASH_WORD match, CLR_UINT32 length )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();

    const BlockDeviceInfo *deviceInfo = g_CLR_RT_Persistence_Manager.m_bankA.m_stream.Device->GetDeviceInfo();
    FLASH_WORD * start_dst = dst;

    FLASH_WORD data=c_Invalidated;

    if (deviceInfo->Attribute.SupportsXIP)
    {
        for(CLR_UINT32 pos=0; pos<length; pos+=sizeof(FLASH_WORD))
        {
            if(*dst == match) 
            {
                g_CLR_RT_Persistence_Manager.m_bankA.m_stream.Device->Write( (UINT32)dst, sizeof(FLASH_WORD), (BYTE *) &data, FALSE );
            }
            dst++;
        }
    }
    else
    {
        // update the buffer
        for(CLR_UINT32 pos=0; pos<length; pos+=sizeof(FLASH_WORD))
        {
            if(*dst == match) 
            {
                *dst = data;
            }
            dst++;
        }
        FindBankWriteNonXIPData( start_dst, length );
    }

}

//--//

void CLR_RT_Persistence_Manager::Uninitialize()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    m_completion.Abort();
}


void CLR_RT_Persistence_Manager::Initialize()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    TINYCLR_CLEAR(*this);

    //--//

                                                  // HAL_COMPLETION                  m_completion;
                                                  //
                                                  // UINT32                          m_margin_BurstWrite;
                                                  // UINT32                          m_margin_BlockErase;
                                                  //
    if(!m_bankA.Initialize( BlockUsage::STORAGE_A )) return; // Bank                            m_bankA;
    if(!m_bankB.Initialize( BlockUsage::STORAGE_B )) return; // Bank                            m_bankB;
                                                  //
    m_state = STATE_FlushNextObject;              // CLR_UINT32                      m_state;
                                                  //
                                                  // CLR_RT_HeapBlock_WeakReference* m_pending_object;
                                                  // ObjectHeader*                   m_pending_header;
                                                  // CLR_UINT32                      m_pending_size;
                                                  // FLASH_WORD*                     m_pending_src;
                                                  // FLASH_WORD*                     m_pending_dst;

    m_completion.InitializeForUserMode( CLR_RT_Persistence_Manager::Callback );

    m_margin_BurstWrite  = (m_bankA.m_stream.Device->MaxSectorWrite_uSec() * (c_MaxWriteBurst / sizeof(FLASH_WORD)) * 2 + 1000 - 1) / 1000;
    m_margin_BlockErase  = (m_bankA.m_stream.Device->MaxBlockErase_uSec()  *                                          2 + 1000 - 1) / 1000;
    if(m_bankB.IsGood())
    {
        if(m_bankA.IsGood() == false || m_bankA.GetBankHeader()->m_sequenceNumber < m_bankB.GetBankHeader()->m_sequenceNumber)
        {
            m_bankA.Switch( m_bankB );
        }
    }

    if(m_bankA.IsGood() == false)
    {
        m_bankA.EraseAll   (   );
        m_bankA.Format     (   );
        m_bankA.SetSequence( 1 );
    }

#if defined(TINYCLR_TRACE_PERSISTENCE)
    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_WeakReference,weak,g_CLR_RT_ExecutionEngine.m_weakReferences)
    {
        Trace_DumpState( "Restored from RAM", NULL, NULL, weak );
    }
    TINYCLR_FOREACH_NODE_END();
#endif

    if(m_bankA.IsGood())
    {
        ObjectHeader* ptr = m_bankA.GetBankHeader()->FirstObjectHeader();

        while((ptr = ObjectHeader::Find( (FLASH_WORD*)ptr, m_bankA.m_end )) != NULL)
        {
            while(ptr->IsGood( false ))
            {
                if(ptr->IsGood( true ))
                {
                    CLR_RT_HeapBlock_WeakReference* weakRef;

                    if(SUCCEEDED(CLR_RT_HeapBlock_WeakReference::CreateInstance( weakRef )))
                    {
                        weakRef->m_identity         =  ptr->m_identity;
                        weakRef->m_targetSerialized = &ptr->m_object;

                        weakRef->InsertInPriorityOrder();

                        weakRef->m_identity.m_flags |= CLR_RT_HeapBlock_WeakReference::WR_Persisted;
                        weakRef->m_identity.m_flags |= CLR_RT_HeapBlock_WeakReference::WR_Restored;

                        Trace_DumpState( "Restored from FLASH", (FLASH_WORD*)ptr, ptr, weakRef );
                    }
                }
                else
                {
                    Trace_DumpState( "Skip deleted item", (FLASH_WORD*)ptr, ptr, NULL );
                }

                ptr = ptr->Next();
            }

            ptr = (ObjectHeader*)Bank::IncrementPointer( (FLASH_WORD*)ptr, sizeof(FLASH_WORD) );
        }
    }

    g_CLR_RT_ExecutionEngine.LoadDownloadedAssemblies();

    EnqueueNextCallback();
}

void CLR_RT_Persistence_Manager::EraseAll()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    if(!m_bankA.Initialize( BlockRange::BLOCKTYPE_STORAGE_A )) return;
    if(!m_bankB.Initialize( BlockRange::BLOCKTYPE_STORAGE_B )) return;

    m_bankA.EraseAll();
    m_bankB.EraseAll();
}

//--//

CLR_RT_Persistence_Manager::ObjectHeader* CLR_RT_Persistence_Manager::RecoverHeader( CLR_RT_HeapBlock_WeakReference* weak )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    ObjectHeader* oh;

    oh = m_bankA.RecoverHeader( weak );
    if(oh == NULL)
    {
        oh = m_bankB.RecoverHeader( weak );
    }

    return oh;
}

void CLR_RT_Persistence_Manager::InvalidateEntry( CLR_RT_HeapBlock_WeakReference* weak )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    if(m_pending_object == weak)
    {
        if(m_pending_header)
        {
            Trace_DumpState( "Abort object", (FLASH_WORD*)m_pending_header, m_pending_header, weak );

            m_pending_header->Delete();

            m_pending_header = NULL;
        }

        m_pending_object = NULL;
    }

    if(weak->m_identity.m_flags & CLR_RT_HeapBlock_WeakReference::WR_Persisted)
    {
        ObjectHeader* oh = RecoverHeader( weak );
        if(oh != NULL)
        {
            Trace_DumpState( "Remove object", (FLASH_WORD*)oh, oh, weak );
            oh->Delete();
        }

        weak->m_identity.m_flags &= ~CLR_RT_HeapBlock_WeakReference::WR_Persisted;
    }

    weak->m_targetSerialized  = NULL;
    weak->m_identity.m_flags &= ~CLR_RT_HeapBlock_WeakReference::WR_Restored;

    switch(m_state)
    {
    case STATE_FlushNextObject   :                                                          break;
    case STATE_Idle              : m_state = STATE_FlushNextObject; EnqueueNextCallback() ; break;
    case STATE_Erase             :                                                          break;
    case STATE_EraseSector       :                                                          break;
    //////////////////////////////
    case STATE_CopyToOtherBank   :
    case STATE_CopyBackToRam     :
    case STATE_SwitchBank        : m_state = STATE_Erase; Trace_Printf( "Aborting erase" ); break;
    }
}

//--//

void CLR_RT_Persistence_Manager::Callback( void* arg )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();

    if(g_CLR_RT_Persistence_Manager.AdvanceState( false ))
    {
        g_CLR_RT_Persistence_Manager.EnqueueNextCallback();
    }
    else
    {
        Trace_Printf( "Content synchronized!!" );
    }
    
}

void CLR_RT_Persistence_Manager::EnqueueNextCallback()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    m_completion.Abort  ( );
    m_completion.EnqueueDelta( 50000 );
}

void CLR_RT_Persistence_Manager::Relocate()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    if(m_pending_object) CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_pending_object );
    if(m_pending_header) CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_pending_header );
    if(m_pending_src   ) CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_pending_src    );
    if(m_pending_dst   ) CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_pending_dst    );
}

bool CLR_RT_Persistence_Manager::AdvanceState( bool force )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    CLR_RT_GarbageCollector::RelocationRegion relocHelper[ CLR_RT_GarbageCollector::c_minimumSpaceForCompact ];
    while(true)
    {
        ::Watchdog_ResetCounter();
        switch(m_state)
        {
        case STATE_FlushNextObject:
            {
                if(!force && g_CLR_RT_ExecutionEngine.IsThereEnoughIdleTime( m_margin_BurstWrite ) == false) return true;

                if(m_pending_object == NULL)
                {
                    CLR_UINT32 usedBytes = 0;

                    //
                    // The list is sorted in priority order, we save higher priority objects first.
                    //
                    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_WeakReference,weak,g_CLR_RT_ExecutionEngine.m_weakReferences)
                    {
                        if(weak->m_identity.m_flags & CLR_RT_HeapBlock_WeakReference::WR_SurvivePowerdown)
                        {
                            CLR_RT_HeapBlock_Array* array = weak->m_targetSerialized;

                            if(array)
                            {
                                usedBytes += ObjectHeader::Length( weak );

                                if((weak->m_identity.m_flags & CLR_RT_HeapBlock_WeakReference::WR_Persisted) == 0)
                                {
                                    if(m_bankA.WriteHeader( weak, m_pending_header, m_pending_dst ))
                                    {
                                        m_pending_object =              weak;
                                        m_pending_src    = (FLASH_WORD*)array->GetFirstElement();
                                        m_pending_size   =              array->m_numOfElements;
                                    }
                                    else
                                    {
                                        m_pending_src  = NULL;
                                        m_pending_size = 0;

                                        if(usedBytes < m_bankA.m_totalSafeBytes)
                                        {
                                            Trace_DumpState( "Triggering copy", NULL, NULL, weak );

                                            //
                                            // We found a non-persisted object that would fit in the storage if we compact.
                                            //
                                            // Let's start the bank switch.
                                            //
                                            m_state = STATE_Erase;
                                            return true;
                                        }

                                        Trace_DumpState( "Skipping block", NULL, NULL, weak );

                                        //
                                        // We don't have enough safety margin.
                                        //
                                        // Nothing to gain from a bank swicth.
                                        //
                                    }

                                    break;
                                }
                            }
                        }
                    }
                    TINYCLR_FOREACH_NODE_END();

                    if(m_pending_object == NULL)
                    {
                        m_state = STATE_Idle;
                        return false;
                    }
                }

                Trace_DumpState( "Saving block", m_pending_dst, m_pending_header, m_pending_object );

                CLR_UINT32 len = m_pending_size; if(len > c_MaxWriteBurst) len = c_MaxWriteBurst;

                if(Bank::Write( m_pending_dst, m_pending_src, len ))
                {
                    m_pending_dst   = Bank::IncrementPointer( m_pending_dst, len );
                    m_pending_src   = Bank::IncrementPointer( m_pending_src, len );
                    m_pending_size -= len;

                    if(m_pending_size == 0)
                    {
                        if(m_pending_object->m_identity.m_flags & CLR_RT_HeapBlock_WeakReference::WR_ArrayOfBytes)
                        {
                            CLR_UINT8* dst = (CLR_UINT8*)&m_pending_header->m_object;
                            CLR_UINT8* src = (CLR_UINT8*) m_pending_object->m_targetSerialized;
                            CLR_UINT32 len =              m_pending_object->m_targetSerialized->DataSize() * sizeof(CLR_RT_HeapBlock);

                            Trace_Printf( "Relocate Array - Flush: %08X %08X %d", (size_t)dst, (size_t)src, len );

                            g_CLR_RT_GarbageCollector.Heap_Relocate_Prepare ( relocHelper, ARRAYSIZE(relocHelper) );
                            g_CLR_RT_GarbageCollector.Heap_Relocate_AddBlock( dst, src, len                       );
                            g_CLR_RT_GarbageCollector.Heap_Relocate         (                                     );
                        }
                        else
                        {
                            m_pending_object->m_targetSerialized = &m_pending_header->m_object;
                        }

                        m_pending_object->m_identity.m_flags |= CLR_RT_HeapBlock_WeakReference::WR_Persisted;

                        m_pending_object = NULL;
                        m_pending_header = NULL;
                        m_pending_src    = NULL;
                        m_pending_dst    = NULL;
                    }
                }
                else
                {
                    Trace_Printf( "FAULT WRITING %08X(%d)", (size_t)m_pending_dst, len );

                    m_pending_header->Delete();

                    m_pending_object = NULL;
                    m_pending_header = NULL;
                    m_pending_src    = NULL;
                    m_pending_dst    = NULL;
                }
            }
            break;

        case STATE_Idle:
            {
                return false;
            }
            break;

        case STATE_Erase:
            {
                if(!force && g_CLR_RT_ExecutionEngine.IsThereEnoughIdleTime( m_margin_BlockErase ) == false) return true;

                Trace_Printf( "Start erase of storage" );

                TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_WeakReference,weak,g_CLR_RT_ExecutionEngine.m_weakReferences)
                {
                    weak->m_targetCopied = NULL;
                }
                TINYCLR_FOREACH_NODE_END();

                m_pending_object = NULL;
                m_pending_header = NULL;
                m_pending_src    = NULL;
                m_pending_dst    = NULL;
                m_pending_size   = 0;

                m_eraseIndex = 0;

                m_state = STATE_EraseSector;
            }
            break;

        case STATE_EraseSector:
            {
                if(!force && g_CLR_RT_ExecutionEngine.IsThereEnoughIdleTime( m_margin_BlockErase  ) == false) return true;

                if(m_bankB.Erase( m_eraseIndex ) )
                {
                    m_bankB.Format();

                    m_state = STATE_CopyToOtherBank;
                }
            }
            break;

        case STATE_CopyToOtherBank:
            {
                if(!force && g_CLR_RT_ExecutionEngine.IsThereEnoughIdleTime( m_margin_BurstWrite ) == false) return true;

                if(m_pending_object == NULL)
                {
                    CLR_UINT32 usedBytes = 0;

                    //
                    // The list is sorted in priority order, we save higher priority objects first.
                    //
                    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_WeakReference,weak,g_CLR_RT_ExecutionEngine.m_weakReferences)
                    {
                        if(weak->m_identity.m_flags & CLR_RT_HeapBlock_WeakReference::WR_SurvivePowerdown)
                        {
                            CLR_RT_HeapBlock_Array* array = weak->m_targetSerialized;

                            if(array)
                            {
                                usedBytes += ObjectHeader::Length( weak );

                                if((weak->m_identity.m_flags & CLR_RT_HeapBlock_WeakReference::WR_Persisted) && weak->m_targetCopied == NULL)
                                {
                                    if(usedBytes < m_bankB.m_totalSafeBytes && m_bankB.WriteHeader( weak, m_pending_header, m_pending_dst ))
                                    {
                                        m_pending_object =              weak;
                                        m_pending_src    = (FLASH_WORD*)array->GetFirstElement();
                                        m_pending_size   =              array->m_numOfElements;
                                    }

                                    break;
                                }
                            }
                        }
                    }
                    TINYCLR_FOREACH_NODE_END();

                    if(m_pending_object == NULL)
                    {
                        m_state = STATE_CopyBackToRam;
                        break;
                    }
                }

                Trace_DumpState( "Copying block", m_pending_dst, m_pending_header, m_pending_object );

                CLR_UINT32 len = m_pending_size; if(len > c_MaxWriteBurst) len = c_MaxWriteBurst;

                if(Bank::Write( m_pending_dst, m_pending_src, len ))
                {
                    m_pending_dst   = Bank::IncrementPointer( m_pending_dst, len );
                    m_pending_src   = Bank::IncrementPointer( m_pending_src, len );
                    m_pending_size -= len;

                    if(m_pending_size == 0)
                    {
                        m_pending_object->m_targetCopied = &m_pending_header->m_object;

                        m_pending_object = NULL;
                        m_pending_header = NULL;
                        m_pending_src    = NULL;
                        m_pending_dst    = NULL;
                    }
                }
                else
                {
                    Trace_Printf( "FAULT WRITING %08X(%d)", (size_t)m_pending_dst, len );

                    m_pending_header->Delete();

                    m_pending_object = NULL;
                    m_pending_header = NULL;
                    m_pending_src    = NULL;
                    m_pending_dst    = NULL;
                }
            }
            break;

        case STATE_CopyBackToRam:
            {
                if(!force && g_CLR_RT_ExecutionEngine.IsThereEnoughIdleTime( m_margin_BurstWrite ) == false) return true;

                if(m_pending_object == NULL)
                {
                    //
                    // The list is sorted in priority order, we save higher priority objects first.
                    //
                    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_WeakReference,weak,g_CLR_RT_ExecutionEngine.m_weakReferences)
                    {
                        if((weak->m_identity.m_flags & CLR_RT_HeapBlock_WeakReference::WR_Persisted) && weak->m_targetCopied == NULL)
                        {
                            m_pending_header = m_bankA.RecoverHeader( weak );

                            if(m_pending_header != NULL)
                            {
                                m_pending_object = weak;
                                break;
                            }
                        }
                    }
                    TINYCLR_FOREACH_NODE_END();

                    if(m_pending_object == NULL)
                    {
                        m_state = STATE_SwitchBank;
                        break;
                    }
                }

                {
                    CLR_RT_HeapBlock ref;
                    CLR_UINT32       len = m_pending_header->m_object.m_numOfElements;

                    if(SUCCEEDED(CLR_RT_HeapBlock_Array::CreateInstance( ref, len, g_CLR_RT_WellKnownTypes.m_UInt8 )))
                    {
                        CLR_RT_HeapBlock_Array* array = ref.DereferenceArray();

                        memcpy( array->GetFirstElement(), m_pending_header->m_object.GetFirstElement(), len );

                        if(m_pending_object->m_identity.m_flags & CLR_RT_HeapBlock_WeakReference::WR_ArrayOfBytes)
                        {
                            CLR_UINT8* dst = (CLR_UINT8*)array;
                            CLR_UINT8* src = (CLR_UINT8*)m_pending_object->m_targetSerialized;
                            CLR_UINT32 len =             m_pending_object->m_targetSerialized->DataSize() * sizeof(CLR_RT_HeapBlock);

                            Trace_Printf( "Relocate Array - Copy: %08X %08X %d", (size_t)dst, (size_t)src, len );

                            g_CLR_RT_GarbageCollector.Heap_Relocate_Prepare ( relocHelper, ARRAYSIZE(relocHelper) );
                            g_CLR_RT_GarbageCollector.Heap_Relocate_AddBlock( dst, src, len                       );
                            g_CLR_RT_GarbageCollector.Heap_Relocate         (                                     );
                        }
                        else
                        {
                            m_pending_object->m_targetSerialized = array;
                        }

                        m_pending_object->m_identity.m_flags &= ~CLR_RT_HeapBlock_WeakReference::WR_Persisted;

                        Trace_DumpState( "Revert block to RAM", NULL, m_pending_header, m_pending_object );

                        m_pending_header->Delete();
                    }
                    else
                    {
                        //
                        // This is bad, out of memory, it should not happen during revert to RAM.
                        //
                        // Another case this could happen is if we try to move a big chunk back to RAM and
                        // the heap is too fragmented to proceed.
                        //
                        // Only course of action: abort the copy and restart everything, sooner or later the memory will be there.
                        //
                        m_state = STATE_Erase;
                        return true;
                    }

                    m_pending_header = NULL;
                    m_pending_object = NULL;
                }
            }
            break;

        case STATE_SwitchBank:
            {
                Trace_Printf( "Switching banks" );

                g_CLR_RT_GarbageCollector.Heap_Relocate_Prepare( relocHelper, ARRAYSIZE(relocHelper) );

                TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_WeakReference,weak,g_CLR_RT_ExecutionEngine.m_weakReferences)
                {
                    if(weak->m_targetCopied)
                    {
                        if(weak->m_identity.m_flags & CLR_RT_HeapBlock_WeakReference::WR_ArrayOfBytes)
                        {
                            CLR_UINT8* dst = (CLR_UINT8*)weak->m_targetCopied;
                            CLR_UINT8* src = (CLR_UINT8*)weak->m_targetSerialized;
                            CLR_UINT32 len =             weak->m_targetSerialized->DataSize() * sizeof(CLR_RT_HeapBlock);

                            Trace_Printf( "Relocate Array - Switch: %08X %08X %d", (size_t)dst, (size_t)src, len );

                            g_CLR_RT_GarbageCollector.Heap_Relocate_AddBlock( dst, src, len );
                        }
                        else
                        {
                            weak->m_targetSerialized = weak->m_targetCopied;
                        }

                        weak->m_targetCopied = NULL;
                    }
                }
                TINYCLR_FOREACH_NODE_END();

                g_CLR_RT_GarbageCollector.Heap_Relocate();

                m_bankB.SetSequence( m_bankA.GetBankHeader()->m_sequenceNumber + 1 );
                m_bankB.Switch     ( m_bankA                                       );

                m_state = STATE_FlushNextObject;
            }
            break;

        default:
            return false;
        }
    }

    return true;
}


void CLR_RT_Persistence_Manager::Flush()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    AdvanceState( true );
}

//--//

#if !defined(BUILD_RTM)

void CLR_RT_Persistence_Manager::GenerateStatistics( CLR_UINT32& totalSize, CLR_UINT32& inUse )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    totalSize = 0;
    inUse     = 0;

    if(m_bankA.IsGood())
    {
        totalSize = m_bankA.m_totalBytes;

        if(m_bankA.m_bankHeader)
        {
            ObjectHeader* ptr = m_bankA.m_bankHeader->FirstObjectHeader();

            while((ptr = ObjectHeader::Find( (FLASH_WORD*)ptr, m_bankA.m_end )) != NULL)
            {
                while(ptr->IsGood( false ))
                {
                    if(ptr->IsGood( true ))
                    {
                        inUse += ptr->Length();
                    }

                    ptr = ptr->Next();
                }

                ptr = (ObjectHeader*)Bank::IncrementPointer( (FLASH_WORD*)ptr, sizeof(FLASH_WORD) );
            }
        }
    }
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

    {
        CLR_RT_HeapBlock* target = targetReference.Dereference();
        CLR_RT_HeapBlock  input ; input .SetObjectReference( target );
        CLR_RT_HeapBlock  output; output.SetObjectReference( NULL   );

        CLR_RT_ProtectFromGC gcInput ( input  );
        CLR_RT_ProtectFromGC gcOutput( output );

        m_targetDirect = target;

        //
        // Check if this is an Extended Weak Reference before calling SerializationEnabled().
        // Checking a flag is faster than calling a function that is unlikely to be inlined
        // (from a separate static library)
        //
        // As WR_ExtendedType can only be set in the constructor of the ExtendedWeakReference class,
        // this flag cannot be unset after an EWR is persisted to flash.
        // Thus, the EWR invalidation code only needs to be called when WR_ExtendedType is set,
        // allowing it to be inside the following 'if' block, and minimizing the number of 'if' conditions
        // to check for the non EWR case.
        //
        // This is particularly important to TinyCore and System.Threading.Dispatcher, which uses a
        // WeakReference to cache the last used Dispatcher and improve event throughput.
        //
        if((m_identity.m_flags & CLR_RT_HeapBlock_WeakReference::WR_ExtendedType) != 0 &&
           CLR_RT_BinaryFormatter::SerializationEnabled())
        {
            if(target)
            {
                switch(target->DataType())
                {
                case DATATYPE_SZARRAY:
                    {
                        CLR_RT_HeapBlock_Array* array = (CLR_RT_HeapBlock_Array*)target;

                        if(array->m_typeOfElement == DATATYPE_U1)
                        {
                            output.SetObjectReference( array );
                            m_identity.m_flags |= CLR_RT_HeapBlock_WeakReference::WR_ArrayOfBytes;
                            break;
                        }
                    }
                    //
                    // Fall through!!!
                    //
                case DATATYPE_STRING:
                case DATATYPE_CLASS:
                case DATATYPE_VALUETYPE:
                    {
                        TINYCLR_CHECK_HRESULT(CLR_RT_BinaryFormatter::Serialize( output, input, NULL, 0 ));

                        m_identity.m_flags &= ~CLR_RT_HeapBlock_WeakReference::WR_ArrayOfBytes;
                    }
                    break;

                default:
                    TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                }
            }

            {
                CLR_RT_HeapBlock_Array* targetSerialized = output.DereferenceArray();
                CLR_UINT32 len     =   targetSerialized ?   targetSerialized->m_numOfElements : 0;
                CLR_UINT32 lenOrig = m_targetSerialized ? m_targetSerialized->m_numOfElements : 0;

                //
                // Check if we need to invalidate an entry.
                //
                if(len != lenOrig || (len > 0 && memcmp( targetSerialized->GetFirstElement(), m_targetSerialized->GetFirstElement(), len )))
                {
                    g_CLR_RT_Persistence_Manager.InvalidateEntry( this );
                    m_identity.m_length = len;

                    if(len > 0)
                    {
                        m_identity.m_crc    = m_identity.ComputeCRC( targetSerialized->GetFirstElement(), len );
                        m_targetSerialized  = targetSerialized;
                    }
                    else
                    {
                        m_identity.m_crc    = 0;
                        m_targetSerialized  = NULL;
                    }
                }
            }
        }
    }

    InsertInPriorityOrder();

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_WeakReference::GetTarget( CLR_RT_HeapBlock& targetReference )
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    TINYCLR_HEADER();

    if(CLR_RT_BinaryFormatter::SerializationEnabled())
    {
        targetReference.SetObjectReference( NULL );

        if(m_targetDirect == NULL && m_targetSerialized != NULL)
        {
            if(m_identity.m_flags & CLR_RT_HeapBlock_WeakReference::WR_ArrayOfBytes)
            {
                m_targetDirect = m_targetSerialized;
            }
            else
            {
                CLR_RT_HeapBlock input ; input .SetObjectReference( m_targetSerialized );
                CLR_RT_HeapBlock output; output.SetObjectReference( NULL               );

                {
                    CLR_RT_ProtectFromGC gcInput ( input  );
                    CLR_RT_ProtectFromGC gcOutput( output );

                    if(FAILED(CLR_RT_BinaryFormatter::Deserialize( output, input, NULL, NULL, 0 )))
                    {
                        output.SetObjectReference( NULL );
                    }
                }

                m_targetDirect = output.Dereference();
            }
        }
    }

    targetReference.SetObjectReference( m_targetDirect );

    TINYCLR_NOCLEANUP_NOLABEL();
}

void CLR_RT_HeapBlock_WeakReference::InsertInPriorityOrder()
{
    NATIVE_PROFILE_CLR_HEAP_PERSISTENCE();
    this->Unlink(); // Remove from the list before looking for a spot, to avoid comparing against ourselves.

    if(m_identity.m_flags & CLR_RT_HeapBlock_WeakReference::WR_ExtendedType)
    {
        CLR_INT32                       pri = m_identity.m_priority;
        CLR_RT_HeapBlock_WeakReference* ptr = (CLR_RT_HeapBlock_WeakReference*)g_CLR_RT_ExecutionEngine.m_weakReferences.FirstNode();
        while(true)
        {
            CLR_RT_HeapBlock_WeakReference* ptrNext = (CLR_RT_HeapBlock_WeakReference*)ptr->Next(); if(!ptrNext) break;

            if(ptr->m_identity.m_priority <= pri) break;

            ptr = ptrNext;
        }

        g_CLR_RT_ExecutionEngine.m_weakReferences.InsertBeforeNode( ptr, this );
    }
    else
    {
        g_CLR_RT_ExecutionEngine.m_weakReferences.LinkAtBack( this );
    }
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

