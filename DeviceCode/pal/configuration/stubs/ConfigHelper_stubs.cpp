////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//

BOOL HAL_CONFIG_BLOCK::IsGoodBlock() const
{
    return TRUE;
}

BOOL HAL_CONFIG_BLOCK::IsGoodData() const
{
    return TRUE;
}

BOOL HAL_CONFIG_BLOCK::IsGood() const
{
    return TRUE;
}

const HAL_CONFIG_BLOCK* HAL_CONFIG_BLOCK::Next() const
{
    return NULL;
}

const void* HAL_CONFIG_BLOCK::Data() const
{
    return NULL;
}

//--//

BOOL HAL_CONFIG_BLOCK::Prepare( const char* Name, void* Data, UINT32 Size )
{
    return TRUE;
}

//--//

const HAL_CONFIG_BLOCK* HAL_CONFIG_BLOCK::Find( const char* Name, BOOL fSkipCurrent, BOOL fAppend ) const
{
    return NULL;
}

//--//

BOOL HAL_CONFIG_BLOCK::GetConfigSectorAddress(HAL_CONFIG_BLOCK_STORAGE_DATA& blData)
{
    return FALSE;
}

BOOL HAL_CONFIG_BLOCK::CompactBlock(HAL_CONFIG_BLOCK_STORAGE_DATA& blData, const ConfigurationSector* cfgStatic, const HAL_CONFIG_BLOCK* cfgEnd)
{
    return FALSE;
}


BOOL HAL_CONFIG_BLOCK::UpdateBlock( const HAL_CONFIG_BLOCK_STORAGE_DATA &blData, const void* pAddress, const HAL_CONFIG_BLOCK *Header, void* Data, size_t Length, const void* LastConfigAddress, BOOL isChipRO )
{
    return FALSE;
}

BOOL HAL_CONFIG_BLOCK::UpdateBlockWithName( const char* Name, void* Data, size_t Length, BOOL isChipRO )
{
    return FALSE;
}

BOOL HAL_CONFIG_BLOCK::ApplyConfig( const char* Name, void* Address, size_t Length )
{
    return FALSE;
}
BOOL HAL_CONFIG_BLOCK::ApplyConfig( const char* Name, void* Address, size_t Length, void** newAlloc )
{
    return FALSE;
}

unsigned int /* ie, BOOL */ GetHalSystemInfo(HalSystemInfo& systemInfo)
{
    return FALSE;
}

