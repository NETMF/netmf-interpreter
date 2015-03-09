////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

//--//

#ifndef _DRIVERS_FLASH_AM29DL_BS_32_H_
#define _DRIVERS_FLASH_AM29DL_BS_32_H_ 1

//--//

struct AM29DL_32_BS_Driver
{
    typedef UINT32 CHIP_WORD;

    //--//

    static const CHIP_WORD SR_WSM_READY                = 0x00800080;
    static const CHIP_WORD SECTOR_WRITE_UNPROTECT      = 0x01;
    static const CHIP_WORD SECTOR_WRITE_PROTECT        = 0x00;

    //--//

    static BOOL ChipInitialize( void* context );

    static BOOL ChipUnInitialize( void* context );

    static const BlockDeviceInfo* GetDeviceInfo( void* context );

    static BOOL Read( void* context, ByteAddress Address, UINT32 NumBytes, BYTE * pSectorBuff );

    static BOOL Write( void* context, ByteAddress Address, UINT32 NumBytes, BYTE * pSectorBuff, BOOL ReadModifyWrite );

    static BOOL Memset( void* context, ByteAddress Address, UINT8 Data, UINT32 NumBytes );

    static BOOL GetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata);

    static BOOL SetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata);

    static BOOL IsBlockErased( void* context, ByteAddress address, UINT32 blockLength );

    static BOOL EraseBlock( void* context, ByteAddress Sector );

    static void SetPowerState( void* context, UINT32 State );

    static UINT32 MaxSectorWrite_uSec( void* context );

    static UINT32 MaxBlockErase_uSec( void* context );

//--//
    static BOOL ChipReadOnly( void* context, BOOL On, UINT32 ProtectionKey );

    static BOOL ReadProductID( void* context, FLASH_WORD& ManufacturerCode, FLASH_WORD& DeviceCode );

private:
    static BOOL WriteX( void* context, ByteAddress StartSector, UINT32 NumSectors,BYTE * pSectorBuff, BOOL ReadModifyWrite, BOOL fIncrementDataPtr );
    static void Action_ReadID     ( void* context, FLASH_WORD& ManufacturerCode, FLASH_WORD& DeviceCode );
    static void Action_Unprotect  ( void* context, volatile CHIP_WORD* BaseAddress                       );
    static BOOL Action_EraseSector( void* context, volatile CHIP_WORD* SectorStart                      );
    static void Action_WriteWord  ( void* context, volatile CHIP_WORD* Sector, CHIP_WORD Data           );
};

//--//

#endif // _DRIVERS_FLASH_AM29DL_BS_32_H_

