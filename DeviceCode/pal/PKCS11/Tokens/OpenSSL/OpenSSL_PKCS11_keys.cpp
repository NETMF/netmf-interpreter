#include "OpenSSL_pkcs11.h"
#include <AES\aes.h>
#include <RAND\rand.h>
#include <RSA\rsa.h>
#include <DSA\dsa.h>
#include <EVP\evp_locl.h>
#include <PEM\pem.h>
#include <EC\EC.h>

KEY_DATA* PKCS11_Keys_OpenSSL::GetKeyFromHandle(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hKey, BOOL getPrivate)
{
    OBJECT_DATA* pObj = PKCS11_Objects_OpenSSL::GetObjectFromHandle(pSessionCtx, hKey);

    if(pObj == NULL) return NULL;
        
    if(pObj->Type == KeyType)
    {
        return (KEY_DATA*)pObj->Data;
    }
    else if(pObj->Type == CertificateType)
    {
        CERT_DATA* pCert = (CERT_DATA*)pObj->Data;

        return getPrivate ? &pCert->privKeyData : &pCert->pubKeyData;
    }

    return NULL;
}

CK_RV PKCS11_Keys_OpenSSL::DeleteKey(Cryptoki_Session_Context* pSessionCtx, KEY_DATA* pKey)
{
    if(pKey == NULL) return CKR_OBJECT_HANDLE_INVALID;
    
    switch(pKey->attrib)
    {
        case Secret:
            break;

        default:
            EVP_PKEY_free((EVP_PKEY*)pKey->key);
            break;
    }

    return CKR_OK;
}

CK_RV PKCS11_Keys_OpenSSL::LoadKeyBlob(Cryptoki_Session_Context* pSessionCtx, const PBYTE pKey, CK_ULONG keyLen, CK_KEY_TYPE keyType, KEY_ATTRIB keyAttrib, CK_OBJECT_HANDLE_PTR phKey )
{
    const UINT8* pBlob = pKey;
    EVP_PKEY* pEvpKey = NULL;

    if(keyAttrib & Private)
    {
        pEvpKey = b2i_PrivateKey(&pBlob, keyLen);
    }
    else if(keyAttrib == Public)
    {
        pEvpKey = b2i_PublicKey(&pBlob, keyLen);
    }

    if(pEvpKey != NULL)
    {
        *phKey = LoadKey( pSessionCtx, pEvpKey, keyType, keyAttrib, EVP_PKEY_bits(pEvpKey) );
    }
    else
    {
        *phKey = LoadKey( pSessionCtx, pKey, keyType, keyAttrib, keyLen );
    }

    return *phKey == CK_OBJECT_HANDLE_INVALID ? CKR_FUNCTION_FAILED : CKR_OK;
}


CK_OBJECT_HANDLE PKCS11_Keys_OpenSSL::LoadKey(Cryptoki_Session_Context* pSessionCtx, void* pKey, CK_KEY_TYPE type, KEY_ATTRIB attrib, size_t keySize)
{
    CK_OBJECT_HANDLE retVal;
    OBJECT_DATA* pData = NULL;
    KEY_DATA* pKeyData = NULL;
    BOOL fCopyKey = false;

    switch(attrib)
    {
        case Secret:
            retVal = PKCS11_Objects_OpenSSL::AllocObject(pSessionCtx, KeyType, (keySize+7)/8 + sizeof(KEY_DATA), &pData);
            fCopyKey = true;
            break;
            
        default:
            retVal = PKCS11_Objects_OpenSSL::AllocObject(pSessionCtx, KeyType, sizeof(KEY_DATA), &pData);
            break;
    }

    if(retVal != CK_OBJECT_HANDLE_INVALID)
    {
        pKeyData = (KEY_DATA*)pData->Data;
        
        if(fCopyKey)
        {
            memcpy( &pKeyData[1], pKey, (keySize+7)/8 );
            pKeyData->key = &pKeyData[1];
        }
        else
        {
            pKeyData->key = pKey;
        }

        pKeyData->type   = type;
        pKeyData->size   = keySize;
        pKeyData->attrib = attrib;
        pKeyData->ctx    = NULL;
    }
    
    return retVal;
}

CK_RV PKCS11_Keys_OpenSSL::GenerateKey(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phKey)
{
    switch(pMechanism->mechanism)
    {
        case CKM_DES3_KEY_GEN:
        case CKM_AES_KEY_GEN:
        case CKM_GENERIC_SECRET_KEY_GEN:
            if(pTemplate != NULL && pTemplate->type == CKA_VALUE_LEN)
            {
                int len = SwapEndianIfBEc32(*(int*)pTemplate->pValue);
                OBJECT_DATA* pData = NULL;
                KEY_DATA* pKey = NULL;

                *phKey = PKCS11_Objects_OpenSSL::AllocObject(pSessionCtx, KeyType, (len+7)/8 + sizeof(KEY_DATA), &pData);

                if(*phKey == CK_OBJECT_HANDLE_INVALID) return CKR_DEVICE_MEMORY;

                pKey = (KEY_DATA*)pData->Data;

                RAND_bytes((UINT8*)&pKey[1], len/8);

                switch(pMechanism->mechanism)
                {
                    case CKM_AES_KEY_GEN:
                        pKey->type = CKK_AES;
                        break;
                    case CKM_DES3_KEY_GEN:
                        pKey->type = CKK_DES3;                        
                        break;
                    case CKM_GENERIC_SECRET_KEY_GEN:
                        pKey->type = CKK_GENERIC_SECRET;                        
                        break;
                }
                pKey->size = len;
                pKey->key = (void*)&pKey[1];
                pKey->ctx = NULL;
                pKey->attrib = Secret;
            }
            else
            {
                return CKR_TEMPLATE_INCOMPLETE;
            }
            break;

        default:
            return CKR_MECHANISM_INVALID;
    }
    
    return CKR_OK;
}

CK_RV PKCS11_Keys_OpenSSL::GenerateKeyPair(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, 
                         CK_ATTRIBUTE_PTR pPublicKeyTemplate , CK_ULONG ulPublicCount, 
                         CK_ATTRIBUTE_PTR pPrivateKeyTemplate, CK_ULONG ulPrivateCount, 
                         CK_OBJECT_HANDLE_PTR phPublicKey    , CK_OBJECT_HANDLE_PTR phPrivateKey)
{
    OPENSSL_HEADER();
    EVP_PKEY_CTX* ctx   = NULL;
    EC_GROUP*     group = NULL;
    OBJECT_DATA*  pData = NULL;
    KEY_DATA*     pKey  = NULL;

    Watchdog_GetSetEnabled( FALSE, TRUE );
    
    switch(pMechanism->mechanism)
    {
        case CKM_EC_KEY_PAIR_GEN:
        case CKM_ECDH_KEY_PAIR_GEN:
            if(pPublicKeyTemplate != NULL && pPublicKeyTemplate->type == CKA_VALUE_LEN)
            {
                int len = SwapEndianIfBEc32(*(int*)pPublicKeyTemplate->pValue);
                
                *phPublicKey = PKCS11_Objects_OpenSSL::AllocObject(pSessionCtx, KeyType, sizeof(KEY_DATA), &pData);
                
                if(*phPublicKey == CK_OBJECT_HANDLE_INVALID) OPENSSL_SET_AND_LEAVE(CKR_DEVICE_MEMORY);
                
                pKey = (KEY_DATA*)pData->Data;
        
                *phPrivateKey = *phPublicKey;


                switch(len)
                {
                    case 256:
                        group = EC_GROUP_new_by_curve_name(NID_X9_62_prime256v1);
                        break;

                    case 384:
                        group = EC_GROUP_new_by_curve_name(NID_secp384r1);
                        break;

                    case 521:
                        group = EC_GROUP_new_by_curve_name(NID_secp521r1);
                        break;
                        
                    default:
                        return CKR_KEY_SIZE_RANGE;
                }
                        
                   
                if(NULL == (ctx = EVP_PKEY_CTX_new_id(EVP_PKEY_EC, NULL))) OPENSSL_SET_AND_LEAVE(CKR_FUNCTION_FAILED); 

                ctx->pkey = EVP_PKEY_new();

                OPENSSL_CHECKRESULT(EVP_PKEY_keygen_init(ctx));

                OPENSSL_CHECKRESULT(EVP_PKEY_set1_EC_KEY(ctx->pkey, EC_KEY_new()));

                OPENSSL_CHECKRESULT(EC_KEY_set_group(ctx->pkey->pkey.ec, group));

                OPENSSL_CHECKRESULT(EVP_PKEY_keygen(ctx, (EVP_PKEY**)&pKey->key));
                
                pKey->type = CKK_EC;
                pKey->size = len;
                pKey->attrib = PrivatePublic;
                pKey->ctx = NULL;
            }
            break;

        case CKM_RSA_PKCS_KEY_PAIR_GEN:
            if(pPublicKeyTemplate != NULL && pPublicKeyTemplate->type == CKA_VALUE_LEN)
            {
                int len = SwapEndianIfBEc32(*(int*)pPublicKeyTemplate->pValue);
                
                *phPublicKey = PKCS11_Objects_OpenSSL::AllocObject(pSessionCtx, KeyType, sizeof(KEY_DATA), &pData);
                
                if(*phPublicKey == CK_OBJECT_HANDLE_INVALID) OPENSSL_SET_AND_LEAVE(CKR_DEVICE_MEMORY);
                
                pKey = (KEY_DATA*)pData->Data;
        
                *phPrivateKey = *phPublicKey;
                
                if(NULL == (ctx = EVP_PKEY_CTX_new_id(EVP_PKEY_RSA, NULL))) OPENSSL_SET_AND_LEAVE(CKR_FUNCTION_FAILED);

                EVP_PKEY_CTX_set_cb(ctx,NULL); 
                    
                OPENSSL_CHECKRESULT(EVP_PKEY_keygen_init(ctx));
                
                OPENSSL_CHECKRESULT(EVP_PKEY_CTX_set_rsa_keygen_bits(ctx, len));
        
                OPENSSL_CHECKRESULT(EVP_PKEY_keygen(ctx, (EVP_PKEY**)&pKey->key));
                
                pKey->type = CKK_RSA;
                pKey->size = len;
                pKey->attrib = PrivatePublic;
                pKey->ctx = NULL;
            }
            break;

        case CKM_DSA_KEY_PAIR_GEN:
            if(pPublicKeyTemplate != NULL && pPublicKeyTemplate->type == CKA_VALUE_LEN)
            {
                int len = SwapEndianIfBEc32(*(int*)pPublicKeyTemplate->pValue);
                EVP_PKEY_CTX* ctx;
                
                OBJECT_DATA* pData = NULL;
                KEY_DATA* pKey = NULL;
                
                *phPublicKey = PKCS11_Objects_OpenSSL::AllocObject(pSessionCtx, KeyType, sizeof(KEY_DATA), &pData);
                
                if(*phPublicKey == CK_OBJECT_HANDLE_INVALID) OPENSSL_SET_AND_LEAVE(CKR_DEVICE_MEMORY);
                
                pKey = (KEY_DATA*)pData->Data;
                
                *phPrivateKey = *phPublicKey;

                if(NULL == (ctx = EVP_PKEY_CTX_new_id(EVP_PKEY_DSA, NULL))) OPENSSL_SET_AND_LEAVE(CKR_FUNCTION_FAILED);
                
                EVP_PKEY_CTX_set_cb(ctx,NULL); 

                OPENSSL_CHECKRESULT(EVP_PKEY_paramgen_init(ctx));

                OPENSSL_CHECKRESULT(EVP_PKEY_CTX_set_dsa_paramgen_bits(ctx, len));

                OPENSSL_CHECKRESULT(EVP_PKEY_paramgen(ctx, &ctx->pkey));

                OPENSSL_CHECKRESULT(EVP_PKEY_keygen_init(ctx));

                OPENSSL_CHECKRESULT(EVP_PKEY_keygen(ctx, (EVP_PKEY**)&pKey->key));
                
                pKey->type = CKK_DSA;
                pKey->size = len;
                pKey->attrib = PrivatePublic;
                pKey->ctx = NULL;
            }
            break;

        default:
            return CKR_MECHANISM_INVALID;
    }

    OPENSSL_CLEANUP();

    Watchdog_GetSetEnabled( TRUE, TRUE );    

    if(ctx != NULL) EVP_PKEY_CTX_free(ctx);

    if(group != NULL) EC_GROUP_free(group);

    if(retVal != CKR_OK)
    {
        if(pData != NULL) 
        {
            PKCS11_Objects_OpenSSL::FreeObject(pSessionCtx, *phPublicKey);
            *phPublicKey = CK_OBJECT_HANDLE_INVALID;
            *phPrivateKey = CK_OBJECT_HANDLE_INVALID;
        }
    }

    OPENSSL_RETURN();
}

CK_RV PKCS11_Keys_OpenSSL::WrapKey(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hWrappingKey, CK_OBJECT_HANDLE hKey, CK_BYTE_PTR pWrappedKey, CK_ULONG_PTR pulWrappedKeyLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Keys_OpenSSL::UnwrapKey(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hUnwrappingKey, CK_BYTE_PTR pWrappedKey, CK_ULONG ulWrappedKeyLen, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulAttributeCount, CK_OBJECT_HANDLE_PTR phKey)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

static BOOL ParseECParam(CK_VOID_PTR pParameter, CK_ULONG ulParamLen, CK_ECDH1_DERIVE_PARAMS& params)
{
    UINT8* pParam = (UINT8*)pParameter;

    memset(&params, 0, sizeof(params));
    
    params.kdf             = SwapEndianIfBEc32(*(UINT32*)pParam); pParam+=4;
    params.ulSharedDataLen = SwapEndianIfBEc32(*(UINT32*)pParam); pParam+=4;

    if(params.ulSharedDataLen > 0)
    {
        params.pSharedData = pParam; pParam += params.ulSharedDataLen;
    }
    params.ulPublicDataLen = SwapEndianIfBEc32(*(UINT32*)pParam); pParam+=4;

    if(params.ulPublicDataLen > 0)
    {
        params.pPublicData = pParam;
    }

    return (params.ulPublicDataLen + params.ulSharedDataLen + 3 * sizeof(UINT32)) <= ulParamLen;
}

CK_RV PKCS11_Keys_OpenSSL::DeriveKey(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hBaseKey, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulAttributeCount, CK_OBJECT_HANDLE_PTR phKey)
{
    OPENSSL_HEADER();
    EVP_PKEY_CTX* ctx   = NULL;
    KEY_DATA* pKey = NULL;
    EVP_PKEY* pPubKey;
    EC_POINT* pPoint;
    
    UINT8  resultKey[1024/8];
    size_t keyBytes = 0;

    memset(resultKey, 0, sizeof(resultKey));

    switch(pMechanism->mechanism)
    {
        case CKM_ECDH1_DERIVE:
            {
                CK_ECDH1_DERIVE_PARAMS  params;
                EVP_PKEY*               pBaseKey;
                const EC_GROUP*         pGroup;
                const EVP_MD*           pDigest = NULL;
                UINT8 pubKey[66*2+1];

                if(!ParseECParam(pMechanism->pParameter, pMechanism->ulParameterLen, params)) OPENSSL_SET_AND_LEAVE(CKR_MECHANISM_PARAM_INVALID);

                ctx  = EVP_PKEY_CTX_new_id(EVP_PKEY_EC, NULL       ); if(ctx  == NULL) OPENSSL_SET_AND_LEAVE(CKR_FUNCTION_FAILED); 
                pKey = GetKeyFromHandle(pSessionCtx, hBaseKey, TRUE); if(pKey == NULL) OPENSSL_SET_AND_LEAVE(CKR_FUNCTION_FAILED);
                
                if(pKey->type != CKK_EC) OPENSSL_SET_AND_LEAVE(CKR_KEY_TYPE_INCONSISTENT);

                pBaseKey = (EVP_PKEY*)pKey->key;                    if(pBaseKey== NULL) OPENSSL_SET_AND_LEAVE(CKR_FUNCTION_FAILED);
                pGroup   = EC_KEY_get0_group(pBaseKey->pkey.ec);    if(pGroup  == NULL) OPENSSL_SET_AND_LEAVE(CKR_FUNCTION_FAILED);
                pPoint   = EC_POINT_new(pGroup);                    if(pPoint  == NULL) OPENSSL_SET_AND_LEAVE(CKR_FUNCTION_FAILED);
                pPubKey  = EVP_PKEY_new();                          if(pPubKey == NULL) OPENSSL_SET_AND_LEAVE(CKR_FUNCTION_FAILED);

                keyBytes = (EVP_PKEY_bits(pBaseKey) + 7) / 8;

                ctx->pkey = pBaseKey;
                OPENSSL_CHECKRESULT(EVP_PKEY_set1_EC_KEY(pPubKey, EC_KEY_new()));

                pubKey[0] = POINT_CONVERSION_UNCOMPRESSED;
                memcpy(&pubKey[1], params.pPublicData, params.ulPublicDataLen);
                
                OPENSSL_CHECKRESULT(EC_POINT_oct2point(pGroup, pPoint, (UINT8*)pubKey, params.ulPublicDataLen+1, NULL));
                
                OPENSSL_CHECKRESULT(EC_KEY_set_group(pPubKey->pkey.ec, pGroup));
                OPENSSL_CHECKRESULT(EC_KEY_set_public_key(pPubKey->pkey.ec, pPoint));
                
                OPENSSL_CHECKRESULT(EVP_PKEY_derive_init(ctx));
                OPENSSL_CHECKRESULT(EVP_PKEY_derive_set_peer(ctx, pPubKey));
                OPENSSL_CHECKRESULT(EVP_PKEY_derive(ctx, resultKey, &keyBytes));            

                
                switch(params.kdf)
                {
                    case CKM_SHA1_KEY_DERIVATION:
                        pDigest = EVP_sha1();
                        break;

                    case CKM_SHA256_KEY_DERIVATION:
                        pDigest = EVP_sha256();
                        break;
                        
                    case CKM_SHA512_KEY_DERIVATION:
                        pDigest = EVP_sha512();
                        break;

                    case CKM_MD5_KEY_DERIVATION:
                        pDigest = EVP_md5();
                        break;                            
                    
                    case CKM_NULL_KEY_DERIVATION:
                        break;

                    case CKM_SHA256_HMAC:
                    case CKM_TLS_MASTER_KEY_DERIVE_DH:
                    default:
                        OPENSSL_SET_AND_LEAVE(CKR_FUNCTION_NOT_SUPPORTED);
                }

                if(pDigest != NULL)
                {
                    UINT8 tmp[1024/8];
                    EVP_MD_CTX pDigestCtx;

                    OPENSSL_CHECKRESULT(EVP_DigestInit(&pDigestCtx, pDigest));
                    EVP_MD_CTX_set_flags(&pDigestCtx,EVP_MD_CTX_FLAG_ONESHOT);
                    OPENSSL_CHECKRESULT(EVP_DigestUpdate(&pDigestCtx, resultKey, keyBytes));
                    OPENSSL_CHECKRESULT(EVP_DigestFinal(&pDigestCtx, tmp, (UINT32*)&keyBytes));
                    OPENSSL_CHECKRESULT(EVP_MD_CTX_cleanup(&pDigestCtx));

                    memcpy(resultKey, tmp, keyBytes);
                }                

                *phKey = LoadKey(pSessionCtx, (void*)resultKey, CKK_GENERIC_SECRET, Secret, keyBytes * 8);
            }
            break;

        default:
            OPENSSL_SET_AND_LEAVE(CKR_MECHANISM_INVALID);
    }
    
    OPENSSL_CLEANUP();
    if(ctx != NULL)
    {
        ctx->pkey = NULL;
        EVP_PKEY_CTX_free(ctx);
    }
    if(pPubKey != NULL)
    {
        EVP_PKEY_free(pPubKey);
    }
    if(pPoint != NULL)
    {
        EC_POINT_free(pPoint);
    }
    OPENSSL_RETURN();
}

CK_RV PKCS11_Keys_OpenSSL::LoadSecretKey(Cryptoki_Session_Context* pSessionCtx, CK_KEY_TYPE keyType, const UINT8* pKey, CK_ULONG ulKeyLength, CK_OBJECT_HANDLE_PTR phKey)
{
    OPENSSL_HEADER();
    
    *phKey = LoadKey(pSessionCtx, (void*)pKey, keyType, Secret, ulKeyLength * 8);

    if(*phKey == CK_OBJECT_HANDLE_INVALID) OPENSSL_SET_AND_LEAVE(CKR_FUNCTION_FAILED);

    OPENSSL_NOCLEANUP();
}

CK_RV PKCS11_Keys_OpenSSL::LoadRsaKey(Cryptoki_Session_Context* pSessionCtx, const RsaKeyData& keyData, CK_BBOOL isPrivate, CK_OBJECT_HANDLE_PTR phKey)
{
    RsaKeyBlob<4096> rsaKey;
    UINT8* pBlob = (UINT8*)rsaKey.Key;
    int keySize = keyData.Modulus_len * 8;
    int len;

    *phKey = CK_OBJECT_HANDLE_INVALID;

    if(isPrivate)
    {
        EVP_PKEY* pKey;
        KEY_ATTRIB keyAttrib;

        len  = rsaKey.CopyFromPrivate(keyData);
        pKey = b2i_PrivateKey((const UINT8**)&pBlob, len);

        if(pKey == NULL) return CKR_FUNCTION_FAILED;

        if(isPrivate)
        {
            if(pKey->pkey.rsa->e != NULL || pKey->pkey.rsa->n != NULL) 
                keyAttrib = PrivatePublic;
            else 
                keyAttrib = Private;
        }
        else
        {
            keyAttrib = Public;
        }
            
        *phKey = LoadKey(pSessionCtx, pKey, CKK_RSA, keyAttrib, keySize); 
    }
    else
    {
        len = rsaKey.CopyFromPublic(keyData);
        *phKey = LoadKey(pSessionCtx, b2i_PublicKey((const UINT8**)&pBlob, len), CKK_RSA, isPrivate ? Private : Public, keySize);
    }

    return *phKey == CK_OBJECT_HANDLE_INVALID ? CKR_FUNCTION_FAILED : CKR_OK;
}

CK_RV PKCS11_Keys_OpenSSL::LoadDsaKey(Cryptoki_Session_Context* pSessionCtx, const DsaKeyData& keyData, CK_BBOOL isPrivate, CK_OBJECT_HANDLE_PTR phKey)
{
    EVP_PKEY *key = NULL;
    DSA *dsa = NULL;
    int keySize = keyData.Base_g_len * 8;
    KEY_ATTRIB keyAttrib;
    
    if((key = EVP_PKEY_new()) == NULL) return CKR_DEVICE_MEMORY;
    if((dsa = DSA_new     ()) == NULL) return CKR_DEVICE_MEMORY;
    
    dsa->p = BN_bin2bn(keyData.Prime_p   , keyData.Prime_p_len   , NULL);
    dsa->q = BN_bin2bn(keyData.Subprime_q, keyData.Subprime_q_len, NULL);
    dsa->g = BN_bin2bn(keyData.Base_g    , keyData.Base_g_len    , NULL);
    
    if(isPrivate)
    {
        dsa->priv_key=BN_bin2bn(keyData.Private_x, keyData.Private_x_len, NULL);
        
        if(keyData.Public_y_len > 0)
        {
            dsa->pub_key=BN_bin2bn(keyData.Public_y, keyData.Public_y_len, NULL);
            keyAttrib = PrivatePublic;
        }
        else
        {
            keyAttrib = Private;
        }
    }
    else
    {
        dsa->pub_key=BN_bin2bn(keyData.Public_y, keyData.Public_y_len, NULL);
        keyAttrib = Public;
    }
    if(EVP_PKEY_assign_DSA(key, dsa) <= 0)
    {
        DSA_free(dsa);
        EVP_PKEY_free(key);
        
        return CKR_ATTRIBUTE_VALUE_INVALID;
    }
    
    *phKey = LoadKey(pSessionCtx, key, CKK_DSA, keyAttrib, keySize);

    if(*phKey == CK_OBJECT_HANDLE_INVALID)
    {
        EVP_PKEY_free(key);

        return CKR_FUNCTION_FAILED;
    }

    return CKR_OK;    
}

CK_RV PKCS11_Keys_OpenSSL::LoadEcKey(Cryptoki_Session_Context* pSessionCtx, const EcKeyData&  keyData, CK_BBOOL isPrivate, CK_OBJECT_HANDLE_PTR phKey)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}


