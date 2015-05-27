////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "I28F_16.h"

//--//

BOOL __section("SectionForFlashOperations")I28F_16_BS_Driver::ChipInitialize( void* context )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();
    
    int i;

    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
   
    // first setup the memory for wait states, read only, etc.
    CPU_EBIU_ConfigMemoryBlock( config->Memory );

    // the flash are now not write protected

    {
        FLASH_WORD ManufacturerCode = 0;
        FLASH_WORD DeviceCode       = 0;

        if(ReadProductID( config, ManufacturerCode, DeviceCode ))
        {
            debug_printf( "Flash product Manufacturer Code = 0x%08x, Device Code = 0x%08x\r\n", ManufacturerCode, DeviceCode );

            if(ManufacturerCode != config->ManufacturerCode )
            {
                debug_printf( "ERROR: Unsupported Flash Manufacturer code: 0x%08x\r\n", ManufacturerCode );
            }

            if(DeviceCode != config->DeviceCode)
            {
                debug_printf( "ERROR: Unsupported Flash Device code: 0x%08x\r\n", DeviceCode );
            }
        }
        else
        {
            debug_printf( "Flash_ReadProductID() failed.\r\n" );
        }
    }

    // verify that all partitions are good by checking manufacturing/device codes (per spec)

    const BlockRegionInfo *pRegion;
    CHIP_WORD * ChipAddress;
    
    ChipReadOnly( config, FALSE, FLASH_PROTECTION_KEY );

    for(i = 0; i < config->BlockConfig.BlockDeviceInformation->NumRegions;  i++)
    {

        pRegion = &(config->BlockConfig.BlockDeviceInformation->Regions[i]);

        ChipAddress = (CHIP_WORD *) pRegion->Start;

        for (int j=0; j<pRegion->NumBlocks; j++)
        {
            FLASH_WORD ManufacturerCode;
            FLASH_WORD DeviceCode;


            Action_ReadID( ChipAddress, ManufacturerCode, DeviceCode );
   
            if(config->ManufacturerCode != ManufacturerCode)
            {
                debug_printf( "Flash_ChipInitialize: ManufacturerCode failure 0x%08x != 0x%08x at 0x%08x\r\n", ManufacturerCode, config->ManufacturerCode, (UINT32)ChipAddress);
            }

            if(config->DeviceCode != DeviceCode)
            {
                debug_printf( "Flash_ChipInitialize: DeviceCode failure 0x%08x != 0x%08x at 0x%08x\r\n", DeviceCode, config->DeviceCode, (UINT32)ChipAddress );
            }

            ChipAddress = (CHIP_WORD *) ( (UINT32)ChipAddress + (UINT32) pRegion->BytesPerBlock);

        }
        
    }

    // unlock all the blocks (sectors) that are locked
    // the default is to have the sectors locked upon powerup, undo that here


    for(i = 0; i < config->BlockConfig.BlockDeviceInformation->NumRegions;  i++)
    {

        pRegion = &(config->BlockConfig.BlockDeviceInformation->Regions[i]);

        // XIP 
        ChipAddress = (CHIP_WORD *)pRegion->Start ;

        for (int j=0; j<pRegion->NumBlocks; j++)
        {
            CHIP_WORD BlockLockStatus = Action_ReadLockStatus( ChipAddress );

            if(BlockLockStatus & LOCK_STATUS_LOCKED)
            {
                debug_printf( "Flash_ChipInitialize: Block at 0x%08x is locked, unlocking now.\r\n", (UINT32)ChipAddress );

                Action_Unlock( ChipAddress );
            }

            Action_ClearStatusRegister( ChipAddress );

            if(BlockLockStatus & LOCK_STATUS_LOCKED_DOWN)
            {
                debug_printf( "Flash_ChipInitialize: Block at 0x%08x is locked down, must reset or powerdown to remove lock-down\r\n", (UINT32)ChipAddress );
            }

            ChipAddress = (CHIP_WORD *) ( (UINT32)ChipAddress + pRegion->BytesPerBlock);
        }
    }

    ChipReadOnly( config, TRUE, FLASH_PROTECTION_KEY );
    
    return TRUE;
}

BOOL __section("SectionForFlashOperations")I28F_16_BS_Driver::ChipUnInitialize( void* context )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    return TRUE;
}

const BlockDeviceInfo* __section("SectionForFlashOperations")I28F_16_BS_Driver::GetDeviceInfo( void* context )
{
    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
    
    return config->BlockConfig.BlockDeviceInformation;    
}

BOOL  __section("SectionForFlashOperations")I28F_16_BS_Driver::ChipReadOnly( void* context, BOOL On, UINT32 ProtectionKey )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
    
    if(ProtectionKey != FLASH_PROTECTION_KEY) { ASSERT(0); return FALSE; }

    config->ChipProtection = On ? 0 : ProtectionKey;

    CPU_EBIU_Memory_ReadOnly( config->Memory, On );

    if(config->BlockConfig.WriteProtectionPin.Pin != GPIO_PIN_NONE)
    {
        CPU_GPIO_SetPinState( config->BlockConfig.WriteProtectionPin.Pin, On ? config->BlockConfig.WriteProtectionPin.ActiveState: !config->BlockConfig.WriteProtectionPin.ActiveState );

        // we need 200nS setup on FLASH_WP_L
        CYCLE_DELAY_LOOP( 6 );   // 5.52 clock cycles @ 27.6 clocks/uSec
    }

    return TRUE;
}



BOOL  __section("SectionForFlashOperations")I28F_16_BS_Driver::Read( void* context, ByteAddress StartSector, UINT32 NumBytes, BYTE * pSectorBuff)
{
    // XIP device does not need to read into a buffer
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    if (pSectorBuff == NULL) return FALSE;

    StartSector = CPU_GetUncachableAddress(StartSector);

    CHIP_WORD* ChipAddress = (CHIP_WORD *)StartSector;
    CHIP_WORD* EndAddress  = (CHIP_WORD *)(StartSector + NumBytes);
    CHIP_WORD *pBuf        = (CHIP_WORD *)pSectorBuff;

    while(ChipAddress < EndAddress)
    {
        *pBuf++ = *ChipAddress++;
    }

    return TRUE;
}


BOOL __section("SectionForFlashOperations")I28F_16_BS_Driver::WriteX(void* context, ByteAddress StartSector, UINT32 NumBytes, BYTE * pSectorBuff, BOOL ReadModifyWrite, BOOL fIncrementDataPtr)
{
    NATIVE_PROFILE_PAL_FLASH();

    volatile CHIP_WORD * ChipAddress;
    BOOL result= TRUE;
    CHIP_WORD * pData = (CHIP_WORD*)pSectorBuff;

    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;

    // if write protected, no update
    const BlockDeviceInfo * deviceInfo = config->BlockConfig.BlockDeviceInformation;
    if (deviceInfo->Attribute.WriteProtected) return FALSE;

    ChipAddress = (volatile CHIP_WORD *)CPU_GetUncachableAddress(StartSector);

    ChipReadOnly( config, FALSE, FLASH_PROTECTION_KEY );

    for(UINT32 i = 0; i < NumBytes; i += sizeof(CHIP_WORD), ChipAddress++)
    {

        // if same, nothing to do, continue nextword.
        if(*ChipAddress != *pData) 
        {
            // check for having to move bits from 0->1, a failure case for a write
            if(0 != (*pData & ~(*ChipAddress)))
            {
                debug_printf( "erase failure: 0x%08x=0x%04x\r\n", (size_t)ChipAddress, *ChipAddress );
                ASSERT(0);
                result =FALSE;
                break;
            }

            CHIP_WORD StatusRegister = Action_WriteWord( ChipAddress, *pData, config );

            if(0 != (StatusRegister & ~SR_WSM_READY))
            {
                debug_printf( "Flash_WriteWord(0x%08x): Status Register non-zero after word program: 0x%08x\r\n", (UINT32)ChipAddress, StatusRegister );
                result= FALSE;
                break;
            }
            
            if (*ChipAddress != *pData)
            {
                debug_printf( "Flash_WriteToSector failure @ 0x%08x, wrote 0x%08x, read 0x%08x\r\n", (UINT32)ChipAddress, *pData, *ChipAddress );
                result = FALSE;
                break;
            }
        }

        if(fIncrementDataPtr) pData++;
    }

    ChipReadOnly(config, TRUE, FLASH_PROTECTION_KEY);
    
    return result;
}

BOOL __section("SectionForFlashOperations")I28F_16_BS_Driver::Write(void* context, ByteAddress Address, UINT32 NumBytes, BYTE * pSectorBuff, BOOL ReadModifyWrite)
{
    NATIVE_PROFILE_PAL_FLASH();

    BYTE * pData;
    BYTE * pBuf = NULL;
    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
    const BlockDeviceInfo * deviceInfo = config->BlockConfig.BlockDeviceInformation;
    
    UINT32 region, range;
    if(ReadModifyWrite) 
    {
        BOOL fRet = TRUE;
        
        if(!deviceInfo->FindRegionFromAddress(Address, region, range)) return FALSE;

        UINT32      bytesPerBlock   = deviceInfo->Regions[region].BytesPerBlock;
        UINT32      regionEnd       = deviceInfo->Regions[region].Start + deviceInfo->Regions[region].Size();
        UINT32      offset          = Address % bytesPerBlock;
        ByteAddress addr            = Address;
        ByteAddress addrEnd         = Address + NumBytes;
        UINT32      index           = 0;

        pBuf = (BYTE*)private_malloc(bytesPerBlock);

        if(pBuf == NULL) return FALSE;


        while(fRet && addr < addrEnd)
        {
            ByteAddress sectAddr = (addr - offset);
            
            if(offset == 0 && NumBytes >= bytesPerBlock)
            {
                pData = &pSectorBuff[index];
            }
            else
            {
                int bytes = __min(bytesPerBlock - offset, NumBytes); 
                
                memcpy( &pBuf[0]     , (void*)sectAddr    , bytesPerBlock );
                memcpy( &pBuf[offset], &pSectorBuff[index], bytes         );

                pData = pBuf;
            }

            if(!EraseBlock( context, sectAddr ))
            {
                fRet = FALSE;
                break;
            }

            fRet = WriteX(context, sectAddr, bytesPerBlock, pData, ReadModifyWrite, TRUE);

            NumBytes -= bytesPerBlock - offset;
            addr     += bytesPerBlock - offset;
            index    += bytesPerBlock - offset;
            offset    = 0;

            if(NumBytes > 0 && addr >= regionEnd)
            {
                region++;

                if(region >= deviceInfo->NumRegions)
                {
                    fRet = FALSE;
                }
                else
                {
                    regionEnd       = deviceInfo->Regions[region].Start + deviceInfo->Regions[region].Size();
                    bytesPerBlock   = deviceInfo->Regions[region].BytesPerBlock;

                    private_free(pBuf);

                    pBuf = (BYTE*)private_malloc(bytesPerBlock);

                    if(pBuf == NULL) fRet = FALSE;
                }
            }
                
        }

        if(pBuf != NULL)
        {
            private_free(pBuf);
        }

        return fRet;            
    }
    else
    {
        return WriteX(context, Address, NumBytes, pSectorBuff, ReadModifyWrite, TRUE);
    }
}

BOOL __section("SectionForFlashOperations")I28F_16_BS_Driver::Memset(void* context, ByteAddress Address, UINT8 Data, UINT32 NumBytes)
{
    NATIVE_PROFILE_PAL_FLASH();

    CHIP_WORD chipData;

    memset(&chipData, Data, sizeof(CHIP_WORD));

    if(Data != 0)
    {
        // TODO: ERASE before memset - currently we only use this to set everything to zero for FS so no need to worry
        ASSERT(FALSE);
    }

    return WriteX(context, Address, NumBytes, (BYTE*)&chipData, TRUE, FALSE);
}


BOOL __section("SectionForFlashOperations")I28F_16_BS_Driver::GetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
{
    return FALSE;
}

BOOL __section("SectionForFlashOperations")I28F_16_BS_Driver::SetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
{
    return FALSE;
}


BOOL __section("SectionForFlashOperations")I28F_16_BS_Driver::IsBlockErased( void* context, ByteAddress BlockStart, UINT32 BlockLength )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    BlockStart = CPU_GetUncachableAddress(BlockStart);

    CHIP_WORD* ChipAddress = (CHIP_WORD *) BlockStart;
    CHIP_WORD* EndAddress  = (CHIP_WORD *)(BlockStart + BlockLength);
    
    while(ChipAddress < EndAddress)
    {
        if(*ChipAddress != 0xFFFF)
        {
            return FALSE;
        }
        ChipAddress++;
    }

    return TRUE;
}


BOOL __section("SectionForFlashOperations")I28F_16_BS_Driver::EraseBlock( void* context, ByteAddress Sector )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    CHIP_WORD * ChipAddress;
    
    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;

    const BlockDeviceInfo * deviceInfo = config->BlockConfig.BlockDeviceInformation;
    
    if (deviceInfo->Attribute.WriteProtected) return FALSE;

    ChipAddress = (CHIP_WORD *)Sector;
    
    ChipReadOnly(config, FALSE, FLASH_PROTECTION_KEY);

    
    ByteAddress StatusRegister = Action_EraseSector( config, ChipAddress, config );

    if(0 != (StatusRegister & ~SR_WSM_READY))
    {
        debug_printf( "Flash_EraseSector(0x%08x): Status Register non-zero after erase: 0x%08x\r\n", (size_t)Sector, StatusRegister );
    }

    ChipReadOnly(config, TRUE, FLASH_PROTECTION_KEY);

    return TRUE;
}



void  __section("SectionForFlashOperations")I28F_16_BS_Driver::SetPowerState( void* context, UINT32 State )
{
    // our flash driver is always ON
    return ;
}
//--//
// Public functions

BOOL __section("SectionForFlashOperations")I28F_16_BS_Driver::ReadProductID( void* context, FLASH_WORD& ManufacturerCode, FLASH_WORD& DeviceCode )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;

    volatile CHIP_WORD* BaseAddress = (CHIP_WORD *)config->BlockConfig.BlockDeviceInformation->Regions[0].Start;

    ChipReadOnly(config, FALSE, FLASH_PROTECTION_KEY);

    Action_ReadID( BaseAddress, ManufacturerCode, DeviceCode );

    ChipReadOnly( config, TRUE, FLASH_PROTECTION_KEY);

    return TRUE;
}

//--// ---------------------------------------------------

#pragma arm section code = "SectionForFlashOperations"

UINT32 __section("SectionForFlashOperations")I28F_16_BS_Driver::MaxSectorWrite_uSec( void* context )
{
    NATIVE_PROFILE_PAL_FLASH();

    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
    
    return config->BlockConfig.BlockDeviceInformation->MaxSectorWrite_uSec;
}


UINT32 __section("SectionForFlashOperations")I28F_16_BS_Driver::MaxBlockErase_uSec( void* context )
{
    NATIVE_PROFILE_PAL_FLASH();
    
    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;

    return config->BlockConfig.BlockDeviceInformation->MaxBlockErase_uSec;
    
}

void __section("SectionForFlashOperations") I28F_16_BS_Driver::Action_ReadID( volatile CHIP_WORD* SectorCheck, FLASH_WORD& ManufacturerCode, FLASH_WORD& DeviceCode )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();
    ///////////////////////////////////
    //
    //
    FLASH_BEGIN_PROGRAMMING_FAST();

    SectorCheck = CPU_GetUncachableAddress( SectorCheck );

    *SectorCheck       = READ_ID;

    ManufacturerCode = SectorCheck[0x0000];
    DeviceCode       = SectorCheck[0x0001];

    // Exit product ID mode
    *SectorCheck = ENTER_READ_ARRAY_MODE;

    FLASH_END_PROGRAMMING_FAST( "I28F_16 ReadID", NULL );
    //
    //
    ///////////////////////////////////
}

I28F_16_BS_Driver::CHIP_WORD __section("SectionForFlashOperations") I28F_16_BS_Driver::Action_ReadLockStatus( volatile CHIP_WORD* SectorCheck )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();
    CHIP_WORD BlockLockStatus;

    ///////////////////////////////////
    //
    //
    FLASH_BEGIN_PROGRAMMING_FAST();

    SectorCheck = CPU_GetUncachableAddress( SectorCheck );

    *SectorCheck    = READ_ID;
    BlockLockStatus = SectorCheck[0x0002];
    *SectorCheck    = ENTER_READ_ARRAY_MODE;     // Exit product ID mode

    FLASH_END_PROGRAMMING_FAST( "I28F_16 ReadLocks", NULL );
    //
    //
    ///////////////////////////////////

    return BlockLockStatus;
}


void __section("SectionForFlashOperations") I28F_16_BS_Driver::Action_Unlock( volatile CHIP_WORD* SectorCheck )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();
    ///////////////////////////////////
    //
    //
    FLASH_BEGIN_PROGRAMMING_FAST();

    SectorCheck = CPU_GetUncachableAddress( SectorCheck );

    *SectorCheck = LOCK_SETUP;
    *SectorCheck = LOCK_UNLOCK_BLOCK;
    *SectorCheck = ENTER_READ_ARRAY_MODE;

    FLASH_END_PROGRAMMING_FAST( "I28F_16 Unlock", NULL );
    //
    //
    ///////////////////////////////////
}

void __section("SectionForFlashOperations") I28F_16_BS_Driver::Action_ClearStatusRegister( volatile CHIP_WORD* SectorCheck )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();
    ///////////////////////////////////
    //
    //
    FLASH_BEGIN_PROGRAMMING_FAST();

    SectorCheck = CPU_GetUncachableAddress( SectorCheck );

    // clear the status register for this block, just in case it has some residual error present
    *SectorCheck = CLEAR_STATUS_REGISTER;
    *SectorCheck = ENTER_READ_ARRAY_MODE;

    FLASH_END_PROGRAMMING_FAST( "I28F_16 ClearStatus", NULL );
    //
    //
    ///////////////////////////////////
}

I28F_16_BS_Driver::CHIP_WORD __section("SectionForFlashOperations") I28F_16_BS_Driver::Action_EraseSector( void* context, volatile CHIP_WORD * Sector, MEMORY_MAPPED_NOR_BLOCK_CONFIG* FlashConfig )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    CHIP_WORD StatusRegister;

    ///////////////////////////////////
    //
    //
    FLASH_BEGIN_PROGRAMMING_FAST(  );

    Sector = (volatile CHIP_WORD*)CPU_GetUncachableAddress( Sector );


    // now setup erasing
    *Sector = BLOCK_ERASE_SETUP;
    *Sector = BLOCK_ERASE_CONFIRM;

    // wait for device to signal completion
    // break when the device signals completion by looking for Data (0xff erasure value) on I/O 7 (a 0 on I/O 7 indicates erasure in progress)
    while((*Sector & SR_WSM_READY) != SR_WSM_READY);

    StatusRegister = *Sector;

    // error conditions must be cleared
    *Sector = CLEAR_STATUS_REGISTER;
    *Sector = ENTER_READ_ARRAY_MODE;

    CPU_FlushCaches();

    FLASH_END_PROGRAMMING_FAST( "I28F_16 EraseSector", Sector );
    //
    //
    ///////////////////////////////////

    return StatusRegister;
}

I28F_16_BS_Driver::CHIP_WORD __section("SectionForFlashOperations") I28F_16_BS_Driver::Action_WriteWord( volatile CHIP_WORD * Sector, CHIP_WORD Data, MEMORY_MAPPED_NOR_BLOCK_CONFIG* FlashConfig )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();
    CHIP_WORD StatusRegister;


    ///////////////////////////////////
    //
    //
    FLASH_BEGIN_PROGRAMMING_FAST();

    Sector = CPU_GetUncachableAddress( Sector );

    // Enter Word Program Mode.
    *Sector = PROGRAM_WORD;
    *Sector = Data;

    // wait for device to signal completion
    // break when the device signals completion by looking for Data (0xff erasure value) on I/O 7 (a 0 on I/O 7 indicates erasure in progress)
    while((*Sector & SR_WSM_READY) != SR_WSM_READY);

    StatusRegister = *Sector;

    // Exit Status Read Mode.
    *Sector = CLEAR_STATUS_REGISTER;
    *Sector = ENTER_READ_ARRAY_MODE;

    CPU_InvalidateAddress( CPU_GetCachableAddress( Sector ) );
 

    FLASH_END_PROGRAMMING_FAST( "I28F_16 WriteWord", Sector );
    //
    //
    ///////////////////////////////////

    return StatusRegister;
}

//--// 

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata = "g_I28F_16_BS_DeviceTable"
#endif

struct IBlockStorageDevice g_I28F_16_BS_DeviceTable = 
{                          
    &I28F_16_BS_Driver::ChipInitialize,
    &I28F_16_BS_Driver::ChipUnInitialize,
    &I28F_16_BS_Driver::GetDeviceInfo,
    &I28F_16_BS_Driver::Read,
    &I28F_16_BS_Driver::Write,
    &I28F_16_BS_Driver::Memset,    
    &I28F_16_BS_Driver::GetSectorMetadata,
    &I28F_16_BS_Driver::SetSectorMetadata,
    &I28F_16_BS_Driver::IsBlockErased,
    &I28F_16_BS_Driver::EraseBlock,
    &I28F_16_BS_Driver::SetPowerState,
    &I28F_16_BS_Driver::MaxSectorWrite_uSec,
    &I28F_16_BS_Driver::MaxBlockErase_uSec,    
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata 
#endif 



