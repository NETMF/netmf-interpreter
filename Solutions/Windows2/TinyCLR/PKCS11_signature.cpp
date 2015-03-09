#include "pkcs11.h"


CK_RV PKCS11_Signature_Windows::SignInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey)
{
    int hashMech = -1;
    
    if(pMechanism->pParameter && pMechanism->ulParameterLen == sizeof(CK_MECHANISM_TYPE))
    {
        hashMech = *(int*)pMechanism->pParameter;
    }
    
    if(!EmulatorNative::GetISignatureDriver()->SignInit((int)pSessionCtx->TokenCtx, (int)pMechanism->mechanism, hashMech, hKey)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}

CK_RV PKCS11_Signature_Windows::Sign(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pSignature, CK_ULONG_PTR pulSignatureLen)
{
    if(!EmulatorNative::GetISignatureDriver()->Sign((int)pSessionCtx->TokenCtx, (IntPtr)pData, (int)ulDataLen, (IntPtr)pSignature, (int%)*pulSignatureLen)) return CKR_FUNCTION_FAILED;

    return (*pulSignatureLen > 0 ? CKR_OK : CKR_FUNCTION_FAILED);
}

CK_RV PKCS11_Signature_Windows::SignUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pPart, CK_ULONG ulPartLen)
{
    if(!EmulatorNative::GetISignatureDriver()->SignUpdate((int)pSessionCtx->TokenCtx, (IntPtr)pPart, (int)ulPartLen)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}

CK_RV PKCS11_Signature_Windows::SignFinal(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pSignature, CK_ULONG_PTR pulSignatureLen)
{
    if(!EmulatorNative::GetISignatureDriver()->SignUpdate((int)pSessionCtx->TokenCtx, (IntPtr)pSignature, (int%)*pulSignatureLen)) return CKR_FUNCTION_FAILED;

    return (*pulSignatureLen > 0 ? CKR_OK : CKR_FUNCTION_FAILED);
}

CK_RV PKCS11_Signature_Windows::VerifyInit(Cryptoki_Session_Context* pSessionCtx, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey)
{
    int hashMech = -1;
    
    if(pMechanism->pParameter && pMechanism->ulParameterLen == sizeof(CK_MECHANISM_TYPE))
    {
        hashMech = *(int*)pMechanism->pParameter;
    }

    if(!EmulatorNative::GetISignatureDriver()->VerifyInit((int)pSessionCtx->TokenCtx, (int)pMechanism->mechanism, hashMech, hKey)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}

CK_RV PKCS11_Signature_Windows::Verify(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pSignature, CK_ULONG ulSignatureLen)
{
    bool fVerify = EmulatorNative::GetISignatureDriver()->Verify((int)pSessionCtx->TokenCtx, (IntPtr)pData, (int)ulDataLen, (IntPtr)pSignature, (int)ulSignatureLen);

    return (fVerify ? CKR_OK : CKR_FUNCTION_FAILED);
}

CK_RV PKCS11_Signature_Windows::VerifyUpdate(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pPart, CK_ULONG ulPartLen)
{
    if(!EmulatorNative::GetISignatureDriver()->VerifyUpdate((int)pSessionCtx->TokenCtx, (IntPtr)pPart, (int)ulPartLen)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}

CK_RV PKCS11_Signature_Windows::VerifyFinal(Cryptoki_Session_Context* pSessionCtx, CK_BYTE_PTR pSignature, CK_ULONG ulSignatureLen)
{
    bool fVerify = EmulatorNative::GetISignatureDriver()->VerifyFinal((int)pSessionCtx->TokenCtx, (IntPtr)pSignature, (int)ulSignatureLen);

    return (fVerify ? CKR_OK : CKR_FUNCTION_FAILED);
}


