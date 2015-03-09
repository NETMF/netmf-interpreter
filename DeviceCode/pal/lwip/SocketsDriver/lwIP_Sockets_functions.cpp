////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "LWIP_Sockets.h"


BOOL HAL_SOCK_Initialize()
{
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::Initialize();
}

BOOL HAL_SOCK_Uninitialize()
{
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::Uninitialize();
}

SOCK_SOCKET HAL_SOCK_socket( int family, int type, int protocol ) 
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::Socket( family, type, protocol );
}
int HAL_SOCK_bind( SOCK_SOCKET socket, const struct SOCK_sockaddr* address, int addressLen  ) 
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::Bind( socket, address, addressLen  );
}
int HAL_SOCK_connect(SOCK_SOCKET socket, const struct SOCK_sockaddr* address, int addressLen) 
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::Connect(socket, address, addressLen);
}
int HAL_SOCK_send(SOCK_SOCKET socket, const char* buf, int len, int flags) 
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::Send(socket, buf, len, flags);
}
int HAL_SOCK_recv(SOCK_SOCKET socket, char* buf, int len, int flags)
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::Recv(socket, buf, len, flags);
}
int HAL_SOCK_close(SOCK_SOCKET socket)
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::Close(socket);
}
int HAL_SOCK_listen( SOCK_SOCKET socket, int backlog )
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::Listen( socket, backlog );
}
SOCK_SOCKET HAL_SOCK_accept( SOCK_SOCKET socket, struct SOCK_sockaddr* address, int* addressLen )
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::Accept( socket, address, addressLen );
}
int HAL_SOCK_shutdown( SOCK_SOCKET socket, int how )
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::Shutdown( socket, how );
}
int HAL_SOCK_getaddrinfo( const char* nodename, char* servname, const struct SOCK_addrinfo* hints, struct SOCK_addrinfo** res )
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::GetAddrInfo( nodename, servname, hints, res );
}
void HAL_SOCK_freeaddrinfo( struct SOCK_addrinfo* ai )
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    LWIP_SOCKETS_Driver::FreeAddrInfo( ai );
}
int HAL_SOCK_ioctl( SOCK_SOCKET socket, int cmd, int* data )
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::Ioctl( socket, cmd, data );
}
int HAL_SOCK_getlasterror()
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::GetLastError();
}
int HAL_SOCK_select( int nfds, SOCK_fd_set* readfds, SOCK_fd_set* writefds, SOCK_fd_set* except, const struct SOCK_timeval* timeout )
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::Select( nfds, readfds, writefds, except, timeout );
}
int HAL_SOCK_setsockopt( SOCK_SOCKET socket, int level, int optname, const char* optval, int  optlen )
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::SetSockOpt( socket, level, optname, optval, optlen );
}
int HAL_SOCK_getsockopt( SOCK_SOCKET socket, int level, int optname,       char* optval, int* optlen )
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::GetSockOpt( socket, level, optname, optval, optlen );
}
int HAL_SOCK_getpeername( SOCK_SOCKET socket, struct SOCK_sockaddr* name, int* namelen )
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::GetPeerName( socket, name, namelen );
}
int HAL_SOCK_getsockname( SOCK_SOCKET socket, struct SOCK_sockaddr* name, int* namelen )
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::GetSockName( socket, name, namelen );
}
int HAL_SOCK_recvfrom( SOCK_SOCKET s, char* buf, int len, int flags, struct SOCK_sockaddr* from, int* fromlen )
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::RecvFrom( s, buf, len, flags, from, fromlen );
}
int HAL_SOCK_sendto( SOCK_SOCKET s, const char* buf, int len, int flags, const struct SOCK_sockaddr* to, int tolen )
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::SendTo( s, buf, len, flags, to, tolen );
}

UINT32 HAL_SOCK_CONFIGURATION_GetAdapterCount()
{
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::GetAdapterCount();
}
HRESULT HAL_SOCK_CONFIGURATION_LoadAdapterConfiguration( UINT32 interfaceIndex, SOCK_NetworkConfiguration* config )
{
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::LoadAdapterConfiguration(interfaceIndex, config);
}
HRESULT HAL_SOCK_CONFIGURATION_UpdateAdapterConfiguration( UINT32 interfaceIndex, UINT32 updateFlags, SOCK_NetworkConfiguration* config )
{
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::UpdateAdapterConfiguration(interfaceIndex, updateFlags, config);
}

HRESULT HAL_SOCK_CONFIGURATION_LoadWirelessConfiguration( UINT32 interfaceIndex, SOCK_WirelessConfiguration* wirelessConfig )
{
    NATIVE_PROFILE_PAL_NETWORK();
    return LWIP_SOCKETS_Driver::LoadWirelessConfiguration(interfaceIndex, wirelessConfig);
}

void HAL_SOCK_EventsSet( UINT32 events ) 
{
    NATIVE_PROFILE_PAL_NETWORK();
    ASSERT( (events == SOCKET_EVENT_FLAG_SOCKET) || (events == SOCKET_EVENT_FLAG_SOCKETS_READY));

    Events_Set( SYSTEM_EVENT_FLAG_SOCKET );
}

void * HAL_SOCK_GlobalLockContext() 
{
    NATIVE_PROFILE_PAL_NETWORK();
    return NULL;
}
