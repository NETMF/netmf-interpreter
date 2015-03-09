#include "pkcs11_stub.h"

static ICryptokiEncryption s_Encryption =
{
    PKCS11_Encryption_stub::EncryptInit,
    PKCS11_Encryption_stub::Encrypt,
    PKCS11_Encryption_stub::EncryptUpdate,
    PKCS11_Encryption_stub::EncryptFinal,
    PKCS11_Encryption_stub::DecryptInit,
    PKCS11_Encryption_stub::Decrypt,
    PKCS11_Encryption_stub::DecryptUpdate,
    PKCS11_Encryption_stub::DecryptFinal,    
};

static ICryptokiDigest s_Digest = 
{
    PKCS11_Digest_stub::DigestInit,
    PKCS11_Digest_stub::Digest,
    PKCS11_Digest_stub::Update,
    PKCS11_Digest_stub::DigestKey,
    PKCS11_Digest_stub::Final,
};

static ICryptokiSignature s_Sign = 
{
    PKCS11_Signature_stub::SignInit,
    PKCS11_Signature_stub::Sign,
    PKCS11_Signature_stub::SignUpdate,
    PKCS11_Signature_stub::SignFinal,
    
    PKCS11_Signature_stub::VerifyInit,
    PKCS11_Signature_stub::Verify,
    PKCS11_Signature_stub::VerifyUpdate,
    PKCS11_Signature_stub::VerifyFinal,
};

static ICryptokiKey s_Key =
{
    PKCS11_Keys_stub::GenerateKey,
    PKCS11_Keys_stub::GenerateKeyPair,
    PKCS11_Keys_stub::WrapKey,
    PKCS11_Keys_stub::UnwrapKey,
    PKCS11_Keys_stub::DeriveKey,
    PKCS11_Keys_stub::LoadSecretKey,
    PKCS11_Keys_stub::LoadRsaKey,
    PKCS11_Keys_stub::LoadDsaKey,
    PKCS11_Keys_stub::LoadEcKey,
    PKCS11_Keys_stub::LoadKeyBlob,
};

static ICryptokiObject s_Object = 
{
    PKCS11_Objects_stub::CreateObject,
    PKCS11_Objects_stub::CopyObject,
    PKCS11_Objects_stub::DestroyObject,
    PKCS11_Objects_stub::GetObjectSize,
    PKCS11_Objects_stub::GetAttributeValue,
    PKCS11_Objects_stub::SetAttributeValue,
    
    PKCS11_Objects_stub::FindObjectsInit,
    PKCS11_Objects_stub::FindObjects,
    PKCS11_Objects_stub::FindObjectsFinal,
};

static ICryptokiRandom s_Random = 
{
    PKCS11_Random_stub::SeedRandom,
    PKCS11_Random_stub::GenerateRandom,
};

static ICryptokiSession s_Session = 
{
    PKCS11_Session_stub::OpenSession,
    PKCS11_Session_stub::CloseSession,
    PKCS11_Session_stub::InitPin,
    PKCS11_Session_stub::SetPin,
    PKCS11_Session_stub::Login,
    PKCS11_Session_stub::Logout,
};

static ICryptokiToken s_Token =
{
    PKCS11_Token_stub::Initialize,
    PKCS11_Token_stub::Uninitialize,
    PKCS11_Token_stub::InitializeToken,
    PKCS11_Token_stub::GetDeviceError,
};

CK_SLOT_INFO g_PKCS11_SlotInfo_stub = 
{
    "PKCS11_Crypto_stub", 
    "<ProviderNameHere>", 
    CKF_TOKEN_PRESENT, 
    { 0, 0 }, 
    { 1, 0 }
};

static CryptokiMechanism s_Mechanisms[] = 
{
    // Add your supported mechanisms here
    
    { CKM_SHA_1, { 160,  160, CKF_DIGEST } },
};

CryptokiToken g_PKCS11_Token_stub = 
{
    { // TOKEN INFO
        "<TokenLable>",
        "<TokenManufacturer>",
        "<model>",
        "<serialNumber>",
        CKF_RNG | CKF_PROTECTED_AUTHENTICATION_PATH | CKF_TOKEN_INITIALIZED,
        10, // max session count
        0, // session count
        10, // max rw session count
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
    NULL,  // persistent storage 
};

