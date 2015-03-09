#include "CryptokiPAL.h"

/* Key management */

/* C_GenerateKey generates a secret key, creating a new key
 * object. */
CK_DEFINE_FUNCTION(CK_RV, C_GenerateKey)
(
  CK_SESSION_HANDLE    hSession,    /* the session's handle */
  CK_MECHANISM_PTR     pMechanism,  /* key generation mech. */
  CK_ATTRIBUTE_PTR     pTemplate,   /* template for new key */
  CK_ULONG             ulCount,     /* # of attrs in template */
  CK_OBJECT_HANDLE_PTR phKey        /* gets handle of new key */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret        != CKR_OK) return ret;
    if(pMechanism == NULL  ) return CKR_ARGUMENTS_BAD;
    if(pTemplate  == NULL  ) return CKR_ARGUMENTS_BAD;
    if(phKey      == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->KeyMgmt != NULL && pSession->Token->KeyMgmt->GenerateKey != NULL)
    {
        ret = pSession->Token->KeyMgmt->GenerateKey(&pSession->Context, pMechanism, pTemplate, ulCount, phKey);

#ifdef _DEBUG
        if(ret == CKR_OK)
        {
            pSession->AddObjectHandle(*phKey);
        }
#endif
        return ret;
    }
    
    return CKR_FUNCTION_NOT_SUPPORTED;
}

/* C_GenerateKeyPair generates a public-key/private-key pair, 
 * creating new key objects. */
CK_DEFINE_FUNCTION(CK_RV, C_GenerateKeyPair)
(
  CK_SESSION_HANDLE    hSession,                    /* session
                                                     * handle */
  CK_MECHANISM_PTR     pMechanism,                  /* key-gen
                                                     * mech. */
  CK_ATTRIBUTE_PTR     pPublicKeyTemplate,          /* template
                                                     * for pub.
                                                     * key */
  CK_ULONG             ulPublicKeyAttributeCount,   /* # pub.
                                                     * attrs. */
  CK_ATTRIBUTE_PTR     pPrivateKeyTemplate,         /* template
                                                     * for priv.
                                                     * key */
  CK_ULONG             ulPrivateKeyAttributeCount,  /* # priv.
                                                     * attrs. */
  CK_OBJECT_HANDLE_PTR phPublicKey,                 /* gets pub.
                                                     * key
                                                     * handle */
  CK_OBJECT_HANDLE_PTR phPrivateKey                 /* gets
                                                     * priv. key
                                                     * handle */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret                  != CKR_OK) return ret;
    if(pMechanism           == NULL  ) return CKR_ARGUMENTS_BAD;
    //if(pPublicKeyTemplate   == NULL  ) return CKR_ARGUMENTS_BAD;
    if(phPublicKey          == NULL  ) return CKR_ARGUMENTS_BAD;
    //if(pPrivateKeyTemplate  == NULL  ) return CKR_ARGUMENTS_BAD;
    if(phPrivateKey         == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->KeyMgmt != NULL && pSession->Token->KeyMgmt->GenerateKeyPair != NULL)
    {
        ret = pSession->Token->KeyMgmt->GenerateKeyPair(&pSession->Context, pMechanism, pPublicKeyTemplate, ulPublicKeyAttributeCount, pPrivateKeyTemplate, ulPrivateKeyAttributeCount, phPublicKey, phPrivateKey);

#ifdef _DEBUG
        if(ret == CKR_OK)
        {
            pSession->AddObjectHandle(*phPublicKey);
        }
#endif

        return ret;
    }
    
    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_WrapKey wraps (i.e., encrypts) a key. */
CK_DEFINE_FUNCTION(CK_RV, C_WrapKey)
(
  CK_SESSION_HANDLE hSession,        /* the session's handle */
  CK_MECHANISM_PTR  pMechanism,      /* the wrapping mechanism */
  CK_OBJECT_HANDLE  hWrappingKey,    /* wrapping key */
  CK_OBJECT_HANDLE  hKey,            /* key to be wrapped */
  CK_BYTE_PTR       pWrappedKey,     /* gets wrapped key */
  CK_ULONG_PTR      pulWrappedKeyLen /* gets wrapped key size */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret              != CKR_OK) return ret;
    if(pMechanism       == NULL  ) return CKR_ARGUMENTS_BAD;
    if(pWrappedKey      == NULL  ) return CKR_ARGUMENTS_BAD;
    if(pulWrappedKeyLen == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->KeyMgmt != NULL && pSession->Token->KeyMgmt->WrapKey != NULL)
    {
        return pSession->Token->KeyMgmt->WrapKey(&pSession->Context, pMechanism, hWrappingKey, hKey, pWrappedKey, pulWrappedKeyLen);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_UnwrapKey unwraps (decrypts) a wrapped key, creating a new
 * key object. */
CK_DEFINE_FUNCTION(CK_RV, C_UnwrapKey)

(
  CK_SESSION_HANDLE    hSession,          /* session's handle */
  CK_MECHANISM_PTR     pMechanism,        /* unwrapping mech. */
  CK_OBJECT_HANDLE     hUnwrappingKey,    /* unwrapping key */
  CK_BYTE_PTR          pWrappedKey,       /* the wrapped key */
  CK_ULONG             ulWrappedKeyLen,   /* wrapped key len */
  CK_ATTRIBUTE_PTR     pTemplate,         /* new key template */
  CK_ULONG             ulAttributeCount,  /* template length */
  CK_OBJECT_HANDLE_PTR phKey              /* gets new handle */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret         != CKR_OK) return ret;
    if(pMechanism  == NULL  ) return CKR_ARGUMENTS_BAD;
    if(pWrappedKey == NULL  ) return CKR_ARGUMENTS_BAD;
    if(phKey       == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->KeyMgmt != NULL && pSession->Token->KeyMgmt->UnwrapKey != NULL)
    {
        ret = pSession->Token->KeyMgmt->UnwrapKey(&pSession->Context, pMechanism, hUnwrappingKey, pWrappedKey, ulWrappedKeyLen, pTemplate, ulAttributeCount, phKey);

#ifdef _DEBUG
        if(ret == CKR_OK)
        {
            pSession->AddObjectHandle(*phKey);
        }
#endif

        return ret;
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_DeriveKey derives a key from a base key, creating a new key
 * object. */
CK_DEFINE_FUNCTION(CK_RV, C_DeriveKey)

(
  CK_SESSION_HANDLE    hSession,          /* session's handle */
  CK_MECHANISM_PTR     pMechanism,        /* key deriv. mech. */
  CK_OBJECT_HANDLE     hBaseKey,          /* base key */
  CK_ATTRIBUTE_PTR     pTemplate,         /* new key template */
  CK_ULONG             ulAttributeCount,  /* template length */
  CK_OBJECT_HANDLE_PTR phKey              /* gets new handle */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret        != CKR_OK) return ret;
    if(pMechanism == NULL  ) return CKR_ARGUMENTS_BAD;
    //if(pTemplate  == NULL  ) return CKR_ARGUMENTS_BAD;
    if(phKey      == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->KeyMgmt != NULL && pSession->Token->KeyMgmt->DeriveKey != NULL)
    {
        ret = pSession->Token->KeyMgmt->DeriveKey(&pSession->Context, pMechanism, hBaseKey, pTemplate, ulAttributeCount, phKey);

#ifdef _DEBUG
        if(ret == CKR_OK)
        {
            pSession->AddObjectHandle(*phKey);
        }
#endif

        return ret;
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}

