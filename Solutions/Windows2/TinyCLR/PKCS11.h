#include <PKCS11\CryptokiPAL.h>

#include "stdafx.h"

using namespace Microsoft::SPOT::Emulator;

#ifndef _WINDOWS2_PKCS11_H_
#define _WINDOWS2_PKCS11_H_ 1

#define UINT32_SWAP_ENDIANNESS(x) (((x) & 0xFF) << 24 | ((x) & 0xFF00) << 8 | ((x) & 0xFF0000) >> 8 | ((x) & 0xFF0000) >> 24)

template <size_t maxKeySize, size_t maxPrimeFactorSize> class DsaKeyBlob
{
public:
    UINT8 Key[4*maxKeySize/8 + 3*maxPrimeFactorSize/8 + sizeof(BLOBHEADER) + sizeof(DSAPUBKEY) + sizeof(UINT32)];
    
    int CopyFromPrivate(DsaKeyData pParams) 
    {
        UINT8* pKey = &Key[0];

        memset(pKey, 0, sizeof(Key));
        
        ASSIGN_BYTE(pKey, PRIVATEKEYBLOB          ); // BlobHeader.bType
        ASSIGN_BYTE(pKey, 2                       ); // BlobHeader.bVersion
        ASSIGN_WORD(pKey, 0                       ); // BlobHeader.reserved
        ASSIGN_DWORD(pKey, CALG_DSA_SIGN           ); // BlobHeader.aiKeyAlg
        ASSIGN_DWORD(pKey, DSA_MAGIC_FOR_PRIVATEKEY); // DsaPubKey.magic
        ASSIGN_DWORD(pKey, pParams.Base_g_len * 8  ); // DsaPubKey.bitlen 
    
        if(pParams.Prime_p    != NULL) be_memcpy( pKey, pParams.Prime_p   , pParams.Prime_p_len    ); pKey += pParams.Prime_p_len;
        if(pParams.Subprime_q != NULL) be_memcpy( pKey, pParams.Subprime_q, pParams.Subprime_q_len ); pKey += pParams.Subprime_q_len;
        if(pParams.Base_g     != NULL) be_memcpy( pKey, pParams.Base_g    , pParams.Base_g_len     ); pKey += pParams.Base_g_len;
        if(pParams.Private_x  != NULL) be_memcpy( pKey, pParams.Private_x , pParams.Private_x_len  ); pKey += pParams.Private_x_len;
        if(pParams.Seed       != NULL) be_memcpy( pKey, pParams.Seed      , pParams.Seed_len       ); pKey += pParams.Seed_len;

        be_memcpy(pKey, &pParams.Counter, sizeof(pParams.Counter)); pKey += sizeof(pParams.Counter);

        //public value
        if(pParams.Public_y   != NULL) 
        {
            be_memcpy( pKey, pParams.Public_y  , pParams.Public_y_len   ); pKey += pParams.Public_y_len;
        }
        else
        {
            // public value == null
            pKey += 4;
        }

        _ASSERTE(((UINT32)pKey - (UINT32)Key) <= ARRAYSIZE(Key));

        return (UINT32)pKey - (UINT32)Key;
    }   
    
    int CopyFromPublic(DsaKeyData pParams) 
    {
        UINT8* pKey = &Key[0];

        memset(pKey, 0, sizeof(Key));
        
        ASSIGN_BYTE(pKey, PUBLICKEYBLOB           ); // BlobHeader.bType
        ASSIGN_BYTE(pKey, 2                       ); // BlobHeader.bVersion
        ASSIGN_WORD(pKey, 0                       ); // BlobHeader.reserved
        ASSIGN_DWORD(pKey, CALG_DSA_SIGN          ); // BlobHeader.aiKeyAlg
        ASSIGN_DWORD(pKey, DSA_MAGIC_FOR_PUBLICKEY); // DsaPubKey.magic
        ASSIGN_DWORD(pKey, pParams.Base_g_len * 8 ); // DsaPubKey.bitlen 
    
        if(pParams.Prime_p    != NULL) be_memcpy( pKey, pParams.Prime_p   , pParams.Prime_p_len    ); pKey += pParams.Prime_p_len;
        if(pParams.Subprime_q != NULL) be_memcpy( pKey, pParams.Subprime_q, pParams.Subprime_q_len ); pKey += pParams.Subprime_q_len;
        if(pParams.Base_g     != NULL) be_memcpy( pKey, pParams.Base_g    , pParams.Base_g_len     ); pKey += pParams.Base_g_len;
        if(pParams.Public_y   != NULL) be_memcpy( pKey, pParams.Public_y  , pParams.Public_y_len   ); pKey += pParams.Public_y_len;
        if(pParams.Seed       != NULL) be_memcpy( pKey, pParams.Seed      , pParams.Seed_len       ); pKey += pParams.Seed_len;

        be_memcpy(pKey, &pParams.Counter, sizeof(pParams.Counter)); pKey += sizeof(pParams.Counter);

        _ASSERTE(((UINT32)pKey - (UINT32)Key) <= ARRAYSIZE(Key));

        return (UINT32)pKey - (UINT32)Key;
    }        
};

struct PKCS11_Token_Windows
{
    static CK_RV Initialize();
    static CK_RV Uninitialize();
    static CK_RV InitializeToken(CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen, CK_UTF8CHAR_PTR pLabel, CK_ULONG ulLabelLen);
    static CK_RV GetDeviceError(CK_ULONG_PTR pErrorCode);
};

struct PKCS11_Encryption_Windows
{

    static CK_RV EncryptInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pEncryptMech, CK_OBJECT_HANDLE hKey);
    static CK_RV Encrypt(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pEncryptedData, CK_ULONG_PTR pulEncryptedDataLen);
    static CK_RV EncryptUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pPart, CK_ULONG ulPartLen, CK_BYTE_PTR pEncryptedPart, CK_ULONG_PTR pulEncryptedPartLen);
    static CK_RV EncryptFinal(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pLastEncryptedPart, CK_ULONG_PTR pulLastEncryptedPartLen);

    static CK_RV DecryptInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pEncryptMech, CK_OBJECT_HANDLE hKey);
    static CK_RV Decrypt(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pEncryptedData, CK_ULONG ulEncryptedDataLen, CK_BYTE_PTR pData, CK_ULONG_PTR pulDataLen);
    static CK_RV DecyptUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pEncryptedPart, CK_ULONG ulEncryptedPartLen, CK_BYTE_PTR pPart, CK_ULONG_PTR pulPartLen);
    static CK_RV DecryptFinal(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pLastPart, CK_ULONG_PTR pulLastPartLen);
};

struct PKCS11_Session_Windows
{
    static CK_RV InitPin(Cryptoki_Session_Context* pSessionCtx,  CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen);
    static CK_RV SetPin(Cryptoki_Session_Context* pSessionCtx,  CK_UTF8CHAR_PTR pOldPin, CK_ULONG ulOldPinLen, CK_UTF8CHAR_PTR pNewPin, CK_ULONG ulNewPinLen);

    static CK_RV OpenSession(Cryptoki_Session_Context* pSessionCtx,  CK_BBOOL fReadWrite);
    static CK_RV CloseSession(Cryptoki_Session_Context* pSessionCtx);

    static CK_RV Login(Cryptoki_Session_Context* pSessionCtx,  CK_USER_TYPE userType, CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen);
    static CK_RV Logout(Cryptoki_Session_Context* pSessionCtx );
};

struct PKCS11_Objects_Windows
{
    static CK_RV CreateObject(Cryptoki_Session_Context* pSessionCtx,  CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phObject);
    static CK_RV CopyObject(Cryptoki_Session_Context* pSessionCtx,  CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phNewObject);
    static CK_RV DestroyObject(Cryptoki_Session_Context* pSessionCtx,  CK_OBJECT_HANDLE hObject);
    static CK_RV GetObjectSize(Cryptoki_Session_Context* pSessionCtx,  CK_OBJECT_HANDLE hObject, CK_ULONG_PTR pulSize);
    static CK_RV GetAttributeValue(Cryptoki_Session_Context* pSessionCtx,  CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount);
    static CK_RV SetAttributeValue(Cryptoki_Session_Context* pSessionCtx,  CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount);    

    static CK_RV FindObjectsInit(Cryptoki_Session_Context* pSessionCtx,  CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount);
    static CK_RV FindObjects(Cryptoki_Session_Context* pSessionCtx,  CK_OBJECT_HANDLE_PTR phObjects, CK_ULONG ulMaxCount, CK_ULONG_PTR pulObjectCount);
    static CK_RV FindObjectsFinal(Cryptoki_Session_Context* pSessionCtx);
};

struct PKCS11_Digest_Windows
{
    static CK_RV DigestInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism);
    static CK_RV Digest(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pDigest, CK_ULONG_PTR pulDigestLen);
    static CK_RV Update(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen);
    static CK_RV DigestKey(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hKey);
    static CK_RV Final(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pDigest, CK_ULONG_PTR pulDigestLen);
};

struct PKCS11_Signature_Windows
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


struct PKCS11_Keys_Windows
{
    static CK_RV GenerateKey       (Cryptoki_Session_Context* pSessionCtx,  CK_MECHANISM_PTR pMechanism, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phKey );
    static CK_RV GenerateKeyPair(Cryptoki_Session_Context* pSessionCtx,  CK_MECHANISM_PTR pMechanism, 
                             CK_ATTRIBUTE_PTR pPublicKeyTemplate , CK_ULONG ulPublicCount, 
                             CK_ATTRIBUTE_PTR pPrivateKeyTemplate, CK_ULONG ulPrivateCount, 
                             CK_OBJECT_HANDLE_PTR phPublicKey    , CK_OBJECT_HANDLE_PTR phPrivateKey);
    static CK_RV WrapKey              (Cryptoki_Session_Context* pSessionCtx,  CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hWrappingKey, CK_OBJECT_HANDLE hKey, CK_BYTE_PTR pWrappedKey, CK_ULONG_PTR pulWrappedKeyLen);
    static CK_RV UnwrapKey(Cryptoki_Session_Context* pSessionCtx,  CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hUnwrappingKey, CK_BYTE_PTR pWrappedKey, CK_ULONG ulWrappedKeyLen, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulAttributeCount, CK_OBJECT_HANDLE_PTR phKey );
    static CK_RV DeriveKey(Cryptoki_Session_Context* pSessionCtx,  CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hBaseKey, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulAttributeCount, CK_OBJECT_HANDLE_PTR phKey );
    static CK_RV LoadSecretKey(Cryptoki_Session_Context* pSessionCtx,  CK_KEY_TYPE keyType, const UINT8* pKey, CK_ULONG ulKeyLength,  CK_OBJECT_HANDLE_PTR phKey);
    static CK_RV LoadRsaKey  (Cryptoki_Session_Context* pSessionCtx,  const RsaKeyData& keyData, CK_BBOOL isPrivate, CK_OBJECT_HANDLE_PTR phKey);
    static CK_RV LoadDsaKey  (Cryptoki_Session_Context* pSessionCtx,  const DsaKeyData& keyData, CK_BBOOL isPrivate, CK_OBJECT_HANDLE_PTR phKey);
    static CK_RV LoadEcKey  (Cryptoki_Session_Context* pSessionCtx,  const EcKeyData& keyData, CK_BBOOL isPrivate, CK_OBJECT_HANDLE_PTR phKey);
    static CK_RV LoadKeyBlob(Cryptoki_Session_Context* pSessionCtx, const PBYTE pKey, CK_ULONG keyLen, CK_KEY_TYPE keyType, KEY_ATTRIB keyAttrib, CK_OBJECT_HANDLE_PTR phKey);
};

struct PKCS11_Random_Windows
{
    static CK_RV SeedRandom(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pSeed, CK_ULONG ulSeedLen );
    static CK_RV GenerateRandom(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pRandomData, CK_ULONG ulRandomLen );    
};

#endif //_WINDOWS2_PKCS11_H_

