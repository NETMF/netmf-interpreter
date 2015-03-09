////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"
#include "FAT_FS.h"
#include "FAT_FS_Utility.h"
#include "TinyCLR_Interop.h"

//--//

void FAT_SectorCache::OnFlushCallback(void* arg)
{
    FAT_SectorCache* pThis = (FAT_SectorCache*)arg;

    pThis->FlushAll();
}

/////////////////////////////////////////////////////////
//    IO buffer scenario
//          buffer size: one block size in hardware device
//          when block size is changed(For different region can has different block size) ,
//          delete old buffer and re-allocate new buffer
//     
//     *m_IOBuffer--IO buffer pointer
//    m_bufferSectorBegin--FAT FS sector index at the beginning of IO buffer
//    m_bufferSectorEnd--FAT FS sector index at the end of IO buffer
//    m_bufferDirty--IO buffer pointer has been rewrited and need to flush
////////////////////////////////////////////////////////////////

void FAT_SectorCache::Initialize( BlockStorageDevice* blockStorageDevice, UINT32 bytesPerSector, UINT32 baseAddress, UINT32 sectorCount )
{
    m_blockStorageDevice = blockStorageDevice;
    m_bytesPerSector     = bytesPerSector;
    m_sectorCount        = sectorCount;
    m_baseByteAddress    = baseAddress;
    m_sectorsPerLine     = SECTORCACHE_LINESIZE / bytesPerSector;
    m_LRUCounter         = 1;

    if((FAT_FS__CACHE_FLUSH_TIMEOUT_USEC) != 0)
    {
        m_flushCompletion.InitializeForUserMode((HAL_CALLBACK_FPN)FAT_SectorCache::OnFlushCallback, this);
    }

    for(int i = 0; i < ARRAYSIZE(m_cacheLines); i++)
    {
        m_cacheLines[i].m_buffer = NULL;
        m_cacheLines[i].m_flags  = 0;
        m_cacheLines[i].m_begin  = 0;
        m_cacheLines[i].m_bsByteAddress = 0;
#ifdef FAT_FS__VALIDATE_READONLY_CACHELINE
        m_cacheLines[i].m_crc    = 0;
#endif
    }
}

void FAT_SectorCache::Uninitialize()
{
    if(FAT_FS__CACHE_FLUSH_TIMEOUT_USEC != 0 && m_flushCompletion.IsLinked())
    {
        m_flushCompletion.Abort();
    }
    
    FAT_CacheLine* cacheLine;
    for(int i = 0; i < ARRAYSIZE(m_cacheLines); i++)
    {
        cacheLine = &m_cacheLines[i];
        
        if(cacheLine->m_buffer)
        {
            FlushSector( cacheLine );

            private_free( cacheLine->m_buffer );

            cacheLine->m_buffer = NULL;
            cacheLine->m_flags  = 0;
            cacheLine->m_begin  = 0;
            cacheLine->m_bsByteAddress = 0;
#ifdef FAT_FS__VALIDATE_READONLY_CACHELINE
            cacheLine->m_crc    = 0;
#endif
        }
    }
}

BYTE* FAT_SectorCache::GetSector( UINT32 sectorIndex, BOOL forWrite )
{
    return GetSector(sectorIndex, TRUE, forWrite);
}

BYTE* FAT_SectorCache::GetSector( UINT32 sectorIndex, BOOL useLRU, BOOL forWrite )
{
    if(sectorIndex > m_sectorCount)  //sectorIndex out of range of this device
        return NULL;

    FAT_CacheLine* cacheLine = GetCacheLine( sectorIndex );

    if(!cacheLine)
    {
        cacheLine = GetUnusedCacheLine(useLRU);

        if(!cacheLine->m_buffer)
        {
            cacheLine->m_buffer = (BYTE*)private_malloc( SECTORCACHE_LINESIZE );

            if(!cacheLine->m_buffer) return NULL;
        }

        cacheLine->m_begin         = sectorIndex - (sectorIndex % m_sectorsPerLine);
        cacheLine->m_bsByteAddress = m_baseByteAddress + cacheLine->m_begin * m_bytesPerSector;
        cacheLine->m_flags         = 0;
        cacheLine->SetLRUCOunter( ++m_LRUCounter );

        if(!m_blockStorageDevice->Read( cacheLine->m_bsByteAddress, SECTORCACHE_LINESIZE, cacheLine->m_buffer ))
        {
            private_free( cacheLine->m_buffer );
            
            cacheLine->m_buffer = NULL;
            
            return NULL;
        }
#ifdef FAT_FS__VALIDATE_READONLY_CACHELINE
        cacheLine->m_crc = SUPPORT_ComputeCRC(cacheLine->m_buffer, SECTORCACHE_LINESIZE, 0);
#endif
    }

    if(forWrite)
    {
        cacheLine->SetDirty( TRUE );

        if((FAT_FS__CACHE_FLUSH_TIMEOUT_USEC) != 0 && !m_flushCompletion.IsLinked())
        {
            m_flushCompletion.EnqueueDelta(FAT_FS__CACHE_FLUSH_TIMEOUT_USEC);
        }
    }

    if((cacheLine->GetLRUCounter()) != m_LRUCounter)
    {
        m_LRUCounter++;

        cacheLine->SetLRUCOunter( m_LRUCounter );
    }

    return cacheLine->m_buffer + (sectorIndex - cacheLine->m_begin) * m_bytesPerSector;
}


void FAT_SectorCache::MarkSectorDirty( UINT32 sectorIndex )
{
    FAT_CacheLine* cacheLine = GetCacheLine( sectorIndex );

    if(cacheLine)
    {
        cacheLine->SetDirty( TRUE );
    }
}

FAT_SectorCache::FAT_CacheLine* FAT_SectorCache::GetUnusedCacheLine(BOOL useLRU)
{
    FAT_CacheLine* cacheLine;
    FAT_CacheLine* topCandidate = NULL;
    FAT_CacheLine* topReadOnlyCandidate = NULL;
    UINT32 counter;
    UINT32 minLRUCounter = useLRU ? 0x7FFFFFFF : 0;
    UINT32 minLRUReadOnlyCounter = useLRU ? 0x7FFFFFFF : 0;
    
    for(int i = 0; i < SECTORCACHE_MAXSIZE; i++)
    {
        cacheLine = &m_cacheLines[i];

        if(!cacheLine->m_buffer)
        {
            return cacheLine;
        }

        counter = cacheLine->GetLRUCounter();
        
        if(( useLRU && (counter < minLRUCounter)) ||
           (!useLRU && (counter > minLRUCounter)))
        {
            minLRUCounter = counter;
            topCandidate  = cacheLine;

            if(useLRU && !cacheLine->IsReadWrite())
            {
                topReadOnlyCandidate = cacheLine;
                minLRUReadOnlyCounter = counter;
            }   
        }
        else if(useLRU && !cacheLine->IsReadWrite())
        {
            if(counter < minLRUReadOnlyCounter)
            {
                topReadOnlyCandidate = cacheLine;
                minLRUReadOnlyCounter = counter;
            }
        }
    }

    if(topReadOnlyCandidate != NULL && minLRUReadOnlyCounter != m_LRUCounter)
    {
        topCandidate = topReadOnlyCandidate;
    }

    FlushSector( topCandidate );

    return topCandidate;
}


FAT_SectorCache::FAT_CacheLine* FAT_SectorCache::GetCacheLine( UINT32 sectorIndex )
{
    FAT_CacheLine* cacheLine;
    
    for(int i = 0; i < SECTORCACHE_MAXSIZE; i++)
    {
        cacheLine = &m_cacheLines[i];

        if(cacheLine->m_buffer && (sectorIndex >= cacheLine->m_begin) && (sectorIndex < (cacheLine->m_begin + m_sectorsPerLine)))
        {
            return cacheLine;
        }
    }

    return NULL;
}

/////////////////////////////////////////////////////////
// Description:
//  flush content in IOBuffer to real hardware storage
// 
// Input:
//  
//
// output:
//   
//
// Remarks:
// 
// Returns:
void FAT_SectorCache::FlushSector( UINT32 sectorIndex )
{
    FAT_CacheLine* cacheLine = GetCacheLine( sectorIndex );

    if(cacheLine)
    {
        FlushSector( cacheLine );
    }
}

void FAT_SectorCache::FlushSector( FAT_CacheLine* cacheLine, BOOL fClearDirtyBit )
{
    if(cacheLine->m_buffer)
    {
#ifdef FAT_FS__VALIDATE_READONLY_CACHELINE
        UINT32 crc = SUPPORT_ComputeCRC(cacheLine->m_buffer, SECTORCACHE_LINESIZE, 0);
#endif

#ifdef FAT_FS__VALIDATE_READONLY_CACHELINE
        if(cacheLine->IsReadWrite() && cacheLine->m_crc != crc)
#else
        if(cacheLine->IsDirty())
#endif
        {
            m_blockStorageDevice->Write( cacheLine->m_bsByteAddress, SECTORCACHE_LINESIZE, cacheLine->m_buffer, TRUE );

            if(fClearDirtyBit)
            {
                cacheLine->SetDirty( FALSE );
            }

#ifdef FAT_FS__VALIDATE_READONLY_CACHELINE
            cacheLine->m_crc = crc;
#endif
        }
#ifdef FAT_FS__VALIDATE_READONLY_CACHELINE
        else
        {
            ASSERT(cacheLine->m_crc == crc);
        }
#endif
    }
}

void FAT_SectorCache::FlushAll()
{
    for(int i = 0; i < SECTORCACHE_MAXSIZE; i++)
    {
        FlushSector( &m_cacheLines[i], FALSE );
    }
}

/////////////////////////////////////////////////////////
// Description:
//  flush one sector in ram buffer (full of 0) to hardware storage
// 
// Input:
//   sectorIndex
//
// output:
//
// Remarks:
// 
// Returns:
void FAT_SectorCache::EraseSector( UINT32 sectorIndex )
{
    BYTE* sector = GetSector( sectorIndex, TRUE, TRUE );

    if(sector)
    {
        memset( sector, 0, m_bytesPerSector );
    }
}

