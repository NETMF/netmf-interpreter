/**
 * @file
 */
/******************************************************************************
 * Copyright (c) 2012,2014, AllSeen Alliance. All rights reserved.
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
#include "aj_debug.h"

#include "efm32_adc.h"
#include "efm32_cmu.h"
#include "efm32_gpio.h"

#include "dmactrl.h"
#include "efm32_dma.h"

#define ACCEL_SENSOR_PORT     gpioPortA
#define ACCEL_SENSOR_PIN      10
#define ACCEL_SELFTEST_PORT   gpioPortA
#define ACCEL_SELFTEST_PIN    2
#define ACCEL_SLEEP_PORT      gpioPortF
#define ACCEL_SLEEP_PIN       4

extern int ButtonPushed;

static const char ServiceName[] = "org.alljoyn.ajlite";
static const uint16_t ServicePort = 24;

static const char* testInterface[] = {
    "org.alljoyn.ajlite_test",
    "!ADC_Update >i",
    "!Gyro_Update >i >i",
    "!Button_Down >i",
    NULL
};


static const AJ_InterfaceDescription testInterfaces[] = {
    testInterface,
    NULL
};

/**
 * Objects implemented by the application
 */
static const AJ_Object AppObjects[] = {
    { "/org/alljoyn/ajlite_test", testInterfaces },
    { NULL }
};

#define APP_ADC_UPDATE_SIGNAL        AJ_APP_MESSAGE_ID(0, 0, 0)
#define APP_GYROSCOPE_UPDATE_SIGNAL  AJ_APP_MESSAGE_ID(0, 0, 1)
#define APP_BUTTON_DOWN_SIGNAL       AJ_APP_MESSAGE_ID(0, 0, 2)

void sendClickSignal(AJ_BusAttachment* bus, int pushed);

/*
 * Let the application do some work
 */
static void AppDoWork(AJ_BusAttachment* bus)
{
    static int oldX = -1;
    static int oldY = -1;
    static int initialized = FALSE;
    int x, y;

    ADC_Init_TypeDef init = ADC_INIT_DEFAULT;
    ADC_InitSingle_TypeDef singleInit = ADC_INITSINGLE_DEFAULT;

    if (initialized == FALSE) {
        /*Turn on accelerometer*/
        GPIO_PinModeSet(ACCEL_SENSOR_PORT, ACCEL_SENSOR_PIN, gpioModePushPull, 1);
        /*Disable sleep*/
        GPIO_PinModeSet(ACCEL_SLEEP_PORT, ACCEL_SLEEP_PIN, gpioModePushPull, 1);
        /*Disable self test*/
        GPIO_PinModeSet(ACCEL_SELFTEST_PORT, ACCEL_SELFTEST_PIN, gpioModePushPull, 0);
    }

    CMU_ClockEnable(cmuClock_HFPER, true);
    CMU_ClockEnable(cmuClock_ADC0, true);

    init.timebase = ADC_TimebaseCalc(0);
    init.prescale = ADC_PrescaleCalc(4000000, 0);
    ADC_Init(ADC0, &init);


    /* Init for single conversion use. */
    singleInit.reference = adcRefVDD; //adcRef2V5;
    singleInit.resolution = adcRes12Bit;

    singleInit.input = adcSingleInpCh6; /* According to Maui HW design */
    ADC_InitSingle(ADC0, &singleInit);
    ADC_IntClear(ADC0, ADC_IF_SINGLE);
    ADC_Start(ADC0, adcStartSingle);
    /* Wait for completion */
    while (!(ADC_IntGet(ADC0) & ADC_IF_SINGLE)) ;
    x = ADC_DataSingleGet(ADC0);

    /* According to Maui HW design */
    singleInit.input = adcSingleInpCh7;
    ADC_InitSingle(ADC0, &singleInit);
    ADC_IntClear(ADC0, ADC_IF_SINGLE);
    ADC_Start(ADC0, adcStartSingle);
    /* Wait for completion */
    while (!(ADC_IntGet(ADC0) & ADC_IF_SINGLE)) ;
    y = ADC_DataSingleGet(ADC0);

    // send the message
    if (x != oldX || y != oldY) {
        AJ_Message msg;
        oldX = x;
        oldY = y;

        AJ_MarshalSignal(bus, &msg, APP_GYROSCOPE_UPDATE_SIGNAL, NULL, 0, 0, 0);
        AJ_MarshalArgs(&msg, "uu", x, y);
        AJ_DeliverMsg(&msg);
    }

    if (ButtonPushed) {
        sendClickSignal(bus, ButtonPushed);
        ButtonPushed = 0;
    }
}

static const char PWD[] = "ABCDEFGH";

static uint32_t PasswordFunc(uint8_t* buffer, uint32_t bufLen)
{
    memcpy(buffer, PWD, sizeof(PWD));
    return sizeof(PWD) - 1;
}


void sendClickSignal(AJ_BusAttachment* bus, int pushed)
{
    AJ_Message msg;
    AJ_MarshalSignal(bus, &msg, APP_BUTTON_DOWN_SIGNAL, NULL, 0, 0, 0);
    AJ_MarshalArgs(&msg, "u", pushed);
    AJ_DeliverMsg(&msg);
}


#define CONNECT_TIMEOUT    (1000 * 60)
#define UNMARSHAL_TIMEOUT  (10)

int AJ_Main(void)
{
    AJ_Status status = AJ_OK;
    AJ_BusAttachment bus;
    uint8_t connected = FALSE;

    /*
     * One time initialization before calling any other AllJoyn APIs
     */
    AJ_Initialize();

    AJ_PrintXML(AppObjects);
    AJ_RegisterObjects(AppObjects, NULL);


    while (TRUE) {
        AJ_Message msg;

        if (!connected) {
            ButtonPushed = 0;

            status = AJ_FindBusAndConnect(&bus, NULL, CONNECT_TIMEOUT);
            if (status != AJ_OK) {
                AJ_AlwaysPrintf(("AllJoyn failed to connect sleeping for 15 seconds\n"));
                AJ_Sleep(15 * 1000);
                continue;
            }
            AJ_AlwaysPrintf(("AllJoyn connected\n"));
            /*
             * Kick things off by binding a session port
             */
            status = AJ_BusBindSessionPort(&bus, ServicePort, NULL);
            if (status != AJ_OK) {
                AJ_AlwaysPrintf(("Failed to send bind session port message\n"));
                break;
            }
            connected = TRUE;
        }

        status = AJ_UnmarshalMsg(&bus, &msg, UNMARSHAL_TIMEOUT);
        if (status != AJ_OK) {
            if (status == AJ_ERR_TIMEOUT) {
                AppDoWork(&bus);
            }
            continue;
        }

        switch (msg.msgId) {
        case AJ_REPLY_ID(AJ_METHOD_BIND_SESSION_PORT):
            /*
             * TODO check the reply args to tell if the request succeeded
             */
            status = AJ_BusRequestName(&bus, ServiceName, AJ_NAME_REQ_DO_NOT_QUEUE);
            break;

        case AJ_REPLY_ID(AJ_METHOD_REQUEST_NAME):
            /*
             * TODO check the reply args to tell if the request succeeded
             */
            status = AJ_BusAdvertiseName(&bus, ServiceName, AJ_TRANSPORT_ANY, AJ_BUS_START_ADVERTISING);
            break;

        case AJ_REPLY_ID(AJ_METHOD_ADVERTISE_NAME):
            /*
             * TODO check the reply args to tell if the request succeeded
             */
            break;

        case AJ_METHOD_ACCEPT_SESSION:
            status = AJ_BusReplyAcceptSession(&msg, TRUE);
            break;

        case AJ_SIGNAL_SESSION_LOST_WITH_REASON:
            /*
             * Force a disconnect
             */
            {
                uint32_t id, reason;
                AJ_UnmarshalArgs(&msg, "uu", &id, &reason);
                AJ_InfoPrintf(("Session lost. ID = %u, reason = %u", id, reason));
            }
            status = AJ_ERR_SESSION_LOST;
            break;

        default:
            /*
             * Pass to the built-in handlers
             */
            status = AJ_BusHandleBusMessage(&msg);
            break;
        }
        /*
         * Messages must be closed to free resources
         */
        AJ_CloseMsg(&msg);

        if ((status == AJ_ERR_SESSION_LOST) || (status == AJ_ERR_READ) || (status == AJ_ERR_LINK_DEAD)) {
            AJ_AlwaysPrintf(("AllJoyn disconnect\n"));
            AJ_Disconnect(&bus);
            connected = FALSE;
            /*
             * Sleep a little while before trying to reconnect
             */
            AJ_Sleep(10 * 1000);
        }
    }
    AJ_AlwaysPrintf(("svclite EXIT %d\n", status));

    return status;
}
#ifdef AJ_MAIN
int main()
{
    return AJ_Main();
}
#endif
