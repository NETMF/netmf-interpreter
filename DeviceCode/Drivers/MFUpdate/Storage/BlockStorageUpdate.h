////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <blockstorage_decl.h>
#include <mfupdate_decl.h>

struct BlockStorageUpdate
{
    static const UINT32 c_FileSignature = 0xD06F00D;
    
    struct UpdateBlockHeader
    {
        UINT32 fileSignature;
        MFUpdateHeader storageHeader;
    };

    struct UpdateFile
    {
        UINT32 StartAddress;
        UINT32 Size;
        UINT32 ID;
        UINT16 Type;
        UINT16 SubType;
    };

    struct FreeListItem
    {
        UpdateFile File;
        struct FreeListItem* Next;
    };
    
    BOOL InitializeFiles( UINT32 blockTypes );

    static BOOL   Initialize();
    static INT32  Create   ( MFUpdateHeader& storageHeader, UINT32 flags                       );
    static INT32  Open     ( INT32  storageID    , UINT16 storageType, UINT16 storageSubType    );
    static void   Close    ( INT32  handleStorage                                              );
    static BOOL   Delete   ( INT32  storageID    , UINT16 storageType, UINT16 storageSubType    );
    static BOOL   GetFiles ( UINT16 storageType  , INT32* storageIDs , INT32* storageCount     );
    static BOOL   IsErased ( INT32  handleStorage, INT32 fileOffset  , INT32  len              );
    static INT32  Write    ( INT32  handleStorage, INT32 fileOffset  , UINT8* pData, INT32 len );
    static INT32  Read     ( INT32  handleStorage, INT32 fileOffset  , UINT8* pData, INT32 len );    
    static BOOL   GetHeader( INT32  handleStorage, MFUpdateHeader* pHeader                     );
    static UINT32 GetEraseSize( INT32  handleStorage                                           );

private:
    static void BlockStorageUpdate_RebootHandler();
    INT32 GetFreeHandle();

    BOOL m_initialized;
    BlockStorageStream m_stream;
    FreeListItem *m_pFreeList;

    struct UpdateFile  m_files[10];
    struct FreeListItem m_freeList[10];    
};

//extern BlockStorageUpdate g_BlockStorageUpdate;

extern IUpdateStorageProvider g_BlockStorageUpdateProvider;


