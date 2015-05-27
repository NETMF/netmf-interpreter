////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "AM29DL_32.h"


#ifdef FLASH_DRIVER_TESTING
//#define FLASH_DELAYS()    HAL_Time_Sleep_MicroSeconds(10); Events_WaitForEventsInternal(0,1)
#define FLASH_DELAYS()  HAL_Time_Sleep_MicroSeconds_InterruptEnabled(1)
#else
#define FLASH_DELAYS()    
#endif

//--//

BOOL __section("SectionForFlashOperations") AM29DL_32_BS_Driver::ChipInitialize( void* context )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
    
    // first setup the memory for wait states, read only, etc.
    CPU_EBIU_ConfigMemoryBlock( config->Memory );

    ChipReadOnly( config, FALSE, FLASH_PROTECTION_KEY );
    
    // the flash are now not write protected
    {
        FLASH_WORD ManufacturerCode = 0;
        FLASH_WORD DeviceCode       = 0;

        if(ReadProductID(config, ManufacturerCode, DeviceCode ))
        {
            debug_printf( "Flash product Manufacturer Code = 0x%08x, Device Code = 0x%08x\r\n", ManufacturerCode, DeviceCode );

            if(ManufacturerCode != config->ManufacturerCode) 
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

    ChipReadOnly(config, TRUE, FLASH_PROTECTION_KEY );
    // the flash are now write protected

    return TRUE;
}


BOOL __section("SectionForFlashOperations") AM29DL_32_BS_Driver::ChipUnInitialize( void* context )
{
    return TRUE;
}


const BlockDeviceInfo* __section("SectionForFlashOperations") AM29DL_32_BS_Driver::GetDeviceInfo( void* context )
{
    return ((MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context)->BlockConfig.BlockDeviceInformation;
}


BOOL __section("SectionForFlashOperations") AM29DL_32_BS_Driver::ChipReadOnly(void* context, BOOL On, UINT32 ProtectionKey )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();
    
    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;

    if(ProtectionKey != FLASH_PROTECTION_KEY) { ASSERT(0); return FALSE; }

    config->ChipProtection = On ? 0 : ProtectionKey;

    CPU_EBIU_Memory_ReadOnly( config->Memory, On );

    if(config->BlockConfig.WriteProtectionPin.Pin != GPIO_PIN_NONE)
    {
        CPU_GPIO_SetPinState( config->BlockConfig.WriteProtectionPin.Pin, On ? config->BlockConfig.WriteProtectionPin.ActiveState: !config->BlockConfig.WriteProtectionPin.ActiveState );

    }

    return TRUE;
}

BOOL __section("SectionForFlashOperations") AM29DL_32_BS_Driver::ReadProductID(void* context, FLASH_WORD& ManufacturerCode, FLASH_WORD& DeviceCode )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();
    
    ChipReadOnly( context, FALSE, FLASH_PROTECTION_KEY );

    Action_ReadID( context, ManufacturerCode, DeviceCode );

    ChipReadOnly( context, TRUE, FLASH_PROTECTION_KEY );

    return TRUE;
}


BOOL __section("SectionForFlashOperations") AM29DL_32_BS_Driver::IsBlockErased( void* context, ByteAddress address, UINT32 blockLength )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    address = CPU_GetUncachableAddress(address);

    volatile CHIP_WORD * ChipAddress = (volatile CHIP_WORD *) address;

    CHIP_WORD * EndAddress = (CHIP_WORD*)(address + blockLength);
    
    while(ChipAddress < EndAddress)
    {
        if( (*ChipAddress ) != 0xFFFFFFFF)
        {
            return FALSE;   
        }
        ChipAddress ++;
    }
    return TRUE;
}

BOOL __section("SectionForFlashOperations") AM29DL_32_BS_Driver::EraseBlock( void* context, ByteAddress address )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    BOOL result;
    CHIP_WORD * ChipAddress;
    
    UINT32 iRegion, iRange;

    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
    const BlockDeviceInfo *    deviceInfo = config->BlockConfig.BlockDeviceInformation;

    if (deviceInfo->Attribute.WriteProtected) return FALSE;
    
    if (!deviceInfo->FindRegionFromAddress(address, iRegion, iRange)) return FALSE;

    address -= (address % deviceInfo->Regions[iRegion].BytesPerBlock);

    ChipAddress = (CHIP_WORD *) address;
    
    ChipReadOnly(context, FALSE, FLASH_PROTECTION_KEY);
    
    result =  Action_EraseSector( context, ChipAddress );

    ChipReadOnly(context, TRUE, FLASH_PROTECTION_KEY);

    return result;
}


BOOL __section("SectionForFlashOperations") AM29DL_32_BS_Driver::Read(void* context, ByteAddress address, UINT32 numBytes, BYTE * pSectorBuff)
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    CHIP_WORD* ChipAddress, *EndAddress;

#if _DEBUG    
    UINT32 iRegion, iRange;

    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
    const BlockDeviceInfo *    deviceInfo = config->BlockConfig.BlockDeviceInformation;
    if(!deviceInfo->FindRegionFromAddress(address, iRegion, iRange)) return FALSE;
#endif

    address = CPU_GetUncachableAddress(address);

    ChipAddress = (CHIP_WORD *) address;
    EndAddress  = (CHIP_WORD *)(address + numBytes);

    CHIP_WORD *pBuf = (CHIP_WORD *)pSectorBuff;

    while(ChipAddress < EndAddress)
    {
        *pBuf++ = *ChipAddress++;
    }

    return TRUE;
}

BOOL __section("SectionForFlashOperations") AM29DL_32_BS_Driver::Write(void* context, ByteAddress address, UINT32 numBytes,BYTE * pSectorBuff, BOOL ReadModifyWrite)
{
    NATIVE_PROFILE_PAL_FLASH();
    return WriteX(context, address, numBytes, pSectorBuff, ReadModifyWrite, TRUE);
}

BOOL __section("SectionForFlashOperations") AM29DL_32_BS_Driver::Memset(void* context, ByteAddress address, UINT8 data, UINT32 numBytes)
{
    NATIVE_PROFILE_PAL_FLASH();

    CHIP_WORD chipData;

    memset(&chipData, data, sizeof(CHIP_WORD));

    return WriteX(context, address, numBytes, (BYTE*)&chipData, TRUE, FALSE);
}


BOOL __section("SectionForFlashOperations") AM29DL_32_BS_Driver::WriteX(void* context, ByteAddress address, UINT32 numBytes, BYTE * pSectorBuff, BOOL ReadModifyWrite, BOOL fIncrementDataPtr)
{
    NATIVE_PROFILE_PAL_FLASH();

    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
    const BlockDeviceInfo *    deviceInfo = config->BlockConfig.BlockDeviceInformation;

    CHIP_WORD* ChipAddress;
    CHIP_WORD* EndAddress, *pData;
    BOOL result = TRUE;

    if (deviceInfo->Attribute.WriteProtected) return FALSE;

    address = CPU_GetUncachableAddress(address);

    ChipAddress = (CHIP_WORD *)address;
    EndAddress  = (CHIP_WORD *)(address + numBytes); 
    pData       = (CHIP_WORD *)pSectorBuff;
    
    ChipReadOnly(config, FALSE, FLASH_PROTECTION_KEY);

    while(ChipAddress < EndAddress)
    {
       // if same, nothing to do, continue nextword.
       if(*ChipAddress != *pData) 
       {
            // check for having to move bits from 0->1, a failure case for a write
            if(0 != (*pData  & ~(*ChipAddress)))
            {
                debug_printf( "erase failure: 0x%08x=0x%04x\r\n", (size_t)ChipAddress, *ChipAddress );
                ASSERT(0);
                result =FALSE;
                break;
            }

            Action_WriteWord( config, ChipAddress, *pData );

            
            if (*ChipAddress != *pData)
            {
                debug_printf( "Flash_WriteToSector failure @ 0x%08x, wrote 0x%08x, read 0x%08x\r\n", (UINT32)ChipAddress, *pData, *ChipAddress );
                result = FALSE;
                break;
            }
        }

        ChipAddress++;
        if(fIncrementDataPtr) pData++;    
    }

    ChipReadOnly(config, TRUE, FLASH_PROTECTION_KEY);
    
    return result;
}

BOOL __section("SectionForFlashOperations") AM29DL_32_BS_Driver::GetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
{
    return TRUE;
}

BOOL __section("SectionForFlashOperations") AM29DL_32_BS_Driver::SetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
{
    return TRUE;
}


void  __section("SectionForFlashOperations") AM29DL_32_BS_Driver::SetPowerState(void* context, UINT32 State )
{
    // our flash driver is always ON
    return ;
}


UINT32 __section("SectionForFlashOperations") AM29DL_32_BS_Driver::MaxSectorWrite_uSec( void* context )
{
    NATIVE_PROFILE_PAL_FLASH();

    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
    
    return config->BlockConfig.BlockDeviceInformation->MaxSectorWrite_uSec;
}


UINT32 __section("SectionForFlashOperations") AM29DL_32_BS_Driver::MaxBlockErase_uSec( void* context )
{
    NATIVE_PROFILE_PAL_FLASH();

    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;

    return config->BlockConfig.BlockDeviceInformation->MaxBlockErase_uSec;
}

//--//

#pragma arm section code = "SectionForFlashOperations"

void __section("SectionForFlashOperations") AM29DL_32_BS_Driver::Action_ReadID( void* context, FLASH_WORD& ManufacturerCode, FLASH_WORD& DeviceCode )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();
    
    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
    
    volatile CHIP_WORD* BaseAddress = (CHIP_WORD*)config->BlockConfig.BlockDeviceInformation->Regions[0].Start;

    ///////////////////////////////////
    //
    //
    FLASH_BEGIN_PROGRAMMING_FAST();
    
    // Exit "Autoselect" mode by writing reset '0xF0' to bus
    BaseAddress[0x0] = 0x00F000F0;
    // Enter "Autoselect" mode to read back chip details
    BaseAddress[0x0555] = 0x00AA00AA;
    BaseAddress[0x02AA] = 0x00550055;
    BaseAddress[0x0555] = 0x00900090;
    ManufacturerCode = BaseAddress[0x0000];
    DeviceCode       = BaseAddress[0x0001];    // read device ID, must read the A[0x01]. A[0x0E]. A[0x0F]
    // Exit "Autoselect" mode by writing reset '0xF0' to bus
    BaseAddress[0x0] = 0x00F000F0;
    
    FLASH_END_PROGRAMMING_FAST( "AM29DL_BS_32 ReadProductID", BaseAddress );
    //
    //
    ///////////////////////////////////
}

void __section("SectionForFlashOperations") AM29DL_32_BS_Driver::Action_Unprotect( void* context, volatile CHIP_WORD* BaseAddress )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();
}


BOOL __section("SectionForFlashOperations") AM29DL_32_BS_Driver::Action_EraseSector( void* context, volatile CHIP_WORD* SectorStart )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    BOOL success = TRUE;
    
    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
    
    volatile CHIP_WORD* BaseAddress = (CHIP_WORD*)config->BlockConfig.BlockDeviceInformation->Regions[0].Start;

    ///////////////////////////////////
    //
    //
    FLASH_BEGIN_PROGRAMMING_FAST();

    // Reset "Autoselect" mode by writing reset '0xF0' to bus
    BaseAddress[0x0]    = 0x00F000F0;
    BaseAddress[0x0555] = 0x00AA00AA;
    BaseAddress[0x02AA] = 0x00550055;
    BaseAddress[0x0555] = 0x00800080;
    BaseAddress[0x0555] = 0x00AA00AA;
    BaseAddress[0x02AA] = 0x00550055;

    *SectorStart = 0x00300030;

    // wait for the full typical time (no continuations are allowed)

//    FLASH_SLEEP_IF_INTERRUPTS_ENABLED( config->CommonConfig.Duration_Typ_SectorErase_uSec );

    // wait for device to signal completion
    // break when the device signals completion by looking for Data DQ6 (Toggle Bit)
    // wake up every 40uSec to check again and take interrupts
    {

        UINT32 Data_DQ5,Data_DQ7;

        while((*SectorStart & SR_WSM_READY) !=  SR_WSM_READY)
        {
            //check DQ 5
            Data_DQ5 = *SectorStart;
            if (Data_DQ5 & 0x00200020)
            {
                Data_DQ7= *SectorStart ;
                if (((Data_DQ5 & 0x00000020) && ((Data_DQ7 & 0x00000080)!= 0x000000080)) || ((Data_DQ5 & 0x00200000) && ((Data_DQ7 & 0x00800000) !=  0x00800000)))
                {
                    success = FALSE;
                    break;
                }
            }
        }
    }

    // Exit "Autoselect" mode by writing reset '0xF0' to bus
    BaseAddress[0x0] = 0x00F000F0;  

    FLASH_END_PROGRAMMING_FAST( "AM29DL_BS_32 EraseSector", SectorStart );       
    //
    //
    ///////////////////////////////////

    return success;        
}

void __section("SectionForFlashOperations") AM29DL_32_BS_Driver::Action_WriteWord( void* context, volatile CHIP_WORD* Address, CHIP_WORD Data )
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    MEMORY_MAPPED_NOR_BLOCK_CONFIG* config = (MEMORY_MAPPED_NOR_BLOCK_CONFIG*)context;
    
    volatile CHIP_WORD* BaseAddress = (CHIP_WORD *)config->BlockConfig.BlockDeviceInformation->Regions[0].Start;
     
    ///////////////////////////////////
    //
    //    
    FLASH_BEGIN_PROGRAMMING_FAST();
    // reset "Autoselect" mode by writing reset '0xF0' to bus
    BaseAddress[0x0]    = 0x00F000F0;  
    BaseAddress[0x0555] = 0x00AA00AA;
    BaseAddress[0x02AA] = 0x00550055;
    BaseAddress[0x0555] = 0x00A000A0;

    *Address = Data ;

    // wait for device to signal completion
    // break when the device signals completion by looking for Data (0xff erasure value) on I/O 7 (a 0 on I/O 7 indicates erasure in progress)
    // There is no timeout at here, if it fails, leave it for the system watchdog to reset it.
    {

        UINT32 Data_DQ5,Data_DQ7;

        while((*Address & SR_WSM_READY) !=  (Data & SR_WSM_READY))
        {
            //check DQ 5
            Data_DQ5 = *Address ;
            if (Data_DQ5 & 0x00200020)
            {
                Data_DQ7= *Address ;
                if (((Data_DQ5 & 0x00000020) && ((Data_DQ7 & 0x00000080)!= (Data &0x000000080))) || ((Data_DQ5 & 0x00200000) && ((Data_DQ7 & 0x00800000) != (Data & 0x00800000))))
                {
                    // Error writing the address
                     break;
                }
            }
        }
    }
    // Exit "Autoselect" mode by writing reset '0xF0' to bus
    BaseAddress[0x0] = 0x00F000F0;

    FLASH_END_PROGRAMMING_FAST( "AM29DL_BS_32 WriteWord", Address );    
    //
    //
    ///////////////////////////////////
}
#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata = "g_AM29DL_32_BS_DeviceTable"
#endif

struct IBlockStorageDevice g_AM29DL_32_BS_DeviceTable = 
{
    &AM29DL_32_BS_Driver::ChipInitialize,
    &AM29DL_32_BS_Driver::ChipUnInitialize,
    &AM29DL_32_BS_Driver::GetDeviceInfo,
    &AM29DL_32_BS_Driver::Read,
    &AM29DL_32_BS_Driver::Write,
    &AM29DL_32_BS_Driver::Memset,
    &AM29DL_32_BS_Driver::GetSectorMetadata,
    &AM29DL_32_BS_Driver::SetSectorMetadata,
    &AM29DL_32_BS_Driver::IsBlockErased,
    &AM29DL_32_BS_Driver::EraseBlock,
    &AM29DL_32_BS_Driver::SetPowerState,
    &AM29DL_32_BS_Driver::MaxSectorWrite_uSec,
    &AM29DL_32_BS_Driver::MaxBlockErase_uSec,     
};

#pragma arm section rodata 

