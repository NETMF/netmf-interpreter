////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//

#define SD_BLOCK_ERASE_TYPICAL_TIME_USEC     1500000
#define SD_SECTOR_WRITE_TYPICAL_TIME_USEC    200
#define SD_BLOCK_ERASE_MAX_TIME_USEC         2000000
#define SD_SECTOR_WRITE_MAX_TIME_USEC        700

#define SD_WP_GPIO_PIN                     GPIO_PIN_NONE
#define SD_WP_ACTIVE                       FALSE


// BlockDeviceInformation

#define SD_REMOVEABLE                      TRUE
#define SD_SUPPORTXIP                      FALSE
#define SD_WRITEPROTECTED                  FALSE
#define SD_NUMREGIONS                      1


//--//

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata = "g_SD_BS_Config"
#endif

BlockRange g_SD_BlockStatus[] =
{
    { BlockRange::BLOCKTYPE_FILESYSTEM, 0, 0 }
};


BlockRegionInfo  g_SD_BlkRegion[SD_NUMREGIONS] = 
{
    0,         // ByteAddress     Start;              // Starting Sector address
    0,         // UINT32          NumBlocks;          // total number of blocks in this region
    0,         // UINT32          BytesPerBlock;      // Total number of bytes per block (MUST be SectorsPerBlock * DataBytesPerSector)

    ARRAYSIZE_CONST_EXPR(g_SD_BlockStatus),
    g_SD_BlockStatus,
};

//--//

BlockDeviceInfo  g_SD_DeviceInfo=
{
    {  
        SD_REMOVEABLE,
        SD_SUPPORTXIP,
        SD_WRITEPROTECTED,
    },
    SD_SECTOR_WRITE_MAX_TIME_USEC,       // UINT32 Duration_Max_WordWrite_uSec;
    SD_BLOCK_ERASE_MAX_TIME_USEC,        // UINT32 Duration_Max_SectorErase_uSec;
    0,                                   // BytesPerSector; // Bytes Per Sector

    0,                                   // UINT32 Size;

    SD_NUMREGIONS,                     // UINT32 NumRegions;
    g_SD_BlkRegion,                    //const BlockRegionInfo* pRegions;
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata
#endif

struct SD_BLOCK_CONFIG g_SD_BS_Config =
{
    {
        SD_WP_GPIO_PIN,            // GPIO_PIN             Pin;
        SD_WP_ACTIVE,              // BOOL                 ActiveState;
    },

     &g_SD_DeviceInfo,             // BlockDeviceinfo
};

//--//
#define SD_CS                    GPIO_PIN_NONE
#define SD_CS_ACTIVE             FALSE
#define SD_MSK_IDLE              TRUE
#define SD_MSK_SAMPLE_EDGE       FALSE
#define SD_16BIT_OP              FALSE
#define SD_CLOCK_RATE_KHZ        400
#define SD_CS_SETUP_USEC         0
#define SD_CS_HOLD_USEC          0
#define SD_MODULE                0
#define SD_INSERT_ISR_PIN        GPIO_PIN_NONE
#define SD_EJECT_ISR_PIN         GPIO_PIN_NONE
#define SD_LOW_VOLTAGE_FLAG      FALSE

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata = "g_SD_BS"
#endif

struct BlockStorageDevice g_SD_BS;

struct SD_BL_CONFIGURATION g_SD_BL_Config =
{      
    {// SPI_CONFIGURATION
        SD_CS,
        SD_CS_ACTIVE,
        SD_MSK_IDLE,
        SD_MSK_SAMPLE_EDGE,
        SD_16BIT_OP,
        SD_CLOCK_RATE_KHZ,
        SD_CS_SETUP_USEC,
        SD_CS_HOLD_USEC,
        SD_MODULE,
    },
    
    SD_INSERT_ISR_PIN,
    SD_EJECT_ISR_PIN,
    TRUE,                   // State_After_Erase will be filled at initialization
    SD_LOW_VOLTAGE_FLAG,
    
    &g_SD_BS,
};


#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata 
#endif 

//--//

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata = "g_SD_BS_Driver"
#endif

struct SD_BS_Driver g_SD_BS_Driver;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata 
#endif 

//--//

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata = "g_SD_BS_DeviceTable"
#endif

struct IBlockStorageDevice g_SD_BS_DeviceTable = 
{
    &SD_BS_Driver::ChipInitialize, 
    &SD_BS_Driver::ChipUnInitialize, 
    &SD_BS_Driver::GetDeviceInfo, 
    &SD_BS_Driver::Read, 
    &SD_BS_Driver::Write,
    &SD_BS_Driver::Memset,
    &SD_BS_Driver::GetSectorMetadata,
    &SD_BS_Driver::SetSectorMetadata,
    &SD_BS_Driver::IsBlockErased, 
    &SD_BS_Driver::EraseBlock, 
    &SD_BS_Driver::SetPowerState, 
    &SD_BS_Driver::MaxSectorWrite_uSec, 
    &SD_BS_Driver::MaxBlockErase_uSec, 
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata 
#endif 
