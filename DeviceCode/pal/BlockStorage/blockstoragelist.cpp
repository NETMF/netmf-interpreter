////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#include <Tinyhal.h>
#include <Tinyhal.h>

//--//

HAL_DblLinkedList<BlockStorageDevice> BlockStorageList::s_deviceList;
BlockStorageDevice*                   BlockStorageList::s_primaryDevice = NULL;
BOOL                                  BlockStorageList::s_Initialized = FALSE;

//--// 

void BlockStorageList::Initialize()
{
    s_deviceList.Initialize();
    s_primaryDevice = NULL;
    s_Initialized = TRUE;
}

// walk through list of devices and calls Init() function
BOOL BlockStorageList::InitializeDevices()
{
    UINT32 regionIndex, rangeIndex;
    ByteAddress Address;
   
    BlockStorageDevice* block = s_deviceList.FirstNode();
        
    if(block == NULL) 
    {
#if defined(PLATFORM_ARM)
        debug_printf( "There are no block storage devices to initialize" );  
#endif
        return FALSE;
    }

    UINT32 BlockUsage = BlockUsage::CONFIG;

    BOOL success = TRUE;
    
    if(s_primaryDevice == NULL)
    {
        // initialize all the "Static" block storage 
        // that is all the non-removeable block storage.
        while(block->Next())
        {
            success = block->InitializeDevice() && success; // even if success == FALSE, InitalizeDevice() will still get called

            if (block->FindForBlockUsage( BlockUsage, Address, regionIndex, rangeIndex ))
            {
                if (s_primaryDevice == NULL)
                {
                    s_primaryDevice = block;
                    
                }    
                else
                {
                    // there must be one and only one primary device
                    return FALSE;
                }
            }

            block = block->Next();

        }
        if (s_primaryDevice == NULL) return FALSE;

    }
    else
    {
        return FALSE;
    }

    return success;        

}

BOOL BlockStorageList::UnInitializeDevices()
{
    BOOL success = TRUE;
    
    BlockStorageDevice* block = s_deviceList.FirstNode();
    BlockStorageDevice* curBlock;

    if(s_primaryDevice != NULL)
    {
        while(block->Next())
        {
            success = block->UninitializeDevice() && success; // even if success == FALSE, UninitalizeDevice() will still get called

            curBlock = block;
            
            block = block->Next();
            // unlink the devices and it will addback
            curBlock ->Unlink();
        }

        s_primaryDevice = NULL;
    }

    return success;        
}


BOOL BlockStorageList::AddDevice( BlockStorageDevice* pBSD, IBlockStorageDevice* vtable, void* config, BOOL init )
{   
    BOOL success = TRUE;

    ASSERT(vtable);
    ASSERT(pBSD);
 
    // initialize the members of the block storage driver
    pBSD->m_BSD     = vtable;
    pBSD->m_context = config;

    // init the dblinkednode;
    pBSD->Initialize();
    if(init)
    {
        success = pBSD->InitializeDevice();
    }

    // only add teh device if initialization was successful, when requested at all
    if( success)
    {
        s_deviceList.LinkAtBack( pBSD );  return TRUE;
    }
    
    return FALSE;        
}

BOOL BlockStorageList::RemoveDevice( BlockStorageDevice* pBSD, BOOL UnInit)
{
    if(pBSD->IsLinked())
    {    
        pBSD->Unlink();
        
        if (UnInit)
        {
            pBSD->UninitializeDevice();
        }

        return TRUE;        
    }
    
    return FALSE;        
}

BOOL BlockStorageList::FindDeviceForPhysicalAddress( BlockStorageDevice** pBSD, UINT32 PhysicalAddress, ByteAddress &BlockAddress)
{
    *pBSD = NULL;
    
    if(!s_Initialized) return FALSE;
    
    BlockStorageDevice* block = s_deviceList.FirstNode();

    // this has to add to make metadataprocessor happy
    if(!block) return FALSE;

    while(block->Next())
    {
        const BlockDeviceInfo* pDeviceInfo = block->GetDeviceInfo();
            
        for(UINT32 i=0; i < pDeviceInfo->NumRegions; i++)
        {
            const BlockRegionInfo* pRegion = &pDeviceInfo->Regions[i];
            
            if(pRegion->Start <= PhysicalAddress && PhysicalAddress < (pRegion->Start + pRegion->NumBlocks * pRegion->BytesPerBlock))
            {
                *pBSD = block; 

                // get block start address 
                BlockAddress = (ByteAddress)((PhysicalAddress - pRegion->Start) / pRegion->BytesPerBlock);
                BlockAddress *= pRegion->BytesPerBlock;
                BlockAddress += pRegion->Start;

                return TRUE;
            }
        }

        block = block->Next();
        
    }
    return FALSE;

}

BlockStorageDevice* BlockStorageList::GetFirstDevice()
{ 
    if(!s_Initialized) return NULL;
    
    return s_deviceList.FirstValidNode(); 
}

BlockStorageDevice* BlockStorageList::GetNextDevice( BlockStorageDevice& device )
{ 
    if(!s_Initialized) return NULL;
    
    BlockStorageDevice* nextDevice = device.Next();

    if(nextDevice && nextDevice->Next())
    {    
        return  nextDevice; 
    }

    return NULL;
}

UINT32 BlockStorageList::GetNumDevices()            
{ 
    if(!s_Initialized) return 0;
    
    return s_deviceList.NumOfNodes();  
}

//--//

BOOL BlockStorageStream::Initialize(UINT32 usage)
{
    return Initialize( usage, NULL );
}

BOOL BlockStorageStream::Initialize(UINT32 usage, BlockStorageDevice* pDevice)
{
    if(BlockStorageList::s_primaryDevice == NULL) return FALSE;
    

    memset(this, 0, sizeof(BlockStorageStream));
    Usage = usage;

    if(pDevice != NULL && pDevice->Next() != NULL)
    {
        RangeIndex = 0xFFFFFFFF;        // when Device != NULL the RangeIndex is increased in NextStream
        Device     = pDevice;

        const BlockDeviceInfo* pDevInfo = this->Device->GetDeviceInfo();
        
        if( pDevInfo->Attribute.SupportsXIP ) Flags |= c_BlockStorageStream__XIP;
        else Flags &= ~c_BlockStorageStream__XIP;
    }

    return NextStream();
}

UINT32 BlockStorageStream::CurrentAddress() 
{ 
    return BaseAddress + CurrentIndex; 
}

BOOL BlockStorageStream::PrevStream()
{
    if(this->Device == NULL || this->Device->Prev() == NULL) return FALSE;

    const BlockRegionInfo* pRegion;
    const BlockDeviceInfo* pDevInfo = this->Device->GetDeviceInfo();

    BlockStorageStream orig;

    memcpy(&orig, this, sizeof(orig));

    do
    {
        bool fLastIndex = false;
        
        if(RangeIndex == 0) 
        {
            if(RegionIndex == 0)
            {
                this->Device = this->Device->Prev();
                
                if(Device == NULL || Device->Prev() == NULL)
                {
                    memcpy(this, &orig, sizeof(orig));
                    return FALSE;
                }
                
                pDevInfo = this->Device->GetDeviceInfo();
            
                RegionIndex = pDevInfo->NumRegions - 1;
            }
            else
            {
                RegionIndex--;
            }

            fLastIndex = true;
        }
        else
        {
            RangeIndex--;
        }

        pRegion = &pDevInfo->Regions[RegionIndex];

        if(fLastIndex)
        {
            RangeIndex = pRegion->NumBlockRanges - 1;
        }

    } while( pRegion->BlockRanges[RangeIndex].GetBlockUsage() != Usage );

    if( pDevInfo->Attribute.SupportsXIP ) Flags |= c_BlockStorageStream__XIP;
    else Flags &= ~c_BlockStorageStream__XIP;
    
    this->BlockLength  = pRegion->BytesPerBlock;
    this->BaseAddress  = pRegion->Start + pRegion->BlockRanges[RangeIndex].StartBlock * BlockLength;
    this->Length       = pRegion->BlockRanges[RangeIndex].GetBlockCount() * BlockLength;
    this->CurrentIndex = 0;

    return TRUE;
}

BOOL BlockStorageStream::NextStream()
{
    BOOL fFirstDevice = (this->Device == NULL);

    BlockStorageStream orig;

    memcpy(&orig, this, sizeof(orig));

    if(fFirstDevice)
    {
        this->Device = BlockStorageList::GetFirstDevice();

        if(this->Device == NULL || Device->Next() == NULL) return FALSE;

        const BlockDeviceInfo* pDevInfo = this->Device->GetDeviceInfo();
        
        if( pDevInfo->Attribute.SupportsXIP ) Flags |= c_BlockStorageStream__XIP;
        else Flags &= ~c_BlockStorageStream__XIP;
    }
    else
    {
        // move to the next range
        RangeIndex++;
    }

    while(TRUE)
    {
        if(this->Device == NULL || this->Device->Next() == NULL) break;

        if(this->Device->FindNextUsageBlock(this->Usage, BaseAddress, RegionIndex, RangeIndex))
        {
            const BlockRegionInfo* pRegion = &this->Device->GetDeviceInfo()->Regions[RegionIndex];

            this->CurrentIndex = 0;
            this->Length       = pRegion->BlockRanges[RangeIndex].GetBlockCount() * pRegion->BytesPerBlock;
            this->BlockLength  = pRegion->BytesPerBlock;
            this->CurrentUsage = (pRegion->BlockRanges[RangeIndex].RangeType & BlockRange::USAGE_MASK);
            return TRUE;
        }

        this->Device = this->Device->Next();

        if(Device->Next() != NULL)
        {
            const BlockDeviceInfo* pDevInfo = this->Device->GetDeviceInfo();
        
            if( pDevInfo->Attribute.SupportsXIP ) Flags |= c_BlockStorageStream__XIP;
            else Flags &= ~c_BlockStorageStream__XIP;
        }
        

        RegionIndex = 0;
        RangeIndex  = 0;
    }

    // go back to original stream if no others can be found
    memcpy(this, &orig, sizeof(orig)); 

    return FALSE;
}

BOOL BlockStorageStream::Seek( INT32 offset, SeekOrigin origin )
{
    INT32 seekIndex;
    
    switch(origin)
    {
        case SeekBegin:
            seekIndex = 0;
            break;

        case SeekCurrent:
            seekIndex = CurrentIndex;
            break;

        case SeekEnd:
            seekIndex = Length;
            break;

        default:

            return FALSE;
    }
    
    if(offset == STREAM_SEEK_NEXT_BLOCK || offset == STREAM_SEEK_PREV_BLOCK)
    {
        INT32 blkOffset = seekIndex % this->BlockLength;

        if(offset == STREAM_SEEK_NEXT_BLOCK) offset = BlockLength - blkOffset;
        else if(blkOffset != 0)              offset = -blkOffset; 
        else                                 offset = -(INT32)BlockLength;
    }

    seekIndex += offset;
    
    while(seekIndex >= (INT32)Length)
    {
        seekIndex -= Length; // subtract the length of the current stream to get the correct offset for the new stream
        
        if(!NextStream()) return FALSE;
    }
    
    while(seekIndex < 0)
    {
        if(!PrevStream()) return FALSE;

        seekIndex += Length; // add the length of the new stream to get the correct offset for the new stream
    }

    this->CurrentIndex = seekIndex;

    return TRUE;
}

BOOL BlockStorageStream::Write( UINT8* data  , UINT32 length )
{
    if(this->Device == NULL || Device->Next() == NULL || !data) return FALSE;

    UINT8* pData = data;

    while(length > 0)
    {
        if(CurrentIndex == Length)
        {
            if(!NextStream()) return FALSE;
        }

        int writeLen = length;
        
        if((CurrentIndex + writeLen) > Length)
        {
            writeLen = Length - CurrentIndex;
        }
        
        if(!this->Device->Write( this->CurrentAddress(), writeLen, pData, IsReadModifyWrite()))
        {
            return FALSE;
        }

        pData        += writeLen;
        length       -= writeLen;
        CurrentIndex += writeLen;
    }

    return TRUE;
}

BOOL BlockStorageStream::Erase( UINT32 length )
{
    BOOL fRet = TRUE;
    
    if(this->Device == NULL || Device->Next() == NULL) return FALSE;

    BlockStorageStream orig;
    memcpy(&orig, this, sizeof(orig));

    if(CurrentIndex == Length)
    {
        if(!NextStream()) return FALSE;
    }

    INT32 len = (INT32)length;

    while(len > 0)
    {
        if(!this->Device->EraseBlock( this->CurrentAddress() )) return FALSE;

        len -= this->BlockLength;

        if(len > 0 && !Seek( BlockStorageStream::STREAM_SEEK_NEXT_BLOCK, BlockStorageStream::SeekCurrent ))
        {
            if(!NextStream())
            {
                fRet = FALSE;
                break;
            }
        }
    }

    // always return stream back to its original position for erases
    memcpy(this, &orig, sizeof(orig));

    return fRet;
}

BOOL BlockStorageStream::ReadIntoBuffer( UINT8* pBuffer, UINT32 length )
{

    if(!IsXIP())
    {
        return Read(&pBuffer, length);
    }
    else
    {
        UINT8* pTmp = NULL;

        if(Read(&pTmp, length))
        {
            memcpy(pBuffer, pTmp, length);

            return TRUE;
        }
    }
    
    return FALSE;
}

BOOL BlockStorageStream::Read( UINT8** ppBuffer, UINT32 length )
{
    if(this->Device == NULL || Device->Next() == NULL || !ppBuffer) return FALSE;

    const BlockDeviceInfo* pDevInfo = this->Device->GetDeviceInfo();
    UINT8* pBuffer = *ppBuffer;
    bool fXIPFound = false;

    while(length > 0)
    {
        int readLen = length;

        // find the next stream if we are at the end of this one
        if(CurrentIndex == Length)
        {
            if(!NextStream()) return FALSE;
        }

        if((CurrentIndex + readLen) > Length)
        {
            readLen = Length - CurrentIndex;
        }

        if(pDevInfo->Attribute.SupportsXIP)
        {
            //
            // This assumes contiguous USAGE blocks, not much else we can do with XIP
            //
            if(!fXIPFound)
            {
                *ppBuffer = (UINT8*)(this->CurrentAddress());
                
                fXIPFound = true;
            }
        }
        else
        {
            if(!this->Device->Read( this->CurrentAddress(), readLen, pBuffer ))
            {
                return FALSE;
            }
        }

        length       -= readLen;
        pBuffer      += readLen;
        CurrentIndex += readLen;
        
    }

    return TRUE;
}

BOOL BlockStorageStream::IsErased( UINT32 length )
{
    UINT32 bkupIdx   = this->CurrentIndex;
    INT32  rem       = bkupIdx % this->BlockLength;
    UINT8  compValue = this->Device->GetDeviceInfo()->Attribute.ErasedBitsAreZero ? 0 : 0xFF;
    INT32  len       = (INT32)length;

    if(rem != 0)
    {
        if(length > this->BlockLength)
        {
            rem = this->BlockLength - rem;
            len -= rem;
        }
        else
        {
            rem = length;
            len = 0;
        }
        
        while(rem > 0)
        {
            UINT8 tmp[512];
            INT32 left = (rem < (INT32)sizeof(tmp) ? rem : (INT32)sizeof(tmp));

            this->ReadIntoBuffer(tmp, left);

            for(int i=0; i<left; i++)
            {
                if(tmp[i] != compValue)
                {
                    return FALSE;
                }
            }

            rem -= left;
        }
    }

    while(len >= (INT32)this->BlockLength)
    {
        if(!this->Device->IsBlockErased(this->BaseAddress + this->CurrentIndex, this->BlockLength)) return FALSE;

        len                -= this->BlockLength;
        this->CurrentIndex += this->BlockLength;
    }

    while(len > 0)
    {
        UINT8 tmp[512];
        INT32 left = (len < (INT32)sizeof(tmp) ? len : (INT32)sizeof(tmp));

        this->ReadIntoBuffer(tmp, left);

        for(int i=0; i<left; i++)
        {
            if(tmp[i] != compValue)
            {
                return FALSE;
            }
        }

        len -= left;
    }

    this->CurrentIndex = bkupIdx;

    return TRUE;
}

//--// 

SectorAddress BlockDeviceInfo::PhysicalToSectorAddress( const BlockRegionInfo* pRegion, ByteAddress phyAddress ) const
{
    return (phyAddress - pRegion->Start) / this->BytesPerSector;
}

BOOL BlockDeviceInfo::FindRegionFromAddress(ByteAddress Address, UINT32 &BlockRegionIndex, UINT32 &BlockRangeIndex ) const
{
    const BlockRegionInfo *pRegion;

    BlockRegionIndex = 0;

    for(UINT32 i = 0; i < this->NumRegions;  i++)
    {
        pRegion = &this->Regions[i];

        if(pRegion->Start <= Address && Address < (pRegion->Start + pRegion->NumBlocks * pRegion->BytesPerBlock))
        {
            UINT32 endRangeAddr = pRegion->Start + pRegion->BytesPerBlock * pRegion->BlockRanges[0].StartBlock;
            
            for(UINT32 j =0; j < pRegion->NumBlockRanges; j++)
            {
                endRangeAddr += pRegion->BytesPerBlock * pRegion->BlockRanges[j].GetBlockCount();
                
                if(Address < endRangeAddr)
                {
                    BlockRegionIndex = i;
                    BlockRangeIndex  = j;
                    return TRUE;
                }
            }
        }
    }
    
    return FALSE;        
}

BOOL BlockDeviceInfo::FindForBlockUsage(UINT32 BlockUsage, ByteAddress &Address, UINT32 &BlockRegionIndex, UINT32 &BlockRangeIndex ) const
{
    Address = 0;
    BlockRegionIndex = 0;
    BlockRangeIndex = 0;
    
    return FindNextUsageBlock( BlockUsage, Address, BlockRegionIndex, BlockRangeIndex );
}

BOOL BlockDeviceInfo::FindNextUsageBlock(UINT32 BlockUsage, ByteAddress &Address, UINT32 &BlockRegionIndex, UINT32 &BlockRangeIndex ) const
{
    const BlockRegionInfo *pRegion;

    for(UINT32 i = BlockRegionIndex; i < this->NumRegions; i++)
    {
        pRegion = &this->Regions[i];

        for( int j = BlockRangeIndex; j < (int)pRegion->NumBlockRanges; j++)
        {
            const BlockRange *pRange = &pRegion->BlockRanges[j];
            
            if ((pRange->RangeType & BlockRange::USAGE_MASK) == BlockUsage || BlockUsage == BlockUsage::ANY)
            {
                Address = pRegion->BlockAddress( pRange->StartBlock );

                BlockRegionIndex = i;

                BlockRangeIndex  = j;

                return TRUE;
            }
        }
        BlockRangeIndex = 0;
    }
    
    return FALSE;
}

