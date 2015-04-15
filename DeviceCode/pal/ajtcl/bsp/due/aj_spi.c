/**
 * @file SPI functionality
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

/******************************************************************************
 * Any time in this file there is a comment including:
    ioport_***, pio_***, pmc_***, spi_***, sysclk_***, dmac_***

 * note that the API associated with it may be subject to this Atmel license:
 * (information about it is also at www.atmel.com/asf)
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 1. Redistributions of source code must retain the above copyright notice, this
 *     list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 3. The name of Atmel may not be used to endorse or promote products derived
 *    from this software without specific prior written permission.
 * 4. This software may only be redistributed and used in connection with an
 *    Atmel microcontroller product.
 * THIS SOFTWARE IS PROVIDED BY ATMEL "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 * PARTICULAR PURPOSE AND NON-INFRINGEMENT ARE EXPRESSLY AND SPECIFICALLY DISCLAIMED. IN
 * NO EVENT SHALL ATMEL BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY,
 * OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
 * GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

#include "aj_target.h"
#include "aj_wsl_target.h"
#include "aj_status.h"
#include "aj_wsl_spi.h"
#include "aj_wsl_htc.h"
#include "aj_buf.h"
#include "aj_wsl_tasks.h"
#include "dmac.h"

/** DMAC receive channel of master. */
#define AJ_DMA_RX_CHANNEL       0
#define AJ_DMA_TX_CHANNEL       1

/** SPI DMA Operation type, receive or send */
#define AJ_DMA_RX       0
#define AJ_DMA_TX       1


/** DMAC Channel HW Interface Number for SPI. */
#define AJ_SPI_TX_INDEX         1
#define AJ_SPI_RX_INDEX         2

/** Address that can enable an interrupt in the system, from the datasheet */
#define AJ_SPI_ISER1_IEN_ADDR 0xE000E104
/** Bit value that will enable the DMAC interrupt, from the datasheet */
#define AJ_SPI_DMAC_IEN_BIT (1 << 7)

/**
 * Two-dimensional array of structures the control how the DMA controller
 * writes and reads data between RAM and the SPI hardware.
 * The structures contain source then destination address, control register
 * settings, followed by an empty field that would allow linking larger requests
 */
static const dma_transfer_descriptor_t transfer_descriptors[2][2] = {
    { /* AJ_DMA_RX descriptors */
        { /* AJ_DMA_RX_CHANNEL */
            (uint32_t) &SPI0->SPI_RDR,
            (uint32_t) NULL,
            DMAC_CTRLA_SRC_WIDTH_BYTE | DMAC_CTRLA_DST_WIDTH_BYTE,
            DMAC_CTRLB_SRC_DSCR | DMAC_CTRLB_DST_DSCR | DMAC_CTRLB_FC_PER2MEM_DMA_FC | DMAC_CTRLB_SRC_INCR_FIXED | DMAC_CTRLB_DST_INCR_INCREMENTING,
            (uint32_t) NULL
        },
        { /* AJ_DMA_TX_CHANNEL */
            (uint32_t) NULL,
            (uint32_t) &SPI0->SPI_TDR,
            DMAC_CTRLA_SRC_WIDTH_BYTE | DMAC_CTRLA_DST_WIDTH_BYTE,
            DMAC_CTRLB_SRC_DSCR | DMAC_CTRLB_DST_DSCR | DMAC_CTRLB_FC_MEM2PER_DMA_FC | DMAC_CTRLB_SRC_INCR_FIXED | DMAC_CTRLB_DST_INCR_FIXED,
            (uint32_t) NULL
        }
    },
    { /* AJ_DMA_TX descriptors */
        { /* AJ_DMA_RX_CHANNEL */
            (uint32_t) &SPI0->SPI_RDR,
            (uint32_t) NULL,
            DMAC_CTRLA_SRC_WIDTH_BYTE | DMAC_CTRLA_DST_WIDTH_BYTE,
            DMAC_CTRLB_SRC_DSCR | DMAC_CTRLB_DST_DSCR | DMAC_CTRLB_FC_PER2MEM_DMA_FC | DMAC_CTRLB_SRC_INCR_FIXED | DMAC_CTRLB_DST_INCR_FIXED,
            (uint32_t) NULL
        },
        { /* AJ_DMA_TX_CHANNEL */
            (uint32_t) NULL,
            (uint32_t) &SPI0->SPI_TDR,
            DMAC_CTRLA_SRC_WIDTH_BYTE | DMAC_CTRLA_DST_WIDTH_BYTE,
            DMAC_CTRLB_SRC_DSCR | DMAC_CTRLB_DST_DSCR | DMAC_CTRLB_FC_MEM2PER_DMA_FC | DMAC_CTRLB_SRC_INCR_INCREMENTING | DMAC_CTRLB_DST_INCR_FIXED,
            (uint32_t) NULL
        }
    }
};



void AJ_WSL_SPI_CHIP_SPI_ISR(uint32_t id, uint32_t mask);

/*
 * Configure the SPI hardware, including SPI clock speed, mode, delays, chip select pins
 * It uses values listed in
 */
void AJ_WSL_SPI_InitializeSPIController(void)
{
    uint32_t config;

    /* Initialize and enable DMA controller. */
    pmc_enable_periph_clk(ID_DMAC);
    dmac_init(DMAC);
    dmac_set_priority_mode(DMAC, DMAC_PRIORITY_ROUND_ROBIN);
    dmac_enable(DMAC);

    /* Configure DMA TX channel. */
    config = 0;
    config |= DMAC_CFG_DST_PER(AJ_SPI_TX_INDEX) |
              DMAC_CFG_DST_H2SEL |
              DMAC_CFG_SOD | DMAC_CFG_FIFOCFG_ALAP_CFG;
    dmac_channel_set_configuration(DMAC, AJ_DMA_TX_CHANNEL, config);

    /* Configure DMA RX channel. */
    config = 0;
    config |= DMAC_CFG_SRC_PER(AJ_SPI_RX_INDEX) |
              DMAC_CFG_SRC_H2SEL |
              DMAC_CFG_SOD | DMAC_CFG_FIFOCFG_ALAP_CFG;
    dmac_channel_set_configuration(DMAC, AJ_DMA_RX_CHANNEL, config);

    /* Enable receive channel interrupt for DMAC. */
    uint8_t* interruptEnableAddress = AJ_SPI_ISER1_IEN_ADDR;
    *interruptEnableAddress = AJ_SPI_DMAC_IEN_BIT;

    dmac_enable_interrupt(DMAC, (1 << AJ_DMA_RX_CHANNEL));
    dmac_enable_interrupt(DMAC, (1 << AJ_DMA_TX_CHANNEL));
    //AJ_WSL_DMA_Setup();
    dmac_channel_disable(DMAC, AJ_DMA_TX_CHANNEL);
    dmac_channel_disable(DMAC, AJ_DMA_RX_CHANNEL);

    /*
     * Configure the hardware to enable SPI and some output pins
     */
    {
        pmc_enable_periph_clk(ID_PIOA);
        pmc_enable_periph_clk(ID_PIOB);
        pmc_enable_periph_clk(ID_PIOC);
        pmc_enable_periph_clk(ID_PIOD);


        // make all of these pins controlled by the right I/O controller
        pio_configure_pin_group(PIOA, 0xFFFFFFFF, PIO_TYPE_PIO_PERIPH_A);
        pio_configure_pin_group(PIOB, 0xFFFFFFFF, PIO_TYPE_PIO_PERIPH_B);
        pio_configure_pin_group(PIOC, 0xFFFFFFFF, PIO_TYPE_PIO_PERIPH_C);
        pio_configure_pin_group(PIOD, 0xFFFFFFFF, PIO_TYPE_PIO_PERIPH_D);


        /*
         * Reset the device by toggling the CHIP_POWER
         */
        ioport_set_pin_dir(AJ_WSL_SPI_CHIP_POWER_PIN, IOPORT_DIR_OUTPUT);
        ioport_set_pin_level(AJ_WSL_SPI_CHIP_POWER_PIN, IOPORT_PIN_LEVEL_LOW);
        AJ_Sleep(10);
        ioport_set_pin_level(AJ_WSL_SPI_CHIP_POWER_PIN, IOPORT_PIN_LEVEL_HIGH);


        /*
         * Reset the device by toggling the CHIP_PWD# signal
         */
        ioport_set_pin_dir(AJ_WSL_SPI_CHIP_PWD_PIN, IOPORT_DIR_OUTPUT);
        ioport_set_pin_level(AJ_WSL_SPI_CHIP_PWD_PIN, IOPORT_PIN_LEVEL_LOW);
        AJ_Sleep(10);
        ioport_set_pin_level(AJ_WSL_SPI_CHIP_PWD_PIN, IOPORT_PIN_LEVEL_HIGH);

        /* configure the pin that detects SPI data ready from the target chip */
        ioport_set_pin_dir(AJ_WSL_SPI_CHIP_SPI_INT_PIN, IOPORT_DIR_INPUT);
        ioport_set_pin_sense_mode(AJ_WSL_SPI_CHIP_SPI_INT_PIN, IOPORT_SENSE_LEVEL_LOW);

        pio_handler_set(PIOC, ID_PIOC, AJ_WSL_SPI_CHIP_SPI_INT_BIT, (PIO_PULLUP | PIO_IT_FALL_EDGE), &AJ_WSL_SPI_CHIP_SPI_ISR);
        pio_handler_set_priority(PIOD, (IRQn_Type) ID_PIOC, 0xB);
        pio_enable_interrupt(PIOC, AJ_WSL_SPI_CHIP_SPI_INT_BIT);
    }

    spi_enable_clock(AJ_WSL_SPI_DEVICE);
    spi_reset(AJ_WSL_SPI_DEVICE);
    spi_set_lastxfer(AJ_WSL_SPI_DEVICE);
    spi_set_master_mode(AJ_WSL_SPI_DEVICE);
    spi_disable_mode_fault_detect(AJ_WSL_SPI_DEVICE);
    spi_set_peripheral_chip_select_value(AJ_WSL_SPI_DEVICE, AJ_WSL_SPI_DEVICE_NPCS);
    spi_set_clock_polarity(AJ_WSL_SPI_DEVICE, AJ_WSL_SPI_DEVICE_NPCS, AJ_WSL_SPI_CLOCK_POLARITY);
    spi_set_clock_phase(AJ_WSL_SPI_DEVICE, AJ_WSL_SPI_DEVICE_NPCS, AJ_WSL_SPI_CLOCK_PHASE);
    spi_set_bits_per_transfer(AJ_WSL_SPI_DEVICE, AJ_WSL_SPI_DEVICE_NPCS, SPI_CSR_BITS_8_BIT);
    spi_set_baudrate_div(AJ_WSL_SPI_DEVICE, AJ_WSL_SPI_DEVICE_NPCS, (sysclk_get_cpu_hz() / AJ_WSL_SPI_CLOCK_RATE));
    spi_set_transfer_delay(AJ_WSL_SPI_DEVICE, AJ_WSL_SPI_DEVICE_NPCS, AJ_WSL_SPI_DELAY_BEFORE_CLOCK, AJ_WSL_SPI_DELAY_BETWEEN_TRANSFERS);
    spi_set_fixed_peripheral_select(AJ_WSL_SPI_DEVICE);
    spi_configure_cs_behavior(AJ_WSL_SPI_DEVICE, AJ_WSL_SPI_DEVICE_NPCS, SPI_CS_RISE_FORCED);

    spi_enable_interrupt(AJ_WSL_SPI_DEVICE, SPI_IER_TDRE | SPI_IER_RDRF);
    spi_enable(AJ_WSL_SPI_DEVICE);
}


/**
 * send_done is set to non-zero when a DMA operation completes,
 * This allows AJ_WSL_SPI_DMATransfer to finish
 */
volatile uint32_t AJ_WSL_DMA_send_done = 0;

/**
 * This interrupt handler is called when a DMA operation completes and clears the waiting code
 * (via send_done) The handler overrides the weak reference to the default interrupt handler.
 */
void DMAC_Handler(void)
{
    uint32_t ret;

    ret = dmac_get_status(DMAC);

    if (ret & (1 << AJ_DMA_TX_CHANNEL)) {
        AJ_WSL_DMA_send_done = 1;
    }
}

void AJ_WSL_SPI_DMATransfer(void* buffer, uint32_t size, uint8_t direction)
{
    dma_transfer_descriptor_t transfer;

    AJ_ASSERT(AJ_WSL_DMA_send_done == 0);

    /* Disable both channels before parameters are set */
    dmac_channel_disable(DMAC, AJ_DMA_TX_CHANNEL);
    dmac_channel_disable(DMAC, AJ_DMA_RX_CHANNEL);
    if (direction == AJ_DMA_TX) {
        /* Direction is TX so set the destination to the SPI hardware */
        transfer = transfer_descriptors[AJ_DMA_TX][AJ_DMA_TX_CHANNEL];
        /* Set the source to the buffer your sending */
        transfer.ul_source_addr = (uint32_t) buffer;
        transfer.ul_ctrlA |= size;
        dmac_channel_single_buf_transfer_init(DMAC, AJ_DMA_TX_CHANNEL, (dma_transfer_descriptor_t*) &transfer);
        /* Enable the channel to start DMA */
        dmac_channel_enable(DMAC, AJ_DMA_TX_CHANNEL);

        /* Setup RX direction as NULL destination and SPI0 as source */
        transfer = transfer_descriptors[AJ_DMA_TX][AJ_DMA_RX_CHANNEL];
        transfer.ul_ctrlA |= size;

        dmac_channel_single_buf_transfer_init(DMAC, AJ_DMA_RX_CHANNEL, &transfer);
        /* Enable the channel to start DMA */
        dmac_channel_enable(DMAC, AJ_DMA_RX_CHANNEL);
        /* Wait for the transfer to complete */
        while (!AJ_WSL_DMA_send_done) ;
    } else {
        /* We are transferring in the RX direction */
        /* Set up the destination address */
        transfer = transfer_descriptors[AJ_DMA_RX][AJ_DMA_RX_CHANNEL];
        transfer.ul_destination_addr = (uint32_t) buffer;
        transfer.ul_ctrlA |= size;


        dmac_channel_single_buf_transfer_init(DMAC, AJ_DMA_RX_CHANNEL, &transfer);
        dmac_channel_enable(DMAC, AJ_DMA_RX_CHANNEL);
        /* Setup the TX channel to transfer from a NULL pointer
         * This must be done in order for the transfer to start
         */
        transfer = transfer_descriptors[AJ_DMA_RX][AJ_DMA_TX_CHANNEL];
        transfer.ul_ctrlA |= size;

        dmac_channel_single_buf_transfer_init(DMAC, AJ_DMA_TX_CHANNEL, (dma_transfer_descriptor_t*) &transfer);
        dmac_channel_enable(DMAC, AJ_DMA_TX_CHANNEL);
        while (!AJ_WSL_DMA_send_done) ;
    }
    /* reset the DMA completed indicator */
    AJ_WSL_DMA_send_done = 0;
    dmac_channel_disable(DMAC, AJ_DMA_TX_CHANNEL);
    dmac_channel_disable(DMAC, AJ_DMA_RX_CHANNEL);
}

void AJ_WSL_SPI_ShutdownSPIController(void)
{
    spi_disable(AJ_WSL_SPI_DEVICE);
    spi_disable_interrupt(AJ_WSL_SPI_DEVICE, 0xFFFFFFFF);
}

aj_spi_status AJ_SPI_READ(Spi* p_spi, uint8_t* us_data, uint8_t* p_pcs)
{
    aj_spi_status status;
    uint16_t data;
    status = spi_read(p_spi, &data, p_pcs);
    AJ_ASSERT(status == SPI_OK);
    *us_data = data & 0xFF;
    //    AJ_InfoPrintf(("=R= %02x\n", (*us_data) & 0xFF));
    return status;
}

aj_spi_status AJ_SPI_WRITE(Spi* p_spi, uint16_t us_data, uint8_t uc_pcs, uint8_t uc_last)
{
    aj_spi_status status;
//    AJ_InfoPrintf(("=WRITE= %x\n", us_data));
    status = spi_write(p_spi, us_data, uc_pcs, uc_last);
    AJ_ASSERT(status == SPI_OK);
    return status;
}

/*
 *  These routines are specific to the hardware target, so they should exist in an  aj_target_wsl_spi.c
 */


/** TX interrupt occurred */
volatile uint8_t g_b_spi_interrupt_tx_ready = false;
/** RX interrupt occurred */
volatile uint8_t g_b_spi_interrupt_rx_ready = false;

extern struct AJ_TaskHandle* AJ_WSL_MBoxListenHandle;
/**
 * SPI interrupt service routine
 */
void AJ_WSL_SPI_ISR(void)
{
    uint32_t status = spi_read_status(AJ_WSL_SPI_DEVICE);

    if (status & SPI_SR_TDRE) {
        //g_b_spi_interrupt_tx_ready = true;
        spi_disable_interrupt(AJ_WSL_SPI_DEVICE, SPI_IDR_TDRE);
    }

    if (status & SPI_SR_RDRF) {
        //g_b_spi_interrupt_rx_ready = true;
        spi_disable_interrupt(AJ_WSL_SPI_DEVICE, SPI_IDR_RDRF);
    }
}

volatile uint8_t g_b_spi_interrupt_data_ready = false;

/**
 * ISR that handles the target chip asserting a data ready signal.
 */
void AJ_WSL_SPI_CHIP_SPI_ISR(uint32_t id, uint32_t mask)
{
    if (ID_PIOC != id || AJ_WSL_SPI_CHIP_SPI_INT_BIT != mask) {
        return;
    }
    pio_disable_interrupt(PIOC, AJ_WSL_SPI_CHIP_SPI_INT_BIT);
    if (mask & AJ_WSL_SPI_CHIP_SPI_INT_BIT) {
        g_b_spi_interrupt_data_ready = TRUE;
        AJ_ResumeTask(AJ_WSL_MBoxListenHandle, TRUE);
    }
    pio_enable_interrupt(PIOC, AJ_WSL_SPI_CHIP_SPI_INT_BIT);
}


