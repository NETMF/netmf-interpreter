/**
 * @file Alljoyn network function implementations
 */
/******************************************************************************
 * Copyright (c) 2014, AllSeen Alliance. All rights reserved.
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

#include <stdio.h>
#include <stdlib.h>
#include <assert.h>
#include <errno.h>
#include <time.h>

#include "aj_target.h"
#include "aj_bufio.h"
#include "aj_net.h"
#include "aj_util.h"
#include "aj_debug.h"
#include "aj_wsl_net.h"
#include "aj_wsl_wmi.h"
/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgNET = 0;
#endif

/*
 * IANA assigned IPv4 multicast group for AllJoyn.
 */
static const char AJ_IPV4_MULTICAST_GROUP[] = "224.0.0.113";
#define AJ_IPV4_MCAST_GROUP                   0xe0000071

/*
 * IANA assigned IPv6 multicast group for AllJoyn.
 */
static const char AJ_IPV6_MULTICAST_GROUP[] = "ff02::13a";
static uint8_t AJ_IPV6_MCAST_GROUP[16] = { 0xff, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x3a };

/*
 * IANA assigned UDP multicast port for AllJoyn
 */
#define AJ_UDP_PORT 9956

/*
 * IANA-assigned IPv4 multicast group for mDNS.
 */
static const char MDNS_IPV4_MULTICAST_GROUP[] = "224.0.0.251";
#define MDNS_IPV4_MCAST_GROUP                   0xe00000fb

/*
 * IANA-assigned IPv6 multicast group for mDNS.
 */
static const char MDNS_IPV6_MULTICAST_GROUP[] = "ff02::fb";
static uint8_t MDNS_IPV6_MCAST_GROUP[16] = { 0xff, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xfb };

/*
 * IANA-assigned UDP multicast port for mDNS
 */
#define MDNS_UDP_PORT 5353

/**
 * Target-specific context for network I/O
 */
typedef struct {
    int tcpSock;
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

/*
 * Current socket thats blocked inside select
 */
static int selectSock;
static NetContext netContext = { INVALID_SOCKET };
static MCastContext mCastContext = { INVALID_SOCKET, INVALID_SOCKET, INVALID_SOCKET, INVALID_SOCKET, INVALID_SOCKET, 0, 0 };

AJ_Status AJ_IntToString(int32_t val, char* buf, size_t buflen)
{
    AJ_Status status = AJ_OK;
    int c = snprintf(buf, buflen, "%d", val);
    if (c <= 0 || c > buflen) {
        status = AJ_ERR_RESOURCES;
    }
    return status;
}

AJ_Status AJ_InetToString(uint32_t addr, char* buf, size_t buflen)
{
    AJ_Status status = AJ_OK;
    int c = snprintf(buf, buflen, "%u.%u.%u.%u", (addr & 0xFF000000) >> 24, (addr & 0x00FF0000) >> 16, (addr & 0x0000FF00) >> 8, (addr & 0x000000FF));
    if (c <= 0 || c > buflen) {
        status = AJ_ERR_RESOURCES;
    }
    return status;
}

/*
 * Call this function from an interrupt context to unblock a select call
 * This only has an effect if select is in a blocking state, any other blocking
 * calls will be unaffected by this call
 */
void AJ_Net_Interrupt(void)
{
    AJ_WSL_NET_signal_interrupted(selectSock);
}

static AJ_Status CloseNetSock(AJ_NetSocket* netSock)
{
    NetContext* context = (NetContext*)netSock->rx.context;
    if (context) {
        if (context->tcpSock != INVALID_SOCKET) {
            AJ_WSL_NET_socket_close(context->tcpSock);
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
            AJ_WSL_NET_socket_close(context->udpSock);
        }
        if (context->udp6Sock != INVALID_SOCKET) {
            AJ_WSL_NET_socket_close(context->udp6Sock);
        }
        if (context->mDnsSock != INVALID_SOCKET) {
            AJ_WSL_NET_socket_close(context->mDnsSock);
        }
        if (context->mDns6Sock != INVALID_SOCKET) {
            AJ_WSL_NET_socket_close(context->mDns6Sock);
        }
        if (context->mDnsRecvSock != INVALID_SOCKET) {
            AJ_WSL_NET_socket_close(context->mDnsRecvSock);
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

    assert(buf->direction == AJ_IO_BUF_TX);

    if (tx > 0) {
        ret = AJ_WSL_NET_socket_send(context->tcpSock, buf->readPtr, tx, 0);
        if (ret == -1) {
            AJ_ErrPrintf(("AJ_Net_Send(): send() failed. errno=\"%s\", status=AJ_ERR_WRITE\n", strerror(errno)));
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

AJ_Status AJ_Net_Recv(AJ_IOBuffer* buf, uint32_t len, uint32_t timeout)
{
    AJ_Status status = AJ_OK;
    int16_t ret;
    AJ_Time timer;
    NetContext* context = (NetContext*) buf->context;
    size_t rx = AJ_IO_BUF_SPACE(buf);

    AJ_InfoPrintf(("AJ_Net_Recv(buf=0x%p, len=%ld., timeout=%ld.)\n", buf, len, timeout));

    assert(buf->direction == AJ_IO_BUF_RX);
    selectSock = context->tcpSock;
    AJ_InitTimer(&timer);
    ret = AJ_WSL_NET_socket_select(context->tcpSock, timeout);
    if (ret == -2) {
        // Select timed out
        return AJ_ERR_TIMEOUT;
    } else if (ret == -1) {
        // We were interrupted
        return AJ_ERR_INTERRUPTED;
    } else if (ret == 0) {
        // The socket was closed
        return AJ_ERR_READ;
    }
    // If we pass these checks there is data ready for receive
    timeout -= AJ_GetElapsedTime(&timer, TRUE);
    rx = min(rx, len);
    if (rx) {
        ret = AJ_WSL_NET_socket_recv(context->tcpSock, buf->writePtr, rx, timeout);
        if (ret == -1) {
            status = AJ_ERR_READ;
        } else if (ret == 0) {
            status = AJ_ERR_TIMEOUT;
        } else {
            buf->writePtr += ret;
        }
    }
//    AJ_InfoPrintf(("AJ_Net_Recv(): status=%s\n", AJ_StatusText(status)));

    return status;
}

static uint8_t rxData[1024];
static uint8_t txData[1500];

AJ_Status AJ_Net_Connect(AJ_NetSocket* netSock, uint16_t port, uint8_t addrType, const uint32_t* addr)
{
    int ret;

    AJ_InfoPrintf(("AJ_Net_Connect(netSock=0x%p, port=%d., addrType=%d., addr=0x%lx)\n", netSock, port, addrType, *addr));

    int tcpSock = AJ_WSL_NET_socket_open(WSL_AF_INET, WSL_SOCK_STREAM, 0);
    if (tcpSock == INVALID_SOCKET) {
        AJ_ErrPrintf(("AJ_Net_Connect(): socket() failed.  status=AJ_ERR_CONNECT\n"));
        return AJ_ERR_CONNECT;
    }
    if (addrType == AJ_ADDR_IPV4) {

    } else {
        //TODO: IPv6 connect. Alljoyn never uses IPv6 TCP but maybe in the future
    }
    ret = AJ_WSL_NET_socket_connect(tcpSock, BE32_TO_CPU(*addr), port, WSL_AF_INET);
    if (ret < 0) {
        //AJ_ErrPrintf(("AJ_Net_Connect(): connect() failed. errno=\"%s\", status=AJ_ERR_CONNECT\n", strerror(errno)));
        return AJ_ERR_CONNECT;
    } else {
        netContext.tcpSock = tcpSock;
        AJ_IOBufInit(&netSock->rx, rxData, sizeof(rxData), AJ_IO_BUF_RX, &netContext);
        netSock->rx.recv = AJ_Net_Recv;
        AJ_IOBufInit(&netSock->tx, txData, sizeof(txData), AJ_IO_BUF_TX, &netContext);
        netSock->tx.send = AJ_Net_Send;
        AJ_InfoPrintf(("AJ_Net_Connect(): status=AJ_OK\n"));
        return AJ_OK;
    }

    return AJ_OK;
}

void AJ_Net_Disconnect(AJ_NetSocket* netSock)
{
    CloseNetSock(netSock);
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
    dataLength = 23 + 1 + sizeof(sid) + strlen(sidStr) + 1 + sizeof(ipv4) + strlen(ipv4Str) + 1 + sizeof(upcv4) + strlen(upcv4Str);
    *pkt++ = (dataLength >> 8) & 0xFF;
    *pkt++ = dataLength & 0xFF;

    // move forward past the static key-value pairs
    pkt += 23;

    // ASSERT: must be at the start of "sid="
    assert(*(pkt + 1) == 's');

    // re-write new values
    *pkt++ = sizeof(sid) + strlen(sidStr);
    memcpy(pkt, sid, sizeof(sid));
    pkt += sizeof(sid);
    memcpy(pkt, sidStr, strlen(sidStr));
    pkt += strlen(sidStr);

    *pkt++ = sizeof(ipv4) + strlen(ipv4Str);
    memcpy(pkt, ipv4, sizeof(ipv4));
    pkt += sizeof(ipv4);
    memcpy(pkt, ipv4Str, strlen(ipv4Str));
    pkt += strlen(ipv4Str);

    *pkt++ = sizeof(upcv4) + strlen(upcv4Str);
    memcpy(pkt, upcv4, sizeof(upcv4));
    pkt += sizeof(upcv4);
    memcpy(pkt, upcv4Str, strlen(upcv4Str));
    pkt += strlen(upcv4Str);

    buf->writePtr = pkt;

    return AJ_OK;
}

AJ_Status AJ_Net_SendTo(AJ_IOBuffer* buf)
{

    int ret;
    size_t tx = 0;
    MCastContext* context = (MCastContext*) buf->context;
    AJ_InfoPrintf(("AJ_Net_SendTo(buf=0x%p)\n", buf));
    assert(buf->direction == AJ_IO_BUF_TX);

    if (buf->flags & AJ_IO_BUF_AJ) {
        tx = AJ_IO_BUF_AVAIL(buf);
    }
    if (tx > 0) {
        // Send out IPv4 multicast
        if (context->udpSock != INVALID_SOCKET) {
            ret = AJ_WSL_NET_socket_sendto(context->udpSock, buf->readPtr, tx, BE32_TO_CPU(AJ_IPV4_MCAST_GROUP), AJ_UDP_PORT, 0);
        }
        // Send to the IPv6 address
        if (context->udp6Sock != INVALID_SOCKET) {
            ret = AJ_WSL_NET_socket_sendto6(context->udp6Sock, buf->readPtr, tx, AJ_IPV6_MULTICAST_GROUP, AJ_UDP_PORT, 0);
        }
    }

    tx = 0;
    if (buf->flags & AJ_IO_BUF_MDNS) {
        if (RewriteSenderInfo(buf, context->mDnsRecvAddr, context->mDnsRecvPort) == AJ_OK) {
            tx = AJ_IO_BUF_AVAIL(buf);
        } else {
            AJ_ErrPrintf(("AJ_Net_SendTo(): RewriteSenderInfo failed.\n"));
        }
    }
    if (tx > 0) {
        // Send out IPv4 multicast
        if (context->mDnsSock != INVALID_SOCKET) {
            ret = AJ_WSL_NET_socket_sendto(context->mDnsSock, buf->readPtr, tx, BE32_TO_CPU(MDNS_IPV4_MCAST_GROUP), MDNS_UDP_PORT, 0);
        }
        // Send to the IPv6 address
        if (context->mDns6Sock != INVALID_SOCKET) {
            ret = AJ_WSL_NET_socket_sendto6(context->mDns6Sock, buf->readPtr, tx, MDNS_IPV6_MULTICAST_GROUP, MDNS_UDP_PORT, 0);
        }
    }
    if (ret == -1) {
        AJ_ErrPrintf(("AJ_Net_SendTo(): sendto() failed. errno=\"%s\", status=AJ_ERR_WRITE\n", strerror(errno)));
        return AJ_ERR_WRITE;
    }
    AJ_IO_BUF_RESET(buf);
    AJ_InfoPrintf(("AJ_Net_SendTo(): status=AJ_OK\n"));

    return AJ_OK;
}

AJ_Status AJ_Net_RecvFrom(AJ_IOBuffer* buf, uint32_t len, uint32_t timeout)
{
    AJ_Status status = AJ_OK;
    int ret;

    MCastContext* context = (MCastContext*) buf->context;
    int sock = context->mDnsRecvSock;
    uint32_t poll = min(100, timeout / 2);
    size_t rx = AJ_IO_BUF_SPACE(buf);

    AJ_InfoPrintf(("AJ_Net_RecvFrom(buf=0x%p, len=%ld, timeout=%ld)\n", buf, len, timeout));

    assert(buf->direction == AJ_IO_BUF_RX);
    assert(context->mDnsRecvSock != INVALID_SOCKET);

    while (1) {

        ret = AJ_WSL_NET_socket_recv(sock, buf->writePtr, rx, poll);
        if (ret == -1) {
            AJ_ErrPrintf(("AJ_Net_RecvFrom(): Invalid socket: %d\n", ret));
            status = AJ_ERR_READ;
            break;
        }

        if (ret > 0) {
            if (sock == context->mDnsRecvSock) {
                buf->flags |= AJ_IO_BUF_MDNS;
            } else {
                buf->flags |= AJ_IO_BUF_AJ;
            }
            buf->writePtr += ret;
            break;
        }

        if (timeout < 100) {
            AJ_ErrPrintf(("AJ_Net_RecvFrom(): select() timed out.\n"));
            status = AJ_ERR_TIMEOUT;
            break;
        }

        // rotate to the next valid socket
        if (sock == context->mDnsRecvSock) {
            if (context->udpSock != INVALID_SOCKET) {
                sock = context->udpSock;
            } else if (context->udp6Sock != INVALID_SOCKET) {
                sock = context->udp6Sock;
            }
        } else if (sock == context->udpSock) {
            if (context->udp6Sock != INVALID_SOCKET) {
                sock = context->udp6Sock;
            } else {
                sock = context->mDnsRecvSock;
            }
        } else if (sock == context->udp6Sock) {
            sock = context->mDnsRecvSock;
        }

        timeout -= 100;
    }

    AJ_InfoPrintf(("AJ_Net_RecvFrom(): status=%s\n", AJ_StatusText(status)));

    return status;
}

/*
 * Need enough space to receive a complete name service packet when used in UDP
 * mode.  NS expects MTU of 1500 subtracts UDP, IP and ethertype overhead.
 * 1500 - 8 -20 - 18 = 1454.  txData buffer size needs to be big enough to hold
 * max(NS WHO-HAS for one name (4 + 2 + 256 = 262),
 *     mDNS query for one name (190 + 5 + 5 + 15 + 256 = 471)) = 471
 */
const uint16_t rxDataMCastSize = 1454;
const uint16_t txDataMCastSize = 471;

#ifndef SO_REUSEPORT
#define SO_REUSEPORT SO_REUSEADDR
#endif


static int MCastUp4(const char group[], uint16_t port)
{
    int ret;
    int mcastSock;

    AJ_InfoPrintf(("MCastUp4()\n"));

    /*
     * Open socket
     */
    mcastSock = AJ_WSL_NET_socket_open(WSL_AF_INET, WSL_SOCK_DGRAM, 0);
    if (mcastSock == INVALID_SOCKET) {
        AJ_ErrPrintf(("MCastUp4(): socket() fails. status=AJ_ERR_READ\n"));
        return INVALID_SOCKET;
    }

    /*
     * Bind port
     */
    ret = AJ_WSL_NET_socket_bind(mcastSock, 0x00000000, port);

    /*
     * Join multicast group
     */
    uint32_t optval[2] = { group, AJ_INADDR_ANY };
    ret = AJ_WSL_NET_set_sock_options(mcastSock, WSL_IPPROTO_IP, WSL_ADD_MEMBERSHIP, sizeof(optval), (uint8_t*)&optval);
    if (ret < 0) {
        AJ_ErrPrintf(("MCastUp4(): setsockopt(WSL_ADD_MEMBERSHIP) failed: %d. errno=\"%s\"\n", ret, strerror(errno)));
        AJ_WSL_NET_socket_close(mcastSock);
        return INVALID_SOCKET;
    }

    return mcastSock;

}

static int MCastUp6(const char group[], uint16_t port)
{
    int ret;
    int mcastSock;

    uint8_t gblAddr[16];
    uint8_t locAddr[16];
    uint8_t gwAddr[16];
    uint8_t gblExtAddr[16];
    uint32_t linkPrefix = 0;
    uint32_t glbPrefix = 0;
    uint32_t gwPrefix = 0;
    uint32_t glbExtPrefix = 0;
    uint16_t IP6_ADDR_ANY[8];
    memset(&IP6_ADDR_ANY, 0, 16);

    /*
     * We pass the current local IPv6 address into the sockopt for joining the multicast group.
     */
    AJ_WSL_ip6config(IPCONFIG_QUERY, (uint8_t*)&gblAddr, (uint8_t*)&locAddr, (uint8_t*)&gwAddr, (uint8_t*)&gblExtAddr, linkPrefix, glbPrefix, gwPrefix, glbExtPrefix);
    AJ_InfoPrintf(("Local Address:\n"));
    AJ_InfoPrintf(("%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x:%02x%02x\n",
                   locAddr[0], locAddr[1], locAddr[2], locAddr[3],
                   locAddr[4], locAddr[5], locAddr[6], locAddr[7],
                   locAddr[8], locAddr[9], locAddr[10], locAddr[11],
                   locAddr[12], locAddr[13], locAddr[14], locAddr[15]));

    mcastSock = AJ_WSL_NET_socket_open(WSL_AF_INET6, WSL_SOCK_DGRAM, 0);

    if (mcastSock == INVALID_SOCKET) {
        AJ_ErrPrintf(("MCastUp6(): socket() fails. status=AJ_ERR_READ\n"));
        return INVALID_SOCKET;
    }
    ret = AJ_WSL_NET_socket_bind6(mcastSock, (uint8_t*)&IP6_ADDR_ANY, port);

    uint8_t optval[32];
    memcpy(&optval, group, 16);
    memcpy(&optval[16], &locAddr, 16);

    ret = AJ_WSL_NET_set_sock_options(mcastSock, WSL_IPPROTO_IP, WSL_JOIN_GROUP, 32, (uint8_t*)&optval);
    if (ret < 0) {
        AJ_ErrPrintf(("MCastUp6(): setsockopt(WSL_JOIN_GROUP) failed. errno=\"%s\", status=AJ_ERR_READ\n", strerror(errno)));
        AJ_WSL_NET_socket_close(mcastSock);
        return INVALID_SOCKET;
    }

    return mcastSock;
}

static int MDnsRecvUp(uint16_t* port)
{
    AJ_Status status;
    uint16_t p;
    int ret;
    int mDnsRecvSock;

    /*
     * Open socket
     */
    mDnsRecvSock = AJ_WSL_NET_socket_open(WSL_AF_INET, WSL_SOCK_DGRAM, 0);
    if (mDnsRecvSock == INVALID_SOCKET) {
        AJ_ErrPrintf(("MCastUp4(): socket() failed. status=AJ_ERR_READ\n"));
        return INVALID_SOCKET;
    }

    /*
     * Bind ephemeral port
     */
    p = AJ_EphemeralPort();
    status = AJ_WSL_NET_socket_bind(mDnsRecvSock, 0x00000000, p);
    if (status != AJ_OK) {
        AJ_ErrPrintf(("MDnsRecvUp(): bind() failed: %d. errno=\"%s\", status=AJ_ERR_READ\n", ret, strerror(errno)));
        goto ExitError;
    }
    *port = p;
    return mDnsRecvSock;

ExitError:
    AJ_WSL_NET_socket_close(mDnsRecvSock);
    return INVALID_SOCKET;
}

AJ_Status AJ_Net_MCastUp(AJ_MCastSocket* mcastSock)
{
    AJ_Status status = AJ_ERR_READ;
    uint32_t ip;
    uint32_t mask;
    uint32_t gateway;
    uint16_t port;

    mCastContext.mDnsRecvSock = MDnsRecvUp(&port);
    if (mCastContext.mDnsRecvSock == INVALID_SOCKET) {
        AJ_ErrPrintf(("AJ_Net_MCastUp(): MDnsRecvUp for mDnsRecvPort failed"));
        return status;
    }
    mCastContext.mDnsRecvPort = port;
    if (AJ_GetIPAddress(&ip, &mask, &gateway) != AJ_OK) {
        AJ_ErrPrintf(("AJ_Net_MCastUp(): no IP address"));
        goto ExitError;
    }
    mCastContext.mDnsRecvAddr = ip;
    if (mCastContext.mDnsRecvAddr == 0) {
        AJ_ErrPrintf(("AJ_Net_MCastUp(): no mDNS recv address"));
        goto ExitError;
    }
    AJ_InfoPrintf(("AJ_Net_MCastUp(): mDNS recv on %d.%d.%d.%d:%d\n", ((mCastContext.mDnsRecvAddr & 0xFF) >> 24), ((mCastContext.mDnsRecvAddr >> 16) & 0xFF), ((mCastContext.mDnsRecvAddr >> 8) & 0xFF), (mCastContext.mDnsRecvAddr & 0xFF), mCastContext.mDnsRecvPort));

    mCastContext.mDnsSock = MCastUp4(MDNS_IPV4_MCAST_GROUP, MDNS_UDP_PORT);
    mCastContext.mDns6Sock = MCastUp6(MDNS_IPV6_MCAST_GROUP, MDNS_UDP_PORT);
    if (AJ_GetMinProtoVersion() < 10) {
        mCastContext.udpSock = MCastUp4(AJ_IPV4_MCAST_GROUP, AJ_UDP_PORT);
        mCastContext.udp6Sock = MCastUp6(AJ_IPV4_MCAST_GROUP, AJ_UDP_PORT);
    }

    if (mCastContext.udpSock != INVALID_SOCKET || mCastContext.udp6Sock != INVALID_SOCKET ||
        mCastContext.mDnsSock != INVALID_SOCKET || mCastContext.mDns6Sock != INVALID_SOCKET) {
        uint8_t* rxDataMCast = NULL;
        uint8_t* txDataMCast = NULL;

        rxDataMCast = (uint8_t*)AJ_Malloc(rxDataMCastSize);
        txDataMCast = (uint8_t*)AJ_Malloc(txDataMCastSize);
        if (!rxDataMCast || !txDataMCast) {
            return AJ_ERR_UNEXPECTED;
        }

        AJ_IOBufInit(&mcastSock->rx, rxDataMCast, rxDataMCastSize, AJ_IO_BUF_RX, &mCastContext);
        mcastSock->rx.recv = AJ_Net_RecvFrom;
        AJ_IOBufInit(&mcastSock->tx, txDataMCast, txDataMCastSize, AJ_IO_BUF_TX, &mCastContext);
        mcastSock->tx.send = AJ_Net_SendTo;
        status = AJ_OK;
    }
    return status;

ExitError:
    AJ_WSL_NET_socket_close(mCastContext.mDnsRecvSock);
    return status;
}

void AJ_Net_MCastDown(AJ_MCastSocket* mcastSock)
{
    int ret;
    MCastContext* context = (MCastContext*) mcastSock->rx.context;
    AJ_InfoPrintf(("AJ_Net_MCastDown(mcastSock=0x%p)\n", mcastSock));

    if (context->udpSock != INVALID_SOCKET) {
        /*
         * Leave AJ multicast group
         */
        uint32_t optval[2] = { AJ_IPV4_MCAST_GROUP, AJ_INADDR_ANY };
        ret = AJ_WSL_NET_set_sock_options(context->udpSock, WSL_IPPROTO_IP, WSL_DROP_MEMBERSHIP, sizeof(optval), (uint8_t*)&optval);
        if (ret < 0) {
            AJ_ErrPrintf(("MCastDown(): setsockopt(WSL_DROP_MEMBERSHIP) failed. errno=\"%d\", status=AJ_ERR_READ\n", ret));
            AJ_WSL_NET_socket_close(context->udpSock);
        }
    }
    if (context->mDnsSock != INVALID_SOCKET) {
        /*
         * Leave mDNS multicast group
         */
        uint32_t optval[2] = { MDNS_IPV4_MCAST_GROUP, AJ_INADDR_ANY };
        ret = AJ_WSL_NET_set_sock_options(context->mDnsSock, WSL_IPPROTO_IP, WSL_DROP_MEMBERSHIP, sizeof(optval), (uint8_t*)&optval);
        if (ret < 0) {
            AJ_ErrPrintf(("MCastDown(): setsockopt(WSL_DROP_MEMBERSHIP) failed. errno=\"%d\", status=AJ_ERR_READ\n", ret));
            AJ_WSL_NET_socket_close(context->udpSock);
        }
    }
    /* release the dynamically allocated buffers */
    AJ_Free(mcastSock->rx.bufStart);
    AJ_Free(mcastSock->tx.bufStart);
    CloseMCastSock(mcastSock);

}
