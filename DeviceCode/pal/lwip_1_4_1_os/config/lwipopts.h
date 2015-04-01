////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#ifndef __LWIPOPTS_H__
#define __LWIPOPTS_H__

#if DEBUG || _DEBUG
#define LWIP_DEBUG 1
#define LWIP_DBG_TYPES_ON ( LWIP_DBG_TRACE | LWIP_DBG_STATE )
#define LWIP_DBG_MIN_LEVEL LWIP_DBG_LEVEL_ALL
#define DHCP_DEBUG LWIP_DBG_ON
#define TIMERS_DEBUG LWIP_DBG_ON
#endif

#define ERRNO                           1

#define NO_SYS                          0
#define MEM_SIZE                        (8*1024)
#define MEM_ALIGNMENT                   4
#define MEMP_NUM_PBUF                   16
#define MEMP_NUM_UDP_PCB                6
#define MEMP_NUM_TCP_PCB                5
#define MEMP_NUM_TCP_PCB_LISTEN         5
#define MEMP_NUM_TCP_SEG                32
#define MEMP_NUM_SYS_TIMEOUT            8
#define MEMP_NUM_NETBUF                 8
#define MEMP_NUM_NETCONN                10
#define PBUF_POOL_SIZE                  12
#define PBUF_POOL_BUFSIZE               256
#define TCP_MSS                         128
#define TCP_SND_BUF                     (2*TCP_MSS)
#define TCP_SND_QUEUELEN                (4*TCP_SND_BUF/TCP_MSS)
#define TCP_WND                         1024
#define TCP_SNDLOWAT                    (TCP_SND_BUF/2)

#define TCPIP_LWIP                      1
#define LWIP_NETIF_API                  1
#define LWIP_DHCP                       1
#define LWIP_TCP                        1
#define LWIP_UDP                        1
#define LWIP_DNS                        1
#define LWIP_ICMP                       1
#define LWIP_IGMP                       1
#define LWIP_ARP                        1
#define LWIP_AUTOIP                     0
#define LWIP_SNMP                       0

#define LWIP_NETIF_LOOPBACK             1
#define LWIP_HAVE_LOOPIF                1

#define LWIP_NETIF_LINK_CALLBACK		1
#define LWIP_NETIF_STATUS_CALLBACK		1

#ifdef DEBUG
#define DHCP_CREATE_RAND_XID        0
#endif

// Keepalive values, compliant with RFC 1122. Don't change this unless you know what you're doing
#define TCP_KEEPIDLE_DEFAULT        10000UL // Default KEEPALIVE timer in milliseconds
#define TCP_KEEPINTVL_DEFAULT       2000UL  // Default Time between KEEPALIVE probes in milliseconds
#define TCP_KEEPCNT_DEFAULT         9U      // Default Counter for KEEPALIVE probes

#endif /* __LWIPOPTS_H__ */
