////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#include <tinyhal.h>
#include <tinyhal.h>

//--//

HAL_DblLinkedList<BlockStorageDevice> BlockStorageList::s_deviceList;
BlockStorageDevice*                   BlockStorageList::s_primaryDevice = NULL;

//--//

__weak void BlockStorageList::Initialize()
{
}

__weak BOOL BlockStorageList::InitializeDevices()
{
    return TRUE;        
}

__weak BOOL BlockStorageList::UnInitializeDevices()
{
    return TRUE;        
}

__weak BOOL BlockStorageList::AddDevice( BlockStorageDevice* pBSD, IBlockStorageDevice* vtable, void* config, BOOL Init)
{
    return TRUE;        
}

__weak BOOL BlockStorageList::RemoveDevice( BlockStorageDevice* pBSD, BOOL UnInit)
{
    return TRUE;        
}

__weak BlockStorageDevice* BlockStorageList::GetFirstDevice()
{ 
    return NULL; 
}

__weak BlockStorageDevice* BlockStorageList::GetNextDevice( BlockStorageDevice& device )
{ 
    return NULL; 
}

__weak UINT32 BlockStorageList::GetNumDevices()            
{ 
    return 0;  
}

__weak BOOL BlockStorageList::FindDeviceForPhysicalAddress( BlockStorageDevice** pBSD, UINT32 PhysicalAddress, ByteAddress &SectAddress)
{
    *pBSD = NULL;
    return FALSE;
}

__weak BOOL BlockStorageStream::Initialize(UINT32 blockUsage)
{
    return FALSE;
}

__weak BOOL BlockStorageStream::Initialize(UINT32 usage, BlockStorageDevice* pDevice)
{
    return FALSE;
}


__weak UINT32 BlockStorageStream::CurrentAddress() 
{
    return 0xFFFFFFFF;
}

__weak BOOL BlockStorageStream::PrevStream()
{
    return FALSE;
}

__weak BOOL BlockStorageStream::NextStream()
{
    return FALSE;
}

__weak BOOL BlockStorageStream::Seek( INT32 offset, SeekOrigin origin )
{
    return TRUE;
}

__weak BOOL BlockStorageStream::Erase( UINT32 length )
{
    return TRUE;
}

__weak BOOL BlockStorageStream::Write( UINT8* data  , UINT32 length )
{
    return TRUE;
}

__weak BOOL BlockStorageStream::ReadIntoBuffer( UINT8* pBuffer, UINT32 length )
{
    return TRUE;
}

__weak BOOL BlockStorageStream::Read( UINT8** ppBuffer, UINT32 length )
{
    return TRUE;
}


//--// 

__weak SectorAddress BlockDeviceInfo::PhysicalToSectorAddress( const BlockRegionInfo* pRegion, ByteAddress phyAddress ) const
{
    return phyAddress;
}

__weak BOOL BlockDeviceInfo::FindRegionFromAddress(ByteAddress Address, UINT32 &BlockRegionIndex, UINT32 &BlockRangeIndex ) const
{
    return FALSE;        
}

__weak BOOL BlockDeviceInfo::FindNextUsageBlock(UINT32 BlockUsage, ByteAddress &Address, UINT32 &BlockRegionIndex, UINT32 &BlockRangeIndex ) const
{
    return FALSE;
}

__weak BOOL BlockDeviceInfo::FindForBlockUsage(UINT32 BlockUsage, ByteAddress &Address, UINT32 &BlockRegionIndex, UINT32 &BlockRangeIndex ) const
{
    return FALSE;
}


//--// 

