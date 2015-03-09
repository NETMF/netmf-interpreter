#include <PKCS11\CryptokiPAL.h>
#include <EVP\evp.h>
#include <HMAC\HMAC.h>

#ifndef _OPENSSL_PKCS11_H_
#define _OPENSSL_PKCS11_H_ 1

#ifndef PKCS11_OPENSSL_MAX_OBJECT_COUNT
#define PKCS11_OPENSSL_MAX_OBJECT_COUNT 40
#endif

#ifndef PKCS11_OPENSSL_MAX_IV_LEN
#define PKCS11_OPENSSL_MAX_IV_LEN 64
#endif

// TODO: Add platform dependent value
#define OPENSSL_MAX_SESSION_COUNT 10


#define OPENSSL_HEADER() \
    CK_RV retVal = CKR_OK

#define OPENSSL_CLEANUP() \
    CleanUp: 

#define OPENSSL_RETURN() \
    return retVal

#define OPENSSL_NOCLEANUP() \
    OPENSSL_CLEANUP(); \
    OPENSSL_RETURN()

#define OPENSSL_LEAVE() \
    goto CleanUp

#define OPENSSL_SET_AND_LEAVE(x) \
{ \
    retVal = x; \
    OPENSSL_LEAVE(); \
}
   
#define OPENSSL_CHECKRESULT(x) \
    if((x) <= 0) OPENSSL_SET_AND_LEAVE(CKR_FUNCTION_FAILED)

#define OPENSSL_CHECK_CK_RESULT(x) \
    if(CKR_OK != (retVal = x)) OPENSSL_LEAVE()


extern CK_SLOT_INFO  g_OpenSSL_SlotInfo;
extern CryptokiToken g_OpenSSL_Token;

typedef struct _KEY_DATA
{
    CK_KEY_TYPE       type;
    CK_ULONG          size;
    KEY_ATTRIB        attrib;
    CK_VOID_PTR       key;
    CK_VOID_PTR       ctx;
} KEY_DATA;

typedef struct _CERT_DATA
{
    X509* cert;
    KEY_DATA pubKeyData;
    KEY_DATA privKeyData;
} CERT_DATA;

typedef enum _ObjectType
{
    KeyType  = 1,
    DataType = 2,
    CertificateType = 3
} ObjectType;

typedef struct _OBJECT_DATA
{
    ObjectType        Type;
    CHAR              FileName[20];
    CHAR              GroupName[20];
    int               RefCount;
    CK_VOID_PTR       Data;
} OBJECT_DATA;


struct PKCS11_Token_OpenSSL
{
    static CK_RV Initialize();
    static CK_RV Uninitialize();
    static CK_RV InitializeToken(CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen, CK_UTF8CHAR_PTR pLabel, CK_ULONG ulLabelLen);
    static CK_RV GetDeviceError(CK_ULONG_PTR pErrorCode);
};

typedef enum _OpenSSLCryptoState
{
    Uninitialized,
    Initialized,
    InProgress,
} OpenSSLCryptoState;

typedef struct _OpenSSLEncryptData
{
    UINT8            IV[PKCS11_OPENSSL_MAX_IV_LEN];
    EVP_CIPHER_CTX   SymmetricCtx;
    BOOL             IsSymmetric;
    KEY_DATA*        Key;
    BOOL             IsUpdateInProgress;
} OpenSSLEncryptData;

struct PKCS11_Encryption_OpenSSL
{
    static CK_RV EncryptInit     (Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pEncryptMech, CK_OBJECT_HANDLE hKey);
    static CK_RV Encrypt           (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pEncryptedData, CK_ULONG_PTR pulEncryptedDataLen);
    static CK_RV EncryptUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pPart, CK_ULONG ulPartLen, CK_BYTE_PTR pEncryptedPart, CK_ULONG_PTR pulEncryptedPartLen);
    static CK_RV EncryptFinal    (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pLastEncryptedPart, CK_ULONG_PTR pulLastEncryptedPartLen);

    static CK_RV DecryptInit     (Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pEncryptMech, CK_OBJECT_HANDLE hKey);
    static CK_RV Decrypt     (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pEncryptedData, CK_ULONG ulEncryptedDataLen, CK_BYTE_PTR pData, CK_ULONG_PTR pulDataLen);
    static CK_RV DecryptUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pEncryptedPart, CK_ULONG ulEncryptedPartLen, CK_BYTE_PTR pPart, CK_ULONG_PTR pulPartLen);
    static CK_RV DecryptFinal  (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pLastPart, CK_ULONG_PTR pulLastPartLen);

private:
    static CK_RV InitHelper(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pEncryptMech, CK_OBJECT_HANDLE hKey, BOOL isEncrypt);
};

struct PKCS11_Session_OpenSSL
{
    static CK_RV InitPin(Cryptoki_Session_Context* pSessionCtx, CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen);
    static CK_RV SetPin(Cryptoki_Session_Context* pSessionCtx, CK_UTF8CHAR_PTR pOldPin, CK_ULONG ulOldPinLen, CK_UTF8CHAR_PTR pNewPin, CK_ULONG ulNewPinLen);

    static CK_RV OpenSession(Cryptoki_Session_Context* pSessionCtx, CK_BBOOL fReadWrite);
    static CK_RV CloseSession(Cryptoki_Session_Context* pSessionCtx);

    static CK_RV Login(Cryptoki_Session_Context* pSessionCtx, CK_USER_TYPE userType, CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen);
    static CK_RV Logout(Cryptoki_Session_Context* pSessionCtx);
};

struct FIND_OBJECT_DATA
{
    UINT32  ObjectType;
    CHAR    FileName[20];
    CHAR    GroupName[20];
};

struct PKCS11_Objects_OpenSSL
{

    static CK_RV CreateObject(Cryptoki_Session_Context* pSessionCtx, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phObject);
    static CK_RV CopyObject(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phNewObject);
    static CK_RV DestroyObject(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject);
    static CK_RV GetObjectSize(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ULONG_PTR pulSize);
    static CK_RV GetAttributeValue(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount);
    static CK_RV SetAttributeValue(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount);    

    static CK_RV FindObjectsInit(Cryptoki_Session_Context* pSessionCtx, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount);
    static CK_RV FindObjects(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE_PTR phObjects, CK_ULONG ulMaxCount, CK_ULONG_PTR pulObjectCount);
    static CK_RV FindObjectsFinal(Cryptoki_Session_Context* pSessionCtx);

    static OBJECT_DATA*     GetObjectFromHandle(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject);
    static BOOL             FreeObject(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject);
    static CK_OBJECT_HANDLE AllocObject(Cryptoki_Session_Context* pSessionCtx, ObjectType type, size_t size, OBJECT_DATA** ppData);

    static void IntitializeObjects();
private:
    static CK_RV LoadX509Cert(Cryptoki_Session_Context* pSessionCtx, X509* x, OBJECT_DATA** ppObject, EVP_PKEY* privateKey, CK_OBJECT_HANDLE_PTR phObject);
    static int FindEmptyObjectHandle();
    static OBJECT_DATA s_Objects[PKCS11_OPENSSL_MAX_OBJECT_COUNT];
};

typedef struct _OpenSSLDigestData
{
    EVP_MD_CTX CurrentCtx;
    HMAC_CTX   HmacCtx;
    KEY_DATA*  HmacKey;
    BOOL       IsUpdateInProgress;
} OpenSSLDigestData;

struct PKCS11_Digest_OpenSSL
{
    static CK_RV DigestInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism);
    static CK_RV Digest(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pDigest, CK_ULONG_PTR pulDigestLen);
    static CK_RV Update(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen);
    static CK_RV DigestKey(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hKey);
    static CK_RV Final(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pDigest, CK_ULONG_PTR pulDigestLen);
};

typedef struct _OpenSSLSignatureData
{
    KEY_DATA*  Key;
    EVP_MD_CTX Ctx;
    BOOL       IsUpdateInProgress;
} OpenSSLSignatureData;


struct PKCS11_Signature_OpenSSL
{
    static CK_RV SignInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey);
    static CK_RV Sign(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pSignature, CK_ULONG_PTR pulSignatureLen);
    static CK_RV SignUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pPart, CK_ULONG ulPartLen);
    static CK_RV SignFinal(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pSignature, CK_ULONG_PTR pulSignatureLen);

    static CK_RV VerifyInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey);
    static CK_RV Verify(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pSignature, CK_ULONG ulSignatureLen);
    static CK_RV VerifyUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pPart, CK_ULONG ulPartLen);
    static CK_RV VerifyFinal(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pSignature, CK_ULONG ulSignatureLen);

private:
    static CK_RV GetDigestFromMech(CK_MECHANISM_PTR pMechanism, const EVP_MD*& pDigest, CK_KEY_TYPE &keyType);
    static CK_RV InitHelper(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey, BOOL isSign);
};


struct PKCS11_Keys_OpenSSL
{
    static CK_RV DeleteKey(Cryptoki_Session_Context* pSessionCtx, KEY_DATA* pKey);
    
    static CK_RV GenerateKey(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phKey);
    static CK_RV GenerateKeyPair(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, 
                             CK_ATTRIBUTE_PTR pPublicKeyTemplate , CK_ULONG ulPublicCount, 
                             CK_ATTRIBUTE_PTR pPrivateKeyTemplate, CK_ULONG ulPrivateCount, 
                             CK_OBJECT_HANDLE_PTR phPublicKey    , CK_OBJECT_HANDLE_PTR phPrivateKey);
    static CK_RV WrapKey  (Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hWrappingKey, CK_OBJECT_HANDLE hKey, CK_BYTE_PTR pWrappedKey, CK_ULONG_PTR pulWrappedKeyLen);
    static CK_RV UnwrapKey(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hUnwrappingKey, CK_BYTE_PTR pWrappedKey, CK_ULONG ulWrappedKeyLen, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulAttributeCount, CK_OBJECT_HANDLE_PTR phKey);
    static CK_RV DeriveKey(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hBaseKey, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulAttributeCount, CK_OBJECT_HANDLE_PTR phKey);

    static CK_RV LoadSecretKey(Cryptoki_Session_Context* pSessionCtx, CK_KEY_TYPE keyType, const UINT8* pKey, CK_ULONG ulKeyLength, CK_OBJECT_HANDLE_PTR phKey);
    static CK_RV LoadRsaKey  (Cryptoki_Session_Context* pSessionCtx, const RsaKeyData& keyData, CK_BBOOL isPrivate, CK_OBJECT_HANDLE_PTR phKey);
    static CK_RV LoadDsaKey  (Cryptoki_Session_Context* pSessionCtx, const DsaKeyData& keyData, CK_BBOOL isPrivate, CK_OBJECT_HANDLE_PTR phKey);
    static CK_RV LoadEcKey   (Cryptoki_Session_Context* pSessionCtx, const EcKeyData&  keyData, CK_BBOOL isPrivate, CK_OBJECT_HANDLE_PTR phKey);
    static CK_RV LoadKeyBlob(Cryptoki_Session_Context* pSessionCtx, const PBYTE pKey, CK_ULONG keyLen, CK_KEY_TYPE keyType, KEY_ATTRIB keyAttrib, CK_OBJECT_HANDLE_PTR phKey);

    //--//

    static KEY_DATA* GetKeyFromHandle(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hKey, BOOL getPrivate);

private:
    static CK_OBJECT_HANDLE LoadKey(Cryptoki_Session_Context* pSessionCtx, void* pKey, CK_KEY_TYPE type, KEY_ATTRIB attrib, size_t keySize);
};

struct PKCS11_Random_OpenSSL
{
    static CK_RV SeedRandom(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pSeed, CK_ULONG ulSeedLen);
    static CK_RV GenerateRandom(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pRandomData, CK_ULONG ulRandomLen);    
};

#endif //_OPENSSL_PKCS11_H_

