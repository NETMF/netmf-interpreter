#include "pkcs11_stub.h"

CK_RV PKCS11_Digest_stub::DigestInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Digest_stub::Digest(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pDigest, CK_ULONG_PTR pulDigestLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Digest_stub::Update(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Digest_stub::DigestKey(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hKey)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Digest_stub::Final(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pDigest, CK_ULONG_PTR pulDigestLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

