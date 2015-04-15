/**
 * @file  UART transport Tester
 */
/******************************************************************************
 * Copyright (c) 2013-2014, AllSeen Alliance. All rights reserved.
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
#include <stdio.h>
#include "alljoyn.h"
#include "aj_util.h"
#include "aj_debug.h"
#include "aj_bufio.h"
#include "aj_serial.h"

#define B115200 111520
#define BITRATE B115200

static uint8_t* txBuffer;
static uint8_t* rxBuffer;

/**
 *  Busy-wait if there is a crash, then we can check the stack with a debugger
 */
void HardFault_Handler(void)
{
    assert(0);
    while (1) ;
}


#ifdef AJ_DEBUG_SERIAL_TARGET
#define AJ_DebugDumpSerialRX(a, b, c) AJ_DumpBytes(a, b, c)
#define AJ_DebugDumpSerialTX(a, b, c) AJ_DumpBytes(a, b, c)
#else
#define AJ_DebugDumpSerialRX(a, b, c)
#define AJ_DebugDumpSerialTX(a, b, c)
#endif

/**
 *  byte-aligned structures that mimic the wire-protocol
 */

#pragma pack(1)
typedef struct __packetStart {
    uint8_t boundary;
    uint8_t ack : 4;
    uint8_t seq : 4;
    uint8_t type : 4;
    uint8_t : 4;
    uint8_t len[2];
} PacketStart;

typedef struct __packetHeader {
    PacketStart start;
    uint8_t tag[4];
} PacketHeader;

typedef struct __packetNegotiate {
    PacketHeader header;
    uint16_t packetSize;
    uint8_t : 4;
    uint8_t protoVersion : 2;
    uint8_t windowSize : 2;
} PacketNegotiate;

typedef struct __packetTail {
    uint8_t CRC[2];
    uint8_t boundaryEnd;
} PacketTail;

#pragma pack()

/**
 * For every byte in the buffer buf,
 * with a P(percent/100) chance, write a random byte
 */
void RandFuzzing(uint8_t* buf, uint32_t len, uint8_t percent)
{
    uint32_t pos = 0;
    uint8_t* p = (uint8_t*)buf;

    while (pos++ < len) {
        uint8_t roll = rand() % 256;
        if (percent > ((100 * roll) / 256)) {
            *p = rand() % 256;
        }
        ++p;
    }
}

/**
 * Given a buffer of fixed length, randomly pick a way to corrupt the data (or not.)
 */
void FuzzBuffer(uint8_t* buf, uint32_t len)
{
    size_t offset;

    uint8_t test = rand() % 32;
    PacketStart* ps = (PacketStart*)buf;
    PacketHeader* ph = (PacketHeader*)buf;

    uint16_t packetLen = (ps->len[0] << 8) | ps->len[1];
    AJ_AlwaysPrintf(("FuzzBuffer: Before case %i Tag:%c%c%c%c Ack:%i Seq:%i Type:%X Len:%i\n"
                     , test, ph->tag[0], ph->tag[1], ph->tag[2], ph->tag[3],
                     ps->ack, ps->seq, ps->type, packetLen));

    switch (test) {
    case 0:
    case 1:
        /*
         * Protect fixed header from fuzzing
         */
        offset = sizeof(PacketStart);
        RandFuzzing(buf + offset, len - offset, 5);
        break;

    case 2:
        /*
         * fuzz the payload
         */
        offset = sizeof(PacketStart);
        RandFuzzing(buf + offset, packetLen, 10);
        break;

    case 3:
        /*
         * change the sequence number
         */
        ps->seq = rand() % 16;
        break;

    case 4:
        /*
         * change the ack number
         */
        ps->ack = rand() % 16;
        break;

    case 5:
        /*
         * change the type field of the packet
         */
        ps->type = rand() % 16;
        break;

    case 6:
        /*
         * change the length field of the packet
         */
        ps->len[0] = rand() % 256;
        ps->len[1] = rand() % 256;
        packetLen = (ps->len[0] << 8) | ps->len[1];
        break;

    case 7:
        /*
         * Fuzz the entire message
         */
        RandFuzzing(buf, len, 1 + (rand() % 10));
        break;

//    case 8: // not ready yet.
//        /*
//         * Protect Negotiate packet header from fuzzing
//         */
//        offset = sizeof(PacketNegotiate);
//        RandFuzzing(buf + offset, len - offset, 5);
//        break;


    default:
        /*
         * don't fuzz anything
         */
        break;
    }
    AJ_AlwaysPrintf(("FuzzBuffer: After  case %i Tag:%c%c%c%c Ack:%i Seq:%i Type:%X Len:%i\n"
                     , test, ph->tag[0], ph->tag[1], ph->tag[2], ph->tag[3],
                     ps->ack, ps->seq, ps->type, packetLen));

    AJ_DebugDumpSerialTX("FuzzBuffer", buf, len);
    __AJ_TX(buf, len);
}

int AJ_Main()
{
    AJ_Status status;

    while (1) {
        int windows = 1 << (rand() % 3); // randomize window width 1,2,4
        int blocksize = 50 + (rand() % 1000); // randomize packet size 50 - 1050
        AJ_AlwaysPrintf(("Windows:%i Blocksize:%i\n", windows, blocksize));
        txBuffer = (uint8_t*) AJ_Malloc(blocksize);
        rxBuffer = (uint8_t*) AJ_Malloc(blocksize);
        memset(txBuffer, 0x41,  blocksize);
        memset(rxBuffer, 'r', blocksize);

#ifdef READTEST
        status = AJ_SerialInit("/dev/ttyUSB0", BITRATE, windows, blocksize);
#else
        status = AJ_SerialInit("/dev/ttyUSB1", BITRATE, windows, blocksize);
#endif

        AJ_AlwaysPrintf(("serial init was %u\n", status));
        if (status != AJ_OK) {
            continue; // init failed perhaps from bad parameters, start the loop again
        }

        // Change the buffer transmission function to one that fuzzes the output.
        AJ_SetTxSerialTransmit(&FuzzBuffer);

#ifdef READTEST
        AJ_Sleep(2000); // wait for the writing side to be running, this should test the queuing of data.
        // try to read everything at once
        int i = 0;

        while (1) {
            AJ_SerialRecv(rxBuffer, blocksize, 50000, NULL);
        }


        AJ_DumpBytes("Post serial recv", rxBuffer, blocksize);
        AJ_Sleep(500);
#else
        AJ_Sleep(5000);
        int i = 0;


        while (1) {
            // change the packet to be sent every time through the loop.
            memset(txBuffer, 0x41 + (i % 26), blocksize);
            memset(rxBuffer, 0x41 + (i % 26), blocksize);
            AJ_SerialSend(txBuffer, blocksize);
            ++i;
            if (i % 20 == 0) {
                AJ_AlwaysPrintf(("Hit iteration %d\n", i));
                break;
            }
            AJ_SerialRecv(rxBuffer, blocksize, 5000, NULL);
        }

        AJ_AlwaysPrintf(("post serial send\n"));
#endif

        // clean up and start again
        AJ_SerialShutdown();
        AJ_Free(txBuffer);
        AJ_Free(rxBuffer);
    }
    return(0);
}

#ifdef AJ_MAIN
int main()
{
    return AJ_Main();
}
#endif
