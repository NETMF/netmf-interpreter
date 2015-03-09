#include "NetMFCrypto.h"

RSAKey  NetMFCrypto::s_RSAKeys[10];
int     NetMFCrypto::s_RSAKeyIndex = 0;
RSAKey* NetMFCrypto::s_pActiveKey  = NULL;



CK_RV NetMFCrypto::GetObjectSize(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ULONG_PTR pulSize)
{
    *pulSize = RSA_KEY_SIZE_BITS;

    return CKR_OK;
}


CK_RV NetMFCrypto::GenerateKeyPair(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, 
                             CK_ATTRIBUTE_PTR pPublicKeyTemplate , CK_ULONG ulPublicCount, 
                             CK_ATTRIBUTE_PTR pPrivateKeyTemplate, CK_ULONG ulPrivateCount, 
                             CK_OBJECT_HANDLE_PTR phPublicKey    , CK_OBJECT_HANDLE_PTR phPrivateKey)
{
    if(pMechanism->mechanism != CKM_RSA_PKCS_KEY_PAIR_GEN) return CKR_MECHANISM_INVALID;

    KeySeed ks;
    UINT16  d0;
    UINT16  d1;

    UINT8 last = 0;

    for(int i=0; i<SEED_SIZE_BYTES; i++)
    {
        last = (HAL_Time_CurrentTicks() % 256) ^ (last<<3);
        ks.Seed[i] = last;
    }

    if(Crypto_CreateZenithKey((BYTE *)&ks, &d0, &d1) != CRYPTO_SUCCESS)
    {
        return false;
    }
    
    ks.delta[0] = d0;
    ks.delta[1] = d1;

    RSAKey* pPrivate = &s_RSAKeys[s_RSAKeyIndex];
    *phPrivateKey = s_RSAKeyIndex++;

    RSAKey* pPublic = &s_RSAKeys[s_RSAKeyIndex];
    *phPublicKey = s_RSAKeyIndex++;

    Crypto_GeneratePrivateKey(&ks, pPrivate);
    
    Crypto_PublicKeyFromPrivate(pPrivate, pPublic);
    
    return CKR_OK;
}

CK_RV NetMFCrypto::SignInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey)
{
    //if(pMechanism->mechanism != CKM_SHA1_RSA_PKCS) return CKR_MECHANISM_INVALID;
    if(s_pActiveKey != NULL) return CKR_FUNCTION_NOT_PARALLEL;

    s_pActiveKey = &s_RSAKeys[hKey];

    return CKR_OK;
}

CK_RV NetMFCrypto::Sign(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pSignature, CK_ULONG_PTR pulSignatureLen)
{
    void* pHandle = NULL;

    CRYPTO_RESULT cr = ::Crypto_SignBuffer( pData, ulDataLen, s_pActiveKey, pSignature, *pulSignatureLen );

    s_pActiveKey = NULL;

    return (cr == CRYPTO_SUCCESS ? CKR_OK : CKR_FUNCTION_FAILED);
}

CK_RV NetMFCrypto::VerifyInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey)
{
    //if(pMechanism->mechanism != CKM_SHA1_RSA_PKCS) return CKR_MECHANISM_INVALID;
    if(s_pActiveKey != NULL) return CKR_FUNCTION_NOT_PARALLEL;

    s_pActiveKey = &s_RSAKeys[hKey];

    return CKR_OK;
}

CK_RV NetMFCrypto::Verify(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pSignature, CK_ULONG ulSignatureLen)
{
    void* pHandle;
    
    CRYPTO_RESULT cr = ::Crypto_StartRSAOperationWithKey( RSA_VERIFYSIGNATURE, s_pActiveKey, pData, ulDataLen, pSignature, ulSignatureLen, &pHandle );

    s_pActiveKey = NULL;

    while(cr == CRYPTO_CONTINUE)
    {
        cr = ::Crypto_StepRSAOperation(&pHandle);
    }

    if(cr != CRYPTO_SUCCESS) return CKR_SIGNATURE_INVALID;

    return CKR_OK;
}


CK_RV NetMFCrypto::EncryptInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pEncryptMech, CK_OBJECT_HANDLE hKey)
{
    if(pEncryptMech->mechanism != CKM_RSA_PKCS) return CKR_MECHANISM_INVALID;
    if(s_pActiveKey != NULL) return CKR_FUNCTION_NOT_PARALLEL;

    s_pActiveKey = &s_RSAKeys[hKey];

    return CKR_OK;
}


CK_RV NetMFCrypto::Encrypt(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pEncryptedData, CK_ULONG_PTR pulEncryptedDataLen)
{
    void* pHandle = NULL;
    
    CRYPTO_RESULT cr = ::Crypto_StartRSAOperationWithKey( RSA_ENCRYPT, s_pActiveKey, pData, ulDataLen, pEncryptedData, *pulEncryptedDataLen, &pHandle );

    s_pActiveKey = NULL;

    while(cr == CRYPTO_CONTINUE)
    {
        cr = ::Crypto_StepRSAOperation(&pHandle);
    }

    if(cr != CRYPTO_SUCCESS) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}

CK_RV NetMFCrypto::DecryptInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pDecryptMech, CK_OBJECT_HANDLE hKey)
{
    if(pDecryptMech->mechanism != CKM_RSA_PKCS) return CKR_MECHANISM_INVALID;
    if(s_pActiveKey != NULL) return CKR_FUNCTION_NOT_PARALLEL;

    s_pActiveKey = &s_RSAKeys[hKey];

    return CKR_OK;
}


CK_RV NetMFCrypto::Decrypt(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pEncryptedData, CK_ULONG ulEncryptedDataLen, CK_BYTE_PTR pData, CK_ULONG_PTR pulDataLen)
{
    void* pHandle = NULL;
    
    CRYPTO_RESULT cr = ::Crypto_StartRSAOperationWithKey( RSA_DECRYPT, s_pActiveKey, pEncryptedData, ulEncryptedDataLen, pData, *pulDataLen, &pHandle );

    s_pActiveKey = NULL;

    while(cr == CRYPTO_CONTINUE)
    {
        cr = ::Crypto_StepRSAOperation(&pHandle);
    }

    if(cr != CRYPTO_SUCCESS) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}

