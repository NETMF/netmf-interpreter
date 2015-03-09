////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_NETWORK_DEFINES_H_
#define _DRIVERS_NETWORK_DEFINES_H_ 1

#define NETWORK_MAX_PACKETSIZE      1514

//--//  The Number of packet buffer pools. Each packet buffer pool
//--//  contains one or more packet buffers of the same size.

#define NETWORK_MEMORY_POOL_SSL_SIZE__min                 ( 64*1024)
#define NETWORK_MEMORY_POOL_SSL_SIZE__default             (128*1024)
#define NETWORK_MEMORY_POOL_SSL_SIZE__max                 (256*1024)

#if defined(NETWORK_MEMORY_POOL__INCLUDE_SSL)
#define NETWORK_MEMORY_POOL_SSL_SIZE 1
#else
#define NETWORK_MEMORY_POOL_SSL_SIZE 0
#endif

#define NETWORK_MEMORY_POOL__SIZE__min                     (( 32*1024) + (NETWORK_MEMORY_POOL_SSL_SIZE * NETWORK_MEMORY_POOL_SSL_SIZE__min))
#define NETWORK_MEMORY_POOL__SIZE__default                 ((128*1024) + (NETWORK_MEMORY_POOL_SSL_SIZE * NETWORK_MEMORY_POOL_SSL_SIZE__default))
#define NETWORK_MEMORY_POOL__SIZE__max                     ((256*1024) + (NETWORK_MEMORY_POOL_SSL_SIZE * NETWORK_MEMORY_POOL_SSL_SIZE__max))

#define NETWORK_PACKET_POOL_0__NUM_PACKETS__min            5
#define NETWORK_PACKET_POOL_0__NUM_PACKETS__default        10
#define NETWORK_PACKET_POOL_0__NUM_PACKETS__max            20
#define NETWORK_PACKET_POOL_0__PACKET_SIZE                 128

#define NETWORK_PACKET_POOL_1__NUM_PACKETS__min            5
#define NETWORK_PACKET_POOL_1__NUM_PACKETS__default        20
#define NETWORK_PACKET_POOL_1__NUM_PACKETS__max            30
#define NETWORK_PACKET_POOL_1__PACKET_SIZE                 256

#define NETWORK_PACKET_POOL_2__NUM_PACKETS__min            5
#define NETWORK_PACKET_POOL_2__NUM_PACKETS__default        10
#define NETWORK_PACKET_POOL_2__NUM_PACKETS__max            30
#define NETWORK_PACKET_POOL_2__PACKET_SIZE                 512

#define NETWORK_PACKET_POOL_3__NUM_PACKETS__min            4
#define NETWORK_PACKET_POOL_3__NUM_PACKETS__default        18
#define NETWORK_PACKET_POOL_3__NUM_PACKETS__max            30
#define NETWORK_PACKET_POOL_3__PACKET_SIZE                 1514

#define NETWORK_PACKET_POOL_4__NUM_PACKETS__min            4
#define NETWORK_PACKET_POOL_4__NUM_PACKETS__default        15
#define NETWORK_PACKET_POOL_4__NUM_PACKETS__max            40
#define NETWORK_PACKET_POOL_4__PACKET_SIZE                 1514

#define NETWORK_PACKET_POOL_5__NUM_PACKETS__min            0
#define NETWORK_PACKET_POOL_5__NUM_PACKETS__default        0
#define NETWORK_PACKET_POOL_5__NUM_PACKETS__max            40
#define NETWORK_PACKET_POOL_5__PACKET_SIZE                 0

//--//

#define NETWORK_NUM_IFACES__min                           1
#define NETWORK_NUM_IFACES__default                       1  // loopback is added if NETWORK_USE_LOOPBACK is defined
#define NETWORK_NUM_IFACES__max                           4

#define NETWORK_NUM_DEVICES__min                          2   // TODO: WHY ISNT THIS IN THE EBSNET SPREADSHEET?
#define NETWORK_NUM_DEVICES__default                      4 
#define NETWORK_NUM_DEVICES__max                          10

#define NETWORK_MULTICAST_LIST_SIZE__min                  2
#define NETWORK_MULTICAST_LIST_SIZE__default              5
#define NETWORK_MULTICAST_LIST_SIZE__max                  20

#define NETWORK_ROUTINGTABLE_SIZE__min                    5
#define NETWORK_ROUTINGTABLE_SIZE__default                10
#define NETWORK_ROUTINGTABLE_SIZE__max                    50

#define NETWORK_ARP_NUM_TABLE_ENTRIES__min                3  
#define NETWORK_ARP_NUM_TABLE_ENTRIES__default            5  //CFG_ARPCLEN
#define NETWORK_ARP_NUM_TABLE_ENTRIES__max                50

#define NETWORK_FRAG_TABLE_SIZE__min                      2
#define NETWORK_FRAG_TABLE_SIZE__default                  6
#define NETWORK_FRAG_TABLE_SIZE__max                      10

#define NETWORK_NAT_NUM_ENTRIES__min                      5
#define NETWORK_NAT_NUM_ENTRIES__default                  25
#define NETWORK_NAT_NUM_ENTRIES__max                      40

#define NETWORK_TCP_NUM_PORTS__SUPPORTED__min             16
#define NETWORK_TCP_NUM_PORTS__SUPPORTED__default         64
#define NETWORK_TCP_NUM_PORTS__SUPPORTED__max             128

#define NETWORK_UDP_NUM_PORTS__SUPPORTED__min             6
#define NETWORK_UDP_NUM_PORTS__SUPPORTED__default         64
#define NETWORK_UDP_NUM_PORTS__SUPPORTED__max             128


//--//  Configurable timeout parameters

#define NETWORK_DHCP_RETRIES__default                     5
#define NETWORK_DHCP_TIMEOUT__default                     8

#define NETWORK_IGMPV1_MAX_DELAY__default                 10

#define NETWORK_ARP_REQ_TIMEOUT__default                  2
#define NETWORK_ARP_MAX_RETRIES__default                  4
#define NETWORK_ARP_RES_TIMEOUT__default                  600

#define NETWORK_DNS_MIN_DELAY__default                    2
#define NETWORK_DNS_MAX_DELAY__default                    2
#define NETWORK_DNS_RETRIES__default                      2


//--// RAM Profiles

#ifdef NETWORK_MEMORY_PROFILE__small
    #define PLATFORM_DEPENDENT__NETWORK_PACKET_POOL_0__NUM_PACKETS   NETWORK_PACKET_POOL_0__NUM_PACKETS__min 
    #define PLATFORM_DEPENDENT__NETWORK_PACKET_POOL_1__NUM_PACKETS   NETWORK_PACKET_POOL_1__NUM_PACKETS__min 
    #define PLATFORM_DEPENDENT__NETWORK_PACKET_POOL_2__NUM_PACKETS   NETWORK_PACKET_POOL_2__NUM_PACKETS__min 
    #define PLATFORM_DEPENDENT__NETWORK_PACKET_POOL_3__NUM_PACKETS   NETWORK_PACKET_POOL_3__NUM_PACKETS__min 
    #define PLATFORM_DEPENDENT__NETWORK_PACKET_POOL_4__NUM_PACKETS   NETWORK_PACKET_POOL_4__NUM_PACKETS__min 
    #define PLATFORM_DEPENDENT__NETWORK_PACKET_POOL_5__NUM_PACKETS   NETWORK_PACKET_POOL_5__NUM_PACKETS__min 
    #define PLATFORM_DEPENDENT__NETWORK_NUM_IFACES                   NETWORK_NUM_IFACES__min                 
    #define PLATFORM_DEPENDENT__NETWORK_NUM_DEVICES                  NETWORK_NUM_DEVICES__min                
    #define PLATFORM_DEPENDENT__NETWORK_MULTICAST_LIST_SIZE          NETWORK_MULTICAST_LIST_SIZE__min        
    #define PLATFORM_DEPENDENT__NETWORK_ROUTINGTABLE_SIZE            NETWORK_ROUTINGTABLE_SIZE__min          
    #define PLATFORM_DEPENDENT__NETWORK_ARP_NUM_TABLE_ENTRIES        NETWORK_ARP_NUM_TABLE_ENTRIES__min      
    #define PLATFORM_DEPENDENT__NETWORK_FRAG_TABLE_SIZE              NETWORK_FRAG_TABLE_SIZE__min            
    #define PLATFORM_DEPENDENT__NETWORK_NAT_NUM_ENTRIES              NETWORK_NAT_NUM_ENTRIES__min            
    #define PLATFORM_DEPENDENT__NETWORK_TCP_NUM_PORTS__SUPPORTED     NETWORK_TCP_NUM_PORTS__SUPPORTED__min   
    #define PLATFORM_DEPENDENT__NETWORK_UDP_NUM_PORTS__SUPPORTED     NETWORK_UDP_NUM_PORTS__SUPPORTED__min   
    #define PLATFORM_DEPENDENT__NETWORK_MEMORY_POOL__SIZE            NETWORK_MEMORY_POOL__SIZE__min
    #define PLATFORM_DEPENDENT__SOCKETS_MAX_COUNT                    (PLATFORM_DEPENDENT__NETWORK_TCP_NUM_PORTS__SUPPORTED + PLATFORM_DEPENDENT__NETWORK_UDP_NUM_PORTS__SUPPORTED)                                     
#endif        

#ifdef NETWORK_MEMORY_PROFILE__medium
    #define PLATFORM_DEPENDENT__NETWORK_PACKET_POOL_0__NUM_PACKETS   NETWORK_PACKET_POOL_0__NUM_PACKETS__default 
    #define PLATFORM_DEPENDENT__NETWORK_PACKET_POOL_1__NUM_PACKETS   NETWORK_PACKET_POOL_1__NUM_PACKETS__default 
    #define PLATFORM_DEPENDENT__NETWORK_PACKET_POOL_2__NUM_PACKETS   NETWORK_PACKET_POOL_2__NUM_PACKETS__default 
    #define PLATFORM_DEPENDENT__NETWORK_PACKET_POOL_3__NUM_PACKETS   NETWORK_PACKET_POOL_3__NUM_PACKETS__default 
    #define PLATFORM_DEPENDENT__NETWORK_PACKET_POOL_4__NUM_PACKETS   NETWORK_PACKET_POOL_4__NUM_PACKETS__default 
    #define PLATFORM_DEPENDENT__NETWORK_PACKET_POOL_5__NUM_PACKETS   NETWORK_PACKET_POOL_5__NUM_PACKETS__default 
    #define PLATFORM_DEPENDENT__NETWORK_NUM_IFACES                   NETWORK_NUM_IFACES__default             
    #define PLATFORM_DEPENDENT__NETWORK_NUM_DEVICES                  NETWORK_NUM_DEVICES__default            
    #define PLATFORM_DEPENDENT__NETWORK_MULTICAST_LIST_SIZE          NETWORK_MULTICAST_LIST_SIZE__default    
    #define PLATFORM_DEPENDENT__NETWORK_ROUTINGTABLE_SIZE            NETWORK_ROUTINGTABLE_SIZE__default      
    #define PLATFORM_DEPENDENT__NETWORK_ARP_NUM_TABLE_ENTRIES        NETWORK_ARP_NUM_TABLE_ENTRIES__default  
    #define PLATFORM_DEPENDENT__NETWORK_FRAG_TABLE_SIZE              NETWORK_FRAG_TABLE_SIZE__default        
    #define PLATFORM_DEPENDENT__NETWORK_NAT_NUM_ENTRIES              NETWORK_NAT_NUM_ENTRIES__default        
    #define PLATFORM_DEPENDENT__NETWORK_TCP_NUM_PORTS__SUPPORTED     NETWORK_TCP_NUM_PORTS__SUPPORTED__default 
    #define PLATFORM_DEPENDENT__NETWORK_UDP_NUM_PORTS__SUPPORTED     NETWORK_UDP_NUM_PORTS__SUPPORTED__default 
    #define PLATFORM_DEPENDENT__NETWORK_MEMORY_POOL__SIZE            NETWORK_MEMORY_POOL__SIZE__default
    #define PLATFORM_DEPENDENT__SOCKETS_MAX_COUNT                    (PLATFORM_DEPENDENT__NETWORK_TCP_NUM_PORTS__SUPPORTED + PLATFORM_DEPENDENT__NETWORK_UDP_NUM_PORTS__SUPPORTED)                                      
#endif

#ifdef NETWORK_MEMORY_PROFILE__large
    #define PLATFORM_DEPENDENT__NETWORK_PACKET_POOL_0__NUM_PACKETS   NETWORK_PACKET_POOL_0__NUM_PACKETS__max 
    #define PLATFORM_DEPENDENT__NETWORK_PACKET_POOL_1__NUM_PACKETS   NETWORK_PACKET_POOL_1__NUM_PACKETS__max 
    #define PLATFORM_DEPENDENT__NETWORK_PACKET_POOL_2__NUM_PACKETS   NETWORK_PACKET_POOL_2__NUM_PACKETS__max 
    #define PLATFORM_DEPENDENT__NETWORK_PACKET_POOL_3__NUM_PACKETS   NETWORK_PACKET_POOL_3__NUM_PACKETS__max 
    #define PLATFORM_DEPENDENT__NETWORK_PACKET_POOL_4__NUM_PACKETS   NETWORK_PACKET_POOL_4__NUM_PACKETS__max 
    #define PLATFORM_DEPENDENT__NETWORK_PACKET_POOL_5__NUM_PACKETS   NETWORK_PACKET_POOL_5__NUM_PACKETS__max 
    #define PLATFORM_DEPENDENT__NETWORK_NUM_IFACES                   NETWORK_NUM_IFACES__max                 
    #define PLATFORM_DEPENDENT__NETWORK_NUM_DEVICES                  NETWORK_NUM_DEVICES__max                
    #define PLATFORM_DEPENDENT__NETWORK_MULTICAST_LIST_SIZE          NETWORK_MULTICAST_LIST_SIZE__max        
    #define PLATFORM_DEPENDENT__NETWORK_ROUTINGTABLE_SIZE            NETWORK_ROUTINGTABLE_SIZE__max          
    #define PLATFORM_DEPENDENT__NETWORK_ARP_NUM_TABLE_ENTRIES        NETWORK_ARP_NUM_TABLE_ENTRIES__max      
    #define PLATFORM_DEPENDENT__NETWORK_FRAG_TABLE_SIZE              NETWORK_FRAG_TABLE_SIZE__max            
    #define PLATFORM_DEPENDENT__NETWORK_NAT_NUM_ENTRIES              NETWORK_NAT_NUM_ENTRIES__max            
    #define PLATFORM_DEPENDENT__NETWORK_TCP_NUM_PORTS__SUPPORTED     NETWORK_TCP_NUM_PORTS__SUPPORTED__max   
    #define PLATFORM_DEPENDENT__NETWORK_UDP_NUM_PORTS__SUPPORTED     NETWORK_UDP_NUM_PORTS__SUPPORTED__max   
    #define PLATFORM_DEPENDENT__NETWORK_MEMORY_POOL__SIZE            NETWORK_MEMORY_POOL__SIZE__max
    #define PLATFORM_DEPENDENT__SOCKETS_MAX_COUNT                    (PLATFORM_DEPENDENT__NETWORK_TCP_NUM_PORTS__SUPPORTED + PLATFORM_DEPENDENT__NETWORK_UDP_NUM_PORTS__SUPPORTED)
#endif      

#endif // _DRIVERS_NETWORK_DEFINES_H_

