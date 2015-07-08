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

#define AJ_MODULE NET

#include "aj_target.h"
#include "aj_bufio.h"
#include "aj_net.h"
#include "aj_util.h"
#include "aj_debug.h"

#ifdef WIFI_UDP_WORKING
#include <SPI.h>
#include <WiFi.h>
#include <WiFiClient.h>
#include <WiFiUdp.h>
#else
#include <SPI.h>
#include <Ethernet.h>
#include <EthernetUdp.h>
#endif

static uint8_t rxDataStash[256];
static uint16_t rxLeftover = 0;

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

#ifdef WIFI_UDP_WORKING
static WiFiClient g_client;
static WiFiUDP g_clientUDP;
#else
static EthernetClient g_client;
static EthernetUDP g_clientUDP;
#endif

AJ_Status AJ_Net_Send(AJ_IOBuffer* buf)
{
    uint32_t ret;
    uint32_t tx = AJ_IO_BUF_AVAIL(buf);

    AJ_InfoPrintf(("AJ_Net_Send(buf=0x%p)\n", buf));

    if (tx > 0) {
        ret = g_client.write(buf->readPtr, tx);
        if (ret == 0) {
            AJ_ErrPrintf(("AJ_Net_Send(): send() failed. error=%d, status=AJ_ERR_WRITE\n", g_client.getWriteError()));
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
    AJ_Status status = AJ_ERR_READ;
    uint32_t ret;
    uint32_t rx = AJ_IO_BUF_SPACE(buf);
    uint32_t recvd = 0;
    unsigned long Recv_lastCall = millis();

    // first we need to clear out our buffer
    uint32_t M = 0;

    AJ_InfoPrintf(("AJ_Net_Recv(buf=0x%p, len=%d., timeout=%d.)\n", buf, len, timeout));

    if (rxLeftover != 0) {
        // there was something leftover from before,
        AJ_InfoPrintf(("AJ_NetRecv(): leftover was: %d\n", rxLeftover));
        M = min(rx, rxLeftover);
        memcpy(buf->writePtr, rxDataStash, M);  // copy leftover into buffer.
        buf->writePtr += M;  // move the data pointer over
        memmove(rxDataStash, rxDataStash + M, rxLeftover - M); // shift left-overs toward the start.
        rxLeftover -= M;
        recvd += M;

        // we have read as many bytes as we can
        // higher level isn't requesting any more
        if (recvd == rx) {
            AJ_InfoPrintf(("AJ_Net_Recv(): status=AJ_OK\n"));
            return AJ_OK;
        }
    }

    if ((M != 0) && (rxLeftover != 0)) {
        AJ_InfoPrintf(("AJ_Net_REcv(): M was: %d, rxLeftover was: %d\n", M, rxLeftover));
    }

    // wait for data to become available
    // time out if nothing arrives
    while (g_client.connected() &&
           g_client.available() == 0 &&
           (millis() - Recv_lastCall < timeout)) {
        delay(50); // wait for data or timeout
    }

    // return timeout if nothing is available
    AJ_InfoPrintf(("AJ_Net_Recv(): millis %d, Last_call %d timeout %d Avail: %d\n", millis(), Recv_lastCall, timeout, g_client.available()));
    if (g_client.connected() && (millis() - Recv_lastCall >= timeout) && (g_client.available() == 0)) {
        AJ_InfoPrintf(("AJ_Net_Recv(): timeout. status=AJ_ERR_TIMEOUT\n"));
        return AJ_ERR_TIMEOUT;
    }

    if (g_client.connected()) {
        uint32_t askFor = rx;
        askFor -= M;
        AJ_InfoPrintf(("AJ_Net_Recv(): ask for: %d\n", askFor));
        ret = g_client.read(buf->writePtr, askFor);
        AJ_InfoPrintf(("AJ_Net_Recv(): read(): ret %d  askfor %d\n", ret, askFor));

        if (askFor < ret) {
            AJ_InfoPrintf(("AJ_Net_Recv(): BUFFER OVERRUN: askFor=%u, ret=%u\n", askFor, ret));
        }

        if (ret == -1) {
            AJ_ErrPrintf(("AJ_Net_Recv(): read() failed. status=AJ_ERR_READ\n"));
            status = AJ_ERR_READ;
        } else {
            AJ_InfoPrintf(("AJ_Net_Recv(): ret now %d\n", ret));
            AJ_DumpBytes("Recv", buf->writePtr, ret);

            if (ret > askFor) {
                AJ_InfoPrintf(("AJ_Net_Recv(): new leftover %d\n", ret - askFor));
                // now shove the extra into the stash
                memcpy(rxDataStash + rxLeftover, buf->writePtr + askFor, ret - askFor);
                rxLeftover += (ret - askFor);
                buf->writePtr += rx;
            } else {
                buf->writePtr += ret;
            }
            status = AJ_OK;
        }
    }

    return status;
}

/*
 * Need enough RX buffer space to receive a complete name service packet when
 * used in UDP mode.  NS expects MTU of 1500 subtracts UDP, IP and Ethernet
 * Type II overhead.  1500 - 8 -20 - 18 = 1454.  txData buffer size needs to
 * be big enough to hold a NS WHO-HAS for one name (4 + 2 + 256 = 262) in UDP
 * mode.  TCP buffer size dominates in that case.
 */
static uint8_t rxData[1454];
static uint8_t txData[1024];

AJ_Status AJ_Net_Connect(AJ_BusAttachment* bus, const AJ_Service* service)
{
    int ret;
    IPAddress ip(service->ipv4);

    if (!(service->addrTypes & AJ_ADDR_TCP4)) {
        AJ_ErrPrintf(("AJ_Net_Connect(): only IPV4 TCP supported\n", ret));
        return AJ_ERR_CONNECT;
    }


    AJ_InfoPrintf(("AJ_Net_Connect(netSock=0x%p, addrType=%d.)\n", netSock, addrType));

    AJ_InfoPrintf(("AJ_Net_Connect(): Connect to 0x%x:%u.\n", addr, port));;

    ret = g_client.connect(ip, service->ipv4port);

#ifdef NOTDEF
    Serial.print("Connecting to: ");
    Serial.print(ip);
    Serial.print(':');
    Serial.println(port);
#endif

    if (ret == -1) {
        AJ_ErrPrintf(("AJ_Net_Connect(): connect() failed: %d: status=AJ_ERR_CONNECT\n", ret));
        return AJ_ERR_CONNECT;
    } else {
        AJ_IOBufInit(&bus->sock.rx, rxData, sizeof(rxData), AJ_IO_BUF_RX, (void*)&g_client);
        bus->sock.rx.recv = AJ_Net_Recv;
        AJ_IOBufInit(&bus->sock.tx, txData, sizeof(txData), AJ_IO_BUF_TX, (void*)&g_client);
        bus->sock.tx.send = AJ_Net_Send;
        AJ_ErrPrintf(("AJ_Net_Connect(): connect() success: status=AJ_OK\n"));
        return AJ_OK;
    }
    AJ_ErrPrintf(("AJ_Net_Connect(): connect() failed: %d: status=AJ_ERR_CONNECT\n", ret));
    return AJ_ERR_CONNECT;
}

void AJ_Net_Disconnect(AJ_NetSocket* netSock)
{
    AJ_InfoPrintf(("AJ_Net_Disconnect(nexSock=0x%p)\n", netSock));
    g_client.stop();
}

AJ_Status AJ_Net_SendTo(AJ_IOBuffer* buf)
{
    int ret;
    uint32_t tx = AJ_IO_BUF_AVAIL(buf);

    AJ_InfoPrintf(("AJ_Net_SendTo(buf=0x%p)\n", buf));

    if (tx > 0) {
        // send to subnet-directed broadcast address
        IPAddress subnet = Ethernet.subnetMask();
        IPAddress localIp = Ethernet.localIP();
        uint32_t directedBcastAddr = (uint32_t(subnet) & uint32_t(localIp)) | (~uint32_t(subnet));
        IPAddress a(directedBcastAddr);
        ret = g_clientUDP.beginPacket(IPAddress(directedBcastAddr), AJ_UDP_PORT);
        AJ_InfoPrintf(("AJ_Net_SendTo(): beginPacket to %d.%d.%d.%d, result = %d\n", a[0], a[1], a[2], a[3], ret));
        if (ret == 0) {
            AJ_InfoPrintf(("AJ_Net_SendTo(): no sender\n"));
        }

        ret = g_clientUDP.write(buf->readPtr, tx);
        AJ_InfoPrintf(("AJ_Net_SendTo(): SendTo write %d\n", ret));
        if (ret == 0) {
            AJ_ErrPrintf(("AJ_Net_Sendto(): no bytes. status=AJ_ERR_WRITE\n"));
            return AJ_ERR_WRITE;
        }

        buf->readPtr += ret;

        ret = g_clientUDP.endPacket();
        if (ret == 0) {
            AJ_ErrPrintf(("AJ_Net_Sendto(): endPacket() error. status=AJ_ERR_WRITE\n"));
            return AJ_ERR_WRITE;
        }

    }
    AJ_IO_BUF_RESET(buf);
    AJ_InfoPrintf(("AJ_Net_SendTo(): status=AJ_OK\n"));
    return AJ_OK;
}

AJ_Status AJ_Net_RecvFrom(AJ_IOBuffer* buf, uint32_t len, uint32_t timeout)
{
    AJ_InfoPrintf(("AJ_Net_RecvFrom(buf=0x%p, len=%d., timeout=%d.)\n", buf, len, timeout));

    AJ_Status status = AJ_OK;
    int ret;
    uint32_t rx = AJ_IO_BUF_SPACE(buf);
    unsigned long Recv_lastCall = millis();

    AJ_InfoPrintf(("AJ_Net_RecvFrom(): len %d, rx %d, timeout %d\n", len, rx, timeout));

    rx = min(rx, len);

    while ((g_clientUDP.parsePacket() == 0) && (millis() - Recv_lastCall < timeout)) {
        delay(10); // wait for data or timeout
    }

    AJ_InfoPrintf(("AJ_Net_RecvFrom(): millis %d, Last_call %d, timeout %d, Avail %d\n", millis(), Recv_lastCall, timeout, g_clientUDP.available()));
    ret = g_clientUDP.read(buf->writePtr, rx);
    AJ_InfoPrintf(("AJ_Net_RecvFrom(): read() returns %d, rx %d\n", ret, rx));

    if (ret == -1) {
        AJ_InfoPrintf(("AJ_Net_RecvFrom(): read() fails. status=AJ_ERR_READ\n"));
        status = AJ_ERR_READ;
    } else {
        if (ret != -1) {
            AJ_DumpBytes("AJ_Net_RecvFrom", buf->writePtr, ret);
        }
        buf->writePtr += ret;
        AJ_InfoPrintf(("AJ_Net_RecvFrom(): status=AJ_OK\n"));
        status = AJ_OK;
    }
    AJ_InfoPrintf(("AJ_Net_RecvFrom(): status=%s\n", AJ_StatusText(status)));
    return status;
}

uint16_t AJ_EphemeralPort(void)
{
    // Return a random port number in the IANA-suggested range
    return 49152 + random(65535 - 49152);
}

AJ_Status AJ_Net_MCastUp(AJ_NetSocket* netSock)
{
    uint8_t ret = 0;

    AJ_InfoPrintf(("AJ_Net_MCastUp(nexSock=0x%p)\n", netSock));

    //
    // Arduino does not choose an ephemeral port if we enter 0 -- it happily
    // uses 0 and then increments each time we bind, up through the well-known
    // system ports.
    //
    ret = g_clientUDP.begin(AJ_EphemeralPort());

    if (ret != 1) {
        g_clientUDP.stop();
        AJ_ErrPrintf(("AJ_Net_MCastUp(): begin() fails. status=AJ_ERR_READ\n"));
        return AJ_ERR_READ;
    } else {
        AJ_IOBufInit(&netSock->rx, rxData, sizeof(rxData), AJ_IO_BUF_RX, (void*)&g_clientUDP);
        netSock->rx.recv = AJ_Net_RecvFrom;
        AJ_IOBufInit(&netSock->tx, txData, sizeof(txData), AJ_IO_BUF_TX, (void*)&g_clientUDP);
        netSock->tx.send = AJ_Net_SendTo;
    }

    AJ_InfoPrintf(("AJ_Net_MCastUp(): status=AJ_OK\n"));
    return AJ_OK;
}

void AJ_Net_MCastDown(AJ_NetSocket* netSock)
{
    AJ_InfoPrintf(("AJ_Net_MCastDown(nexSock=0x%p)\n", netSock));
    g_clientUDP.flush();
    g_clientUDP.stop();
}
