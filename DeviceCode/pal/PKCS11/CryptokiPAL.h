#ifndef _NETMF_CRYPTOKIPAL_H_
#define _NETMF_CRYPTOKIPAL_H_


#include <tinyhal.h>
#include <tinyCLR_Endian.h>
#include "cryptoki.h"


#ifndef PLATFORM_DEPENDENT__NETMF_CRYPTOKI_MAX_SLOTS
#define NETMF_CRYPTOKI_MAX_SLOTS 10
#else
#define NETMF_CRYPTOKI_MAX_SLOTS PLATFORM_DEPENDENT__NETMF_CRYPTOKI_MAX_SLOTS
#endif

#ifndef PLATFORM_DEPENDENT__NETMF_CRYPTOKI_MAX_SESSION
#define NETMF_CRYPTOKI_MAX_SESSION 10
#else
#define NETMF_CRYPTOKI_MAX_SESSION PLATFORM_DEPENDENT__NETMF_CRYPTOKI_MAX_SESSION
#endif

#define CKM_VENDOR_RNG          (CKM_VENDOR_DEFINED | 0x00000001)
#define CKM_ECDH_KEY_PAIR_GEN   (CKM_VENDOR_DEFINED | 0x00000002)
#define CKM_AES_ECB_PAD         (CKM_VENDOR_DEFINED | 0x00000003)
#define CKM_NULL_KEY_DERIVATION (CKM_VENDOR_DEFINED | 0x00000004)


#define CKA_SIGN_NO_NODIGEST_FLAG 0x40000000

#define CKA_PERSIST              (CKA_VENDOR_DEFINED | 0x00000001)
#define CKA_PASSWORD             (CKA_VENDOR_DEFINED | 0x00000002)
#define CKA_SECURITY_LEVEL       (CKA_VENDOR_DEFINED | 0x00000003)

#define CK_SECURITY_LEVEL_HighestAvailable 0
#define CK_SECURITY_LEVEL_TamperResistent  1
#define CK_SECURITY_LEVEL_Wrapped          2
#define CK_SECURITY_LEVEL_PlainText        3

typedef enum _KEY_ATTRIB
{
    Private       = 0x01,
    Public        = 0x02,
    Secret        = 0x04,
    PrivatePublic = Private | Public,
} KEY_ATTRIB;


#define CK_OBJECT_HANDLE_INVALID  (CK_OBJECT_HANDLE)-1
#define CK_SESSION_HANDLE_INVALID (CK_SESSION_HANDLE)-1
#define CK_SLOT_ID_INVALID        (CK_SLOT_ID)-1

#define NETMF_CRYPTOKI_VERSION_MAJOR 1
#define NETMF_CRYPTOKI_VERSION_MINOR 0

#define DSA_MAGIC_FOR_PUBLICKEY  0x31535344
#define DSA_MAGIC_FOR_PRIVATEKEY 0x32535344

#define RSA_MAGIC_FOR_PUBLICKEY  0x31415352
#define RSA_MAGIC_FOR_PRIVATEKEY 0x32415352

#define CALG_DSA_SIGN  0x2200

#ifndef _WIN32
typedef UINT32 ALG_ID;

#define CALG_RSA_SIGN  (1ul<<13 | 2ul<<9)
#define CALG_RSA_KEYX  (5ul<<13 | 2ul<<9)

#define PRIVATEKEYBLOB 7
#define PUBLICKEYBLOB  6

typedef struct _BLOB {
  UINT32 cbSize;
  PBYTE  pBlobData;
} BLOB;


typedef struct _PUBLICKEYSTRUC 
{
  BYTE   bType;
  BYTE   bVersion;
  WORD   reserved;
  ALG_ID aiKeyAlg;
} BLOBHEADER;


typedef struct _RSAPUBKEY {
  DWORD magic;
  DWORD bitlen;
  DWORD pubexp;
} RSAPUBKEY;

#endif

typedef struct _DSAPUBKEY {
  DWORD magic;
  DWORD bitlen;
} DSAPUBKEY;

typedef struct _DsaKeyData
{
    PBYTE  Prime_p;
    UINT32 Prime_p_len;
    UINT32 Subprime_q_len;
    UINT32 Base_g_len;
    UINT32 Public_y_len;
    UINT32 Private_x_len;
    UINT32 Seed_len;
    UINT32 Counter;
    PBYTE  Subprime_q;
    PBYTE  Base_g;
    PBYTE  Public_y;
    PBYTE  Private_x;
    PBYTE  Seed;
} DsaKeyData;

typedef struct _EcKeyData
{
} EcKeyData;

UINT8* be_memcpy(void* dest, const void* src, size_t len);

typedef struct _RsaKeyData
{
    UINT32 Modulus_len;
    UINT32 PrivateExponent_len;
    UINT32 PublicExponent_len;
    UINT32 Prime1_len;
    UINT32 Prime2_len;
    UINT32 Exponent1_len;
    UINT32 Exponent2_len;
    UINT32 Coefficient_len;
    PBYTE  Modulus;
    PBYTE  PrivateExponent;
    PBYTE  PublicExponent;
    PBYTE  Prime1;
    PBYTE  Prime2;
    PBYTE  Exponent1;
    PBYTE  Exponent2;
    PBYTE  Coefficient;
} RsaKeyData;

#define ASSIGN_BYTE(x, y) *x++ = y
#define ASSIGN_WORD(x, y) *((WORD*)x) = SwapEndianIfBEc16(y); x+=2
#define ASSIGN_DWORD(x, y) *((DWORD*)x) = SwapEndianIfBEc32(y); x+=4

#define BCRYPT_ECDH_PUBLIC_P256_MAGIC   0x314B4345  // ECK1 
#define BCRYPT_ECDH_PRIVATE_P256_MAGIC  0x324B4345  // ECK2 
#define BCRYPT_ECDH_PUBLIC_P384_MAGIC   0x334B4345  // ECK3 
#define BCRYPT_ECDH_PRIVATE_P384_MAGIC  0x344B4345  // ECK4 
#define BCRYPT_ECDH_PUBLIC_P521_MAGIC   0x354B4345  // ECK5 
#define BCRYPT_ECDH_PRIVATE_P521_MAGIC  0x364B4345  // ECK6 

#define BCRYPT_ECDSA_PUBLIC_P256_MAGIC  0x31534345  // ECS1 
#define BCRYPT_ECDSA_PRIVATE_P256_MAGIC 0x32534345  // ECS2 
#define BCRYPT_ECDSA_PUBLIC_P384_MAGIC  0x33534345  // ECS3 
#define BCRYPT_ECDSA_PRIVATE_P384_MAGIC 0x34534345  // ECS4 
#define BCRYPT_ECDSA_PUBLIC_P521_MAGIC  0x35534345  // ECS5 
#define BCRYPT_ECDSA_PRIVATE_P521_MAGIC 0x36534345  // ECS6 

typedef struct _EccKeyData
{
    UINT32 X_len;
    UINT32 Y_len;
    UINT32 d_len;

    PBYTE X;
    PBYTE Y;
    PBYTE d;
} EccKeyData;

template <size_t maxKeySize> class EccKeyBlob
{
public:
    UINT8 Key[2*sizeof(UINT32) + maxKeySize*3];
    
    int CopyFromPrivate(EccKeyData params) 
    {
        UINT8* pKey = &Key[0];

        memset(pKey, 0, sizeof(Key));

        switch(params.X_len)
        {
            case 256:
                ASSIGN_DWORD(pKey, BCRYPT_ECDH_PRIVATE_P256_MAGIC); // BCRYPT_ECCKEY_BLOB.Magic
                break;

            case 384:
                ASSIGN_DWORD(pKey, BCRYPT_ECDH_PRIVATE_P384_MAGIC); // BCRYPT_ECCKEY_BLOB.Magic
                break;

            case 521:
                ASSIGN_DWORD(pKey, BCRYPT_ECDH_PRIVATE_P521_MAGIC); // BCRYPT_ECCKEY_BLOB.Magic
                break;

            default:
                return -1;
        }

        ASSIGN_DWORD(pKey, params.X_len );                  // BCRYPT_ECCKEY_BLOB.cbKey
    
        if(params.X != NULL) be_memcpy( (void*)pKey, (void*)params.X, params.X_len ); pKey += params.X_len;
        if(params.Y != NULL) be_memcpy( (void*)pKey, (void*)params.Y, params.Y_len ); pKey += params.Y_len;
        if(params.d != NULL) be_memcpy( (void*)pKey, (void*)params.d, params.d_len ); pKey += params.d_len;

        return (UINT32)pKey - (UINT32)Key;
    }   
    
    int CopyFromPublic(EccKeyData params) 
    {
        UINT8* pKey = &Key[0];

        memset(pKey, 0, sizeof(Key));

        switch(params.X_len)
        {
            case 256:
                ASSIGN_DWORD(pKey, BCRYPT_ECDH_PUBLIC_P256_MAGIC); // BCRYPT_ECCKEY_BLOB.Magic
                break;

            case 384:
                ASSIGN_DWORD(pKey, BCRYPT_ECDH_PUBLIC_P384_MAGIC); // BCRYPT_ECCKEY_BLOB.Magic
                break;

            case 521:
                ASSIGN_DWORD(pKey, BCRYPT_ECDH_PUBLIC_P521_MAGIC); // BCRYPT_ECCKEY_BLOB.Magic
                break;

            default:
                return -1;
        }

        ASSIGN_DWORD(pKey, params.X_len );                  // BCRYPT_ECCKEY_BLOB.cbKey
    
        if(params.X != NULL) be_memcpy( (void*)pKey, (void*)params.X, params.X_len ); pKey += params.X_len;
        if(params.Y != NULL) be_memcpy( (void*)pKey, (void*)params.Y, params.Y_len ); pKey += params.Y_len;

        return (UINT32)pKey - (UINT32)Key;
    }        
};

template <size_t maxKeySize> class RsaKeyBlob
{
public:
    UINT8 Key[2*maxKeySize/8 + 5*maxKeySize/16 + 4 /*public expoennt*/ + sizeof(BLOBHEADER) + sizeof(RSAPUBKEY)];
    
    int CopyFromPrivate(RsaKeyData pParams) 
    {
        UINT8* pKey = &Key[0];

        memset(pKey, 0, sizeof(Key));
        
        ASSIGN_BYTE(pKey,  PRIVATEKEYBLOB          ); //BlobHeader.bType
        ASSIGN_BYTE(pKey,  2                       ); // BlobHeader.bVersion
        ASSIGN_WORD(pKey,  0                       ); // BlobHeader.reserved
        ASSIGN_DWORD(pKey, CALG_RSA_KEYX           ); // BlobHeader.aiKeyAlg
        ASSIGN_DWORD(pKey, RSA_MAGIC_FOR_PRIVATEKEY); // DsaPubKey.magic
        ASSIGN_DWORD(pKey, pParams.Modulus_len * 8 ); // DsaPubKey.bitlen 

        if(pParams.PublicExponent  != NULL) be_memcpy( (void*)pKey, (void*)pParams.PublicExponent , pParams.PublicExponent_len ); pKey += pParams.PublicExponent_len;
        while(0 != ((UINT32)pKey & 0x3))
        {
            pKey++; // public exponent is only three bytes, we need to align
        }
        if(pParams.Modulus         != NULL) be_memcpy( (void*)pKey, (void*)pParams.Modulus        , pParams.Modulus_len        ); pKey += pParams.Modulus_len;
        if(pParams.Prime1          != NULL) be_memcpy( (void*)pKey, (void*)pParams.Prime1         , pParams.Prime1_len         ); pKey += pParams.Prime1_len;
        if(pParams.Prime2          != NULL) be_memcpy( (void*)pKey, (void*)pParams.Prime2         , pParams.Prime2_len         ); pKey += pParams.Prime2_len;
        if(pParams.Exponent1       != NULL) be_memcpy( (void*)pKey, (void*)pParams.Exponent1      , pParams.Exponent1_len      ); pKey += pParams.Exponent1_len;
        if(pParams.Exponent2       != NULL) be_memcpy( (void*)pKey, (void*)pParams.Exponent2      , pParams.Exponent2_len      ); pKey += pParams.Exponent2_len;
        if(pParams.Coefficient     != NULL) be_memcpy( (void*)pKey, (void*)pParams.Coefficient    , pParams.Coefficient_len    ); pKey += pParams.Coefficient_len;
        if(pParams.PrivateExponent != NULL) be_memcpy( (void*)pKey, (void*)pParams.PrivateExponent, pParams.PrivateExponent_len); pKey += pParams.PrivateExponent_len;

        _ASSERTE(((UINT32)pKey - (UINT32)Key) <= ARRAYSIZE(Key));

        return (UINT32)pKey - (UINT32)Key;
    }   
    
    int CopyFromPublic(RsaKeyData pParams) 
    {
        UINT8* pKey = &Key[0];

        memset(pKey, 0, sizeof(Key));
        
        ASSIGN_BYTE(pKey,  PUBLICKEYBLOB          ); //BlobHeader.bType
        ASSIGN_BYTE(pKey,  2                      ); // BlobHeader.bVersion
        ASSIGN_WORD(pKey,  0                      ); // BlobHeader.reserved
        ASSIGN_DWORD(pKey, CALG_RSA_KEYX          ); // BlobHeader.aiKeyAlg
        ASSIGN_DWORD(pKey, RSA_MAGIC_FOR_PUBLICKEY); // DsaPubKey.magic
        ASSIGN_DWORD(pKey, pParams.Modulus_len * 8); // DsaPubKey.bitlen 
    
        if(pParams.PublicExponent  != NULL) be_memcpy( (void*)pKey, (void*)pParams.PublicExponent , pParams.PublicExponent_len ); pKey += pParams.PublicExponent_len;
        while(0 != ((UINT32)pKey & 0x3))
        {
            pKey++; // public exponent is only three bytes, we need to align
        }
        if(pParams.Modulus         != NULL) be_memcpy( (void*)pKey, (void*)pParams.Modulus        , pParams.Modulus_len        ); pKey += pParams.Modulus_len;

        _ASSERTE(((UINT32)pKey - (UINT32)Key) <= ARRAYSIZE(Key));

        return (UINT32)pKey - (UINT32)Key;
    }        
};

typedef void (*PFNSlotEvent) (void* arg, unsigned int event);

typedef struct _PAL_CRYPTOKI_SLOT_STATE
{
    PFNSlotEvent SlotEventCallback;
    void*        Context;
} PAL_CRYPTOKI_SLOT_STATE;

struct CryptokiSession;

typedef struct _Cryptoki_Session_Context
{
    void* TokenCtx;
    void* LoginCtx;
    void* EncryptionCtx;
    void* DecryptionCtx;
    void* DigestCtx;
    void* SignCtx;
    void* VerifyCtx;    
    void* FindObjectsCtx;
} Cryptoki_Session_Context;

struct ICryptokiEncryption
{
    CK_RV (*EncryptInit)  (Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pEncryptMech, CK_OBJECT_HANDLE hKey);
    CK_RV (*Encrypt)      (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pEncryptedData, CK_ULONG_PTR pulEncryptedDataLen);
    CK_RV (*EncryptUpdate)(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pPart, CK_ULONG ulPartLen, CK_BYTE_PTR pEncryptedPart, CK_ULONG_PTR pulEncryptedPartLen);
    CK_RV (*EncryptFinal) (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pLastEncryptedPart, CK_ULONG_PTR pulLastEncryptedPartLen);

    CK_RV (*DecryptInit)  (Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pEncryptMech, CK_OBJECT_HANDLE hKey);
    CK_RV (*Decrypt)      (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pEncryptedData, CK_ULONG ulEncryptedDataLen, CK_BYTE_PTR pData, CK_ULONG_PTR pulDataLen);
    CK_RV (*DecryptUpdate)(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pEncryptedPart, CK_ULONG ulEncryptedPartLen, CK_BYTE_PTR pPart, CK_ULONG_PTR pulPartLen);
    CK_RV (*DecryptFinal) (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pLastPart, CK_ULONG_PTR pulLastPartLen);
};


struct ICryptokiDigest
{
    CK_RV (*DigestInit)  (Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism);
    CK_RV (*Digest)      (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pDigest, CK_ULONG_PTR pulDigestLen);
    CK_RV (*DigestUpdate)(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen);
    CK_RV (*DigestKey)   (Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hKey);
    CK_RV (*DigestFinal) (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pDigest, CK_ULONG_PTR pulDigestLen);
};

struct ICryptokiSignature
{
    CK_RV (*SignInit)    (Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey);
    CK_RV (*Sign)        (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pSignature, CK_ULONG_PTR pulSignatureLen);
    CK_RV (*SignUpdate)  (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pPart, CK_ULONG ulPartLen);
    CK_RV (*SignFinal)   (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pSignature, CK_ULONG_PTR pulSignatureLen);

    CK_RV (*VerifyInit)  (Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey);
    CK_RV (*Verify)      (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pSignature, CK_ULONG ulSignatureLen);
    CK_RV (*VerifyUpdate)(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pPart, CK_ULONG ulPartLen);
    CK_RV (*VerifyFinal) (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pSignature, CK_ULONG ulSignatureLen);
};

struct ICryptokiKey
{
    CK_RV (*GenerateKey)    (Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, 
                             CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phKey);
    
    CK_RV (*GenerateKeyPair)(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, 
                             CK_ATTRIBUTE_PTR pPublicKeyTemplate  , CK_ULONG ulPublicCount, 
                             CK_ATTRIBUTE_PTR pPrivateKeyTemplate , CK_ULONG ulPrivateCount, 
                             CK_OBJECT_HANDLE_PTR phPublicKey     , CK_OBJECT_HANDLE_PTR phPrivateKey);
    
    CK_RV (*WrapKey  )      (Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism  , CK_OBJECT_HANDLE hWrappingKey, 
                             CK_OBJECT_HANDLE hKey                , CK_BYTE_PTR      pWrappedKey , CK_ULONG_PTR pulWrappedKeyLen);
    
    CK_RV (*UnwrapKey)      (Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hUnwrappingKey, 
                             CK_BYTE_PTR pWrappedKey              , CK_ULONG ulWrappedKeyLen   , CK_ATTRIBUTE_PTR pTemplate     , 
                             CK_ULONG ulAttributeCount            , CK_OBJECT_HANDLE_PTR phKey );
    
    CK_RV (*DeriveKey)      (Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hBaseKey, 
                             CK_ATTRIBUTE_PTR pTemplate           , CK_ULONG ulAttributeCount  , CK_OBJECT_HANDLE_PTR phKey );
    
    CK_RV (*LoadSecretKey)  (Cryptoki_Session_Context* pSessionCtx, CK_KEY_TYPE keyType, const UINT8* pKey, 
                             CK_ULONG ulKeyLength                 , CK_OBJECT_HANDLE_PTR phKey);
    
    CK_RV (*LoadRsaKey)     (Cryptoki_Session_Context* pSessionCtx, const RsaKeyData& keyData, CK_BBOOL isPrivate,                                              CK_OBJECT_HANDLE_PTR phKey);
    CK_RV (*LoadDsaKey)     (Cryptoki_Session_Context* pSessionCtx, const DsaKeyData& keyData, CK_BBOOL isPrivate,                                              CK_OBJECT_HANDLE_PTR phKey);
    CK_RV (*LoadECKey)      (Cryptoki_Session_Context* pSessionCtx, const EcKeyData&  keyData, CK_BBOOL isPrivate,                                              CK_OBJECT_HANDLE_PTR phKey);
    CK_RV (*LoadKeyBlob)    (Cryptoki_Session_Context* pSessionCtx, const PBYTE       pKey   , CK_ULONG keyLen   , CK_KEY_TYPE keyType  , KEY_ATTRIB keyAttrib, CK_OBJECT_HANDLE_PTR phKey);
};

struct ICryptokiObject
{
    CK_RV (*CreateObject)     (Cryptoki_Session_Context* pSessionCtx, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phObject);
    CK_RV (*CopyObject)       (Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phNewObject);
    CK_RV (*DestroyObject)    (Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject);
    CK_RV (*GetObjectSize)    (Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ULONG_PTR pulSize);
    CK_RV (*GetAttributeValue)(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount);
    CK_RV (*SetAttributeValue)(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount);    

    CK_RV (*FindObjectsInit)  (Cryptoki_Session_Context* pSessionCtx, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount);
    CK_RV (*FindObjects)      (Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE_PTR phObjects, CK_ULONG ulMaxCount, CK_ULONG_PTR pulObjectCount);
    CK_RV (*FindObjectsFinal) (Cryptoki_Session_Context* pSessionCtx);
};

struct ICryptokiRandom
{
    CK_RV (*SeedRandom)    (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pSeed      , CK_ULONG ulSeedLen   );
    CK_RV (*GenerateRandom)(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pRandomData, CK_ULONG ulRandomLen );    
};

struct ICryptokiSession
{
    CK_RV (*OpenSession) (Cryptoki_Session_Context* pSessionCtx, CK_BBOOL fReadWrite);
    CK_RV (*CloseSession)(Cryptoki_Session_Context* pSessionCtx);

    CK_RV (*InitPin)     (Cryptoki_Session_Context* pSessionCtx, CK_UTF8CHAR_PTR pPin   , CK_ULONG ulPinLen                                            );
    CK_RV (*SetPin)      (Cryptoki_Session_Context* pSessionCtx, CK_UTF8CHAR_PTR pOldPin, CK_ULONG ulOldLen, CK_UTF8CHAR_PTR pNewPin, CK_ULONG ulNewLen);

    CK_RV (*Login)       (Cryptoki_Session_Context* pSessionCtx, CK_USER_TYPE userType, CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen);
    CK_RV (*Logout)      (Cryptoki_Session_Context* pSessionCtx);
};

struct ICryptokiToken
{
    CK_RV (*Initialize     )();
    CK_RV (*Uninitialize   )();
    CK_RV (*InitializeToken)(CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen, CK_UTF8CHAR_PTR pLabel, CK_ULONG ulLabelLen);
    CK_RV (*GetDeviceError )(CK_ULONG_PTR pErrorCode);
};

struct FileEnumCtx
{
    UINT32 Offset;
    LPCSTR GroupName;
    UINT32 FileType;
};

struct ISecureStorage
{
    BOOL  (*Create     )( LPCSTR fileName , LPCSTR groupName, UINT32  fileType, UINT8* data, UINT32  dataLength );
    BOOL  (*Read       )( LPCSTR fileName , LPCSTR groupName, UINT32& fileType, UINT8* data, UINT32& dataLength );
    BOOL  (*GetFileEnum)( LPCSTR groupName, UINT32 fileType , FileEnumCtx& enumCtx );
    BOOL  (*GetNextFile)( FileEnumCtx& enumCtx, CHAR*fileName,UINT32 fileNameLen );
    BOOL  (*Delete     )( LPCSTR fileName , LPCSTR groupName );
};

struct CryptokiMechanism
{
    CK_MECHANISM_TYPE         Mechanism;    
    CK_MECHANISM_INFO         MechanismInfo;
};

typedef enum _CryptokiSessionState
{
    ReadWriteFlag = 0x8000,
        
    ROPublic = 0,
    ROUser,
    RWPublic            = ROPublic     | ReadWriteFlag,
    RWUser              = ROUser       | ReadWriteFlag,
    RWSecurityOfficer   = (ROUser + 1) | ReadWriteFlag,
} CryptokiSessionState;

#define NETMF_CRYPTOKI_SESSION_IS_RW(x) \
    ((x) & ReadWriteFlag)

#define NETMF_CRYPTOKI_SESSION_IS_PUBLIC(x) \
    (((x) & (~ReadWriteFlag)) == ROPublic)

#define NETMF_CRYPTOKI_SESSION_STATE_CONVERT(x) \
    NETMF_CRYPTOKI_SESSION_IS_RW(x) ? ((x) & (~ReadWriteFlag)) + ROUser + 1 :  (x)

#define NETMF_CRYPTOKI_SESSION_STATE_CONVERT_TOKEN(x, rw) \
    (x == ROUser + 1) ? CKS_RW_SO_FUNCTIONS : NETMF_CRYPTOKI_SESSION_IS_RW(rw) ? (x) + ROUser + 1 :  (x)


struct CryptokiToken
{
public:
    CK_TOKEN_INFO             TokenInfo;
    CryptokiSessionState      TokenWideState;
    CK_MECHANISM              PinWrappingMechanism;
    LPCWSTR                   PinWrappingKey;
    UINT32                    MaxProcessingBytes;
    UINT32                    MechanismCount;
    CryptokiMechanism*        Mechanisms;
    
    
public:
    ICryptokiToken*           TokenState;
    ICryptokiEncryption*      Encryption;
    ICryptokiDigest*          Digest;
    ICryptokiSignature*       Signature;
    ICryptokiKey*             KeyMgmt;
    ICryptokiObject*          ObjectMgmt;
    ICryptokiRandom*          Random;
    ICryptokiSession*         SessionMgmt;
    ISecureStorage*           Storage;
};

struct CryptokiSession
{
    CK_SESSION_HANDLE         SessionHandle;
    CK_SLOT_ID                SlotID;
    CK_VOID_PTR               ApplicationData;  /* passed to callback */
    CK_NOTIFY                 Notify;           /* callback function */
    CryptokiSessionState      State;
    BOOL                      IsLoginContext;
    Cryptoki_Session_Context  Context;

    CryptokiToken*            Token;

#ifdef _DEBUG
    CK_OBJECT_HANDLE          SessionObjects[40];
    INT32                     SessionObjectCount;

    BOOL RemoveObjectHandle(INT32 hObject)
    {
        int i;

        if(SessionObjectCount == 0) return FALSE;
    
        for(i=0; i<ARRAYSIZE(SessionObjects); i++)
        {
            if(SessionObjects[i] == hObject)
            {
                SessionObjectCount--;
                SessionObjects[i] = CK_OBJECT_HANDLE_INVALID;
                return TRUE;
            }
        }

        return FALSE;
    }

    BOOL AddObjectHandle(INT32 hObject)
    {
        int i;

        if(SessionObjectCount >= ARRAYSIZE(SessionObjects))
        {
            return FALSE;
        }
        
        for(i=0; i<ARRAYSIZE(SessionObjects); i++)
        {
            if(SessionObjects[i] == CK_OBJECT_HANDLE_INVALID)
            {
                SessionObjectCount++;
                SessionObjects[i] = hObject;
                return TRUE;
            }
        }

        return FALSE;
    }
#endif
};

struct CryptokiSlot
{
    CK_SLOT_INFO_PTR  SlotInfo;
    CryptokiToken*    Token;
};

extern CryptokiSlot  g_CryptokiSlots[];
extern const UINT32  g_CryptokiSlotCount;
extern CK_RV Cryptoki_GetSlotIDFromSession(CK_SESSION_HANDLE session, CK_SLOT_ID_PTR pSlotID, CryptokiSession** ppSession);
extern void Cryptoki_InitializeSession();
extern void Cryptoki_InitializeSlots();
extern void Cryptoki_SetSlotEvent(CK_SLOT_ID slotID);
extern BOOL Cryptoki_ConnectSlotEventSink(int slotID, void* pContext, PFNSlotEvent pfnEvtHandler);
extern void Cryptoki_PostSlotEvent(void* pContext, unsigned int event);

extern CK_SLOT_ID Cryptoki_FindSlot(LPCSTR szProvider, CK_MECHANISM_TYPE_PTR mechs, INT32 mechCount);

CK_RV Cryptoki_GetTokenMaxProcessingBytes(CK_SLOT_ID slotID, CK_ULONG_PTR pMaxProcessingBytes);

extern BOOL g_isCryptokiInitialized;

#endif //_NETMF_CRYPTOKIPAL_H_
