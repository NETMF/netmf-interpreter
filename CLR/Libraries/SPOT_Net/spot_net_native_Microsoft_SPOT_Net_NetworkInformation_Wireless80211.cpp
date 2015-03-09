////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



#include "SPOT_Net.h"
/*
BOOL CheckSignatureKeyEmpty( UINT32 Index )
{
    BYTE *data = (BYTE *)(&g_ConfigurationSector.DeploymentKeys[ Index ].SectorKey[ 0 ]);
    for(int i=0; i<sizeof(TINYBOOTER_KEY_CONFIG); i++)
    {
        if( data[ i ] !=0xFF)
            return FALSE;
    }
    return TRUE;
}

UINT8* GetDeploymentKeys( UINT32 Index )
{    
    return (UINT8 *)&(g_ConfigurationSector.DeploymentKeys[ Index ].SectorKey[ 0 ]);
}  


RSAKey* RetrieveWirelessEncryptionKey()
{
    if(g_ConfigurationSector.Version.TinyBooter != ConfigurationSector::c_CurrentVersionTinyBooter) return NULL;

    if (CheckSignatureKeyEmpty( ConfigurationSector::c_DeployKeyDeployment ))
    {
        return NULL;
    }

    RSAKey* key = (RSAKey*)GetDeploymentKeys( ConfigurationSector::c_DeployKeyDeployment );

    return key;
}

/// Symmetric encryption, based on what is described in the spec.
BOOL Encrypt( BYTE *Key, BYTE* pPlainText, DWORD cbPlainText, BYTE *pCypherText, DWORD cbCypherText )
{
    CLR_UINT8* IVPtr;
    CLR_UINT32 IVLen;
    UINT8      tmp[ TEA_KEY_SIZE_BYTES ];

    IVPtr = tmp;
    IVLen = sizeof(tmp);
    memset( tmp, 0, sizeof(tmp) );

    return Crypto_Encrypt( Key, IVPtr, IVLen, pPlainText, cbPlainText, pCypherText, cbCypherText );
}

BOOL Decrypt( BYTE *Key, BYTE *pCypherText, DWORD cbCypherText, BYTE* pPlainText, DWORD cbPlainText )
{
    CLR_UINT8* IVPtr;
    CLR_UINT32 IVLen;
    UINT8      tmp[ TEA_KEY_SIZE_BYTES ];

    IVPtr = tmp;
    IVLen = sizeof(tmp);
    memset( tmp, 0, sizeof(tmp) );

    return Crypto_Decrypt( Key, IVPtr, IVLen, pCypherText, cbCypherText, pPlainText, cbPlainText );
}
*/

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_NetworkInformation_Wireless80211::UpdateConfiguration___STATIC__VOID__MicrosoftSPOTNetNetworkInformationWireless80211__BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    SOCK_WirelessConfiguration config; 
    CLR_RT_HeapBlock*          pConfig         = stack.Arg0().Dereference();  _ASSERTE(pConfig != NULL);
    bool                       useEncryption   = stack.Arg1().NumericByRef().s1 != 0;
    CLR_UINT32                 interfaceIndex  = pConfig[ FIELD__Id ].NumericByRefConst().u4;
    CLR_UINT32                 authentication;
    CLR_UINT32                 encryption;
    CLR_UINT32                 radio;
    CLR_RT_HeapBlock_String*   hbPassPhrase    = NULL;
    CLR_RT_HeapBlock_Array*    pNetworkKey     = pConfig[ FIELD__NetworkKey    ].DereferenceArray(); _ASSERTE(pNetworkKey != NULL);
    CLR_RT_HeapBlock_Array*    pReKeyInternal  = pConfig[ FIELD__ReKeyInternal ].DereferenceArray(); _ASSERTE(pReKeyInternal != NULL);
    CLR_RT_HeapBlock_String*   hbSsId          = NULL;
    CLR_UINT32                 ssidLength;
    CLR_UINT32                 passPhraseLength;
    //RSAKey*                    key             = NULL;    

    TINYCLR_CLEAR(config);

    authentication                = pConfig[ FIELD__Authentication ].NumericByRef().u4;
    encryption                    = pConfig[ FIELD__Encryption     ].NumericByRef().u4;
    radio                         = pConfig[ FIELD__Radio          ].NumericByRef().u4;

    config.wirelessFlags = WIRELESS_FLAG_AUTHENTICATION__set(authentication) | WIRELESS_FLAG_ENCRYPTION__set(encryption) | WIRELESS_FLAG_RADIO__set(radio);

    hbPassPhrase = pConfig[ FIELD__PassPhrase ].DereferenceString(); FAULT_ON_NULL(hbPassPhrase);     
    passPhraseLength = hal_strlen_s(hbPassPhrase->StringText());
    if (passPhraseLength >= sizeof(config.passPhrase)) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);        

    hbSsId = pConfig[ FIELD__Ssid ].DereferenceString(); FAULT_ON_NULL(hbSsId);     
    ssidLength = hal_strlen_s(hbSsId->StringText());
    if (ssidLength >= sizeof(config.ssid)) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);        

    if (pNetworkKey->m_numOfElements > sizeof(config.networkKey)) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);        
    config.networkKeyLength = pNetworkKey->m_numOfElements;

    if (pReKeyInternal->m_numOfElements > sizeof(config.reKeyInternal)) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    config.reKeyLength = pReKeyInternal->m_numOfElements;

    if (useEncryption)
    {
        ASSERT(FALSE); // TODO: IMPLEMENT ENCRYPTION
        //key = RetrieveWirelessEncryptionKey();
    }
        
    /// Only update this when we have the key (and it is symmetric??).
    /*
        if (key != NULL)
        {   
            char passPhraseBuff[WIRELESS_PASSPHRASE_LENGTH];

            /// Data is encrypted when stored in config sector.
            config.wirelessFlags |= WIRELESS_FLAG_DATA__set(WIRELESS_FLAG_DATA_ENCRYPTED);

            hal_strncpy_s( passPhraseBuff, WIRELESS_PASSPHRASE_LENGTH, hbPassPhrase->StringText(), passPhraseLength );
                            
            Encrypt( (BYTE *)key, (BYTE *)passPhraseBuff, WIRELESS_PASSPHRASE_LENGTH - 1, (BYTE *)config.passPhrase, WIRELESS_PASSPHRASE_LENGTH - 1 );
            config.passPhrase[ WIRELESS_PASSPHRASE_LENGTH - 1 ] = 0;

            if (pNetworkKey->m_numOfElements > 0)
            {
                Encrypt( (BYTE *)key, (BYTE *)pNetworkKey->GetFirstElement(), pNetworkKey->m_numOfElements, (BYTE *)&config.networkKey, pNetworkKey->m_numOfElements );
            }
            if (pReKeyInternal->m_numOfElements > 0)
            {
                Encrypt( (BYTE *)key, (BYTE *)pReKeyInternal->GetFirstElement(), pReKeyInternal->m_numOfElements, (BYTE *)&config.reKeyInternal, pReKeyInternal->m_numOfElements );
            }
        }
        else
        */
    {                
        /// Data is saved as clear text.
        config.wirelessFlags &= ~(WIRELESS_FLAG_DATA__set(WIRELESS_FLAG_DATA_ENCRYPTED));

        /// Unable to encrypt, in that case keep things as is.
        hal_strncpy_s( config.passPhrase, WIRELESS_PASSPHRASE_LENGTH, hbPassPhrase->StringText(), passPhraseLength );    

        if(config.networkKeyLength > 0)
        {
            memcpy( &config.networkKey, pNetworkKey->GetFirstElement(), config.networkKeyLength); 
        }
        if(config.reKeyLength > 0)
        {
            memcpy( &config.reKeyInternal, pReKeyInternal->GetFirstElement(), config.reKeyLength); 
        }
    }

    hal_strncpy_s( config.ssid, WIRELESS_PASSPHRASE_LENGTH, hbSsId->StringText(), ssidLength );    

    TINYCLR_CHECK_HRESULT(SOCK_CONFIGURATION_UpdateWirelessConfiguration( interfaceIndex, &config ));
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_net_native_Microsoft_SPOT_Net_NetworkInformation_Wireless80211::SaveAllConfigurations___STATIC__VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_NETWORK();
    TINYCLR_HEADER();

    TINYCLR_CHECK_HRESULT(SOCK_CONFIGURATION_SaveAllWirelessConfigurations());

    TINYCLR_NOCLEANUP();
}
