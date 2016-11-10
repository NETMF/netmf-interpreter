////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

//--//

__weak BOOL Network_Initialize()
{ 
    NATIVE_PROFILE_PAL_COM();
    return TRUE; 
}

__weak BOOL Network_Uninitialize()
{ 
    NATIVE_PROFILE_PAL_COM();
    return TRUE; 
}

__weak BOOL SOCKETS_Initialize( int ComPortNum )
{
    NATIVE_PROFILE_PAL_COM();
    return TRUE;
}

__weak BOOL SOCKETS_Uninitialize( int ComPortNum )
{
    NATIVE_PROFILE_PAL_COM();
    return TRUE;
}

__weak int SOCKETS_Write( int ComPortNum, const char* Data, size_t size )
{ 
    NATIVE_PROFILE_PAL_COM();
    return 0; 
}

__weak int SOCKETS_Read( int ComPortNum, char* Data, size_t size )
{ 
    NATIVE_PROFILE_PAL_COM();
    return 0; 
}

__weak BOOL SOCKETS_Flush( int ComPortNum )
{ 
    NATIVE_PROFILE_PAL_COM();
    return FALSE; 
}

__weak void SOCKETS_CloseConnections()
{
    NATIVE_PROFILE_PAL_COM();
}

__weak BOOL SOCKETS_UpgradeToSsl( INT32 ComPortNum, const UINT8* pCACert, UINT32 caCertLen, const UINT8* pDeviceCert, UINT32 deviceCertLen, LPCSTR szTargetHost )
{
    NATIVE_PROFILE_PAL_COM();
    return FALSE;
}

__weak BOOL SOCKETS_IsUsingSsl( INT32 ComPortNum )
{
    NATIVE_PROFILE_PAL_COM();
    return FALSE;
}


__weak BOOL SOCKETS_ProcessSocketActivity(SOCK_SOCKET signalSocket)
{
    NATIVE_PROFILE_PAL_COM();
    return TRUE;
}

__weak UINT32 SOCK_CONFIGURATION_GetAdapterCount()
{
    NATIVE_PROFILE_PAL_COM();
    return 0;
}

__weak HRESULT SOCK_CONFIGURATION_LoadAdapterConfiguration( UINT32 interfaceIndex, SOCK_NetworkConfiguration* config )
{ 
    NATIVE_PROFILE_PAL_COM();
    return CLR_E_FAIL; 
}

__weak HRESULT SOCK_CONFIGURATION_UpdateAdapterConfiguration( UINT32 interfaceIndex, UINT32 updateFlags, SOCK_NetworkConfiguration* config )
{ 
    NATIVE_PROFILE_PAL_COM();
    return CLR_E_FAIL; 
}

__weak HRESULT SOCK_CONFIGURATION_LoadConfiguration( UINT32 interfaceIndex, SOCK_NetworkConfiguration* config )
{
    NATIVE_PROFILE_PAL_COM();
    return CLR_E_FAIL; 
}

__weak HRESULT SOCK_CONFIGURATION_LoadWirelessConfiguration( UINT32 interfaceIndex, SOCK_WirelessConfiguration* wirelessConfig )
{
    NATIVE_PROFILE_PAL_COM();
    return CLR_E_FAIL; 
}

__weak HRESULT SOCK_CONFIGURATION_UpdateWirelessConfiguration( UINT32 interfaceIndex, SOCK_WirelessConfiguration* wirelessConfig )
{
    NATIVE_PROFILE_PAL_COM();
    return CLR_E_FAIL; 
}

__weak HRESULT SOCK_CONFIGURATION_SaveAllWirelessConfigurations( )
{
    NATIVE_PROFILE_PAL_COM();
    return CLR_E_FAIL; 
}

__weak HRESULT HAL_SOCK_CONFIGURATION_LoadWirelessConfiguration( UINT32 interfaceIndex, SOCK_WirelessConfiguration* wirelessConfig )
{
    NATIVE_PROFILE_PAL_COM();
    return CLR_E_FAIL; 
}

__weak int SOCK_socket( int family, int type, int protocol ) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return SOCK_SOCKET_ERROR; 
}

__weak int SOCK_bind( int socket, const struct SOCK_sockaddr* address, int addressLen  ) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return SOCK_SOCKET_ERROR; 
}

__weak int SOCK_connect(int socket, const struct SOCK_sockaddr* address, int addressLen) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return SOCK_SOCKET_ERROR; 
}

__weak int SOCK_send(int socket, const char* buf, int len, int flags) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return SOCK_SOCKET_ERROR; 
}

__weak int SOCK_recv(int socket, char* buf, int len, int timeout) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return SOCK_SOCKET_ERROR; 
}

__weak int SOCK_close(int socket) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return SOCK_SOCKET_ERROR; 
}

__weak int SOCK_listen( int socket, int backlog ) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return SOCK_SOCKET_ERROR; 
}

__weak int SOCK_accept( int socket, struct SOCK_sockaddr* address, int* addressLen ) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return SOCK_SOCKET_ERROR; 
}

__weak int SOCK_shutdown( int socket, int how ) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return SOCK_SOCKET_ERROR; 
}

__weak int SOCK_getaddrinfo(  const char* nodename, char* servname, const struct SOCK_addrinfo* hints, struct SOCK_addrinfo** res ) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return SOCK_SOCKET_ERROR; 
}

__weak void SOCK_freeaddrinfo( struct SOCK_addrinfo* ai )
{
    NATIVE_PROFILE_PAL_COM();
}
    

__weak int SOCK_ioctl( int socket, int cmd, int* data ) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return SOCK_SOCKET_ERROR; 
}

__weak int SOCK_getlasterror() 
{ 
    NATIVE_PROFILE_PAL_COM();
    return SOCK_SOCKET_ERROR; 
}

__weak int SOCK_select( int socket, SOCK_fd_set* readfds, SOCK_fd_set* writefds, SOCK_fd_set* except, const struct SOCK_timeval* timeout ) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return SOCK_SOCKET_ERROR; 
}

__weak int SOCK_setsockopt( int socket, int level, int optname, const char* optval, int  optlen ) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return SOCK_SOCKET_ERROR; 
}

__weak int SOCK_getsockopt( int socket, int level, int optname,       char* optval, int* optlen ) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return SOCK_SOCKET_ERROR; 
}

__weak int SOCK_getpeername( int socket, struct SOCK_sockaddr* name, int* namelen ) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return SOCK_SOCKET_ERROR; 
}

__weak int SOCK_getsockname( int socket, struct SOCK_sockaddr* name, int* namelen ) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return SOCK_SOCKET_ERROR; 
}

__weak int SOCK_recvfrom( int s, char* buf, int len, int flags, struct SOCK_sockaddr* from, int* fromlen ) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return SOCK_SOCKET_ERROR; 
}

__weak int SOCK_sendto( int s, const char* buf, int len, int flags, const struct SOCK_sockaddr* to, int tolen ) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return SOCK_SOCKET_ERROR; 
}

