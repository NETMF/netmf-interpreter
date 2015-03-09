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

//--//

#ifndef _DRIVERS_BS_SST39WF_16_H_
#define _DRIVERS_BS_SST39WF_16_H_ 1

//--//


struct SST39WF_16_BS_Driver
{
    typedef UINT16 CHIP_WORD;

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

    	static BOOL WriteX( void* context, ByteAddress Address, UINT32 NumBytes, BYTE * pSectorBuff, BOOL ReadModifyWrite, BOOL fIncrementDataPtr );

	static void Action_ReadID	( volatile CHIP_WORD* SectorCheck, FLASH_WORD& ManufacturerCode, FLASH_WORD& DeviceCode );

	static CHIP_WORD Action_ReadLockStatus     ( volatile CHIP_WORD* SectorCheck                                                       );	

    	static void Action_Unlock             ( volatile CHIP_WORD* SectorCheck );
	
    	static void Action_ClearStatusRegister( volatile CHIP_WORD* SectorCheck );
	
    	static void Action_EraseBlock        ( void* context, volatile CHIP_WORD* Sector );
	
    	static void Action_WriteWord          ( void* context, volatile CHIP_WORD* Sector, CHIP_WORD Data );
	
};

//--//

#endif // _DRIVERS_BS_SST39WF_16_H_
