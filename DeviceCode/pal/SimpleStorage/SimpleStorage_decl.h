////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <TinyHal.h>
#include <PKCS11\CryptokiPAL.h>


#ifndef _DRIVERS_SIMPLESTORAGE_DECL_H_
#define _DRIVERS_SIMPLESTORAGE_DECL_H_ 1

//--//

#define SIMPLESTORAGE_FILE_SIGNATURE 0x600DD06
#define SIMPLESTORAGE_MAX_FILENAME  19
#define SIMPLESTORAGE_ACTIVE_BLOCK_MARKER 0x123ABC
#define SIMPLESTORAGE_DELETED_ATTRIB 0
#define SIMPLESTORAGE_FILE_SIGNATURE_UNINIT 0xFFFFFFFF

struct SIMPLESTORAGE_BLOCK_HEADER
{
    UINT32 ActiveBLock;
};

struct SIMPLESTORAGE_FILE_HEADER
{
public:
    UINT32   Signature;
    CHAR     FileName[20];
    CHAR     Group[20];
    UINT32   FileType;
    UINT32   Length;
    UINT32   HeaderCRC;
    UINT32   Attrib;
};

//--//

struct SimpleStorage
{
public:
    static BOOL Create     ( LPCSTR       fileName , LPCSTR groupName, UINT32 type , UINT8* data, UINT32  dataLength );
    static BOOL Read       ( LPCSTR       fileName , LPCSTR groupName, UINT32& type, UINT8* data, UINT32& dataLength );
    static BOOL Delete     ( LPCSTR       fileName , LPCSTR groupName );
    static BOOL GetFileEnum( LPCSTR       groupName, UINT32 fileType, FileEnumCtx& enumCtx     );
    static BOOL GetNextFile( FileEnumCtx& enumCtx  , CHAR*  fileName, UINT32       fileNameLen );

private:
    static BOOL Initialize();
    static BOOL Compact();
    static BOOL ReadToNextFile( SIMPLESTORAGE_FILE_HEADER& header );
    static BOOL SeekToFile( LPCSTR fileName, LPCSTR groupName, SIMPLESTORAGE_FILE_HEADER& header, BOOL createNew );
    
    static BOOL s_IsInitialized;
    static BlockStorageStream* s_pCurrentStream;
    static BlockStorageStream s_BsStreamA;
    static BlockStorageStream s_BsStreamB;
};

#endif //_DRIVERS_SIMPLESTORAGE_DECL_H_

