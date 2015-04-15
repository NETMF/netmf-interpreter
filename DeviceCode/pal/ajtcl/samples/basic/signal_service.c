/**
 * @file
 */
/******************************************************************************
 * Copyright (c) 2012, 2014, AllSeen Alliance. All rights reserved.
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
#define AJ_MODULE SIGNAL_SERVICE

#include <stdio.h>
#include <aj_debug.h>
#include "alljoyn.h"
#include "aj_msg.h"

uint8_t dbgSIGNAL_SERVICE = 0;
/**
 * Statics.
 */
static AJ_BusAttachment busAttachment;
static char propertyName[128] = "Default name";

/**
 * Static constants.
 */
static const size_t propertyNameSize = sizeof(propertyName) / sizeof(propertyName[0]);
static const int ConnectAttempts = 10;
static const char InterfaceName[] = "org.alljoyn.Bus.signal_sample";
static const char ServiceName[] = "org.alljoyn.Bus.signal_sample";
static const char ServicePath[] = "/";
static const uint16_t ServicePort = 25;

/**
 * The interface name followed by the method signatures.
 *
 * See also .\inc\aj_introspect.h
 */
static const char* const sampleInterface[] = {
    InterfaceName,              /* The first entry is the interface name. */
    "!nameChanged newName>s",   /* Signal at index 0 with an output string of the new name. */
    "@name=s",                  /* Read/write property of the name. */
    NULL
};

/**
 * A NULL terminated collection of all interfaces.
 */
static const AJ_InterfaceDescription sampleInterfaces[] = {
    AJ_PropertiesIface,     /* This must be included for any interface that has properties. */
    sampleInterface,
    NULL
};

/**
 * Objects implemented by the application. The first member in the AJ_Object structure is the path.
 * The second is the collection of all interfaces at that path.
 */
static const AJ_Object AppObjects[] = {
    { ServicePath, sampleInterfaces },
    { NULL }
};

/*
 * The value of the arguments are the indices of the
 * object path in AppObjects (above), interface in sampleInterfaces (above), and
 * member indices in the interface.
 * The 'nameChanged' index is 0 because the first entry in sampleInterface is the interface name.
 * This makes the first index (index 0 of the methods) the second string in
 * sampleInterface[].
 *
 * See also .\inc\aj_introspect.h
 */
#define BASIC_SIGNAL_SERVICE_SIGNAL     AJ_APP_MESSAGE_ID(0, 1, 0)
#define BASIC_SIGNAL_SERVICE_GET_NAME   AJ_APP_MESSAGE_ID(0, 0, AJ_PROP_GET)
#define BASIC_SIGNAL_SERVICE_SET_NAME   AJ_APP_MESSAGE_ID(0, 0, AJ_PROP_SET)

/*
 * Property identifiers for the properies this application implements
 * Encode a property id from the object path, interface, and member indices.
 */
#define BASIC_SIGNAL_SERVICE_NAME_ID    AJ_APP_PROPERTY_ID(0, 1, 1)

static AJ_Status SendSignal()
{
    AJ_Message msg;

    AJ_AlwaysPrintf(("Emitting Name Changed Signal. New value for property 'name' is '%s'.\n", propertyName));

    /* For the signal to transmit outside of the current process the session ID must be 0. */
    AJ_MarshalSignal(&busAttachment, &msg, BASIC_SIGNAL_SERVICE_SIGNAL, NULL, 0, AJ_FLAG_GLOBAL_BROADCAST, 0);
    AJ_MarshalArgs(&msg, "s", propertyName);

    return AJ_DeliverMsg(&msg);
}

static AJ_Status GetName(AJ_Message* replyMsg, uint32_t propId, void* context)
{
    AJ_Status status = AJ_ERR_UNEXPECTED;

    if (propId == BASIC_SIGNAL_SERVICE_NAME_ID) {
        status = AJ_MarshalArgs(replyMsg, "s", propertyName);
    }

    return status;
}

static AJ_Status SetName(AJ_Message* replyMsg, uint32_t propId, void* context)
{
    AJ_Status status = AJ_ERR_UNEXPECTED;

    if (propId == BASIC_SIGNAL_SERVICE_NAME_ID) {
        char*string;
        AJ_UnmarshalArgs(replyMsg, "s", &string);
        strncpy(propertyName, string, propertyNameSize);
        propertyName[propertyNameSize - 1] = '\0';
        AJ_AlwaysPrintf(("Set 'name' property was called changing name to '%s'.\n", propertyName));
        status = AJ_OK;
    }

    return status;
}



/* All times are expressed in milliseconds. */
#define CONNECT_TIMEOUT     (1000 * 60)
#define UNMARSHAL_TIMEOUT   (1000 * 5)
#define SLEEP_TIME          (1000 * 2)

int AJ_Main(void)
{
    AJ_Status status = AJ_OK;
    uint8_t connected = FALSE;

    /* One time initialization before calling any other AllJoyn APIs. */
    AJ_Initialize();

    /* This is for debug purposes and is optional. */
    AJ_PrintXML(AppObjects);

    AJ_RegisterObjects(AppObjects, NULL);

    while (TRUE) {
        AJ_Message msg;

        if (!connected) {
            status = AJ_StartService(&busAttachment,
                                     NULL,
                                     CONNECT_TIMEOUT,
                                     FALSE,
                                     ServicePort,
                                     ServiceName,
                                     AJ_NAME_REQ_DO_NOT_QUEUE,
                                     NULL);

            if (status != AJ_OK) {
                continue;
            }

            AJ_InfoPrintf(("StartService returned %d\n", status));
            connected = TRUE;
        }

        status = AJ_UnmarshalMsg(&busAttachment, &msg, UNMARSHAL_TIMEOUT);

        if (AJ_ERR_TIMEOUT == status) {
            continue;
        }

        if (AJ_OK == status) {
            switch (msg.msgId) {
            case AJ_METHOD_ACCEPT_SESSION:
                {
                    uint16_t port;
                    char* joiner;
                    uint32_t sessionId;

                    AJ_UnmarshalArgs(&msg, "qus", &port, &sessionId, &joiner);
                    status = AJ_BusReplyAcceptSession(&msg, TRUE);
                    AJ_InfoPrintf(("Accepted session. Port=%u, session_id=%u joiner='%s'.\n",
                                   port, sessionId, joiner));
                }
                break;

            case BASIC_SIGNAL_SERVICE_GET_NAME:
                status = AJ_BusPropGet(&msg, GetName, NULL);
                break;

            case BASIC_SIGNAL_SERVICE_SET_NAME:
                status = AJ_BusPropSet(&msg, SetName, NULL);

                if (AJ_OK == status) {
                    status = SendSignal();
                    AJ_InfoPrintf(("SendSignal reports status 0x%04x.\n", status));
                }
                break;

            case AJ_SIGNAL_SESSION_LOST_WITH_REASON:
                {
                    uint32_t id, reason;
                    AJ_UnmarshalArgs(&msg, "uu", &id, &reason);
                    AJ_AlwaysPrintf(("Session lost. ID = %u, reason = %u", id, reason));
                }
                status = AJ_ERR_SESSION_LOST;
                break;

            default:
                /* Pass to the built-in handlers. */
                status = AJ_BusHandleBusMessage(&msg);
                break;
            }
        }

        /* Messages MUST be discarded to free resources. */
        AJ_CloseMsg(&msg);

        if ((status == AJ_ERR_SESSION_LOST || status == AJ_ERR_READ)) {
            AJ_AlwaysPrintf(("AllJoyn disconnect.\n"));
            AJ_Disconnect(&busAttachment);
            connected = FALSE;

            /* Sleep a little while before trying to reconnect. */
            AJ_Sleep(SLEEP_TIME);
        }
    }

    AJ_AlwaysPrintf(("Basic service exiting with status 0x%04x.\n", status));

    return status;
}

#ifdef AJ_MAIN
int main()
{
    return AJ_Main();
}
#endif
