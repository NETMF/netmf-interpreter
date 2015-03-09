#include <e_os.h>
#include <tinyclr/ssl_functions.h>
#include <openssl/ssl.h>
#include <openssl.h>
#include <PKCS11\Tokens\OpenSSL\OpenSSL_pkcs11.h>

extern CK_RV Cryptoki_GetSlotIDFromSession(CK_SESSION_HANDLE session, CK_SLOT_ID_PTR pSlotID, CryptokiSession** ppSession);

int ssl_connect_internal(int sd, const char* szTargetHost, int sslContextHandle)
{
    int err = SOCK_SOCKET_ERROR;
    SSL *ssl = NULL;
    int nonblock = 0;

    // Retrieve SSL struct from g_SSL_Driver    
    if((sslContextHandle >= ARRAYSIZE(g_SSL_Driver.m_sslContextArray)) || (sslContextHandle < 0))
    {
        goto error;
    }
    
    // sd should already have been created
    // Now do the SSL negotiation   
    ssl = (SSL*)g_SSL_Driver.m_sslContextArray[sslContextHandle].SslContext;
    if (ssl == NULL) goto error;

    if (!SSL_set_fd(ssl, sd))
    {
        goto error;
    }

    if(ssl->verify_mode != SSL_VERIFY_NONE)
    {
        SSL_CTX* pCtx = SSL_get_SSL_CTX(ssl);

        if(pCtx != NULL)
        {
            X509_STORE *pStore = SSL_CTX_get_cert_store(pCtx);

            if(sk_num(&pStore->objs->stack) == 0)
            {
                CryptokiSession* pSession;
                CK_SLOT_ID  slotID;
                OBJECT_DATA* pObj;
                CK_ATTRIBUTE attribs[2];
                CK_OBJECT_CLASS cls = SwapEndianIfBEc32(CKO_CERTIFICATE);
                LPSTR label = "CA";
                CK_SESSION_HANDLE hSess;

                if(CKR_OK == C_OpenSession(0, CKF_SERIAL_SESSION, NULL, NULL, &hSess) && 
                   CKR_OK == Cryptoki_GetSlotIDFromSession(hSess, &slotID, &pSession))
                {
                    attribs[0].type = CKA_CLASS;
                    attribs[0].pValue = &cls;
                    attribs[0].ulValueLen = sizeof(cls);

                    attribs[1].type = CKA_LABEL;
                    attribs[1].pValue = label;
                    attribs[1].ulValueLen = 2;

                    if(CKR_OK == C_FindObjectsInit(hSess, attribs, ARRAYSIZE(attribs)))
                    {
                        CK_OBJECT_HANDLE hObjs[20];
                        CK_ULONG cnt = 0;

                        if(CKR_OK == C_FindObjects(hSess, hObjs, ARRAYSIZE(hObjs), &cnt) && cnt > 0)
                        {
                            for(int i=0; i<cnt; i++)
                            {
                                pObj = PKCS11_Objects_OpenSSL::GetObjectFromHandle(&pSession->Context, hObjs[i]);

                                if(pObj != NULL && pObj->Type == 3 /*CertificateType*/)
                                {
                                    CERT_DATA* pCert = (CERT_DATA*)pObj->Data;

                                    X509_STORE_add_cert(pStore, pCert->cert);
                                }
                            }
                        }

                        C_FindObjectsFinal(hSess);
                    }
                }

                if(pStore->objs == NULL || 0 == sk_num(&pStore->objs->stack))
                {
                    ssl->verify_mode = SSL_VERIFY_NONE;
                }

                C_CloseSession(hSess);
            }
        }
    }

    if(szTargetHost != NULL && szTargetHost[0] != 0)
    {
        SSL_set_tlsext_host_name(ssl, szTargetHost);
    }

    SOCK_ioctl(sd, SOCK_FIONBIO, &nonblock);

    err = SSL_connect (ssl);    

    nonblock = 1;
    SOCK_ioctl(sd, SOCK_FIONBIO, &nonblock);

    err = SSL_get_error(ssl,err);
      
    if(err == SSL_ERROR_WANT_READ)
    {
        err = SOCK_EWOULDBLOCK;
#if !defined(TCPIP_LWIP) && !defined(TCPIP_LWIP_OS)
        SOCKET_DRIVER.ClearStatusBitsForSocket( sd, FALSE );        
#endif
    }
    else if(err == SSL_ERROR_WANT_WRITE)
    {
        err = SOCK_TRY_AGAIN;
#if !defined(TCPIP_LWIP) && !defined(TCPIP_LWIP_OS)
        SOCKET_DRIVER.ClearStatusBitsForSocket( sd, TRUE );        
#endif
    }

    SOCKET_DRIVER.SetSocketSslData(sd, (void*)ssl);

error:
    return err;
}

