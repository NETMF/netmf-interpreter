////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for MCBSTM32F400 board
//
//  *** M29W640FB Flash Driver ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <stm32f4xx.h>

#include "M29W640FB_Flash.h"
#include "Driver_Flash.h"

extern ARM_DRIVER_FLASH Driver_Flash0;
    
BOOL M29W640FB_WaitReady()
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();
    
    while(Driver_Flash0.GetStatus().busy == 1);
    
    return TRUE;
}

//--//

/////////////////////////////////////////////////////////
// Description:
//    Initializes a given block device for use
// 
// Input:
//
// Returns:
//   true if successful; false if not
//
// Remarks:
//    No other functions in this interface may be called
//    until after Init returns.
//    This initialization function assumes that the GPIOs and FSMC are already inited

BOOL M29W640FB_Flash_Driver::ChipInitialize( void* context )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();
        
    if(Driver_Flash0.Initialize(NULL) != ARM_DRIVER_OK)
    {
        return FALSE;
    }

    return TRUE;
}

/////////////////////////////////////////////////////////
// Description:
//    Initializes a given block device for use
// 
// Returns:
//   true if succesful; false if not
//
// Remarks:
//   De initializes the device when no longer needed
//
BOOL M29W640FB_Flash_Driver::ChipUnInitialize( void* context )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    return TRUE;
}

/////////////////////////////////////////////////////////
// Description:
//    Gets the information describing the device
//
const BlockDeviceInfo* M29W640FB_Flash_Driver::GetDeviceInfo( void* context )
{
    BLOCK_CONFIG* config = (BLOCK_CONFIG*)context;
    
    return config->BlockDeviceInformation;    
}

/////////////////////////////////////////////////////////
// Description:
//    Reads data from a set of sectors
//
// Input:
//    StartSector - Starting Sector for the read
//    NumSectors  - Number of sectors to read
//    pSectorBuff - pointer to buffer to read the data into.
//                  Must be large enough to hold all of the data
//                  being read.
//
// Returns:
//   true if successful; false if not
//
// Remarks:
//   This function reads the number of sectors specified from the device.
//   
BOOL  M29W640FB_Flash_Driver::Read( void* context, ByteAddress StartSector, UINT32 NumBytes, BYTE * pSectorBuff)
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    int32_t wordsReadCount;
    uint32_t NumWords = NumBytes / 2;

    M29W640FB_WaitReady();
    wordsReadCount = Driver_Flash0.ReadData(StartSector, pSectorBuff, NumWords);	
    if(wordsReadCount != NumWords)
    {
        return FALSE;
    }

    return TRUE;
}


/////////////////////////////////////////////////////////
// Description:
//    Writes data to a set of sectors
//
// Input:
//    StartSector - Starting Sector for the write
//    NumSectors  - Number of sectors to write
//    pSectorBuff - pointer to data to write.
//                  Must be large enough to hold complete sectors
//                  for the number of sectors being written.
//
// Returns:
//   true if successful; false if not
//
// Remarks:
//   This function reads the number of sectors specified from the device.
//   
BOOL M29W640FB_Flash_Driver::Write(void* context, ByteAddress Address, UINT32 NumBytes, BYTE * pSectorBuff, BOOL ReadModifyWrite)
{
    NATIVE_PROFILE_PAL_FLASH();
    int32_t wordsWrittenCount;
    uint32_t NumWords = NumBytes / 2;
        
    // Read-modify-write is used for FAT filesystems only
    if (ReadModifyWrite)
    {
        return FALSE;
    }
    
    M29W640FB_WaitReady();
    wordsWrittenCount = Driver_Flash0.ProgramData(Address, pSectorBuff, NumWords);
    if(wordsWrittenCount != NumWords)
    {
        return FALSE;
    }
    M29W640FB_WaitReady();
    return TRUE;
}

BOOL M29W640FB_Flash_Driver::Memset(void* context, ByteAddress Address, UINT8 Data, UINT32 NumBytes)
{
    NATIVE_PROFILE_PAL_FLASH();

    // used for FAT filesystems only
    return FALSE;
}


BOOL M29W640FB_Flash_Driver::GetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
{
    // no metadata
    return FALSE;
}

BOOL M29W640FB_Flash_Driver::SetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
{
    // no metadata
    return FALSE;
}


/////////////////////////////////////////////////////////
// Description:
//    Check a block is erased or not.
// 
// Input:
//    BlockStartAddress - Logical Sector Address
//
// Returns:
//   true if it is erased, otherwise false
//
// Remarks:
//    Check  the block containing the sector address specified.
//    
BOOL M29W640FB_Flash_Driver::IsBlockErased( void* context, ByteAddress BlockStart, UINT32 BlockLength )
{
    const uint32_t bufLenWords = 128;
    const uint32_t bufLenBytes = bufLenWords * 2;
    uint16_t readBuffer[bufLenWords];
    uint32_t wordsReadCount;
    uint32_t numWords;
    uint32_t currentAddress = BlockStart;
    uint32_t endAddress = BlockStart + BlockLength;
    
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    while(currentAddress < endAddress)
    {
        /* The number of bytes to read will be equal to readBufLen unless the BlockLength is not a even multiple of readBufLen */
        if((endAddress - currentAddress) < bufLenBytes)
        {
            numWords = (endAddress - currentAddress) / 2;
        }
        else
        {
            numWords = bufLenWords;
        }
        
        /* Read data from flash */
        M29W640FB_WaitReady();
        wordsReadCount = Driver_Flash0.ReadData(currentAddress, readBuffer, numWords);
        if(wordsReadCount != numWords)
        {
            return FALSE;
        }
        
        currentAddress += wordsReadCount * 2;
        
        /* Validate the data we get back is all erased to 0xFF */
        for (int i = 0; i < wordsReadCount; i++) {
            if (readBuffer[i] != 0xFFFF)
            {
                return FALSE;
            }
        }
    }

    return TRUE;
}


/////////////////////////////////////////////////////////
// Description:
//    Erases a block
// 
// Input:
//    Address - Logical Sector Address
//
// Returns:
//   true if succesful; false if not
//
// Remarks:
//    Erases the block containing the sector address specified.
//    
BOOL M29W640FB_Flash_Driver::EraseBlock( void* context, ByteAddress Sector )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    M29W640FB_WaitReady();
    Driver_Flash0.EraseSector(Sector);
    M29W640FB_WaitReady();

    return TRUE;
}



/////////////////////////////////////////////////////////
// Description:
//   Changes the power state of the device
// 
// Input:
//    State   - true= power on; false = power off
//
// Remarks:
//   This function allows systems to conserve power by 
//   shutting down the hardware when the system is 
//   going into low power states.
//
void  M29W640FB_Flash_Driver::SetPowerState( void* context, UINT32 State )
{
    // no settable power state
}


//--// ---------------------------------------------------

UINT32 M29W640FB_Flash_Driver::MaxSectorWrite_uSec( void* context )
{
    NATIVE_PROFILE_PAL_FLASH();

    BLOCK_CONFIG* config = (BLOCK_CONFIG*)context;
    
    return config->BlockDeviceInformation->MaxSectorWrite_uSec;
}


UINT32 M29W640FB_Flash_Driver::MaxBlockErase_uSec( void* context )
{
    NATIVE_PROFILE_PAL_FLASH();
    
    BLOCK_CONFIG* config = (BLOCK_CONFIG*)context;

    return config->BlockDeviceInformation->MaxBlockErase_uSec;
    
}

struct IBlockStorageDevice g_M29W640FB_Flash_DeviceTable = 
{                          
    &M29W640FB_Flash_Driver::ChipInitialize,
    &M29W640FB_Flash_Driver::ChipUnInitialize,
    &M29W640FB_Flash_Driver::GetDeviceInfo,
    &M29W640FB_Flash_Driver::Read,
    &M29W640FB_Flash_Driver::Write,
    &M29W640FB_Flash_Driver::Memset,    
    &M29W640FB_Flash_Driver::GetSectorMetadata,
    &M29W640FB_Flash_Driver::SetSectorMetadata,
    &M29W640FB_Flash_Driver::IsBlockErased,
    &M29W640FB_Flash_Driver::EraseBlock,
    &M29W640FB_Flash_Driver::SetPowerState,
    &M29W640FB_Flash_Driver::MaxSectorWrite_uSec,
    &M29W640FB_Flash_Driver::MaxBlockErase_uSec,    
};
