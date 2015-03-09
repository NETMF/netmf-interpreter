////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>


// Helper class for FS_MountVolume() to resolve namespace conflicts
struct NextAvailableName
{
    char  nextAvailableName[FS_NAME_MAXLENGTH];
    char* digit;
    int   iteration;

    bool Initialize( LPCSTR originalName )
    {
        if(!originalName) return false;
        
        UINT32 nameLength = hal_strlen_s( originalName );

        if(nameLength == 0 || nameLength >= FS_NAME_MAXLENGTH)
        {
            return false;
        }

        hal_strncpy_s( nextAvailableName, FS_NAME_MAXLENGTH, originalName, nameLength );

        iteration = 0;

        return true;
    }

    // GenerateNextAvailableName will generate the next possible namespace
    // when the previous namespace is already taken. It does so by tagging
    // "_n"  to the end of the namespace where n is a number from 1 to 9
    // If the resulting namespace is longer than FS_NAME_DEFAULT_LENGTH, 
    // the namespace will be truncated to accomodate the _n.
    //
    // The result is in nextAvailableName field. The return value is true if it's successful.
    //
    // For example, if the namespace is originally "FLASH"
    // each subsequent call to GenerateNextAvailableName will generate the following:
    // "FLASH", "FLASH_1", "FLASH_2", "FLASH_3" .... "FLASH_9"
    //
    // Note that first time this is called, nextAvailableName is the originalName
    // given in Initialize()
    bool GenerateNextAvailableName()
    {
        if(iteration == 0) // the first time around, we use the original name
        {
            iteration++;
            return true;
        }
        else if (iteration == 1)
        {
            UINT32 digitPos = hal_strlen_s( nextAvailableName ) + 1;
            
            if(digitPos >= FS_NAME_DEFAULT_LENGTH)
            { // if suffix is too long, trim the namespace
                digitPos = FS_NAME_DEFAULT_LENGTH - 1;
            }

            digit = &(nextAvailableName[digitPos]);

            *(digit-1) = '_';
            *digit     = '1';

            iteration++;
        }
        else
        {
            // We only support _1 thru _9 for now, as it seems unlikely to need more
            if((*digit) == '9') return false;
            
            (*digit)++;
        }
        
        return true;
    }
};

HAL_DblLinkedList<FileSystemVolume> FileSystemVolumeList::s_zombieVolumeList;

void FS_MountVolume( LPCSTR nameSpace, UINT32 serialNumber, UINT32 deviceFlags, BlockStorageDevice* blockStorageDevice )
{
    FileSystemVolume* volume = NULL;
    UINT32 numVolumes = 0;
    FILESYSTEM_DRIVER_INTERFACE* fsDriver = NULL;
    STREAM_DRIVER_INTERFACE* streamDriver = NULL;
    UINT32 i;
    
    if(!nameSpace || !blockStorageDevice)
    {
        return;
    }

    //--//

    // free up any memory taken up by this block storage device in the zombie list
    // so at any given time, we'll only have one set of FileSystemVolume per block 
    // storage device. Here we release (private_free) the memory that was allocated
    // in the previous insertion, later, prior to AddVolume, we'll allocate new memory
    // needed for this insertion.
    FileSystemVolume* current = FileSystemVolumeList::s_zombieVolumeList.FirstValidNode();
    FileSystemVolume* next;
    
    while(current)
    {
        next = current->Next();

        if(!(next && next->Next()))
        {    
            next = NULL;
        }

        // We'll only free the memory of this storage device
        if(current->m_volumeId.blockStorageDevice == blockStorageDevice)
        {
            current->Unlink();

            private_free( current );
        }

        current = next;
    }

    //--//

    /// First we find the FSDriver that will mount the block storage
    for (i = 0; i < g_InstalledFSCount; i++)
    {
        if (g_AvailableFSInterfaces[i].fsDriver && g_AvailableFSInterfaces[i].fsDriver->IsLoadableMedia &&
            g_AvailableFSInterfaces[i].fsDriver->IsLoadableMedia( blockStorageDevice, &numVolumes ))
        {
            fsDriver = g_AvailableFSInterfaces[i].fsDriver;
            streamDriver = g_AvailableFSInterfaces[i].streamDriver;
            break;
        }
    }

    if (!fsDriver)
    {
        numVolumes = 1;
    }

    for (i = 0; i < numVolumes; i++)
    {
        // Allocate the memory for this FileSystemVolume
        volume = (FileSystemVolume*)private_malloc( sizeof(FileSystemVolume) );

        if(!volume) // allocation failed
        {
            return;
        }

        // initialize content to 0
        memset( volume, 0, sizeof(FileSystemVolume) );

        if(!FileSystemVolumeList::AddVolume( volume, nameSpace, serialNumber, deviceFlags,
                                            streamDriver, fsDriver, blockStorageDevice, i, (fsDriver) ? TRUE : FALSE )) // init only when we have a valid fsDriver
        {
            // If for some reason, AddVolume fails, we'll keep trying other volumes
            private_free( volume );
            continue;
        }

        // Now we can notify managed code
        PostManagedEvent( EVENT_STORAGE, EVENT_SUBCATEGORY_MEDIAINSERT, 0, (UINT32)volume );
    }
}

void FS_UnmountVolume( BlockStorageDevice* blockStorageDevice )
{
    FileSystemVolume* current = FileSystemVolumeList::GetFirstVolume();
    FileSystemVolume* next;
    
    while(current)
    {
        if (current->m_volumeId.blockStorageDevice == blockStorageDevice)
        {
            // get the next node from the link list before removing it

            next = FileSystemVolumeList::GetNextVolume( *current );

            /// Let FS uninitialize now for this volume. Note this happens before managed stack 
            /// is informed.
            FileSystemVolumeList::RemoveVolume( current, TRUE );

            // Move the volume into the zombie list rather than free up the memory so all the subsequent Close() from
            // opened handles will complete and/or fail properly
            FileSystemVolumeList::s_zombieVolumeList.LinkAtBack( current );

            /// Notify managed code.
            PostManagedEvent( EVENT_STORAGE, EVENT_SUBCATEGORY_MEDIAEJECT, 0, (UINT32)current );

            current = next;
        }
        else
        {
            current = FileSystemVolumeList::GetNextVolume( *current );
        }
    }
}

//--// 

void FS_Initialize()
{
    for(UINT32 i = 0; i < g_InstalledFSCount; i++)
    {
        g_AvailableFSInterfaces[i].streamDriver->Initialize();
    }
}

//--//

HAL_DblLinkedList<FileSystemVolume> FileSystemVolumeList::s_volumeList;

//--//

void FileSystemVolumeList::Initialize()
{
    s_volumeList.Initialize();
    s_zombieVolumeList.Initialize();
}

BOOL FileSystemVolumeList::InitializeVolumes()
{
    FileSystemVolume* volume = s_volumeList.FirstNode();
        
    if(volume == NULL) 
    {
#if defined(PLATFORM_ARM)
        debug_printf( "There are no file system volume to initialize" );  
#endif
        return FALSE;
    }

    BOOL success = TRUE;
    
    while(volume->Next())
    {
        if(volume->InitializeVolume())
        {
            volume->m_fsDriver->GetVolumeLabel(&volume->m_volumeId, volume->m_label, ARRAYSIZE(volume->m_label));
        }
        else
        {
            success = FALSE; // even if success == FALSE, InitalizeVolume() will still get called
        }

        volume = volume->Next();
    }
    
    return success;
}

BOOL FileSystemVolumeList::UninitializeVolumes()
{
    BOOL success = TRUE;
    
    FileSystemVolume* volume = s_volumeList.FirstNode();
    FileSystemVolume* curVolume;

    while(volume->Next())
    {
        success = volume->UninitializeVolume() && success;  // even if success == FALSE, UninitalizeVolume() will still get called
        curVolume = volume;
        
        volume = volume->Next();
        
        curVolume->Unlink();
    }

    return success;
}

BOOL FileSystemVolumeList::AddVolume( FileSystemVolume* fsv, LPCSTR nameSpace, UINT32 serialNumber, UINT32 deviceFlags,
                                      STREAM_DRIVER_INTERFACE* streamDriver, FILESYSTEM_DRIVER_INTERFACE* fsDriver,
                                      BlockStorageDevice* blockStorageDevice, UINT32 volumeId, BOOL init )
{
    BOOL success = TRUE;

    if(!fsv) return FALSE;

    NextAvailableName nextName;
    FileSystemVolume* current;
    bool nameOK = true;

    // Set up the helper in case of namespace conflicts
    if(!nextName.Initialize( nameSpace ))
    {
        return FALSE;
    }

    do
    {
        // get the next namespace (the first time this is called, the original namespace is "generated")
        if(!nextName.GenerateNextAvailableName())
        {
            return FALSE;
        }
    
        nameOK = true;
        
        /// Detect if this namespace is already taken over or not.
        current = FileSystemVolumeList::GetFirstVolume();
        while (current)
        {
            if(0 == hal_stricmp( current->m_nameSpace, nextName.nextAvailableName ))
            {
                nameOK = false;
                break;
            }
    
            current = FileSystemVolumeList::GetNextVolume( *current );
        }
    }
    while(nameOK == false);

    
    // initialize the members of the FileSystemVolume
    memcpy( fsv->m_nameSpace, nextName.nextAvailableName, FS_NAME_MAXLENGTH );

    fsv->m_serialNumber                = serialNumber;
    fsv->m_deviceFlags                 = deviceFlags;
    fsv->m_streamDriver                = streamDriver;
    fsv->m_fsDriver                    = fsDriver;
    fsv->m_volumeId.blockStorageDevice = blockStorageDevice;
    fsv->m_volumeId.volumeId           = volumeId;

    // init the dblinkednode;
    fsv->Initialize();
    if(init)
    {
        success = fsv->InitializeVolume();

        fsDriver->GetVolumeLabel(&fsv->m_volumeId, fsv->m_label, ARRAYSIZE(fsv->m_label));
    }
    else
    {
        fsv->m_label[0] = 0;
    }

    // only add the volume if initialization was successful, when requested at all
    if(success)
    {
        s_volumeList.LinkAtBack( fsv );
        return TRUE;
    }
    
    return FALSE;
}

BOOL FileSystemVolumeList::RemoveVolume( FileSystemVolume* fsv, BOOL uninit )
{
    if(fsv->IsLinked())
    {    
        fsv->Unlink();
        
        if (uninit)
        {
            fsv->UninitializeVolume();
        }

        return TRUE;
    }
    
    return FALSE;
}

FileSystemVolume* FileSystemVolumeList::GetFirstVolume()
{ 
    return s_volumeList.FirstValidNode();
}

FileSystemVolume* FileSystemVolumeList::GetNextVolume( FileSystemVolume& volume )
{ 
    FileSystemVolume* nextVolume = volume.Next();

    if(nextVolume && nextVolume->Next())
    {    
        return nextVolume;
    }

    return NULL;
}

UINT32 FileSystemVolumeList::GetNumVolumes()
{
    return s_volumeList.NumOfNodes();
}

FileSystemVolume* FileSystemVolumeList::FindVolume( LPCSTR nameSpace, UINT32 nameSpaceLength )
{
    FileSystemVolume* volume = GetFirstVolume();
    char ns[FS_NAME_MAXLENGTH];

    memcpy(ns, nameSpace, nameSpaceLength);
    ns[nameSpaceLength] = 0;

    while(volume)
    {
        if(hal_stricmp( volume->m_nameSpace, ns ) == 0)
        {
            // Make sure we match the entire namespace string by checking for null terminator
            // In case nameSpace is a substring of m_nameSpace (i.e. nameSpace = "ROOT", m_nameSpace == "ROOT1")
            if(volume->m_nameSpace[nameSpaceLength] == '\0')
            {
                return volume;
            }
        }

        volume = FileSystemVolumeList::GetNextVolume( *volume );
    }
    
    return NULL;
}

BOOL FileSystemVolumeList::Contains( FileSystemVolume* fsv )
{
    FileSystemVolume* volume = GetFirstVolume();

    while(volume)
    {
        if(volume == fsv)
        {
            return TRUE;
        }

        volume = FileSystemVolumeList::GetNextVolume( *volume );
    }

    return FALSE;
}

