////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include <FS_decl.h>
#include "..\..\..\DeviceCode\Drivers\FS\FAT\FAT_fs.h"

using namespace System;
using namespace System::Runtime::InteropServices;

using namespace Microsoft::SPOT::Emulator;
using namespace Microsoft::SPOT::Emulator::FS;

//--//

STREAM_DRIVER_DETAILS* g_Emulator_FS_DriverDetails;
FileSystemVolume*      g_Emulator_FS_Volumes;
UINT32                 g_Emulator_FS_NumVolumes;

//--//

void WINDOWS_STREAMING_Initialize()
{
}

BOOL WINDOWS_STREAMING_InitializeVolume( const VOLUME_ID* volume )
{
    if(volume->blockStorageDevice != NULL) return FALSE;
    
    return TRUE;
}

BOOL WINDOWS_STREAMING_UninitializeVolume( const VOLUME_ID* volume )
{
    return TRUE;
}

STREAM_DRIVER_DETAILS* WINDOWS_STREAMING_DriverDetails( const VOLUME_ID* volume )
{
    if(volume->volumeId < g_Emulator_FS_NumVolumes)
    {
        return &(g_Emulator_FS_DriverDetails[volume->volumeId]);
    }

    return NULL;
}

HRESULT WINDOWS_STREAMING_Open( const VOLUME_ID* volume, LPCWSTR path, UINT32* fileHandle )
{
    if(volume->blockStorageDevice != NULL) return CLR_E_FAIL;

    return EmulatorNative::GetIFSDriver()->Open( volume->volumeId, gcnew String( path ), (UINT32 %)*fileHandle );
}

HRESULT WINDOWS_STREAMING_Close( UINT32 handle )
{
    return EmulatorNative::GetIFSDriver()->Close( handle );
}

HRESULT WINDOWS_STREAMING_Read( UINT32 handle, BYTE* buffer, int size, int* bytesRead )
{
    return EmulatorNative::GetIFSDriver()->Read( handle, IntPtr( buffer ), size, (int %)*bytesRead );
}

HRESULT WINDOWS_STREAMING_Write( UINT32 handle, BYTE* buffer, int size, int* bytesWritten )
{
    return EmulatorNative::GetIFSDriver()->Write( handle, IntPtr( buffer ), size, (int %)*bytesWritten );
}

HRESULT WINDOWS_STREAMING_Flush( UINT32 handle )
{
    return EmulatorNative::GetIFSDriver()->Flush( handle );
}

HRESULT WINDOWS_STREAMING_Seek( UINT32 handle, INT64 offset, UINT32 seekOrigin, INT64* position )
{
    return EmulatorNative::GetIFSDriver()->Seek( handle, offset, seekOrigin, (INT64 %)*position );
}

HRESULT WINDOWS_STREAMING_GetLength( UINT32 handle, INT64* length )
{
    return EmulatorNative::GetIFSDriver()->GetLength( handle, (INT64 %)*length );
}

HRESULT WINDOWS_STREAMING_SetLength( UINT32 handle, INT64 length )
{
    return EmulatorNative::GetIFSDriver()->SetLength( handle, length );
}

STREAM_DRIVER_INTERFACE g_WINDOWS_STREAMING_DriverInterface = 
{
    &WINDOWS_STREAMING_Initialize,
    &WINDOWS_STREAMING_InitializeVolume,
    &WINDOWS_STREAMING_UninitializeVolume,
    &WINDOWS_STREAMING_DriverDetails,
    &WINDOWS_STREAMING_Open,
    &WINDOWS_STREAMING_Close,
    &WINDOWS_STREAMING_Read,
    &WINDOWS_STREAMING_Write,
    &WINDOWS_STREAMING_Flush,
    &WINDOWS_STREAMING_Seek,
    &WINDOWS_STREAMING_GetLength,
    &WINDOWS_STREAMING_SetLength
};

HRESULT WINDOWS_FILE_SYSTEM_FindOpen( const VOLUME_ID* volume, LPCWSTR path, UINT32* handle )
{
    if(volume->blockStorageDevice != NULL) return CLR_E_FAIL;
    
    return EmulatorNative::GetIFSDriver()->FindOpen( volume->volumeId, gcnew String( path ), (UINT32 %)*handle );
}

HRESULT WINDOWS_FILE_SYSTEM_FindNext( UINT32 handle, FS_FILEINFO* fi, BOOL* fileFound )
{
    FsFileInfo fileInfo;
    bool found = false;
    int hr = EmulatorNative::GetIFSDriver()->FindNext( handle, (FsFileInfo %) fileInfo, (bool %) found );

    if ((hr == 0) && (found))
    {
        *fileFound = TRUE;

        fi->Attributes = fileInfo.Attributes;
        fi->CreationTime = fileInfo.CreationTime;
        fi->LastAccessTime = fileInfo.LastAccessTime;
        fi->LastWriteTime = fileInfo.LastWriteTime;
        fi->Size = fileInfo.Size;

        if(fileInfo.FileName->Length >= (int)fi->FileNameSize)
        {
            return CLR_E_PATH_TOO_LONG;
        }
        
        pin_ptr<const wchar_t> wch = PtrToStringChars( fileInfo.FileName );

        /// ISSUE 07/22/2008-munirula: Remove string copy code below once
        /// something like lstrncpyW is found.

        UINT16* w = (UINT16 *)wch;
        int i = 0;
        for(;;)
        {
            if (i >= fileInfo.FileName->Length || i >= FS_MAX_PATH_LENGTH - 1)
            {
                fi->FileName[i] = 0;
                break;
            }

            fi->FileName[i] = w[i];
            
            if (w[i] == 0)
                break;

            i++;        
        }
    }
    else
    {
        *fileFound = FALSE;
    }

    return hr;
}

HRESULT WINDOWS_FILE_SYSTEM_FindClose( UINT32 handle )
{
    return EmulatorNative::GetIFSDriver()->FindClose( handle );
}

HRESULT WINDOWS_FILE_SYSTEM_GetFileInfo( const VOLUME_ID* volume, LPCWSTR path, FS_FILEINFO* fi, BOOL* fileFound )
{
    FsFileInfo fileInfo;
    bool found = false;
    int hr = EmulatorNative::GetIFSDriver()->GetFileInfo( volume->volumeId, gcnew String( path ), (FsFileInfo %)fileInfo, (bool %)found );

    if ((hr == 0) && (found))
    {
        fi->Attributes = fileInfo.Attributes;
        fi->CreationTime = fileInfo.CreationTime;
        fi->LastAccessTime = fileInfo.LastAccessTime;
        fi->LastWriteTime = fileInfo.LastWriteTime;
        fi->Size = fileInfo.Size;

        *fileFound = TRUE;
    }
    else
    {
        *fileFound = FALSE;
    }

    return hr;
}

HRESULT WINDOWS_FILE_SYSTEM_CreateDirectory( const VOLUME_ID* volume, LPCWSTR path )
{
    if(volume->blockStorageDevice != NULL) return CLR_E_FAIL;
    
    return EmulatorNative::GetIFSDriver()->CreateDirectory( volume->volumeId, gcnew String( path ) );
}

HRESULT WINDOWS_FILE_SYSTEM_Move( const VOLUME_ID* volume, LPCWSTR oldPath, LPCWSTR newPath )
{
    if(volume->blockStorageDevice != NULL) return CLR_E_FAIL;
    
    return EmulatorNative::GetIFSDriver()->Move( volume->volumeId, gcnew String( oldPath ), gcnew String( newPath ) );
}

HRESULT WINDOWS_FILE_SYSTEM_Delete( const VOLUME_ID* volume, LPCWSTR path )
{
    if(volume->blockStorageDevice != NULL) return CLR_E_FAIL;
    
    return EmulatorNative::GetIFSDriver()->Delete( volume->volumeId, gcnew String( path ) );
}

HRESULT WINDOWS_FILE_SYSTEM_GetAttributes( const VOLUME_ID* volume, LPCWSTR path, UINT32* attributes )
{
    if(volume->blockStorageDevice != NULL) return CLR_E_FAIL;
    
    return EmulatorNative::GetIFSDriver()->GetAttributes( volume->volumeId, gcnew String( path ), (UINT32 %)*attributes );
}

HRESULT WINDOWS_FILE_SYSTEM_SetAttributes( const VOLUME_ID* volume, LPCWSTR path, UINT32 attributes )
{
    if(volume->blockStorageDevice != NULL) return CLR_E_FAIL;
    
    return EmulatorNative::GetIFSDriver()->SetAttributes( volume->volumeId, gcnew System::String( path ), attributes );
}

HRESULT WINDOWS_FILE_SYSTEM_Format( const VOLUME_ID* volume, LPCSTR volumeLabel, UINT32 parameter )
{
    if(volume->blockStorageDevice != NULL) return CLR_E_FAIL;
    
    return EmulatorNative::GetIFSDriver()->Format( volume->volumeId, gcnew System::String(volumeLabel), parameter );
}

BOOL WINDOWS_FILE_SYSTEM_IsLoadableMedia( BlockStorageDevice* driverInterface, UINT32* numVolumes )
{
    numVolumes = 0;
    return FALSE;
}

HRESULT WINDOWS_FILE_SYSTEM_GetSizeInfo( const VOLUME_ID* volume, INT64* totalSize, INT64* totalFreeSpace )
{
    return EmulatorNative::GetIFSDriver()->GetSizeInfo( volume->volumeId, (INT64 %)*totalSize, (INT64%)*totalFreeSpace );
}

HRESULT WINDOWS_FILE_SYSTEM_FlushAll( const VOLUME_ID* volume )
{
    return EmulatorNative::GetIFSDriver()->FlushAll( volume->volumeId );
}

HRESULT WINDOWS_FILE_SYSTEM_GetVolumeLabel( const VOLUME_ID* volume, LPSTR volumeLabel, INT32 volumeLabelLen )
{
    return EmulatorNative::GetIFSDriver()->GetVolumeLabel( volume->volumeId, (IntPtr)(char*)volumeLabel, (int)volumeLabelLen );
}

FILESYSTEM_DRIVER_INTERFACE g_WINDOWS_FILE_SYSTEM_DriverInterface = 
{    
    &WINDOWS_FILE_SYSTEM_FindOpen,
    &WINDOWS_FILE_SYSTEM_FindNext,
    &WINDOWS_FILE_SYSTEM_FindClose,

    &WINDOWS_FILE_SYSTEM_GetFileInfo,

    &WINDOWS_FILE_SYSTEM_CreateDirectory,
    &WINDOWS_FILE_SYSTEM_Move,
    &WINDOWS_FILE_SYSTEM_Delete,

    &WINDOWS_FILE_SYSTEM_GetAttributes,
    &WINDOWS_FILE_SYSTEM_SetAttributes,    

    &WINDOWS_FILE_SYSTEM_Format,
    &WINDOWS_FILE_SYSTEM_IsLoadableMedia,
    &WINDOWS_FILE_SYSTEM_GetSizeInfo,
    &WINDOWS_FILE_SYSTEM_FlushAll,
    &WINDOWS_FILE_SYSTEM_GetVolumeLabel,

    "WINFS",
    FS_DRIVER_ATTRIBUTE__FORMAT_REQUIRES_ERASE,
};

//--//

void FS_AddVolumes()
{
    FileSystemVolume* volume;
    STREAM_DRIVER_DETAILS* sdd;
    int numVolumes;
    pin_ptr<const unsigned char> nameSpace;
    FILESYSTEM_DRIVER_INTERFACE* fsDriver;
    STREAM_DRIVER_INTERFACE*     streamDriver;
    BlockStorageDevice*          bsd;
    UINT32                       volumeId;
    
    array<InternalDriverDetails>^ volumes;

    volumes = EmulatorNative::GetIFSDriver()->GetVolumesInfo();

    numVolumes = volumes->Length;

    g_Emulator_FS_Volumes       = new FileSystemVolume     [numVolumes];
    g_Emulator_FS_DriverDetails = new STREAM_DRIVER_DETAILS[numVolumes];
    g_Emulator_FS_NumVolumes    = numVolumes;

    memset( g_Emulator_FS_Volumes, 0, sizeof(FileSystemVolume) * numVolumes );

    for(int i = 0; i < numVolumes; i++)
    {
        if(volumes[i].IsNative)
        {
            fsDriver = NULL;
            
            for(int j = 0; j < (int)g_InstalledFSCount; j++)
            {
                String^ fsName = gcnew String( g_AvailableFSInterfaces[j].fsDriver->Name );
                if(fsName->Equals( volumes[i].FileSystemName ))
                {
                    fsDriver = g_AvailableFSInterfaces[j].fsDriver;
                    streamDriver = g_AvailableFSInterfaces[j].streamDriver;
                }
            }

            if(!fsDriver)
            {
                // throw exception instead?
                continue;
            }

            bsd      = &g_Emulator_BS_Devices[volumes[i].BlockStorageDeviceContext];
            volumeId = volumes[i].VolumeId;
        }
        else
        {
            // Set up the DriverDetails first, because AddVolume need these info for validatation
            sdd = &(g_Emulator_FS_DriverDetails[i]);

            sdd->bufferingStrategy = (FS_BUFFERING_STRATEGY)(int)(volumes[i].BufferingStrategy);
            sdd->canRead           = volumes[i].CanRead;
            sdd->canWrite          = volumes[i].CanWrite;
            sdd->canSeek           = volumes[i].CanSeek;
            sdd->readTimeout       = volumes[i].ReadTimeout;
            sdd->writeTimeout      = volumes[i].WriteTimeout;
            sdd->inputBufferSize   = volumes[i].InputBufferSize;
            sdd->outputBufferSize  = volumes[i].OutputBufferSize;

            if(volumes[i].InputBuffer != nullptr)
            {
                sdd->inputBufferSize = volumes[i].InputBuffer->Length;
                sdd->inputBuffer     = new BYTE[sdd->inputBufferSize];
            }

            if(volumes[i].OutputBuffer != nullptr)
            {
                sdd->outputBufferSize = volumes[i].OutputBuffer->Length;
                sdd->outputBuffer     = new BYTE[sdd->outputBufferSize];
            }

            fsDriver     = &g_WINDOWS_FILE_SYSTEM_DriverInterface;
            streamDriver = &g_WINDOWS_STREAMING_DriverInterface;
            bsd          = NULL;
            volumeId     = i;
        }

        nameSpace = &(volumes[i].Namespace[0]);

        volume = &(g_Emulator_FS_Volumes[i]);

        // The index into the g_Emulator_FS_Volumes will be used as volumeId so the emulator
        // file system driver can differentiate between different volumes
        FileSystemVolumeList::AddVolume( volume, (LPCSTR)nameSpace, volumes[i].SerialNumber, volumes[i].DeviceFlags,
            streamDriver, fsDriver, bsd, volumeId, FALSE );
    }
}

void FS_MountRemovableVolumes()
{
    return EmulatorNative::GetIBlockStorageDriver()->MountInsertedRemovableDevices();
}

//--//

FILESYSTEM_DRIVER_INTERFACE g_EMULATOR_FAT32_FILE_SYSTEM_DriverInterface = 
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
    FS_DRIVER_ATTRIBUTE__FORMAT_REQUIRES_ERASE,
};


extern STREAM_DRIVER_INTERFACE     g_FAT32_STREAM_DriverInterface;

//--//

FILESYSTEM_INTERFACES g_AvailableFSInterfaces[] =
{
    { &g_WINDOWS_FILE_SYSTEM_DriverInterface       , &g_WINDOWS_STREAMING_DriverInterface },
    { &g_EMULATOR_FAT32_FILE_SYSTEM_DriverInterface, &g_FAT32_STREAM_DriverInterface      },
};

const size_t g_InstalledFSCount = ARRAYSIZE(g_AvailableFSInterfaces);
