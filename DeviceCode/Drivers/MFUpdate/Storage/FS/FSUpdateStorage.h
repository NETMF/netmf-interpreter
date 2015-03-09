////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <FS_decl.h>
#include <mfupdate_decl.h>

struct FSUpdateStorage
{
    static const UINT32 c_FileSignature = 0xD06F00D;
    static const UINT32 c_MaxFileLength = 64;
    
    struct FSUpdateHeader
    {
        UINT32 fileSignature;
        MFUpdateHeader storageHeader;
    };

    struct FSUpdateFile
    {
        WCHAR  FilePath[c_MaxFileLength];
        UINT32 Handle;
        FileSystemVolume *Volume;
        UINT32 Size;
        UINT32 ID;
        UINT16 Type;
        UINT16 SubType;
    };

    BOOL InitializeFiles();

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
    static UINT32 GetEraseSize( INT32  handleStorage );

private:
    static void InitializeVolumeRoot(FileSystemVolume* pVolume, FS_FILEINFO& findData);
    static void FSUpdateStorage_RebootHandler();
    static INT32 GetFreeHandle();

    BOOL m_initialized;
    FileSystemVolume *m_defaultVolume;

    struct FSUpdateFile  m_files[10];
};

extern IUpdateStorageProvider g_FSUpdateStorageProvider;


