////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SslUpdateValidation.h"

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_UpdateValidation"
#endif

UpdateValidationSSL g_UpdateValidationSSL;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

IUpdateValidationProvider g_SslUpdateValidationProvider = 
{
    UpdateValidationSSL::AuthCommand,
    UpdateValidationSSL::Authenticate,
    UpdateValidationSSL::ValidatePacket,
    UpdateValidationSSL::ValidateUpdate,

    NULL,
};

static UINT8 s_UpdateSslPublicKeyBuffer[HAL_UPDATE_SIGNATURE_SIZE + 64]; // Add room for blob header

BOOL UpdateValidationSSL::AuthCommand( MFUpdate* pUpdate, UINT32 cmd, UINT8* pArgs, INT32 argsLen, UINT8* pResponse, INT32& responseLen )
{
    BOOL fRet = FALSE;
    
    switch(cmd)
    {
        case MFUPDATE_VALIDATION_COMMAND__GET_AUTH_TYPE:
            if(pResponse == NULL)
            {
                responseLen = sizeof(UINT32);
                fRet = TRUE;
            }
            else if(responseLen >= sizeof(UINT32))
            {
                *(UINT32*)pResponse = MFUPDATE_AUTHENTICATION_TYPE__SSL;

                fRet = TRUE;
            }
            break;
    }

    return fRet;    
}
BOOL UpdateValidationSSL::Authenticate( MFUpdate* pUpdate, UINT8* pAuthData, INT32 authLen )
{
    if(authLen > sizeof(s_UpdateSslPublicKeyBuffer))
    {
        return FALSE;
    }

    if(authLen > 0)
    {
        memcpy(s_UpdateSslPublicKeyBuffer, pAuthData, authLen);
    }

    g_UpdateValidationSSL.m_pPublicCertData = s_UpdateSslPublicKeyBuffer;
    g_UpdateValidationSSL.m_publicCertLen   = authLen;
    
    return DebuggerPort_UpgradeToSsl( HalSystemConfig.DebuggerPorts[0], 0 );
}

BOOL UpdateValidationSSL::ValidatePacket( MFUpdate* pUpdate, UINT8* pPacket, INT32 packetLen, UINT8* pValidation, INT32 validationLen )
{
    if(validationLen != sizeof(UINT32) || pPacket == NULL || pValidation == NULL) return FALSE;
    
    return (SUPPORT_ComputeCRC(pPacket, packetLen, 0) == *(UINT32*)pValidation);
}

BOOL UpdateValidationSSL::ValidateUpdate( MFUpdate* pUpdate, UINT8* pValidation, INT32 validationLen )
{
    CK_MECHANISM_TYPE mechs[] = { CKM_RSA_PKCS };
    CK_SLOT_ID slotID;
    CK_SESSION_HANDLE session;
    CK_OBJECT_CLASS ckoPubKey = CKO_PUBLIC_KEY;
    CK_OBJECT_HANDLE hPubKey;
    CK_MECHANISM_TYPE sha1Mech = CKM_SHA_1;
    CK_KEY_TYPE keyType = CKK_RSA;
    CK_MECHANISM mech = { CKM_RSA_PKCS, &sha1Mech, sizeof(sha1Mech) };
    BOOL retVal = FALSE;

    CK_ATTRIBUTE attribs[] =
    {
        { CKA_CLASS   , &ckoPubKey                             , sizeof(ckoPubKey)                     },
        { CKA_KEY_TYPE, &keyType                               , sizeof(keyType)                       },
        { CKA_VALUE   , g_UpdateValidationSSL.m_pPublicCertData, g_UpdateValidationSSL.m_publicCertLen }
    };

    if(pUpdate->Providers->Storage == NULL) return FALSE;

    if(pValidation == NULL) return FALSE;

    C_Initialize(NULL);

    slotID = Cryptoki_FindSlot(NULL, mechs, ARRAYSIZE(mechs));  if(CK_SLOT_ID_INVALID == slotID) return FALSE;
    
    if(CKR_OK == C_OpenSession(slotID, CKF_SERIAL_SESSION, NULL, NULL, &session) && (CK_SESSION_HANDLE_INVALID != session))
    {
        if(CKR_OK == C_CreateObject(session, attribs, ARRAYSIZE(attribs), &hPubKey) && hPubKey != CK_OBJECT_HANDLE_INVALID)
        {
            if(CKR_OK == C_VerifyInit(session, &mech, hPubKey)) 
            {
                UINT8 buff[512];
                INT32 len = sizeof(buff);
                INT32 updateSize = pUpdate->Header.UpdateSize;
                INT32 offset = 0;

                while(offset < updateSize)
                {
                    if((offset + len) > updateSize)
                    {
                        len = updateSize - offset;
                    }
                    
                    if(!pUpdate->Providers->Storage->Read(pUpdate->StorageHandle, offset, buff, len)) break;

                    C_VerifyUpdate(session, buff, len);

                    offset += len;
                }
                retVal = CKR_OK == C_VerifyFinal(session, pValidation, (CK_ULONG)validationLen);
            }

            C_DestroyObject(session, hPubKey);
        }
        
        C_CloseSession(session);
    }

    return retVal;    
}



