////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _LWIP_SOCKETS_H_
#define _LWIP_SOCKETS_H_

#include <tinyhal.h>
#include "net_decl_lwip.h"
#include <lwip\Init.h>
#include <lwip\Sockets.h>
#include "lwip\dhcp.h"
#include <lwip\Netdb.h>

//////////////////////////////////////////////////////////////////////////////
// SOCKET driver
// 

struct LWIP_DRIVER_INTERFACE_DATA
{
    int          m_interfaceNumber;
};

#define SOCK_SOCKADDR_TO_SOCKADDR(ssa, sa, addrLen) \
  sa.sin_len         = (u8_t)sizeof(sa); \
  sa.sin_family      = (u8_t)((SOCK_sockaddr_in*)ssa)->sin_family; \
  sa.sin_port        = ((SOCK_sockaddr_in*)ssa)->sin_port; \
  sa.sin_addr.s_addr = ((SOCK_sockaddr_in*)ssa)->sin_addr.S_un.S_addr; \
  memcpy(sa.sin_zero, ((SOCK_sockaddr_in*)ssa)->sin_zero, sizeof(sa.sin_zero)); \
  *addrLen = sizeof(sa)

#define SOCKADDR_TO_SOCK_SOCKADDR(ssa, sa, addrLen) \
    ((SOCK_sockaddr_in*)ssa)->sin_port             = sa.sin_port; \
    ((SOCK_sockaddr_in*)ssa)->sin_addr.S_un.S_addr = sa.sin_addr.s_addr; \
    ((SOCK_sockaddr_in*)ssa)->sin_family           = sa.sin_family; \
    memcpy(((SOCK_sockaddr_in*)ssa)->sin_zero, sa.sin_zero, sizeof(((SOCK_sockaddr_in*)ssa)->sin_zero)); \
    *addrLen = sizeof(SOCK_sockaddr_in)


struct LWIP_SOCKETS_Driver
{
    static BOOL Initialize();
    
    static BOOL Uninitialize();
    
    static SOCK_SOCKET Socket( int family, int type, int protocol );
    
    static int Bind( SOCK_SOCKET socket, const SOCK_sockaddr* address, int addressLen  );

    static int Connect(SOCK_SOCKET socket, const SOCK_sockaddr* address, int addressLen);

    static int Send(SOCK_SOCKET socket, const char* buf, int len, int flags);

    static int Recv(SOCK_SOCKET socket, char* buf, int len, int flags);

    static int Close(SOCK_SOCKET socket);

    static int Listen(SOCK_SOCKET socket, int backlog );

    static SOCK_SOCKET Accept(SOCK_SOCKET socket, SOCK_sockaddr* address, int* addressLen );


    static int Shutdown(SOCK_SOCKET socket, int how );

    static int GetAddrInfo(const char* nodename, 
                  char* servname, 
                  const SOCK_addrinfo* hints, 
                  SOCK_addrinfo** res );

    static void FreeAddrInfo(SOCK_addrinfo* ai );

    static int Ioctl(SOCK_SOCKET socket, int cmd, int* data );

    static int GetLastError();

    static int GetNativeTcpOption (int optname);

    static int GetNativeSockOption (int optname);

    static int GetNativeError( int error );

    static int GetNativeIPOption (int optname);

    static int Select(int nfds, SOCK_fd_set* readfds, SOCK_fd_set* writefds, SOCK_fd_set* exceptfds, const SOCK_timeval* timeout );

    static int SetSockOpt(SOCK_SOCKET socket, int level, int optname, const char* optval, int  optlen );

    static int GetSockOpt(SOCK_SOCKET socket, int level, int optname, char* optval, int* optlen );

    static int GetPeerName(SOCK_SOCKET socket, SOCK_sockaddr* name, int* namelen );
    
    static int GetSockName(SOCK_SOCKET socket, SOCK_sockaddr* name, int* namelen );

    static int RecvFrom(SOCK_SOCKET s, char* buf, int len, int flags, SOCK_sockaddr* from, int* fromlen );
    
    static int SendTo(SOCK_SOCKET s, const char* buf, int len, int flags, const SOCK_sockaddr* to, int tolen );

    static UINT32 GetAdapterCount();

    static HRESULT LoadAdapterConfiguration( UINT32 interfaceIndex, SOCK_NetworkConfiguration* config );
    
    static HRESULT UpdateAdapterConfiguration( UINT32 interfaceIndex, UINT32 updateFlags, SOCK_NetworkConfiguration* config );

    static HRESULT LoadWirelessConfiguration( UINT32 interfaceIndex, SOCK_WirelessConfiguration* wirelessConfig );


private:
    LWIP_DRIVER_INTERFACE_DATA m_interfaces[NETWORK_INTERFACE_COUNT];
};

//
// SOCKET driver
/////////////////////////////////////////////////////////////////////////////////////////////////

#endif // _LWIP_SOCKETS_H_ 1

