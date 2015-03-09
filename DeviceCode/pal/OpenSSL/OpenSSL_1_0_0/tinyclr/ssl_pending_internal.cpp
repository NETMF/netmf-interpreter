#include <tinyclr/ssl_functions.h>
#include <openssl/ssl.h>
#include <openssl.h>

int ssl_pending_internal( int sd )
{
    int avail = 0;
    SSL *ssl = (SSL*)SOCKET_DRIVER.GetSocketSslData(sd);

    if(ssl == NULL || ssl == (void*)SSL_SOCKET_ATTEMPTED_CONNECT)
    {
        return SOCK_SOCKET_ERROR;
    }
    
    avail = SSL_pending(ssl);  /* send SSL/TLS close_notify */

    return avail;
}


