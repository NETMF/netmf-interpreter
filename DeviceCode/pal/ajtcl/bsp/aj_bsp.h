/**
 * @file Platform specific function declarations that must be defined per platform port
 */
/******************************************************************************
 * Copyright (c) 2014, AllSeen Alliance. All rights reserved.
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

#ifndef AJ_BSP_H_
#define AJ_BSP_H_

#include "aj_target.h"

#ifdef __cplusplus
extern "C" {
#endif

/*
 * This file contains the layer between alljoyn and platform specific code.
 * All the functions in this header need to be implemented in the platform
 * folder for example 'due'.
 */

/*
 * This function should include any initialization that needs to be done
 * for the specific platform. (Clock, UART, SPI, GPIO etc.)
 */
void AJ_PlatformInit(void);

/*
 * This function should be implemented on a specific platform to write
 * a byte of data over SPI
 */
//aj_spi_status AJ_SPI_WRITE(uint8_t spi_device, uint8_t byte, uint8_t pcs, uint8_t cont);
/*
 * This function should be implemented on a specific platform to read
 * a byte of data over SPI
 */
//aj_spi_status AJ_SPI_READ(uint8_t spi_device, uint8_t* data, uint8_t pcs);

/*
 * This function should be implemented on a specific platform to initialize
 * the SPI hardware
 */
void AJ_WSL_SPI_InitializeSPIController(void);
/*
 * This function should be implemented on a specific platform to shutdown
 * the SPI hardware
 */
void AJ_WSL_SPI_ShutdownSPIController(void);
/*
 * This function should be implemented on a specific platform as the
 * SPI interrupt handler
 */
void AJ_WSL_SPI_ISR(void);

void AJ_WSL_SPI_CHIP_SPI_ISR(uint32_t id, uint32_t mask);
AJ_Status AJ_WSL_SPI_DMATransfer(uint8_t* buffer, uint16_t len, uint8_t direction);

#ifdef __cplusplus
}
#endif

#endif /* AJ_BSP_H_ */
