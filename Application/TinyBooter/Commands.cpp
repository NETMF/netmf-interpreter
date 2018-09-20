////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "TinyBooter.h"
#include <TinyBooterEntry.h>
#include "ConfigurationManager.h"
#include <TinyCLR_Endian.h>


////////////////////////////////////////////////////////////////////////////////////////////////////

extern Loader_Engine g_eng;

//--//

UINT8* g_ConfigBuffer       = NULL;
int    g_ConfigBufferLength = 0;
static const int g_ConfigBufferTotalSize = sizeof(ConfigurationSector);


//--//

static const int AccessMemory_Check    = 0x00;
static const int AccessMemory_Read     = 0x01;
static const int AccessMemory_Write    = 0x02;
static const int AccessMemory_Erase    = 0x03;
static const int AccessMemory_Mask     = 0x0F;

static bool AccessMemory( UINT32 location, UINT32 lengthInBytes, BYTE* buf, int mode );

//--//

struct BitFieldManager
{

    volatile SECTOR_BIT_FIELD* m_signatureCheck ;
    SECTOR_BIT_FIELD           m_skipCfgSectorCheck;
    BOOL                       m_fSignatureCheckDirty;
    BlockStorageDevice*        m_blockDevice;
    UINT32                     m_cfgPhysicalAddress;
    const BlockRegionInfo*     m_region;    
    BOOL                       m_fUsingRAM;

    //--//
    
    static const UINT32 s_bitFieldOffset = offsetof(ConfigurationSector, SignatureCheck);

    //--//
 
    // current non-volatile signature check sector bitfield
    #define BITS_PER_UINT32 (8*sizeof(UINT32))
    #define TINYBOOTER_SECTOR_BITFIELD_VALID_BIT   (1ul<<(BITS_PER_UINT32-1))
    #define TINYBOOTER_IS_SECTOR_BITFIELD_VALID(x) (TINYBOOTER_SECTOR_BITFIELD_VALID_BIT == (x->BitField[ SECTOR_BIT_FIELD::c_MaxFieldUnits-1 ] & TINYBOOTER_SECTOR_BITFIELD_VALID_BIT))

    void ResetBitFieldManager()
    {
        m_signatureCheck       = NULL;
        m_fSignatureCheckDirty = false;
        m_blockDevice          = NULL;
        m_cfgPhysicalAddress   = 0;
        m_region               = NULL;
        m_fUsingRAM            = FALSE;
    }

    // The ResetSignatureCheckArray method is called when there are no available signature check bitfields left.  
    // Reset the SECTOR_BIT_FIELD in teh CONFIG block of the Current pointing block device
    void ResetSignatureCheckArray()
    {
        BYTE*           data;

        if(m_blockDevice == NULL) return; // no Block device has set or RAM device

        if(m_fUsingRAM) 
        {
            memset(&m_skipCfgSectorCheck, 0xff, sizeof(m_skipCfgSectorCheck));
            m_signatureCheck = &m_skipCfgSectorCheck;
        }
        else 
        {
            const BlockDeviceInfo* deviceInfo = m_blockDevice->GetDeviceInfo();
            
            data = (BYTE*)private_malloc( m_region->BytesPerBlock );

            if(data != NULL)
            {
                // read the whole configure block, no matter it is NAND/NOR
                if(deviceInfo->Attribute.SupportsXIP)
                {
                    memcpy(data, (void*)m_cfgPhysicalAddress, m_region->BytesPerBlock);
                }
                else
                {
                    m_blockDevice->Read( m_cfgPhysicalAddress, m_region->BytesPerBlock, data );
                }
    
                m_blockDevice->EraseBlock( m_cfgPhysicalAddress );
                // erase signature check area
                ConfigurationSector *pCfg = (ConfigurationSector*)data;
                memset( (void*)&pCfg->SignatureCheck[ 0 ], 0xFF, sizeof(pCfg->SignatureCheck) );
                m_blockDevice->Write( m_cfgPhysicalAddress, m_region->BytesPerBlock,data, FALSE );
                private_free(data);
            }
            else
            {
                debug_printf("Failed to malloc spaces at ResetSignatureCheckArray\r\n");
            }
        }
    }

    // The SetDirtySectorBit method is used to mark a sector as dirty.  It does so by setting a bit in non-
    // volatile FLASH memory.  The sectors are indexed from LSb to MSb.  The MSb bit in the last word of the
    // entire bitfield indicates the validity of the entire field. 
    // if is at NAND, SECTOR_BIT_FIELD bitField is most likely point to RAM has been loaded up 
    void SetDirtySectorBit( UINT32 sectorIndex ) 
    { 
        volatile FLASH_WORD* dataAddr;
        volatile FLASH_WORD* dataRdAddr;

        FLASH_WORD           data;

        if(sectorIndex > SECTOR_BIT_FIELD::c_MaxSectorCount) return;  // Error condition!  

        if(m_fUsingRAM) 
        {
            m_signatureCheck->BitField[ sectorIndex / BITS_PER_UINT32 ] &= ~( 1ul << (sectorIndex % BITS_PER_UINT32) );
            m_fSignatureCheckDirty = true; 

            return;
        }

        if(m_blockDevice == NULL) return; // no Block device has set or it is RAM write, then no need to check dirty bit

        const BlockDeviceInfo* deviceInfo = m_blockDevice->GetDeviceInfo();
        
        m_fSignatureCheckDirty = true; 
        
        if(deviceInfo->Attribute.SupportsXIP)
        {
            dataAddr = (volatile FLASH_WORD*)&m_signatureCheck->BitField[ sectorIndex / BITS_PER_UINT32 ];
            
            // read back the 
            dataRdAddr = (volatile FLASH_WORD*)CPU_GetUncachableAddress( dataAddr );
            data = (*dataRdAddr) & ~( 1ul << (sectorIndex % BITS_PER_UINT32) );

            // write directly
            m_blockDevice->Write( (UINT32)dataAddr, sizeof(FLASH_WORD), (BYTE*)&data, FALSE );

        }
        else
        {
            UINT32 length  = m_region->BytesPerBlock;
            BYTE*  dataptr = (BYTE*)private_malloc(length);
            
            if(dataptr != NULL)
            {
                // read the whole configure block, no matter 
                // There is a assumption that ConfigurationSector is at the beginning of the Block if there is A CONFIG BLOCK in the device
                m_blockDevice->Read( m_cfgPhysicalAddress, length, dataptr );

                // erase signature check area
                ConfigurationSector *pCfg = (ConfigurationSector*)dataptr;
                
                for( int i = 0; i < ConfigurationSector::c_MaxSignatureCount; ++i )
                {
                    SECTOR_BIT_FIELD* bit = (SECTOR_BIT_FIELD*)&pCfg->SignatureCheck[ i ];
                    
                    if(TINYBOOTER_IS_SECTOR_BITFIELD_VALID(bit))
                    {
                        bit->BitField[ sectorIndex / BITS_PER_UINT32 ] &= ~( 1ul << (sectorIndex % BITS_PER_UINT32) );
                        break;
                    }
                }

                // write back to sector, as we only change one bit from 0 to 1, no need to erase sector
                m_blockDevice->Write( m_cfgPhysicalAddress, length, dataptr, FALSE );
                private_free(dataptr);
            }

        }     
    }


    // The GetNextSectorSignatureCheck method gets the next valid signature check bitfield for use during 
    // programming (or boot up).  It searches for a valid bitfield in the signature check bitfield array.
    // If a valid field can not be found, the array is reset and the first field is returned.
    void GetNextSectorSignatureCheck( BlockStorageDevice* device, SECTOR_BIT_FIELD& bitField )
    {
        m_blockDevice = device;

        if(device == NULL)  return;

        BlockStorageStream stream;

        if (!stream.Initialize( BlockUsage::CONFIG, device ))
        {    
            memset( &m_skipCfgSectorCheck, 0xff, sizeof(m_skipCfgSectorCheck ));
            m_signatureCheck        = &m_skipCfgSectorCheck;
            m_fUsingRAM             = TRUE;
            m_cfgPhysicalAddress    = 0;
            bitField = m_skipCfgSectorCheck;
        }
        else
        {
            UINT32 regionIndex, rangeIndex; 
            
            ConfigurationSector* configSector;
            BYTE*                data;
            
            const BlockDeviceInfo* deviceInfo = stream.Device->GetDeviceInfo();            

            m_cfgPhysicalAddress = stream.BaseAddress;

            if(!deviceInfo->FindRegionFromAddress( stream.BaseAddress, regionIndex, rangeIndex ))
            {
                ASSERT(FALSE);
                return;
            }

            m_region = &deviceInfo->Regions[ regionIndex ];

            m_fUsingRAM = FALSE;

            if(!deviceInfo->Attribute.SupportsXIP)
            {
                // have to read the whole block;
                UINT32 length = sizeof(ConfigurationSector);
                
                memset( &m_skipCfgSectorCheck, 0xff, sizeof(m_skipCfgSectorCheck) );
                data         = (BYTE*)private_malloc(length);
                stream.Device->Read( m_cfgPhysicalAddress, length, (BYTE *)data );
                configSector = (ConfigurationSector*)data;
                m_signatureCheck = NULL;
                
                for(int i=0; i<ConfigurationSector::c_MaxSignatureCount; i++)
                {

                    SECTOR_BIT_FIELD *pBF = (SECTOR_BIT_FIELD*)&configSector->SignatureCheck[ i ];

                    if(TINYBOOTER_IS_SECTOR_BITFIELD_VALID(pBF))
                    {
                        memcpy( (BYTE *)&m_skipCfgSectorCheck,(BYTE*) &configSector->SignatureCheck[ i ], sizeof(SECTOR_BIT_FIELD) );
                        bitField          =  m_skipCfgSectorCheck; 
                        m_signatureCheck  = &m_skipCfgSectorCheck;
                        break;
                    }
                }
                
                if(m_signatureCheck ==NULL)
                {
                    ResetSignatureCheckArray();
                    bitField         =  m_skipCfgSectorCheck;                     
                    m_signatureCheck = &m_skipCfgSectorCheck;
                }

                private_free(data);

            }
            else // XIP device
            {

                configSector = (ConfigurationSector*)CPU_GetUncachableAddress( m_cfgPhysicalAddress ); // global config not in flash for bootloader
            
                m_signatureCheck = NULL;
                
                for(int i=0; i<ConfigurationSector::c_MaxSignatureCount; i++)
                {
                    SECTOR_BIT_FIELD* pBF = (SECTOR_BIT_FIELD*)&configSector->SignatureCheck[ i ];
                    if(TINYBOOTER_IS_SECTOR_BITFIELD_VALID(pBF))
                    {
                        bitField = *pBF; 
                        m_signatureCheck =pBF;
                        break;
                    }
                }

                if(m_signatureCheck == NULL)
                {
                    ResetSignatureCheckArray();
                    bitField = configSector->SignatureCheck[ 0 ];
                    m_signatureCheck = (volatile SECTOR_BIT_FIELD*)&(configSector->SignatureCheck[ 0 ]);
                }
            }
        }
    }

    // The InvalidateSectorBitField method is used to invalidate the given signature bitfield
    // It does so by invalidating the most significant bit of the last word in the bitfield.
    void InvalidateSectorBitField()
    { 
        SECTOR_BIT_FIELD bitField;

        if(m_fSignatureCheckDirty) 
        { 
            // set the most significant bit of the last word to indicate that this bit field is invalid
            SetDirtySectorBit( SECTOR_BIT_FIELD::c_MaxSectorCount ); 
            GetNextSectorSignatureCheck( m_blockDevice, bitField );
            m_fSignatureCheckDirty = false; 
        } 
    }

// The EraseOnSignatureCheckFail method is called on initialization to validate the last
// programming 
    void EraseOnSignatureCheckFail()
    {
        SECTOR_BIT_FIELD bitField;
        BlockStorageDevice* device = BlockStorageList::GetFirstDevice();
        ConfigurationSector Cfg;

        while (device != NULL)
        {
            // clear all the member before checking.
            ResetBitFieldManager();
            GetNextSectorSignatureCheck( device, bitField );

            int iRegion = 0, iRange = 0, iAbsBlock = 0;
            const BlockDeviceInfo* deviceInfo = device->GetDeviceInfo();
            const BlockRegionInfo* pRegion = &deviceInfo->Regions[ iRegion ];
            
            for(int i=0; i<SECTOR_BIT_FIELD::c_MaxFieldUnits; i++ )
            {
                // zero bit represents a dirty sector
                if(bitField.BitField[ i ] != 0xFFFFFFFF)
                {
                    int sectorIndex = i * BITS_PER_UINT32;
                    m_fSignatureCheckDirty = true;

                    for(int j=0; j<BITS_PER_UINT32; j++)
                    {
                        // Erase any sector that is marked as dirty
                        if(0 == (bitField.BitField[ i ] & (1ul<<j)))
                        {
                            int blockIndex = sectorIndex + j;
                            bool found = false;
                            

                            while(!found)
                            {
                                while(!found)
                                {
                                    int rangeBlocks = pRegion->BlockRanges[ iRange ].GetBlockCount();
                                    
                                    if(iAbsBlock <= blockIndex && blockIndex <= (iAbsBlock + rangeBlocks))
                                    {
                                        ByteAddress eraseAddress = pRegion->BlockAddress(blockIndex - iAbsBlock);

                                        if (CheckFlashSectorPermission(device, eraseAddress))
                                            device->EraseBlock( eraseAddress );
                                        else
                                            debug_printf(" BOOTER BLOCK IS Dirty ??? !!! ");

                                        found = true;
                                        break;
                                    }

                                    iAbsBlock += rangeBlocks;
                                    iRange++;

                                    if(iRange >= pRegion->NumBlockRanges)
                                    {
                                        break;
                                    }
                                }

                                if(!found)
                                {
                                    iRegion++;
                                    if(iRegion < deviceInfo->NumRegions)
                                    {
                                        pRegion = &deviceInfo->Regions[ iRegion ];
                                    }
                                    else
                                    {
                                        ASSERT(FALSE);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // if we found any dirty sector (and erased them) then invalidate this sector field
            if(m_fSignatureCheckDirty)
            {
                InvalidateSectorBitField();
            }


            device = BlockStorageList::GetNextDevice( *device );
        }
        // clear the members data
    }
} g_BitFieldManager;// end of BitFiedManage

/////////////////////////////////////////////////////////////////////

//location is the physical address of the memory to be written
static bool AccessMemory( UINT32 location, UINT32 lengthInBytes, BYTE* buf, int mode )
{
    mode &= AccessMemory_Mask;
    BlockStorageDevice *device;
    ByteAddress sectAddress;

    if (BlockStorageList::FindDeviceForPhysicalAddress( &device, location, sectAddress )) 
    {
        UINT32 iRegion, iRange;
        const BlockDeviceInfo* deviceInfo = device->GetDeviceInfo() ;

        if(!device->FindRegionFromAddress( location, iRegion, iRange )) 
        {
            debug_printf(" Invalid condition - Fail to find the block number from the ByteAddress %x \r\n",location);  
        
            return FALSE;
        }

        UINT32        accessPhyAddress = location;
        BYTE*         bufPtr           = buf;
        BOOL          success          = TRUE;
        INT32         accessLenInBytes = lengthInBytes;
        INT32         blockOffset      = deviceInfo->Regions[ iRegion ].OffsetFromBlock( accessPhyAddress );

        if(blockOffset < 0)
        {
            blockOffset = 0;            
            ASSERT(FALSE);
        }

        for(; iRegion < deviceInfo->NumRegions; iRegion++)
        {
            const BlockRegionInfo *pRegion = &deviceInfo->Regions[ iRegion ];

            UINT32 RangeBaseAddress = pRegion->BlockAddress( pRegion->BlockRanges[ iRange ].StartBlock );
            UINT32 blockIndex       = pRegion->BlockIndexFromAddress( accessPhyAddress );
            UINT32 accessMaxLength  = pRegion->BytesPerBlock - blockOffset;

            blockOffset = 0;

            for(;blockIndex < pRegion->NumBlocks; blockIndex++)
            {
                //accessMaxLength =the current largest number of bytes can be read from the block from the address to its block boundary.
                UINT32 NumOfBytes = __min(accessMaxLength, accessLenInBytes);

                accessMaxLength  = pRegion->BytesPerBlock;

                if(blockIndex > pRegion->BlockRanges[ iRange ].EndBlock)
                {
                    iRange++;

                    if(iRange >= pRegion->NumBlockRanges)
                    {
                        ASSERT(FALSE);
                        break;
                    }

                    RangeBaseAddress = pRegion->BlockAddress( pRegion->BlockRanges[ iRange ].StartBlock );
                }

                switch(mode)
                {
                case AccessMemory_Check:
                case AccessMemory_Read:
                    if (deviceInfo->Attribute.SupportsXIP)
                    {
                        if(mode == AccessMemory_Check)
                        {
                            *(UINT32*)buf = SUPPORT_ComputeCRC( (const void*)accessPhyAddress, NumOfBytes, *(UINT32*)buf );
                        }
                        else
                        {
                            memcpy( (BYTE*)bufPtr, (const void*)accessPhyAddress, NumOfBytes );
                        }
                    }
                    else
                    {
                        if (mode == AccessMemory_Check)
                        {
                            bufPtr = (BYTE*) private_malloc(NumOfBytes);
                            if(!bufPtr) return false;
                        }

                        success = device->Read( accessPhyAddress, NumOfBytes, (BYTE *)bufPtr );

                        if(!success)  
                        {
                            if (mode == AccessMemory_Check)
                            {
                                private_free(bufPtr);
                            }
                                
                            break;
                        }

                        if (mode == AccessMemory_Check)  
                        {
                            *(UINT32*)buf = SUPPORT_ComputeCRC( bufPtr, NumOfBytes, *(UINT32*)buf );
                            private_free(bufPtr);
                        }
                    }
                    break;
                    
                    case AccessMemory_Write:
                        if(!CheckFlashSectorPermission(device, accessPhyAddress)) return false;
                            
                        if(!pRegion->BlockRanges[ iRange ].IsConfig())
                        {    
                            UINT32 startBlock = pRegion->BlockAddress( blockIndex );

                            if(accessPhyAddress == startBlock && !device->IsBlockErased( startBlock, pRegion->BytesPerBlock ))
                            {
                                device->EraseBlock( startBlock );
                            }
                            
                            if(!(success = device->Write( accessPhyAddress , NumOfBytes, (BYTE *)bufPtr, FALSE )))
                            {
                                debug_printf(" Failed WriteSector at location %x, size %x \r\n", accessPhyAddress, NumOfBytes);
                                break;
                            }
                        }
                        else // write to RAM for config sector
                        {
                            // new write to a config block.
                            if (accessPhyAddress == RangeBaseAddress)
                            {
                                 if(g_ConfigBuffer != NULL)
                                 {
                                     private_free(g_ConfigBuffer);
                                 }
                                 g_ConfigBufferLength = 0;
                                 
                                 // g_ConfigBuffer = (UINT8*)private_malloc(pRegion->BytesPerBlock);
                                 // Just allocate the configuration Sector size, configuration block can be large and not necessary to have that buffer.
                                 g_ConfigBuffer = (UINT8*)private_malloc(g_ConfigBufferTotalSize);

                            }
                            else if(g_ConfigBufferTotalSize < ( g_ConfigBufferLength + lengthInBytes))
                            {
                                UINT8* tmp = (UINT8*)private_malloc(g_ConfigBufferLength + lengthInBytes);

                                if(tmp == NULL)
                                {
                                    return false;
                                }

                                memcpy( tmp, g_ConfigBuffer, g_ConfigBufferLength );

                                private_free(g_ConfigBuffer);

                                g_ConfigBuffer = tmp;
                            }
                            
                            // out of memory or it was not re-writing from the begining of the config sector
                            if(g_ConfigBuffer == NULL)
                            {
                                return false;
                            }

                            memcpy( &g_ConfigBuffer[ g_ConfigBufferLength ], bufPtr, lengthInBytes );

                            g_ConfigBufferLength += lengthInBytes;
                        }
                        break;
                        
                    case AccessMemory_Erase:
                        if(!CheckFlashSectorPermission(device, accessPhyAddress)) return false;
                        
                        // don't erase of config sector (we will only do that after checking the signature in RAM)
                        if(!pRegion->BlockRanges[ iRange ].IsConfig())
                        {
                            success = device->EraseBlock( pRegion->BlockAddress( blockIndex ) );

                            if(!success) break;
                        }
                        break;
                    
                }

                accessLenInBytes -= NumOfBytes;

                if (accessLenInBytes <= 0 || (!success)) break;

                if(bufPtr) bufPtr += NumOfBytes;
                
                accessPhyAddress  += NumOfBytes;
            }

            if (accessLenInBytes <= 0 || (!success)) break;

            blockIndex = 0;
            iRange     = 0;
        }
    }
    else  // device == NULL ->RAM operation
    {
        UINT32 locationEnd     = location + lengthInBytes;
        UINT32 ramStartAddress = HalSystemConfig.RAM1.Base;
        UINT32 ramEndAddress   = ramStartAddress + HalSystemConfig.RAM1.Size ;

        if((location <ramStartAddress) || (location >=ramEndAddress) || (locationEnd >ramEndAddress) )
        {
            debug_printf(" Invalid address %x and range %x Ram Start %x, Ram end %x\r\n", location, lengthInBytes, ramStartAddress, ramEndAddress);

            return FALSE;
        }

        switch(mode)
        {
        case AccessMemory_Check:
           break;

        case AccessMemory_Read:
            memcpy( buf, (const void*)location, lengthInBytes );
            break;

        case AccessMemory_Write:

            BYTE * memPtr;
            memPtr = (BYTE*)location;
            memcpy( memPtr, buf, lengthInBytes );
            break;

        case AccessMemory_Erase:
            memPtr = (BYTE*)location;
            if (lengthInBytes !=0) memset( memPtr, 0xFF, lengthInBytes );
            break;
           
        default:
            break;
        }

    }

    return true;
}



////////////////////////////////////////////////////////////////////////////////////////////////////

bool Loader_Engine::SignedDataState::CheckDirty()
{
    return ((m_dataAddress != 0) || (m_dataLength != 0));
}

void Loader_Engine::SignedDataState::Reset()
{
    m_dataAddress = 0;
    m_dataLength  = 0;
    m_pDevice = NULL;
    g_BitFieldManager.ResetBitFieldManager();
}

void Loader_Engine::SignedDataState::EraseMemoryAndReset()
{
    if(m_dataAddress != 0)
    {
        AccessMemory( m_dataAddress, m_dataLength, NULL, AccessMemory_Erase );

        g_BitFieldManager.InvalidateSectorBitField();
        g_BitFieldManager.ResetBitFieldManager();
        
        Reset();    
    }
}

bool Loader_Engine::SignedDataState::VerifyContiguousData( UINT32 address, UINT32 length )
{

    BlockStorageDevice *device;
    ByteAddress sectAddr;
    UINT32 BlockType;
    UINT32 regionIndex, rangeIndex;

    if (BlockStorageList::FindDeviceForPhysicalAddress( &device, address, sectAddr)) 
    {
        SECTOR_BIT_FIELD p;
        const BlockDeviceInfo* deviceInfo = device->GetDeviceInfo();
        
        if(device->FindRegionFromAddress( sectAddr, regionIndex, rangeIndex))
        {
            BlockType = deviceInfo->Regions[ regionIndex ].BlockRanges[ rangeIndex ].RangeType;

            // The only blocks that should be distinguished by TinyBooter are CONFIG, 
            // Bootstrap and reserved blocks (DirtyBit is another version of CONFIG).
            if(BlockRange::IsBlockTinyBooterAgnostic(BlockType))
            {
                BlockType = BlockRange::BLOCKTYPE_CODE;
            }
        }
        else
        {
            return false;
        }
    }
    else
    {
        // if not found - RAM
        device    = NULL;
        sectAddr  = address;
        BlockType = 0;
    }
    // initial condition
    if(m_dataAddress == 0)
    {
        // We don't verify signature for config sector
        m_dataAddress = address;
        m_dataLength  = length;
        m_pDevice = device;

        // assume the data bytes per sector is same for all
        m_sectorType = BlockType;
        // set up the bitField manager
        SECTOR_BIT_FIELD p;
        
        g_BitFieldManager.GetNextSectorSignatureCheck( m_pDevice, p );

        return true;
    }
    else
    {
        if((device != m_pDevice ) || (BlockType != m_sectorType)  || ((m_dataAddress + m_dataLength)!= address )) return false;

        m_dataLength += length;
    }

    return true;
}

bool Loader_Engine::SignedDataState::VerifySignature( UINT8* signature, UINT32 length, UINT32 keyIndex )
{
    BYTE *signCheckedAddr;
    bool fret;

    if(
       (keyIndex >= ConfigurationSector::c_DeployKeyCount)  ||
       (length   != RSA_BLOCK_SIZE_BYTES)
      )
    {
        EraseMemoryAndReset();

        return false;
    }

   // for RAM device, return 
    if (m_pDevice == NULL)
    {
        Reset();

        return TRUE;
    }
 
    // if it is non-XIP device, need to reload the content from memory to RAM for signature checking.
    const BlockDeviceInfo* deviceInfo = m_pDevice->GetDeviceInfo();
    if(!deviceInfo->Attribute.SupportsXIP)
    {
        signCheckedAddr = (BYTE*)private_malloc(m_dataLength);
        if (signCheckedAddr == NULL)
        {    
            EraseMemoryAndReset();

            return false;
        }
        if(!m_pDevice->Read( m_dataAddress, m_dataLength, signCheckedAddr ))
        {    
            EraseMemoryAndReset();
            private_free(signCheckedAddr);

            return false;
        }
    }
    else
    {
        signCheckedAddr = (BYTE *)m_dataAddress;
    }
        
    CryptoState st( (UINT32)signCheckedAddr, m_dataLength, signature, length, m_sectorType );

    if(st.VerifySignature( keyIndex ))
    {
        g_BitFieldManager.InvalidateSectorBitField();
        Reset();

        fret = true;
    }
    else
    {
        EraseMemoryAndReset();
        fret =false;
    }
    
    if(!deviceInfo->Attribute.SupportsXIP)
    {
        private_free(signCheckedAddr);
    }

    return fret;

}
          
////////////////////////////////////////////////////////////////////////////////////////////////////

static const Loader_Engine::CommandHandlerLookup c_Lookup_Request[] =
{
    /*******************************************************************************************************************************************************************/
#define DEFINE_CMD(cmd) { CLR_DBG_Commands::c_Monitor_##cmd, &Loader_Engine::Monitor_##cmd }
    DEFINE_CMD(Ping       ),
    DEFINE_CMD(Reboot     ),
    //
    DEFINE_CMD(ReadMemory ),
    DEFINE_CMD(WriteMemory),
    DEFINE_CMD(CheckMemory),
    DEFINE_CMD(EraseMemory),
    //
    DEFINE_CMD(Execute    ),
    DEFINE_CMD(MemoryMap  ),
    //
    DEFINE_CMD(CheckSignature),
    //
    DEFINE_CMD(FlashSectorMap    ),
    DEFINE_CMD(SignatureKeyUpdate),
    
    DEFINE_CMD(OemInfo),

#undef DEFINE_CMD
    /*******************************************************************************************************************************************************************/
};

static const Loader_Engine::CommandHandlerLookup c_Lookup_Reply[] =
{
    /*******************************************************************************************************************************************************************/
#define DEFINE_CMD(cmd) { CLR_DBG_Commands::c_Monitor_##cmd, &Loader_Engine::Monitor_##cmd }
    DEFINE_CMD(Ping),
#undef DEFINE_CMD
    /*******************************************************************************************************************************************************************/
};

////////////////////////////////////////////////////////////////////////////////////////////////////

bool Loader_Engine::Phy_ReceiveBytes( void* state, UINT8* & ptr, UINT32 & size )
{
    Loader_Engine* pThis = (Loader_Engine*)state;

    if(size)
    {
        int read = DebuggerPort_Read( pThis->m_port, (char*)ptr, size ); if(read <= 0) return false;

        ptr  += read;
        size -= read;
    }

    return true;
}

bool Loader_Engine::Phy_TransmitMessage( void* state, const WP_Message* msg )
{
    Loader_Engine* pThis = (Loader_Engine*)state;

    return pThis->TransmitMessage( msg, true );
}

//--//

bool Loader_Engine::App_ProcessHeader( void* state, WP_Message* msg )
{
    Loader_Engine* pThis = (Loader_Engine*)state;

    if(pThis->ProcessHeader( msg ) == false)
    {
        return false;
    }

    if(LOADER_ENGINE_ISFLAGSET(pThis, c_LoaderEngineFlag_ReceptionBufferInUse) || msg->m_header.m_size > sizeof(pThis->m_receptionBuffer))
    {
        return false;
    }

    msg->m_payload = pThis->m_receptionBuffer;
    LOADER_ENGINE_SETFLAG(pThis, c_LoaderEngineFlag_ReceptionBufferInUse);

    return true;
}

bool Loader_Engine::App_ProcessPayload( void* state, WP_Message* msg )
{
    Loader_Engine* pThis = (Loader_Engine*)state;

    // Prevent processing duplicate packets
    if((UINT32)msg->m_header.m_seq == pThis->m_lastPacketSequence)
        return false;       // Do not even respond to a repeat packet
    pThis->m_lastPacketSequence = (UINT32)msg->m_header.m_seq;

    if(pThis->ProcessPayload( msg ) == false)
    {
        return false;
    }

    return true;
}

bool Loader_Engine::App_Release( void* state, WP_Message* msg )
{
    Loader_Engine* pThis = (Loader_Engine*)state;

    if(msg->m_payload == pThis->m_receptionBuffer)
    {
        LOADER_ENGINE_CLEARFLAG(pThis, c_LoaderEngineFlag_ReceptionBufferInUse);

        msg->m_payload = NULL;
    }

    return true;
}

//--//

const WP_PhysicalLayer c_Debugger_phy =
{
    &Loader_Engine::Phy_ReceiveBytes   , 
    &Loader_Engine::Phy_TransmitMessage, 
};

const WP_ApplicationLayer c_Debugger_app =
{
    &Loader_Engine::App_ProcessHeader , 
    &Loader_Engine::App_ProcessPayload, 
    &Loader_Engine::App_Release       , 
};

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT Loader_Engine::Initialize( COM_HANDLE port )
{
    TINYCLR_HEADER();

    LOADER_ENGINE_CLEARFLAG(this, c_LoaderEngineFlag_ValidConnection);

    m_port = port;

    // Initialize to a packet sequence number impossible to encounter
    m_lastPacketSequence = 0x00FEFFFF;


    m_controller.Initialize( MARKER_DEBUGGER_V1, &c_Debugger_phy, &c_Debugger_app, this );

    m_signedDataState.Reset();

    g_PrimaryConfigManager.LocateConfigurationSector( BlockUsage::CONFIG );
    
    // make sure we performed signature check during the last programming
    g_BitFieldManager.EraseOnSignatureCheckFail();

    
    TINYCLR_NOCLEANUP_NOLABEL();
}

//--//

void Loader_Engine::ProcessCommands()
{
    m_controller.AdvanceState();
}

bool Loader_Engine::ProcessHeader( WP_Message* msg )
{
    return true;
}

bool Loader_Engine::ProcessPayload( WP_Message* msg )
{
    if(msg->m_header.m_flags & WP_Flags::c_NACK)
    {
        //
        // Bad packet...
        //
        return true;
    }

    LOADER_ENGINE_SETFLAG( this, c_LoaderEngineFlag_ValidConnection );

    //--//
#if defined(NETMF_TARGET_BIG_ENDIAN)
    SwapEndian( msg, msg->m_payload, msg->m_header.m_size, false );
#endif
    size_t                      num;
    const CommandHandlerLookup* cmd;

    if(msg->m_header.m_flags & WP_Flags::c_Reply)
    {
        num = ARRAYSIZE(c_Lookup_Reply);
        cmd =           c_Lookup_Reply;
    }
    else
    {
        num = ARRAYSIZE(c_Lookup_Request);
        cmd =           c_Lookup_Request;
    }

    while(num--)
    {
        if(cmd->cmd == msg->m_header.m_cmd)
        {
            ReplyToCommand( msg, (this->*(cmd->hnd))( msg ), false );
            return true;
        }

        cmd++;
    }

    ReplyToCommand( msg, false, false );
    return true;
}

bool Loader_Engine::EnumerateAndLaunch()
{
    static const int    MAX_PROGRAMS = 8;
    
    UINT32              programs[ MAX_PROGRAMS ];
    UINT32              programCount=0;

    //
    // build a list of programs found in memory sector start locations
    //
    //--//

    BlockStorageStream codeStream;
    FLASH_WORD ProgramWordCheck = 0;
    FLASH_WORD *pWord = &ProgramWordCheck;

    if(!codeStream.Initialize( BlockUsage::CODE )) return false;

    do
    {
        do
        {
            UINT32 addr = codeStream.CurrentAddress();

            codeStream.Read( (BYTE**)&pWord, sizeof(FLASH_WORD) );

            if(*pWord == Tinybooter_ProgramWordCheck())
            {
                hal_printf("*** nXIP Program found at 0x%08x\r\n", addr );
                programs[ programCount++ ] = addr;
            }

            if(programCount == MAX_PROGRAMS) break;

            // reset the pointer and default value
            ProgramWordCheck = 0;
            pWord = &ProgramWordCheck;
        }
        while( codeStream.Seek( BlockStorageStream::STREAM_SEEK_NEXT_BLOCK, BlockStorageStream::SeekCurrent ) );

    } while(codeStream.NextStream());

    //--//

    if( programCount == 0 ) return false;
    
    ApplicationStartAddress startAddress = (ApplicationStartAddress)programs[ 0 ];

    Launch( startAddress );

    return true;
}

void Loader_Engine::Launch( ApplicationStartAddress startAddress )
{
    // for non-XIP devices when this call returns the startAddress must contain 
    // the startAddress of the executable code non-compressed in  the corresponding 
    // XIP device or the header so that the compressed image 
    ApplicationStartAddress retAddress;

    TinyBooter_OnStateChange( State_Launch, (void*)startAddress, (void **)&retAddress );

    if (retAddress != NULL)
        startAddress = retAddress;

    LCD_Clear();

    DebuggerPort_Flush( m_port );
    
    // Some devices cannot reset the USB controller so we need to allow them to skip uninitialization
    // of the debug transport
    if(!g_fDoNotUninitializeDebuggerPort)
    {
        DebuggerPort_Uninitialize( m_port );
    }

    DISABLE_INTERRUPTS();

    LCD_Uninitialize();

    CPU_DisableCaches();

    if(Tinybooter_ImageIsCompressed())
    {
        CompressedImage_Header *hdr = (CompressedImage_Header*)startAddress;

        if((UINT32)(hdr->Destination) == Tinybooter_ProgramWordCheck())
        {
            Tinybooter_PrepareForDecompressedLaunch();

            // if startAddress at this point refers to a compressed image then this is where 
            // we retrieve the destination address where the imange will be decompressed to 
            startAddress = (ApplicationStartAddress)Tinybooter_CompressedImageStart( *hdr );

            LZ77_Decompress( (UINT8*)&hdr[ 1 ], hdr->Compressed, (UINT8*)hdr->Destination, hdr->Uncompressed );  
        }
    }

    (*startAddress)();
}

//--//

bool Loader_Engine::TransmitMessage( const WP_Message* msg, bool fQueue )
{

    UINT32 payloadSize;
    UINT32 flags;

#if !defined(NETMF_TARGET_BIG_ENDIAN)
    payloadSize = msg->m_header.m_size;
    flags       = msg->m_header.m_flags;
#else
    payloadSize = ::SwapEndian( msg->m_header.m_size  );
    flags       = ::SwapEndian( msg->m_header.m_flags );
#endif



    if(DebuggerPort_Write( m_port, (char*)&msg->m_header, sizeof(msg->m_header) ) != sizeof(msg->m_header)) return false;

    if(msg->m_header.m_size && msg->m_payload)
    {
        if(DebuggerPort_Write( m_port, (char*)msg->m_payload, payloadSize ) != payloadSize) return false;
    }
    DebuggerPort_Flush( m_port );

    return true;
}

//--//

void Loader_Engine::SendTextMessage( char* buffer, int length )
{
    m_controller.SendProtocolMessage( CLR_DBG_Commands::c_Monitor_Message, WP_Flags::c_NonCritical | WP_Flags::c_NoCaching, length, (UINT8*)buffer );
}

//--//

void Loader_Engine::ReplyToCommand( WP_Message* msg, bool fSuccess, bool fCritical, void* ptr, int size )
{
    WP_Message msgReply;
    UINT32     flags = 0;

    //
    // Make sure we reply only once!
    //
    if(msg->m_header.m_flags & WP_Flags::c_NonCritical) return;
    msg->m_header.m_flags |= WP_Flags::c_NonCritical;

    //
    // No caching in the request, no caching in the reply...
    //
    if(msg->m_header.m_flags & WP_Flags::c_NoCaching) flags |= WP_Flags::c_NoCaching;

    if(fSuccess  ) flags |= WP_Flags::c_ACK;
    else           flags |= WP_Flags::c_NACK;    
    if(!fCritical) flags |= WP_Flags::c_NonCritical;

    if(fSuccess == false)
    {
        ptr  = NULL;
        size = 0;
    }

    msgReply.Initialize( &m_controller );

#if defined(NETMF_TARGET_BIG_ENDIAN)
    SwapEndian( msg, ptr, size, true );
#endif
    msgReply.PrepareReply( msg->m_header, flags, size, (UINT8*)ptr );

    m_controller.SendProtocolMessage( msgReply );
}

void Loader_Engine::ReplyToCommand( WP_Message* msg, bool fSuccess, bool fCritical )
{
    ReplyToCommand( msg, fSuccess, fCritical, NULL, 0 );
}

////////////////////////////////////////////////////////////////////////////////////////////////////

bool Loader_Engine::Monitor_Ping( WP_Message* msg )
{
    //
    // There's someone on the other side!!
    //
    if((msg->m_header.m_flags & WP_Flags::c_Reply      ) == 0)
    {
        CLR_DBG_Commands::Monitor_Ping::Reply cmdReply;
        cmdReply.m_source = CLR_DBG_Commands::Monitor_Ping::c_Ping_Source_TinyBooter;

#if defined(NETMF_TARGET_BIG_ENDIAN)
        cmdReply.m_dbg_flags  = CLR_DBG_Commands::Monitor_Ping::c_Ping_DbgFlag_BigEndian;
#endif


        ReplyToCommand( msg, true, false, &cmdReply, sizeof(cmdReply) );
    }

    return true;
}

void MfReleaseInfo::Init(MfReleaseInfo& mfReleaseInfo, UINT16 major, UINT16 minor, UINT16 build, UINT16 revision, const char *info, size_t infoLen)
{
    MFVersion::Init( mfReleaseInfo.version, major, minor, build, revision );
    mfReleaseInfo.infoString[ 0 ] = 0;
    if ( NULL != info && infoLen > 0 )
    {
        const size_t len = MIN(infoLen, sizeof(mfReleaseInfo.infoString)-1);
        hal_strncpy_s( (char*)&(mfReleaseInfo.infoString[ 0 ]), sizeof(mfReleaseInfo.infoString), info, len );
    }
}

bool Loader_Engine::Monitor_OemInfo( WP_Message* msg )
{
    if((msg->m_header.m_flags & WP_Flags::c_Reply      ) == 0)
    {
        CLR_DBG_Commands::Monitor_OemInfo::Reply cmdReply;     
        bool fOK = TinyBooter_GetReleaseInfo(cmdReply.m_releaseInfo) == TRUE;
        ReplyToCommand( msg, fOK, false, &cmdReply, sizeof(cmdReply) );
    }

    return true;
}

bool Loader_Engine::Monitor_Reboot( WP_Message* msg )
{
    CLR_DBG_Commands::Monitor_Reboot* cmd = (CLR_DBG_Commands::Monitor_Reboot*)msg->m_payload;

    ReplyToCommand( msg, true, false );

    if(cmd != NULL)
    { 
        // only reset if we are not trying to get into the bootloader
        if( CLR_DBG_Commands::Monitor_Reboot::c_EnterBootloader != (cmd->m_flags & CLR_DBG_Commands::Monitor_Reboot::c_EnterBootloader))
        {
            Events_WaitForEvents( 0, 100 );

            CPU_Reset();
        }
    }

    return true;
}

//--//

bool Loader_Engine::Monitor_ReadMemory( WP_Message* msg )
{
    CLR_DBG_Commands::Monitor_ReadMemory* cmd = (CLR_DBG_Commands::Monitor_ReadMemory*)msg->m_payload;
    UINT8                                 buf[ 1024 ];
    UINT32                                len = cmd->m_length; if(len > sizeof(buf)) len = sizeof(buf);

    if(m_signedDataState.CheckDirty())
    {
        m_signedDataState.EraseMemoryAndReset();
        
        return false;
    }

    // if not found, then assume it is in the RAM area
    AccessMemory( cmd->m_address, len, buf, AccessMemory_Read );

    ReplyToCommand( msg, true, false, buf, len );

    return true;
}

bool Loader_Engine::Monitor_WriteMemory( WP_Message* msg )
{
    bool fRet;
    
    CLR_DBG_Commands::Monitor_WriteMemory* cmd = (CLR_DBG_Commands::Monitor_WriteMemory*)msg->m_payload;

    if(!m_signedDataState.VerifyContiguousData( cmd->m_address, cmd->m_length ))
    {
        m_signedDataState.EraseMemoryAndReset();
        
        return false;
    }

    TinyBooter_OnStateChange( State_MemoryWrite, (void*)cmd->m_address );

    // assume at RAM, directly use the original address 
    fRet = AccessMemory( cmd->m_address, cmd->m_length, cmd->m_data, AccessMemory_Write );
  
    ReplyToCommand( msg, fRet, false );

    return fRet;
}

bool Loader_Engine::Monitor_CheckMemory( WP_Message* msg )
{
    CLR_DBG_Commands::Monitor_CheckMemory*       cmd      = (CLR_DBG_Commands::Monitor_CheckMemory*)msg->m_payload;
    CLR_DBG_Commands::Monitor_CheckMemory::Reply cmdReply;

    if(m_signedDataState.CheckDirty())
    {
        m_signedDataState.EraseMemoryAndReset();
        
        return false;
    }

    AccessMemory( cmd->m_address, cmd->m_length, (UINT8*)&cmdReply.m_crc, AccessMemory_Check );

    ReplyToCommand( msg, true, false, &cmdReply, sizeof(cmdReply) );

    return true;
}

bool Loader_Engine::Monitor_EraseMemory( WP_Message* msg )
{
    bool                fRet = false;
    
    CLR_DBG_Commands::Monitor_EraseMemory* cmd = (CLR_DBG_Commands::Monitor_EraseMemory*)msg->m_payload;

    if(m_signedDataState.CheckDirty())
    {
        m_signedDataState.EraseMemoryAndReset();
        
        return false;
    }
    
    TinyBooter_OnStateChange( State_MemoryErase, (void*)cmd->m_address );
    
    fRet = AccessMemory( cmd->m_address, cmd->m_length, NULL, AccessMemory_Erase );

    ReplyToCommand( msg, fRet, false );
        
    return fRet;
}

bool Loader_Engine::Monitor_Execute( WP_Message* msg )
{
    bool fEnumAndLaunch                      = false;
    bool fAddressOK                          = false;
    const UINT32 c_EnumerateAndLaunchAddress = 0x0;
    CLR_DBG_Commands::Monitor_Execute* cmd   = (CLR_DBG_Commands::Monitor_Execute*)msg->m_payload;

#if defined(COMPILE_THUMB2)
    UINT32 signatureAddress = cmd->m_address & 0xFFFFFFFC; // align & clear Thumb bit
#else
    UINT32 signatureAddress = cmd->m_address;
#endif

    BlockStorageDevice *device;
    ByteAddress      sectAddress;

    if(m_signedDataState.CheckDirty())
    {
        m_signedDataState.EraseMemoryAndReset();
        
        return false;
    }

    // we use the address 0x0 to indicate to tinybooter to launch the first
    // application in flash (if it exists).  This is used to exit tinybooter mode
    // and to erase the boot entry flag. 
    fEnumAndLaunch = (c_EnumerateAndLaunchAddress == cmd->m_address);

    // check if it is a valid program 
    if (BlockStorageList::FindDeviceForPhysicalAddress( &device, cmd->m_address, sectAddress)) 
    {
        UINT32 data;
        device->Read(signatureAddress, 4, (BYTE*)&data);
        fAddressOK = (data == Tinybooter_ProgramWordCheck());        
    }
    else //ram
    {
        fAddressOK = (*(UINT32*)signatureAddress == Tinybooter_ProgramWordCheck());
    }
    
    if(fAddressOK || fEnumAndLaunch)
    {             
        ReplyToCommand( msg, true, false );

        Events_WaitForEvents( 0, 200 ); // give response some time before jumping into code

        g_PrimaryConfigManager.CleanBootLoaderFlag();

        // If we have a valid program at the given address then launch it
        // otherwise enumerate and launch
        if(fAddressOK)
        {
            Launch( ((void (*)())(size_t)cmd->m_address ));
        }
        else
        {
            EnumerateAndLaunch();
        }
    }
    else
    {
        ReplyToCommand( msg, false, false );        
    }

    return true;
}

bool Loader_Engine::Monitor_MemoryMap( WP_Message* msg )
{
    CLR_DBG_Commands::Monitor_MemoryMap::Range map[ 2 ];

    if(m_signedDataState.CheckDirty())
    {
        m_signedDataState.EraseMemoryAndReset();
        
        return false;
    }

    map[ 0 ].m_address = HalSystemConfig.RAM1.Base;
    map[ 0 ].m_length  = HalSystemConfig.RAM1.Size;
    map[ 0 ].m_flags   = CLR_DBG_Commands::Monitor_MemoryMap::c_RAM;

    map[ 1 ].m_address = HalSystemConfig.FLASH.Base;
    map[ 1 ].m_length  = HalSystemConfig.FLASH.Size;
    map[ 1 ].m_flags   = CLR_DBG_Commands::Monitor_MemoryMap::c_FLASH;

    ReplyToCommand( msg, true, false, map, sizeof(map) );

    return true;
}

bool Loader_Engine::Monitor_CheckSignature( WP_Message* msg )
{
    bool fSuccess = false;
    
    CLR_DBG_Commands::Monitor_Signature* cmd = (CLR_DBG_Commands::Monitor_Signature*)msg->m_payload;

    TinyBooter_OnStateChange( State_CryptoStart, NULL );

    fSuccess = m_signedDataState.VerifySignature( cmd->m_signature, cmd->m_length, cmd->m_keyIndex );

    TinyBooter_OnStateChange( State_CryptoResult, (void*)fSuccess );
    
    ReplyToCommand( msg, fSuccess, false );

    return true;
}

#ifdef DEBUG
// dumps binary block in a form useable as C code constants for isolated testing and verification
void DumpBlockDeclaration( char const* name, UINT8 const* pBlock, size_t len )
{
    debug_printf( "const char %s[] = {", name );
    for( int i = 0; i < len; ++i )
        debug_printf( "%c%d", i == 0 ? ' ' : ',', pBlock[ i ] );
    debug_printf( "};\n" );
}
#endif

bool Loader_Engine::Monitor_SignatureKeyUpdate( WP_Message* msg )
{
    bool fSuccess = false;

    if(g_PrimaryConfigManager.m_device!= NULL)
    {
        CLR_DBG_Commands::Monitor_SignatureKeyUpdate* cmd = (CLR_DBG_Commands::Monitor_SignatureKeyUpdate*)msg->m_payload;

        if(cmd != NULL && (cmd->m_keyIndex < ConfigurationSector::c_DeployKeyCount))
        {
            // If the signature key has not been set we don't need to run the signature check to update the key
            // This variale will be used to compare the key location to see if it is set or not
            if (g_PrimaryConfigManager.CheckSignatureKeyEmpty( cmd->m_keyIndex ))
            {
                g_PrimaryConfigManager.UpdateSignatureKey( cmd->m_keyIndex,(BYTE*)(RSAKey*)&cmd->m_newKey[ 0 ] );
                if(!(g_PrimaryConfigManager.VerifiySignatureKey( cmd->m_keyIndex, (BYTE*)&cmd->m_newKey[ 0 ] )))
                    ASSERT(0);
                fSuccess = true;
             
             }
            // Otherwise, we need to update the key.  We will have to perform a signature check on the new key (produced with the old key) to verify
            // the caller has authority to change the key
            else if(cmd->m_newKeySignature != NULL)
            {
                // perform signature check with given new key signature
                CryptoState st( (UINT32)&cmd->m_newKey[ 0 ], sizeof(RSAKey), (PBYTE)&cmd->m_newKeySignature[ 0 ], sizeof(cmd->m_newKeySignature), 0 );
                
                if(st.VerifySignature( cmd->m_keyIndex ))
                {
                    g_PrimaryConfigManager.UpdateSignatureKey( cmd->m_keyIndex,(BYTE*)(RSAKey*)&cmd->m_newKey[ 0 ] );
                    if(!(g_PrimaryConfigManager.VerifiySignatureKey( cmd->m_keyIndex, (BYTE*)&cmd->m_newKey[ 0 ] )))
                        ASSERT(0);
                    fSuccess = true;
                }
                else
                {
#ifdef DEBUG
                    debug_printf( "Failed cert check for new key:\n");
                    DumpBlockDeclaration( "newKey", cmd->m_newKey, sizeof(RSAKey) );
                    DumpBlockDeclaration( "newKeySig", cmd->m_newKeySignature, sizeof( cmd->m_newKeySignature ) );
                    DumpBlockDeclaration( "currentKey", g_PrimaryConfigManager.GetDeploymentKeys( cmd->m_keyIndex ), sizeof(RSAKey) );
#endif                    
                    fSuccess = false;
                }
            }
        }
    }
    
    ReplyToCommand( msg, fSuccess, false );

    return true;
}

bool Loader_Engine::Monitor_FlashSectorMap( WP_Message* msg )
{
    struct Flash_Sector 
    {
        UINT32 Start;
        UINT32 Length;
        UINT32 Usage;
          
    } *pData = NULL;

    UINT32 rangeCount = 0;
    UINT32 rangeIndex = 0;


    for(int cnt = 0; cnt < 2; cnt++)
    {
        BlockStorageDevice* device = BlockStorageList::GetFirstDevice();

        if(device == NULL)
        {
            ReplyToCommand(msg, true, false, NULL, 0);
            return false;
        }

        if(cnt == 1)
        {
            pData = (struct Flash_Sector*)private_malloc(rangeCount * sizeof(struct Flash_Sector));

            if(pData == NULL)
            {
                ReplyToCommand(msg, true, false, NULL, 0);
                return false;
            }
        }
        
        do
        {
            const BlockDeviceInfo* deviceInfo = device->GetDeviceInfo();

            for(int i = 0; i < deviceInfo->NumRegions;  i++)
            {
                const BlockRegionInfo* pRegion = &deviceInfo->Regions[ i ];
                
                for(int j = 0; j < pRegion->NumBlockRanges; j++)
                {
                    
                    if(cnt == 0)
                    {
                        rangeCount++;    
                    }
                    else
                    {
                        pData[ rangeIndex ].Start  = pRegion->BlockAddress( pRegion->BlockRanges[ j ].StartBlock );
                        pData[ rangeIndex ].Length = pRegion->BlockRanges[ j ].GetBlockCount() * pRegion->BytesPerBlock;
                        pData[ rangeIndex ].Usage  = pRegion->BlockRanges[ j ].RangeType & BlockRange::USAGE_MASK;
                        rangeIndex++;
                    }
                }
            }
        }
        while(device = BlockStorageList::GetNextDevice( *device ));
    }


    ReplyToCommand(msg, true, false, (void*)pData, rangeCount * sizeof (struct Flash_Sector) );

    private_free(pData);

    return true;
}

#if defined(NETMF_TARGET_BIG_ENDIAN)

UINT32 Loader_Engine::SwapEndianPattern( UINT8* &buffer, UINT32 size, UINT32 count )
{
    UINT32 consumed=0;
    
    if (1==size)
    {
        // Do no swapping, just increment pointer
        buffer += (size*count);
        consumed = size*count;
    }
    else
    {
        while (count--)
        {
            switch ( size )
            {
                case 1      :
                    
                    break;
                case 2      :
                    {
                        UINT16 *p16 = (UINT16 *)buffer;
                        *p16 = ::SwapEndian( *p16 );
                    }
                    break;
                case 4      :
                    {
                        UINT32 *p32 = (UINT32 *)buffer;
                        *p32 = ::SwapEndian( *p32 );
                    }
                    break;
                case 8      :
                    {
                        UINT64 *p64 = (UINT64 *)buffer;
                        *p64 = ::SwapEndian( *p64 );
                    }
                    break;
            }
            buffer += size;
            consumed += size;
        }
    }
    return consumed;
}
void Loader_Engine::SwapDebuggingValue( UINT8* &payload, UINT32 payloadSize )
{
    UINT32 count = payloadSize / sizeof(CLR_DBG_Commands::Debugging_Value);
    while (count--)
    {
        SwapEndianPattern( payload, sizeof(UINT32), 4);
        SwapEndianPattern( payload, 1, 128 );
        SwapEndianPattern( payload, sizeof(UINT32), 2);
        SwapEndianPattern( payload, sizeof(UINT32), 1);
        SwapEndianPattern( payload, sizeof(UINT32), 2);
        SwapEndianPattern( payload, sizeof(UINT32), 1);
        SwapEndianPattern( payload, sizeof(UINT32), 2);
    }
}

void Loader_Engine::SwapEndian( WP_Message* msg, void* ptr, int size, bool fReply )
{

    UINT8 *payload      = (UINT8*)ptr;
    UINT32 payloadSize  = size ;

    ASSERT(sizeof(int)==sizeof(UINT32));

    // Some commands may have a zero payload if not supproted, protect here.
    if (NULL==ptr) return;

    switch ( msg->m_header.m_cmd )
    {
    case CLR_DBG_Commands::c_Monitor_Ping               :
        SwapEndianPattern( payload, sizeof(UINT32), 2  );
        break;

    case CLR_DBG_Commands::c_Monitor_Message            :
        // string (no NULL termination) - nothing to do
        break;
    case CLR_DBG_Commands::c_Monitor_ReadMemory         :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:2 );
        break;
    case CLR_DBG_Commands::c_Monitor_WriteMemory        :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:2 );
        break;
    case CLR_DBG_Commands::c_Monitor_CheckMemory        :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?1:2  );
        break;
    case CLR_DBG_Commands::c_Monitor_EraseMemory        :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:2  );
        break;
    case CLR_DBG_Commands::c_Monitor_Execute            :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:1  );
        break;
    case CLR_DBG_Commands::c_Monitor_Reboot             :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:1 );
        break;
    case CLR_DBG_Commands::c_Monitor_MemoryMap          :
        // do for each range entry
        SwapEndianPattern( payload, sizeof(UINT32), payloadSize/sizeof(UINT32) );
        break;

    case CLR_DBG_Commands::c_Monitor_CheckSignature     :
        // Monitor_Signature struct
        SwapEndianPattern( payload, sizeof(UINT32), fReply?0:2 );
        break;
    case CLR_DBG_Commands::c_Monitor_FlashSectorMap     :
        SwapEndianPattern( payload, sizeof(UINT32), fReply?payloadSize/sizeof(UINT32):0 );
        break;
    case CLR_DBG_Commands::c_Monitor_SignatureKeyUpdate :
        if (!fReply)
        {
            SwapEndianPattern( payload, sizeof(UINT32), 1 );
            SwapEndianPattern( payload, 1, 128 );
            SwapEndianPattern( payload, 1, 260 );
            SwapEndianPattern( payload, sizeof(UINT32), 1 );
        }
        break;
    case CLR_DBG_Commands::c_Monitor_OemInfo            :
        // swap the version, leave the rest
        SwapEndianPattern( payload, sizeof(UINT16), fReply?4:0 );
        break;

    }
}
#endif


