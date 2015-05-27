//-----------------------------------------------------------------------------
// Software that is described herein is for illustrative purposes only  
// which provides customers with programming information regarding the  
// products. This software is supplied "AS IS" without any warranties.  
// NXP Semiconductors assumes no responsibility or liability for the 
// use of the software, conveys no license or title under any patent, 
// copyright, or mask work right to the product. NXP Semiconductors 
// reserves the right to make changes in the software without 
// notification. NXP Semiconductors also make no representation or 
// warranty that such application will be suitable for the specified 
// use without further testing or modification. 
//-----------------------------------------------------------------------------

#include <tinyhal.h>
#include "SST39WF_16.h"

//--//

BOOL __section("SectionForFlashOperations")SST39WF_16_BS_Driver::ChipInitialize( void* context )
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

    ChipReadOnly( config, TRUE, FLASH_PROTECTION_KEY );
    
    return TRUE;
}

BOOL __section("SectionForFlashOperations")SST39WF_16_BS_Driver::ChipUnInitialize( void* context )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    return TRUE;
}

const BlockDeviceInfo* __section("SectionForFlashOperations")SST39WF_16_BS_Driver::GetDeviceInfo( void* context )
{
    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
    
    return config->BlockConfig.BlockDeviceInformation;
}

BOOL  __section("SectionForFlashOperations")SST39WF_16_BS_Driver::ChipReadOnly( void* context, BOOL On, UINT32 ProtectionKey )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
    
    if(ProtectionKey != FLASH_PROTECTION_KEY) { ASSERT(0); return FALSE; }

    config->ChipProtection = On ? 0 : ProtectionKey;

    CPU_EBIU_Memory_ReadOnly( config->Memory, On );
	// THIS WAS ALWAYS FALSE IN THE OLD SST39WF DRIVER, PROBABLY STILL IS
    if(config->BlockConfig.WriteProtectionPin.Pin != GPIO_PIN_NONE)
    {
        CPU_GPIO_SetPinState( config->BlockConfig.WriteProtectionPin.Pin, On ? config->BlockConfig.WriteProtectionPin.ActiveState: !config->BlockConfig.WriteProtectionPin.ActiveState );

        // we need 200nS setup on FLASH_WP_L
        CYCLE_DELAY_LOOP( 6 );   // 5.52 clock cycles @ 27.6 clocks/uSec
    }

    return TRUE;
}



BOOL  __section("SectionForFlashOperations")SST39WF_16_BS_Driver::Read( void* context, ByteAddress StartSector, UINT32 NumBytes, BYTE * pSectorBuff)
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



BOOL __section("SectionForFlashOperations")SST39WF_16_BS_Driver::WriteX(void* context, ByteAddress StartSector, UINT32 NumBytes, BYTE * pSectorBuff, BOOL ReadModifyWrite, BOOL fIncrementDataPtr)
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

            Action_WriteWord( config,  ChipAddress, *pData );

            
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



BOOL __section("SectionForFlashOperations")SST39WF_16_BS_Driver::Write(void* context, ByteAddress Address, UINT32 NumBytes, BYTE * pSectorBuff, BOOL ReadModifyWrite)
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
            if(offset == 0 && NumBytes >= bytesPerBlock)
            {
                pData = &pSectorBuff[index];
            }
            else
            {
                int bytes = __min(bytesPerBlock - offset, NumBytes); 
                
                memcpy( &pBuf[0]     , (void*)(addr - offset), bytesPerBlock );
                memcpy( &pBuf[offset], &pSectorBuff[index]   , bytes         );

                pData = pBuf;
            }

            if(!EraseBlock( context, addr - offset ))
            {
                fRet = FALSE;
                break;
            }

            fRet = WriteX(context, (addr - offset), bytesPerBlock, pData, ReadModifyWrite, TRUE);

            NumBytes -= bytesPerBlock;
            addr     += bytesPerBlock;
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




BOOL __section("SectionForFlashOperations")SST39WF_16_BS_Driver::Memset(void* context, ByteAddress Address, UINT8 Data, UINT32 NumBytes)
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


BOOL __section("SectionForFlashOperations")SST39WF_16_BS_Driver::GetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
{
    return FALSE;
}

BOOL __section("SectionForFlashOperations")SST39WF_16_BS_Driver::SetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
{
    return FALSE;
}


BOOL __section("SectionForFlashOperations")SST39WF_16_BS_Driver::IsBlockErased( void* context, ByteAddress BlockStart, UINT32 BlockLength )
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


BOOL __section("SectionForFlashOperations")SST39WF_16_BS_Driver::EraseBlock( void* context, ByteAddress Sector )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    CHIP_WORD * ChipAddress;
    
    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;

    const BlockDeviceInfo * deviceInfo = config->BlockConfig.BlockDeviceInformation;

    if (deviceInfo->Attribute.WriteProtected) return FALSE;
    
    ChipAddress = (CHIP_WORD *)Sector;
    
    ChipReadOnly(config, FALSE, FLASH_PROTECTION_KEY);

	Action_EraseBlock( config, ChipAddress );

    ChipReadOnly(config, TRUE, FLASH_PROTECTION_KEY);

    return TRUE;
}

void  __section("SectionForFlashOperations")SST39WF_16_BS_Driver::SetPowerState( void* context, BOOL State )
{
    // the SST39WF goes to Low Power mode automatically after each successful read
    return ;
}
//--//
// Public functions

BOOL __section("SectionForFlashOperations")SST39WF_16_BS_Driver::ReadProductID( void* context, FLASH_WORD& ManufacturerCode, FLASH_WORD& DeviceCode )
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

UINT32 __section("SectionForFlashOperations")SST39WF_16_BS_Driver::MaxSectorWrite_uSec( void* context )
{
    NATIVE_PROFILE_PAL_FLASH();

    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
    
    return config->BlockConfig.BlockDeviceInformation->MaxSectorWrite_uSec;
}


UINT32 __section("SectionForFlashOperations")SST39WF_16_BS_Driver::MaxBlockErase_uSec( void* context )
{
    NATIVE_PROFILE_PAL_FLASH();
    
    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;

    return config->BlockConfig.BlockDeviceInformation->MaxBlockErase_uSec;
    
}

void __section("SectionForFlashOperations") SST39WF_16_BS_Driver::Action_ReadID( volatile CHIP_WORD* SectorCheck, FLASH_WORD& ManufacturerCode, FLASH_WORD& DeviceCode )
{
	NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    // assume both CS is using the same flash chip, take
    SectorCheck = CPU_GetUncachableAddress( SectorCheck );

    ///////////////////////////////////
    //
    //
    FLASH_BEGIN_PROGRAMMING_FAST();

    // Enter product ID mode
    SectorCheck[0x5555] = 0x00AA;
    SectorCheck[0x2AAA] = 0x0055;
    SectorCheck[0x5555] = 0x0090;

    ManufacturerCode = SectorCheck[0x0000];
    DeviceCode       = SectorCheck[0x0001];

    // Exit product ID mode
    SectorCheck[0x5555] = 0x00AA;
    SectorCheck[0x2AAA] = 0x0055;
    SectorCheck[0x5555] = 0x00F0;


    FLASH_END_PROGRAMMING_FAST( "SST39WF_16 ReadProductID", SectorCheck );
    //
    //
    ///////////////////////////////////

}

void __section("SectionForFlashOperations")SST39WF_16_BS_Driver::Action_EraseBlock( void* context, volatile CHIP_WORD * Block)
{
	NATIVE_PROFILE_HAL_DRIVERS_FLASH();
    
	 MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
	
    //volatile CHIP_WORD* BaseAddress=(CHIP_WORD*) FlashConfig->Flash_ComputeBaseAddress ((UINT32)SectorStart);
	volatile CHIP_WORD* BaseAddress = (CHIP_WORD*) config->Memory.BaseAddress;
	
    ///////////////////////////////////
    //
    //
    FLASH_BEGIN_PROGRAMMING_FAST();

    BaseAddress[0x5555] = 0x00AA;
    BaseAddress[0x2AAA] = 0x0055;
    BaseAddress[0x5555] = 0x0080;
    BaseAddress[0x5555] = 0x00AA;
    BaseAddress[0x2AAA] = 0x0055;

    *Block = 0x0030;	//SST Sector (=MS Block) erase. 

    // wait for the full max time (no continuations are allowed)
    //FLASH_SLEEP_IF_INTERRUPTS_ENABLED( FlashConfig->CommonConfig.Duration_Max_SectorErase_uSec );
	//This macro is no longer present in 3.0RTM. What matters most, we are using FLASH_BEGIN_PROGRAMMING_FAST, which disables IRQs anyway.

    // the 50 mSec erase is in progress.
    // wait for device to signal completion
    // done is indicated by no toggling bits.
    // wake up every 40uSec to take interrupts
    while(*Block != *Block);

    FLASH_END_PROGRAMMING_FAST( "SST39WF_16 EraseBlock", Block );
}

void __section("SectionForFlashOperations") SST39WF_16_BS_Driver::Action_WriteWord(void * context, volatile CHIP_WORD * Sector, CHIP_WORD Data )
{
	NATIVE_PROFILE_HAL_DRIVERS_FLASH();
	MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
    
	//volatile CHIP_WORD* BaseAddress = (CHIP_WORD*)FlashConfig->Flash_ComputeBaseAddress((UINT32)Address);
	volatile CHIP_WORD* BaseAddress = (CHIP_WORD*)config->Memory.BaseAddress;

    ///////////////////////////////////
    //
    //
    FLASH_BEGIN_PROGRAMMING_FAST();

    BaseAddress[0x5555] = 0x00AA;
    BaseAddress[0x2AAA] = 0x0055;
    BaseAddress[0x5555] = 0x00A0;

    *Sector = Data;

    // wait for device to signal completion
    // done is indicated by no toggling bits.
    // wake up every 10uSec to check again and take interrupts
    while(*Sector != *Sector);

    FLASH_END_PROGRAMMING_FAST( "SST39WF_16 WriteWord", Sector );
    //
    //
    ///////////////////////////////////
	
}

//--// 

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata = "g_SST39WF_16_BS_DeviceTable"
#endif

struct IBlockStorageDevice g_SST39WF_16_BS_DeviceTable = 
{                          
    &SST39WF_16_BS_Driver::ChipInitialize,
    &SST39WF_16_BS_Driver::ChipUnInitialize,
    &SST39WF_16_BS_Driver::GetDeviceInfo,
    &SST39WF_16_BS_Driver::Read,
    &SST39WF_16_BS_Driver::Write,
    &SST39WF_16_BS_Driver::Memset,
    &SST39WF_16_BS_Driver::GetSectorMetadata,
    &SST39WF_16_BS_Driver::SetSectorMetadata,
    &SST39WF_16_BS_Driver::IsBlockErased,    
    &SST39WF_16_BS_Driver::EraseBlock,
    &SST39WF_16_BS_Driver::SetPowerState,
    &SST39WF_16_BS_Driver::MaxSectorWrite_uSec,
    &SST39WF_16_BS_Driver::MaxBlockErase_uSec,    
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata 
#endif 








