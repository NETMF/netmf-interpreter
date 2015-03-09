#include <tinyclr/ssl_functions.h>
#include <openssl/ssl.h>
#include <openssl.h>
#include <PKCS12\PKCS12.h>
#include <PKCS11\Tokens\OpenSSL\OpenSSL_pkcs11.h>

extern CK_RV Cryptoki_GetSlotIDFromSession(CK_SESSION_HANDLE session, CK_SLOT_ID_PTR pSlotID, CryptokiSession** ppSession);

BOOL ssl_add_cert_auth_internal( int sslContextHandle, const char* certificate, 
	int cert_len, const char* szCertPwd )
{
    SSL *ssl = NULL;
    int ret = FALSE;
    X509 *x=NULL;

    if((sslContextHandle >= ARRAYSIZE(g_SSL_Driver.m_sslContextArray)) || (sslContextHandle < 0))
    {
        goto error;
    }

    ssl = (SSL*)g_SSL_Driver.m_sslContextArray[sslContextHandle].SslContext;
    if (ssl == NULL)
    {
        goto error;
    }

    if(cert_len == sizeof(INT32))
    {
        CryptokiSession* pSession;
        CK_SLOT_ID  slotID;
        OBJECT_DATA* pObj;
        CERT_DATA* pCert;
        CK_SESSION_HANDLE sessCtx;

        if(szCertPwd == NULL) return FALSE;

        sessCtx = *(INT32*)szCertPwd;
        
        if(CKR_OK != Cryptoki_GetSlotIDFromSession(sessCtx, &slotID, &pSession)) return FALSE;

        pObj = PKCS11_Objects_OpenSSL::GetObjectFromHandle(&pSession->Context, *(INT32*)certificate);

        if(pObj == NULL || pObj->Type != CertificateType) return FALSE;

        pCert = (CERT_DATA*)pObj->Data;

        x = pCert->cert;
    }
    else
    {
        x = ssl_parse_certificate((void*)certificate, cert_len, szCertPwd, NULL);
    }

    if(x != NULL)
    {
        X509_NAME* pName = X509_get_subject_name(x);

        if(pName)
        {
            SSL_CTX* pCtx = SSL_get_SSL_CTX(ssl);

            if(pCtx == NULL) 
            {
                if(cert_len != sizeof(INT32)) X509_free(x);   
                    
                return FALSE;
            }

            ret = X509_STORE_add_cert(SSL_CTX_get_cert_store(pCtx), x);
        }
    }

error:
	return ret;
}


