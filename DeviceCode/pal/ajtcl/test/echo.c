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


#define BITRATE B115200
#define AJ_SERIAL_WINDOW_SIZE   4
#define AJ_SERIAL_ENABLE_CRC    1
#define AJ_SERIAL_PACKET_SIZE  104

static uint8_t txBuffer[32];
static uint8_t rxBuffer[32];


void TimerCallbackEndProc(uint32_t timerId, void* context)
{
    AJ_AlwaysPrintf(("TimerCallback %.6d \n", timerId));
    exit(0);
}


#ifdef AJ_MAIN
int main()
{
    AJ_Status status;
    memset(&rxBuffer, 'R', sizeof(rxBuffer));

#ifdef READTEST
    status = AJ_SerialInit("/dev/ttyUSB0", BITRATE, AJ_SERIAL_WINDOW_SIZE, AJ_SERIAL_ENABLE_CRC, AJ_SERIAL_PACKET_SIZE);
#else
    status = AJ_SerialInit("/dev/ttyUSB1", BITRATE, AJ_SERIAL_WINDOW_SIZE, AJ_SERIAL_ENABLE_CRC, AJ_SERIAL_PACKET_SIZE);
#endif

    AJ_AlwaysPrintf(("serial init was %u\n", status));

    uint32_t timerEndProc = 9999;
    status = AJ_TimerRegister(10000, &TimerCallbackEndProc, NULL, &timerEndProc);
    AJ_AlwaysPrintf(("Added id %u\n", timerEndProc));




    uint16_t echocount = 0;
    while (1) {
        snprintf((char*)&txBuffer, sizeof(txBuffer), "echo t %i", ++echocount);

#ifdef READTEST
        uint16_t recv;
        AJ_SerialRecv(rxBuffer, sizeof(rxBuffer), 2000, &recv);
        AJ_DumpBytes("Post serial recv", rxBuffer, sizeof(rxBuffer));

#else
        AJ_Sleep(500);
        AJ_SerialSend(txBuffer, sizeof(txBuffer));
        AJ_AlwaysPrintf(("post serial send\n"));
#endif

        AJ_Sleep(400);
    }


    return(0);
}
#endif
