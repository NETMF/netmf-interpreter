#include "pkcs11_stub.h"

CK_RV PKCS11_Keys_stub::GenerateKey(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phKey)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Keys_stub::GenerateKeyPair(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, 
                         CK_ATTRIBUTE_PTR pPublicKeyTemplate , CK_ULONG ulPublicCount, 
                         CK_ATTRIBUTE_PTR pPrivateKeyTemplate, CK_ULONG ulPrivateCount, 
                         CK_OBJECT_HANDLE_PTR phPublicKey    , CK_OBJECT_HANDLE_PTR phPrivateKey)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Keys_stub::WrapKey(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hWrappingKey, CK_OBJECT_HANDLE hKey, CK_BYTE_PTR pWrappedKey, CK_ULONG_PTR pulWrappedKeyLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Keys_stub::UnwrapKey(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hUnwrappingKey, CK_BYTE_PTR pWrappedKey, CK_ULONG ulWrappedKeyLen, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulAttributeCount, CK_OBJECT_HANDLE_PTR phKey)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Keys_stub::DeriveKey(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hBaseKey, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulAttributeCount, CK_OBJECT_HANDLE_PTR phKey)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Keys_stub::LoadKeyBlob(Cryptoki_Session_Context* pSessionCtx, const PBYTE pKey, CK_ULONG keyLen, CK_KEY_TYPE keyType, KEY_ATTRIB keyAttrib, CK_OBJECT_HANDLE_PTR phKey )
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Keys_stub::LoadSecretKey(Cryptoki_Session_Context* pSessionCtx, CK_KEY_TYPE keyType, const UINT8* pKey, CK_ULONG ulKeyLength, CK_OBJECT_HANDLE_PTR phKey)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Keys_stub::LoadRsaKey(Cryptoki_Session_Context* pSessionCtx, const RsaKeyData& keyData, CK_BBOOL isPrivate, CK_OBJECT_HANDLE_PTR phKey)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Keys_stub::LoadDsaKey(Cryptoki_Session_Context* pSessionCtx, const DsaKeyData& keyData, CK_BBOOL isPrivate, CK_OBJECT_HANDLE_PTR phKey)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Keys_stub::LoadEcKey(Cryptoki_Session_Context* pSessionCtx, const EcKeyData&  keyData, CK_BBOOL isPrivate, CK_OBJECT_HANDLE_PTR phKey)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

