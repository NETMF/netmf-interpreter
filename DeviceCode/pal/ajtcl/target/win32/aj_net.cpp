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

#include <Winsock2.h>
#include <Mswsock.h>

#include <ws2tcpip.h>
#pragma comment(lib, "Ws2_32.lib")

#include <iphlpapi.h>
#pragma comment(lib, "iphlpapi.lib")

#include <assert.h>
#include <stdio.h>
#include <time.h>

#include "aj_target.h"
#include "aj_bufio.h"
#include "aj_net.h"
#include "aj_util.h"
#include "aj_connect.h"
#include "aj_debug.h"

#ifdef AJ_ARDP
#include "aj_ardp.h"
#endif

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgNET = 0;
#endif


static void WinsockCheck()
{
    static uint8_t initialized = FALSE;
    if (!initialized) {
        WSADATA wsaData;
        WORD version = MAKEWORD(2, 0);
        int ret;
        AJ_InfoPrintf(("WinsockCheck\n"));

        ret = WSAStartup(version, &wsaData);
        if (ret) {
            AJ_ErrPrintf(("WSAStartup failed with error: %d\n", ret));
        } else {
            initialized = TRUE;
        }
    }
}

/*
 * IANA assigned IPv4 multicast group for AllJoyn.
 */
static const char AJ_IPV4_MULTICAST_GROUP[] = "224.0.0.113";

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
static const char MDNS_IPV4_MULTICAST_GROUP[] = "224.0.0.251";

/*
 * IANA-assigned IPv6 multicast group for mDNS.
 */
static const char MDNS_IPV6_MULTICAST_GROUP[] = "ff02::fb";

/*
 * IANA-assigned UDP multicast port for mDNS
 */
#define MDNS_UDP_PORT 5353

/*
 * Various events for I/O
 */
static WSAEVENT interruptEvent = WSA_INVALID_EVENT;
static WSAEVENT recvEvent = WSA_INVALID_EVENT;
static WSAEVENT sendEvent = WSA_INVALID_EVENT;

/**
 * Target-specific contexts for network I/O
 */
typedef struct {
    SOCKET tcpSock;
    SOCKET udpSock;
} NetContext;

static NetContext netContext = { INVALID_SOCKET, INVALID_SOCKET };

#ifdef AJ_ARDP
/**
 * Need to predeclare a few things for ARDP
 */
static AJ_Status AJ_Net_ARDP_Connect(AJ_BusAttachment* bus, const AJ_Service* service);

#endif // AJ_ARDP

/*
 * This function is called to cancel a pending select.
 */
void AJ_Net_Interrupt()
{
    if (interruptEvent != WSA_INVALID_EVENT) {
        WSASetEvent(interruptEvent);
    }
}

static AJ_Status AJ_Net_Send(AJ_IOBuffer* buf)
{
    DWORD ret;
    DWORD tx = AJ_IO_BUF_AVAIL(buf);

    AJ_InfoPrintf(("AJ_Net_Send(buf=0x%p)\n", buf));

    assert(buf->direction == AJ_IO_BUF_TX);

    if (tx > 0) {
        NetContext* ctx = (NetContext*) buf->context;
        WSAOVERLAPPED ov;
        DWORD flags = 0;
        WSABUF wsbuf;

        memset(&ov, 0, sizeof(ov));
        ov.hEvent = sendEvent;
        wsbuf.len = tx;
        wsbuf.buf = (CHAR*)buf->readPtr;

        ret = WSASend(ctx->tcpSock, &wsbuf, 1, NULL, flags, &ov, NULL);
        if (!WSAGetOverlappedResult(ctx->tcpSock, &ov, &tx, TRUE, &flags)) {
            AJ_ErrPrintf(("AJ_Net_Send(): send() failed. WSAGetLastError()=0x%x, status=AJ_ERR_WRITE\n", WSAGetLastError()));
            return AJ_ERR_WRITE;
        }
        buf->readPtr += tx;
    }
    if (AJ_IO_BUF_AVAIL(buf) == 0) {
        AJ_IO_BUF_RESET(buf);
    }
    AJ_InfoPrintf(("AJ_Net_Send(): status=AJ_OK\n"));
    return AJ_OK;
}

static WSAOVERLAPPED wsaOverlapped;
static WSABUF wsbuf;

static AJ_Status AJ_Net_Recv(AJ_IOBuffer* buf, uint32_t len, uint32_t timeout)
{
    AJ_Status status = AJ_ERR_READ;
    WSAEVENT events[2];
    DWORD rx = AJ_IO_BUF_SPACE(buf);
    DWORD flags = 0;
    DWORD ret = SOCKET_ERROR;
    NetContext* ctx = (NetContext*) buf->context;

    AJ_InfoPrintf(("AJ_Net_Recv(buf=0x%p, len=%d, timeout=%d)\n", buf, len, timeout));

    assert(buf->direction == AJ_IO_BUF_RX);

    rx = min(rx, len);
    if (!rx) {
        return AJ_OK;
    }
    /*
     * Overlapped receives cannot be cancelled. We are relying the fact that a timedout or
     * interrupted receive will be eventually reposted with the same buffer.
     */
    if (wsaOverlapped.hEvent == INVALID_HANDLE_VALUE) {
        wsbuf.len = rx;
        wsbuf.buf = (CHAR*)buf->writePtr;
        memset(&wsaOverlapped, 0, sizeof(WSAOVERLAPPED));
        wsaOverlapped.hEvent = recvEvent;
        ret = WSARecv(ctx->tcpSock, &wsbuf, 1, NULL, &flags, &wsaOverlapped, NULL);
        if ((ret == SOCKET_ERROR) && (WSAGetLastError() != WSA_IO_PENDING)) {
            AJ_ErrPrintf(("WSARecv(): failed WSAGetLastError()=%d\n", WSAGetLastError()));
            return AJ_ERR_READ;
        }
    }
    /*
     * Assert that the buffer and length are the same in the case where this is a reposting of the
     * receive after an timeout or interrupt.
     */
    AJ_ASSERT(wsbuf.buf == (CHAR*)buf->writePtr);
    AJ_ASSERT(wsbuf.len == rx);

    events[0] = wsaOverlapped.hEvent;
    events[1] = interruptEvent;
    ret = WSAWaitForMultipleEvents(2, events, FALSE, timeout, TRUE);
    if (ret == WSA_WAIT_EVENT_0) {
        if (WSAGetOverlappedResult(ctx->tcpSock, &wsaOverlapped, &rx, TRUE, &flags)) {
            status = AJ_OK;
        }
    } else if (ret == WSA_WAIT_TIMEOUT) {
        status = AJ_ERR_TIMEOUT;
    } else if (ret == (WSA_WAIT_EVENT_0 + 1)) {
        WSAResetEvent(interruptEvent);
        status = AJ_ERR_INTERRUPTED;
    } else {
        AJ_ErrPrintf(("AJ_Net_Recv(): WSAGetLastError()=%d\n", WSAGetLastError()));
    }
    if (status == AJ_OK) {
        /*
         * Reset recv event and clear overlapped struct for the next call
         */
        WSAResetEvent(wsaOverlapped.hEvent);
        wsaOverlapped.hEvent = INVALID_HANDLE_VALUE;
        buf->writePtr += rx;
        AJ_InfoPrintf(("AJ_Net_Recv(): read %d bytes\n", rx));
    }
    return status;
}

/*
 * Statically sized buffers for I/O
 */
static uint8_t rxData[2048];
static uint8_t txData[2048];

AJ_Status AJ_Net_Connect(AJ_BusAttachment* bus, const AJ_Service* service)
{
    DWORD ret;
    SOCKADDR_STORAGE addrBuf;
    socklen_t addrSize;
    SOCKET sock;

    /* Initialize Winsock, if not done already */
    WinsockCheck();

#ifdef AJ_ARDP
    if (service->addrTypes & (AJ_ADDR_UDP4 | AJ_ADDR_UDP6)) {
        return AJ_Net_ARDP_Connect(bus, service);
    }
#endif

    AJ_InfoPrintf(("AJ_Net_Connect(bus=0x%p, addrType=%d.)\n", bus, service->addrTypes));

    memset(&addrBuf, 0, sizeof(addrBuf));

    if (service->addrTypes & AJ_ADDR_TCP4) {
        struct sockaddr_in* sa = (struct sockaddr_in*)&addrBuf;

        sock = WSASocket(AF_INET, SOCK_STREAM, IPPROTO_TCP, NULL, 0, WSA_FLAG_OVERLAPPED);
        if (sock == INVALID_SOCKET) {
            AJ_ErrPrintf(("AJ_Net_Connect(): invalid socket.  status=AJ_ERR_CONNECT\n"));
            return AJ_ERR_CONNECT;
        }

        sa->sin_family = AF_INET;
        sa->sin_port = htons(service->ipv4port);
        sa->sin_addr.s_addr = service->ipv4;
        addrSize = sizeof(*sa);
        AJ_InfoPrintf(("AJ_Net_Connect(): Connect to \"%s:%u\"\n", inet_ntoa(sa->sin_addr), service->ipv4port));;
    } else if (service->addrTypes & AJ_ADDR_TCP6) {
        struct sockaddr_in6* sa = (struct sockaddr_in6*)&addrBuf;

        sock = WSASocket(AF_INET6, SOCK_STREAM, IPPROTO_TCP, NULL, 0, WSA_FLAG_OVERLAPPED);
        if (sock == INVALID_SOCKET) {
            AJ_ErrPrintf(("AJ_Net_Connect(): invalid socket.  status=AJ_ERR_CONNECT\n"));
            return AJ_ERR_CONNECT;
        }

        sa->sin6_family = AF_INET6;
        sa->sin6_port = htons(service->ipv6port);
        memcpy(sa->sin6_addr.s6_addr, service->ipv6, sizeof(sa->sin6_addr.s6_addr));
        addrSize = sizeof(*sa);
    } else {
        AJ_ErrPrintf(("AJ_Net_Connect: only TCPv6 and TCPv4 are supported\n"));
        return AJ_ERR_CONNECT;
    }

    ret = connect(sock, (struct sockaddr*)&addrBuf, addrSize);
    if (ret == SOCKET_ERROR) {
        AJ_ErrPrintf(("AJ_Net_Connect(): connect() failed. WSAGetLastError()=0x%x, status=AJ_ERR_CONNECT\n", WSAGetLastError()));
        closesocket(sock);
        return AJ_ERR_CONNECT;
    } else {
        AJ_IOBufInit(&bus->sock.rx, rxData, sizeof(rxData), AJ_IO_BUF_RX, &netContext);
        bus->sock.rx.recv = AJ_Net_Recv;
        AJ_IOBufInit(&bus->sock.tx, txData, sizeof(txData), AJ_IO_BUF_TX, &netContext);
        bus->sock.tx.send = AJ_Net_Send;

        netContext.tcpSock = sock;
        AJ_InfoPrintf(("AJ_Net_Connect(): status=AJ_OK\n"));
        sendEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
        recvEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
        interruptEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
        wsaOverlapped.hEvent = INVALID_HANDLE_VALUE;
        return AJ_OK;
    }
}

void AJ_Net_Disconnect(AJ_NetSocket* netSock)
{
    AJ_InfoPrintf(("AJ_Net_Disconnect(nexSock=0x%p)\n", netSock));

    if (netContext.tcpSock != INVALID_SOCKET) {
        shutdown(netContext.tcpSock, 0);
        closesocket(netContext.tcpSock);
        netContext.tcpSock = INVALID_SOCKET;
    } else if (netContext.udpSock != INVALID_SOCKET) {
#ifdef AJ_ARDP
        AJ_ARDP_Disconnect(FALSE);
#endif
        shutdown(netContext.udpSock, 0);
        closesocket(netContext.udpSock);
        netContext.udpSock = INVALID_SOCKET;
    }

    memset(netSock, 0, sizeof(AJ_NetSocket));
    WSACloseEvent(recvEvent);
    WSACloseEvent(sendEvent);
    WSACloseEvent(interruptEvent);
}

typedef struct {
    SOCKET sock;
    int family;
    struct in_addr v4_bcast;
    uint16_t recv_port;
    uint8_t is_mdns;
    uint8_t is_mdnsrecv;
    uint8_t has_mcast4;
    uint8_t has_mcast6;
    struct in_addr v4_addr;
} mcast_info_t;

static mcast_info_t* McastSocks = NULL;
static size_t NumMcastSocks = 0;

static AJ_Status RewriteSenderInfo(AJ_IOBuffer* buf, uint32_t addr, uint16_t port)
{
    size_t tx = AJ_IO_BUF_AVAIL(buf);
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

static AJ_Status AJ_Net_SendTo(AJ_IOBuffer* buf)
{
    DWORD ret;
    DWORD tx = AJ_IO_BUF_AVAIL(buf);
    int numWrites = 0;

    AJ_InfoPrintf(("AJ_Net_SendTo(buf=0x%p)\n", buf));

    assert(buf->direction == AJ_IO_BUF_TX);
    assert(NumMcastSocks > 0);

    if (tx > 0) {
        size_t i;

        // our router (hopefully) lives on one of the networks but we don't know which one.
        // send discovery requests to all of them.
        for (i = 0; i < NumMcastSocks; ++i) {
            SOCKET sock = McastSocks[i].sock;
            int family = McastSocks[i].family;

            if ((buf->flags & AJ_IO_BUF_AJ) && !McastSocks[i].is_mdns) {
                // try sending IPv6 multicast
                if (family == AF_INET6) {
                    struct sockaddr_in6 sin6;
                    memset(&sin6, 0, sizeof(struct sockaddr_in6));
                    sin6.sin6_family = AF_INET6;
                    sin6.sin6_port = htons(AJ_UDP_PORT);
                    inet_pton(AF_INET6, AJ_IPV6_MULTICAST_GROUP, &sin6.sin6_addr);
                    ret = sendto(sock, (const char*)buf->readPtr, tx, 0, (struct sockaddr*) &sin6, sizeof(struct sockaddr_in6));
                    if (ret == SOCKET_ERROR) {
                        AJ_ErrPrintf(("AJ_Net_SendTo(): sendto() failed (IPV6). WSAGetLastError()=0x%x\n", WSAGetLastError()));
                    } else {
                        ++numWrites;
                    }
                }
                // try sending IPv4 multicast
                if (family == AF_INET && McastSocks[i].has_mcast4) {
                    struct sockaddr_in sin;
                    memset(&sin, 0, sizeof(sin));
                    sin.sin_family = AF_INET;
                    sin.sin_port = htons(AJ_UDP_PORT);
                    inet_pton(AF_INET, AJ_IPV4_MULTICAST_GROUP, &sin.sin_addr);

                    ret = sendto(sock, (const char*)buf->readPtr, tx, 0, (struct sockaddr*) &sin, sizeof(struct sockaddr_in));
                    if (ret == SOCKET_ERROR) {
                        AJ_ErrPrintf(("AJ_Net_SendTo(): sendto() failed (IPV4). WSAGetLastError()=0x%x\n", WSAGetLastError()));
                    } else {
                        ++numWrites;
                    }
                }

                // try sending IPv4 subnet broadcast
                if (family == AF_INET && McastSocks[i].v4_bcast.s_addr) {
                    struct sockaddr_in bsin;
                    memset(&bsin, 0, sizeof(bsin));
                    bsin.sin_family = AF_INET;
                    bsin.sin_port = htons(AJ_UDP_PORT);
                    bsin.sin_addr.s_addr = McastSocks[i].v4_bcast.s_addr;
                    ret = sendto(sock, (const char*)buf->readPtr, tx, 0, (struct sockaddr*) &bsin, sizeof(struct sockaddr_in));
                    if (ret == SOCKET_ERROR) {
                        AJ_ErrPrintf(("AJ_Net_SendTo(): sendto() failed (bcast). WSAGetLastError()=0x%x\n", WSAGetLastError()));
                    } else {
                        ++numWrites;
                    }
                }
            }

            if ((buf->flags & AJ_IO_BUF_MDNS) && McastSocks[i].is_mdns) {

                // Update the packet with receiver info for this socket
                if (RewriteSenderInfo(buf, ntohl(McastSocks[i].v4_addr.s_addr), McastSocks[i].recv_port) != AJ_OK) {
                    AJ_WarnPrintf(("AJ_Net_SendTo(): RewriteSenderInfo failed.\n"));
                    continue;
                }
                tx = AJ_IO_BUF_AVAIL(buf);

                // try sending IPv4 multicast
                if (family == AF_INET && McastSocks[i].has_mcast4) {
                    struct sockaddr_in sin;
                    memset(&sin, 0, sizeof(sin));
                    sin.sin_family = AF_INET;
                    sin.sin_port = htons(MDNS_UDP_PORT);
                    inet_pton(AF_INET, MDNS_IPV4_MULTICAST_GROUP, &sin.sin_addr);

                    ret = sendto(sock, (const char*)buf->readPtr, tx, 0, (struct sockaddr*) &sin, sizeof(struct sockaddr_in));
                    if (ret == SOCKET_ERROR) {
                        AJ_ErrPrintf(("AJ_Net_SendTo(): sendto() multicast failed (IPV4). WSAGetLastError()=0x%x\n", WSAGetLastError()));
                    } else {
                        ++numWrites;
                    }
                }

                // try sending IPv4 subnet broadcast
                if (family == AF_INET && McastSocks[i].v4_bcast.s_addr) {
                    struct sockaddr_in bsin;
                    memset(&bsin, 0, sizeof(bsin));
                    bsin.sin_family = AF_INET;
                    bsin.sin_port = htons(MDNS_UDP_PORT);
                    bsin.sin_addr.s_addr = McastSocks[i].v4_bcast.s_addr;
                    ret = sendto(sock, (const char*)buf->readPtr, tx, 0, (struct sockaddr*) &bsin, sizeof(struct sockaddr_in));
                    if (ret == SOCKET_ERROR) {
                        AJ_ErrPrintf(("AJ_Net_SendTo(): sendto() broadcast failed. WSAGetLastError()=0x%x\n", WSAGetLastError()));
                    } else {
                        ++numWrites;
                    }
                }

                // try sending IPv6 multicast
                if (family == AF_INET6) {
                    struct sockaddr_in6 sin6;
                    memset(&sin6, 0, sizeof(struct sockaddr_in6));
                    sin6.sin6_family = AF_INET6;
                    sin6.sin6_port = htons(MDNS_UDP_PORT);
                    inet_pton(AF_INET6, MDNS_IPV6_MULTICAST_GROUP, &sin6.sin6_addr);
                    ret = sendto(sock, (const char*)buf->readPtr, tx, 0, (struct sockaddr*) &sin6, sizeof(struct sockaddr_in6));
                    if (ret == SOCKET_ERROR) {
                        AJ_ErrPrintf(("AJ_Net_SendTo(): sendto() failed (IPV6). WSAGetLastError()=0x%x\n", WSAGetLastError()));
                    } else {
                        ++numWrites;
                    }
                }
            }
        }

        if (numWrites == 0) {
            AJ_ErrPrintf(("AJ_Net_SendTo(): Did not sendto() at least one socket.  status=AJ_ERR_WRITE\n"));
            return AJ_ERR_WRITE;
        }
        buf->readPtr += ret;
    }
    AJ_IO_BUF_RESET(buf);
    AJ_InfoPrintf(("AJ_Net_SendTo(): status=AJ_OK\n"));
    return AJ_OK;
}

static AJ_Status AJ_Net_RecvFrom(AJ_IOBuffer* buf, uint32_t len, uint32_t timeout)
{
    AJ_Status status;
    DWORD ret;
    DWORD rx = AJ_IO_BUF_SPACE(buf);
    fd_set fds;
    size_t rc = 0;
    size_t i;
    const struct timeval tv = { timeout / 1000, 1000 * (timeout % 1000) };
    SOCKET sock;
    int numSocks = 0;

    AJ_InfoPrintf(("AJ_Net_RecvFrom(buf=0x%p, len=%d., timeout=%d.)\n", buf, len, timeout));

    assert(buf->direction == AJ_IO_BUF_RX);
    assert(NumMcastSocks > 0);

    // we sent the discovery requests out on ALL broadcast and multicast interfaces
    // now we need to listen on the NS version 1 sockets and the mDNS recv sockets
    FD_ZERO(&fds);
    for (i = 0; i < NumMcastSocks; ++i) {
        if (!McastSocks[i].is_mdns || McastSocks[i].is_mdnsrecv) {
            SOCKET sock = McastSocks[i].sock;
            FD_SET(sock, &fds);
            numSocks++;
        }
    }

    // wait for discovery response
    rc = select(numSocks, &fds, NULL, NULL, &tv);
    if (rc == 0) {
        AJ_InfoPrintf(("AJ_Net_RecvFrom(): select() timed out. status=AJ_ERR_TIMEOUT\n"));
        return AJ_ERR_TIMEOUT;
    } else if (rc < 0) {
        AJ_ErrPrintf(("AJ_Net_RecvFrom(): select() failed. WSAGetLastError()=0x%x, status=AJ_ERR_READ\n", WSAGetLastError()));
        return AJ_ERR_READ;
    }

    // ignore multiple replies; only consider the first one to arrive
    rx = min(rx, len);
    for (i = 0; i < NumMcastSocks; ++i) {
        if (!McastSocks[i].is_mdns || McastSocks[i].is_mdnsrecv) {
            if (FD_ISSET(McastSocks[i].sock, &fds)) {
                sock = McastSocks[i].sock;
                if (McastSocks[i].is_mdnsrecv) {
                    buf->flags |= AJ_IO_BUF_MDNS;
                } else {
                    buf->flags |= AJ_IO_BUF_AJ;
                }
                break;
            }
        }
    }

    if (sock != INVALID_SOCKET) {
        ret = recvfrom(sock, (char*)buf->writePtr, rx, 0, NULL, 0);
        if (ret == SOCKET_ERROR) {
            AJ_ErrPrintf(("AJ_Net_RecvFrom(): recvfrom() failed. WSAGetLastError()=0x%x, status=AJ_ERR_READ\n", WSAGetLastError()));
            status = AJ_ERR_READ;
        } else {
            buf->writePtr += ret;
            status = AJ_OK;
        }
    } else {
        AJ_ErrPrintf(("AJ_Net_RecvFrom(): invalid socket.  status=AJ_ERR_READ\n"));
        status = AJ_ERR_READ;
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
static uint8_t rxDataMCast[1454];
static uint8_t txDataMCast[471];

static void Mcast6Up(const char* group, uint16_t port, uint8_t mdns, uint16_t recv_port)
{
    char iface_buffer[sizeof(IP_ADAPTER_ADDRESSES) * 150];
    char v4_iface_buffer[sizeof(IP_ADAPTER_ADDRESSES) * 150];
    PIP_ADAPTER_ADDRESSES interfaces = (PIP_ADAPTER_ADDRESSES) iface_buffer;
    PIP_ADAPTER_ADDRESSES v4_interfaces = (PIP_ADAPTER_ADDRESSES) v4_iface_buffer;
    DWORD num_bytes = sizeof(iface_buffer);

    // Get the IPv6 adapter addresses
    if (ERROR_SUCCESS != GetAdaptersAddresses(AF_INET6, GAA_FLAG_SKIP_MULTICAST | GAA_FLAG_SKIP_ANYCAST | GAA_FLAG_SKIP_DNS_SERVER, NULL, interfaces, &num_bytes)) {
        AJ_ErrPrintf(("Mcast6Up(): GetAdaptersAddresses for IPv6 failed. WSAGetLastError()=%0x%x\n", WSAGetLastError()));
        return;
    }

    // Get the IPv4 adapter addresses
    if (ERROR_SUCCESS != GetAdaptersAddresses(AF_INET, GAA_FLAG_SKIP_MULTICAST | GAA_FLAG_SKIP_ANYCAST | GAA_FLAG_SKIP_DNS_SERVER, NULL, v4_interfaces, &num_bytes)) {
        AJ_ErrPrintf(("Mcast6Up(): GetAdaptersAddresses for IPv4 failed. WSAGetLastError()=%0x%x\n", WSAGetLastError()));
        return;
    }

    for (; interfaces != NULL; interfaces = interfaces->Next) {
        int ret = 0;
        struct sockaddr_in6 addr;
        struct sockaddr_in v4_addr;
        struct ipv6_mreq mreq6;

        mcast_info_t new_sock;
        new_sock.sock = INVALID_SOCKET;
        new_sock.family = AF_INET6;
        new_sock.recv_port = recv_port;
        new_sock.has_mcast4 = FALSE;
        new_sock.is_mdns = mdns;
        new_sock.v4_bcast.s_addr = 0;
        new_sock.v4_addr.s_addr = 0;

        memset(&mreq6, 0, sizeof(struct ipv6_mreq));

        if (interfaces->OperStatus != IfOperStatusUp || interfaces->NoMulticast) {
            continue;
        }

        memcpy(&addr, interfaces->FirstUnicastAddress->Address.lpSockaddr, sizeof(struct sockaddr_in6));

        // find and save the IPv4 address from this same adapter
        for (; v4_interfaces != NULL; v4_interfaces = v4_interfaces->Next) {
            if (interfaces->Ipv6IfIndex == v4_interfaces->IfIndex) {
                memcpy(&v4_addr, v4_interfaces->FirstUnicastAddress->Address.lpSockaddr, sizeof(struct sockaddr_in));
                new_sock.v4_addr.s_addr = v4_addr.sin_addr.s_addr;
            }
        }

        // create a socket
        new_sock.sock = socket(AF_INET6, SOCK_DGRAM, 0);
        if (new_sock.sock == INVALID_SOCKET) {
            AJ_ErrPrintf(("Mcast6Up(): socket() failed. WSAGetLastError()=0x%x\n", WSAGetLastError()));
            continue;
        }

        // bind the socket to the supplied port
        addr.sin6_family = AF_INET6;
        addr.sin6_port = htons(port);

        ret = bind(new_sock.sock, (struct sockaddr*) &addr, sizeof(struct sockaddr_in6));
        if (ret == SOCKET_ERROR) {
            AJ_ErrPrintf(("Mcast6Up(): bind() failed. WSAGetLastError()=0x%x\n", WSAGetLastError()));
            closesocket(new_sock.sock);
            new_sock.sock = INVALID_SOCKET;
            continue;
        }

        // because routers are advertised silently, the reply will be unicast
        // however, Windows forces us to join the multicast group before we can broadcast our WhoHas packets
        inet_pton(AF_INET6, group, &mreq6.ipv6mr_multiaddr);
        mreq6.ipv6mr_interface = interfaces->IfIndex;

        ret = setsockopt(new_sock.sock, IPPROTO_IPV6, IPV6_ADD_MEMBERSHIP, (char*) &mreq6, sizeof(mreq6));
        if (ret == SOCKET_ERROR) {
            AJ_ErrPrintf(("Mcast6Up(): setsockopt(IP_ADD_MEMBERSHIP) failed. WSAGetLastError()=0x%x\n", WSAGetLastError()));
            closesocket(new_sock.sock);
            new_sock.sock = INVALID_SOCKET;
            continue;
        }

        if (new_sock.sock != INVALID_SOCKET) {
            NumMcastSocks++;
            McastSocks = (mcast_info_t*)realloc(McastSocks, NumMcastSocks * sizeof(mcast_info_t));
            memcpy(&McastSocks[NumMcastSocks - 1], &new_sock, sizeof(mcast_info_t));
        }
    }
}


static void Mcast4Up(const char* group, uint16_t port, uint8_t mdns, uint16_t recv_port)
{
    int ret = 0;
    INTERFACE_INFO interfaces[150];
    DWORD num_bytes, num_ifaces;
    SOCKET tmp_sock;
    uint32_t i = 0;
    int reuse = 1;

    tmp_sock = socket(AF_INET, SOCK_DGRAM, 0);
    if (tmp_sock == INVALID_SOCKET) {
        AJ_ErrPrintf(("Mcast4Up(): socket failed. WSAGetLastError()=%0x%x\n", WSAGetLastError()));
        return;
    }

    if (SOCKET_ERROR == WSAIoctl(tmp_sock, SIO_GET_INTERFACE_LIST, 0, 0, &interfaces, sizeof(interfaces), &num_bytes, 0, 0)) {
        AJ_ErrPrintf(("Mcast4Up(): WSAIoctl failed. WSAGetLastError()=%0x%x\n", WSAGetLastError()));
        return;
    }


    closesocket(tmp_sock);
    num_ifaces = num_bytes / sizeof(INTERFACE_INFO);

    for (i = 0; i < num_ifaces; ++i) {
        LPINTERFACE_INFO info = &interfaces[i];
        struct sockaddr_in* addr =  &info->iiAddress.AddressIn;
        mcast_info_t new_sock;

        new_sock.sock = INVALID_SOCKET;
        new_sock.family = AF_INET;
        new_sock.has_mcast4 = FALSE;
        new_sock.is_mdnsrecv = FALSE;
        new_sock.is_mdns = mdns;
        new_sock.v4_bcast.s_addr = 0;
        new_sock.recv_port = recv_port;
        new_sock.v4_addr.s_addr = 0;

        if (!(info->iiFlags & IFF_UP)) {
            continue;
        }

        // create a socket
        new_sock.sock = socket(AF_INET, SOCK_DGRAM, 0);
        if (new_sock.sock == INVALID_SOCKET) {
            AJ_ErrPrintf(("Mcast4Up(): socket() failed. WSAGetLastError()=0x%x\n", WSAGetLastError()));
            continue;
        }

        ret = setsockopt(new_sock.sock, SOL_SOCKET, SO_REUSEADDR, (const char*)&reuse, sizeof(reuse));

        if (ret != 0) {
            AJ_ErrPrintf(("MCast4Up(): setsockopt(SO_REUSEADDR) failed. errno=\"%s\", status=AJ_ERR_READ\n", strerror(errno)));
            closesocket(new_sock.sock);
            new_sock.sock = INVALID_SOCKET;
            continue;
        }
        new_sock.v4_addr.s_addr = info->iiAddress.AddressIn.sin_addr.s_addr;

        // if this address supports IPV4 broadcast, calculate the subnet bcast address and save it
        if (info->iiFlags & IFF_BROADCAST) {
            int bcast = 1;
            ret = setsockopt(new_sock.sock, SOL_SOCKET, SO_BROADCAST, (const char*) &bcast, sizeof(bcast));
            if (ret != 0) {
                AJ_ErrPrintf(("Mcast4Up(): setsockopt(SO_BROADCAST) failed. WSAGetLastError()=0x%x\n", WSAGetLastError()));
                closesocket(new_sock.sock);
                new_sock.sock = INVALID_SOCKET;
                continue;
            }

            new_sock.v4_bcast.s_addr = info->iiAddress.AddressIn.sin_addr.s_addr | ~(info->iiNetmask.AddressIn.sin_addr.s_addr);
        }

        // and if it supports multicast, join the IPV4 mcast group
        if (info->iiFlags & IFF_MULTICAST) {
            struct ip_mreq mreq;
            struct sockaddr_in sin;
            memset(&mreq, 0, sizeof(struct ip_mreq));

            // bind the socket to the address with supplied port
            sin.sin_family = AF_INET;
            sin.sin_port = htons(port);
            // need to bind to INADDR_ANY for mdns
            if (mdns == TRUE) {
                sin.sin_addr.s_addr = INADDR_ANY;
            } else {
                memcpy(&sin, addr, sizeof(struct sockaddr_in));
            }
            AJ_InfoPrintf(("MCast4Up(): Binding to port %d and group %s\n", port, group));

            ret = bind(new_sock.sock, (struct sockaddr*) &sin, sizeof(sin));
            if (ret == SOCKET_ERROR) {
                AJ_ErrPrintf(("Mcast4Up(): bind() failed. WSAGetLastError()=0x%x\n", WSAGetLastError()));
                closesocket(new_sock.sock);
                new_sock.sock = INVALID_SOCKET;
                continue;
            }

            // because routers are advertised silently, the reply will be unicast
            // however, Windows forces us to join the multicast group before we can broadcast our WhoHas packets
            inet_pton(AF_INET, group, &mreq.imr_multiaddr);
            memcpy(&mreq.imr_interface, &sin.sin_addr, sizeof(struct in_addr));
            ret = setsockopt(new_sock.sock, IPPROTO_IP, IP_ADD_MEMBERSHIP, (char*) &mreq, sizeof(mreq));
            if (ret == SOCKET_ERROR) {
                AJ_ErrPrintf(("Mcast4Up(): setsockopt(IP_ADD_MEMBERSHIP) failed. WSAGetLastError()=0x%x\n", WSAGetLastError()));
                closesocket(new_sock.sock);
                new_sock.sock = INVALID_SOCKET;
                continue;
            }

            new_sock.has_mcast4 = TRUE;
        }

        if (new_sock.sock != INVALID_SOCKET) {
            NumMcastSocks++;
            McastSocks = (mcast_info_t*)realloc(McastSocks, NumMcastSocks * sizeof(mcast_info_t));
            memcpy(&McastSocks[NumMcastSocks - 1], &new_sock, sizeof(mcast_info_t));
        }
    }
}


static SOCKET MDnsRecvUp()
{
    int ret = 0;
    SOCKET tmp_sock;
    uint32_t i = 0;
    struct sockaddr_in sin;
    mcast_info_t new_sock;

    tmp_sock = socket(AF_INET, SOCK_DGRAM, 0);
    if (tmp_sock == INVALID_SOCKET) {
        AJ_ErrPrintf(("MDnsRecvUp(): socket failed. WSAGetLastError()=%0x%x\n", WSAGetLastError()));
        return tmp_sock;
    }

    // bind the socket to an ephemeral port
    sin.sin_family = AF_INET;
    sin.sin_port = htons(0);
    sin.sin_addr.s_addr = INADDR_ANY;

    ret = bind(tmp_sock, (struct sockaddr*) &sin, sizeof(sin));
    if (ret == SOCKET_ERROR) {
        AJ_ErrPrintf(("MDnsRecvUp(): bind() failed. WSAGetLastError()=0x%x\n", WSAGetLastError()));
        closesocket(tmp_sock);
        tmp_sock = INVALID_SOCKET;
        return tmp_sock;
    }

    new_sock.sock = tmp_sock;
    new_sock.family = AF_INET;
    new_sock.has_mcast4 = FALSE;
    new_sock.is_mdns = FALSE;
    new_sock.is_mdnsrecv = TRUE;
    new_sock.v4_bcast.s_addr = 0;
    new_sock.v4_addr.s_addr = 0;
    NumMcastSocks++;
    McastSocks = (mcast_info_t*)realloc(McastSocks, NumMcastSocks * sizeof(mcast_info_t));
    memcpy(&McastSocks[NumMcastSocks - 1], &new_sock, sizeof(mcast_info_t));
    return tmp_sock;
}


AJ_Status AJ_Net_MCastUp(AJ_MCastSocket* mcastSock)
{
    AJ_Status status = AJ_OK;
    size_t numMDnsRecvSocks;
    struct sockaddr_storage addrBuf;
    socklen_t addrLen = sizeof(addrBuf);
    struct sockaddr_in* sin;
    SOCKET tmp_sock = INVALID_SOCKET;
    // bring up WinSock
    WinsockCheck();

    AJ_InfoPrintf(("AJ_Net_MCastUp(mcastSock=0x%p)\n", mcastSock));

    // create the mDNS recv socket
    //tmp_sock = MDnsRecvUp(mcastSock);
    tmp_sock = MDnsRecvUp();
    if (tmp_sock != INVALID_SOCKET) {
        getsockname(tmp_sock, (struct sockaddr*) &addrBuf, &addrLen);
        sin = (struct sockaddr_in*) &addrBuf;
        AJ_InfoPrintf(("AJ_Net_MCastUp(): mDNS recv port: %d\n", ntohs(sin->sin_port)));
    }

    if (NumMcastSocks == 0) {
        AJ_ErrPrintf(("AJ_Net_MCastUp(): No mDNS recv socket found. status=AJ_ERR_READ\n"));
        return AJ_ERR_READ;
    }

    numMDnsRecvSocks = NumMcastSocks;

    // create the sending sockets
    Mcast4Up(MDNS_IPV4_MULTICAST_GROUP, MDNS_UDP_PORT, TRUE, ntohs(sin->sin_port));
    Mcast6Up(MDNS_IPV6_MULTICAST_GROUP, MDNS_UDP_PORT, TRUE, ntohs(sin->sin_port));

    // create the NS sockets only if considering pre-14.06 routers
    if (AJ_GetMinProtoVersion() < 10) {
        Mcast4Up(AJ_IPV4_MULTICAST_GROUP, AJ_UDP_PORT, FALSE, 0);
        Mcast6Up(AJ_IPV6_MULTICAST_GROUP, AJ_UDP_PORT, FALSE, 0);
    }

    AJ_IOBufInit(&mcastSock->rx, rxDataMCast, sizeof(rxDataMCast), AJ_IO_BUF_RX, (void*) McastSocks);
    mcastSock->rx.recv = AJ_Net_RecvFrom;
    AJ_IOBufInit(&mcastSock->tx, txDataMCast, sizeof(txDataMCast), AJ_IO_BUF_TX, (void*) McastSocks);
    mcastSock->tx.send = AJ_Net_SendTo;

    return AJ_OK;
}

void AJ_Net_MCastDown(AJ_MCastSocket* mcastSock)
{
    size_t i;
    AJ_InfoPrintf(("AJ_Net_MCastDown(nexSock=0x%p)\n", mcastSock));

    // shutdown and close all sockets
    for (i = 0; i < NumMcastSocks; ++i) {
        SOCKET sock = McastSocks[i].sock;

        // leave multicast groups
        if ((McastSocks[i].family == AF_INET) && McastSocks[i].has_mcast4) {
            struct ip_mreq mreq;
            inet_pton(AF_INET, AJ_IPV4_MULTICAST_GROUP, &mreq.imr_multiaddr);
            mreq.imr_interface.s_addr = INADDR_ANY;
            setsockopt(sock, IPPROTO_IP, IP_DROP_MEMBERSHIP, (char*) &mreq, sizeof(mreq));
        } else if ((McastSocks[i].family == AF_INET6) && McastSocks[i].has_mcast6) {
            struct ipv6_mreq mreq6;
            inet_pton(AF_INET6, AJ_IPV6_MULTICAST_GROUP, &mreq6.ipv6mr_multiaddr);
            mreq6.ipv6mr_interface = 0;
            setsockopt(sock, IPPROTO_IPV6, IPV6_DROP_MEMBERSHIP, (char*) &mreq6, sizeof(mreq6));
        }

        shutdown(sock, 0);
        closesocket(sock);
    }

    NumMcastSocks = 0;
    free(McastSocks);
    McastSocks = NULL;
    memset(mcastSock, 0, sizeof(AJ_MCastSocket));
}



#ifdef AJ_ARDP

static AJ_Status AJ_ARDP_UDP_Send(void* context, uint8_t* buf, size_t len, size_t* sent)
{
    AJ_Status status = AJ_OK;
    DWORD ret;
    NetContext* ctx = (NetContext*) context;
    WSAOVERLAPPED ov;
    DWORD flags = 0;
    WSABUF wsbuf;

    memset(&ov, 0, sizeof(ov));
    ov.hEvent = sendEvent;
    wsbuf.len = len;
    wsbuf.buf = buf;

    AJ_InfoPrintf(("AJ_ARDP_UDP_Send(buf=0x%p, len=%lu)\n", buf, len));

    ret = WSASend(ctx->udpSock, &wsbuf, 1, NULL, flags, &ov, NULL);
    if (ret == SOCKET_ERROR) {
        AJ_ErrPrintf(("AJ_ARDP_UDP_Send(): WSASend() failed. WSAGetLastError()=0x%x, status=AJ_ERR_WRITE\n", WSAGetLastError()));
        *sent = 0;
        return AJ_ERR_WRITE;
    }

    if (!WSAGetOverlappedResult(ctx->udpSock, &ov, sent, TRUE, &flags)) {
        AJ_ErrPrintf(("AJ_ARDP_UDP_Send(): WSAGetOverlappedResult() failed. WSAGetLastError()=0x%x, status=AJ_ERR_WRITE\n", WSAGetLastError()));
        return AJ_ERR_WRITE;
    }

    return status;
}

static AJ_Status AJ_ARDP_UDP_Recv(void* context, uint8_t** data, uint32_t* recved, uint32_t timeout)
{
    NetContext* ctx = (NetContext*) context;
    DWORD ret = SOCKET_ERROR;
    WSAEVENT events[2];
    DWORD flags = 0;
    static uint8_t buffer[UDP_SEGBMAX];

    *data = NULL;

    if (wsaOverlapped.hEvent == INVALID_HANDLE_VALUE) {
        wsbuf.len = sizeof(buffer);
        wsbuf.buf = buffer;
        memset(&wsaOverlapped, 0, sizeof(WSAOVERLAPPED));
        wsaOverlapped.hEvent = recvEvent;
        ret = WSARecvFrom(ctx->udpSock, &wsbuf, 1, NULL, &flags, NULL, NULL, &wsaOverlapped, NULL);
        if ((ret == SOCKET_ERROR) && (WSAGetLastError() != WSA_IO_PENDING)) {
            AJ_ErrPrintf(("WSARecvFrom(): failed WSAGetLastError()=%d\n", WSAGetLastError()));
            return AJ_ERR_READ;
        }
    }

    events[0] = wsaOverlapped.hEvent;
    events[1] = interruptEvent;

    ret = WSAWaitForMultipleEvents(2, events, FALSE, timeout, TRUE);
    switch (ret) {
    case WSA_WAIT_EVENT_0:
        flags = 0;
        if (WSAGetOverlappedResult(ctx->udpSock, &wsaOverlapped, recved, TRUE, &flags)) {
            WSAResetEvent(wsaOverlapped.hEvent);
            wsaOverlapped.hEvent = INVALID_HANDLE_VALUE;
            *data = buffer;
            return AJ_OK;
        } else {
            AJ_ErrPrintf(("AJ_ARDP_UDP_Recv(): WSAGetOverlappedResult error; WSAGetLastError()=%d\n", WSAGetLastError()));
            return AJ_ERR_READ;
        }
        break;

    case WSA_WAIT_EVENT_0 + 1:
        WSAResetEvent(interruptEvent);
        return AJ_ERR_INTERRUPTED;

    case WSA_WAIT_TIMEOUT:
        return AJ_ERR_TIMEOUT;
        break;

    default:
        AJ_ErrPrintf(("AJ_ARDP_UDP_Recv(): WSAWaitForMultipleEvents error; WSAGetLastError()=%d\n", WSAGetLastError()));
        return AJ_ERR_READ;
    }
}

static AJ_Status AJ_Net_ARDP_Connect(AJ_BusAttachment* bus, const AJ_Service* service)
{
    SOCKET udpSock = INVALID_SOCKET;
    AJ_Status status;
    SOCKADDR_STORAGE addrBuf;
    socklen_t addrSize;
    DWORD ret;

    AJ_ARDP_InitFunctions(AJ_ARDP_UDP_Recv, AJ_ARDP_UDP_Send);

    // otherwise backpressure is guaranteed!
    assert(sizeof(txData) <= UDP_SEGMAX * (UDP_SEGBMAX - ARDP_HEADER_SIZE - UDP_HEADER_SIZE));

    memset(&addrBuf, 0, sizeof(addrBuf));

    if (service->addrTypes & AJ_ADDR_UDP4) {
        struct sockaddr_in* sa = (struct sockaddr_in*) &addrBuf;
        udpSock = WSASocket(AF_INET, SOCK_DGRAM, IPPROTO_UDP, NULL, 0, WSA_FLAG_OVERLAPPED);
        if (udpSock == INVALID_SOCKET) {
            AJ_ErrPrintf(("AJ_Net_ARDP_Connect(): socket() failed.  status=AJ_ERR_CONNECT\n"));
            goto ConnectError;
        }

        sa->sin_family = AF_INET;
        sa->sin_port = htons(service->ipv4portUdp);
        sa->sin_addr.s_addr = service->ipv4Udp;
        addrSize = sizeof(struct sockaddr_in);
        AJ_InfoPrintf(("AJ_Net_ARDP_Connect(): Connect to \"%s:%u\"\n", inet_ntoa(sa->sin_addr), service->ipv4portUdp));;
    } else if (service->addrTypes & AJ_ADDR_UDP6) {
        struct sockaddr_in6* sa = (struct sockaddr_in6*) &addrBuf;
        udpSock = WSASocket(AF_INET6, SOCK_DGRAM, IPPROTO_UDP, NULL, 0, WSA_FLAG_OVERLAPPED);
        if (udpSock == INVALID_SOCKET) {
            AJ_ErrPrintf(("AJ_Net_ARDP_Connect(): socket() failed.  status=AJ_ERR_CONNECT\n"));
            goto ConnectError;
        }

        sa->sin6_family = AF_INET6;
        sa->sin6_port = htons(service->ipv6portUdp);
        memcpy(sa->sin6_addr.s6_addr, service->ipv6Udp, sizeof(sa->sin6_addr.s6_addr));
        addrSize = sizeof(struct sockaddr_in6);
    } else {
        AJ_ErrPrintf(("AJ_Net_ARDP_Connect(): Invalid addrTypes %u, status=AJ_ERR_CONNECT\n", service->addrTypes));
        return AJ_ERR_CONNECT;
    }


    ret = connect(udpSock, (struct sockaddr*) &addrBuf, addrSize);
    // must do this before calling AJ_MarshalMethodCall!
    if (ret == SOCKET_ERROR) {
        AJ_ErrPrintf(("AJ_Net_Connect(): connect() failed. WSAGetLastError()=0x%x, status=AJ_ERR_CONNECT\n", WSAGetLastError()));
        goto ConnectError;
    } else {
        netContext.udpSock = udpSock;
        AJ_IOBufInit(&bus->sock.rx, rxData, sizeof(rxData), AJ_IO_BUF_RX, &netContext);
        bus->sock.rx.recv = AJ_ARDP_Recv;
        AJ_IOBufInit(&bus->sock.tx, txData, sizeof(txData), AJ_IO_BUF_TX, &netContext);
        bus->sock.tx.send = AJ_ARDP_Send;

        sendEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
        recvEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
        interruptEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
        wsaOverlapped.hEvent = INVALID_HANDLE_VALUE;
    }

    status = AJ_ARDP_UDP_Connect(bus, &netContext, service, &bus->sock);
    if (status != AJ_OK) {
        goto ConnectError;
    }

    return AJ_OK;

ConnectError:

    if (udpSock != INVALID_SOCKET) {
        closesocket(udpSock);
    }

    return AJ_ERR_CONNECT;
}

#endif // AJ_ARDP
