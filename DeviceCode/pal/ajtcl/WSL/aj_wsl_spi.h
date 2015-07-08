/**
 * @file SPI function declarations
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

#ifndef AJ_WSL_SPI_H_
#define AJ_WSL_SPI_H_

#include "aj_target.h"
#include "aj_status.h"
#include "aj_wsl_target.h"
#include "aj_wsl_spi_constants.h"
#include "aj_buf.h"

#ifdef __cplusplus
extern "C" {
#endif

#pragma pack(push, 1)

/*
 *  These methods are
 */
void AJ_WSL_SPI_ModuleInit(void);

/*
 *  These methods are SPI module interfaces, and are target-specific implementations
 */
void AJ_WSL_SPI_InitializeSPIController(void);
void AJ_WSL_SPI_ShutdownSPIController(void);

/*
 * signal to the SPI hardware if more data is coming in this transaction
 */
#define AJ_WSL_SPI_CONTINUE 0
#define AJ_WSL_SPI_END 1

/*
 * Define the layout of the SPI commands between the host and target
 */
#define AJ_WSL_SPI_WRITE 0
#define AJ_WSL_SPI_READ  1
#define AJ_WSL_SPI_EXTERNAL   0
#define AJ_WSL_SPI_INTERNAL   1

#define AJ_WSL_SPI_REG_READ  ((AJ_WSL_SPI_READ << 1) | AJ_WSL_SPI_INTERNAL)
#define AJ_WSL_SPI_REG_WRITE ((AJ_WSL_SPI_WRITE << 1) | AJ_WSL_SPI_INTERNAL)
#define AJ_WSL_SPI_DMA_READ  ((AJ_WSL_SPI_READ << 1) | AJ_WSL_SPI_EXTERNAL)
#define AJ_WSL_SPI_DMA_WRITE ((AJ_WSL_SPI_WRITE << 1) | AJ_WSL_SPI_EXTERNAL)
#define AJ_WSL_SPI_REG_EXTERNAL_WRITE ((AJ_WSL_SPI_WRITE << 1) | AJ_WSL_SPI_EXTERNAL)

#define AJ_WSL_SPI_HDR_REG_READ  ((uint16_t) AJ_WSL_SPI_REG_READ << 14)
#define AJ_WSL_SPI_HDR_REG_WRITE ((uint16_t) AJ_WSL_SPI_REG_WRITE << 14)
#define AJ_WSL_SPI_HDR_DMA_READ  ((uint16_t) AJ_WSL_SPI_DMA_READ << 14)
#define AJ_WSL_SPI_HDR_DMA_WRITE ((uint16_t) AJ_WSL_SPI_DMA_WRITE << 14)

typedef struct _wsl_spi_command {
    uint16_t cmd_addr : 14,
             cmd_reg : 1,
             cmd_rx : 1;
} wsl_spi_command;

typedef struct _wsl_spi_status {
    uint8_t ready : 1,
            wr_err : 1,
            rd_err : 1,
            addr_err : 1,
            res : 4;
} wsl_spi_status;

/*
 * Global definitions
 */
extern wsl_spi_command qsc_HOST_INT_STATUS;

AJ_Status AJ_WSL_SPI_WriteByte8(uint8_t spi_data, uint8_t end);
AJ_Status AJ_WSL_SPI_WriteByte16(uint16_t spi_data, uint8_t end);

/*
 * Helper functions
 */
AJ_Status AJ_WSL_GetDMABufferSize(uint16_t* dmaSize);
AJ_Status AJ_WSL_SetDMABufferSize(uint16_t dmaSize);
AJ_Status AJ_WSL_GetWriteBufferSpaceAvailable(uint16_t* spaceAvailable);
AJ_Status AJ_WSL_GetReadBufferSpaceAvailable(uint16_t* spaceAvailable);



AJ_Status AJ_WSL_SPI_RegisterRead(uint16_t reg, uint8_t* spi_data);
AJ_Status AJ_WSL_SPI_RegisterWrite(uint16_t reg, uint16_t spi_data);
AJ_Status AJ_WSL_SPI_DMAWriteStart(uint16_t targetAddress);
AJ_Status AJ_WSL_SPI_DMAWrite8(uint16_t targetAddress, uint16_t len, uint8_t* spi_data);
AJ_Status AJ_WSL_SPI_DMAWrite16(uint16_t targetAddress, uint16_t len, uint16_t* spi_data);
AJ_Status AJ_WSL_SPI_DMARead16(uint16_t targetAddress, uint16_t len, uint16_t* spi_data);

AJ_Status AJ_WSL_SPI_HostControlRegisterWrite(uint32_t targetRegister, uint8_t increment, uint16_t cbLen, uint8_t* spi_data);
AJ_Status AJ_WSL_SPI_HostControlRegisterRead(uint32_t targetRegister, uint8_t increment, uint16_t cbLen, uint8_t* spi_data);

AJ_EXPORT AJ_Status AJ_WSL_ReadFromMBox(uint8_t box, uint16_t* len, uint8_t** buf);

AJ_EXPORT AJ_Status AJ_WSL_WriteBufListToMBox(uint8_t box, uint8_t endpoint, uint16_t len, AJ_BufList* list);

#pragma pack(pop)

#ifdef __cplusplus
}
#endif

#endif /* AJ_WSL_SPI_H_ */
