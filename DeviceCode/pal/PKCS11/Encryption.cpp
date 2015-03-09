#include "CryptokiPAL.h"


/* Encryption and decryption */

/* C_EncryptInit initializes an encryption operation. */
CK_DEFINE_FUNCTION(CK_RV, C_EncryptInit)
(
  CK_SESSION_HANDLE hSession,    /* the session's handle */
  CK_MECHANISM_PTR  pMechanism,  /* the encryption mechanism */
  CK_OBJECT_HANDLE  hKey         /* handle of encryption key */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret        != CKR_OK) return ret;
    if(pMechanism == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Encryption != NULL && pSession->Token->Encryption->EncryptInit != NULL)
    {
        return pSession->Token->Encryption->EncryptInit(&pSession->Context, pMechanism, hKey);
    }
    
    return CKR_FUNCTION_NOT_SUPPORTED;
}

/* C_Encrypt encrypts single-part data. */
CK_DEFINE_FUNCTION(CK_RV, C_Encrypt)
(
  CK_SESSION_HANDLE hSession,            /* session's handle */
  CK_BYTE_PTR       pData,               /* the plaintext data */
  CK_ULONG          ulDataLen,           /* bytes of plaintext */
  CK_BYTE_PTR       pEncryptedData,      /* gets ciphertext */
  CK_ULONG_PTR      pulEncryptedDataLen  /* gets c-text size */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret                 != CKR_OK) return ret;
    if(pData               == NULL  ) return CKR_ARGUMENTS_BAD;
    if(pulEncryptedDataLen == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Encryption != NULL && pSession->Token->Encryption->Encrypt != NULL)
    {
        return pSession->Token->Encryption->Encrypt(&pSession->Context, pData, ulDataLen, pEncryptedData, pulEncryptedDataLen);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_EncryptUpdate continues a multiple-part encryption
 * operation. */
CK_DEFINE_FUNCTION(CK_RV, C_EncryptUpdate)
(
  CK_SESSION_HANDLE hSession,           /* session's handle */
  CK_BYTE_PTR       pPart,              /* the plaintext data */
  CK_ULONG          ulPartLen,          /* plaintext data len */
  CK_BYTE_PTR       pEncryptedPart,     /* gets ciphertext */
  CK_ULONG_PTR      pulEncryptedPartLen /* gets c-text size */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret                 != CKR_OK) return ret;
    if(pPart               == NULL  ) return CKR_ARGUMENTS_BAD;
    if(pEncryptedPart      == NULL  ) return CKR_ARGUMENTS_BAD;
    if(pulEncryptedPartLen == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Encryption != NULL && pSession->Token->Encryption->EncryptUpdate != NULL)
    {
        return pSession->Token->Encryption->EncryptUpdate(&pSession->Context, pPart, ulPartLen, pEncryptedPart, pulEncryptedPartLen);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_EncryptFinal finishes a multiple-part encryption
 * operation. */
CK_DEFINE_FUNCTION(CK_RV, C_EncryptFinal)
(
  CK_SESSION_HANDLE hSession,                /* session handle */
  CK_BYTE_PTR       pLastEncryptedPart,      /* last c-text */
  CK_ULONG_PTR      pulLastEncryptedPartLen  /* gets last size */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret                     != CKR_OK) return ret;
    if(pLastEncryptedPart      == NULL  ) return CKR_ARGUMENTS_BAD;
    if(pulLastEncryptedPartLen == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Encryption != NULL && pSession->Token->Encryption->EncryptFinal != NULL)
    {
        return pSession->Token->Encryption->EncryptFinal(&pSession->Context, pLastEncryptedPart, pulLastEncryptedPartLen);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_DecryptInit initializes a decryption operation. */
CK_DEFINE_FUNCTION(CK_RV, C_DecryptInit)
(
  CK_SESSION_HANDLE hSession,    /* the session's handle */
  CK_MECHANISM_PTR  pMechanism,  /* the decryption mechanism */
  CK_OBJECT_HANDLE  hKey         /* handle of decryption key */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret        != CKR_OK) return ret;
    if(pMechanism == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Encryption != NULL && pSession->Token->Encryption->DecryptInit != NULL)
    {
        return pSession->Token->Encryption->DecryptInit(&pSession->Context, pMechanism, hKey);
    }
    
    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_Decrypt decrypts encrypted data in a single part. */
CK_DEFINE_FUNCTION(CK_RV, C_Decrypt)
(
  CK_SESSION_HANDLE hSession,           /* session's handle */
  CK_BYTE_PTR       pEncryptedData,     /* ciphertext */
  CK_ULONG          ulEncryptedDataLen, /* ciphertext length */
  CK_BYTE_PTR       pData,              /* gets plaintext */
  CK_ULONG_PTR      pulDataLen          /* gets p-text size */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret            != CKR_OK) return ret;
    if(pEncryptedData == NULL  ) return CKR_ARGUMENTS_BAD;
    if(pulDataLen     == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Encryption != NULL && pSession->Token->Encryption->Decrypt != NULL)
    {
        return pSession->Token->Encryption->Decrypt(&pSession->Context, pEncryptedData, ulEncryptedDataLen, pData, pulDataLen);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_DecryptUpdate continues a multiple-part decryption
 * operation. */
CK_DEFINE_FUNCTION(CK_RV, C_DecryptUpdate)
(
  CK_SESSION_HANDLE hSession,            /* session's handle */
  CK_BYTE_PTR       pEncryptedPart,      /* encrypted data */
  CK_ULONG          ulEncryptedPartLen,  /* input length */
  CK_BYTE_PTR       pPart,               /* gets plaintext */
  CK_ULONG_PTR      pulPartLen           /* p-text size */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret            != CKR_OK) return ret;
    if(pPart          == NULL  ) return CKR_ARGUMENTS_BAD;
    if(pEncryptedPart == NULL  ) return CKR_ARGUMENTS_BAD;
    if(pulPartLen     == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Encryption != NULL && pSession->Token->Encryption->DecryptUpdate != NULL)
    {
        return pSession->Token->Encryption->DecryptUpdate(&pSession->Context, pEncryptedPart, ulEncryptedPartLen, pPart, pulPartLen);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_DecryptFinal finishes a multiple-part decryption
 * operation. */
CK_DEFINE_FUNCTION(CK_RV, C_DecryptFinal)
(
  CK_SESSION_HANDLE hSession,       /* the session's handle */
  CK_BYTE_PTR       pLastPart,      /* gets plaintext */
  CK_ULONG_PTR      pulLastPartLen  /* p-text size */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret            != CKR_OK) return ret;
    if(pLastPart      == NULL  ) return CKR_ARGUMENTS_BAD;
    if(pulLastPartLen == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Encryption != NULL && pSession->Token->Encryption->DecryptFinal != NULL)
    {
        return pSession->Token->Encryption->DecryptFinal(&pSession->Context, pLastPart, pulLastPartLen);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}


