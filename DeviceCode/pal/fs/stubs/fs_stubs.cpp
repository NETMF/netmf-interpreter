////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#include <tinyhal.h>
#include <tinyhal.h>

//--//

void FS_MountVolume( LPCSTR nameSpace, UINT32 serialNumber, UINT32 deviceFlags, BlockStorageDevice* blockStorageDevice )
{
}

void FS_UnmountVolume( BlockStorageDevice* blockStorageDevice )
{
}

//--//

void FS_Initialize()
{
}

//--//

HAL_DblLinkedList<FileSystemVolume> FileSystemVolumeList::s_volumeList;

//--//

void FileSystemVolumeList::Initialize()
{
}

BOOL FileSystemVolumeList::InitializeVolumes()
{
    return TRUE;
}

BOOL FileSystemVolumeList::UninitializeVolumes()
{
    return TRUE;
}

BOOL FileSystemVolumeList::AddVolume( FileSystemVolume* fsv, LPCSTR nameSpace, UINT32 serialNumber, UINT32 deviceFlags,
                                      STREAM_DRIVER_INTERFACE* streamDriver, FILESYSTEM_DRIVER_INTERFACE* fsDriver,
                                      BlockStorageDevice* blockStorageDevice, UINT32 volumeId, BOOL init )
{
    return TRUE;
}

BOOL FileSystemVolumeList::RemoveVolume( FileSystemVolume* fsv, BOOL uninit )
{
    return TRUE;
}

FileSystemVolume* FileSystemVolumeList::GetFirstVolume()
{ 
    return NULL;
}

FileSystemVolume* FileSystemVolumeList::GetNextVolume( FileSystemVolume& volume )
{ 
    return NULL;
}

UINT32 FileSystemVolumeList::GetNumVolumes()
{
    return 0;
}

FileSystemVolume* FileSystemVolumeList::FindVolume( LPCSTR nameSpace, UINT32 nameSpaceLength )
{
    return NULL;
}

BOOL FileSystemVolumeList::Contains( FileSystemVolume* fsv )
{
    return FALSE;
}
