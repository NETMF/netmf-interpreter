////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//


void FS__STUB_Initialize()
{
}

BOOL FS__STUB_InitializeVolume( const VOLUME_ID* volume )
{
    return FALSE;
}

BOOL FS__STUB_UninitializeVolume( const VOLUME_ID* volume )
{
    return FALSE;
}

STREAM_DRIVER_DETAILS* FS__STUB_DriverDetails( const VOLUME_ID* volume )
{
    return NULL;
}

HRESULT FS__STUB_Open( const VOLUME_ID* volume, LPCWSTR path, UINT32* fileHandle )
{
    return CLR_E_NOT_SUPPORTED;
}

HRESULT FS__STUB_Close( UINT32 handle )
{
    return CLR_E_NOT_SUPPORTED;
}

HRESULT FS__STUB_Read( UINT32 handle, BYTE* buffer, int size, int* bytesRead )
{
    return CLR_E_NOT_SUPPORTED;
}

HRESULT FS__STUB_Write( UINT32 handle, BYTE* buffer, int size, int* bytesWritten )
{
    return CLR_E_NOT_SUPPORTED;
}

HRESULT FS__STUB_Flush( UINT32 handle )
{
    return CLR_E_NOT_SUPPORTED;
}

HRESULT FS__STUB_Seek( UINT32 handle, INT64 offset, UINT32 seekOrigin, INT64* position )
{
    return CLR_E_NOT_SUPPORTED;
}

HRESULT FS__STUB_GetLength( UINT32 handle, INT64* length )
{
    return CLR_E_NOT_SUPPORTED;
}

HRESULT FS__STUB_SetLength( UINT32 handle, INT64 length )
{
    return CLR_E_NOT_SUPPORTED;
}

STREAM_DRIVER_INTERFACE g_FS__STUB_STREAMING_DriverInterface = 
{
    &FS__STUB_Initialize,
    &FS__STUB_InitializeVolume,
    &FS__STUB_UninitializeVolume,
    
    &FS__STUB_DriverDetails,
    &FS__STUB_Open,
    &FS__STUB_Close,

    &FS__STUB_Read,
    &FS__STUB_Write,
    &FS__STUB_Flush,
    &FS__STUB_Seek,
    &FS__STUB_GetLength,
    &FS__STUB_SetLength
};

HRESULT FS__STUB_FindOpen( const VOLUME_ID* volume, LPCWSTR fileSpec, UINT32* handle )
{
    return CLR_E_NOT_SUPPORTED;
}

HRESULT FS__STUB_FindNext( UINT32 handle, FS_FILEINFO* fi, BOOL* fileFound )
{
    return CLR_E_NOT_SUPPORTED;
}

HRESULT FS__STUB_FindClose( UINT32 handle )
{
    return CLR_E_NOT_SUPPORTED;
}

HRESULT FS__STUB_CreateDirectory( const VOLUME_ID* volume, LPCWSTR path)
{
    return CLR_E_NOT_SUPPORTED;
}

HRESULT FS__STUB_GetFileInfo( const VOLUME_ID* /*volume*/, LPCWSTR /*path*/, FS_FILEINFO* /*fileInfo*/, BOOL* /*found*/ )
{
    return CLR_E_NOT_SUPPORTED;
}

HRESULT FS__STUB_Move( const VOLUME_ID* volume, LPCWSTR oldPath, LPCWSTR newPath )
{
    return CLR_E_NOT_SUPPORTED;
}

HRESULT FS__STUB_Delete( const VOLUME_ID* volume, LPCWSTR path )
{
    return CLR_E_NOT_SUPPORTED;
}

HRESULT FS__STUB_GetAttributes( const VOLUME_ID* volume, LPCWSTR path, UINT32* attributes )
{
    return CLR_E_NOT_SUPPORTED;
}

HRESULT FS__STUB_SetAttributes( const VOLUME_ID* volume, LPCWSTR path, UINT32 attributes )
{
    return CLR_E_NOT_SUPPORTED;
}

HRESULT FS__STUB_Format( const VOLUME_ID* volume, LPCSTR volumeLabel, UINT32 parameter )
{
    return CLR_E_NOT_SUPPORTED;
}

BOOL FS__STUB_IsLoadableMedia( BlockStorageDevice* driverInterface, UINT32* numVolumes )
{
    return FALSE;
}

HRESULT FS__STUB_GetSizeInfo( const VOLUME_ID* /*volume*/, INT64* /*totalSize*/, INT64* /*totalFreeSpace*/ )
{
    return CLR_E_NOT_SUPPORTED;
}

HRESULT FS__STUB_FlushAll( const VOLUME_ID* /*volume*/ )
{
    return CLR_E_NOT_SUPPORTED;
}

HRESULT FS__STUB_GetVolumeLabel( const VOLUME_ID* /*volume*/, LPSTR /*volumeLabel*/, INT32 /*volumeLabelLen*/ )
{
    return CLR_E_NOT_SUPPORTED;
}

FILESYSTEM_DRIVER_INTERFACE g_FS__STUB_FILE_SYSTEM_DriverInterface = 
{    
    &FS__STUB_FindOpen,
    &FS__STUB_FindNext,
    &FS__STUB_FindClose,
    
    &FS__STUB_GetFileInfo, /****/
    
    &FS__STUB_CreateDirectory,
    &FS__STUB_Move,
    &FS__STUB_Delete,

    &FS__STUB_GetAttributes,
    &FS__STUB_SetAttributes,    

    &FS__STUB_Format,
    &FS__STUB_IsLoadableMedia,
    &FS__STUB_GetSizeInfo,  /****/
    &FS__STUB_FlushAll,     /****/
    &FS__STUB_GetVolumeLabel,
    
    "FS__STUB",
    0,
};

