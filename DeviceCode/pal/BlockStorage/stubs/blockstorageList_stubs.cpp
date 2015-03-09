////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#include <tinyhal.h>
#include <tinyhal.h>

//--//

HAL_DblLinkedList<BlockStorageDevice> BlockStorageList::s_deviceList;
BlockStorageDevice*                   BlockStorageList::s_primaryDevice = NULL;

//--//

void BlockStorageList::Initialize()
{
}

BOOL BlockStorageList::InitializeDevices()
{
    return TRUE;        
}

BOOL BlockStorageList::UnInitializeDevices()
{
    return TRUE;        
}

BOOL BlockStorageList::AddDevice( BlockStorageDevice* pBSD, IBlockStorageDevice* vtable, void* config, BOOL Init)
{
    return TRUE;        
}

BOOL BlockStorageList::RemoveDevice( BlockStorageDevice* pBSD, BOOL UnInit)
{
    return TRUE;        
}

BlockStorageDevice* BlockStorageList::GetFirstDevice()
{ 
    return NULL; 
}

BlockStorageDevice* BlockStorageList::GetNextDevice( BlockStorageDevice& device )
{ 
    return NULL; 
}

UINT32 BlockStorageList::GetNumDevices()            
{ 
    return 0;  
}

BOOL BlockStorageList::FindDeviceForPhysicalAddress( BlockStorageDevice** pBSD, UINT32 PhysicalAddress, ByteAddress &SectAddress)
{
    *pBSD = NULL;
    return FALSE;
}

BOOL BlockStorageStream::Initialize(UINT32 blockUsage)
{
    return FALSE;
}

BOOL BlockStorageStream::Initialize(UINT32 usage, BlockStorageDevice* pDevice)
{
    return FALSE;
}


UINT32 BlockStorageStream::CurrentAddress() 
{
    return 0xFFFFFFFF;
}

BOOL BlockStorageStream::PrevStream()
{
    return FALSE;
}

BOOL BlockStorageStream::NextStream()
{
    return FALSE;
}

BOOL BlockStorageStream::Seek( INT32 offset, SeekOrigin origin )
{
    return TRUE;
}

BOOL BlockStorageStream::Erase( UINT32 length )
{
    return TRUE;
}

BOOL BlockStorageStream::Write( UINT8* data  , UINT32 length )
{
    return TRUE;
}

BOOL BlockStorageStream::ReadIntoBuffer( UINT8* pBuffer, UINT32 length )
{
    return TRUE;
}

BOOL BlockStorageStream::Read( UINT8** ppBuffer, UINT32 length )
{
    return TRUE;
}


//--// 

SectorAddress BlockDeviceInfo::PhysicalToSectorAddress( const BlockRegionInfo* pRegion, ByteAddress phyAddress ) const
{
    return phyAddress;
}

BOOL BlockDeviceInfo::FindRegionFromAddress(ByteAddress Address, UINT32 &BlockRegionIndex, UINT32 &BlockRangeIndex ) const
{
    return FALSE;        
}

BOOL BlockDeviceInfo::FindNextUsageBlock(UINT32 BlockUsage, ByteAddress &Address, UINT32 &BlockRegionIndex, UINT32 &BlockRangeIndex ) const
{
    return FALSE;
}

BOOL BlockDeviceInfo::FindForBlockUsage(UINT32 BlockUsage, ByteAddress &Address, UINT32 &BlockRegionIndex, UINT32 &BlockRangeIndex ) const
{
    return FALSE;
}


//--// 

