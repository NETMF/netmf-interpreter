////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for MCBSTM32E board (STM32): Copyright (c) Oberon microsystems, Inc.
//
//  *** Block Storage Configuration ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//


#define FLASH_MANUFACTURER_CODE              0x0000  // not used
#define FLASH_DEVICE_CODE                    0x0000  // not used
#define FLASH_MEMORY_BASE                    0x08000000
#define FLASH_BLOCK_COUNT                    256
#define FLASH_BYTES_PER_BLOCK                2048
#define FLASH_BYTES_PER_SECTOR               2
#define FLASH_BLOCK_ERASE_TYPICAL_TIME_USEC  1000000 // not used
#define FLASH_SECTOR_WRITE_TYPICAL_TIME_USEC 10      // not used
#define FLASH_BLOCK_ERASE_MAX_TIME_USEC      4000000 // not used
#define FLASH_SECTOR_WRITE_MAX_TIME_USEC     100     // not used
#define FLASH_BLOCK_ERASE_ACTUAL_TIME_USEC   10000   // not used

//--//

// EBIU Information

#define STM32__CHIP_SELECT       0
#define STM32__WAIT_STATES       5 // not used
#define STM32__RELEASE_COUNTS    0
#define STM32__BIT_WIDTH         16
#define STM32__BASE_ADDRESS      FLASH_MEMORY_BASE
#define STM32__SIZE_IN_BYTES     FLASH_BLOCK_COUNT * FLASH_BYTES_PER_BLOCK
#define STM32__WP_GPIO_PIN       GPIO_PIN_NONE
#define STM32__WP_ACTIVE         FALSE



//--//



// BlockDeviceInformation

#define STM32__IS_REMOVABLE      FALSE
#define STM32__SUPPORTS_XIP      TRUE
#define STM32__WRITE_PROTECTED   FALSE
#define STM32__SUPP_COPY_BACK    FALSE
#define STM32__NUM_REGIONS       1


//--//


const BlockRange g_STM32_BlockRange[] =
{
    { BlockRange::BLOCKTYPE_BOOTSTRAP ,    0,  21 },  // 08000000 bootloader   44k
    { BlockRange::BLOCKTYPE_CODE      ,   22, 191 },  // 0800B000 CLR         340k
    { BlockRange::BLOCKTYPE_CONFIG    ,  192, 195 },  // 08060000 config        8k
    { BlockRange::BLOCKTYPE_STORAGE_A ,  196, 199 },  // 08062000 storage A     8k
    { BlockRange::BLOCKTYPE_STORAGE_B ,  200, 203 },  // 08064000 storage B     8k
    { BlockRange::BLOCKTYPE_CODE      ,  204, 255 }   // 08066000 unused      104k
};


const BlockRegionInfo  g_STM32_BlkRegion[STM32__NUM_REGIONS] = 
{
    {
        STM32__BASE_ADDRESS,    // ByteAddress   Start;           // Starting Sector address
        FLASH_BLOCK_COUNT,      // UINT32        NumBlocks;       // total number of blocks in this region
        FLASH_BYTES_PER_BLOCK,  // UINT32        BytesPerBlock;   // Total number of bytes per block
        ARRAYSIZE_CONST_EXPR(g_STM32_BlockRange),
        g_STM32_BlockRange,
    }
};


//--//

#undef MEMORY_BLOCKTYPE_GCC_SPECIAL_CODE 

//--//


const BlockDeviceInfo g_STM32_DeviceInfo=
{
    {  
        STM32__IS_REMOVABLE,               // BOOL Removable;
        STM32__SUPPORTS_XIP,               // BOOL SupportsXIP;
        STM32__WRITE_PROTECTED,            // BOOL WriteProtected;
        STM32__SUPP_COPY_BACK              // BOOL SupportsCopyBack
    },
    FLASH_SECTOR_WRITE_TYPICAL_TIME_USEC,  // UINT32 MaxSectorWrite_uSec;
    FLASH_BLOCK_ERASE_ACTUAL_TIME_USEC,    // UINT32 MaxBlockErase_uSec;
    FLASH_BYTES_PER_SECTOR,                // UINT32 BytesPerSector;     

    FLASH_MEMORY_Size,                     // UINT32 Size;

    STM32__NUM_REGIONS,                    // UINT32 NumRegions;
    g_STM32_BlkRegion,                     // const BlockRegionInfo* pRegions;
};


struct MEMORY_MAPPED_NOR_BLOCK_CONFIG g_STM32_BS_Config =
{
    { // BLOCK_CONFIG
        {
            STM32__WP_GPIO_PIN,            // GPIO_PIN             Pin;
            STM32__WP_ACTIVE,              // BOOL                 ActiveState;
        },

        &g_STM32_DeviceInfo,               // BlockDeviceinfo
    },

    { // CPU_MEMORY_CONFIG
        STM32__CHIP_SELECT,                // UINT8  CPU_MEMORY_CONFIG::ChipSelect;
        TRUE,                              // UINT8  CPU_MEMORY_CONFIG::ReadOnly;
        STM32__WAIT_STATES,                // UINT32 CPU_MEMORY_CONFIG::WaitStates;
        STM32__RELEASE_COUNTS,             // UINT32 CPU_MEMORY_CONFIG::ReleaseCounts;
        STM32__BIT_WIDTH,                  // UINT32 CPU_MEMORY_CONFIG::BitWidth;
        STM32__BASE_ADDRESS,               // UINT32 CPU_MEMORY_CONFIG::BaseAddress;
        STM32__SIZE_IN_BYTES,              // UINT32 CPU_MEMORY_CONFIG::SizeInBytes;
        0,                                 // UINT8  CPU_MEMORY_CONFIG::XREADYEnable 
        0,                                 // UINT8  CPU_MEMORY_CONFIG::ByteSignalsForRead 
        0,                                 // UINT8  CPU_MEMORY_CONFIG::ExternalBufferEnable
    },

    0,                                     // UINT32 ChipProtection;
    FLASH_MANUFACTURER_CODE,               // UINT32 ManufacturerCode;
    FLASH_DEVICE_CODE,                     // UINT32 DeviceCode;
};

//--//


#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata = "g_STM32_BS"
#endif

struct BlockStorageDevice g_STM32_BS;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata 
#endif 

//--//

