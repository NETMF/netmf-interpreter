#include "OpenSSL_pkcs11.h"

CK_RV PKCS11_Digest_OpenSSL::DigestInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism)
{
    OPENSSL_HEADER();
    
    OpenSSLDigestData* pDigData;
    const EVP_MD*      pDigest;
    CK_OBJECT_HANDLE   hKey   = CK_OBJECT_HANDLE_INVALID;
    bool               isHMAC = false;

    if(pSessionCtx            == NULL) return CKR_SESSION_CLOSED;
    if(pSessionCtx->DigestCtx != NULL) return CKR_SESSION_PARALLEL_NOT_SUPPORTED; // another digest is in progress
    
    pDigData = (OpenSSLDigestData*)TINYCLR_SSL_MALLOC(sizeof(*pDigData));

    if(pDigData == NULL) return CKR_DEVICE_MEMORY;

    TINYCLR_SSL_MEMSET(pDigData, 0, sizeof(*pDigData));
    
    EVP_MD_CTX_init(&pDigData->CurrentCtx);
    
    switch(pMechanism->mechanism)
    {
        case CKM_SHA_1:
            pDigest = EVP_sha1();
            break;
        case CKM_SHA224:
            pDigest = EVP_sha224();
            break;
        case CKM_SHA256:
            pDigest = EVP_sha256();
            break;
        case CKM_SHA384:
            pDigest = EVP_sha384();
            break;
        case CKM_SHA512:
            pDigest = EVP_sha512();
            break;

        case CKM_MD5:
            pDigest = EVP_md5();
            break;

        case CKM_RIPEMD160:
            pDigest = EVP_ripemd160();
            break;

        case CKM_MD5_HMAC:
            pDigest = EVP_md5();
            isHMAC = true;
            break;

        case CKM_SHA_1_HMAC:
            pDigest = EVP_sha1();
            isHMAC = true;
            break;

        case CKM_SHA256_HMAC:
            pDigest = EVP_sha256();
            isHMAC = true;
            break;

        case CKM_SHA384_HMAC:
            pDigest = EVP_sha384();
            isHMAC = true;
            break;

        case CKM_SHA512_HMAC:
            pDigest = EVP_sha512();
            isHMAC = true;
            break;

        case CKM_RIPEMD160_HMAC:
            pDigest = EVP_ripemd160();
            isHMAC = true;
            break;
            

        default:
            OPENSSL_SET_AND_LEAVE(CKR_MECHANISM_INVALID);
    }


    if(isHMAC)
    {
        if(pMechanism->pParameter != NULL && pMechanism->ulParameterLen == sizeof(CK_OBJECT_HANDLE))
        {
            hKey = SwapEndianIfBEc32(*(CK_OBJECT_HANDLE*)pMechanism->pParameter);
        }
        else 
        {
            OPENSSL_SET_AND_LEAVE(CKR_MECHANISM_PARAM_INVALID);
        }

        pDigData->HmacKey = PKCS11_Keys_OpenSSL::GetKeyFromHandle(pSessionCtx, hKey, TRUE);

        if(pDigData->HmacKey==NULL) OPENSSL_SET_AND_LEAVE(CKR_MECHANISM_PARAM_INVALID);

        pDigData->HmacCtx.md = pDigest;

        OPENSSL_CHECKRESULT(HMAC_Init(&pDigData->HmacCtx, pDigData->HmacKey->key, pDigData->HmacKey->size/8, pDigData->HmacCtx.md));
    }
    else
    {
        OPENSSL_CHECKRESULT(EVP_DigestInit_ex(&pDigData->CurrentCtx, pDigest, NULL));
    }
    
    pSessionCtx->DigestCtx = pDigData;
    
    OPENSSL_CLEANUP();
    if(retVal != CKR_OK && pDigData != NULL)
    {
        TINYCLR_SSL_FREE(pDigData);
    }
    OPENSSL_RETURN();
}

CK_RV PKCS11_Digest_OpenSSL::Digest(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pDigest, CK_ULONG_PTR pulDigestLen)
{
    OPENSSL_HEADER();
    
    UINT32             digestLen = *pulDigestLen;
    OpenSSLDigestData* pDigData;

    if(pSessionCtx == NULL || pSessionCtx->DigestCtx == NULL) return CKR_SESSION_CLOSED;

    pDigData = (OpenSSLDigestData*)pSessionCtx->DigestCtx;

    if(pDigData->HmacKey != NULL)
    {
        OPENSSL_CHECKRESULT(HMAC_Update(&pDigData->HmacCtx, pData  , ulDataLen ));
        OPENSSL_CHECKRESULT(HMAC_Final (&pDigData->HmacCtx, pDigest, &digestLen));
    }
    else
    {
        OPENSSL_CHECKRESULT(EVP_Digest(pData, ulDataLen, pDigest, &digestLen, pDigData->CurrentCtx.digest, NULL));
    }

    *pulDigestLen = digestLen;

    OPENSSL_CLEANUP();
    
    TINYCLR_SSL_FREE(pDigData);
    pSessionCtx->DigestCtx = NULL;
    
    OPENSSL_RETURN();
}

CK_RV PKCS11_Digest_OpenSSL::Update(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen)
{
    OPENSSL_HEADER();
    
    OpenSSLDigestData* pDigest;

    if(pSessionCtx == NULL || pSessionCtx->DigestCtx == NULL) return CKR_SESSION_CLOSED;

    pDigest = (OpenSSLDigestData*)pSessionCtx->DigestCtx;

    if(pDigest->HmacKey != NULL)
    {
        OPENSSL_CHECKRESULT(HMAC_Update(&pDigest->HmacCtx, pData, ulDataLen ));
    }
    else
    {
        OPENSSL_CHECKRESULT(EVP_DigestUpdate(&pDigest->CurrentCtx , pData, ulDataLen));
    }

    OPENSSL_CLEANUP();

    if(retVal != CKR_OK)
    {
        TINYCLR_SSL_FREE(pDigest);
        pSessionCtx->DigestCtx = NULL;
    }
    
    OPENSSL_RETURN();
}

CK_RV PKCS11_Digest_OpenSSL::DigestKey(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hKey)
{
    // TODO: Should we support this simply for PKCS11?
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Digest_OpenSSL::Final(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pDigest, CK_ULONG_PTR pulDigestLen)
{
    OPENSSL_HEADER();
    
    OpenSSLDigestData* pDigData;
    UINT32             digestLen = *pulDigestLen;

    if(pSessionCtx == NULL || pSessionCtx->DigestCtx == NULL) return CKR_SESSION_CLOSED;

    pDigData = (OpenSSLDigestData*)pSessionCtx->DigestCtx;

    if(pDigData->HmacKey != NULL)
    {
        OPENSSL_CHECKRESULT(HMAC_Final (&pDigData->HmacCtx, pDigest, &digestLen));
    }
    else
    {
        OPENSSL_CHECKRESULT(EVP_DigestFinal(&pDigData->CurrentCtx, pDigest, &digestLen));
    }

    *pulDigestLen = digestLen;

    OPENSSL_CLEANUP();
    
    TINYCLR_SSL_FREE(pDigData);
    pSessionCtx->DigestCtx = NULL;
    
    OPENSSL_RETURN();
}

