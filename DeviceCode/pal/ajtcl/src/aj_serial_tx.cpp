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
#define AJ_MODULE SERIAL_TX

#include "aj_target.h"
#include "aj_status.h"
#include "aj_serial.h"
#include "aj_serial_rx.h"
#include "aj_serial_tx.h"
#include "aj_crc16.h"
#include "aj_util.h"
#include "aj_debug.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgSERIAL_TX = 0;
#endif

/**
 * Throughput may be improved by always ack'ing received packets immediately.
 */
//#define ALWAYS_ACK   1

/*
 * transmit packet
 */
typedef struct _TxPkt {
    struct _TxPkt volatile* next;
    uint16_t len;
    uint8_t seq;
    uint8_t type;
    uint8_t* payload;
} TxPkt;


/**
 * packets queued and ready to send
 */
static TxPkt volatile* txQueue;

/**
 * packets sent but not yet acknowledged
 */
static TxPkt volatile* txSent;

/**
 * Free list for data packets
 */
static TxPkt volatile* txFreeList;

/**
 * packet reserved for sending unreliable (control and ack) packets
 */
static TxPkt volatile* txUnreliable;

/* Linked list of slipped buffers to be transmitted */
static AJ_SlippedBuffer volatile* bufferTxPending;

/* Linked list of buffers that can be filled with slipped SLAP packets
 * and transferred into bufferTxPending
 */
static AJ_SlippedBuffer volatile* bufferTxFreeList;

/* The buffer currently being sent by the Transmit callback
 */
static AJ_SlippedBuffer volatile* pendingSendBuffer;

/**
 * current transmit sequence number
 */
static uint8_t txSeqNum;


/**
 * number of received packets waiting to be ACKed
 */
static uint8_t pendingAcks;

/**
 * sequence number of the packet we expect to ACK next
 */
static uint8_t currentTxAck;

static uint8_t resendPrimed = FALSE;

/**
 * When to resend un-acked packets
 */
AJ_Time resendTime;

/**
 * When to send explicit ACK
 */
AJ_Time ackTime;

/****************** Forward declarations *******************/

void ResendTimer(uint32_t timerId, void* context);

void SendPureAck(uint32_t timerId, void* context);

extern AJ_Status AJ_UART_Tx(uint8_t* buffer, uint16_t len);

extern void __AJ_TX(uint8_t* buf, uint32_t len);


/************** End of forward declarations *****************/

#ifdef AJ_DEBUG_SERIAL_SEND
#define AJ_DebugDumpSerialSend(a, b, c) AJ_DumpBytes(a, b, c)
#else
#define AJ_DebugDumpSerialSend(a, b, c)
#endif

void AJ_TransmitCallback(uint8_t* buffer, uint16_t bytesRead);

AJ_Status AJ_SerialTX_Init()
{
    int i;
    TxPkt volatile* prev;

    if (AJ_SerialLinkParams.packetSize == 0) {
        return AJ_ERR_DRIVER;
    }

    /*
     * Initialize local static data
     */
    txQueue = NULL;
    txSent = NULL;
    txFreeList = NULL;
    txUnreliable = NULL;
    txSeqNum = 0;
    resendPrimed = FALSE;
    pendingAcks = 0;
    currentTxAck = 0;
    dataSent = 1;

    /*
     * Data packets: To maximize throughput we need as many packets as the
     * window size.
     */
    for (i = 0; i < AJ_SerialLinkParams.maxWindowSize; ++i) {
        void* payload;
        prev = txFreeList;
        txFreeList = AJ_Malloc(sizeof(TxPkt));
        payload = AJ_Malloc(AJ_SerialLinkParams.packetSize);
        if (!txFreeList || !payload) {
            return AJ_ERR_RESOURCES;
        }
        txFreeList->payload = payload;
        txFreeList->next = prev;
    }

    AJ_SlippedBuffer volatile* prevBuf = NULL;
    bufferTxFreeList = NULL;
    for (i = 0; i < AJ_SerialLinkParams.maxWindowSize; i++) {
        void* buf;
        prevBuf = bufferTxFreeList;
        bufferTxFreeList = AJ_Malloc(sizeof(AJ_SlippedBuffer));
        buf = AJ_Malloc(SLIPPED_LEN(AJ_SerialLinkParams.packetSize)); //TODO: calculate slipped length based on packet size
        if (!bufferTxFreeList || !buf) {
            return AJ_ERR_RESOURCES;
        }
        bufferTxFreeList->buffer = buf;
        bufferTxFreeList->actualLen = 0;
        bufferTxFreeList->allocatedLen = SLIPPED_LEN(AJ_SerialLinkParams.packetSize);
        bufferTxFreeList->next = prevBuf;
    }

    prevBuf = bufferTxFreeList;

    /*
     * Buffer for unreliable packets
     */
    txUnreliable = AJ_Malloc(sizeof(TxPkt));
    memset((void*)txUnreliable, 0, sizeof(TxPkt));
    txUnreliable->payload = AJ_Malloc(AJ_LINK_PACKET_PAYLOAD);

    AJ_InitTimer(&resendTime);
    AJ_TimeAddOffset(&resendTime, AJ_TIMER_FOREVER);
    resendPrimed = FALSE;

    AJ_InitTimer(&ackTime);
    AJ_TimeAddOffset(&ackTime, AJ_TIMER_FOREVER);

    AJ_SetTxSerialTransmit(&__AJ_TX);
    AJ_SetTxCB(&AJ_TransmitCallback);
    return AJ_OK;
}

static void DeleteTxPacket(volatile TxPkt* pkt)
{
    while (pkt != NULL) {
        volatile TxPkt* prev = pkt;
        pkt = pkt->next;
        AJ_Free(prev->payload);
        AJ_Free((void*) prev);
        prev = NULL;
    }
}

void AJ_SerialTX_Shutdown(void)
{
    AJ_PauseTX();
    // delete the unreliable packet in a moment
    if (txUnreliable == txQueue) {
        txQueue = txUnreliable->next;
        txUnreliable->next = NULL;
    }

    DeleteTxPacket(txQueue);
    txQueue = NULL;
    DeleteTxPacket(txSent);
    txSent = NULL;
    DeleteTxPacket(txFreeList);
    txFreeList = NULL;
    DeleteTxPacket(txUnreliable);
    txUnreliable = NULL;

    ClearSlippedBuffer(bufferTxFreeList);
    bufferTxFreeList = NULL;
    ClearSlippedBuffer(bufferTxPending);
    bufferTxPending = NULL;
    ClearSlippedBuffer(pendingSendBuffer);
    pendingSendBuffer = NULL;
}

/**
 * This function resets the transmit side of the transport.
 */
AJ_Status AJ_SerialTX_Reset(void)
{
    TxPkt volatile* pkt;

    /*
     * Hold the timeouts.
     */
    AJ_InitTimer(&resendTime);
    AJ_TimeAddOffset(&resendTime, AJ_TIMER_FOREVER);

    AJ_InitTimer(&ackTime);
    AJ_TimeAddOffset(&ackTime, AJ_TIMER_FOREVER);

    /*
     * Put ACL packets back on the free list.
     */
    while (txSent != NULL) {
        pkt = txSent;
        txSent = txSent->next;
        if (pkt->type == AJ_SERIAL_DATA) {
            pkt->next = txFreeList;
            txFreeList = pkt;
        }
    }

    while (txQueue != NULL) {
        pkt = txQueue;
        txQueue = txQueue->next;
        if (pkt->type == AJ_SERIAL_DATA) {
            pkt->next = txFreeList;
            txFreeList = pkt;
        }
    }
    /*
     * Re-initialize global state.
     */
    txSeqNum = 0;
    pendingAcks = 0;
    currentTxAck = 0;
    txSeqNum = 0;
    resendPrimed = FALSE;
    return AJ_OK;
}


/**
 * This function is called if an acknowledgement is not received within the required
 * timeout period.
 */
void ResendPackets()
{
    TxPkt volatile* last;

    /*
     * Re-register the send timeout callback, it will not be primed until it
     * is needed.
     */
    resendPrimed = FALSE;
    AJ_InitTimer(&resendTime);
    AJ_TimeAddOffset(&resendTime, AJ_TIMER_FOREVER);

    /*
     * No resends if the link is not up.
     */
    if (AJ_SerialLinkParams.linkState != AJ_LINK_ACTIVE) {
        return;
    }
    /*
     * To preserve packet order, all unacknowleged packets must be resent. This
     * simply means moving packets on txSent to the head of txQueue.
     */
    if (txSent != NULL) {
        last = txSent;
        while (last->next != NULL) {
            last = last->next;
        }
        /*
         * Put resend packets after the unreliable packet.
         */
        if (txQueue == txUnreliable) {
            last->next = txQueue->next;
            txQueue->next = last;
        } else {
            last->next = txQueue;
            txQueue = txSent;
        }

        txSent = NULL;
    }
}

/**
 * Unreliable packets are given priority by putting them at the front of the
 * transmit queue.
 */
static void QueueUnreliable(void)
{
    /*
     * Except for ACK packets, unreliable packets have a zero seq number.
     */
    txUnreliable->seq = (txUnreliable->type == AJ_SERIAL_ACK) ? currentTxAck : 0;
    /*
     * Add to front of transmit queue. It is possible that the unreliable packet
     * structure has already been queued due to a explicit ACK or due to an earlier
     * link control packet.  In any case, because these are unreliable packets it is OK
     * for the new packet to overwrite the older packet. It would NOT be OK for
     * the unreliable packet to get queued twice.
     */
    if (txQueue == txUnreliable) {
        AJ_AlwaysPrintf(("QueueUnreliable: type %i unreliable packet already queued! %p\n", txUnreliable->type, txUnreliable));
    } else {
        txUnreliable->next = txQueue;
        txQueue = txUnreliable;
    }
}


/**
 * Reliable packets are sent in order so are appended to the end of the transmit queue.
 */
static void QueueReliable(TxPkt volatile* pkt)
{
    TxPkt volatile* last;

    pkt->seq = txSeqNum;
    pkt->next = NULL;
    /*
     * updates sequence number
     */
    txSeqNum = (txSeqNum + 1) & 0x7;
    /*
     * Add to the end of the transmit queue.
     */
    if (txQueue == NULL) {
        txQueue = pkt;
    } else {
        last = txQueue;
        while (last->next != NULL) {
            last = last->next;
        }
        last->next = pkt;
    }
}


AJ_Status AJ_SerialSend(uint8_t* buffer,
                        uint16_t bufLen)
{
    AJ_Status status = AJ_OK;
    uint16_t len = bufLen;

    while (len) {
        if (AJ_SerialLinkParams.linkState == AJ_LINK_DEAD) {
            status = AJ_ERR_LINK_DEAD;
            break;
        }

        // wait until there is space to send a packet.
        if (!txFreeList || AJ_SerialLinkParams.linkState != AJ_LINK_ACTIVE) {
            AJ_StateMachine();
            continue;
        }
        /*
         * Fill as many packets as we can
         */
        while (txFreeList && len) {
            uint16_t num = min(AJ_SerialLinkParams.packetSize, len);
            TxPkt volatile* pkt = txFreeList;

            txFreeList = txFreeList->next;
            pkt->type = AJ_SERIAL_DATA;
            pkt->len  = num;
            memcpy(pkt->payload, buffer + (bufLen - len), num);

            QueueReliable(pkt);
            len -= num;
        }
    }
    return status;
}



void AJ_SerialTX_EnqueueCtrl(const uint8_t* packet,
                             uint16_t pktLen,
                             uint8_t type)
{
    /*
     * Unreliable packets are given special treatment because they are sent
     * ahead of any packets already in the txQueue and do not require
     * acknowledgment.
     */
    txUnreliable->type = type;
    memcpy(txUnreliable->payload, packet, min(pktLen, AJ_LINK_PACKET_PAYLOAD));
    txUnreliable->len = pktLen;

    QueueUnreliable();
}

static uint16_t SlipBytes(AJ_SlippedBuffer volatile* slip,
                          uint8_t* data,
                          uint16_t len)
{
    uint16_t i;
    uint8_t b;

    for (i = 0; i < len; ++i) {
        if (slip->actualLen == slip->allocatedLen) {
            AJ_ASSERT(FALSE);
            break;
        }
        b = *data++;
        if ((b == BOUNDARY_BYTE) || (b == ESCAPE_BYTE)) {
            /*
             * need room for two bytes
             */
            if ((slip->actualLen + 1) == slip->allocatedLen) {
                break;
            }
            slip->buffer[slip->actualLen++] = ESCAPE_BYTE;
            b = (b == ESCAPE_BYTE) ? ESCAPE_SUBSTITUTE : BOUNDARY_SUBSTITUTE;
        }
        slip->buffer[slip->actualLen++] = b;
    }
    return i;
}

/*
 * Number of packets on the txSent queue. These are packets that have been sent
 * but not yet acknowledged.
 */
static uint8_t txSentPending(void)
{
    uint8_t n = 0;
    static TxPkt volatile* pkt;

    for (pkt = txSent; pkt != NULL; pkt = pkt->next) {
        ++n;
    }
    AJ_ASSERT(n <= AJ_SerialLinkParams.windowSize);
    return n;
}

void ConvertPacketToBytes(AJ_SlippedBuffer volatile* slip, TxPkt volatile* txCurrent)
{
    uint8_t header[4];
    uint16_t crc = AJ_SERIAL_CRC_INIT;
    uint8_t crcBytes[2];

    //AJ_DumpBytes("Raw buffer",txCurrent->payload,txCurrent->len);
    slip->actualLen = 0;
    /*
     * Apply SLIP encoding to the header.
     */
    slip->buffer[slip->actualLen++] = BOUNDARY_BYTE;


    /*
     * Compose flags
     */
    // byte 1 is the message type
    header[1] = txCurrent->type;
    if (txCurrent->type == AJ_SERIAL_DATA) {
        /*
         * If we have maxed the windows we need to go idle
         */
        if (txSentPending() == AJ_SerialLinkParams.windowSize) {
            AJ_AlwaysPrintf(("TxSend - reached window size: %u\n", txSentPending()));
            AJ_ASSERT(FALSE);
        }

        header[0] = (txCurrent->seq << 4);
//                AJ_AlwaysPrintf("Tx seq %d, ack %d\n",  txCurrent->seq, currentTxAck);
    } else {
        header[0] = 0;
//                AJ_AlwaysPrintf("Tx %s seq %d\n", !txCurrent->type ? "ack" : "unreliable", txCurrent->seq);
    }
    /*
     * All packets except link control packets carry ACK information.
     */
    if (txCurrent->type != AJ_SERIAL_CTRL) {
        // Acknowledge the last packet received.
        header[0] |= (currentTxAck & 0x0F);
        /*
         * If there was an ACK backlog, we halt the explicit ACK timeout.
         */
        if (pendingAcks) {
            pendingAcks = 0;
            pendingAcks = 0;
            AJ_InitTimer(&ackTime);
            AJ_TimeAddOffset(&ackTime, AJ_TIMER_FOREVER);

        }
    }

    // bytes 2 and 3 are the payload length
    header[2] = txCurrent->len >> 8;
    header[3] = txCurrent->len & 0x00FF;

    AJ_CRC16_Compute(header, ArraySize(header), &crc);
    AJ_CRC16_Compute(txCurrent->payload, txCurrent->len, &crc);
    SlipBytes(slip, header, 4);
    SlipBytes(slip, txCurrent->payload, txCurrent->len);
    AJ_CRC16_Complete(crc, crcBytes);
    SlipBytes(slip, crcBytes, 2);
    slip->buffer[slip->actualLen++] = BOUNDARY_BYTE;
}


/**
 * This function is called by the receive layer when a data packet or an explicit ACK
 * has been received. The ACK value is one greater (modulo 8) than the seq number of the
 * last packet successfully received.
 */
void AJ_SerialTx_ReceivedAck(uint8_t ack)
{
    TxPkt volatile* ackedPkt = NULL;

    if (txSent == NULL) {
        return;
    }

    /*
     * Remove acknowledged packets from sent queue.
     */
    while ((txSent != NULL) && SEQ_GT(ack, txSent->seq)) {
        ackedPkt = txSent;
        txSent = txSent->next;
        //AJ_AlwaysPrintf("Releasing seq=%d (acked by %d)\n", ackedPkt->seq, ack);

        AJ_ASSERT(ackedPkt->type == AJ_SERIAL_DATA);
        /*
         * Return pkt to ACL free list.
         */
        ackedPkt->next = txFreeList;
        txFreeList = ackedPkt;

        /*
         * If all packet have been ack'd, halt the resend timer and return.
         */
        if (txSent == NULL) {
            AJ_InitTimer(&resendTime);
            AJ_TimeAddOffset(&resendTime, AJ_TIMER_FOREVER);
            resendPrimed = FALSE;
            return;
        }
    }
    /*
     * Reset the resend timer if one or more packets were ack'd.
     */
    if (ackedPkt != NULL) {
        AJ_InitTimer(&resendTime);
        AJ_TimeAddOffset(&resendTime, AJ_SerialLinkParams.txResendTimeout);
        resendPrimed = TRUE;
    }
}


/*
 * Send a explicit ACK (acknowledgement).
 */
void SendAck()
{
    if (pendingAcks) {
        pendingAcks = 0;
        AJ_SerialTX_EnqueueCtrl(NULL, 0, AJ_SERIAL_ACK);
    }
    /*
     * Disable explicit ack.
     */
    AJ_InitTimer(&ackTime);
    AJ_TimeAddOffset(&ackTime, AJ_TIMER_FOREVER);
}


/*
 * This function is called from the receive side with the sequence number of
 * the last packet received.
 */
void AJ_SerialTx_ReceivedSeq(uint8_t seq)
{
    /*
     * If we think we have already acked this sequence number we don't adjust
     * the ack count.
     */
    if (!SEQ_GT(currentTxAck, seq)) {
        currentTxAck = (seq + 1) & 0x7;
    }

#ifdef ALWAYS_ACK
    AJ_SerialTX_EnqueueCtrl(NULL, 0, AJ_SERIAL_ACK);
#else
    ++pendingAcks;

    /*
     * If there are no packets to send we are allowed to accumulate a
     * backlog of pending ACKs up to a maximum equal to the window size.
     * In any case we are required to send an ack within a timeout
     * period so if this is the first pending ack we need to prime a timer.
     */
    if (pendingAcks == 1) {
        AJ_InitTimer(&ackTime);
        AJ_TimeAddOffset(&ackTime, AJ_SerialLinkParams.txAckTimeout);
        return;
    }

    /*
     * If we have hit our pending ACK limit send a explicit ACK packet immediately.
     */
    if (pendingAcks == AJ_SerialLinkParams.windowSize) {
        AJ_SerialTX_EnqueueCtrl(NULL, 0, AJ_SERIAL_ACK);
    }
#endif
}

void AJ_TransmitCallback(uint8_t* buffer, uint16_t bytesWritten)
{
    dataSent = 1;
    AJ_ASSERT((buffer == pendingSendBuffer->buffer) && (bytesWritten == pendingSendBuffer->actualLen));
    // put pendingSendBuffer on the free list
    pendingSendBuffer->next = bufferTxFreeList;
    bufferTxFreeList = pendingSendBuffer;
    pendingSendBuffer = bufferTxPending;
    if (pendingSendBuffer != NULL) {
        bufferTxPending = bufferTxPending->next;
        pendingSendBuffer->next = NULL;
        AJ_TX(pendingSendBuffer->buffer, pendingSendBuffer->actualLen);
        AJ_ResumeTX();
    }
}

void AJ_FillTxBufferList()
{
    AJ_SlippedBuffer volatile* currentSlippedBuffer;

    if (!txQueue) {
        return;
    }

    AJ_PauseTX();

    while (bufferTxFreeList && txQueue) {
        // Pull the head off the queue.
        TxPkt volatile* txCurrent;
        currentSlippedBuffer = bufferTxFreeList;
        bufferTxFreeList = bufferTxFreeList->next;
        currentSlippedBuffer->next  = NULL;

        if (pendingSendBuffer != NULL) {
            AJ_ResumeTX();
        }

        txCurrent = txQueue;
        txQueue = txQueue->next;
        txCurrent->next = NULL;

        ConvertPacketToBytes(currentSlippedBuffer, txCurrent);
        if (txCurrent->type == AJ_SERIAL_DATA) {
            //put it onto txSent
            if (txSent == NULL) {
                txSent = txCurrent;
            } else {
                TxPkt volatile* last = txSent;
                while (last->next != NULL) {
                    last = last->next;
                }
                last->next = txCurrent;
            }

            if (!resendPrimed) {
                AJ_InitTimer(&resendTime);
                AJ_TimeAddOffset(&resendTime, AJ_SerialLinkParams.txResendTimeout);
                resendPrimed = FALSE;
            }
        }
        AJ_PauseTX();
        //put the buffer on the pending list
        if (pendingSendBuffer == NULL) {
            //Free list was previously NULL, so re-enable reading
            //Save a pointer to the recv buffer, so we can keep track when the AJ_RecieveCallback occurs.
            pendingSendBuffer = currentSlippedBuffer;
            //pendingSendBuffer->next = NULL;
            AJ_TX(pendingSendBuffer->buffer, pendingSendBuffer->actualLen);
        } else {
            if (bufferTxPending != NULL) {
                volatile AJ_SlippedBuffer* buf = bufferTxPending;
                while (buf->next != NULL) {
                    buf = buf->next;
                }
                buf->next = currentSlippedBuffer;
            } else {
                bufferTxPending = currentSlippedBuffer;
            }
        }
    }

    if (!bufferTxFreeList) {
        dataSent = FALSE;
    }

    if (pendingSendBuffer != NULL) {
        AJ_ResumeTX();
    }
}

#endif /* AJ_SERIAL_CONNECTION */
