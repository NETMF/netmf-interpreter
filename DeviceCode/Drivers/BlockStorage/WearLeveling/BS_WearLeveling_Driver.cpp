////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "BS_WearLeveling.h"

//#define _VISUAL_WEARLEVELING_ 1

//#define _WEAR_LEVEL_ASSERT(x) ASSERT(x)
#define _WEAR_LEVEL_ASSERT(x)

//--//

#ifdef _VISUAL_WEARLEVELING_
static UINT16 *g_pLcdBuffer    = NULL;
static BOOL    g_WearLevelInit = FALSE;
#define BLOCK_SIZE 6

#define COLOR_RED    0x001f
#define COLOR_GREEN  0x03c0
#define COLOR_BLUE   0xf800
#define COLOR_PURPLE 0xC018
#define COLOR_YELLOW 0xC380
#endif

BOOL        BS_WearLeveling_Driver::s_inCompaction   = FALSE;
ByteAddress BS_WearLeveling_Driver::s_lastFreeBlock  = 0;

#ifdef _VISUAL_WEARLEVELING_

void SetBlockColor(ByteAddress BlockAddress, UINT16 color, BS_WearLeveling_Config* config);
    
static void InitializeVisualWearLeveling(BS_WearLeveling_Config* config)
{
    g_pLcdBuffer = (UINT16*)LCD_GetFrameBuffer();

    if(g_pLcdBuffer)
    {
        WL_SectorMetadata      virtMeta;
        const BlockDeviceInfo *pDevInfo  = config->Device->GetDeviceInfo(config->BlockConfig);
        const int              numBlocks = pDevInfo->Regions[0].NumBlocks;
        ByteAddress            addr      = config->BaseAddress;

        LCD_Clear(); 
        g_WearLevelInit = TRUE; 

        for(int i=0; i<numBlocks; i++)
        {
            UINT16 color = COLOR_BLUE;
            
            if(!config->Device->GetSectorMetadata(config->BlockConfig, addr, (SectorMetadata*)&virtMeta) || virtMeta.IsBadBlock())
            {
                color = COLOR_PURPLE;
            }
            else if(!virtMeta.IsBlockFormatted()) color = COLOR_YELLOW;
            else if(virtMeta.IsBlockFree())       color = COLOR_GREEN;
            else if(virtMeta.IsBlockTrash())      color = COLOR_RED;
            
            SetBlockColor( addr - config->BaseAddress, color, config );

            addr += config->BytesPerBlock;
        }
    }

}

static void SetBlockColor(ByteAddress BlockAddress, UINT16 color, BS_WearLeveling_Config* config)
{
    if(!g_pLcdBuffer || !g_WearLevelInit)
    {
        InitializeVisualWearLeveling(config);
    }
    
    if(g_pLcdBuffer)
    {
        int qqIdx = (BlockAddress / config->BytesPerBlock) * BLOCK_SIZE;
        int qy    = (qqIdx / LCD_GetWidth()) * BLOCK_SIZE;
        qqIdx     = qqIdx % LCD_GetWidth();
           
        for(int qq=0; qq<BLOCK_SIZE; qq++)
        {
            for(int qx=0; qx<BLOCK_SIZE; qx++)
            {
                g_pLcdBuffer[qqIdx + (qy + qq) * LCD_GetWidth() + qx] = color;
            }
        }
    }
}


#endif

//
// Initialize wear leveling device 
//
BOOL BS_WearLeveling_Driver::InitializeDevice(void *context)
{
    BS_WearLeveling_Config *config = (BS_WearLeveling_Config*)context;

    if(config == NULL || config->Device == NULL) return FALSE;

    BOOL fResult = config->Device->InitializeDevice(config->BlockConfig);

    const BlockDeviceInfo *pDevInfo   = config->Device->GetDeviceInfo(config->BlockConfig);
    UINT32                 BlockCount = pDevInfo->Size / config->BytesPerBlock;
    
    config->BlockIndexMask = 0x80000000;

    // verify that we have simple heap support
    void* tmp = SimpleHeap_Allocate(4); 

    if(NULL == tmp) { _WEAR_LEVEL_ASSERT(FALSE); return FALSE; }
    else            { SimpleHeap_Release(tmp);                 }

    // 
    // Block index mas is used to determine block map addressing
    //
    while(config->BlockIndexMask > BlockCount) 
    {
        config->BlockIndexMask >>= 1;
    }

    //
    // Assure that we start out compacted
    //
    //if(!CompactBlocks(config, 0xFFFFFFFF)) return FALSE;


    WL_SectorMetadata  meta;
    const UINT32       NumBlocks = pDevInfo->Size / config->BytesPerBlock;
    UINT32             Blocks;
    ByteAddress        BlockAddress;

    //
    // Look for bad blocks so that we can maintain a list
    // In the first round, look for previously marked bad block replacements.
    // In the second round, look for any unmapped bad blocks
    //
    for(int round=1; round<=2; round++)
    {       
        Blocks       = NumBlocks;
        BlockAddress = config->BaseAddress;
        
        while(Blocks--)
        {
            BOOL fMeta = GetSectorMetadataInternal(config, BlockAddress, (SectorMetadata*)&meta);

            if(round == 1 && (!fMeta || meta.IsBadBlock() || !meta.IsBlockFormatted()))
            {
                FormatBlock(config, BlockAddress);
                
                fMeta = GetSectorMetadataInternal(config, BlockAddress, (SectorMetadata*)&meta);
            }

            // 
            // First pass looks for valid bad block replacements
            // Second pass looks for bad blocks 
            //
            if((round == 1 && !meta.IsBlockTrash() && meta.IsBadBlockReplacement()) ||
                (round == 2 && (!fMeta || meta.IsBadBlock())))
            {
                UINT16 badIndex = GetBlockIndex( BlockAddress, config->BaseAddress, config->BytesPerBlock );

                WL_BadBlockMap* pBadMap = config->BadBlockList;
                
                while(pBadMap != NULL)
                {
                    if(meta.IsBadBlock()            && pBadMap->VirtualBlockIndex  == badIndex) break;
                    if(meta.IsBadBlockReplacement() && pBadMap->PhysicalBlockIndex == badIndex) break;

                    pBadMap = pBadMap->Next;
                }

                if(pBadMap == NULL)
                {
                    //
                    // We found a bad block (in the second round) which is not in the bad block list
                    //
                    if(round == 2)
                    {
                        
                        pBadMap = (WL_BadBlockMap*)SimpleHeap_Allocate(sizeof(WL_BadBlockMap));

                        if(!pBadMap) return FALSE;

                        pBadMap->VirtualBlockIndex  = badIndex;
                        pBadMap->PhysicalBlockIndex = WL_SectorMetadata::c_FREE_LINK_INDEX;

                        pBadMap->Next = config->BadBlockList;
                        config->BadBlockList = pBadMap;

                        debug_printf("Bad Block: 0x%08X\n", 
                            config->BaseAddress + (pBadMap->VirtualBlockIndex  * config->BytesPerBlock));
                    }
                    //
                    // We found a bad block replacement that has not been added to the list
                    //
                    else
                    {
                        pBadMap = (WL_BadBlockMap*)SimpleHeap_Allocate(sizeof(WL_BadBlockMap));

                        if(!pBadMap) return FALSE;

                        pBadMap->VirtualBlockIndex  = meta.wOwnerBlock;
                        pBadMap->PhysicalBlockIndex = badIndex;

                        pBadMap->Next = config->BadBlockList;
                        config->BadBlockList = pBadMap;

                        debug_printf("Bad Block Map: 0x%08X => 0x%04X\n", 
                            config->BaseAddress + (pBadMap->VirtualBlockIndex  * config->BytesPerBlock), 
                            config->BaseAddress + (pBadMap->PhysicalBlockIndex * config->BytesPerBlock));
                    }
                }
                else
                {
                    _WEAR_LEVEL_ASSERT(round == 2 || pBadMap->VirtualBlockIndex == meta.wOwnerBlock && pBadMap->PhysicalBlockIndex == badIndex);
                    _WEAR_LEVEL_ASSERT(round == 1 || pBadMap->VirtualBlockIndex == badIndex);
                }
            }
            
            BlockAddress += config->BytesPerBlock;
        }
    }

    return fResult;
}

WL_BadBlockMap* BS_WearLeveling_Driver::GetBadBlockMap(BS_WearLeveling_Config* config, ByteAddress BadBlockAddress)
{
    UINT16 BadBlockIndex = GetBlockIndex( BadBlockAddress, config->BaseAddress, config->BytesPerBlock );

    // 
    // Either find the block or an empty space in the bad block list
    //
    WL_BadBlockMap* pBadMap = config->BadBlockList;
    
    while(pBadMap != NULL)
    {
        if(pBadMap->VirtualBlockIndex  == BadBlockIndex) break;

        pBadMap = pBadMap->Next;
    }

    return pBadMap;
}

BOOL BS_WearLeveling_Driver::GetFreeBlock(BS_WearLeveling_Config* config, ByteAddress& freeBlock, WL_SectorMetadata* meta)
{
    const BlockDeviceInfo *pDevInfo  = config->Device->GetDeviceInfo(config->BlockConfig);
    const ByteAddress BaseAddressEnd = config->BaseAddress + pDevInfo->Size;
    ByteAddress start, end;
    INT32 i;

    if(s_lastFreeBlock < config->BaseAddress || s_lastFreeBlock >= BaseAddressEnd)
    {
        s_lastFreeBlock = config->BaseAddress + pDevInfo->Size - config->BytesPerBlock;
    }

    start = s_lastFreeBlock;
    end   = config->BaseAddress;

    //
    // Find a free block
    //
    for(i=0; i<2; i++)
    {    
        while(start >= end && start < BaseAddressEnd)
        {
            if(GetSectorMetadataInternal( config, start, (SectorMetadata*)meta ) 
                && !meta->IsBadBlock()
                && (meta->IsBlockFree() || (meta->IsBlockTrash() && !meta->IsValidBlockMapOffset() && !meta->IsValidOwnerBlock())))
            {
                if(meta->IsBlockTrash())
                {
                    FormatBlock(config, start);
                    GetSectorMetadataInternal( config, start, (SectorMetadata*)meta );
                }

                meta->SetBlockInUse();

                if(SetSectorMetadataInternal( config, start, (SectorMetadata*)meta ))
                {
                    freeBlock       = start;
                    s_lastFreeBlock = start - config->BytesPerBlock;
                    return TRUE;
                }
                else
                {
                    FormatBlock(config, start);
                }
            }

            start -= config->BytesPerBlock;
        }

        start = config->BaseAddress + pDevInfo->Size - config->BytesPerBlock;
        end   = s_lastFreeBlock;
    }

    s_lastFreeBlock = config->BaseAddress + pDevInfo->Size - config->BytesPerBlock;
    
    freeBlock = 0;

    _WEAR_LEVEL_ASSERT(FALSE);
    
    return FALSE;
}

//
// Replace a bad block with a new free block
//
BOOL BS_WearLeveling_Driver::ReplaceBadBlock(BS_WearLeveling_Config* config, ByteAddress BadBlockAddress, ByteAddress &NewPhyBlockAddress)
{
    ByteAddress       freeBlock = 0;
    WL_SectorMetadata meta;
    WL_SectorMetadata badMeta;

    // 
    // Either find the block or an empty space in the bad block list
    //
    WL_BadBlockMap* pBadMap = GetBadBlockMap(config, BadBlockAddress);

    if(!GetFreeBlock(config, freeBlock, &meta))
    {
        if(CompactBlocks(config, 0xFFFFFFFE))
        {
            if(!GetFreeBlock(config, freeBlock, &meta)) return FALSE;
        }
    }

    //
    // add the bad block map to the list
    //
    if(pBadMap == NULL)
    {
        pBadMap = (WL_BadBlockMap*)SimpleHeap_Allocate( sizeof(WL_BadBlockMap) );

        if(pBadMap == NULL) return FALSE;

        pBadMap->Next = config->BadBlockList;
        config->BadBlockList = pBadMap;

        pBadMap->VirtualBlockIndex  = GetBlockIndex( BadBlockAddress, config->BaseAddress, config->BytesPerBlock );
        pBadMap->PhysicalBlockIndex = GetBlockIndex( freeBlock      , config->BaseAddress, config->BytesPerBlock );
    }
    else
    {
        if(pBadMap->PhysicalBlockIndex != WL_SectorMetadata::c_FREE_LINK_INDEX)
        {
            WL_SectorMetadata metaOld;
            ByteAddress oldBlockAddr = config->BaseAddress + pBadMap->PhysicalBlockIndex * config->BytesPerBlock;

            if(GetSectorMetadataInternal(config, oldBlockAddr, (SectorMetadata*)&metaOld) && !metaOld.IsBadBlock())
            {
                _WEAR_LEVEL_ASSERT(metaOld.wOwnerBlock == pBadMap->VirtualBlockIndex);
                
                if(!ReplaceBlock(config, BadBlockAddress, oldBlockAddr, freeBlock))
                {
                    _WEAR_LEVEL_ASSERT(FALSE);
                    return FALSE;
                }
            }

            FreeBadBlockReplacement(config, BadBlockAddress);
        }
        
        pBadMap->PhysicalBlockIndex = GetBlockIndex( freeBlock, config->BaseAddress, config->BytesPerBlock );
    }

    debug_printf("Bad Block Map: 0x%08X => 0x%08X\n", 
        config->BaseAddress + (pBadMap->VirtualBlockIndex  * config->BytesPerBlock), 
        config->BaseAddress + (pBadMap->PhysicalBlockIndex * config->BytesPerBlock));
    

    memset(&badMeta, 0, sizeof(badMeta));
    SetSectorMetadataInternal(config, BadBlockAddress, (SectorMetadata*)&badMeta);

    GetSectorMetadataInternal(config, freeBlock, (SectorMetadata*)&meta);
    //
    // Set the meta data on the replacement block so that it can be identified as a replacement block
    //
    meta.SetBlockInUse();
    meta.SetBadBlockReplacement();
    meta.wOwnerBlock = pBadMap->VirtualBlockIndex;

#ifdef _VISUAL_WEARLEVELING_
SetBlockColor( BadBlockAddress, COLOR_PURPLE, config );
SetBlockColor( freeBlock      , COLOR_BLUE  , config );
#endif

    NewPhyBlockAddress = freeBlock;

    return SetSectorMetadataInternal( config, freeBlock, (SectorMetadata*)&meta );
}

BOOL BS_WearLeveling_Driver::UninitializeDevice(void *context)
{    
    BS_WearLeveling_Config *config = (BS_WearLeveling_Config*)context;
    WL_BadBlockMap* pMap;

    if(config == NULL || config->Device == NULL) return FALSE;

    pMap = config->BadBlockList;

    while(pMap != NULL)
    {
        WL_BadBlockMap* pTmp = pMap->Next;
        
        SimpleHeap_Release(pMap);

        pMap = pTmp;
    }

    config->BadBlockList = NULL;

#ifdef _VISUAL_WEARLEVELING_
    g_WearLevelInit = FALSE;
#endif

    return config->Device->UninitializeDevice(config->BlockConfig);
}

const BlockDeviceInfo *BS_WearLeveling_Driver::GetDeviceInfo(void *context)
{
    BS_WearLeveling_Config *config = (BS_WearLeveling_Config*)context;

    if(config == NULL || config->Device == NULL) return FALSE;

    return config->Device->GetDeviceInfo(config->BlockConfig);
}

void BS_WearLeveling_Driver::FreeBadBlockReplacement(BS_WearLeveling_Config* config, ByteAddress badBlockAddress)
{
    WL_BadBlockMap* pMap = GetBadBlockMap( config, badBlockAddress );
    
    if(pMap && pMap->PhysicalBlockIndex != WL_SectorMetadata::c_FREE_LINK_INDEX)
    {
        WL_SectorMetadata phyMeta;
        SectorAddress     phyAddr = config->BaseAddress + pMap->PhysicalBlockIndex * config->BytesPerBlock;
            
        if(GetSectorMetadataInternal(config, phyAddr, (SectorMetadata*)&phyMeta))
        {
            _WEAR_LEVEL_ASSERT(phyMeta.wOwnerBlock == pMap->VirtualBlockIndex);
            _WEAR_LEVEL_ASSERT(GetBlockIndex(phyAddr, config->BaseAddress, config->BytesPerBlock) == pMap->PhysicalBlockIndex);
                
            phyMeta.SetBlockTrash();
            phyMeta.SetOwnerBlockInvalid();
        
            SetSectorMetadataInternal(config, phyAddr, (SectorMetadata*)&phyMeta);
        }

        pMap->PhysicalBlockIndex = WL_SectorMetadata::c_FREE_LINK_INDEX;        
    }
}

BOOL BS_WearLeveling_Driver::CompactBlocks(BS_WearLeveling_Config* config, ByteAddress virtSectAddress)
{
    if(s_inCompaction) return FALSE;
    
    //GLOBAL_LOCK(x);
    
    const BlockDeviceInfo *pDevInfo          = config->Device->GetDeviceInfo(config->BlockConfig);
    WL_SectorMetadata      meta;
    const UINT32           BlockCount        = pDevInfo->Size / config->BytesPerBlock;
    UINT32                 NumBlocks         = BlockCount;
    ByteAddress            BlockAddress;
    const UINT32           SectorsPerBlock   = config->BytesPerBlock / pDevInfo->BytesPerSector;
    ByteAddress            virtBlockAddr     = GetBlockStartAddress( virtSectAddress, config->BaseAddress, config->BytesPerBlock );
    ByteAddress            CopyBlockAddress1 = 0xFFFFFFFF;
    ByteAddress            CopyBlockAddress2 = 0xFFFFFFFF;
    UINT32                 FreeBlockCount    = 0;
    UINT32                 TrashBlockCount   = 0;

    //
    // Get a 2 free blocks to use during compaction as temporary blocks
    //
    if(!GetFreeBlock(config, CopyBlockAddress1, &meta)) {                                           _WEAR_LEVEL_ASSERT(FALSE); return FALSE; }
    if(!GetFreeBlock(config, CopyBlockAddress2, &meta)) { FormatBlock( config, CopyBlockAddress1 ); _WEAR_LEVEL_ASSERT(FALSE); return FALSE; }

    if(CopyBlockAddress1 == virtBlockAddr)
    {
        FormatBlock( config, CopyBlockAddress1 );

        if(!GetFreeBlock(config, CopyBlockAddress1, &meta)) 
        { 
            FormatBlock( config, CopyBlockAddress2 ); 
            _WEAR_LEVEL_ASSERT(FALSE);
            return FALSE; 
        }
    }
    if(CopyBlockAddress2 == virtBlockAddr)
    {
        FormatBlock( config, CopyBlockAddress2 );

        if(!GetFreeBlock(config, CopyBlockAddress2, &meta)) 
        { 
            FormatBlock( config, CopyBlockAddress1 ); 
            _WEAR_LEVEL_ASSERT(FALSE);
            return FALSE; 
        }
    }
    
    s_inCompaction = TRUE;

    NumBlocks       = BlockCount;
    BlockAddress    = config->BaseAddress;

    while(NumBlocks--)
    {
        UINT32      trashSectorCount = 0;
        ByteAddress sectAddr         = BlockAddress;
#ifdef _DEBUG
        WL_SectorMetadata blockMeta;
#endif

        //
        // Skip temporary copy blocks
        //
        if(BlockAddress == CopyBlockAddress1 || BlockAddress == CopyBlockAddress2)
        {
#ifdef _VISUAL_WEARLEVELING_
            SetBlockColor( BlockAddress, COLOR_GREEN, config );
#endif
            BlockAddress += config->BytesPerBlock;

            FreeBlockCount++;
            continue;
        }

#ifdef _VISUAL_WEARLEVELING_
        SetBlockColor( BlockAddress, COLOR_BLUE, config );
#endif
        
        for(UINT32 SectorIndex=0; SectorIndex < SectorsPerBlock; SectorIndex++)
        {
            BOOL badSector = FALSE;

            if(!GetSectorMetadataInternal(config, sectAddr, (SectorMetadata*)&meta))
            {
                badSector = TRUE;
            }
                
            //
            // The first sector contains block information so we need to special case it
            //
            if(SectorIndex == 0)
            {
#ifdef _DEBUG
                memcpy(&blockMeta, &meta, sizeof(meta));
#endif                
                //
                // Swap out the replacement block for bad blocks, so that the old replacment can be re-mapped
                //
                if(meta.IsBadBlock() || badSector)
                {
                    if(virtBlockAddr == BlockAddress)
                    {
                        ByteAddress badBlockReplace;
                       
                        if(!ReplaceBadBlock(config, BlockAddress, badBlockReplace)) 
                        {
                            _WEAR_LEVEL_ASSERT(FALSE);
                        }
                    }
                    
#ifdef _VISUAL_WEARLEVELING_
                    SetBlockColor( BlockAddress, COLOR_PURPLE, config );
#endif              
                    break;
                }
                else if(meta.IsBlockFree())
                {
#ifdef _VISUAL_WEARLEVELING_
                    SetBlockColor( BlockAddress, COLOR_GREEN, config );
#endif
                    FreeBlockCount++;
                    break;
                }
                //
                // The current block is mapped to another address
                //
                else if(meta.IsValidBlockMapOffset())
                {
                    UINT32 NextBlock = BlockAddress;
                    UINT32 MappedBlock = config->BaseAddress + meta.GetBlockMapOffset() * config->BytesPerBlock;

                    while(TRUE)
                    {
                        WL_SectorMetadata metaCopy;

                        //
                        // Re-map bad block so that we free up the current block
                        //                        
                        if(meta.IsBadBlockReplacement())
                        {
                            ByteAddress badBlockReplace;
                            ByteAddress OwnerBlock = config->BaseAddress + meta.wOwnerBlock * config->BytesPerBlock;

                            if(meta.IsBlockTrash())
                            {
                                FreeBadBlockReplacement(config, OwnerBlock);
                            }
                            else
                            {
#ifdef _DEBUG
                                WL_BadBlockMap* pMap = GetBadBlockMap(config, OwnerBlock);
                                _WEAR_LEVEL_ASSERT(pMap && GetBlockIndex(BlockAddress, config->BaseAddress, config->BytesPerBlock) == pMap->PhysicalBlockIndex);
#endif                                
                                if(!ReplaceBadBlock(config, OwnerBlock, badBlockReplace)) 
                                {
                                    _WEAR_LEVEL_ASSERT(FALSE);
                                }
                            }
                        }
                        //
                        // if the current block's data is owned by another block (in other words, if another block is mapped to this one)
                        // then we need to save the data to one of the temporary blocks while we format and compact this block
                        //
                        else if(meta.IsValidOwnerBlock())
                        {
                            GetSectorMetadataInternal(config, NextBlock, (SectorMetadata*)&metaCopy);

                            // Make sure we invalidate owner block so we can replace the block
                            metaCopy.SetOwnerBlockInvalid();
                            
                            SetSectorMetadataInternal(config, NextBlock, (SectorMetadata*)&metaCopy);

                            FormatBlock( config, CopyBlockAddress1 );

                            if(!ReplaceBlock( config, CopyBlockAddress1, NextBlock, CopyBlockAddress1 ))
                            {
                                _WEAR_LEVEL_ASSERT(FALSE);
                            }
                        }

                        //
                        // Otherwise, format and copy the blocks mapped data from the mapped block (so that we end up with a direct map)
                        //
                        {
                            GetSectorMetadataInternal(config, NextBlock, (SectorMetadata*)&metaCopy);

                            // Make sure we invalidate block offset map so we can format the block
                            metaCopy.InvalidateBlockMapOffset();
                            
                            SetSectorMetadataInternal(config, NextBlock, (SectorMetadata*)&metaCopy);
                            
                            FormatBlock( config, NextBlock );

                            if(!ReplaceBlock( config, NextBlock, MappedBlock, NextBlock ))
                            {
                                _WEAR_LEVEL_ASSERT(FALSE);
                                break;
                            }
                        }

                        //
                        // Set up the next block in the the chain of mapped blocks
                        //
                        if(meta.IsValidOwnerBlock() && !meta.IsBadBlockReplacement())
                        {
#ifdef _DEBUG   
                            UINT16 wOwnerIndex = meta.wOwnerBlock;
#endif

                            MappedBlock = CopyBlockAddress1;
                            NextBlock   = config->BaseAddress + meta.wOwnerBlock * config->BytesPerBlock;

                            CopyBlockAddress1 = CopyBlockAddress2;
                            CopyBlockAddress2 = MappedBlock;
                            
                            if(!GetSectorMetadataInternal(config, NextBlock, (SectorMetadata*)&meta) || meta.IsBadBlock())
                            {
                                _WEAR_LEVEL_ASSERT(FALSE);
                                break;
                            }
#ifdef _DEBUG
                            _WEAR_LEVEL_ASSERT(meta.GetBlockMapOffset() == wOwnerIndex);
#endif
                        }
                        //
                        // Otherwise, we are done (no more mapped blocks in the chain)
                        //
                        else
                        {
                            break;
                        }
                    }

                    break;
                }
                //
                // No mapping and the block is either not formatted or it is trash, so format the block
                //
                else if(!meta.IsBlockFormatted())
                {
                    FormatBlock( config, BlockAddress );
                    break;
                }
                else if(meta.IsBadBlockReplacement())
                {
                    if(meta.IsBlockTrash())
                    {
                        SectorAddress badBlockAddr = config->BaseAddress + meta.wOwnerBlock * config->BytesPerBlock;

                        FreeBadBlockReplacement(config, badBlockAddr);

                        TrashBlockCount++;
                    }
                    // only change out the bad block replacement if we need to
                    else if(virtBlockAddr == BlockAddress)
                    {
                        ByteAddress badBlockReplace;
                        ByteAddress OwnerBlock = config->BaseAddress + meta.wOwnerBlock * config->BytesPerBlock;
#ifdef _DEBUG
                        WL_BadBlockMap* pMap = GetBadBlockMap(config, OwnerBlock);
                        _WEAR_LEVEL_ASSERT(pMap && GetBlockIndex(BlockAddress, config->BaseAddress, config->BytesPerBlock) == pMap->PhysicalBlockIndex);
#endif
                        
                        if(!ReplaceBadBlock(config, OwnerBlock, badBlockReplace))
                        {
                            _WEAR_LEVEL_ASSERT(FALSE);
                            s_inCompaction = FALSE;
                            return FALSE;
                        }
                    }

                    break;
                }
                //
                //  This block will be taken care of when the owner is processed
                //
                else if(meta.IsValidOwnerBlock())
                {
                    if(meta.IsBlockTrash())
                    {
                        TrashBlockCount++;
                    }
#ifdef _DEBUG
                    ValidateOwnerBlock(config, BlockAddress);
#endif
                    break;
                }
                // otherwise we are trash
                else if(meta.IsBlockTrash())
                {
#ifdef _VISUAL_WEARLEVELING_
                    SetBlockColor( BlockAddress, COLOR_RED, config );
#endif
                    TrashBlockCount++;

                    // at this point we don't have a valid owner or block offset
                    if(virtBlockAddr == BlockAddress)
                    {
                        _WEAR_LEVEL_ASSERT(!meta.IsValidBlockMapOffset() && !meta.IsValidOwnerBlock());
                        FormatBlock( config, virtBlockAddr );
                    }
                    break;
                }
                //
                // compact the sectors
                //
                else if(meta.IsSectorMapped() || virtBlockAddr == BlockAddress)
                {
                    // At this point we don't have a valid owner or block offset
                    _WEAR_LEVEL_ASSERT(!meta.IsValidBlockMapOffset() && !meta.IsValidOwnerBlock());

                    FormatBlock( config, CopyBlockAddress1 );
                    if(!ReplaceBlock( config, CopyBlockAddress1, BlockAddress, CopyBlockAddress1 ))
                    {
                        _WEAR_LEVEL_ASSERT(FALSE);
                    }
                    FormatBlock( config, BlockAddress );
                    if(!ReplaceBlock( config, BlockAddress, CopyBlockAddress1, BlockAddress ))

                    {
                        _WEAR_LEVEL_ASSERT(FALSE);
                    }
                    break;
                }
                //
                // increment the trash sector count (if all sectors are trash, then format)
                //
                else if(meta.IsSectorTrash() || meta.IsSectorBad())
                {
                    trashSectorCount++;
                }
                //
                // If we have at least one sector that is live, then we will not format this block so bail out
                //
                else
                {
                    break;
                }
            }
            //
            // Get the WL_SectorMetadata from the device at the given address, if there is a failure then indicate a bad
            // sector in our map
            //
            else if(badSector)
            {
                trashSectorCount++;
            }
            else if(meta.IsSectorMapped() || virtSectAddress == sectAddr)
            {
#ifdef _DEBUG
                _WEAR_LEVEL_ASSERT(!blockMeta.IsValidOwnerBlock() && !blockMeta.IsValidBlockMapOffset());
#endif                
                //
                // compact the sectors
                //
                FormatBlock( config, CopyBlockAddress1 );
                if(!ReplaceBlock( config, CopyBlockAddress1, BlockAddress, CopyBlockAddress1 ))
                {
                    _WEAR_LEVEL_ASSERT(FALSE);
                }
                FormatBlock( config, BlockAddress );
                if(!ReplaceBlock( config, BlockAddress, CopyBlockAddress1, BlockAddress ))
                {
                    _WEAR_LEVEL_ASSERT(FALSE);
                }
                break;
            }
            else if(meta.IsSectorBad() || meta.IsSectorTrash())
            {
                trashSectorCount++;
            }
            else // if we have a live, good sector then we can break out.
            {
                break;
            }

            sectAddr += pDevInfo->BytesPerSector;
        }

        //
        // If all the sectors of a configuration are trash, then mark the block for erase
        //
        if(SectorsPerBlock == trashSectorCount)
        {
            if(!GetSectorMetadataInternal(config, BlockAddress, (SectorMetadata*)&meta) || meta.IsBadBlock())
            {
                _WEAR_LEVEL_ASSERT(FALSE);
            }
            
            if(!meta.IsBlockTrash())
            {
                meta.SetBlockTrash();

                SetSectorMetadataInternal(config, BlockAddress, (SectorMetadata*)&meta);

#ifdef _VISUAL_WEARLEVELING_
                SetBlockColor( BlockAddress, COLOR_RED, config );
#endif
                TrashBlockCount++;
            }
        }

        BlockAddress += config->BytesPerBlock;
    }

    if(virtBlockAddr >= config->BaseAddress && virtBlockAddr < (config->BaseAddress + pDevInfo->Size))
    {
        if(GetSectorMetadataInternal(config, virtBlockAddr, (SectorMetadata*)&meta) && !meta.IsBadBlock())
        {
            if(meta.IsBlockTrash())
            {
                FormatBlock(config, virtBlockAddr);
            }
        }
#ifdef _DEBUG
        if(GetSectorMetadataInternal(config, virtSectAddress, (SectorMetadata*)&meta) && !meta.IsBadBlock())
        {
            _WEAR_LEVEL_ASSERT(meta.IsSectorFree());
        }
#endif
    }
    
    //
    // Any blocks still mapped on the second iteration means that they are orphaned, so format them
    //
    // erase orphaned blocks when we have less than 20% free
    if((FreeBlockCount << 2) < TrashBlockCount || (virtSectAddress == 0xFFFFFFFF) || FreeBlockCount < 4)
    {

        NumBlocks    = BlockCount;
        BlockAddress = pDevInfo->Regions[0].Start;

        while(NumBlocks--)
        {
            if(GetSectorMetadataInternal(config, BlockAddress, (SectorMetadata*)&meta) && 
              !meta.IsBadBlock() && meta.IsBlockTrash() && !meta.IsValidBlockMapOffset() &&
              !meta.IsValidOwnerBlock())
            {
                FormatBlock( config, BlockAddress );
            }
            BlockAddress += config->BytesPerBlock;
        }
    }

    FormatBlock( config, CopyBlockAddress1 );
    FormatBlock( config, CopyBlockAddress2 );

    s_inCompaction = FALSE;

    return TRUE;    
}

BOOL BS_WearLeveling_Driver::FormatBlock(BS_WearLeveling_Config* config, ByteAddress phyBlockAddress)
{
    WL_SectorMetadata meta;

    phyBlockAddress = GetBlockStartAddress(phyBlockAddress, config->BaseAddress, config->BytesPerBlock);

    //
    // If the block is bad, change the replacement block 
    //
    if(!GetSectorMetadataInternal(config, phyBlockAddress, (SectorMetadata*)&meta) || meta.IsBadBlock())
    {
        FreeBadBlockReplacement(config, phyBlockAddress);

#ifdef _VISUAL_WEARLEVELING_
        SetBlockColor( phyBlockAddress, COLOR_PURPLE, config );
#endif
    }
    else
    {
        _WEAR_LEVEL_ASSERT(!meta.IsBlockFormatted() || (!meta.IsValidBlockMapOffset() && !meta.IsValidOwnerBlock()));
        //
        // Erase the block and mark it as formatted
        //
        config->Device->EraseBlock(config->BlockConfig, phyBlockAddress);

        GetSectorMetadataInternal(config, phyBlockAddress, (SectorMetadata*)&meta);

        meta.SetBlockFormated();

        SetSectorMetadataInternal(config, phyBlockAddress, (SectorMetadata*)&meta);

#ifdef _VISUAL_WEARLEVELING_
        SetBlockColor( phyBlockAddress, COLOR_GREEN, config );
#endif
    }
    
    return TRUE;
}

BOOL BS_WearLeveling_Driver::GetNextFreeSector(BS_WearLeveling_Config* config, ByteAddress phyAddress, ByteAddress &phyFreeAddress, WL_SectorMetadata &metaFree)
{
    const BlockDeviceInfo *pDevInfo        = config->Device->GetDeviceInfo(config->BlockConfig);
    const UINT32           BytesPerSector  = pDevInfo->BytesPerSector;
          UINT32           phyBlockAddress = GetBlockStartAddress( phyAddress, config->BaseAddress, config->BytesPerBlock );
          UINT32           nextFree        = phyBlockAddress + config->BytesPerBlock - BytesPerSector;

    phyFreeAddress = 0;

    //
    // Start from the end of the block to find free sectors (because users are likely to program sequentially)
    // This will hopefully cut down on the number of mapped sectors.
    // Ignore first block - to simplify mapping scheme we don't allow maps to the first sector it can only be direct mapped
    //
    while(nextFree >= phyBlockAddress)
    {
        if(GetSectorMetadataInternal( config, nextFree, (SectorMetadata*)&metaFree ))
        {
            if(metaFree.IsSectorFree())
            {
                phyFreeAddress = nextFree;
                return TRUE;
            }
        }

        nextFree -= BytesPerSector;
    }

    return FALSE;
}

//
// This method determines how many bits are set to 1 in a UINT16
//
__inline UINT16 BitsSet16(UINT16 x) 
{
    x = ((x & 0xaaaa) >> 1) + (x & 0x5555);
    x = ((x & 0xcccc) >> 2) + (x & 0x3333);
    x = ((x & 0xF0F0) >> 4) + (x & 0x0F0F);
    x = ((x & 0xFF00) >> 8) + (x & 0x00FF);

    return x;
}

//
// Block mapping has can normally only be done once per BlockOffset field, because you have to erase the enitre block to write a new offset in the metadata
// To get around this issue, we look for the next available block that flips the fewest bits from 1->0.  This way we can reuse the field more than once.  
// (e.g. 0xFFFF -> 0xFFEFF -> 0xF7EFF -> etc).  Note that the indexes are bitwise inverted in order to maintain more 1s than 0s.
//
BOOL BS_WearLeveling_Driver::GetNextFreeBlock(BS_WearLeveling_Config* config, ByteAddress virtAddress, ByteAddress &phyNewBlockAddress)
{
    ByteAddress            virtBlockAddress = GetBlockStartAddress( virtAddress, config->BaseAddress, config->BytesPerBlock );
    UINT16                 curBlockIndex;
    WL_SectorMetadata      meta, metaVirt;
     
    ByteAddress            phySectAddr;
    UINT16                 maxBits;
    UINT16                 maxValue       = (config->BlockIndexMask << 1) - 1;
    BOOL                   fRound2        = FALSE;
    const BlockDeviceInfo *pDevInfo       = config->Device->GetDeviceInfo(config->BlockConfig);
    const UINT32           BaseAddressEnd = config->BaseAddress + pDevInfo->Size;

    if(!GetSectorMetadataInternal( config, virtBlockAddress, (SectorMetadata*)&metaVirt ) || metaVirt.IsBadBlock())
    {
        _WEAR_LEVEL_ASSERT(FALSE);
        return FALSE;
    }

    curBlockIndex = metaVirt.GetBlockMapOffset();

    maxBits = BitsSet16(curBlockIndex) + 1;

    //
    // we don't have any bits to play with, so exit
    //
    if(maxBits > maxValue)
    {
        return FALSE;
    }

    //
    // two rounds since we have two block offset fields - we may want to change this to use more than the first sector
    // to limit the block erases on the virtual block
    //
    while(TRUE)
    {
        //
        // Start by looking for addresses with only 1 bit difference, then increase to 2, and so on ...
        //
        while(TRUE)
        {
            INT32 start = maxValue;
            INT32 i;
            
            for(i=start; i>0; i--)
            {
                if(BitsSet16((i | curBlockIndex)) == maxBits)
                {
                    phySectAddr = config->BaseAddress + (i | curBlockIndex) * config->BytesPerBlock;

                    if((phySectAddr < BaseAddressEnd) &&
                       GetSectorMetadataInternal( config, phySectAddr, (SectorMetadata*)&meta ) && 
                       meta.IsBlockFree())
                    {
                        phyNewBlockAddress = phySectAddr;

                        return TRUE;
                    }   
                }
            }

            maxBits++;

            if((1ul << maxBits) > maxValue)
            {
                break;
            }
        }

        if(fRound2) return FALSE;

        //
        // block indexes are stored as inverted bits (to enable more flash writes 
        // since you can go from 1->0 but not the inverse)
        //
        curBlockIndex = ~(metaVirt.wBlockOffset[1]);

        maxBits = BitsSet16(curBlockIndex) + 1;
        fRound2 = TRUE;
    }

    return FALSE;
}


BOOL BS_WearLeveling_Driver::GetPhysicalBlockAddress(BS_WearLeveling_Config* config, ByteAddress virtAddress, ByteAddress &phyBlockAddress, BOOL fAllocateNew)
{
    UINT16            virtualBlockIndex  = GetBlockIndex( virtAddress, config->BaseAddress, config->BytesPerBlock );
    ByteAddress       virtBlockAddress   = config->BaseAddress + config->BytesPerBlock * virtualBlockIndex;
    WL_SectorMetadata meta;

    phyBlockAddress = virtBlockAddress;

    //
    // The block map offset is the index to the referenced block
    //
    if(GetSectorMetadataInternal( config, virtBlockAddress, (SectorMetadata*)&meta ) && !meta.IsBadBlock())
    {
        BOOL fOffset = FALSE;
        //
        // This block maps to another location
        //
        if(meta.IsValidBlockMapOffset())
        {
            phyBlockAddress = config->BaseAddress + meta.GetBlockMapOffset() * config->BytesPerBlock;
            
            GetSectorMetadataInternal( config, phyBlockAddress, (SectorMetadata*)&meta );
            
            fOffset = TRUE;
        }
        
        // 
        // This block is owned by another block and we are not mapped OR the block is trash
        // Then we make a new map for this block.   
        //
        if((!fOffset && meta.IsValidOwnerBlock()) || 
            ((!meta.IsValidOwnerBlock() || fOffset) && meta.IsBlockTrash()))
        {
            if(fAllocateNew)
            {
                SectorAddress          phyNewBlockAddress;
                const BlockDeviceInfo *pDevInfo     = config->Device->GetDeviceInfo(config->BlockConfig);
                ByteAddress            virtSectAddr = virtAddress - (virtAddress % pDevInfo->BytesPerSector);
                

                if(!HandleBlockReplacement(config, virtSectAddr, phyBlockAddress, phyNewBlockAddress))
                {
                    return FALSE;
                }

                phyBlockAddress = phyNewBlockAddress;
            }
            else
            {
                return FALSE;
            }
        }
    }
    //
    // Look in the bad block list for this address
    //
    else
    {
        WL_BadBlockMap *pBadList = config->BadBlockList;
        
        while(pBadList != NULL)
        {
            if(pBadList->VirtualBlockIndex == virtualBlockIndex)
            {
                if(pBadList->PhysicalBlockIndex == WL_SectorMetadata::c_FREE_LINK_INDEX) break;
                
                phyBlockAddress = config->BaseAddress + pBadList->PhysicalBlockIndex * config->BytesPerBlock;

                //
                // Assign new replacement block if the old one is trash or went bad
                //
                if(!GetSectorMetadataInternal( config, phyBlockAddress, (SectorMetadata*)&meta ) || meta.IsBadBlock())
                {
                    break;
                }

                if(meta.IsBlockTrash())
                {
                    FreeBadBlockReplacement(config, virtBlockAddress);
                    break;
                }

                _WEAR_LEVEL_ASSERT(meta.wOwnerBlock == virtualBlockIndex);
                
                return TRUE;
            }
            pBadList = pBadList->Next;
        }

        if(!fAllocateNew) return FALSE;

        if(!ReplaceBadBlock(config, virtBlockAddress, phyBlockAddress))
        {
            _WEAR_LEVEL_ASSERT(FALSE);
            return FALSE;
        }
    }

    _WEAR_LEVEL_ASSERT(phyBlockAddress <= (config->BaseAddress + ((config->BlockIndexMask << 1) - 1) * config->BytesPerBlock));    

    return TRUE;
}

BOOL BS_WearLeveling_Driver::ReplaceBlock(BS_WearLeveling_Config* config, ByteAddress virtAddress, ByteAddress currentBlockAddr, ByteAddress phyNewBlockAddress)
{
    WL_SectorMetadata meta;
    ByteAddress    phyNewSectAddr;
    const BlockDeviceInfo *pDevInfo = config->Device->GetDeviceInfo(config->BlockConfig);
    const UINT32   BytesPerSector   = pDevInfo->BytesPerSector;
    ByteAddress    virtBlockAddress = GetBlockStartAddress( virtAddress, config->BaseAddress, config->BytesPerBlock );
    UINT32         SectorCount      = config->BytesPerBlock / BytesPerSector;
    UINT32         curSectAddr      = GetBlockStartAddress( currentBlockAddr, config->BaseAddress, config->BytesPerBlock );

    currentBlockAddr = curSectAddr;
    phyNewSectAddr   = phyNewBlockAddress;

    UINT8 *pData = (UINT8*)SimpleHeap_Allocate(BytesPerSector);

    if(pData == NULL) return FALSE;

    //
    // Copy the data sector by sector
    //
    for(UINT32 i=0; i<SectorCount; i++)
    {
        if(GetSectorMetadataInternal( config, curSectAddr, (SectorMetadata*)&meta ))
        {
            BOOL fLinkedSector = FALSE;
            SectorAddress curAddr = curSectAddr;
            
            // if the block is trash do not copy it!
            if(i == 0 && (meta.IsBlockTrash() || !meta.IsBlockDirty()))
            {
                break;
            }
            if(meta.IsValidSectorMap()) 
            {
                UINT32 linkedSectAddr = currentBlockAddr + meta.wMappedSectorOffset * BytesPerSector;
                fLinkedSector = TRUE;

                //
                // Since the sector is mapped, we need to unwind the linkage and place the physical sector back into its
                // direct mapping location.
                //
                while(TRUE)
                {
                    if(!GetSectorMetadataInternal( config, linkedSectAddr, (SectorMetadata*)&meta )) 
                    {
                        _WEAR_LEVEL_ASSERT(FALSE);
                        break;
                    }

                    //
                    // Follow the link to the next sector
                    //
                    if(meta.IsValidSectorLink()) 
                    {
                        linkedSectAddr = currentBlockAddr + meta.wLinkedSectorOffset * BytesPerSector;
                    }
                    
                    //
                    // we are at the end of the linked mapping move the data to the direct map location of the new block
                    //
                    else
                    {
                        curAddr = linkedSectAddr;
                        break;
                    }
                }
            }

            //
            // Only copy if the sector is still valid
            //
            if( !meta.IsSectorBad()   && (!meta.IsSectorMapped() || fLinkedSector) && 
                !meta.IsSectorTrash() &&   meta.IsSectorInUse()  && meta.IsSectorDirty())
            {
                //
                // Check the old sectors data integrity 
                //
                if(config->Device->Read(config->BlockConfig, curAddr, BytesPerSector, pData))//&&
                   //meta.CRC == 0 || meta.CRC == SUPPORT_ComputeCRC(pData, BytesPerSector, 0))
                {  
                    _WEAR_LEVEL_ASSERT(meta.CRC == 0 || meta.CRC == SUPPORT_ComputeCRC(pData, BytesPerSector, 0));
                    
                    if(!WriteToPhysicalSector( config, phyNewSectAddr, pData, BytesPerSector ))
                    {
                        _WEAR_LEVEL_ASSERT(FALSE);
                        SimpleHeap_Release(pData);
                        return FALSE;
                    }
                }
                else
                {
                    //
                    // Not much we can do at this point
                    //
                    _WEAR_LEVEL_ASSERT(FALSE);

                    GetSectorMetadataInternal(config, currentBlockAddr, (SectorMetadata*)&meta);
                    meta.SetBlockBad();
                    SetSectorMetadataInternal(config, currentBlockAddr, (SectorMetadata*)&meta);
                }
            }
        }
        else
        {
            // Unable to read sector meta data
            _WEAR_LEVEL_ASSERT(FALSE);
        }

        curSectAddr    += BytesPerSector;
        phyNewSectAddr += BytesPerSector;
    }   

    SimpleHeap_Release(pData);

    //
    // only format the old block if it was a mapped block
    //
    if(virtBlockAddress != currentBlockAddr)
    {
        GetSectorMetadataInternal( config, currentBlockAddr, (SectorMetadata*)&meta );

        _WEAR_LEVEL_ASSERT(!meta.IsValidOwnerBlock() || meta.wOwnerBlock == GetBlockIndex(virtBlockAddress, config->BaseAddress, config->BytesPerBlock));

        meta.SetOwnerBlockInvalid();

        meta.SetBlockTrash();

#ifdef _VISUAL_WEARLEVELING_
SetBlockColor( currentBlockAddr, COLOR_RED, config );
#endif
        if(!SetSectorMetadataInternal( config, currentBlockAddr, (SectorMetadata*)&meta ))
        {
            _WEAR_LEVEL_ASSERT(FALSE);
        }
    }

    //
    // Now set up the sector metadata
    //
    if(virtBlockAddress != phyNewBlockAddress)
    {
        WL_SectorMetadata origMeta;

        if(GetSectorMetadataInternal( config, virtBlockAddress, (SectorMetadata*)&origMeta ) && !origMeta.IsBadBlock())
        {
            UINT16 phyNewBlockIndex = GetBlockIndex( phyNewBlockAddress, config->BaseAddress, config->BytesPerBlock );
            UINT16 virtBlockIndex   = GetBlockIndex( virtBlockAddress  , config->BaseAddress, config->BytesPerBlock );

            //
            // Link the direct mapped block with the new block
            //
            origMeta.SetBlockMapOffset(phyNewBlockIndex);

            if(!SetSectorMetadataInternal( config, virtBlockAddress, (SectorMetadata*)&origMeta ))
            {
                _WEAR_LEVEL_ASSERT(FALSE);
            }
        }
    }

    // 
    // Mark the new block as in use and mapped
    //
    if(!GetSectorMetadataInternal( config, phyNewBlockAddress, (SectorMetadata*)&meta ) || meta.IsBadBlock())
    {
        _WEAR_LEVEL_ASSERT(FALSE);
        return FALSE;
    }

    meta.SetBlockInUse();

    //
    // If the virtual address is not the physical address, we need to update the block mapping
    // meta data
    //
    if(virtBlockAddress != phyNewBlockAddress)
    {
        UINT16 virtBlockIndex = GetBlockIndex( virtBlockAddress, config->BaseAddress, config->BytesPerBlock );
        
        meta.wOwnerBlock = virtBlockIndex;
    }            

    if(!SetSectorMetadataInternal( config, phyNewBlockAddress, (SectorMetadata*)&meta ))
    {
        _WEAR_LEVEL_ASSERT(FALSE);
        return FALSE;
    }

    return TRUE;
}

BOOL BS_WearLeveling_Driver::HandleBlockReplacement(BS_WearLeveling_Config* config, ByteAddress virtAddress, ByteAddress phyBlockAddress, ByteAddress &phyNewBlockAddress)
{
    WL_SectorMetadata virtMeta;
    ByteAddress       virtBlockAddress  = GetBlockStartAddress( virtAddress, config->BaseAddress, config->BytesPerBlock );

    //
    // Handle bad block replacement
    //
    if(!GetSectorMetadataInternal(config, virtBlockAddress,(SectorMetadata*)&virtMeta) || virtMeta.IsBadBlock())
    {
        if(!ReplaceBadBlock(config, virtBlockAddress, phyNewBlockAddress))
        {
            _WEAR_LEVEL_ASSERT(FALSE);
            return FALSE;
        }
    }
    //
    // Handle normal block replacment
    //
    else
    {
        BOOL fCompact = TRUE;
        ByteAddress newAddr;

        // 
        // Get the next available block that can be mapped and replace the old
        // physical block
        //
        if(GetNextFreeBlock( config, virtBlockAddress, phyNewBlockAddress )) 
        {
            if(ReplaceBlock( config, virtBlockAddress, phyBlockAddress, phyNewBlockAddress ))
            {
                fCompact = FALSE;
            }
        }

        //
        // we have failed to get a valid reference block for the target location, so
        // now we will compact the blocks in an attempt to clean up the storage.
        // 
        if(fCompact)
        {
            if(!CompactBlocks(config, virtAddress))
            {
                _WEAR_LEVEL_ASSERT(FALSE);
                return FALSE;
            }

            // 
            // We should have a direct mapping after a successful compaction
            //
            phyNewBlockAddress = virtBlockAddress;
        }

        GetPhysicalBlockAddress(config, virtBlockAddress, newAddr, FALSE);

        if(newAddr != phyNewBlockAddress)
        {
            _WEAR_LEVEL_ASSERT(FALSE);
            
            phyNewBlockAddress = newAddr;
        }
    }
    
    return TRUE;
}

BOOL BS_WearLeveling_Driver::GetPhysicalAddress(BS_WearLeveling_Config* config, ByteAddress virtAddress, ByteAddress &phyAddress, BOOL &fDirectSectorMap)
{
    const BlockDeviceInfo *pDevInfo     = config->Device->GetDeviceInfo(config->BlockConfig);
    const UINT32      BytesPerSector    = pDevInfo->BytesPerSector;
    ByteAddress       virtBlockAddress  = GetBlockStartAddress( virtAddress, config->BaseAddress, config->BytesPerBlock );
    ByteAddress       virtSectAddr      = virtAddress - (virtAddress % BytesPerSector);
    UINT32            virtSectOffset    = virtSectAddr - virtBlockAddress;
    UINT32            virtualOffset     = virtAddress - virtSectAddr;
    ByteAddress       phySectAddr;
    ByteAddress       phyBlockAddr;
    WL_SectorMetadata meta;
    bool              fMappedSectAddr   = false;

    fDirectSectorMap = FALSE;

    //
    // First get the mapped block address
    //
    if(!GetPhysicalBlockAddress(config, virtAddress, phyBlockAddr, TRUE)) { _WEAR_LEVEL_ASSERT(FALSE); return FALSE; }

    //
    // Calculate the sector address from the mapped block
    //
    phySectAddr = phyBlockAddr + virtSectOffset;

    //
    // Determine the mapped sector address within the mapped block
    //
    while(TRUE)
    {
        if(!GetSectorMetadataInternal( config, phySectAddr, (SectorMetadata*)&meta ) || meta.IsSectorBad())
        {
            SectorAddress phyNewBlock;

            if(!HandleBlockReplacement(config, virtSectAddr, phyBlockAddr, phyNewBlock))
            {
                return FALSE;
            }

#ifdef _VISUAL_WEARLEVELING_
    SetBlockColor( phyBlockAddr, COLOR_PURPLE, config );
#endif
            phyBlockAddr = phyNewBlock;
            phySectAddr  = phyBlockAddr + virtSectOffset;

            // we will not have a mapped sector since we performed a block replacement
            fMappedSectAddr = false;

            break;
        }

        //
        // Sector mapping starts witht the wMappedSectorOffset and then each linked
        // sector uses the wLinkedSectorOffset.  fMappedSectAddr indicates whether or 
        // not we have started looking at linked sectors.
        //
        if(!fMappedSectAddr)
        {
            if(meta.IsValidSectorMap())
            {
                fMappedSectAddr = true;

                phySectAddr =  phyBlockAddr + meta.wMappedSectorOffset * BytesPerSector;
            }
            //
            // If the current sector is mapped but the wMappedSectorOffset is not set, then
            // it means this sector has not been used yet, so get the next free sector and
            // link it to this one.
            //
            else if(meta.IsSectorMapped())
            {
                WL_SectorMetadata metaNew;
                ByteAddress newPhyAddr;
                
                //
                // Get the next free sector in this block
                //
                if(GetNextFreeSector(config, phyBlockAddr, newPhyAddr, metaNew))
                {
                    metaNew.SetSectorMapped();
                    metaNew.SetSectorInUse();

                    if(!SetSectorMetadataInternal( config, newPhyAddr, (SectorMetadata*)&metaNew ))
                    {
                        _WEAR_LEVEL_ASSERT(FALSE);
                    }

                    meta.wMappedSectorOffset = (newPhyAddr - phyBlockAddr) / BytesPerSector;

                    _WEAR_LEVEL_ASSERT(meta.wMappedSectorOffset < (config->BytesPerBlock / BytesPerSector));

                    if(!SetSectorMetadataInternal( config, phySectAddr, (SectorMetadata*)&meta ))
                    {
                        _WEAR_LEVEL_ASSERT(FALSE);
                    }

                    fMappedSectAddr = true;

                    phySectAddr = newPhyAddr;

                    break;
                }
                //
                // We ran out of free sectors, so replace this block (compacting the sectors)
                //
                else
                {
                    ByteAddress phyNewBlockAddress;

                    if(!HandleBlockReplacement(config, virtSectAddr, phyBlockAddr, phyNewBlockAddress))
                    {
                        return FALSE;
                    }

                    phyBlockAddr = phyNewBlockAddress;
                    phySectAddr  = phyBlockAddr + virtSectOffset;

                    // After compaction we should not have any sector mapping for this block
                    fMappedSectAddr = false;

                    break;
                }
            }
            else // we are at the correct sector already
            {
                break;
            }
        }
        else
        {
            //
            // No more linking so we must be at the correct sector
            //
            if( meta.wLinkedSectorOffset == WL_SectorMetadata::c_FREE_LINK_INDEX)
            {
                break;
            }
            else 
            {
                phySectAddr =  phyBlockAddr + meta.wLinkedSectorOffset * BytesPerSector;
            }
        }
    }

    fDirectSectorMap = !fMappedSectAddr;
    phyAddress       = phySectAddr + virtualOffset;

    return TRUE;
}

BOOL BS_WearLeveling_Driver::Read(void *context, ByteAddress virtAddress, UINT32 NumBytes, BYTE *pSectorBuff)
{
    //GLOBAL_LOCK(x);

    BS_WearLeveling_Config *config = (BS_WearLeveling_Config*)context;

    if(config == NULL || config->Device == NULL) return FALSE;

    const BlockDeviceInfo *pDevInfo = config->Device->GetDeviceInfo(config->BlockConfig);

    SectorAddress phyAddr;
    UINT32        offset        = virtAddress % pDevInfo->BytesPerSector;
    UINT32        bytes         = (NumBytes + offset > pDevInfo->BytesPerSector ? pDevInfo->BytesPerSector - offset : NumBytes);
    BYTE*         pRead         = pSectorBuff;
    BOOL          fDirectMap;

    if(config == NULL || config->Device == NULL) return FALSE;

    //
    // We must read sector by sector to ensure sector validity
    //
    while(NumBytes > 0)
    {
        if(GetPhysicalAddress(config, virtAddress, phyAddr, fDirectMap))
        {
            if(!config->Device->Read(config->BlockConfig, phyAddr, bytes, pRead)) return FALSE;
        }
        else
        {
            return FALSE;
        }

        NumBytes    -= bytes;
        virtAddress += bytes;
        pRead       += bytes;

        bytes = __min(NumBytes, pDevInfo->BytesPerSector);
    }

    return TRUE;
}

BOOL BS_WearLeveling_Driver::Write(void *context, ByteAddress phyAddr, UINT32 NumBytes, BYTE *pSectorBuff, BOOL ReadModifyWrite )
{
    return WriteInternal(context, phyAddr, NumBytes, pSectorBuff, ReadModifyWrite, FALSE);
}

BOOL BS_WearLeveling_Driver::WriteToPhysicalSector(BS_WearLeveling_Config* config, ByteAddress sectStart, UINT8* pSectorData, UINT32 length )
{
    BOOL fRet = FALSE;
    WL_SectorMetadata meta;
    SectorAddress blockAddr;

    UINT8* pCrcBuffer = (UINT8*)SimpleHeap_Allocate(length); if(!pCrcBuffer) return FALSE;

    _WEAR_LEVEL_ASSERT(length == config->Device->GetDeviceInfo(config->BlockConfig)->BytesPerSector);
    
    //
    // Write the data to the new sector
    //
    if(!config->Device->Write(config->BlockConfig, sectStart, length, pSectorData, FALSE))
    {
        _WEAR_LEVEL_ASSERT(FALSE);
        goto CLEANUP;
    }

    //
    // Read the entire sector back into the buffer to check the CRC 
    //
    if(!config->Device->Read(config->BlockConfig, sectStart, length, pCrcBuffer) 
       || (0 != memcmp(pSectorData, pCrcBuffer, length)))
    {
        _WEAR_LEVEL_ASSERT(FALSE);
        goto CLEANUP;
    }

    if(GetSectorMetadataInternal(config, sectStart, (SectorMetadata*)&meta) && !meta.IsSectorBad())
    {
        UINT32 crc;
        crc = SUPPORT_ComputeCRC(pSectorData, length, 0);

        meta.SetSectorInUse();

        // reuse the CRC value if we can, but we can not flip bits from 0->1
        if((~meta.CRC) & crc) 
        {
            meta.CRC = 0;
        }
        else
        {
            meta.CRC = crc;
        }
    
        if(!SetSectorMetadataInternal(config, sectStart, (SectorMetadata*)&meta))
        {
            _WEAR_LEVEL_ASSERT(FALSE);
            goto CLEANUP;
        }
    }
    else
    {
        _WEAR_LEVEL_ASSERT(FALSE);
        goto CLEANUP;
    }

    blockAddr = GetBlockStartAddress( sectStart, config->BaseAddress, config->BytesPerBlock );
    //
    // Set the block in use bit
    //
    if(GetSectorMetadataInternal(config, blockAddr, (SectorMetadata*)&meta) && !meta.IsBadBlock())
    {
        if(!meta.IsBlockInUse() || !meta.IsBlockDirty())
        {
            meta.SetBlockInUse();
            meta.SetBlockDirty();

#ifdef _VISUAL_WEARLEVELING_
            SetBlockColor( blockAddr, COLOR_BLUE, config );
#endif
            if(!SetSectorMetadataInternal(config, blockAddr, (SectorMetadata*)&meta))
            {
                _WEAR_LEVEL_ASSERT(FALSE);
                goto CLEANUP;
            }
        }
    }
    else
    {
        _WEAR_LEVEL_ASSERT(FALSE);
        goto CLEANUP;
    }
    

    fRet = TRUE;
    
CLEANUP:
    SimpleHeap_Release(pCrcBuffer);

    return fRet;
}
    

BOOL BS_WearLeveling_Driver::WriteToSector(BS_WearLeveling_Config* config, ByteAddress sectStart, UINT8* pSectorData, UINT32 offset, UINT32 length, BOOL fMemFill)
{
    const BlockDeviceInfo *pDevInfo     = config->Device->GetDeviceInfo(config->BlockConfig);
    const UINT32    BytesPerSector      = pDevInfo->BytesPerSector;
    BOOL            fReadModifyNeeded   = FALSE;
    ByteAddress     phyAddr;
    ByteAddress     mappedSectStart;
    WL_SectorMetadata  phyMeta, origPhyMeta;
    ByteAddress     virtSectStart       = sectStart;
    ByteAddress     virtBlockStart      = GetBlockStartAddress( virtSectStart, config->BaseAddress, config->BytesPerBlock );
    BOOL            fDirectMap;
    BOOL            fResult             = FALSE;

    UINT8*          pSectorBuffer = (UINT8*)SimpleHeap_Allocate(BytesPerSector); if(!pSectorBuffer) { return FALSE; }

    //
    // FIND THE NEXT AVAILABLE FREE SECTOR
    //
    if(!GetPhysicalAddress(config, virtSectStart, phyAddr, fDirectMap))
    {
        _WEAR_LEVEL_ASSERT(FALSE);
        goto CLEANUP;
    }

    if((offset + length) > BytesPerSector)
    {
        _WEAR_LEVEL_ASSERT(FALSE);
        length = BytesPerSector - offset;
    }

    GetSectorMetadataInternal(config, phyAddr, (SectorMetadata*)&origPhyMeta);
    
    mappedSectStart = phyAddr;

    //
    // Read in the entire sector that we wish to write (for read/modify/write), then we will store it in another
    // sector.
    //
    config->Device->Read(config->BlockConfig, phyAddr, BytesPerSector, pSectorBuffer);

    //
    // we only need a new sector if we are moving any bits from 0 -> 1 
    // 
    //if(origPhyMeta.IsSectorDirty())
    //{
    //    fReadModifyNeeded = TRUE;
    //}
    //else 
    if((!fMemFill || *pSectorData != 0) && origPhyMeta.IsSectorDirty()) 
    { 
        for(UINT32 i=0; i<length; i++)
        {
            if((fMemFill ? *pSectorData : pSectorData[i]) & (~pSectorBuffer[offset + i]))
            {
                fReadModifyNeeded = TRUE;
                break;
            }
        }
    }

    //
    // Fill or write memory into the sector buffer prior to saving to a new sector address
    //
    if(fMemFill)
    {
        memset( &pSectorBuffer[offset], *pSectorData, length );
    }
    else
    {
        memcpy( &pSectorBuffer[offset], pSectorData, length );
    }

    while(TRUE)
    {
        //
        // The current sector has data that can not be over written by the new data (bits from 0->1)
        //
        if(fReadModifyNeeded)
        {
            //
            // Try to find a new sector in this block
            //
            if(!GetNextFreeSector( config, phyAddr, mappedSectStart, phyMeta ))
            {
                ByteAddress phyBlockAddress    = GetBlockStartAddress( phyAddr, config->BaseAddress, config->BytesPerBlock );
                ByteAddress newPhyBlockAddress = 0;

                GetSectorMetadataInternal(config, phyAddr, (SectorMetadata*)&origPhyMeta);

                if(!origPhyMeta.IsSectorTrash())
                {
                    origPhyMeta.SetSectorTrash();

                    SetSectorMetadataInternal(config, phyAddr, (SectorMetadata*)&origPhyMeta);
                }

                if(!HandleBlockReplacement(config, virtSectStart, phyBlockAddress, newPhyBlockAddress))
                {
                    goto CLEANUP;
                }
                
                if(!GetPhysicalAddress( config, virtSectStart, phyAddr, fDirectMap ))
                {
                    _WEAR_LEVEL_ASSERT(FALSE);
                    goto CLEANUP;
                }

                _WEAR_LEVEL_ASSERT(GetSectorMetadataInternal(config, phyAddr, (SectorMetadata*)&origPhyMeta) && origPhyMeta.IsSectorFree());

                // if we compacted then we do not need to map the address
                fReadModifyNeeded = FALSE;                        
                mappedSectStart = phyAddr;
            }
            
        }
        else
        {
            mappedSectStart = phyAddr;
        }

        // 
        // Try again if we failed
        //
        if(!WriteToPhysicalSector(config, mappedSectStart, pSectorBuffer, BytesPerSector))
        {
            _WEAR_LEVEL_ASSERT(FALSE);
            goto BAD_SECTOR;
        }
        
        goto GOOD_SECTOR;

        //
        // BAD SECTOR - The CRC check failed on the write, which indicates we are at a bad sector, so mark the sector
        // and continue with the next free sector.
        //
BAD_SECTOR:
        if(GetSectorMetadataInternal(config, mappedSectStart, (SectorMetadata*)&phyMeta))
        {
            // make the block bad
            phyMeta.SetSectorBad();
                
            if(!SetSectorMetadataInternal(config, mappedSectStart, (SectorMetadata*)&phyMeta))
            {
                _WEAR_LEVEL_ASSERT(FALSE);
                goto CLEANUP;
            }
        }
        
        {
            ByteAddress phyNewBlock;
            ByteAddress phyOldBlock;

            if(GetPhysicalBlockAddress(config, virtBlockStart, phyOldBlock, FALSE))
            {
                if(!HandleBlockReplacement(config, virtSectStart, phyOldBlock, phyNewBlock))
                {
                    _WEAR_LEVEL_ASSERT(FALSE);
                    goto CLEANUP;
                }
            }
            else
            {
                _WEAR_LEVEL_ASSERT(FALSE);
                goto CLEANUP;
            }
            
            phyAddr = phyNewBlock + (virtSectStart - virtBlockStart);

            fReadModifyNeeded = FALSE;
        }
        
        
        // try again with a new sector
        continue;


        //
        // GOOD SECTOR - The CRC check passed so we need to update the sector metadata and the sector config map to indicate
        // this sector is in use.
        //
GOOD_SECTOR:
        if(fReadModifyNeeded)
        {
            SectorAddress blockAddr;
            
            if(!GetSectorMetadataInternal(config, mappedSectStart, (SectorMetadata*)&phyMeta) || phyMeta.IsSectorBad())
            {
                _WEAR_LEVEL_ASSERT(FALSE);
                goto BAD_SECTOR;
            }

            phyMeta.SetSectorMapped();

            if(!SetSectorMetadataInternal(config, mappedSectStart, (SectorMetadata*)&phyMeta))
            {
                _WEAR_LEVEL_ASSERT(FALSE);
                goto CLEANUP;
            }

            if(!GetSectorMetadataInternal(config, phyAddr, (SectorMetadata*)&origPhyMeta) || origPhyMeta.IsSectorBad())
            {
                _WEAR_LEVEL_ASSERT(FALSE);
                goto CLEANUP;
            }
            
            //
            // mark the old sector as trash
            //
            origPhyMeta.SetSectorTrash();

            blockAddr = GetBlockStartAddress( mappedSectStart, config->BaseAddress, config->BytesPerBlock );

            // Direct mapping uses the sector offset, and linked mapping uses the linked sector offset field
            //
            if(fDirectMap)
            {
                origPhyMeta.wMappedSectorOffset = (mappedSectStart - blockAddr) / BytesPerSector;
                _WEAR_LEVEL_ASSERT(origPhyMeta.wMappedSectorOffset < (config->BytesPerBlock / BytesPerSector));
            }                    
            else
            {
                origPhyMeta.wLinkedSectorOffset = (mappedSectStart - blockAddr) / BytesPerSector;
                _WEAR_LEVEL_ASSERT(origPhyMeta.wLinkedSectorOffset < (config->BytesPerBlock / BytesPerSector));
                _WEAR_LEVEL_ASSERT(origPhyMeta.wMappedSectorOffset != 0xFFFFFFFF);
            }
            
            if(!SetSectorMetadataInternal(config, phyAddr, (SectorMetadata*)&origPhyMeta))
            {
                _WEAR_LEVEL_ASSERT(FALSE);
                goto CLEANUP;
            }
        }

        // we are done so break out of the loop
        break;
    }

    fResult = TRUE;

CLEANUP:
    if(pSectorBuffer) { SimpleHeap_Release(pSectorBuffer); }

    return fResult;
}

BOOL BS_WearLeveling_Driver::WriteInternal(void *context, ByteAddress Address, UINT32 NumBytes, BYTE *pSectorBuff, BOOL ReadModifyWrite, BOOL fFillMem)
{
    //GLOBAL_LOCK(x);

    BS_WearLeveling_Config *config = (BS_WearLeveling_Config*)context;

    if(config == NULL || config->Device == NULL) return FALSE;

    const BlockDeviceInfo *pDevInfo = config->Device->GetDeviceInfo(config->BlockConfig);

    UINT32 BytesPerSector   = pDevInfo->BytesPerSector;
    UINT32 sectStart        = Address - (Address % BytesPerSector);
    UINT32 sectOffset       = Address - sectStart;
    UINT32 bytes            = (NumBytes + sectOffset > BytesPerSector ? BytesPerSector - sectOffset : NumBytes);
    BYTE*  pWrite           = pSectorBuff;

    //
    // Write sector by sector to ensure data integrity
    //
    while(NumBytes > 0)
    {
        if(!WriteToSector( config, sectStart, pWrite, sectOffset, bytes, fFillMem )) return FALSE;

        sectStart += BytesPerSector;

        sectOffset = 0;
        NumBytes  -= bytes;
        //
        // Only increment if we are not filling memory
        //
        if(!fFillMem) pWrite += bytes;

        bytes = __min(NumBytes, BytesPerSector);
    }

    return TRUE;    
}

BOOL BS_WearLeveling_Driver::Memset(void *context, ByteAddress phyAddr, UINT8 Data, UINT32 NumBytes )
{
    return WriteInternal(context, phyAddr, NumBytes, &Data, TRUE, TRUE);
}

BOOL BS_WearLeveling_Driver::GetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
{
    ASSERT(FALSE);

    return FALSE;
}

BOOL BS_WearLeveling_Driver::SetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
{
    ASSERT(FALSE);

    return FALSE;
}

BOOL BS_WearLeveling_Driver::GetSectorMetadataInternal(BS_WearLeveling_Config* config, ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
{
    BOOL retVal;
    
    if(config->Device->GetSectorMetadata( config->BlockConfig, SectorStart, pSectorMetadata ))
    {
        return TRUE;
    }

    retVal = config->Device->GetSectorMetadata( config->BlockConfig, SectorStart, pSectorMetadata );

    _WEAR_LEVEL_ASSERT(retVal);
    return retVal;
}
BOOL BS_WearLeveling_Driver::SetSectorMetadataInternal(BS_WearLeveling_Config* config, ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
{
    BOOL retVal;
    SectorMetadata smd;

#ifdef _DEBUG
    //
    // Validate we are not changing any zero's to ones for the sector metadata
    //
    config->Device->GetSectorMetadata( config->BlockConfig, SectorStart, &smd );

    UINT32* pSrc = (UINT32*)pSectorMetadata;
    UINT32* pDst = (UINT32*)&smd;

    for(int i=0; i<sizeof(smd); i+=sizeof(UINT32))
    {
        ASSERT(0 == (*pSrc++ & ~(*pDst++)));
    }
    
#endif

    retVal = config->Device->SetSectorMetadata( config->BlockConfig, SectorStart, pSectorMetadata );

    config->Device->GetSectorMetadata( config->BlockConfig, SectorStart, &smd );

    //
    // Retry
    //
    if(!retVal || 0 != memcmp(&smd, pSectorMetadata, sizeof(smd)))
    {
        config->Device->SetSectorMetadata( config->BlockConfig, SectorStart, pSectorMetadata );
        config->Device->GetSectorMetadata( config->BlockConfig, SectorStart, &smd );
        
        retVal = 0 == memcmp(&smd, pSectorMetadata, sizeof(smd));

        _WEAR_LEVEL_ASSERT(retVal);
    }

    return retVal;
}

BOOL BS_WearLeveling_Driver::IsBlockErased(void *context, ByteAddress Address, UINT32 BlockLength)
{
    BS_WearLeveling_Config *config = (BS_WearLeveling_Config*)context;

    if(config == NULL || config->Device == NULL) return FALSE;

    ByteAddress phyAddr;
    WL_SectorMetadata meta;

    if(!GetPhysicalBlockAddress(config, Address, phyAddr, FALSE)) return TRUE;

    if(!GetSectorMetadataInternal(config, phyAddr, (SectorMetadata*)&meta)) return FALSE;

    return (!meta.IsBadBlock() && (meta.IsBlockFree() || meta.IsBlockTrash()));
}

void BS_WearLeveling_Driver::ValidateOwnerBlock(BS_WearLeveling_Config *config, SectorAddress virtAddr)
{
    WL_SectorMetadata meta;
    //
    // Validate Owner block
    //
    if(GetSectorMetadataInternal(config, virtAddr, (SectorMetadata*)&meta) && !meta.IsBadBlock())      
    {
        if(meta.IsValidOwnerBlock())
        {
            WL_SectorMetadata metaOwner;
            ByteAddress ownerBlock = config->BaseAddress + meta.wOwnerBlock * config->BytesPerBlock;

            if(GetSectorMetadataInternal(config, ownerBlock, (SectorMetadata*)&metaOwner) && !metaOwner.IsBadBlock())
            {
                bool invalid = false;
                if(metaOwner.IsValidBlockMapOffset())
                {
                    ByteAddress mappedBlock = config->BaseAddress + metaOwner.GetBlockMapOffset() * config->BytesPerBlock;

                    if(mappedBlock != virtAddr)
                    {
                        invalid = true;
                    }
                }
                else
                {
                    invalid = true;
                }

                if(invalid)
                {
                    _WEAR_LEVEL_ASSERT(FALSE);
                    meta.SetBlockTrash();
                    SetSectorMetadataInternal(config, virtAddr, (SectorMetadata*)&meta);
                }
            }
            else
            {
                WL_BadBlockMap* pMap = GetBadBlockMap(config, ownerBlock);

                if(pMap)
                {
                    if(pMap->PhysicalBlockIndex != GetBlockIndex(virtAddr, config->BaseAddress, config->BytesPerBlock))
                    {
                        _WEAR_LEVEL_ASSERT(FALSE);
                        meta.SetOwnerBlockInvalid();
                        meta.SetBlockTrash();
                        SetSectorMetadataInternal(config, virtAddr, (SectorMetadata*)&meta);
                    }
                }
            }
        }
    }
}

BOOL BS_WearLeveling_Driver::EraseBlock(void *context, ByteAddress Address)
{
    //GLOBAL_LOCK(x);
    
    BS_WearLeveling_Config *config = (BS_WearLeveling_Config*)context;

    if(config == NULL || config->Device == NULL) return FALSE;

    ByteAddress       virtAddr = GetBlockStartAddress( Address, config->BaseAddress, config->BytesPerBlock );
    ByteAddress       phyAddr;
    WL_SectorMetadata meta;

    //
    // GetPhysicalBlockAddress will return false if the virtual address is not assigned yet
    //
    if(!GetPhysicalBlockAddress(config, virtAddr, phyAddr, FALSE))
    {   
        //
        // Make sure we don't have any orphans running around
        //
        ValidateOwnerBlock(config, virtAddr);
        return TRUE;
    }

    if(GetSectorMetadataInternal(config, phyAddr, (SectorMetadata*)&meta) && !meta.IsBadBlock())
    {
        //
        // if the block is free, then the block is already erased
        //
        if(!meta.IsBlockTrash() && !meta.IsBlockFree())
        {
            meta.SetBlockTrash();

            SetSectorMetadataInternal(config, phyAddr, (SectorMetadata*)&meta);

#ifdef _VISUAL_WEARLEVELING_
            SetBlockColor( phyAddr, COLOR_RED, config );
#endif                
        }
    }
    else
    {
        _WEAR_LEVEL_ASSERT(FALSE);
        return FALSE;
    }

    return TRUE;
}


void BS_WearLeveling_Driver::SetPowerState(void *context, UINT32 State)
{
    BS_WearLeveling_Config *config = (BS_WearLeveling_Config*)context;

    if(config == NULL || config->Device == NULL) return;

    return config->Device->SetPowerState(config->BlockConfig, State);
}

UINT32 BS_WearLeveling_Driver::MaxSectorWrite_uSec(void *context)
{
    BS_WearLeveling_Config *config = (BS_WearLeveling_Config*)context;

    if(config == NULL || config->Device == NULL) return FALSE;

    return config->Device->MaxSectorWrite_uSec(config->BlockConfig);
}

UINT32 BS_WearLeveling_Driver::MaxBlockErase_uSec(void *context)
{
    BS_WearLeveling_Config *config = (BS_WearLeveling_Config*)context;

    if(config == NULL || config->Device == NULL) return FALSE;

    return config->Device->MaxBlockErase_uSec(config->BlockConfig);
}

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata = "g_BS_WearLeveling_DeviceTable"
#endif

struct IBlockStorageDevice g_BS_WearLeveling_DeviceTable = 
{
    &BS_WearLeveling_Driver::InitializeDevice, 
    &BS_WearLeveling_Driver::UninitializeDevice, 
    &BS_WearLeveling_Driver::GetDeviceInfo, 
    &BS_WearLeveling_Driver::Read, 
    &BS_WearLeveling_Driver::Write,
    &BS_WearLeveling_Driver::Memset,
    &BS_WearLeveling_Driver::GetSectorMetadata,
    &BS_WearLeveling_Driver::SetSectorMetadata,
    &BS_WearLeveling_Driver::IsBlockErased, 
    &BS_WearLeveling_Driver::EraseBlock, 
    &BS_WearLeveling_Driver::SetPowerState, 
    &BS_WearLeveling_Driver::MaxSectorWrite_uSec, 
    &BS_WearLeveling_Driver::MaxBlockErase_uSec, 
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata 
#endif 

