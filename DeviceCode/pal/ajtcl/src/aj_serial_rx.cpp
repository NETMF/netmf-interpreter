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
#define AJ_MODULE SERIAL_RX

#include "aj_target.h"
#include "aj_status.h"
#include "aj_serial.h"
#include "aj_serial_rx.h"
#include "aj_serial_tx.h"
#include "aj_crc16.h"
#include "aj_util.h"
#include "aj_serio.h"
#include "aj_debug.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgSERIAL_RX = 0;
#endif

/**
 * maximum read size
 */
static uint16_t maxRxFrameSize;

/**
 * This enumerated type is for tracking the state of the receive packet as it is being decoded.
 */
typedef enum {
    PACKET_NEW,
    PACKET_OPEN,
    PACKET_FLUSH,
    PACKET_ESCAPE
} PKT_STATE;


typedef struct _RX_PKT {
    uint8_t* buffer;
    uint16_t len;
    PKT_STATE state;
    struct _RX_PKT volatile* next;
} RX_PKT;


static RX_PKT volatile* RxPacket;
static RX_PKT volatile* RxRecv;
static RX_PKT volatile* RxFreeList;

//Linked list of free buffers that can be used to recieve data.
static AJ_SlippedBuffer volatile* bufferRxFreeList;
//Linked list of received slipped buffers
static AJ_SlippedBuffer volatile* bufferRxQueue;
// the buffer currently being sent
static AJ_SlippedBuffer volatile* pendingRecvBuffer;

/**
 * number of received packets waiting to be delivered to the upper layers
 */
static uint8_t volatile pendingRecv;

/**
 * sequence number expected in the next packet
 */
static uint8_t expectedSeq;

void AJ_ReceiveCallback(uint8_t* buffer, uint16_t bytesRead)
{
    // move pendingRecvBuffer from the pending list to the free list
    // it will be reused

    dataReceived = 1;
    if (bufferRxQueue == NULL) {
        bufferRxQueue = pendingRecvBuffer;
    } else {
        volatile AJ_SlippedBuffer* buf = bufferRxQueue;
        while (buf->next != NULL) {
            buf = buf->next;
        }

        buf->next = pendingRecvBuffer;
    }

    pendingRecvBuffer->next = NULL;
    pendingRecvBuffer->actualLen = bytesRead;
    pendingRecvBuffer = bufferRxFreeList;

    // if anything is pending, send it!
    if (pendingRecvBuffer != NULL) {
        bufferRxFreeList = bufferRxFreeList->next;
        pendingRecvBuffer->next = NULL;
        AJ_RX(pendingRecvBuffer->buffer, pendingRecvBuffer->allocatedLen);
        AJ_ResumeRX();
    }
}

#ifdef  AJ_DEBUG_PACKET_LISTS
void _AJ_DebugCheckPacketList(RX_PKT volatile* list, char* listName)
{
    // BUGBUG take a lock
    RX_PKT volatile* iter = list;
    AJ_AlwaysPrintf(("%s list %p\n", listName, list));

    while (iter) {
        AJ_ASSERT(iter != iter->next);  //check for a single loop
        AJ_AlwaysPrintf(("%s list %p, iter %p next %p\n", listName, list, iter, iter->next));
        iter = iter->next;
    }
}
#define AJ_DebugCheckPacketList(a, b) _AJ_DebugCheckPacketList(a, b)
#else
#define AJ_DebugCheckPacketList(a, b)
#endif

#ifdef AJ_DEBUG_SERIAL_RECV
#define AJ_DebugDumpSerialRecv(a, b, c) AJ_DumpBytes(a, b, c)
#else
#define AJ_DebugDumpSerialRecv(a, b, c)
#endif

/**
 * global timer id that controls waiting in AJ_SerialRecv
 */
#define AJ_SERIAL_READ_TIMER 0x100


static void DeleteRxPacket(volatile RX_PKT* pkt)
{
    while (pkt != NULL) {
        volatile RX_PKT* prev = pkt;
        AJ_Free(pkt->buffer);
        pkt = pkt->next;
        AJ_Free((void*) prev);
    }
}


void AJ_SerialRX_Shutdown(void)
{
    AJ_PauseRX();
    ClearSlippedBuffer(bufferRxFreeList);
    bufferRxFreeList = NULL;
    ClearSlippedBuffer(bufferRxQueue);
    bufferRxQueue = NULL;
    ClearSlippedBuffer(pendingRecvBuffer);
    pendingRecvBuffer = NULL;

    DeleteRxPacket(RxPacket);
    RxPacket = NULL;
    DeleteRxPacket(RxFreeList);
    RxFreeList = NULL;
    DeleteRxPacket(RxRecv);
    RxRecv = NULL;
}

/**
 * This function initializes the receive path
 */
AJ_Status AJ_SerialRX_Init(void)
{
    int i;
    RX_PKT volatile* prev;
    AJ_SlippedBuffer volatile* prevBuf = NULL;

    if (AJ_SerialLinkParams.packetSize == 0) {
        return AJ_ERR_FAILURE;
    }
    /*
     * Initialize local static data
     */
    RxRecv = NULL;
    RxPacket = NULL;
    pendingRecv = 0;
    expectedSeq = 0;
    dataReceived = 0;
    /*
     * The maximum frame size is the packet length plus the header length plus
     * two bytes for the packet boundary bytes.
     */
    maxRxFrameSize = AJ_SerialLinkParams.packetSize + 2 + AJ_SERIAL_HDR_LEN;

    /*
     * Data packets: To maximize throughput we need as many packets as the
     * window size, plus one for the current packet
     */
    for (i = 0; i < AJ_SerialLinkParams.maxWindowSize + 1; ++i) {
        void* buf;
        prev = RxFreeList;
        RxFreeList = AJ_Malloc(sizeof(RX_PKT));
        buf = AJ_Malloc(maxRxFrameSize);
        if (!RxFreeList || !buf) {
            return AJ_ERR_RESOURCES;
        }
        RxFreeList->buffer = buf;
        RxFreeList->state = PACKET_NEW;
        RxFreeList->len = 0;
        RxFreeList->next = prev;
    }

    bufferRxFreeList = NULL;
    for (i = 0; i < AJ_SerialLinkParams.maxWindowSize + 1; i++) {
        void* buf;
        prevBuf = bufferRxFreeList;
        bufferRxFreeList = AJ_Malloc(sizeof(AJ_SlippedBuffer));
        buf = AJ_Malloc(SLIPPED_LEN(AJ_SerialLinkParams.packetSize));
        if (!bufferRxFreeList || !buf) {
            return AJ_ERR_RESOURCES;
        }
        bufferRxFreeList->buffer = buf;
        bufferRxFreeList->actualLen = 0;
        bufferRxFreeList->allocatedLen = SLIPPED_LEN(AJ_SerialLinkParams.packetSize);
        bufferRxFreeList->next = prevBuf;
    }

    AJ_SetRxCB(&AJ_ReceiveCallback);
    pendingRecvBuffer = bufferRxFreeList;
    bufferRxFreeList = bufferRxFreeList->next;
    pendingRecvBuffer->next = NULL;
    AJ_RX(pendingRecvBuffer->buffer, pendingRecvBuffer->allocatedLen);
    AJ_ResumeRX();

    // pull the first buffer off of the free list.
    RxPacket = RxFreeList;
    RxFreeList = RxFreeList->next;
    RxPacket->next = NULL;

    AJ_DebugCheckPacketList(RxFreeList, "RxFreeList during init");
    AJ_DebugCheckPacketList(RxRecv, "RxRecv during init");
    AJ_DebugCheckPacketList(RxPacket, "RxPacket during init");

    return AJ_OK;
}

void AJ_SerialReturnPacketToFreeList(volatile RX_PKT* pkt)
{
    // return the packet to the free list and reinitialize it
    if (pkt) {
        pkt->state = PACKET_NEW;
        pkt->next = RxFreeList;
        RxFreeList = pkt;
    }
}


/**
 * This function resets the receive path for the serial transport.
 */
AJ_Status AJ_SerialRX_Reset(void)
{
    RX_PKT volatile* pkt;
    /*
     * Put ACL packets back on the free list.
     */
    while (RxRecv != NULL) {
        pkt = RxRecv;
        RxRecv = RxRecv->next;
        AJ_SerialReturnPacketToFreeList(pkt);
    }

    AJ_DebugCheckPacketList(RxFreeList, "RxFreeList reset");
    AJ_DebugCheckPacketList(RxRecv, "RxRecv during reset");
    AJ_DebugCheckPacketList(RxPacket, "RxPacket during reset");

    expectedSeq = 0;
    pendingRecv = 0;
    RxPacket->state = PACKET_NEW;
    return AJ_OK;
}

AJ_Status AJ_SerialRecv(uint8_t* buffer,
                        uint16_t req,
                        uint32_t timeout,
                        uint16_t* recv)
{
    AJ_Status status = AJ_OK;

    // call the target-specific routine that copies data when
    // it arrives into the buffer we are providing.
    uint16_t len = req;
    AJ_Time now;
    AJ_Time readTimeoutTimeStamp;
    AJ_InitTimer(&readTimeoutTimeStamp);
    AJ_TimeAddOffset(&readTimeoutTimeStamp, timeout);
    AJ_InitTimer(&now);
    do {
        if (AJ_SerialLinkParams.linkState == AJ_LINK_DEAD) {
            status = AJ_ERR_LINK_DEAD;
            break;
        }
        /*
         * Fill as many packets as we can
         */
        while (RxRecv && len) {
            RX_PKT volatile* pkt = RxRecv;
            uint16_t num = min(pkt->len - 6, len);
            AJ_DebugCheckPacketList(RxFreeList, "RxFreeList serialrecv");
            AJ_DebugCheckPacketList(RxRecv, "RxRecv during serialrecv");
            AJ_DebugCheckPacketList(RxPacket, "RxPacket during serialrecv");

            AJ_DebugDumpSerialRecv("AJ_SerialRecv", pkt->buffer, num + 4);

            memcpy(buffer + (req - len), pkt->buffer + 4,  num);
            len -= num;

            if (num == (pkt->len - 6)) {   /// use pkt->len, because we might have eaten out of this buffer already
                // we used a full packet
                // put that packet back on the RxFreeList, decrement pendingRecv, and send Ack.
                RxRecv = RxRecv->next;
                AJ_SerialReturnPacketToFreeList(pkt);
                AJ_DebugCheckPacketList(RxFreeList, "RxFreeList serialrecv AFTER FULL PACKET");
                AJ_DebugCheckPacketList(RxRecv, "RxRecv serialrecv AFTER FULL PACKET");
                AJ_DebugCheckPacketList(RxPacket, "RxFreeList serialrecv AFTER FULL PACKET");
                --pendingRecv;

            } else {
                // move the data in the buffer over, then return from this function.
                memmove(pkt->buffer + 4, pkt->buffer + 4 + num, pkt->len - num);
                pkt->len -= num;
            }
        }
        // Running state machine, waiting for RxRecv to get another buffer.
        AJ_StateMachine();
        AJ_InitTimer(&now);
    } while (len && (AJ_CompareTime(readTimeoutTimeStamp, now) > 0));

    if (AJ_CompareTime(readTimeoutTimeStamp, now) <= 0 && (req == len)) {
        status = AJ_ERR_TIMEOUT;
    }

    if (recv) {
        *recv = req - len;
    }
    return status;
}


/**
 * This function checks packet integrity and forwards good packets to the appropriate
 * upper-layer interface.
 */
static void CompletePacket()
{
    RX_PKT volatile* pkt  = RxPacket;
    uint8_t ack;
    uint8_t seq;
    uint8_t pktType;
    uint16_t expectedLen;

    uint8_t checksum;
    uint8_t* rcvdCrc = &pkt->buffer[pkt->len - 2];
    uint8_t checkCrc[2];
    uint16_t crc = AJ_SERIAL_CRC_INIT;

    if (pkt->len < AJ_SERIAL_HDR_LEN) {
        /*
         * Packet is too small.
         */
        AJ_AlwaysPrintf(("Short packet %d\n", pkt->len));
        return;
    }

    /*
     * Compute the CRC on the packet header and payload.
     */
    AJ_CRC16_Compute(pkt->buffer, pkt->len - 2, &crc);
    AJ_CRC16_Complete(crc, checkCrc);

    /*
     * Check the computed and received CRC's match.
     */
    if ((rcvdCrc[0] != checkCrc[0]) || (rcvdCrc[1] != checkCrc[1])) {
        AJ_AlwaysPrintf(("Data integrity error - discarding packet\n"));
        AJ_AlwaysPrintf(("rcvdCrc = %u %u\n", rcvdCrc[0], rcvdCrc[1]));
        AJ_AlwaysPrintf(("checkCrc = %u %u\n", checkCrc[0], checkCrc[1]));
        return;
    }


    seq = (pkt->buffer[0] >> 4);
    ack = pkt->buffer[0] & 0x0F;

    pktType = pkt->buffer[1] & 0x0F;

    /*
     * Check that the payload length in the header matches the bytes read.
     */
    expectedLen = ((uint16_t) pkt->buffer[2]) << 8;
    expectedLen |= (pkt->buffer[3]);
    if (expectedLen != (pkt->len - AJ_SERIAL_HDR_LEN - 2)) {
        AJ_AlwaysPrintf(("Wrong packet length header says %d read %d bytes\n", expectedLen, pkt->len - AJ_SERIAL_HDR_LEN - 2));
        return;
    }


    //AJ_AlwaysPrintf("Rx %d, seq=%d, ack=%d\n", pktType, seq, ack);

    /*
     * Handle link control packets.
     */
    if (pktType == AJ_SERIAL_CTRL) {
        AJ_Serial_LinkPacket(pkt->buffer + AJ_SERIAL_HDR_LEN, expectedLen);
        return;
    }
    /*
     * If the link is not active non-link packets are discarded.
     */
    if (AJ_SerialLinkParams.linkState != AJ_LINK_ACTIVE) {
        AJ_AlwaysPrintf(("Link not up - discarding data packet\n"));
        return;
    }


    /*
     * Pass the ACK to the transmit side.
     */
    AJ_SerialTx_ReceivedAck(ack);

    if (pktType == AJ_SERIAL_DATA) {
        /*
         * If a reliable packet does not have the expected sequence number, then
         * it is either a repeated packet or we missed a packet. In either case,
         * we must ignore the packet but we need to ACK repeated packets.
         */
        if (seq != expectedSeq) {
            if (SEQ_GT(seq, expectedSeq)) {
                AJ_AlwaysPrintf(("Missing packet - expected = %d, got %d\n", expectedSeq, seq));
            } else {
                AJ_AlwaysPrintf(("Repeated packet seq = %d, expected %d\n", seq, expectedSeq));
                AJ_SerialTx_ReceivedSeq(seq);
            }
        } else {
            if (RxFreeList != NULL) {
                // push the RxPacket on to the back of the RxRecv list.
                RX_PKT volatile* last;
                expectedSeq = (expectedSeq + 1) & 0x07;
                /*
                 * Add to the end of the receive queue.
                 */
                if (RxRecv == NULL) {
                    RxRecv = pkt;
                } else {
                    last = RxRecv;
                    while (last->next != NULL) {
                        last = last->next;
                    }
                    last->next = pkt;
                }
                pkt->next = NULL;
                RxPacket = RxFreeList;
                RxFreeList = RxFreeList->next;
                RxPacket->next = NULL;

                ++pendingRecv; // we now have another packet enqueued.
                AJ_SerialTx_ReceivedSeq(seq);
            }
        }
    }
}



/**
 * This function is called from the physical transport layer when data is received.
 * @param buffer       pointer to the buffer holding the received data
 * @param bytes        length of the received data in bytes
 */
static uint32_t UART_RxComplete(uint8_t* buffer, uint16_t bytes)
{
    uint8_t rx;

    while (bytes-- > 0) {
        rx = *buffer++;
        switch (RxPacket->state) {
        case PACKET_FLUSH:
            /*
             * If we are not at a packet boundary as expected, then we need to flush
             * the receive system until we see a closing packet boundary.
             */
            if (rx == BOUNDARY_BYTE) {
                RxPacket->state = PACKET_NEW;
            }
            break;

        case PACKET_NEW:
            /*
             * If we are not at a packet boundary as expected we need to flush
             * rx until we see a closing packet boundary.
             */
            if (rx == BOUNDARY_BYTE) {
                RxPacket->state = PACKET_OPEN;
            } else {
                RxPacket->state = PACKET_FLUSH;
                AJ_AlwaysPrintf(("AJ_SerialRx_Receive: Flushing input at %2x\n", rx));
            }
            RxPacket->len = 0;
            break;

        case PACKET_ESCAPE:
            /*
             * Handle a SLIP escape sequence.
             */
            RxPacket->state = PACKET_OPEN;
            if (rx == BOUNDARY_SUBSTITUTE) {
                RxPacket->buffer[RxPacket->len++] = BOUNDARY_BYTE;
                break;
            }
            if (rx == ESCAPE_SUBSTITUTE) {
                RxPacket->buffer[RxPacket->len++] = ESCAPE_BYTE;
                break;
            }
            AJ_AlwaysPrintf(("AJ_SerialRx_Receive: Bad escape sequence %2x\n", rx));
            /*
             * Bad escape sequence: discard everything up to the current
             * byte. This means that we need to restore the current byte.
             */
            ++bytes;
            --buffer;
            RxPacket->state = PACKET_NEW;
            break;

        case PACKET_OPEN:
            /*
             * Decode received bytes and transfer them to the receive packet.
             */
            if (rx == BOUNDARY_BYTE) {
                RX_PKT volatile* pkt = RxPacket;
                pkt->state = PACKET_NEW;
                pkt->next = NULL;

                CompletePacket();
                // the packet will be put back on the RxFreeList when the upper layer has retrieved it.
                break;
            }
            if (rx == ESCAPE_BYTE) {
                RxPacket->state = PACKET_ESCAPE;
                break;
            }
            if (RxPacket->len == maxRxFrameSize) {
                /*
                 * Packet overrun: discard the packet.
                 */
                RxPacket->state = PACKET_NEW;
                AJ_AlwaysPrintf(("AJ_SerialRx_Receive: Packet overrun %d\n", RxPacket->len));
                break;
            }
            RxPacket->buffer[RxPacket->len++] = rx;
            break;

        default:
            AJ_ASSERT(FALSE);
        }
    }
    /*
     * Only read as many bytes as we can consume.
     */
    return maxRxFrameSize - RxPacket->len;
}

void AJ_ProcessRxBufferList()
{
    AJ_SlippedBuffer volatile* currentSlippedBuffer;

    if (!RxFreeList) {
        return;
    }

    AJ_PauseRX();
    while (bufferRxQueue && RxFreeList) {
        // Pull the head off the queue.
        currentSlippedBuffer = bufferRxQueue;
        bufferRxQueue = bufferRxQueue->next;
        AJ_ResumeRX();

        UART_RxComplete(currentSlippedBuffer->buffer, currentSlippedBuffer->actualLen);
        AJ_PauseRX();

        if (pendingRecvBuffer == NULL) {
            //Free list was previously NULL, so re-enable reading
            //Save a pointer to the recv buffer, so we can keep track when the AJ_RecieveCallback occurs.
            pendingRecvBuffer = currentSlippedBuffer;
            pendingRecvBuffer->next = NULL;
            AJ_RX(pendingRecvBuffer->buffer, pendingRecvBuffer->allocatedLen);
        } else {
            currentSlippedBuffer->next = bufferRxFreeList;
            bufferRxFreeList = currentSlippedBuffer;
        }
    }

    if (!bufferRxQueue) {
        dataReceived = FALSE;
    }
    AJ_ResumeRX();
}
#endif /* AJ_SERIAL_CONNECTION */
