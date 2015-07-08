#ifndef _AJ_NET_H
#define _AJ_NET_H

/**
 * @file aj_net.h
 * @defgroup aj_net Network Send and Receive
 * @{
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

#include "aj_target.h"
#include "aj_status.h"
#include "aj_bufio.h"

#ifdef __cplusplus
extern "C" {
#endif

#define AJ_ADDR_UDP4  0x01      /**< UDP ip4 address */
#define AJ_ADDR_UDP6  0x02      /**< UDP ip6 address */

#define AJ_ADDR_TCP4  0x04      /**< TCP ip4 address */
#define AJ_ADDR_TCP6  0x08      /**< TCP ip6 address */

struct _AJ_Service;
struct _AJ_BusAttachment;

/**
 * Abstracts a network socket
 */
typedef struct _AJ_NetSocket {
    AJ_IOBuffer tx;             /**< transmit network socket */
    AJ_IOBuffer rx;             /**< receive network socket */
} AJ_NetSocket;

/**
 * Connect to bus at an IPV4 or IPV6 address, either UDP or TCP
 *
 * @return        Return AJ_Status
 */
AJ_Status AJ_Net_Connect(struct _AJ_BusAttachment* bus, const struct _AJ_Service* service);

/**
 * Disconnect from the bus
 */
void AJ_Net_Disconnect(AJ_NetSocket* netSock);

/**
 * Send from an I/O buffer
 *
 * @return        Return AJ_Status
 */
AJ_Status AJ_Net_Send(AJ_IOBuffer* txBuf);

/**
 * Send into an I/O buffer
 *
 * @return        Return AJ_Status
 */
AJ_Status AJ_Net_Recv(AJ_IOBuffer* rxBuf, uint32_t len, uint32_t timeout);

/**
 * Abstracts discovery sockets
 */
typedef struct _AJ_MCastSocket {
    AJ_IOBuffer tx;
    AJ_IOBuffer rx;
} AJ_MCastSocket;

/**
 * Enable multicast (for discovery)
 *
 * @return        Return AJ_Status
 */
AJ_Status AJ_Net_MCastUp(AJ_MCastSocket* mcastSock);

/**
 * Disable multicast (for discovery)
 */
void AJ_Net_MCastDown(AJ_MCastSocket* mcastSock);

/**
 * Function that signals AJ_Net_Recv() to bail out early if it
 * is blocking on select.
 */
void AJ_Net_Interrupt(void);

#ifdef __cplusplus
}
#endif

/**
 * @}
 */
#endif
