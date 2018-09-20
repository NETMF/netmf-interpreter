////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "sockets_lwip.h"

//--//

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_PAL_Sockets_LWIP_Driver"
#endif

Sockets_LWIP_Driver g_Sockets_LWIP_Driver;

HAL_COMPLETION Sockets_LWIP_Driver::s_DebuggerTimeoutCompletion;

static HAL_CONTINUATION MulticastResponseContinuation;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

INT32 g_DebuggerPort_SslCtx_Handle = -1;

SOCK_SOCKET SOCK_socket( INT32 family, INT32 type, INT32 protocol ) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return Sockets_LWIP_Driver::Socket( family, type, protocol, FALSE );
}
INT32 SOCK_bind( SOCK_SOCKET socket, const struct SOCK_sockaddr* address, INT32 addressLen  ) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return HAL_SOCK_bind( socket, address, addressLen  );
}
INT32 SOCK_connect(SOCK_SOCKET socket, const struct SOCK_sockaddr* address, INT32 addressLen) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return Sockets_LWIP_Driver::Connect(socket, address, addressLen);
}
INT32 SOCK_send(SOCK_SOCKET socket, const char* buf, INT32 len, INT32 flags) 
{ 
    NATIVE_PROFILE_PAL_COM();    
    return Sockets_LWIP_Driver::Send(socket, buf, len, flags);
}
INT32 SOCK_recv(SOCK_SOCKET socket, char* buf, INT32 len, INT32 flags)
{ 
    NATIVE_PROFILE_PAL_COM();
    return Sockets_LWIP_Driver::Recv(socket, buf, len, flags);
}
INT32 SOCK_close(SOCK_SOCKET socket)
{ 
    NATIVE_PROFILE_PAL_COM();
    return Sockets_LWIP_Driver::Close(socket);
}
INT32 SOCK_listen( SOCK_SOCKET socket, INT32 backlog )
{ 
    NATIVE_PROFILE_PAL_COM();
    return Sockets_LWIP_Driver::Listen( socket, backlog );
}
SOCK_SOCKET SOCK_accept( SOCK_SOCKET socket, struct SOCK_sockaddr* address, INT32* addressLen )
{ 
    NATIVE_PROFILE_PAL_COM();
    return Sockets_LWIP_Driver::Accept( socket, address, addressLen, FALSE );
}
INT32 SOCK_shutdown( SOCK_SOCKET socket, INT32 how )
{ 
    NATIVE_PROFILE_PAL_COM();
    return HAL_SOCK_shutdown( socket, how );
}
INT32 SOCK_getaddrinfo( const char* nodename, char* servname, const struct SOCK_addrinfo* hints, struct SOCK_addrinfo** res )
{ 
    NATIVE_PROFILE_PAL_COM();
    return HAL_SOCK_getaddrinfo( nodename, servname, hints, res );
}
void SOCK_freeaddrinfo( struct SOCK_addrinfo* ai )
{ 
    NATIVE_PROFILE_PAL_COM();
    HAL_SOCK_freeaddrinfo( ai );
}
INT32 SOCK_ioctl( SOCK_SOCKET socket, INT32 cmd, INT32* data )
{ 
    NATIVE_PROFILE_PAL_COM();
    return HAL_SOCK_ioctl( socket, cmd, data );
}
INT32 SOCK_getlasterror()
{ 
    NATIVE_PROFILE_PAL_COM();
    return HAL_SOCK_getlasterror();
}
INT32 SOCK_select( INT32 nfds, SOCK_fd_set* readfds, SOCK_fd_set* writefds, SOCK_fd_set* except, const struct SOCK_timeval* timeout )
{ 
    NATIVE_PROFILE_PAL_COM();
    return Sockets_LWIP_Driver::Select( nfds, readfds, writefds, except, timeout );
}
INT32 SOCK_setsockopt( SOCK_SOCKET socket, INT32 level, INT32 optname, const char* optval, INT32  optlen )
{ 
    NATIVE_PROFILE_PAL_COM();
    return HAL_SOCK_setsockopt( socket, level, optname, optval, optlen );
}
INT32 SOCK_getsockopt( SOCK_SOCKET socket, INT32 level, INT32 optname,       char* optval, INT32* optlen )
{ 
    NATIVE_PROFILE_PAL_COM();
    return HAL_SOCK_getsockopt( socket, level, optname, optval, optlen );
}
INT32 SOCK_getpeername( SOCK_SOCKET socket, struct SOCK_sockaddr* name, INT32* namelen )
{ 
    NATIVE_PROFILE_PAL_COM();
    return HAL_SOCK_getpeername( socket, name, namelen );
}
INT32 SOCK_getsockname( SOCK_SOCKET socket, struct SOCK_sockaddr* name, INT32* namelen )
{ 
    NATIVE_PROFILE_PAL_COM();
    return HAL_SOCK_getsockname( socket, name, namelen );
}
INT32 SOCK_recvfrom( SOCK_SOCKET s, char* buf, INT32 len, INT32 flags, struct SOCK_sockaddr* from, INT32* fromlen )
{ 
    NATIVE_PROFILE_PAL_COM();
    return Sockets_LWIP_Driver::RecvFrom( s, buf, len, flags, from, fromlen );
}
INT32 SOCK_sendto( SOCK_SOCKET s, const char* buf, INT32 len, INT32 flags, const struct SOCK_sockaddr* to, INT32 tolen )
{ 
    NATIVE_PROFILE_PAL_COM();
    return Sockets_LWIP_Driver::SendTo( s, buf, len, flags, to, tolen );
}

BOOL Network_Initialize()
{
    NATIVE_PROFILE_PAL_COM();
    return Sockets_LWIP_Driver::Initialize( );
}

BOOL Network_Uninitialize()
{
    NATIVE_PROFILE_PAL_COM();

    return Sockets_LWIP_Driver::Uninitialize( );
}

void Sockets_LWIP_Driver::CloseDebuggerSocket()
{
    if(g_Sockets_LWIP_Driver.m_SocketDebugStream != SOCK_SOCKET_ERROR)
    {
        if(g_Sockets_LWIP_Driver.m_usingSSL)
        {
            SSL_CloseSocket( g_Sockets_LWIP_Driver.m_SocketDebugStream );
        }
        else
        {
            SOCK_close( g_Sockets_LWIP_Driver.m_SocketDebugStream );
        }

        g_Sockets_LWIP_Driver.m_usingSSL = FALSE;
        
        g_Sockets_LWIP_Driver.m_SocketDebugStream = SOCK_SOCKET_ERROR;

        g_Sockets_LWIP_Driver.m_stateDebugSocket = DbgSock_Listening;

        s_DebuggerTimeoutCompletion.Abort();
    }
}

void Sockets_LWIP_Driver::OnDebuggerTimeout(void* arg)
{
    CloseDebuggerSocket();
}

BOOL  SOCKETS_Initialize( int ComPortNum )
{
    NATIVE_PROFILE_PAL_COM();

    return Sockets_LWIP_Driver::InitializeDbgListener( ComPortNum );
}
BOOL  SOCKETS_Uninitialize( int ComPortNum )
{
    NATIVE_PROFILE_PAL_COM();
    return Sockets_LWIP_Driver::UninitializeDbgListener( ComPortNum );
}

INT32 SOCKETS_Write( INT32 ComPortNum, const char* Data, size_t size )
{
    NATIVE_PROFILE_PAL_COM();
    return Sockets_LWIP_Driver::Write( ComPortNum, Data, size );
}

INT32 SOCKETS_Read( INT32 ComPortNum, char* Data, size_t size )
{
    NATIVE_PROFILE_PAL_COM();
    return Sockets_LWIP_Driver::Read( ComPortNum, Data, size );
}

BOOL SOCKETS_Flush( INT32 ComPortNum )
{
    NATIVE_PROFILE_PAL_COM();
    //Events_WaitForEvents(0, 2);
    return TRUE;
}

void SOCKETS_CloseConnections()
{
    NATIVE_PROFILE_PAL_COM();
    Sockets_LWIP_Driver::CloseConnections(FALSE);
}

BOOL SOCKETS_UpgradeToSsl( INT32 ComPortNum, const UINT8* pCACert, UINT32 caCertLen, const UINT8* pDeviceCert, UINT32 deviceCertLen, LPCSTR szTargetHost )
{
    NATIVE_PROFILE_PAL_COM();
    return Sockets_LWIP_Driver::UpgradeToSsl( ComPortNum, pCACert, caCertLen, pDeviceCert, deviceCertLen, szTargetHost );
}

BOOL SOCKETS_IsUsingSsl( INT32 ComPortNum )
{
    NATIVE_PROFILE_PAL_COM();
    return Sockets_LWIP_Driver::IsUsingSsl( ComPortNum );
}


UINT32 SOCK_CONFIGURATION_GetAdapterCount()
{
    NATIVE_PROFILE_PAL_COM();
    return HAL_SOCK_CONFIGURATION_GetAdapterCount();
}

HRESULT SOCK_CONFIGURATION_LoadAdapterConfiguration( UINT32 interfaceIndex, SOCK_NetworkConfiguration* config )
{
    NATIVE_PROFILE_PAL_COM();
    return HAL_SOCK_CONFIGURATION_LoadAdapterConfiguration(interfaceIndex, config);
}
HRESULT SOCK_CONFIGURATION_UpdateAdapterConfiguration( UINT32 interfaceIndex, UINT32 updateFlags, SOCK_NetworkConfiguration* config )
{
    NATIVE_PROFILE_PAL_COM();
    HRESULT hr = S_OK;
    BOOL fDbg = FALSE;

    if(interfaceIndex >= NETWORK_INTERFACE_COUNT) 
    {
        return CLR_E_INVALID_PARAMETER;
    }

    const UINT32 c_reInitFlag = SOCK_NETWORKCONFIGURATION_UPDATE_DHCP       | 
                                SOCK_NETWORKCONFIGURATION_UPDATE_DHCP_RENEW | 
                                SOCK_NETWORKCONFIGURATION_UPDATE_MAC;

    const UINT32 c_uninitFlag = c_reInitFlag | SOCK_NETWORKCONFIGURATION_UPDATE_DHCP_RELEASE;

    if(0 != (updateFlags & c_uninitFlag))
    {
        fDbg = SOCKETS_Uninitialize(COM_SOCKET_DBG);
    }

    hr = HAL_SOCK_CONFIGURATION_UpdateAdapterConfiguration(interfaceIndex, updateFlags, config);

    if(SUCCEEDED(hr))
    {
        Sockets_LWIP_Driver::SaveConfig(interfaceIndex, config);
    }
    else
    {
        // restore the network configuration
        HAL_SOCK_CONFIGURATION_UpdateAdapterConfiguration(interfaceIndex, updateFlags, &g_NetworkConfig.NetworkInterfaces[interfaceIndex]);
    }

    if(0 != (updateFlags & c_reInitFlag))
    {
        if(fDbg) SOCKETS_Initialize(COM_SOCKET_DBG);
    }

    return hr;
}

HRESULT SOCK_CONFIGURATION_LoadConfiguration( UINT32 interfaceIndex, SOCK_NetworkConfiguration* config )
{
    NATIVE_PROFILE_PAL_COM();
    HRESULT hr = S_OK;

    if(interfaceIndex >= NETWORK_INTERFACE_COUNT || config == NULL) 
    {
        return CLR_E_INVALID_PARAMETER;
    }

    // load current DCHP settings
    hr = SOCK_CONFIGURATION_LoadAdapterConfiguration(interfaceIndex, config);

    return hr;
}

HRESULT SOCK_CONFIGURATION_LoadWirelessConfiguration( UINT32 interfaceIndex, SOCK_WirelessConfiguration* wirelessConfig )
{
    NATIVE_PROFILE_PAL_COM();

    if (interfaceIndex >= WIRELESS_INTERFACE_COUNT)
    {
        return CLR_E_INVALID_PARAMETER;
    }

    Sockets_LWIP_Driver::ApplyWirelessConfig();

    /// Hal version is given a chance if it wants to override stored predifned values.
    if (HAL_SOCK_CONFIGURATION_LoadWirelessConfiguration(interfaceIndex, wirelessConfig) != S_OK)
    {
        memcpy( wirelessConfig, &g_WirelessConfig.WirelessInterfaces[interfaceIndex], sizeof(SOCK_WirelessConfiguration) );
    }

    return S_OK;
}

HRESULT SOCK_CONFIGURATION_UpdateWirelessConfiguration( UINT32 interfaceIndex, SOCK_WirelessConfiguration* wirelessConfig )
{
    NATIVE_PROFILE_PAL_COM();

    if (interfaceIndex >= WIRELESS_INTERFACE_COUNT)
    {
        return CLR_E_INVALID_PARAMETER;
    }

    memcpy( &g_WirelessConfig.WirelessInterfaces[interfaceIndex], wirelessConfig, sizeof(SOCK_WirelessConfiguration) );

    return S_OK;
}

HRESULT SOCK_CONFIGURATION_SaveAllWirelessConfigurations( )
{
    HAL_CONFIG_BLOCK::UpdateBlockWithName(g_WirelessConfig.GetDriverName(), &g_WirelessConfig, sizeof(g_WirelessConfig), TRUE);

    return S_OK;
}


#define SOCKET_SHUTDOWN_READ         0
#define SOCKET_SHUTDOWN_WRITE        1
#define SOCKET_SHUTDOWN_READ_WRITE   2

#define ISSET_SOCKET_FLAG(x,y) ((y) == ((y) & (x).m_flags))
#define SET_SOCKET_FLAG(x,y)   (x).m_flags |= (y)
#define CLEAR_SOCKET_FLAG(x,y) (x).m_flags &= ~(y)

//-----------------------------------------------------------------------------
//
//  CloseConnections - close all connections with the option of leaving the debugger sockets open.
//  debugger sockets are left open on CLR reboot so that communication with the debugger is not
//  lost.
//
//-----------------------------------------------------------------------------
void Sockets_LWIP_Driver::CloseConnections(BOOL fCloseDbg)
{
    NATIVE_PROFILE_PAL_COM();

    INT32 cnt = g_Sockets_LWIP_Driver.m_cntSockets;
    INT32 idx = 0;

    // round one - close all SSL sockets
    for( INT32 i=cnt-1; i>=0; i-- )
    {
        struct SocketRegisterMap& entry = g_Sockets_LWIP_Driver.m_socketHandles[i];
        
        if(entry.m_socket  != SOCK_SOCKET_ERROR && 
           entry.m_sslData != NULL)
        {
            SSL_CloseSocket(entry.m_socket);
        }
    }

    // round two - close all non-SSL sockets
    cnt = g_Sockets_LWIP_Driver.m_cntSockets;

    for( INT32 i=0; i<cnt; i++ )
    {
        struct SocketRegisterMap& entry = g_Sockets_LWIP_Driver.m_socketHandles[i];
        
        if(entry.m_socket != SOCK_SOCKET_ERROR)
        {
            if(fCloseDbg || !ISSET_SOCKET_FLAG(entry, SocketRegisterMap::c_DebugSocket))
            {
                // use the HAL method so we don't unregister the socket since we handle that here
                HAL_SOCK_close(entry.m_socket);
                
                g_Sockets_LWIP_Driver.m_socketHandles[i].m_socket  = SOCK_SOCKET_ERROR;
                g_Sockets_LWIP_Driver.m_socketHandles[i].m_flags   = 0;
                g_Sockets_LWIP_Driver.m_socketHandles[i].m_sslData = NULL;
                g_Sockets_LWIP_Driver.m_cntSockets--;
            }
            else if(i > 0)
            {
                memcpy( &g_Sockets_LWIP_Driver.m_socketHandles[i], 
                        &g_Sockets_LWIP_Driver.m_socketHandles[idx++], 
                        sizeof(g_Sockets_LWIP_Driver.m_socketHandles[i]) );
            }
        }
    }
}

//--//

SOCK_SOCKET Sockets_LWIP_Driver::Socket( INT32 family, INT32 type, INT32 protocol, BOOL fDebug )
{
    NATIVE_PROFILE_PAL_COM();

    int ret = HAL_SOCK_socket(family, type, protocol);

    if (ret != SOCK_SOCKET_ERROR)
    {
        RegisterSocket(ret, (protocol != SOCK_IPPROTO_TCP), fDebug);
    }
    return ret;
}
INT32 Sockets_LWIP_Driver::Connect(SOCK_SOCKET socket, const struct SOCK_sockaddr* address, INT32 addressLen)
{
    NATIVE_PROFILE_PAL_COM();

    return HAL_SOCK_connect(socket, address, addressLen);
}
INT32 Sockets_LWIP_Driver::Send(SOCK_SOCKET socket, const char* buf, INT32 len, INT32 flags) 
{
    NATIVE_PROFILE_PAL_COM();

    return HAL_SOCK_send(socket, buf, len, flags);
}
INT32 Sockets_LWIP_Driver::Recv(SOCK_SOCKET socket, char* buf, INT32 len, INT32 flags)
{
    NATIVE_PROFILE_PAL_COM();

    return HAL_SOCK_recv(socket, buf, len, flags);
}

INT32 Sockets_LWIP_Driver::Shutdown(SOCK_SOCKET sock, INT32 how)
{
    NATIVE_PROFILE_PAL_COM();

    return HAL_SOCK_shutdown(sock, how);
}

//-----------------------------------------------------------------------------
//
// Close - The close method marks a socket to be closed by the select thread.  Close is handled
// in this way because (at least for one implementation) the select method can not be set for a 
// closed socket.  Therfore in the select thread the socket is closed.
//
//-----------------------------------------------------------------------------
INT32 Sockets_LWIP_Driver::Close(SOCK_SOCKET sock)
{
    NATIVE_PROFILE_PAL_COM();
    UnregisterSocket(sock);
    return HAL_SOCK_close(sock);
}
INT32 Sockets_LWIP_Driver::Listen( SOCK_SOCKET socket, INT32 backlog )
{
    NATIVE_PROFILE_PAL_COM();

    return HAL_SOCK_listen(socket, backlog);
}
SOCK_SOCKET Sockets_LWIP_Driver::Accept( SOCK_SOCKET socket, struct SOCK_sockaddr* address, INT32* addressLen, BOOL fDebug )
{
    NATIVE_PROFILE_PAL_COM();

    int ret = HAL_SOCK_accept(socket, address, addressLen);

    if (ret != SOCK_SOCKET_ERROR)
    {
        RegisterSocket(ret, TRUE, fDebug);
    }    

    return ret;
}    

INT32 Sockets_LWIP_Driver::Select( INT32 nfds, SOCK_fd_set* readfds, SOCK_fd_set* writefds, SOCK_fd_set* exceptfds, const SOCK_timeval* timeout )
{
    NATIVE_PROFILE_PAL_COM();

    return HAL_SOCK_select(nfds, readfds, writefds, exceptfds, timeout);
}

INT32 Sockets_LWIP_Driver::RecvFrom( SOCK_SOCKET s, char* buf, INT32 len, INT32 flags, struct SOCK_sockaddr* from, INT32* fromlen )
{
    NATIVE_PROFILE_PAL_COM();

    return HAL_SOCK_recvfrom(s, buf, len, flags, from, fromlen);
}

INT32 Sockets_LWIP_Driver::SendTo( SOCK_SOCKET s, const char* buf, INT32 len, INT32 flags, const struct SOCK_sockaddr* to, INT32 tolen )
{
    NATIVE_PROFILE_PAL_COM();
    
    return HAL_SOCK_sendto(s, buf, len, flags, to, tolen);
}


//--//

//-----------------------------------------------------------------------------
//
//  MulticastDiscoveryRespond - Respond to a NETMF multicast message.  This is
//  a simple discovery mechanism that uses multicast sockets over UDP.  The
//  client will send a message to the ip address defined by
//     SOCK_DISCOVERY_MULTICAST_IPADDR 
//  over port
//     SOCK_DISCOVERY_MULTICAST_PORT
//  If the message contains the sting "NETMF", then a response is sent with the
//  the data structure SOCK_discoveryinfo (which includes the ip address and mac
//  address of the device).
//
//-----------------------------------------------------------------------------

void Sockets_LWIP_Driver::MulticastDiscoverySchedule()
{
	if (g_Sockets_LWIP_Driver.m_multicastSocket != SOCK_SOCKET_ERROR)
	{
		SOCK_fd_set  privRead;
		SOCK_timeval to;

		to.tv_sec = 0;
		to.tv_usec = 0;

		privRead.fd_array[0] = g_Sockets_LWIP_Driver.m_multicastSocket;
		privRead.fd_count = 1;

		if (1 == HAL_SOCK_select(1, &privRead, NULL, NULL, &to))
		{
			if (!MulticastResponseContinuation.IsLinked())
			{
				MulticastResponseContinuation.Enqueue();
			}
		}
	}
}

void Sockets_LWIP_Driver::MulticastDiscoveryRespond(void* arg)
{
    NATIVE_PROFILE_PAL_COM();
    SOCK_sockaddr from;

    char data[64];
    INT32 fromlen = sizeof(from);

    // intentionally call the HAL recvfrom so as not to invoke the signalnotifythread
    INT32 len = HAL_SOCK_recvfrom( g_Sockets_LWIP_Driver.m_multicastSocket, data, sizeof(data), 0, &from, &fromlen );

    if(len > 0)
    {
        INT32 idx = 0;
        BOOL fFound = FALSE;
        const char* c_Signature = SOCK_DISCOVERY_MULTICAST_TOKEN;
        INT32 sigLen = hal_strlen_s(c_Signature);

        // search for discovery token
        while(idx <= (len-sigLen))
        {
            if(0 == memcmp( &data[idx], c_Signature, sigLen))
            {
                fFound = TRUE;
                break;
            }
            idx++;
        }
        
        if(fFound)
        {
            SOCK_NetworkConfiguration current;
            SOCK_discoveryinfo info;
            SOCK_sockaddr_in sockAddr;
            SOCK_sockaddr_in sockAddrMulticast;
            INT32 opt = 64;
            SOCK_SOCKET sock;
            INT32 nonblocking = 1;
            SOCKET_CHECK_ENTER();

            // Load is required here because the g_NetworkConfig contains only the static ip address (not DHCP)
            HAL_SOCK_CONFIGURATION_LoadAdapterConfiguration(0, &current);

            info.ipaddr        = current.ipaddr;
            info.macAddressLen = current.macAddressLen;
            memcpy( &info.macAddressBuffer[0], &current.macAddressBuffer[0], current.macAddressLen );

            sock = Socket(SOCK_AF_INET, SOCK_SOCK_DGRAM, SOCK_IPPROTO_UDP, FALSE);
            SOCKET_CHECK_RESULT(sock);

            memset( &sockAddr, 0, sizeof(sockAddr) );
            sockAddr.sin_family           = SOCK_AF_INET;
            sockAddr.sin_port             = SOCK_htons(0);
            sockAddr.sin_addr.S_un.S_addr = info.ipaddr;

            memset( &sockAddrMulticast, 0, sizeof(sockAddrMulticast) );
            sockAddrMulticast.sin_family           = SOCK_AF_INET;
            sockAddrMulticast.sin_port             = SOCK_htons(SOCK_DISCOVERY_MULTICAST_PORT);
            sockAddrMulticast.sin_addr.S_un.S_addr = SOCK_htonl(SOCK_DISCOVERY_MULTICAST_IPADDR_SND);

            SOCKET_CHECK_RESULT(HAL_SOCK_ioctl(sock, SOCK_FIONBIO, &nonblocking));
            SOCKET_CHECK_RESULT(HAL_SOCK_setsockopt(sock, SOCK_IPPROTO_IP, SOCK_IPO_MULTICAST_TTL, (const char *) &opt, sizeof(opt)));
            SOCKET_CHECK_RESULT(HAL_SOCK_bind(sock,  (SOCK_sockaddr*)&sockAddr, sizeof(sockAddr)));
            // send a multicast socket back to the caller
            SOCKET_CHECK_RESULT(SendTo(sock, (const char*)&info, sizeof(info), 0, (SOCK_sockaddr*)&sockAddrMulticast, sizeof(sockAddrMulticast)));

            SOCK_close(sock);

            SOCKET_CLEANUP();

            if(sock != SOCK_SOCKET_ERROR)
            {
                SOCK_close(sock);

                debug_printf("MULTICAST RESP SOCKET_ERROR: %d\r\n", HAL_SOCK_getlasterror() );
            }

            SOCKET_CHECK_EXIT_NORETURN();            
        }
    }

	MulticastDiscoverySchedule();
}

BOOL Sockets_LWIP_Driver::InitializeDbgListener( int ComPortNum )
{   
    NATIVE_PROFILE_PAL_COM();
    SOCK_sockaddr_in sockAddr;
    INT32 nonblocking = 1;
    INT32 optval = 1;
    INT32 optLinger = 0;
    SOCKET_CHECK_ENTER();

    if(ComPortNum != ConvertCOM_SockPort(COM_SOCKET_DBG)) return FALSE;

    if(g_Sockets_LWIP_Driver.m_SocketDebugListener != SOCK_SOCKET_ERROR) return TRUE;

    s_DebuggerTimeoutCompletion.InitializeForUserMode(OnDebuggerTimeout);

    //-- debug api socket --//

    g_Sockets_LWIP_Driver.m_SocketDebugListener = Socket( SOCK_AF_INET, SOCK_SOCK_STREAM, SOCK_IPPROTO_TCP, TRUE );
    SOCKET_CHECK_RESULT( g_Sockets_LWIP_Driver.m_SocketDebugListener );

    memset( &sockAddr, 0, sizeof(sockAddr) );
    sockAddr.sin_family             = SOCK_AF_INET;
    sockAddr.sin_port               = SOCK_htons(DEBUG_SOCKET_PORT);
    sockAddr.sin_addr.S_un.S_addr   = SOCK_htonl(SOCK_INADDR_ANY);

    SOCKET_CHECK_RESULT( SOCK_ioctl(g_Sockets_LWIP_Driver.m_SocketDebugListener, SOCK_FIONBIO, &nonblocking) );

    SOCKET_CHECK_RESULT( HAL_SOCK_setsockopt( g_Sockets_LWIP_Driver.m_SocketDebugListener, SOCK_IPPROTO_TCP, SOCK_TCP_NODELAY, (char*)&optval, sizeof(optval) ) );

    SOCKET_CHECK_RESULT( HAL_SOCK_setsockopt(g_Sockets_LWIP_Driver.m_SocketDebugListener, SOCK_SOL_SOCKET, SOCK_SOCKO_LINGER, (const char*)&optLinger, sizeof(INT32)) );

    SOCKET_CHECK_RESULT( SOCK_bind( g_Sockets_LWIP_Driver.m_SocketDebugListener,  (SOCK_sockaddr*)&sockAddr, sizeof(sockAddr) ) );

    SOCKET_CHECK_RESULT( SOCK_listen( g_Sockets_LWIP_Driver.m_SocketDebugListener, 1 ) );

    g_Sockets_LWIP_Driver.m_stateDebugSocket = DbgSock_Listening;

    InitializeMulticastDiscovery();
	MulticastDiscoverySchedule();

    SOCKET_CLEANUP();

    if(g_Sockets_LWIP_Driver.m_SocketDebugListener != SOCK_SOCKET_ERROR)
    {
        SOCK_close(g_Sockets_LWIP_Driver.m_SocketDebugListener);
        g_Sockets_LWIP_Driver.m_SocketDebugListener = SOCK_SOCKET_ERROR;

        debug_printf("DBGLISTENER SOCKET_ERROR: %d\r\n", HAL_SOCK_getlasterror() );
    }

    SOCKET_CHECK_EXIT_BOOL_CLEANUP();
}

BOOL Sockets_LWIP_Driver::UninitializeDbgListener( int ComPortNum )
{
    NATIVE_PROFILE_PAL_COM();

    if(ComPortNum != ConvertCOM_SockPort(COM_SOCKET_DBG)) return FALSE;

    g_Sockets_LWIP_Driver.m_stateDebugSocket = DbgSock_Uninitialized;

    CloseDebuggerSocket();

    if(g_Sockets_LWIP_Driver.m_SocketDebugListener != SOCK_SOCKET_ERROR)
    {
        SOCK_close( g_Sockets_LWIP_Driver.m_SocketDebugListener );
        
        g_Sockets_LWIP_Driver.m_SocketDebugListener = SOCK_SOCKET_ERROR;
    }

    return TRUE;
}

void Sockets_LWIP_Driver::SaveConfig(INT32 index, SOCK_NetworkConfiguration *cfg)
{
    NATIVE_PROFILE_PAL_COM();
    if(index >= NETWORK_INTERFACE_COUNT) return;

    if(cfg) 
    {
        memcpy( &g_NetworkConfig.NetworkInterfaces[index], cfg, sizeof(SOCK_NetworkConfiguration) );
    }
    
    HAL_CONFIG_BLOCK::UpdateBlockWithName(g_NetworkConfig.GetDriverName(), &g_NetworkConfig, sizeof(g_NetworkConfig), TRUE);
}

void Sockets_LWIP_Driver::ApplyConfig()
{
    NATIVE_PROFILE_PAL_COM();
    if(!HAL_CONFIG_BLOCK::ApplyConfig( g_NetworkConfig.GetDriverName(), &g_NetworkConfig, sizeof(g_NetworkConfig) ))
    {
        // save to the dynamic config section so that MFDeploy will be able to get the configuration.
        SaveConfig(0, NULL);            
    }
}

void Sockets_LWIP_Driver::ApplyWirelessConfig()
{
    NATIVE_PROFILE_PAL_COM();

    if(!s_wirelessInitialized)
    {
        if(!HAL_CONFIG_BLOCK::ApplyConfig( g_WirelessConfig.GetDriverName(), &g_WirelessConfig, sizeof(g_WirelessConfig) ))
        {
            SaveWirelessConfig(0, NULL);
        }
        s_wirelessInitialized = TRUE;
    }
}

void Sockets_LWIP_Driver::SaveWirelessConfig(INT32 index, SOCK_NetworkConfiguration *cfg)
{
    NATIVE_PROFILE_PAL_COM();
    if(index >= WIRELESS_INTERFACE_COUNT) return;
    
    if(cfg) 
    {
        memcpy( &g_WirelessConfig.WirelessInterfaces[index], cfg, sizeof(SOCK_WirelessConfiguration) );
    }
    
    HAL_CONFIG_BLOCK::UpdateBlockWithName(g_WirelessConfig.GetDriverName(), &g_WirelessConfig, sizeof(g_WirelessConfig), TRUE);    
}


//-----------------------------------------------------------------------------
//
//  InitializeMulticastDiscovery - Initialize the NETMF discovery service for
//  sockets.  We use multicast sockets to create the discovery mechanism.   
//
//-----------------------------------------------------------------------------
BOOL Sockets_LWIP_Driver::InitializeMulticastDiscovery()
{
    NATIVE_PROFILE_PAL_COM();
    SOCKET_CHECK_ENTER(); 
    SOCK_sockaddr_in sockAddr;
    INT32 nonblocking = 1;

    if(g_Sockets_LWIP_Driver.s_discoveryInitialized) return TRUE;

    MulticastResponseContinuation.InitializeCallback(MulticastDiscoveryRespond, NULL);

    // set up discovery socket to list to defined discovery port for any ip address
    memset( &sockAddr, 0, sizeof(sockAddr) );
    sockAddr.sin_family           = SOCK_AF_INET;
    sockAddr.sin_port             = SOCK_htons(SOCK_DISCOVERY_MULTICAST_PORT);
    sockAddr.sin_addr.S_un.S_addr = SOCK_htonl(SOCK_INADDR_ANY);

    // UDP socket is easier in our scenario because it isn't session based
    g_Sockets_LWIP_Driver.m_multicastSocket = Socket( SOCK_AF_INET, SOCK_SOCK_DGRAM, SOCK_IPPROTO_UDP, FALSE );
    SOCKET_CHECK_RESULT( g_Sockets_LWIP_Driver.m_multicastSocket );

    // set sock option for multicast
    SOCK_ip_mreq multicast;
    multicast.imr_multiaddr.S_un.S_addr = SOCK_htonl(SOCK_DISCOVERY_MULTICAST_IPADDR);
    multicast.imr_interface.S_un.S_addr = SOCK_htonl(SOCK_INADDR_ANY);

    SOCKET_CHECK_RESULT( SOCK_ioctl(g_Sockets_LWIP_Driver.m_multicastSocket, SOCK_FIONBIO, &nonblocking) );

    SOCKET_CHECK_RESULT( SOCK_setsockopt( g_Sockets_LWIP_Driver.m_multicastSocket, SOCK_IPPROTO_IP, SOCK_IPO_ADD_MEMBERSHIP, (const char*)&multicast, sizeof(multicast) ) );

    SOCKET_CHECK_RESULT( SOCK_bind( g_Sockets_LWIP_Driver.m_multicastSocket, (SOCK_sockaddr*)&sockAddr, sizeof(sockAddr) ) );

    g_Sockets_LWIP_Driver.s_discoveryInitialized = TRUE;


    SOCKET_CLEANUP()

    if(g_Sockets_LWIP_Driver.m_multicastSocket != SOCK_SOCKET_ERROR)
    {
        SOCK_close(g_Sockets_LWIP_Driver.m_multicastSocket);
        g_Sockets_LWIP_Driver.m_multicastSocket = SOCK_SOCKET_ERROR;

        debug_printf("MULTICAST SOCKET_ERROR: %d\r\n", HAL_SOCK_getlasterror() );
    }   

    SOCKET_CHECK_EXIT_BOOL_CLEANUP();
}

// TODO. Schedule the Discovery Multicast Response
    
BOOL Sockets_LWIP_Driver::Initialize()
{
    NATIVE_PROFILE_PAL_COM();
    SOCKET_CHECK_ENTER(); 

    if(!s_initialized)
    {
        g_Sockets_LWIP_Driver.m_fShuttingDown = FALSE;

        memset(&MulticastResponseContinuation               , 0, sizeof(MulticastResponseContinuation               ));
        memset(&s_DebuggerTimeoutCompletion                 , 0, sizeof(s_DebuggerTimeoutCompletion                 ));


        g_Sockets_LWIP_Driver.m_multicastSocket     = SOCK_SOCKET_ERROR;
        g_Sockets_LWIP_Driver.m_SocketDebugStream   = SOCK_SOCKET_ERROR;
        g_Sockets_LWIP_Driver.m_SocketDebugListener = SOCK_SOCKET_ERROR;

        g_Sockets_LWIP_Driver.m_cntSockets = 0;
        
        for( INT32 i=0; i<SOCKETS_MAX_COUNT; i++ )
        {
            g_Sockets_LWIP_Driver.m_socketHandles[i].m_socket  = SOCK_SOCKET_ERROR;
            g_Sockets_LWIP_Driver.m_socketHandles[i].m_flags   = 0;
            g_Sockets_LWIP_Driver.m_socketHandles[i].m_sslData = NULL;
        }


        g_Sockets_LWIP_Driver.m_usingSSL = FALSE;

        g_Sockets_LWIP_Driver.m_stateDebugSocket = DbgSock_Uninitialized;

        ApplyConfig();        
        ApplyWirelessConfig();
        
        SOCKET_CHECK_BOOL( HAL_SOCK_Initialize() );

        SSL_Initialize();

        s_initialized = TRUE;
    }
     
    
    SOCKET_CHECK_EXIT_BOOL();
}

BOOL Sockets_LWIP_Driver::Uninitialize( )
{
    NATIVE_PROFILE_PAL_COM();
    BOOL ret = TRUE;
   
    if(s_initialized)
    {
        MulticastResponseContinuation.Abort();
        s_DebuggerTimeoutCompletion.Abort();
        
        g_Sockets_LWIP_Driver.m_stateDebugSocket = DbgSock_Uninitialized;

        // close all connections (including debugger sockets)
        CloseConnections(TRUE);

        SSL_Uninitialize();
    
        g_Sockets_LWIP_Driver.m_multicastSocket     = SOCK_SOCKET_ERROR;
        g_Sockets_LWIP_Driver.m_SocketDebugStream   = SOCK_SOCKET_ERROR;
        g_Sockets_LWIP_Driver.m_SocketDebugListener = SOCK_SOCKET_ERROR;

        ret = HAL_SOCK_Uninitialize();

        s_initialized          = FALSE;
        s_wirelessInitialized  = FALSE;
        s_discoveryInitialized = FALSE;
    }
   
    
    return ret;
}


//-----------------------------------------------------------------------------
//
//  Write - The Write method will write data to the debugger stream socket (if a connection is active).
//  In addition if the write fails for a reason other than EWOULDBLOCK then we should shutdown
//  the debugger stream socket and change to the listening state
//
//-----------------------------------------------------------------------------
INT32 Sockets_LWIP_Driver::Write( int ComPortNum, const char* Data, size_t size )
{
    NATIVE_PROFILE_PAL_COM();
    INT32 ret;

    if(ComPortNum != ConvertCOM_SockPort(COM_SOCKET_DBG)) return SOCK_SOCKET_ERROR;

    if((g_Sockets_LWIP_Driver.m_stateDebugSocket  != DbgSock_Connected) ||
       (g_Sockets_LWIP_Driver.m_SocketDebugStream == SOCK_SOCKET_ERROR)) 
    {
        return SOCK_SOCKET_ERROR;
    }

    if(g_Sockets_LWIP_Driver.m_usingSSL)
    {
        ret = SSL_Write(g_Sockets_LWIP_Driver.m_SocketDebugStream, Data, size);
    }
    else
    {
        ret = SOCK_send( g_Sockets_LWIP_Driver.m_SocketDebugStream, Data, size, 0 );
    }

    if(ret < 0) 
    {
        INT32 err = SOCK_getlasterror();

        // debugger stream is no longer active, change to listening state
        if(err != SOCK_EWOULDBLOCK)
        {
            CloseDebuggerSocket();
        }
        else
        {   
            ret = 0;
        }
    }

    return ret;
}

//-----------------------------------------------------------------------------
//
//  Read - the Read method performs two duties: first to read data from the debug stream; and 
//  second to manage the debugger connection state.  The initial state is to be listening for debug
//  connections, and therefore each read performs an accept to see if there is a pending connection.
//  Once a connection is made the state changes to connected and the debugger stream socket 
//  is read for each Read call.  During the Connected state an Accept is still called so that no other 
//  connections will be handled.  If the debugger is still receiving data on the debugger stream, then
//  new connections will be closed immediately.  If the debugger stream socket has been closed then
//  the state return to the listening state (unless there is an pending connection in which case the 
//  pending connection becomes the new debugger stream).
//
//-----------------------------------------------------------------------------
INT32 Sockets_LWIP_Driver::Read( int ComPortNum, char* Data, size_t size )
{
    NATIVE_PROFILE_PAL_COM();
    SOCK_SOCKET sock;
    SOCK_sockaddr_in addr;
    INT32 len = sizeof(addr);
    INT32 ret = 0;
    SOCK_timeval timeout;
    SOCK_fd_set  readSet;

    if(ComPortNum != ConvertCOM_SockPort(COM_SOCKET_DBG)) return 0;

    if(g_Sockets_LWIP_Driver.m_stateDebugSocket == DbgSock_Uninitialized) return 0;

    memset(&addr, 0, sizeof(addr));

    timeout.tv_sec  = 0;
    timeout.tv_usec = 0;

    readSet.fd_array[0] = g_Sockets_LWIP_Driver.m_SocketDebugListener;
    readSet.fd_count = 1;

    // if we are connected, then read from the debug stream
    if(g_Sockets_LWIP_Driver.m_stateDebugSocket == DbgSock_Connected)
    {
        if(g_Sockets_LWIP_Driver.m_usingSSL)
        {
            ret = SSL_Read(g_Sockets_LWIP_Driver.m_SocketDebugStream, Data, size);
        }
        else
        {
            ret = SOCK_recv(g_Sockets_LWIP_Driver.m_SocketDebugStream, Data, size, 0);
        }

        // return value of zero indicates a shutdown of the socket.  Also we shutdown for any error except
        // ewouldblock.  If either of these happens, then we go back to the listening state
        if((ret == 0) || (ret == SOCK_SOCKET_ERROR && SOCK_getlasterror() != SOCK_EWOULDBLOCK))
        {
            CloseDebuggerSocket();
        }
        else if(ret != SOCK_SOCKET_ERROR && s_DebuggerTimeoutCompletion.IsLinked())
        {
            // we either got data or the socket was closed, so kill the timeout
            s_DebuggerTimeoutCompletion.Abort();
        }
    }

    if(SOCK_SOCKET_ERROR != HAL_SOCK_select( SOCK_FD_SETSIZE, &readSet, NULL, NULL, &timeout))
    {
        // we always perform an accept so that we handle pending connections
        // if we alread are connected and the debug stream socket is still active, then we immediately close
        // the pending connection
        sock = Accept( g_Sockets_LWIP_Driver.m_SocketDebugListener, (SOCK_sockaddr*)&addr, &len, TRUE );

        if(SOCK_SOCKET_ERROR != sock)
        {
            INT32 nonblocking = 1;
            BOOL  optval      = 1;
            INT32 optLinger   = 0;

            // if we are already in the connected state, then verify that the debugger stream socket is still active
            if(DbgSock_Connected == g_Sockets_LWIP_Driver.m_stateDebugSocket)
            {
                // the debugger stream is still active, so shutdown the pending connection
                HAL_SOCK_setsockopt( sock, SOCK_SOL_SOCKET, SOCK_SOCKO_LINGER, (const char*)&optLinger, sizeof(INT32) );
                SOCK_close(sock);

                // set timeout since another connection is trying to use us.
                if(!s_DebuggerTimeoutCompletion.IsLinked())
                {
                    s_DebuggerTimeoutCompletion.EnqueueDelta(5000000); // 5 seconds
                }
            }
            else // we are in the listening state, so accept the pending connection and update the state
            {
                g_Sockets_LWIP_Driver.m_SocketDebugStream = sock;
                SOCKET_CHECK_RESULT( SOCK_ioctl(g_Sockets_LWIP_Driver.m_SocketDebugStream, SOCK_FIONBIO, &nonblocking) );
                SOCKET_CHECK_RESULT( HAL_SOCK_setsockopt( g_Sockets_LWIP_Driver.m_SocketDebugStream, SOCK_IPPROTO_TCP, SOCK_TCP_NODELAY, (char*)&optval, sizeof(optval) ) );
                SOCKET_CHECK_RESULT( HAL_SOCK_setsockopt( g_Sockets_LWIP_Driver.m_SocketDebugStream, SOCK_SOL_SOCKET, SOCK_SOCKO_LINGER, (const char*)&optLinger, sizeof(INT32) ) );

                g_Sockets_LWIP_Driver.m_stateDebugSocket = DbgSock_Connected;

                SOCKET_CLEANUP()

                if(g_Sockets_LWIP_Driver.m_SocketDebugStream != SOCK_SOCKET_ERROR)
                {
                    SOCK_close(g_Sockets_LWIP_Driver.m_SocketDebugStream);
                    g_Sockets_LWIP_Driver.m_SocketDebugStream = SOCK_SOCKET_ERROR;

                    debug_printf("DBGSTREAM SOCKET_ERROR: %d\r\n", HAL_SOCK_getlasterror() );
                }

                SOCKET_CHECK_EXIT_NORETURN();
            }
            
            HAL_SOCK_EventsSet( SOCKET_EVENT_FLAG_SOCKET );
        }
    }

    if(ret < 0)
    {  
        ret = 0;
    }

    return ret;
}

BOOL Sockets_LWIP_Driver::IsUsingSsl( int ComPortNum )
{
    if(ComPortNum != ConvertCOM_SockPort(COM_SOCKET_DBG)) return FALSE;
    if(g_Sockets_LWIP_Driver.m_stateDebugSocket == DbgSock_Uninitialized) return FALSE;

    if(g_Sockets_LWIP_Driver.m_stateDebugSocket == DbgSock_Connected)
    {
        return g_Sockets_LWIP_Driver.m_usingSSL;
    }

    return FALSE;
}


BOOL Sockets_LWIP_Driver::UpgradeToSsl( int ComPortNum, const UINT8* pCACert, UINT32 caCertLen, const UINT8* pDeviceCert, UINT32 deviceCertLen, LPCSTR szTargetHost  )
{
    if(ComPortNum != ConvertCOM_SockPort(COM_SOCKET_DBG)) return 0;
    if(g_Sockets_LWIP_Driver.m_stateDebugSocket == DbgSock_Uninitialized) return 0;
    
    if(g_Sockets_LWIP_Driver.m_stateDebugSocket == DbgSock_Connected)
    {
        if(g_Sockets_LWIP_Driver.m_usingSSL) return TRUE;

        // TLS only and Verify=Required --> only verify the server
        if(SSL_ClientInit( 0x10, 0x04, (const char*)pDeviceCert, deviceCertLen, NULL, g_DebuggerPort_SslCtx_Handle ))
        {
            INT32 ret;
    
            SSL_AddCertificateAuthority( g_DebuggerPort_SslCtx_Handle, (const char*)pCACert, caCertLen, NULL);
    
            do
            {
                ret = SSL_Connect( g_Sockets_LWIP_Driver.m_SocketDebugStream, szTargetHost, g_DebuggerPort_SslCtx_Handle );
            }
            while(ret == SOCK_EWOULDBLOCK || ret == SOCK_TRY_AGAIN);
    
    
            if(ret != 0)
            {
                SSL_CloseSocket(g_Sockets_LWIP_Driver.m_SocketDebugStream);
                SSL_ExitContext(g_DebuggerPort_SslCtx_Handle);
            }
            else
            {
                g_Sockets_LWIP_Driver.m_usingSSL = TRUE;
            }
    
            return ret == 0;
        }
    }

    return FALSE;
}


//-----------------------------------------------------------------------------
//
//  RegisterSocket - socket tracking.  This method is used to track sockets
//    opened by this driver.  It does not include the fake socket.  Register
//    should be called after a socket is opened (via socket or accept methods).
//    The selectable parameter should only be true if the socket is in the 
//    listening or connected state
//
//-----------------------------------------------------------------------------
void Sockets_LWIP_Driver::RegisterSocket( SOCK_SOCKET sock, BOOL selectable, BOOL fDebug )
{
    NATIVE_PROFILE_PAL_COM();
    if(sock == SOCK_SOCKET_ERROR)
    {
        ASSERT(FALSE);
        return;
    }

    if(g_Sockets_LWIP_Driver.m_cntSockets >= SOCKETS_MAX_COUNT) return;

    GLOBAL_LOCK_SOCKETS(lock);

    g_Sockets_LWIP_Driver.m_socketHandles[g_Sockets_LWIP_Driver.m_cntSockets].m_socket  = sock;
    g_Sockets_LWIP_Driver.m_socketHandles[g_Sockets_LWIP_Driver.m_cntSockets].m_flags   = 0;
    g_Sockets_LWIP_Driver.m_socketHandles[g_Sockets_LWIP_Driver.m_cntSockets].m_sslData = NULL;

    if(fDebug) SET_SOCKET_FLAG(g_Sockets_LWIP_Driver.m_socketHandles[g_Sockets_LWIP_Driver.m_cntSockets], SocketRegisterMap::c_DebugSocket);

    g_Sockets_LWIP_Driver.m_cntSockets++;
}

//-----------------------------------------------------------------------------
//
//  UnregisterSocket - No longer track a given socket for clean up and selection.
//    This method should only be called immediately before closing down a socket.
//
//-----------------------------------------------------------------------------
void Sockets_LWIP_Driver::UnregisterSocket( SOCK_SOCKET sock )
{
    INT32 index= -1;
    
    NATIVE_PROFILE_PAL_COM();
    GLOBAL_LOCK_SOCKETS(lock);

    g_Sockets_LWIP_Driver.GetSocketSslData(sock, index);

    if (index == -1) return;

    g_Sockets_LWIP_Driver.m_cntSockets--;

    if(index != g_Sockets_LWIP_Driver.m_cntSockets)
    {
        memcpy( &g_Sockets_LWIP_Driver.m_socketHandles[index], 
                &g_Sockets_LWIP_Driver.m_socketHandles[g_Sockets_LWIP_Driver.m_cntSockets], 
                sizeof(g_Sockets_LWIP_Driver.m_socketHandles[index]) );
    }

    g_Sockets_LWIP_Driver.m_socketHandles[g_Sockets_LWIP_Driver.m_cntSockets].m_socket = SOCK_SOCKET_ERROR;
    g_Sockets_LWIP_Driver.m_socketHandles[g_Sockets_LWIP_Driver.m_cntSockets].m_flags  = 0;
}

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_Sockets_LWIP_Driver"
#endif

BOOL Sockets_LWIP_Driver::s_initialized=FALSE;
BOOL Sockets_LWIP_Driver::s_wirelessInitialized=FALSE;
BOOL Sockets_LWIP_Driver::s_discoveryInitialized=FALSE;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

