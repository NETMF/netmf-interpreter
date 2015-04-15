/**
 * @file SPI MBOX implementation
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

#define AJ_MODULE WSL_SPI_MBOX

#include "aj_target.h"
#include "aj_wsl_target.h"
#include "aj_status.h"
#include "aj_wsl_spi.h"
#include "aj_wsl_htc.h"
#include "aj_buf.h"
#include "aj_rtos.h"
#include "aj_bsp.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgWSL_SPI_MBOX = 0;
#endif

extern AJ_WSL_HTC_CONTEXT AJ_WSL_HTC_Global;

uint32_t AJ_WSL_MBOX_BLOCK_SIZE;

void AJ_WSL_SPI_ModuleInit(void)
{
    AJ_BufList_ModuleInit();
}




AJ_Status AJ_WSL_GetDMABufferSize(uint16_t* dmaSize)
{
    AJ_Status status = AJ_ERR_SPI_READ;
    uint16_t size = 0;
    status = AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_DMA_SIZE, (uint8_t*)&size);
    *dmaSize = size & ((1 << 12) - 1);

    return status;
}

AJ_Status AJ_WSL_SetDMABufferSize(uint16_t dmaSize)
{
    AJ_ASSERT(0 != dmaSize);
    dmaSize &= ((1 << 12) - 1); // mask the reserved bits
    return AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_DMA_SIZE, dmaSize);
}


AJ_Status AJ_WSL_GetWriteBufferSpaceAvailable(uint16_t* spaceAvailable)
{
    AJ_Status status = AJ_ERR_SPI_READ;
    uint16_t space = 0;
    status = AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_WRBUF_SPC_AVA, (uint8_t*)&space);
    *spaceAvailable = space & ((1 << 12) - 1);
    AJ_InfoPrintf(("write buffer space available: %x %d\n", space, *spaceAvailable));
    return status;
}

AJ_Status AJ_WSL_GetReadBufferSpaceAvailable(uint16_t* spaceAvailable)
{
    AJ_Status status = AJ_ERR_SPI_READ;
    uint16_t space = 0;
    status = AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_RDBUF_BYTE_AVA, (uint8_t*)&space);
    *spaceAvailable = space & ((1 << 12) - 1);

    return status;
}

static void AJ_WSL_BufListIterate_DMA(AJ_BufList* list)
{
    if (list != NULL) {
        AJ_BufNode* node = list->head;
        while (node != NULL) {
            AJ_BufNode* nextNode = node->next;
            AJ_WSL_SPI_DMATransfer(node->buffer, node->length, 1);
            node = nextNode;
        }
    }
}

AJ_EXPORT AJ_Status AJ_WSL_WriteBufListToMBox(uint8_t box, uint8_t endpoint, uint16_t len, AJ_BufList* list)
{
    AJ_Status status = AJ_ERR_SPI_WRITE;
    uint16_t spaceAvailable = 0;
    uint16_t bytesRemaining;
    uint16_t cause = 0;

    AJ_ASSERT(0 == box);
//    AJ_InfoPrintf(("=HTC Credits 0:%x, 1:%x, 2:%x\n", AJ_WSL_HTC_Global.endpoints[0].txCredits, AJ_WSL_HTC_Global.endpoints[1].txCredits, AJ_WSL_HTC_Global.endpoints[2].txCredits));
    AJ_Time credit_timer;
    AJ_InitTimer(&credit_timer);
    while (AJ_WSL_HTC_Global.endpoints[endpoint].txCredits < 1) {
        // do nothing and wait until there are credits
        if (AJ_GetElapsedTime(&credit_timer, TRUE) > 1500) {
            AJ_WSL_HTC_Global.endpoints[endpoint].txCredits++;
            break;
        }
        AJ_YieldCurrentTask();
    }

    // don't let the other tasks interrupt our SPI access
    AJ_EnterCriticalRegion();


    AJ_Time space_timer;
    AJ_InitTimer(&space_timer);
    // read space available in mbox from register
    do {
        if (AJ_GetElapsedTime(&space_timer, TRUE) > 1500) {
            spaceAvailable = 0xc5b;
            AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_SPI_CONFIG, 1 << 15);
            break;
        }
        status = AJ_WSL_GetWriteBufferSpaceAvailable(&spaceAvailable);
    } while (spaceAvailable == 0);

    AJ_ASSERT((status == AJ_OK) && (spaceAvailable >= len));
    if ((status == AJ_OK) && (spaceAvailable >= len)) {
        uint16_t targetAddress;


        // write size to be transferred
        status = AJ_WSL_SetDMABufferSize(len);
        AJ_ASSERT(status == AJ_OK);

        // write the target address (where we want to send data)
        // the write should end up at the end of the MBox alias
        // example 0xFFF - len
        targetAddress = AJ_WSL_SPI_MBOX_0_EOM_ALIAS - len;
        status = AJ_WSL_SPI_DMAWriteStart(targetAddress);
        AJ_ASSERT(status == AJ_OK);

        bytesRemaining = len;

        // Take the AJ_BufList to write out and write it out to the SPI interface via DMA
        AJ_WSL_BufListIterate_DMA(list);

        // clear the packet available interrupt
        cause = 0x1f;
        status = AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_INTR_ENABLE, cause);
        AJ_ASSERT(status == AJ_OK);


        AJ_WSL_HTC_Global.endpoints[endpoint].txCredits -= 1;


    } else {
        status = AJ_ERR_SPI_NO_SPACE;
    }
    AJ_LeaveCriticalRegion();
    return status;
}

void AJ_WSL_SPI_ReadIntoBuffer(uint16_t bytesToRead, uint8_t** buf)
{
    aj_spi_status rc;
    wsl_spi_command send;
    AJ_Status status = AJ_ERR_SPI_READ;
    uint8_t pcs = AJ_WSL_SPI_PCS;
    uint16_t toss;

    // initialize an SPI CMD structure with the register of interest
    send.cmd_rx = AJ_WSL_SPI_READ;
    send.cmd_reg = AJ_WSL_SPI_EXTERNAL;
    send.cmd_addr = AJ_WSL_SPI_MBOX_0_EOM_ALIAS - bytesToRead;

    // write the register
    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *((uint8_t*)&send + 1), AJ_WSL_SPI_PCS, AJ_WSL_SPI_CONTINUE);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, (uint8_t*)&toss, pcs); // toss.
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *(uint8_t*)&send, AJ_WSL_SPI_PCS, AJ_WSL_SPI_END);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, (uint8_t*)&toss, pcs); // toss.
    AJ_ASSERT(rc == SPI_OK);

    AJ_WSL_SPI_DMATransfer((uint8_t*)*buf, bytesToRead, 0);
}



//Mailbox Read Steps:
//1. Interrupt going from the QCA4002 to the SPI host.
//2. INTERNAL read from INTR_CAUSE register.
//3. INTERNAL read from RDBUF_BYTE_AVA register.
//4. Internal read from RDBUF_LOOKAHEAD1 register
//5. Internal read from RDBUF_LOOKAHEAD2 register. From the 4 bytes we have read from RDBUF_LOOKAHEAD registers, get the packet size.
//6. INTERNAL write to DMA_SIZE register with the packet size.
//7. Start DMA read command and start reading the data by de-asserting chip select pin.
//8. The packet available will be cleared by HW at the end of the DMA read.
//
AJ_EXPORT AJ_Status AJ_WSL_ReadFromMBox(uint8_t box, uint16_t* len, uint8_t** buf)
{
    AJ_Status status = AJ_ERR_SPI_READ;
    uint16_t cause = 0;
    uint16_t bytesInBuffer = 0;
    uint16_t bytesToRead = 0;
    uint16_t lookAhead;
    uint16_t payloadLength;

    AJ_ASSERT(0 == box);
    AJ_EnterCriticalRegion();
    //2. INTERNAL read from INTR_CAUSE register.
    do {

        //3. INTERNAL read from RDBUF_BYTE_AVA register.
        status = AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_RDBUF_BYTE_AVA, (uint8_t*)&bytesInBuffer);

        AJ_ASSERT(status == AJ_OK);
        //bytesInBuffer = CPU_TO_BE16(bytesInBuffer);

        // The first few bytes of the packet can now be examined and the right amount of data read from the target
        //4. Internal read from RDBUF_LOOKAHEAD1 register
        //5. Internal read from RDBUF_LOOKAHEAD2 register. From the 4 bytes we have read from RDBUF_LOOKAHEAD registers, get the packet size.
        status = AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_RDBUF_LOOKAHEAD1, (uint8_t*)&lookAhead);
        AJ_ASSERT(status == AJ_OK);
        lookAhead = CPU_TO_BE16(lookAhead);

        status = AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_RDBUF_LOOKAHEAD2, (uint8_t*)&payloadLength);
        AJ_ASSERT(status == AJ_OK);
        payloadLength = CPU_TO_BE16(payloadLength);

        // calculate number of bytes to read from the lookahead info, and round up to the next block size
        bytesToRead = payloadLength + 6; //sizeof(header);
        bytesToRead = ((bytesToRead / AJ_WSL_MBOX_BLOCK_SIZE) + ((bytesToRead % AJ_WSL_MBOX_BLOCK_SIZE) ? 1 : 0)) * AJ_WSL_MBOX_BLOCK_SIZE;
        *buf = (uint8_t*)AJ_WSL_Malloc(bytesToRead);
        *len = bytesToRead;
        //6. INTERNAL write to DMA_SIZE register with the packet size.
        // write size to be transferred
        status = AJ_WSL_SetDMABufferSize(bytesToRead);
        AJ_ASSERT(status == AJ_OK);

        AJ_WSL_SPI_ReadIntoBuffer(bytesToRead, buf);

        // clear the packet available interrupt
        cause = 0x1;
        status = AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_INTR_CAUSE, cause);
        AJ_ASSERT(status == AJ_OK);

        break;

    } while (0);
    AJ_LeaveCriticalRegion();
    return status;
}

AJ_Status AJ_WSL_SPI_RegisterRead(uint16_t reg, uint8_t* spi_data)
{
    aj_spi_status rc;
    wsl_spi_command send;
    AJ_Status status = AJ_ERR_SPI_READ;
    uint8_t pcs = AJ_WSL_SPI_PCS;

    // initialize an SPI CMD structure with the register of interest
    send.cmd_rx = AJ_WSL_SPI_READ;
    send.cmd_reg = AJ_WSL_SPI_INTERNAL;
    send.cmd_addr = reg;
    /* Test write: should return OK. */
    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *((uint8_t*)&send + 1), AJ_WSL_SPI_PCS, AJ_WSL_SPI_CONTINUE);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, spi_data, pcs); // toss.
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *(uint8_t*)&send, AJ_WSL_SPI_PCS, AJ_WSL_SPI_END);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, spi_data, pcs); // toss.
    AJ_ASSERT(rc == SPI_OK);

    // read the first byte of response
    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, 0 /*xFF*/, AJ_WSL_SPI_PCS, AJ_WSL_SPI_CONTINUE); // junk to write while reading
    AJ_ASSERT(rc == SPI_OK);
    if (rc == SPI_OK) {
        /* Test read: should return OK with what is sent. */
        rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, spi_data, pcs);
        AJ_ASSERT(rc == SPI_OK);
        status = AJ_OK;
    }

    // read the second byte
    spi_data++;
    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, 0 /*xFF*/, AJ_WSL_SPI_PCS, AJ_WSL_SPI_CONTINUE); // junk to write while reading
    AJ_ASSERT(rc == SPI_OK);
    if (rc == SPI_OK) {
        /* Test read: should return OK with what is sent. */
        rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, spi_data, pcs);
        AJ_ASSERT(rc == SPI_OK);
        status = AJ_OK;
    }
    spi_data--; // move back to the original location
    *(uint16_t*)spi_data = CPU_TO_BE16(*(uint16_t*)spi_data);
    return status;
}

AJ_Status AJ_WSL_SPI_RegisterWrite(uint16_t reg, uint16_t spi_data)
{
    aj_spi_status rc;
    wsl_spi_command send;
    AJ_Status status = AJ_ERR_SPI_WRITE;
    uint8_t pcs = AJ_WSL_SPI_PCS;
    uint8_t toss;
    uint8_t* bytePoint = (uint8_t*)&spi_data;
    // initialize an SPI CMD structure with the register of interest
    send.cmd_rx = AJ_WSL_SPI_WRITE;
    send.cmd_reg = AJ_WSL_SPI_INTERNAL;
    send.cmd_addr = reg;

    // write the register, one byte at a time, in the right order

    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *((uint8_t*)&send + 1), AJ_WSL_SPI_PCS, AJ_WSL_SPI_CONTINUE);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs); // toss.
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *(uint8_t*)&send, AJ_WSL_SPI_PCS, AJ_WSL_SPI_END);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs); // toss.
    AJ_ASSERT(rc == SPI_OK);

    if (rc == SPI_OK) {
        rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *(bytePoint + 1), AJ_WSL_SPI_PCS, AJ_WSL_SPI_END);
        AJ_ASSERT(rc == SPI_OK);
        rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs);
        AJ_ASSERT(rc == SPI_OK);
        if (rc == SPI_OK) {
            status = AJ_OK;
        }
    }
    if (rc == SPI_OK) {
        rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *(bytePoint) & 0xFF, AJ_WSL_SPI_PCS, AJ_WSL_SPI_END);
        AJ_ASSERT(rc == SPI_OK);
        rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs);
        AJ_ASSERT(rc == SPI_OK);
        if (rc == SPI_OK) {
            status = AJ_OK;
        }
    }
    return status;
}

// Set up a transfer: send a write command with an address
AJ_Status AJ_WSL_SPI_DMAWriteStart(uint16_t targetAddress)
{
    aj_spi_status rc;
    wsl_spi_command send;
    AJ_Status status = AJ_ERR_SPI_WRITE;
    uint8_t toss;
    uint8_t pcs = AJ_WSL_SPI_PCS;

    // initialize an SPI CMD structure with the register of interest
    send.cmd_rx = AJ_WSL_SPI_WRITE;
    send.cmd_reg = AJ_WSL_SPI_EXTERNAL;
    send.cmd_addr = targetAddress;

    // write the register
    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *((uint8_t*)&send + 1), AJ_WSL_SPI_PCS, AJ_WSL_SPI_CONTINUE);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs); // toss.
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *(uint8_t*)&send, AJ_WSL_SPI_PCS, AJ_WSL_SPI_END);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs); // toss.
    AJ_ASSERT(rc == SPI_OK);
    if (rc == SPI_OK) {
        status = AJ_OK;
    }

    return status;
}

// Set up a transfer: send a write command with an address
AJ_Status AJ_WSL_SPI_DMAWrite16(uint16_t targetAddress, uint16_t len, uint16_t* spi_data)
{
    aj_spi_status rc;
    wsl_spi_command send;
    AJ_Status status = AJ_ERR_SPI_WRITE;
    uint8_t toss;
    uint8_t pcs = AJ_WSL_SPI_PCS;
    uint8_t* bytePoint = (uint8_t*)spi_data;

    // initialize an SPI CMD structure with the register of interest
    send.cmd_rx = AJ_WSL_SPI_WRITE;
    send.cmd_reg = AJ_WSL_SPI_EXTERNAL;
    send.cmd_addr = targetAddress;

    // write the register
    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *((uint8_t*)&send + 1), AJ_WSL_SPI_PCS, AJ_WSL_SPI_CONTINUE);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs); // toss.
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *(uint8_t*)&send, AJ_WSL_SPI_PCS, AJ_WSL_SPI_END);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs); // toss.
    AJ_ASSERT(rc == SPI_OK);
    if (rc == SPI_OK) {
        while ((rc == SPI_OK) && (len > 1)) {
            /* Test read: should return OK with what is sent. */
            rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *bytePoint, AJ_WSL_SPI_PCS, AJ_WSL_SPI_CONTINUE);
            AJ_ASSERT(rc == SPI_OK);
            rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs);
            AJ_ASSERT(rc == SPI_OK);
            bytePoint++;
            len = len - 1;
        }
        if (rc == SPI_OK) {
            rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *bytePoint, AJ_WSL_SPI_PCS, AJ_WSL_SPI_END);
            AJ_ASSERT(rc == SPI_OK);
            rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs);
            AJ_ASSERT(rc == SPI_OK);
        }
        if (rc == SPI_OK) {
            status = AJ_OK;
        }
    }

    return status;
}

// Set up a transfer: send a write command with an address
AJ_Status AJ_WSL_SPI_DMARead16(uint16_t targetAddress, uint16_t len, uint16_t* spi_data)
{
    aj_spi_status rc;
    wsl_spi_command send;
    AJ_Status status = AJ_ERR_SPI_READ;
    uint8_t pcs = AJ_WSL_SPI_PCS;
    uint8_t toss;
    uint8_t* bytePoint = (uint8_t*)spi_data;

    // initialize an SPI CMD structure with the register of interest
    send.cmd_rx = AJ_WSL_SPI_READ;
    send.cmd_reg = AJ_WSL_SPI_EXTERNAL;
    send.cmd_addr = targetAddress;

    // write the register
    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *((uint8_t*)&send + 1), AJ_WSL_SPI_PCS, AJ_WSL_SPI_CONTINUE);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs); // toss.
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *(uint8_t*)&send, AJ_WSL_SPI_PCS, AJ_WSL_SPI_END);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs); // toss.
    AJ_ASSERT(rc == SPI_OK);


    while ((rc == SPI_OK) && (len > 1)) {
        /* Test read: should return OK with what is sent. */
        rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, 0 /*xFF*/, AJ_WSL_SPI_PCS, AJ_WSL_SPI_CONTINUE);
        AJ_ASSERT(rc == SPI_OK);
        rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, bytePoint, pcs);
        AJ_ASSERT(rc == SPI_OK);
        bytePoint++;
        len = len - 1;
    }
    if (rc == SPI_OK) {
        rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, 0 /*xFF*/, AJ_WSL_SPI_PCS, AJ_WSL_SPI_END);
        AJ_ASSERT(rc == SPI_OK);
        rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, bytePoint, pcs);
        AJ_ASSERT(rc == SPI_OK);
    }
    if (rc == SPI_OK) {
        status = AJ_OK;
    }

    return status;
}

/*
 * @remarks SPI mode is not being changed here.
 */
AJ_Status AJ_WSL_SPI_WriteByte8(uint8_t spi_data, uint8_t end)
{
    aj_spi_status rc;
    AJ_Status status = AJ_ERR_SPI_WRITE;
    uint8_t pcs = AJ_WSL_SPI_PCS;
    uint8_t toss;

    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, (uint16_t)spi_data, AJ_WSL_SPI_PCS, end);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs);
    AJ_ASSERT(rc == SPI_OK);
    if (rc == SPI_OK) {
        status = AJ_OK;
    }
    return status;
}

/*
 * @remarks SPI mode is not being changed here.
 */
AJ_Status AJ_WSL_SPI_WriteByte16(uint16_t spi_data, uint8_t end)
{
    aj_spi_status rc;
    AJ_Status status = AJ_ERR_SPI_WRITE;
    uint8_t pcs = AJ_WSL_SPI_PCS;
    uint8_t toss;

    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, spi_data, AJ_WSL_SPI_PCS, end);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs);
    AJ_ASSERT(rc == SPI_OK);

    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, spi_data & 0xFF, AJ_WSL_SPI_PCS, AJ_WSL_SPI_CONTINUE);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs); // toss.
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, spi_data >> 8, AJ_WSL_SPI_PCS, AJ_WSL_SPI_END);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs); // toss.
    AJ_ASSERT(rc == SPI_OK);

    if (rc == SPI_OK) {
        status = AJ_OK;
    }
    return status;
}

AJ_Status AJ_WSL_SPI_HostControlRegisterWrite(uint32_t targetRegister, uint8_t increment, uint16_t cbLen, uint8_t* spi_data)
{
    aj_spi_status rc;
    wsl_spi_command send;
    AJ_Status status = AJ_ERR_SPI_WRITE;
    uint8_t toss;
    uint8_t pcs = AJ_WSL_SPI_PCS;

    // write the size
    AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_HOST_CTRL_BYTE_SIZE, cbLen | (increment ? 0 : 0x40));

    // initialize an SPI CMD structure with the register of interest
    send.cmd_rx = AJ_WSL_SPI_WRITE;
    send.cmd_reg = AJ_WSL_SPI_INTERNAL;
    send.cmd_addr = AJ_WSL_SPI_REG_HOST_CTRL_WR_PORT;

    // write the register, one byte at a time, in the right order
    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *((uint8_t*)&send + 1), AJ_WSL_SPI_PCS, AJ_WSL_SPI_CONTINUE);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs); // toss.
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *(uint8_t*)&send, AJ_WSL_SPI_PCS, AJ_WSL_SPI_END);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs); // toss.
    AJ_ASSERT(rc == SPI_OK);


    if (rc == SPI_OK) {
        while ((rc == SPI_OK) && (cbLen > 1)) {
            rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *spi_data, AJ_WSL_SPI_PCS, AJ_WSL_SPI_CONTINUE);
            AJ_ASSERT(rc == SPI_OK);
            rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs);
            AJ_ASSERT(rc == SPI_OK);
            spi_data++;
            cbLen = cbLen - 1;
        }

        if (rc == SPI_OK) {
            rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *spi_data, AJ_WSL_SPI_PCS, AJ_WSL_SPI_END);
            AJ_ASSERT(rc == SPI_OK);
            rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, &toss, pcs);
            AJ_ASSERT(rc == SPI_OK);
        }

        if (rc == SPI_OK) {
            status = AJ_OK;
        }
    }

    // now send the host_control_config register update
    {
        uint16_t externalRegister = 0;
        externalRegister = (1 << 15) | (1 << 14) | (targetRegister);  // external access, write, counter dec
        externalRegister = CPU_TO_LE16(externalRegister);

        AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_HOST_CTRL_CONFIG, externalRegister);

    }

    // clear the rd/wr buffer interrupt
    {
        uint16_t spi_16 = 0x300;
        spi_16 = CPU_TO_LE16(spi_16);
        AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_INTR_CAUSE, spi_16);
    }

    return status;
}

AJ_Status AJ_WSL_SPI_HostControlRegisterRead(uint32_t targetRegister, uint8_t increment, uint16_t cbLen, uint8_t* spi_data)
{
    aj_spi_status rc;
    wsl_spi_command send;
    AJ_Status status = AJ_ERR_SPI_WRITE;
    uint8_t pcs = AJ_WSL_SPI_PCS;

    // write the size
    AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_HOST_CTRL_BYTE_SIZE, cbLen | (increment ? 0 : 0x40));

    // now send the host_control_config register update
    {
        uint16_t externalRegister = 0;
        externalRegister = (1 << 15) | (0 << 14) | (targetRegister);  // external access, write, counter dec
        externalRegister = CPU_TO_LE16(externalRegister);

        AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_HOST_CTRL_CONFIG, externalRegister);

    }



    // get the spi status
    {
        uint16_t spi_16 = 0;
        AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_SPI_STATUS, (uint8_t*)&spi_16);
        spi_16 = LE16_TO_CPU(spi_16);
    }


    // initialize an SPI CMD structure with the register of interest
    send.cmd_rx = AJ_WSL_SPI_READ;
    send.cmd_reg = AJ_WSL_SPI_INTERNAL;
    send.cmd_addr = AJ_WSL_SPI_REG_HOST_CTRL_RD_PORT;

    // write the register, one byte at a time, in the right order
    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *((uint8_t*)&send + 1), AJ_WSL_SPI_PCS, AJ_WSL_SPI_CONTINUE);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, spi_data, pcs); // toss.
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, *(uint8_t*)&send, AJ_WSL_SPI_PCS, AJ_WSL_SPI_END);
    AJ_ASSERT(rc == SPI_OK);
    rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, spi_data, pcs); // toss.
    AJ_ASSERT(rc == SPI_OK);


    // now, read the data back
    if (rc == SPI_OK) {
        while ((rc == SPI_OK) && (cbLen > 1)) {
            /* Test read: should return OK with what is sent. */
            rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, 0, AJ_WSL_SPI_PCS, AJ_WSL_SPI_CONTINUE);
            AJ_ASSERT(rc == SPI_OK);
            rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, spi_data, pcs);
            AJ_ASSERT(rc == SPI_OK);
            spi_data++;
            cbLen = cbLen - 1;
        }
        if (rc == SPI_OK) {
            rc = AJ_SPI_WRITE(AJ_WSL_SPI_DEVICE, 0, AJ_WSL_SPI_PCS, AJ_WSL_SPI_END);
            AJ_ASSERT(rc == SPI_OK);
            rc = AJ_SPI_READ(AJ_WSL_SPI_DEVICE, spi_data, pcs);
            AJ_ASSERT(rc == SPI_OK);
        }
        if (rc == SPI_OK) {
            status = AJ_OK;
        }
    }

    // clear the rd/wr buffer interrupt
    {
        uint16_t spi_16 = 0x300;
        spi_16 = CPU_TO_LE16(spi_16);
        AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_INTR_CAUSE, spi_16);
    }



    return status;
}

