/**
 * @file ArdpProtocol is an implementation of the Reliable Datagram Protocol
 * (RDP) adapted to AllJoyn.
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

#ifndef _ALLJOYN_ARDP_PROTOCOL_H
#define _ALLJOYN_ARDP_PROTOCOL_H

#ifdef AJ_ARDP

#ifdef __cplusplus
extern "C" {
#endif

#include "aj_bufio.h"
#include "aj_net.h"
#include "aj_status.h"
#include "aj_target.h"

/**
 * @brief Per-protocol-instance (global) configuration variables.
 */
#define UDP_CONNECT_TIMEOUT 1000  /**< How long before we expect a connection to complete */
#define UDP_CONNECT_RETRIES 10  /**< How many times do we retry a connection before giving up */
#define UDP_INITIAL_DATA_TIMEOUT 1000  /**< Initial value for how long do we wait before retrying sending data */
#define UDP_TOTAL_DATA_RETRY_TIMEOUT 30000  /**< Initial total amount of time to try and send data before giving up */
#define UDP_MIN_DATA_RETRIES 5  /**< Minimum number of times to try and send data before giving up */
#define UDP_PERSIST_INTERVAL 1000  /**< How long do we wait before pinging the other side due to a zero window */
#define UDP_TOTAL_APP_TIMEOUT 30000  /**< How long to we try to ping for window opening before deciding app is not pulling data */
#define UDP_LINK_TIMEOUT 30000  /**< How long before we decide a link is down (with no reponses to keepalive probes */
#define UDP_KEEPALIVE_RETRIES 5  /**< How many times do we try to probe on an idle link before terminating the connection */
#define UDP_FAST_RETRANSMIT_ACK_COUNTER 1  /**< How many duplicate acknowledgements to we need to trigger a data retransmission */
#define UDP_DELAYED_ACK_TIMEOUT 100 /**< How long do we wait until acknowledging received segments */
#define UDP_TIMEWAIT 1000  /**< How long do we stay in TIMWAIT state before releasing the per-connection resources */
#define UDP_MINIMUM_TIMEOUT 100 /**< The minimum amount of time between calls to ARDP_Recv */
#define UDP_BACKPRESSURE_TIMEOUT 100 /**< How long can backpressure block the program before a disconnect is triggered? */
#define UDP_DISCONNECT_TIMEOUT 1000  /**< How long can disconnect block the program waiting for TX queue to drain */

#define UDP_SEGBMAX 1472  /**< Maximum size of an ARDP segment (quantum of reliable transmission) */
#define UDP_SEGMAX 2  /**< Maximum number of ARDP segment in-flight (bandwidth-delay product sizing) */


/* Protocol specific values */
#define UDP_HEADER_SIZE 8
#define ARDP_HEADER_SIZE 36
#define ARDP_TTL_INFINITE   0

/*
 * SEGMAX and SEGBMAX  on both send and receive sides are inidcted by SYN header in Connection request:
 * the acceptor cannot modify these parameters, only reject in case the request cannot be accomodated.
 * No EACKs: only acknowledge segments received in sequence.
 */
#define ARDP_FLAG_SIMPLE_MODE 2

struct _AJ_IOBuffer;

/*
 *       ARDP onnection request. Connect begins the SYN,
 *       SYN-ACK, ACK thre-way handshake.
 *
 *       Returns error code:
 *         AJ_OK - all is good, connection transitioned to SYN_SENT state;
 *         fail error code otherwise
 */
AJ_Status AJ_ARDP_Connect(uint8_t* data, uint16_t dataLen, void* context, AJ_NetSocket* netSock);

/*
 *      Disconnect is used to actively close the connection.
 *          forced (IN) - if set to TRUE, the connection should be torn down regardless
 *                        of whether there are pending data retransmits. Otherwise, the
 *                        callee should check the value of returned error code.
 */
void AJ_ARDP_Disconnect(uint8_t forced);

/*
 *      StartMsgSend informs the ARDP protocol that next chunk of data to be sent
 *      is a beginning of anew message with a specified TTL.
 *      Returns error code:
 *         AJ_OK - all is good
 *         AJ_ERR_ARDP_TTL_EXPIRED - Discard this message. TTL is less than 1/2 estimated roundtrip time.
 *         AJ_ERR_ARDP_INVALID_CONNECTION - Connection does not exist (efffectively connection record is NULL)
 */
AJ_Status AJ_ARDP_StartMsgSend(uint32_t ttl);

/**
 * AJ_ARDP_Send will attempt to send the data in buf.  It will attempt to send the message, waiting for
 *      backpressure to clear if necessary.  It can potentially block for as long as is necessary
 *      for the backpressure to be cleared.
 *
 * returns:
 *      AJ_OK - if we sent successfully
 *      AJ_ERR_WRITE - if we were unable to send.
 */
AJ_Status AJ_ARDP_Send(struct _AJ_IOBuffer* buf);

/*
 *       ARDP_Recv: data are being read, buffered, and timers are checked.
 *         rxBuf - (IN) app buffer to be filled
 *         len   - (IN) the number of bytes the app wants
 *         timeout - (IN) the longest time period we can spend here
 *       Returns error code:
 *         AJ_OK;
 *         AJ_ERR_ARDP_RECV_EXPIRED;
 *         AJ_ERR_ARDP_REMOTE_DISCONNECT;
 *         AJ_ERR_TIMEOUT;
 *         AJ_ERR_INTERRUPTED;
 *         AJ_ERR_READ;
 *         AJ_ERR_PROBE_TIMEOUT
 *
 */
AJ_Status AJ_ARDP_Recv(struct _AJ_IOBuffer* rxBuf, uint32_t len, uint32_t timeout);

/**
 *  A pointer to the function used by ARDP to receive from a socket
 *
 *  @param context - (IN)  The context pointer
 *  @param buf     - (OUT) A pointer to the underlying data that was received
 *  @param recved  - (OUT) The number of bytes received into buf
 *  @param timeout - (IN)  The timeout
 *
 *  @return error code
 *      AJ_OK               if data was recived
 *      AJ_ERR_TIMEOUT      if no data was received by <timeout> msec
 *      AJ_ERR_INTERRUPTED  if the receive operation was interrupted by AJ_Net_Interrupt
 *      AJ_ERR_READ         if an error has occured
 */
typedef AJ_Status (*ReceiveFunction)(void* context, uint8_t** buf, uint32_t* recved, uint32_t timeout);

/**
 *  A pointer to the function used by ARDP to send raw data to a socket
 *
 *  @param context - (IN)  The context pointer
 *  @param buf     - (IN)  A pointer to the buffer to send
 *  @param recved  - (IN)  The number of bytes to send
 *  @param timeout - (OUT) The number of bytes actually sent
 *
 *  @return error code
 *      AJ_OK               if data was sent
 *      AJ_ERR_READ         if an error has occured
 */
typedef AJ_Status (*SendFunction)(void* context, uint8_t* buf, size_t len, size_t* sent);

void AJ_ARDP_InitFunctions(ReceiveFunction recv, SendFunction send);

#ifdef __cplusplus
}
#endif

#endif // AJ_ARDP

#endif // _ALLJOYN_ARDP_PROTOCOL_H
