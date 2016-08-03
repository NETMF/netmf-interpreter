////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CryptoInterface.h"
#include "ConfigurationManager.h"

extern UINT8* g_ConfigBuffer;
extern int g_ConfigBufferLength;

CryptoState::CryptoState( UINT32 dataAddress, UINT32 dataLength, BYTE* sig, UINT32 sigLength, UINT32 sectorType ) : 
#if defined(ARM_V1_2)
    m_dataAddress( CPU_GetCachableAddress( dataAddress ) ),
#else
    m_dataAddress( dataAddress    ),
#endif
    m_dataLength ( dataLength      ),
    m_sig        ( sig             ),
    m_sigLength  ( sigLength       ),
    m_sectorType ( sectorType      ),
    m_handle     ( NULL            ),
    m_res        ( CRYPTO_FAILURE  )
{
}

CryptoState::~CryptoState()
{
    ::Crypto_AbortRSAOperation( &m_handle );
}

bool CryptoState::VerifySignature( UINT32 keyIndex )
{   
    RSAKey* key = NULL;

    // global config pointer not in bootloader image

    //---------------------------------------------
    // IF THERE IS NO CONFIG SECTOR IN THE FLASH SECTOR TABLE, THEN WE DON'T HAVE KEYS, 
    // THEREFORE WE WILL NOT PERFORM SIGNATURE CHECKING.
    // 
    if(g_PrimaryConfigManager.m_device == NULL)
        return true;


    switch(m_sectorType)
    {
    case BlockRange::BLOCKTYPE_DEPLOYMENT:
        // backwards compatibility
        if(g_PrimaryConfigManager.GetTinyBooterVersion() != ConfigurationSector::c_CurrentVersionTinyBooter)
            return true;

        // if there is no key then we do not need to check the signature for the deployment sectors ONLY
        if(g_PrimaryConfigManager.CheckSignatureKeyEmpty( ConfigurationSector::c_DeployKeyDeployment ))
        {
            return true;
        }

        key = (RSAKey*)g_PrimaryConfigManager.GetDeploymentKeys( ConfigurationSector::c_DeployKeyDeployment );
        
        break;

        // We have to be carefull of how we update the config sector as it contains the signature keys for firmware updates.
        // Unlike all other updates, the config data is written into RAM in the buffer g_ConfigBuffer.  THis is necessary because
        // we don't want to blow away the keys unless we know that the signature is OK.
        //
        // Also, if the keys are unchanged then we don't require signature verification.  Besides the keys there is not really anything
        // we need to protect in this sector.
    case BlockRange::BLOCKTYPE_CONFIG:
        {
            ASSERT(g_ConfigBufferLength > 0);
            ASSERT(g_ConfigBuffer != NULL);

            if(g_ConfigBuffer == NULL || g_ConfigBufferLength <= 0)
                return false;

            // the g_ConfigBuffer contains the new configuration data
            const ConfigurationSector* pNewCfg = (const ConfigurationSector*)g_ConfigBuffer;

            bool fCanWrite = false;
            bool fRet      = false;

            if(pNewCfg->Version.TinyBooter == ConfigurationSector::c_CurrentVersionTinyBooter)
            {
                // allow updates of old config - for backwards compatibility
                if(g_PrimaryConfigManager.GetTinyBooterVersion() != ConfigurationSector::c_CurrentVersionTinyBooter)
                {
                    fCanWrite = true;
                }
                else
                {
                    bool key1Change = (!g_PrimaryConfigManager.VerifiySignatureKey(0, (BYTE *)&pNewCfg->DeploymentKeys[0]));
                    bool key2Change = (!g_PrimaryConfigManager.VerifiySignatureKey(1, (BYTE *)&pNewCfg->DeploymentKeys[1]));
                    bool key1Empty  =   g_PrimaryConfigManager.CheckSignatureKeyEmpty(0);
                    bool key2Empty  =   g_PrimaryConfigManager.CheckSignatureKeyEmpty(1);
                    

                    // make sure there are no key changes (unelss the key was uninitialized)    
                    if((key1Empty || !key1Change) &&
                       (key2Empty || !key2Change))
                    {
                        fCanWrite = true;
                    }
                }   

                if(fCanWrite)
                {                
                    // write the configuration
                    g_PrimaryConfigManager.EraseWriteConfigBlock( (BYTE *) g_ConfigBuffer, g_ConfigBufferLength );
                    fRet = true;
                }
            }

            // free RAM buffer
            private_free(g_ConfigBuffer);
            g_ConfigBuffer = NULL;

            return fRet;
            
        }
        break;

    default:
        // backwards compatibility


        if(g_PrimaryConfigManager.GetTinyBooterVersion() != ConfigurationSector::c_CurrentVersionTinyBooter)
            return true;

        // if there is no key then we do not need to check the signature for the deployment sectors ONLY
        if (g_PrimaryConfigManager.CheckSignatureKeyEmpty( keyIndex ))
        {
            return true;
        }

        key = (RSAKey*)g_PrimaryConfigManager.GetDeploymentKeys( keyIndex );

        break;
    };

    if(key == NULL)
    {
        return false; 
    }

    m_res = ::Crypto_StartRSAOperationWithKey( RSA_VERIFYSIGNATURE, key, (BYTE*)m_dataAddress, m_dataLength, (BYTE*)m_sig, m_sigLength, &m_handle );

    
    while(CRYPTO_CONTINUE == m_res)
    {
        m_res = ::Crypto_StepRSAOperation( &m_handle );
    }

    return m_res == CRYPTO_SUCCESS;
}

