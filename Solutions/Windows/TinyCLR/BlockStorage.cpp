////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include <tinyhal.h>
#include "..\..\..\DeviceCode\Drivers\BlockStorage\WearLeveling\BS_WearLeveling.h"
#include "MemoryMappedFile.h"
#include "Win32Settings.h"

using namespace Microsoft::Win32;

extern Win32Settings g_CommandLineParams;

const UINT32 FLASH_BLOCK_COUNT = 128;
const UINT32 FLASH_BYTES_PER_BLOCK = 2048;
const UINT32 FLASH_BYTES_PER_SECTOR = 1;

static const BlockRange s_BlockRanges[] =
{   // NOTE: start and end of the range are "inclusive"
    { BlockRange::BLOCKTYPE_DEPLOYMENT,   0, 119 },  // DAT         240k
    { BlockRange::BLOCKTYPE_STORAGE_A , 120, 121 },  // storage A     4k
    { BlockRange::BLOCKTYPE_STORAGE_B , 122, 123 },  // storage B     4k
    { BlockRange::BLOCKTYPE_CONFIG    , 124, 127 },  // config        8k
};

static BlockRegionInfo s_BlockRegions[] = 
{
    {
        0,    // ByteAddress   Start;           // Starting Sector address (filled in by init whith memory mapped file)
        FLASH_BLOCK_COUNT,      // UINT32        NumBlocks;       // total number of blocks in this region
        FLASH_BYTES_PER_BLOCK,  // UINT32        BytesPerBlock;   // Total number of bytes per block
        ARRAYSIZE_CONST_EXPR(s_BlockRanges),
        s_BlockRanges,
    }
};

static const BlockDeviceInfo s_DeviceInfo=
{
    {  
        false, // BOOL Removable;
        true,  // BOOL SupportsXIP;
        false, // BOOL WriteProtected;
        true   // BOOL SupportsCopyBack
    },
    0,  // UINT32 MaxSectorWrite_uSec;
    0,  // UINT32 MaxBlockErase_uSec;
    FLASH_BYTES_PER_SECTOR,                // UINT32 BytesPerSector;     
    (256*1024),                            // UINT32 Size;
    ARRAYSIZE_CONST_EXPR(s_BlockRegions),  // UINT32 NumRegions;
    s_BlockRegions,                        // const BlockRegionInfo* Regions;
};

// memory mapped file to use as the emulated device NOR FLASH
MemoryMappedFile FlashMemoryFile;

BOOL MemoryMappedFileStorage_Initialize( void* context )
{
    auto pDeviceInfo = reinterpret_cast<BlockDeviceInfo*>( context );
    if( pDeviceInfo != &s_DeviceInfo )
        return FALSE;

    if( pDeviceInfo->Regions[0].Start != 0 )
        return TRUE;

    if(! FlashMemoryFile.Open( L"FLASH_MEMORY.bin", pDeviceInfo->Size ) )
        return FALSE;
    
    // const_cast needed as the storage structure design assumed XIP that
    // was at a location known at compile time. Since memory mapped files
    // can be mapped at any virtual address available that isn't a valid
    // assumption. The underlying data structure (e.g. s_BlockRegions is
    // not qualified as const so the "const" only applies to normal access
    // through the DeviceInfo
    auto pRegions = const_cast<BlockRegionInfo*>( pDeviceInfo->Regions );
    pRegions[0].Start = reinterpret_cast<ByteAddress>( FlashMemoryFile.GetDataPointer() );
    if( pDeviceInfo->Regions[0].Start == 0 )
        return FALSE;

    return pDeviceInfo->Regions[0].Start != 0 ? TRUE : FALSE;
}

BOOL MemoryMappedFileStorage_Uninitialize( void* context )
{
    auto pDeviceInfo = reinterpret_cast<BlockDeviceInfo*>( context );
    auto pRegions = const_cast<BlockRegionInfo*>( pDeviceInfo->Regions );
    pRegions[0].Start = 0;
    return FlashMemoryFile.Close();
}

const BlockDeviceInfo* MemoryMappedFileStorage_GetDeviceInfo( void* context )
{
    auto pDeviceInfo = reinterpret_cast<BlockDeviceInfo*>( context );
    return pDeviceInfo;
}

BOOL MemoryMappedFileStorage_Read( void* context, ByteAddress Address, UINT32 NumBytes, BYTE* pSectorBuff )
{
    auto pDeviceInfo = reinterpret_cast<BlockDeviceInfo*>( context );
    auto pFlashBase = FlashMemoryFile.GetDataPointer();
    auto pFlashExclusiveEnd = pFlashBase + pDeviceInfo->Size;
    auto ptr = reinterpret_cast<uint8_t*>( Address );
    if( ptr == nullptr )
        return FALSE;
    
    if(  ptr < pFlashBase || ( ptr + NumBytes >= pFlashExclusiveEnd ) )
        return FALSE;

    memcpy( pSectorBuff, ptr, NumBytes);
    return TRUE;
}

BOOL MemoryMappedFileStorage_Write( void* context, ByteAddress Address, UINT32 NumBytes, BYTE * pSectorBuff, BOOL ReadModifyWrite )
{
    auto pDeviceInfo = reinterpret_cast<BlockDeviceInfo*>( context );
    auto pFlashBase = FlashMemoryFile.GetDataPointer();
    auto pFlashExclusiveEnd = pFlashBase + pDeviceInfo->Size;
    auto ptr = reinterpret_cast<uint8_t*>( Address );
    if( ptr == nullptr )
        return FALSE;
    
    if(  ptr < pFlashBase || ( ptr + NumBytes >= pFlashExclusiveEnd ) )
        return FALSE;

    memcpy( ptr, pSectorBuff, NumBytes);
    return TRUE;
}

BOOL MemoryMappedFileStorage_Memset( void* context, ByteAddress Address, UINT8 Data, UINT32 NumBytes )
{
    auto pDeviceInfo = reinterpret_cast<BlockDeviceInfo*>( context );
    auto pFlashBase = FlashMemoryFile.GetDataPointer();
    auto pFlashExclusiveEnd = pFlashBase + pDeviceInfo->Size;
    auto ptr = reinterpret_cast<uint8_t*>( Address );
    if( ptr == nullptr )
        return FALSE;
    
    if(  ptr < pFlashBase || ( ptr + NumBytes >= pFlashExclusiveEnd ) )
        return FALSE;

    memset( ptr, Data, NumBytes);
    return TRUE;
}

BOOL MemoryMappedFileStorage_GetSectorMetadata( void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata )
{
    return FALSE;
}

BOOL MemoryMappedFileStorage_SetSectorMetadata( void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata )
{
    return FALSE;
}

BOOL MemoryMappedFileStorage_IsBlockErased( void* context, ByteAddress Address, UINT32 BlockLength )
{
    // just return FALSE to indicate not erased. Since erase operations are faster
    // than scanning the entire block to see if it is in fact all erase (e.g. 0xFF)
    return FALSE;
}

BOOL MemoryMappedFileStorage_EraseBlock( void* context, ByteAddress Address )
{
    // REVIEW: Should this verify Address actually begins on a block?
    // While it doesn't matter in this implementation, it might be helpful to catch 
    // errors where upper layer code is using non block alligned addresses.
    return MemoryMappedFileStorage_Memset( context, Address, 0xFF, FLASH_BYTES_PER_BLOCK );
}

void MemoryMappedFileStorage_SetPowerState( void* context, UINT32 State )
{
}

UINT32 MemoryMappedFileStorage_MaxSectorWrite_uSec( void* context )
{
    return 0;
}

UINT32 MemoryMappedFileStorage_MaxBlockErase_uSec( void* context )
{
    return 0;
}

BlockStorageDevice StorageDeviceWrapper;

struct IBlockStorageDevice Emulator_BS_DeviceTable =
{
    &MemoryMappedFileStorage_Initialize,
    &MemoryMappedFileStorage_Uninitialize,
    &MemoryMappedFileStorage_GetDeviceInfo,
    &MemoryMappedFileStorage_Read,
    &MemoryMappedFileStorage_Write,
    &MemoryMappedFileStorage_Memset,
    &MemoryMappedFileStorage_GetSectorMetadata,
    &MemoryMappedFileStorage_SetSectorMetadata,
    &MemoryMappedFileStorage_IsBlockErased,
    &MemoryMappedFileStorage_EraseBlock,
    &MemoryMappedFileStorage_SetPowerState,
    &MemoryMappedFileStorage_MaxSectorWrite_uSec,
    &MemoryMappedFileStorage_MaxBlockErase_uSec,    
};

void BlockStorage_AddDevices()
{
    // since this implementation only supports a single device the "context" is nullptr
    BlockStorageList::AddDevice( &StorageDeviceWrapper
                               , &Emulator_BS_DeviceTable
                               , const_cast<BlockDeviceInfo*>( &s_DeviceInfo )
                               , true
                               );
}

