/**
 * @file
 */
/******************************************************************************
 * Copyright AllSeen Alliance. All rights reserved.
 *
 *    Permission to use, copy, modify, and/or distribute this software for any
 *    purpose with or without fee is hereby granted, provided that the above
 *    copyright notice and this permission notice appear in all copies.
 *
 *    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 *    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 *    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 *    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 *    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 *    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 *    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 ******************************************************************************/

/**
 * Per-module definition of the current module for debug logging.  Must be defined
 * prior to first inclusion of aj_debug.h
 */
#define AJ_MODULE NET

// DEBUGGING ENABLE ////////
// uncomment the following lines for debug messages
#undef  NDEBUG
#define AJ_DEBUG_RESTRICT AJ_DEBUG_WARN 
//#define AJ_DEBUG_RESTRICT AJ_DEBUG_INFO
#define AJ_PRINTF   1
////////////////////////////////////

#include <tinyhal.h>

#include "aj_target.h"
#include "aj_bufio.h"
#include "aj_net.h"
#include "aj_util.h"
#include "aj_debug.h"
#include "aj_connect.h"
#include "aj_bus.h"
#include "aj_disco.h"
#include "aj_config.h"
#include "aj_std.h"

#ifdef AJ_ARDP
#include "aj_ardp.h"
#endif


extern "C" {

AJ_Status AJ_Net_MCastUp(AJ_MCastSocket* mcastSock);

void AJ_Net_MCastDown(AJ_MCastSocket* mcastSock);

AJ_Status AJ_AcquireIPAddress(uint32_t* ip, uint32_t* mask, uint32_t* gateway, int32_t timeout);


}
extern int _AJ_DbgHeader(AJ_DebugLevel level, const char* file, int line);
/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgNET = 0;
#endif

#define INVALID_SOCKET (-1)

/*
 * IANA assigned IPv4 multicast group for AllJoyn.
 */
static const u_long AJ_IPV4_ADDR_MULTICAST_GROUP = SOCK_MAKE_IP_ADDR(224, 0, 0, 113);

/*
 * IANA assigned IPv6 multicast group for AllJoyn.
 */
static const char AJ_IPV6_MULTICAST_GROUP[] = "ff02::13a";

/*
 * IANA assigned UDP multicast port for AllJoyn
 */
#define AJ_UDP_PORT 9956

/*
 * IANA-assigned IPv4 multicast group for mDNS.
 */
static const u_long MDNS_IPV4_MULTICAST_GROUP = SOCK_MAKE_IP_ADDR(224, 0, 0, 251);


/*
 * IANA-assigned IPv6 multicast group for mDNS.
 */
static const char MDNS_IPV6_MULTICAST_GROUP[] = "ff02::fb";

/*
 * IANA-assigned UDP multicast port for mDNS
 */
#define MDNS_UDP_PORT 5353



AJ_Status AJ_IntToString(int32_t val, char* buf, size_t buflen)
{

    AJ_Status status = AJ_OK;
    int c = hal_snprintf(buf, buflen, "%d", val);
    if (c <= 0 || c > buflen) {
        status = AJ_ERR_RESOURCES;
    }
    return status;
};

AJ_Status AJ_InetToString(uint32_t addr, char* buf, size_t buflen)
{
    AJ_Status status = AJ_OK;
    int c = hal_snprintf((char*)buf, buflen, "%u.%u.%u.%u", (addr & 0xFF000000) >> 24, (addr & 0x00FF0000) >> 16, (addr & 0x0000FF00) >> 8, (addr & 0x000000FF));
    if (c <= 0 || c > buflen) {
        status = AJ_ERR_RESOURCES;
    }
    return status;
};

/**
 * Target-specific context for network I/O
 */


typedef struct {
    int tcpSock;
    int udpSock;
} NetContext;

typedef struct {
    int udpSock;
    int udp6Sock;
    int mDnsSock;
    int mDns6Sock;
    int mDnsRecvSock;
    uint32_t mDnsRecvAddr;
    uint16_t mDnsRecvPort;
} MCastContext;

static NetContext netContext = { INVALID_SOCKET, INVALID_SOCKET };
static MCastContext mCastContext = { INVALID_SOCKET, INVALID_SOCKET, INVALID_SOCKET, INVALID_SOCKET };

#ifdef AJ_ARDP
/**
 * Need to predeclare a few things for ARDP
 */
static AJ_Status AJ_Net_ARDP_Connect(AJ_BusAttachment* bus, const AJ_Service* service);
static void AJ_Net_ARDP_Disconnect(AJ_NetSocket* netSock);

#endif // AJ_ARDP

static AJ_Status CloseNetSock(AJ_NetSocket* netSock)
{
    NetContext* context = (NetContext*)netSock->rx.context;
    if (context) {
        if (context->tcpSock != INVALID_SOCKET) {
#ifdef BUILD_LINGER // uncritical if not supported
            struct linger l;
            l.l_onoff = 1;
            l.l_linger = 0;
            SOCK_setsockopt(context->tcpSock, SOL_SOCKET, SOCK_SOCKO_LINGER, (void*)&l, sizeof(l));
#endif
            SOCK_shutdown(context->tcpSock, SOCKET_EVENT_FLAG_SOCKETS_SHUTDOWN);
            SOCK_close(context->tcpSock);
        }
        
        context->tcpSock = INVALID_SOCKET;
        memset(netSock, 0, sizeof(AJ_NetSocket));
    }
    return AJ_OK;
}

static AJ_Status CloseMCastSock(AJ_MCastSocket* mcastSock)
{
    MCastContext* context = (MCastContext*)mcastSock->rx.context;
    if (context) {

        if (context->udpSock != INVALID_SOCKET) {
            SOCK_close(context->udpSock);
        }
        if (context->udp6Sock != INVALID_SOCKET) {
            SOCK_close(context->udp6Sock);
        }
        if (context->mDnsSock != INVALID_SOCKET) {
            SOCK_close(context->mDnsSock);
        }
        if (context->mDns6Sock != INVALID_SOCKET) {
            SOCK_close(context->mDns6Sock);
        }
        if (context->mDnsRecvSock != INVALID_SOCKET) {
            SOCK_close(context->mDnsRecvSock);
        }
        context->udpSock = context->udp6Sock = context->mDnsSock = context->mDns6Sock = context->mDnsRecvSock = INVALID_SOCKET;
        memset(mcastSock, 0, sizeof(AJ_MCastSocket));

    }
    return AJ_OK;
}

AJ_Status AJ_Net_Send(AJ_IOBuffer* buf)
{
    NetContext* context = (NetContext*) buf->context;
    int ret;
    size_t tx = AJ_IO_BUF_AVAIL(buf);

    AJ_InfoPrintf(("AJ_Net_Send(buf=0x%p)\n", buf));

    ASSERT(buf->direction == AJ_IO_BUF_TX);

    while (tx > 0) {
        do {
            ret = SOCK_send(context->tcpSock, (const char *)buf->readPtr, tx, 0);
        } while ((ret == -1) && (SOCK_getlasterror() == SOCK_EWOULDBLOCK));  
        // //???? Would this block ???
        if (ret == -1) {
            AJ_ErrPrintf(("AJ_Net_Send(): send() failed. errno=%d, status=AJ_ERR_WRITE\n", SOCK_getlasterror()));
            return AJ_ERR_WRITE;
        }
        tx -= ret;
        buf->readPtr += ret;
    }
    if (AJ_IO_BUF_AVAIL(buf) == 0) {
        AJ_IO_BUF_RESET(buf);
    }

    AJ_InfoPrintf(("AJ_Net_Send(): status=AJ_OK\n"));

    return AJ_OK;
}

/*
 * An eventfd handle used for interrupting a network read blocked on select
 */
static int interruptFd = INVALID_SOCKET;

/*
 * The socket that is blocked in select
 */
static uint8_t blocked;

/*
 * This function is called to cancel a pending select.
 */
void AJ_Net_Interrupt()
{
//AJ to be complete....

}

AJ_Status AJ_Net_Recv(AJ_IOBuffer* buf, uint32_t len, uint32_t timeout)
{
    NetContext* context = (NetContext*) buf->context;
    AJ_Status status = AJ_OK;
    size_t rx = AJ_IO_BUF_SPACE(buf);
    SOCK_fd_set fds;
    int rc = 0;
    int maxFd = context->tcpSock;
    struct SOCK_timeval tv = { timeout / 1000, 1000 * (timeout % 1000) };

    AJ_InfoPrintf(("AJ_Net_Recv(buf=0x%p, len=%d., timeout=%d.)\n", buf, len, timeout));

    ASSERT(buf->direction == AJ_IO_BUF_RX);

    SOCK_FD_ZERO(&fds);
    SOCK_FD_SET(context->tcpSock, &fds);
    if (interruptFd >= 0) {
        SOCK_FD_SET(interruptFd, &fds);
        maxFd = max(maxFd, interruptFd);
    }
    blocked = TRUE;
    rc = SOCK_select(maxFd + 1, &fds, NULL, NULL, &tv);
    blocked = FALSE;
    if (rc == 0) {
        return AJ_ERR_TIMEOUT;
    }
    rx = min(rx, len);
    if (rx) {
        int ret = SOCK_recv(context->tcpSock, (char *)buf->writePtr, rx, 0);
        if ((ret == -1) || (ret == 0)){
            AJ_ErrPrintf(("AJ_Net_Recv(): recv() failed. errno=\"%d\"\n", SOCK_getlasterror()));
            status = AJ_ERR_READ;
        } else {
//            AJ_InfoPrintf(("AJ_Net_Recv(): recv'd %d from tcp\n", ret));
            buf->writePtr += ret;
        }
    }

//    AJ_InfoPrintf(("AJ_Net_Recv(): status=%s\n", AJ_StatusText(status)));
    return status;
}

#define RXDATASIZE 1500
#define TXDATASIZE 1500
static uint8_t *rxData = NULL;
static uint8_t *txData = NULL;

AJ_Status AJ_Net_Connect(AJ_BusAttachment* bus, const AJ_Service* service)
{
    int ret;
    struct SOCK_sockaddr_in addrBuf;
    int addrSize;
    int tcpSock = INVALID_SOCKET;
    INT32 nonblocking = 1;
    INT32 optval = 1;
    INT32 optLinger = 0;

    AJ_InfoPrintf(("AJ_Net_Connect(pService=0x%p, port=%d, addrType=%d, addr=0x%x)\n", service, service->ipv4port, service->addrTypes, service->ipv4));
#ifdef AJ_ARDP
        if (service->addrTypes & (AJ_ADDR_UDP4 | AJ_ADDR_UDP6)) {
            return AJ_Net_ARDP_Connect(bus, service);
        }
#endif


    if (rxData == NULL) rxData = (uint8_t *)AJ_Malloc(RXDATASIZE);
    if (txData == NULL) txData = (uint8_t *)AJ_Malloc(TXDATASIZE);

    if (txData == NULL || rxData == NULL)
    {
        AJ_ErrPrintf(("AJ_Net_Connect(): failed to allocate buffers\n"));
        goto ConnectError;
    }

    memset(&addrBuf, 0, sizeof(addrBuf));

    tcpSock = SOCK_socket(SOCK_AF_INET, SOCK_SOCK_STREAM, SOCK_IPPROTO_TCP);
    if (tcpSock == INVALID_SOCKET) {
        AJ_ErrPrintf(("AJ_Net_Connect(): socket() failed.  status=AJ_ERR_CONNECT\n"));
        goto ConnectError;
    }
    if (service->addrTypes & AJ_ADDR_TCP4) {
        struct SOCK_sockaddr_in* sa = (struct SOCK_sockaddr_in*)&addrBuf;
        sa->sin_family = SOCK_AF_INET;
        sa->sin_port = SOCK_htons(service->ipv4port);
        sa->sin_addr.S_un.S_addr = service->ipv4;
        addrSize = sizeof(struct SOCK_sockaddr_in);
        AJ_InfoPrintf(("AJ_Net_Connect(): Connect to %x:%u\n", sa->sin_addr, service->ipv4port));
    } else if (service->addrTypes & AJ_ADDR_TCP6) {
#ifdef BUILD_IPV6
        struct sockaddr_in6* sa = (struct sockaddr_in6*)&addrBuf;
        sa->sin6_family = AF_INET6;
        sa->sin6_port = SOCK_htons(service->ipv6port);
        memcpy(sa->sin6_addr.s6_addr, service->ipv6, sizeof(sa->sin6_addr.s6_addr));
        addrSize = sizeof(struct sockaddr_in6);
#endif
    } else {
        AJ_ErrPrintf(("AJ_Net_Connect(): socket() failed.  status=AJ_ERR_CONNECT\n"));
        goto ConnectError;
    }

    ret = SOCK_ioctl( tcpSock, SOCK_FIONBIO, &nonblocking);
    if (ret < 0) {
        AJ_ErrPrintf(("AJ_Net_Connect(): SOCK_FIONBIO failed. errno=%d\n", SOCK_getlasterror()));
    }
    ret = SOCK_setsockopt(tcpSock, SOCK_IPPROTO_TCP, SOCK_TCP_NODELAY, (char*)&optval, sizeof(optval));
    if (ret < 0) {
        AJ_ErrPrintf(("AJ_Net_Connect(): TCP_NODELAY failed. errno=%d\n", SOCK_getlasterror()));
    }
    ret = SOCK_setsockopt(tcpSock, SOCK_SOL_SOCKET, SOCK_SOCKO_LINGER, (const char*)&optLinger, sizeof(INT32));
    if (ret < 0) {
        AJ_ErrPrintf(("AJ_Net_Connect(): SOCK_LINGER failed. errno=%d\n", SOCK_getlasterror()));
    }

    ret = SOCK_connect(tcpSock, (struct SOCK_sockaddr*)&addrBuf, addrSize);
    if ((ret < 0) && (SOCK_getlasterror() != SOCK_EWOULDBLOCK)) {
        AJ_ErrPrintf(("AJ_Net_Connect(): connect() failed. errno=%d, status=AJ_ERR_CONNECT\n", SOCK_getlasterror()));
        goto ConnectError;
    } else {
        netContext.tcpSock = tcpSock;
        AJ_IOBufInit(&bus->sock.rx, rxData, sizeof(uint8_t)*RXDATASIZE, AJ_IO_BUF_RX, &netContext);
        bus->sock.rx.recv = AJ_Net_Recv;
        AJ_IOBufInit(&bus->sock.tx, txData, sizeof(uint8_t)*TXDATASIZE, AJ_IO_BUF_TX, &netContext);
        bus->sock.tx.send = AJ_Net_Send;
        AJ_InfoPrintf(("AJ_Net_Connect(): status=AJ_OK\n"));
    }

    return AJ_OK;

ConnectError:
    if (tcpSock != INVALID_SOCKET) {
        SOCK_close(tcpSock);
    }

    return AJ_ERR_CONNECT;
}

void AJ_Net_Disconnect(AJ_NetSocket* netSock)
{

    AJ_Free(rxData);
    AJ_Free(txData);
    rxData = NULL;
    txData = NULL;
#ifdef AJ_ARDP
    if (netContext.udpSock != INVALID_SOCKET) {
        // we are using UDP!
        AJ_Net_ARDP_Disconnect(netSock);
        memset(netSock, 0, sizeof(AJ_NetSocket));
    } else {
        CloseNetSock(netSock);
    }
#else
    CloseNetSock(netSock);
#endif
}

static uint8_t sendToBroadcast(int sock, uint16_t port, void* ptr, size_t tx)
{
    uint8_t sendSucceeded = FALSE;
    SOCK_addrinfo* addrs;
    SOCK_addrinfo* addr;
    
    int ret = SOCK_getaddrinfo( "", NULL, NULL, &addrs);
    if (ret != 0)
    {
        AJ_ErrPrintf(("sendToBroadcast(): getaddrinfo failed. errno=%d\n", SOCK_getlasterror()));
    }
    addr = addrs;
    while (addr != NULL) {
        // only care about IPV4
        if (addr->ai_addr != NULL && addr->ai_addr->sa_family == SOCK_AF_INET) {

            struct SOCK_sockaddr_in* sin_bcast = (struct SOCK_sockaddr_in*) addr->ai_addr;
            sin_bcast->sin_port = SOCK_htons(port);
            AJ_InfoPrintf(("sendToBroadcast: sending to bcast addr %d.%d.%d.%d\n", 
                sin_bcast->sin_addr.S_un.S_un_b.s_b1,
                sin_bcast->sin_addr.S_un.S_un_b.s_b2,
                sin_bcast->sin_addr.S_un.S_un_b.s_b3,
                sin_bcast->sin_addr.S_un.S_un_b.s_b4
                ));
            ret = SOCK_sendto(sock, (const char *)ptr, tx, 0/*MSG_NOSIGNAL*/, (struct SOCK_sockaddr*) sin_bcast, sizeof(struct SOCK_sockaddr_in));
            if (tx == ret) {
                sendSucceeded = TRUE;
            } else {
                AJ_ErrPrintf(("sendToBroadcast(): sendto failed. errno=\"%d\"\n", SOCK_getlasterror()));
            }
        }
        addr = addr->ai_next;
    }
    SOCK_freeaddrinfo(addrs);
    return sendSucceeded;
}

static AJ_Status RewriteSenderInfo(AJ_IOBuffer* buf, uint32_t addr, uint16_t port)
{
    uint16_t sidVal;
    const char send[4] = { 'd', 'n', 'e', 's' };
    const char sid[] = { 's', 'i', 'd', '=' };
    const char ipv4[] = { 'i', 'p', 'v', '4', '=' };
    const char upcv4[] = { 'u', 'p', 'c', 'v', '4', '=' };
    char sidStr[6];
    char ipv4Str[17];
    char upcv4Str[6];
    uint8_t* pkt;
    uint16_t dataLength;
    int match;
    AJ_Status status;

    // first, pluck the search ID from the mDNS header
    sidVal = *(buf->readPtr) << 8;
    sidVal += *(buf->readPtr + 1);

    // convert to strings
    status = AJ_IntToString((int32_t) sidVal, sidStr, sizeof(sidStr));
    if (status != AJ_OK) {
        return AJ_ERR_WRITE;
    }
    status = AJ_IntToString((int32_t) port, upcv4Str, sizeof(upcv4Str));
    if (status != AJ_OK) {
        return AJ_ERR_WRITE;
    }
    status = AJ_InetToString(addr, ipv4Str, sizeof(ipv4Str));
    if (status != AJ_OK) {
        return AJ_ERR_WRITE;
    }

    // ASSUMPTIONS: sender-info resource record is the final resource record in the packet.
    // sid, ipv4, and upcv4 key value pairs are the final three key/value pairs in the record.
    // The length of the other fields in the record are static.
    //
    // search backwards through packet to find the start of "sender-info"
    pkt = buf->writePtr;
    match = 0;
    do {
        if (*(pkt--) == send[match]) {
            match++;
        } else {
            match = 0;
        }
    } while (pkt != buf->readPtr && match != 4);
    if (match != 4) {
        return AJ_ERR_WRITE;
    }

    // move forward to the Data Length field
    pkt += 22;

    // actual data length is the length of the static values already in the buffer plus
    // the three dynamic key-value pairs to re-write
    dataLength = 23 + 1 + sizeof(sid) + hal_strlen_s(sidStr) + 1 + sizeof(ipv4) + hal_strlen_s(ipv4Str) + 1 + sizeof(upcv4) + hal_strlen_s(upcv4Str);
    *pkt++ = (dataLength >> 8) & 0xFF;
    *pkt++ = dataLength & 0xFF;

    // move forward past the static key-value pairs
    pkt += 23;

    // ASSERT: must be at the start of "sid="
    ASSERT(*(pkt + 1) == 's');

    // re-write new values
    *pkt++ = sizeof(sid) + hal_strlen_s(sidStr);
    memcpy(pkt, sid, sizeof(sid));
    pkt += sizeof(sid);
    memcpy(pkt, sidStr, hal_strlen_s(sidStr));
    pkt += hal_strlen_s(sidStr);

    *pkt++ = sizeof(ipv4) + hal_strlen_s(ipv4Str);
    memcpy(pkt, ipv4, sizeof(ipv4));
    pkt += sizeof(ipv4);
    memcpy(pkt, ipv4Str, hal_strlen_s(ipv4Str));
    pkt += hal_strlen_s(ipv4Str);

    *pkt++ = sizeof(upcv4) + hal_strlen_s(upcv4Str);
    memcpy(pkt, upcv4, sizeof(upcv4));
    pkt += sizeof(upcv4);
    memcpy(pkt, upcv4Str, hal_strlen_s(upcv4Str));
    pkt += hal_strlen_s(upcv4Str);

    buf->writePtr = pkt;

    return AJ_OK;
}



AJ_Status AJ_Net_SendTo(AJ_IOBuffer* buf)
{
    int ret = -1;
    uint8_t sendSucceeded = FALSE;
    size_t tx = AJ_IO_BUF_AVAIL(buf);
    MCastContext* context = (MCastContext*) buf->context;
    AJ_InfoPrintf(("AJ_Net_SendTo(buf=0x%p, buf->readPtr=0x%p, size tx=%d)\n", buf, buf->readPtr, tx));
    ASSERT(buf->direction == AJ_IO_BUF_TX);

    if (tx > 0) {
        if ((context->udpSock != INVALID_SOCKET) && (buf->flags & AJ_IO_BUF_AJ)) {
            struct SOCK_sockaddr_in sin;
            sin.sin_family = SOCK_AF_INET;
            sin.sin_port = SOCK_htons(AJ_UDP_PORT);
            sin.sin_addr.S_un.S_addr = SOCK_htonl(AJ_IPV4_ADDR_MULTICAST_GROUP);

            ret = SOCK_sendto(context->udpSock, (const char *)buf->readPtr, tx, 0 /*MSG_NOSIGNAL*/, (struct SOCK_sockaddr*)&sin, sizeof(sin));
            if (tx == ret) {
                sendSucceeded = TRUE;
            }else {
                AJ_ErrPrintf(("AJ_Net_SendTo(): sendto AJ IPv4 failed.errno= %d \n", SOCK_getlasterror()));
            }
            if (sendToBroadcast(context->udpSock, AJ_UDP_PORT, buf->readPtr, tx) == TRUE) {
                sendSucceeded = TRUE;
            }
        }

#ifdef BUILD_IPV6
// need to updated...
        // now sendto the ipv6 address
        if ((context->udp6Sock != INVALID_SOCKET) && (buf->flags & AJ_IO_BUF_AJ)) {
            struct sockaddr_in6 sin6;
            sin6.sin6_family = AF_INET6;
            sin6.sin6_flowinfo = 0;
            sin6.sin6_scope_id = 0;
            sin6.sin6_port = htons(AJ_UDP_PORT);
            if (inet_pton(AF_INET6, AJ_IPV6_MULTICAST_GROUP, &sin6.sin6_addr) == 1) {
                ret = sendto(context->udp6Sock, buf->readPtr, tx, MSG_NOSIGNAL, (struct sockaddr*) &sin6, sizeof(sin6));
                if (tx == ret) {
                    sendSucceeded = TRUE;
                } else {
                    AJ_ErrPrintf(("AJ_Net_SendTo(): sendto AJ IPv6 failed. errno=\"%d\"\n", SOCK_getlasterror()));
                }
            } else {
                AJ_ErrPrintf(("AJ_Net_SendTo(): Invalid AJ IPv6 address. errno=\"%d\"\n", SOCK_getlasterror()));
            }
        }
#endif
    }

    if (buf->flags & AJ_IO_BUF_MDNS) {
        if (RewriteSenderInfo(buf, context->mDnsRecvAddr, context->mDnsRecvPort) != AJ_OK) {
            AJ_WarnPrintf(("AJ_Net_SendTo(): RewriteSenderInfo failed.\n"));
            tx = 0;
        } else {
            tx = AJ_IO_BUF_AVAIL(buf);
        }
    }
    
    if (tx > 0) {
        if ((context->mDnsSock != INVALID_SOCKET) && (buf->flags & AJ_IO_BUF_MDNS)) {
            struct SOCK_sockaddr_in sin;
            sin.sin_family = SOCK_AF_INET;
            sin.sin_port = SOCK_htons(MDNS_UDP_PORT);
            sin.sin_addr.S_un.S_addr = SOCK_htonl(MDNS_IPV4_MULTICAST_GROUP);
            ret = SOCK_sendto(context->mDnsSock, (const char *)buf->readPtr, tx, 0 /*MSG_NOSIGNAL*/, (struct SOCK_sockaddr*)&sin, sizeof(sin));
            if (tx == ret) {
                sendSucceeded = TRUE;
            } else {
                AJ_ErrPrintf(("AJ_Net_SendTo(): sendto mDNS IPv4 failed. errno=\"%d\"\n",  SOCK_getlasterror()));
            }

            if (sendToBroadcast(context->mDnsSock, MDNS_UDP_PORT, buf->readPtr, tx) == TRUE) {
                sendSucceeded = TRUE;
            } // leave sendSucceeded unchanged if FALSE
        }
#ifdef BUILD_IPV6

        if ((context->mDns6Sock != INVALID_SOCKET) && (buf->flags & AJ_IO_BUF_MDNS)) {
            struct sockaddr_in6 sin6;
            sin6.sin6_family = AF_INET6;
            sin6.sin6_flowinfo = 0;
            sin6.sin6_scope_id = 0;
            sin6.sin6_port = htons(MDNS_UDP_PORT);
            if (inet_pton(AF_INET6, MDNS_IPV6_MULTICAST_GROUP, &sin6.sin6_addr) == 1) {
                ret = sendto(context->mDns6Sock, buf->readPtr, tx, MSG_NOSIGNAL, (struct sockaddr*) &sin6, sizeof(sin6));
                if (tx == ret) {
                    sendSucceeded = TRUE;
                } else {
                    AJ_ErrPrintf(("AJ_Net_SendTo(): sendto mDNS IPv6 failed. errno=\"%d\"\n", SOCK_getlasterror()));
                }
            } else {
                AJ_ErrPrintf(("AJ_Net_SendTo(): Invalid mDNS IPv6 address. errno=\"%d\"\n", SOCK_getlasterror()));
            }
        }

#endif
    
    
        if (!sendSucceeded) {
            /* Both IPv4 and IPv6 failed, return an error */
            AJ_ErrPrintf(("AJ_Net_SendTo(): sendto() failed. errno=%d, status=AJ_ERR_WRITE\n", SOCK_getlasterror()));
            return AJ_ERR_WRITE;
        }
        buf->readPtr += ret;
    }
    AJ_IO_BUF_RESET(buf);
    AJ_InfoPrintf(("AJ_Net_SendTo(): status=AJ_OK\n"));

    return AJ_OK;
}

AJ_Status AJ_Net_RecvFrom(AJ_IOBuffer* buf, uint32_t len, uint32_t timeout)
{
    MCastContext* context = (MCastContext*) buf->context;
    AJ_Status status = AJ_OK;
    int ret;
    size_t rx;
    SOCK_fd_set fds;
    int maxFd = INVALID_SOCKET;
    int rc = 0;
    struct SOCK_timeval tv = { timeout / 1000, 1000 * (timeout % 1000) };

    AJ_InfoPrintf(("AJ_Net_RecvFrom(buf=0x%p, len=%d., timeout=%d. Context udp%d, mDNS %d)\n", buf, len, timeout,context->mDnsRecvSock,context->udpSock));
    

    ASSERT(buf->direction == AJ_IO_BUF_RX);
    ASSERT(context->mDnsRecvSock != INVALID_SOCKET);


    SOCK_FD_ZERO(&fds);
    
    SOCK_FD_SET(context->mDnsRecvSock, &fds);
    maxFd = context->mDnsRecvSock;
    
    if (context->udpSock != INVALID_SOCKET) {
        SOCK_FD_SET(context->udpSock, &fds);
        maxFd = max(maxFd, context->udpSock);
    }

#ifdef BUILD_IPV6
    if (context->udp6Sock != INVALID_SOCKET) {
        SOCK_FD_SET(context->udp6Sock, &fds);
        maxFd = max(maxFd, context->udp6Sock);
    }
#endif

    rc = SOCK_select(maxFd + 1, &fds, NULL, NULL, &tv);
    if (rc == 0) {
        AJ_InfoPrintf(("AJ_Net_RecvFrom(): select() timed out. status=AJ_ERR_TIMEOUT\n"));
        return AJ_ERR_TIMEOUT;
    }
    // we need to read from the first socket that has data available.

    rx = AJ_IO_BUF_SPACE(buf);
    if (context->mDnsRecvSock != INVALID_SOCKET /*&& FD_ISSET(context->mDnsRecvSock, &fds) */) {
        rx = min(rx, len);
        if (rx) {
            ret = SOCK_recvfrom(context->mDnsRecvSock, (char *)buf->writePtr, rx, 0, NULL, 0);
            if (ret == -1) {
                AJ_ErrPrintf(("AJ_Net_RecvFrom(): mDnsRecvSock recvfrom() failed. errno=\"%d\"\n",  SOCK_getlasterror()));
                status = AJ_ERR_READ;
            } else {
                AJ_InfoPrintf(("AJ_Net_RecvFrom(): recv'd %d from mDNS\n", (int) ret));
                buf->flags |= AJ_IO_BUF_MDNS;
                buf->writePtr += ret;
                status = AJ_OK;
                goto Finished;
            }
        }
    }


#ifdef BUILD_IPV6
    rx = AJ_IO_BUF_SPACE(buf);
    if (context->udp6Sock != INVALID_SOCKET && FD_ISSET(context->udp6Sock, &fds)) {
        rx = min(rx, len);
        if (rx) {
            ret = recvfrom(context->udp6Sock, buf->writePtr, rx, 0, NULL, 0);
            if (ret == -1) {
                AJ_ErrPrintf(("AJ_Net_RecvFrom(): recvfrom() failed. errno=\"%d\"\n", SOCK_getlasterror()));
                status = AJ_ERR_READ;
            } else {
                AJ_InfoPrintf(("AJ_Net_RecvFrom(): recv'd %d from udp6\n", (int) ret));
                buf->flags |= AJ_IO_BUF_AJ;
                buf->writePtr += ret;
                status = AJ_OK;
                goto Finished;
            }
        }
    }
#endif

    rx = AJ_IO_BUF_SPACE(buf);
    if (context->udpSock != INVALID_SOCKET /*&& SOCK_FD_ISSET(context->udpSock, &fds)*/) {
        rx = min(rx, len);
        if (rx) {
            ret = SOCK_recvfrom(context->udpSock, (char *)buf->writePtr, rx, 0, NULL, 0);
            if (ret == -1) {
                AJ_ErrPrintf(("AJ_Net_RecvFrom(): recvfrom() failed. errno=%d, status=AJ_ERR_READ\n", SOCK_getlasterror()));
                status = AJ_ERR_READ;
            } else {
                AJ_InfoPrintf(("AJ_Net_RecvFrom(): recvfrom() %d bytes, status=AJ_OK\n", ret));
                buf->flags |= AJ_IO_BUF_AJ;
                buf->writePtr += ret;
                status = AJ_OK;
                goto Finished;
                
            }
        }
    }

Finished:
    AJ_InfoPrintf(("AJ_Net_RecvFrom(): status=%s\n", AJ_StatusText(status)));
    return status;
}

/*
 * Need enough space to receive a complete name service packet when used in UDP
 * mode.  NS expects MTU of 1500 subtracts UDP, IP and ethertype overhead.
 * 1500 - 8 -20 - 18 = 1454.  txData buffer size needs to be big enough to hold
 * max(NS WHO-HAS for one name (4 + 2 + 256 = 262),
*  mDNS query for one name (190 + 5 + 5 + 15 + 256 = 471)) = 471
*/

#define RXDATAMCASTSIZE 1454
#define TXDATAMCASTSIZE 471

static uint8_t *rxDataMCast = NULL;
static uint8_t *txDataMCast = NULL;

static int MCastUp4(const u_long group, uint16_t port)
{
    int ret = 0;
	SOCK_ip_mreq mreq;
    SOCK_sockaddr_in sin;
    int reuse = 1;
    int bcast = 1;
    SOCK_SOCKET mcastSock = INVALID_SOCKET;

    memset(&sin, 0, sizeof(sin));

    AJ_InfoPrintf(("MCastUp4()\n"));

//    mcastSock = SOCK_socket(SOCK_AF_INET, SOCK_SOCK_DGRAM, SOCK_IPPROTO_UDP);

    mcastSock = SOCK_socket(SOCK_AF_INET, SOCK_SOCK_DGRAM, SOCK_IPPROTO_UDP);

    if (mcastSock == INVALID_SOCKET) {
        AJ_ErrPrintf(("MCastUp4(): socket() fails. status=AJ_ERR_READ\n"));
        return AJ_ERR_READ;
    }

    ret = SOCK_setsockopt(mcastSock, SOCK_SOL_SOCKET, SOCK_SOCKO_REUSEADDRESS, (char*)&reuse, sizeof(reuse));
    if (ret != 0) {
        AJ_ErrPrintf(("MCastUp4(): setsockopt(SO_REUSEADDR) failed. errno=%d\n", SOCK_getlasterror()));
        goto ExitError;
    }

    // enable IP broadcast on this socket.
    // This is needed for bcast router discovery
    ret = SOCK_setsockopt(mcastSock, SOCK_SOL_SOCKET, SOCK_SOCKO_BROADCAST, (char*)&bcast, sizeof(bcast));
    if (ret != 0) {
        AJ_ErrPrintf(("BcastUp4(): setsockopt(SOL_SOCKET, SO_BROADCAST) failed. errno=%d\n", SOCK_getlasterror()));
        goto ExitError;
    }
    /*
     * Bind an supplied port
     */
    sin.sin_family = SOCK_AF_INET;
    sin.sin_port = SOCK_htons(port);
    sin.sin_addr.S_un.S_addr = SOCK_htonl(SOCK_INADDR_ANY);
    ret = SOCK_bind(mcastSock, (const SOCK_sockaddr *)&sin, sizeof(sin));
    if (ret < 0) {
        AJ_ErrPrintf(("MCastUp4(): bind() failed. errno=%d, status=AJ_ERR_READ\n", SOCK_getlasterror()));
        goto ExitError;
    }

    /*
    * Join our multicast group
    */
//    SOCK_ip_mreq mreq;
    mreq.imr_interface.S_un.S_addr = SOCK_htonl(SOCK_INADDR_ANY);
    mreq.imr_multiaddr.S_un.S_addr = SOCK_htonl(group);
    ret = SOCK_setsockopt(mcastSock, SOCK_IPPROTO_IP, SOCK_IPO_ADD_MEMBERSHIP, (const char *)&mreq, sizeof(mreq));
    if (ret != 0) {
        ret = SOCK_getlasterror();
        AJ_ErrPrintf(("MCastUp4(): setsockopt(IP_ADD_MEMBERSHIP) failed. errno=%d, status=AJ_ERR_READ\n", SOCK_getlasterror()));
        goto ExitError;
    }

#if 0
    /*
     * Bind an supplied port
     */
    sin.sin_family = SOCK_AF_INET;
    sin.sin_port = SOCK_htons(port);
    sin.sin_addr.S_un.S_addr = SOCK_htonl(SOCK_INADDR_ANY);
    ret = SOCK_bind(mcastSock, (const SOCK_sockaddr *)&sin, sizeof(sin));
    if (ret < 0) {
        AJ_ErrPrintf(("MCastUp4(): bind() failed. errno=%d, status=AJ_ERR_READ\n", SOCK_getlasterror()));
        goto ExitError;
    }
#endif
    return mcastSock;

ExitError:
    SOCK_close(mcastSock);
    return INVALID_SOCKET;
}

#ifdef BUILD_IPV6
static int MCastUp6(const char* group, uint16_t port)
{
    int ret;
    struct ipv6_mreq mreq6;
    struct sockaddr_in6 sin6;
    int reuse = 1;
    int mcastSock;

    mcastSock = socket(AF_INET6, SOCK_DGRAM, 0);
    if (mcastSock == INVALID_SOCKET) {
        AJ_ErrPrintf(("MCastUp6(): socket() fails. errno=\"%d\" status=AJ_ERR_READ\n", SOCK_getlasterror()));
        return INVALID_SOCKET;
    }

    ret = setsockopt(mcastSock, SOL_SOCKET, SO_REUSEADDR, &reuse, sizeof(reuse));
    if (ret != 0) {
        AJ_ErrPrintf(("MCastUp6(): setsockopt(SO_REUSEADDR) failed. errno=\"%d\", status=AJ_ERR_READ\n", SOCK_getlasterror()));
        goto ExitError;
    }

    /*
     * Bind supplied port
     */
    memset(&sin6, 0, sizeof(sin6));
    sin6.sin6_family = AF_INET6;
    sin6.sin6_port = htons(port);
    sin6.sin6_addr = in6addr_any;
    ret = bind(mcastSock, (struct sockaddr*) &sin6, sizeof(sin6));
    if (ret < 0) {
        AJ_ErrPrintf(("MCastUp6(): bind() failed. errno=\"%d\", status=AJ_ERR_READ\n", SOCK_getlasterror()));
        goto ExitError;
    }

    /*
     * Join multicast group
     */
    memset(&mreq6, 0, sizeof(mreq6));
    inet_pton(AF_INET6, group, &mreq6.ipv6mr_multiaddr);
    mreq6.ipv6mr_interface = 0;
    ret = setsockopt(mcastSock, IPPROTO_IPV6, IPV6_JOIN_GROUP, &mreq6, sizeof(mreq6));
    if (ret < 0) {
        AJ_ErrPrintf(("MCastUp6(): setsockopt(IP_ADD_MEMBERSHIP) failed. errno=\"%d\", status=AJ_ERR_READ\n", SOCK_getlasterror()));
        goto ExitError;
    }

    return mcastSock;

ExitError:
    close(mcastSock);
    return INVALID_SOCKET;
}
#endif

static uint32_t chooseMDnsRecvAddr()
{
    uint32_t recvAddr = 0;
    SOCK_addrinfo* addrs;
    SOCK_addrinfo* addr;


    int ret = SOCK_getaddrinfo( "", NULL, NULL, &addrs);
    if (ret != 0)
    {
        AJ_ErrPrintf(("chooseMDnsRecvAddr(): getaddrinfo failed. errno=%d\n", SOCK_getlasterror()));
    }
    addr = addrs;

    while (addr != NULL) {
        // Choose first IPv4 address that is not LOOPBACK
        if (addr->ai_addr != NULL && addr->ai_addr->sa_family == SOCK_AF_INET /*&& !(addr->ifa_flags & IFF_LOOPBACK) */) {
            struct SOCK_sockaddr_in* sin = (struct SOCK_sockaddr_in*) addr->ai_addr;
            recvAddr = sin->sin_addr.S_un.S_addr;
        }
        addr = addr->ai_next;
    }
    SOCK_freeaddrinfo(addrs);
	
    return recvAddr;
}

static int MDnsRecvUp()
{
    int ret;
    struct SOCK_sockaddr_in sin;
    int reuse = 1;
    int recvSock;

     memset(&sin, 0, sizeof(sin));

    recvSock = SOCK_socket(SOCK_AF_INET, SOCK_SOCK_DGRAM, SOCK_IPPROTO_UDP);
    if (recvSock == INVALID_SOCKET) {
        AJ_ErrPrintf(("MDnsRecvUp(): socket() fails. status=AJ_ERR_READ\n"));
        goto ExitError;
    }

    ret = SOCK_setsockopt(recvSock, SOCK_SOL_SOCKET, SOCK_SOCKO_REUSEADDRESS, (char *)&reuse, sizeof(reuse));
    if (ret != 0) {
        AJ_ErrPrintf(("MDnsRecvUp(): setsockopt(SO_REUSEADDR) failed. errno=\"%d\", status=AJ_ERR_READ\n", SOCK_getlasterror()));
        goto ExitError;
    }

    sin.sin_family = SOCK_AF_INET;
    sin.sin_port = SOCK_htons(0);
    sin.sin_addr.S_un.S_addr = SOCK_htonl(SOCK_INADDR_ANY);
    ret = SOCK_bind(recvSock, (struct SOCK_sockaddr*) &sin, sizeof(sin));
    if (ret < 0) {
        AJ_ErrPrintf(("MDnsRecvUp(): bind() failed. errno=\"%d\", status=AJ_ERR_READ\n", SOCK_getlasterror()));
        goto ExitError;
    }
    return recvSock;

ExitError:
    SOCK_close(recvSock);
    return INVALID_SOCKET;
}


#if defined(LITTLE_ENDIAN) || defined(PLATFORM_WINDOWS)
#define SOCK_ntohl(x) SOCK_htonl(x)
#else
#define SOCK_ntohl(x) ((UINT32)(x))
#endif


AJ_Status AJ_Net_MCastUp(AJ_MCastSocket* mcastSock)
{
    struct SOCK_sockaddr_in addrBuf;
    int addrLen =sizeof(SOCK_sockaddr_in);
    struct SOCK_sockaddr_in* sin;
    AJ_Status status = AJ_ERR_READ;

    AJ_InfoPrintf(("AJ_Net_MCastUp(castSock=0x%p)\n", mcastSock));
    if (rxDataMCast == NULL) rxDataMCast = (uint8_t *)AJ_Malloc(RXDATAMCASTSIZE);
    if (txDataMCast == NULL) txDataMCast = (uint8_t *)AJ_Malloc(TXDATAMCASTSIZE);

    mCastContext.mDnsRecvSock = MDnsRecvUp();
    if (mCastContext.mDnsRecvSock == INVALID_SOCKET) {
        AJ_ErrPrintf(("AJ_Net_MCastUp(): MDnsRecvUp for mDnsRecvPort failed"));
        return status;
    }
    if (SOCK_getsockname(mCastContext.mDnsRecvSock, (struct SOCK_sockaddr*) &addrBuf, &addrLen)) {
        AJ_ErrPrintf(("AJ_Net_MCastUp(): getsockname for mDnsRecvPort failed"));
        goto ExitError;
    }
    sin = (struct SOCK_sockaddr_in*) &addrBuf;
    mCastContext.mDnsRecvPort = SOCK_ntohs(sin->sin_port);

    mCastContext.mDnsRecvAddr = SOCK_ntohl(chooseMDnsRecvAddr());
    if (mCastContext.mDnsRecvAddr == 0) {
        AJ_ErrPrintf(("AJ_Net_MCastUp(): no mDNS recv address"));
        goto ExitError;
    }
    AJ_InfoPrintf(("AJ_Net_MCastUp(): mDNS recv on %d.%d.%d.%d:%d\n", ((mCastContext.mDnsRecvAddr >> 24) & 0xFF), ((mCastContext.mDnsRecvAddr >> 16) & 0xFF), ((mCastContext.mDnsRecvAddr >> 8) & 0xFF), (mCastContext.mDnsRecvAddr & 0xFF), mCastContext.mDnsRecvPort));

    mCastContext.mDnsSock = MCastUp4(MDNS_IPV4_MULTICAST_GROUP, 0);
#ifdef BUILD_IPV6
    mCastContext.mDns6Sock = MCastUp6(MDNS_IPV6_MULTICAST_GROUP, MDNS_UDP_PORT);
#endif
    if (AJ_GetMinProtoVersion() < 10) {
        mCastContext.udpSock = MCastUp4(AJ_IPV4_ADDR_MULTICAST_GROUP, 0);
#ifdef BUILD_IPV6
        mCastContext.udp6Sock = MCastUp6(AJ_IPV6_MULTICAST_GROUP, 0);
#endif
    }

    if ((txDataMCast != NULL) && (rxDataMCast != NULL ))
    {
	    if (   mCastContext.udpSock  != INVALID_SOCKET 
			|| mCastContext.mDnsSock != INVALID_SOCKET 

#ifdef BUILD_IPV6			
			|| mCastContext.udp6Sock  != INVALID_SOCKET 
			|| mCastContext.mDns6Sock != INVALID_SOCKET
#endif
		) {
	        AJ_IOBufInit(&mcastSock->rx, rxDataMCast, RXDATAMCASTSIZE, AJ_IO_BUF_RX, &mCastContext);
	        mcastSock->rx.recv = AJ_Net_RecvFrom;
	        AJ_IOBufInit(&mcastSock->tx, txDataMCast, TXDATAMCASTSIZE, AJ_IO_BUF_TX, &mCastContext);
	        mcastSock->tx.send = AJ_Net_SendTo;
	        status = AJ_OK;
	    }
	}
    return status;
ExitError:
	SOCK_close(mCastContext.mDnsRecvSock);
	return status;

}

void AJ_Net_MCastDown(AJ_MCastSocket* mcastSock)
{
    MCastContext* context = (MCastContext*) mcastSock->rx.context;
    AJ_InfoPrintf(("AJ_Net_MCastDown(nexSock=0x%p)\n", mcastSock));

    if (context->udpSock != INVALID_SOCKET) {

        struct SOCK_ip_mreq mreq;
        mreq.imr_interface.S_un.S_addr = SOCK_htonl(SOCK_INADDR_ANY);
        mreq.imr_multiaddr.S_un.S_addr = SOCK_htonl(AJ_IPV4_ADDR_MULTICAST_GROUP);
        SOCK_setsockopt(context->udpSock, SOCK_IPPROTO_IP, SOCK_IPO_DROP_MEMBERSHIP, (char*)&mreq, sizeof(mreq));
    }

#ifdef BUILD_IPV6
    if (context->udp6Sock != INVALID_SOCKET) {
        struct ipv6_mreq mreq6;
        inet_pton(AF_INET6, AJ_IPV6_MULTICAST_GROUP, &mreq6.ipv6mr_multiaddr);
        mreq6.ipv6mr_interface = 0;
        setsockopt(context->udp6Sock, IPPROTO_IPV6, IPV6_LEAVE_GROUP, &mreq6, sizeof(mreq6));
    }
#endif

    if (context->mDnsSock != INVALID_SOCKET) {
        struct SOCK_ip_mreq mreq;
        mreq.imr_interface.S_un.S_addr = SOCK_htonl(SOCK_INADDR_ANY);
        mreq.imr_multiaddr.S_un.S_addr = SOCK_htonl(MDNS_IPV4_MULTICAST_GROUP);
        SOCK_setsockopt(context->udpSock, SOCK_IPPROTO_IP, SOCK_IPO_DROP_MEMBERSHIP, (char*) &mreq, sizeof(mreq));
    }

#ifdef BUILD_IPV6
    if (context->mDns6Sock != INVALID_SOCKET) {
        struct ipv6_mreq mreq6;
        inet_pton(AF_INET6, MDNS_IPV6_MULTICAST_GROUP, &mreq6.ipv6mr_multiaddr);
        mreq6.ipv6mr_interface = 0;
        setsockopt(context->udp6Sock, IPPROTO_IPV6, IPV6_LEAVE_GROUP, &mreq6, sizeof(mreq6));
    }
#endif
    CloseMCastSock(mcastSock);

    AJ_Free(rxDataMCast);
    AJ_Free(txDataMCast);
    rxDataMCast = NULL;
    txDataMCast = NULL;

}

AJ_Status AJ_AcquireIPAddress(uint32_t* ip, uint32_t* mask, uint32_t* gateway, int32_t timeout)
{
    AJ_InfoPrintf(("AJ_AcquireIPAddress(ip=0x%p, mask=0x%p, gateway=0x%p, timeout=%d)\n", ip, mask, gateway, timeout));
    return AJ_OK;
}

// AJ_ARDP_UDP is not tested, the v15.04 official source code is bogus
// it has syntax error.
// the following code is added for future use and it is not tested due to the 
// error of the aj source code
#ifdef AJ_ARDP

static AJ_Status AJ_ARDP_UDP_Send(void* context, uint8_t* buf, size_t len, size_t* sent)
{
    AJ_Status status = AJ_OK;
    size_t ret;

    AJ_InfoPrintf(("AJ_ARDP_UDP_Send(buf=0x%p, len=%lu)\n", buf, len));

    // we can send( rather than sendto( because we did a UDP connect()
    // may send less that the len size, do we need to loop until all data send ??/
    ret = SOCK_send(ctx->udpSock, (const char *)buf, len, 0);
    if (ret == -1) {
        status = AJ_ERR_WRITE;
    } else {
        *sent = (size_t) ret;
    }

    return status;
}

static AJ_Status AJ_ARDP_UDP_Recv(void* context, uint8_t** data, uint32_t* recved, uint32_t timeout)
{
    SOCK_fd_set fds;
    struct SOCK_timeval tv = { timeout / 1000, 1000 * (timeout % 1000) };

    int ret;
    NetContext* ctx = (NetContext*) context;
    int maxFd = max(ctx->udpSock, interruptFd);

    /**
     * Let the platform code own this buffer.  This makes it easier to avoid double-buffering
     * on platforms that allow it.
     */
    static uint8_t buffer[UDP_SEGBMAX];

    *data = NULL;

    AJ_InfoPrintf(("AJ_ARDP_UDP_Recv(data=0x%p, recved=0x%p, timeout=%u)\n", data, recved, timeout));

    SOCK_FD_ZERO(&fds);
    SOCK_FD_SET(ctx->udpSock, &fds);
    if (interruptFd > 0) {
        SOCK_FD_SET(interruptFd, &fds);
    }

    blocked = TRUE;
    ret = SOCK_select(maxFd + 1, &fds, NULL, NULL, &tv);
    blocked = FALSE;

    if (ret == 0) {
        // timeout!
        return AJ_ERR_TIMEOUT;
    } else if (ret == -1) {
        AJ_ErrPrintf(("AJ_ARDP_UDP_Recv(): Error read (select)\n"));
        return AJ_ERR_READ;
/*
    } else if ((interruptFd > 0) && FD_ISSET(interruptFd, &fds)) {
        uint64_t u64;
        read(interruptFd, &u64, sizeof(u64));
        return AJ_ERR_INTERRUPTED;
*/        
    } else if (SOCK_FD_ISSET(ctx->udpSock, &fds)) {
        ret = SOCK_recvfrom(ctx->udpSock, (char *)buffer, sizeof(buffer), 0, NULL, 0);

        if (ret == -1) {
            // this will only happen if we are on a local machine
//            perror("recvfrom");
            AJ_ErrPrintf(("AJ_ARDP_UDP_Recv(): Error read (recvFrom)\n"));

            return AJ_ERR_READ;
        }

        *recved = ret;
        *data = buffer;
    }

    return AJ_OK;
}

static AJ_Status AJ_Net_ARDP_Connect(AJ_BusAttachment* bus, const AJ_Service* service)
{
    int udpSock = INVALID_SOCKET;
    AJ_Status status;
    struct SOCK_sockaddr_in addrBuf;
    int addrSize;
    int ret;

    AJ_ARDP_InitFunctions(AJ_ARDP_UDP_Recv, AJ_ARDP_UDP_Send);

    // otherwise backpressure is guaranteed!
    ASSERT(sizeof(txData) <= UDP_SEGMAX * (UDP_SEGBMAX - ARDP_HEADER_SIZE - UDP_HEADER_SIZE));

    memset(&addrBuf, 0, sizeof(addrBuf));
/*
    interruptFd = eventfd(0, O_NONBLOCK);  // Use O_NONBLOCK instead of EFD_NONBLOCK due to bug in OpenWrt's uCLibc
    if (interruptFd < 0) {
        AJ_ErrPrintf(("AJ_Net_ARDP_Connect(): failed to created interrupt event\n"));
        goto ConnectError;
    }
*/
    if (service->addrTypes & AJ_ADDR_UDP4) {
        struct SOCK_sockaddr_in* sa = (struct SOCK_sockaddr_in*) &addrBuf;
        udpSock = SOCK_socket(SOCK_AF_INET, SOCK_SOCK_DGRAM, SOCK_IPPROTO_UDP);
        if (udpSock == INVALID_SOCKET) {
            AJ_ErrPrintf(("AJ_Net_ARDP_Connect(): socket() failed.  status=AJ_ERR_CONNECT\n"));
            goto ConnectError;
        }

        sa->sin_family = SOCK_AF_INET;
        sa->sin_port = SOCK_htons(service->ipv4portUdp);
        sa->sin_addr.S_un.S_addr= service->ipv4Udp;
        addrSize = sizeof(struct SOCK_sockaddr_in);
        AJ_InfoPrintf(("AJ_Net_ARDP_Connect(): Connect to \"%x:%u\"\n", sa->sin_addr, service->ipv4portUdp));;
    } else if (service->addrTypes & AJ_ADDR_UDP6) {
#ifdef BUILD_IPV6
        struct sockaddr_in6* sa = (struct sockaddr_in6*) &addrBuf;
        udpSock = socket(AF_INET6, SOCK_DGRAM, 0);
        if (udpSock == INVALID_SOCKET) {
            AJ_ErrPrintf(("AJ_Net_ARDP_Connect(): socket() failed.  status=AJ_ERR_CONNECT\n"));
            goto ConnectError;            
        }

        sa->sin6_family = SOCK_AF_INET6;
        sa->sin6_port = htons(service->ipv6portUdp);
        memcpy(sa->sin6_addr.s6_addr, service->ipv6Udp, sizeof(sa->sin6_addr.s6_addr));
        addrSize = sizeof(struct sockaddr_in6);
#endif        
    } else {
        AJ_ErrPrintf(("AJ_Net_ARDP_Connect(): Invalid addrTypes %u, status=AJ_ERR_CONNECT\n", service->addrTypes));
        return AJ_ERR_CONNECT;
    }

    // When you 'connect' a UDP socket, it means that this is the default sendto address.
    // Therefore, we don't have to make the address a global variable and can
    // simply use send() rather than sendto().  See: man 7 udp
    ret = SOCK_connect(udpSock, (struct SOCK_sockaddr*) &addrBuf, addrSize);

    // must do this before calling AJ_MarshalMethodCall!
    if (ret == 0) {
        netContext.udpSock = udpSock;
        AJ_IOBufInit(&bus->sock.rx, rxData, sizeof(uint8_t)*RXDATASIZE, AJ_IO_BUF_RX, &netContext);
        bus->sock.rx.recv = AJ_ARDP_Recv;
        AJ_IOBufInit(&bus->sock.tx, txData, sizeof(uint8_t)*TXDATASIZE, AJ_IO_BUF_TX, &netContext);
        bus->sock.tx.send = AJ_ARDP_Send;
    } else {
        AJ_ErrPrintf(("AJ_Net_ARDP_Connect(): Error connecting\n"));
        goto ConnectError;
    }

    status = AJ_ARDP_UDP_Connect(bus, &netContext, service, &bus->sock);
    if (status != AJ_OK) {
        AJ_Net_ARDP_Disconnect(&bus->sock);
        goto ConnectError;
    }

    return AJ_OK;

ConnectError:
    if (interruptFd != INVALID_SOCKET) {
        SOCK_close(interruptFd);
        interruptFd = INVALID_SOCKET;
    }

    if (udpSock != INVALID_SOCKET) {
        SOCK_close(udpSock);
    }

    return AJ_ERR_CONNECT;
}

static void AJ_Net_ARDP_Disconnect(AJ_NetSocket* netSock)
{
    AJ_ARDP_Disconnect(FALSE);

    SOCK_close(netContext.udpSock);
    netContext.udpSock = INVALID_SOCKET;
    memset(netSock, 0, sizeof(AJ_NetSocket));
}

#endif // AJ_ARDP

