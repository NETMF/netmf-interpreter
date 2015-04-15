/**
 * @file
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
 * This code statically links to code available from
 * http://www.st.com/web/en/catalog/tools/ and that code is subject to a license
 * agreement with terms and conditions that you will be responsible for from
 * STMicroelectronics if you employ that code. Use of such code is your responsibility.
 * Neither AllSeen Alliance nor any contributor to this AllSeen code base has any
 * obligations with respect to the STMicroelectronics code that to which you will be
 * statically linking this code. One requirement in the license is that the
 * STMicroelectronics code may only be used with STMicroelectronics processors as set
 * forth in their agreement."
 *******************************************************************************/

#define AJ_MODULE STM_NVRAM

#include "aj_nvram.h"
#include "aj_target_platform.h"
#include "aj_debug.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgSTM_NVRAM = 0;
#endif

/*
 * Identifies an AJ NVRAM block
 */
static const char AJ_NV_SENTINEL[4] = "AJNV";

#define INVALID_ID (0)
#define INVALID_DATA (0xFFFF)
#define INVALID_DATA_BYTE (0xFF)
#define AJ_NVRAM_SECTOR_SIZE (4 * 1024)

uint8_t* const AJ_NV_SECTOR0    = (uint8_t*)(0x080C0000);
uint8_t* const AJ_NV_SECTOR1    = (uint8_t*)(0x080E0000);
uint8_t* AJ_NVRAM_BASE_ADDRESS; //  = AJ_NV_SECTOR0;
uint8_t* AJS_BASE_ADDRESS       = (uint8_t*)(0x080E0000);
/*
 * How much space in flash we request to allocate for NVRAM. This will be rounded up to a whole
 * number of sectors so the actual space allocated may be larger.
 */
#define AJ_NVRAM_REQUESTED  AJ_NVRAM_SIZE

typedef struct _NV_EntryHeader {
    uint16_t id;
    uint16_t sz;
} NV_EntryHeader;

#define ALIGN_SIZE(x) ((x + 7) & 0xFFF8)


static void NV_ErasePartition(uint32_t offset);

void AJ_NVRAM_Layout_Print(void);

void* AJ_Flash_GetBaseAddr();

void _AJ_NVRAM_Clear(void);

/*
 * Sector size
 */
static uint32_t NV_SectorSize = AJ_NVRAM_SIZE;
/*
 * Size of each of the two partitions
 */
static uint32_t NV_PartitionSize = AJ_NVRAM_SIZE;
/*
 * Base offset for active parition
 */
static uint32_t NV_Active;
/*
 * Base offset for backup parition
 */
static uint32_t NV_Backup;

static uint8_t* NV_BaseAddr;

static uint8_t NV_Busy;

static void NV_Dump(uint8_t* base, const char* tag)
{
    uint32_t pos = sizeof(AJ_NV_SENTINEL);

    AJ_InfoPrintf(("============ AJ NVRAM Map %s===========\n", tag));
    AJ_InfoPrintf(("0x%x\n", base));

    while (pos < NV_PartitionSize) {
        NV_EntryHeader* nvEntry = (NV_EntryHeader*)(base + pos);
        if (nvEntry->id == INVALID_DATA) {
            break;
        }
        AJ_InfoPrintf(("ID = %d, capacity = %d addr=0x%x\n", nvEntry->id, nvEntry->sz, nvEntry));
        pos += nvEntry->sz + sizeof(NV_EntryHeader);
    }
    AJ_InfoPrintf(("============ End ===========\n"));
}

/*
 * Swap out the active and passive partitions
 */
static void NV_SwapPartitions()
{
    uint32_t tmp = NV_Active;
    NV_Active = NV_Backup;
    NV_Backup = tmp;
}

/*
 * Read from the active NVRAM section.
 *
 * @param dest  An offset relative to the start of the active NVRAM section
 * @param data  Buffer to receive the data
 * @param size  The size of the data to read
 */
int32_t _AJ_NV_Read(uint32_t src, void* buf, uint16_t size)
{
    //uint8_t* base = NV_BaseAddr + NV_Active + src;
    uint8_t* base = src;
    memcpy(buf, base, size);
    return size;
}

/*
 * Write to the active NVRAM section.
 *
 * @param dest  An offset relative to the start of the active NVRAM section
 * @param data  The data to write
 * @param size  The size of the data to write
 */
int32_t _AJ_NV_Write(uint32_t dest, void* data, uint16_t size)
{
    int i;
    FLASH_Unlock();
    FLASH_SetLatency(FLASH_Latency_7);
    FLASH_ClearFlag(FLASH_FLAG_BSY | FLASH_FLAG_EOP | FLASH_FLAG_OPERR | FLASH_FLAG_WRPERR |
                    FLASH_FLAG_PGAERR | FLASH_FLAG_PGPERR | FLASH_FLAG_PGSERR);
    for (i = 0; i < size; i++) {
        FLASH_ProgramByte(dest + i, (uint8_t)*((uint8_t*)data + i));
    }
    return AJ_OK;
}
AJ_Status _AJ_CompactNVStorage(void) {

    AJ_Status status;
    uint8_t* srcPos = sizeof(AJ_NV_SENTINEL);
    uint8_t* destPos = NV_Backup + sizeof(AJ_NV_SENTINEL);
    uint32_t swap;
    /*
     * First erase the new partition that we are compacting into
     */
    if (NV_Active > 0) {
        // We're in the second partition so erase the first
        FLASH_EraseSector(FLASH_Sector_10, VoltageRange_4);
    } else {
        // We're in the first partition so erase the second
        FLASH_EraseSector(FLASH_Sector_11, VoltageRange_4);
    }

    while (srcPos < NV_SectorSize) {
        uint16_t sz;
        NV_EntryHeader nvEntry;
        _AJ_NV_Read((uint32_t)AJ_NVRAM_BASE_ADDRESS + (uint32_t)srcPos, &nvEntry, sizeof(nvEntry));
        if (nvEntry.sz == INVALID_DATA) {
            break;
        }
        sz = nvEntry.sz + sizeof(nvEntry);
        if (nvEntry.id != INVALID_DATA) {
            _AJ_NV_Write((uint32_t)NV_BaseAddr + (uint32_t)destPos, (uint32_t)AJ_NVRAM_BASE_ADDRESS + (uint32_t)srcPos, sz);
            destPos += sz;
        }
        srcPos += sz;
    }
    if (NV_Active > 0) {
        // We copied from sector 1 so erase it
        FLASH_EraseSector(FLASH_Sector_11, VoltageRange_4);
        // Set the base to sector 0
        AJ_NVRAM_BASE_ADDRESS = AJ_NV_SECTOR0;
    } else {
        // We copied from sector 0 so erase it
        FLASH_EraseSector(FLASH_Sector_10, VoltageRange_4);
        // Set the base to sector 1
        AJ_NVRAM_BASE_ADDRESS = AJ_NV_SECTOR1;
    }
    swap = NV_Backup;
    NV_Backup = NV_Active;
    NV_Active = swap;
    _AJ_NV_Write(AJ_NVRAM_BASE_ADDRESS, AJ_NV_SENTINEL, sizeof(AJ_NV_SENTINEL));

}

void _AJ_NVRAM_Clear(void)
{
    FLASH_EraseSector(FLASH_Sector_10, VoltageRange_1);
    FLASH_EraseSector(FLASH_Sector_11, VoltageRange_1);
    _AJ_NV_Write((uint32_t)NV_BaseAddr + NV_Active, "AJNV", 4);
}


void AJ_NVRAM_Init()
{
    FLASH_Unlock();
    FLASH_SetLatency(FLASH_Latency_7);
    FLASH_ClearFlag(FLASH_FLAG_BSY | FLASH_FLAG_EOP | FLASH_FLAG_OPERR | FLASH_FLAG_WRPERR |
                    FLASH_FLAG_PGAERR | FLASH_FLAG_PGPERR | FLASH_FLAG_PGSERR);

    // Base address will always be sector 0
    NV_BaseAddr = AJ_NV_SECTOR0;

    if (*((uint32_t*)AJ_NV_SECTOR0) == *((uint32_t*)AJ_NV_SENTINEL)) {
        // NVRAM is in sector 0. Set the active partition offset to zero
        // and the backup to the size which is the distance to sector 1
        AJ_NVRAM_BASE_ADDRESS = AJ_NV_SECTOR0;
        NV_Active = 0;              //base + 0
        NV_Backup = AJ_NVRAM_SIZE;  //base + size

    } else if (*((uint32_t*)AJ_NV_SECTOR1) == *((uint32_t*)AJ_NV_SENTINEL)) {
        // NVRAM is in sector 1. Set the active partition offset to sector 1
        // and the backup to zero (sector 0)
        AJ_NVRAM_BASE_ADDRESS = AJ_NV_SECTOR1;
        NV_Active = AJ_NVRAM_SIZE;  //base + size
        NV_Backup = 0;              //base + 0
    } else {
        // No sentinel so set the base address to sector 0
        AJ_InfoPrintf(("Sentinel has not been set, clearing NVRAM\n"));
        AJ_NVRAM_BASE_ADDRESS = AJ_NV_SECTOR0;
        NV_Active = 0;              //base + 0
        NV_Backup = AJ_NVRAM_SIZE;  //base + size
        _AJ_NVRAM_Clear();
    }
    return;
}

static NV_EntryHeader* NV_FindEntry(uint16_t id, uint8_t* base)
{
    size_t pos = sizeof(AJ_NV_SENTINEL);
    NV_EntryHeader* nvEntry;

    if (base == NULL) {
        base = NV_BaseAddr + NV_Active;
    }
    while (pos < NV_PartitionSize) {
        nvEntry = (NV_EntryHeader*)(base + pos);
        if (nvEntry->id == id) {
            return nvEntry;
        }
        pos += nvEntry->sz + sizeof(NV_EntryHeader);
    }
    return NULL;
}

static NV_EntryHeader* NV_Create(uint8_t* mem, uint16_t id, uint16_t sz)
{
    NV_EntryHeader* entry;

    AJ_ASSERT(sz > 0);

    entry = NV_FindEntry(INVALID_DATA, mem);
    /*
     * Check there is space for this entry
     */
    if (!entry || (sz + sizeof(NV_EntryHeader) > (NV_PartitionSize - ((uint8_t*)entry - mem)))) {
        AJ_ErrPrintf(("Error: Do not have enough NVRAM storage space.\n"));
        return NULL;
    }
    entry->id = id;
    entry->sz = sz;
    return entry;
}

static void DeleteEntry(uint8_t* mem, uint16_t id)
{
    NV_EntryHeader* entry = NV_FindEntry(id, mem);

    if (entry) {
        size_t sz = entry->sz + sizeof(NV_EntryHeader);
        size_t before = (uint8_t*)entry - mem;
        size_t after = NV_PartitionSize - (before + sz);
        /*
         * This should use memmove but memmove is broken
         */
        size_t i;
        uint32_t* dest = (uint32_t*)entry;
        uint32_t* src = dest + sz / 4;
        for (i = 0; i < after / 4; ++i) {
            *dest++ = *src++;
        }
        memset(mem + before + after, INVALID_DATA_BYTE, sz);
    }
}

void* AJ_Flash_GetBaseAddr()
{
    return (uint8_t*)AJS_BASE_ADDRESS;
}

AJ_Status AJ_Flash_Write(uint32_t offset, void* data, size_t len)
{
    return _AJ_NV_Write(AJ_Flash_GetBaseAddr() + offset, data, len);
}
AJ_Status AJ_Flash_Clear()
{
    FLASH_Unlock();
    FLASH_SetLatency(FLASH_Latency_7);
    FLASH_ClearFlag(FLASH_FLAG_BSY | FLASH_FLAG_EOP | FLASH_FLAG_OPERR | FLASH_FLAG_WRPERR |
                    FLASH_FLAG_PGAERR | FLASH_FLAG_PGPERR | FLASH_FLAG_PGSERR);
    FLASH_EraseSector(FLASH_Sector_11, VoltageRange_1);
    return AJ_OK;
}
size_t AJ_Flash_GetSize()
{
    return (size_t)AJ_NVRAM_SIZE;
}

USART_InitTypeDef UartHandle;
/*
 * Function that hooks into printf
 */

int __io_putchar(char c)
{
    while (USART_GetFlagStatus(USART2, USART_FLAG_TXE) == RESET) ;

    if (c == '\n') {
        USART_SendData(USART2, '\r');
        USART_SendData(USART2, '\n');

        return 1;
    }
    USART_SendData(USART2, c);

    return 1;
}
/*
 * SysTick handler for FreeRTOS. This function also increments the STM32
 * HAL layer system tick count
 */
void fill_ccm() __attribute__ ((section(".init")));
void fill_ccm() {
    extern char _eidata, _sccm, _eccm;

    char*data = &_eidata;
    char*ccm = &_sccm;
    while (ccm < &_eccm) {
        *ccm++ = *data++;
    }
}

void _AJ_PlatformInit(void)
{
    GPIO_InitTypeDef USART_GPIO;
    USART_ClockInitTypeDef USART_ClkInit;

    fill_ccm();
    /* Enable GPIOA clock */
    RCC_AHB1PeriphClockCmd(RCC_AHB1Periph_GPIOA, ENABLE);

    /* Enable UART2 clock */
    //RCC_AHB1PeriphClockCmd(RCC_APB1Periph_USART2, ENABLE);
    RCC_APB1PeriphClockCmd(RCC_APB1Periph_USART2, ENABLE);

    /* Enable SPI1 reset state */
    RCC_AHB1PeriphClockCmd(RCC_APB2Periph_SPI1, ENABLE);

    /* Enable GPIOE clock */
    RCC_AHB1PeriphClockCmd(RCC_AHB1Periph_GPIOE, ENABLE);

    /* Enable RNG Clock */
    RCC_AHB2PeriphClockCmd(RCC_AHB2Periph_RNG, ENABLE);

    /* Configure GPIOA pin 2 as UART TX */
    GPIO_PinAFConfig(GPIOA, GPIO_PinSource2, GPIO_AF_USART2);

    USART_Cmd(USART2, ENABLE);

    USART_GPIO.GPIO_Pin = GPIO_Pin_2;
    USART_GPIO.GPIO_Mode = GPIO_Mode_AF;
    USART_GPIO.GPIO_Speed = GPIO_Speed_50MHz;
    USART_GPIO.GPIO_OType = GPIO_OType_PP;
    USART_GPIO.GPIO_PuPd = GPIO_PuPd_UP;

    GPIO_Init(GPIOA, &USART_GPIO);

    /* Initialise USART clock to its default values */
    USART_ClockStructInit(&USART_ClkInit);

    USART_ClockInit(USART2, &USART_ClkInit);

    /* Setup UART */
    UartHandle.USART_BaudRate = 345600;
    UartHandle.USART_HardwareFlowControl = USART_HardwareFlowControl_None;
    UartHandle.USART_Mode = USART_Mode_Rx | USART_Mode_Tx;
    UartHandle.USART_Parity = USART_Parity_No;
    UartHandle.USART_StopBits = USART_StopBits_1;
    UartHandle.USART_WordLength = USART_WordLength_8b;

    USART_Init(USART2, &UartHandle);

    RNG_Cmd(ENABLE);
    return;
}
uint16_t AJ_ByteSwap16(uint16_t x)
{
    return swap16(x);
}

uint32_t swap32(uint32_t x)
{
    return AJ_ByteSwap32(x);
}

uint32_t AJ_ByteSwap32(uint32_t x)
{
    return ((x >> 24) & 0x000000FF) | ((x >> 8) & 0x0000FF00) |
           ((x << 24) & 0xFF000000) | ((x << 8) & 0x00FF0000);
}

uint64_t AJ_ByteSwap64(uint64_t x)
{
    return ((x >> 56) & 0x00000000000000FF) | ((x >> 40) & 0x000000000000FF00) |
           ((x << 56) & 0xFF00000000000000) | ((x << 40) & 0x00FF000000000000) |
           ((x >> 24) & 0x0000000000FF0000) | ((x >>  8) & 0x00000000FF000000) |
           ((x << 24) & 0x0000FF0000000000) | ((x <<  8) & 0x000000FF00000000);
}

uint8_t AJ_SeedRNG(void)
{
    while (RNG_GetFlagStatus(RNG_FLAG_DRDY) == RESET) ;
    return RNG_GetRandomNumber();
}

void AJ_PreSchedulerInit(void)
{
    NVIC_PriorityGroupConfig(NVIC_PriorityGroup_4);
}
void _AJ_Reboot(void)
{
    NVIC_SystemReset();
}
