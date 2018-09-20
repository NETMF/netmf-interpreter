////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <Tinyhal.h>
#include "configurationManager.h"

////////////////////////////////////////////////////////////////////////////////////////////////////
// The ConfiguraionSectorManager is only used for the Booter section, as it doesn't get the g_ConfigurationSector, no Configuraitonsector is defined for it.


void ConfigurationSectorManager::LocateConfigurationSector( UINT32 BlockUsage )
{
    BlockStorageStream     stream;

    m_booterAddressOffset = 0xFFFFFFFF;

    if(stream.Initialize( BlockUsage ))
    {
        const BlockDeviceInfo *DeviceInfo;
        UINT32 rangeIndex, regionIndex;

        m_device             = stream.Device;
        m_cfgPhysicalAddress = stream.BaseAddress;
        DeviceInfo           = m_device->GetDeviceInfo();

        if(DeviceInfo->FindRegionFromAddress( m_cfgPhysicalAddress, regionIndex, rangeIndex ))
        {
            m_region = &DeviceInfo->Regions[ regionIndex ];
        }
        else
        {
            ASSERT(FALSE);
        }

        if (DeviceInfo->Attribute.SupportsXIP)
        {
            m_fSupportsXIP = TRUE;
            m_fUsingRAM = FALSE;
            // Get the real address 
            m_configurationSector = (ConfigurationSector *)CPU_GetUncachableAddress( m_cfgPhysicalAddress );
        }
        else
        {
            m_fSupportsXIP = FALSE;

            m_configurationSector = &m_localCfg;
            if (m_configurationSector !=NULL)
            {
                m_device->Read( m_cfgPhysicalAddress, c_ConfigurationSectorSize, (BYTE *)m_configurationSector );
            }
            else
            {
                ASSERT(0);
            }
         }
    }
    else
    {
        m_device = NULL;
        m_region = NULL;
        m_fSupportsXIP = TRUE;  // will use RAM 
        m_fUsingRAM = TRUE;
    }  
}



void ConfigurationSectorManager::LoadConfiguration()
{
    if (m_device ==NULL)
            return;
        
    if (m_fSupportsXIP)
    {
        // Get the real address 
        m_configurationSector = (ConfigurationSector *)CPU_GetUncachableAddress( m_cfgPhysicalAddress );
        return ;

    }
    else
    {
        m_configurationSector = &m_localCfg;
        m_device->Read( m_cfgPhysicalAddress, c_ConfigurationSectorSize, (BYTE *)m_configurationSector );
        return ;
     }
}



void ConfigurationSectorManager::WriteConfiguration( UINT32 writeOffset, BYTE *data, UINT32 size, BOOL checkWrite )
{
    BOOL eraseWrite = FALSE;
    UINT32 writeLengthInBytes ;

    if (m_device ==NULL)
            return ;

    LoadConfiguration();

    // Get the real address 
    BYTE* configurationInBytes = (BYTE*)m_configurationSector;

    if (!m_fSupportsXIP)
    {
        writeLengthInBytes   = c_ConfigurationSectorSize;
    }
    else
    {
        writeLengthInBytes   = size;
    }
    
    // Validity  write 
    if (checkWrite)
    {
        for (UINT32 i = 0; i<size; i++)
        {
            if ((~configurationInBytes[ i + writeOffset ]) & data[ i ])
            { 
                eraseWrite = TRUE;
                writeLengthInBytes   = m_region->BytesPerBlock;

                break;
            }
        }
            
        //else XIP device directly write.
    }

    // Copy the whole block to a buffer, for NonXIP or need to erase block
    if ((eraseWrite) || (!m_fSupportsXIP))
    {        
        configurationInBytes =(BYTE*)private_malloc(writeLengthInBytes);

        // load data to the local buffer.
        if (configurationInBytes)
        {
            m_device->Read( m_cfgPhysicalAddress, writeLengthInBytes, configurationInBytes );
            // copy the new data to the configdata.
            for (UINT32 i = 0; i<size; i++)
            {
                configurationInBytes[ i + writeOffset ] = data[ i ];

            }
        }
        else
            ASSERT(0); // must have enough ram space for it.

        if(eraseWrite)
        {
            m_device->EraseBlock( m_cfgPhysicalAddress );
        }

        // rewrite from the start of block
        m_device->Write( m_cfgPhysicalAddress, writeLengthInBytes, configurationInBytes, FALSE ); 

        private_free(configurationInBytes);


    }
    else // no need to erase and XIP device
    {
        UINT32 physicalAddr = (UINT32)m_configurationSector + writeOffset;
        m_device->Write( physicalAddr, writeLengthInBytes, data, FALSE );         
    }

    // No need to reload as the if XIP-, the pCFg is pointing to the righ address,it will updated when it read ad hoc..
    // for the non-XIP, the m_configurationSector points to the LocalConfiguration, which has the newly write data.
    //LoadConfiguration();
}


// Caller provide the new data and replace with the new data
void ConfigurationSectorManager::EraseWriteConfigBlock( BYTE * data, UINT32 sizeInBytes )
{                
    m_device->EraseBlock( m_cfgPhysicalAddress );
    m_device->Write( m_cfgPhysicalAddress, sizeInBytes, data, FALSE ); 
    // reload configuratoin
    LoadConfiguration();
}


BOOL ConfigurationSectorManager::IsBootLoaderRequired( INT32 &bootModeTimeout )
{
    const UINT32 c_Empty = 0xFFFFFFFF;

    if(m_device == NULL)
            return FALSE;

    volatile UINT32* data = (volatile UINT32*)&m_configurationSector->BooterFlagArray[ 0 ];

    for(int i=0; i<ConfigurationSector::c_MaxBootEntryFlags; i++ )
    {
        switch(data[ i ])
        {
            case ConfigurationSector::c_BootEntryKey:
                bootModeTimeout = 20000; // 20 seconds wait time
                m_booterAddressOffset = (UINT32)&data[ i ]- (UINT32)m_configurationSector;
                return TRUE;

            case c_Empty:  // anything else means we do not enter boot loader
                return FALSE;
        }
    }
    return FALSE; 
}

void ConfigurationSectorManager::CleanBootLoaderFlag()
{
    const UINT32 eraseKey =  0; 
    // as going to write zeros, no need to validate the writeable or not.
    if (m_booterAddressOffset != 0xFFFFFFFF)

    {
        WriteConfiguration( m_booterAddressOffset,(BYTE*)&eraseKey, sizeof(UINT32), FALSE );
    }
}
    


BOOL ConfigurationSectorManager::CheckSignatureKeyEmpty( UINT32 Index )
{
    BYTE *data = &m_configurationSector->DeploymentKeys[ Index ].SectorKey[ 0 ] ;
    for(int i=0; i<sizeof(TINYBOOTER_KEY_CONFIG); i++)
    {
        if( data[ i ] !=0xFF)
        {
            return FALSE;
        }
    }
    return TRUE;
}


void ConfigurationSectorManager::UpdateSignatureKey( UINT32 Index, BYTE* data )
{
    UINT32 Offset = (UINT32)&m_configurationSector->DeploymentKeys[ Index ].SectorKey[ 0 ] - (UINT32)m_configurationSector;
    WriteConfiguration( Offset, data, sizeof(TINYBOOTER_KEY_CONFIG), TRUE );
}

BOOL ConfigurationSectorManager::VerifiySignatureKey( UINT32 Index, BYTE*data )
{
    // reload the configuration, make sure it is right;
    LoadConfiguration();
    for(int i=0; i<sizeof(TINYBOOTER_KEY_CONFIG); i++)
    {
        if( data[ i ] !=m_configurationSector->DeploymentKeys[ Index ].SectorKey[ i ])
        {
            return FALSE;
        }
    }
    return TRUE;
}

UINT8* ConfigurationSectorManager::GetDeploymentKeys( UINT32 Index )
{
    if (m_device == NULL) return 0;

    LoadConfiguration();
    return &(m_configurationSector->DeploymentKeys[ Index ].SectorKey[ 0 ]);
}    

UINT8 ConfigurationSectorManager::GetTinyBooterVersion()
{
    if (m_device == NULL) return 0;

    return m_configurationSector->Version.TinyBooter;
}

UINT16 ConfigurationSectorManager::GetVersion()
{
    if (m_device == NULL) return 0;
    return m_configurationSector->Version.Major <<8 | m_configurationSector->Version.Minor ;
}

//--//

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_PrimaryConfigManager"
#endif

ConfigurationSectorManager g_PrimaryConfigManager;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

//--//

