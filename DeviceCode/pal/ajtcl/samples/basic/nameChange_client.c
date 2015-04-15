/*
 * @file
 */

/******************************************************************************
 * Copyright (c) 2012-2014, AllSeen Alliance. All rights reserved.
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
#define AJ_MODULE NAME_CHANGE

#include <stdio.h>
#include <stdlib.h>
#include <aj_debug.h>
#include "alljoyn.h"

uint8_t dbgNAME_CHANGE = 0;
/**
 * Static constants.
 */
static const char InterfaceName[] = "org.alljoyn.Bus.signal_sample";
static const char ServiceName[] = "org.alljoyn.Bus.signal_sample";
static const char ServicePath[] = "/";
static const uint16_t ServicePort = 25;

/*
 * Buffer to hold the full service name. This buffer must be big enough to hold
 * a possible 255 characters plus a null terminator (256 bytes)
 */
static char fullServiceName[AJ_MAX_SERVICE_NAME_SIZE];

/**
 * The interface name followed by the method signatures.
 * This sample changes a property in the signal_service sample.
 *
 * See also .\inc\aj_introspect.h
 */
static const char* const sampleInterface[] = {
    InterfaceName, /* The first entry is the interface name. */
    "@name=s",     /* Property at index 0. */
    NULL
};

/**
 * A NULL terminated collection of all interfaces.
 */
static const AJ_InterfaceDescription sampleInterfaces[] = {
    sampleInterface,
    AJ_PropertiesIface,
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
 * The value of the arguments are the indices of the object path in AppObjects (above),
 * interface in sampleInterfaces (above), and member indices in the interface.
 * The 'name' index is 0 because the first entry in sampleInterface is the interface name.
 *
 * Encode the property id from the object path, interface, and member indices.
 *
 * See also .\inc\aj_introspect.h
 */
#define PRX_SET_NAME    AJ_PRX_PROPERTY_ID(0, 0, 0)
#define PRX_SET_PROP    AJ_PRX_MESSAGE_ID(0, 1, AJ_PROP_SET)

#define CONNECT_TIMEOUT    (1000 * 60)
#define UNMARSHAL_TIMEOUT  (1000 * 5)
#define METHOD_TIMEOUT     (100 * 10)

AJ_Status SendNewName(AJ_BusAttachment* bus, uint32_t sessionId, char*newName)
{
    AJ_Status status;
    AJ_Message msg;

    status = AJ_MarshalMethodCall(bus, &msg, PRX_SET_PROP, fullServiceName, sessionId, 0, METHOD_TIMEOUT);

    if (status == AJ_OK) {
        status = AJ_MarshalPropertyArgs(&msg, PRX_SET_NAME);

        if (status == AJ_OK) {
            status = AJ_MarshalArgs(&msg, "s", newName);
        }

        if (status == AJ_OK) {
            status = AJ_DeliverMsg(&msg);
        }
    }
    return status;
}

int main(int argc, char*argv[])
{
    AJ_Status status = AJ_ERR_INVALID;

    if (argc > 1) {
        AJ_BusAttachment bus;
        uint8_t connected = FALSE;
        uint8_t done = FALSE;
        uint32_t sessionId = 0;
        char*newName = argv[1];

        status = AJ_OK;

        /*
         * One time initialization before calling any other AllJoyn APIs
         */
        AJ_Initialize();
        AJ_PrintXML(AppObjects);
        AJ_RegisterObjects(NULL, AppObjects);

        while (!done) {
            AJ_Message msg;

            if (!connected) {
                status = AJ_StartClientByName(&bus,
                                              NULL,
                                              CONNECT_TIMEOUT,
                                              FALSE,
                                              ServiceName,
                                              ServicePort,
                                              &sessionId,
                                              NULL,
                                              fullServiceName);

                if (status == AJ_OK) {
                    AJ_InfoPrintf(("StartClient returned %d, sessionId=%u.\n", status, sessionId));
                    connected = TRUE;
                    SendNewName(&bus, sessionId, newName);
                } else {
                    AJ_InfoPrintf(("StartClient returned 0x%04x.\n", status));
                    break;
                }
            }

            status = AJ_UnmarshalMsg(&bus, &msg, UNMARSHAL_TIMEOUT);

            if (AJ_ERR_TIMEOUT == status) {
                continue;
            }

            if (AJ_OK == status) {
                switch (msg.msgId) {
                case AJ_REPLY_ID(PRX_SET_PROP):
                    done = TRUE;
                    AJ_AlwaysPrintf(("Name on the interface '%s' at service '%s' was set to '%s'.\n",
                                     InterfaceName,
                                     fullServiceName,
                                     newName));
                    break;

                case AJ_SIGNAL_SESSION_LOST_WITH_REASON:
                    /* Force a disconnect. */
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

            if (status == AJ_ERR_SESSION_LOST) {
                AJ_AlwaysPrintf(("AllJoyn disconnect.\n"));
                AJ_Disconnect(&bus);
                exit(0);
            }
        }
    } else {
        AJ_ErrPrintf(("Error. New name not given: nameChange_client [new name].\n"));
    }

    AJ_AlwaysPrintf(("nameChange_Client exiting with status 0x%04x.\n", status));

    return status;
}
