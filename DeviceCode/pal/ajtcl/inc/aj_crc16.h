#ifndef _AJ_CRC16_H
#define _AJ_CRC16_H

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

#include "aj_target.h"

#ifdef __cplusplus
extern "C" {
#endif
/**
 * Computes a 16-bit CRC on a buffer. The caller provides the context for the running CRC.
 *
 * @param buffer         buffer over which to compute the CRC
 * @param bufLen         length of the buffer in bytes
 * @param runningCrc     On input the current CRC, on output the updated CRC.
 */
void AJ_CRC16_Compute(const uint8_t* buffer,
                      uint16_t bufLen,
                      uint16_t* runningCrc);

/**
 * This function completes the CRC computation by rearranging the CRC bits and bytes
 * into the correct order.
 *
 * @param crc       computed crc as calculated by AJ_CRC16_Compute()
 * @param crcBlock  pointer to a 2-byte buffer where the resulting CRC will be stored
 */

void AJ_CRC16_Complete(uint16_t crc,
                       uint8_t* crcBlock);

#ifdef __cplusplus
}
#endif

#endif /* _AJ_CRC16_H */
