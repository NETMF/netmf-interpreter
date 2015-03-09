////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "X509UpdateValidation.h"

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_UpdateValidation"
#endif

UpdateValidationX509 g_UpdateValidationX509;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

IUpdateValidationProvider g_X509UpdateValidationProvider = 
{
    UpdateValidationX509::AuthCommand,
    UpdateValidationX509::Authenticate,
    UpdateValidationX509::ValidatePacket,
    UpdateValidationX509::ValidateUpdate,

    NULL,
};

//static UINT8 s_UpdateX509PublicKeyBuffer[HAL_UPDATE_SIGNATURE_SIZE + 64]; // Add room for blob header

BOOL UpdateValidationX509::AuthCommand( MFUpdate* pUpdate, UINT32 cmd, UINT8* pArgs, INT32 argsLen, UINT8* pResponse, INT32& responseLen )
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
                *(UINT32*)pResponse = MFUPDATE_AUTHENTICATION_TYPE__X509;

                fRet = TRUE;
            }
            break;
    }

    return fRet;    
}

BOOL UpdateValidationX509::Authenticate( MFUpdate* pUpdate, UINT8* pAuthData, INT32 authLen )
{
    return TRUE;
}

BOOL UpdateValidationX509::ValidatePacket( MFUpdate* pUpdate, UINT8* pPacket, INT32 packetLen, UINT8* pValidation, INT32 validationLen )
{
    if(validationLen != sizeof(UINT32) || pPacket == NULL || pValidation == NULL) return FALSE;
    
    return (SUPPORT_ComputeCRC(pPacket, packetLen, 0) == *(UINT32*)pValidation);
}

BOOL UpdateValidationX509::ValidateUpdate( MFUpdate* pUpdate, UINT8* pValidation, INT32 validationLen )
{
    CK_MECHANISM_TYPE mechs[] = { CKM_RSA_PKCS };
    CK_SLOT_ID slotID;
    CK_SESSION_HANDLE session;
    CK_OBJECT_CLASS ckoCert = CKO_CERTIFICATE;
    CK_OBJECT_HANDLE hCACert;
    CK_MECHANISM_TYPE sha1Mech = CKM_SHA_1;
    CK_MECHANISM mech = { CKM_RSA_PKCS, &sha1Mech, sizeof(sha1Mech) };
    BOOL retVal = FALSE;
    UINT8* caCert;
    UINT32 certLen = 0;

    if(g_DebuggerPortSslConfig.GetCertificateAuthority == NULL)
    {
        return FALSE;
    }
    
    g_DebuggerPortSslConfig.GetCertificateAuthority( &caCert, &certLen );

    CK_ATTRIBUTE attribs[] =
    {
        { CKA_CLASS   , &ckoCert, sizeof(ckoCert) },
        { CKA_VALUE   , caCert  , certLen         }
    };

    if(pUpdate->Providers->Storage == NULL) return FALSE;
    if(certLen == 0 || caCert == NULL     ) return FALSE;
    if(pValidation == NULL                ) return FALSE;

    C_Initialize(NULL);

    slotID = Cryptoki_FindSlot(NULL, mechs, ARRAYSIZE(mechs));  if(CK_SLOT_ID_INVALID == slotID) return FALSE;
    
    if(CKR_OK == C_OpenSession(slotID, CKF_SERIAL_SESSION, NULL, NULL, &session) && (CK_SESSION_HANDLE_INVALID != session))
    {
        if(CKR_OK == C_CreateObject(session, attribs, ARRAYSIZE(attribs), &hCACert) && hCACert != CK_OBJECT_HANDLE_INVALID)
        {
            if(CKR_OK == C_VerifyInit(session, &mech, hCACert)) 
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

            C_DestroyObject(session, hCACert);
        }
        
        C_CloseSession(session);
    }

    return retVal;    
}



