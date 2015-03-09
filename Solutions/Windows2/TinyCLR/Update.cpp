////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "update.h"

using namespace Microsoft::SPOT::Emulator::Update;


static IUpdateProvider s_EmulatorUpdateProvider =
{
    Windows_UpdateProvider::InitializeUpdate,
    Windows_UpdateProvider::GetProperty,
    Windows_UpdateProvider::SetProperty,
    Windows_UpdateProvider::InstallUpdate,

    NULL // Extension
};

   
static IUpdateStorageProvider s_Storage = 
{
    Windows_UpdateStorageProvider::Initialize,
    Windows_UpdateStorageProvider::Create,
    Windows_UpdateStorageProvider::Open,
    Windows_UpdateStorageProvider::Close,
    Windows_UpdateStorageProvider::Delete,
    Windows_UpdateStorageProvider::GetFiles,
    Windows_UpdateStorageProvider::IsErased,
    Windows_UpdateStorageProvider::Write,
    Windows_UpdateStorageProvider::Read,
    Windows_UpdateStorageProvider::GetHeader,
    NULL, // GetEraseSize
    NULL, // Extension
};

static IUpdateBackupProvider s_Backup = 
{
    Windows_BackupProvider::CreateBackup,
    Windows_BackupProvider::RestoreBackup,
    Windows_BackupProvider::DeleteBackup,

    NULL // Extension
};

static IUpdateValidationProvider s_Validation =
{
    Windows_ValidationProvider::AuthCommand,
    Windows_ValidationProvider::Authenticate,
    Windows_ValidationProvider::ValidatePacket,
    Windows_ValidationProvider::ValidateUpdate,

    NULL // Extension
};

static const IUpdatePackage s_UpdatePackages[] =
{
    {
        "HTTPSUpdate",
        &s_EmulatorUpdateProvider,
        &s_Validation,
        &s_Storage,
        &s_Backup,

        NULL, // Extension
    }, 
};

const IUpdatePackage* g_UpdatePackages     = s_UpdatePackages;
const INT32           g_UpdatePackageCount = ARRAYSIZE(s_UpdatePackages);

BOOL Windows_UpdateStorageProvider::Initialize()
{
    return TRUE;
}


INT32  Windows_UpdateStorageProvider::Create( MFUpdateHeader& header, UINT32 flags )
{
    MFUpdate_Header managedHeader;

    memcpy(&managedHeader, &header, sizeof(managedHeader)); 
        
    return EmulatorNative::GetIUpdateStorage()->Create( managedHeader );
}
INT32  Windows_UpdateStorageProvider::Open  ( INT32 storageID, UINT16 storageType, UINT16 storageSubType )
{
    return EmulatorNative::GetIUpdateStorage()->Open( storageID, storageType, storageSubType );
}
void Windows_UpdateStorageProvider::Close  ( INT32 handleStorage )
{
    EmulatorNative::GetIUpdateStorage()->Close( handleStorage );
}
BOOL  Windows_UpdateStorageProvider::Delete  ( INT32 storageID, UINT16 storageType, UINT16 storageSubType )
{
    return EmulatorNative::GetIUpdateStorage()->Delete( storageID, storageType, storageSubType );
}
BOOL Windows_UpdateStorageProvider::GetFiles(UINT16 storageType, INT32* storageIDs, INT32* storageCount)
{
    return EmulatorNative::GetIUpdateStorage()->GetFiles( storageType, (IntPtr)storageIDs, (int%)storageCount );
}
BOOL Windows_UpdateStorageProvider::IsErased(INT32 handleStorage, INT32 fileOffset, INT32 len)
{
    return EmulatorNative::GetIUpdateStorage()->IsErased( handleStorage, fileOffset, len );
}
INT32  Windows_UpdateStorageProvider::Write ( INT32 handleStorage, INT32 fileOffset, UINT8* pData, INT32 len )
{
    return EmulatorNative::GetIUpdateStorage()->Write( handleStorage, fileOffset, (IntPtr)pData, len );
}
INT32  Windows_UpdateStorageProvider::Read  ( INT32 handleStorage, INT32 fileOffset, UINT8* pData, INT32 len )
{
    return EmulatorNative::GetIUpdateStorage()->Read( handleStorage, fileOffset, (IntPtr)pData, len );
}
BOOL Windows_UpdateStorageProvider::GetHeader( INT32  handleStorage, MFUpdateHeader* pHeader )
{
    BOOL fRet;
    MFUpdate_Header managedHeader;

    memcpy(&managedHeader, pHeader, sizeof(managedHeader)); 
    
    fRet = EmulatorNative::GetIUpdateStorage()->GetHeader(handleStorage, managedHeader);

    memcpy(pHeader, &managedHeader, sizeof(managedHeader)); 

    return fRet;
}


BOOL   Windows_BackupProvider::CreateBackup(UINT16 updateType, UINT16 updateSubType)
{
    return EmulatorNative::GetIUpdateBackup()->CreateBackup(updateType, updateSubType);    
}
BOOL   Windows_BackupProvider::RestoreBackup(UINT16 updateType, UINT16 updateSubType)
{
    return EmulatorNative::GetIUpdateBackup()->RestoreBackup(updateType, updateSubType);
}
BOOL   Windows_BackupProvider::DeleteBackup(UINT16 updateType, UINT16 updateSubType)
{
    return EmulatorNative::GetIUpdateBackup()->DeleteBackup(updateType, updateSubType);
}


BOOL Windows_UpdateProvider::InitializeUpdate(MFUpdate * pUpdate)
{
    UINT8* pHeader = (UINT8*)&pUpdate->Header;
    BOOL retVal = EmulatorNative::GetIUpdateProvider()->InitializeUpdate((IntPtr)pHeader);

    return retVal;
}

static void Marshal_MFUpdateToMFUpdate_Emu( MFUpdate* pUpdate, MFUpdate_Emu& update)
{
    memcpy(&update.Header, &pUpdate->Header, sizeof(update.Header)); 
    
    update.Flags         = pUpdate->Flags;
    update.StorageHandle = pUpdate->StorageHandle;
}

BOOL Windows_UpdateProvider::GetProperty(MFUpdate* pUpdate, LPCSTR szPropName, UINT8* pPropValue, INT32* pPropValueSize)
{
    MFUpdate_Emu update;

    Marshal_MFUpdateToMFUpdate_Emu(pUpdate, update);
    
    return EmulatorNative::GetIUpdateProvider()->GetProperty(update, gcnew String( szPropName ), (IntPtr)pPropValue, (int%) pPropValueSize);
}
BOOL Windows_UpdateProvider::SetProperty(MFUpdate* pUpdate, LPCSTR szPropName, UINT8* pPropValue, INT32 pPropValueSize)
{
    MFUpdate_Emu update;

    Marshal_MFUpdateToMFUpdate_Emu(pUpdate, update);

    return EmulatorNative::GetIUpdateProvider()->SetProperty(update, gcnew String( szPropName ), (IntPtr)pPropValue, pPropValueSize);
}
BOOL Windows_UpdateProvider::InstallUpdate( MFUpdate* pUpdate, UINT8* pValidationData, INT32 validationLen )
{
    MFUpdate_Emu update;

    Marshal_MFUpdateToMFUpdate_Emu(pUpdate, update);

    return EmulatorNative::GetIUpdateProvider()->InstallUpdate(update, (IntPtr)pValidationData, validationLen);
}

BOOL Windows_ValidationProvider::AuthCommand( MFUpdate* pUpdate, UINT32 cmd, UINT8* pArgs, INT32 argsLen, UINT8* pResponse, INT32& responseLen )
{
    MFUpdate_Emu update;

    Marshal_MFUpdateToMFUpdate_Emu(pUpdate, update);

    return EmulatorNative::GetIUpdateValidation()->AuthCommand(update, cmd, (IntPtr)pArgs, argsLen, (IntPtr)pResponse, (int%)responseLen);
}

BOOL Windows_ValidationProvider::Authenticate( MFUpdate* pUpdate, UINT8* pAuth, INT32  authLen )
{
    MFUpdate_Emu update;

    Marshal_MFUpdateToMFUpdate_Emu(pUpdate, update);

    return EmulatorNative::GetIUpdateValidation()->Authenticate(update, (IntPtr)pAuth, authLen);
}

BOOL Windows_ValidationProvider::ValidatePacket( MFUpdate* pUpdate, UINT8* packetData, INT32 packetLen, UINT8* validationData, INT32 validationLen )
{
    MFUpdate_Emu update;

    Marshal_MFUpdateToMFUpdate_Emu(pUpdate, update);

    return EmulatorNative::GetIUpdateValidation()->ValidatePacket(update, (IntPtr)packetData, packetLen, (IntPtr)validationData, validationLen);
}
BOOL Windows_ValidationProvider::ValidateUpdate( MFUpdate* pUpdate, UINT8* pValidationData, INT32 validationLen )
{
    MFUpdate_Emu update;

    Marshal_MFUpdateToMFUpdate_Emu(pUpdate, update);

    return EmulatorNative::GetIUpdateValidation()->ValidateUpdate(update, (IntPtr)pValidationData, validationLen);
}

