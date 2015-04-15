/*
 * @file
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

#include <aj_crypto.h>
#include <aj_debug.h>
#include <alljoyn.h>

static const uint16_t SIG_LEN_MAX = 65000;
static const uint16_t SIG_LEN_MIN = 100;

/*
 * Generate a random signal length between SIG_LEN_MIN and SIG_LEN_MAX,
 * if RANDOM_SIGNAL_LENGTH is TRUE. If RANDOM_SIGNAL_LENGTH is FALSE,
 * then fix the length of signal as average of SIG_LEN_MIN and SIG_LEN_MAX.
 */
static const uint8_t RANDOM_SIGNAL_LENGTH = TRUE;

static const char DaemonName[] = "org.alljoyn.BusNode";

static const char ServiceName[] = "org.alljoyn.LargeSignals";
static const uint16_t ServicePort = 42;

static uint8_t g_prop_ok_to_send = FALSE;

static AJ_BusAttachment g_bus;
static uint32_t g_sessionId = 0;

static const char* const testInterface[] = {
    "org.alljoyn.LargeSignals",
    "!large_signal >u >ay",
    "@ok_to_send=b",
    NULL
};

static const AJ_InterfaceDescription testInterfaces[] = {
    testInterface,
    AJ_PropertiesIface,
    NULL
};

/**
 * Objects implemented by the application
 */
static const AJ_Object ProxyObjects[] = {
    { "/org/alljoyn/LargeSignals", testInterfaces },
    { NULL }
};

#define APP_MY_SIGNAL      AJ_APP_MESSAGE_ID(0, 0, 0)
#define APP_GET_PROP       AJ_APP_MESSAGE_ID(0, 1, AJ_PROP_GET)
#define APP_SET_PROP       AJ_APP_MESSAGE_ID(0, 1, AJ_PROP_SET)
#define APP_BOOL_VAL_PROP  AJ_APP_PROPERTY_ID(0, 0, 1)

static const uint32_t CONNECT_TIMEOUT = 1000 * 200;
static const uint32_t UNMARSHAL_TIMEOUT = 100 * 5;

#define ARRAY_SIZE 10000
static const uint8_t byte_array[ARRAY_SIZE];
static uint8_t g_error = 0;


/* set property handler */
AJ_Status AppHandleSetProp(AJ_Message* replyMsg, uint32_t propId, void* context)
{
    AJ_Status status = AJ_ERR_UNEXPECTED;
    uint32_t prop_val = 0;
    status = AJ_UnmarshalArgs(replyMsg, "b", &prop_val);
    AJ_Printf("\tDEBUG: Received the property %s\n", (0 == prop_val) ? "FALSE" : "TRUE");
    g_prop_ok_to_send = prop_val;
    return status;
}

void HandleSignal(AJ_Message*msg) {
    uint32_t number = 0;
    uint32_t length = 0;
    uint32_t bytesUnmarshalled;
    size_t sz;
    void* raw = NULL;

    uint8_t firstByte = 0;
    uint8_t lastByte = 0;

    AJ_Status status = AJ_UnmarshalArgs(msg, "u", &number);
    if (AJ_OK == status) {
        status = AJ_UnmarshalRaw(msg, (const void**)&raw, sizeof(bytesUnmarshalled), &sz);
        if (AJ_OK != status) {
            AJ_Printf("ERROR: UnmarshalRaw 1st call status is %s \n", AJ_StatusText(status));
            g_error = 1;
        } else {
            length = *((uint32_t*)raw);
        }
    }

    if (AJ_OK == status) {
        uint32_t bytesToBeFetched = length;
        uint32_t count = 0;
        while (0 != bytesToBeFetched) {
            raw = NULL;
            status = AJ_UnmarshalRaw(msg, (const void**)&raw, bytesToBeFetched, &sz);
            if (AJ_OK != status) {
                AJ_Printf("UnmarshalRaw status is %s \n", AJ_StatusText(status));
                AJ_Printf("Bytes to be fetched: %u, actual bytes fetched: %u \n", bytesToBeFetched, (uint32_t)sz);
            } else {
                /*
                 * Read the first byte. We need the first byte and the
                 * last byte of the array.
                 */
                if (count == 0) {
                    firstByte = *((uint8_t*) raw);
                }
                bytesToBeFetched -= sz;
                count++;
            }
        }

        /* Read the last byte here. */
        lastByte = *((uint8_t*) raw + sz - 1);
        if (firstByte != lastByte) {
            AJ_Printf("ERROR: Integrity check failed. first element != last element. \n");
            g_error = 1;
        }

        AJ_Printf("============> Received my_signal # %u, length %u, firstByte %u, lastByte %u\n", number, length, firstByte, lastByte);
    }
}

static AJ_Status SendSignal(AJ_BusAttachment* bus, uint32_t sessionId, char* fullServiceName)
{
    AJ_Status status = AJ_OK;
    AJ_Message msg;

    static uint32_t current_signal_number = 0;

    uint32_t array_size = 0;
    uint16_t random = 0;
    uint32_t i = 0;
    int remaining = 0;
    uint8_t data = 0;

    status = AJ_MarshalSignal(bus, &msg, APP_MY_SIGNAL, fullServiceName, sessionId, 0, 0);
    if (AJ_OK == status) {
        status = AJ_MarshalArgs(&msg, "u", current_signal_number);

        if (RANDOM_SIGNAL_LENGTH) {
            AJ_RandBytes((uint8_t*)&random, sizeof(random));
            if (random == 0) {
                random = 1;
            }
            array_size = SIG_LEN_MIN + random % (SIG_LEN_MAX - SIG_LEN_MIN);
        } else {
            array_size = (SIG_LEN_MIN + SIG_LEN_MAX) / 2;
        }

        AJ_Printf("\tDEBUG: Size of array (in signal payload) is %u\n", array_size);

        /*
         * Raw Marshaling of an array involves:
         * a. Marshaling of length of array (uint32_t)
         * b. Marshaling of the array itself
         */
        status = AJ_DeliverMsgPartial(&msg, sizeof(uint32_t) + array_size);
        if (AJ_OK != status) {
            AJ_Printf("ERROR: DeliverMsgPartial returned: %s.\n", AJ_StatusText(status));
            return status;
        }

        status = AJ_MarshalRaw(&msg, &array_size, sizeof(uint32_t));
        if (AJ_OK != status) {
            AJ_Printf("ERROR: AJ_MarshalRaw of array_size returned %s.\n", AJ_StatusText(status));
            return status;
        }

        /* Marshal raw the data. it will keep sending the data partially. */

        AJ_RandBytes(&data, sizeof(data));

        /* Marshal the first data byte separately. */
        status = AJ_MarshalRaw(&msg, &data, 1);
        if (AJ_OK != status) {
            AJ_Printf("ERROR: Marshaling of first byte returned %s.\n", AJ_StatusText(status));
            return status;
        }

        if (ARRAY_SIZE < (array_size - 2)) {
            for (i = 0; i < (array_size - 2) / ARRAY_SIZE; i++) {

                status = AJ_MarshalRaw(&msg, byte_array, ARRAY_SIZE);
                if (AJ_OK != status) {
                    AJ_Printf("ERROR: 1 Marshaling element (index: %d) failed. Got status: %s.\n", i, AJ_StatusText(status));
                    break;
                }
            }
        }

        if (0 != (array_size - 2) % ARRAY_SIZE) {
            remaining = (array_size - 2) % ARRAY_SIZE;
            status = AJ_MarshalRaw(&msg, byte_array, remaining);
            if (AJ_OK != status) {
                AJ_Printf("ERROR: 2 Marshaling element (index: %d) failed. Got status: %s.\n", i, AJ_StatusText(status));
            }
        }

        /* fill the last byte with data */
        status = AJ_MarshalRaw(&msg, &data, 1);
        if (AJ_OK != status) {
            AJ_Printf("ERROR: Marshaling last element returned %s.\n", AJ_StatusText(status));
            return status;
        }
    }

    if (AJ_OK == status) {
        status = AJ_DeliverMsg(&msg);
        if (AJ_OK == status) {
            AJ_Printf("Signal sent successfully.\n");
        }
        current_signal_number++;
    }
    return status;
}

static void AppDoWork(char* fullServiceName)
{
    if (g_prop_ok_to_send) {
        if (AJ_OK != SendSignal(&g_bus, g_sessionId, fullServiceName)) {
            AJ_Printf("ERROR: Sending signal failed.\n");
        } else {
            g_prop_ok_to_send = FALSE;
        }
    } else {
        AJ_Printf("Nothing to do at the moment. \n");
    }
}

void AJ_Main(void)
{
    AJ_Status status = AJ_OK;
    uint8_t connected = FALSE;
    /*
     * Buffer to hold the full service name. The buffer size must be AJ_MAX_SERVICE_NAME_SIZE.
     */
    char fullServiceName[AJ_MAX_SERVICE_NAME_SIZE];

    AJ_Initialize();

    AJ_PrintXML(ProxyObjects);
    AJ_RegisterObjects(ProxyObjects, NULL);

    while (TRUE) {
        AJ_Message msg;

        if (!connected) {
            status = AJ_StartClientByName(&g_bus, DaemonName, CONNECT_TIMEOUT, 0, ServiceName, ServicePort, &g_sessionId, NULL, fullServiceName);
            if (AJ_OK == status) {
                AJ_Printf("StartClient returned %s, sessionId=%u\n", AJ_StatusText(status), g_sessionId);
                AJ_Printf("Connected to Daemon: %s\n", AJ_GetUniqueName(&g_bus));
                connected = TRUE;
            } else {
                AJ_Printf("StartClient returned %s\n", AJ_StatusText(status));
                break;
            }
        }

        status = AJ_UnmarshalMsg(&g_bus, &msg, UNMARSHAL_TIMEOUT);
        if ((AJ_ERR_TIMEOUT != status) && (AJ_OK != status)) {
            AJ_Printf("ERROR: UnmarshalMsg returned %s \n", AJ_StatusText(status));
        }

        if (AJ_ERR_TIMEOUT == status) {
            AppDoWork(fullServiceName);
            continue;
        }

        if (AJ_OK == status) {
            switch (msg.msgId) {
            case APP_MY_SIGNAL:
                HandleSignal(&msg);
                status = AJ_OK;
                break;

            case APP_SET_PROP:
                status = AJ_BusPropSet(&msg, AppHandleSetProp, NULL);
                break;

            case AJ_SIGNAL_SESSION_LOST:
                AJ_Printf("Session Lost  \n");
                status = AJ_ERR_READ;
                break;

            default:
                AJ_Printf("Default: msg id is %u\n", msg.msgId);
                status = AJ_BusHandleBusMessage(&msg);
                break;
            }
        }
        /*
         * Messages must be closed to free resources
         */
        if (status == AJ_OK) {
            status = AJ_CloseMsg(&msg);
            if (status != AJ_OK) {
                AJ_Printf("Close message failed \n");
                AJ_Disconnect(&g_bus);
                connected = FALSE;
                break;
            }
        }

        if ((AJ_ERR_READ == status) || g_error) {
            AJ_Printf("AllJoyn disconnect %s  \n", AJ_StatusText(status));
            AJ_Printf("Disconnected from Daemon:%s\n", AJ_GetUniqueName(&g_bus));
            AJ_Disconnect(&g_bus);
            connected = FALSE;
            break;
        }
    }
    AJ_Printf("sigtest exiting with status: %s\n", AJ_StatusText(status));
}

#ifdef AJ_MAIN
int main(void)
{
    /* We are not expecting AJ_Main to return */
    AJ_Main();

    return 0;
}
#endif
