////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#include <tinyhal.h>
#include <tinyhal.h>

//--//

__weak void FS_MountVolume( LPCSTR nameSpace, UINT32 serialNumber, UINT32 deviceFlags, BlockStorageDevice* blockStorageDevice )
{
}

__weak void FS_UnmountVolume( BlockStorageDevice* blockStorageDevice )
{
}

//--//

__weak void FS_Initialize()
{
}

//--//

__weak HAL_DblLinkedList<FileSystemVolume> FileSystemVolumeList::s_volumeList;

//--//

__weak void FileSystemVolumeList::Initialize()
{
}

__weak BOOL FileSystemVolumeList::InitializeVolumes()
{
    return TRUE;
}

__weak BOOL FileSystemVolumeList::UninitializeVolumes()
{
    return TRUE;
}

__weak BOOL FileSystemVolumeList::AddVolume( FileSystemVolume* fsv, LPCSTR nameSpace, UINT32 serialNumber, UINT32 deviceFlags,
                                      STREAM_DRIVER_INTERFACE* streamDriver, FILESYSTEM_DRIVER_INTERFACE* fsDriver,
                                      BlockStorageDevice* blockStorageDevice, UINT32 volumeId, BOOL init )
{
    return TRUE;
}

__weak BOOL FileSystemVolumeList::RemoveVolume( FileSystemVolume* fsv, BOOL uninit )
{
    return TRUE;
}

__weak FileSystemVolume* FileSystemVolumeList::GetFirstVolume()
{ 
    return NULL;
}

__weak FileSystemVolume* FileSystemVolumeList::GetNextVolume( FileSystemVolume& volume )
{ 
    return NULL;
}

__weak UINT32 FileSystemVolumeList::GetNumVolumes()
{
    return 0;
}

__weak FileSystemVolume* FileSystemVolumeList::FindVolume( LPCSTR nameSpace, UINT32 nameSpaceLength )
{
    return NULL;
}

__weak BOOL FileSystemVolumeList::Contains( FileSystemVolume* fsv )
{
    return FALSE;
}
