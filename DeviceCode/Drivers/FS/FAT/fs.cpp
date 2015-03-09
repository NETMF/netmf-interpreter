////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "FAT_FS.h"
#include "TinyCLR_ErrorCodes.h"


////////////////////////////////////////////////////////////////////////////////////////////////////
//
// FAT_FS_Driver
//
////////////////////////////////////////////////////////////////////////////////////////////////////

BOOL FAT_FS_Driver::IsLoadableMedia( BlockStorageDevice* driverInterface, UINT32* numVolumes )
{
    return FAT_LogicDisk::IsLoadableMedia( driverInterface, numVolumes );
}

STREAM_DRIVER_DETAILS *FAT_FS_Driver::DriverDetails( const VOLUME_ID* volume )
{
    static STREAM_DRIVER_DETAILS driverDetail = 
    {
        SYSTEM_BUFFERED_IO, NULL, NULL, 0, 0, TRUE, TRUE, TRUE, 0, 0
    };

    return &driverDetail;
}

//--//

void FAT_FS_Driver::Initialize()
{
    FAT_MemoryManager::Initialize();
}

BOOL FAT_FS_Driver::InitializeVolume( const VOLUME_ID* volume )
{
    return (FAT_LogicDisk::Initialize( volume )) ? TRUE : FALSE;
}

BOOL FAT_FS_Driver::UnInitializeVolume( const VOLUME_ID* volume )
{
    FAT_LogicDisk* logicDisk = FAT_MemoryManager::GetLogicDisk( volume );

    if(logicDisk)
    {
        return logicDisk->Uninitialize();
    }

    return TRUE;
}

HRESULT FAT_FS_Driver::Format( const VOLUME_ID* volume, LPCSTR volumeLabel, UINT32 parameters )
{
    ::Watchdog_GetSetEnabled( FALSE, TRUE );

    HRESULT hr = FAT_LogicDisk::Format( volume, volumeLabel, parameters );

    ::Watchdog_GetSetEnabled( TRUE, TRUE );

    return hr;
}

HRESULT FAT_FS_Driver::GetSizeInfo( const VOLUME_ID* volume, INT64* totalSize, INT64* totalFreeSpace )
{
    FAT_LogicDisk* logicDisk = FAT_MemoryManager::GetLogicDisk( volume );

    if(logicDisk)
    {
        *totalSize      = (INT64)logicDisk->GetDiskTotalSize();
        *totalFreeSpace = (INT64)logicDisk->GetDiskFreeSize();

        return S_OK;
    }

    return CLR_E_INVALID_DRIVER;
}

HRESULT FAT_FS_Driver::FlushAll( const VOLUME_ID* volume )
{
    FAT_LogicDisk* logicDisk = FAT_MemoryManager::GetLogicDisk( volume );

    if(logicDisk)
    {
        logicDisk->SectorCache.FlushAll();
        
        return S_OK;
    }

    return CLR_E_INVALID_DRIVER;
}

HRESULT FAT_FS_Driver::GetVolumeLabel( const VOLUME_ID* volume, LPSTR volumeLabel, INT32 volumeLabelLen )
{
    FAT_LogicDisk* logicDisk = FAT_MemoryManager::GetLogicDisk( volume );

    if(logicDisk)
    {
        if(volumeLabelLen < FAT_Directory::DIR_Name__size)
        {
            return CLR_E_BUFFER_TOO_SMALL;
        }

        return logicDisk->GetDiskVolLab( volumeLabel );
    }

    return CLR_E_INVALID_DRIVER;
}

//--//

HRESULT FAT_FS_Driver::Open( const VOLUME_ID *volume, LPCWSTR path, UINT32 *handle )
{
    FAT_LogicDisk* logicDisk = FAT_MemoryManager::GetLogicDisk( volume );

    if(logicDisk)
    {
        return logicDisk->Open( path, handle );
    }

    return CLR_E_INVALID_DRIVER;
}

HRESULT FAT_FS_Driver::Close( UINT32 handle )
{
    if(handle == 0)
        return CLR_E_INVALID_PARAMETER;

    FAT_FileHandle* fileHandle = (FAT_FileHandle*)handle;

    return fileHandle->Close();
}

HRESULT FAT_FS_Driver::Read( UINT32 handle, BYTE* buffer, int size, int* bytesRead )
{
    if(handle == 0)
        return CLR_E_INVALID_PARAMETER;
    
    FAT_FileHandle* fileHandle = (FAT_FileHandle*)handle;

    return fileHandle->Read( buffer, size, bytesRead );
}

HRESULT FAT_FS_Driver::Write( UINT32 handle, BYTE* buffer, int size, int* bytesWritten )
{
    if(handle == 0)
        return CLR_E_INVALID_PARAMETER;

    FAT_FileHandle* fileHandle = (FAT_FileHandle*)handle;

    return fileHandle->Write( buffer, size, bytesWritten );
}

HRESULT FAT_FS_Driver::Flush(UINT32 handle)
{
    if(handle == 0)
        return CLR_E_INVALID_PARAMETER;

    FAT_FileHandle* fileHandle = (FAT_FileHandle*)handle;

    return fileHandle->Flush();
}

HRESULT FAT_FS_Driver::Seek( UINT32 handle, INT64 offset, UINT32 origin, INT64* position )
{
    if(handle == 0)
        return CLR_E_INVALID_PARAMETER;

    FAT_FileHandle* fileHandle = (FAT_FileHandle*)handle;

    return fileHandle->Seek( offset, origin, position );
}

HRESULT FAT_FS_Driver::GetLength( UINT32 handle, INT64* length )
{
    if(handle == 0)
        return CLR_E_INVALID_PARAMETER;

    FAT_FileHandle* fileHandle = (FAT_FileHandle*)handle;

    return fileHandle->GetLength( length );
}

HRESULT FAT_FS_Driver::SetLength( UINT32 handle, INT64 length )
{
    if(handle == 0)
        return CLR_E_INVALID_PARAMETER;

    FAT_FileHandle* fileHandle = (FAT_FileHandle*)handle;

    return fileHandle->SetLength( length );
}

//--//

HRESULT FAT_FS_Driver::FindOpen( const VOLUME_ID *volume, LPCWSTR path, UINT32* findHandle )
{
    FAT_LogicDisk* logicDisk = FAT_MemoryManager::GetLogicDisk( volume );

    if(logicDisk)
    {
        return logicDisk->FindOpen( path, findHandle );
    }

    return CLR_E_INVALID_DRIVER;
}

HRESULT FAT_FS_Driver::FindNext( UINT32 handle, FS_FILEINFO *fi, BOOL *fileFound )
{
    if(handle == 0)
        return CLR_E_INVALID_PARAMETER;

    FAT_FINDFILES* fileInfo = (FAT_FINDFILES*)handle;

    return fileInfo->FindNext( fi, fileFound );

}

HRESULT FAT_FS_Driver::FindClose( UINT32 handle )
{
    if(handle == 0)
        return CLR_E_INVALID_PARAMETER;

    FAT_FINDFILES* fileInfo = (FAT_FINDFILES*)handle;

    return fileInfo->FindClose();

}

HRESULT FAT_FS_Driver::GetFileInfo( const VOLUME_ID* volume, LPCWSTR path, FS_FILEINFO* fileInfo, BOOL * found )
{
    FAT_LogicDisk* logicDisk = FAT_MemoryManager::GetLogicDisk( volume );

    if(logicDisk)
    {
        return logicDisk->GetFileInfo( path, fileInfo, found );
    }

    return CLR_E_INVALID_DRIVER;
}

HRESULT FAT_FS_Driver::GetAttributes( const VOLUME_ID* volume, LPCWSTR path, UINT32* attributes )
{
    FAT_LogicDisk* logicDisk = FAT_MemoryManager::GetLogicDisk( volume );

    if(logicDisk)
    {
        return logicDisk->GetAttributes( path, attributes );
    }

    return CLR_E_INVALID_DRIVER;
}

HRESULT FAT_FS_Driver::SetAttributes( const VOLUME_ID* volume, LPCWSTR path, UINT32 attributes )
{
    FAT_LogicDisk* logicDisk = FAT_MemoryManager::GetLogicDisk( volume );

    if(logicDisk)
    {
        return logicDisk->SetAttributes( path, attributes );
    }

    return CLR_E_INVALID_DRIVER;
}

HRESULT FAT_FS_Driver::CreateDirectory( const VOLUME_ID* volume, LPCWSTR path )
{
    FAT_LogicDisk* logicDisk = FAT_MemoryManager::GetLogicDisk( volume );

    if(logicDisk)
    {
        return logicDisk->CreateDirectory( path );
    }

    return CLR_E_INVALID_DRIVER;
}

HRESULT FAT_FS_Driver::Move( const VOLUME_ID* volume, LPCWSTR oldPath, LPCWSTR newPath )
{
    FAT_LogicDisk* logicDisk = FAT_MemoryManager::GetLogicDisk( volume );

    if(logicDisk)
    {
        return logicDisk->Move( oldPath, newPath );
    }

    return CLR_E_INVALID_DRIVER;
}

HRESULT FAT_FS_Driver::Delete( const VOLUME_ID *volume, LPCWSTR path )
{
    FAT_LogicDisk* logicDisk = FAT_MemoryManager::GetLogicDisk( volume );

    if(logicDisk)
    {
        return logicDisk->Delete( path );
    }

    return CLR_E_INVALID_DRIVER;
}
//////////////////

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata = "g_FAT32_FILE_SYSTEM_DriverInterface"
#endif

FILESYSTEM_DRIVER_INTERFACE g_FAT32_FILE_SYSTEM_DriverInterface = 
{    
    &FAT_FS_Driver::FindOpen,
    &FAT_FS_Driver::FindNext,
    &FAT_FS_Driver::FindClose,

    &FAT_FS_Driver::GetFileInfo,

    &FAT_FS_Driver::CreateDirectory,
    &FAT_FS_Driver::Move,
    &FAT_FS_Driver::Delete,

    &FAT_FS_Driver::GetAttributes,
    &FAT_FS_Driver::SetAttributes,    

    &FAT_FS_Driver::Format,
    &FAT_FS_Driver::IsLoadableMedia, 
    &FAT_FS_Driver::GetSizeInfo,
    &FAT_FS_Driver::FlushAll,
    &FAT_FS_Driver::GetVolumeLabel,

    "FAT",
    0,
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata
#endif

//--//

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata = "g_FAT32_STREAM_DriverInterface"
#endif

STREAM_DRIVER_INTERFACE g_FAT32_STREAM_DriverInterface = 
{
    &FAT_FS_Driver::Initialize,
    &FAT_FS_Driver::InitializeVolume,
    &FAT_FS_Driver::UnInitializeVolume,
    &FAT_FS_Driver::DriverDetails,
    &FAT_FS_Driver::Open,
    &FAT_FS_Driver::Close,
    &FAT_FS_Driver::Read,
    &FAT_FS_Driver::Write,
    &FAT_FS_Driver::Flush,
    &FAT_FS_Driver::Seek,
    &FAT_FS_Driver::GetLength,
    &FAT_FS_Driver::SetLength,
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata
#endif

