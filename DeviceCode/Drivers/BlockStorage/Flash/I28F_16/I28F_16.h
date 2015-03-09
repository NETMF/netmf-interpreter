////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#include <tinyhal.h>
#include <tinyhal.h>

//--//
#ifndef _DRIVERS_BS_I28F_16_H_
#define _DRIVERS_BS_I28F_16_H_ 1

//--//

struct I28F_16_BS_Driver
{
    typedef UINT16 CHIP_WORD;

    //--//
    // the following are the constant for Intel I28F chips
    static const CHIP_WORD PROGRAM_WORD             = 0x0040;

    static const CHIP_WORD READ_STATUS_REGISTER     = 0x0070;
    static const CHIP_WORD SR_PROTECT_ERROR         = 0x0002;
    static const CHIP_WORD SR_VPP_ERROR             = 0x0008;
    static const CHIP_WORD SR_PROGRAM_ERROR         = 0x0010;
    static const CHIP_WORD SR_ERASE_ERROR           = 0x0020;
    static const CHIP_WORD SR_ERASE_SUSPENDED       = 0x0040;
    static const CHIP_WORD SR_WSM_READY             = 0x0080;

    static const CHIP_WORD CLEAR_STATUS_REGISTER    = 0x0050;

    static const CHIP_WORD ENTER_READ_ARRAY_MODE    = 0x00FF;

    static const CHIP_WORD READ_ID                  = 0x0090;
    static const CHIP_WORD LOCK_STATUS_LOCKED       = 0x0001;
    static const CHIP_WORD LOCK_STATUS_LOCKED_DOWN  = 0x0002;

    static const CHIP_WORD BLOCK_ERASE_SETUP        = 0x0020;

    static const CHIP_WORD BLOCK_ERASE_CONFIRM      = 0x00D0;

    static const CHIP_WORD LOCK_SETUP               = 0x0060;
    static const CHIP_WORD LOCK_LOCK_BLOCK          = 0x0001;
    static const CHIP_WORD LOCK_UNLOCK_BLOCK        = 0x00D0;
    static const CHIP_WORD LOCK_LOCK_DOWN_BLOCK     = 0x002F;

    static const CHIP_WORD CONFIG_SETUP             = 0x0060;       // Command to set up configuration word write
    static const CHIP_WORD CONFIG_WRITE             = 0x0003;       // Command to perform configuration word write (from LO 16 address lines)

    // Read Configuration Register definitions
    static const CHIP_WORD FLASH_CONFIG_RESERVED    = 0x0580;       // Bits which must always be set
    static const CHIP_WORD FLASH_CONFIG_ASYNC       = 0x8000;       // Asynchronous operation
    static const CHIP_WORD FLASH_CONFIG_SYNC        = 0x0000;       // Synchronous operation
    static const UINT32    FLASH_CONFIG_LAT_SHIFT   = 11;           // Shift for read latency (2-7 clocks)
    static const CHIP_WORD FLASH_CONFIG_DATA_HOLD_1 = 0x0000;       // Data hold for 1 clock
    static const CHIP_WORD FLASH_CONFIG_DATA_HOLD_2 = 0x0200;       // Data hold for 2 clocks
    static const CHIP_WORD FLASH_CONFIG_CLK_HI_EDGE = 0x0040;       // Clock active on rising edge
    static const CHIP_WORD FLASH_CONFIG_CLK_LW_EDGE = 0x0000;       // Clock active on falling edge
    static const CHIP_WORD FLASH_CONFIG_BURST_WRAP  = 0x0000;       // Data burst wraps
    static const CHIP_WORD FLASH_CONFIG_NO_WRAP     = 0x0008;       // Data burst does not wrap
    static const CHIP_WORD FLASH_CONFIG_BURST_8     = 0x0002;       // Data burst is 8 words long
    static const CHIP_WORD FLASH_CONFIG_BURST_16    = 0x0003;       // Data burst is 16 words long

    //--//

    static BOOL ChipInitialize( void* context );

    static BOOL ChipUnInitialize( void* context );

    static const BlockDeviceInfo* GetDeviceInfo( void* context );

    static BOOL Read( void* context, ByteAddress Address, UINT32 NumBytes, BYTE * pSectorBuff );

    static BOOL Write( void* context, ByteAddress Address, UINT32 NumBytes, BYTE * pSectorBuff, BOOL ReadModifyWrite );

    static BOOL Memset( void* context, ByteAddress Address, UINT8 Data, UINT32 NumBytes );

    static BOOL GetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata);

    static BOOL SetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata);

    static BOOL IsBlockErased( void* context, ByteAddress BlockStart, UINT32 BlockLength );

    static BOOL EraseBlock( void* context, ByteAddress Address );

    static void SetPowerState( void* context, UINT32 State );

    static UINT32 MaxSectorWrite_uSec( void* context );

    static UINT32 MaxBlockErase_uSec( void* context );

//--//

    static BOOL ChipReadOnly( void* context, BOOL On, UINT32 ProtectionKey );

    static BOOL ReadProductID( void* context, FLASH_WORD& ManufacturerCode, FLASH_WORD& DeviceCode );

private:
    static BOOL      WriteX( void* context, ByteAddress Address, UINT32 NumBytes, BYTE * pSectorBuff, BOOL ReadModifyWrite, BOOL fIncrementDataPtr );

    static void      Action_ReadID             ( volatile CHIP_WORD* SectorCheck, FLASH_WORD& ManufacturerCode, FLASH_WORD& DeviceCode );
    static CHIP_WORD Action_ReadLockStatus     ( volatile CHIP_WORD* SectorCheck                                                       );
    static void      Action_Unlock             ( volatile CHIP_WORD* SectorCheck                                                       );
    static void      Action_ClearStatusRegister( volatile CHIP_WORD* SectorCheck                                                       );
    static CHIP_WORD Action_EraseSector        ( void* context, volatile CHIP_WORD* Sector, MEMORY_MAPPED_NOR_BLOCK_CONFIG* FlashConfig                                 );
    static CHIP_WORD Action_WriteWord          ( volatile CHIP_WORD* Sector, CHIP_WORD Data, MEMORY_MAPPED_NOR_BLOCK_CONFIG* FlashConfig                 );

    //--//
};

//--//

#endif // _DRIVERS_BS_I28F_16_H_

