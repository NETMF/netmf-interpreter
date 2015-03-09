#include "OpenSSL_pkcs11.h"

CK_RV PKCS11_Session_OpenSSL::InitPin(Cryptoki_Session_Context* pSessionCtx, CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Session_OpenSSL::SetPin(Cryptoki_Session_Context* pSessionCtx, CK_UTF8CHAR_PTR pOldPin, CK_ULONG ulOldPinLen, CK_UTF8CHAR_PTR pNewPin, CK_ULONG ulNewPinLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Session_OpenSSL::OpenSession(Cryptoki_Session_Context* pSessionCtx, CK_BBOOL fReadWrite)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Session_OpenSSL::CloseSession(Cryptoki_Session_Context* pSessionCtx)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Session_OpenSSL::Login(Cryptoki_Session_Context* pSessionCtx, CK_USER_TYPE userType, CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Session_OpenSSL::Logout(Cryptoki_Session_Context* pSessionCtx)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

