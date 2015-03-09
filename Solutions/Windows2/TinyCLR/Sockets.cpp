////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace Microsoft::SPOT::Emulator;
using namespace Microsoft::SPOT::Emulator::Sockets;

BOOL SOCK_Initialize()
{
    return EmulatorNative::GetISocketsDriver()->Initialize();
}

BOOL SOCK_Uninitialize()
{
    return EmulatorNative::GetISocketsDriver()->Uninitialize();
}

int SOCK_socket( int family, int type, int protocol )
{
    return EmulatorNative::GetISocketsDriver()->socket( family, type, protocol );
}

int SOCK_bind( int socket, const struct SOCK_sockaddr* address, int addressLen )
{
    return EmulatorNative::GetISocketsDriver()->bind( socket, (IntPtr)(void*)address, addressLen );
}

int SOCK_connect(int socket, const struct SOCK_sockaddr* address, int addressLen)
{
    return EmulatorNative::GetISocketsDriver()->connect( socket, (IntPtr)(void*)address, addressLen );
}
 
int SOCK_send(int socket, const char* buf, int len, int flags)
{
    return EmulatorNative::GetISocketsDriver()->send( socket, (IntPtr)(void*)buf, len, flags );
}
 
int SOCK_recv(int socket, char* buf, int len, int flags)
{
    return EmulatorNative::GetISocketsDriver()->recv( socket, (IntPtr)buf, len, flags );
}

int SOCK_close(int socket)
{
    return EmulatorNative::GetISocketsDriver()->close( socket );
}

int SOCK_listen( int socket, int backlog )
{
    return EmulatorNative::GetISocketsDriver()->listen( socket, backlog );
}
        
int SOCK_accept( int socket, struct SOCK_sockaddr* address, int* addressLen )
{
    return EmulatorNative::GetISocketsDriver()->accept( socket, (IntPtr)(void*)address, (int%)addressLen );
}

int SOCK_shutdown( int socket, int how )
{       
    return EmulatorNative::GetISocketsDriver()->shutdown( socket, how );
}

int SOCK_getaddrinfo( const char* nodename, char* servname, const struct SOCK_addrinfo* hints, struct SOCK_addrinfo** res )
{
    IntPtr addr;
    
    int ret = EmulatorNative::GetISocketsDriver()->getaddrinfo( (IntPtr)(void*)nodename, (IntPtr)servname, (IntPtr)(void*)hints, addr );

    *res = (struct SOCK_addrinfo*)addr.ToPointer();

    return ret;
}

void SOCK_freeaddrinfo( struct SOCK_addrinfo* ai )
{
    EmulatorNative::GetISocketsDriver()->freeaddrinfo( (IntPtr)ai );
}
        
int SOCK_ioctl( int socket, int cmd, int* data )
{
    return EmulatorNative::GetISocketsDriver()->ioctl( socket, cmd, (int%)*data );
}

int SOCK_getlasterror()
{
    return EmulatorNative::GetISocketsDriver()->getlasterror();
}

int SOCK_select( int nfds, SOCK_fd_set* readfds, SOCK_fd_set* writefds, SOCK_fd_set* exceptfds, const struct SOCK_timeval* timeout )
{
    return EmulatorNative::GetISocketsDriver()->select( nfds, (IntPtr)readfds, (IntPtr)writefds, (IntPtr)exceptfds, (TimeVal%)*timeout );
}

int SOCK_setsockopt( int socket, int level, int optname, const char* optval, int optlen )
{
    return EmulatorNative::GetISocketsDriver()->setsockopt( socket, level, optname, (IntPtr)(void*)optval, optlen );
}

int SOCK_getsockopt( int socket, int level, int optname, char* optval, int* optlen )
{
    return EmulatorNative::GetISocketsDriver()->getsockopt( socket, level, optname, (IntPtr)optval, (int%)*optlen );
}

int SOCK_getpeername( int socket, struct SOCK_sockaddr* name, int* namelen )
{
    return EmulatorNative::GetISocketsDriver()->getpeername( socket, (IntPtr)name, (int%)*namelen );
}

int SOCK_getsockname( int socket, struct SOCK_sockaddr* name, int* namelen )
{
    return EmulatorNative::GetISocketsDriver()->getsockname( socket, (IntPtr)name, (int%)*namelen );
}

int SOCK_recvfrom( int s, char* buf, int len, int flags, struct SOCK_sockaddr* from, int* fromlen )
{
    return EmulatorNative::GetISocketsDriver()->recvfrom( s, (IntPtr)buf, len, flags, (IntPtr)from, (int%)*fromlen );
}

int SOCK_sendto( int s, const char* buf, int len, int flags, const struct SOCK_sockaddr* to, int tolen )
{
    return EmulatorNative::GetISocketsDriver()->sendto( s, (IntPtr)(void*)buf, len, flags, (IntPtr)(void*)to, (int%)tolen );
}

UINT32 SOCK_CONFIGURATION_GetAdapterCount()
{
    return EmulatorNative::GetISocketsDriver()->NetworkAdapterCount;
}

HRESULT SOCK_CONFIGURATION_LoadAdapterConfiguration( UINT32 interfaceIndex, SOCK_NetworkConfiguration* config )
{
    return EmulatorNative::GetISocketsDriver()->LoadAdapterConfiguration( interfaceIndex, (NetworkAdapterConfiguration%)*config );
}

HRESULT SOCK_CONFIGURATION_UpdateAdapterConfiguration( UINT32 interfaceIndex, UINT32 updateFlags, SOCK_NetworkConfiguration* config )
{
    return EmulatorNative::GetISocketsDriver()->UpdateAdapterConfiguration( interfaceIndex, updateFlags, (NetworkAdapterConfiguration%)*config );
}

HRESULT SOCK_CONFIGURATION_LoadConfiguration( UINT32 interfaceIndex, SOCK_NetworkConfiguration* config )
{
    return SOCK_CONFIGURATION_LoadAdapterConfiguration(interfaceIndex, config);
}

HRESULT SOCK_CONFIGURATION_LoadWirelessConfiguration( UINT32 interfaceIndex, SOCK_WirelessConfiguration* wirelessConfig )
{
    return CLR_E_FAIL;
}

HRESULT SOCK_CONFIGURATION_UpdateWirelessConfiguration( UINT32 interfaceIndex, SOCK_WirelessConfiguration* wirelessConfig )
{
    return CLR_E_FAIL;
}

HRESULT SOCK_CONFIGURATION_SaveAllWirelessConfigurations( )
{
    return CLR_E_FAIL;
}


