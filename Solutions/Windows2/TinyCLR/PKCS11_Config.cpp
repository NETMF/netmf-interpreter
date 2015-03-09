#include "pkcs11.h"
#include <PKCS11\Tokens\Legacy\NetMFCrypto.h>

extern CK_SLOT_INFO  g_OpenSSL_SlotInfo;
extern CryptokiToken g_OpenSSL_Token;

static ICryptokiEncryption s_Encryption =
{
    PKCS11_Encryption_Windows::EncryptInit,
    PKCS11_Encryption_Windows::Encrypt,
    PKCS11_Encryption_Windows::EncryptUpdate,
    PKCS11_Encryption_Windows::EncryptFinal,
    PKCS11_Encryption_Windows::DecryptInit,
    PKCS11_Encryption_Windows::Decrypt,
    PKCS11_Encryption_Windows::DecyptUpdate,
    PKCS11_Encryption_Windows::DecryptFinal,    
};

static ICryptokiDigest s_Digest = 
{
    PKCS11_Digest_Windows::DigestInit,
    PKCS11_Digest_Windows::Digest,
    PKCS11_Digest_Windows::Update,
    PKCS11_Digest_Windows::DigestKey,
    PKCS11_Digest_Windows::Final,
};

static ICryptokiSignature s_Sign = 
{
    PKCS11_Signature_Windows::SignInit,
    PKCS11_Signature_Windows::Sign,
    PKCS11_Signature_Windows::SignUpdate,
    PKCS11_Signature_Windows::SignFinal,
    
    PKCS11_Signature_Windows::VerifyInit,
    PKCS11_Signature_Windows::Verify,
    PKCS11_Signature_Windows::VerifyUpdate,
    PKCS11_Signature_Windows::VerifyFinal,
};

static ICryptokiKey s_Key =
{
    PKCS11_Keys_Windows::GenerateKey,
    PKCS11_Keys_Windows::GenerateKeyPair,
    PKCS11_Keys_Windows::WrapKey,
    PKCS11_Keys_Windows::UnwrapKey,
    PKCS11_Keys_Windows::DeriveKey,
    PKCS11_Keys_Windows::LoadSecretKey,
    PKCS11_Keys_Windows::LoadRsaKey,
    PKCS11_Keys_Windows::LoadDsaKey,
    PKCS11_Keys_Windows::LoadEcKey,
    PKCS11_Keys_Windows::LoadKeyBlob,
};

static ICryptokiObject s_Object = 
{
    PKCS11_Objects_Windows::CreateObject,
    PKCS11_Objects_Windows::CopyObject,
    PKCS11_Objects_Windows::DestroyObject,
    PKCS11_Objects_Windows::GetObjectSize,
    PKCS11_Objects_Windows::GetAttributeValue,
    PKCS11_Objects_Windows::SetAttributeValue,
    
    PKCS11_Objects_Windows::FindObjectsInit,
    PKCS11_Objects_Windows::FindObjects,
    PKCS11_Objects_Windows::FindObjectsFinal,
};

static ICryptokiRandom s_Random = 
{
    PKCS11_Random_Windows::SeedRandom,
    PKCS11_Random_Windows::GenerateRandom,
};

static ICryptokiSession s_Session = 
{
    PKCS11_Session_Windows::OpenSession,
    PKCS11_Session_Windows::CloseSession,
    PKCS11_Session_Windows::InitPin,
    PKCS11_Session_Windows::SetPin,
    PKCS11_Session_Windows::Login,
    PKCS11_Session_Windows::Logout,
};

static ICryptokiToken s_TokenImpl =
{
    PKCS11_Token_Windows::Initialize,
    PKCS11_Token_Windows::Uninitialize,
    PKCS11_Token_Windows::InitializeToken,
    PKCS11_Token_Windows::GetDeviceError,
};

static CK_SLOT_INFO s_Info = 
{
    "MYSLOT1", 
    "MICROSOFT", 
    CKF_HW_SLOT | CKF_TOKEN_PRESENT, 
    { 1, 0 }, 
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

static CryptokiToken s_Token = 
{
    { // TOKEN INFO
        "Emulator_Crypto",
        "TOKEN_MANU",
        "TOKEN_MODEL",
        "TOKEN_SN",
        CKF_RNG | CKF_PROTECTED_AUTHENTICATION_PATH | CKF_TOKEN_INITIALIZED,
        100, // max session count
        0, // session count
        100, // max rw session count
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
    1024,
    ARRAYSIZE(s_Mechanisms),
    s_Mechanisms,

    &s_TokenImpl,
    &s_Encryption,
    &s_Digest,
    &s_Sign,
    &s_Key,
    &s_Object,
    &s_Random,    
    &s_Session,
    NULL,
};

static ICryptokiEncryption s_TinyEncryption =
{
    NetMFCrypto::EncryptInit,
    NetMFCrypto::Encrypt,
    NULL,
    NULL,
    NetMFCrypto::DecryptInit,
    NetMFCrypto::Decrypt,
    NULL,
    NULL,    
};

static ICryptokiSignature s_TinySign = 
{
    NetMFCrypto::SignInit,
    NetMFCrypto::Sign,
    NULL,
    NULL,
    
    NetMFCrypto::VerifyInit,
    NetMFCrypto::Verify,
    NULL,
    NULL,
};

static ICryptokiKey s_TinyKey =
{
    NULL,
    NetMFCrypto::GenerateKeyPair,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL
};

static ICryptokiObject s_TinyObject =
{
    NULL,
    NULL,
    NULL,
    NetMFCrypto::GetObjectSize,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
};


static CryptokiMechanism s_TinyMechanisms[] = 
{
    { CKM_RSA_PKCS   , {1024, 2048, CKF_ENCRYPT | CKF_DECRYPT} },
    { CKM_AES_KEY_GEN, { 512, 1024, CKF_GENERATE             } },
};

static CryptokiToken s_TinyToken = 
{
    { // TOKEN INFO
        "Legacy_Crypto",
        "TINY_TOKEN_MANU",
        "TINY_TOKEN_MO",
        "TINY_TOKEN_SN",
        CKF_RNG | CKF_PROTECTED_AUTHENTICATION_PATH | CKF_TOKEN_INITIALIZED,
        3, // max session count
        0, // session count
        0, // max rw session count
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
    1024,
    ARRAYSIZE(s_TinyMechanisms),
    s_TinyMechanisms,

    NULL, //ICryptokiToken
    &s_TinyEncryption,
    NULL,
    &s_TinySign,
    &s_TinyKey,
    &s_TinyObject,
    NULL,    
    NULL,
};


static CK_SLOT_INFO s_Info2 = 
{
    "MYSLOT2", 
    "MICROSOFT", 
    CKF_HW_SLOT | CKF_TOKEN_PRESENT, 
    { 1, 1 }, 
    { 1, 2 }
};


CryptokiSlot  g_CryptokiSlots[] = 
{
    {
        &g_OpenSSL_SlotInfo,
        &g_OpenSSL_Token,
    },
    {
        &s_Info,
        &s_Token,
    },
    {
        &s_Info2,
        &s_TinyToken,
    }
};

const UINT32 g_CryptokiSlotCount = ARRAYSIZE(g_CryptokiSlots);


