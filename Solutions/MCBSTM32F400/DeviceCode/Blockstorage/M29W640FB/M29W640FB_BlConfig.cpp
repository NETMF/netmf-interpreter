////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for MCBSTM32F400 board
//
//  *** Block Storage Configuration for External M29W640FB NOR Flash ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#define FLASH_BYTES_PER_SECTOR               2
#define FLASH_SECTOR_WRITE_TYPICAL_TIME_USEC 1400    // not used
#define FLASH_BLOCK_ERASE_ACTUAL_TIME_USEC   800000  // not used
#define FLASH_BLOCK_ERASE_MAX_TIME_USEC      6000000 // not used

#define FLASH_ADDRESS1                       0x60000000
#define FLASH_BLOCK1_COUNT                   8
#define FLASH_BLOCK1_BYTES_PER_BLOCK         8192

#define FLASH_ADDRESS2                       0x60010000
#define FLASH_BLOCK2_COUNT                   127
#define FLASH_BLOCK2_BYTES_PER_BLOCK         65536

// EBIU Information

#define M29W640FB__SIZE_IN_BYTES     0x00800000      // 8MB
#define M29W640FB__WP_GPIO_PIN       GPIO_PIN_NONE
#define M29W640FB__WP_ACTIVE         FALSE

// BlockDeviceInformation

#define M29W640FB__IS_REMOVABLE      FALSE
#define M29W640FB__SUPPORTS_XIP      TRUE
#define M29W640FB__WRITE_PROTECTED   FALSE
#define M29W640FB__SUPP_COPY_BACK    FALSE
#define M29W640FB__NUM_REGIONS       2


const BlockRange g_M29W640FB_BlockRange1[] =
{
    // ER_CONFIG
    { BlockRange::BLOCKTYPE_CONFIG,   0,  7 },        // 8x8KB=64KB
};

const BlockRange g_M29W640FB_BlockRange2[] =
{
    // ER_DAT
    { BlockRange::BLOCKTYPE_CODE,   0,  3 },          // 4x64KB = 256KB
#ifdef DEBUG
    // In debug builds with TRACE pins enabled, only 1MB is available
    { BlockRange::BLOCKTYPE_DEPLOYMENT,   4,  14 },   // 11x64KB = 704KB
#else
    { BlockRange::BLOCKTYPE_DEPLOYMENT,   4,  126 },  // 123x64KB = 7872KB
#endif
};


const BlockRegionInfo  g_M29W640FB_BlkRegion[M29W640FB__NUM_REGIONS] = 
{
    {
        FLASH_ADDRESS1,                 // ByteAddress  Start;         // Starting Sector address
        FLASH_BLOCK1_COUNT,             // UINT32       NumBlocks;     // total number of blocks in this region
        FLASH_BLOCK1_BYTES_PER_BLOCK,   // UINT32       BytesPerBlock; // Total number of bytes per block
        ARRAYSIZE_CONST_EXPR(g_M29W640FB_BlockRange1),
        g_M29W640FB_BlockRange1,
    },
    {
        FLASH_ADDRESS2,                 // ByteAddress  Start;         // Starting Sector address
        FLASH_BLOCK2_COUNT,             // UINT32       NumBlocks;     // total number of blocks in this region
        FLASH_BLOCK2_BYTES_PER_BLOCK,   // UINT32       BytesPerBlock; // Total number of bytes per block
        ARRAYSIZE_CONST_EXPR(g_M29W640FB_BlockRange2),
        g_M29W640FB_BlockRange2,
    }
};

const BlockDeviceInfo g_M29W640FB_DeviceInfo=
{
    {  
        M29W640FB__IS_REMOVABLE,           // BOOL Removable;
        M29W640FB__SUPPORTS_XIP,           // BOOL SupportsXIP;
        M29W640FB__WRITE_PROTECTED,        // BOOL WriteProtected;
        M29W640FB__SUPP_COPY_BACK          // BOOL SupportsCopyBack
    },
    FLASH_SECTOR_WRITE_TYPICAL_TIME_USEC,  // UINT32 MaxSectorWrite_uSec;
    FLASH_BLOCK_ERASE_ACTUAL_TIME_USEC,    // UINT32 MaxBlockErase_uSec;
    FLASH_BYTES_PER_SECTOR,                // UINT32 BytesPerSector;     

    M29W640FB__SIZE_IN_BYTES,              // UINT32 Size;

    M29W640FB__NUM_REGIONS,                // UINT32 NumRegions;
    g_M29W640FB_BlkRegion,                 // const BlockRegionInfo* pRegions;
};


struct BLOCK_CONFIG g_M29W640FB_BS_Config =
{
    {
        M29W640FB__WP_GPIO_PIN,           // GPIO_PIN             Pin;
        M29W640FB__WP_ACTIVE,             // BOOL                 ActiveState;
    },

    &g_M29W640FB_DeviceInfo,              // BlockDeviceinfo
};

struct BlockStorageDevice g_M29W640FB_BS;

