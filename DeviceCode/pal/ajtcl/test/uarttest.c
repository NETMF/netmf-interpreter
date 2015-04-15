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
#define AJ_SERIAL_WINDOW_SIZE   4
#define AJ_SERIAL_ENABLE_CRC    1
#define LOCAL_DATA_PACKET_SIZE  100
#define AJ_SERIAL_PACKET_SIZE  LOCAL_DATA_PACKET_SIZE

static uint8_t txBuffer[1600];
static uint8_t rxBuffer[1600];


void TimerCallbackEndProc(uint32_t timerId, void* context)
{
    AJ_AlwaysPrintf(("TimerCallbackEndProc %.6d \n", timerId));
#ifdef READTEST
    if (0 == memcmp(txBuffer, rxBuffer, sizeof(rxBuffer))) {
        AJ_AlwaysPrintf(("Passed: buffers match.\n"));
    } else {
        AJ_AlwaysPrintf(("FAILED: buffers mismatch.\n"));
        exit(-1);
    }
#endif
    exit(0);
}


int AJ_Main()
{
    AJ_Status status;
    memset(&txBuffer, 'T', sizeof(txBuffer));
    memset(&rxBuffer, 'R', sizeof(rxBuffer));

    int blocks;
    int blocksize = LOCAL_DATA_PACKET_SIZE;
    for (blocks = 0; blocks < 16; blocks++) {
        memset(txBuffer + (blocks * blocksize), 0x41 + (uint8_t)blocks, blocksize);
    }

#ifdef READTEST
    status = AJ_SerialInit("/dev/ttyUSB0", BITRATE, AJ_SERIAL_WINDOW_SIZE, AJ_SERIAL_PACKET_SIZE);
#else
    status = AJ_SerialInit("/dev/ttyUSB1", BITRATE, AJ_SERIAL_WINDOW_SIZE, AJ_SERIAL_PACKET_SIZE);
#endif

    AJ_AlwaysPrintf(("serial init was %u\n", status));




#ifdef READTEST
    AJ_Sleep(2000); // wait for the writing side to be running, this should test the queuing of data.
    // try to read everything at once
    int i = 0;

    //for ( ; i < 10000; ++i) {
    while (1) {
        AJ_SerialRecv(rxBuffer, sizeof(rxBuffer), 50000, NULL);
    }


/*
    //Read small chunks of a packet at one time.
    for (blocks = 0 ; blocks < 16*4; blocks++) {
        AJ_SerialRecv(rxBuffer+(blocks*blocksize/4), blocksize/4, 2000, NULL);
   //        AJ_Sleep(200);
    }
 */
    AJ_DumpBytes("Post serial recv", rxBuffer, sizeof(rxBuffer));
    AJ_Sleep(500);
#else
    AJ_Sleep(5000);
    int i = 0;


    while (1) {
        AJ_SerialSend(txBuffer, sizeof(txBuffer));
        ++i;
        if (i % 500 == 0) {
            AJ_AlwaysPrintf(("Hit iteration %d\n", i));
        }
    }

    AJ_AlwaysPrintf(("post serial send\n"));
#endif


    while (1) {
        AJ_StateMachine();
    }


    return(0);
}

#ifdef AJ_MAIN
int main()
{
    return AJ_Main();
}
#endif
