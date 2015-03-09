////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include <pkcs11\cryptokiPAL.h>

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace Microsoft::SPOT::Emulator;
using namespace Microsoft::SPOT::Emulator::Sockets;
using namespace Microsoft::SPOT::Emulator::Sockets::Security;

extern "C"
{
    void ssl_rand_seed(const void *seed, int length)
    {
        
    }
}

BOOL SSL_Initialize()
{
    return EmulatorNative::GetISslDriver()->Initialize();
}

BOOL SSL_Uninitialize()
{
    return EmulatorNative::GetISslDriver()->Uninitialize();
}

extern CK_RV Cryptoki_GetSlotIDFromSession(CK_SESSION_HANDLE session, CK_SLOT_ID_PTR pSlotID, CryptokiSession** ppSession);

BOOL SSL_ServerInit( INT32 sslMode, INT32 sslVerify, const char* certificate, INT32 cert_len, const char* szCertPwd, INT32& sslContextHandle )
{ 
    if(cert_len == sizeof(INT32))
    {
        CryptokiSession* pSession;
        CK_SLOT_ID slotID;
        
        if(CKR_OK != Cryptoki_GetSlotIDFromSession(sslContextHandle, &slotID, &pSession)) return FALSE;

        sslContextHandle = (INT32)pSession->Context.TokenCtx;
    }
    
    return EmulatorNative::GetISslDriver()->ServerInit( sslMode, sslVerify, (IntPtr)(void*)certificate, cert_len, (IntPtr)(void*)szCertPwd, (int%)sslContextHandle );
}

BOOL SSL_ClientInit( INT32 sslMode, INT32 sslVerify, const char* certificate, INT32 cert_len, const char* szCertPwd,INT32& sslContextHandle )
{ 
    if(cert_len == sizeof(INT32))
    {
        CryptokiSession* pSession;
        CK_SLOT_ID slotID;
        
        if(CKR_OK != Cryptoki_GetSlotIDFromSession(sslContextHandle, &slotID, &pSession)) return FALSE;

        sslContextHandle = (INT32)pSession->Context.TokenCtx;
    }

    return EmulatorNative::GetISslDriver()->ClientInit( sslMode, sslVerify, (IntPtr)(void*)certificate, cert_len, (IntPtr)(void*)szCertPwd, (int%)sslContextHandle );
}

BOOL SSL_AddCertificateAuthority( int sslContextHandle, const char* certificate, int cert_len, const char* szCertPwd )
{
    return EmulatorNative::GetISslDriver()->AddCertificateAuthority( sslContextHandle, (IntPtr)(void*)certificate, cert_len, (IntPtr)(void*)szCertPwd );
}

void SSL_ClearCertificateAuthority( int sslContextHandle )
{
    return EmulatorNative::GetISslDriver()->ClearCertificateAuthority( sslContextHandle );
}

BOOL SSL_ExitContext( INT32 sslContextHandle )
{ 
    return EmulatorNative::GetISslDriver()->ExitContext( sslContextHandle );
}

INT32 SSL_Accept( SOCK_SOCKET socket, INT32 sslContextHandle )
{ 
    return EmulatorNative::GetISslDriver()->Accept( socket, sslContextHandle );
}

INT32 SSL_Connect( SOCK_SOCKET socket, const char* szTargetHost, INT32 sslContextHandle )
{ 
    return EmulatorNative::GetISslDriver()->Connect( socket, (IntPtr)(void*)szTargetHost, sslContextHandle );
}

INT32 SSL_Write( SOCK_SOCKET socket, const char* Data, size_t size )
{ 
    return EmulatorNative::GetISslDriver()->Write( socket, (IntPtr)(void*)Data, (INT32)size );
}

INT32 SSL_Read( SOCK_SOCKET socket, char* Data, size_t size )
{ 
    return EmulatorNative::GetISslDriver()->Read( socket, (IntPtr)(void*)Data, (INT32)size );
}

INT32 SSL_CloseSocket( SOCK_SOCKET socket )
{
    return EmulatorNative::GetISslDriver()->CloseSocket( socket );
}

void SSL_GetTime(DATE_TIME_INFO* pdt)
{
    return EmulatorNative::GetISslDriver()->GetTime( (IntPtr)(void*)pdt );    
}

void SSL_RegisterTimeCallback(SSL_DATE_TIME_FUNC pfn)
{
    return EmulatorNative::GetISslDriver()->RegisterTimeCallback( (RegisterTimeCallback^)pfn );
}

BOOL SSL_ParseCertificate( const char* certificate, size_t certLength, const char* szPwd, X509CertData* certData )
{
    return EmulatorNative::GetISslDriver()->ParseCertificate( (IntPtr)(void*)certificate, certLength, (IntPtr)(void*)szPwd, (IntPtr)(void*)certData );
}

INT32 SSL_DataAvailable( SOCK_SOCKET socket )
{
    return EmulatorNative::GetISslDriver()->DataAvailable( socket );
}


