////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

__weak BOOL HAL_SOCK_Initialize()
{
    return FALSE;
}

__weak BOOL HAL_SOCK_Uninitialize()
{
    return FALSE;
}

__weak SOCK_SOCKET HAL_SOCK_socket( int family, int type, int protocol ) 
{
    return SOCK_SOCKET_ERROR; 
}
__weak int HAL_SOCK_bind( SOCK_SOCKET socket, const struct SOCK_sockaddr* address, int addressLen  ) 
{ 
    return SOCK_SOCKET_ERROR; 
}
__weak int HAL_SOCK_connect(SOCK_SOCKET socket, const struct SOCK_sockaddr* address, int addressLen) 
{ 
    return SOCK_SOCKET_ERROR; 
}
__weak int HAL_SOCK_send(SOCK_SOCKET socket, const char* buf, int len, int flags) 
{ 
    return SOCK_SOCKET_ERROR; 
}
__weak int HAL_SOCK_recv(SOCK_SOCKET socket, char* buf, int len, int flags)
{ 
    return SOCK_SOCKET_ERROR; 
}
__weak int HAL_SOCK_close(SOCK_SOCKET socket)
{ 
    return SOCK_SOCKET_ERROR; 
}
__weak int HAL_SOCK_listen( SOCK_SOCKET socket, int backlog )
{ 
    return SOCK_SOCKET_ERROR; 
}
__weak SOCK_SOCKET HAL_SOCK_accept( SOCK_SOCKET socket, struct SOCK_sockaddr* address, int* addressLen )
{ 
    return SOCK_SOCKET_ERROR; 
}
__weak int HAL_SOCK_shutdown( SOCK_SOCKET socket, int how )
{ 
    return SOCK_SOCKET_ERROR; 
}
__weak int HAL_SOCK_getaddrinfo( const char* nodename, char* servname, const struct SOCK_addrinfo* hints, struct SOCK_addrinfo** res )
{ 
    return SOCK_SOCKET_ERROR; 
}
__weak void HAL_SOCK_freeaddrinfo( struct SOCK_addrinfo* ai )
{ 
}
__weak int HAL_SOCK_ioctl( SOCK_SOCKET socket, int cmd, int* data )
{ 
    return SOCK_SOCKET_ERROR; 
}
__weak int HAL_SOCK_getlasterror()
{ 
    return SOCK_SOCKET_ERROR; 
}
__weak int HAL_SOCK_select( int nfds, SOCK_fd_set* readfds, SOCK_fd_set* writefds, SOCK_fd_set* except, const struct SOCK_timeval* timeout )
{ 
    return SOCK_SOCKET_ERROR; 
}
__weak int HAL_SOCK_setsockopt( SOCK_SOCKET socket, int level, int optname, const char* optval, int  optlen )
{ 
    return SOCK_SOCKET_ERROR; 
}
__weak int HAL_SOCK_getsockopt( SOCK_SOCKET socket, int level, int optname,       char* optval, int* optlen )
{ 
    return SOCK_SOCKET_ERROR; 
}
__weak int HAL_SOCK_getpeername( SOCK_SOCKET socket, struct SOCK_sockaddr* name, int* namelen )
{ 
    return SOCK_SOCKET_ERROR; 
}
__weak int HAL_SOCK_getsockname( SOCK_SOCKET socket, struct SOCK_sockaddr* name, int* namelen )
{ 
    return SOCK_SOCKET_ERROR; 
}
__weak int HAL_SOCK_recvfrom( SOCK_SOCKET s, char* buf, int len, int flags, struct SOCK_sockaddr* from, int* fromlen )
{ 
    return SOCK_SOCKET_ERROR; 
}
__weak int HAL_SOCK_sendto( SOCK_SOCKET s, const char* buf, int len, int flags, const struct SOCK_sockaddr* to, int tolen )
{ 
    return SOCK_SOCKET_ERROR; 
}

__weak UINT32 HAL_SOCK_CONFIGURATION_GetAdapterCount()
{
    return 0;
}
__weak HRESULT HAL_SOCK_CONFIGURATION_LoadAdapterConfiguration( UINT32 interfaceIndex, SOCK_NetworkConfiguration* config )
{
    return CLR_E_FAIL; 
}
__weak HRESULT HAL_SOCK_CONFIGURATION_UpdateAdapterConfiguration( UINT32 interfaceIndex, UINT32 updateFlags, SOCK_NetworkConfiguration* config )
{
    return CLR_E_FAIL; 
}

__weak HRESULT HAL_SOCK_CONFIGURATION_LoadWirelessConfiguration( UINT32 interfaceIndex, SOCK_WirelessConfiguration* wirelessConfig )
{
    return CLR_E_FAIL; 
}

__weak void HAL_SOCK_EventsSet( UINT32 events ) 
{
}

__weak void * HAL_SOCK_GlobalLockContext() 
{
    return NULL;
}
