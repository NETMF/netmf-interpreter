#include "CryptokiPAL.h"


/* Message digesting */

/* C_DigestInit initializes a message-digesting operation. */
CK_DEFINE_FUNCTION(CK_RV, C_DigestInit)
(
  CK_SESSION_HANDLE hSession,   /* the session's handle */
  CK_MECHANISM_PTR  pMechanism  /* the digesting mechanism */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret                 != CKR_OK) return ret;
    if(pMechanism          == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Digest != NULL && pSession->Token->Digest->DigestInit != NULL)
    {
        return pSession->Token->Digest->DigestInit(&pSession->Context, pMechanism);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}

/* C_Digest digests data in a single part. */
CK_DEFINE_FUNCTION(CK_RV, C_Digest)
(
  CK_SESSION_HANDLE hSession,     /* the session's handle */
  CK_BYTE_PTR       pData,        /* data to be digested */
  CK_ULONG          ulDataLen,    /* bytes of data to digest */
  CK_BYTE_PTR       pDigest,      /* gets the message digest */
  CK_ULONG_PTR      pulDigestLen  /* gets digest length */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret          != CKR_OK) return ret;
    if(pData        == NULL  ) return CKR_ARGUMENTS_BAD;
    if(pDigest      == NULL  ) return CKR_ARGUMENTS_BAD;
    if(pulDigestLen == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Digest != NULL && pSession->Token->Digest->Digest != NULL)
    {
        return pSession->Token->Digest->Digest(&pSession->Context, pData, ulDataLen, pDigest, pulDigestLen);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_DigestUpdate continues a multiple-part message-digesting
 * operation. */
CK_DEFINE_FUNCTION(CK_RV, C_DigestUpdate)
(
  CK_SESSION_HANDLE hSession,  /* the session's handle */
  CK_BYTE_PTR       pPart,     /* data to be digested */
  CK_ULONG          ulPartLen  /* bytes of data to be digested */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret   != CKR_OK) return ret;
    if(pPart == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Digest != NULL && pSession->Token->Digest->DigestUpdate != NULL)
    {
        return pSession->Token->Digest->DigestUpdate(&pSession->Context, pPart, ulPartLen);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_DigestKey continues a multi-part message-digesting
 * operation, by digesting the value of a secret key as part of
 * the data already digested. */
CK_DEFINE_FUNCTION(CK_RV, C_DigestKey)
(
  CK_SESSION_HANDLE hSession,  /* the session's handle */
  CK_OBJECT_HANDLE  hKey       /* secret key to digest */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret != CKR_OK) return ret;

    if(pSession->Token->Digest != NULL && pSession->Token->Digest->DigestKey != NULL)
    {
        return pSession->Token->Digest->DigestKey(&pSession->Context, hKey);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_DigestFinal finishes a multiple-part message-digesting
 * operation. */
CK_DEFINE_FUNCTION(CK_RV, C_DigestFinal)

(
  CK_SESSION_HANDLE hSession,     /* the session's handle */
  CK_BYTE_PTR       pDigest,      /* gets the message digest */
  CK_ULONG_PTR      pulDigestLen  /* gets byte count of digest */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret          != CKR_OK) return ret;
    if(pDigest      == NULL  ) return CKR_ARGUMENTS_BAD;
    if(pulDigestLen == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Digest != NULL && pSession->Token->Digest->DigestFinal != NULL)
    {
        return pSession->Token->Digest->DigestFinal(&pSession->Context, pDigest, pulDigestLen);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}


