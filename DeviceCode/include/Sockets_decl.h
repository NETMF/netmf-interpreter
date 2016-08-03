////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_SOCKETS_DECL_H_
#define _DRIVERS_SOCKETS_DECL_H_ 1

#include <platform_selector.h>

//--//

#if defined(__ADSPBLACKFIN__) || defined(__GNUC__) || defined(__RENESAS__)
#define __int64 long long
#endif

typedef struct _DATE_TIME_INFO
{
    unsigned int  year;           /* year, AD                   */
    unsigned int  month;          /* 1 = January, 12 = December */
    unsigned int  day;            /* 1 = first of the month     */
    unsigned int  hour;           /* 0 = midnight, 12 = noon    */
    unsigned int  minute;         /* minutes past the hour      */
    unsigned int  second;         /* seconds in minute          */
    unsigned int  msec;           /* milliseconds in second     */

    /* These two fields help  */
    /* interpret the absolute */
    /* time meaning of the    */
    /* above values.          */
    unsigned int  dlsTime;           /* boolean; daylight savings time is in effect                      */
    int           tzOffset;              /* signed int; difference in seconds imposed by timezone (from GMT) */
} DATE_TIME_INFO;

typedef struct _X509CertData
{
    char           Issuer[256];
    char           Subject[256];
    DATE_TIME_INFO EffectiveDate;
    DATE_TIME_INFO ExpirationDate;   
} X509CertData;


//Avoid including windows socket definitions

#ifndef TINYCLR_SOCK_STRUCTURES
#define TINYCLR_SOCK_STRUCTURES

#define SOCK_EINTR                   10004L
#define SOCK_EBADF                   10009L
#define SOCK_EACCES                  10013L
#define SOCK_EFAULT                  10014L
#define SOCK_EINVAL                  10022L
#define SOCK_EMFILE                  10024L
#define SOCK_EWOULDBLOCK             10035L /*WSAEWOULDBLOCK*/
#define SOCK_EINPROGRESS             10036L /*WSAEINPROGRESS*/
#define SOCK_EALREADY                10037L
#define SOCK_ENOTSOCK                10038L
#define SOCK_EDESTADDRREQ            10039L
#define SOCK_EMSGSIZE                10040L
#define SOCK_EPROTOTYPE              10041L
#define SOCK_ENOPROTOOPT             10042L
#define SOCK_EPROTONOSUPPORT         10043L
#define SOCK_ESOCKTNOSUPPORT         10044L
#define SOCK_EOPNOTSUPP              10045L
#define SOCK_EPFNOSUPPORT            10046L
#define SOCK_EAFNOSUPPORT            10047L
#define SOCK_EADDRINUSE              10048L
#define SOCK_EADDRNOTAVAIL           10049L
#define SOCK_ENETDOWN                10050L
#define SOCK_ENETUNREACH             10051L
#define SOCK_ENETRESET               10051L
#define SOCK_ECONNABORTED            10053L
#define SOCK_ECONNRESET              10054L
#define SOCK_ENOBUFS                 10055L
#define SOCK_EISCONN                 10056L
#define SOCK_ENOTCONN                10057L
#define SOCK_ESHUTDOWN               10058L
#define SOCK_ETOOMANYREFS            10059L
#define SOCK_ETIMEDOUT               10060L
#define SOCK_ECONNREFUSED            10061L
#define SOCK_ELOOP                   10062L
#define SOCK_ENAMETOOLONG            10063L
#define SOCK_EHOSTDOWN               10064L
#define SOCK_EHOSTUNREACH            10065L
#define SOCK_ENOTEMPTY               10066L
#define SOCK_EPROCLIM                10067L
#define SOCK_EUSERS                  10068L
#define SOCK_EDQUOT                  10069L
#define SOCK_ESTALE                  10070L
#define SOCK_EREMOTE                 10071L
#define SOCK_SYSNOTREADY             10091L
#define SOCK_VERNOTSUPPORTED         10092L
#define SOCK_NOTINITIALISED          10093L
#define SOCK_EDISCON                 10101L
#define SOCK_ENOMORE                 10102L
#define SOCK_ECANCELLED              10103L
#define SOCK_EINVALIDPROCTABLE       10104L
#define SOCK_EINVALIDPROVIDER        10105L
#define SOCK_EPROVIDERFAILEDINIT     10106L
#define SOCK_SYSCALLFAILURE          10107L
#define SOCK_SERVICE_NOT_FOUND       10108L
#define SOCK_TYPE_NOT_FOUND          10109L
#define SOCK__E_NO_MORE              10110L
#define SOCK__E_CANCELLED            10111L
#define SOCK_EREFUSED                10112L
#define SOCK_HOST_NOT_FOUND          11001L
#define SOCK_TRY_AGAIN               11002L
#define SOCK_NO_RECOVERY             11003L
#define SOCK_NO_DATA                 11004L
//
typedef unsigned char   u_char;
typedef unsigned short  u_short;
typedef unsigned int    u_int;
typedef unsigned long   u_long;
typedef unsigned __int64 u_int64;
//
//
#define SOCK_AF_UNSPEC       0               /* unspecified */
#define SOCK_AF_INET         2               /* internetwork: UDP, TCP, etc. */
#define SOCK_AF_INET6       28

typedef int SOCK_SOCKET;

#define SOCK_INADDR_ANY         0x00000000UL
#define SOCK_INADDR_LOOPBACK    0x7F000001UL


#define SOCK_SOCK_STREAM     1
#define SOCK_SOCK_DGRAM      2
#define SOCK_SOCK_RAW        3       
#define SOCK_SOCK_RDM        4       
#define SOCK_SOCK_SEQPACKET  5       
#define SOCK_SOCK_PACK_EX    6       

#define SOCK_TCP_NODELAY  0x0001

#define SOCK_IPPROTO_IP                                 0
#define SOCK_IPPROTO_ICMP                               1
#define SOCK_IPPROTO_IGMP                               2
#define SOCK_IPPROTO_IPV4                               4 /* IP-in-IP encapsulation */
#define SOCK_IPPROTO_TCP                                6
#define SOCK_IPPROTO_PUP                               12
#define SOCK_IPPROTO_UDP                               17
#define SOCK_IPPROTO_IDP                               22
#define SOCK_IPPROTO_IPV6                              41
#define SOCK_IPPROTO_IPv6RoutingHeader                 43
#define SOCK_IPPROTO_IPv6FragmentHeader                44
#define SOCK_IPPROTO_RDP                               46
#define SOCK_IPPROTO_GRE                               47
#define SOCK_IPPROTO_IPSecEncapsulatingSecurityPayload 50
#define SOCK_IPPROTO_IPSecAuthenticationHeader         51
#define SOCK_IPPROTO_IcmpV6                            58
#define SOCK_IPPROTO_IPv6NoNextHeader                  59
#define SOCK_IPPROTO_IPv6DestinationOptions            60
#define SOCK_IPPROTO_ND                                77
#define SOCK_IPPROTO_OSPF                              89
#define SOCK_IPPROTO_TPACKET                          127
#define SOCK_IPPROTO_RAW                              255
#define SOCK_IPPROTO_IPX                             1000
#define SOCK_IPPROTO_SPX                             1256
#define SOCK_IPPROTO_SPXII                           1257
#define SOCK_SOL_SOCKET                            0xFFFF

/* Option flags per-socket */
#define SOCK_SOCKO_DEBUG                 0x0001           // turn on debugging info recording
#define SOCK_SOCKO_NOCHECKSUM            0x0001
#define SOCK_SOCKO_ACCEPTCONNECTION      0x0002           // socket has had listen()
#define SOCK_SOCKO_REUSEADDRESS          0x0004           // allow local address reuse
#define SOCK_SOCKO_KEEPALIVE             0x0008           // keep connections alive
#define SOCK_SOCKO_DONTROUTE             0x0010           // just use interface addresses
#define SOCK_SOCKO_BROADCAST             0x0020           // permit sending of broadcast msgs
#define SOCK_SOCKO_USELOOPBACK           0x0040           // bypass hardware when possible
#define SOCK_SOCKO_LINGER                0x0080           // linger on close if data present
#define SOCK_SOCKO_OUTOFBANDINLINE       0x0100           // leave received OOB data in line
#define SOCK_SOCKO_DONTLINGER            ~SOCK_SOCKO_LINGER
#define SOCK_SOCKO_EXCLUSIVEADDRESSUSE   ~SOCK_SOCKO_REUSEADDRESS    // disallow local address reuse
#define SOCK_SOCKO_SENDBUFFER            0x1001           // send buffer size
#define SOCK_SOCKO_SNDBUF                SOCK_SOCKO_SENDBUFFER
#define SOCK_SOCKO_RECEIVEBUFFER         0x1002           // receive buffer size
#define SOCK_SOCKO_RCVBUF                SOCK_SOCKO_RECEIVEBUFFER
#define SOCK_SOCKO_SENDLOWWATER          0x1003           // send low-water mark
#define SOCK_SOCKO_RECEIVELOWWATER       0x1004           // receive low-water mark
#define SOCK_SOCKO_SENDTIMEOUT           0x1005           // send timeout
#define SOCK_SOCKO_RECEIVETIMEOUT        0x1006           // receive timeout
#define SOCK_SOCKO_ERROR                 0x1007           // get error status and clear
#define SOCK_SOCKO_TYPE                  0x1008           // get socket type
#define SOCK_SOCKO_UPDATE_ACCEPT_CTX     0x700B           // This option updates the properties of the socket which are inherited from the listening socket.
#define SOCK_SOCKO_UPDATE_CONNECT_CTX    0x7010           // This option updates the properties of the socket after the connection is established.
#define SOCK_SOCKO_MAXCONNECTIONS        0x7FFFFFFF       // Maximum queue length specifiable by listen.

/* Option flags per-IP  */
#define SOCK_IPO_OPTIONS                0x0001
#define SOCK_IPO_HDRINCL                0x0002
#define SOCK_IPO_TOS                    0x0003
#define SOCK_IPO_TTL                    0x0004
#define SOCK_IPO_MULTICAST_IF           0x0009
#define SOCK_IPO_MULTICAST_TTL          0x000A
#define SOCK_IPO_MULTICAST_LOOP         0x000B
#define SOCK_IPO_ADD_MEMBERSHIP         0x000C
#define SOCK_IPO_DROP_MEMBERSHIP        0x000D
#define SOCK_IPO_IP_DONTFRAGMENT        0x000E
#define SOCK_IPO_ADD_SOURCE_MEMBERSHIP  0x000F
#define SOCK_IPO_DROP_SOURCE_MEMBERSHIP 0x0010
#define SOCK_IPO_BLOCK_SOURCE           0x0011
#define SOCK_IPO_UBLOCK_SOURCE          0x0012
#define SOCK_IPO_PACKET_INFO            0x0013


#define SOCK_SOCKET_ERROR           -1
#define SOCK_WSATRY_AGAIN           11002
#define SOCK_WSAEINVAL              10022
#define SOCK_WSANO_RECOVERY         11003
#define SOCK_WSAEAFNOSUPPORT        10047
#define SOCK_WSA_NOT_ENOUGH_MEMORY  -1
#define SOCK_WSAHOST_NOT_FOUND      11001
#define SOCK_WSATYPE_NOT_FOUND      10109
#define SOCK_WSAESOCKTNOSUPPORT     10044
//
#define SOCK_IOCPARM_MASK    0x7f            /* parameters must be < 128 bytes */
#define SOCK_IOC_VOID        0x20000000      /* no parameters */
#define SOCK_IOC_OUT         0x40000000      /* copy out parameters */
#define SOCK_IOC_IN          0x80000000      /* copy in parameters */
#define SOCK_IOC_INOUT       (SOCK_IOC_IN|SOCK_IOC_OUT)
                                        /* 0x20000000 distinguishes new &
                                           old ioctl's */
#define SOCK__IO(x,y)        (SOCK_IOC_VOID|((x)<<8)|(y))

#define SOCK__IOR(x,y,t)     (SOCK_IOC_OUT|(((long)sizeof(t)&SOCK_IOCPARM_MASK)<<16)|((x)<<8)|(y))

#define SOCK__IOW(x,y,t)     (SOCK_IOC_IN|(((long)sizeof(t)&SOCK_IOCPARM_MASK)<<16)|((x)<<8)|(y))
//
#define SOCK_FIONREAD    SOCK__IOR('f', 127, u_long) /* get # bytes to read */
#define SOCK_FIONBIO     SOCK__IOW('f', 126, u_long) /* set/clear non-blocking i/o */
#define SOCK_FIOASYNC    SOCK__IOW('f', 125, u_long) /* set/clear async i/o */
//

#define SOCK_FD_SETSIZE    256
#define SOCK_MAX_ADDR_SIZE 14

typedef struct SOCK_fd_set {  
    unsigned int fd_count;  
    int fd_array[SOCK_FD_SETSIZE];
}SOCK_fd_set;

typedef struct SOCK_sockaddr {
    unsigned short sa_family;                /* address family */
    char    sa_data[SOCK_MAX_ADDR_SIZE];     /* up to SOCK_MAX_ADDR_SIZE bytes of direct address */
}SOCK_sockaddr;

CT_ASSERT_UNIQUE_NAME(sizeof(SOCK_sockaddr)==(16), SOCK_SOCKADDR)

typedef ADS_PACKED struct GNU_PACKED SOCK_in_addr{  
    ADS_PACKED union GNU_PACKED {    
        ADS_PACKED struct GNU_PACKED {      
            u_char s_b1,s_b2,s_b3,s_b4;    
        } S_un_b;    
        
        ADS_PACKED struct GNU_PACKED {      
            u_short s_w1,s_w2;    
        } S_un_w;    
        
        u_long S_addr;  
    } S_un;
} SOCK_in_addr;

typedef ADS_PACKED struct GNU_PACKED SOCK_sockaddr_in {
        short   sin_family;
        u_short sin_port;
        SOCK_in_addr sin_addr;
        char    sin_zero[8];
} SOCK_sockaddr_in;

CT_ASSERT_UNIQUE_NAME(sizeof(SOCK_sockaddr_in)==16, SOCK_SOCKADDR_IN)

typedef struct SOCK_addrinfo 
{  
    int ai_flags;  
    int ai_family;  
    int ai_socktype;  
    int ai_protocol;  
    size_t ai_addrlen;  
    char* ai_canonname;  
    struct SOCK_sockaddr* ai_addr;  
    struct SOCK_addrinfo* ai_next;
}SOCK_addrinfo;

typedef struct SOCK_ip_mreq
{
    SOCK_in_addr imr_multiaddr; /* IPv4 Class D multicast address */
    SOCK_in_addr imr_interface; /* IPv4 address of local interface */
} SOCK_ip_mreq;

typedef struct SOCK_discoveryinfo 
{
    UINT32 ipaddr;
    UINT32 macAddressLen;
    char   macAddressBuffer[64];    
} SOCK_discoveryinfo;

#define SOCK_MAKE_IP_ADDR(w,x,y,z)           ((UINT32)(w) & 0xFF)<<24 | ((UINT32)(x) & 0xff)<<16 | ((UINT32)(y) & 0xff)<<8 | ((UINT32)(z) & 0xff)
#define SOCK_MAKE_IP_ADDR_LITTLEEND(w,x,y,z) ((UINT32)(z) & 0xFF)<<24 | ((UINT32)(y) & 0xff)<<16 | ((UINT32)(x) & 0xff)<<8 | ((UINT32)(w) & 0xff)

#ifndef SOCKET_READ_PEEK_OPTION
#define SOCKET_READ_PEEK_OPTION             2
#endif 

#ifndef DEBUG_SOCKET_PORT
#define DEBUG_SOCKET_PORT                   26000
#endif

#ifndef SOCK_DISCOVERY_MULTICAST_PORT
#define SOCK_DISCOVERY_MULTICAST_PORT       26001
#endif 

//A multicast address is an IP address which belongs to classe D included in the range between 224.0.0.0 and 239.255.255.255
#ifndef SOCK_DISCOVERY_MULTICAST_IPADDR
#define SOCK_DISCOVERY_MULTICAST_IPADDR     SOCK_MAKE_IP_ADDR(234,102,98,44)
#endif

#ifndef SOCK_DISCOVERY_MULTICAST_IPADDR_SND
#define SOCK_DISCOVERY_MULTICAST_IPADDR_SND SOCK_MAKE_IP_ADDR(234,102,98,45)
#endif

#ifndef SOCK_DISCOVERY_MULTICAST_TOKEN
#define SOCK_DISCOVERY_MULTICAST_TOKEN      "DOTNETMF"
#endif

#define SOCKET_EVENT_WAIT_FOREVER          0xFFFFFFFF
#define SOCKET_EVENT_FLAG_SOCKETS_SHUTDOWN 0x00000001
#define SOCKET_EVENT_FLAG_SOCKETS_READY    0x00000002
#define SOCKET_EVENT_FLAG_SOCKET           0x00000004

/*
 * Structure used in select() call, taken from the BSD file sys/time.h.
 */
typedef struct SOCK_timeval {
        long    tv_sec;         /* seconds */
        long    tv_usec;        /* and microseconds */
}SOCK_timeval;


#define SOCK_NETWORKCONFIGURATION_FLAGS_DHCP                0x00000001
#define SOCK_NETWORKCONFIGURATION_FLAGS_DYNAMIC_DNS         0x00000002

/// Bits 19-16 of SOCK_NetworkConfiguration.flags: Type of SOCK_NetworkConfiguration
/// 0 - NetworkInterface
/// 1 - Wireless
#define SOCK_NETWORKCONFIGURATION_FLAGS_NETWORK_INTERFACE   0
#define SOCK_NETWORKCONFIGURATION_FLAGS_WIRELESS            1
#define SOCK_NETWORKCONFIGURATION_FLAGS_TYPE__mask          0x000F0000
#define SOCK_NETWORKCONFIGURATION_FLAGS_TYPE__shift         16
#define SOCK_NETWORKCONFIGURATION_FLAGS_TYPE__value(x)      (((x) & SOCK_NETWORKCONFIGURATION_FLAGS_TYPE__mask) >> SOCK_NETWORKCONFIGURATION_FLAGS_TYPE__shift)
#define SOCK_NETWORKCONFIGURATION_FLAGS_TYPE__set(x)        (((x) << SOCK_NETWORKCONFIGURATION_FLAGS_TYPE__shift) & SOCK_NETWORKCONFIGURATION_FLAGS_TYPE__mask)
#define SOCK_NETWORKCONFIGURATION_FLAGS_SUBINDEX__mask      0x00F00000
#define SOCK_NETWORKCONFIGURATION_FLAGS_SUBINDEX__shift     20
#define SOCK_NETWORKCONFIGURATION_FLAGS_SUBINDEX__value(x)  (((x) & SOCK_NETWORKCONFIGURATION_FLAGS_SUBINDEX__mask) >> SOCK_NETWORKCONFIGURATION_FLAGS_SUBINDEX__shift)
#define SOCK_NETWORKCONFIGURATION_FLAGS_SUBINDEX__set(x)    (((x) << SOCK_NETWORKCONFIGURATION_FLAGS_SUBINDEX__shift) & SOCK_NETWORKCONFIGURATION_FLAGS_SUBINDEX__mask)

#define SOCK_NETWORKCONFIGURATION_UPDATE_DNS                0x00000001
#define SOCK_NETWORKCONFIGURATION_UPDATE_DHCP               0x00000002
#define SOCK_NETWORKCONFIGURATION_UPDATE_DHCP_RENEW         0x00000004
#define SOCK_NETWORKCONFIGURATION_UPDATE_DHCP_RELEASE       0x00000008
#define SOCK_NETWORKCONFIGURATION_UPDATE_MAC                0x00000010

#define SOCK_NETWORKCONFIGURATION_INTERFACETYPE_UNKNOWN        0
#define SOCK_NETWORKCONFIGURATION_INTERFACETYPE_ETHERNET       6
#define SOCK_NETWORKCONFIGURATION_INTERFACETYPE_WIRELESS_80211 71


//--//

///
/// Keep these values in sync with the manged code enumeration Wireless80211.AuthenticationType in wireless.cs
///
#define WIRELESS_FLAG_AUTHENTICATION_NONE      0
#define WIRELESS_FLAG_AUTHENTICATION_EAP       1
#define WIRELESS_FLAG_AUTHENTICATION_PEAP      2
#define WIRELESS_FLAG_AUTHENTICATION_WCN       3
#define WIRELESS_FLAG_AUTHENTICATION_OPEN      4
#define WIRELESS_FLAG_AUTHENTICATION_SHARED    5
#define WIRELESS_FLAG_AUTHENTICATION__shift    0
#define WIRELESS_FLAG_AUTHENTICATION__mask     0x0000000F
#define WIRELESS_FLAG_AUTHENTICATION__value(x) (((x) & WIRELESS_FLAG_AUTHENTICATION__mask) >> WIRELESS_FLAG_AUTHENTICATION__shift)
#define WIRELESS_FLAG_AUTHENTICATION__set(x)   (((x) << WIRELESS_FLAG_AUTHENTICATION__shift) & WIRELESS_FLAG_AUTHENTICATION__mask)

///
/// Keep these values in sync with the manged code enumeration Wireless80211.EncryptionType in wireless.cs
///
#define WIRELESS_FLAG_ENCRYPTION_NONE          0
#define WIRELESS_FLAG_ENCRYPTION_WEP           1
#define WIRELESS_FLAG_ENCRYPTION_WPA           2
#define WIRELESS_FLAG_ENCRYPTION_WPAPSK        3
#define WIRELESS_FLAG_ENCRYPTION_Certificate   4
#define WIRELESS_FLAG_ENCRYPTION__shift        4
#define WIRELESS_FLAG_ENCRYPTION__mask         0x000000F0
#define WIRELESS_FLAG_ENCRYPTION__value(x)     (((x) & WIRELESS_FLAG_ENCRYPTION__mask) >> WIRELESS_FLAG_ENCRYPTION__shift)
#define WIRELESS_FLAG_ENCRYPTION__set(x)       (((x) << WIRELESS_FLAG_ENCRYPTION__shift) & WIRELESS_FLAG_ENCRYPTION__mask)
    
///
/// Keep these values in sync with the manged code enumeration Wireless80211.RadioType in wireless.cs
///
#define WIRELESS_FLAG_RADIO_a                  1
#define WIRELESS_FLAG_RADIO_b                  2
#define WIRELESS_FLAG_RADIO_g                  4
#define WIRELESS_FLAG_RADIO_n                  8
#define WIRELESS_FLAG_RADIO__shift             8
#define WIRELESS_FLAG_RADIO__mask              0x00000F00
#define WIRELESS_FLAG_RADIO__value(x)          (((x) & WIRELESS_FLAG_RADIO__mask) >> WIRELESS_FLAG_RADIO__shift)
#define WIRELESS_FLAG_RADIO__set(x)            (((x) << WIRELESS_FLAG_RADIO__shift) & WIRELESS_FLAG_RADIO__mask)


#define WIRELESS_FLAG_DATA_ENCRYPTED           1
#define WIRELESS_FLAG_DATA__shift              12
#define WIRELESS_FLAG_DATA__mask               0x0000F000
#define WIRELESS_FLAG_DATA__value(x)           (((x) & WIRELESS_FLAG_DATA__mask) >> WIRELESS_FLAG_DATA__shift)
#define WIRELESS_FLAG_DATA__set(x)             (((x) << WIRELESS_FLAG_DATA__shift) & WIRELESS_FLAG_DATA__mask)
    
extern const ConfigurationSector g_ConfigurationSector;

//--//

#if defined(NETMF_TARGET_LITTLE_ENDIAN)
#define SOCK_htons(x) ( (((x) & 0x000000FFUL) <<  8) | (((x) & 0x0000FF00UL) >>  8) )
#define SOCK_htonl(x) ( (((x) & 0x000000FFUL) << 24) | (((x) & 0x0000FF00UL) << 8) | (((x) & 0x00FF0000UL) >> 8) | (((x) & 0xFF000000UL) >> 24) )
#define SOCK_ntohs(x) SOCK_htons(x)
#else
#define SOCK_htons(x) ( x )
#define SOCK_htonl(x) ( x )
#define SOCK_ntohs(x) ((UINT16)(x))
#endif

#define SOCK_FD_ZERO(x)     memset(x, 0, sizeof(*x))
__inline BOOL SOCK_FD_ISSET(int y, SOCK_fd_set* x)        
{
    GLOBAL_LOCK_SOCKETS(irq);

    for(int i=0; i<(int)x->fd_count; i++)
    {                               
        if(x->fd_array[i] == y)     
        {                           
            return TRUE;            
        }                           
    }                               
    return FALSE;                   
}                                   

__inline void SOCK_FD_SET(int y, SOCK_fd_set* x)     
{
    if(SOCK_FD_ISSET(y, x)) return; 
                                    
    GLOBAL_LOCK_SOCKETS(irq);

    if(x->fd_count < SOCK_FD_SETSIZE)
    {                               
        x->fd_array[x->fd_count++] = y; 
    }
    else
    {
        ASSERT(FALSE);              
    }
}                                   
    
__inline void SOCK_FD_CLR(int y, SOCK_fd_set* x)           
{
    GLOBAL_LOCK_SOCKETS(irq);

    for(int i=0; i<(int)x->fd_count; i++)
    {                               
        if(x->fd_array[i] == y)     
        {                           
            x->fd_count--;          

            if(i < (int)x->fd_count) 
            {                       
                x->fd_array[i] = x->fd_array[x->fd_count];
            }                       
            x->fd_array[x->fd_count] = 0;
            break;                  
        }                           
    }                               
}                                

#endif //TINYCLR_SOCK_STRUCTURES

#ifndef DEBUG_SOCKET_PORT  // default debug socket port
#define DEBUG_SOCKET_PORT 26000
#endif


struct SOCK_NetworkConfiguration;
struct SOCK_WirelessConfiguration;

//--//

BOOL  Network_Initialize();
BOOL  Network_Uninitialize();

BOOL Network_Interface_Bind(int index);
int  Network_Interface_Open(int index);
BOOL Network_Interface_Close(int index);

//--//

#define EVENT_NETWORK                            4

#define NETWORK_EVENT_TYPE__AVAILABILITY_CHANGED 1
#define NETWORK_EVENT_TYPE_ADDRESS_CHANGED       2

#define NETWORK_EVENT_FLAGS_IS_AVAILABLE         1

void Network_PostEvent(unsigned int eventType, unsigned int flags);

//--//

BOOL  SOCKETS_Initialize( int ComPortNum );
BOOL  SOCKETS_Uninitialize( int ComPortNum );
int   SOCKETS_Write( int ComPortNum, const char* Data, size_t size );
int   SOCKETS_Read( int ComPortNum, char* Data, size_t size );
BOOL  SOCKETS_Flush( int ComPortNum );
BOOL  SOCKETS_UpgradeToSsl( int ComPortNum, const UINT8* pCACert, UINT32 caCertLen, const UINT8* pDeviceCert, UINT32 deviceCertLen, LPCSTR szTargetHost );
BOOL  SOCKETS_IsUsingSsl( INT32 ComPortNum );


void  SOCKETS_CloseConnections();
// RTIP
BOOL  SOCKETS_ProcessSocketActivity(SOCK_SOCKET signalSocket);
//LWIP
void  SOCKETS_CreateTcpIpProcessor(HAL_CALLBACK_FPN callback, void* arg);
void  SOCKETS_RestartTcpIpProcessor(UINT32 timeFromNow_us);

int SOCK_socket( int family, int type, int protocol );
int SOCK_bind( int socket, const struct SOCK_sockaddr* address, int addressLen );
int SOCK_connect(int socket, const struct SOCK_sockaddr* address, int addressLen );
int SOCK_send(int socket, const char* buf, int len, int flags );
int SOCK_recv(int socket, char* buf, int len, int flags );
int SOCK_close(int socket); 
int SOCK_listen( int socket, int backlog );
int SOCK_accept( int socket, struct SOCK_sockaddr* address, int* addressLen );
int SOCK_shutdown( int socket, int how );
int SOCK_getaddrinfo(  const char* nodename, char* servname, const struct SOCK_addrinfo* hints, struct SOCK_addrinfo** res );
void SOCK_freeaddrinfo( struct SOCK_addrinfo* ai );
int SOCK_ioctl( int socket, int cmd, int* data );
int SOCK_getlasterror();
int SOCK_select( int socket, SOCK_fd_set* readfds, SOCK_fd_set* writefds, SOCK_fd_set* except, const struct SOCK_timeval* timeout );
int SOCK_setsockopt( int socket, int level, int optname, const char* optval, int  optlen );
int SOCK_getsockopt( int socket, int level, int optname,       char* optval, int* optlen );
int SOCK_getpeername( int socket, struct SOCK_sockaddr* name, int* namelen );
int SOCK_getsockname( int socket, struct SOCK_sockaddr* name, int* namelen );
int SOCK_recvfrom( int s, char* buf, int len, int flags, struct SOCK_sockaddr* from, int* fromlen );
int SOCK_sendto( int s, const char* buf, int len, int flags, const struct SOCK_sockaddr* to, int tolen );

//network adapter settings

UINT32 SOCK_CONFIGURATION_GetAdapterCount();
HRESULT SOCK_CONFIGURATION_LoadAdapterConfiguration( UINT32 interfaceIndex, SOCK_NetworkConfiguration* config );
HRESULT SOCK_CONFIGURATION_UpdateAdapterConfiguration( UINT32 interfaceIndex, UINT32 updateFlags, SOCK_NetworkConfiguration* config );

HRESULT SOCK_CONFIGURATION_LoadConfiguration( UINT32 interfaceIndex, SOCK_NetworkConfiguration* config );

/// Wireless adapter specific settings.
HRESULT SOCK_CONFIGURATION_LoadWirelessConfiguration( UINT32 interfaceIndex, SOCK_WirelessConfiguration* wirelessConfig );
HRESULT SOCK_CONFIGURATION_UpdateWirelessConfiguration( UINT32 interfaceIndex, SOCK_WirelessConfiguration* wirelessConfig );
HRESULT SOCK_CONFIGURATION_SaveAllWirelessConfigurations( );

//--// SSL 

typedef void (*SSL_DATE_TIME_FUNC)(DATE_TIME_INFO* pdt);

#define SSL_RESULT__WOULD_BLOCK -2

BOOL SSL_Initialize  ();
BOOL SSL_Uninitialize();
BOOL SSL_ServerInit ( int sslMode, int sslVerify, const char* certificate, int cert_len, const char* certPwd, int& sslContextHandle );
BOOL SSL_ClientInit ( int sslMode, int sslVerify, const char* certificate, int cert_len, const char* certPwd, int& sslContextHandle );
BOOL SSL_AddCertificateAuthority( int sslContextHandle, const char* certificate, int cert_len, const char* certPwd );
void SSL_ClearCertificateAuthority( int sslContextHandle );
BOOL SSL_ExitContext( int sslContextHandle );
int  SSL_Accept     ( int socket, int sslContextHandle );
int  SSL_Connect    ( int socket, const char* szTargetHost, int sslContextHandle );
int  SSL_Write      ( int socket, const char* Data, size_t size );
int  SSL_Read       ( int socket, char* Data, size_t size );
int  SSL_CloseSocket( int socket );
void SSL_GetTime(DATE_TIME_INFO* pdt);
void SSL_RegisterTimeCallback(SSL_DATE_TIME_FUNC pfn);
BOOL SSL_ParseCertificate( const char* certificate, size_t certLength, const char* szPwd, X509CertData* certData );
int  SSL_DataAvailable( int socket );

//--//


BOOL HAL_SOCK_Initialize();
BOOL HAL_SOCK_Uninitialize();
void HAL_SOCK_SignalSocketThread();
int HAL_SOCK_socket( int family, int type, int protocol );
int HAL_SOCK_bind( int socket, const struct SOCK_sockaddr* address, int addressLen );
int HAL_SOCK_connect(int socket, const struct SOCK_sockaddr* address, int addressLen );
int HAL_SOCK_send(int socket, const char* buf, int len, int flags );
int HAL_SOCK_recv(int socket, char* buf, int len, int flags );
int HAL_SOCK_close(int socket); 
int HAL_SOCK_listen( int socket, int backlog );
int HAL_SOCK_accept( int socket, struct SOCK_sockaddr* address, int* addressLen );
int HAL_SOCK_shutdown( int socket, int how );
int HAL_SOCK_getaddrinfo(  const char* nodename, char* servname, const struct SOCK_addrinfo* hints, struct SOCK_addrinfo** res );
void HAL_SOCK_freeaddrinfo( struct SOCK_addrinfo* ai );
int HAL_SOCK_ioctl( int socket, int cmd, int* data );
int HAL_SOCK_getlasterror();
int HAL_SOCK_select( int socket, SOCK_fd_set* readfds, SOCK_fd_set* writefds, SOCK_fd_set* except, const struct SOCK_timeval* timeout );
int HAL_SOCK_setsockopt( int socket, int level, int optname, const char* optval, int  optlen );
int HAL_SOCK_getsockopt( int socket, int level, int optname,       char* optval, int* optlen );
int HAL_SOCK_getpeername( int socket, struct SOCK_sockaddr* name, int* namelen );
int HAL_SOCK_getsockname( int socket, struct SOCK_sockaddr* name, int* namelen );
int HAL_SOCK_recvfrom( int s, char* buf, int len, int flags, struct SOCK_sockaddr* from, int* fromlen );
int HAL_SOCK_sendto( int s, const char* buf, int len, int flags, const struct SOCK_sockaddr* to, int tolen );

UINT32 HAL_SOCK_CONFIGURATION_GetAdapterCount();
HRESULT HAL_SOCK_CONFIGURATION_LoadAdapterConfiguration( UINT32 interfaceIndex, SOCK_NetworkConfiguration* config );
HRESULT HAL_SOCK_CONFIGURATION_UpdateAdapterConfiguration( UINT32 interfaceIndex, UINT32 updateFlags, SOCK_NetworkConfiguration* config );

HRESULT HAL_SOCK_CONFIGURATION_LoadWirelessConfiguration( UINT32 interfaceIndex, SOCK_WirelessConfiguration* wirelessConfig );

void* HAL_SOCK_GlobalLockContext();
void  HAL_SOCK_EventsSet(UINT32 events);

//--//

#endif // _DRIVERS_SOCKETS_DECL_H_
