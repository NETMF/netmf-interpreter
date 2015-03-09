#include <tinyclr/ssl_functions.h>
#include <openssl/ssl.h>
#include <openssl.h>

void ssl_clear_cert_auth_internal( int sslContextHandle )
{
    SSL *ssl = NULL;
    
    if((sslContextHandle >= ARRAYSIZE(g_SSL_Driver.m_sslContextArray)) || (sslContextHandle < 0))
    {
        return;
    }

    ssl = (SSL*)g_SSL_Driver.m_sslContextArray[sslContextHandle].SslContext;
    if (ssl == NULL)
    {
        return;
    }

    // Set a NULL cert store, SSL_CTX_set_cert_store will free the existing cert store
    SSL_CTX_set_cert_store( SSL_get_SSL_CTX(ssl), NULL );
}


