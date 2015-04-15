/**
 * @file  UART transport Tester
 */
/******************************************************************************
 * Copyright (c) 2013-1014, AllSeen Alliance. All rights reserved.
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
#define RANDOM_BYTES_MAX 5000

#ifdef ECHO
static uint8_t txBuffer[RANDOM_BYTES_MAX];
#endif
static uint8_t rxBuffer[RANDOM_BYTES_MAX];



int AJ_Main()
{
    AJ_Status status;

    status = AJ_SerialInit("/dev/ttyUSB1", BITRATE, AJ_SERIAL_WINDOW_SIZE, AJ_SERIAL_PACKET_SIZE);
    AJ_AlwaysPrintf(("serial init was %u\n", status));
    uint16_t txlen;
    uint16_t rxlen;
    int i = 0;

#ifdef ECHO
    while (1) {
        AJ_AlwaysPrintf(("Iteration %d\n", i++));
        status = AJ_SerialRecv(rxBuffer, RANDOM_BYTES_MAX, 5000, &rxlen);
        if (status == AJ_ERR_TIMEOUT) {
            continue;
        }
        if (status != AJ_OK) {
            AJ_AlwaysPrintf(("AJ_SerialRecv returned %d\n", status));
            exit(1);
        }
        AJ_Sleep(rand() % 5000);

        status = AJ_SerialSend(rxBuffer, rxlen);
        if (status != AJ_OK) {
            AJ_AlwaysPrintf(("AJ_SerialSend returned %d\n", status));
            exit(1);
        }

        AJ_Sleep(rand() % 5000);
    }
#else
    txlen = 0;
    while (1) {
        AJ_AlwaysPrintf(("Iteration %d\n", i++));
        txlen = rand() % 5000;
        for (int i = 0; i < txlen; i++) {
            txBuffer[i] = rand() % 256;
            rxBuffer[i] = 1;
        }
        status = AJ_SerialSend(txBuffer, txlen);
        if (status != AJ_OK) {
            AJ_AlwaysPrintf(("AJ_SerialSend returned %d\n", status));
            exit(1);
        }
        AJ_Sleep(rand() % 5000);
        status = AJ_SerialRecv(rxBuffer, txlen, 50000, &rxlen);
        if (status != AJ_OK) {
            AJ_AlwaysPrintf(("AJ_SerialRecv returned %d\n", status));
            exit(1);
        }
        if (rxlen != txlen) {
            AJ_AlwaysPrintf(("Failed: length match rxlen=%d txlen=%d.\n", rxlen, txlen));
            exit(-1);
        }
        if (0 != memcmp(txBuffer, rxBuffer, rxlen)) {
            AJ_AlwaysPrintf(("Failed: buffers match.\n"));
            exit(-1);
        }

    }
#endif
    return(0);
}

#ifdef AJ_MAIN
int main()
{
    return AJ_Main();
}
#endif
