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

#ifdef AJ_SERIAL_CONNECTION

/**
 * Per-module definition of the current module for debug logging.  Must be defined
 * prior to first inclusion of aj_debug.h
 */
#define AJ_MODULE SERIAL

#include "aj_target.h"
#include "aj_status.h"
#include "aj_serial.h"
#include "aj_serial_rx.h"
#include "aj_serial_tx.h"
#include "aj_debug.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgSERIAL = 0;
#endif

#define SLAP_VERSION 1

/**
 * SLAP added a disconnect feature in version 1
 */
const uint8_t SLAP_VERSION_DISCONNECT_FEATURE = 1;

/**
 * global variable for link parameters
 */
AJ_LinkParameters AJ_SerialLinkParams;

/**
 * controls rate at which we send synch packets when the link is down in milliseconds
 */
const uint32_t CONN_TIMEOUT = 200;

/**
 * controls how long we will wait for a confirmation packet after we synchronize in milliseconds
 */
const uint32_t NEGO_TIMEOUT = 200;

/**
 * controls how long we will send disconnect packet when the application is closing the link in milliseconds
 */
const uint32_t DISC_TIMEOUT = 200;


#define LINK_PACKET_SIZE    4
#define NEGO_PACKET_SIZE    3

/** link packet types */
typedef enum {
    UNKNOWN_PKT = 0,
    CONN_PKT,
    ACPT_PKT,
    NEGO_PKT,
    NRSP_PKT,
    RESU_PKT,
    DISC_PKT,
    DRSP_PKT
} LINK_PKT_TYPE;


/*
 * link establishment packets
 */
static const char ConnPkt[LINK_PACKET_SIZE] = { 'C', 'O', 'N', 'N' };
static const char AcptPkt[LINK_PACKET_SIZE] = { 'A', 'C', 'P', 'T' };
static const char NegoPkt[LINK_PACKET_SIZE] = { 'N', 'E', 'G', 'O' };
static const char NrspPkt[LINK_PACKET_SIZE] = { 'N', 'R', 'S', 'P' };
static const char ResuPkt[LINK_PACKET_SIZE] = { 'R', 'E', 'S', 'U' };
static const char DiscPkt[LINK_PACKET_SIZE] = { 'D', 'I', 'S', 'C' };
static const char DrspPkt[LINK_PACKET_SIZE] = { 'D', 'R', 'S', 'P' };

/**
 * Time to send the LinkPacket
 */
static AJ_Time sendLinkPacketTime;


/**
 * link configuration information
 */
static uint8_t NegotiationPacket[LINK_PACKET_SIZE + NEGO_PACKET_SIZE];

/************ forward declarations *****************/

static void ScheduleLinkControlPacket(uint32_t timeout);

/********* end of forward declarations *************/

// calculate the timeout values in milliseconds based on transmission paramters
static void AdjustTimeoutValues(uint32_t packetSize)
{
    // one start bit, eight data bits, one parity, and one stop bit equals eleven bits sent per byte
    // acknowledgement packets should be sent within twice the time to send one packet
    // resends should be sent shortly after three times the packet delivery time
    AJ_SerialLinkParams.txAckTimeout = (packetSize * 11 * 1000 * 2) / AJ_SerialLinkParams.bitRate;
    AJ_SerialLinkParams.txResendTimeout = (packetSize * 11 * 1000 * 3) / AJ_SerialLinkParams.bitRate;
    AJ_InfoPrintf(("new ack timeout %i, new resend timeout %i\n", AJ_SerialLinkParams.txAckTimeout,  AJ_SerialLinkParams.txResendTimeout));
}

// Converge the remote endpoint's values with my own
static void ProcessNegoPacket(const uint8_t* buffer)
{
    uint16_t max_payload;
    uint8_t proto_version;
    uint8_t window_size;

    max_payload = (((uint16_t) buffer[4]) << 8) | ((uint16_t) buffer[5]);
    AJ_AlwaysPrintf(("Read max payload: %u\n", max_payload));
    AJ_SerialLinkParams.packetSize = min(AJ_SerialLinkParams.packetSize, max_payload);

    proto_version = (buffer[6] >> 2) & 0x003F;
    AJ_AlwaysPrintf(("Read protocol version: %u\n", proto_version));
    AJ_SerialLinkParams.protoVersion = min(AJ_SerialLinkParams.protoVersion, proto_version);

    // last two bits
    window_size = buffer[6] & 0x03;
    // translate the window size
    switch (window_size) {
    case 0:
        window_size = 1;
        break;

    case 1:
        window_size = 2;
        break;

    case 2:
        window_size = 4;
        break;

    case 4:
        window_size = 8;
        break;

    default:
        AJ_AlwaysPrintf(("Invalid window size: %u\n", window_size));
        break;
    }


    AJ_AlwaysPrintf(("Read max window size: %u\n", window_size));
    AJ_SerialLinkParams.windowSize = min(AJ_SerialLinkParams.maxWindowSize, window_size);
}

static void SendNegotiationPacket(const char* pkt_type)
{
    uint8_t encoded_window_size = 0;

    // NegoPkt is the packet type
    memset(&NegotiationPacket[0], 0, sizeof(NegotiationPacket));
    // can be NEGO or NRSP
    memcpy(&NegotiationPacket[0], pkt_type, LINK_PACKET_SIZE);

    // max payload length we can support
    NegotiationPacket[4] = (AJ_SerialLinkParams.packetSize & 0xFF00) >> 8;
    NegotiationPacket[5] = (AJ_SerialLinkParams.packetSize & 0x00FF);

    switch (AJ_SerialLinkParams.maxWindowSize) {
    case 1:
        encoded_window_size = 0;
        break;

    case 2:
        encoded_window_size = 1;
        break;

    case 4:
        encoded_window_size = 2;
        break;

    case 8:
        encoded_window_size = 3;
        break;

    default:
        break;
    }

    NegotiationPacket[6] = (AJ_SerialLinkParams.protoVersion << 2) | encoded_window_size;
    AJ_SerialTX_EnqueueCtrl(NegotiationPacket, sizeof(NegotiationPacket), AJ_SERIAL_CTRL);
}


/**
 * a function that periodically sends sync or conf packets depending on
 * the link state
 */
static void SendLinkPacket()
{
    switch (AJ_SerialLinkParams.linkState) {
    case AJ_LINK_UNINITIALIZED:
        /*
         * Send a sync packet.
         */
        AJ_SerialTX_EnqueueCtrl((uint8_t*) ConnPkt, sizeof(ConnPkt), AJ_SERIAL_CTRL);
        AJ_AlwaysPrintf(("Send CONN\n"));
        ScheduleLinkControlPacket(CONN_TIMEOUT);
        break;

    case AJ_LINK_INITIALIZED:
        /*
         * Send a conf packet.
         */
        SendNegotiationPacket(NegoPkt);
        AJ_AlwaysPrintf(("Send NEGO\n"));
        ScheduleLinkControlPacket(NEGO_TIMEOUT);
        break;

    case AJ_LINK_DYING:
        /*
         * only send a DISC packet to daemons that understand them.
         */
        if (AJ_SerialLinkParams.protoVersion >= SLAP_VERSION_DISCONNECT_FEATURE) {
            AJ_SerialTX_EnqueueCtrl((uint8_t*) DiscPkt, sizeof(DiscPkt), AJ_SERIAL_CTRL);
            AJ_InfoPrintf(("Send DISC\n"));
            ScheduleLinkControlPacket(DISC_TIMEOUT);
            break;
        }

    default:
        /**
         * Do nothing. No more link control packets to be sent.
         */
        ScheduleLinkControlPacket(AJ_TIMER_FOREVER);
        break;

    }
}

/**
 * This function registers the time to send a link control packet
 * specified by the timeout parameter.
 *
 * @param timeout   time (in milliseconds) when the link control packet
 *                  must be sent
 */
static void ScheduleLinkControlPacket(uint32_t timeout)
{
    AJ_InitTimer(&sendLinkPacketTime);
    AJ_TimeAddOffset(&sendLinkPacketTime, timeout);
}

/**
 * This function returns a link packet type, given a packet as an argument.
 *
 *  @param buffer    pointer to the buffer holding the packet
 *  @param bufLen    size of the buffer
 */
static LINK_PKT_TYPE ClassifyPacket(uint8_t* buffer, uint16_t bufLen)
{
    if (bufLen < LINK_PACKET_SIZE) {
        return UNKNOWN_PKT;
    }

    if (0 == memcmp(buffer, ConnPkt, sizeof(ConnPkt))) {
        return CONN_PKT;
    } else if (0 == memcmp(buffer, AcptPkt, sizeof(AcptPkt))) {
        return ACPT_PKT;
    } else if (0 == memcmp(buffer, NegoPkt, sizeof(NegoPkt))) {
        return NEGO_PKT;
    } else if (0 == memcmp(buffer, NrspPkt, sizeof(NrspPkt))) {
        return NRSP_PKT;
    } else if (0 == memcmp(buffer, ResuPkt, sizeof(ResuPkt))) {
        return RESU_PKT;
    } else if (0 == memcmp(buffer, DiscPkt, sizeof(DiscPkt))) {
        return DISC_PKT;
    } else if (0 == memcmp(buffer, DrspPkt, sizeof(DrspPkt))) {
        return DRSP_PKT;
    }

    return UNKNOWN_PKT;
}


void AJ_Serial_LinkPacket(uint8_t* buffer, uint16_t len)
{
    LINK_PKT_TYPE pktType = ClassifyPacket(buffer, len);

    if (pktType == UNKNOWN_PKT) {
        AJ_WarnPrintf(("Unknown link packet type %x\n", (int) pktType));
        return;
    }

    switch (AJ_SerialLinkParams.linkState) {
    case AJ_LINK_UNINITIALIZED:
        /*
         * In the uninitialized state we need to respond to conn packets
         * and acpt packets.
         */
        if (pktType == CONN_PKT) {
            AJ_SerialTX_EnqueueCtrl((uint8_t*) AcptPkt, sizeof(AcptPkt), AJ_SERIAL_CTRL);
        } else if (pktType == ACPT_PKT) {
            AJ_InfoPrintf(("Received sync response - moving to LINK_INITIALIZED\n"));
            AJ_SerialLinkParams.linkState = AJ_LINK_INITIALIZED;
            SendNegotiationPacket(NegoPkt);
        }

        break;

    case AJ_LINK_INITIALIZED:
        /*
         * In the initialized state we need to respond to conn packets, nego
         * packets, and nego-resp packets.
         */
        if (pktType == CONN_PKT) {
            AJ_SerialTX_EnqueueCtrl((uint8_t*) AcptPkt, sizeof(AcptPkt), AJ_SERIAL_CTRL);
        } else if (pktType == NEGO_PKT) {
            ProcessNegoPacket(buffer);
            SendNegotiationPacket(NrspPkt);
        } else if (pktType == NRSP_PKT) {
            AJ_InfoPrintf(("Received nego response - Moving to LINK_ACTIVE\n"));
            AJ_SerialLinkParams.linkState = AJ_LINK_ACTIVE;

            // update the timeout values now that the link is active
            AdjustTimeoutValues(AJ_SerialLinkParams.packetSize);
        }
        break;

    case AJ_LINK_ACTIVE:
        /*
         * In the initialized state we need to respond to nego-resp packets.
         */
        if (pktType == NEGO_PKT) {
            ProcessNegoPacket(buffer);
            SendNegotiationPacket(NrspPkt);
        } else if (pktType == CONN_PKT) {
            // got a connection after active, so the link must have gone down without our knowledge
            AJ_WarnPrintf(("Received CONN while in active state - Moving to LINK_DEAD\n"));
            AJ_SerialLinkParams.linkState = AJ_LINK_DEAD;
        } else if (pktType == DISC_PKT) {
            // received a disconnect packet, move to link dying state.
            AJ_WarnPrintf(("Received DISC while in active state - other side is going away.\n"));
            AJ_SerialTX_EnqueueCtrl((uint8_t*) DrspPkt, sizeof(DrspPkt), AJ_SERIAL_CTRL);
            AJ_SerialLinkParams.linkState = AJ_LINK_DYING;
        }
        break;

    case AJ_LINK_DYING:
        if (pktType == DISC_PKT) {
            // simultaneous DISC while in dying state, send a DRSP to cleanly shutdown the other side.
            AJ_WarnPrintf(("Received DISC while in dying state.\n"));
            AJ_SerialLinkParams.linkState = AJ_LINK_DEAD;
            AJ_SerialTX_EnqueueCtrl((uint8_t*) DrspPkt, sizeof(DrspPkt), AJ_SERIAL_CTRL);
        } else if (pktType == DRSP_PKT) {
            AJ_WarnPrintf(("Received DRSP while in dying state - other side knows we are going away.\n"));
            AJ_SerialLinkParams.linkState = AJ_LINK_DEAD;
        }
        break;

    case AJ_LINK_DEAD:
        break;


    }

    /*
     * Ignore any other packets.
     */
    AJ_ErrPrintf(("Discarding link packet %d in state %d\n", pktType, AJ_SerialLinkParams.linkState));
}

AJ_Status AJ_SerialInit(const char* ttyName,
                        uint32_t bitRate,
                        uint8_t windowSize,
                        uint16_t packetSize)
{
    AJ_Status status;
    if ((windowSize < MIN_WINDOW_SIZE) || (windowSize > MAX_WINDOW_SIZE)) {
        return AJ_ERR_INVALID;
    }

    AJ_AlwaysPrintf(("Initializing serial transport\n"));

    /** Initialize protocol default values */
    AJ_SerialLinkParams.protoVersion = SLAP_VERSION;
    AJ_SerialLinkParams.maxWindowSize = windowSize;
    AJ_SerialLinkParams.windowSize = windowSize;
    AJ_SerialLinkParams.packetSize = packetSize;
    AJ_SerialLinkParams.linkState = AJ_LINK_UNINITIALIZED;
    AJ_SerialLinkParams.bitRate = bitRate;
    AdjustTimeoutValues(AJ_LINK_PACKET_PAYLOAD);

    /** Initialize serial ports */
    status = AJ_SerialTargetInit(ttyName, bitRate);
    if (status != AJ_OK) {
        return status;
    }

    /** Initialize transmit buffers, state */
    status = AJ_SerialTX_Init();
    if (status != AJ_OK) {
        return status;
    }

    /** Initialize receive buffers, state */
    status = AJ_SerialRX_Init();
    if (status != AJ_OK) {
        return status;
    }

    AJ_SerialLinkParams.linkState = AJ_LINK_UNINITIALIZED;
    ScheduleLinkControlPacket(10);
    return AJ_OK;
}


void AJ_SerialShutdown(void)
{
    AJ_SerialTX_Shutdown();
    AJ_SerialRX_Shutdown();
}


extern AJ_Time resendTime;
extern AJ_Time ackTime;
volatile int dataReceived;
volatile int dataSent = 1;

/** This state machine is called from AJ_SerialRecv and AJ_SerialSend.
 * This processes any buffers copied in the Recieve Callback, adds
 * buffers to be written in the Transmit Callback, resends packets
 * after a timeout and sends pure acks in the case of lack of data to send.
 * Also, this sends link control packets periodically depending on the state.
 */
void AJ_StateMachine()
{
    AJ_Time now;
    AJ_InitTimer(&now);

    if (dataReceived) {
        /* Data has been received in the receive callback, process the data,
         * convert it into SLAP packets, validate and process the packets */
        AJ_ProcessRxBufferList();
    }

    if (dataSent) {
        /* There is space in the transmit free list, queue up more buffers to be
         * sent if there are SLAP packets to be sent
         */
        AJ_FillTxBufferList();
    }

    if (AJ_CompareTime(resendTime, now) < 0) {
        /* Resend any data packets that have not been acked */
        ResendPackets();
    }

    if (AJ_CompareTime(ackTime, now) < 0) {
        /* Send an ack for a received packet.(If there is data to send,
         * the ack is sent as a part of the header, but in this case,
         * this end didnt have data to send, so we send an explicit
         * ack packet.)
         */
        SendAck();
    }

    if (AJ_CompareTime(sendLinkPacketTime, now) < 0) {
        /* Time to send a link packet to get the Link to the active state. */
        SendLinkPacket();
    }
}

void ClearSlippedBuffer(volatile AJ_SlippedBuffer* buf)
{
    while (buf != NULL) {
        volatile AJ_SlippedBuffer* prev = buf;
        AJ_Free(buf->buffer);
        buf = buf->next;
        AJ_Free((void*) prev);
    }
}

void AJ_SerialDisconnect(void)
{
    AJ_Time start, now;
    AJ_InitTimer(&start);

    if (AJ_SerialLinkParams.linkState != AJ_LINK_DEAD) {
        AJ_SerialLinkParams.linkState = AJ_LINK_DYING;
        ScheduleLinkControlPacket(DISC_TIMEOUT);

        // Run the state machine up to 4 timeouts to wait for DRSP packets
        do {
            AJ_StateMachine();
            AJ_InitTimer(&now);
        } while (AJ_SerialLinkParams.linkState != AJ_LINK_DEAD && AJ_GetTimeDifference(&now, &start) < DISC_TIMEOUT * 4);

        if (AJ_SerialLinkParams.linkState != AJ_LINK_DEAD) {
            AJ_InfoPrintf(("serial link wasn't gracefully disconnected\n"));
            AJ_SerialLinkParams.linkState = AJ_LINK_DEAD;
        }

    }
}

#endif /* AJ_SERIAL_CONNECTION */
