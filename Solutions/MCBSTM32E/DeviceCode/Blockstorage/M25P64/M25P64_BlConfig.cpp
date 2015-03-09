////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for MCBSTM32E board (STM32): Copyright (c) Oberon microsystems, Inc.
//
//  *** Block Storage Configuration for External M25P64 Serial Flash ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//


#define FLASH_BLOCK_COUNT                    128
#define FLASH_BYTES_PER_BLOCK                65536
#define FLASH_BYTES_PER_SECTOR               256
#define FLASH_SECTOR_WRITE_TYPICAL_TIME_USEC 1400    // not used
#define FLASH_BLOCK_ERASE_ACTUAL_TIME_USEC   1000000 // not used
#define FLASH_BLOCK_ERASE_MAX_TIME_USEC      3000000 // not used

//--//

// EBIU Information

#define M25P64__SIZE_IN_BYTES     FLASH_BLOCK_COUNT * FLASH_BYTES_PER_BLOCK
#define M25P64__WP_GPIO_PIN       GPIO_PIN_NONE
#define M25P64__WP_ACTIVE         FALSE


//--//


// BlockDeviceInformation

#define M25P64__IS_REMOVABLE      FALSE
#define M25P64__SUPPORTS_XIP      FALSE
#define M25P64__WRITE_PROTECTED   FALSE
#define M25P64__SUPP_COPY_BACK    FALSE
#define M25P64__NUM_REGIONS       1


//--//

const BlockRange g_M25P64_BlockRange[] =
{
    { BlockRange::BLOCKTYPE_DEPLOYMENT,   0,  63 }, // 4M
    { BlockRange::BLOCKTYPE_FILESYSTEM,  64, 127 }  // 4M
};


const BlockRegionInfo  g_M25P64_BlkRegion[M25P64__NUM_REGIONS] = 
{
    {
        0,                     // ByteAddress  Start;         // Starting Sector address
        FLASH_BLOCK_COUNT,     // UINT32       NumBlocks;     // total number of blocks in this region
        FLASH_BYTES_PER_BLOCK, // UINT32       BytesPerBlock; // Total number of bytes per block
        ARRAYSIZE_CONST_EXPR(g_M25P64_BlockRange),
        g_M25P64_BlockRange,
    }
};


//--//


const BlockDeviceInfo g_M25P64_DeviceInfo=
{
    {  
        M25P64__IS_REMOVABLE,              // BOOL Removable;
        M25P64__SUPPORTS_XIP,              // BOOL SupportsXIP;
        M25P64__WRITE_PROTECTED,           // BOOL WriteProtected;
        M25P64__SUPP_COPY_BACK             // BOOL SupportsCopyBack
    },
    FLASH_SECTOR_WRITE_TYPICAL_TIME_USEC,  // UINT32 MaxSectorWrite_uSec;
    FLASH_BLOCK_ERASE_ACTUAL_TIME_USEC,    // UINT32 MaxBlockErase_uSec;
    FLASH_BYTES_PER_SECTOR,                // UINT32 BytesPerSector;     

    FLASH_MEMORY_Size,                     // UINT32 Size;

    M25P64__NUM_REGIONS,                   // UINT32 NumRegions;
    g_M25P64_BlkRegion,                    // const BlockRegionInfo* pRegions;
};


struct BLOCK_CONFIG g_M25P64_BS_Config =
{
    {
        M25P64__WP_GPIO_PIN,           // GPIO_PIN             Pin;
        M25P64__WP_ACTIVE,             // BOOL                 ActiveState;
    },

    &g_M25P64_DeviceInfo,              // BlockDeviceinfo
};

//--//


#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata = "g_M25P64_BS"
#endif

struct BlockStorageDevice g_M25P64_BS;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata 
#endif 

//--//

