/**
 * @file
 */
/******************************************************************************
 * Copyright (c) 2013-2014, AllSeen Alliance. All rights reserved.
 *
 *    Permission to use, copy, modify, and/or distribute this software for any
 *    purpose with or without fee is hereby granted, provided that the above
 *    copyright notice and this permission notice appear in all copies.
 *
 *    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 *    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 *    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 *    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 *    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 *    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 *    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 ******************************************************************************/

/**
 * Per-module definition of the current module for debug logging.  Must be defined
 * prior to first inclusion of aj_debug.h
 */
#define AJ_MODULE NVRAM

#include "aj_nvram.h"
#include "aj_target_nvram.h"
#include "aj_debug.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgNVRAM = 0;
#endif

extern uint8_t* AJ_NVRAM_BASE_ADDRESS;

static uint8_t isCompact = FALSE;

#define AJ_NVRAM_END_ADDRESS (AJ_NVRAM_BASE_ADDRESS + AJ_NVRAM_SIZE)

uint32_t AJ_NVRAM_GetSize(void)
{
    uint32_t size = 0;
    uint16_t* data = (uint16_t*)(AJ_NVRAM_BASE_ADDRESS + SENTINEL_OFFSET);
    uint16_t entryId = 0;
    uint16_t capacity = 0;
    while ((uint8_t*)data < (uint8_t*)AJ_NVRAM_END_ADDRESS && *data != INVALID_DATA) {
        entryId = *data;
        capacity = *(data + 1);
        if (entryId != 0) {
            size += capacity + ENTRY_HEADER_SIZE;
        }
        data += (ENTRY_HEADER_SIZE + capacity) >> 1;
    }
    return size + SENTINEL_OFFSET;
}

extern AJ_Status _AJ_CompactNVStorage();

uint32_t AJ_NVRAM_GetSizeRemaining(void)
{
    if (!isCompact) {
        _AJ_CompactNVStorage();
        isCompact = TRUE;
    }
    return AJ_NVRAM_SIZE - AJ_NVRAM_GetSize();
}

void AJ_NVRAM_Layout_Print()
{
    int i = 0;
    uint16_t* data = (uint16_t*)(AJ_NVRAM_BASE_ADDRESS + SENTINEL_OFFSET);
    uint16_t entryId = 0;
    uint16_t capacity = 0;
    AJ_AlwaysPrintf(("============ AJ NVRAM Map ===========\n"));
    for (i = 0; i < SENTINEL_OFFSET; i++) {
        AJ_AlwaysPrintf(("%c", *((uint8_t*)(AJ_NVRAM_BASE_ADDRESS + i))));
    }
    AJ_AlwaysPrintf(("\n"));

    while ((uint8_t*)data < (uint8_t*)AJ_NVRAM_END_ADDRESS && *data != INVALID_DATA) {
        entryId = *data;
        capacity = *(data + 1);
        AJ_AlwaysPrintf(("ID = %d, capacity = %d\n", entryId, capacity));
        data += (ENTRY_HEADER_SIZE + capacity) >> 1;
    }
    AJ_AlwaysPrintf(("============ End ===========\n"));
}

/**
 * Find an entry in the NVRAM with the specific id
 *
 * @return Pointer pointing to an entry in the NVRAM if an entry with the specified id is found
 *         NULL otherwise
 */
uint8_t* AJ_FindNVEntry(uint16_t id)
{
    uint16_t capacity = 0;
    uint16_t* data = (uint16_t*)(AJ_NVRAM_BASE_ADDRESS + SENTINEL_OFFSET);

    AJ_InfoPrintf(("AJ_FindNVEntry(id=%d.)\n", id));

    while ((uint8_t*)data < (uint8_t*)AJ_NVRAM_END_ADDRESS) {
        if (*data != id) {
            capacity = *(data + 1);
            if (*data == INVALID_DATA) {
                break;
            }
            data += (ENTRY_HEADER_SIZE + capacity) >> 1;
        } else {
            AJ_InfoPrintf(("AJ_FindNVEntry(): data=0x%p\n", data));
            return (uint8_t*)data;
        }
    }
    AJ_InfoPrintf(("AJ_FindNVEntry(): data=NULL\n"));
    return NULL;
}

AJ_Status AJ_NVRAM_Create(uint16_t id, uint16_t capacity)
{
    uint8_t* ptr;
    NV_EntryHeader header;

    AJ_InfoPrintf(("AJ_NVRAM_Create(id=%d., capacity=%d.)\n", id, capacity));

    if (!capacity || AJ_NVRAM_Exist(id)) {
        AJ_ErrPrintf(("AJ_NVRAM_Create(): AJ_ERR_FAILURE\n"));
        return AJ_ERR_FAILURE;
    }

    capacity = WORD_ALIGN(capacity); // 4-byte alignment
    ptr = AJ_FindNVEntry(INVALID_DATA);
    if (!ptr || (ptr + ENTRY_HEADER_SIZE + capacity > AJ_NVRAM_END_ADDRESS)) {
        if (!isCompact) {
            AJ_InfoPrintf(("AJ_NVRAM_Create(): _AJ_CompactNVStorage()\n"));
            _AJ_CompactNVStorage();
            isCompact = TRUE;
        }
        ptr = AJ_FindNVEntry(INVALID_DATA);
        if (!ptr || ptr + ENTRY_HEADER_SIZE + capacity > AJ_NVRAM_END_ADDRESS) {
            AJ_InfoPrintf(("AJ_NVRAM_Create(): AJ_ERR_FAILURE\n"));
            return AJ_ERR_FAILURE;
        }
    }
    header.id = id;
    header.capacity = capacity;
    _AJ_NV_Write(ptr, &header, ENTRY_HEADER_SIZE);
    return AJ_OK;
}

AJ_Status AJ_NVRAM_Delete(uint16_t id)
{
    NV_EntryHeader newHeader;
    uint8_t* ptr;

    AJ_InfoPrintf(("AJ_NVRAM_Delete(id=%d.)\n", id));

    ptr = AJ_FindNVEntry(id);

    if (!ptr) {
        AJ_ErrPrintf(("AJ_NVRAM_Delete(): AJ_ERR_FAILURE\n"));
        return AJ_ERR_FAILURE;
    }

    memcpy(&newHeader, ptr, ENTRY_HEADER_SIZE);
    newHeader.id = 0;
    _AJ_NV_Write(ptr, &newHeader, ENTRY_HEADER_SIZE);
    isCompact = FALSE;
    return AJ_OK;
}

AJ_NV_DATASET* AJ_NVRAM_Open(uint16_t id, char* mode, uint16_t capacity)
{
    AJ_Status status = AJ_OK;
    uint8_t* entry = NULL;
    AJ_NV_DATASET* handle = NULL;

    AJ_InfoPrintf(("AJ_NVRAM_Open(id=%d., mode=\"%s\", capacity=%d.)\n", id, mode, capacity));

    if (!id) {
        AJ_ErrPrintf(("AJ_NVRAM_Open(): invalid id\n"));
        goto OPEN_ERR_EXIT;
    }
    if (!mode || mode[1] || (*mode != 'r' && *mode != 'w')) {
        AJ_ErrPrintf(("AJ_NVRAM_Open(): invalid access mode\n"));
        goto OPEN_ERR_EXIT;
    }
    if (*mode == AJ_NV_DATASET_MODE_WRITE) {
        if (capacity == 0) {
            AJ_ErrPrintf(("AJ_NVRAM_Open(): invalid capacity\n"));
            goto OPEN_ERR_EXIT;
        }

        if (AJ_NVRAM_Exist(id)) {
            status = AJ_NVRAM_Delete(id);
        }
        if (status != AJ_OK) {
            AJ_ErrPrintf(("AJ_NVRAM_Open(): AJ_NVRAM_Delete() failure: status=%s\n", AJ_StatusText(status)));
            goto OPEN_ERR_EXIT;
        }

        status = AJ_NVRAM_Create(id, capacity);
        if (status != AJ_OK) {
            AJ_ErrPrintf(("AJ_NVRAM_Open(): AJ_NVRAM_Create() failure: status=%s\n", AJ_StatusText(status)));
            goto OPEN_ERR_EXIT;
        }
        entry = AJ_FindNVEntry(id);
        if (!entry) {
            AJ_ErrPrintf(("AJ_NVRAM_Open(): Data set %d. does not exist\n", id));
            goto OPEN_ERR_EXIT;
        }
    } else {
        entry = AJ_FindNVEntry(id);
        if (!entry) {
            AJ_WarnPrintf(("AJ_NVRAM_Open(): Data set %d. does not exist\n", id));
            goto OPEN_ERR_EXIT;
        }
    }

    handle = (AJ_NV_DATASET*)AJ_Malloc(sizeof(AJ_NV_DATASET));
    if (!handle) {
        AJ_ErrPrintf(("AJ_NVRAM_Open(): AJ_Malloc() failure\n"));
        goto OPEN_ERR_EXIT;
    }

    handle->id = id;
    handle->curPos = 0;
    handle->mode = *mode;
    handle->capacity = ((NV_EntryHeader*)entry)->capacity;
    handle->inode = entry;
    return handle;

OPEN_ERR_EXIT:
    if (handle) {
        AJ_Free(handle);
        handle = NULL;
    }
    AJ_ErrPrintf(("AJ_NVRAM_Open(): failure: status=%s\n", AJ_StatusText(status)));
    return NULL;
}

size_t AJ_NVRAM_Write(const void* ptr, uint16_t size, AJ_NV_DATASET* handle)
{
    int16_t bytesWrite = 0;
    uint8_t patchBytes = 0;
    uint8_t* buf = (uint8_t*)ptr;
    NV_EntryHeader* header;

    if (!handle || handle->mode == AJ_NV_DATASET_MODE_READ) {
        AJ_ErrPrintf(("AJ_NVRAM_Write(): AJ_ERR_ACCESS\n"));
        return -1;
    }

    header = (NV_EntryHeader*)handle->inode;

    AJ_InfoPrintf(("AJ_NVRAM_Write(ptr=0x%p, size=%d., handle=0x%p)\n", ptr, size, handle));

    if (header->capacity <= handle->curPos) {
        AJ_AlwaysPrintf(("AJ_NVRAM_Write(): AJ_ERR_RESOURCES\n"));
        return -1;
    }

    bytesWrite = header->capacity - handle->curPos;
    bytesWrite = (bytesWrite < size) ? bytesWrite : size;
    if (bytesWrite > 0 && ((handle->curPos & 0x3) != 0)) {
        uint8_t tmpBuf[4];
        uint16_t alignedPos = handle->curPos & (~0x3);
        memset(tmpBuf, INVALID_DATA_BYTE, sizeof(tmpBuf));
        patchBytes = 4 - (handle->curPos & 0x3);
        memcpy(tmpBuf, handle->inode + sizeof(NV_EntryHeader) + alignedPos, handle->curPos & 0x3);
        if (patchBytes > bytesWrite) {
            patchBytes = (uint8_t)bytesWrite;
        }
        memcpy(tmpBuf + (handle->curPos & 0x3), buf, patchBytes);
        _AJ_NV_Write(handle->inode + sizeof(NV_EntryHeader) + alignedPos, tmpBuf, 4);
        buf += patchBytes;
        bytesWrite -= patchBytes;
        handle->curPos += patchBytes;
    }

    if (bytesWrite > 0) {
        _AJ_NV_Write(handle->inode + sizeof(NV_EntryHeader) + handle->curPos, buf, bytesWrite);
        handle->curPos += bytesWrite;
    }
    return bytesWrite + patchBytes;
}

const void* AJ_NVRAM_Peek(AJ_NV_DATASET* handle)
{
    NV_EntryHeader* header;

    if (!handle || handle->mode == AJ_NV_DATASET_MODE_WRITE) {
        AJ_ErrPrintf(("AJ_NVRAM_Peek(): AJ_ERR_ACCESS\n"));
        return NULL;
    }
    header = (NV_EntryHeader*)handle->inode;
    return (const void*)(handle->inode + sizeof(NV_EntryHeader) +  handle->curPos);
}

size_t AJ_NVRAM_Read(void* ptr, uint16_t size, AJ_NV_DATASET* handle)
{
    uint16_t bytesRead = 0;
    NV_EntryHeader* header;

    if (!handle || handle->mode == AJ_NV_DATASET_MODE_WRITE) {
        AJ_ErrPrintf(("AJ_NVRAM_Read(): AJ_ERR_ACCESS\n"));
        return -1;
    }

    header = (NV_EntryHeader*)handle->inode;

    AJ_InfoPrintf(("AJ_NVRAM_Read(ptr=0x%p, size=%d., handle=0x%p)\n", ptr, size, handle));

    if (header->capacity <= handle->curPos) {
        AJ_ErrPrintf(("AJ_NVRAM_Read(): AJ_ERR_RESOURCES\n"));
        return -1;
    }
    bytesRead = header->capacity -  handle->curPos;
    bytesRead = (bytesRead < size) ? bytesRead : size;
    if (bytesRead > 0) {
        _AJ_NV_Read(handle->inode + sizeof(NV_EntryHeader) +  handle->curPos, ptr, bytesRead);
        handle->curPos += bytesRead;
    }
    return bytesRead;
}

AJ_Status AJ_NVRAM_Close(AJ_NV_DATASET* handle)
{
    AJ_InfoPrintf(("AJ_NVRAM_Close(handle=0x%p)\n", handle));

    if (!handle) {
        AJ_ErrPrintf(("AJ_NVRAM_Close(): AJ_ERR_INVALID\n"));
        return AJ_ERR_INVALID;
    }

    AJ_Free(handle);
    handle = NULL;
    return AJ_OK;
}

uint8_t AJ_NVRAM_Exist(uint16_t id)
{
    AJ_InfoPrintf(("AJ_NVRAM_Exist(id=%d.)\n", id));

    if (!id) {
        AJ_ErrPrintf(("AJ_NVRAM_Exist(): AJ_ERR_INVALID\n"));
        return FALSE; // the unique id is not allowed to be 0
    }
    return (NULL != AJ_FindNVEntry(id));
}

void AJ_NVRAM_Clear()
{
    _AJ_NVRAM_Clear();
}

