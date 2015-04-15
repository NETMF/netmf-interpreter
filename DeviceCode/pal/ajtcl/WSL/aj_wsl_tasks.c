/**
 * @file Implementation for tasks related to WSL
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
#define AJ_MODULE WSL_TASKS

#include "aj_target.h"
#include "aj_buf.h"
#include "aj_wsl_spi_constants.h"
#include "aj_wsl_wmi.h"
#include "aj_wsl_htc.h"
#include "aj_wsl_net.h"
#include "aj_wsl_spi.h"
#include "aj_wsl_tasks.h"


/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgWSL_TASKS = 0;
#endif

extern uint32_t AJ_WSL_MBOX_BLOCK_SIZE;

static void set_SPI_registers(void)
{
    volatile uint16_t spi_API = 0;

    AJ_AlwaysPrintf(("\n\n**************\nTEST:  %s\n\n", __FUNCTION__));

    // reset the target
    //    spi_API = (1 << 15);
    //    AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_SPI_CONFIG, spi_API);
    //    AJ_Sleep(1 << 22);

    // one extra write to force the device out of the reset state.
    spi_API = 0x80;
    AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_SPI_CONFIG, spi_API);
    AJ_Sleep(100);

    // write
    spi_API = 0x80; // same as capture
    AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_SPI_CONFIG, spi_API);
    //    AJ_InfoPrintf(("AJ_WSL_SPI_REG_SPI_CONFIG    was %04x\n", spi_API));

    AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_WRBUF_SPC_AVA, (uint8_t*)&spi_API);
    spi_API = LE16_TO_CPU(spi_API);
    AJ_InfoPrintf(("AJ_WSL_SPI_REG_WRBUF_SPC_AVA          was %04x\n", spi_API));


    spi_API = 0x40; // same as capture
    AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_WRBUF_WATERMARK, spi_API);
    //    AJ_InfoPrintf(("AJ_WSL_SPI_REG_SPI_CONFIG    was %04x\n", spi_API));

    spi_API = 0x400; // same as capture
    AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_INTR_CAUSE, spi_API);
    AJ_InfoPrintf(("AJ_WSL_SPI_REG_INTR_CAUSE             was %04x\n", spi_API));

    spi_API = 0x3ff;
    AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_INTR_ENABLE, spi_API);
    AJ_InfoPrintf(("AJ_WSL_SPI_REG_INTR_ENABLE    was %04x\n", spi_API));

    spi_API = 0x0e;
    AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_INTR_ENABLE, spi_API);
    AJ_InfoPrintf(("AJ_WSL_SPI_REG_INTR_ENABLE    was %04x\n", spi_API));

    spi_API = 0x1e;
    AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_INTR_ENABLE, spi_API);
    AJ_InfoPrintf(("AJ_WSL_SPI_REG_INTR_ENABLE    was %04x\n", spi_API));

    spi_API = 0x0;
    AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_INTR_ENABLE, spi_API);
    AJ_InfoPrintf(("AJ_WSL_SPI_REG_INTR_ENABLE    was %04x\n", spi_API));

    spi_API = 0x1e;
    AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_INTR_ENABLE, spi_API);
    AJ_InfoPrintf(("AJ_WSL_SPI_REG_INTR_ENABLE    was %04x\n", spi_API));

    AJ_Sleep(100);

}

/*
 * This test reads a few registers using the SPI hardware.
 */
/*
   static void examine_SPI_registers(void)
   {
    uint16_t spi_API = 0;
    uint16_t spi_lookahead[2];

    AJ_AlwaysPrintf(("\n\n**************\nTEST:  %s\n\n", __FUNCTION__));


    AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_DMA_SIZE, (uint8_t*)&spi_API);
    spi_API = LE16_TO_CPU(spi_API);
    AJ_InfoPrintf(("AJ_WSL_SPI_REG_DMA_SIZE               was %04x\n", spi_API));
    AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_WRBUF_SPC_AVA, (uint8_t*)&spi_API);
    spi_API = LE16_TO_CPU(spi_API);

    AJ_InfoPrintf(("AJ_WSL_SPI_REG_WRBUF_SPC_AVA          was %04x\n", spi_API));
    AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_RDBUF_BYTE_AVA, (uint8_t*)&spi_API);
    spi_API = LE16_TO_CPU(spi_API);
    AJ_InfoPrintf(("AJ_WSL_SPI_REG_RDBUF_BYTE_AVA         was %04x\n", spi_API));

    AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_SPI_CONFIG, (uint8_t*)&spi_API);
    spi_API = LE16_TO_CPU(spi_API);
    AJ_InfoPrintf(("AJ_WSL_SPI_REG_SPI_CONFIG             was %04x\n", spi_API));

    AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_SPI_STATUS, (uint8_t*)&spi_API);
    spi_API = LE16_TO_CPU(spi_API);
    AJ_InfoPrintf(("AJ_WSL_SPI_REG_SPI_STATUS             was %04x\n", spi_API));

    AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_HOST_CTRL_BYTE_SIZE, (uint8_t*)&spi_API);
    spi_API = LE16_TO_CPU(spi_API);
    AJ_InfoPrintf(("AJ_WSL_SPI_REG_HOST_CTRL_BYTE_SIZE    was %04x\n", spi_API));

    AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_HOST_CTRL_CONFIG, (uint8_t*)&spi_API);
    spi_API = LE16_TO_CPU(spi_API);
    AJ_InfoPrintf(("AJ_WSL_SPI_REG_HOST_CTRL_CONFIG       was %04x\n", spi_API));


    AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_INTR_CAUSE, (uint8_t*)&spi_API);
    spi_API = LE16_TO_CPU(spi_API);
    AJ_InfoPrintf(("AJ_WSL_SPI_REG_INTR_CAUSE             was %04x\n", spi_API));


    AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_WRBUF_WATERMARK, (uint8_t*)&spi_API);
    spi_API = CPU_TO_LE16(spi_API);
    AJ_InfoPrintf(("AJ_WSL_SPI_REG_WRBUF_WATERMARK  was %04x\n", spi_API));


    memset(spi_lookahead, 0, sizeof(spi_lookahead));
    AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_RDBUF_LOOKAHEAD1, (uint8_t*)&spi_lookahead[0]);
    spi_API = LE16_TO_CPU(spi_API);
    AJ_InfoPrintf(("AJ_WSL_SPI_REG_RDBUF_LOOKAHEAD1 was %04x\n", spi_lookahead[0]));
    AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_RDBUF_LOOKAHEAD2, (uint8_t*)&spi_lookahead[1]);
    spi_API = LE16_TO_CPU(spi_API);
    AJ_InfoPrintf(("AJ_WSL_SPI_REG_RDBUF_LOOKAHEAD2 was %04x\n", spi_lookahead[1]));

   }
 */

static void write_BOOT_PARAM(void)
{
    uint32_t spi_API = 0;
    uint16_t spi_API16 = 0;
    AJ_AlwaysPrintf(("\n\n**************\nTEST:  %s\n\n", __FUNCTION__));


    // read the clock speed value
    spi_API = 0x88888888;
    AJ_WSL_SPI_HostControlRegisterWrite(AJ_WSL_SPI_TARGET_ADDR_READ + 1, FALSE, 4, (uint8_t*)&spi_API);
    spi_API = 0x42424242;
    AJ_WSL_SPI_HostControlRegisterWrite(AJ_WSL_SPI_TARGET_ADDR_READ + 2, FALSE, 4, (uint8_t*)&spi_API);
    spi_API = 0x00000000;
    AJ_WSL_SPI_HostControlRegisterWrite(AJ_WSL_SPI_TARGET_ADDR_READ + 3, FALSE, 4, (uint8_t*)&spi_API);
    spi_API = AJ_WSL_SPI_TARGET_CLOCK_SPEED_ADDR; //0x00428878;
    spi_API = CPU_TO_LE32(spi_API);
    AJ_WSL_SPI_HostControlRegisterWrite(AJ_WSL_SPI_TARGET_ADDR_READ, TRUE, 4, (uint8_t*)&spi_API);
    // now read back the value from the data port.
    AJ_WSL_SPI_HostControlRegisterRead(AJ_WSL_SPI_TARGET_VALUE, TRUE, 4, (uint8_t*)&spi_API);
    spi_API = LE32_TO_CPU(spi_API);
    //AJ_InfoPrintf(("cycles read back          was %ld \n", spi_API));


    // read the flash is present value
    {
        // let's try this dance of writing multiple times...
        spi_API = 0x88888888;
        AJ_WSL_SPI_HostControlRegisterWrite(AJ_WSL_SPI_TARGET_ADDR_READ + 1, FALSE, 4, (uint8_t*)&spi_API);
        spi_API = 0x42424242;
        AJ_WSL_SPI_HostControlRegisterWrite(AJ_WSL_SPI_TARGET_ADDR_READ + 2, FALSE, 4, (uint8_t*)&spi_API);
        spi_API = 0x00000000;
        AJ_WSL_SPI_HostControlRegisterWrite(AJ_WSL_SPI_TARGET_ADDR_READ + 3, FALSE, 4, (uint8_t*)&spi_API);
        spi_API = AJ_WSL_SPI_TARGET_FLASH_PRESENT_ADDR; //0x0042880C;
        spi_API = CPU_TO_LE32(spi_API);
        AJ_WSL_SPI_HostControlRegisterWrite(AJ_WSL_SPI_TARGET_ADDR_READ, TRUE, 4, (uint8_t*)&spi_API);

        // now read back the value from the data port.
        AJ_WSL_SPI_HostControlRegisterRead(AJ_WSL_SPI_TARGET_VALUE, TRUE, 4, (uint8_t*)&spi_API);
        spi_API = LE32_TO_CPU(spi_API);

        //AJ_InfoPrintf(("host if flash is present read back          was %ld \n", spi_API));

    }

    // now write out the flash_is_present value
    spi_API = 0x00000002;
    spi_API = CPU_TO_LE32(spi_API);
    AJ_WSL_SPI_HostControlRegisterWrite(AJ_WSL_SPI_TARGET_VALUE, TRUE, 4, (uint8_t*)&spi_API);

    spi_API = 0x88888888;
    AJ_WSL_SPI_HostControlRegisterWrite(AJ_WSL_SPI_TARGET_ADDR_WRITE + 1, FALSE, 4, (uint8_t*)&spi_API);
    spi_API = 0x42424242;
    AJ_WSL_SPI_HostControlRegisterWrite(AJ_WSL_SPI_TARGET_ADDR_WRITE + 2, FALSE, 4, (uint8_t*)&spi_API);
    spi_API = 0x00000000;
    AJ_WSL_SPI_HostControlRegisterWrite(AJ_WSL_SPI_TARGET_ADDR_WRITE + 3, FALSE, 4, (uint8_t*)&spi_API);
    spi_API = AJ_WSL_SPI_TARGET_FLASH_PRESENT_ADDR; //0x0042880C;
    spi_API = CPU_TO_LE32(spi_API);
    AJ_WSL_SPI_HostControlRegisterWrite(AJ_WSL_SPI_TARGET_ADDR_WRITE, TRUE, 4, (uint8_t*)&spi_API);



    // read the mbox block size
    spi_API = 0x88888888;
    AJ_WSL_SPI_HostControlRegisterWrite(AJ_WSL_SPI_TARGET_ADDR_READ + 1, FALSE, 4, (uint8_t*)&spi_API);
    spi_API = 0x42424242;
    AJ_WSL_SPI_HostControlRegisterWrite(AJ_WSL_SPI_TARGET_ADDR_READ + 2, FALSE, 4, (uint8_t*)&spi_API);
    spi_API = 0x00000000;
    AJ_WSL_SPI_HostControlRegisterWrite(AJ_WSL_SPI_TARGET_ADDR_READ + 3, FALSE, 4, (uint8_t*)&spi_API);
    spi_API = AJ_WSL_SPI_TARGET_MBOX_BLOCKSZ_ADDR; //0x0042886C;
    spi_API = CPU_TO_LE32(spi_API);
    AJ_WSL_SPI_HostControlRegisterWrite(AJ_WSL_SPI_TARGET_ADDR_READ, TRUE, 4, (uint8_t*)&spi_API);
    // now read back the value from the data port.
    AJ_WSL_SPI_HostControlRegisterRead(AJ_WSL_SPI_TARGET_VALUE, TRUE, 4, (uint8_t*)&spi_API);
    spi_API = LE32_TO_CPU(spi_API);
    AJ_WSL_MBOX_BLOCK_SIZE = spi_API;
    //AJ_InfoPrintf(("block size           was %ld \n", spi_API));




    spi_API16 = 0x001f;
    spi_API16 = CPU_TO_LE16(spi_API16);
    AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_INTR_ENABLE, spi_API16);


    // wait until the write has been processed.
    spi_API16 = 0;
    while (!(spi_API16 & 1)) {
        AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_SPI_STATUS, (uint8_t*)&spi_API16);
        spi_API16 = LE16_TO_CPU(spi_API16);
        uint16_t space = 0;
        AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_WRBUF_SPC_AVA, (uint8_t*)&space);
    }
    // clear the read and write interrupt cause register
    spi_API = (1 << 9) | (1 << 8);
    spi_API = CPU_TO_LE16(spi_API);
    AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_INTR_CAUSE, spi_API);

    {
        spi_API16 = 0x1;
        spi_API16 = CPU_TO_LE16(spi_API16);
        AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_HOST_CTRL_BYTE_SIZE, spi_API16);

        // waiting seems to allow the following write to succeed and thus enable interrupts.
        // we need something more deterministic.
        AJ_Sleep(1000);
        spi_API16 = 0x00FF;
        spi_API = CPU_TO_LE16(spi_API);
        AJ_WSL_SPI_HostControlRegisterWrite(AJ_WSL_SPI_CPU_INT_STATUS, FALSE, 1, (uint8_t*)&spi_API16);
    }



}

extern AJ_WSL_HTC_CONTEXT AJ_WSL_HTC_Global;
extern wsl_socket_context AJ_WSL_SOCKET_CONTEXT[5];
extern volatile uint8_t g_b_spi_interrupt_data_ready;
void AJ_WSL_MBoxListenAndProcessTask(void* parameters)
{
    g_b_spi_interrupt_data_ready = FALSE;
    AJ_WSL_SPI_InitializeSPIController();

    set_SPI_registers();
    write_BOOT_PARAM();

    g_b_spi_interrupt_data_ready = FALSE;

    // loop and process all of the responses
    while (1) {
        AJ_Status status;

        AJ_WSL_HTC_ProcessInterruptCause();

        uint8_t i;
        for (i = 0; i < AJ_WSL_SOCKET_MAX; i++) {
            do {
                wsl_work_item* item = NULL;

                // peek at the queue, then make sure we have enough credits
                // if we have enough, pull and send the workitem to the MBOX
                // otherwise, move to the next socket and then eventually out of this loop.
                // Credits would be available again once AJ_WSL_HTC_ProcessInterruptCause has
                // read any credit-adjustment trailers
                status = AJ_QueuePeek(AJ_WSL_SOCKET_CONTEXT[i].workTxQueue, &item);
                if ((status != AJ_OK) || (AJ_WSL_HTC_Global.endpoints[item->endpoint].txCredits < 1)) {
                    break;
                }

                // pull a work item off of a socket queue and send it
                status = AJ_QueuePull(AJ_WSL_SOCKET_CONTEXT[i].workTxQueue, &item, 0);
                if (!item || (status != AJ_OK) || !item->list) {
                    break;
                } else {
                    //AJ_AlwaysPrintf(("AJ_WSL_MBoxListenAndProcessTask: %x\n", item->itemType));
                    AJ_WSL_WriteBufListToMBox(0, item->endpoint, AJ_BufListLengthOnWire(item->list), item->list);

                    /*
                     * if this item is sending data, create a workitem indicating completion
                     * this special check is needed because there is no socket send command in WMI,
                     * otherwise we could handle it in AJ_WSL_WMI_ProcessWMIEvent
                     */
                    if (item->itemType == AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_NET_DATA_TX)) {
                        wsl_work_item** ppWork;
                        wsl_work_item* sockWork;
                        sockWork = (wsl_work_item*)AJ_WSL_Malloc(sizeof(wsl_work_item));
                        memset(sockWork, 0, sizeof(wsl_work_item));
                        sockWork->itemType = item->itemType;
                        sockWork->endpoint = item->endpoint;
                        ppWork = &sockWork;
                        AJ_QueuePush(AJ_WSL_SOCKET_CONTEXT[i].workRxQueue, ppWork, AJ_TIMER_FOREVER);
                    }
                    AJ_WSL_WMI_FreeWorkItem(item);
                }
            } while (1);
        }
        if (!g_b_spi_interrupt_data_ready) {
            AJ_YieldCurrentTask();
        }
        g_b_spi_interrupt_data_ready = FALSE; // reset the state of the interrupt signal
    }
}


