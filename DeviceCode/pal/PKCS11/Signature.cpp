#include "CryptokiPAL.h"

/* Signing and MACing */

/* C_SignInit initializes a signature (private key encryption)
 * operation, where the signature is (will be) an appendix to
 * the data, and plaintext cannot be recovered from the
 *signature. */
CK_DEFINE_FUNCTION(CK_RV, C_SignInit)
(
  CK_SESSION_HANDLE hSession,    /* the session's handle */
  CK_MECHANISM_PTR  pMechanism,  /* the signature mechanism */
  CK_OBJECT_HANDLE  hKey         /* handle of signature key */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret        != CKR_OK) return ret;
    if(pMechanism == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Signature != NULL && pSession->Token->Signature->SignInit != NULL)
    {
        return pSession->Token->Signature->SignInit(&pSession->Context, pMechanism, hKey);
    }
    
    return CKR_FUNCTION_NOT_SUPPORTED;
}

/* C_Sign signs (encrypts with private key) data in a single
 * part, where the signature is (will be) an appendix to the
 * data, and plaintext cannot be recovered from the signature. */
CK_DEFINE_FUNCTION(CK_RV, C_Sign)
(
  CK_SESSION_HANDLE hSession,        /* the session's handle */
  CK_BYTE_PTR       pData,           /* the data to sign */
  CK_ULONG          ulDataLen,       /* count of bytes to sign */
  CK_BYTE_PTR       pSignature,      /* gets the signature */
  CK_ULONG_PTR      pulSignatureLen  /* gets signature length */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret              != CKR_OK) return ret;
    if(pData            == NULL  ) return CKR_ARGUMENTS_BAD;
    if(pulSignatureLen  == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Signature != NULL && pSession->Token->Signature->Sign != NULL)
    {
        return pSession->Token->Signature->Sign(&pSession->Context, pData, ulDataLen, pSignature, pulSignatureLen);
    }
    
    return CKR_FUNCTION_NOT_SUPPORTED;    
}

/* C_SignUpdate continues a multiple-part signature operation,
 * where the signature is (will be) an appendix to the data, 
 * and plaintext cannot be recovered from the signature. */
CK_DEFINE_FUNCTION(CK_RV, C_SignUpdate)
(
  CK_SESSION_HANDLE hSession,  /* the session's handle */
  CK_BYTE_PTR       pPart,     /* the data to sign */
  CK_ULONG          ulPartLen  /* count of bytes to sign */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret              != CKR_OK) return ret;
    if(pPart            == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Signature != NULL && pSession->Token->Signature->SignUpdate != NULL)
    {
        return pSession->Token->Signature->SignUpdate(&pSession->Context, pPart, ulPartLen);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;    
}


/* C_SignFinal finishes a multiple-part signature operation, 
 * returning the signature. */
CK_DEFINE_FUNCTION(CK_RV, C_SignFinal)
(
  CK_SESSION_HANDLE hSession,        /* the session's handle */
  CK_BYTE_PTR       pSignature,      /* gets the signature */
  CK_ULONG_PTR      pulSignatureLen  /* gets signature length */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret             != CKR_OK) return ret;
    if(pSignature      == NULL  ) return CKR_ARGUMENTS_BAD;
    if(pulSignatureLen == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Signature != NULL && pSession->Token->Signature->SignFinal != NULL)
    {
        return pSession->Token->Signature->SignFinal(&pSession->Context, pSignature, pulSignatureLen);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;    
}

/* C_SignRecoverInit initializes a signature operation, where
 * the data can be recovered from the signature. */
CK_DEFINE_FUNCTION(CK_RV, C_SignRecoverInit)
(
  CK_SESSION_HANDLE hSession,   /* the session's handle */
  CK_MECHANISM_PTR  pMechanism, /* the signature mechanism */
  CK_OBJECT_HANDLE  hKey        /* handle of the signature key */
)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

/* C_SignRecover signs data in a single operation, where the
 * data can be recovered from the signature. */
CK_DEFINE_FUNCTION(CK_RV, C_SignRecover)
(
  CK_SESSION_HANDLE hSession,        /* the session's handle */
  CK_BYTE_PTR       pData,           /* the data to sign */
  CK_ULONG          ulDataLen,       /* count of bytes to sign */
  CK_BYTE_PTR       pSignature,      /* gets the signature */
  CK_ULONG_PTR      pulSignatureLen  /* gets signature length */
)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}




/* Verifying signatures and MACs */

/* C_VerifyInit initializes a verification operation, where the
 * signature is an appendix to the data, and plaintext cannot
 *  cannot be recovered from the signature (e.g. DSA). */
CK_DEFINE_FUNCTION(CK_RV, C_VerifyInit)
(
  CK_SESSION_HANDLE hSession,    /* the session's handle */
  CK_MECHANISM_PTR  pMechanism,  /* the verification mechanism */
  CK_OBJECT_HANDLE  hKey         /* verification key */ 
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret        != CKR_OK) return ret;
    if(pMechanism == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Signature != NULL && pSession->Token->Signature->VerifyInit != NULL)
    {
        return pSession->Token->Signature->VerifyInit(&pSession->Context, pMechanism, hKey);
    }
    
    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_Verify verifies a signature in a single-part operation, 
 * where the signature is an appendix to the data, and plaintext
 * cannot be recovered from the signature. */
CK_DEFINE_FUNCTION(CK_RV, C_Verify)
(
  CK_SESSION_HANDLE hSession,       /* the session's handle */
  CK_BYTE_PTR       pData,          /* signed data */
  CK_ULONG          ulDataLen,      /* length of signed data */
  CK_BYTE_PTR       pSignature,     /* signature */
  CK_ULONG          ulSignatureLen  /* signature length*/
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret        != CKR_OK) return ret;
    if(pData      == NULL  ) return CKR_ARGUMENTS_BAD;
    if(pSignature == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Signature != NULL && pSession->Token->Signature->Verify != NULL)
    {
        return pSession->Token->Signature->Verify(&pSession->Context, pData, ulDataLen, pSignature, ulSignatureLen);
    }
    
    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_VerifyUpdate continues a multiple-part verification
 * operation, where the signature is an appendix to the data, 
 * and plaintext cannot be recovered from the signature. */
CK_DEFINE_FUNCTION(CK_RV, C_VerifyUpdate)
(
  CK_SESSION_HANDLE hSession,  /* the session's handle */
  CK_BYTE_PTR       pPart,     /* signed data */
  CK_ULONG          ulPartLen  /* length of signed data */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret   != CKR_OK) return ret;
    if(pPart == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Signature != NULL && pSession->Token->Signature->VerifyUpdate != NULL)
    {
        return pSession->Token->Signature->VerifyUpdate(&pSession->Context, pPart, ulPartLen);
    }
    
    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_VerifyFinal finishes a multiple-part verification
 * operation, checking the signature. */
CK_DEFINE_FUNCTION(CK_RV, C_VerifyFinal)
(
  CK_SESSION_HANDLE hSession,       /* the session's handle */
  CK_BYTE_PTR       pSignature,     /* signature to verify */
  CK_ULONG          ulSignatureLen  /* signature length */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret        != CKR_OK) return ret;
    if(pSignature == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->Signature != NULL && pSession->Token->Signature->VerifyFinal != NULL)
    {
        return pSession->Token->Signature->VerifyFinal(&pSession->Context, pSignature, ulSignatureLen);
    }
    
    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_VerifyRecoverInit initializes a signature verification
 * operation, where the data is recovered from the signature. */
CK_DEFINE_FUNCTION(CK_RV, C_VerifyRecoverInit)
(
  CK_SESSION_HANDLE hSession,    /* the session's handle */
  CK_MECHANISM_PTR  pMechanism,  /* the verification mechanism */
  CK_OBJECT_HANDLE  hKey         /* verification key */
)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_VerifyRecover verifies a signature in a single-part
 * operation, where the data is recovered from the signature. */
CK_DEFINE_FUNCTION(CK_RV, C_VerifyRecover)
(
  CK_SESSION_HANDLE hSession,        /* the session's handle */
  CK_BYTE_PTR       pSignature,      /* signature to verify */
  CK_ULONG          ulSignatureLen,  /* signature length */
  CK_BYTE_PTR       pData,           /* gets signed data */
  CK_ULONG_PTR      pulDataLen       /* gets signed data len */
)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

