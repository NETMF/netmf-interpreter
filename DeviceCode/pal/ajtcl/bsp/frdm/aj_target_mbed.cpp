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
/**
 * This C++ file is a wrapper for the MBED C++ API's so they can
 * be call-able from C. The file extension gets changed to .cpp
 * at compile time.
 */
#include "mbed.h"
#include "SDFileSystem.h"
#include "fsl_uart_hal.h"
#include "aj_target_platform.h"
#include "aj_target_rtos.h"
#include "aj_status.h"
#include "fsl_dspi_hal.h"
#include "aj_wsl_tasks.h"
#include "fsl_port_hal.h"
#include <stdarg.h>
#include "aj_debug.h"
#include "aj_crypto.h"

SDFileSystem sd(PTE3, PTE1, PTE2, PTE4, "sd");
Serial pc(PTB17, PTB16);
SPI spi(PTD2, PTD3, PTD1, NC);
DigitalOut* pwd;

InterruptIn* interrupt;

extern "C" {

void BoardPrintf(const char* fmat, ...)
{
    char buf[256];
    va_list args;
    va_start(args, fmat);
    vsnprintf(buf, 256, fmat, args);
    AJ_EnterCriticalRegion();
    pc.printf("%s", buf);
    AJ_LeaveCriticalRegion();
    va_end(args);
}

void BoardPrintfInit(uint32_t baud)
{
    pc.baud(baud);
}
/** TX interrupt occurred */
volatile uint8_t g_b_spi_interrupt_tx_ready = 0;
/** RX interrupt occurred */
volatile uint8_t g_b_spi_interrupt_rx_ready = 0;

volatile uint8_t g_b_spi_interrupt_data_ready = 0;

aj_spi_status AJ_SPI_WRITE(uint8_t* spi_device, uint8_t byte, uint8_t pcs, uint8_t cont)
{
    /*
     * spi_device is not used since we always know the SPI peripheral controlling
     * the WiFI chip
     */
    dspi_command_config_t commandConfig;
    commandConfig.isChipSelectContinuous = cont ? 0 : 1;
    commandConfig.whichCtar = kDspiCtar0;
    commandConfig.whichPcs = kDspiPcs0;
    commandConfig.clearTransferCount = true;
    commandConfig.isEndOfQueue = false;
    dspi_hal_write_data_master_mode(0, &commandConfig, byte);
    /* Wait for the TX Complete flag meaning the SPI operation (write) has finished */
    while (!dspi_hal_get_status_flag(0, kDspiTxComplete));
    /* Un-set the flag for the next call to AJ_SPI_WRITE */
    dspi_hal_clear_status_flag(0, kDspiTxComplete);

    return SPI_OK;
}

aj_spi_status AJ_SPI_READ(uint8_t spi_device, uint8_t* data, uint8_t pcs)
{
    /*
     * spi_device is not used since we always know the SPI peripheral controlling
     * the WiFi chip.
     * pcs (chip select) is not used in the context of read because in reality the
     * SPI read has already taken place. This call simply copies the data.
     */
    uint32_t read = dspi_hal_read_data(0);
    memcpy(data, (const void*)&read, 1);

    return SPI_OK;
}

AJ_Status AJ_WSL_SPI_DMATransfer(uint8_t* buffer, uint16_t len, uint8_t direction)
{
    int32_t i = 0;
    if (direction == 1) { //Transmit
        uint8_t toss;
        while (i < (len - 1)) {
            AJ_SPI_WRITE(0, *(buffer + i), 0, 0);
            AJ_SPI_READ(0, &toss, 0);
            i++;
        }
        AJ_SPI_WRITE(0, *(buffer + i), 0, 1);
        AJ_SPI_READ(0, &toss, 0);
    } else { // Receive
        while (i < (len - 1)) {
            AJ_SPI_WRITE(0, 0, 0, 0);
            AJ_SPI_READ(0, (buffer + i), 0);
            i++;
        }
        AJ_SPI_WRITE(0, 0, 0, 1);
        AJ_SPI_READ(0, (buffer + i), 1);
    }
    return AJ_OK;
}

void AJ_WSL_SPI_PowerCycleWiFiChip(void)
{
    pwd->write(0);
    AJ_Sleep(100);
    pwd->write(1);
}

extern struct AJ_TaskHandle* AJ_WSL_MBoxListenHandle;

void AJ_WSL_SPI_CHIP_SPI_ISR(void)
{
    __disable_irq();
    g_b_spi_interrupt_data_ready = TRUE;
    AJ_ResumeTask(AJ_WSL_MBoxListenHandle, TRUE);
    __enable_irq();
}

void AJ_WSL_SPI_InitializeSPIController(void)
{
    uint32_t calculatedBaudRate;
    dspi_master_config_t dspiConfig;
    dspi_delay_settings_config_t delayConfig;

    dspiConfig.isEnabled = false;
    dspiConfig.whichCtar = kDspiCtar0;
    dspiConfig.bitsPerSec = 0;
    dspiConfig.sourceClockInHz = 120000000;
    dspiConfig.isSckContinuous = false;
    dspiConfig.whichPcs = kDspiPcs0;
    dspiConfig.pcsPolarity = kDspiPcs_ActiveLow;
    dspiConfig.masterInSample = kDspiSckToSin_1Clock;
    dspiConfig.isModifiedTimingFormatEnabled = true;
    dspiConfig.isTxFifoDisabled = false;
    dspiConfig.isRxFifoDisabled = false;
    dspiConfig.dataConfig.bitsPerFrame = 8;
    dspiConfig.dataConfig.clkPolarity = kDspiClockPolarity_ActiveLow;
    dspiConfig.dataConfig.clkPhase = kDspiClockPhase_SecondEdge;
    dspiConfig.dataConfig.direction = kDspiMsbFirst;
    dspi_hal_disable(0);
    dspi_hal_master_init(0, &dspiConfig, &calculatedBaudRate);
    PORTD_PCR0 = PORT_PCR_MUX(2);

    delayConfig.pcsToSckPre = 0x4;
    delayConfig.pcsToSck = 0x4;
    delayConfig.afterSckPre = 0x3;
    delayConfig.afterSck = 0x3;
    delayConfig.afterTransferPre = 0x5;
    delayConfig.afterTransfer = 0x5;
    dspi_hal_configure_delays(0, kDspiCtar0, &delayConfig);
    dspi_hal_configure_interrupt(0, kDspiTxComplete, true);
    dspi_hal_enable(0);

    pwd = new DigitalOut(PTA1, 0);

    AJ_WSL_SPI_PowerCycleWiFiChip();

    NVIC_SetPriority(PORTC_IRQn, 0x0f);
    interrupt = new InterruptIn(PTC3);
    interrupt->mode(PullUp);
    interrupt->fall(&AJ_WSL_SPI_CHIP_SPI_ISR);
    NVIC_SetPriority(PORTC_IRQn, 0x0f);

    AJ_WSL_SPI_PowerCycleWiFiChip();
}

void AJ_WSL_SPI_ShutdownSPIController(void)
{
    if (pwd) {
        delete pwd;
        pwd = NULL;
    }
}

static uint8_t seed[16];
static uint8_t key[16];

static uint8_t RandBit(AnalogIn* adc)
{
    return (uint8_t)(adc->read_u16() & 1);
}

static void GatherBits(uint8_t* buffer, uint32_t len)
{
    int i;
    AnalogIn* adc = new AnalogIn(PTC10);
    if (adc) {
        memset(buffer, 0, len);
        for (i = 0; i < len; ++i) {
            int j;
            uint8_t r = 0;
            for (j = 0; j < 8; ++j) {
                r <<= 1;
                r |= RandBit(adc);
            }
            buffer[i] = r;
        }
        delete adc;
    } else {
        AJ_ErrPrintf(("GatherBits(): Could access ADC device\n"));
    }
#ifdef SHOW_RANDOM_BITS
    for (i = 0; i < len; ++i) {
        int j;
        int r = buffer[i];
        for (j = 0; j < 8; ++j) {
            AJ_Printf("%c", '0' + (r & 1));
            r >>= 1;
        }
    }
    AJ_Printf("\n");
#endif
}

/* Variable holding the number of milliseconds since startup */
extern uint32_t os_time;

void AJ_RandBytes(uint8_t* rand, uint32_t len)
{
    /*
     * If this is the first call we need to accumulate
     * entropy for the seed and key
     */
    if (seed[0] == 0) {
        GatherBits(seed, sizeof(seed));
        GatherBits(key, sizeof(key));
    }
    AJ_AES_Enable(key);

    while (len) {
        uint32_t tmp[4];
        uint32_t sz = min(16, len);
        uint32_t ticks = os_time; //Tick rate is 1000 Hz: 1 tick per millisecond
        tmp[0] = ticks;
        tmp[1] += 1;
        AJ_AES_ECB_128_ENCRYPT(key, (uint8_t*)tmp, (uint8_t*)tmp);
        AJ_AES_CBC_128_ENCRYPT(key, seed, seed, 16, (uint8_t*)tmp);
        memcpy(rand, seed, sz);
        AJ_AES_CBC_128_ENCRYPT(key, seed, seed, 16, (uint8_t*)tmp);
        len -= sz;
        rand += sz;
    }
    AJ_AES_Disable();
}

}
