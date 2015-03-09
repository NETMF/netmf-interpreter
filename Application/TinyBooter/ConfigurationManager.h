#ifndef __CONFIGURATIONMANAGER_H__
#define __CONFIGURATIONMANAGER_H__



struct ConfigurationSectorManager
{
    static const UINT32 c_ConfigurationSectorSize = sizeof(ConfigurationSector);

    //--//
    
    ConfigurationSector    m_localCfg;             // for non XIP device or as a buffer when rewrite NOR
    ConfigurationSector*   m_configurationSector;  // should point to the g_ConfiguraitonBlock
    BlockStorageDevice*    m_device;               // The block storage device where the configruation sector is 

    //The Configurationsector is consumed one block of a block device
    UINT32                 m_cfgPhysicalAddress;
    const BlockRegionInfo* m_region;    
    BOOL                   m_fSupportsXIP;
    BOOL                   m_fUsingRAM;
    UINT32                 m_booterAddressOffset;

    //--//

    void   LocateConfigurationSector( UINT32 BlockRange );
    void   LoadConfiguration        ( );
    void   WriteConfiguration       ( UINT32 WriteOffset, BYTE *pData, UINT32 size, BOOL CheckWrite );
    void   EraseWriteConfigBlock    ( BYTE * pData, UINT32 sizeInBytes );
    BOOL   IsBootLoaderRequired     ( INT32 &bootModeTimeout );
    void   CleanBootLoaderFlag      ( );
    BOOL   CheckSignatureKeyEmpty   ( UINT32 Index );
    void   UpdateSignatureKey       ( UINT32 Index, BYTE* pData );
    BOOL   VerifiySignatureKey      ( UINT32 Index, BYTE*pData );
    UINT8* GetDeploymentKeys        ( UINT32 Index );
    UINT8  GetTinyBooterVersion     ( );
    UINT16 GetVersion               ( );
};

//--//

extern ConfigurationSectorManager g_PrimaryConfigManager;

//--//

#endif

