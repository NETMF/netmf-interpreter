#include "OpenSSL_pkcs11.h"
#include <SimpleStorage\SimpleStorage_decl.h>

static ICryptokiEncryption s_Encryption =
{
    PKCS11_Encryption_OpenSSL::EncryptInit,
    PKCS11_Encryption_OpenSSL::Encrypt,
    PKCS11_Encryption_OpenSSL::EncryptUpdate,
    PKCS11_Encryption_OpenSSL::EncryptFinal,
    PKCS11_Encryption_OpenSSL::DecryptInit,
    PKCS11_Encryption_OpenSSL::Decrypt,
    PKCS11_Encryption_OpenSSL::DecryptUpdate,
    PKCS11_Encryption_OpenSSL::DecryptFinal,    
};

static ICryptokiDigest s_Digest = 
{
    PKCS11_Digest_OpenSSL::DigestInit,
    PKCS11_Digest_OpenSSL::Digest,
    PKCS11_Digest_OpenSSL::Update,
    PKCS11_Digest_OpenSSL::DigestKey,
    PKCS11_Digest_OpenSSL::Final,
};

static ICryptokiSignature s_Sign = 
{
    PKCS11_Signature_OpenSSL::SignInit,
    PKCS11_Signature_OpenSSL::Sign,
    PKCS11_Signature_OpenSSL::SignUpdate,
    PKCS11_Signature_OpenSSL::SignFinal,
    
    PKCS11_Signature_OpenSSL::VerifyInit,
    PKCS11_Signature_OpenSSL::Verify,
    PKCS11_Signature_OpenSSL::VerifyUpdate,
    PKCS11_Signature_OpenSSL::VerifyFinal,
};

static ICryptokiKey s_Key =
{
    PKCS11_Keys_OpenSSL::GenerateKey,
    PKCS11_Keys_OpenSSL::GenerateKeyPair,
    PKCS11_Keys_OpenSSL::WrapKey,
    PKCS11_Keys_OpenSSL::UnwrapKey,
    PKCS11_Keys_OpenSSL::DeriveKey,
    PKCS11_Keys_OpenSSL::LoadSecretKey,
    PKCS11_Keys_OpenSSL::LoadRsaKey,
    PKCS11_Keys_OpenSSL::LoadDsaKey,
    PKCS11_Keys_OpenSSL::LoadEcKey,
    PKCS11_Keys_OpenSSL::LoadKeyBlob,
};

static ICryptokiObject s_Object = 
{
    PKCS11_Objects_OpenSSL::CreateObject,
    PKCS11_Objects_OpenSSL::CopyObject,
    PKCS11_Objects_OpenSSL::DestroyObject,
    PKCS11_Objects_OpenSSL::GetObjectSize,
    PKCS11_Objects_OpenSSL::GetAttributeValue,
    PKCS11_Objects_OpenSSL::SetAttributeValue,
    
    PKCS11_Objects_OpenSSL::FindObjectsInit,
    PKCS11_Objects_OpenSSL::FindObjects,
    PKCS11_Objects_OpenSSL::FindObjectsFinal,
};

static ICryptokiRandom s_Random = 
{
    PKCS11_Random_OpenSSL::SeedRandom,
    PKCS11_Random_OpenSSL::GenerateRandom,
};

static ICryptokiSession s_Session = 
{
    PKCS11_Session_OpenSSL::OpenSession,
    PKCS11_Session_OpenSSL::CloseSession,
    PKCS11_Session_OpenSSL::InitPin,
    PKCS11_Session_OpenSSL::SetPin,
    PKCS11_Session_OpenSSL::Login,
    PKCS11_Session_OpenSSL::Logout,
};

static ICryptokiToken s_Token =
{
    PKCS11_Token_OpenSSL::Initialize,
    PKCS11_Token_OpenSSL::Uninitialize,
    PKCS11_Token_OpenSSL::InitializeToken,
    PKCS11_Token_OpenSSL::GetDeviceError,
};

static ISecureStorage s_Storage =
{
    SimpleStorage::Create,
    SimpleStorage::Read,
    SimpleStorage::GetFileEnum,
    SimpleStorage::GetNextFile,
    SimpleStorage::Delete,
};

CK_SLOT_INFO g_OpenSSL_SlotInfo = 
{
    "OpenSSL_Crypto", 
    "MICROSOFT", 
    CKF_TOKEN_PRESENT, 
    { 0, 0 }, 
    { 1, 0 }
};

static CryptokiMechanism s_Mechanisms[] = 
{
    { CKM_EC_KEY_PAIR_GEN      , { 256,  521, CKF_GENERATE_KEY_PAIR                            } },
    { CKM_ECDSA                , { 256,  521,                             CKF_SIGN | CKF_VERIFY} },
    { CKM_ECDH1_DERIVE         , { 256,  521, CKF_DERIVE                                       } },
    { CKM_GENERIC_SECRET_KEY_GEN,{   1, 3072, CKF_GENERATE                                     } },
    { CKM_DES3_KEY_GEN         , {  56,  168, CKF_GENERATE                                     } },
    { CKM_DES3_CBC             , {  56,  168, CKF_ENCRYPT | CKF_DECRYPT                        } },
    { CKM_DES3_CBC_PAD         , {  56,  168, CKF_ENCRYPT | CKF_DECRYPT                        } },
    { CKM_MD5_HMAC             , { 128,  128, CKF_DIGEST                | CKF_SIGN | CKF_VERIFY} },
    { CKM_MD5                  , { 128,  128, CKF_DIGEST                                       } },
    { CKM_SHA_1_HMAC           , { 160,  160, CKF_DIGEST                | CKF_SIGN | CKF_VERIFY} },
    { CKM_SHA256_HMAC          , { 256,  256, CKF_DIGEST                | CKF_SIGN | CKF_VERIFY} },
    { CKM_SHA384_HMAC          , { 384,  384, CKF_DIGEST                | CKF_SIGN | CKF_VERIFY} },
    { CKM_SHA512_HMAC          , { 512,  512, CKF_DIGEST                | CKF_SIGN | CKF_VERIFY} },
    { CKM_RIPEMD160_HMAC       , { 160,  160, CKF_DIGEST                | CKF_SIGN | CKF_VERIFY} },
    { CKM_SHA_1                , { 160,  160, CKF_DIGEST                                       } },
    { CKM_SHA224               , { 224,  224, CKF_DIGEST                                       } },
    { CKM_SHA256               , { 256,  256, CKF_DIGEST                                       } },
    { CKM_SHA384               , { 384,  384, CKF_DIGEST                                       } },
    { CKM_SHA512               , { 512,  512, CKF_DIGEST                                       } },
    { CKM_RIPEMD160            , { 160,  160, CKF_DIGEST                                       } },
    { CKM_DSA_KEY_PAIR_GEN     , {1024, 2048, CKF_GENERATE_KEY_PAIR                            } },
    { CKM_DSA                  , {1024, 3072,                             CKF_SIGN | CKF_VERIFY} },
    { CKM_RSA_PKCS_KEY_PAIR_GEN, {1024, 2048, CKF_GENERATE_KEY_PAIR                            } },
    { CKM_RSA_PKCS             , {1024, 2048, CKF_ENCRYPT | CKF_DECRYPT | CKF_SIGN | CKF_VERIFY} },
    { CKM_AES_CBC              , { 128,  256, CKF_ENCRYPT | CKF_DECRYPT                        } },
    { CKM_AES_CBC_PAD          , { 128,  256, CKF_ENCRYPT | CKF_DECRYPT                        } },
    { CKM_AES_ECB              , { 128,  256, CKF_ENCRYPT | CKF_DECRYPT                        } },
    { CKM_AES_ECB_PAD          , { 128,  256, CKF_ENCRYPT | CKF_DECRYPT                        } },
    { CKM_AES_KEY_GEN          , { 128,  256, CKF_GENERATE                                     } },
};

CryptokiToken g_OpenSSL_Token = 
{
    { // TOKEN INFO
        "OpenSSL_Crypto",
        "Microsoft",
        "OpenSSL 1.0",
        "000-000-000",
        CKF_RNG | CKF_PROTECTED_AUTHENTICATION_PATH | CKF_TOKEN_INITIALIZED,
        OPENSSL_MAX_SESSION_COUNT, // max session count
        0, // session count
        OPENSSL_MAX_SESSION_COUNT, // max rw session count
        0, // rw session count
        0, // max pin len
        0, // min pin len
        1024, // Total public mem
        1024, // free public mem
        0, // total private mem
        0, // free private mem

        { 1, 0 }, // hardware version
        { 1, 0 }, // firmware version
        "201001010101000", // utc time
    },

    ROPublic,  // token wide session state
    {   // key wrapping mechanism
        CKM_AES_CBC,
        NULL,
        0
    },
    L"PinWrappingKey",
    4*1024,
    ARRAYSIZE(s_Mechanisms),
    s_Mechanisms,

    &s_Token,
    &s_Encryption,
    &s_Digest,
    &s_Sign,
    &s_Key,
    &s_Object,
    &s_Random,    
    &s_Session,
    &s_Storage,
};

