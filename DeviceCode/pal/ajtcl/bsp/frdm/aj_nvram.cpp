/**
 * @file
 */
/******************************************************************************
 * Copyright AllSeen Alliance. All rights reserved.
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

#include "mbed.h"
#include "SDFileSystem.h"
#include "aj_nvram.h"
#include "aj_target_nvram.h"
#include "aj_debug.h"

extern "C" {
/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgNVRAM = 0;
#endif

#define MAX_FNAME_SZ 14

static void idToString(uint16_t id, char* str)
{
    sprintf(str, "/sd/%u.ajnv", id);
}

static uint16_t stringToId(char* str)
{
    uint8_t i = 0;
    char buf[6];
    uint16_t id;
    while (i < strlen(str)) {
        if (str[i] == '.') {
            break;
        }
        buf[i] = str[i];
        i++;
    }
    buf[i] = '\0';
    id = atoi(buf);
    return id;
}

static uint8_t isEntry(char* file)
{
    uint8_t len = strlen(file);
    if (file[len - 4] == 'a' && file[len - 3] == 'j' && file[len - 2] == 'n' && file[len - 1] == 'v') {
        return 1;
    }
    return 0;
}

void AJ_NVRAM_Init(void)
{
    return;
}

uint32_t AJ_NVRAM_GetSize(void)
{
    DIR* dir;
    FILE* f;
    struct dirent* entry;
    uint32_t size = 0;
    dir = opendir("/sd");
    if (dir) {
        while ((entry = readdir(dir)) != NULL) {
            uint16_t sz = 0;
            char buf[MAX_FNAME_SZ + 1];
            buf[0] = '\0';
            if (isEntry(entry->d_name)) {
                strcpy(buf, "/sd/");
                /* Append 11 characters max: "12345.ajnv\0" */
                strncat(buf, entry->d_name, 11);
                f = fopen(buf, "r");
                if (f == NULL) {
                    AJ_ErrPrintf(("AJ_NVRAM_GetSize(): Error opening file\n"));
                    return 0;
                }
                fread(&sz, 1, sizeof(uint16_t), f);
                size += sz;
                fclose(f);
            }
        }
        closedir(dir);
    }
    return size;
}

uint32_t AJ_NVRAM_GetSizeRemaining(void)
{
    return AJ_NVRAM_SIZE - AJ_NVRAM_GetSize();
}

void AJ_NVRAM_Layout_Print()
{
    DIR* dir;
    FILE* f;
    struct dirent* entry;
    dir = opendir("/sd");
    AJ_AlwaysPrintf(("============ AJ NVRAM Map ===========\n"));
    if (dir) {
        while ((entry = readdir(dir)) != NULL) {
            uint16_t sz;
            char buf[MAX_FNAME_SZ + 1];
            buf[0] = '\0';
            if (isEntry(entry->d_name)) {
                strcpy(buf, "/sd/");
                strncat(buf, entry->d_name, 11);
                f = fopen(buf, "r");
                if (f == NULL) {
                    AJ_ErrPrintf(("AJ_NVRAM_Layout_Print(): Could not open file, AJ_ERR_FAILURE\n"));
                    continue;
                }
                fread(&sz, 1, sizeof(uint16_t), f);
                AJ_AlwaysPrintf(("ID = %d, capacity = %d\n", stringToId(entry->d_name), sz));
                fclose(f);
            }
        }
        closedir(dir);
    }
    AJ_AlwaysPrintf(("============ End ===========\n"));
}

uint8_t AJ_NVRAM_Exist(uint16_t id)
{
    char fname[MAX_FNAME_SZ + 1];
    DIR* dir;
    struct dirent* entry;
    idToString(id, fname);
    dir = opendir("/sd");
    if (dir) {
        while ((entry = readdir(dir)) != NULL) {
            if (isEntry(entry->d_name)) {
                if (strcmp(entry->d_name, fname + 4) == 0) {
                    return 1;
                }
            }
        }
        closedir(dir);
    }
    return 0;
}

AJ_Status AJ_NVRAM_Create(uint16_t id, uint16_t capacity)
{
    char fname[MAX_FNAME_SZ + 1];
    idToString(id, fname);
    FILE* f;
    AJ_InfoPrintf(("AJ_NVRAM_Create(id=%d., capacity=%d.)\n", id, capacity));
    if (!capacity || AJ_NVRAM_Exist(id)) {
        AJ_ErrPrintf(("AJ_NVRAM_Create(): AJ_ERR_FAILURE\n"));
        return AJ_ERR_FAILURE;
    }
    f = fopen(fname, "w");
    if (f == NULL) {
        AJ_ErrPrintf(("AJ_NVRAM_Create(): Could not open file, AJ_ERR_FAILURE\n"));
        return AJ_ERR_FAILURE;
    }
    fwrite(&capacity, sizeof(uint16_t), 1, f);
    fflush(f);
    fclose(f);
    return AJ_OK;
}

AJ_Status AJ_NVRAM_Delete(uint16_t id)
{
    char fname[MAX_FNAME_SZ + 1];
    idToString(id, fname);
    if (remove(fname) != 0) {
        AJ_ErrPrintf(("AJ_NVRAM_Delete(): Could not delete file %s\n", fname));
        return AJ_ERR_FAILURE;
    }
    return AJ_OK;
}

uint8_t* AJ_FindNVEntry(uint16_t id)
{
    FILE* f;
    char fname[MAX_FNAME_SZ + 1];
    uint16_t size;
    uint8_t* data;

    idToString(id, fname);
    f = fopen(fname, "r");
    if (f == NULL) {
        AJ_ErrPrintf(("AJ_FindNVEntry(): Error opening file\n"));
        return NULL;
    }
    fseek(f, 0, SEEK_SET);
    fread(&size, 1, sizeof(uint16_t), f);
    if (!size) {
        AJ_ErrPrintf(("AJ_FindNVEntry(): Error when reading entry, zero size\n"));
        fclose(f);
        return NULL;
    }
    data = (uint8_t*)AJ_Malloc(size + sizeof(uint16_t));
    fseek(f, 0, SEEK_SET);
    fread(data, size + sizeof(uint16_t), 1, f);
    fflush(f);
    fclose(f);
    return data;
}

FILE* _AJ_NV_Open(uint16_t id, char* mode)
{
    FILE* f;
    char fname[MAX_FNAME_SZ + 1];
    idToString(id, fname);
    if (!id) {
        AJ_ErrPrintf(("AJ_NVRAM_Open(): invalid id\n"));
        goto OPEN_ERR_EXIT;
    }
    if (!mode || mode[1] || (*mode != 'r' && *mode != 'w')) {
        AJ_ErrPrintf(("AJ_NVRAM_Open(): invalid access mode\n"));
        goto OPEN_ERR_EXIT;
    }
    if (*mode == 'w') {
        /*
         * File already has the length written from AJ_NVRAM_Create() so
         * opening as mode "w" would erase that from the file.
         */
        f = fopen(fname, "a");
    } else {
        f = fopen(fname, "r");
    }
    if (f == NULL) {
        AJ_ErrPrintf(("_AJ_NV_Open(): Could not open file\n"));
        goto OPEN_ERR_EXIT;
    }
    return f;

OPEN_ERR_EXIT:
    AJ_ErrPrintf(("AJ_NVRAM_Open(): failure: status=%s\n", AJ_StatusText(AJ_ERR_FAILURE)));
    return NULL;
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

        if (!AJ_NVRAM_Exist(id)) {
            status = AJ_NVRAM_Create(id, capacity);
        }
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
    handle->capacity = capacity;
    handle->inode = entry;
    handle->internal = (void*)_AJ_NV_Open(id, mode);
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
    FILE* f;
    if (!handle || handle->mode == AJ_NV_DATASET_MODE_READ) {
        AJ_ErrPrintf(("AJ_NVRAM_Write(): AJ_ERR_ACCESS\n"));
        return -1;
    }
    f = (FILE*)handle->internal;
    /* Copy the data into the current RAM section */
    memcpy(handle->inode + handle->curPos + sizeof(uint16_t), ptr, size);
    fseek(f, handle->curPos + sizeof(uint16_t), SEEK_SET);
    /* fwrite returns number of items written, which should be 1 */
    if (fwrite(ptr, size, 1, f) != 1) {
        AJ_ErrPrintf(("AJ_NVRAM_Write(): Could not write to file\n"));
        return -1;
    }
    fflush(f);
    handle->curPos += size;
    return size;
}

const void* AJ_NVRAM_Peek(AJ_NV_DATASET* handle)
{
    if (!handle || handle->mode == AJ_NV_DATASET_MODE_WRITE) {
        AJ_ErrPrintf(("AJ_NVRAM_Peek(): AJ_ERR_ACCESS\n"));
        return NULL;
    }
    return (const void*)(handle->inode + sizeof(uint16_t) + handle->curPos);
}

size_t AJ_NVRAM_Read(void* ptr, uint16_t size, AJ_NV_DATASET* handle)
{
    if (!handle || handle->mode == AJ_NV_DATASET_MODE_WRITE) {
        AJ_ErrPrintf(("AJ_NVRAM_Read(): AJ_ERR_ACCESS\n"));
        return -1;
    }
    /* Copy the data from the current RAM section */
    memcpy(ptr, handle->inode + handle->curPos + sizeof(uint16_t), size);
    handle->curPos += size;
    return size;
}

AJ_Status AJ_NVRAM_Close(AJ_NV_DATASET* handle)
{
    if (!handle) {
        AJ_ErrPrintf(("AJ_NVRAM_Close(): AJ_ERR_INVALID\n"));
        return AJ_ERR_INVALID;
    }
    fclose((FILE*)handle->internal);
    AJ_Free(handle->inode);
    AJ_Free(handle);
    return AJ_OK;
}

void AJ_NVRAM_Clear()
{
    DIR* dir;
    struct dirent* entry;
    int ret = 0;
    dir = opendir("/sd");
    if (dir) {
        while ((entry = readdir(dir)) != NULL) {
            char fname[MAX_FNAME_SZ + 1];
            fname[0] = '\0';
            if (isEntry(entry->d_name)) {
                strcat(fname, "/sd/");
                strncat(fname, entry->d_name, 11);
                ret = remove(fname);
                if (ret != 0) {
                    AJ_ErrPrintf(("Could not remove entry %s, ret = %d\n", entry->d_name, ret));
                }
            }
        }
        closedir(dir);
    }
}

}
