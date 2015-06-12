#ifndef _AJ_SERIAL_H
#define _AJ_SERIAL_H

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

#include "aj_target.h"
#include "aj_status.h"

#ifdef __cplusplus
extern "C" {
#endif

/*
 * SLIP encapsulation characters as defined in (the ancient) RFC 1055
 */
#define BOUNDARY_BYTE        0xC0
#define BOUNDARY_SUBSTITUTE  0xDC
#define ESCAPE_BYTE          0xDB
#define ESCAPE_SUBSTITUTE    0xDD

/*
 * maximum and minimum window sizes
 */
#define MIN_WINDOW_SIZE        1
#define MAX_WINDOW_SIZE        4

/**
 * Packet header is four bytes.
 */
#define AJ_SERIAL_HDR_LEN  4


#define AJ_SERIAL_DATA  0  /**< Indicates a data packet */
#define AJ_SERIAL_UDATA 1  /**< Indicates an ack packet */
#define AJ_SERIAL_CTRL  14  /**< Indicates an Link control packet */
#define AJ_SERIAL_ACK   15  /**< Indicates an ack packet */

#define AJ_CRC_LEN 2
#define AJ_BOUNDARY_BYTES 2
#define SLIPPED_LEN(packetsize) (((packetsize) + AJ_SERIAL_HDR_LEN + AJ_CRC_LEN) * 2 + AJ_BOUNDARY_BYTES)

/**
 * This is big enough to hold the control payloads stored in unreliable packets
 */
#define AJ_LINK_PACKET_PAYLOAD      32

/**
 * packets that carry transport control information
 */
#define CNTRL_PACKET(t) \
    (((t) == AJ_SERIAL_ACK) || ((t) == AJ_SERIAL_CTRL))

/**
 * Reliable packets are ACKed; unreliable packets are not.
 */
  #define RELIABLE_PACKET(t) \
    ((t) == AJ_SERIAL_DATA)

/**
 * Data packets can only be sent and received when the link is active.
 */
typedef enum {
    AJ_LINK_UNINITIALIZED,
    AJ_LINK_INITIALIZED,
    AJ_LINK_ACTIVE,
    AJ_LINK_DYING,
    AJ_LINK_DEAD
} AJ_LinkState;

/**
 * link parameter structure
 */
typedef struct _AJ_LinkParameters {
    uint8_t protoVersion;        /**< Protocol version*/
    AJ_LinkState linkState;      /**< State of the link */
    uint8_t maxWindowSize;       /**< Window size configuration parameter */
    uint8_t windowSize;          /**< Negotiated window size */
    uint16_t packetSize;         /**< Packet size configuration parameter */
    uint32_t bitRate;            /**< Bit rate of the underlying serial link */
    uint32_t txResendTimeout;    /**< how long to wait for an acknowledgement before resending a packet */
    uint32_t txAckTimeout;       /**< how long to wait after a packet has been received before sending a ACK packet */
} AJ_LinkParameters;


/**
 * Struct used for transferring buffers to and from the Rx/Tx interrupts.
 */

typedef struct __AJ_SlippedBuffer {
    uint8_t* buffer;
    uint16_t allocatedLen;
    uint16_t actualLen;
    struct __AJ_SlippedBuffer volatile* next;
} AJ_SlippedBuffer;

/**
 * Function pointer type for an abstracted serial transmit function
 *
 * @param buf     The buffer to be transmitted
 * @param len     The number of bytes to write
 */
typedef void (*AJ_SerialTxFunc)(uint8_t* buf, uint32_t len);
/**
 * global function pointer for serial transmit funciton
 */
extern AJ_SerialTxFunc g_AJ_TX;

void ClearSlippedBuffer(volatile AJ_SlippedBuffer* buf);

extern volatile int dataReceived;
extern volatile int dataSent;
/**
 * global variable for link parameters
 */
extern AJ_LinkParameters AJ_SerialLinkParams;

/**
 * Determine relative ordering of two sequence numbers. Sequence numbers are
 * modulo 8 so 0 > 7.
 *
 * This is used to test for ACKs and to detect gaps in the sequence of received
 * packets.
 */
#define SEQ_GT(s1, s2)  (((7 + (s1) - (s2)) & 7) < AJ_SerialLinkParams.windowSize)


/**
 * CRC Computation. The CRC computation must be initialized to the following value.
 */
#define AJ_SERIAL_CRC_INIT  0xFFFF


/*
 * Handle a link control packet.
 */
void AJ_SerialLinkPacket(uint8_t* buffer,
                         uint16_t len);


/**
 * Function prototype for function called when the serial transport is open for business.
 */
typedef void (AJ_SerialInitialized)();

/**
 * This function initializes the serial transport layer.
 *
 * @param ttyName              string used to choose the correct tty
 * @param bitRate              configure the UART to this speed
 * @param windowSize           Window size for acks and packet retransmission
 * @param enableCRC            If TRUE enable CRC checks on data packets
 * @param packetSize           Packet size with which the serial transport layer is initialized
 */
AJ_Status AJ_SerialInit(const char* ttyName,
                        uint32_t bitRate,
                        uint8_t windowSize,
                        uint16_t packetSize);

/**
 * This function calls AJ_SerialInit with the appropriate parameters.
 * Its implementation is target-dependent.
 */
AJ_Status AJ_Serial_Up();

/**
 * This function shuts down the serial transport layer.
 *
 */
void AJ_SerialShutdown(void);

/**
 * Send a data buffer over the serial transport. This function blocks until the data has been queued
 * or sent.
 *
 * @param buffer       The buffer containing the data to be sent
 * @param len          The length of the data in the buffer
 *
 * @return    - AJ_OK if the data was succesfully queued or sent.
 */
AJ_Status AJ_SerialSend(uint8_t* buffer,
                        uint16_t len);

/**
 * Receive data from the serial transport. This function blocks until the request data has been
 * read into the buffer.
 *
 * @param buffer       The buffer containing the data to be sent
 * @param len          The length of the buffer
 * @param timeout      The amount of time to wait for data to arrive
 * @param recv         The amount of data received
 *
 * @return    - AJ_OK if data was succesfully received.
 */
AJ_Status AJ_SerialRecv(uint8_t* buffer,
                        uint16_t len,
                        uint32_t timeout,
                        uint16_t* recv);

/**
 * This function disconnects the serial transport layer.
 *
 */
void AJ_SerialDisconnect(void);

#ifdef __cplusplus
}
#endif

#endif /* AJ_SERIAL_CONNECTION */

#endif /* _AJ_SERIAL_H */
