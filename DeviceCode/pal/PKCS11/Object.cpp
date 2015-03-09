#include "CryptokiPAL.h"

/* C_CreateObject creates a new object. */
CK_DEFINE_FUNCTION(CK_RV, C_CreateObject)
(
  CK_SESSION_HANDLE hSession,    /* the session's handle */
  CK_ATTRIBUTE_PTR  pTemplate,   /* the object's template */
  CK_ULONG          ulCount,     /* attributes in template */
  CK_OBJECT_HANDLE_PTR phObject  /* gets new object's handle. */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;
    CK_ULONG i = 0;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret       != CKR_OK) return ret;
    if(pTemplate == NULL  ) return CKR_ARGUMENTS_BAD;
    if(phObject  == NULL  ) return CKR_ARGUMENTS_BAD;

    *phObject = (CK_OBJECT_HANDLE)-1;
    
    if(pTemplate[i].type == CKA_CLASS && pTemplate[i].ulValueLen == sizeof(CK_OBJECT_CLASS))
    {
        CK_OBJECT_CLASS cls = SwapEndianIfBEc32(*(CK_OBJECT_CLASS*)pTemplate[i].pValue);

        i++; if(i > ulCount) return CKR_ARGUMENTS_BAD;

        switch(cls)
        {
            case CKO_SECRET_KEY:
                if(pSession->Token->KeyMgmt != NULL && pSession->Token->KeyMgmt->LoadSecretKey != NULL )
                {
                    if(pTemplate[i].type == CKA_KEY_TYPE)
                    {
                        CK_KEY_TYPE type = SwapEndianIfBEc32(*(CK_KEY_TYPE*)pTemplate[i].pValue);

                        i++; if(i > ulCount) return CKR_ARGUMENTS_BAD;
                        
                        if(pTemplate[i].type == CKA_VALUE)
                        {
                            ret = pSession->Token->KeyMgmt->LoadSecretKey(&pSession->Context, type, (const UINT8*)pTemplate[i].pValue, pTemplate[i].ulValueLen, phObject);
                            
#ifdef _DEBUG
                            if(ret == CKR_OK)
                            {
                                pSession->AddObjectHandle(*phObject);
                            }
#endif
                            
                            return ret;
                        }
                    }
                }
                break;
            case CKO_PRIVATE_KEY:
            case CKO_PUBLIC_KEY:
                if(pSession->Token->KeyMgmt != NULL)
                {
                    if(pTemplate[i].type == CKA_KEY_TYPE)
                    {
                        CK_KEY_TYPE type = SwapEndianIfBEc32(*(CK_KEY_TYPE*)pTemplate[i].pValue);

                        bool bPrivate = (cls == CKO_PRIVATE_KEY);
                        
                        if(type == CKK_DSA && pSession->Token->KeyMgmt->LoadDsaKey != NULL)
                        {
                            DsaKeyData dsaKeyData;

                            memset(&dsaKeyData, 0, sizeof(dsaKeyData));
                           
                            i++;

                            for(;i<ulCount; i++)
                            {
                                switch(pTemplate[i].type)
                                {
                                    case CKA_PRIME:
                                        dsaKeyData.Prime_p        = (PBYTE)pTemplate[i].pValue;
                                        dsaKeyData.Prime_p_len    = pTemplate[i].ulValueLen;
                                        break;
 
                                    case CKA_SUBPRIME:
                                        dsaKeyData.Subprime_q     = (PBYTE)pTemplate[i].pValue;
                                        dsaKeyData.Subprime_q_len = pTemplate[i].ulValueLen;
                                        break;
                                        
                                    case CKA_BASE:
                                        dsaKeyData.Base_g         = (PBYTE)pTemplate[i].pValue;
                                        dsaKeyData.Base_g_len     = pTemplate[i].ulValueLen;
                                        break;
 
                                     case CKA_PRIVATE_EXPONENT:
                                         dsaKeyData.Private_x     = (PBYTE)pTemplate[i].pValue;
                                         dsaKeyData.Private_x_len = pTemplate[i].ulValueLen;
                                         break;
                                         
                                    case CKA_VALUE:
                                        if(bPrivate)
                                        {
                                            dsaKeyData.Private_x     = (PBYTE)pTemplate[i].pValue;
                                            dsaKeyData.Private_x_len = pTemplate[i].ulValueLen;
                                        }
                                        else
                                        {
                                            dsaKeyData.Public_y      = (PBYTE)pTemplate[i].pValue;
                                            dsaKeyData.Public_y_len  = pTemplate[i].ulValueLen;
                                        }
                                        break;

                                    // public and private in same key
                                    case CKA_PUBLIC_EXPONENT:
                                        if(bPrivate)
                                        {
                                            dsaKeyData.Public_y      = (PBYTE)pTemplate[i].pValue;
                                            dsaKeyData.Public_y_len  = pTemplate[i].ulValueLen;
                                        }
                                        break;
                                }
                            }

                            dsaKeyData.Seed_len = dsaKeyData.Base_g_len;

                            ret = pSession->Token->KeyMgmt->LoadDsaKey(&pSession->Context, dsaKeyData, bPrivate, phObject);
                            
#ifdef _DEBUG
                            if(ret == CKR_OK)
                            {
                                pSession->AddObjectHandle(*phObject);
                            }
#endif

                            return ret;
                        }
                        else if(type == CKK_RSA && pSession->Token->KeyMgmt->LoadRsaKey != NULL)
                        {
                            PBYTE    pRawData = NULL;
                            CK_ULONG rawDataLen = 0;
                            RsaKeyData pParams;

                            memset(&pParams, 0, sizeof(pParams));
                            
                            i++;

                            for(;i<ulCount; i++)
                            {
                                switch(pTemplate[i].type)
                                {
                                    case CKA_MODULUS:
                                        pParams.Modulus             = (PBYTE)pTemplate[i].pValue;
                                        pParams.Modulus_len         = pTemplate[i].ulValueLen;
                                        break;

                                    case CKA_PUBLIC_EXPONENT:
                                        pParams.PublicExponent      = (PBYTE)pTemplate[i].pValue;
                                        pParams.PublicExponent_len  = pTemplate[i].ulValueLen;
                                        break;
                                        
                                    case CKA_PRIVATE_EXPONENT:
                                        pParams.PrivateExponent     = (PBYTE)pTemplate[i].pValue;
                                        pParams.PrivateExponent_len = pTemplate[i].ulValueLen;
                                        break;

                                    case CKA_PRIME_1:
                                        pParams.Prime1          = (PBYTE)pTemplate[i].pValue;
                                        pParams.Prime1_len      = pTemplate[i].ulValueLen;
                                        break;
                                    case CKA_PRIME_2:
                                        pParams.Prime2          = (PBYTE)pTemplate[i].pValue;
                                        pParams.Prime2_len      = pTemplate[i].ulValueLen;
                                        break;
                                    case CKA_EXPONENT_1:
                                        pParams.Exponent1       = (PBYTE)pTemplate[i].pValue;
                                        pParams.Exponent1_len   = pTemplate[i].ulValueLen;
                                        break;
                                    case CKA_EXPONENT_2:
                                        pParams.Exponent2       = (PBYTE)pTemplate[i].pValue;
                                        pParams.Exponent2_len   = pTemplate[i].ulValueLen;
                                        break;
                                    case CKA_COEFFICIENT:
                                        pParams.Coefficient     = (PBYTE)pTemplate[i].pValue;
                                        pParams.Coefficient_len = pTemplate[i].ulValueLen;
                                        break;                                        

                                    case CKA_VALUE:
                                        pRawData                = (PBYTE)pTemplate[i].pValue;
                                        rawDataLen              = pTemplate[i].ulValueLen;
                                        break;
                                }
                            }

                            if(pRawData != NULL && rawDataLen > 0)
                            {
                                ret = pSession->Token->KeyMgmt->LoadKeyBlob(&pSession->Context, pRawData, rawDataLen, CKK_RSA, Public, phObject);
                            }
                            else
                            {
                                ret = pSession->Token->KeyMgmt->LoadRsaKey(&pSession->Context, pParams, bPrivate, phObject);
                            }
                            
#ifdef _DEBUG
                            if(ret == CKR_OK)
                            {
                                pSession->AddObjectHandle(*phObject);
                            }
#endif
                            
                            return ret;
                        }
                    }
                }
                break;
        }
    }

    // give the drivers object management a chance to handle this
    if(pSession->Token->ObjectMgmt != NULL && pSession->Token->ObjectMgmt->CreateObject != NULL)
    {
        ret = pSession->Token->ObjectMgmt->CreateObject(&pSession->Context, pTemplate, ulCount, phObject);
        
#ifdef _DEBUG
        if(ret == CKR_OK)
        {
            pSession->AddObjectHandle(*phObject);
        }
#endif
        
        return ret;
    }
    
    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_CopyObject copies an object, creating a new object for the
 * copy. */
CK_DEFINE_FUNCTION(CK_RV, C_CopyObject)
(
  CK_SESSION_HANDLE    hSession,    /* the session's handle */
  CK_OBJECT_HANDLE     hObject,     /* the object's handle */
  CK_ATTRIBUTE_PTR     pTemplate,   /* template for new object */
  CK_ULONG             ulCount,     /* attributes in template */
  CK_OBJECT_HANDLE_PTR phNewObject  /* receives handle of copy */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret         != CKR_OK) return ret;
    if(pTemplate   == NULL  ) return CKR_ARGUMENTS_BAD;
    if(phNewObject == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->ObjectMgmt != NULL && pSession->Token->ObjectMgmt->CopyObject)
    {
        ret = pSession->Token->ObjectMgmt->CopyObject(&pSession->Context, hObject, pTemplate, ulCount, phNewObject);
        
#ifdef _DEBUG
        if(ret == CKR_OK)
        {
            pSession->AddObjectHandle(*phNewObject);
        }
#endif

        return ret;
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}

/* C_DestroyObject destroys an object. */
CK_DEFINE_FUNCTION(CK_RV, C_DestroyObject)
(
  CK_SESSION_HANDLE hSession,  /* the session's handle */
  CK_OBJECT_HANDLE  hObject    /* the object's handle */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret != CKR_OK) return ret;

    if(pSession->Token->ObjectMgmt != NULL && pSession->Token->ObjectMgmt->DestroyObject)
    {
        ret = pSession->Token->ObjectMgmt->DestroyObject(&pSession->Context, hObject);

#ifdef _DEBUG
        pSession->RemoveObjectHandle(hObject);
#endif        
        
        return ret;
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}

/* C_GetObjectSize gets the size of an object in bytes. */
CK_DEFINE_FUNCTION(CK_RV, C_GetObjectSize)
(
  CK_SESSION_HANDLE hSession,  /* the session's handle */
  CK_OBJECT_HANDLE  hObject,   /* the object's handle */
  CK_ULONG_PTR      pulSize    /* receives size of object */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret     != CKR_OK) return ret;
    if(pulSize == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->ObjectMgmt != NULL && pSession->Token->ObjectMgmt->GetObjectSize)
    {
        return pSession->Token->ObjectMgmt->GetObjectSize(&pSession->Context, hObject, pulSize);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}

/* C_GetAttributeValue obtains the value of one or more object
 * attributes. */
CK_DEFINE_FUNCTION(CK_RV, C_GetAttributeValue)
(
  CK_SESSION_HANDLE hSession,   /* the session's handle */
  CK_OBJECT_HANDLE  hObject,    /* the object's handle */
  CK_ATTRIBUTE_PTR  pTemplate,  /* specifies attrs; gets vals */
  CK_ULONG          ulCount     /* attributes in template */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret       != CKR_OK) return ret;
    if(pTemplate == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->ObjectMgmt != NULL && pSession->Token->ObjectMgmt->GetAttributeValue)
    {
        return pSession->Token->ObjectMgmt->GetAttributeValue(&pSession->Context, hObject, pTemplate, ulCount);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}

/* C_SetAttributeValue modifies the value of one or more object
 * attributes */
CK_DEFINE_FUNCTION(CK_RV, C_SetAttributeValue)
(
  CK_SESSION_HANDLE hSession,   /* the session's handle */
  CK_OBJECT_HANDLE  hObject,    /* the object's handle */
  CK_ATTRIBUTE_PTR  pTemplate,  /* specifies attrs and values */
  CK_ULONG          ulCount     /* attributes in template */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret       != CKR_OK) return ret;
    if(pTemplate == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->ObjectMgmt != NULL && pSession->Token->ObjectMgmt->SetAttributeValue)
    {
        return pSession->Token->ObjectMgmt->SetAttributeValue(&pSession->Context, hObject, pTemplate, ulCount);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}

/* C_FindObjectsInit initializes a search for token and session
 * objects that match a template. */
CK_DEFINE_FUNCTION(CK_RV, C_FindObjectsInit)
(
  CK_SESSION_HANDLE hSession,   /* the session's handle */
  CK_ATTRIBUTE_PTR  pTemplate,  /* attribute values to match */
  CK_ULONG          ulCount     /* attrs in search template */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret       != CKR_OK) return ret;
    if(pTemplate == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->ObjectMgmt != NULL && pSession->Token->ObjectMgmt->FindObjectsInit)
    {
        return pSession->Token->ObjectMgmt->FindObjectsInit(&pSession->Context, pTemplate, ulCount);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}


/* C_FindObjects continues a search for token and session
 * objects that match a template, obtaining additional object
 * handles. */
CK_DEFINE_FUNCTION(CK_RV, C_FindObjects)
(
 CK_SESSION_HANDLE    hSession,          /* session's handle */
 CK_OBJECT_HANDLE_PTR phObject,          /* gets obj. handles */
 CK_ULONG             ulMaxObjectCount,  /* max handles to get */
 CK_ULONG_PTR         pulObjectCount     /* actual # returned */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret            != CKR_OK) return ret;
    if(pulObjectCount == NULL  ) return CKR_ARGUMENTS_BAD;

    if(pSession->Token->ObjectMgmt != NULL && pSession->Token->ObjectMgmt->FindObjects)
    {
        return pSession->Token->ObjectMgmt->FindObjects(&pSession->Context, phObject, ulMaxObjectCount, pulObjectCount);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}

/* C_FindObjectsFinal finishes a search for token and session
 * objects. */
CK_DEFINE_FUNCTION(CK_RV, C_FindObjectsFinal)
(
  CK_SESSION_HANDLE hSession  /* the session's handle */
)
{
    CK_RV ret;
    CK_SLOT_ID slotID;
    CryptokiSession* pSession;

    ret = Cryptoki_GetSlotIDFromSession(hSession, &slotID, &pSession);

    if(ret != CKR_OK) return ret;

    if(pSession->Token->ObjectMgmt != NULL && pSession->Token->ObjectMgmt->FindObjectsFinal)
    {
        return pSession->Token->ObjectMgmt->FindObjectsFinal(&pSession->Context);
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}

