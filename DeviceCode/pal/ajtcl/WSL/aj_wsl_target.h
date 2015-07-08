/**
 * @file WSL module header
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

#ifndef AJ_WSL_TARGET_H__
#define AJ_WSL_TARGET_H__

#include "aj_target_platform.h"
#include "aj_target.h"
#include "aj_RTOS.h"
#include "aj_status.h"

#ifdef __cplusplus
extern "C" {
#endif

void AJ_WSL_ModuleInit(void);
AJ_Status AJ_WSL_DriverStart(void);
AJ_Status AJ_WSL_DriverStop(void);
void AJ_WSL_NET_StackInit(void);

/*
 * Platform specific defines are here
 */
#define AJ_WSL_SPI_CLOCK_RATE 28000000
#define AJ_WSL_SPI_CLOCK_POLARITY 1
#define AJ_WSL_SPI_CLOCK_PHASE    0
#define AJ_WSL_SPI_DELAY_BEFORE_CLOCK   0x01
#define AJ_WSL_SPI_DELAY_BETWEEN_TRANSFERS 0x01

#ifdef AJ_WSL_USE_REGULAR_MALLOC
#define AJ_WSL_Malloc AJ_Malloc
#define AJ_WSL_Free AJ_Free

#else
/*
 *  WSL memory allocations come from a WSL-specific heap
 */
void* AJ_WSL_Malloc(size_t size);
void AJ_WSL_Free(void* ptr);
#endif
AJ_Status AJ_WSL_SPI_RegisterRead(uint16_t reg, uint8_t* spi_data);

/**
 * global flag that indicates the target chip has SPI data available
 */
extern volatile uint8_t g_b_spi_interrupt_data_ready;

/*
 * Set the native endianness
 */
#if HOST_IS_BIG_ENDIAN
    #define BE8_8_TO_CPU(v, w) (((v) << 8) | (w))
    #define BE16_TO_CPU(v) (v)
    #define BE32_TO_CPU(v) (v)
    #define CPU_TO_BE16(v) (v)
    #define CPU_TO_BE32(v) (v)
    #define LE8_8_TO_CPU(v, w) ((v) | ((w) << 8))
    #define LE16_TO_CPU(v) (((v) >> 8) | ((v) << 8))
    #define LE32_TO_CPU(v) (((v) >> 24) | (((v) & 0xFF0000) >> 8) | (((v) & 0x00FF00) << 8) | ((v) << 24))
    #define CPU_TO_LE16(v) (((v) >> 8) | ((v) << 8))
    #define CPU_TO_LE32(v) (((v) >> 24) | (((v) & 0xFF0000) >> 8) | (((v) & 0x00FF00) << 8) | ((v) << 24))
#else
    #define BE8_8_TO_CPU(v, w) ((v) | ((w) << 8))
#ifndef LE16_TO_CPU
    #define BE16_TO_CPU(v) (((v) >> 8) | ((v) << 8))
    #define BE32_TO_CPU(v) (((v) >> 24) | (((v) & 0xFF0000) >> 8) | (((v) & 0x00FF00) << 8) | ((v) << 24))
    #define CPU_TO_BE16(v) (((v) >> 8) | ((v) << 8))
    #define CPU_TO_BE32(v) (((v) >> 24) | (((v) & 0xFF0000) >> 8) | (((v) & 0x00FF00) << 8) | ((v) << 24))
#endif
    #define LE8_8_TO_CPU(v, w) (((v) << 8) | (w))
#ifndef LE16_TO_CPU
    #define LE16_TO_CPU(v) (v)
    #define LE32_TO_CPU(v) (v)
    #define CPU_TO_LE16(v) (v)
    #define CPU_TO_LE32(v) (v)
#endif
#endif

#ifdef __cplusplus
}
#endif

#endif // AJ_WSL_TARGET_H__
