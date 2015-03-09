////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#include "openssl.h"
#include "e_os.h"   // from openssl
#include <tinyclr/ssl_functions.h>
#include <objects/objects.h>
//--//

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_SSL_Driver"
#endif

SSL_Driver g_SSL_Driver;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

// Flag to postpone init until after heap has been cleared
// in tinyhal.cpp
static BOOL s_init_done = FALSE;

extern "C"
{
void RAND_seed(const void *buf, int num);

void ssl_rand_seed(const void *seed, int length)
{
    RAND_seed(seed, length);
}
}

#ifdef TCIP_LWIP

void HAL_SOCK_SignalSocketThread()
{
}

#endif


BOOL SSL_Initialize()
{
    NATIVE_PROFILE_PAL_COM();

    memset(&g_SSL_Driver, 0, sizeof(g_SSL_Driver));
    
    return TRUE;
}

BOOL SSL_Uninitialize()
{
    NATIVE_PROFILE_PAL_COM();
    BOOL retVal;

    retVal = ssl_uninitialize_internal();

    s_init_done = FALSE;
    
    return retVal;
}

static BOOL SSL_GenericInit( INT32 sslMode, INT32 sslVerify, const char* certificate, INT32 cert_len, const char* pwd, INT32& sslContextHandle, BOOL isServer )
{
    if (!s_init_done) s_init_done=ssl_initialize_internal();
    return ssl_generic_init_internal( sslMode, sslVerify, certificate, cert_len, pwd, sslContextHandle, isServer );     
}

void SSL_GetTime(DATE_TIME_INFO* pdt)
{
    // Only used internally by RTIP SSL implementation
}

void SSL_RegisterTimeCallback(SSL_DATE_TIME_FUNC pfn)
{
    NATIVE_PROFILE_PAL_COM();
    g_SSL_Driver.m_pfnGetTimeFuncPtr = pfn;
}

BOOL SSL_ServerInit( INT32 sslMode, INT32 sslVerify, const char* certificate, INT32 cert_len, const char* szCertPwd, INT32& sslContextHandle )
{
    NATIVE_PROFILE_PAL_COM();
    return SSL_GenericInit( sslMode, sslVerify, certificate, cert_len, szCertPwd, sslContextHandle, TRUE );
}

BOOL SSL_ClientInit( INT32 sslMode, INT32 sslVerify, const char* certificate, INT32 cert_len, const char* szCertPwd, INT32& sslContextHandle )
{ 
    NATIVE_PROFILE_PAL_COM();
    return SSL_GenericInit( sslMode, sslVerify, certificate, cert_len, szCertPwd, sslContextHandle, FALSE );
}

BOOL SSL_AddCertificateAuthority( int sslContextHandle, const char* certificate, int cert_len, const char* szCertPwd )
{
    return ssl_add_cert_auth_internal(sslContextHandle, certificate, cert_len, szCertPwd);    
}

void SSL_ClearCertificateAuthority( int sslContextHandle )
{
    ssl_clear_cert_auth_internal(sslContextHandle);
}

BOOL SSL_ExitContext( INT32 sslContextHandle )
{ 
    return ssl_exit_context_internal(sslContextHandle);
}

INT32 SSL_Accept( SOCK_SOCKET socket, INT32 sslContextHandle )
{ 
    NATIVE_PROFILE_PAL_COM();
    return ssl_accept_internal(socket, sslContextHandle);
}

INT32 SSL_Connect( SOCK_SOCKET socket, const char* szTargetHost, INT32 sslContextHandle )
{ 
    NATIVE_PROFILE_PAL_COM();
    return ssl_connect_internal(socket, szTargetHost, sslContextHandle);
}


INT32 SSL_Write( SOCK_SOCKET socket, const char* Data, size_t size  )
{ 
    NATIVE_PROFILE_PAL_COM();
    return ssl_write_internal(socket, Data, size);
}

INT32 SSL_Read( SOCK_SOCKET socket, char* Data, size_t size )
{ 
    NATIVE_PROFILE_PAL_COM();
    return ssl_read_internal(socket, Data, size);
}

INT32 SSL_CloseSocket( SOCK_SOCKET socket )
{
    return ssl_closesocket_internal(socket);
}

BOOL SSL_ParseCertificate( const char* certificate, size_t certLength, const char* szPwd, X509CertData* certData )
{
    if (!s_init_done) s_init_done=ssl_initialize_internal();
    NATIVE_PROFILE_PAL_COM();
    return ssl_parse_certificate_internal((void *)certificate,
                                          TINYCLR_SSL_STRLEN(certificate),
                                          NULL, (void*)certData);
}

INT32 SSL_DataAvailable( SOCK_SOCKET socket )
{
    return ssl_pending_internal(socket);
}

