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
#define AJ_MODULE TARGET_NVRAM

#include "aj_nvram.h"
#include "aj_target_nvram.h"
#include "aj_debug.h"
#include <tinyhal.h>

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgTARGET_NVRAM = 0;
#endif
//--// Use netmf configure block as the NV-RAM

#define AJ_USE_HAL_CONFIG       1
#define AJ_NVRAM_CONFIGURE      "AJ_NVRAM"



//--//
uint8_t AJ_EMULATED_NVRAM[AJ_NVRAM_SIZE];
uint8_t* AJ_NVRAM_BASE_ADDRESS;


extern void AJ_NVRAM_Layout_Print();

#define NV_FILE "ajtcl.nvram"

const char* nvFile = NV_FILE;

void AJ_SetNVRAM_FilePath(const char* path)
{
    if (path) {
        nvFile = path;
    }
}


void AJ_NVRAM_Init()
{
    AJ_NVRAM_BASE_ADDRESS = AJ_EMULATED_NVRAM;
    _AJ_LoadNVFromFile();
    if (*((uint32_t*)AJ_NVRAM_BASE_ADDRESS) != AJ_NV_SENTINEL) {
        _AJ_NVRAM_Clear();
        _AJ_StoreNVToFile();
    }
}

void _AJ_NV_Write(void* dest, void* buf, uint16_t size)
{
    
    if (((uint32_t)size + (uint32_t) dest) > (uint32_t)AJ_NVRAM_END_ADDRESS)
        return;  
    memcpy(dest, buf, size);
    _AJ_StoreNVToFile();
}

void _AJ_NV_Read(void* src, void* buf, uint16_t size)
{
    memcpy(buf, src, size);
}

void _AJ_NVRAM_Clear()
{
    memset((uint8_t*)AJ_NVRAM_BASE_ADDRESS, INVALID_DATA_BYTE, AJ_NVRAM_SIZE);
    *((uint32_t*)AJ_NVRAM_BASE_ADDRESS) = AJ_NV_SENTINEL;
    _AJ_StoreNVToFile();
}

AJ_Status _AJ_LoadNVFromFile()
{

#ifdef AJ_USE_HAL_CONFIG
    BOOL state ;

    memset(AJ_NVRAM_BASE_ADDRESS, INVALID_DATA_BYTE, AJ_NVRAM_SIZE);
    state = HAL_CONFIG_BLOCK::ApplyConfig( AJ_NVRAM_CONFIGURE, AJ_NVRAM_BASE_ADDRESS, AJ_NVRAM_SIZE );
    if (state)
        return AJ_OK;
    else
        return AJ_ERR_FAILURE;
#endif

    return AJ_OK;
}

AJ_Status _AJ_StoreNVToFile()
{

#ifdef AJ_USE_HAL_CONFIG

    BOOL state ;
    state = HAL_CONFIG_BLOCK::UpdateBlockWithName( AJ_NVRAM_CONFIGURE, AJ_NVRAM_BASE_ADDRESS, AJ_NVRAM_SIZE, FALSE );
    if (state)
        return AJ_OK;
    else
        return AJ_ERR_FAILURE;
#endif

    return AJ_OK;
}

// Compact the storage by removing invalid entries
AJ_Status _AJ_CompactNVStorage()
{
    uint16_t capacity = 0;
    uint16_t id = 0;
    uint16_t* data = (uint16_t*)(AJ_NVRAM_BASE_ADDRESS + SENTINEL_OFFSET);
    uint8_t* writePtr = (uint8_t*)data;
    uint16_t entrySize = 0;
    uint16_t garbage = 0;
    //AJ_NVRAM_Layout_Print();
    while ((uint8_t*)data < (uint8_t*)AJ_NVRAM_END_ADDRESS && *data != INVALID_DATA) {
        id = *data;
        capacity = *(data + 1);
        entrySize = ENTRY_HEADER_SIZE + capacity;

        // add the check for capacity size, just to avoid corruption of data cause some 
        // the data set to larger than our AJ_NVRAM_SIZE
        if ((id != INVALID_ID) && (capacity <0x100)  )  {
            // make sure the size of the entry is not large than the memory size.                
            if ((uint32_t)( writePtr + entrySize) >= (uint32_t)AJ_NVRAM_END_ADDRESS)
                break;
            // instead keep update the NV ram, updated the RAM first, write it at the end.
            memcpy(writePtr, data, entrySize);
//            _AJ_NV_Write(writePtr, data, entrySize);
            writePtr += entrySize;
        } else {
            garbage += entrySize;
        }
        data += (entrySize >> 1);
    }

    memset(writePtr, INVALID_DATA_BYTE, garbage);
    _AJ_StoreNVToFile();
    //AJ_NVRAM_Layout_Print();
    return AJ_OK;
}
