#include "OpenSSL_pkcs11.h"
#include <PEM\pem.h>
#include <PKCS12\pkcs12.h>
#include <TinyCLR\ssl_functions.h>

OBJECT_DATA PKCS11_Objects_OpenSSL::s_Objects[];


void PKCS11_Objects_OpenSSL::IntitializeObjects()
{
    memset(PKCS11_Objects_OpenSSL::s_Objects, 0, sizeof(PKCS11_Objects_OpenSSL::s_Objects));
}

int PKCS11_Objects_OpenSSL::FindEmptyObjectHandle()
{
    int index;
    
    for(index=0; index<ARRAYSIZE(s_Objects); index++)
    {
        if(s_Objects[index].Data == NULL) break;
    }

    return index < ARRAYSIZE(s_Objects) ? index : -1;
}

OBJECT_DATA* PKCS11_Objects_OpenSSL::GetObjectFromHandle(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject)
{
    OBJECT_DATA* retVal;
        
    if((int)hObject < 0 || hObject >= ARRAYSIZE(s_Objects))
    {
        return NULL;
    }

    retVal = &s_Objects[(int)hObject];

    return (retVal->Data == NULL) ? NULL : retVal;
}


BOOL PKCS11_Objects_OpenSSL::FreeObject(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject)
{
    OBJECT_DATA* retVal;
        
    if((int)hObject < 0 || hObject >= ARRAYSIZE(s_Objects))
    {
        return FALSE;
    }

    retVal = &s_Objects[(int)hObject];

    if(retVal->Data == NULL) return FALSE;

    TINYCLR_SSL_FREE(retVal->Data);

    retVal->Data = NULL;
    retVal->RefCount = 0;

    return TRUE;
}


CK_OBJECT_HANDLE PKCS11_Objects_OpenSSL::AllocObject(Cryptoki_Session_Context* pSessionCtx, ObjectType type, size_t size, OBJECT_DATA** ppData)
{
    int idx = FindEmptyObjectHandle();

    *ppData = NULL;

    if(idx == -1) return idx;

    *ppData = &s_Objects[idx];

    (*ppData)->Type = type;
    (*ppData)->Data = TINYCLR_SSL_MALLOC(size);
    (*ppData)->RefCount = 1;

    if((*ppData)->Data == NULL) return CK_OBJECT_HANDLE_INVALID;

    TINYCLR_SSL_MEMSET((*ppData)->Data, 0, size);

    return (CK_OBJECT_HANDLE)idx;
}


CK_RV PKCS11_Objects_OpenSSL::LoadX509Cert(Cryptoki_Session_Context* pSessionCtx, X509* x, OBJECT_DATA** ppObject, EVP_PKEY* privateKey, CK_OBJECT_HANDLE_PTR phObject)
{
    CERT_DATA* pCert;
    EVP_PKEY*  pubKey  = X509_get_pubkey(x);
    UINT32     keyType = CKK_VENDOR_DEFINED;

    if(phObject == NULL) return CKR_ARGUMENTS_BAD;
    
    *phObject = AllocObject(pSessionCtx, CertificateType, sizeof(CERT_DATA), ppObject);
    
    if(*phObject == CK_OBJECT_HANDLE_INVALID) 
    {
        return CKR_FUNCTION_FAILED;
    }
           
    if(pubKey != NULL)
    {
        switch(pubKey->type)
        {
            case EVP_PKEY_RSA:
            case EVP_PKEY_RSA2:
                keyType = CKK_RSA;
                break;
        
            case EVP_PKEY_DSA:
            case EVP_PKEY_DSA1:
            case EVP_PKEY_DSA2:
            case EVP_PKEY_DSA3:
            case EVP_PKEY_DSA4:
                keyType = CKK_DSA;
                break;
        
            case EVP_PKEY_DH:
                keyType = CKK_DH;
                break;
        
            case EVP_PKEY_EC:
                keyType = CKK_DH;
                break;
        }
    }
    
    
    pCert = (CERT_DATA*)(*ppObject)->Data;
    
    pCert->cert = x;
    pCert->privKeyData.key = privateKey;
    if(privateKey != NULL)
    {
        pCert->privKeyData.attrib = Private;
        pCert->privKeyData.type   = keyType;
        pCert->privKeyData.size   = EVP_PKEY_bits(privateKey);
    }
    
    pCert->pubKeyData.key    = pubKey;
    pCert->pubKeyData.attrib = Public;
    pCert->pubKeyData.type   = keyType;
    pCert->pubKeyData.size   = EVP_PKEY_bits(pubKey);
    
    return CKR_OK;
}


CK_RV PKCS11_Objects_OpenSSL::CreateObject(Cryptoki_Session_Context* pSessionCtx, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phObject)
{
    UINT32 attribIndex = 0;
    
    if(pTemplate[attribIndex].type == CKA_CLASS && pTemplate[attribIndex].ulValueLen == sizeof(CK_OBJECT_CLASS))
    {
        CK_OBJECT_CLASS cls = SwapEndianIfBEc32(*(CK_OBJECT_CLASS*)pTemplate[attribIndex].pValue);
        CHAR pwd[64];
        int len;

        switch(cls)
        {
            case CKO_CERTIFICATE:
                for(++attribIndex; attribIndex < ulCount; attribIndex++)
                {
                    switch(pTemplate[attribIndex].type)
                    {
                        case CKA_PASSWORD:
                            len = pTemplate[attribIndex].ulValueLen;

                            if(len >= ARRAYSIZE(pwd))
                            {
                                len = ARRAYSIZE(pwd) - 1;
                            }
                        
                            TINYCLR_SSL_MEMCPY(pwd, pTemplate[attribIndex].pValue, len);

                            pwd[len] = 0;

                            break;

                        case CKA_VALUE:
                        {
                            OBJECT_DATA* pObject = NULL;
                            EVP_PKEY* privateKey = NULL;
                            X509 *x =  ssl_parse_certificate(pTemplate[attribIndex].pValue, pTemplate[attribIndex].ulValueLen, pwd, &privateKey);

                            if (x == NULL)
                            {
                                TINYCLR_SSL_PRINTF("Unable to load certificate\n");
                                return CKR_FUNCTION_FAILED;
                            }
                            else
                            {
                                CK_RV retVal = LoadX509Cert(pSessionCtx, x, &pObject, privateKey, phObject);

                                if(retVal != CKR_OK) X509_free(x);

                                return retVal;
                            }
                            
                        }
                    }
                }
                break;
                
        }
    }

    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Objects_OpenSSL::CopyObject(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phNewObject)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

CK_RV PKCS11_Objects_OpenSSL::DestroyObject(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject)
{
    OBJECT_DATA* pData = GetObjectFromHandle(pSessionCtx, hObject);

    if(pData == NULL) return CKR_OBJECT_HANDLE_INVALID;

    if(--pData->RefCount <= 0)
    {
        if(pData->Type == KeyType)
        {
            PKCS11_Keys_OpenSSL::DeleteKey(pSessionCtx, (KEY_DATA *)pData->Data);
        }
        else if(pData->Type == CertificateType)
        {
            CERT_DATA* pCert = (CERT_DATA*)pData->Data;
            X509_free(pCert->cert);
        }
        
        return FreeObject(pSessionCtx, hObject) == NULL ? CKR_OBJECT_HANDLE_INVALID : CKR_OK;
    }

    return CKR_OK;
}

CK_RV PKCS11_Objects_OpenSSL::GetObjectSize(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ULONG_PTR pulSize)
{
    return CKR_FUNCTION_NOT_SUPPORTED;
}

//MS: copied decoding algo from get_ASN1_UTCTIME of asn1_openssl.lib
//MS: populate DATE_TIME_INFO struct with year,month, day,hour,minute,second,etc
static int ssl_get_ASN1_UTCTIME(const ASN1_UTCTIME *tm, DATE_TIME_INFO *dti)
{
    const char *v;
    int gmt=0;
    int i;
    int y=0,M=0,d=0,h=0,m=0,s=0;

    memset(dti, 0, sizeof(*dti));

    i=tm->length;
    v=(const char *)tm->data;

    if (i < 10) { goto err; }
    if (v[i-1] == 'Z') gmt=1;
    for (i=0; i<10; i++)
        if ((v[i] > '9') || (v[i] < '0')) { goto err; }
    y= (v[0]-'0')*10+(v[1]-'0');
    if (y < 50) y+=100;
    M= (v[2]-'0')*10+(v[3]-'0');
    if ((M > 12) || (M < 1)) { goto err; }
    d= (v[4]-'0')*10+(v[5]-'0');
    h= (v[6]-'0')*10+(v[7]-'0');
    m=  (v[8]-'0')*10+(v[9]-'0');
    if (tm->length >=12 &&
        (v[10] >= '0') && (v[10] <= '9') &&
        (v[11] >= '0') && (v[11] <= '9'))
        s=  (v[10]-'0')*10+(v[11]-'0');

    dti->year = SwapEndianIfBEc32(y+1900);
    dti->month = SwapEndianIfBEc32(M);
    dti->day = SwapEndianIfBEc32(d);
    dti->hour = SwapEndianIfBEc32(h);
    dti->minute = SwapEndianIfBEc32(m);
    dti->second = SwapEndianIfBEc32(s);
    dti->dlsTime = 0; //TODO:HOW to find
    dti->tzOffset = SwapEndianIfBEc32(gmt); //TODO:How to find

    return(1);

    
err:
    TINYCLR_SSL_PRINTF("Bad time value\r\n");
    return(0);
}


CK_RV PKCS11_Objects_OpenSSL::GetAttributeValue(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount)
{
    OBJECT_DATA* pObj = PKCS11_Objects_OpenSSL::GetObjectFromHandle(pSessionCtx, hObject);

    if(pObj == NULL) return CKR_OBJECT_HANDLE_INVALID;

    if(pObj->Type == CertificateType)
    {
        CERT_DATA* pCertData = (CERT_DATA*)pObj->Data;
        X509* pCert = pCertData->cert;
        INT32 valLen = 0;
    
        for(int i=0; i<(int)ulCount; i++)
        {
            switch(pTemplate[i].type)
            {
                case CKA_CLASS:
                    *(INT32*)pTemplate[i].pValue = SwapEndianIfBEc32(CKO_CERTIFICATE);
                    break;
                case CKA_PRIVATE:
                    *(INT32*)pTemplate[i].pValue = SwapEndianIfBEc32(pCertData->privKeyData.key == NULL ? 0 : 1);
                    valLen = sizeof(INT32);
                    break;

                case CKA_VALUE_BITS:
                    {
                        *(INT32*)pTemplate[i].pValue = SwapEndianIfBEc32(pCertData->pubKeyData.size);
                        valLen = sizeof(INT32);
                    }
                    break;
                    
                case CKA_KEY_TYPE:
                    {
                        *(UINT32*)pTemplate[i].pValue = SwapEndianIfBEc32(pCertData->pubKeyData.type);
                        valLen = sizeof(UINT32);
                    }
                    break;

                case CKA_ISSUER:
                    {
                        char* name=X509_NAME_oneline(X509_get_issuer_name(pCert),NULL,0);
                        valLen = TINYCLR_SSL_STRLEN(name) + 1;

                        if(valLen > pTemplate[i].ulValueLen) valLen = pTemplate[i].ulValueLen;
                        
                        hal_strcpy_s((char*)pTemplate[i].pValue, valLen, name);
                        OPENSSL_free(name);
                    }
                    break;

                case CKA_SUBJECT:
                    {
                        char* subject=X509_NAME_oneline(X509_get_subject_name(pCert),NULL,0);
                        valLen = TINYCLR_SSL_STRLEN(subject) + 1;

                        if(valLen > pTemplate[i].ulValueLen) valLen = pTemplate[i].ulValueLen;
                        
                        hal_strcpy_s((char*)pTemplate[i].pValue, valLen, subject);
                        OPENSSL_free(subject);
                    }
                    break;

                case CKA_SERIAL_NUMBER:
                    {
                        ASN1_INTEGER* asn = X509_get_serialNumber(pCert);

                        valLen = asn->length;

                        if(valLen > pTemplate[i].ulValueLen) valLen = pTemplate[i].ulValueLen;

                        TINYCLR_SSL_MEMCPY(pTemplate[i].pValue, asn->data, valLen);
                    }
                    break;

                case CKA_MECHANISM_TYPE:
                    if(pCert->sig_alg != NULL)
                    {
                        int signature_nid;
                        CK_MECHANISM_TYPE type = CKM_VENDOR_DEFINED;
                        
                        signature_nid = OBJ_obj2nid(pCert->sig_alg->algorithm);

                        switch(signature_nid)
                        {
                            case NID_sha1WithRSA:
                            case NID_sha1WithRSAEncryption:
                                //szNid = "1.2.840.113549.1.1.5";
                                type = CKM_SHA1_RSA_PKCS;
                                break;

                            case NID_md5WithRSA:
                            case NID_md5WithRSAEncryption:
                                //szNid = "1.2.840.113549.1.1.4";
                                type = CKM_MD5_RSA_PKCS;
                                break;

                            case NID_sha256WithRSAEncryption:
                                //szNid = "1.2.840.113549.1.1.11";
                                type = CKM_SHA256_RSA_PKCS;
                                break;

                            case NID_sha384WithRSAEncryption:
                                //szNid = "1.2.840.113549.1.1.12";
                                type = CKM_SHA384_RSA_PKCS;
                                break;

                            case NID_sha512WithRSAEncryption:
                                //szNid = "1.2.840.113549.1.1.13";
                                type = CKM_SHA512_RSA_PKCS;
                                break;
                        }

                        valLen = sizeof(CK_MECHANISM_TYPE);
                        *(CK_MECHANISM_TYPE*)pTemplate[i].pValue = SwapEndianIfBEc32(type);
                    }
                    break;

                case CKA_START_DATE:
                    {
                        DATE_TIME_INFO dti;
                        
                        ssl_get_ASN1_UTCTIME(X509_get_notBefore(pCert), &dti);

                        valLen = (INT32)pTemplate[i].ulValueLen;

                        if(valLen > sizeof(dti)) valLen = sizeof(dti);

                        TINYCLR_SSL_MEMCPY(pTemplate[i].pValue, &dti, valLen);
                    }
                    break;

                case CKA_END_DATE:
                    {
                        DATE_TIME_INFO dti;
                        
                        ssl_get_ASN1_UTCTIME(X509_get_notAfter(pCert), &dti);

                        valLen = pTemplate[i].ulValueLen;

                        if(valLen > sizeof(dti)) valLen = sizeof(dti);

                        TINYCLR_SSL_MEMCPY(pTemplate[i].pValue, &dti, valLen);
                    }
                    break;

                case CKA_VALUE:
                    {
                        UINT8* pData = (UINT8*)pTemplate[i].pValue;
                        UINT8* pTmp = pData;

                        valLen = i2d_X509(pCert, NULL);

                        if(valLen > pTemplate[i].ulValueLen) return CKR_DEVICE_MEMORY;
                        
                        valLen = i2d_X509(pCert, &pTmp);

                        if(valLen < 0) return CKR_FUNCTION_FAILED;
                    }
                    break;

                case CKA_VALUE_LEN:
                    *(UINT32*)pTemplate[i].pValue = SwapEndianIfBEc32(valLen);
                    break;

                default:
                    return CKR_ATTRIBUTE_TYPE_INVALID;
            }
        }
    }
    else if(pObj->Type == KeyType)
    {
        KEY_DATA* pKey = (KEY_DATA*)pObj->Data;
        int valLen = 0;
        bool isPrivate = false;

        for(int i=0; i<(int)ulCount; i++)
        {
            switch(pTemplate[i].type)
            {
                case CKA_CLASS:
                    *(INT32*)pTemplate[i].pValue = SwapEndianIfBEc32((0 != (pKey->attrib & Private) ? CKO_PRIVATE_KEY : 0 != (pKey->attrib & Public) ? CKO_PUBLIC_KEY : CKO_SECRET_KEY));
                    break;
                    
                case CKA_MODULUS:
                    if(pKey->type == CKK_RSA)
                    {
                        EVP_PKEY* pRealKey = (EVP_PKEY*)pKey->key;

                        valLen = BN_bn2bin(pRealKey->pkey.rsa->n, (UINT8*)pTemplate[i].pValue);
                    }    
                    break;

                case CKA_EXPONENT_1:
                    if(pKey->type == CKK_RSA)
                    {
                        EVP_PKEY* pRealKey = (EVP_PKEY*)pKey->key;

                        valLen = BN_bn2bin(pRealKey->pkey.rsa->dmp1, (UINT8*)pTemplate[i].pValue);
                    }    
                    break;

                case CKA_EXPONENT_2:
                    if(pKey->type == CKK_RSA)
                    {
                        EVP_PKEY* pRealKey = (EVP_PKEY*)pKey->key;

                        valLen = BN_bn2bin(pRealKey->pkey.rsa->dmq1, (UINT8*)pTemplate[i].pValue);
                    }    
                    break;

                case CKA_COEFFICIENT:
                    if(pKey->type == CKK_RSA)
                    {
                        EVP_PKEY* pRealKey = (EVP_PKEY*)pKey->key;

                        valLen = BN_bn2bin(pRealKey->pkey.rsa->iqmp, (UINT8*)pTemplate[i].pValue);
                    }    
                    break;
                    
                case CKA_PRIME_1:
                    if(pKey->type == CKK_RSA)
                    {
                        EVP_PKEY* pRealKey = (EVP_PKEY*)pKey->key;

                        valLen = BN_bn2bin(pRealKey->pkey.rsa->p, (UINT8*)pTemplate[i].pValue);
                    }    
                    break;

                case CKA_PRIME_2:
                    if(pKey->type == CKK_RSA)
                    {
                        EVP_PKEY* pRealKey = (EVP_PKEY*)pKey->key;

                        valLen = BN_bn2bin(pRealKey->pkey.rsa->q, (UINT8*)pTemplate[i].pValue);
                    }    
                    break;

                
                case CKA_PRIVATE_EXPONENT:
                    if(pKey->type == CKK_DSA)
                    {
                        EVP_PKEY* pRealKey = (EVP_PKEY*)pKey->key;

                        valLen = BN_bn2bin(pRealKey->pkey.dsa->priv_key, (UINT8*)pTemplate[i].pValue);
                    }
                    else if(pKey->type == CKK_RSA)
                    {
                        EVP_PKEY* pRealKey = (EVP_PKEY*)pKey->key;

                        valLen = BN_bn2bin(pRealKey->pkey.rsa->d, (UINT8*)pTemplate[i].pValue);
                    }
                    break;
                case CKA_PUBLIC_EXPONENT:
                    if(pKey->type == CKK_DSA)
                    {
                        EVP_PKEY* pRealKey = (EVP_PKEY*)pKey->key;
                
                        valLen = BN_bn2bin(pRealKey->pkey.dsa->pub_key, (UINT8*)pTemplate[i].pValue);
                    }
                    else if(pKey->type == CKK_EC)
                    {
                        UINT8 pTmp[66*2+1];
                    
                        EC_KEY* pEC = ((EVP_PKEY*)pKey->key)->pkey.ec;
                        
                        const EC_POINT* point = EC_KEY_get0_public_key(pEC);
                        valLen = EC_POINT_point2oct(EC_KEY_get0_group(pEC), point, POINT_CONVERSION_UNCOMPRESSED, (UINT8*)pTmp, ARRAYSIZE(pTmp), NULL);
                    
                        if(valLen == 0) return CKR_FUNCTION_FAILED;
                            
                        memmove(pTemplate[i].pValue, &pTmp[1], valLen-1); // remove POINT_CONVERSION_UNCOMPRESSED header byte
                    }
                    else if(pKey->type == CKK_RSA)
                    {
                        EVP_PKEY* pRealKey = (EVP_PKEY*)pKey->key;

                        valLen = BN_bn2bin(pRealKey->pkey.rsa->e, (UINT8*)pTemplate[i].pValue);
                    }                    
                    break;

                case CKA_PRIME:
                    if(pKey->type == CKK_DSA)
                    {
                        EVP_PKEY* pRealKey = (EVP_PKEY*)pKey->key;

                        valLen = BN_bn2bin(pRealKey->pkey.dsa->p, (UINT8*)pTemplate[i].pValue);
                    }
                    break;

                case CKA_SUBPRIME:
                    if(pKey->type == CKK_DSA)
                    {
                        EVP_PKEY* pRealKey = (EVP_PKEY*)pKey->key;

                        valLen = BN_bn2bin(pRealKey->pkey.dsa->q, (UINT8*)pTemplate[i].pValue);
                    }
                    break;

                case CKA_BASE:
                    if(pKey->type == CKK_DSA)
                    {
                        EVP_PKEY* pRealKey = (EVP_PKEY*)pKey->key;
                        
                        valLen = BN_bn2bin(pRealKey->pkey.dsa->g, (UINT8*)pTemplate[i].pValue);
                    }
                    break;

                case CKA_PRIVATE:
                    isPrivate = 0 != *(INT32*)pTemplate[i].pValue;
                    break;
                

                case CKA_KEY_TYPE:
                    *(INT32*)pTemplate[i].pValue = SwapEndianIfBEc32(pKey->type);
                    break; 

                case CKA_VALUE_BITS:
                    *(UINT32*)pTemplate[i].pValue = SwapEndianIfBEc32(pKey->size);
                    break;

                case CKA_VALUE:
                    switch(pKey->type)
                    {
                        case CKK_AES:
                        case CKK_GENERIC_SECRET:
                            {
                                // TODO: Add permissions to export key
                                int len = (pKey->size + 7) / 8;
                                
                                if(len > (INT32)pTemplate[i].ulValueLen) len = (int)pTemplate[i].ulValueLen;
                                
                                TINYCLR_SSL_MEMCPY(pTemplate[i].pValue, pKey->key, len);
                                valLen = len;
                            }
                            break;

                        default:
                            return CKR_ATTRIBUTE_TYPE_INVALID;                                
                    }
                    break;

                case CKA_VALUE_LEN:
                    switch(pKey->type)
                    {
                        case CKK_EC:
                            *(UINT32*)pTemplate[i].pValue = SwapEndianIfBEc32(valLen);
                            break;

                        default:
                            return CKR_ATTRIBUTE_TYPE_INVALID;
                    }
                    break;

                default:
                    return CKR_ATTRIBUTE_TYPE_INVALID;
            }
        }
    }

    return CKR_OK;    
}

CK_RV PKCS11_Objects_OpenSSL::SetAttributeValue(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount) 
{
    OBJECT_DATA* pObj = PKCS11_Objects_OpenSSL::GetObjectFromHandle(pSessionCtx, hObject);

    if(pObj == NULL) return CKR_OBJECT_HANDLE_INVALID;

    if(pObj->Type == CertificateType)
    {
        CERT_DATA* pCertData = (CERT_DATA*)pObj->Data;
        X509* pCert = pCertData->cert;
        CHAR group[20] = {0};
        CHAR fileName[20] = {0};
        INT32 len = 0;

        BOOL fSave = FALSE;
        BOOL fDelete = FALSE;
    
        for(int i=0; i<(int)ulCount; i++)
        {
            switch(pTemplate[i].type)
            {
                case CKA_PERSIST:
                    fSave = SwapEndianIfBEc32(*(INT32*)pTemplate[i].pValue) != 0;
                    fDelete = !fSave;
                    break;

                case CKA_LABEL:
                    len = pTemplate[i].ulValueLen;

                    if(len >= ARRAYSIZE(group))
                    {
                        len = ARRAYSIZE(group) - 1;
                    }
                    
                    TINYCLR_SSL_MEMCPY(group, pTemplate[i].pValue, len);
                    
                    group[len] = 0;
                    
                    break;

                case CKA_OBJECT_ID:
                    len = pTemplate[i].ulValueLen;

                    if(len >= ARRAYSIZE(fileName))
                    {
                        len = ARRAYSIZE(fileName) - 1;
                    }
                    
                    TINYCLR_SSL_MEMCPY(fileName, pTemplate[i].pValue, len);
                    
                    fileName[len] = 0;
                    
                    break;

                default:
                    return CKR_ATTRIBUTE_TYPE_INVALID;
            }
        }

        if(fDelete)
        {
            hal_strcpy_s(fileName, ARRAYSIZE(pObj->FileName), pObj->FileName);

            pObj->GroupName[0] = 0;
                
            if(g_OpenSSL_Token.Storage != NULL && g_OpenSSL_Token.Storage->Delete != NULL)
            {
                if(!g_OpenSSL_Token.Storage->Delete(fileName, group)) return CKR_FUNCTION_FAILED;
            }
            else
            {
                return CKR_FUNCTION_NOT_SUPPORTED;
            }
        }
        else if(fSave)
        {
            hal_strcpy_s(pObj->GroupName, ARRAYSIZE(pObj->GroupName), group   );
            hal_strcpy_s(pObj->FileName , ARRAYSIZE(pObj->FileName ), fileName);
            
            if(g_OpenSSL_Token.Storage != NULL && g_OpenSSL_Token.Storage->Create != NULL)
            {
                BOOL res;
                UINT8* pData, *pTmp;
                INT32 len = i2d_X509( pCert, NULL );

                if(len <= 0) return CKR_FUNCTION_FAILED;

                pData = (UINT8*)TINYCLR_SSL_MALLOC(len);

                if(pData == NULL) return CKR_DEVICE_MEMORY;

                pTmp = pData;

                i2d_X509(pCert, &pTmp);

                // TODO: Save private key as well
                
                res = g_OpenSSL_Token.Storage->Create( fileName, group, CertificateType, pData, len );

                TINYCLR_SSL_FREE(pData);

                if(!res) return CKR_FUNCTION_FAILED;
            }
            else
            {
                return CKR_FUNCTION_NOT_SUPPORTED;
            }
        }
        else
        {
            return CKR_ATTRIBUTE_TYPE_INVALID;
        }
    }

    return CKR_OK;
}

CK_RV PKCS11_Objects_OpenSSL::FindObjectsInit(Cryptoki_Session_Context* pSessionCtx, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount)
{
    FIND_OBJECT_DATA *pData;
    INT32 i = 0, len;

    if(pSessionCtx->FindObjectsCtx != NULL)
    {
        return CKR_FUNCTION_NOT_PARALLEL;
    }

    pData = (FIND_OBJECT_DATA*)TINYCLR_SSL_MALLOC(sizeof(FIND_OBJECT_DATA));

    if(pData == NULL) return CKR_DEVICE_MEMORY;

    pSessionCtx->FindObjectsCtx = pData;

    pData->FileName[0] = 0;
    pData->GroupName[0] = 0;

    while(i < ulCount)
    {
        switch(pTemplate[i].type)
        {
            case CKA_CLASS:
                switch(SwapEndianIfBEc32(*(INT32*)pTemplate[i].pValue))
                {
                    case CKO_CERTIFICATE:
                        pData->ObjectType = CertificateType;
                        break;

                    case CKO_PUBLIC_KEY:
                    case CKO_SECRET_KEY:
                    case CKO_PRIVATE_KEY:
                        pData->ObjectType = KeyType;
                        break;

                    default:
                        pData->ObjectType = DataType;
                        break;
                }
                break;

            case CKA_LABEL:
                len = pTemplate[i].ulValueLen;

                if(len >= ARRAYSIZE(pData->GroupName))
                {
                    len = ARRAYSIZE(pData->GroupName) - 1;
                }
                
                TINYCLR_SSL_MEMCPY( pData->GroupName, pTemplate[i].pValue, len );

                pData->GroupName[len] = 0;
                break;

            case CKA_OBJECT_ID:
                len = pTemplate[i].ulValueLen;

                if(len >= ARRAYSIZE(pData->FileName))
                {
                    len = ARRAYSIZE(pData->FileName) - 1;
                }

                TINYCLR_SSL_MEMCPY( pData->FileName, pTemplate[i].pValue, len );

                pData->FileName[len] = 0;
                break;

            default:
                return CKR_ATTRIBUTE_TYPE_INVALID;
        }
        i++;
    }   
    
    return CKR_OK;
}

CK_RV PKCS11_Objects_OpenSSL::FindObjects(Cryptoki_Session_Context* pSessionCtx, CK_OBJECT_HANDLE_PTR phObjects, CK_ULONG ulMaxCount, CK_ULONG_PTR pulObjectCount)
{
    FIND_OBJECT_DATA *pData = (FIND_OBJECT_DATA*)pSessionCtx->FindObjectsCtx;
    UINT32 cntObj = 0;

    *pulObjectCount = 0;
    
    if(pData == NULL) return CKR_FUNCTION_FAILED;
    if(ulMaxCount == 0) return CKR_ARGUMENTS_BAD;

    {
        FileEnumCtx ctx;
        CHAR fileName[20];
        
        if(g_OpenSSL_Token.Storage == NULL || g_OpenSSL_Token.Storage->GetFileEnum == NULL)
        {
            return CKR_FUNCTION_NOT_SUPPORTED;
        }

        if(!g_OpenSSL_Token.Storage->GetFileEnum(pData->GroupName, pData->ObjectType, ctx)) return CKR_FUNCTION_FAILED;

        while(g_OpenSSL_Token.Storage->GetNextFile(ctx, fileName, ARRAYSIZE(fileName)))
        {
            UINT32 dataLen = 0;
            
            if(g_OpenSSL_Token.Storage->Read(fileName, pData->GroupName, pData->ObjectType, NULL, dataLen))
            {
                UINT8* pObj = (UINT8*)TINYCLR_SSL_MALLOC(dataLen);

                if(pObj)
                {
                    g_OpenSSL_Token.Storage->Read(fileName, pData->GroupName, pData->ObjectType, pObj, dataLen);
                    
                    switch(pData->ObjectType)
                    {
                        case CertificateType:
                            {
                                OBJECT_DATA *pObject = NULL;
                                const UINT8* pTmp = pObj;
                                CK_OBJECT_HANDLE hObjTmp, *pObjHandle;

                                X509* x509 = d2i_X509(NULL, &pTmp, dataLen);

                                if(x509 == NULL) 
                                {
                                    TINYCLR_SSL_FREE(pObj);    
                                    return CKR_FUNCTION_FAILED;
                                }

                                if(phObjects == NULL)
                                {
                                    pObjHandle = &hObjTmp;
                                }
                                else
                                {
                                    pObjHandle = &phObjects[cntObj];
                                }                                

                                if(CKR_OK != LoadX509Cert(pSessionCtx, x509, &pObject, NULL, pObjHandle))
                                {
                                    X509_free(x509);
                                    TINYCLR_SSL_FREE(pObj);
                                    //return CKR_FUNCTION_FAILED;

                                    g_OpenSSL_Token.Storage->Delete( fileName, pData->GroupName );
                                    continue;
                                }

                                hal_strcpy_s(pObject->GroupName, ARRAYSIZE(pObject->GroupName), pData->GroupName);
                                hal_strcpy_s(pObject->FileName , ARRAYSIZE(pObject->FileName ), fileName );
                                cntObj++;

                                TINYCLR_SSL_FREE(pObj);

                                if(cntObj >= ulMaxCount)
                                {
                                    *pulObjectCount = cntObj;
                                    return CKR_OK;
                                }
                            }
                            break;

                        case KeyType:
                            {
                                // TODO:
                                TINYCLR_SSL_FREE(pObj);
                            }
                            break;

                        default:
                            // TODO:
                            TINYCLR_SSL_FREE(pObj);
                            break;
                    }
                }
            }
        }
    }

    *pulObjectCount = cntObj;

    return CKR_OK;
}

CK_RV PKCS11_Objects_OpenSSL::FindObjectsFinal(Cryptoki_Session_Context* pSessionCtx)
{
    if(pSessionCtx->FindObjectsCtx != NULL)
    {
        TINYCLR_SSL_FREE(pSessionCtx->FindObjectsCtx);
        pSessionCtx->FindObjectsCtx = NULL;
    }
    
    return CKR_OK;
}

