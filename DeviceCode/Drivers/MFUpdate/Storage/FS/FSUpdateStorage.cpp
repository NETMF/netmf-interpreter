////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "FSUpdateStorage.h"

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_FSUpdateStorage"
#endif

FSUpdateStorage g_FSUpdateStorage;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

void FSUpdateStorage::FSUpdateStorage_RebootHandler()
{
    g_FSUpdateStorage.m_initialized = FALSE;
}

IUpdateStorageProvider g_FSUpdateStorageProvider = 
{
    FSUpdateStorage::Initialize,
    FSUpdateStorage::Create,
    FSUpdateStorage::Open,
    FSUpdateStorage::Close,
    FSUpdateStorage::Delete,
    FSUpdateStorage::GetFiles,
    FSUpdateStorage::IsErased,
    FSUpdateStorage::Write,
    FSUpdateStorage::Read,
    FSUpdateStorage::GetHeader,
    FSUpdateStorage::GetEraseSize,
};

static int FS_LWSTR_COPY(WCHAR* dest, int count, WCHAR* src)
{
    int i;
    
    if(dest == NULL || src == NULL) return 0;
    
    for(i=0; src[i] != 0 && i<count; i++)
    {
        dest[i] = src[i];
    }

    return i;
}

static int FS_LWSTR_LEN(WCHAR* txt)
{
    int i=0;
    
    if(txt == NULL) return 0;

    while(txt[i] != 0)
    {
        i++;
    }

    return i;
}

void FSUpdateStorage::InitializeVolumeRoot(FileSystemVolume* pVolume, FS_FILEINFO& findData)
{
    FSUpdateHeader header;
    UINT32 searchHandle = 0;
    
    if(SUCCEEDED(pVolume->FindOpen(L"\\", &searchHandle)))
    {
        BOOL found = FALSE;
    
        if(g_FSUpdateStorage.m_defaultVolume == NULL)
        {
            g_FSUpdateStorage.m_defaultVolume = pVolume;
        }
        
        while(SUCCEEDED(pVolume->FindNext(searchHandle, &findData, &found)) && found)
        {
            int len = FS_LWSTR_LEN((WCHAR*)findData.FileName);
    
            if(len >= 4 && findData.FileName[len-4] == L'.' && findData.FileName[len-3] == L'N' && findData.FileName[len-2] == L'M' && findData.FileName[len-1] == L'F')
            {
                UINT32 fileHandle = 0;
                WCHAR path[128];
                int idx = 0;
                
                path[idx++] = L'\\';
                
                idx += FS_LWSTR_COPY(&path[1], ARRAYSIZE(path)-1, (WCHAR*)findData.FileName);

                path[idx] = 0;
                    
                if(SUCCEEDED(pVolume->Open((LPCWSTR)path, &fileHandle)))
                {
                    int bytesRead = 0;
                    bool fDeleteFile = false;

                    if(SUCCEEDED(pVolume->Read(fileHandle, (UINT8*)&header, sizeof(header), &bytesRead)))
                    {
                        if(header.fileSignature == c_FileSignature)
                        {
                            INT32 handle = GetFreeHandle();
                    
                            int idx = FS_LWSTR_COPY(g_FSUpdateStorage.m_files[handle].FilePath, ARRAYSIZE(g_FSUpdateStorage.m_files[handle].FilePath), (WCHAR*)path);

                            g_FSUpdateStorage.m_files[handle].FilePath[idx] = 0;
                            
                            g_FSUpdateStorage.m_files[handle].Volume     = pVolume;
                            g_FSUpdateStorage.m_files[handle].Size       = header.storageHeader.UpdateSize + sizeof(header);
                            g_FSUpdateStorage.m_files[handle].ID         = header.storageHeader.UpdateID;
                            g_FSUpdateStorage.m_files[handle].Type       = header.storageHeader.UpdateType;
                            g_FSUpdateStorage.m_files[handle].SubType    = header.storageHeader.UpdateSubType;
                            g_FSUpdateStorage.m_files[handle].Handle     = fileHandle;
                        }
                        else
                        {
                            fDeleteFile = true;
                        }
                    }
    
                    if(fDeleteFile) 
                    {
                        pVolume->Close(fileHandle);
                        pVolume->Delete((LPCWSTR)path);
                    }
                }
            }
        }
    
        pVolume->FindClose(searchHandle);
    }
    
}

BOOL FSUpdateStorage::Initialize()
{
    if(FileSystemVolumeList::GetFirstVolume() == NULL)
    {
        FS_Initialize();
        
        FileSystemVolumeList::Initialize();
        
        FS_AddVolumes();
        
        return FileSystemVolumeList::InitializeVolumes();
    }

    return TRUE;
}

BOOL FSUpdateStorage::InitializeFiles()
{
    FileSystemVolume* pVolume;

    if(g_FSUpdateStorage.m_initialized) return TRUE;

    HAL_AddSoftRebootHandler(FSUpdateStorage_RebootHandler);

    memset(g_FSUpdateStorage.m_files, 0, sizeof(g_FSUpdateStorage.m_files ));

    if(!Initialize()) return FALSE;

    pVolume = FileSystemVolumeList::GetFirstVolume();

    if(pVolume == NULL) return FALSE;

    do
    {
        WCHAR fileNameBuf[c_MaxFileLength];
        FS_FILEINFO findData;
        BOOL initVolume = FALSE;
        BOOL fOK = TRUE;

        fileNameBuf[0] = 0;

        if(pVolume->m_fsDriver == NULL || pVolume->m_fsDriver->Name == NULL)
        {
            initVolume = TRUE;
        }
        else
        {
            INT64 size=0, free;

            if(FAILED(pVolume->GetSizeInfo( &size, &free )) || size <= 0)
            {
                initVolume = TRUE;
            }
        }

        if(initVolume)
        {
            if(fOK = SUCCEEDED(pVolume->Format("", 0)))
            {
                fOK = pVolume->InitializeVolume();
            }
        }

        if(fOK)
        {
            memset(&findData, 0, sizeof(findData));
            
            findData.FileName = (UINT16*)fileNameBuf;
            findData.FileNameSize = sizeof(fileNameBuf);
                
            InitializeVolumeRoot( pVolume, findData );
        }
    }
    while(NULL != (pVolume = FileSystemVolumeList::GetNextVolume(*pVolume)));

    g_FSUpdateStorage.m_initialized = TRUE;

    return TRUE;
}

INT32 FSUpdateStorage::GetFreeHandle()
{
    INT32 newHandle = -1;

    for(int idx = 0; idx < ARRAYSIZE(g_FSUpdateStorage.m_files); idx++)
    {
        if(g_FSUpdateStorage.m_files[idx].Size == 0)
        {
            newHandle = idx;
            break;
        }
    }

    return newHandle;
}

static const WCHAR s_HexTable[] = 
{
    L'0', L'1', L'2', L'3', L'4', L'5', L'6', L'7', L'8', L'9', L'A', L'B', L'C', L'D', L'E', L'F'
};

static int FS_ITW(UINT32 value, WCHAR* str, int strLen)
{
    int idx = strLen - 1;

    do
    {
        int num = value % 16;
        
        str[idx--] = s_HexTable[num];
        
        value = value/16;
    }
    while(value > 0 && idx >= 0);

    if(idx > 0)
    {
        idx++;
        memmove( str, &str[idx], (strLen - idx)*sizeof(WCHAR) ); 
    }

    return strLen - idx;
}

static bool CreateFileName(INT32 updateID, UINT16 updateType, WCHAR* fileName, int fileNameCount)
{
    int idx = 0;

    fileName[idx++] = L'\\';
    fileName[idx++] = L'T';

    idx += FS_ITW(updateType, &fileName[idx], fileNameCount - idx);

    if(idx >= fileNameCount) goto ADD_EXTENSION;

    fileName[idx++] = L'U';

    idx += FS_ITW(updateID, &fileName[idx], fileNameCount - idx);

ADD_EXTENSION:
    if(idx + 5 > fileNameCount)
    {
        idx = (fileNameCount - 5);
    }

    fileName[idx++] = L'.';
    fileName[idx++] = L'N';
    fileName[idx++] = L'M';
    fileName[idx++] = L'F';
    fileName[idx++] = 0;    

    return true;
}

INT32 FSUpdateStorage::Create( MFUpdateHeader& storageHeader, UINT32 flags )
{
    INT32 newHandle = -1, i, cntWrite;
    FSUpdateHeader header;
    MFUpdateHeader hdr;
    FSUpdateFile* pFile;
    INT64 position = 0;
    header.fileSignature = c_FileSignature;

    if(!g_FSUpdateStorage.m_initialized) g_FSUpdateStorage.InitializeFiles();

    INT32 updateSizeTotal = storageHeader.UpdateSize + sizeof(header);

    // If we are creating a new deployment we should erase any other of the same type 
    Delete( MFUPDATE_UPDATEID_ANY, storageHeader.UpdateType, MFUPDATE_UPDATESUBTYPE_ANY );

    for(i=0; i<ARRAYSIZE(g_FSUpdateStorage.m_files); i++)
    {
        if( g_FSUpdateStorage.m_files[i].Type    == storageHeader.UpdateType    && 
            g_FSUpdateStorage.m_files[i].SubType == storageHeader.UpdateSubType && 
            g_FSUpdateStorage.m_files[i].ID      == storageHeader.UpdateID      && 
            g_FSUpdateStorage.m_files[i].Size    != 0 )
        {
            return -1;
        }
    }
    
    memcpy(&header.storageHeader, &storageHeader, sizeof(header.storageHeader));

    newHandle = g_FSUpdateStorage.GetFreeHandle();

    if(newHandle == -1) return -1;

    pFile = &g_FSUpdateStorage.m_files[newHandle];

    if(!CreateFileName(storageHeader.UpdateID, storageHeader.UpdateType, pFile->FilePath, sizeof(pFile->FilePath)))
    {
        return -1;
    }

    if(FAILED(g_FSUpdateStorage.m_defaultVolume->Open((LPCWSTR)pFile->FilePath, &pFile->Handle)))
    {
        return -1;
    }

    pFile->Volume       = g_FSUpdateStorage.m_defaultVolume;
    pFile->ID           = storageHeader.UpdateID;
    pFile->Type         = storageHeader.UpdateType;
    pFile->SubType      = storageHeader.UpdateSubType;
    pFile->Size         = updateSizeTotal;

    pFile->Volume->SetLength(pFile->Handle, updateSizeTotal);

    if(FAILED(pFile->Volume->Seek(pFile->Handle, 0, SEEKORIGIN_BEGIN, &position)))
    {
        return -1;
    }

    if(FAILED(pFile->Volume->Write(pFile->Handle, (BYTE*)&header, sizeof(header), &cntWrite) || cntWrite == 0))
    {
        pFile->Volume->Close (pFile->Handle);
        pFile->Volume->Delete((LPCWSTR)pFile->FilePath);

        memset(pFile, 0, sizeof(*pFile));

        return -1;
    }
    
    pFile->Volume->Flush(pFile->Handle);
    
    return newHandle;
}
INT32 FSUpdateStorage::Open( INT32 storageID, UINT16 storageType, UINT16 storageSubType)
{
    INT32 i;
    struct FSUpdateHeader header;

    memset(&header, 0, sizeof(header));

    if(!g_FSUpdateStorage.m_initialized) g_FSUpdateStorage.InitializeFiles();

    for(i=0; i<ARRAYSIZE(g_FSUpdateStorage.m_files); i++)
    {
        if( (MFUPDATE_UPDATEID_ANY      == storageID      || g_FSUpdateStorage.m_files[i].ID      == storageID     ) && 
            (MFUPDATE_UPDATETYPE_ANY    == storageType    || g_FSUpdateStorage.m_files[i].Type    == storageType   ) &&
            (MFUPDATE_UPDATESUBTYPE_ANY == storageSubType || g_FSUpdateStorage.m_files[i].SubType == storageSubType) &&
            g_FSUpdateStorage.m_files[i].Size > 0)
        {
            
            return i;
        }
    }

    return -1;
}

void FSUpdateStorage::Close( INT32 handleStorage )
{
    if(handleStorage < 0 || handleStorage >= ARRAYSIZE(g_FSUpdateStorage.m_files) ) return;

    g_FSUpdateStorage.m_files[handleStorage].Volume->Flush(g_FSUpdateStorage.m_files[handleStorage].Handle);
    g_FSUpdateStorage.m_files[handleStorage].Volume->Close(g_FSUpdateStorage.m_files[handleStorage].Handle);
}

BOOL FSUpdateStorage::Delete( INT32 storageID, UINT16 storageType, UINT16 storageSubType )
{
    for(int i=0; i<ARRAYSIZE(g_FSUpdateStorage.m_files); i++)
    {
        if( (MFUPDATE_UPDATEID_ANY      == storageID   || g_FSUpdateStorage.m_files[i].ID      == storageID     ) && 
            (MFUPDATE_UPDATETYPE_ANY    == storageType || g_FSUpdateStorage.m_files[i].Type    == storageType   ) &&
            (MFUPDATE_UPDATESUBTYPE_ANY == storageType || g_FSUpdateStorage.m_files[i].SubType == storageSubType) &&
            g_FSUpdateStorage.m_files[i].Size > 0)
        {
            g_FSUpdateStorage.m_files[i].Volume->Close ( g_FSUpdateStorage.m_files[i].Handle   );
            g_FSUpdateStorage.m_files[i].Volume->Delete( g_FSUpdateStorage.m_files[i].FilePath );

            memset(&g_FSUpdateStorage.m_files[i], 0, sizeof(g_FSUpdateStorage.m_files[i]));
        }
    }

    return FALSE;
}
BOOL FSUpdateStorage::GetFiles( UINT16 storageType, INT32* storageIDs, INT32* storageCount )
{
    if(storageCount == NULL) return FALSE;
    if(storageIDs == NULL) *storageCount = 0;

    INT32 idx = 0;

    if(!g_FSUpdateStorage.m_initialized) g_FSUpdateStorage.InitializeFiles();
        
    for(int i=0; i<ARRAYSIZE(g_FSUpdateStorage.m_files); i++)
    {
        if((MFUPDATE_UPDATETYPE_ANY == storageType || g_FSUpdateStorage.m_files[i].Type == storageType) && 
            g_FSUpdateStorage.m_files[i].Size > 0)
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
BOOL FSUpdateStorage::IsErased ( INT32  handleStorage, INT32 fileOffset, INT32  len )
{
    if(handleStorage < 0 || handleStorage >= ARRAYSIZE(g_FSUpdateStorage.m_files)) return FALSE;

    return TRUE;
}
INT32 FSUpdateStorage::Write( INT32  handleStorage, INT32 fileOffset, UINT8* pData, INT32 len )
{
    INT32 bytesWritten;
    FSUpdateFile *pFile;
    INT64 position;

    fileOffset += sizeof(FSUpdateHeader);
    
    if(handleStorage < 0 || handleStorage >= ARRAYSIZE(g_FSUpdateStorage.m_files)) return 0;

    pFile = &g_FSUpdateStorage.m_files[handleStorage];

    if(FAILED(pFile->Volume->Seek(pFile->Handle, fileOffset, SEEKORIGIN_BEGIN, &position)))
    {
        return 0;
    }

    return SUCCEEDED(pFile->Volume->Write(pFile->Handle, pData, len, &bytesWritten)) ? bytesWritten : 0;
}

INT32 FSUpdateStorage::Read( INT32  handleStorage, INT32 fileOffset, UINT8* pData, INT32 len )
{
    INT32 bytesRead;
    INT64 position;
    FSUpdateFile *pFile;

    fileOffset += sizeof(FSUpdateHeader);
    
    if(handleStorage < 0 || handleStorage >= ARRAYSIZE(g_FSUpdateStorage.m_files)) return 0;

    pFile = &g_FSUpdateStorage.m_files[handleStorage];

    if(FAILED(pFile->Volume->Seek(pFile->Handle, fileOffset, SEEKORIGIN_BEGIN, &position)))
    {
        return 0;
    }

    return SUCCEEDED(pFile->Volume->Read(pFile->Handle, pData, len, &bytesRead)) ? bytesRead : 0;
}

BOOL FSUpdateStorage::GetHeader( INT32 handleStorage, MFUpdateHeader* pHeader )
{
    FSUpdateHeader hdr;
    INT64 position;
    INT32 bytesRead;
    BOOL bRet;
    FSUpdateFile *pFile;
    
    if(handleStorage < 0 || handleStorage >= ARRAYSIZE(g_FSUpdateStorage.m_files)) return FALSE;

    pFile = &g_FSUpdateStorage.m_files[handleStorage];

    if(FAILED(pFile->Volume->Seek(pFile->Handle, 0, SEEKORIGIN_BEGIN, &position)))
    {
        return 0;
    }

    bRet = SUCCEEDED(pFile->Volume->Read(pFile->Handle, (BYTE*)&hdr, sizeof(hdr), &bytesRead));

    memcpy(pHeader, &hdr.storageHeader, sizeof(hdr.storageHeader));

    return bRet;
}

UINT32 FSUpdateStorage::GetEraseSize( INT32 handleStorage )
{
    return 512;
}

