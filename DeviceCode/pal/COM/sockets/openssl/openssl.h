////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _SOCKETS_OPENSSL_H_
#define _SOCKETS_OPENSSL_H_ 1

//--//

#include "tinyhal.h"

#if defined(TCPIP_LWIP)
#include "sockets_lwip\sockets_lwip.h"
#define SOCKET_DRIVER g_Sockets_LWIP_Driver
#elif defined(TCPIP_LWIP_OS)
#include "sockets_lwip_os\sockets_lwip.h"
#define SOCKET_DRIVER g_Sockets_LWIP_Driver
#else
#include "../sockets.h"
#define SOCKET_DRIVER g_Sockets_Driver
#endif
//--//

//--//
#define SSL_SOCKET_ATTEMPTED_CONNECT -1
//--//

typedef unsigned char * X509Certificate;

typedef struct
{
    X509Certificate cert;
    int size;
} RTCertificate;


struct SSL_Driver
{
    static const int c_MaxSslContexts = SOCKETS_MAX_COUNT;
    static const int c_MaxCertificatesPerContext = 10;

    //--//

    struct SSL_Context
    {
        void* SslContext;
        INT32 CryptokiSession;

        RTCertificate m_certificates[c_MaxCertificatesPerContext];
        INT32         m_certificateCount;
    };

    SSL_Context        m_sslContextArray[c_MaxSslContexts];
    INT32              m_sslContextCount;   
    SSL_DATE_TIME_FUNC m_pfnGetTimeFuncPtr;

};

extern SSL_Driver g_SSL_Driver;

#endif //_SOCKETS_OPENSSL_H_
