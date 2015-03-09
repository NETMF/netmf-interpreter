////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"
#include "FAT_FS.h"

//--//

FAT_MemoryManager::FAT_LogicDiskBuffer FAT_MemoryManager::s_logicDisks[MAX_VOLUMES];
FAT_MemoryManager::FAT_HandleBuffer    FAT_MemoryManager::s_handles   [MAX_OPEN_HANDLES];

//--//

void FAT_MemoryManager::Initialize()
{
    int i;

    for(i = 0; i < MAX_VOLUMES; i++)
    {
        s_logicDisks[i].inUse = FALSE;
        memset(&s_logicDisks[i].logicDisk, 0, sizeof(s_logicDisks[i].logicDisk));
    }

    for(i = 0 ; i < MAX_OPEN_HANDLES; i++)
    {
        s_handles[i].inUse = FALSE;
        memset(&s_handles[i].handle, 0, sizeof(s_handles[i].handle));
    }
}

FAT_LogicDisk* FAT_MemoryManager::AllocateLogicDisk( const VOLUME_ID * volume )
{
    if(!volume || !(volume->blockStorageDevice))
    {
        return NULL;
    }

    FAT_LogicDisk* logicDisk = GetLogicDisk( volume );

    if(logicDisk) return logicDisk;

    for(int i = 0; i < MAX_VOLUMES; i++)
    {
        if(!s_logicDisks[i].inUse)
        {
            s_logicDisks[i].inUse = TRUE;
            return &(s_logicDisks[i].logicDisk);
        }
    }

    return NULL; // if all slots are in use
}

FAT_LogicDisk* FAT_MemoryManager::GetLogicDisk( const VOLUME_ID * volume )
{
    if(!volume || !(volume->blockStorageDevice))
    {
        return NULL;
    }
    
    FAT_LogicDiskBuffer* buffer;
    
    for(int i = 0; i < MAX_VOLUMES; i++)
    {
        buffer = &s_logicDisks[i];
        if(buffer->inUse && buffer->logicDisk.m_volumeId == volume->volumeId && buffer->logicDisk.m_blockStorageDevice == volume->blockStorageDevice)
        {
            return &(buffer->logicDisk);
        }
    }

    return NULL;
}

void FAT_MemoryManager::FreeLogicDisk( FAT_LogicDisk* logicDisk )
{
    for(int i = 0; i < MAX_VOLUMES; i++)
    {
        if(&(s_logicDisks[i].logicDisk) == logicDisk)
        {
            s_logicDisks[i].inUse = FALSE;
            memset(&s_logicDisks[i].logicDisk, 0, sizeof(s_logicDisks[i].logicDisk));
            return;
        }
    }
}

void* FAT_MemoryManager::AllocateHandle()
{
    for(int i = 0; i < MAX_OPEN_HANDLES; i++)
    {
        if(!s_handles[i].inUse)
        {
            s_handles[i].inUse = TRUE;
            return &(s_handles[i].handle);
        }
    }

    return NULL; // if all slots are in use
}

void FAT_MemoryManager::FreeHandle( void* handle )
{
    for(int i = 0; i < MAX_OPEN_HANDLES; i++)
    {
        if(&(s_handles[i].handle) == handle)
        {
            s_handles[i].inUse = FALSE;
            memset(&s_handles[i].handle, 0, sizeof(s_handles[i].handle));
            return;
        }
    }
}

