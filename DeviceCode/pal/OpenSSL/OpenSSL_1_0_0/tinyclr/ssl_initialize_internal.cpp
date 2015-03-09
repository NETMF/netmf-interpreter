#include <tinyclr/ssl_functions.h>
#include <openssl/ssl.h>
#include <openssl.h>

extern INT64 s_TimeUntil1900;

BOOL ssl_initialize_internal()
{
    TINYCLR_SSL_MEMSET(&g_SSL_Driver, 0, sizeof(g_SSL_Driver));

    s_TimeUntil1900 = 0;

    SSL_COMP_init();
    SSL_library_init();
    SSL_load_error_strings();

    return TRUE;
}

