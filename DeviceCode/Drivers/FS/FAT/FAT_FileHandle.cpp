////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"
#include "FAT_FS.h"
#include "FAT_FS_Utility.h"
#include "TinyCLR_Interop.h"

//--//

FAT_FileHandle* FAT_FileHandle::Open( FAT_LogicDisk* logicDisk, FAT_FILE* fileInfo, UINT32 clusIndex )
{
    FAT_FileHandle* fileHandle = (FAT_FileHandle*)FAT_MemoryManager::AllocateHandle();

    if(!fileHandle) return NULL;
    
    fileHandle->m_logicDisk      = logicDisk;
    fileHandle->m_clusIndex      = clusIndex;
    fileHandle->m_sectIndex      = logicDisk->ClusToSect( clusIndex );
    fileHandle->m_dataIndex      = 0;
    fileHandle->m_position       = 0;
    fileHandle->m_readWriteState = ReadWriteState__NONE;

    memcpy( &fileHandle->m_file, fileInfo, sizeof(FAT_FILE) );

    return fileHandle;
}

HRESULT FAT_FileHandle::Close()
{
    Flush();

    FAT_MemoryManager::FreeHandle( this );
    
    return S_OK;
}

HRESULT FAT_FileHandle::ReadWriteSeekHelper( int type, BYTE* buffer, int size, int* bytesDone )
{
    TINYCLR_HEADER();
    
    BYTE* sector;
    BYTE getNextSectFlag = (type == Helper_Write) ? FAT_LogicDisk::GetNextSect__CREATE : FAT_LogicDisk::GetNextSect__NONE;
    int done = 0;
    UINT32 sectorSize = m_logicDisk->m_bytesPerSector;

    while(size > 0)
    {
        if(m_dataIndex == sectorSize) // if we are at the end of a sector, page in the next sector
        {
            TINYCLR_CHECK_HRESULT(m_logicDisk->GetNextSect( &m_clusIndex, &m_sectIndex, getNextSectFlag ));
        
            m_dataIndex = 0;
        }
        
        //get data size can be read in current sector
        size_t count = sectorSize - m_dataIndex;

        // Adjust if we only need a portion of it
        if(size < (INT32)count) count = size;

        if(type == Helper_Flush)
        {
            m_logicDisk->SectorCache.FlushSector( m_sectIndex );
        }
        else if(type != Helper_Seek)
        {
            sector = m_logicDisk->SectorCache.GetSector( m_sectIndex, (type == Helper_Write) ? TRUE : FALSE );

            if(!sector) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);
            
            if(type == Helper_Read)
            {
                memcpy( buffer, &(sector[m_dataIndex]), count );
            }
            else // type == Helper_Write
            {
                if(buffer)
                {
                    memcpy( &(sector[m_dataIndex]), buffer, count );
                }
                else
                {
                    memset( &(sector[m_dataIndex]), 0, count );
                }                
            }

            // adjust counters
            if(buffer) buffer += count;
            /********/ done   += count;
        }

        // adjust counters
        m_dataIndex += count;
        m_position  += count;
        size        -= count;
    }

    if (bytesDone) *bytesDone = done;

    TINYCLR_NOCLEANUP();
}


HRESULT FAT_FileHandle::Read( BYTE* buffer, int size, int* bytesRead )
{
    TINYCLR_HEADER();

    FAT_Directory* dirEntry;
    int limit;
    UINT32 dirFileSize;
    
    if(!buffer || size < 0 || !bytesRead)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    dirEntry = m_file.GetDirectoryEntry( FALSE );

    if(!dirEntry) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);

    // If there's another handle that's modifying the filesize, make sure m_position is valid

    dirFileSize = dirEntry->Get_DIR_FileSize();
    if(m_position > dirFileSize )
    {
        m_position = dirFileSize ;
    }

    limit = (int)(dirFileSize - m_position);

    if(limit == 0)
    {
        // returns -1 to signify the end of the file
        *bytesRead = -1;
        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    //if size to read is larger than limit, set it to limit
    if(size > limit)
    {
        size = limit;
    }

    // set the cluster information if it wasn't previously available
    if(m_clusIndex == 0)
    {
        m_position  = 0;
        m_clusIndex = dirEntry->GetFstClus();
        m_sectIndex = m_logicDisk->ClusToSect( m_clusIndex );
        m_dataIndex = 0;
    }

    TINYCLR_CHECK_HRESULT(ReadWriteSeekHelper( Helper_Read, buffer, size, bytesRead ));

    m_readWriteState |= ReadWriteState__READ;

#ifndef FAT_FS__DO_NOT_UPDATE_FILE_ACCESS_TIME
    UINT16 date,time;
    UINT8 timeTenth;

    FAT_Utility::GetCurrentFATTime( &date, &time, &timeTenth );

    dirEntry = m_file.GetDirectoryEntry(TRUE);
    dirEntry->Set_DIR_LstAccDate(date);
#endif    

    TINYCLR_NOCLEANUP();
}

HRESULT FAT_FileHandle::Write( BYTE* buffer, int size, int* bytesWritten )
{
    TINYCLR_HEADER();

    FAT_Directory* dirEntry;
    
    if(size < 0) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

    dirEntry = m_file.GetDirectoryEntry( FALSE );

    if(!dirEntry) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);

    //haven't allocate any cluster for this file
    if(dirEntry->GetFstClus() == 0)
    {
        UINT32 nextFreeClus = m_logicDisk->GetNextFreeClus( FALSE );

        if(nextFreeClus == CLUST_ERROR) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);
        
        // GetNextFreeClus() might invalidate dirEntry

        dirEntry = m_file.GetDirectoryEntry( TRUE );

        if(!dirEntry) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);

        dirEntry->SetFstClus( nextFreeClus );

        //update FAT table
        m_logicDisk->WriteFAT( nextFreeClus, CLUST_EOF );

        m_position  = 0;
        m_clusIndex = nextFreeClus;
        m_sectIndex = m_logicDisk->ClusToSect( m_clusIndex );
        m_dataIndex = 0;
    }
    else if(m_clusIndex == 0) // set the cluster information if it wasn't previously available
    {
        m_position  = 0;
        m_clusIndex = dirEntry->GetFstClus();
        m_sectIndex = m_logicDisk->ClusToSect( m_clusIndex );
        m_dataIndex = 0;
    }

    TINYCLR_CHECK_HRESULT(ReadWriteSeekHelper( Helper_Write, buffer, size, bytesWritten ));

    // Need to refresh directory entry, as the sector may be paged out by ReadWriteSeekHelper()
    dirEntry = m_file.GetDirectoryEntry( FALSE );

    if(!dirEntry) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);

    //if file is resized
    if(dirEntry->Get_DIR_FileSize() < m_position)
    {
        dirEntry = m_file.GetDirectoryEntry( TRUE );

        if(!dirEntry) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);

        dirEntry->Set_DIR_FileSize((UINT32)m_position);

        // update the access time since we are already updating the size
#ifdef FAT_FS__DO_NOT_UPDATE_FILE_ACCESS_TIME
        UINT16 date,time;

        FAT_Utility::GetCurrentFATTime( &date, &time, NULL );

        dirEntry->Set_DIR_WrtDate ( date );
        dirEntry->Set_DIR_WrtTime ( time );
        dirEntry->Set_DIR_LstAccDate(date);
#endif
    }

    //set file write flag
    m_readWriteState |= ReadWriteState__WRITE;

#ifndef FAT_FS__DO_NOT_UPDATE_FILE_ACCESS_TIME
    UINT16 date,time;

    FAT_Utility::GetCurrentFATTime( &date, &time, NULL );

    dirEntry = m_file.GetDirectoryEntry( TRUE );

    if(!dirEntry) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);
    
    dirEntry->Set_DIR_WrtDate ( date );
    dirEntry->Set_DIR_WrtTime ( time );
    dirEntry->Set_DIR_LstAccDate(date);
#endif
    
    TINYCLR_NOCLEANUP();
}

HRESULT FAT_FileHandle::Flush()
{
    TINYCLR_HEADER();

    if (0 != (m_readWriteState & ReadWriteState__WRITE))
    {
        UINT16         date, time;
        FAT_Directory* dirEntry;

#ifndef FAT_FS__DO_NOT_UPDATE_FILE_ACCESS_TIME
        dirEntry = m_file.GetDirectoryEntry( TRUE );

        if(!dirEntry) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);

        FAT_Utility::GetCurrentFATTime( &date, &time, NULL );
        
        if(dirEntry->Get_DIR_LstAccDate() != date)
        {            
            dirEntry->Set_DIR_LstAccDate( date );
        }
        
        if((m_readWriteState & ReadWriteState__WRITE) == ReadWriteState__WRITE)
        {
            dirEntry->Set_DIR_WrtDate ( date );
            dirEntry->Set_DIR_WrtTime ( time );
        } 
#else 
        dirEntry = m_file.GetDirectoryEntry( FALSE );

        if(!dirEntry) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);

#endif //FAT_FS__DO_NOT_UPDATE_FILE_ACCESS_TIME

        {
            INT64 pos = m_position;
        
            m_clusIndex = dirEntry->GetFstClus();
            m_sectIndex = m_logicDisk->ClusToSect( m_clusIndex );
            m_dataIndex = 0;
            m_position  = 0;

            TINYCLR_CHECK_HRESULT(ReadWriteSeekHelper( Helper_Flush, NULL, dirEntry->Get_DIR_FileSize(), NULL ));    

            Seek( pos, SEEKORIGIN_BEGIN, NULL );
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT FAT_FileHandle::Seek( INT64 offset, UINT32 origin, INT64* position )
{
    TINYCLR_HEADER();
    
    UINT32         dirFileSize;
    FAT_Directory* dirEntry = m_file.GetDirectoryEntry( FALSE );
    
    if(!dirEntry) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);

    dirFileSize = dirEntry->Get_DIR_FileSize();

    // If there's another handle that's modifying the filesize, make sure m_position is valid
    if(m_position > dirFileSize)
    {
        m_position = dirFileSize;
    }
    
    INT64 newPosition;
    switch(origin)
    {
    case SEEKORIGIN_BEGIN:
        newPosition = offset;
        break;
    case SEEKORIGIN_CURRENT:
        newPosition = m_position + offset;
        break;
    case SEEKORIGIN_END:
        newPosition = dirFileSize + offset;
        break;
    default:
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    //if position exceeds the file size, extends the file
    if(newPosition > dirFileSize)
    {
        TINYCLR_CHECK_HRESULT(SetLength( newPosition ));
    }
    else if(newPosition < 0) //if position < 0, return ERROR
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);
    }

    // Get the offset relative to the current position
    offset = newPosition - m_position;

    // If we are going backwards, we have to start from the beginning
    if(offset < 0)
    {
        if (m_dataIndex >= (-1 * offset)) // Unless we're still staying in the same cluster
        {
            m_dataIndex = (UINT32)(m_dataIndex + offset); // offset is < 0
            m_position  += offset; // offset is < 0

            if(position) *position = m_position;
            
            TINYCLR_SET_AND_LEAVE(S_OK);
        }

        // Reload dirEntry after SetLength
        dirEntry = m_file.GetDirectoryEntry( FALSE );

        if(!dirEntry) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);
        
        m_clusIndex = dirEntry->GetFstClus();
        m_sectIndex = m_logicDisk->ClusToSect( m_clusIndex );
        m_dataIndex = 0;
        m_position  = 0;

        offset = newPosition;
    }

    TINYCLR_CHECK_HRESULT(ReadWriteSeekHelper( Helper_Seek, NULL, (int)offset, NULL ));
    
    if(position) *position = m_position;

    TINYCLR_NOCLEANUP();
}

HRESULT FAT_FileHandle::GetLength( INT64* length )
{
    if (!length)
    {
        return CLR_E_INVALID_PARAMETER;
    }
    
    FAT_Directory* dirEntry = m_file.GetDirectoryEntry( FALSE );

    if(!dirEntry) return CLR_E_FILE_IO;
    
    *length = dirEntry->Get_DIR_FileSize();

    return S_OK;
}

HRESULT FAT_FileHandle::SetLength( INT64 length )
{
    TINYCLR_HEADER();

    FAT_Directory* dirEntry;
    UINT32 length32;
    UINT32 startClus;
    UINT32 dirFileSize;

    if(length < 0 || length > 0xFFFFFFFF )
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
    }

    dirEntry = m_file.GetDirectoryEntry( FALSE );

    if(!dirEntry) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);
    
    length32  = (UINT32)length;
    startClus = dirEntry->GetFstClus();

    dirFileSize = dirEntry->Get_DIR_FileSize();

    if((length32 == dirFileSize) ||
       (length32 == 0 && startClus == 0))
    {
        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    // Shrinking
    if(length32 <= dirFileSize)
    {
        UINT32 clusterSize  = m_logicDisk->m_bytesPerSector * m_logicDisk->m_sectorsPerCluster;
        UINT32 oldClusCount = (dirFileSize + clusterSize - 1) / clusterSize;
        UINT32 newClusCount = (length32               + clusterSize - 1) / clusterSize;

        UINT32 clus = startClus;
        UINT32 clusNum = 0;

        //--//
        
        while(clusNum++ < oldClusCount)
        {
            //write cluster end flag
            if(clusNum == newClusCount)
            {
                clus = m_logicDisk->WriteFAT( clus, CLUST_EOF );
            }
            else if(clusNum > newClusCount) //free next cluster
            {
                clus = m_logicDisk->WriteFAT( clus, CLUST_NONE );
            }

            //check data cluster legal
            if(m_logicDisk->GetClusType( clus ) != CLUSTYPE_DATA)
                break;
        }

        if(length <= m_position)
        {
            TINYCLR_CHECK_HRESULT(Seek( length, SEEKORIGIN_BEGIN, NULL ));
        }

        // Update the Directory entry, (need to reload in case WriteFAT page out the sector)
        dirEntry = m_file.GetDirectoryEntry( TRUE );

        if (!dirEntry) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);

        dirEntry->Set_DIR_FileSize((UINT32) length32 );
        
        if(length32 == 0)
        {
            dirEntry->SetFstClus( 0 );
            m_logicDisk->WriteFAT( startClus, CLUST_NONE );
        }
    }
    else //growing
    {
        //return fail if there isn't enough space left
        if(length32 - dirFileSize > m_logicDisk->GetDiskFreeSize())
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);
        }

        INT64 oldPosition = m_position;
        INT64 endPosition;

        TINYCLR_CHECK_HRESULT(Seek( 0, SEEKORIGIN_END, &endPosition ));

        TINYCLR_CHECK_HRESULT(Write( NULL, (int)(length - endPosition), NULL ));

        TINYCLR_CHECK_HRESULT(Seek( oldPosition, SEEKORIGIN_BEGIN, NULL ));
    }

    TINYCLR_NOCLEANUP();
}


