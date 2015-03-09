#ifndef _NETMF_CRYPTOKI_TOKENS_NETMFCRYPTO_H_
#define _NETMF_CRYPTOKI_TOKENS_NETMFCRYPTO_H_

#include <TinyHAL.h>
#include <crypto.h>
#include <PKCS11\CryptokiPAL.h>

struct NetMFCrypto
{
    static RSAKey  s_RSAKeys[];
    static int     s_RSAKeyIndex;
    static RSAKey* s_pActiveKey;

    static CK_RV GetObjectSize(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ULONG_PTR pulSize);

    static CK_RV EncryptInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pEncryptMech, CK_OBJECT_HANDLE hKey);
    static CK_RV Encrypt(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pEncryptedData, CK_ULONG_PTR pulEncryptedDataLen);
    //static CK_RV EncryptUpdate(CK_BYTE_PTR pPart, CK_ULONG ulPartLen, CK_BYTE_PTR pEncryptedPart, CK_ULONG_PTR pulEncryptedPartLen);
    //static CK_RV EncryptFinal(CK_BYTE_PTR pLastEncryptedPart, CK_ULONG_PTR pulLastEncryptedPartLen);
    
    static CK_RV DecryptInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pEncryptMech, CK_OBJECT_HANDLE hKey);
    static CK_RV Decrypt(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pEncryptedData, CK_ULONG ulEncryptedDataLen, CK_BYTE_PTR pData, CK_ULONG_PTR pulDataLen);
    //static CK_RV DecyptUpdate(CK_BYTE_PTR pEncryptedPart, CK_ULONG ulEncryptedPartLen, CK_BYTE_PTR pPart, CK_ULONG_PTR pulPartLen);
    //static CK_RV DecryptFinal(CK_BYTE_PTR pLastPart, CK_ULONG_PTR pulLastPartLen);

    static CK_RV SignInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey);
    static CK_RV Sign(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pSignature, CK_ULONG_PTR pulSignatureLen);
    //static CK_RV SignUpdate(CK_BYTE_PTR pPart, CK_ULONG ulPartLen);
    //static CK_RV SignFinal(CK_BYTE_PTR pSignature, CK_ULONG_PTR pulSignatureLen);

    static CK_RV VerifyInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey);
    static CK_RV Verify(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pSignature, CK_ULONG ulSignatureLen);
    //static CK_RV VerifyUpdate(CK_BYTE_PTR pPart, CK_ULONG ulPartLen);
    //static CK_RV VerifyFinal(CK_BYTE_PTR pSignature, CK_ULONG ulSignatureLen);


    static CK_RV GenerateKeyPair(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, 
                             CK_ATTRIBUTE_PTR pPublicKeyTemplate , CK_ULONG ulPublicCount, 
                             CK_ATTRIBUTE_PTR pPrivateKeyTemplate, CK_ULONG ulPrivateCount, 
                             CK_OBJECT_HANDLE_PTR phPublicKey    , CK_OBJECT_HANDLE_PTR phPrivateKey);
};

#endif // _NETMF_CRYPTOKI_TOKENS_NETMFCRYPTO_H_
