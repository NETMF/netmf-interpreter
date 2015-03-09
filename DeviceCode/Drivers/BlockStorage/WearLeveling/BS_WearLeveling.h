////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#include <tinyhal.h>

//--//
#ifndef _DRIVERS_BS_WEARLEVELING_H_
#define _DRIVERS_BS_WEARLEVELING_H_ 1

struct BS_WL_BlockCache
{
    UINT16 VirtualBlockIndex;
    UINT16 PhysicalBlockIndex;
};

//
// Sector meta data should only be 16 bytes
//
ADS_PACKED 
struct GNU_PACKED WL_SectorMetadata
{
    WORD  wBlockOffset[2];      // block offsets
    WORD  wOwnerBlock;          // Backwards link to the owner block (the block that maps to this one)
    WORD  wMappedSectorOffset;  // DirectMap offset for sector
    WORD  wLinkedSectorOffset;  // LinkedMap offset for sector
    BYTE  bSectorFlags;         // sector flags
    BYTE  bBlockFlags;          // Block flags (only on the first sector)
    UINT32 CRC;                 // CRC check for sector

    //--//
    
    const static UINT16 c_FREE_LINK_INDEX = 0xFFFF;
    const static UINT16 c_DEAD_LINK_INDEX = 0x0000;

private:
    static const UINT32 c_BAD_SECTOR_FLAG      = 0x01;
    static const UINT32 c_READONLY_SECTOR_FLAG = 0x02;
    static const UINT32 c_TRASH_SECTOR_FLAG    = 0x04;
    static const UINT32 c_INUSE_SECTOR_FLAG    = 0x08;
    static const UINT32 c_MAPPED_SECTOR_FLAG   = 0x10;
    static const UINT32 c_SECTOR_FLAG_MASK     = 0x1F;

    // block status (on first sector of each block only)
    static const UINT32 c_BADREP_BLOCK_FLAG         = 0x01;
    static const UINT32 c_INVALID_OWNER_BLOCK_FLAG  = 0x02;
    static const UINT32 c_TRASH_BLOCK_FLAG          = 0x04;
    static const UINT32 c_INUSE_BLOCK_FLAG          = 0x08;
    static const UINT32 c_VALID_BLOCK_OFFSET        = 0x10;
    static const UINT32 c_DIRTY_BLOCK_FLAG          = 0x20;
    static const UINT32 c_FORMAT_BLOCK_FLAG         = 0x80;
    static const UINT32 c_BLOCK_FLAG_MASK           = 0x3F;


    //--//

public:
    void SetSectorInUse()        { bSectorFlags &= (~c_INUSE_SECTOR_FLAG);     }
    void SetSectorReadonly()     { bSectorFlags &= (~c_READONLY_SECTOR_FLAG);  }
    void SetSectorBad()          { bSectorFlags &= (~c_BAD_SECTOR_FLAG);       }
    void SetSectorTrash()        { bSectorFlags &= (~c_TRASH_SECTOR_FLAG);     }
    void SetSectorMapped()       { bSectorFlags &= (~c_MAPPED_SECTOR_FLAG);    }

    BOOL IsValidSectorMap()      { return (wMappedSectorOffset != c_FREE_LINK_INDEX); }
    BOOL IsValidSectorLink()     { return (wLinkedSectorOffset != c_FREE_LINK_INDEX); }
    
    BOOL IsSectorTrash()         { return (c_TRASH_SECTOR_FLAG     == ((~bSectorFlags) & c_TRASH_SECTOR_FLAG    )); }   
    BOOL IsSectorReadOnly()      { return (c_READONLY_SECTOR_FLAG  == ((~bSectorFlags) & c_READONLY_SECTOR_FLAG )); }   
    BOOL IsSectorBad()           { return (c_BAD_SECTOR_FLAG       == ((~bSectorFlags) & c_BAD_SECTOR_FLAG      )); }        
    BOOL IsSectorInUse()         { return (c_INUSE_SECTOR_FLAG     == ((~bSectorFlags) & c_INUSE_SECTOR_FLAG    )); }        
    BOOL IsSectorMapped()        { return (c_MAPPED_SECTOR_FLAG    == ((~bSectorFlags) & c_MAPPED_SECTOR_FLAG   )); }
    BOOL IsSectorFree()          { return (c_SECTOR_FLAG_MASK      == (( bSectorFlags) & c_SECTOR_FLAG_MASK     )); }
    BOOL IsSectorDirty()         { return CRC != 0xFFFFFFFF; }


    void SetBlockBad()           { bBlockFlags  = 0;                             }
    void SetBlockTrash()         { bBlockFlags &= (~c_TRASH_BLOCK_FLAG        ); }
    void SetBlockDirty()         { bBlockFlags &= (~c_DIRTY_BLOCK_FLAG        ); }
    void SetBlockFormated()      { bBlockFlags &= (~c_FORMAT_BLOCK_FLAG       ); }
    void SetBlockInUse()         { bBlockFlags &= (~c_INUSE_BLOCK_FLAG        ); }
    void SetBadBlockReplacement(){ bBlockFlags &= (~c_BADREP_BLOCK_FLAG       ); }
    void SetOwnerBlockInvalid()  { bBlockFlags &= (~c_INVALID_OWNER_BLOCK_FLAG); }
    void SetValidBlockOffset()   { bBlockFlags &= (~c_VALID_BLOCK_OFFSET      ); }

    BOOL IsValidOwnerBlock()     { return ((wOwnerBlock != c_FREE_LINK_INDEX) && (c_INVALID_OWNER_BLOCK_FLAG != ((~bBlockFlags) & c_INVALID_OWNER_BLOCK_FLAG))); } 
    BOOL IsBadBlock()            { return (0 == bBlockFlags); }
    BOOL IsBlockFormatted()      { return (c_FORMAT_BLOCK_FLAG == ((~bBlockFlags ) & c_FORMAT_BLOCK_FLAG )); }
    BOOL IsBlockInUse()          { return (c_INUSE_BLOCK_FLAG  == ((~bBlockFlags ) & c_INUSE_BLOCK_FLAG  )); }
    BOOL IsBlockDirty()          { return (c_DIRTY_BLOCK_FLAG  == ((~bBlockFlags ) & c_DIRTY_BLOCK_FLAG  )); }   
    BOOL IsBlockTrash()          { return (c_TRASH_BLOCK_FLAG  == ((~bBlockFlags ) & c_TRASH_BLOCK_FLAG  )); }   
    BOOL IsBadBlockReplacement() { return (c_BADREP_BLOCK_FLAG == ((~bBlockFlags ) & c_BADREP_BLOCK_FLAG )) && IsValidOwnerBlock(); }
    BOOL IsBlockFree()           { return (c_BLOCK_FLAG_MASK   == (( bBlockFlags ) & c_BLOCK_FLAG_MASK   )); }

    BOOL IsValidBlockMapOffset()      
    { 
        return ((c_VALID_BLOCK_OFFSET == ((~bBlockFlags) & c_VALID_BLOCK_OFFSET)) && 
               ((wBlockOffset[0] != c_DEAD_LINK_INDEX) || (wBlockOffset[1] != c_DEAD_LINK_INDEX)));
    }

    void InvalidateBlockMapOffset() { wBlockOffset[0] = c_DEAD_LINK_INDEX; wBlockOffset[1] = c_DEAD_LINK_INDEX; }

    void SetBlockMapOffset(UINT32 map)
    {
        if(0 == (0xFFFF & (~wBlockOffset[0]) & (~map)))
        {
            wBlockOffset[0] = ~map;
            SetValidBlockOffset();
        }
        else
        {
            wBlockOffset[0] = c_DEAD_LINK_INDEX;
            
            // Make sure that no bits move from 0->1 
            ASSERT(0 == (0xFFFF & (~wBlockOffset[1]) & (~map)));

            wBlockOffset[1] = ~map;
        }
    }

    UINT16 GetBlockMapOffset() 
    {
        if(wBlockOffset[0] != c_DEAD_LINK_INDEX) return ~(wBlockOffset[0]);
        else                                     return ~(wBlockOffset[1]);
    }
};

struct WL_BadBlockMap
{
    UINT16 VirtualBlockIndex;
    UINT16 PhysicalBlockIndex;
    
    struct WL_BadBlockMap *Next;
};

struct BS_WearLeveling_Config
{
    BLOCK_CONFIG        *BlockConfig;
    IBlockStorageDevice *Device;

    UINT32               BlockIndexMask;
    UINT32               BytesPerBlock;
    UINT32               BaseAddress;

    WL_BadBlockMap      *BadBlockList;
};

struct BS_WearLeveling_Driver
{
    //--//

    static BOOL InitializeDevice(void *context);

    static BOOL UninitializeDevice(void *context);

    static const BlockDeviceInfo *GetDeviceInfo(void *context);

    static BOOL Read(void *context, ByteAddress Address, UINT32 NumBytes, BYTE *pSectorBuff);

    static BOOL Write(void *context, ByteAddress Address, UINT32 NumBytes, BYTE *pSectorBuff, BOOL ReadModifyWrite);

    static BOOL Memset(void *context, ByteAddress Address, UINT8 Data, UINT32 NumBytes);

    static BOOL GetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata);

    static BOOL SetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata);

    static BOOL IsBlockErased(void *context, ByteAddress BlockStartAddress, UINT32 BlockLength);

    static BOOL EraseBlock(void *context, ByteAddress Sector);

    static void SetPowerState(void *context, UINT32 State);

    static UINT32 MaxSectorWrite_uSec(void *context);

    static UINT32 MaxBlockErase_uSec(void *context);

    static BOOL ChipReadOnly(void *context, BOOL On, UINT32 ProtectionKey);

    static BOOL GetPhysicalAddress(BS_WearLeveling_Config* config, ByteAddress virtAddress, ByteAddress &phyAddress, BOOL &fDirectSectorMap);
    static BOOL FormatBlock(BS_WearLeveling_Config* config, ByteAddress phyBlockAddress);
    static BOOL CompactBlocks(BS_WearLeveling_Config* config, ByteAddress virtSectAddress);
   
private:
    static BOOL ReplaceBadBlock(BS_WearLeveling_Config* config, ByteAddress BadBlockAddress, ByteAddress &NewPhyBlockAddress);
    static BOOL ReplaceBlock(BS_WearLeveling_Config* config, ByteAddress virtAddress, ByteAddress currentBlockAddr, ByteAddress phyNewBlockAddress);
    static BOOL GetNextFreeSector (BS_WearLeveling_Config* config, ByteAddress phyAddress, ByteAddress &phyFreeAddress, WL_SectorMetadata &metaFree);
    static BOOL GetNextFreeBlock(BS_WearLeveling_Config* config, ByteAddress virtAddress, ByteAddress &phyNewBlockAddress);
    static BOOL GetPhysicalBlockAddress(BS_WearLeveling_Config* config, ByteAddress virtAddress, ByteAddress &phyBlockAddress, BOOL fAllocateNew);
    static BOOL WriteInternal(void *context, ByteAddress Address, UINT32 NumBytes, BYTE *pSectorBuff, BOOL ReadModifyWrite, BOOL fFillMem); 
    static BOOL WriteToSector(BS_WearLeveling_Config* config, ByteAddress virtSectStart, UINT8* pSectorData, UINT32 offset, UINT32 length, BOOL fMemFill);
    static BOOL WriteToPhysicalSector(BS_WearLeveling_Config* config, ByteAddress sectStart, UINT8* pSectorData, UINT32 length );
    static WL_BadBlockMap* GetBadBlockMap(BS_WearLeveling_Config* config, ByteAddress virtSectStart);
    static BOOL GetFreeBlock(BS_WearLeveling_Config* config, ByteAddress& freeBlock, WL_SectorMetadata* pmeta);
    static BOOL GetSectorMetadataInternal(BS_WearLeveling_Config* config, ByteAddress SectorStart, SectorMetadata* pSectorMetadata);
    static BOOL SetSectorMetadataInternal(BS_WearLeveling_Config* config, ByteAddress SectorStart, SectorMetadata* pSectorMetadata);
    static BOOL HandleBlockReplacement(BS_WearLeveling_Config* config, ByteAddress virtBlockAddress, ByteAddress phyBlockAddress, ByteAddress &phyNewBlockAddress);
    static void ValidateOwnerBlock(BS_WearLeveling_Config *config, SectorAddress virtAddr);
    static void FreeBadBlockReplacement(BS_WearLeveling_Config* config, ByteAddress badBlockAddress);

    static UINT16 GetBlockIndex(ByteAddress address, ByteAddress addressBase, UINT32 bytesPerBlock)
    {
        return (UINT16)((address - addressBase) / bytesPerBlock);
    }

    static ByteAddress GetBlockStartAddress(ByteAddress address, ByteAddress addressBase, UINT32 bytesPerBlock)
    {
        address = address - addressBase;
        return addressBase + address - (address % bytesPerBlock);
    }
    
    static BOOL  s_inCompaction;
    static ByteAddress s_lastFreeBlock;
};

//--//

#endif // _DRIVERS_BS_WEARLEVELING_H_
