////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#include "tinyhal.h"
#include "tinyhal.h"
   
////////////////////////////////////////////////////////////////////////////////////////////////////

// Stubs for the non-inlined platforms
//

BOOL HAL_SOCK_Initialize() 
{ 
    return FALSE; 
}

BOOL HAL_SOCK_Uninitialize() 
{ 
    return TRUE; 
}

int HAL_SOCK_socket( int family, int type, int protocol )
{ 
    return SOCK_SOCKET_ERROR; 
}

int HAL_SOCK_bind( int socket, const struct SOCK_sockaddr* address, int addressLen  ) 
{ 
    return SOCK_SOCKET_ERROR; 
}

int HAL_SOCK_connect(int socket, const struct SOCK_sockaddr* address, int addressLen) 
{
    return SOCK_SOCKET_ERROR; 
}

int HAL_SOCK_send(int socket, const char* buf, int len, int flags) 
{ 
    return SOCK_SOCKET_ERROR; 
}

int HAL_SOCK_recv(int socket, char* buf, int len, int timeout) 
{ 
    return SOCK_SOCKET_ERROR; 
}

int HAL_SOCK_close(int socket) 
{ 
    return SOCK_SOCKET_ERROR; 
}

int HAL_SOCK_listen( int socket, int backlog ) 
{ 
    return SOCK_SOCKET_ERROR; 
}

int HAL_SOCK_accept( int socket, struct SOCK_sockaddr* address, int* addressLen ) 
{ 
    return SOCK_SOCKET_ERROR; 
}

int HAL_SOCK_shutdown( int socket, int how ) 
{ 
    return SOCK_SOCKET_ERROR; 
}

int HAL_SOCK_getaddrinfo(  const char* nodename, char* servname, const struct SOCK_addrinfo* hints, struct SOCK_addrinfo** res ) 
{ 
    return SOCK_SOCKET_ERROR; 
}

void HAL_SOCK_freeaddrinfo( struct SOCK_addrinfo* ai ) 
{ }                

int HAL_SOCK_ioctl( int socket, int cmd, int* data ) 
{ 
    return SOCK_SOCKET_ERROR; 
}

int HAL_SOCK_getlasterror() 
{ 
    return SOCK_SOCKET_ERROR; 
}

int HAL_SOCK_select( int socket, SOCK_fd_set* readfds, SOCK_fd_set* writefds, SOCK_fd_set* except, const struct SOCK_timeval* timeout ) 
{ 
        return SOCK_SOCKET_ERROR; 
}

int HAL_SOCK_setsockopt( int socket, int level, int optname, const char* optval, int  optlen ) 
{ 
    return SOCK_SOCKET_ERROR; 
}

int HAL_SOCK_getsockopt( int socket, int level, int optname,       char* optval, int* optlen ) 
{ 
    return SOCK_SOCKET_ERROR; 
}

int HAL_SOCK_getpeername( int socket, struct SOCK_sockaddr* name, int* namelen ) 
{ 
    return SOCK_SOCKET_ERROR; 
}

int HAL_SOCK_getsockname( int socket, struct SOCK_sockaddr* name, int* namelen ) 
{ 
    return SOCK_SOCKET_ERROR; 
}

int HAL_SOCK_recvfrom( int s, char* buf, int len, int flags, struct SOCK_sockaddr* from, int* fromlen ) 
{ 
    return SOCK_SOCKET_ERROR; 
}

int HAL_SOCK_sendto( int s, const char* buf, int len, int flags, const struct SOCK_sockaddr* to, int tolen ) 
{ 
    return SOCK_SOCKET_ERROR; 
}


UINT32 HAL_SOCK_CONFIGURATION_GetAdapterCount()
{
    return 0;
}

HRESULT HAL_SOCK_CONFIGURATION_LoadAdapterConfiguration( UINT32 interfaceIndex, SOCK_NetworkConfiguration* config )
{

    return S_OK; 
}

HRESULT HAL_SOCK_CONFIGURATION_UpdateAdapterConfiguration( UINT32 interfaceIndex, UINT32 updateFlags, SOCK_NetworkConfiguration* config )
{ 
    return S_OK; 
}

HRESULT HAL_SOCK_CONFIGURATION_LoadWirelessConfiguration( UINT32 interfaceIndex, SOCK_WirelessConfiguration* wirelessConfig )
{
    return CLR_E_FAIL;
}

void* HAL_SOCK_GlobalLockContext()
{
    return NULL;
}

void HAL_SOCK_EventsSet(UINT32 events)
{
}


