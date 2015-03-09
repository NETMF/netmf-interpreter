#include <PKCS11\CryptokiPAL.h>

#ifndef _PKCS11_STUB_H_
#define _PKCS11_STUB_H_ 1

struct PKCS11_Token_stub
{
    static CK_RV Initialize();
    static CK_RV Uninitialize();
    static CK_RV InitializeToken(CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen, CK_UTF8CHAR_PTR pLabel, CK_ULONG ulLabelLen);
    static CK_RV GetDeviceError(CK_ULONG_PTR pErrorCode);
};

struct PKCS11_Encryption_stub
{
    static CK_RV EncryptInit     (Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pEncryptMech, CK_OBJECT_HANDLE hKey);
    static CK_RV Encrypt           (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pEncryptedData, CK_ULONG_PTR pulEncryptedDataLen);
    static CK_RV EncryptUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pPart, CK_ULONG ulPartLen, CK_BYTE_PTR pEncryptedPart, CK_ULONG_PTR pulEncryptedPartLen);
    static CK_RV EncryptFinal    (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pLastEncryptedPart, CK_ULONG_PTR pulLastEncryptedPartLen);

    static CK_RV DecryptInit     (Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pEncryptMech, CK_OBJECT_HANDLE hKey);
    static CK_RV Decrypt     (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pEncryptedData, CK_ULONG ulEncryptedDataLen, CK_BYTE_PTR pData, CK_ULONG_PTR pulDataLen);
    static CK_RV DecryptUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pEncryptedPart, CK_ULONG ulEncryptedPartLen, CK_BYTE_PTR pPart, CK_ULONG_PTR pulPartLen);
    static CK_RV DecryptFinal  (Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pLastPart, CK_ULONG_PTR pulLastPartLen);
};

struct PKCS11_Session_stub
{
    static CK_RV InitPin(Cryptoki_Session_Context* pSessionCtx, CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen);
    static CK_RV SetPin(Cryptoki_Session_Context* pSessionCtx, CK_UTF8CHAR_PTR pOldPin, CK_ULONG ulOldPinLen, CK_UTF8CHAR_PTR pNewPin, CK_ULONG ulNewPinLen);

    static CK_RV OpenSession(Cryptoki_Session_Context* pSessionCtx, CK_BBOOL fReadWrite);
    static CK_RV CloseSession(Cryptoki_Session_Context* pSessionCtx);

    static CK_RV Login(Cryptoki_Session_Context* pSessionCtx, CK_USER_TYPE userType, CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen);
    static CK_RV Logout(Cryptoki_Session_Context* pSessionCtx);
};

struct PKCS11_Objects_stub
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
};

struct PKCS11_Digest_stub
{
    static CK_RV DigestInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism);
    static CK_RV Digest(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pDigest, CK_ULONG_PTR pulDigestLen);
    static CK_RV Update(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen);
    static CK_RV DigestKey(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hKey);
    static CK_RV Final(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pDigest, CK_ULONG_PTR pulDigestLen);
};

struct PKCS11_Signature_stub
{
    static CK_RV SignInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey);
    static CK_RV Sign(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pSignature, CK_ULONG_PTR pulSignatureLen);
    static CK_RV SignUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pPart, CK_ULONG ulPartLen);
    static CK_RV SignFinal(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pSignature, CK_ULONG_PTR pulSignatureLen);

    static CK_RV VerifyInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey);
    static CK_RV Verify(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pSignature, CK_ULONG ulSignatureLen);
    static CK_RV VerifyUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pPart, CK_ULONG ulPartLen);
    static CK_RV VerifyFinal(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pSignature, CK_ULONG ulSignatureLen);
};


struct PKCS11_Keys_stub
{
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
};

struct PKCS11_Random_stub
{
    static CK_RV SeedRandom(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pSeed, CK_ULONG ulSeedLen);
    static CK_RV GenerateRandom(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pRandomData, CK_ULONG ulRandomLen);    
};

#endif //_PKCS11_STUB_H_

