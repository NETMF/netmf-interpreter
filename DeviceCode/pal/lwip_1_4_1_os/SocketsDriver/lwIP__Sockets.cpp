////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "LWIP_sockets.h"

extern "C"
{
#include "lwip\init.h"
#include "lwip\tcpip.h"
#include "lwip\dns.h"
#include "lwip\netifapi.h"
#include "lwip\Netdb.h"
#include "lwip\tcp.h"
#include "lwip\Sockets.h"
}

extern const HAL_CONFIG_BLOCK   g_NetworkConfigHeader;
extern NETWORK_CONFIG           g_NetworkConfig;

#if defined(__RENESAS__)
volatile int errno;
#elif !( defined(_MSC_VER) && defined(_WIN32) && defined(_DLL) )
int errno;
#endif

//--// 

#if defined(DEBUG)
#define DEBUG_HANDLE_SOCKET_ERROR(t,a) 
// assume there is something to add in later??
#else
#define DEBUG_HANDLE_SOCKET_ERROR(t,a) 
#endif

struct netif *netif_find_interface(int num);

//--//

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_LWIP_SOCKETS_Driver"
#endif

LWIP_SOCKETS_Driver g_LWIP_SOCKETS_Driver;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

//--//

static HAL_CONTINUATION PostAddressChangedContinuation;
static HAL_CONTINUATION PostAvailabilityOnContinuation;
static HAL_CONTINUATION PostAvailabilityOffContinuation;

void LWIP_SOCKETS_Driver::PostAddressChanged(void* arg)
{
	Network_PostEvent(NETWORK_EVENT_TYPE_ADDRESS_CHANGED, 0);
}

void LWIP_SOCKETS_Driver::PostAvailabilityOn(void* arg)
{
	Network_PostEvent(NETWORK_EVENT_TYPE__AVAILABILITY_CHANGED, 1);
}

void LWIP_SOCKETS_Driver::PostAvailabilityOff(void* arg)
{
	Network_PostEvent(NETWORK_EVENT_TYPE__AVAILABILITY_CHANGED, 0);
}

void LWIP_SOCKETS_Driver::Link_callback(struct netif *netif)
{
	if (netif_is_link_up(netif))
	{
		if (!PostAvailabilityOnContinuation.IsLinked())
			PostAvailabilityOnContinuation.Enqueue();
	}
	else
	{
		if (!PostAvailabilityOffContinuation.IsLinked())
			PostAvailabilityOffContinuation.Enqueue();
	}
    Events_Set(SYSTEM_EVENT_FLAG_SOCKET);
    Events_Set(SYSTEM_EVENT_FLAG_NETWORK);
}

void LWIP_SOCKETS_Driver::Status_callback(struct netif *netif)
{
	if (!PostAddressChangedContinuation.IsLinked())
		PostAddressChangedContinuation.Enqueue();

#if !defined(BUILD_RTM)
	lcd_printf("\f\n\n\n\n\n\nLink Update: %s\n", (netif_is_up(netif) ? "UP  " : "DOWN"));
	lcd_printf("         IP: %d.%d.%d.%d\n", (netif->ip_addr.addr >> 0) & 0xFF,
		(netif->ip_addr.addr >> 8) & 0xFF,
		(netif->ip_addr.addr >> 16) & 0xFF,
		(netif->ip_addr.addr >> 24) & 0xFF);
	lcd_printf("         SM: %d.%d.%d.%d\n", (netif->netmask.addr >> 0) & 0xFF,
		(netif->netmask.addr >> 8) & 0xFF,
		(netif->netmask.addr >> 16) & 0xFF,
		(netif->netmask.addr >> 24) & 0xFF);
	lcd_printf("         GW: %d.%d.%d.%d\n", (netif->gw.addr >> 0) & 0xFF,
		(netif->gw.addr >> 8) & 0xFF,
		(netif->gw.addr >> 16) & 0xFF,
		(netif->gw.addr >> 24) & 0xFF);
	debug_printf("IP Address: %d.%d.%d.%d\n", (netif->ip_addr.addr >> 0) & 0xFF,
		(netif->ip_addr.addr >> 8) & 0xFF,
		(netif->ip_addr.addr >> 16) & 0xFF,
		(netif->ip_addr.addr >> 24) & 0xFF);
#if LWIP_DNS
	if (netif->flags & NETIF_FLAG_DHCP)
	{
		struct ip_addr dns1 = dns_getserver(0);
		struct ip_addr dns2 = dns_getserver(1);

		lcd_printf("         dns1: %d.%d.%d.%d\n", (dns1.addr >> 0) & 0xFF,
			(dns1.addr >> 8) & 0xFF,
			(dns1.addr >> 16) & 0xFF,
			(dns1.addr >> 24) & 0xFF);

		lcd_printf("         dns2: %d.%d.%d.%d\n", (dns2.addr >> 0) & 0xFF,
			(dns2.addr >> 8) & 0xFF,
			(dns2.addr >> 16) & 0xFF,
			(dns2.addr >> 24) & 0xFF);
	}
#endif
#endif
    Events_Set(SYSTEM_EVENT_FLAG_SOCKET);
    Events_Set(SYSTEM_EVENT_FLAG_NETWORK);
}

void LWIP_SOCKETS_Driver::TcpipInitDone(void* arg)
{
	struct netif *pNetIf;

	for (int i = 0; i<g_NetworkConfig.NetworkInterfaceCount; i++)
	{
		int interfaceNumber;

		SOCK_NetworkConfiguration *pNetCfg = &g_NetworkConfig.NetworkInterfaces[i];

		/* Bind and Open the Ethernet driver */
		Network_Interface_Bind(i);
		interfaceNumber = Network_Interface_Open(i);

		if (interfaceNumber == SOCK_SOCKET_ERROR)
		{
			DEBUG_HANDLE_SOCKET_ERROR("Network init", FALSE);
			debug_printf("SocketError: %d\n", errno);
			continue;
		}

		g_LWIP_SOCKETS_Driver.m_interfaces[i].m_interfaceNumber = interfaceNumber;

		UpdateAdapterConfiguration(i, SOCK_NETWORKCONFIGURATION_UPDATE_DHCP | SOCK_NETWORKCONFIGURATION_UPDATE_DNS, pNetCfg);

		pNetIf = netif_find_interface(interfaceNumber);

		if (pNetIf)
		{		
			netif_set_link_callback(pNetIf, Link_callback);
			if (netif_is_link_up(pNetIf))
				Link_callback(pNetIf);

			netif_set_status_callback(pNetIf, Status_callback);
			if (netif_is_up(pNetIf))
				Status_callback(pNetIf);

			// default debugger interface
            if (0 == i)
            {
                UINT8* addr = (UINT8*)&pNetIf->ip_addr.addr;
                lcd_printf("\f\n\n\n\n\n\n\nip address: %d.%d.%d.%d\r\n", addr[0], addr[1], addr[2], addr[3]);
                debug_printf("ip address from interface info: %d.%d.%d.%d\r\n", addr[0], addr[1], addr[2], addr[3]);
            }
		}
	}
}

BOOL LWIP_SOCKETS_Driver::Initialize()
{   
    NATIVE_PROFILE_PAL_NETWORK();

    PostAddressChangedContinuation.InitializeCallback(PostAddressChanged, NULL);
    PostAvailabilityOnContinuation.InitializeCallback(PostAvailabilityOn, NULL);
    PostAvailabilityOffContinuation.InitializeCallback(PostAvailabilityOff, NULL);

    /* Initialize the lwIP stack */
    tcpip_init(TcpipInitDone, NULL);
    
    return TRUE;
}

BOOL LWIP_SOCKETS_Driver::Uninitialize()
{
    NATIVE_PROFILE_PAL_NETWORK();      

    PostAddressChangedContinuation.Abort();
    PostAvailabilityOnContinuation.Abort();
    PostAvailabilityOffContinuation.Abort();

    for(int i=0; i<g_NetworkConfig.NetworkInterfaceCount; i++)
    {
        Network_Interface_Close(i);
    }

    tcpip_shutdown();

    return TRUE;
}


SOCK_SOCKET LWIP_SOCKETS_Driver::Socket(int family, int type, int protocol) 
{  
    NATIVE_PROFILE_PAL_NETWORK();

    switch(protocol)
    {
        case SOCK_IPPROTO_TCP:
            protocol = IPPROTO_TCP;
            break;
        case SOCK_IPPROTO_UDP:
            protocol = IPPROTO_UDP;
            break;
        case SOCK_IPPROTO_ICMP:
            protocol = IP_PROTO_ICMP;
            break;

        case SOCK_IPPROTO_IGMP:
            protocol = IP_PROTO_IGMP;
            break;
    }
    
    return lwip_socket(family, type, protocol);
}

int LWIP_SOCKETS_Driver::Bind(SOCK_SOCKET socket, const SOCK_sockaddr* address, int addressLen) 
{ 
    NATIVE_PROFILE_PAL_NETWORK();

    sockaddr_in addr;

    SOCK_SOCKADDR_TO_SOCKADDR(address, addr, &addressLen);
        
    return lwip_bind(socket, (sockaddr*)&addr, addressLen);
}

int LWIP_SOCKETS_Driver::Connect(SOCK_SOCKET socket, const SOCK_sockaddr* address, int addressLen) 
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    
    sockaddr_in addr;

    SOCK_SOCKADDR_TO_SOCKADDR(address, addr, &addressLen);
        
    return lwip_connect(socket, (sockaddr*)&addr, addressLen);
}

int LWIP_SOCKETS_Driver::Send(SOCK_SOCKET socket, const char* buf, int len, int flags) 
{ 
    NATIVE_PROFILE_PAL_NETWORK();
        
    return lwip_send(socket, (const void*)buf, len, flags);
}


int LWIP_SOCKETS_Driver::Recv(SOCK_SOCKET socket, char* buf, int len, int flags)
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    int     nativeFlag;
    
    switch (flags)
    {
        case SOCKET_READ_PEEK_OPTION:
            nativeFlag = MSG_PEEK;
            break;
        default:
            nativeFlag = flags;
            break;
    }
    
    return lwip_recv(socket,(void*)buf, len, nativeFlag);
}

int LWIP_SOCKETS_Driver::Close(SOCK_SOCKET socket)
{ 
    NATIVE_PROFILE_PAL_NETWORK();

    return lwip_close(socket);
}

int LWIP_SOCKETS_Driver::Listen(SOCK_SOCKET socket, int backlog)
{    
    NATIVE_PROFILE_PAL_NETWORK();
    
    return lwip_listen(socket, backlog);
}

SOCK_SOCKET LWIP_SOCKETS_Driver::Accept(SOCK_SOCKET socket, SOCK_sockaddr* address, int* addressLen)
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    SOCK_SOCKET ret;

    sockaddr_in addr;

    if (address)
    {
        SOCK_SOCKADDR_TO_SOCKADDR(address, addr, addressLen);
    }
    
    ret = lwip_accept(socket, address?(sockaddr*)&addr:NULL, (u32_t*)addressLen);
    
    if(address)
    {
        SOCKADDR_TO_SOCK_SOCKADDR(address, addr, addressLen);
    }
    
    return ret;
}

int LWIP_SOCKETS_Driver::Shutdown( SOCK_SOCKET socket, int how )
{    
    NATIVE_PROFILE_PAL_NETWORK();
    
    return lwip_shutdown (socket, how);
}

int LWIP_SOCKETS_Driver::GetAddrInfo(const char* nodename, char* servname, const SOCK_addrinfo* hints, SOCK_addrinfo** res)
{ 
#if LWIP_DNS
    NATIVE_PROFILE_PAL_NETWORK();

    SOCK_addrinfo *ai;
    SOCK_sockaddr_in *sa = NULL;
    int total_size = sizeof(SOCK_addrinfo) + sizeof(SOCK_sockaddr_in);
    struct addrinfo *lwipAddrinfo = NULL;

    if(res == NULL) return -1;

    *res = NULL;

    // if the nodename == "" then return the IP address of this device
    if(nodename[0] == 0 && servname == NULL)
    {
        struct netif *pNetIf = netif_find_interface(g_LWIP_SOCKETS_Driver.m_interfaces[0].m_interfaceNumber);

        if(pNetIf == NULL) return -1;

        ai = (SOCK_addrinfo*)mem_malloc(total_size);
        if (ai == NULL) 
        {
            return -1;
        }
        memset(ai, 0, total_size);
        sa = (SOCK_sockaddr_in*)((u8_t*)ai + sizeof(SOCK_addrinfo));
        /* set up sockaddr */
        sa->sin_addr.S_un.S_addr = pNetIf->ip_addr.addr;
        sa->sin_family = AF_INET;
        sa->sin_port = 0;
        
        /* set up addrinfo */
        ai->ai_family = AF_INET;
        if (hints != NULL) 
        {
            /* copy socktype & protocol from hints if specified */
            ai->ai_socktype = hints->ai_socktype;
            ai->ai_protocol = hints->ai_protocol;
        }

        ai->ai_addrlen = sizeof(SOCK_sockaddr_in);
        ai->ai_addr = (SOCK_sockaddr*)sa;

        *res = ai;

        return 0;
    }

    int err = lwip_getaddrinfo(nodename, servname, (addrinfo*)hints, &lwipAddrinfo);

    if(err == 0)
    {
        ///
        /// Marshal addrinfo data
        ///
        struct sockaddr_in* lwip_sockaddr_in;
        
        ai = (SOCK_addrinfo*)mem_malloc(total_size);
        if (ai == NULL) 
        {
            lwip_freeaddrinfo(lwipAddrinfo);
            return -1;
        }
        memset(ai, 0, total_size);

        lwip_sockaddr_in = ((struct sockaddr_in*)lwipAddrinfo->ai_addr);

        sa = (SOCK_sockaddr_in*)((u8_t*)ai + sizeof(SOCK_addrinfo));
        /* set up sockaddr */
        sa->sin_addr.S_un.S_addr = lwip_sockaddr_in->sin_addr.s_addr;
        sa->sin_family = lwip_sockaddr_in->sin_family;
        sa->sin_port = lwip_sockaddr_in->sin_port;
        
        /* set up addrinfo */
        ai->ai_family = lwipAddrinfo->ai_family;
        if (hints != NULL) 
        {
            /* copy socktype & protocol from hints if specified */
            ai->ai_socktype = hints->ai_socktype;
            ai->ai_protocol = hints->ai_protocol;
        }
        
        ai->ai_addrlen = sizeof(SOCK_sockaddr_in);
        ai->ai_addr = (SOCK_sockaddr*)sa;
        
        *res = ai;

        // free marshalled addrinfo
        lwip_freeaddrinfo(lwipAddrinfo);
        
    }
    else
    {
        err = -1;
    }
 
    return err;
#else
    return -1;
#endif
}

void LWIP_SOCKETS_Driver::FreeAddrInfo( SOCK_addrinfo* ai )
{ 
    NATIVE_PROFILE_PAL_NETWORK();

    SOCK_addrinfo *next;
    
    while (ai != NULL) {
      next = ai->ai_next;
      mem_free(ai);
      ai = next;
    }
}

int LWIP_SOCKETS_Driver::Ioctl( SOCK_SOCKET socket, int cmd, int* data )
{ 
    NATIVE_PROFILE_PAL_NETWORK();

    return lwip_ioctl(socket,cmd,data);
}

int LWIP_SOCKETS_Driver::GetLastError()
{
    NATIVE_PROFILE_PAL_NETWORK();

    return GetNativeError(errno);
}

static int MARSHAL_SOCK_FDSET_TO_FDSET(SOCK_fd_set *sf, fd_set *f)
{
    if(f != NULL && sf != NULL) 
    { 
        FD_ZERO(f);
        
        for(unsigned int i=0; i < sf->fd_count; i++) 
        { 
            FD_SET(sf->fd_array[i], f); 
        } 
        return sf->fd_count;
    } 

    return 0;
}

static void MARSHAL_FDSET_TO_SOCK_FDSET(SOCK_fd_set *sf, fd_set *f)
{
    if(sf != NULL && f != NULL) 
    { 
        int cnt = sf->fd_count;
        sf->fd_count = 0; 
        for(int i=0; i<cnt; i++) 
        { 
            if(FD_ISSET(sf->fd_array[i],f)) 
            { 
                sf->fd_array[sf->fd_count] = sf->fd_array[i]; 
                sf->fd_count++; 
            } 
        } 
    } 
}
    

int LWIP_SOCKETS_Driver::Select( int nfds, SOCK_fd_set* readfds, SOCK_fd_set* writefds, SOCK_fd_set* exceptfds, const SOCK_timeval* timeout )
{
    NATIVE_PROFILE_PAL_NETWORK();
    int ret = 0;

    fd_set read;
    fd_set write;
    fd_set excpt;

    fd_set* pR = (readfds   != NULL) ? &read  : NULL;
    fd_set* pW = (writefds  != NULL) ? &write : NULL;
    fd_set* pE = (exceptfds != NULL) ? &excpt : NULL;

    // If the network goes down then we should alert any pending socket actions
    if(exceptfds != NULL && exceptfds->fd_count > 0)
    {
        struct netif *pNetIf = netif_find_interface(g_LWIP_SOCKETS_Driver.m_interfaces[0].m_interfaceNumber);

        if(pNetIf != NULL)
        {
            if(!netif_is_up(pNetIf))
            {
                if(readfds  != NULL) readfds->fd_count = 0;
                if(writefds != NULL) writefds->fd_count = 0;

                errno = ENETDOWN;

                return exceptfds->fd_count;
            }
        }
    }

    MARSHAL_SOCK_FDSET_TO_FDSET(readfds  , pR);
    MARSHAL_SOCK_FDSET_TO_FDSET(writefds , pW);
    MARSHAL_SOCK_FDSET_TO_FDSET(exceptfds, pE);

    ret = lwip_select(MEMP_NUM_NETCONN, pR, pW, pE, (struct timeval *)timeout);

    MARSHAL_FDSET_TO_SOCK_FDSET(readfds  , pR);
    MARSHAL_FDSET_TO_SOCK_FDSET(writefds , pW);
    MARSHAL_FDSET_TO_SOCK_FDSET(exceptfds, pE);

    return ret;
}

int LWIP_SOCKETS_Driver::SetSockOpt( SOCK_SOCKET socket, int level, int optname, const char* optval, int  optlen )
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    int nativeLevel;
    int nativeOptionName;
    int nativeIntValue;
    char *pNativeOptionValue = (char*)optval;

    switch(level)
    {
        case SOCK_IPPROTO_IP:
            nativeLevel = IPPROTO_IP;
            nativeOptionName = GetNativeIPOption(optname);
            break;
        case SOCK_IPPROTO_TCP:    
            nativeLevel = IPPROTO_TCP;
            nativeOptionName = GetNativeTcpOption(optname);
            break;
        case SOCK_IPPROTO_UDP: 
        case SOCK_IPPROTO_ICMP:
        case SOCK_IPPROTO_IGMP:
        case SOCK_IPPROTO_IPV4:
        case SOCK_SOL_SOCKET:
            nativeLevel      = SOL_SOCKET;
            nativeOptionName = GetNativeSockOption(optname);            

            switch(optname)
            {        
                // LINGER and DONTLINGER are not implemented in LWIP
                case SOCK_SOCKO_LINGER:
                    errno = SOCK_ENOPROTOOPT;
                    return SOCK_SOCKET_ERROR;
                case SOCK_SOCKO_DONTLINGER:
                    errno = SOCK_ENOPROTOOPT;
                    return SOCK_SOCKET_ERROR;
				// ignore this item to enable http to work
				case SOCK_SOCKO_REUSEADDRESS:
					return 0;
                
                case SOCK_SOCKO_EXCLUSIVEADDRESSUSE:
                    nativeIntValue     = !*(int*)optval;
                    pNativeOptionValue = (char*)&nativeIntValue;
                    break;
                default:
                    break;
            }
            break;
        default:
            nativeLevel         = 0;
            nativeOptionName    = 0;
            break;
    }

    return lwip_setsockopt(socket, nativeLevel, nativeOptionName, pNativeOptionValue, optlen);
}

int LWIP_SOCKETS_Driver::GetSockOpt( SOCK_SOCKET socket, int level, int optname, char* optval, int* optlen )
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    int         nativeLevel;
    int         nativeOptionName;
    char*       pNativeOptval = optval;
    int         ret;
    
    switch(level)
    {
        case SOCK_IPPROTO_IP:
            nativeLevel         = IPPROTO_IP;
            nativeOptionName    = GetNativeIPOption(optname);
            break;
        case SOCK_IPPROTO_TCP:    
            nativeLevel         = IPPROTO_TCP;
            nativeOptionName    = GetNativeTcpOption(optname);
            break;
        case SOCK_IPPROTO_UDP: 
        case SOCK_IPPROTO_ICMP:
        case SOCK_IPPROTO_IGMP:
        case SOCK_IPPROTO_IPV4:
        case SOCK_SOL_SOCKET:
            nativeLevel         = SOL_SOCKET;
            nativeOptionName    = GetNativeSockOption(optname);
            switch(optname)
            {        
                // LINGER and DONTLINGER are not implemented in LWIP
                case SOCK_SOCKO_LINGER:
                    errno = SOCK_ENOPROTOOPT;
                    return SOCK_SOCKET_ERROR;
                case SOCK_SOCKO_DONTLINGER:
                    errno = SOCK_ENOPROTOOPT;
                    return SOCK_SOCKET_ERROR;
                default:
                    break;
            }
            break;
        default:
            nativeLevel         = level;
            nativeOptionName    = optname;
            break;
    }

    ret = lwip_getsockopt(socket, nativeLevel, nativeOptionName, pNativeOptval, (u32_t*)optlen);

    if(ret == 0)
    {
        switch(level)
        {
            case SOCK_SOL_SOCKET:
                switch(optname)
                {       
                    case SOCK_SOCKO_EXCLUSIVEADDRESSUSE:
                        *optval = !(*(int*)optval != 0);
                        break;
                        
                    case SOCK_SOCKO_ACCEPTCONNECTION:
                    case SOCK_SOCKO_BROADCAST:
                    case SOCK_SOCKO_KEEPALIVE:
                        *optval = (*(int*)optval != 0);
                        break;
                }
                break;
        }
    }

    return ret;    
}

int LWIP_SOCKETS_Driver::GetPeerName( SOCK_SOCKET socket, SOCK_sockaddr* name, int* namelen )
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    int ret;

    sockaddr_in addr;

    SOCK_SOCKADDR_TO_SOCKADDR(name, addr, namelen);

    ret = lwip_getpeername(socket, (sockaddr*)&addr, (u32_t*)namelen);

    SOCKADDR_TO_SOCK_SOCKADDR(name, addr, namelen);

    return ret;
}

int LWIP_SOCKETS_Driver::GetSockName( SOCK_SOCKET socket, SOCK_sockaddr* name, int* namelen )
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    int ret;

    sockaddr_in addr;

    SOCK_SOCKADDR_TO_SOCKADDR(name, addr, namelen);

    ret = lwip_getsockname(socket, (sockaddr*)&addr, (u32_t*)namelen);

    SOCKADDR_TO_SOCK_SOCKADDR(name, addr, namelen);

    return ret;
}

int LWIP_SOCKETS_Driver::RecvFrom( SOCK_SOCKET socket, char* buf, int len, int flags, SOCK_sockaddr* from, int* fromlen )
{ 
    NATIVE_PROFILE_PAL_NETWORK();
    sockaddr_in addr;
    sockaddr *pFrom = NULL;
    int ret;

    if(from)
    {
        SOCK_SOCKADDR_TO_SOCKADDR(from, addr, fromlen);
        pFrom = (sockaddr*)&addr;
    }
        
    ret = lwip_recvfrom(socket, buf, len, flags, pFrom, (u32_t*)fromlen);

    if(from && ret != SOCK_SOCKET_ERROR)
    {
        SOCKADDR_TO_SOCK_SOCKADDR(from, addr, fromlen);
    }

    return ret;
}

int LWIP_SOCKETS_Driver::SendTo( SOCK_SOCKET socket, const char* buf, int len, int flags, const SOCK_sockaddr* to, int tolen )
{ 
    NATIVE_PROFILE_PAL_NETWORK();

    sockaddr_in addr;

    SOCK_SOCKADDR_TO_SOCKADDR(to, addr, &tolen);

    return lwip_sendto(socket, buf, len, flags, (sockaddr*)&addr, (u32_t)tolen);
}

UINT32 LWIP_SOCKETS_Driver::GetAdapterCount()
{
    NATIVE_PROFILE_PAL_NETWORK();
    return NETWORK_INTERFACE_COUNT;
}

HRESULT LWIP_SOCKETS_Driver::LoadAdapterConfiguration( UINT32 interfaceIndex, SOCK_NetworkConfiguration* config )
{
    NATIVE_PROFILE_PAL_NETWORK();

    if(interfaceIndex >= NETWORK_INTERFACE_COUNT) 
    {
        return CLR_E_INVALID_PARAMETER;
    }

    memcpy(config, &g_NetworkConfig.NetworkInterfaces[interfaceIndex], sizeof(g_NetworkConfig.NetworkInterfaces[interfaceIndex]));

    if(config->flags & SOCK_NETWORKCONFIGURATION_FLAGS_DHCP)
    {
        struct netif *pNetIf;

        if (pNetIf = netif_find_interface(g_LWIP_SOCKETS_Driver.m_interfaces[interfaceIndex].m_interfaceNumber))
        {
            config->ipaddr     = pNetIf->ip_addr.addr;
            config->subnetmask = pNetIf->netmask.addr;
            config->gateway    = pNetIf->gw.addr;
#if LWIP_DNS
            config->dnsServer1 = dns_getserver(0).addr;
            config->dnsServer2 = dns_getserver(1).addr;
#endif
        }
        else
        {
            config->ipaddr     = 0;
            config->subnetmask = 0;
            config->gateway    = 0;
        }
    }
    
    return S_OK;
}

HRESULT LWIP_SOCKETS_Driver::LoadWirelessConfiguration( UINT32 interfaceIndex, SOCK_WirelessConfiguration* wirelessConfig )
{
    /// Load wireless specific settings if any. You must return S_OK, otherwise default values will be
    /// loaded by PAL.

    return CLR_E_FAIL;
}

struct dhcp_client_id
{
    UINT8 code;
    UINT8 length;
    UINT8 type;
    UINT8 clientId[6];
};

HRESULT LWIP_SOCKETS_Driver::UpdateAdapterConfiguration( UINT32 interfaceIndex, UINT32 updateFlags, SOCK_NetworkConfiguration* config )
{
    NATIVE_PROFILE_PAL_NETWORK();
    if(interfaceIndex >= NETWORK_INTERFACE_COUNT) 
    {
        return CLR_E_INVALID_PARAMETER;
    }
    
    BOOL fEnableDhcp = (0 != (config->flags & SOCK_NETWORKCONFIGURATION_FLAGS_DHCP));
    BOOL fDynamicDns = (0 != (config->flags & SOCK_NETWORKCONFIGURATION_FLAGS_DYNAMIC_DNS));
    BOOL fDhcpStarted;

    struct netif *pNetIf = netif_find_interface(g_LWIP_SOCKETS_Driver.m_interfaces[interfaceIndex].m_interfaceNumber);
    if (NULL == pNetIf)
    {
        return CLR_E_FAIL;
    }

    fDhcpStarted = (0 != (pNetIf->flags & NETIF_FLAG_DHCP));

#if LWIP_DNS
    // when using DHCP do not use the static settings
    if(0 != (updateFlags & SOCK_NETWORKCONFIGURATION_UPDATE_DNS))
    {
        if(!fDynamicDns && (config->dnsServer1 != 0 || config->dnsServer2 != 0))
        {
            // user defined DNS addresses
            if(config->dnsServer1 != 0)
            {
                u8_t idx = 0;
                
                dns_setserver(idx, (struct ip_addr *)&config->dnsServer1);
            }
            if(config->dnsServer2 != 0)
            {
                u8_t idx = 1;

                dns_setserver(idx, (struct ip_addr *)&config->dnsServer2);
            }
        }
    }
#endif

#if LWIP_DHCP
    if(0 != (updateFlags & SOCK_NETWORKCONFIGURATION_UPDATE_DHCP))
    {
        if(fEnableDhcp)
        {   
            if(!fDhcpStarted)
            {
                if(ERR_OK != dhcp_start(pNetIf))
                {
                    return CLR_E_FAIL;
                }
            }
        }
        else
        {
            if(fDhcpStarted)
            {
                dhcp_stop(pNetIf);
            }

            netif_set_addr(pNetIf, (struct ip_addr *) &config->ipaddr, (struct ip_addr *)&config->subnetmask, (struct ip_addr *)&config->gateway);

            Network_PostEvent( NETWORK_EVENT_TYPE_ADDRESS_CHANGED, 0 );
        }
    }

    if(fEnableDhcp && fDhcpStarted)
    {
        // Try Renew before release since renewing after release will fail
        if(0 != (updateFlags & SOCK_NETWORKCONFIGURATION_UPDATE_DHCP_RENEW))
        {
            //netifapi_netif_common(pNetIf, NULL, dhcp_renew);
            dhcp_renew(pNetIf);
        }
        else if(0 != (updateFlags & SOCK_NETWORKCONFIGURATION_UPDATE_DHCP_RELEASE))
        {
            //netifapi_netif_common(pNetIf, NULL, dhcp_release);
            dhcp_release(pNetIf);
        }
    }
#endif

    if(0 != (updateFlags & SOCK_NETWORKCONFIGURATION_UPDATE_MAC))
    {
        int len = __min(config->macAddressLen, sizeof(pNetIf->hwaddr));
        
        memcpy(pNetIf->hwaddr, config->macAddressBuffer, len);
        pNetIf->hwaddr_len = len;

        // mac address requires stack re-init
        Network_Interface_Close(interfaceIndex);
        g_LWIP_SOCKETS_Driver.m_interfaces[interfaceIndex].m_interfaceNumber = Network_Interface_Open(interfaceIndex);
    }

    return S_OK;

}

int LWIP_SOCKETS_Driver::GetNativeTcpOption (int optname)
{
    NATIVE_PROFILE_PAL_NETWORK();
    int nativeOptionName = 0;

    switch(optname)
    {
        case SOCK_TCP_NODELAY:
            nativeOptionName = TCP_NODELAY;
            break;

        case SOCK_SOCKO_KEEPALIVE:
            nativeOptionName = TCP_KEEPALIVE;
            break;

        // allow the C# user to specify LWIP options that our managed enum
        // doesn't support
        default:
            nativeOptionName = optname;
            break;
    }
    return nativeOptionName;
}

int LWIP_SOCKETS_Driver::GetNativeSockOption (int optname)
{
    NATIVE_PROFILE_PAL_NETWORK();
    int nativeOptionName = 0;

    switch(optname)
    {
        case SOCK_SOCKO_DONTLINGER:
        case SOCK_SOCKO_LINGER:    
            nativeOptionName = SO_LINGER;
            break;
        case SOCK_SOCKO_SENDTIMEOUT:          
            nativeOptionName = SO_SNDTIMEO;
            break;
        case SOCK_SOCKO_RECEIVETIMEOUT:       
            nativeOptionName = SO_RCVTIMEO;
            break;
        case SOCK_SOCKO_EXCLUSIVEADDRESSUSE: 
        case SOCK_SOCKO_REUSEADDRESS:         
            nativeOptionName = SO_REUSEADDR;
            break;
        case SOCK_SOCKO_KEEPALIVE:  
            nativeOptionName = SO_KEEPALIVE;
            break;
        case SOCK_SOCKO_ERROR:                  
            nativeOptionName = SO_ERROR;
            break;
        case SOCK_SOCKO_BROADCAST:              
            nativeOptionName = SO_BROADCAST;
            break;
        case SOCK_SOCKO_RECEIVEBUFFER:
            nativeOptionName =  SO_RCVBUF;
            break;
        case SOCK_SOCKO_SENDBUFFER:
            nativeOptionName = SO_SNDBUF;
            break;
        case SOCK_SOCKO_ACCEPTCONNECTION:
            nativeOptionName = SO_ACCEPTCONN;
            break;
        case SOCK_SOCKO_TYPE:
            nativeOptionName = SO_TYPE;
            break;
            
        case SOCK_SOCKO_USELOOPBACK:
            nativeOptionName = SO_USELOOPBACK;
            break;
        case SOCK_SOCKO_DONTROUTE:  
            nativeOptionName = SO_DONTROUTE;
            break;
        case SOCK_SOCKO_OUTOFBANDINLINE:
            nativeOptionName = SO_OOBINLINE;
            break;

        case SOCK_SOCKO_DEBUG:
            nativeOptionName = SO_DEBUG;
            break;
            
        case SOCK_SOCKO_SENDLOWWATER:
            nativeOptionName = SO_SNDLOWAT;
            break;
            
        case SOCK_SOCKO_RECEIVELOWWATER:
            nativeOptionName = SO_RCVLOWAT;
            break;
            
//        case SOCK_SOCKO_MAXCONNECTIONS:         //don't support
        case SOCK_SOCKO_UPDATE_ACCEPT_CTX:
        case SOCK_SOCKO_UPDATE_CONNECT_CTX:
            nativeOptionName = 0;
            break;
            
        // allow the C# user to specify LWIP options that our managed enum
        // doesn't support
        default:
            nativeOptionName = optname;
            break;
            
    }

    return nativeOptionName;
}

int LWIP_SOCKETS_Driver::GetNativeIPOption (int optname)
{
    NATIVE_PROFILE_PAL_NETWORK();
    int nativeOptionName = 0;

    switch(optname)
    {
        case SOCK_IPO_TTL:           
            nativeOptionName = IP_TTL;
            break;
        case SOCK_IPO_TOS:    
            nativeOptionName = IP_TOS;
            break;
#if LWIP_IGMP
        case SOCK_IPO_MULTICAST_IF:
            nativeOptionName = IP_MULTICAST_IF;
            break;
        case SOCK_IPO_MULTICAST_TTL:  
            nativeOptionName = IP_MULTICAST_TTL;
            break;
        case SOCK_IPO_MULTICAST_LOOP: 
            nativeOptionName = IP_MULTICAST_LOOP;
            break;
        case SOCK_IPO_ADD_MEMBERSHIP:
            nativeOptionName = IP_ADD_MEMBERSHIP;
            break;
        case SOCK_IPO_DROP_MEMBERSHIP:
            nativeOptionName = IP_DROP_MEMBERSHIP;
            break;
#else
        case SOCK_IPO_MULTICAST_IF:
        case SOCK_IPO_MULTICAST_TTL:  
        case SOCK_IPO_MULTICAST_LOOP: 
        case SOCK_IPO_ADD_MEMBERSHIP:
        case SOCK_IPO_DROP_MEMBERSHIP:
#endif
        case SOCK_IPO_ADD_SOURCE_MEMBERSHIP:
        case SOCK_IPO_DROP_SOURCE_MEMBERSHIP:
        case SOCK_IPO_OPTIONS:
        case SOCK_IPO_HDRINCL:
        case SOCK_IPO_IP_DONTFRAGMENT:
        case SOCK_IPO_BLOCK_SOURCE:
        case SOCK_IPO_UBLOCK_SOURCE:
        case SOCK_IPO_PACKET_INFO: 
            nativeOptionName = 0;
            break;

        // allow the C# user to specify LWIP options that our managed enum
        // doesn't support
        default:
            nativeOptionName = optname;
            break;
    }
    
    return nativeOptionName;
}   

int LWIP_SOCKETS_Driver::GetNativeError ( int error )
{
    NATIVE_PROFILE_PAL_NETWORK();
    int ret;

    switch(error)
    {
        case EINTR:
            ret = SOCK_EINTR;
            break;

        case EACCES:
            ret = SOCK_EACCES;
            break;

        case EFAULT:
            ret = SOCK_EFAULT;
            break;

        case EINVAL:
            ret = SOCK_EINVAL;
            break;

        case EMFILE:
            ret = SOCK_EMFILE;
            break;

        case EAGAIN:
        case EBUSY:
        /* case EWOULDBLOCK: same as EINPROGRESS */ 
        case EINPROGRESS:
            ret = SOCK_EWOULDBLOCK;
            break;

        case EALREADY:
            ret = SOCK_EALREADY;
            break;

        case ENOTSOCK:
            ret = SOCK_ENOTSOCK;
            break;

        case EDESTADDRREQ:
            ret = SOCK_EDESTADDRREQ;
            break;

        case EMSGSIZE:
            ret = SOCK_EMSGSIZE;
            break;

        case EPROTOTYPE:
            ret = SOCK_EPROTOTYPE;
            break;

        case ENOPROTOOPT:
            ret = SOCK_ENOPROTOOPT;
            break;

        case EPROTONOSUPPORT:
            ret = SOCK_EPROTONOSUPPORT;
            break;

        case ESOCKTNOSUPPORT:
            ret = SOCK_ESOCKTNOSUPPORT;
            break;

        case EPFNOSUPPORT:
            ret = SOCK_EPFNOSUPPORT;
            break;

        case EAFNOSUPPORT:
            ret = SOCK_EAFNOSUPPORT;
            break;

        case EADDRINUSE:
            ret = SOCK_EADDRINUSE;
            break;

        case EADDRNOTAVAIL:
            ret = SOCK_EADDRNOTAVAIL;
            break;

        case ENETDOWN:
            ret = SOCK_ENETDOWN;
            break;

        case ENETUNREACH:
            ret = SOCK_ENETUNREACH;
            break;

        case ENETRESET:
            ret = SOCK_ENETRESET;
            break;

        case ECONNABORTED:
            ret = SOCK_ECONNABORTED;
            break;

        case ECONNRESET:
            ret = SOCK_ECONNRESET;
            break;

        case ENOBUFS:
        case ENOMEM:
            ret = SOCK_ENOBUFS;
            break;

        case EISCONN:
            ret = SOCK_EISCONN;
            break;

        case ENOTCONN:
            ret = SOCK_EISCONN;
            break;

#if !defined(__GNUC__) // same as ENOTSOCK for GCC
        case ESHUTDOWN:
            ret = SOCK_ESHUTDOWN;
            break;
#endif

        case ETIMEDOUT:
            ret = SOCK_ETIMEDOUT;
            break;

        case ECONNREFUSED:
            ret = SOCK_ECONNREFUSED;
            break;

        case EHOSTDOWN:
            ret = SOCK_EHOSTDOWN;
            break;

        case EHOSTUNREACH:
            ret = SOCK_EHOSTUNREACH;
            break;

        case ENODATA:
            ret = SOCK_NO_DATA;
            break;

        default:
            ret = error;
            break;
    } 
    
    return (ret);   
}

/**
 * Find a network interface by searching for its number
 * Similar to LWIP's netif_find(char *name)
 */
struct netif *netif_find_interface(int num)
{
    struct netif *pNetIf;

    for (pNetIf = netif_list; pNetIf != NULL; pNetIf = pNetIf->next)
    {
        if (num == pNetIf->num)
        {
            return pNetIf;
        }
    }
    return NULL;
}

