////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#include <Tinyhal.h>
#include "SimpleStorage_decl.h"

//--//

BOOL                SimpleStorage::s_IsInitialized  = FALSE;
BlockStorageStream* SimpleStorage::s_pCurrentStream = NULL;
BlockStorageStream  SimpleStorage::s_BsStreamA;
BlockStorageStream  SimpleStorage::s_BsStreamB;

//--//

BOOL SimpleStorage::Initialize()
{
    BOOL retVal = s_IsInitialized;
    
    if(!s_IsInitialized)
    {
        if(s_BsStreamA.Initialize(BlockUsage::SIMPLE_A) && s_BsStreamB.Initialize(BlockUsage::SIMPLE_B))
        {
            UINT32 activeBlockMarker = 0;
            
            if(!s_BsStreamA.ReadIntoBuffer((UINT8*)&activeBlockMarker, sizeof(activeBlockMarker))) return FALSE;

            if(activeBlockMarker != SIMPLESTORAGE_ACTIVE_BLOCK_MARKER)
            {
                if(!s_BsStreamB.ReadIntoBuffer((UINT8*)&activeBlockMarker, sizeof(activeBlockMarker))) return FALSE;

                if(activeBlockMarker == SIMPLESTORAGE_ACTIVE_BLOCK_MARKER)
                {
                    s_pCurrentStream = &s_BsStreamB; 
                }
                else
                {
                    activeBlockMarker = SIMPLESTORAGE_ACTIVE_BLOCK_MARKER;
                    
                    s_BsStreamA.Seek(0, BlockStorageStream::SeekBegin);
                    s_BsStreamA.Erase(s_BsStreamA.Length);
                    s_BsStreamA.Seek(0, BlockStorageStream::SeekBegin);
                    s_BsStreamA.Write((UINT8*)&activeBlockMarker, sizeof(activeBlockMarker));

                    s_pCurrentStream = &s_BsStreamA;
                }
            }         
            else
            {
                s_pCurrentStream = &s_BsStreamA;
            }
            
            retVal = TRUE;
            s_IsInitialized = TRUE;
        }
    }

    return retVal;
}

BOOL SimpleStorage::ReadToNextFile(SIMPLESTORAGE_FILE_HEADER& header)
{
    UINT32 crc = 0;

    while(s_pCurrentStream->CurrentIndex + sizeof(header) < s_pCurrentStream->Length)
    {
        if(!s_pCurrentStream->ReadIntoBuffer((UINT8*)&header, sizeof(header))) return FALSE;

        if(header.Signature == SIMPLESTORAGE_FILE_SIGNATURE_UNINIT)
        {
            s_pCurrentStream->Seek(-((INT32)sizeof(header)), BlockStorageStream::SeekCurrent);
            return TRUE;
        }

        crc = SUPPORT_ComputeCRC(&header, sizeof(header) - sizeof(header.HeaderCRC) - sizeof(header.Attrib), 0);

        if(header.HeaderCRC != crc) 
        {
            UINT32 sig = 0;

            while(s_pCurrentStream->CurrentIndex + sizeof(header) < s_pCurrentStream->Length)
            {
                if(!s_pCurrentStream->ReadIntoBuffer((UINT8*)&sig, sizeof(sig)))
                {
                    return FALSE;
                }

                if(sig == SIMPLESTORAGE_FILE_SIGNATURE) 
                {
                    s_pCurrentStream->Seek(-4, BlockStorageStream::SeekCurrent);
                    break;
                }
            }

            continue;
        }

        if(header.Attrib == SIMPLESTORAGE_DELETED_ATTRIB) s_pCurrentStream->Seek(ROUNDTOMULTIPLE(header.Length, 4), BlockStorageStream::SeekCurrent);
        else return TRUE;
    }

    return FALSE;
}

BOOL SimpleStorage::Compact()
{
    GLOBAL_LOCK(irq);
    BlockStorageStream* pFreeBlock = (s_pCurrentStream == &s_BsStreamA) ? &s_BsStreamB : &s_BsStreamA;
    SIMPLESTORAGE_FILE_HEADER header;
    UINT8 buffer[256];
    const UINT32 eraseBlock = 0x0;
    const UINT32 activeBlock = SIMPLESTORAGE_ACTIVE_BLOCK_MARKER;

    pFreeBlock->Seek(0, BlockStorageStream::SeekBegin);
    if(!pFreeBlock->Erase(pFreeBlock->Length)) return FALSE;

    pFreeBlock->Seek(sizeof(SIMPLESTORAGE_BLOCK_HEADER), BlockStorageStream::SeekBegin);

    s_pCurrentStream->Seek(sizeof(SIMPLESTORAGE_BLOCK_HEADER), BlockStorageStream::SeekBegin);

    do
    {
        INT32 len;
        
        if(!ReadToNextFile(header)) break;

        if(header.Signature != SIMPLESTORAGE_FILE_SIGNATURE) break;

        pFreeBlock->Write((UINT8*)&header, sizeof(header));

        len = header.Length;
        while(len > 0)
        {
            int bufLen = len < ARRAYSIZE(buffer) ? len : ARRAYSIZE(buffer);
            
            if(!s_pCurrentStream->ReadIntoBuffer((UINT8*)buffer, bufLen)) break;
            if(!pFreeBlock->Write((UINT8*)buffer, bufLen)) break;

            len -= bufLen;
        }
    }
    while(s_pCurrentStream->CurrentIndex + sizeof(header) < s_pCurrentStream->Length);

    s_pCurrentStream->Seek(0, BlockStorageStream::SeekBegin);

    s_pCurrentStream->Write((UINT8*)&eraseBlock, sizeof(eraseBlock));

    pFreeBlock->Seek(0, BlockStorageStream::SeekBegin);

    pFreeBlock->Write((UINT8*)&activeBlock, sizeof(activeBlock)); 

    s_pCurrentStream = pFreeBlock;

    return TRUE;
}

BOOL SimpleStorage::SeekToFile(LPCSTR fileName, LPCSTR groupName, SIMPLESTORAGE_FILE_HEADER& header, BOOL createNew)
{
    s_pCurrentStream->Seek(sizeof(SIMPLESTORAGE_BLOCK_HEADER), BlockStorageStream::SeekBegin);

    for(;;)
    {
        if(!ReadToNextFile(header)) return FALSE;

        if(header.Signature == SIMPLESTORAGE_FILE_SIGNATURE_UNINIT)
        {
            if(createNew)
            {
                return TRUE;
            }
            else
            {
                return FALSE;
            }
        }
        else if(hal_strncmp_s(fileName , header.FileName, ARRAYSIZE(header.FileName)) == 0 && 
                hal_strncmp_s(groupName, header.Group   , ARRAYSIZE(header.Group   )) == 0)
        {
            return createNew ? FALSE : TRUE;
        }

        s_pCurrentStream->Seek(ROUNDTOMULTIPLE(header.Length, 4), BlockStorageStream::SeekCurrent);
    }
	
	return FALSE;
}

BOOL SimpleStorage::Create( LPCSTR fileName, LPCSTR groupName, UINT32 fileType, UINT8* data, UINT32 dataLength )
{
    BOOL retVal = FALSE;
    SIMPLESTORAGE_FILE_HEADER header;
    INT32 available;

    if(fileName == NULL) return FALSE;
    if(!Initialize()) return FALSE;

    if(!SeekToFile(fileName, groupName, header, TRUE))
    {
        Compact();

        if(!SeekToFile(fileName, groupName, header, TRUE)) return FALSE;
    }

    available = s_pCurrentStream->Length - s_pCurrentStream->CurrentIndex;

    if(available < (INT32)dataLength)
    {
        Compact();

        if(!SeekToFile(fileName, groupName, header, TRUE)) return FALSE;
    }

    header.Signature = SIMPLESTORAGE_FILE_SIGNATURE;
    hal_strcpy_s(header.FileName, ARRAYSIZE(header.FileName), fileName);
    if(groupName == NULL)
    {
        header.Group[0] = 0;
    }
    else
    {
        hal_strcpy_s(header.Group, ARRAYSIZE(header.Group), groupName);
    }
    header.FileType = fileType;
    header.Length = dataLength;
    header.Attrib = 0xFFFFFFFF; // uninitialized flash
    header.HeaderCRC = SUPPORT_ComputeCRC(&header, sizeof(header) - sizeof(header.HeaderCRC) - sizeof(header.Attrib), 0);

    if(s_pCurrentStream->Write((UINT8*)&header, sizeof(header)))
    {
        retVal = s_pCurrentStream->Write((UINT8*)data, dataLength);
    }

    return retVal;
}

BOOL SimpleStorage::Read   ( LPCSTR fileName, LPCSTR groupName, UINT32& fileType, UINT8* data, UINT32& dataLength )
{
    SIMPLESTORAGE_FILE_HEADER header;
    
    if(!Initialize()) return FALSE;

    if(!SeekToFile(fileName, groupName, header, FALSE)) return FALSE;

    fileType = header.FileType;

    if(data != NULL)
    {
        if(dataLength > header.Length)
        {
            dataLength = header.Length;
        }

        if(!s_pCurrentStream->ReadIntoBuffer((UINT8*)data, dataLength)) return FALSE;
    }
    else 
    {
        dataLength = header.Length;

        s_pCurrentStream->Seek(-((INT32)sizeof(header)), BlockStorageStream::SeekCurrent);
    }

    return TRUE;
}


BOOL SimpleStorage::GetFileEnum( LPCSTR groupName, UINT32 fileType , FileEnumCtx& enumCtx )
{
    if(groupName == NULL) return FALSE;

    enumCtx.Offset    = sizeof(SIMPLESTORAGE_BLOCK_HEADER);
    enumCtx.GroupName = groupName;
    enumCtx.FileType  = fileType;

    return TRUE;
}

BOOL SimpleStorage::GetNextFile( FileEnumCtx& enumCtx, CHAR* fileName, UINT32 fileNameLen )
{
    BOOL fRes = FALSE;
    SIMPLESTORAGE_FILE_HEADER header;

    if(!Initialize()   ) return FALSE;
    if(fileName == NULL) return FALSE;

    s_pCurrentStream->Seek(enumCtx.Offset, BlockStorageStream::SeekBegin);

    while(TRUE)
    {
        if(!ReadToNextFile(header) ||
           (header.Signature == SIMPLESTORAGE_FILE_SIGNATURE_UNINIT)) 
        {
           break;
        }

        if(enumCtx.FileType == header.FileType && (0 == hal_strncmp_s(enumCtx.GroupName, header.Group, ARRAYSIZE(header.Group))))
        {
            hal_strcpy_s(fileName, fileNameLen, header.FileName);
            fRes = TRUE;
            break;
        }

        s_pCurrentStream->Seek(ROUNDTOMULTIPLE(header.Length, 4), BlockStorageStream::SeekCurrent);
    }

    enumCtx.Offset = s_pCurrentStream->CurrentIndex + ROUNDTOMULTIPLE(header.Length, 4);

    return fRes;
}

BOOL SimpleStorage::Delete ( LPCSTR fileName, LPCSTR groupName )
{
    SIMPLESTORAGE_FILE_HEADER header;

    if(!Initialize()) return FALSE;

    if(!SeekToFile(fileName, groupName, header, FALSE)) return FALSE;

    header.Attrib = SIMPLESTORAGE_DELETED_ATTRIB;

    s_pCurrentStream->Seek(-(INT32)sizeof(header), BlockStorageStream::SeekCurrent);

    return s_pCurrentStream->Write((UINT8*)&header, sizeof(header));
}

