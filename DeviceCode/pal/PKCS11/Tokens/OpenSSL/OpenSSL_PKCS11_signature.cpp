#include "OpenSSL_pkcs11.h"
#include <AES\aes.h>
#include <RSA\rsa.h>
#include <EVP\evp_locl.h>

// fake digest for performing signing of a hash value
// This occurs when the user just wants to sign an already computed hash,
// all other signatures requests will result in a hash creation then signature.
static int update(EVP_MD_CTX *ctx,const void *data,size_t count)
{ 
    ctx->md_data = (void*)data;
    ctx->flags   = (unsigned long)count << 8;
    
    return 1; 
}

static int init(EVP_MD_CTX *ctx)
{
    ctx->update = update;
    
    return 1; 
}

static int final(EVP_MD_CTX *ctx,unsigned char *md)
{ 
    memcpy(md, ctx->md_data, (size_t)(ctx->flags >> 8) );
    return 1; 
}

static EVP_MD nodigest_md=
{
    NID_sha1,
    NID_undef,
    160/8,
    EVP_MD_FLAG_PKEY_METHOD_SIGNATURE|EVP_MD_FLAG_DIGALGID_ABSENT,
    init,
    update,
    final,
    NULL,
    NULL,
    0,
    0,
    sizeof(EVP_MD *),
};

CK_RV PKCS11_Signature_OpenSSL::GetDigestFromMech(CK_MECHANISM_PTR pMechanism, const EVP_MD*& pDigest, CK_KEY_TYPE &keyType)
{

    if(pMechanism->pParameter && pMechanism->ulParameterLen == sizeof(CK_MECHANISM_TYPE))
    {
        CK_MECHANISM_TYPE hash = SwapEndianIfBEc32(*(CK_MECHANISM_TYPE_PTR)pMechanism->pParameter);
        bool fSignHash = false;

        if(hash & CKA_SIGN_NO_NODIGEST_FLAG)
        {
            hash &= ~CKA_SIGN_NO_NODIGEST_FLAG;
            fSignHash = true;
            pDigest = &nodigest_md;
        }
        
        switch(hash)
        {
            case CKM_SHA_1:
                if(fSignHash)
                {
                    nodigest_md.md_size = 160/8;
                    nodigest_md.type = NID_sha1;
                }
                else pDigest = EVP_sha1();
                break;

            case CKM_SHA224:
                if(fSignHash)
                {
                    nodigest_md.md_size = 224/8;
                    nodigest_md.type = NID_sha224;
                }
                else pDigest= EVP_sha224();
                break;

            case CKM_SHA256:
                if(fSignHash)
                {
                    nodigest_md.md_size = 256/8;
                    nodigest_md.type = NID_sha256;
                }
                else pDigest = EVP_sha256();
                break;

            case CKM_SHA384:
                if(fSignHash)
                {
                    nodigest_md.md_size = 384/8;
                    nodigest_md.type = NID_sha384;
                }
                else pDigest = EVP_sha384();
                break;

            case CKM_SHA512:
                if(fSignHash)
                {
                    nodigest_md.md_size = 512/8;
                    nodigest_md.type = NID_sha512;
                }
                else pDigest = EVP_sha512();
                break;

            case CKM_MD5:
                if(fSignHash)
                {
                    nodigest_md.md_size = 128/8;
                    nodigest_md.type = NID_md5;
                }
                else pDigest = EVP_md5();
                break;

            default:
                return CKR_MECHANISM_INVALID;  
        }
    }
    else 
    {
        pDigest = &nodigest_md;
    }

    switch(pMechanism->mechanism)
    {
        case CKM_RSA_PKCS:
            keyType = CKK_RSA;
            break;

        case CKM_DSA:
            keyType = CKK_DSA;
            break;

        case CKM_ECDSA:
            keyType = CKK_EC;
            break;

        default:
            return CKR_MECHANISM_INVALID;
    }

    return CKR_OK;
}

CK_RV PKCS11_Signature_OpenSSL::InitHelper(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey, BOOL isSign)
{
    OPENSSL_HEADER();
    
    OpenSSLSignatureData* pSig;
    const EVP_MD* digest = NULL;
    CK_KEY_TYPE keyType;

    if(           pSessionCtx            == NULL) return CKR_SESSION_CLOSED;
    if( isSign && pSessionCtx->SignCtx   != NULL) return CKR_SESSION_PARALLEL_NOT_SUPPORTED; // another signature is in progress
    if(!isSign && pSessionCtx->VerifyCtx != NULL) return CKR_SESSION_PARALLEL_NOT_SUPPORTED; // another verify is in progress
    
    pSig = (OpenSSLSignatureData*)TINYCLR_SSL_MALLOC(sizeof(*pSig));

    if(pSig == NULL) return CKR_DEVICE_MEMORY;
    
    TINYCLR_SSL_MEMSET(pSig, 0, sizeof(*pSig));

    OPENSSL_CHECK_CK_RESULT(GetDigestFromMech(pMechanism, digest, keyType));

    pSig->Key = PKCS11_Keys_OpenSSL::GetKeyFromHandle(pSessionCtx, hKey, isSign);

    if(keyType != pSig->Key->type) OPENSSL_SET_AND_LEAVE(CKR_KEY_TYPE_INCONSISTENT);

    if(isSign)
    {
        if(pSig->Key->attrib == Public)
        {
            return CKR_KEY_TYPE_INCONSISTENT;
        }

        OPENSSL_CHECKRESULT(EVP_SignInit  (&pSig->Ctx, digest));
        pSessionCtx->SignCtx = pSig;
    }
    else
    {
        if(pSig->Key->attrib == Private)
        {
            return CKR_KEY_TYPE_INCONSISTENT;
        }

        OPENSSL_CHECKRESULT(EVP_VerifyInit(&pSig->Ctx, digest));
        pSessionCtx->VerifyCtx = pSig;
    }
        
    OPENSSL_CLEANUP();

    if(retVal != CKR_OK && pSig != NULL)
    {
        TINYCLR_SSL_FREE(pSig);
    }

    OPENSSL_RETURN();
}


CK_RV PKCS11_Signature_OpenSSL::SignInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey)
{
    return InitHelper(pSessionCtx, pMechanism, hKey, TRUE);
}

CK_RV PKCS11_Signature_OpenSSL::Sign(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pSignature, CK_ULONG_PTR pulSignatureLen)
{
    OPENSSL_HEADER();

    if(pSignature == NULL)
    {        
        OpenSSLSignatureData* pSig;
        
        if(pSessionCtx == NULL || pSessionCtx->SignCtx == NULL) return CKR_SESSION_CLOSED;
        
        pSig = (OpenSSLSignatureData*)pSessionCtx->SignCtx;
        
        *pulSignatureLen = EVP_PKEY_size((EVP_PKEY*)pSig->Key->key);

        return CKR_OK;
    }

    OPENSSL_CHECK_CK_RESULT(SignUpdate(pSessionCtx, pData     , ulDataLen      ));
    OPENSSL_CHECK_CK_RESULT(SignFinal (pSessionCtx, pSignature, pulSignatureLen));
        
    OPENSSL_NOCLEANUP();
}

CK_RV PKCS11_Signature_OpenSSL::SignUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pPart, CK_ULONG ulPartLen)
{
    OPENSSL_HEADER();

    OpenSSLSignatureData* pSig;
    
    if(pSessionCtx == NULL || pSessionCtx->SignCtx == NULL) return CKR_SESSION_CLOSED;

    pSig = (OpenSSLSignatureData*)pSessionCtx->SignCtx;

    OPENSSL_CHECKRESULT(EVP_SignUpdate(&pSig->Ctx, pPart, ulPartLen));

    OPENSSL_CLEANUP();

    if(retVal != CKR_OK)
    {
        TINYCLR_SSL_FREE(pSig);
        pSessionCtx->SignCtx = NULL;
    }
    
    OPENSSL_RETURN();
}

CK_RV PKCS11_Signature_OpenSSL::SignFinal(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pSignature, CK_ULONG_PTR pulSignatureLen)
{
    OPENSSL_HEADER();

    OpenSSLSignatureData* pSig;
    UINT32                sigLen = *pulSignatureLen;
    
    if(pSessionCtx == NULL || pSessionCtx->SignCtx == NULL) return CKR_SESSION_CLOSED;

    pSig = (OpenSSLSignatureData*)pSessionCtx->SignCtx;

    OPENSSL_CHECKRESULT(EVP_SignFinal(&pSig->Ctx, pSignature, &sigLen, (EVP_PKEY *)pSig->Key->key));

    *pulSignatureLen = sigLen;

    OPENSSL_CLEANUP();

    TINYCLR_SSL_FREE(pSig);
    pSessionCtx->SignCtx = NULL;

    OPENSSL_RETURN();
}

CK_RV PKCS11_Signature_OpenSSL::VerifyInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey)
{
    return InitHelper(pSessionCtx, pMechanism, hKey, FALSE);
}

CK_RV PKCS11_Signature_OpenSSL::Verify(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pSignature, CK_ULONG ulSignatureLen)
{
    OPENSSL_HEADER();

    OPENSSL_CHECK_CK_RESULT(VerifyUpdate(pSessionCtx, pData     , ulDataLen     ));
    OPENSSL_CHECK_CK_RESULT(VerifyFinal (pSessionCtx, pSignature, ulSignatureLen));
        
    OPENSSL_NOCLEANUP();
}

CK_RV PKCS11_Signature_OpenSSL::VerifyUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pPart, CK_ULONG ulPartLen)
{
    OPENSSL_HEADER();

    OpenSSLSignatureData* pVer;
    
    if(pSessionCtx == NULL || pSessionCtx->VerifyCtx == NULL) return CKR_SESSION_CLOSED;

    pVer = (OpenSSLSignatureData*)pSessionCtx->VerifyCtx;

    OPENSSL_CHECKRESULT(EVP_VerifyUpdate(&pVer->Ctx, pPart, ulPartLen));

    OPENSSL_CLEANUP();

    if(retVal != CKR_OK)
    {
        TINYCLR_SSL_FREE(pVer);
        pSessionCtx->VerifyCtx = NULL;
    }
    
    OPENSSL_RETURN();
}

CK_RV PKCS11_Signature_OpenSSL::VerifyFinal(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pSignature, CK_ULONG ulSignatureLen)
{
    OPENSSL_HEADER();
    
    OpenSSLSignatureData* pVer;
    
    if(pSessionCtx == NULL || pSessionCtx->VerifyCtx == NULL) return CKR_SESSION_CLOSED;
    
    pVer = (OpenSSLSignatureData*)pSessionCtx->VerifyCtx;
    
    OPENSSL_CHECKRESULT(EVP_VerifyFinal(&pVer->Ctx, pSignature, ulSignatureLen, (EVP_PKEY *)pVer->Key->key));
    
    OPENSSL_CLEANUP();
    
    TINYCLR_SSL_FREE(pVer);
    pSessionCtx->VerifyCtx = NULL;
    
    OPENSSL_RETURN();
}


