/**
 * @file
 */
/******************************************************************************
 * Copyright (c) 2012-2014, AllSeen Alliance. All rights reserved.
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
// uncomment the following lines for debug messages
//#undef  NDEBUG
//#define AJ_DEBUG_RESTRICT AJ_DEBUG_WARN // AJ_DEBUG_INFO
//#define AJ_PRINTF   1

#include <tinyhal.h>

#include "aj_target.h"
#include "aj_bufio.h"
#include "aj_net.h"
#include "aj_util.h"
#include "aj_debug.h"

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


/**
 * Target-specific context for network I/O
 */
typedef struct {
    int tcpSock;
    int udpSock;
    int udp6Sock;
} NetContext;

static NetContext netContext = { INVALID_SOCKET, INVALID_SOCKET, INVALID_SOCKET };

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
        if (context->udpSock != INVALID_SOCKET) {
            SOCK_close(context->udpSock);
        }
        if (context->udp6Sock != INVALID_SOCKET) {
            SOCK_close(context->udp6Sock);
        }

        context->tcpSock = context->udpSock = context->udp6Sock = INVALID_SOCKET;
        memset(netSock, 0, sizeof(AJ_NetSocket));
    }
    return AJ_OK;
}

AJ_Status AJ_Net_Send(AJ_IOBuffer* buf)
{
    NetContext* context = (NetContext*) buf->context;
    int ret;
    size_t tx = AJ_IO_BUF_AVAIL(buf);

    AJ_InfoPrintf(("AJ_Net_Send(buf=0x%p)\n", buf));

    if (tx > 0) {
        do {
            ret = SOCK_send(context->tcpSock, (const char *)buf->readPtr, tx, 0);
        } while ((ret == -1) && (SOCK_getlasterror() == SOCK_EWOULDBLOCK));
        if (ret == -1) {
            AJ_ErrPrintf(("AJ_Net_Send(): send() failed. errno=%d, status=AJ_ERR_WRITE\n", SOCK_getlasterror()));
            return AJ_ERR_WRITE;
        }
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

    //assert(buf->direction == AJ_IO_BUF_RX);

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
        if ((ret == -1) || (ret == 0)) {
            AJ_ErrPrintf(("AJ_Net_Recv(): recv() failed. errno=%d status=AJ_ERR_READ\n", SOCK_getlasterror()));
            status = AJ_ERR_READ;
        } else {
            buf->writePtr += ret;
        }
    }
    AJ_InfoPrintf(("AJ_Net_Recv(): status=%s\n", AJ_StatusText(status)));
    return status;
}

#define RXDATASIZE 1024
#define TXDATASIZE 1500
static uint8_t *rxData = NULL;
static uint8_t *txData = NULL;

AJ_Status AJ_Net_Connect(AJ_NetSocket* netSock, uint16_t port, uint8_t addrType, const uint32_t* addr)
{
    int ret;
    struct SOCK_sockaddr_in addrBuf;
    int addrSize;
    int tcpSock = INVALID_SOCKET;
    INT32 nonblocking = 1;
    INT32 optval = 1;
    INT32 optLinger = 0;

    AJ_InfoPrintf(("AJ_Net_Connect(netSock=0x%p, port=%d, addrType=%d, addr=0x%p)\n", netSock, port, addrType, addr));


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
    if (addrType == AJ_ADDR_IPV4) {
        struct SOCK_sockaddr_in* sa = (struct SOCK_sockaddr_in*)&addrBuf;
        sa->sin_family = SOCK_AF_INET;
        sa->sin_port = SOCK_htons(port);
        sa->sin_addr.S_un.S_addr = *addr;
        addrSize = sizeof(struct SOCK_sockaddr_in);
        AJ_InfoPrintf(("AJ_Net_Connect(): Connect to %x:%u\n", sa->sin_addr, port));
    } else {
#ifdef BUILD_IPV6
        struct sockaddr_in6* sa = (struct sockaddr_in6*)&addrBuf;
        sa->sin6_family = AF_INET6;
        sa->sin6_port = htons(port);
        memcpy(sa->sin6_addr.s6_addr, addr, sizeof(sa->sin6_addr.s6_addr));
        addrSize = sizeof(struct sockaddr_in6);
#else
        AJ_ErrPrintf(("AJ_Net_Connect(): socket() failed.  status=AJ_ERR_CONNECT\n"));
        goto ConnectError;
#endif
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
        AJ_IOBufInit(&netSock->rx, rxData, sizeof(uint8_t)*RXDATASIZE, AJ_IO_BUF_RX, &netContext);
        netSock->rx.recv = AJ_Net_Recv;
        AJ_IOBufInit(&netSock->tx, txData, sizeof(uint8_t)*TXDATASIZE, AJ_IO_BUF_TX, &netContext);
        netSock->tx.send = AJ_Net_Send;
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

}

static void sendToBroadcast(int sock, void* ptr, size_t tx)
{
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
            sin_bcast->sin_port = SOCK_htons(AJ_UDP_PORT);
            AJ_InfoPrintf(("sendToBroadcast: sending to bcast addr %d.%d.%d.%d\n", 
                sin_bcast->sin_addr.S_un.S_un_b.s_b1,
                sin_bcast->sin_addr.S_un.S_un_b.s_b2,
                sin_bcast->sin_addr.S_un.S_un_b.s_b3,
                sin_bcast->sin_addr.S_un.S_un_b.s_b4
                ));
            SOCK_sendto(sock, (const char *)ptr, tx, 0/*MSG_NOSIGNAL*/, (struct SOCK_sockaddr*) sin_bcast, sizeof(struct SOCK_sockaddr_in));
        }
        addr = addr->ai_next;
    }
    SOCK_freeaddrinfo(addrs);
}

AJ_Status AJ_Net_SendTo(AJ_IOBuffer* buf)
{
    int ret = -1;
    uint8_t sendSucceeded = FALSE;
    size_t tx = AJ_IO_BUF_AVAIL(buf);
    NetContext* context = (NetContext*) buf->context;
    AJ_InfoPrintf(("AJ_Net_SendTo(buf=0x%p, buf->readPtr=0x%p, size tx=%d)\n", buf, buf->readPtr, tx));

    if (tx > 0) {
        if (context->udpSock != INVALID_SOCKET) {
            struct SOCK_sockaddr_in sin;
            sin.sin_family = SOCK_AF_INET;
            sin.sin_port = SOCK_htons(AJ_UDP_PORT);
            sin.sin_addr.S_un.S_addr = SOCK_htonl(AJ_IPV4_ADDR_MULTICAST_GROUP);
            ret = SOCK_sendto(context->udpSock, (const char *)buf->readPtr, tx, 0 /*MSG_NOSIGNAL*/, (struct SOCK_sockaddr*)&sin, sizeof(sin));
            if (tx == ret) {
                sendSucceeded = TRUE;
            }
            sendToBroadcast(context->udpSock, buf->readPtr, tx);
        }

#ifdef BUILD_IPV6
        // now sendto the ipv6 address
        if (context->udp6Sock != INVALID_SOCKET) {
            struct sockaddr_in6 sin6;
            sin6.sin6_family = AF_INET6;
            sin6.sin6_flowinfo = 0;
            sin6.sin6_scope_id = 0;
            sin6.sin6_port = htons(AJ_UDP_PORT);
            if (inet_pton(AF_INET6, AJ_IPV6_MULTICAST_GROUP, &sin6.sin6_addr) == 1) {
                ret = sendto(context->udp6Sock, buf->readPtr, tx, MSG_NOSIGNAL, (struct sockaddr*) &sin6, sizeof(sin6));
                if (tx == ret) {
                    sendSucceeded = TRUE;
                }
            } else {
                AJ_ErrPrintf(("AJ_Net_SendTo(): Invalid address IP address. errno=\"%s\"", strerror(errno)));
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
    NetContext* context = (NetContext*) buf->context;
    AJ_Status status = AJ_OK;
    int ret;
    size_t rx;
    SOCK_fd_set fds;
    int maxFd = INVALID_SOCKET;
    int rc = 0;
    struct SOCK_timeval tv = { timeout / 1000, 1000 * (timeout % 1000) };

    AJ_InfoPrintf(("AJ_Net_RecvFrom(buf=0x%p, len=%d., timeout=%d.)\n", buf, len, timeout));

    SOCK_FD_ZERO(&fds);
    if (context->udpSock != INVALID_SOCKET) {
        SOCK_FD_SET(context->udpSock, &fds);
        maxFd = context->udpSock;
    }

#ifdef BUILD_IPV6
    if (context->udp6Sock != INVALID_SOCKET) {
        SOCK_FD_SET(context->udp6Sock, &fds);
    }
#endif

    maxFd = max(context->udp6Sock, context->udpSock);

    rc = SOCK_select(maxFd + 1, &fds, NULL, NULL, &tv);
    if (rc == 0) {
        AJ_InfoPrintf(("AJ_Net_RecvFrom(): select() timed out. status=AJ_ERR_TIMEOUT\n"));
        return AJ_ERR_TIMEOUT;
    }

    // we need to read from whichever socket has data availble.
    // if both sockets are ready, read from both in order to
    // reset the state

#ifdef BUILD_IPV6
    rx = AJ_IO_BUF_SPACE(buf);
    if (context->udp6Sock != INVALID_SOCKET && FD_ISSET(context->udp6Sock, &fds)) {
        rx = min(rx, len);
        if (rx) {
            ret = recvfrom(context->udp6Sock, buf->writePtr, rx, 0, NULL, 0);
            if (ret == -1) {
                AJ_ErrPrintf(("AJ_Net_RecvFrom(): recvfrom() failed. errno=\"%s\", status=AJ_ERR_READ\n", strerror(errno)));
                status = AJ_ERR_READ;
            } else {
                buf->writePtr += ret;
                status = AJ_OK;
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
                buf->writePtr += ret;
                status = AJ_OK;
            }
        }
    }

    AJ_InfoPrintf(("AJ_Net_RecvFrom(): status=%s\n", AJ_StatusText(status)));
    return status;
}

/*
 * Need enough space to receive a complete name service packet when used in UDP
 * mode.  NS expects MTU of 1500 subtracts UDP, IP and ethertype overhead.
 * 1500 - 8 -20 - 18 = 1454.  txData buffer size needs to be big enough to hold
 * a NS WHO-HAS for one name (4 + 2 + 256 = 262)
 */

#define RXDATAMCASTSIZE 1454
#define TXDATAMCASTSIZE 262

static uint8_t *rxDataMCast = NULL;
static uint8_t *txDataMCast = NULL;

static int MCastUp4()
{
    int ret = 0;
    SOCK_sockaddr_in sin;
    int reuse = 1;
    int bcast = 1;
    SOCK_SOCKET mcastSock = INVALID_SOCKET;

    memset(&sin, 0, sizeof(sin));

    AJ_InfoPrintf(("MCastUp4()\n"));

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
    * Join our multicast group
    */
    SOCK_ip_mreq mreq;
    mreq.imr_interface.S_un.S_addr = SOCK_htonl(SOCK_INADDR_ANY);
    mreq.imr_multiaddr.S_un.S_addr = SOCK_htonl(AJ_IPV4_ADDR_MULTICAST_GROUP);
    ret = SOCK_setsockopt(mcastSock, SOCK_IPPROTO_IP, SOCK_IPO_ADD_MEMBERSHIP, (const char *)&mreq, sizeof(mreq));
    if (ret != 0) {
        ret = SOCK_getlasterror();
        AJ_ErrPrintf(("MCastUp4(): setsockopt(IP_ADD_MEMBERSHIP) failed. errno=%d, status=AJ_ERR_READ\n", SOCK_getlasterror()));
        goto ExitError;
    }

    /*
     * Bind an ephemeral port
     */
    sin.sin_family = SOCK_AF_INET;
    sin.sin_port = SOCK_htons(0);
    sin.sin_addr.S_un.S_addr = SOCK_htonl(SOCK_INADDR_ANY);
    ret = SOCK_bind(mcastSock, (const SOCK_sockaddr *)&sin, sizeof(sin));
    if (ret < 0) {
        AJ_ErrPrintf(("MCastUp4(): bind() failed. errno=%d, status=AJ_ERR_READ\n", SOCK_getlasterror()));
        goto ExitError;
    }

    return mcastSock;

ExitError:
    SOCK_close(mcastSock);
    return INVALID_SOCKET;
}

#ifdef BUILD_IPV6
static int MCastUp6()
{
    int ret;
    struct ipv6_mreq mreq6;
    struct sockaddr_in6 sin6;
    int reuse = 1;
    int mcastSock;

    AJ_InfoPrintf(("MCastUp6()\n"));

    mcastSock = socket(AF_INET6, SOCK_DGRAM, 0);
    if (mcastSock == INVALID_SOCKET) {
        AJ_ErrPrintf(("MCastUp6(): socket() fails. errno=\"%s\" status=AJ_ERR_READ\n", strerror(errno)));
        return INVALID_SOCKET;
    }

    ret = setsockopt(mcastSock, SOL_SOCKET, SO_REUSEADDR, &reuse, sizeof(reuse));
    if (ret != 0) {
        AJ_ErrPrintf(("MCastUp6(): setsockopt(SO_REUSEADDR) failed. errno=\"%s\", status=AJ_ERR_READ\n", strerror(errno)));
        goto ExitError;
    }

    /*
     * Bind an ephemeral port
     */
    sin6.sin6_family = AF_INET6;
    sin6.sin6_port = htons(0);
    sin6.sin6_addr = in6addr_any;
    ret = bind(mcastSock, (struct sockaddr*) &sin6, sizeof(sin6));
    if (ret < 0) {
        AJ_ErrPrintf(("MCastUp6(): bind() failed. errno=\"%s\", status=AJ_ERR_READ\n", strerror(errno)));
        goto ExitError;
    }

    /*
     * Join our multicast group
     */
    inet_pton(AF_INET6, AJ_IPV6_MULTICAST_GROUP, &mreq6.ipv6mr_multiaddr);
    mreq6.ipv6mr_interface = 0;
    ret = setsockopt(mcastSock, IPPROTO_IPV6, IPV6_JOIN_GROUP, &mreq6, sizeof(mreq6));
    if (ret < 0) {
        AJ_ErrPrintf(("MCastUp6(): setsockopt(IP_ADD_MEMBERSHIP) failed. errno=\"%s\", status=AJ_ERR_READ\n", strerror(errno)));
        goto ExitError;
    }

    return mcastSock;

ExitError:
    close(mcastSock);
    return INVALID_SOCKET;
}
#endif


AJ_Status AJ_Net_MCastUp(AJ_NetSocket* netSock)
{
    AJ_Status status = AJ_ERR_READ;
    AJ_InfoPrintf(("AJ_Net_MCastUp(nexSock=0x%p)\n", netSock));

    if (rxDataMCast == NULL) rxDataMCast = (uint8_t *)AJ_Malloc(RXDATAMCASTSIZE);
    if (txDataMCast == NULL) txDataMCast = (uint8_t *)AJ_Malloc(TXDATAMCASTSIZE);

    netContext.udpSock = MCastUp4();
#ifdef BUILD_IPV6
    netContext.udp6Sock = MCastUp6();
#endif

    if (txDataMCast != NULL && 
        rxDataMCast != NULL &&
        (netContext.udpSock != INVALID_SOCKET
#ifdef BUILD_IPV6
        || netContext.udp6Sock != INVALID_SOCKET
#endif
        )) {
        AJ_IOBufInit(&netSock->rx, rxDataMCast, RXDATAMCASTSIZE, AJ_IO_BUF_RX, &netContext);
        netSock->rx.recv = AJ_Net_RecvFrom;
        AJ_IOBufInit(&netSock->tx, txDataMCast, TXDATAMCASTSIZE, AJ_IO_BUF_TX, &netContext);
        netSock->tx.send = AJ_Net_SendTo;
        status = AJ_OK;
    }

    return status;
}

void AJ_Net_MCastDown(AJ_NetSocket* netSock)
{
    NetContext* context = (NetContext*) netSock->rx.context;
    AJ_InfoPrintf(("AJ_Net_MCastDown(nexSock=0x%p)\n", netSock));

    if (context->udpSock != INVALID_SOCKET) {
        /*
         * Leave our multicast group
         */
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

    CloseNetSock(netSock);

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
