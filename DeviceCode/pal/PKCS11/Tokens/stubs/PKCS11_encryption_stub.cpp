#include "pkcs11_stub.h"

CK_RV PKCS11_Encryption_stub::EncryptInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pEncryptMech, CK_OBJECT_HANDLE hKey)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Encryption_stub::Encrypt(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pEncryptedData, CK_ULONG_PTR pulEncryptedDataLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Encryption_stub::EncryptUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pPart, CK_ULONG ulPartLen, CK_BYTE_PTR pEncryptedPart, CK_ULONG_PTR pulEncryptedPartLen)   
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Encryption_stub::EncryptFinal(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pLastEncryptedPart, CK_ULONG_PTR pulLastEncryptedPartLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}    

CK_RV PKCS11_Encryption_stub::DecryptInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pDecryptMech, CK_OBJECT_HANDLE hKey)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Encryption_stub::Decrypt(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pEncryptedData, CK_ULONG ulEncryptedDataLen, CK_BYTE_PTR pData, CK_ULONG_PTR pulDataLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}    

CK_RV PKCS11_Encryption_stub::DecryptUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pEncryptedPart, CK_ULONG ulEncryptedPartLen, CK_BYTE_PTR pPart, CK_ULONG_PTR pulPartLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}    

CK_RV PKCS11_Encryption_stub::DecryptFinal(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pLastPart, CK_ULONG_PTR pulLastPartLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}    


