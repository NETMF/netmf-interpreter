////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for CMSIS: Copyright (c) Eclo Solutions based in original code from Oberon microsystems, Inc.
//
//  *** Flash Driver ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"
#include "Flash.h"

//--//

BOOL __section("SectionForFlashOperations")Flash_Driver::ChipInitialize(void* context)
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();
    return TRUE;
}

BOOL __section("SectionForFlashOperations")Flash_Driver::ChipUnInitialize(void* context)
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();
    return TRUE;
}

const BlockDeviceInfo* __section("SectionForFlashOperations")Flash_Driver::GetDeviceInfo(void* context)
{
    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
    
    return config->BlockConfig.BlockDeviceInformation;    
}
  
BOOL __section("SectionForFlashOperations")Flash_Driver::Read(void* context, ByteAddress startSector, UINT32 numBytes, BYTE * pSectorBuff)
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    if (pSectorBuff == NULL) return FALSE;

    UINT16* address = (UINT16 *)startSector;
    UINT16* endAddress  = (UINT16 *)(startSector + numBytes);
    UINT16 *pBuf        = (UINT16 *)pSectorBuff;

    while(address < endAddress)
    {
        *pBuf++ = *address++;
    }

    return TRUE;
}

BOOL __section("SectionForFlashOperations")Flash_Driver::Write(void* context, ByteAddress address, UINT32 numBytes, BYTE * dataBuffer, BOOL readModifyWrite)
{
    NATIVE_PROFILE_PAL_FLASH();

    // Read-modify-write is used for FAT filesystems only
    if (readModifyWrite) return FALSE;
    
    __IO uint32_t programAddress = address;
    UINT16* bufferPointer = (UINT16*)dataBuffer;

    while(programAddress < (address + numBytes)) 
    {
        if(HAL_FLASH_Program(FLASH_TYPEPROGRAM_HALFWORD, programAddress, *bufferPointer) != HAL_OK)
        {
            debug_printf( "Flash_WriteToSector failure @ 0x%08x\r\n", (UINT32)programAddress);
            return FALSE;
        }
        
        programAddress++;
        bufferPointer++;
    }
    
    return TRUE;
}

BOOL __section("SectionForFlashOperations")Flash_Driver::Memset(void* context, ByteAddress Address, UINT8 Data, UINT32 NumBytes)
{
    NATIVE_PROFILE_PAL_FLASH();

    // used for FAT filesystems only
    return FALSE;
}


BOOL __section("SectionForFlashOperations")Flash_Driver::GetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
{
    // no metadata
    return FALSE;
}

BOOL __section("SectionForFlashOperations")Flash_Driver::SetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
{
    // no metadata
    return FALSE;
}
  
BOOL __section("SectionForFlashOperations")Flash_Driver::IsBlockErased(void* context, ByteAddress blockStart, UINT32 blockLength)
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();
    
    UINT16* address = (UINT16 *) blockStart;
    UINT16* endAddress  = (UINT16 *)(blockStart + blockLength);
    
    while(address < endAddress)
    {
        if(*address != (UINT16)-1)
        {
            return FALSE;
        }
        address++;
    }

    return TRUE;
}

BOOL __section("SectionForFlashOperations")Flash_Driver::EraseBlock(void* context, ByteAddress sector)
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    FLASH_EraseInitTypeDef FLASH_EraseInitStruct;
    uint32_t sectorError = 0;
    
    // NETMF PAL 'block' designation is the equivalent to Cortex-M 'sector' so it's OK to use the address directly into the FLASH erase init struct 
     
    // init FLASH erase structure with following configs
    // erase a single sector in flash bank 1 (NETMF doesn't have the concept of multiple FLASH banks)    
    // sector to erase
    FLASH_EraseInitStruct.TypeErase = FLASH_TYPEERASE_SECTORS;
    FLASH_EraseInitStruct.Sector = (UINT32)sector;
    FLASH_EraseInitStruct.NbSectors = 1;
    FLASH_EraseInitStruct.Banks = FLASH_BANK_1;
    
    // set voltage range according to VDD_VALUE
    // FLASH_VOLTAGE_RANGE_1: when the device voltage range is 1.8V to 2.1V
    #if ((VDD_VALUE >= 1800U) && (VDD_VALUE < 2100U))
        FLASH_EraseInitStruct.VoltageRange = FLASH_VOLTAGE_RANGE_1;
    // FLASH_VOLTAGE_RANGE_2: when the device voltage range is 2.1V to 2.7V
    #elif ((VDD_VALUE >= 2100U) && (VDD_VALUE < 2700U))
        FLASH_EraseInitStruct.VoltageRange = FLASH_VOLTAGE_RANGE_2;
    // FLASH_VOLTAGE_RANGE_3: when the device voltage range is 2.7V to 3.6V,        
    #elif ((VDD_VALUE >= 2700U) && (VDD_VALUE < 3600U))
        FLASH_EraseInitStruct.VoltageRange = FLASH_VOLTAGE_RANGE_3;
    // FLASH_VOLTAGE_RANGE_4: when the device voltage range is 2.7V to 3.6V + External Vpp
    #elif (VDD_VALUE >= 3600U)
        FLASH_EraseInitStruct.VoltageRange = FLASH_VOLTAGE_RANGE_4;
    #else
        #error CONFIGURATION ERROR: missing or invalid value for VDD_VALUE (check xxxxxxxxx_hal_conf.h in solution folder)
    #endif
    
    // perform erase operation    
    if(HAL_FLASHEx_Erase(&FLASH_EraseInitStruct, &sectorError) != HAL_OK)
    {
        debug_printf( "Flash_EraseBlock failure @ sector %08x\r\n", (UINT32)sectorError);
        return FALSE;        
    }

    return TRUE;
}

void  __section("SectionForFlashOperations")Flash_Driver::SetPowerState(void* context, UINT32 state)
{
}


//--// ---------------------------------------------------

#pragma arm section code = "SectionForFlashOperations"

UINT32 __section("SectionForFlashOperations")Flash_Driver::MaxSectorWrite_uSec(void* context)
{
    NATIVE_PROFILE_PAL_FLASH();

    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
    
    return config->BlockConfig.BlockDeviceInformation->MaxSectorWrite_uSec;
}


UINT32 __section("SectionForFlashOperations")Flash_Driver::MaxBlockErase_uSec(void* context)
{
    NATIVE_PROFILE_PAL_FLASH();
    
    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;

    return config->BlockConfig.BlockDeviceInformation->MaxBlockErase_uSec;  
}

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata = "g_CMSIS_Flash_DeviceTable"
#endif

struct IBlockStorageDevice g_CMSIS_Flash_DeviceTable = 
{                          
    &Flash_Driver::ChipInitialize,
    &Flash_Driver::ChipUnInitialize,
    &Flash_Driver::GetDeviceInfo,
    &Flash_Driver::Read,
    &Flash_Driver::Write,
    &Flash_Driver::Memset,    
    &Flash_Driver::GetSectorMetadata,
    &Flash_Driver::SetSectorMetadata,
    &Flash_Driver::IsBlockErased,
    &Flash_Driver::EraseBlock,
    &Flash_Driver::SetPowerState,
    &Flash_Driver::MaxSectorWrite_uSec,
    &Flash_Driver::MaxBlockErase_uSec,    
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata 
#endif 
