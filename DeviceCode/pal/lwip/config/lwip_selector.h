////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#define NETWORK_MEMORY_PROFILE_LWIP__medium         1
#define TCPIP_LWIP                                  1

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

#ifdef _DEBUG
#define LWIP_DEBUG                      1
#endif
