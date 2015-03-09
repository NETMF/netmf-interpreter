#include "pkcs11_stub.h"

CK_RV PKCS11_Token_stub::Initialize()
{
    return CKR_OK;
}
CK_RV PKCS11_Token_stub::Uninitialize()
{
    return CKR_OK;
}

CK_RV PKCS11_Token_stub::InitializeToken(CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen, CK_UTF8CHAR_PTR pLabel, CK_ULONG ulLabelLen)
{
    return CKR_OK;
}

CK_RV PKCS11_Token_stub::GetDeviceError(CK_ULONG_PTR pErrorCode)
{
    *pErrorCode = 0;
    return CKR_OK;
}
    

