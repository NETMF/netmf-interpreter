////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "BlockStorageUpdate.h"

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_BlockStorageUpdate"
#endif

BlockStorageUpdate g_BlockStorageUpdate;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

void BlockStorageUpdate::BlockStorageUpdate_RebootHandler()
{
    g_BlockStorageUpdate.m_initialized = FALSE;
}

IUpdateStorageProvider g_BlockStorageUpdateProvider = 
{
    BlockStorageUpdate::Initialize,
    BlockStorageUpdate::Create,
    BlockStorageUpdate::Open,
    BlockStorageUpdate::Close,
    BlockStorageUpdate::Delete,
    BlockStorageUpdate::GetFiles,
    BlockStorageUpdate::IsErased,
    BlockStorageUpdate::Write,
    BlockStorageUpdate::Read,
    BlockStorageUpdate::GetHeader,
    BlockStorageUpdate::GetEraseSize,
};

BOOL BlockStorageUpdate::Initialize()
{
    if(NULL == BlockStorageList::GetFirstDevice())
    {
        BlockStorageList::Initialize();

        BlockStorage_AddDevices();

        BlockStorageList::InitializeDevices();
    }    

    return TRUE;
}

BOOL BlockStorageUpdate::InitializeFiles( UINT32 blockTypes )
{
    struct UpdateBlockHeader header;
    int freeListIdx = 0;

    if(g_BlockStorageUpdate.m_initialized) return FALSE;

    HAL_AddSoftRebootHandler(BlockStorageUpdate_RebootHandler);
    
    memset(g_BlockStorageUpdate.m_files   , 0, sizeof(g_BlockStorageUpdate.m_files   ));
    memset(g_BlockStorageUpdate.m_freeList, 0, sizeof(g_BlockStorageUpdate.m_freeList));

    g_BlockStorageUpdate.m_pFreeList = g_BlockStorageUpdate.m_freeList;

    if(!g_BlockStorageUpdate.m_stream.Initialize(blockTypes)) return FALSE;

    do
    {
        if(!g_BlockStorageUpdate.m_stream.IsErased(g_BlockStorageUpdate.m_stream.BlockLength))
        {
            if(g_BlockStorageUpdate.m_stream.ReadIntoBuffer((UINT8*)&header, sizeof(header)))
            {
                if(header.fileSignature == c_FileSignature)
                {
                    INT32 handle = GetFreeHandle();

                    g_BlockStorageUpdate.m_files[handle].StartAddress = g_BlockStorageUpdate.m_stream.CurrentIndex - sizeof(header);
                    g_BlockStorageUpdate.m_files[handle].Size         = header.storageHeader.UpdateSize + sizeof(header);
                    g_BlockStorageUpdate.m_files[handle].ID           = header.storageHeader.UpdateID;
                    g_BlockStorageUpdate.m_files[handle].Type         = header.storageHeader.UpdateType;
                    g_BlockStorageUpdate.m_files[handle].SubType      = header.storageHeader.UpdateSubType;

                    g_BlockStorageUpdate.m_stream.Seek(g_BlockStorageUpdate.m_files[handle].Size, BlockStorageStream::SeekCurrent);
                }
            }
        }
        else if(freeListIdx < ARRAYSIZE(g_BlockStorageUpdate.m_freeList))
        {
            int size = g_BlockStorageUpdate.m_freeList[freeListIdx].File.Size;
            
            if(size == 0)
            {
                g_BlockStorageUpdate.m_freeList[freeListIdx].File.StartAddress = g_BlockStorageUpdate.m_stream.CurrentIndex;
                g_BlockStorageUpdate.m_freeList[freeListIdx].File.Size         = g_BlockStorageUpdate.m_stream.BlockLength;
            }
            else if(g_BlockStorageUpdate.m_freeList[freeListIdx].File.StartAddress + size == g_BlockStorageUpdate.m_stream.CurrentIndex)
            {
                g_BlockStorageUpdate.m_freeList[freeListIdx].File.Size += g_BlockStorageUpdate.m_stream.BlockLength;
            }
            else
            {
                g_BlockStorageUpdate.m_freeList[freeListIdx].Next              = &g_BlockStorageUpdate.m_freeList[++freeListIdx];
                g_BlockStorageUpdate.m_freeList[freeListIdx].File.StartAddress = g_BlockStorageUpdate.m_stream.CurrentIndex;
                g_BlockStorageUpdate.m_freeList[freeListIdx].File.Size         = g_BlockStorageUpdate.m_stream.BlockLength;
            }
        }
    }
    while(g_BlockStorageUpdate.m_stream.Seek(BlockStorageStream::STREAM_SEEK_NEXT_BLOCK, BlockStorageStream::SeekCurrent));

    g_BlockStorageUpdate.m_initialized = TRUE;

    return TRUE;
}

INT32 BlockStorageUpdate::GetFreeHandle()
{
    INT32 newHandle = -1;

    for(int idx = 0; idx < ARRAYSIZE(g_BlockStorageUpdate.m_files); idx++)
    {
        if(g_BlockStorageUpdate.m_files[idx].Size == 0)
        {
            newHandle = idx;
            break;
        }
    }

    return newHandle;
}

INT32 BlockStorageUpdate::Create( MFUpdateHeader& storageHeader, UINT32 flags )
{
    INT32 newHandle = -1, i;
    UpdateBlockHeader header;
    header.fileSignature = c_FileSignature;
    FreeListItem* pCur, *pLast;
    INT32 mod;

    if(!g_BlockStorageUpdate.m_initialized) g_BlockStorageUpdate.InitializeFiles(BlockUsage::UPDATE);

    INT32 updateSizeTotal = storageHeader.UpdateSize + sizeof(header);

    // make sure we are on block boundaries for erases
    mod = (updateSizeTotal % g_BlockStorageUpdate.m_stream.BlockLength);

    if(mod != 0)
    {
        updateSizeTotal += g_BlockStorageUpdate.m_stream.BlockLength - mod;
    }

    // If we are creating a new deployment we should erase any other of the same type 
    Delete( MFUPDATE_UPDATEID_ANY, storageHeader.UpdateType, MFUPDATE_UPDATESUBTYPE_ANY );

    for(i=0; i<ARRAYSIZE(g_BlockStorageUpdate.m_files); i++)
    {
        if( g_BlockStorageUpdate.m_files[i].Type    == storageHeader.UpdateType    && 
            g_BlockStorageUpdate.m_files[i].SubType == storageHeader.UpdateSubType && 
            g_BlockStorageUpdate.m_files[i].ID      == storageHeader.UpdateID      && 
            g_BlockStorageUpdate.m_files[i].Size    != 0 )
        {
            return -1;
        }
    }
    
    memcpy(&header.storageHeader, &storageHeader, sizeof(header.storageHeader));

    newHandle = g_BlockStorageUpdate.GetFreeHandle();

    if(newHandle == -1) return -1;

    pCur = g_BlockStorageUpdate.m_pFreeList;
    pLast = pCur;
    
    while(pCur)
    {
        if(pCur->File.Size >= updateSizeTotal)
        {
            g_BlockStorageUpdate.m_files[newHandle].ID           = storageHeader.UpdateID;
            g_BlockStorageUpdate.m_files[newHandle].Type         = storageHeader.UpdateType;
            g_BlockStorageUpdate.m_files[newHandle].SubType      = storageHeader.UpdateSubType;
            g_BlockStorageUpdate.m_files[newHandle].StartAddress = pCur->File.StartAddress;
            g_BlockStorageUpdate.m_files[newHandle].Size         = updateSizeTotal;

            if(pCur->File.Size > updateSizeTotal)
            {
                pCur->File.StartAddress += updateSizeTotal;
                pCur->File.Size         -= updateSizeTotal;
            }
            else if(pCur == g_BlockStorageUpdate.m_pFreeList)
            {
                g_BlockStorageUpdate.m_pFreeList = pCur->Next;
            }
            else
            {
                pLast->Next = pCur->Next;
            }
            
            break;
        }

        pLast = pCur;
        pCur  = pCur->Next;
    }

    if(pCur == NULL) return -1;

    g_BlockStorageUpdate.m_stream.Device->Write(g_BlockStorageUpdate.m_files[newHandle].StartAddress + g_BlockStorageUpdate.m_stream.BaseAddress, sizeof(header), (UINT8*)&header, FALSE);

    return newHandle;
}
INT32 BlockStorageUpdate::Open( INT32 storageID, UINT16 storageType, UINT16 storageSubType)
{
    INT32 i;
    struct UpdateBlockHeader header;

    memset(&header, 0, sizeof(header));

    if(!g_BlockStorageUpdate.m_initialized) g_BlockStorageUpdate.InitializeFiles(BlockUsage::UPDATE);

    for(i=0; i<ARRAYSIZE(g_BlockStorageUpdate.m_files); i++)
    {
        if( (MFUPDATE_UPDATEID_ANY      == storageID      || g_BlockStorageUpdate.m_files[i].ID      == storageID     ) && 
            (MFUPDATE_UPDATETYPE_ANY    == storageType    || g_BlockStorageUpdate.m_files[i].Type    == storageType   ) &&
            (MFUPDATE_UPDATESUBTYPE_ANY == storageSubType || g_BlockStorageUpdate.m_files[i].SubType == storageSubType) &&
            g_BlockStorageUpdate.m_files[i].Size > 0)
        {
            
            return i;
        }
    }

    return -1;
}

void  BlockStorageUpdate::Close     ( INT32 handleStorage )
{
}

BOOL BlockStorageUpdate::Delete( INT32 storageID, UINT16 storageType, UINT16 storageSubType )
{
    for(int i=0; i<ARRAYSIZE(g_BlockStorageUpdate.m_files); i++)
    {
        if( (MFUPDATE_UPDATEID_ANY      == storageID   || g_BlockStorageUpdate.m_files[i].ID      == storageID     ) && 
            (MFUPDATE_UPDATETYPE_ANY    == storageType || g_BlockStorageUpdate.m_files[i].Type    == storageType   ) &&
            (MFUPDATE_UPDATESUBTYPE_ANY == storageType || g_BlockStorageUpdate.m_files[i].SubType == storageSubType) &&
            g_BlockStorageUpdate.m_files[i].Size > 0)
        {
            FreeListItem* pCur  = g_BlockStorageUpdate.m_pFreeList;
            FreeListItem* pLast = pCur;

            g_BlockStorageUpdate.m_stream.Seek(g_BlockStorageUpdate.m_files[i].StartAddress, BlockStorageStream::SeekBegin);
            g_BlockStorageUpdate.m_stream.Erase(g_BlockStorageUpdate.m_files[i].Size);

            while(pCur)
            {
                int size = g_BlockStorageUpdate.m_files[i].Size;
                int start = g_BlockStorageUpdate.m_files[i].StartAddress;
                
                if(pCur->File.StartAddress > size)
                {
                    if(size + start == pCur->File.StartAddress)
                    {
                        pCur->File.StartAddress -= size;
                        pCur->File.Size         += size;
                    }
                    else
                    {
                        for(int j=0; j<ARRAYSIZE(g_BlockStorageUpdate.m_freeList); j++)
                        {
                            if(g_BlockStorageUpdate.m_freeList[j].File.Size == 0)
                            {
                                FreeListItem *pItem = &g_BlockStorageUpdate.m_freeList[j];

                                pItem->File.StartAddress = start;
                                pItem->File.Size         = size;

                                pItem->Next = pCur;

                                if(g_BlockStorageUpdate.m_pFreeList == pCur)
                                {
                                    g_BlockStorageUpdate.m_pFreeList = pItem;
                                }
                                else
                                {
                                    pLast->Next = pItem;
                                }
                            }
                        }
                    }
                    break;
                }
                pLast = pCur;
                pCur  = pCur->Next;
            }
            
            g_BlockStorageUpdate.m_files[i].Size = 0;
            
            return TRUE;
        }
    }

    return FALSE;
}
BOOL BlockStorageUpdate::GetFiles( UINT16 storageType, INT32* storageIDs, INT32* storageCount )
{
    if(storageCount == NULL) return FALSE;
    if(storageIDs == NULL) *storageCount = 0;

    INT32 idx = 0;

    if(!g_BlockStorageUpdate.m_initialized) g_BlockStorageUpdate.InitializeFiles(BlockUsage::UPDATE);
        
    for(int i=0; i<ARRAYSIZE(g_BlockStorageUpdate.m_files); i++)
    {
        if((MFUPDATE_UPDATETYPE_ANY == storageType || g_BlockStorageUpdate.m_files[i].Type == storageType) && 
            g_BlockStorageUpdate.m_files[i].Size > 0)
        {
            if(storageIDs == NULL)
            {
                *storageCount++;
            }
            else
            {
                if(idx < *storageCount)
                {
                    storageIDs[idx++] = i;
                }
                else
                {
                    break;
                }
            }
        }
    }

    return TRUE;
}
BOOL BlockStorageUpdate::IsErased ( INT32  handleStorage, INT32 fileOffset, INT32  len )
{
    if(handleStorage < 0 || handleStorage >= ARRAYSIZE(g_BlockStorageUpdate.m_files) || g_BlockStorageUpdate.m_files[handleStorage].Size <= 0) return FALSE;

    g_BlockStorageUpdate.m_stream.Seek( g_BlockStorageUpdate.m_files[handleStorage].StartAddress + sizeof(UpdateBlockHeader) + fileOffset, BlockStorageStream::SeekBegin);
        
    return g_BlockStorageUpdate.m_stream.IsErased(len);
}
INT32 BlockStorageUpdate::Write( INT32  handleStorage, INT32 fileOffset, UINT8* pData, INT32 len )
{
    if(handleStorage < 0 || handleStorage >= ARRAYSIZE(g_BlockStorageUpdate.m_files) || g_BlockStorageUpdate.m_files[handleStorage].Size <= 0) return FALSE;

    g_BlockStorageUpdate.m_stream.Seek( g_BlockStorageUpdate.m_files[handleStorage].StartAddress + sizeof(UpdateBlockHeader) + fileOffset, BlockStorageStream::SeekBegin );
        
    return g_BlockStorageUpdate.m_stream.Write(pData, len) ? len : -1;
}
INT32 BlockStorageUpdate::Read( INT32  handleStorage, INT32 fileOffset, UINT8* pData, INT32 len )
{
    if(handleStorage < 0 || handleStorage >= ARRAYSIZE(g_BlockStorageUpdate.m_files) || g_BlockStorageUpdate.m_files[handleStorage].Size <= 0) return FALSE;

    g_BlockStorageUpdate.m_stream.Seek( g_BlockStorageUpdate.m_files[handleStorage].StartAddress + sizeof(UpdateBlockHeader) + fileOffset, BlockStorageStream::SeekBegin );
        
    return g_BlockStorageUpdate.m_stream.ReadIntoBuffer(pData, len) ? len : -1;
}

BOOL BlockStorageUpdate::GetHeader( INT32 handleStorage, MFUpdateHeader* pHeader )
{
    UpdateBlockHeader hdr;
    
    if(handleStorage < 0 || handleStorage >= ARRAYSIZE(g_BlockStorageUpdate.m_files) || g_BlockStorageUpdate.m_files[handleStorage].Size <= 0) return FALSE;

    g_BlockStorageUpdate.m_stream.Seek( g_BlockStorageUpdate.m_files[handleStorage].StartAddress, BlockStorageStream::SeekBegin );
        
    if(!g_BlockStorageUpdate.m_stream.ReadIntoBuffer((UINT8*)&hdr, sizeof(hdr))) return FALSE;

    memcpy(pHeader, &hdr.storageHeader, sizeof(hdr.storageHeader));

    return TRUE;
}

UINT32 BlockStorageUpdate::GetEraseSize( INT32 handleStorage )
{
    if(handleStorage < 0 || handleStorage >= ARRAYSIZE(g_BlockStorageUpdate.m_files) || g_BlockStorageUpdate.m_files[handleStorage].Size <= 0) return FALSE;

    return g_BlockStorageUpdate.m_stream.BlockLength;
}

