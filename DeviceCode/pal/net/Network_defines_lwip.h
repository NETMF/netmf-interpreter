////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_NETWORK_DEFINES_LWIP_H_
#define _DRIVERS_NETWORK_DEFINES_LWIP_H_ 1

/* Pick min, default or max configuration based on platform */
#include <lwip_selector.h>

#if !defined(NETWORK_MEMORY_PROFILE_LWIP__small) && !defined(NETWORK_MEMORY_PROFILE_LWIP__medium) && !defined(NETWORK_MEMORY_PROFILE_LWIP__large) && !defined(NETWORK_MEMORY_PROFILE_LWIP__custom)
#error You must define a NETWORK_MEMORY_PROFILE_LWIP_xxx for this platform
#endif

#define TCPIP_LWIP 1

/* min, default, max configuration for lwIP. Values have initially 
been taken from lwiopts.small.h, lwipopts.h and lwipopts.big.h */

/* MEM_SIZE: the size of the heap memory. If the application will send
a lot of data that needs to be copied, this should be set high. */
#define MEM_SIZE__min                       (16*1024)
#define MEM_SIZE__default                   (64*1024)
#define MEM_SIZE__max                       (1024*1024)  // TODO - this seems a bit extreme

/* MEMP_NUM_PBUF: the number of memp struct pbufs. If the application
   sends a lot of data out of ROM (or other static memory), this
   should be set high. */
#define MEMP_NUM_PBUF__min                  16
#define MEMP_NUM_PBUF__default              32
#define MEMP_NUM_PBUF__max                  32

/* MEMP_NUM_UDP_PCB: the number of UDP protocol control blocks. One
   per active UDP "connection". */
#define MEMP_NUM_UDP_PCB__min               6
#define MEMP_NUM_UDP_PCB__default           8
#define MEMP_NUM_UDP_PCB__max               16

/* MEMP_NUM_TCP_PCB: the number of simulatenously active TCP
   connections. */
#define MEMP_NUM_TCP_PCB__min               8
#define MEMP_NUM_TCP_PCB__default           16
#define MEMP_NUM_TCP_PCB__max               32

/* MEMP_NUM_TCP_PCB_LISTEN: the number of listening TCP
   connections. */
#define MEMP_NUM_TCP_PCB_LISTEN__min        4
#define MEMP_NUM_TCP_PCB_LISTEN__default    8
#define MEMP_NUM_TCP_PCB_LISTEN__max        12

/* MEMP_NUM_TCP_SEG: the number of simultaneously queued TCP
   segments. */
#define MEMP_NUM_TCP_SEG__min               32
#define MEMP_NUM_TCP_SEG__default           64
#define MEMP_NUM_TCP_SEG__max               128

/* MEMP_NUM_SYS_TIMEOUT: the number of simulateously active
   timeouts. */
#define MEMP_NUM_SYS_TIMEOUT__min           8   // set to 3 in lwiopts.small.h but didn't compile
#define MEMP_NUM_SYS_TIMEOUT__default       12
#define MEMP_NUM_SYS_TIMEOUT__max           16

/* MEMP_NUM_NETBUF: the number of struct netbufs. */
#define MEMP_NUM_NETBUF__min                8
#define MEMP_NUM_NETBUF__default            16
#define MEMP_NUM_NETBUF__max                32

/* MEMP_NUM_NETCONN: the number of struct netconns. */
#define MEMP_NUM_NETCONN__min               10
#define MEMP_NUM_NETCONN__default           20
#define MEMP_NUM_NETCONN__max               40

/* PBUF_POOL_SIZE: the number of buffers in the pbuf pool. */
#define PBUF_POOL_SIZE__min                 40
#define PBUF_POOL_SIZE__default             128
#define PBUF_POOL_SIZE__max                 256

/* PBUF_POOL_BUFSIZE: the size of each pbuf in the pbuf pool. */
#define PBUF_POOL_BUFSIZE__min              512
#define PBUF_POOL_BUFSIZE__default          1024
#define PBUF_POOL_BUFSIZE__max              2048

/* TCP Maximum segment size. */
#define TCP_MSS__min                        536
#define TCP_MSS__default                    1460
#define TCP_MSS__max                        1460

/* TCP sender buffer space (bytes). */
#define TCP_SND_BUF__min                    (2*TCP_MSS)
#define TCP_SND_BUF__default                (4*TCP_MSS)
#define TCP_SND_BUF__max                    (8*TCP_MSS)

/* TCP sender buffer space (pbufs). This must be at least = 2 *
   TCP_SND_BUF/TCP_MSS for things to work. */
#define TCP_SND_QUEUELEN__min               ((2*TCP_SND_BUF)/TCP_MSS)
#define TCP_SND_QUEUELEN__default           ((4*TCP_SND_BUF)/TCP_MSS)
#define TCP_SND_QUEUELEN__max               ((8*TCP_SND_BUF)/TCP_MSS)

/* TCP receive window. */
#define TCP_WND__min                        (2*1024)
#define TCP_WND__default                    (8*1024)
#define TCP_WND__max                        (32*1024)

/* TCP writable space (bytes). This must be less than or equal
   to TCP_SND_BUF. It is the amount of space which must be
   available in the tcp snd_buf for select to return writable */
#define TCP_SNDLOWAT__min                   (TCP_SND_BUF/2)
#define TCP_SNDLOWAT__default               (TCP_SND_BUF/2)
#define TCP_SNDLOWAT__max                   (TCP_SND_BUF/32)


//--// RAM Profiles

#ifdef NETWORK_MEMORY_PROFILE_LWIP__small
    #define PLATFORM_DEPENDENT__MEM_SIZE                        MEM_SIZE__min
    #define PLATFORM_DEPENDENT__MEMP_NUM_PBUF                   MEMP_NUM_PBUF__min
    #define PLATFORM_DEPENDENT__MEMP_NUM_UDP_PCB                MEMP_NUM_UDP_PCB__min
    #define PLATFORM_DEPENDENT__MEMP_NUM_TCP_PCB                MEMP_NUM_TCP_PCB__min
    #define PLATFORM_DEPENDENT__MEMP_NUM_TCP_PCB_LISTEN         MEMP_NUM_TCP_PCB_LISTEN__min
    #define PLATFORM_DEPENDENT__MEMP_NUM_TCP_SEG                MEMP_NUM_TCP_SEG__min
    #define PLATFORM_DEPENDENT__MEMP_NUM_SYS_TIMEOUT            MEMP_NUM_SYS_TIMEOUT__min
    #define PLATFORM_DEPENDENT__MEMP_NUM_NETBUF                 MEMP_NUM_NETBUF__min
    #define PLATFORM_DEPENDENT__MEMP_NUM_NETCONN                MEMP_NUM_NETCONN__min
    #define PLATFORM_DEPENDENT__PBUF_POOL_SIZE                  PBUF_POOL_SIZE__min
    #define PLATFORM_DEPENDENT__PBUF_POOL_BUFSIZE               PBUF_POOL_BUFSIZE__min
    #define PLATFORM_DEPENDENT__TCP_MSS                         TCP_MSS__min
    #define PLATFORM_DEPENDENT__TCP_SND_BUF                     TCP_SND_BUF__min
    #define PLATFORM_DEPENDENT__TCP_SND_QUEUELEN                TCP_SND_QUEUELEN__min
    #define PLATFORM_DEPENDENT__TCP_WND                         TCP_WND__min
    #define PLATFORM_DEPENDENT__TCP_SNDLOWAT                    TCP_SNDLOWAT__min
#endif

#ifdef NETWORK_MEMORY_PROFILE_LWIP__medium
    #define PLATFORM_DEPENDENT__MEM_SIZE                        MEM_SIZE__default
    #define PLATFORM_DEPENDENT__MEMP_NUM_PBUF                   MEMP_NUM_PBUF__default
    #define PLATFORM_DEPENDENT__MEMP_NUM_UDP_PCB                MEMP_NUM_UDP_PCB__default
    #define PLATFORM_DEPENDENT__MEMP_NUM_TCP_PCB                MEMP_NUM_TCP_PCB__default
    #define PLATFORM_DEPENDENT__MEMP_NUM_TCP_PCB_LISTEN         MEMP_NUM_TCP_PCB_LISTEN__default
    #define PLATFORM_DEPENDENT__MEMP_NUM_TCP_SEG                MEMP_NUM_TCP_SEG__default
    #define PLATFORM_DEPENDENT__MEMP_NUM_SYS_TIMEOUT            MEMP_NUM_SYS_TIMEOUT__default
    #define PLATFORM_DEPENDENT__MEMP_NUM_NETBUF                 MEMP_NUM_NETBUF__default
    #define PLATFORM_DEPENDENT__MEMP_NUM_NETCONN                MEMP_NUM_NETCONN__default
    #define PLATFORM_DEPENDENT__PBUF_POOL_SIZE                  PBUF_POOL_SIZE__default
    #define PLATFORM_DEPENDENT__PBUF_POOL_BUFSIZE               PBUF_POOL_BUFSIZE__default
    #define PLATFORM_DEPENDENT__TCP_MSS                         TCP_MSS__default
    #define PLATFORM_DEPENDENT__TCP_SND_BUF                     TCP_SND_BUF__default
    #define PLATFORM_DEPENDENT__TCP_SND_QUEUELEN                TCP_SND_QUEUELEN__default
    #define PLATFORM_DEPENDENT__TCP_WND                         TCP_WND__default
    #define PLATFORM_DEPENDENT__TCP_SNDLOWAT                    TCP_SNDLOWAT__default
#endif

#ifdef NETWORK_MEMORY_PROFILE_LWIP__large
    #define PLATFORM_DEPENDENT__MEM_SIZE                        MEM_SIZE__max
    #define PLATFORM_DEPENDENT__MEMP_NUM_PBUF                   MEMP_NUM_PBUF__max
    #define PLATFORM_DEPENDENT__MEMP_NUM_UDP_PCB                MEMP_NUM_UDP_PCB__max
    #define PLATFORM_DEPENDENT__MEMP_NUM_TCP_PCB                MEMP_NUM_TCP_PCB__max
    #define PLATFORM_DEPENDENT__MEMP_NUM_TCP_PCB_LISTEN         MEMP_NUM_TCP_PCB_LISTEN__max
    #define PLATFORM_DEPENDENT__MEMP_NUM_TCP_SEG                MEMP_NUM_TCP_SEG__max
    #define PLATFORM_DEPENDENT__MEMP_NUM_SYS_TIMEOUT            MEMP_NUM_SYS_TIMEOUT__max
    #define PLATFORM_DEPENDENT__MEMP_NUM_NETBUF                 MEMP_NUM_NETBUF__max
    #define PLATFORM_DEPENDENT__MEMP_NUM_NETCONN                MEMP_NUM_NETCONN__max
    #define PLATFORM_DEPENDENT__PBUF_POOL_SIZE                  PBUF_POOL_SIZE__max
    #define PLATFORM_DEPENDENT__PBUF_POOL_BUFSIZE               PBUF_POOL_BUFSIZE__max
    #define PLATFORM_DEPENDENT__TCP_MSS                         TCP_MSS__max
    #define PLATFORM_DEPENDENT__TCP_SND_BUF                     TCP_SND_BUF__max
    #define PLATFORM_DEPENDENT__TCP_SND_QUEUELEN                TCP_SND_QUEUELEN__max
    #define PLATFORM_DEPENDENT__TCP_WND                         TCP_WND__max
    #define PLATFORM_DEPENDENT__TCP_SNDLOWAT                    TCP_SNDLOWAT__max
#endif

#ifdef NETWORK_MEMORY_PROFILE_LWIP__custom
    #define PLATFORM_DEPENDENT__MEM_SIZE                        MEM_SIZE__custom
    #define PLATFORM_DEPENDENT__MEMP_NUM_PBUF                   MEMP_NUM_PBUF__custom
    #define PLATFORM_DEPENDENT__MEMP_NUM_UDP_PCB                MEMP_NUM_UDP_PCB__custom
    #define PLATFORM_DEPENDENT__MEMP_NUM_TCP_PCB                MEMP_NUM_TCP_PCB__custom
    #define PLATFORM_DEPENDENT__MEMP_NUM_TCP_PCB_LISTEN         MEMP_NUM_TCP_PCB_LISTEN__custom
    #define PLATFORM_DEPENDENT__MEMP_NUM_TCP_SEG                MEMP_NUM_TCP_SEG__custom
    #define PLATFORM_DEPENDENT__MEMP_NUM_SYS_TIMEOUT            MEMP_NUM_SYS_TIMEOUT__custom
    #define PLATFORM_DEPENDENT__MEMP_NUM_NETBUF                 MEMP_NUM_NETBUF__custom
    #define PLATFORM_DEPENDENT__MEMP_NUM_NETCONN                MEMP_NUM_NETCONN__custom
    #define PLATFORM_DEPENDENT__PBUF_POOL_SIZE                  PBUF_POOL_SIZE__custom
    #define PLATFORM_DEPENDENT__PBUF_POOL_BUFSIZE               PBUF_POOL_BUFSIZE__custom
    #define PLATFORM_DEPENDENT__TCP_MSS                         TCP_MSS__custom
    #define PLATFORM_DEPENDENT__TCP_SND_BUF                     TCP_SND_BUF__custom
    #define PLATFORM_DEPENDENT__TCP_SND_QUEUELEN                TCP_SND_QUEUELEN__custom
    #define PLATFORM_DEPENDENT__TCP_WND                         TCP_WND__custom
    #define PLATFORM_DEPENDENT__TCP_SNDLOWAT                    TCP_SNDLOWAT__custom
#endif

#define PLATFORM_DEPENDENT__SOCKETS_MAX_COUNT                   (PLATFORM_DEPENDENT__MEMP_NUM_UDP_PCB + PLATFORM_DEPENDENT__MEMP_NUM_TCP_PCB)                                     

#endif // _DRIVERS_NETWORK_DEFINES_LWIP_H_

