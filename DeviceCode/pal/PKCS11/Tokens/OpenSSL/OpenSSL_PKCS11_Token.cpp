#include "OpenSSL_pkcs11.h"
#include <objects/objects.h>
#include <EVP/evp.h>
#include <Crypto/crypto.h>

CK_RV PKCS11_Token_OpenSSL::Initialize()
{
    OpenSSL_add_all_algorithms();

    return CKR_OK;
}
CK_RV PKCS11_Token_OpenSSL::Uninitialize()
{
    CRYPTO_cleanup_all_ex_data();
    EVP_cleanup();
    
    return CKR_OK;
}

CK_RV PKCS11_Token_OpenSSL::InitializeToken(CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen, CK_UTF8CHAR_PTR pLabel, CK_ULONG ulLabelLen)
{
    PKCS11_Objects_OpenSSL::IntitializeObjects();
    
    return CKR_OK;
}

CK_RV PKCS11_Token_OpenSSL::GetDeviceError(CK_ULONG_PTR pErrorCode)
{
    *pErrorCode = 0;
    return CKR_OK;
}
    

