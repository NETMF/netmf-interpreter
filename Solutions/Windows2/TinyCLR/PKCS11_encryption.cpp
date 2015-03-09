#include "pkcs11.h"

CK_RV PKCS11_Encryption_Windows::EncryptInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pEncryptMech, CK_OBJECT_HANDLE hKey)
{
    if(!EmulatorNative::GetIEncryptionDriver()->EncryptInit((int)pSessionCtx->TokenCtx, (int)pEncryptMech->mechanism, (IntPtr)pEncryptMech->pParameter, pEncryptMech->ulParameterLen, hKey)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}

CK_RV PKCS11_Encryption_Windows::Encrypt(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pEncryptedData, CK_ULONG_PTR pulEncryptedDataLen)
{
    if(!EmulatorNative::GetIEncryptionDriver()->Encrypt((int)pSessionCtx->TokenCtx, (IntPtr)pData, (int)ulDataLen, (IntPtr)pEncryptedData, (int%)*pulEncryptedDataLen)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}

CK_RV PKCS11_Encryption_Windows::EncryptUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pPart, CK_ULONG ulPartLen, CK_BYTE_PTR pEncryptedPart, CK_ULONG_PTR pulEncryptedPartLen)   
{
    if(!EmulatorNative::GetIEncryptionDriver()->EncryptUpdate((int)pSessionCtx->TokenCtx, (IntPtr)pPart, (int)ulPartLen, (IntPtr)pEncryptedPart, (int%)*pulEncryptedPartLen)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}

CK_RV PKCS11_Encryption_Windows::EncryptFinal(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pLastEncryptedPart, CK_ULONG_PTR pulLastEncryptedPartLen)
{
    if(!EmulatorNative::GetIEncryptionDriver()->EncryptFinal((int)pSessionCtx->TokenCtx, (IntPtr)pLastEncryptedPart, (int%)*pulLastEncryptedPartLen)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}    

CK_RV PKCS11_Encryption_Windows::DecryptInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pDecryptMech, CK_OBJECT_HANDLE hKey)
{
    if(!EmulatorNative::GetIEncryptionDriver()->DecryptInit((int)pSessionCtx->TokenCtx, (int)pDecryptMech->mechanism, (IntPtr)pDecryptMech->pParameter, pDecryptMech->ulParameterLen, hKey)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}

CK_RV PKCS11_Encryption_Windows::Decrypt(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pEncryptedData, CK_ULONG ulEncryptedDataLen, CK_BYTE_PTR pData, CK_ULONG_PTR pulDataLen)
{
    if(!EmulatorNative::GetIEncryptionDriver()->Decrypt((int)pSessionCtx->TokenCtx, (IntPtr)pEncryptedData, (int)ulEncryptedDataLen, (IntPtr)pData, (int%)*pulDataLen)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}    

CK_RV PKCS11_Encryption_Windows::DecyptUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pEncryptedPart, CK_ULONG ulEncryptedPartLen, CK_BYTE_PTR pPart, CK_ULONG_PTR pulPartLen)
{
    if(!EmulatorNative::GetIEncryptionDriver()->DecryptUpdate((int)pSessionCtx->TokenCtx, (IntPtr)pEncryptedPart, (int)ulEncryptedPartLen, (IntPtr)pPart, (int%)*pulPartLen)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}    

CK_RV PKCS11_Encryption_Windows::DecryptFinal(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pLastPart, CK_ULONG_PTR pulLastPartLen)
{
    if(!EmulatorNative::GetIEncryptionDriver()->DecryptFinal((int)pSessionCtx->TokenCtx, (IntPtr)pLastPart, (int%)*pulLastPartLen)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}    


