/**
 * @file Platform specific functions
 */
/******************************************************************************
 * Copyright (c) 2014, AllSeen Alliance. All rights reserved.
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

/******************************************************************************
 * Any time in this file there is a comment including:
    nvm_***, sysclk_***, board_***, stdio_***, rstc_***

 * note that the API associated with it may be subject to this Atmel license:
 * (information about it is also at www.atmel.com/asf)
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 1. Redistributions of source code must retain the above copyright notice, this
 *     list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 3. The name of Atmel may not be used to endorse or promote products derived
 *    from this software without specific prior written permission.
 * 4. This software may only be redistributed and used in connection with an
 *    Atmel microcontroller product.
 * THIS SOFTWARE IS PROVIDED BY ATMEL "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 * PARTICULAR PURPOSE AND NON-INFRINGEMENT ARE EXPRESSLY AND SPECIFICALLY DISCLAIMED. IN
 * NO EVENT SHALL ATMEL BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY,
 * OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
 * GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/


#include "aj_bsp.h"
#include "aj_target_platform.h"
#include "stdio_serial.h"
#include "aj_nvram.h"
#include "aj_target_nvram.h"

void ASSERT(int i)
{
    if (!i) {
        printf("ASSERT FAILED");
    }
}
#define AJ_NVRAM_SECTOR_SIZE (1024)
// Reserve one sector as a buffer for NVRAM storage compaction
//#define AJ_NVRAM_COMPACTION_BUF (total - AJ_NVRAM_SECTOR_SIZE)

uint8_t* const AJ_NVRAM_BASE_ADDRESS = (uint8_t*)(0x100000 - AJ_NVRAM_SECTOR_SIZE - AJ_NVRAM_SIZE);
uint8_t* const AJ_NVRAM_COMPACTION_BUF = (uint8_t*)(0x100000 - AJ_NVRAM_SECTOR_SIZE);
#define AJ_NVRAM_END_ADDRESS (AJ_NVRAM_BASE_ADDRESS + AJ_NVRAM_SIZE)
uint32_t AJ_NVRAM_PageSize = 256; /* default page size of NVRAM memory */

AJ_Status _AJ_NV_Write(void* dest, void* buf, uint16_t size)
{
    if (nvm_write(INT_FLASH, (uint32_t)dest, buf, size) != 0) {
        return AJ_ERR_INVALID;
    }
    return AJ_OK;
}

AJ_Status _AJ_NV_Read(void* src, void* buf, uint16_t size)
{
    if (nvm_read(INT_FLASH, (uint32_t)src, buf, size) != 0) {
        return AJ_ERR_INVALID;
    }
    return AJ_OK;
}

void AJ_NVRAM_Init()
{
    nvm_init(INT_FLASH);
    nvm_get_page_size(INT_FLASH, &AJ_NVRAM_PageSize);
    if (*((uint32_t*)AJ_NVRAM_BASE_ADDRESS) != AJ_NV_SENTINEL) {
        AJ_AlwaysPrintf(("Sentinel has not been set, clearing NVRAM\n"));
        _AJ_NVRAM_Clear();
    }
}
void AJ_NV_EraseSector(uint32_t sector)
{
    int i;
    for (i = 0; i < (AJ_NVRAM_SECTOR_SIZE / AJ_NVRAM_PageSize); i++) {
        uint32_t page_number;
        nvm_get_pagenumber(INT_FLASH, sector + (i * AJ_NVRAM_PageSize), &page_number);
        nvm_page_erase(INT_FLASH, page_number);
    }
}
void _AJ_NVRAM_Clear()
{
    //Erase the first sector starting at the base address
    int i;
    uint8_t* eraseSectorAddr = AJ_NVRAM_BASE_ADDRESS;
    AJ_NV_EraseSector(eraseSectorAddr);
    // Write the sentinel string
    _AJ_NV_Write(AJ_NVRAM_BASE_ADDRESS, "AJNV", 4);
    eraseSectorAddr += AJ_NVRAM_SECTOR_SIZE;
    while (eraseSectorAddr < AJ_NVRAM_END_ADDRESS) {
        AJ_NV_EraseSector(eraseSectorAddr);
        eraseSectorAddr += AJ_NVRAM_SECTOR_SIZE;
    }
}

// Compact the storage by removing invalid entries
AJ_Status _AJ_CompactNVStorage()
{

    uint16_t capacity = 0;
    uint16_t id = 0;
    uint16_t* data = (uint16_t*)(AJ_NVRAM_BASE_ADDRESS + SENTINEL_OFFSET);
    uint16_t entrySize = 0;
    uint16_t offset = 0;
    uint16_t copyBytes = 0;
    uint8_t* buf = (uint8_t*)AJ_NVRAM_COMPACTION_BUF;
    uint8_t* eraseSectorAddr = AJ_NVRAM_BASE_ADDRESS;

    // copy sentinel
    AJ_NV_EraseSector(AJ_NVRAM_COMPACTION_BUF);
    _AJ_NV_Write(AJ_NVRAM_COMPACTION_BUF, "AJNV", SENTINEL_OFFSET);
    offset += SENTINEL_OFFSET;
    extern void AJ_NVRAM_Layout_Print();
    //AJ_NVRAM_Layout_Print();

    while ((uint8_t*)data < (uint8_t*)AJ_NVRAM_END_ADDRESS && (*data != INVALID_DATA)) {
        id = *data;
        capacity = *(data + 1);
        entrySize = ENTRY_HEADER_SIZE + capacity;
        if (id != INVALID_ID) {
            if (offset + entrySize >= AJ_NVRAM_SECTOR_SIZE) {
                uint16_t leftOver = 0;
                copyBytes = AJ_NVRAM_SECTOR_SIZE - offset;
                leftOver = entrySize - copyBytes;
                _AJ_NV_Write(buf + offset, data, copyBytes);
                AJ_NV_EraseSector(eraseSectorAddr);
                _AJ_NV_Write(eraseSectorAddr, buf, AJ_NVRAM_SECTOR_SIZE);
                AJ_NV_EraseSector(AJ_NVRAM_COMPACTION_BUF);
                eraseSectorAddr += AJ_NVRAM_SECTOR_SIZE;
                offset = 0;

                if (leftOver > 0) {
                    _AJ_NV_Write((uint8_t*)data + copyBytes, buf + offset, leftOver);
                    offset = leftOver;
                }
            } else {
                _AJ_NV_Write(buf + offset, data, entrySize);
                offset += entrySize;
            }
        }
        data += entrySize >> 1;
    }
    if (offset > 0) {
        AJ_NV_EraseSector(eraseSectorAddr);
        _AJ_NV_Write(eraseSectorAddr, buf, offset);
        eraseSectorAddr += AJ_NVRAM_SECTOR_SIZE;
    }

    while (eraseSectorAddr < AJ_NVRAM_END_ADDRESS) {
        AJ_NV_EraseSector(eraseSectorAddr);
        eraseSectorAddr += AJ_NVRAM_SECTOR_SIZE;
    }

    //AJ_NVRAM_Layout_Print();
    return AJ_OK;

}

/*
 * Initialize functions for DUE
 * (Called by AJ_PlatformInit())
 */
void _AJ_PlatformInit(void)
{
    /*
     * Init sequence for the DUE
     */
    const usart_serial_options_t usart_serial_options = {
        .baudrate   = 115200,
        .charlength = 0,
        .paritytype = UART_MR_PAR_NO,
        .stopbits   = false
    };
    sysclk_init();
    board_init();
    //configure_uart();
    stdio_serial_init(CONSOLE_UART, &usart_serial_options);
    AJ_WSL_ModuleInit();
    AJ_InitRNG();
}

uint16_t AJ_ByteSwap16(uint16_t x)
{
    return swap16(x);
}

uint32_t AJ_ByteSwap32(uint32_t x)
{
    return swap32(x);
}

uint64_t AJ_ByteSwap64(uint64_t x)
{
    return swap64(x);
}
void _AJ_Reboot(void)
{
    rstc_start_software_reset(RSTC);
}
