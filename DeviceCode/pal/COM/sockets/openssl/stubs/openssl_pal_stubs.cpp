////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

//--//

extern "C"
{
__weak void ssl_rand_seed(const void *seed, int length)
{
}
}

__weak BOOL SSL_Initialize()
{
    NATIVE_PROFILE_PAL_COM();
    return FALSE;
}

__weak BOOL SSL_Uninitialize()
{
    NATIVE_PROFILE_PAL_COM();
    return TRUE;
}
__weak BOOL SSL_ServerInit( INT32 sslMode, INT32 sslVerify, const char* certificate, INT32 cert_len, const char* szCertPwd, INT32& sslContextHandle )
{ 
    NATIVE_PROFILE_PAL_COM();
    return TRUE; 
}

__weak BOOL SSL_ClientInit( INT32 sslMode, INT32 sslVerify, const char* certificate, INT32 cert_len, const char* szCertPwd, INT32& sslContextHandle )
{ 
    NATIVE_PROFILE_PAL_COM();
    return TRUE; 
}

__weak BOOL SSL_AddCertificateAuthority( int sslContextHandle, const char* certificate, int cert_len, const char* szCertPwd )
{
    NATIVE_PROFILE_PAL_COM();
    return TRUE;
}

__weak void SSL_ClearCertificateAuthority( int sslContextHandle )
{
    NATIVE_PROFILE_PAL_COM();
}

__weak BOOL SSL_ExitContext( INT32 sslContextHandle )
{ 
    NATIVE_PROFILE_PAL_COM();
    return TRUE; 
}

__weak INT32 SSL_Accept( SOCK_SOCKET socket, INT32 sslContextHandle )
{ 
    NATIVE_PROFILE_PAL_COM();
    return 0; 
}

__weak INT32 SSL_Connect( SOCK_SOCKET socket, const char* szTargetHost, INT32 sslContextHandle )
{ 
    NATIVE_PROFILE_PAL_COM();
    return 0; 
}

__weak INT32 SSL_Write( SOCK_SOCKET socket, const char* Data, size_t size )
{ 
    NATIVE_PROFILE_PAL_COM();
    return 0; 
}

__weak INT32 SSL_Read( SOCK_SOCKET socket, char* Data, size_t size )
{ 
    NATIVE_PROFILE_PAL_COM();
    return 0; 
}

__weak INT32 SSL_CloseSocket( SOCK_SOCKET socket )
{
    NATIVE_PROFILE_PAL_COM();
    return 0;
}

__weak void SSL_GetTime(DATE_TIME_INFO* pdt)
{
    NATIVE_PROFILE_PAL_COM();
}

__weak void SSL_RegisterTimeCallback(SSL_DATE_TIME_FUNC pfn)
{
    NATIVE_PROFILE_PAL_COM();
}

__weak BOOL SSL_ParseCertificate( const char* certificate, size_t cert_length, const char* szPwd, X509CertData* certData )
{
    NATIVE_PROFILE_PAL_COM();
    return TRUE;
}

__weak INT32 SSL_DataAvailable( SOCK_SOCKET socket )
{
    NATIVE_PROFILE_PAL_COM();
    return 0;
}

