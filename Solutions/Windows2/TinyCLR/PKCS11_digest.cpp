#include "pkcs11.h"

CK_RV PKCS11_Digest_Windows::DigestInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism)
{
    int hHmac  = -1;

    if(pMechanism->pParameter != NULL && pMechanism->ulParameterLen == sizeof(int))
    {
        hHmac = *(int*)pMechanism->pParameter;
    }
    
    if(!EmulatorNative::GetIDigestDriver()->DigestInit((int)pSessionCtx->TokenCtx, (int)pMechanism->mechanism, hHmac)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}

CK_RV PKCS11_Digest_Windows::Digest(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pDigest, CK_ULONG_PTR pulDigestLen)
{
    if(!EmulatorNative::GetIDigestDriver()->Digest((int)pSessionCtx->TokenCtx, (IntPtr)pData, (int)ulDataLen, (IntPtr)pDigest, (int%)*pulDigestLen)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}

CK_RV PKCS11_Digest_Windows::Update(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen)
{
    if(!EmulatorNative::GetIDigestDriver()->DigestUpdate((int)pSessionCtx->TokenCtx, (IntPtr)pData, (int)ulDataLen)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}

CK_RV PKCS11_Digest_Windows::DigestKey(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hKey)
{
    if(!EmulatorNative::GetIDigestDriver()->DigestKey((int)pSessionCtx->TokenCtx, (int)hKey)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}

CK_RV PKCS11_Digest_Windows::Final(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pDigest, CK_ULONG_PTR pulDigestLen)
{
    if(!EmulatorNative::GetIDigestDriver()->DigestFinal((int)pSessionCtx->TokenCtx, (IntPtr)pDigest, (int%)*pulDigestLen)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}

