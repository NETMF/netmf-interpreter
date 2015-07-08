#ifndef _AJ_BUFIO_H
#define _AJ_BUFIO_H

/**
 * @file aj_bufio.h
 * @defgroup aj_bufio Buffer Input/Output
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

#ifdef __cplusplus
extern "C" {
#endif

/*
 * Forward declaration
 */
struct _AJ_IOBuffer;

/**
 * Function pointer type for an abstracted transmit function
 */
typedef AJ_Status (*AJ_TxFunc)(struct _AJ_IOBuffer* buf);

/**
 * Function pointer type for an abstracted receive function
 *
 * @param buf     The buffer to read into
 * @param len     The requested number of bytes to read. More or fewer bytes may actually be read into
 *                the buffer.
 * @param timeout A timeout in milliseconds after which the read will return with an error status if
 *                there is not data to read.
 *
 * @return
 *         - AJ_OK if some data was read
 *         - AJ_ERR_TIMEOUT if the read timedout
 *         - AJ_ERR_RESOURCES if there isn't enough room in the buffer to read len bytes. The buffer
 *           will contain the bytes actually read so this is not a fatal error.
 *         - AJ_ERR_READ the read failed irrecoverably
 *         - AJ_ERR_LINK_DEAD the network link is dead
 */
typedef AJ_Status (*AJ_RxFunc)(struct _AJ_IOBuffer* buf, uint32_t len, uint32_t timeout);

#define AJ_IO_BUF_RX     1 /**< I/O direction is receive */
#define AJ_IO_BUF_TX     2 /**< I/O direction is send */

#define AJ_IO_BUF_AJ     1 /**< send/receive data to/from AJ */
#define AJ_IO_BUF_MDNS   2 /**< send/receive data to/from mDNS */

/**
 * A type for managing a receive or transmit buffer
 */
typedef struct _AJ_IOBuffer {
    uint8_t direction;  /**< I/O buffer is either a Tx buffer or an Rx buffer */
    uint8_t flags;      /**< ports to send to or receive on */
    uint16_t bufSize;   /**< Size of the data buffer */
    uint8_t* bufStart;  /**< Start for the data buffer */
    uint8_t* readPtr;   /**< Current position in buf for reading data */
    uint8_t* writePtr;  /**< Current position in buf for writing data */
    /*
     * Function pointer to send or recv function
     */
    union {
        AJ_TxFunc send;
        AJ_RxFunc recv;
    };
    void* context;      /**< Abstracted context for managing I/O */

} AJ_IOBuffer;

/**
 * How much data is available to read from the buffer
 */
#define AJ_IO_BUF_AVAIL(iobuf)  (uint32_t)(((iobuf)->writePtr - (iobuf)->readPtr))

/**
 * How much space is available to write to the buffer
 */
#define AJ_IO_BUF_SPACE(iobuf)  ((uint32_t)((iobuf)->bufSize - ((iobuf)->writePtr - (iobuf)->bufStart)))

/**
 * How much data has been consumed from the buffer
 */
#define AJ_IO_BUF_CONSUMED(iobuf)  (uint32_t)(((iobuf)->readPtr - (iobuf)->bufStart))

/**
 * Reset and IO buffer
 */
#define AJ_IO_BUF_RESET(iobuf) \
    do { \
        (iobuf)->readPtr = (iobuf)->bufStart; \
        (iobuf)->writePtr = (iobuf)->bufStart; \
        (iobuf)->flags = 0; \
    } while (0)

/**
 * Initialize an I/O Buffer.
 *
 * @param ioBuf     The I/O buffer to initialize
 * @param buffer    The data buffer to use
 * @param bufLen    The size of the data buffer
 * @param direction Indicates if the buffer is being used for sending or receiving data
 * @param context   Abstracted context for managing I/O
 */
void AJ_IOBufInit(AJ_IOBuffer* ioBuf, uint8_t* buffer, uint32_t bufLen, uint8_t direction, void* context);

/**
 * Move any unconsumed data to the start of the buffer.
 *
 * @param ioBuf    An RX I/O buf that may contain unconsumed data
 * @param preserve Data (if any) at front of buffer that must be preserved
 */
void AJ_IOBufRebase(AJ_IOBuffer* ioBuf, size_t preserve);

#ifdef __cplusplus
}
#endif
/**
 * @}
 */
#endif
