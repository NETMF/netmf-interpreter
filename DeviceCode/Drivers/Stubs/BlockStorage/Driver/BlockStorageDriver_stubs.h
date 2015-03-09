////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#include <tinyhal.h>
#include <tinyhal.h>

//--//
#ifndef _DRIVERS_BLOCKSTORAGEDRIVER_STUBS_H_
#define _DRIVERS_BLOCKSTORAGEDRIVER_STUBS_H_ 1

//--//

struct STUB_BlockStorage_Driver
{

    //--//

    static BOOL ChipInitialize( void* context );

    static BOOL ChipUnInitialize( void* context );

    static const BlockDeviceInfo* GetDeviceInfo( void* context );

    static BOOL Read( void* context, ByteAddress Address, UINT32 NumBytes, BYTE * pSectorBuff);
    
    static BOOL Write( void* context, ByteAddress Address, UINT32 NumBytes, BYTE * pSectorBuff, BOOL ReadModifyWrite);

    static BOOL Memset( void* context, ByteAddress Address, UINT8 Data, UINT32 NumBytes );

    static BOOL GetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata);

    static BOOL SetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata);

    static BOOL IsBlockErased( void* context, ByteAddress Address, UINT32 BlockLength );

    static BOOL EraseBlock( void* context, ByteAddress Address );

    static void SetPowerState( void* context, UINT32 State );

    static UINT32 MaxSectorWrite_uSec(void* context);

    static UINT32 MaxBlockErase_uSec(void* context);

    static BOOL ChipReadOnly( void* context, BOOL On, UINT32 ProtectionKey );

    static BOOL ReadProductID( void* context, FLASH_WORD& ManufacturerCode, FLASH_WORD& DeviceCode );
};

//--//

#endif // _DRIVERS_BLOCKSTORAGEDRIVER_STUBS_H_

