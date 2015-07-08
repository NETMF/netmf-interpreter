#ifndef _AJ_TARGET_PLATFORM_H_
#define _AJ_TARGET_PLATFORM_H_
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

#ifdef __cplusplus
extern "C" {
#endif

#include <stdint.h>
#include "aj_target_mbed.h"

#define AJ_Printf BoardPrintf


#define A_UINT32 uint32_t

#ifdef AJ_NVRAM_SIZE
#undef AJ_NVRAM_SIZE
#define AJ_NVRAM_SIZE (0x10000)
#else
#define AJ_NVRAM_SIZE (0x10000)
#endif

#define AJ_WSL_SPI_DEVICE 0
#define AJ_WSL_SPI_DEVICE_ID 0
#define AJ_WSL_SPI_DEVICE_NPCS 0
#define AJ_WSL_SPI_PCS 0
#define AJ_WSL_SPI_CHIP_PWD_PIN 0
#define AJ_WSL_SPI_CHIP_SPI_INT_PIN 0
#define AJ_WSL_SPI_CHIP_SPI_INT_BIT 0
#define AJ_WSL_SPI_CHIP_POWER_PIN 0
#define AJ_WSL_STACK_SIZE   3000

void _AJ_NVRAM_Clear(void);
void AJ_NVRAM_Init(void);
void _AJ_PlatformInit(void);
uint8_t AJ_SeedRNG(void);


#ifdef __cplusplus
}
#endif


#endif

