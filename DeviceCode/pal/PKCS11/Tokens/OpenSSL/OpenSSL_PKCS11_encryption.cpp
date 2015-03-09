#include "OpenSSL_pkcs11.h"
#include <AES\aes.h>
#include <RSA\rsa.h>
#include <EVP\evp_locl.h>

CK_RV PKCS11_Encryption_OpenSSL::InitHelper(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pEncryptMech, CK_OBJECT_HANDLE hKey, BOOL isEncrypt)
{
    OPENSSL_HEADER();
    
    OpenSSLEncryptData* pEnc;
    const EVP_CIPHER* pCipher;
    int padding = 0;

    if(             pSessionCtx                 == NULL) return CKR_SESSION_CLOSED;
    if( isEncrypt && pSessionCtx->EncryptionCtx != NULL) return CKR_SESSION_PARALLEL_NOT_SUPPORTED;
    if(!isEncrypt && pSessionCtx->DecryptionCtx != NULL) return CKR_SESSION_PARALLEL_NOT_SUPPORTED;
    
    pEnc = (OpenSSLEncryptData*)TINYCLR_SSL_MALLOC(sizeof(*pEnc));

    if(pEnc == NULL) return CKR_DEVICE_MEMORY;

    TINYCLR_SSL_MEMSET(pEnc, 0, sizeof(*pEnc));

    pEnc->Key = PKCS11_Keys_OpenSSL::GetKeyFromHandle(pSessionCtx, hKey, !isEncrypt);

    pEnc->IsSymmetric = TRUE;

    switch(pEncryptMech->mechanism)
    {
        case CKM_AES_CBC:
        case CKM_AES_CBC_PAD:
            switch(pEnc->Key->size)
            {
                case 128:
                    pCipher = EVP_aes_128_cbc();
                    break;
                case 192:
                    pCipher = EVP_aes_192_cbc();
                    break;
                case 256:
                    pCipher = EVP_aes_256_cbc();
                    break;
                default:
                    OPENSSL_SET_AND_LEAVE(CKR_MECHANISM_INVALID);
            }
            if(pEncryptMech->mechanism == CKM_AES_CBC_PAD)
            {
                padding = 1;
            }
            break;

        case CKM_AES_ECB:
        case CKM_AES_ECB_PAD:
            switch(pEnc->Key->size)
            {
                case 128:
                    pCipher = EVP_aes_128_ecb();
                    break;
                case 192:
                    pCipher = EVP_aes_192_ecb();
                    break;
                case 256:
                    pCipher = EVP_aes_256_ecb();
                    break;
                default:
                    OPENSSL_SET_AND_LEAVE(CKR_MECHANISM_INVALID);
            }
            if(pEncryptMech->mechanism == CKM_AES_ECB_PAD)
            {
                padding = 1;
            }
            break;

        case CKM_DES3_CBC:
            pCipher = EVP_des_ede3_cbc();
            break;

        case CKM_DES3_CBC_PAD:
            pCipher = EVP_des_ede3_cbc();
            padding = 1;
            break;

        case CKM_RSA_PKCS:
            pEnc->IsSymmetric= FALSE;
            padding = RSA_PKCS1_PADDING;
            break;

        default:
            OPENSSL_SET_AND_LEAVE(CKR_MECHANISM_INVALID);
    }

    if(pEnc->IsSymmetric)
    {
        if(pEncryptMech->ulParameterLen > 0 && pEncryptMech->ulParameterLen > 0)
        {
            memcpy(pEnc->IV, pEncryptMech->pParameter, pEncryptMech->ulParameterLen);
        }

        pEnc->Key->ctx = &pEnc->SymmetricCtx;

        if(isEncrypt) 
        {
            OPENSSL_CHECKRESULT(EVP_EncryptInit(&pEnc->SymmetricCtx, pCipher, (const UINT8*)pEnc->Key->key, pEnc->IV));
        }
        else
        {
            OPENSSL_CHECKRESULT(EVP_DecryptInit(&pEnc->SymmetricCtx, pCipher, (const UINT8*)pEnc->Key->key, pEnc->IV));
        }

        OPENSSL_CHECKRESULT(EVP_CIPHER_CTX_set_padding(&pEnc->SymmetricCtx, padding));
    }
    else
    {
        pEnc->Key->ctx = EVP_PKEY_CTX_new((EVP_PKEY*)pEnc->Key->key, NULL);

        if(isEncrypt)
        {
            OPENSSL_CHECKRESULT(EVP_PKEY_encrypt_init       ((EVP_PKEY_CTX*)pEnc->Key->ctx         ));
            OPENSSL_CHECKRESULT(EVP_PKEY_CTX_set_rsa_padding((EVP_PKEY_CTX*)pEnc->Key->ctx, padding));
        }
        else
        {
            OPENSSL_CHECKRESULT(EVP_PKEY_decrypt_init       ((EVP_PKEY_CTX*)pEnc->Key->ctx         ));
            OPENSSL_CHECKRESULT(EVP_PKEY_CTX_set_rsa_padding((EVP_PKEY_CTX*)pEnc->Key->ctx, padding));
        }
    }

    if(isEncrypt) pSessionCtx->EncryptionCtx = pEnc;
    else          pSessionCtx->DecryptionCtx = pEnc;

    OPENSSL_CLEANUP();
    if(retVal != CKR_OK && pEnc != NULL)
    {
        TINYCLR_SSL_FREE(pEnc);
    }
    OPENSSL_RETURN();
    
}


CK_RV PKCS11_Encryption_OpenSSL::EncryptInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pEncryptMech, CK_OBJECT_HANDLE hKey)
{
    return InitHelper(pSessionCtx, pEncryptMech, hKey, TRUE);
}

CK_RV PKCS11_Encryption_OpenSSL::Encrypt(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pEncryptedData, CK_ULONG_PTR pulEncryptedDataLen)
{
    OPENSSL_HEADER();

    if(pEncryptedData == NULL)
    {
        OpenSSLEncryptData *pEnc;
        int blockSize;
        int mod;
        
        if(pSessionCtx == NULL || pSessionCtx->EncryptionCtx == NULL) return CKR_SESSION_CLOSED;
        
        pEnc = (OpenSSLEncryptData*)pSessionCtx->EncryptionCtx;

        if(pEnc->IsSymmetric)
        {
            blockSize = EVP_CIPHER_block_size(pEnc->SymmetricCtx.cipher);
        }
        else
        {
            blockSize = EVP_PKEY_size((EVP_PKEY*)pEnc->Key->key);
        }

        mod = ulDataLen % blockSize;
        if(0 != mod)
        {
            *pulEncryptedDataLen = ulDataLen + (blockSize - mod);
        }
        else
        {
            *pulEncryptedDataLen = ulDataLen + blockSize;
        }
        
        return CKR_OK;
    }
    
    CK_ULONG tmp = *pulEncryptedDataLen;

    OPENSSL_CHECK_CK_RESULT(PKCS11_Encryption_OpenSSL::EncryptUpdate(pSessionCtx, pData, ulDataLen, pEncryptedData, pulEncryptedDataLen));

    tmp -= *pulEncryptedDataLen;

    OPENSSL_CHECK_CK_RESULT(PKCS11_Encryption_OpenSSL::EncryptFinal(pSessionCtx, &pEncryptedData[*pulEncryptedDataLen], &tmp));

    *pulEncryptedDataLen += tmp;

    OPENSSL_NOCLEANUP();
}

CK_RV PKCS11_Encryption_OpenSSL::EncryptUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pPart, CK_ULONG ulPartLen, CK_BYTE_PTR pEncryptedPart, CK_ULONG_PTR pulEncryptedPartLen)   
{
    OPENSSL_HEADER();
    OpenSSLEncryptData *pEnc;
    
    if(pSessionCtx == NULL || pSessionCtx->EncryptionCtx == NULL) return CKR_SESSION_CLOSED;

    pEnc = (OpenSSLEncryptData*)pSessionCtx->EncryptionCtx;
    
    if(pEnc->IsSymmetric)
    {
        int outLen = *pulEncryptedPartLen;
        
        OPENSSL_CHECKRESULT(EVP_EncryptUpdate((EVP_CIPHER_CTX*)pEnc->Key->ctx, pEncryptedPart, &outLen, pPart, ulPartLen));
        
        *pulEncryptedPartLen = outLen;
    }
    else
    {
        size_t encLen = *pulEncryptedPartLen;
        
        OPENSSL_CHECKRESULT(EVP_PKEY_encrypt((EVP_PKEY_CTX*)pEnc->Key->ctx, pEncryptedPart, &encLen, pPart, ulPartLen));

        *pulEncryptedPartLen = encLen;
    }

    OPENSSL_CLEANUP();

    if(retVal != CKR_OK)
    {
        TINYCLR_SSL_FREE(pEnc);
        pSessionCtx->EncryptionCtx = NULL;
    }

    OPENSSL_RETURN();
}

CK_RV PKCS11_Encryption_OpenSSL::EncryptFinal(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pLastEncryptedPart, CK_ULONG_PTR pulLastEncryptedPartLen)
{
    OPENSSL_HEADER();
    OpenSSLEncryptData *pEnc;
    
    if(pSessionCtx == NULL || pSessionCtx->EncryptionCtx == NULL) return CKR_SESSION_CLOSED;

    pEnc = (OpenSSLEncryptData*)pSessionCtx->EncryptionCtx;
    
    if(pEnc->IsSymmetric)
    {
        int outLen = *pulLastEncryptedPartLen;
        
        OPENSSL_CHECKRESULT(EVP_EncryptFinal_ex((EVP_CIPHER_CTX*)pEnc->Key->ctx, pLastEncryptedPart, &outLen));
        
        *pulLastEncryptedPartLen = outLen;

        OPENSSL_CHECKRESULT(EVP_CIPHER_CTX_cleanup((EVP_CIPHER_CTX*)pEnc->Key->ctx));
    }
    else
    {
        EVP_PKEY_CTX_free((EVP_PKEY_CTX*)pEnc->Key->ctx);
        
        *pulLastEncryptedPartLen = 0;
    }

    OPENSSL_CLEANUP();

    TINYCLR_SSL_FREE(pEnc);
    pSessionCtx->EncryptionCtx = NULL;

    OPENSSL_RETURN();
}    

CK_RV PKCS11_Encryption_OpenSSL::DecryptInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pDecryptMech, CK_OBJECT_HANDLE hKey)
{
    return InitHelper(pSessionCtx, pDecryptMech, hKey, FALSE);
}

CK_RV PKCS11_Encryption_OpenSSL::Decrypt(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pEncryptedData, CK_ULONG ulEncryptedDataLen, CK_BYTE_PTR pData, CK_ULONG_PTR pulDataLen)
{
    OPENSSL_HEADER();

    if(pData == NULL)
    {
        OpenSSLEncryptData *pDecr;
        int blockSize;
        int mod;
        
        if(pSessionCtx == NULL || pSessionCtx->DecryptionCtx == NULL) return CKR_SESSION_CLOSED;
        
        pDecr = (OpenSSLEncryptData*)pSessionCtx->DecryptionCtx;

        if(pDecr->IsSymmetric)
        {
            blockSize = EVP_CIPHER_block_size(pDecr->SymmetricCtx.cipher);
        }
        else
        {
            blockSize = EVP_PKEY_size((EVP_PKEY*)pDecr->Key->key);
        }

        mod = ulEncryptedDataLen % blockSize;
        if(0 != mod)
        {
            *pulDataLen = ulEncryptedDataLen + (blockSize - mod);
        }
        else
        {
            *pulDataLen = ulEncryptedDataLen + blockSize;
        }
        
        return CKR_OK;
    }
    
    CK_ULONG tmp = *pulDataLen;

    OPENSSL_CHECK_CK_RESULT(PKCS11_Encryption_OpenSSL::DecryptUpdate(pSessionCtx, pEncryptedData, ulEncryptedDataLen, pData, pulDataLen));

    tmp -= *pulDataLen;

    OPENSSL_CHECK_CK_RESULT(PKCS11_Encryption_OpenSSL::DecryptFinal(pSessionCtx, &pData[*pulDataLen], &tmp));

    *pulDataLen += tmp;

    OPENSSL_NOCLEANUP();
}    

CK_RV PKCS11_Encryption_OpenSSL::DecryptUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pEncryptedPart, CK_ULONG ulEncryptedPartLen, CK_BYTE_PTR pPart, CK_ULONG_PTR pulPartLen)
{
    OPENSSL_HEADER();

    OpenSSLEncryptData *pDec;
    
    if(pSessionCtx == NULL || pSessionCtx->DecryptionCtx == NULL) return CKR_SESSION_CLOSED;

    pDec = (OpenSSLEncryptData*)pSessionCtx->DecryptionCtx;

    if(pDec->IsSymmetric)
    {
        int outLen = *pulPartLen;
        
        OPENSSL_CHECKRESULT(EVP_DecryptUpdate((EVP_CIPHER_CTX*)pDec->Key->ctx, pPart, &outLen, pEncryptedPart, ulEncryptedPartLen));
        
        *pulPartLen = outLen;
    }
    else
    {
        size_t outLen = *pulPartLen;
        
        OPENSSL_CHECKRESULT(EVP_PKEY_decrypt((EVP_PKEY_CTX*)pDec->Key->ctx, pPart, &outLen, pEncryptedPart, ulEncryptedPartLen));

        *pulPartLen = outLen;
    }

    OPENSSL_CLEANUP();

    if(retVal != CKR_OK)
    {
        TINYCLR_SSL_FREE(pDec);
        pSessionCtx->DecryptionCtx = NULL;
    }

    OPENSSL_RETURN();
}    

CK_RV PKCS11_Encryption_OpenSSL::DecryptFinal(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pLastPart, CK_ULONG_PTR pulLastPartLen)
{
    OPENSSL_HEADER();

    OpenSSLEncryptData *pDec;
    
    if(pSessionCtx == NULL || pSessionCtx->DecryptionCtx == NULL) return CKR_SESSION_CLOSED;

    pDec = (OpenSSLEncryptData*)pSessionCtx->DecryptionCtx;

    if(pDec->IsSymmetric)
    {
        int outLen = *pulLastPartLen;
        
        OPENSSL_CHECKRESULT(EVP_DecryptFinal((EVP_CIPHER_CTX*)pDec->Key->ctx, pLastPart, &outLen));
        
        *pulLastPartLen = outLen;

        OPENSSL_CHECKRESULT(EVP_CIPHER_CTX_cleanup((EVP_CIPHER_CTX*)pDec->Key->ctx));
    }
    else
    {
        EVP_PKEY_CTX_free((EVP_PKEY_CTX*)pDec->Key->ctx);
        
        *pulLastPartLen = 0;
    }

    OPENSSL_CLEANUP();

    TINYCLR_SSL_FREE(pDec);
    pSessionCtx->DecryptionCtx = NULL;

    OPENSSL_RETURN();
}    


