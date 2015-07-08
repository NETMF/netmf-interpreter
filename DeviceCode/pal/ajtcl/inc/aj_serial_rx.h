#ifndef _AJ_SERIAL_RX_H
#define _AJ_SERIAL_RX_H

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
#include "aj_serial.h"

#ifdef __cplusplus
extern "C" {
#endif

/**
 * This function initializes the receive path
 */
AJ_Status AJ_SerialRX_Init(void);

/**
 * This function shuts down the receive path
 */
void AJ_SerialRX_Shutdown(void);

/**
 * This function resets the receive path
 */
AJ_Status AJ_SerialRX_Reset(void);

/**
 * Process the buffers read by the Receive callback - called by the StateMachine.
 */
void AJ_ProcessRxBufferList();

#ifdef __cplusplus
}
#endif

#endif /* AJ_SERIAL_CONNECTION */

#endif /* _AJ_SERIAL_RX_H */
