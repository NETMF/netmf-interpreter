#ifndef SSL_FUNCTIONS_H
#define SSL_FUNCTIONS_H

#include <tinyhal.h>
#include <ossl_typ.h>

#define TINYCLR_SSL_MODE_TLS1 0x10
#define TINYCLR_SSL_MODE_SSL3 0x08

#define TINYCLR_SSL_VERIFY_NONE         0x01
#define TINYCLR_SSL_VERIFY_PEER         0x02
#define TINYCLR_SSL_VERIFY_CERT_REQ     0x04
#define TINYCLR_SSL_VERIFY_CLIENT_ONCE  0x08


// Lifted from Apps.h
#define FORMAT_UNDEF    0
#define FORMAT_ASN1     1
#define FORMAT_TEXT     2
#define FORMAT_PEM      3
#define FORMAT_NETSCAPE 4
#define FORMAT_PKCS12   5
#define FORMAT_SMIME    6
#define FORMAT_ENGINE   7
#define FORMAT_IISSGC   8  

X509* ssl_parse_certificate(void* pCert, size_t certLen, LPCSTR pwd, EVP_PKEY** privateKey);
BOOL ssl_parse_certificate_internal(void* buf, size_t size, void* pwd, void* x509 );
int ssl_connect_internal(int sd, const char* szTargetHost, int sslContextHandle);
int ssl_accept_internal( int socket, int sslContextHandle );
int ssl_read_internal( int socket, char* Data, size_t size );
int ssl_write_internal( int socket, const char* Data, size_t size);
int ssl_closesocket_internal( int sd );
int ssl_pending_internal( int sd );
BOOL ssl_exit_context_internal(int sslContextHandle );
BOOL ssl_generic_init_internal( int sslMode, int sslVerify, const char* certificate, int cert_len, const char* pwd, int& sslContextHandle, BOOL isServer );
BOOL ssl_initialize_internal();
BOOL ssl_uninitialize_internal();
void ssl_clear_cert_auth_internal(int sslContextHandle );
BOOL ssl_add_cert_auth_internal( int sslContextHandle, const char* certificate, 	int cert_len, const char* szCertPwd );

#endif
