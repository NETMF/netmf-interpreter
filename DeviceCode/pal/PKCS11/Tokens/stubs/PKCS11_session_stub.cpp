#include "pkcs11_stub.h"

CK_RV PKCS11_Session_stub::InitPin(Cryptoki_Session_Context* pSessionCtx, CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Session_stub::SetPin(Cryptoki_Session_Context* pSessionCtx, CK_UTF8CHAR_PTR pOldPin, CK_ULONG ulOldPinLen, CK_UTF8CHAR_PTR pNewPin, CK_ULONG ulNewPinLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Session_stub::OpenSession(Cryptoki_Session_Context* pSessionCtx, CK_BBOOL fReadWrite)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Session_stub::CloseSession(Cryptoki_Session_Context* pSessionCtx)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Session_stub::Login(Cryptoki_Session_Context* pSessionCtx, CK_USER_TYPE userType, CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Session_stub::Logout(Cryptoki_Session_Context* pSessionCtx)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

