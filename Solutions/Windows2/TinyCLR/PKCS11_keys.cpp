#include "pkcs11.h"


CK_RV PKCS11_Keys_Windows::GenerateKey(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phKey )
{
    if(pTemplate != NULL && pTemplate->type == CKA_VALUE_LEN)
    {
        int keySize = *(int*)pTemplate->pValue;

        if(!EmulatorNative::GetIKeyManagementDriver()->GenerateKey((int)pSessionCtx->TokenCtx, (int)pMechanism->mechanism, keySize, (int%)*phKey)) return CKR_FUNCTION_FAILED;
    }
    else
    {
        return CKR_TEMPLATE_INCOMPLETE;
    }

    return CKR_OK;
}

CK_RV PKCS11_Keys_Windows::GenerateKeyPair(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, 
                         CK_ATTRIBUTE_PTR pPublicKeyTemplate , CK_ULONG ulPublicCount, 
                         CK_ATTRIBUTE_PTR pPrivateKeyTemplate, CK_ULONG ulPrivateCount, 
                         CK_OBJECT_HANDLE_PTR phPublicKey    , CK_OBJECT_HANDLE_PTR phPrivateKey)
{
    if(pPublicKeyTemplate != NULL && pPublicKeyTemplate->type == CKA_VALUE_LEN)
    {
        int keySize = *(int*)pPublicKeyTemplate->pValue;

        if(!EmulatorNative::GetIKeyManagementDriver()->GenerateKeyPair((int)pSessionCtx->TokenCtx, (int)pMechanism->mechanism, keySize, (int%)*phPublicKey, (int%)*phPrivateKey)) return CKR_FUNCTION_FAILED;
    }
    else
    {
        return CKR_TEMPLATE_INCOMPLETE;
    }

    return CKR_OK;
}

CK_RV PKCS11_Keys_Windows::WrapKey  (Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hWrappingKey, CK_OBJECT_HANDLE hKey, CK_BYTE_PTR pWrappedKey, CK_ULONG_PTR pulWrappedKeyLen)
{
    return EmulatorNative::GetIKeyManagementDriver()->WrapKey((int)pSessionCtx->TokenCtx, (int)pMechanism->mechanism, (int)hWrappingKey, (IntPtr)pWrappedKey, (int%)*pulWrappedKeyLen) ? CKR_OK : CKR_FUNCTION_FAILED;
}

CK_RV PKCS11_Keys_Windows::UnwrapKey(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hUnwrappingKey, CK_BYTE_PTR pWrappedKey, CK_ULONG ulWrappedKeyLen, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulAttributeCount, CK_OBJECT_HANDLE_PTR phKey )
{
    return EmulatorNative::GetIKeyManagementDriver()->UnWrapKey((int)pSessionCtx->TokenCtx, (int)pMechanism->mechanism, (int)hUnwrappingKey, (IntPtr)pWrappedKey, (int)ulWrappedKeyLen, (int%)*phKey) ? CKR_OK : CKR_FUNCTION_FAILED;
}

CK_RV PKCS11_Keys_Windows::DeriveKey(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hBaseKey, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulAttributeCount, CK_OBJECT_HANDLE_PTR phKey )
{
    return EmulatorNative::GetIKeyManagementDriver()->DeriveKey((int)pSessionCtx->TokenCtx, (int)pMechanism->mechanism, (IntPtr)pMechanism->pParameter, (int)pMechanism->ulParameterLen, (int)hBaseKey, (int%)*phKey) ? CKR_OK : CKR_FUNCTION_FAILED;
}

CK_RV PKCS11_Keys_Windows::LoadSecretKey(Cryptoki_Session_Context* pSessionCtx, CK_KEY_TYPE keyType, const UINT8* pKey, CK_ULONG ulKeyLength,  CK_OBJECT_HANDLE_PTR phKey)
{
    Microsoft::SPOT::Emulator::PKCS11::KeyType kt = (Microsoft::SPOT::Emulator::PKCS11::KeyType)keyType;
        
    return EmulatorNative::GetIKeyManagementDriver()->LoadSymmetricKey((int)pSessionCtx->TokenCtx, (IntPtr)(UINT8*)pKey, (int)ulKeyLength, kt, (int%)*phKey) ? CKR_OK : CKR_FUNCTION_FAILED;
}

CK_RV PKCS11_Keys_Windows::LoadRsaKey(Cryptoki_Session_Context* pSessionCtx, const RsaKeyData& keyData, CK_BBOOL isPrivate, CK_OBJECT_HANDLE_PTR phKey)
{
    BLOB keyBlob;
    RsaKeyBlob<1024> rsaKey;
    int len;

    if(isPrivate)
    {
        len = rsaKey.CopyFromPrivate(keyData);
    }
    else
    {
        len = rsaKey.CopyFromPublic(keyData);
    }

    keyBlob.cbSize = len;
    keyBlob.pBlobData = (BYTE*)rsaKey.Key;

    Microsoft::SPOT::Emulator::PKCS11::KeyType kt = (Microsoft::SPOT::Emulator::PKCS11::KeyType)CKK_RSA;
    
    return EmulatorNative::GetIKeyManagementDriver()->LoadAsymmetricKey((int)pSessionCtx->TokenCtx, (IntPtr)&keyBlob, sizeof(keyBlob), kt, (int%)*phKey) ? CKR_OK : CKR_FUNCTION_FAILED;
}

CK_RV PKCS11_Keys_Windows::LoadDsaKey(Cryptoki_Session_Context* pSessionCtx, const DsaKeyData& keyData, CK_BBOOL isPrivate, CK_OBJECT_HANDLE_PTR phKey)
{
    BLOB keyBlob;
    DsaKeyBlob<1024, 160> dsaKey;
    int len;

    if(isPrivate)
    {
        len = dsaKey.CopyFromPrivate(keyData);
    }
    else
    {
        len = dsaKey.CopyFromPublic(keyData);
    }

    keyBlob.cbSize = len;
    keyBlob.pBlobData = (BYTE*)dsaKey.Key;

    Microsoft::SPOT::Emulator::PKCS11::KeyType kt = (Microsoft::SPOT::Emulator::PKCS11::KeyType)CKK_DSA;
    
    return EmulatorNative::GetIKeyManagementDriver()->LoadAsymmetricKey((int)pSessionCtx->TokenCtx, (IntPtr)&keyBlob, sizeof(keyBlob), kt, (int%)*phKey) ? CKR_OK : CKR_FUNCTION_FAILED;
}

CK_RV PKCS11_Keys_Windows::LoadEcKey(Cryptoki_Session_Context* pSessionCtx, const EcKeyData& keyData, CK_BBOOL isPrivate, CK_OBJECT_HANDLE_PTR phKey)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Keys_Windows::LoadKeyBlob(Cryptoki_Session_Context* pSessionCtx, const PBYTE pKey, CK_ULONG keyLen, CK_KEY_TYPE keyType, KEY_ATTRIB keyAttrib, CK_OBJECT_HANDLE_PTR phKey)
{
    Microsoft::SPOT::Emulator::PKCS11::KeyType      kt = (Microsoft::SPOT::Emulator::PKCS11::KeyType     )keyType;
    Microsoft::SPOT::Emulator::PKCS11::KeyAttribute ka = (Microsoft::SPOT::Emulator::PKCS11::KeyAttribute)keyAttrib;

    return EmulatorNative::GetIKeyManagementDriver()->LoadKeyBlob((int)pSessionCtx->TokenCtx, (IntPtr)pKey, keyLen, kt, ka, (int%)*phKey) ? CKR_OK : CKR_FUNCTION_FAILED;
}

