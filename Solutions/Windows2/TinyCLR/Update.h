#include "stdafx.h"
#include <mfupdate_decl.h>

using namespace Microsoft::SPOT::Emulator::Update;

#ifndef _WINDOWS2_UPDATE_H_
#define _WINDOWS2_UPDATE_H_ 1


struct Windows_UpdateStorageProvider
{
    static BOOL   Initialize ( );
    static INT32  Create   ( MFUpdateHeader& header, UINT32 flags );
    static INT32  Open     ( INT32  storageID    , UINT16 storageType, UINT16 storageSubType            );
    static void   Close    ( INT32  handleStorage                                             );
    static BOOL   Delete   ( INT32  storageID    , UINT16 storageType, UINT16 storageSubType );
    static BOOL   GetFiles ( UINT16 storageType  , INT32* storageIDs, INT32* storageCount     );
    static BOOL   IsErased ( INT32  handleStorage, INT32 fileOffset , INT32  len              );
    static INT32  Write    ( INT32  handleStorage, INT32 fileOffset , UINT8* pData, INT32 len );
    static INT32  Read     ( INT32  handleStorage, INT32 fileOffset , UINT8* pData, INT32 len );
    static BOOL   GetHeader( INT32  handleStorage, MFUpdateHeader* pHeader                    );
};

struct Windows_BackupProvider
{
    static BOOL CreateBackup ( UINT16 updateType, UINT16 updateSubType );
    static BOOL RestoreBackup( UINT16 updateType, UINT16 updateSubType );
    static BOOL DeleteBackup ( UINT16 updateType, UINT16 updateSubType );
};

struct Windows_UpdateProvider
{
    static BOOL   InitializeUpdate( MFUpdate* pUpdate                                                                                 );
    static BOOL   GetProperty     ( MFUpdate* pUpdate, LPCSTR szPropName, UINT8* pPropValue, INT32* pPropValueSize                    );
    static BOOL   SetProperty     ( MFUpdate* pUpdate, LPCSTR szPropName, UINT8* pPropValue, INT32 pPropValueSize                     );
    static BOOL   InstallUpdate   ( MFUpdate* pUpdate, UINT8* pValidationData, INT32 validationLen                                    );
};

struct Windows_ValidationProvider
{
    static BOOL AuthCommand   ( MFUpdate* pUpdate, UINT32 cmd        , UINT8* pArgs        , INT32  argsLen    , UINT8* pResponse   , INT32& responseLen );
    static BOOL Authenticate  ( MFUpdate* pUpdate, UINT8* pAuth      , INT32  authLen                                                                    );    
    static BOOL ValidatePacket  ( MFUpdate* pUpdate, UINT8* packetData, INT32 packetLen, UINT8* validationData, INT32 validationLen );
    static BOOL ValidateUpdate  ( MFUpdate* pUpdate, UINT8* pValidationData, INT32 validationLen                                    );
};

#endif //_WINDOWS2_UPDATE_H_

