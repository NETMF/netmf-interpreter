#include <tinyclr/ssl_functions.h>
#include <openssl/ssl.h>
#include <openssl.h>

int ssl_closesocket_internal( int sd )
{
    int err = 0;
    SSL *ssl = (SSL*)SOCKET_DRIVER.GetSocketSslData(sd);

    if(ssl == NULL || ssl == (void*)SSL_SOCKET_ATTEMPTED_CONNECT)
    {
        return SOCK_SOCKET_ERROR;
    }

    SOCKET_DRIVER.SetSocketSslData(sd, NULL);
    SOCKET_DRIVER.UnregisterSocket(sd);
    
    SSL_shutdown (ssl);  /* send SSL/TLS close_notify */

    SOCK_close( sd );

    return err;
}

