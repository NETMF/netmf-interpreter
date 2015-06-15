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

#ifndef AJ_TARGET_MBED_H_
#define AJ_TARGET_MBED_H_

typedef enum {
    SPI_OK,
    SPI_ERR
}aj_spi_status;

/**
 * Printf function that prints to the serial port on the MBED board
 *
 * @param fmat          Format string (same as printf)
 * @param ...           List of arguments that will be placed in the format string
 *
 * note: Currently this function only supports printing 256 characters per call.
 * Anything over that would need to have successive BoardPrintf calls.
 */
void BoardPrintf(const char* fmat, ...);

/**
 * Initialize the serial port so BoardPrintf can be used. This must be called
 * before BoardPrintf or it will not function.
 *
 * @param baud          Baud rate for the serial port
 */
void BoardPrintfInit(uint32_t baud);

/**
 * Initialize the SPI controller. This function does all the initialization in order for the SPI
 * peripheral to work the the GT-202/QCA4004/2
 */
void AJ_WSL_SPI_InitializeSPIController(void);

/**
 * Sleep for a specified time
 *
 * @param time          Time to sleep in milliseconds
 */
void AJ_Sleep(uint32_t time);

#endif /* AJ_TARGET_MBED_H_ */
