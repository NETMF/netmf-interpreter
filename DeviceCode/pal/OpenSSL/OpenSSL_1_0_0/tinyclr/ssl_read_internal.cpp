#include <e_os.h>
#include <tinyclr/ssl_functions.h>
#include <openssl/ssl.h>
#include <openssl.h>

int ssl_read_internal( int sd, char* data, size_t size )
{
    SSL *ssl = (SSL*)SOCKET_DRIVER.GetSocketSslData(sd);

    if(ssl == NULL || ssl == (void*)SSL_SOCKET_ATTEMPTED_CONNECT)
    {
        return SOCK_SOCKET_ERROR;
    }
    
    int read = SSL_read( ssl, data, size ); 

    int err = SSL_get_error(ssl,read);

    if(err == SSL_ERROR_WANT_READ)
    {
#if !defined(TCPIP_LWIP) && !defined(TCPIP_LWIP_OS)
        SOCKET_DRIVER.ClearStatusBitsForSocket( sd, FALSE );
#endif
        return SSL_RESULT__WOULD_BLOCK;
    }

    if(SSL_DataAvailable(sd) <= 0)
    {
#if !defined(TCPIP_LWIP) && !defined(TCPIP_LWIP_OS)
        SOCKET_DRIVER.ClearStatusBitsForSocket( sd, FALSE );
#endif
    }

    return read;
}

