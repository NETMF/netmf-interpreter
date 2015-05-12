////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//

#undef  TRACE_ALWAYS
#undef  TRACE_CONFIG

#define TRACE_ALWAYS               0x00000001
#define TRACE_CONFIG               0x00000002

#undef  DEBUG_TRACE
#define DEBUG_TRACE (TRACE_ALWAYS)

#ifndef HAL_REDUCESIZE
const size_t ConfigLengthCookie = offsetof( ConfigurationSector, FirstConfigBlock );
#endif

BOOL HAL_CONFIG_BLOCK::IsGoodBlock() const
{
    if(Signature != c_Version_V2)
    {
        return FALSE;
    }

    DEBUG_TRACE2( TRACE_CONFIG, "read header CRC=0x%08x at %08x\r\n", HeaderCRC, (size_t)this );

    // what is the header's CRC
    UINT32 CRC = SUPPORT_ComputeCRC( ((UINT8*)&DataCRC), sizeof(*this) - offsetof(HAL_CONFIG_BLOCK,DataCRC), c_Seed );

    DEBUG_TRACE1(TRACE_CONFIG, "calc header CRC=0x%08x\r\n", CRC);

    if(CRC != HeaderCRC)
    {
        DEBUG_TRACE3( TRACE_ALWAYS, "FAILED HEADER CRC at %08x: 0x%08x != 0x%08x\r\n", (size_t)this, CRC, HeaderCRC );
        return FALSE;
    }

    return TRUE;
}

BOOL HAL_CONFIG_BLOCK::IsGoodData() const
{
    DEBUG_TRACE1( TRACE_CONFIG, "read Size=%5d\r\n", Size );

    // what is the blob's CRC
    UINT32 CRC = SUPPORT_ComputeCRC( Data(), Size, 0 );

    DEBUG_TRACE1( TRACE_CONFIG, "calc blob CRC=0x%08x\r\n", CRC );

    // this indicates that this record has been marked as invalid, but still allows the helper to move
    // to the next record.
    if(CRC != DataCRC)
    {
        return FALSE;
    }

    return TRUE;
}

BOOL HAL_CONFIG_BLOCK::IsGood() const
{
    return IsGoodBlock();
}

const HAL_CONFIG_BLOCK* HAL_CONFIG_BLOCK::Next() const
{
    if(IsGood())
    {
        return (const HAL_CONFIG_BLOCK*)((size_t)Data() + RoundLength( Size ));
    }

    return NULL;
}

const void* HAL_CONFIG_BLOCK::Data() const
{
    return (const void*)&this[1];
}

//--//

BOOL HAL_CONFIG_BLOCK::Prepare( const char* Name, void* Data, UINT32 Size )
{
    if(Name == NULL || hal_strlen_s( Name ) >= sizeof(DriverName)) return FALSE;

    memset( this, 0, sizeof(*this) );

    this->Signature = c_Version_V2;
    this->DataCRC   = SUPPORT_ComputeCRC( Data, Size, 0 );
    this->Size      = Size;

    hal_strcpy_s( this->DriverName, ARRAYSIZE(this->DriverName), Name );

    this->HeaderCRC = SUPPORT_ComputeCRC( ((UINT8*)&DataCRC), sizeof(*this) - offsetof(HAL_CONFIG_BLOCK,DataCRC), c_Seed );

    return TRUE;
}

//--//

const HAL_CONFIG_BLOCK* HAL_CONFIG_BLOCK::Find( const char* Name, BOOL fSkipCurrent, BOOL fAppend ) const
{
    const HAL_CONFIG_BLOCK* ptr;

    if(fSkipCurrent)
    {
        ptr = Next();
    }
    else
    {
        ptr = this;
    }

    while(ptr->IsGood())
    {
        if(ptr->IsGoodData() && Name && strcmp( Name, ptr->DriverName ) == 0)
        {
            return ptr;
        }

        ptr = ptr->Next();
    }

    return fAppend ? ptr : NULL;
}

//--//

BOOL HAL_CONFIG_BLOCK::GetConfigSectorAddress(HAL_CONFIG_BLOCK_STORAGE_DATA& blData)
{

#if defined(HAL_REDUCESIZE)
    // No config update.
    return FALSE;
#else

    BOOL fRet = FALSE;

    BlockStorageStream stream;

    if(stream.Initialize( BlockUsage::CONFIG ))
    {
        const BlockDeviceInfo* DeviceInfo = stream.Device->GetDeviceInfo();

        blData.Device = stream.Device;

        if(DeviceInfo)
        {
            UINT32 regionIndex, rangeIndex;

            DeviceInfo->FindRegionFromAddress( stream.BaseAddress, regionIndex, rangeIndex );

            const BlockRegionInfo* pRegion = &DeviceInfo->Regions[regionIndex];

            blData.ConfigAddress = pRegion->BlockAddress( pRegion->BlockRanges[rangeIndex].StartBlock );
            blData.BlockLength = stream.Length; //pRegion->BytesPerBlock;

            if (0 == DeviceInfo->Attribute.SupportsXIP)
            {
                // For windows builds this will cause a memory access violation because g_ConfigurationSector
                // is a const.  We also don't need to do this for the emulator because we only use the dynamic
                // section of the config sector as non-XIP (in other words, g_ConfigurationSector is accessible).
#ifndef _WIN32
                BYTE* pConfig = (BYTE*)&g_ConfigurationSector;

                // get the latest right data from nonXIP block
                stream.Read( &pConfig, sizeof(ConfigurationSector) );
#endif

                blData.isXIP = FALSE ;
            }
            else
            {
                blData.isXIP = TRUE;
            }

            fRet = TRUE;
        }
    }

    return fRet;
#endif
}

BOOL HAL_CONFIG_BLOCK::CompactBlock(HAL_CONFIG_BLOCK_STORAGE_DATA& blData, const ConfigurationSector* cfgStatic, const HAL_CONFIG_BLOCK* cfgEnd)
{
    BOOL fRet = FALSE;
    
    int saveLength = (UINT32)cfgEnd - blData.ConfigAddress;
    
    //
    UINT8 *pBackup = (UINT8*)private_malloc( saveLength );

    if(pBackup)
    {
        int writeLength = cfgStatic->ConfigurationLength;
        UINT8* pNextCfg = pBackup;

        const HAL_CONFIG_BLOCK* cfgStart = (const HAL_CONFIG_BLOCK*)((UINT32)cfgStatic + cfgStatic->ConfigurationLength);
        memcpy(pNextCfg, cfgStatic, cfgStatic->ConfigurationLength);
        pNextCfg += cfgStatic->ConfigurationLength;

        while(cfgStart != NULL && cfgStart < cfgEnd)
        {
            if(cfgStart->Size > 0)
            {
                HAL_DRIVER_CONFIG_HEADER* pCfgHeader = (HAL_DRIVER_CONFIG_HEADER*)&cfgStart[1];

                if(pCfgHeader->Enable)
                {
                    int len = sizeof(HAL_CONFIG_BLOCK) + HAL_CONFIG_BLOCK::RoundLength(cfgStart->Size);

                    memcpy( pNextCfg, cfgStart, len );

                    pNextCfg += len;
                    writeLength += len;
                }
            }
            cfgStart = cfgStart->Next();
        }

        blData.Device->EraseBlock(blData.ConfigAddress);

        fRet = blData.Device->Write((UINT32)blData.ConfigAddress, writeLength, pBackup, FALSE);

        // if the heap is not yet initialized this does nothing
        private_free( pBackup );
    }

    return fRet;
}

BOOL HAL_CONFIG_BLOCK::UpdateBlock( const HAL_CONFIG_BLOCK_STORAGE_DATA &blData, const void* pAddress, const HAL_CONFIG_BLOCK *Header, void* Data, size_t Length, const void* LastConfigAddress, BOOL isChipRO )
{
    BOOL fRet = TRUE;
    ByteAddress newCfgAddr = (ByteAddress)LastConfigAddress;
    
    if(pAddress != LastConfigAddress)
    {
        HAL_CONFIG_BLOCK *pCfg = (HAL_CONFIG_BLOCK*)pAddress;

        HAL_DRIVER_CONFIG_HEADER* pCfgHeader = (HAL_DRIVER_CONFIG_HEADER*)&pCfg[1];

        BOOL fDirty = FALSE;
        
        fRet &= blData.Device->Write((ByteAddress)&pCfgHeader->Enable, sizeof(BOOL), (BYTE*)&fDirty, FALSE);
    }    

    if(Header != NULL && Data != NULL)
    {
        if((newCfgAddr + Length + sizeof(HAL_CONFIG_BLOCK)) > (blData.ConfigAddress + blData.BlockLength))
        {
            return FALSE;
        }

        fRet &= blData.Device->Write(newCfgAddr, sizeof(HAL_CONFIG_BLOCK), (BYTE*)Header, FALSE);
        
        fRet &= blData.Device->Write(newCfgAddr + sizeof(HAL_CONFIG_BLOCK), Length, (BYTE*)Data, FALSE);
    }

    return fRet;
}

BOOL HAL_CONFIG_BLOCK::InvalidateBlockWithName( const char* Name, BOOL isChipRO )
{
    return UpdateBlockWithName(Name, NULL, 0, isChipRO);
}

BOOL HAL_CONFIG_BLOCK::UpdateBlockWithName( const char* Name, void* Data, size_t Length, BOOL isChipRO )
{
#if defined(HAL_REDUCESIZE)
    return FALSE;
#else

    if(Name == NULL)
        return FALSE;

    BOOL fRet = FALSE;
    BYTE* pXipConfigBuf = NULL;
    const HAL_CONFIG_BLOCK *pLastConfig = NULL;
    HAL_CONFIG_BLOCK_STORAGE_DATA blData;

    if(!GetConfigSectorAddress(blData))
        return FALSE;

    if(g_ConfigurationSector.ConfigurationLength != ConfigLengthCookie )
    {
        return FALSE;
    }

    const ConfigurationSector* pStaticCfg = NULL;

    const HAL_CONFIG_BLOCK *pConfig = NULL;

    GLOBAL_LOCK(irq);

    if(blData.isXIP)
    {
        pConfig = (const HAL_CONFIG_BLOCK*)(blData.ConfigAddress+ g_ConfigurationSector.ConfigurationLength);
        pStaticCfg = &g_ConfigurationSector;
    }
    else
    {
        pXipConfigBuf = (BYTE*)private_malloc(blData.BlockLength);

        if(pXipConfigBuf != NULL)
        {
            blData.Device->Read( blData.ConfigAddress, blData.BlockLength, pXipConfigBuf );

            pConfig = (const HAL_CONFIG_BLOCK*)&pXipConfigBuf[g_ConfigurationSector.ConfigurationLength];

            pStaticCfg = (const ConfigurationSector*)pXipConfigBuf;
        }
        else
        {
            return FALSE;
        }
    }

    if(pConfig != NULL)
    {
        pConfig = pConfig->Find(Name, FALSE, TRUE);

        pLastConfig = pConfig->Find("", FALSE, TRUE);
    }

    if(pConfig != NULL)
    {
        const void* physAddr;
        const void* physEnd;

        // try compacting the config block if the first attempt to update fails
        for(int i=0; i<2; i++)
        {
            // if we could not get the last config then we must be full
            if(pLastConfig != NULL)
            {
                if(blData.isXIP)
                {
                    physAddr = (const void*)pConfig;
                    physEnd  = (const void*)pLastConfig;
                }
                else
                {
                    physAddr = (const void*)(blData.ConfigAddress + ((size_t)pConfig - (size_t)pXipConfigBuf));
                    physEnd  = (const void*)(blData.ConfigAddress + ((size_t)pLastConfig - (size_t)pXipConfigBuf));

                    private_free(pXipConfigBuf);
                    pXipConfigBuf = NULL;
                }

                if(Data != NULL)
                {
                    HAL_CONFIG_BLOCK header;

                    header.Prepare( Name, Data, Length );

                    fRet = UpdateBlock( blData, physAddr, (const HAL_CONFIG_BLOCK*)&header, Data, Length, physEnd, isChipRO );
                }
                else if(physAddr != physEnd)
                {
                    fRet = UpdateBlock( blData, physAddr, NULL, NULL, 0, physEnd, isChipRO );
                }
                else
                {
                    fRet = TRUE;
                }

                if(fRet) break;
            }
        
            CompactBlock( blData, pStaticCfg, pLastConfig );

            if(!blData.isXIP && pXipConfigBuf != NULL)
            {
                blData.Device->Read( blData.ConfigAddress, blData.BlockLength, pXipConfigBuf );
            
                pConfig = (const HAL_CONFIG_BLOCK*)&pXipConfigBuf[g_ConfigurationSector.ConfigurationLength];
            }
            else
            {
                pConfig = (const HAL_CONFIG_BLOCK*)(blData.ConfigAddress + g_ConfigurationSector.ConfigurationLength);
            }

            pConfig = pConfig->Find(Name, FALSE, TRUE);

            pLastConfig = pConfig->Find("", FALSE, TRUE);
        }
    }


    if(pXipConfigBuf != NULL)
    {
        private_free(pXipConfigBuf);
    }

    return fRet;
#endif
}

BOOL HAL_CONFIG_BLOCK::ApplyConfig( const char* Name, void* Address, size_t Length )
{
    return ApplyConfig( Name, Address, Length, NULL );
}

BOOL HAL_CONFIG_BLOCK::ApplyConfig( const char* Name, void* Address, size_t Length, void** newAlloc )
{
#if defined(HAL_REDUCESIZE)
    // No config update.
    return FALSE;
#else
    BYTE* pXipConfigBuf = NULL;
    HAL_CONFIG_BLOCK_STORAGE_DATA blData;
    const HAL_CONFIG_BLOCK *header = NULL;

    if(!GetConfigSectorAddress(blData)) return FALSE;

    if(g_ConfigurationSector.ConfigurationLength != ConfigLengthCookie )
        return FALSE;

    if(blData.isXIP)
    {
        header = (const HAL_CONFIG_BLOCK*)(blData.ConfigAddress + g_ConfigurationSector.ConfigurationLength);
    }
    else
    {
        pXipConfigBuf = (BYTE*)private_malloc(blData.BlockLength);

        if(pXipConfigBuf != NULL)

        {
            blData.Device->Read( blData.ConfigAddress, blData.BlockLength, pXipConfigBuf );

            header = (const HAL_CONFIG_BLOCK*)&pXipConfigBuf[g_ConfigurationSector.ConfigurationLength];
        }
    }

    if(header != NULL)
    {
        header = header->Find(Name, FALSE, FALSE);
    }

    if(header)
    {
        if(newAlloc != NULL)
        {
            *newAlloc = private_malloc(header->Size);

            if(*newAlloc)
            {
                memcpy( *newAlloc, header->Data(), header->Size );

                return TRUE;
            }
        }
        else if(header->Size == Length)
        {
            if(Address)
            {
                memcpy( Address, header->Data(), Length );
            }

            return TRUE;
        }
    }

    if(pXipConfigBuf != NULL)
    {
        private_free(pXipConfigBuf);
    }

    return FALSE;
#endif
}

unsigned int /* ie, BOOL */ GetHalSystemInfo(HalSystemInfo& systemInfo) // placate some compilers that handle BOOL unconventionally
{
#if defined(HAL_REDUCESIZE)
    // No config update.
    return FALSE;
#else

    // MfReleaseInfo:
    systemInfo.m_releaseInfo.version.usMajor       = VERSION_MAJOR;
    systemInfo.m_releaseInfo.version.usMinor       = VERSION_MINOR;
    systemInfo.m_releaseInfo.version.usBuild       = VERSION_BUILD;
    systemInfo.m_releaseInfo.version.usRevision    = VERSION_REVISION;
    const size_t len = sizeof(systemInfo.m_releaseInfo.infoString);

#if defined(PLATFORM_SH)
#undef OEMSTR(str) 
#define OEMSTR(str) # str

    hal_strncpy_s ((char*)&systemInfo.m_releaseInfo.infoString[0], len, OEMSTR(OEMSYSTEMINFOSTRING), len-1 );
#undef OEMSTR
#else
    hal_strncpy_s ((char*)&systemInfo.m_releaseInfo.infoString[0], len, OEMSYSTEMINFOSTRING, len-1 );
#endif

    // OEM_MODEL_SKU:
    memcpy((void*)&(systemInfo.m_OemModelInfo), (void*)&(g_ConfigurationSector.OEM_Model_SKU), sizeof(OEM_MODEL_SKU));

    // OEM_SERIAL_NUMBERS:
    memcpy((void*)&(systemInfo.m_OemSerialNumbers), (void*)&(g_ConfigurationSector.OemSerialNumbers), sizeof(OEM_SERIAL_NUMBERS));

    return TRUE;
#endif
}

