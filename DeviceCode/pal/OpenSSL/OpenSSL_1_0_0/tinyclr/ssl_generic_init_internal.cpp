#include <tinyclr/ssl_functions.h>
#include <openssl/ssl.h>
#include <openssl/err.h>
#include <openssl.h>
#include <PKCS11\Tokens\OpenSSL\OpenSSL_pkcs11.h>

extern CK_RV Cryptoki_GetSlotIDFromSession(CK_SESSION_HANDLE session, CK_SLOT_ID_PTR pSlotID, CryptokiSession** ppSession);

BOOL ssl_generic_init_internal( int sslMode, int sslVerify, const char* certificate, 
    int cert_len, const char* szCertPwd, int& sslContextHandle, BOOL isServer )
{
    SSL*                ssl = NULL;
    SSL_CTX*            ctx = NULL;
    SSL_METHOD*         meth = NULL;
    X509*               cert_x509 = NULL;
    EVP_PKEY*           pkey = NULL;

    int                 sslCtxIndex = -1;

    for(int i=0; i<ARRAYSIZE(g_SSL_Driver.m_sslContextArray); i++)
    { 
        if(g_SSL_Driver.m_sslContextArray[i].SslContext == NULL)
        {
            sslCtxIndex = i;           
            break;
        }
    }
    
    if(sslCtxIndex == -1) return FALSE;

    if(isServer)
    {
        // TODO:  we should be setting up the CA lsit
         //SSL_CTX_set_client_CA_list

        if(sslMode & TINYCLR_SSL_MODE_TLS1)
        {
            meth = (SSL_METHOD*)TLSv1_server_method();
        }
        else
        {
            meth = (SSL_METHOD*)SSLv3_server_method();  
        }
    }
    else
    {
        if(sslMode & TINYCLR_SSL_MODE_TLS1)
        {
            meth = (SSL_METHOD*)TLSv1_client_method();
        }
        else
        {
            meth = (SSL_METHOD*)SSLv3_client_method();  
        }
    }

    // SSL_CTX is freed along with SSL in ssl_exit_context_internal
    ctx = SSL_CTX_new (meth);

    if (ctx == NULL)
    {
        ERR_print_errors_fp(OPENSSL_TYPE__FILE_STDERR);
        goto err;
    }

    if(certificate != NULL && cert_len > 0)
    {
        // Certificate Handle passed
        if(cert_len == sizeof(INT32))
        {
            CryptokiSession* pSession;
            CK_SLOT_ID  slotID;
            OBJECT_DATA* pObj;
            CERT_DATA* pCert;
            
            if(CKR_OK != Cryptoki_GetSlotIDFromSession(sslContextHandle, &slotID, &pSession)) return FALSE;

            pObj = PKCS11_Objects_OpenSSL::GetObjectFromHandle(&pSession->Context, *(INT32*)certificate);

            if(pObj == NULL || pObj->Type != CertificateType) return FALSE;

            pCert = (CERT_DATA*)pObj->Data;

            cert_x509 = pCert->cert;
            pkey = (EVP_PKEY*)pCert->privKeyData.key;

            g_SSL_Driver.m_sslContextArray[sslCtxIndex].CryptokiSession = sslContextHandle;
        }
        else
        {
            cert_x509 = ssl_parse_certificate((void*)certificate, cert_len, szCertPwd, &pkey);

            if (cert_x509 == NULL || pkey == NULL)
            {
                ERR_print_errors_fp(OPENSSL_TYPE__FILE_STDERR);
                goto err;
            }
        }
    }

    if(cert_x509 != NULL)
    {
        if (SSL_CTX_use_certificate(ctx, cert_x509) <= 0) 
        {
            ERR_print_errors_fp(OPENSSL_TYPE__FILE_STDERR);
            goto err;
        }
    }

    if(pkey != NULL)
    {
        if (SSL_CTX_use_PrivateKey(ctx, pkey) <= 0) 
        {
            ERR_print_errors_fp(OPENSSL_TYPE__FILE_STDERR);
            goto err;
        }
        
        if (!SSL_CTX_check_private_key(ctx)) 
        {
            TINYCLR_SSL_FPRINTF(OPENSSL_TYPE__FILE_STDERR,
                "Private key does not match the certificate public key\n");
            goto err;
        }
    }

    // create the SSL object
    ssl = SSL_new(ctx);
    if (ssl == NULL)
    {
        ERR_print_errors_fp(OPENSSL_TYPE__FILE_STDERR);
        goto err;
    }
    
    if (ssl == NULL) goto err;

    if(cert_len != sizeof(INT32))
    {
        if(cert_x509 != NULL) X509_free(cert_x509);
    }
    

    // TINYCLR_SSL_VERIFY_XXX >> 1 == SSL_VERIFY_xxx
    ssl->verify_mode = (sslVerify >> 1);

    g_SSL_Driver.m_sslContextArray[sslCtxIndex].SslContext = ssl;
    g_SSL_Driver.m_sslContextCount++;

    sslContextHandle = sslCtxIndex;

    return (ctx != NULL);

err:
    if(ssl != NULL) SSL_free(ssl);
    if(ctx != NULL) SSL_CTX_free(ctx);
    
    if(cert_len != sizeof(INT32))
    {
        X509_free(cert_x509);
        EVP_PKEY_free(pkey);
    }
    
    return FALSE;
}

