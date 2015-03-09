#include "pkcs11.h"


CK_RV PKCS11_Session_Windows::InitPin(Cryptoki_Session_Context* pSessionCtx, CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Session_Windows::SetPin(Cryptoki_Session_Context* pSessionCtx, CK_UTF8CHAR_PTR pOldPin, CK_ULONG ulOldPinLen, CK_UTF8CHAR_PTR pNewPin, CK_ULONG ulNewPinLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Session_Windows::OpenSession(Cryptoki_Session_Context* pSessionCtx, CK_BBOOL fReadWrite)
{
    if(!EmulatorNative::GetISessionDriver()->OpenSession((fReadWrite == CK_TRUE), (int%)pSessionCtx->TokenCtx)) return CKR_FUNCTION_FAILED;
    
    return CKR_OK;
}

CK_RV PKCS11_Session_Windows::CloseSession(Cryptoki_Session_Context* pSessionCtx)
{
    int hSession = (int)pSessionCtx->TokenCtx;

    if(!EmulatorNative::GetISessionDriver()->CloseSession(hSession)) return CKR_FUNCTION_FAILED;
    
    return CKR_OK;
}

CK_RV PKCS11_Session_Windows::Login(Cryptoki_Session_Context* pSessionCtx, CK_USER_TYPE userType, CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen)
{
    int hSession = (int)pSessionCtx->TokenCtx;

    if(!EmulatorNative::GetISessionDriver()->Login(hSession, (int)userType, gcnew String((LPCSTR)pPin))) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}

CK_RV PKCS11_Session_Windows::Logout(Cryptoki_Session_Context* pSessionCtx)
{
    int hSession = (int)pSessionCtx->TokenCtx;

    if(!EmulatorNative::GetISessionDriver()->Logout(hSession)) return CKR_FUNCTION_FAILED;

    return CKR_OK;
}

