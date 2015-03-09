#include <tinyclr/ssl_functions.h>
#include <openssl/ssl.h>
#include <openssl.h>

BOOL ssl_exit_context_internal(int sslContextHandle )
{
    if((sslContextHandle >= ARRAYSIZE(g_SSL_Driver.m_sslContextArray)) || (sslContextHandle < 0) || (g_SSL_Driver.m_sslContextArray[sslContextHandle].SslContext == NULL))
    {
        return FALSE;
    }

    SSL *ssl = (SSL*)g_SSL_Driver.m_sslContextArray[sslContextHandle].SslContext;
    
    SSL_free(ssl);

    TINYCLR_SSL_MEMSET(&g_SSL_Driver.m_sslContextArray[sslContextHandle], 0, sizeof(g_SSL_Driver.m_sslContextArray[sslContextHandle]));

    g_SSL_Driver.m_sslContextCount --;

    return TRUE;
}


