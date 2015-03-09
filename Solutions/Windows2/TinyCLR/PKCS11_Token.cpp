#include "pkcs11.h"

#define PKCS11_TOKEN_LABEL_LEN 32

static BOOL s_IsTokenInit = FALSE;
static CK_UTF8CHAR s_TokenPin[100];
static CK_UTF8CHAR s_Label[PKCS11_TOKEN_LABEL_LEN+1];

CK_RV PKCS11_Token_Windows::Initialize()
{
    return CKR_OK;
}

CK_RV PKCS11_Token_Windows::Uninitialize()
{
    return CKR_OK;
}

CK_RV PKCS11_Token_Windows::InitializeToken(CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen, CK_UTF8CHAR_PTR pLabel, CK_ULONG ulLabelLen)
{
    CK_ULONG i;
    
    if(!s_IsTokenInit)
    {
        if((ulPinLen-1) >= ARRAYSIZE(s_TokenPin)) return CKR_PIN_LEN_RANGE;
        
        memcpy(s_TokenPin, pPin, ulPinLen);
        
        s_TokenPin[ulPinLen] = 0;

        s_IsTokenInit = TRUE;
    }
    else
    {
        if(hal_strlen_s((const char*)s_TokenPin) != ulPinLen) return CKR_PIN_INCORRECT;

        for(i=0; i<ulPinLen; i++)
        {
            if(pPin[i] != s_TokenPin[i]) return CKR_PIN_INCORRECT;
        }
        
        //destroy all objects on token
        
    }

    if(ulLabelLen > PKCS11_TOKEN_LABEL_LEN) ulLabelLen = PKCS11_TOKEN_LABEL_LEN;
    
    memcpy(s_Label, pLabel, ulLabelLen);
    
    for(i=(int)ulLabelLen; i<PKCS11_TOKEN_LABEL_LEN; i++)
    {
        s_Label[i] = ' ';
    }
    
    s_Label[PKCS11_TOKEN_LABEL_LEN] = 0;
   
    return CKR_OK;
}

CK_RV PKCS11_Token_Windows::GetDeviceError(CK_ULONG_PTR pErrorCode)
{
    *pErrorCode = 0;
    return CKR_OK;
}
    

