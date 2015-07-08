/*
 * @file
 */

/******************************************************************************
 * Copyright AllSeen Alliance. All rights reserved.
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
#define AJ_MODULE SIGNAL_CONSUMER

#include <stdio.h>
#include <stdlib.h>
#include <aj_debug.h>
#include "alljoyn.h"

uint8_t dbgSIGNAL_CONSUMER = 0;
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
 * This sample receives a signal of a property change in the sample signal_service.
 *
 * See also .\inc\aj_introspect.h
 */
static const char* const sampleInterface[] = {
    InterfaceName,              /* The first entry is the interface name. */
    "!nameChanged newName>s",   /* Signal at index 0 with an output string of the new name. */
    NULL
};

/**
 * A NULL terminated collection of all interfaces.
 */
static const AJ_InterfaceDescription sampleInterfaces[] = {
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
 * The value of the arguments are the indices of the object path in AppObjects (above),
 * interface in sampleInterfaces (above), and member indices in the interface.
 * The 'name' index is 0 because the first entry in sampleInterface is the interface name.
 *
 * Encode the property id from the object path, interface, and member indices.
 *
 * See also .\inc\aj_introspect.h
 */
#define NAMECHANGE_SIGNAL AJ_PRX_MESSAGE_ID(0, 0, 0)

#define CONNECT_TIMEOUT    (1000 * 60)
#define UNMARSHAL_TIMEOUT  (1000 * 5)
#define METHOD_TIMEOUT     (100 * 10)

AJ_Status ReceiveNewName(AJ_Message*msg)
{
    AJ_Arg arg;
    AJ_Status status = AJ_UnmarshalArg(msg, &arg);

    if (status == AJ_OK) {
        AJ_AlwaysPrintf(("--==## signalConsumer: Name Changed signal Received ##==--\n"));
        AJ_AlwaysPrintf(("\tNew name: '%s'.\n", arg.val.v_string));
    }

    return status;
}

int AJ_Main(void)
{
    AJ_Status status = AJ_OK;
    AJ_BusAttachment bus;
    uint8_t connected = FALSE;
    uint8_t done = FALSE;
    uint32_t sessionId = 0;

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
                status = AJ_BusAddSignalRule(&bus, "nameChanged", InterfaceName, AJ_BUS_SIGNAL_ALLOW);
            } else {
                AJ_InfoPrintf(("StartClient returned 0x%04x.\n", status));
                break;
            }
        }

        status = AJ_UnmarshalMsg(&bus, &msg, UNMARSHAL_TIMEOUT);

        if (AJ_ERR_TIMEOUT == status) {
            continue;
        }

        switch (status) {
        case AJ_OK:
            /*
             * The contents of the message are meaningful, only when
             * the message was unmarshaled successfully.
             */
            switch (msg.msgId) {
            case NAMECHANGE_SIGNAL:
                ReceiveNewName(&msg);
                break;

            case AJ_REPLY_ID(AJ_METHOD_JOIN_SESSION):
                AJ_InfoPrintf(("JoinSession SUCCESS (Session id=%d).\n", sessionId));
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
            break;

        case AJ_ERR_NULL:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'Unexpected NULL pointer'.\n"));
            break;

        case AJ_ERR_UNEXPECTED:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'An operation was unexpected at this time'.\n"));
            break;

        case AJ_ERR_INVALID:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'A value was invalid'.\n"));
            break;

        case AJ_ERR_IO_BUFFER:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'An I/O buffer was invalid or in the wrong state'.\n"));
            break;

        case AJ_ERR_READ:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'An error while reading data from the network'.\n"));
            break;

        case AJ_ERR_WRITE:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'An error while writing data to the network'.\n"));
            break;

        case AJ_ERR_TIMEOUT:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'A timeout occurred'.\n"));
            break;

        case AJ_ERR_MARSHAL:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'Marshaling failed due to badly constructed message argument'.\n"));
            break;

        case AJ_ERR_UNMARSHAL:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'Unmarshaling failed due to a corrupt or invalid message'.\n"));
            break;

        case AJ_ERR_END_OF_DATA:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'No enough data'.\n"));
            break;

        case AJ_ERR_RESOURCES:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'Insufficient memory to perform the operation'.\n"));
            break;

        case AJ_ERR_NO_MORE:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'Attempt to unmarshal off the end of an array'.\n"));
            break;

        case AJ_ERR_SECURITY:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'Authentication or decryption failed'.\n"));
            break;

        case AJ_ERR_CONNECT:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'Network connect failed'.\n"));
            break;

        case AJ_ERR_UNKNOWN:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'A unknown value'.\n"));
            break;

        case AJ_ERR_NO_MATCH:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'Something didn't match'.\n"));
            break;

        case AJ_ERR_SIGNATURE:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'Signature is not what was expected'.\n"));
            break;

        case AJ_ERR_DISALLOWED:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'An operations was not allowed'.\n"));
            break;

        case AJ_ERR_FAILURE:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'A failure has occured'.\n"));
            break;

        case AJ_ERR_RESTART:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'The OEM event loop must restart'.\n"));
            break;

        case AJ_ERR_LINK_TIMEOUT:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'The bus link is inactive too long'.\n"));
            break;

        case AJ_ERR_DRIVER:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned 'An error communicating with a lower-layer driver'.\n"));
            break;

        case AJ_ERR_SESSION_LOST:
            AJ_ErrPrintf(("The session was lost\n"));
            break;

        default:
            AJ_ErrPrintf(("AJ_UnmarshalMsg() returned '%s'.\n", AJ_StatusText(status)));
            break;
        }

        /* Messages MUST be discarded to free resources. */
        AJ_CloseMsg(&msg);

        if (status == AJ_ERR_SESSION_LOST) {
            AJ_AlwaysPrintf(("AllJoyn disconnect.\n"));
            AJ_Disconnect(&bus);
            exit(0);
        }
    }

    AJ_AlwaysPrintf(("signalConsumer_Client exiting with status 0x%04x.\n", status));

    return status;
}

#ifdef AJ_MAIN
int main()
{
    return AJ_Main();
}
#endif
